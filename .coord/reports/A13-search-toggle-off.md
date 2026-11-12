# A13-search-toggle-off

## 1. Summary
The top-right Search button now closes an open search overlay.

## 2. Change list
- Added `ToggleSearch()` in `PadWindow.Keys.cs`.
- Wired the top-right Search icon to `ToggleSearch()` in `PadWindow.Layout.cs`.

## 3. Behavior changes
Clicking Search opens search; clicking it again closes any `SearchOverlay`, including trash search. Menu Search and Ctrl+F/P/K continue to open search. Other overlays are unaffected.

## 4. Out-of-scope files
None edited. The gate generated its standard `.coord/reports/A13-search-toggle-off.gate.md` report.

## 5. Plan deviations
None. NuGet audit was disabled for offline builds as directed by the gate. The temporary `build/A13-search-toggle-off-check` directory could not be removed because the workspace policy rejected the cleanup command.

## 6. Contracts
None added.

## 7. Lens review
Skipped: quick role.

## 8. Checks run
- Release build to `build/A13-search-toggle-off-check`: 0 warnings, 0 errors, with `NuGetAudit=false`.
- `--selftest`: `SELFTEST PASS`.
- `tools/gate.ps1 -Task A13-search-toggle-off -Offline`: passed build, selftest, seam and tokens. Gate notes offline NuGet audit disabled.

## 9. Hand checks for the user
- Click Search: overlay opens.
- Click Search again: overlay closes, panes return and note cursor is restored.
- Click Search again: fresh search opens.
- Esc and the overlay X still close search.
