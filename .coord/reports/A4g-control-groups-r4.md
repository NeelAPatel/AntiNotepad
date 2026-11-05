# A4g-control-groups-r4

## Summary
Revised the control map with concrete comparisons, grouped the inspected previews, swatches and editor chip, and replaced variant token proposals with one row per group. The map records remaining coverage and verification gaps; unequal values await design-questions.

## Change list
| File | Change |
| --- | --- |
| `.coord/reports/A4g-control-groups-map.md` | Revised group comparisons, instance placement, token proposals and gaps. |
| `.coord/reports/A4g-control-groups-r4.md` | This report. |

## Behavior changes
None (report only).

## Out-of-scope files
None.

## Plan deviations
None.

## Contracts
None.

## Lens review
Not run; this round changes a report only.

## Checks run
- Offline gate: **PASS**; Build, Selftest, Seam, Tokens, Format, Scope, Size and Cleanup passed; LoopLint skipped.
- `git diff --name-only feat/A4-design-tokens -- product/AntiNotepad`: no output.

## Hand checks for the user
None (report only).
