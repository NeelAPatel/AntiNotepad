# T11-roadmap-hide-rows

## Changed
- `tools/roadmap/index.html`: added a hover-only Hide gutter for eligible Plan rows, persistent per-browser hidden IDs in `roadmap-hidden`, Show controls for hidden rows in other Plan filters, and the All count with a hidden-eye count.
- `tools/roadmap-serve.ps1`: added optional `-Label`; it survives background/restart launches and appears as a small header badge. Without a label, the page is unchanged.
- Parent/child association uses the nearest preceding `idea:` row within the same STATUS group because the roadmap data has no explicit parent field. Parent rows can be hidden when all associated `step:` rows are done, dropped, or marked closed/won't fix.

## Checks run
- Extracted inline page script: `node --check` PASS.
- `tools/roadmap-serve.ps1`: PowerShell parser PASS.
- `git diff --check` PASS (Git warned that the pre-existing task card uses LF and may be converted to CRLF).
- `tools/gate.ps1 -Task T11-roadmap-hide-rows -Offline`: PASS (Build, Selftest, Seam, Tokens, Format, Scope, LoopLint, Size).

## Open questions
- The labeled page was not launched here. The planned port 47820 visual comparison and hover/click hand check remain for the user.
- Parent-child grouping follows row order within a group; STATUS currently has no explicit relationship field for validating other arrangements.
