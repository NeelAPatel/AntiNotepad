# Loop2-loop-v3-1-rehearsal

Step 3.1i: the v3.1 tools run together once, in a throwaway clone. Run by the builder session itself (scripts only);
Codex only for check 7. Nothing in the main checkout changed except this report. Nothing was pushed to the real origin.

## Summary
Eight checks run; all PASS. One expectation in the brief did not match the tool's check order (check 3, below), and one
claim in check 7 is weaker than it looks. No live `/scan-backlog` with Haiku scouts was run.

## Setup
- `git clone --no-hardlinks` of the main checkout into `build/Loop2-loop-v3-1-rehearsal` (commit `68b4d9b`, which carries
  steps 3.1a to 3.1h); `dev` there created from `origin/chore/Loop2-loop-v3-1`; local `user.name` / `user.email` set.
- `origin` pointed at a bare copy `build/Loop2-loop-v3-1-rehearsal-origin.git`, so fetch and push stayed offline.
- A small `.coord/STATUS.md` with the Plan rows `T90-rehearsal-item` (`plan ready`) and `idea: rehearsal-scan-a` was committed
  on the clone's `dev`.
- The clone built and ran the selftest for real (gate runs below include `Build` and `Selftest` PASS, not skips).

## Checks
| # | Command (in the clone) | Expected | Actual | Result |
|---|---|---|---|---|
| 1 | `pwsh -NoProfile -File tools/approve.ps1 -Task T90-rehearsal-item -Prefix chore` (card from the item template, Files = `docs/decisions.md`, Approve filled) | `APPROVE: PASS`, worktree at `.claude/worktrees/chore/T90-rehearsal-item`, card committed there | `FILES-OVERLAP: PASS`, worktree created, `git commit -m "T90-rehearsal-item: card"` in it, `APPROVE: PASS (card committed)`, exit 0 | PASS |
| 2a | In the worktree: edit `docs/decisions.md` and a stray `docs/agents.md`; `tools/gate.ps1 -Task T90-rehearsal-item -Offline` | `Scope: FAIL` | `[FAIL] Scope: 1 changed file(s) outside the brief's Files list`, `GATE: FAIL` (Build, Selftest, Seam, Format, LoopLint PASS) | PASS |
| 2b | `git checkout -- docs/agents.md`; same gate | `Scope: PASS` | `[PASS] Scope: 1 changed file(s); all are allowed by the brief`, `GATE: PASS`; commit `T90-rehearsal-item: docs line` | PASS |
| 3 | Commit an unrelated change (`docs/coding.md`) on the clone's `dev`; `tools/merge-preflight.ps1 -Task T90-rehearsal-item` | `FAIL (behind dev ...)` | First run: `MERGE-PREFLIGHT: FAIL (Check section missing or unfilled)`. After I filled the card's Check and Hand test and committed: `MERGE-PREFLIGHT: FAIL (behind dev: run tools/catch-up.ps1 -Task T90-rehearsal-item)` | PASS, see note 1 |
| 4a | In the worktree: `tools/catch-up.ps1 -Task T90-rehearsal-item` | `CLEAN`, `RETEST: no` | `CLEAN: merged dev (1 incoming file(s))`, `incoming docs/coding.md`, `RETEST: no`, exit 0; merge commit `T90-rehearsal-item: catch up with dev` | PASS |
| 4b | Gate again in the worktree | `GATE: PASS`, report `Head:` = new tip | `GATE: PASS`, `Head: 181074544752fd680798fbcbf5bd9bf75528d21d`, equal to `git rev-parse HEAD`; `LoopLint: SKIP` (no loop files changed) | PASS |
| 5a | `tools/merge-preflight.ps1 -Task T90-rehearsal-item` from the clone's dev | `MERGE-PREFLIGHT: PASS` | Branch card phases filled; item checkout clean and caught up; newest gate PASS at `1810745...`; `Main checkout on dev: squash can run`; `MERGE-PREFLIGHT: PASS (ready)` | PASS |
| 5b | `git merge --squash chore/T90-rehearsal-item`, commit, `git worktree remove`, `git branch -D` | one new commit on dev | dev commit count 168 to 169; commit `T90-rehearsal-item: add decisions line` (2 files); `git worktree remove` without `--force` succeeded even with the ignored gate report present; branch deleted | PASS |
| 6 | `.coord/research/rehearsal-scan-a.md` (Scanned at = dev commit before check 3; Where = `docs/coding.md`, `docs/glossary.md`); `tools/scan-delta.ps1 -Slug rehearsal-scan-a` | check 3 commit listed, its file `WHERE`, `DELTA: where (1 of 2 ...)`, `RESCAN: no` | COMMITS lists the squash and the check 3 commit; `WHERE docs/coding.md`; `NEAR docs/decisions.md`; CARDS lists `.coord/tasks/T90-rehearsal-item.md`; `RESCAN: no`; `DELTA: where (1 of 2 Where files changed)`; exit 0 | PASS |
| 7 | Second worktree `.claude/worktrees/chore/T91-role-check`; `CODEX_WORKDIR=<it> CODEX_SANDBOX=read-only bash .coord/tools/codex-run.sh worker T91-role-check <brief>` | Codex lists the project roles; record whether `lens_reviewer` is listed; record any "untrusted" line | Exit 0. Codex reported "The project role name is `lens_reviewer`", spawned nothing, edited nothing. No untrusted-folder or "project `.codex/` not loaded" message appeared in the log | PASS, see note 2 |
| 8 | `pwsh -NoProfile -File tools/loop-lint.ps1` in the clone | `LOOP-LINT: PASS` | `LOOP-LINT: PASS (37 files)`, exit 0 | PASS |

## Surprises
1. **Check 3 order.** `merge-preflight.ps1` checks the card's sections before it checks whether dev is an ancestor. With the
   brief's step order (Check and Hand test filled only in check 4) the first message is the unfilled-section FAIL, not
   "behind dev". The "behind dev" line itself works: it appeared as soon as the card was filled. The order is reasonable;
   only the rehearsal's expectation was off. I filled the card before check 4 instead of after, and recorded it.
2. **Check 7 is weaker than "listed".** The Codex log shows it answered by reading the `.codex/agents/` definitions and
   its own notes; it did not show a runtime list of spawnable roles. So this proves the nested worktree has the file and
   raises no trust warning, not that `lens_reviewer` spawns there. A direct proof needs a director run that spawns it.
3. `docs/decisions.md` shows as `NEAR` in the scan delta (same folder as a Where file), as designed.
4. The whole clone, including builds and a real selftest, worked from a path inside `build/` under the main checkout. The
   Codex wrapper counted its run in the clone's `.coord/logs`, not the main checkout's, because it finds the ledger through
   `git-common-dir`.

## Out-of-scope files
None. This report only.

## Plan deviations
Check 3 order (surprise 1). Otherwise none.

## Contracts
None.

## Lens review
Not run (scripts only, no new code in this step).

## Checks run
The table above. No product code was built outside the clone's own gate runs.

## Hand checks for the user
None. A live `/scan-backlog` with Haiku scouts was not run here.

## Cleanup
Both throwaway folders, `build/Loop2-loop-v3-1-rehearsal` and `build/Loop2-loop-v3-1-rehearsal-origin.git`, are deleted
(`ls build | grep rehearsal` is empty). The first `rm` reported "Device or resource busy" on the clone, and a second attempt
a few seconds later removed it. The main checkout's `git status` shows no branch, worktree or file from the rehearsal.
