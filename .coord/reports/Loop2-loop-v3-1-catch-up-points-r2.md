# Loop2-loop-v3-1-catch-up-points-r2: step 3.1c rework report

## Summary
Incoming and conflicting paths now use Git's NUL-delimited literal filenames. Listed non-ASCII incoming paths correctly
require a repeated hand test, and CLEAN/CONFLICT filename listings no longer contain Git quoting.

## Change list
| File:lines | What changed | Plan line |
|---|---|---|
| `tools/catch-up.ps1:71-73` | Read incoming paths with `-z`, split on NUL after checking exit status | Rework fix; After 1 |
| `tools/catch-up.ps1:82-84` | Read conflicting paths with `-z`, split on NUL after checking exit status | Rework fix; After 1 |

## Behavior changes
- RETEST compares literal incoming filenames with the card's existing normalized Files entries.
- CLEAN incoming and CONFLICT lists display literal filenames. Exit codes and other output remain unchanged.
- No encoding setting changed: native Git output round-tripped `docs/café.md` under this host's existing UTF-8 console
  and pipeline encodings.

## Out-of-scope files
Only this requested report and regenerated gate output. Disposable probe seed, clone, bundle and harness were under
`build/Loop2-loop-v3-1-probe`; cleanup is blocked (see Plan deviations). Existing checkout changes were preserved.

## Plan deviations
The throwaway clone used a local bundle origin, as in the original step's probe. Automatic approval review rejected
recursive deletion of the verified probe directory with "blocked by policy", including a retry with the exact literal
absolute path. Its `seed/`, `clone/`, `origin.bundle` and `probe.ps1` remain under `build/Loop2-loop-v3-1-probe`.
No implementation deviations.

## Contracts
None (no product changes).

## Lens review
Fresh lens_reviewer: VERDICT accept; no proven findings or retained risks. Independently checked Unicode, multiple-path
and empty native Git output parsing, PowerShell syntax and diff whitespace. Reviewed the probe harness and gate report;
did not independently repeat builds or merges.

## Checks run
- Real-Git bundle clone under `build/Loop2-loop-v3-1-probe`, with `core.quotePath=true`:
  - Listed `docs/café.md`: CLEAN, exit 0, literal incoming listing, `RETEST: yes (docs/café.md)`.
  - Listed ASCII `docs/listed.md` (card uses backslashes): CLEAN, exit 0, RETEST yes.
  - `docs/unlisted.md` listed only in another card section: CLEAN, exit 0, RETEST no.
  - Initial and post-merge UP TO DATE: exit 0, RETEST no.
  - All CLEAN cases used local dev ahead of origin; NOTE printed and dev became an ancestor of HEAD.
  - Conflicting `docs/café.md`: exit 2, literal CONFLICT listing, MERGE_HEAD present, no RETEST; merge then aborted.
  - Dirty tracked file: REFUSED, exit 1, no RETEST.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline`: GATE PASS; build 0 warnings/errors;
  SELFTEST PASS; CONTRACTS 13 rows, 12 pass, 1 manual; seam, format, scope, size and cleanup PASS.
- PowerShell AST: 0 errors; scoped `git diff --check`: PASS. Probe cleanup: blocked by policy.

## Hand checks for the user
None (not visible).
