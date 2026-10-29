## Summary
The split divider now extends to the panes' bottom edge by removing its reserved pager clearance. The pager retains its existing placement above the divider; visual confirmation remains a user hand check.

## Change list
- `product/AntiNotepad/Theme/ChromeTokens.cs:65-70` | remove unused clearance; set divider bottom margin to zero | divider reaches bottom; toast keeps its own margin.
- `product/AntiNotepad/UI/PadWindow.Layout.cs:31-31,61-74,104-110` | share divider construction and unchanged chrome layer order with the selftest | full-height divider; pager above divider.
- `product/AntiNotepad/SelfTest.cs:5-5,135-170` | check production divider margin and docked/overlay layer order without creating a window | regression promise.
- `product/AntiNotepad/SelfTest/Contracts.cs:62-68` | register `split-divider-full-height` | required CONTRACTS row.

## Behavior changes
In split view, the divider extends from its unchanged top margin to the bottom edge, including behind the pager when shown.

## Out-of-scope files
- `.coord/STATUS.md`: required dispatch, milestone and completion status.
- `.coord/reports/A53-split-divider-height.md`: requested task report.
- `.coord/reports/A53-split-divider-height.gate.md`: generated required gate evidence (ignored).

## Contracts
Adds `split-divider-full-height`; existing rows retained, including manual `window-style-bits`.

## Lens review
Accepted; no proven findings. Reviewed helper callers, split binding, bar reparenting and top-blur initialization; behavior list matches the diff. No fixes required.

## Checks run
- Check build (telemetry opted out; `-p:NuGetAudit=false`): `Build succeeded. 0 Warning(s), 0 Error(s)`.
- Check executable `--selftest`: `CONTRACTS: 8 rows, 7 pass, 1 manual`; `SELFTEST PASS` (exit 0). New row passes; `window-style-bits` remains manual.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A53-split-divider-height -Offline`: `GATE: PASS` (exit 0); build 0 warnings/errors, selftest PASS, seam 0 violations, scope PASS (4 product files).
- Gate notices: offline NuGet audit disabled; size WARN (`SelfTest.cs` 2763 -> 2801 lines). Gate cleanup PASS.
- Lens `git diff --check`: PASS.
- Requested check-folder cleanup attempted twice: automatic approval review rejected recursive removal with `blocked by policy`, including the exact literal folder path. `build/A53-split-divider-height-check` remains; no permission escalation is available. The gate's own output/data folders were removed successfully.

## Hand checks for the user
In the dev copy, check split view in light and dark themes with the pager hidden and showing. The divider should reach the bottom edge, keep its current top, width and color, and draw behind the clean pager pill. Check docked, overlay and float bar styles, toggle split off/on, and confirm pane widths and toast position remain unchanged. Live appearance and window corners/shadow remain unverified here.

## Manager verdict
Tier Low. Gate: `GATE: PASS` (build 0 warnings, `SELFTEST PASS`, CONTRACTS 8 rows: 7 pass, 1 manual; seam 0 violations; scope PASS). Size WARN (`SelfTest.cs` 2763 -> 2801) accepted: the new contract check lives there. No review at Low tier. Diff read: token set to 0, `DividerPagerClearance` removed, divider and chrome layer order extracted for the selftest, unchanged order (pager above divider). Codex's edit to `.coord/STATUS.md` in the worktree was reverted (master owns STATUS). Accepted.

## Hand-test follow-up (user, 2026-10-29)
Full bleed looked wrong: wants a small gap at the bottom matching the top gap. Manager gate fix (tiny): `DividerBottomMargin = DividerTopMargin` (6), selftest and contract text updated. Gate PASS, 8 rows 7 pass 1 manual. Behavior change: divider now has a 6 px gap at the bottom (same as top) instead of 0; still no longer reserves the pager strip.

## Hand-test follow-up 2 (user, 2026-10-29)
User's target: divider ends just above the pager area, pill showing or not. That is the pre-A53 token (`PagerStripHeight + DividerPagerClearance`), restored. Contract row renamed `split-divider-above-pager` (selftest asserts that margin and pager-above-divider layer order). Gate PASS. Net product change vs dev: only the selftest helpers/contract; no visible change.
