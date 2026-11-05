# A4g-control-groups-r2

## Summary

Extended the draft with proposed group names, the slider search, and source-checked Settings preview and editor chip rows. The requested complete control map remains unfinished and is not ready for the naming checkpoint or Part 2.

## Change list

| File | Change |
| --- | --- |
| `.coord/reports/A4g-control-groups-map.md` | Added preview and editor measurements, proposed names, and explicit gaps. |
| `.coord/reports/A4g-control-groups-r2.md` | This report. |
| `.coord/reports/A4g-control-groups-r2.gate.md` | Generated gate report. |

## Behavior changes

None (report only).

## Out-of-scope files

None. The completion and gate reports are requested by the rework instructions and gate.

## Plan deviations

The map is incomplete: per-instance tables and disagreements for all groups still need a full source and template pass. No separate design dump was run. The draft disclaimer remains.

## Contracts

None.

## Lens review

No independent lens review was performed. The remaining completeness gap is explicitly recorded in the map and this report.

## Checks run

- Offline gate: GATE PASS. Build, Selftest, Seam, Tokens, Format, Scope, Size and Cleanup PASS; LoopLint SKIP. Offline NuGet audit was disabled for the build.
- Scope reported zero tracked changed files; ignored report files are not inventoried by that count.

## Hand checks for the user

None (report only).
