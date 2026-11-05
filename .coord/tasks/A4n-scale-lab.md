# A4n-scale-lab

Tier: Medium · Branch: feat/A4n-scale-lab (off feat/A4-design-tokens) · Step of A4-design-tokens · Lab build

## Request
User, 2026-11-05, after reviewing the playground: "Use Recommended, everythign looks good. Make a dev build using all
recommended values, regardless if its on the playground or not"

## Findings
- The approved scale is `docs/design-scale.md` (A4j). Recommended (R) values per role: section 3.1 (Typography, Shape,
  Motion, Paint), 3.2 (ChromeTokens; bold rows change: 22 rows / 26 constants), 4 (the 15 control kinds), 5 (aliases),
  6 (13 exceptions that keep their literal), 7 (visible changes to check).
- `docs/playground/tokens.json` (aligned in A4m) carries the same R value per role (`recommended` / per-role steps) and
  is the machine-readable cross-check. Where the two disagree, design-scale.md wins; list every disagreement.
- Product homes: `product/AntiNotepad/Theme/` (ChromeTokens.cs, Typography.cs, Shape.cs, Paint.cs, Palette.cs),
  `Motion/MotionTokens.cs`, plus control-level literals in `UI/` and `Editor/` that section 4 maps to roles.
- The lock question (design-scale.md section 8) is answered for this lab only: the user said "all recommended values",
  so the locked ChromeTokens rows change here too. `docs/decisions.md` is not edited in this step.

## Plan
1. Change VALUES only to their R value, every role in sections 3.1, 3.2 and 4, whether or not the playground draws it
   (Settings window, divider, top backdrop, timer chip, badges included). No renames, no new token structure, no
   scale > role refactor (that is the later scale-apply step).
2. The 13 exceptions in section 6 keep their literal. Weight.Bold in the editor stays 700.
3. A control literal that section 4 maps to a role takes that role's R value; a literal with no mapping stays and is
   listed in the report.
4. Report: a table role | file:line | today -> R, plus "left as is" with reasons and any design-scale vs tokens.json
   disagreement.

## Files
- `product/AntiNotepad/Editor/Renderers.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/Theme/Paint.cs`
- `product/AntiNotepad/Theme/Shape.cs`
- `product/AntiNotepad/Theme/Typography.cs`
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `product/AntiNotepad/UI/Kit.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `tools/design-tokens-baseline.tsv`
(SelfTest/Design.cs: design-dump hash refresh only. Theme/Palette.cs: no mapped change.)

Token baseline reason: PadWindow.Layout.cs spacing literals 24 -> 26 from the two section 4 save-prompt compact 8 x 2 paddings on existing button content borders (lab build; the scale-apply step turns them into roles).

## Checks
Build clean; `--selftest` passes; the gate (Scope, Format) passes with -Base feat/A4-design-tokens. Codex checks offline only.
Hand test (user, lab build from `tools/dev.ps1`, dev badge, Ctrl+Alt+Shift+D): walk section 7 items 1-9: bar, pager,
search, menu, toast, Settings window, overlay/float, light theme; time show/hide (speed is the priority).

## Approve
User, 2026-11-05: the request above ("Make a dev build using all recommended values").

## Hand test
- Round 1 (user, 2026-11-05): asked to double the gap below the pill (dc13bfc, 4 -> 8); then "Reduce to 4 and 4"
  (reverted in 1eee3ed). Found two search problems in Overlay mode (bar covers the field; no window drag); both
  pre-existing on feat/A4-design-tokens, moved to STATUS `idea: search-bar-overlap`.
- Round 2 (user, 2026-11-05): "looks good. we can wrap up and merge." PASS.
