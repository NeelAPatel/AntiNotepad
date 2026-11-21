Read by /merge-task for branch, worktree, commit and catch-up detail.

## Branches, worktrees and commits (user, 2026-10-29)
- **Umbrellas:** on the user's go, child branches start from their parent branch. The child reviews commits, rebases onto the parent when ready, and gates against it; the parent runs preflight with `-Base <parent-branch>` and merges `--no-ff` without STATUS edits. Record queue, order and landed on `.coord/tasks/<umbrella>-board.md`. Once all children have landed, the director reviews and merges the umbrella to dev once using the normal STATUS and lessons procedure.
- **`main`** holds releases only. At a release, on the user's go, it fast-forwards to `dev` (no merge commits) and gets
  a plain semver tag: `v0.7.0`, with `-rc1`, `-rc2` for builds not yet hand-tested. No pull requests for now; they come
  back with the release pipeline.
- **`dev`** is integration: each item (or lettered step) lands as its own tidy commits plus one `--no-ff` merge commit
  that carries its STATUS and lessons edits. No other commits.
- **Every item gets a branch** `<prefix>/<ID-slug>` off the latest dev, created at **Approve** (not before), including
  docs and tooling items. The prefix follows the kind of change: `feat/`, `fix/` or `refactor/` for A, `docs/` for D,
  `chore/` for Loop and T. Rework rounds stay on the same branch.
- **Worktree per item, by default** (user, 2026-11-06: "worktree preferred for parallelization unless explicitly said not
  to"), in a folder that mirrors the branch: `.claude/worktrees/<prefix>/<ID-slug>`
  (`git worktree add .claude/worktrees/feat/A1-list-commands -b feat/A1-list-commands dev`; also fix/, refactor/, docs/, chore/). The master's checkout stays
  on dev. The item chat starts in the main checkout (`use_worktree: false`), then works only inside its worktree path, with absolute
  paths; Codex runs from the worktree's own `.coord/tools/codex-run.sh`. **At most 3 items** past Approve at once, and
  only if their Files lists do not overlap. After Check accepts, the item chat runs `tools/dev.ps1`
  from the worktree immediately before the hand checks; it verifies the slot's badge and EXE/DATA/log paths. Each worktree runs its own colored slot copy (up to six), so up to
  3 items can build and hand test at the same time.
  Without worktrees there is no parallel work at all (user, 2026-10-30).
- **No worktree** (only when the user says so; never suggested for docs or tooling items): the branch is checked out in
  the main checkout (`tools/approve.ps1 -NoWorktree`) and nothing else runs until it merges.
- **Before Approve** there is no branch: the card and any audit brief and report live untracked in the main checkout;
  the item chat's read-only Codex audit runs there. At Approve they move into the new worktree and are committed on the branch.
- **What lands on the branch:** the card, audit, reports and code, committed there as often as useful (Codex cannot
  commit: the item chat does). Push the branch to origin freely. Branches never edit STATUS.
- **What lands on dev: the item's tidy commits plus one merge commit** (user, 2026-11-06: "so that i can atleast trace
  out which incremental change broke something"; "i'm more worried about slop commits than high number of comits").
  Between merges the master edits `.coord/STATUS.md` on dev and leaves it uncommitted (the roadmap reads the working
  copy). No other direct commits to dev; urgent one-line fixes also go through a branch. At Merge, in this order:
  1. **Commit review**, by the item's own session before it asks for a merge: list `git log dev..<branch>` and fold
     tiny and back-and-forth commits into the commit they belong to (`git commit --fixup`, then
     `GIT_SEQUENCE_EDITOR=: git rebase -i --autosquash dev`; `-i` itself is not available here). No fixed count: one
     commit per meaningful change, and a few lines fixed after some back and forth is not a commit. Messages say what
     changed (`<ID-slug>: <what>`), never wip, fix or try-again. Lab variants and debug code are removed. Every commit
     builds.
  2. **Rebase** the branch onto dev (`tools/catch-up.ps1`, rebase only; merge from the local branch, no force-push), then re-run the gate: it must pass at the final tip.
  3. **Preflight**: `tools/merge-preflight.ps1` also builds each commit of `dev..<branch>` and runs `--selftest`,
     skipping commits that touch no product code. A failure names the commit.
  4. **Merge**: the master runs `git merge --no-ff --no-commit <branch>` on dev, adds the item's STATUS edits and its
     lessons, then commits once. The merge commit's subject is `<ID-slug>: <what changed>`.
  STATUS rides in the item's merge commit, so there are no separate `chore: status` commits; with nothing to merge it
  stays uncommitted.
- **Commit message:** the branch's commits: subject `<ID-slug>: <what changed>`, 72 characters or fewer, body only when
  the why is not obvious. The merge commit: the same subject and a body of 3-6 short lines: why, what changed, the gate
  result, the hand test. No Claude attribution lines (`.claude/settings.json`).
- **Clean-up at Merge:** delete any untracked `.coord/reports/*.codex.md` files in the worktree (known leftovers from the
  older wrapper); tracked files are never touched. Then remove the worktree (`git worktree remove`, never `--force`:
  any other leftover files stop the merge and are listed for the user), then delete the branch locally (`git branch -d`, which works because the merge marks it merged)
  and on origin.

**Catching up with dev:** rebase, never merge (user, 2026-11-06). Catch up at two points: before Check and before Merge
when dev is not an ancestor of HEAD. The item's own session runs `tools/catch-up.ps1`, which rebases the item branch onto dev (no push, never a force-push), then runs the gate. A conflict leaves the rebase open: resolve, `git add`, `git rebase --continue`. `RETEST: yes` repeats the hand test
before Merge; `RETEST: no` keeps the earlier result.
