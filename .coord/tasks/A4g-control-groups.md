# A4g-control-groups: group the look tokens by control kind, starting with a map of where each kind disagrees

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Medium · touches many controls' look (values must stay identical)
Branch: `refactor/A4g-control-groups` in `.claude/worktrees/refactor/A4g-control-groups` (cut by hand off
`feat/A4-design-tokens` at Approve; squashed back into the umbrella, not dev)

## Request
- 2026-11-02, user: "hm i want to find a way we can conciously group certain controls together and clean up the messy
  tokens like the ones you detected"
- 2026-11-02, user: "Your ideation of grouping controls based on purpose makes sense. we should do that, but also we'll
  take time to examine design language and principles followed by microsoft for their sizes and observe patterns."
  (the second half is A4h-fluent-study)
- 2026-11-02, user, on "Squash A4f into the umbrella, then plan control-groups and fluent-study?": "Yes, do both"

## Findings
- **Kind and why:** polish (design system); the role tokens exist (Typography, Shape, Paint, MotionTokens, Palette,
  ChromeTokens), but nothing says which control uses which, so the same kind of control picks different values.
- **Now:** tokens are named by role (`Typography.Caption`, `Shape.Control`, `Paint.Hover`), not by control. The pinned
  design dump (`SelfTest/Design.cs`) lists every control path with font, radius, opacity, brushes, border, shadow,
  margin and padding, but only by .NET type (`Border`, `Button`), so a card and a popup both read `Border`: grouping by
  purpose needs the factories in `UI/Kit.cs`, `UI/SettingsParts.cs`, `UI/PadMenu.cs` and the overlays.
- **Known duplicates:** ChromeTokens repeats role values under its own names: `BadgeFontSize` / `RunningTimerFontSize` /
  `PagerCountFontSize` 11.5 (= `Typography.Caption`), `SavedLabelFontSize` 12 (= `Typography.Small`), `IconGlyphSize` 16
  (= `Typography.IconMedium`), hover radii 8 (= `Shape.Popup`), `PillCornerRadius` 12 (= `Shape.Dialog`),
  `BadgeDarkFillAlpha` .18 / `BadgeLightFillAlpha` .13 (near `Paint.BadgeFill` .14/.10).
- **Lock:** `docs/decisions.md`, "Bar insets and chrome tokens": "every top bar and pager measurement lives in
  `ChromeTokens` ... no per-control numbers" (locked 2026-10-25, extend, not replace). So ChromeTokens names stay; only
  their values may become references to the role tokens.
- **Where:** `product/AntiNotepad/Theme/*`, `UI/Kit.cs`, `UI/SettingsParts.cs`, `UI/PadMenu.cs`, `UI/SearchOverlay.cs`,
  `UI/HelpOverlay.cs`, `UI/PadWindow.*.cs`, `Editor/Renderers.cs`.
- **Risk:** naming the groups is a design choice, and a wrong reference changes a value. The pinned DESIGN hash
  (`8123f5c3...`, 16160 lines) catches any changed value on the dumped surfaces.
- **Source:** master scout (design dump format, ChromeTokens, Typography, decisions lock). The Medium audit is skipped:
  Part 1 below is itself a read-only map, so a separate audit would do the same work twice.

## Plan
Two parts with a user checkpoint between them (lab-first: the names are a design choice).

- **Part 1, map (report only, no product change):** `.coord/reports/A4g-control-groups-map.md` groups every visible
  control by purpose: text button, icon button, toggle, dropdown, text field, slider, card, popup and menu, badge, pill,
  overlay panel, toast, and the text roles. Per group: where it is built (factory and call sites), the values each
  instance uses (size, radius, padding, font size and weight, fill and border alpha, shadow), which token each comes
  from or the literal, and where the group disagrees with itself. It ends with a proposed set of group tokens (names and
  which role token each points at) and the ChromeTokens aliases that can become references.
- **Checkpoint:** the user reads the map and approves or changes the group names. Recorded under `## Approve`.
- **Part 2, group tokens (rework round -r2, values unchanged):** add the approved group tokens over the role tokens,
  point the factories at them, and turn the ChromeTokens duplicates into references (names kept). Its Files list is
  filled here at the checkpoint.
