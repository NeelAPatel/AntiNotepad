## Summary
Stamped the app as 0.7.0-rc2 in `AppInfo.props`. Release build and selftest passed; the gate passed offline.

## Change list
`product/AntiNotepad/AppInfo.props:4-5` | Set `AppVersion` to 0.7.0 and `AppVersionLabel` to rc2 | Plan: assembly version 0.7.0 and informational version 0.7.0-rc2.

## Behavior changes
None.

## Out-of-scope files
None.

## Plan deviations
The direct Release build initially reported two NU1900 warnings because the sandbox could not reach NuGet's vulnerability-data endpoint. The required offline gate rebuilt Release with NuGet audit disabled and reported 0 warnings; the manager can rerun the gate online as planned.

## Contracts
None.

## Lens review
Skipped: Low tier, one property line.

## Checks run
- Direct Release build to `build/A7-version-rc2-check`: succeeded, 0 errors; 2 NU1900 vulnerability-data warnings due to unavailable NuGet endpoint.
- Release selftest from that build: `SELFTEST PASS` (exit 0).
- `pwsh -NoProfile -File tools/gate.ps1 -Task A7-version-rc2 -Offline`: `GATE: PASS`; Build 0 warnings/0 errors; Selftest PASS; Seam PASS; Tokens PASS; Format PASS; Scope PASS; Size PASS; LoopLint skipped (no loop files changed); Cleanup PASS.
- Removed `build/A7-version-rc2-check` after checks.

## Hand checks for the user
None (not visible).
