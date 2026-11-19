# A32-selection-convert-menu-r5 report

## 1 Summary
Replaced the Numbered list icon's clipped text digits with a small custom drawing control that renders hand-drawn 1, 2 and 3 digits beside three thin rules.

## 2 Change list
- `UI/PadMenu.cs`: draws all icon marks as geometry in the centered 16 px square, retaining the 28 px menu column and using `p.Ink` with 1 px rounded strokes.
- Removed the icon's `TextBlock`, `Typography.Micro` and `Shape.Hairline` use.

## 3 Behavior changes
Only the Numbered list icon changes. Its digits no longer depend on text layout and each row aligns with the bullet icon's three-row layout.

## 4 Out-of-scope files
None.

## 5 Plan deviations
None.

## 6 Contracts
No behavior contracts changed.

## 7 Lens review
Not run; this rework changes one icon in one file. Visual comparison remains for hand verification.

## 8 Checks run
- `pwsh -NoProfile -File tools/gate.ps1 -Task A32-selection-convert-menu-r5 -Offline`: PASS; build had 0 warnings and 0 errors, `SELFTEST PASS`, seam, tokens, format, scope and size checks passed. Offline NuGet audit was disabled as expected.
- `git diff --check`: PASS.

## 9 Hand checks
Pending: compare the Numbered list icon beside the Bullet list icon in the dev copy. AntiNotepad was not launched, per request.
