# A32-selection-convert-menu-r2 report

## 1 Summary
Close an open submenu when the pointer moves onto another parent menu row.

## 2 Change list
Track the active submenu and hovered row at each menu level in PadMenu; preserve the existing control tree and keyboard focus path.

## 3 Behavior changes
Entering another row closes the open submenu. Leaving its opener alone does not close it, allowing travel into the popup. Right, Left, Esc, and native light dismissal retain their existing paths.

## 4 Out-of-scope files
No other product files changed in this round. The gate generated its report.

## 5 Plan deviations
None. The dedicated check build folder remains because recursive removal was blocked by the execution policy.

## 6 Contracts
The existing conversion undo and blank-line contract remains unchanged. The submenu pointer behavior requires the hand check below.

## 7 Lens review
Review of the revised code found no proven logic regression. Displayed popup interaction remains unverified.

## 8 Checks run
Dedicated offline build: zero warnings and errors. Dedicated selftest: SELFTEST PASS, including the unchanged design snapshot. Offline gate: GATE PASS, zero build warnings and SELFTEST PASS. An earlier wrapper implementation failed the design snapshot; it was replaced before the passing checks.

## 9 Hand checks
Pending user in the dev slot: hover Convert to list, move into the submenu and choose an item; reopen and hover enabled and disabled sibling rows and the action row; check Right, Left, Esc, and clicking outside. Also perform the original conversion and undo checks. No app was launched for hand testing.
