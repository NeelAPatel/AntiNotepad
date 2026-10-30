# Loop2-loop-v3-1-catch-up-points: step 3.1c report

## Summary
Catch-up merges the local base branch and reports whether incoming files touch the item's Files list. Check and Merge
instructions now route catch-up through the item's own session, with a repeated hand test when RETEST says yes.

## Change list
| File:lines | What changed | Plan line |
|---|---|---|
| `tools/catch-up.ps1:24-40` | Parse the checkout card's Files section and print RETEST after success | After 1 |
| `tools/catch-up.ps1:59-78` | Use the local base; note differing origin; report RETEST for CLEAN and UP TO DATE | After 1 |
| `.claude/skills/review-task/SKILL.md:20-22,65-66` | Catch up and push before the gate; repeat the hand test when required | After 2 |
| `.claude/skills/merge-task/SKILL.md:15-17` | Route behind-dev preflight failures to the item's session and wait | After 3 |
| `docs/loop.md:239-241,268-271` | Catch-up paragraph and four script rows only | After 4 |

## Behavior changes
- Local `-Base` (default dev) is authoritative even when origin differs. Fetch remains required; one NOTE line reports a
  differing remote base. A missing local base refuses instead of using origin alone.
- CLEAN and UP TO DATE append RETEST, using exact Files entries with slash normalization, matching files-overlap's
  parser. Only matching incoming filenames appear in RETEST yes. UP TO DATE has no incoming files and prints no.
- CONFLICT and refusals keep exits 2 and 1 and do not print RETEST; success remains exit 0.
- Review catches up before Check; behind-dev Merge waits for the item session to catch up, push and gate again.
  RETEST yes after acceptance requires another hand test; no preserves the earlier result.

## Out-of-scope files
None. This requested report and generated, uncommitted gate output are coordination artifacts. Temporary probe files
under build were removed after verification.

## Plan deviations
The throwaway clone used a local Git bundle as origin: ordinary local-directory cloning failed when Git launched
sh.exe (CreateFileMapping, Win32 error 5) in this sandbox. Bundle cloning and fetch worked with real Git refs and merges.
No implementation scope changes.

## Contracts
None (no product changes).

## Lens review
Fresh lens_reviewer: VERDICT accept; no proven findings or unproven risks. Behavior list matches the diff and all four
implementation files are in plan. Reviewer checked PowerShell syntax, parser parity with files-overlap, scoped diff
formatting and the gate report; reviewed probe results without independently repeating merges.

## Checks run
- Throwaway clone under `build/Loop2-loop-v3-1-probe`, real Git commits and merges:
  - Initial UP TO DATE: exit 0, RETEST no.
  - Local dev ahead of origin, incoming `docs/listed.txt` listed with backslashes in the card: exit 0, exactly one
    `NOTE: origin/dev differs from dev`, CLEAN, RETEST yes (docs/listed.txt); dev confirmed an ancestor of HEAD.
  - Second run after merge: UP TO DATE, exit 0, RETEST no.
  - Incoming `outside.txt` listed only in another card section: CLEAN, exit 0, RETEST no.
  - Conflicting edits: exit 2, CONFLICT filename, MERGE_HEAD present, no RETEST; probe then aborted the merge.
  - Dirty tracked file, wrong branch, missing local base, failed fetch: exit 1, REFUSED, no RETEST.
  - Custom local base `integration` without an origin ref: exit 0, UP TO DATE, RETEST no.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline`: GATE PASS; build 0 warnings/errors;
  SELFTEST PASS; CONTRACTS 13 rows, 12 pass, 1 manual; seam, format, scope, size and cleanup PASS.
  Offline mode reports its expected NuGet-audit warning. No live app test was run.
- `git diff --check`: PASS.

## Hand checks for the user
None (not visible).
