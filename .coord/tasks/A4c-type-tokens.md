# A4c-type-tokens: every font size, weight and family comes from one token class, look byte-identical

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Low · look-identical (values unchanged, pinned by the design dump)
Branch: `refactor/A4c-type-tokens` in `.claude/worktrees/refactor/A4c-type-tokens`, cut from the umbrella
`feat/A4-design-tokens` (not dev) and squashed back into it (created at Approve)

## Request
2026-11-02, user: "Start type-tokens". Scope as on the STATUS row `step: type-tokens` and the idea row: one source of
truth for fonts; the app looks exactly as today.

## Findings
- **Kind and why:** refactor; fonts are hard-coded at 105 call sites, so a future type change means hunting literals.
- **Now:** token scan inventory on the umbrella (5d8c394): font-size 60, font-weight 38, font-family 7, in 15 files. Top:
  `UI/SettingsParts.cs` (17 size, 11 weight, 2 family), `UI/SettingsWindow.cs` (12 size), `UI/SearchOverlay.cs`,
  `UI/Kit.cs`, `UI/PadWindow.Layout.cs`, `Editor/Colorizers.cs` (6 weight). Only `ChromeTokens.IconGlyphSize` exists today.
- **Values in use:**
  - Text sizes 11, 11.5, 12, 12.5, 13, 13.5, 14, 15, 17, 18, 26 (`Kit.Text(..., size)`, `FontSize =`).
  - Icon glyph sizes 13 (`GlyphIcon` default), 14, 15, 16, 18, 20.
  - Weights: SemiBold 26, Normal 9, Bold 3.
  - Families: "Segoe UI Variable Text, Segoe UI" (pad, settings, a Typeface), "Cascadia Mono, Consolas" (help keys, search,
    settings sample), the editor's fallback tail ", Cascadia Mono, Consolas, monospace" after the user's font.
- **Where:** `Theme/` is exempt from the Tokens scan (`tools/check-design-tokens.ps1:335`). LAYERS: Theme must not use UI
  or Editor; `Avalonia.Media.FontWeight` is fine. `UI/SettingsWindow.cs` is 780 lines: only literal swaps, no new code, so the
  "one job?" check does not apply.
- **Risk:** a swap that changes a value. The design dump catches it for the pad, Settings, help, search and menu. Sites
  the dump cannot see (colorizers, completion popup, renderers, elements, motion preview) rely on the review's
  literal-to-token check.
- **Source:** master scout.

## Plan
- **After:**
  - New `Theme/Typography.cs`, a static class of consts (no objects, so every call site keeps building its own
    `FontFamily`/`Typeface` exactly as now):
    - Families: `UiFamily` = "Segoe UI Variable Text, Segoe UI", `MonoFamily` = "Cascadia Mono, Consolas",
      `EditorFallback` = ", Cascadia Mono, Consolas, monospace".
    - Weights (const `FontWeight`): `Regular` = Normal, `Strong` = SemiBold, `Bold` = Bold.
    - Text sizes, by role: `Micro` 11, `Caption` 11.5, `Small` 12, `Label` 12.5, `Body` 13, `BodyEmphasis` 13.5,
      `Large` 14, `Input` 15, `DialogTitle` 17, `PanelTitle` 18, `Display` 26.
    - Icon glyph sizes: `IconBody` 13, `IconSmall` 14, `IconInput` 15, `IconMedium` 16, `IconLarge` 18, `IconXLarge` 20.
    - One `///` line per group saying where it is used. If a site's role is unclear, pick the token with the same value
      and the closest role; never invent a new value.
  - Every font literal outside `Theme/`, `SelfTest/` and `Calc/` uses these tokens, including default parameters
    (`Kit.Text(... double size = Typography.Body, FontWeight weight = Typography.Regular)`, `GlyphIcon(... size =
    Typography.IconBody ...)`). `ChromeTokens.IconGlyphSize` stays as it is (locked file, extend only).
  - `tools/design-tokens-baseline.tsv` refreshed with `-Update`: font-size, font-weight and font-family reach 0 (or the
    report names each one left and why).
- **Expected time:** about an hour, request to umbrella merge (A4a/A4b: Codex rounds of 2 to 4 minutes, plus review).
- **Must not change:** any applied value; the editor's relative heading scales (multipliers of the user's font size stay where they are); layout, spacing, colors, motion; `ChromeTokens.cs`; files outside the list.
- **Scope questions:** None (role names are mine; design-unify can merge near values later).
- **Test plan, code:** `--selftest` passes with the DESIGN hash unchanged
  (`8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b`, 16160 lines); the Tokens gate shows font counts
  down. Review: every removed literal maps to a token of the same value (diff walk, sites outside the dump first).
- **Test plan, UI:** dev build, about one minute: pad text and headings, a math/list/code note (colorizer weights), the
  completion popup, search (Ctrl+F), help, the pad menu, three Settings sections. Nothing looks different.
- **Gates:** tier defaults: Build, Selftest (DESIGN hash = the look proof), Format, Seam, Tokens (font counts must fall,
  none rise), Scope (Files below), Layers (Typography must not use UI or Editor).
