# A34-pill-bar-per-mode-r12

## Change list
- Deferred slot title resolution until filled slot tooltips are built; added stack and slot tooltip capability checks and a regression contract.
- Confirmed `Note.IsEmpty` directly returns the stored `Blank` flag without reading a body.

## Behavior changes
- Stack bar snapshots and rebuilds no longer resolve slot titles. Slot tooltip text remains unchanged; empty slots do not resolve titles.

## Plan deviations
- Added the required bug regression contract row. The initial no-restore build inherited two stale NU1900 audit warnings; the final offline build with audit disabled had none.
- Cleanup of `build/A34-check` was rejected by automatic approval review as blocked by policy; the folder remains.

## Gate result
- `dotnet build product/AntiNotepad -c Debug -p:NuGetAudit=false -o build/A34-check`: passed, 0 warnings, 0 errors.
- `build/A34-check/AntiNotepad.exe --selftest`: passed; 41 contracts (40 pass, 1 manual). Design dump unchanged: `f08e0c448f3f7a400dd73fe7710bd51e05f6545782c40b3caa6ed5c24466aa79`, 18,048 lines.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A34-pill-bar-per-mode -Offline`: PASS; offline audit warning only.
