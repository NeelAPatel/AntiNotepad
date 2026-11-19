# A32-selection-convert-menu-r4 report

## 1 Summary
Redrew the Numbered list icon as three small digits paired with horizontal rules in the bullet icon's 28 px column.

## 2 Change list
- `UI/PadMenu.cs`: replaced the stacked text icon with three aligned number-and-line rows using existing theme tokens.

## 3 Behavior changes
Only the Numbered list icon changes. The digits use the menu font and `Typography.Micro`; the rules use `p.Ink` and `Shape.Hairline`.

## 4 Out-of-scope files
None changed for this round. Existing item changes remain in the worktree.

## 5 Plan deviations
None.

## 6 Contracts
No behavior contracts changed.

## 7 Lens review
Not run; this round changes one icon only. Visual alignment and legibility remain for hand verification.

## 8 Checks run
- `pwsh -NoProfile -File tools/gate.ps1 -Task A32-selection-convert-menu-r4 -Offline`: PASS; build had 0 warnings and 0 errors, `SELFTEST PASS`, seam, tokens, format and scope passed. Offline NuGet audit was disabled as expected.
- `git diff --check`: PASS.

## 9 Hand checks
Pending: compare the Numbered list icon beside the Bullet list icon and verify digit size, line alignment and visual weight in the dev copy.
