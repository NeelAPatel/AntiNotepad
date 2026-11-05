# A4f-paint-tokens: hover, press, fill, border and dim strengths come from one token class, look identical

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Medium · look-identical, part outside the dump (hover/press states and editor drawing are not in the design dump)
Branch: `refactor/A4f-paint-tokens` in `.claude/worktrees/refactor/A4f-paint-tokens`, cut from the umbrella
`feat/A4-design-tokens` (not dev) and squashed back into it (created at Approve)

## Request
2026-11-02, user: "Start paint-tokens" (earlier the same day, on the trimmed row: "we might want to do this"). Scope as on
the STATUS row `step: paint-tokens`: alphas and opacities by role at today's values. The user's tint-range note (Acrylic and
Mica tint choices 0.55-0.92 are too narrow) is a look change for design-unify, lab-first, not this step.

## Findings
- **Kind and why:** refactor; 50 brush opacities and byte alphas are written inline at 13 files, many repeating the same pair
  (hover .09/.06, press .15/.11, selected accent .22/.18) in several places.
- **Now:** token scan on the umbrella (7d37a14): opacity 45, alpha 5. By file: `UI/SettingsParts.cs` 14, `UI/SearchOverlay.cs`
  6, `UI/Kit.cs` 6, `UI/PadMenu.cs` 5, `Editor/Renderers.cs` 4, `UI/PadWindow.Layout.cs` 3, `UI/MotionPreview.cs` 3,
  `Editor/PadView.cs` 3, `Editor/Structure.cs` 2, `UI/SettingsWindow.cs` 1, `UI/PadWindow.cs` 1, `UI/HelpOverlay.cs` 1,
  `Editor/Motion.cs` 1. (The row's "about 85" was the older A59 estimate.)
  - Theme pairs (dark / light): hover .09/.06 (Kit, PadMenu), press .15/.11 (Kit, PadMenu), selected accent .22/.18 (Kit 2,
    SettingsParts 2, SettingsWindow 1), card surface .92/.9, help wash .1/.06, preview frame .09/.10, preview well .06/.045,
    settings row .03/.025, completion fill .085/.075, badge fill .14/.10.
  - Single values: popup border .45 (PadMenu, save dialog), search field border .5, result border .25 / current .9, result
    surface .55, dimmed result .55, focus border .9 / rest .5 (Kit), error hint .78 (PadView 2), toast .92, scrim .55 and
    preview canvas .6 (glass only), menu caption .8, menu separator .4, caret block .45 (Motion, SettingsParts), structure
    marker .35 / .8, key pen .55, accent pen .9, button hover face .86, theme card border .9 / .3, grid pen .9, preview bar
    .10 and pad .8.
  - Byte alphas: settings icon ink 220, pinned glyph 110, sample gradient 90 and 40.
  - Animation start opacities (`0.25 + 0.75 * t` note arrive, `0.35 + 0.65 * t` content reveal) are motion values: out of
    scope, left for control-groups.
- **Where:** `Theme/` is exempt from the Tokens scan and already holds `Palette`, `ChromeTokens`, `Typography`, `Shape`.
  `ChromeTokens` keeps its own alphas (badge, pill, divider, top backdrop). LAYERS: Theme must not use UI or Editor.
- **Risk:** a swap that changes a value. The dump covers the resting brushes of the pad, Settings, help, search and menu.
  Not covered: hover and press brushes (set on pointer events), the focus style setter, editor drawing (`Renderers`,
  `Structure`, caret), and the save-failure dialog; the review checks those by hand, site by site.
- **Source:** master scout.

## Plan
- **After:**
  - New `Theme/Paint.cs`, a static class:
    - Theme pairs as small methods taking `bool dark`: `Hover`, `Press`, `Selected`, `CardSurface`, `HelpWash`,
      `PreviewFrame`, `PreviewWell`, `RowWash`, `CompletionFill`, `BadgeFill` (each `dark ? x : y` at today's values).
    - Single opacities as `double` consts by role: `PopupBorder` .45, `FieldBorder` .5, `ResultBorder` .25, `ResultCurrent`
      .9, `ResultSurface` .55, `Dimmed` .55, `FocusBorder` .9, `RestBorder` .5, `ErrorHint` .78, `Toast` .92, `GlassScrim`
      .55, `GlassPreview` .6, `MenuCaption` .8, `MenuSeparator` .4, `CaretBlock` .45, `MarkerFaint` .35, `MarkerStrong` .8,
      `KeyPen` .55, `AccentPen` .9, `HoverFace` .86, `CardBorderOn` .9, `CardBorderOff` .3, `GridPen` .9, `PreviewBar` .10,
      `PreviewPad` .8. Where a value repeats with the same role it uses the same token; same value with a different role
      gets its own name (merging is design-unify).
    - Byte alphas as `byte` consts: `IconInkAlpha` 220, `PinnedGlyphAlpha` 110, `SampleGradientAlpha` 90,
      `SampleFadeAlpha` 40.
    - One `///` line per group. Never invent a new value. No brush factories (dropped from the row to keep this a pure swap).
  - Every listed site uses its token; the two animation start opacities stay as they are.
  - `tools/design-tokens-baseline.tsv` refreshed with `-Update`: alpha reaches 0 and opacity drops to the 2 animation
    starts (or the report names each one left and why).
- **Expected time:** about 60 minutes, request to umbrella merge (A4d: 25 sites, two rounds; this is 50 sites in 13 files).
- **Must not change:** any applied value; fonts, radii, spacing, motion, timers, `ChromeTokens.cs`, `Palette.cs`; files
  outside the list.
- **Scope questions:** None.
- **Test plan, code:** `--selftest` passes with the DESIGN hash unchanged
  (`8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b`, 16160 lines); Tokens alpha 5 → 0, opacity 45 → 2.
  Review: every removed number maps to a token of the same value, both halves of every dark/light pair; the sites the dump
  cannot see first (hover, press, focus, editor drawing, save dialog).
- **Test plan, UI:** dev build, about two minutes, in a dark and a light theme: hover and press a menu item and a Settings
  button, tab into a text box (focus border), open search and arrow through results, look at a Settings card, the theme
  cards and the caret preview, open help, show a toast, and in a note look at bracket guides and the block caret. Nothing
  looks different.
- **Gates:** tier defaults: Build, Selftest (DESIGN hash), Format, Seam, Tokens (alpha and opacity fall, none rise), Scope,
  Layers (Paint must not use UI or Editor).
- **Needs from you:** approve; the two-minute hand look.

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Any other file needs a line under
"Out-of-scope files" in the report.
- `product/AntiNotepad/Theme/Paint.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/UI/Kit.cs`
- `product/AntiNotepad/UI/PadMenu.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/MotionPreview.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `product/AntiNotepad/Editor/Renderers.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/Editor/Structure.cs`
- `product/AntiNotepad/Editor/Motion.cs`
- `tools/design-tokens-baseline.tsv`

## Contracts
- None changed. The DESIGN selftest hash must stay as above.

## Approve
2026-11-02, user: "Approve"

## For Codex
Role: terra (`docs/agents.md`), because it is a careful value-preserving swap across 13 files with dark/light pairs.
Product `product/AntiNotepad`. The branch is already checked out: never switch, merge or commit (you cannot write to
`.git`). Read `AGENTS.md`, `docs/coding.md`, then the files above. Line numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above. No drive-by refactors; no font, radius, spacing, motion or timer edits.
- Every swap keeps the exact value, both halves of every pair. Never change the DESIGN hash constant in `SelfTest/Design.cs`.
- Keep CRLF line endings. Lines you touch must pass the gate's Format step (one initializer entry per line where the
  formatter asks for it).
- Get the inventory with `pwsh -NoProfile -File tools/check-design-tokens.ps1 -Inventory <path>`; refresh the baseline
  with `-Update` only after the swaps.
- Build into `build/A4f-paint-tokens-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A4f-paint-tokens -Offline -Base feat/A4-design-tokens`; the
  manager re-runs it online.
- Never launch or kill AntiNotepad except `--selftest` from your check build. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means (offline checks only: build, selftest, gate, script checks, files; never ports, servers, browser or screen):
- `Theme/Paint.cs` exists as in "After"; only the two animation start opacities remain (or the report lists each one left and why).
- `--selftest` prints `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160` and `SELFTEST PASS`.
- Build 0 warnings, the gate passes.

Before you report (director only): spawn the `lens_reviewer` agent on your diff (it reads
`.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why not. Workers without spawning: run the same
checklist yourself and say so.

Report: `.coord/reports/A4f-paint-tokens.md`, short, with exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per file: `file | literals swapped (count) | tokens used`.
3. **Behavior changes**: write "None" only if true; the review checks it against the diff.
4. **Out-of-scope files**: file and why, or "None".
5. **Plan deviations**: anything done differently from the Plan, and why, or "None".
6. **Contracts**: rows added or changed, or "None".
7. **Lens review**: findings and what you did with each.
8. **Checks run**: build, selftest (with the DESIGN line), gate result lines, Tokens counts before and after.
9. **Hand checks for the user**: the Plan's UI checks.

## Check
2026-11-02, builder: round 1 (23ae026) values accepted site by site; gate failed only Format, fixed in -r2 (5b100f5). Gate: Build (only NU1900, offline), Selftest, Seam, Tokens, Format, Scope, Size pass. DESIGN `8123f5c3...1de55b 16160`, SELFTEST PASS. Tokens alpha 5 -> 0, opacity 45 -> 2. CRLF kept; Design.cs, ChromeTokens.cs, Palette.cs untouched.

## Hand test
2026-11-02, user, dark and light themes on the dev build: "Looks identical".