- **Needs from you:** approve; the one-minute hand look.

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Any other file needs a line under
"Out-of-scope files" in the report.
- `product/AntiNotepad/Theme/Typography.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/UI/Kit.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadMenu.cs`
- `product/AntiNotepad/UI/MotionPreview.cs`
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `product/AntiNotepad/Editor/Colorizers.cs`
- `product/AntiNotepad/Editor/Completion.cs`
- `product/AntiNotepad/Editor/Renderers.cs`
- `product/AntiNotepad/Editor/Elements.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `tools/design-tokens-baseline.tsv`

## Contracts
- None changed. The DESIGN selftest hash is the promise and must stay as above.

## Approve
2026-11-02, user: "Approve".

## For Codex
Role: terra (`docs/agents.md`), because it is a mechanical swap across 15 files that needs care to keep every value equal.
Product `product/AntiNotepad`. The branch is already checked out: never switch, merge or commit (you cannot write to
`.git`). Read `AGENTS.md`, `docs/coding.md`, then the files above. Line numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above. No drive-by refactors, renames, or spacing/color/radius/motion
  edits; those are later steps.
- Every swap keeps the exact value. Never change the DESIGN hash constant in `SelfTest/Design.cs`; if the selftest's
  DESIGN line fails, a swap changed a value: find it and fix the swap.
- Get the inventory with `pwsh -NoProfile -File tools/check-design-tokens.ps1 -Inventory <path>`; refresh the baseline
  with `-Update` only after the swaps.
- Build into `build/A4c-type-tokens-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A4c-type-tokens -Offline -Base feat/A4-design-tokens`; the
  manager re-runs it online.
- Never launch or kill AntiNotepad except `--selftest` from your check build. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means (offline checks only: build, selftest, gate, script checks, files; never ports, servers, browser or screen):
- `Theme/Typography.cs` exists as in "After"; no font size, weight or family literal remains outside `Theme/`, `SelfTest/`,
  `Calc/` (the Tokens inventory shows 0 for the three font categories, or the report lists each one left and why).
- `--selftest` prints `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160` and `SELFTEST PASS`.
- Build 0 warnings, the gate passes.

Before you report (director only): spawn the `lens_reviewer` agent on your diff (it reads
`.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why not. Workers without spawning: run the same
checklist yourself and say so.

Report: `.coord/reports/A4c-type-tokens.md`, short, with exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per file: `file | literals swapped (count) | tokens used`.
3. **Behavior changes**: write "None" only if true; the review checks it against the diff.
4. **Out-of-scope files**: file and why, or "None".
5. **Plan deviations**: anything done differently from the Plan (a token added, renamed, or a literal left), and why, or "None".
6. **Contracts**: rows added or changed, or "None".
7. **Lens review**: findings and what you did with each.
8. **Checks run**: build, selftest (with the DESIGN line), gate result lines, Tokens counts before and after.
9. **Hand checks for the user**: the Plan's UI checks.

## Check
Round 1 (456a26c) and round `-r2` (brief `A4c-type-tokens-r2.md`); gate re-run by the reviewer.
- **Gate (`-r2`, `-Offline -Base feat/A4-design-tokens`):** Build PASS 0 warnings, Selftest PASS, Seam PASS, Tokens PASS, Format PASS (15 files, none on changed lines; 91 older ignored), Scope PASS (16 files), Size PASS, Cleanup PASS: `GATE: PASS`. Online, Build only warns `NU1900` (no network to the NuGet audit), not code.
- **Look proof:** own build, `--selftest`: `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160` and `SELFTEST PASS`. `SelfTest/Design.cs` and `Theme/ChromeTokens.cs` untouched.
- **Bounds:** the diff against the umbrella touches only the card's 16 Files plus card, briefs and reports: within plan. Behavior changes "None": confirmed.
- **Diff walk:** a script expanded every `Typography.X` in added lines to its literal and compared the removed lines for all 14 call-site files: identical in 12; the other two differ only in form and agree in value. `Editor/PadView.cs` goes from `$"{settings.Font}, Cascadia Mono, Consolas, monospace"` to `settings.Font + Typography.EditorFallback`, the same string. `Editor/Renderers.cs` goes from `font + ", Cascadia Mono, Consolas"` to `font + ", " + Typography.MonoFamily`, the same string. Sites outside the dump read by eye: `Colorizers.cs` (SemiBold/Bold to Strong/Bold), `Completion.cs` (12 and SemiBold), `Renderers.cs` (default `Regular`, SemiBold typeface), `Elements.cs` (default weight), `PadView.cs`, `MotionPreview.cs` (three 11.5 to Caption). All within plan.
- **Tokens inventory:** font-size 60 to 10, font-weight 38 to 0, font-family 7 to 0. All 10 remaining font-size hits are scanner false positives (a token or ChromeTokens size followed by a glyph weight `0` or a width 88/104). Icon font resource and glyph-discovery family names stay literal (no catalog token): justified in the report.
- **Justified difference:** the manager changed line endings of 15 files LF to CRLF after round 1 (Codex wrote LF; worktree is CRLF); no content change.
- **Misses:**
  - Format failed on 15 touched lines (35 problems): found at gate; cause `code`; the brief did not say touched initializer lines must be formatter-clean; a "Format on touched lines" line in the "For Codex" template would catch it earlier. Lesson: Codex writes LF; tell it to keep CRLF and to run `dotnet format --verify-no-changes --include` on touched files.
- **Noticed:** none.
- **Verdict:** accepted.

## Hand test
2026-11-02, user: "everything looks fine as far as i can tell"

Dev copy from `build/dev` (data `build/testdata`), checks as listed in the Plan: pass.
