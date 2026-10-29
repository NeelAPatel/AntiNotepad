---
name: merge-task
description: Master-session end of the AntiNotepad loop - after the user's hand test passes, merge feat/<ID-slug> into dev with --no-ff, re-run the gate on dev, make the one STATUS commit, push and remove the worktree. Use when the user says "/merge-task <ID-slug>", "merge <ID-slug>", or reports a passed hand test for an accepted task.
argument-hint: <ID-slug>
---

# /merge-task <ID-slug>

Step 9 of `docs/loop.md` ("Branches, worktrees and commits"). Only for a task whose STATUS state is `accepted` (or
`hand-tested`) and whose hand test the user passed in their own words. Not for release markers: dev -> main and tags happen
only on the user's explicit go.

1. In `.claude/worktrees/<ID-slug>`, check `git status` and `git diff --name-only dev...feat/<ID-slug>`: only the brief's
   Files and the item's `.coord/tasks` and `.coord/reports` files. Anything else (STATUS included): ask the user. Commit
   anything still uncommitted on the branch as `<ID-slug>: <what changed>`.
2. In the master checkout (dev): `git merge --no-ff feat/<ID-slug>`. Conflicts: stop and tell the user.
3. Re-gate on dev: `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>` (scope step is expected to warn on dev; build,
   selftest and seam must pass; the gate report is not committed). Fail: tell the user, do not push.
4. STATUS: the plan row's State cell `done`, merge hash in "Now", gate line, anything deferred (follow-ups go to the plan or
   Backlog as `<ID-slug>` items). Commit STATUS once as `S<n>-status: <ID-slug> merged`; it carries every STATUS edit since
   the last merge. This and the merge are the only commits dev gets.
5. `git push origin dev feat/<ID-slug>`. Remove the worktree (`git worktree remove .claude/worktrees/<ID-slug>`); keep the
   branch until the next release marker.
6. Tell the user with the status card: merged hash, gate result, and the next plan item (offer `/next-task`).