- **After:** each control kind reads its look from one named group; the app looks exactly the same.
- **Expected time:** Part 1 about 30 minutes; Part 2 about 1 hour (similar to A4f-paint-tokens).
- **Must not change:** any rendered value (the DESIGN hash stays `8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b`,
  16160 lines, and the dump format stays as it is); ChromeTokens names; timing; Palette colors.
- **Scope questions:** whether disagreements the map finds (for example 11.5 vs 12 captions) merge now or wait for
  design-questions. Proposed: they wait; this item only names and references.
- **Test plan, code:** `--selftest` DESIGN check passes with the pinned hash; token scanner count does not rise.
- **Test plan, UI:** Part 2 only: dark and light themes on the dev build, pad, menu, search, help, every Settings page:
  "looks identical".
- **Gates:** Part 1: Format and Scope (report only, no product files). Part 2: Build, Selftest (DESIGN hash proves the
  look), Tokens (the ratchet catches new literals), Scope, Format.
- **Needs from you:** Approve now; the group names at the checkpoint; the look test after Part 2.

## Files
Part 1 (only this file may change):
- `.coord/reports/A4g-control-groups-map.md`

Part 2: filled at the checkpoint.

## Contracts
- None in Part 1. Part 2: the DESIGN pinned hash stays unchanged (existing check).

## Approve
2026-11-02, user: "Approve both"

## For Codex
Role: terra (`docs/agents.md`), because mapping many controls to their tokens is careful reading across files. Product
`product/AntiNotepad`. The branch is already checked out: never switch, merge or commit (you cannot write to `.git`).
Read `AGENTS.md`, then the files above. Line numbers are from the current tree; re-read before editing.

Constraints:
- **Part 1 only.** Do not change any product file, token file, selftest or tool. Write only the map report.
- You may build into `build/A4g-control-groups-check` with `AVALONIA_TELEMETRY_OPTOUT=1` and run
  `--design-dump <file>` there to read real values, then delete that folder.
- If the Findings turn out wrong, stop and report that instead.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A4g-control-groups -Base feat/A4-design-tokens -Offline`.
- Never launch or kill AntiNotepad. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means (offline checks only):
- `.coord/reports/A4g-control-groups-map.md` exists with one section per group (sites, values with their token or
  literal, disagreements), a proposed group-token table (name | points at | used by) and a ChromeTokens alias table
  (name | value | role token it equals | same value: yes/no).
- No product file changed; the gate passes.

Report: `.coord/reports/A4g-control-groups.md`, short, with exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per changed file.
3. **Behavior changes**: "None" (report only).
4. **Out-of-scope files**: file and why, or "None".
5. **Plan deviations**: or "None".
6. **Contracts**: "None".
7. **Lens review**: findings and what you did with each.
8. **Checks run**: gate result lines.
9. **Hand checks for the user**: "None (report only)".

## Check
Approve: Narrow round 4 (user, 2026-11-03; rounds r2 and r3 ran free).
- Gate (r4): PASS (Build, Selftest, Seam, Tokens, Format, Scope, Size, Cleanup). No product file changed.
- Round 1-3: self-declared incomplete, sent back. r4: placeholders replaced by concrete lists, extra table folded into Preview tile / Swatch / Badge, one group-token row per group with an "Instances disagree?" column, "(draft)" removed, Gaps honest.
- ChromeTokens alias table: every value matches Theme/ChromeTokens.cs, Shape.cs, Typography.cs, Paint.cs (11 rows; 9 match, 2 badge alphas do not).
- Within plan: all listed groups covered (slider: none found). Justified: the group-token table is mostly dashes because most groups disagree; those go to design-questions, as the card proposed.
- Noticed: Gaps list navigation row, menu separator, help text, motion trace not mapped (named, not blocking the names checkpoint).

## Hand test
None (report only). 2026-11-03, user on the reshaped design-system path (systems study, scale proposal, lab build,
apply): "Yes, this path." Part 2 (group tokens, ChromeTokens references) is dropped: the scale steps redo those values.
The map is input to the scale proposal.
