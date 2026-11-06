# A5-search-bar-overlap-r2

## Summary
Split the AddOverlayDragBand initializer properties onto separate lines without changing behavior.
Build, selftest and Format pass; the offline gate fails only Scope because the supplied round brief has no backticked paths in its Files section.

## Change list
product/AntiNotepad/UI/PadWindow.Layout.cs:101-104 | One initializer property per line | Round 2 formatting requirement; original Plan unchanged.

## Behavior changes
None.

## Out-of-scope files
None.

## Plan deviations
Gate used -Keep to honor the round's instruction to leave build folders. Scope remains blocked by the supplied round brief; neither brief nor gate was edited.

## Contracts
None.

## Lens review
Not repeated, as instructed for this whitespace-only round. Round 1 accepted (per round brief).

## Checks run
- Offline build to build/A5-search-bar-overlap-check with AVALONIA_TELEMETRY_OPTOUT=1 and -p:NuGetAudit=false: 0 warnings, 0 errors. Initial --no-restore build succeeded with one cached NU1900 audit warning; the offline rebuild resolved it.
- build/A5-search-bar-overlap-check/AntiNotepad.exe --selftest with isolated build data: exit 0, SELFTEST PASS.
- pwsh -NoProfile -File tools/gate.ps1 -Task A5-search-bar-overlap-r2 -Offline -Keep: GATE FAIL, exit 1. Build PASS (0 warnings), Selftest PASS, Seam PASS, Tokens PASS, Format PASS (4 changed C# files, no format problems on changed lines, 0 older problems).
- Scope FAIL: .coord/tasks/A5-search-bar-overlap-r2.md has no backticked ## Files paths. The gate reads the parent card but requires explicit paths in every round brief. Manager must list the inherited allowed paths in that brief before rerunning the same gate.
- Size WARN: PadWindow.Layout.cs 575 -> 606 lines (accepted by round brief). Build folders retained.
- git diff --check: PASS. Product diff contains only the requested initializer line breaks.

## Hand checks for the user
None (not visible). Original Plan hand checks remain applicable to round 1 behavior.
