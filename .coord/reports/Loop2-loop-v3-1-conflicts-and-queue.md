# Loop2-loop-v3-1-conflicts-and-queue: step 3.1d report

## Summary
The review workflow now routes catch-up conflicts to the item's branch session, with semantic clashes returning to `plan`.
The loop cause list includes `conflict`, and hand tests are ordered oldest accepted item first.

## Change list
| File:lines | What changed | Plan line |
|---|---|---|
| `.claude/skills/review-task/SKILL.md:60` | Add the conflict route, including merge resolution, gate rerun and semantic-clash abort | After 1 |
| `docs/loop.md:78` | Add `conflict` to the triage cause list | After 2, cause list |
| `docs/loop.md:223` | Order hand-test turns by acceptance age | After 2, hand-test sentence |
| `.coord/lessons.md:10` | Add `conflict` to the Misses cause list; leave table rows unchanged | After 3 |

## Behavior changes
- `/review-task` now routes a catch-up conflict to resolution on the item's branch in its session, followed by a merge
  commit and gate rerun. A semantic clash aborts the merge and routes to `plan`.
- Triage recognizes `conflict` as a cause, and accepted items take hand-test turns oldest first.
- No product runtime behavior changes.

## Out-of-scope files
No out-of-scope source files were changed. The required report and generated gate report are coordination artifacts.
Unrelated workspace changes were left untouched, including `.coord/STATUS.md`, `.coord/history-preview.md`, and task cards.

## Plan deviations
None.

## Contracts
None (documentation and workflow rules only).

## Lens review
Worker self-review against all nine lens-review checklist areas: no proven findings. Each hunk serves the approved After;
no product methods, shared state, layers, cleanup paths or product test promises changed. The conflict route and its
semantic-clash abort are stated explicitly.

## Checks run
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline`: GATE PASS; build 0 warnings/errors; SELFTEST
  PASS; CONTRACTS 13 rows (12 pass, 1 manual); seam, format, scope, size and cleanup PASS. Expected offline NuGet audit
  warning.
- `git diff --check`: PASS.
- Scoped `git diff --stat`: three requested files, 4 insertions and 3 deletions. `.coord/STATUS.md` had a pre-existing
  modification and was not changed by this step.

## Hand checks for the user
None (not visible).
