# Loop2-loop-v3-1-rehearsal: step 3.1i, run the v3.1 tools together once

Step brief of `Loop2-loop-v3-1` (card `.coord/tasks/Loop2-loop-v3-1.md`, trimmed rehearsal in "Round 2 scope change").
Run by the builder session itself (Claude, scripts only); Codex only for check 7. Nothing in this checkout changes
except this step's report.

## Setup
A throwaway clone: `git clone --no-hardlinks "<main checkout>" build/Loop2-loop-v3-1-rehearsal` (inside `build/`, which is
ignored). In the clone: `git switch -c dev origin/chore/Loop2-loop-v3-1` so `dev` there carries all v3.1 tools; set a
local `user.name`/`user.email`; point `origin` at a bare copy `build/Loop2-loop-v3-1-rehearsal-origin.git` so fetch and
push work offline. Give the clone a small `.coord/STATUS.md` Plan table with two rows: `T90-rehearsal-item` (`plan ready`)
and `idea: rehearsal-scan-a`. Delete both folders at the end.

## Checks (record each command and its last lines in the report)
1. **Approve:** card `.coord/tasks/T90-rehearsal-item.md` from `.coord/templates/item.md`, Files = one docs file, Approve
   filled. `tools/approve.ps1 -Task T90-rehearsal-item -Prefix chore` -> `APPROVE: PASS`, worktree at
   `.claude/worktrees/chore/T90-rehearsal-item`, card committed there.
2. **Strict scope:** in the worktree, edit the docs file and one file not in Files; gate
   `tools/gate.ps1 -Task T90-rehearsal-item -Offline` -> `Scope: FAIL` (build steps may be skipped if the clone cannot
   build; say so). Revert the stray file; gate -> `Scope: PASS`. Commit.
3. **Behind dev:** commit an unrelated change on clone `dev`. `tools/merge-preflight.ps1 -Task T90-rehearsal-item` ->
   `FAIL (behind dev ...)`.
4. **Catch-up:** in the worktree `tools/catch-up.ps1 -Task T90-rehearsal-item` -> `CLEAN`, `RETEST: no`. Re-run the gate
   (gate report `Head:` = new tip). Fill the card's Check and Hand test (`None (not visible)`), commit.
5. **Merge preflight passes** -> `MERGE-PREFLIGHT: PASS`. Then do the real squash on clone `dev`
   (`git merge --squash`, commit), `git worktree remove`, `git branch -D`. Confirm dev has one new commit.
6. **Scan and delta:** hand-write `.coord/research/rehearsal-scan-a.md` from `.coord/templates/scan.md` with
   `Scanned at:` = a dev commit before check 3 and a Where list naming the file changed in check 3 and one other file.
   `tools/scan-delta.ps1 -Slug rehearsal-scan-a` -> the check 3 commit listed, its file marked `WHERE`, `DELTA: where (1 of
   2 ...)`, `RESCAN: no`. (A live `/scan-backlog` with Haiku scouts is not run here; say so.)
7. **Codex role in a nested worktree:** create a second throwaway worktree `.claude/worktrees/chore/T91-role-check` in
   the clone and run `bash .coord/tools/codex-run.sh worker T91-role-check <brief>` there (`CODEX_WORKDIR` = that
   worktree), brief: "List the project agent roles you can spawn (from `.codex/agents/`), spawn none, report the list."
   Record whether `lens_reviewer` is listed. If Codex reports the folder is untrusted, record that line exactly; do not
   change any trust setting.
8. **Lint:** `tools/loop-lint.ps1` in the clone -> `LOOP-LINT: PASS`.

## Done means
Report `.coord/reports/Loop2-loop-v3-1-rehearsal.md`: one row per check (command, expected, actual, PASS/FAIL), anything
that surprised you, and confirmation that both throwaway folders are deleted. A FAIL is reported, not fixed.

## Constraints
Never change this checkout except the report (and the check 7 Codex log/last-message files the wrapper writes in the
main `.coord/`). Never push to the real origin. Never launch or kill AntiNotepad.
