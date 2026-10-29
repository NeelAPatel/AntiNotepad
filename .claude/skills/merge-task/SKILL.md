---
name: merge-task
description: Master-session end of the AntiNotepad loop - after the user's hand test passes, commit the task, merge feat/<ID-slug> into dev with --no-ff, re-run the gate on dev, update STATUS and push. Use when the user says "/merge-task <ID-slug>", "merge <ID-slug>", or reports a passed hand test for an accepted task.
argument-hint: <ID-slug>
---

# /merge-task <ID-slug>

Step 9 of `docs/loop.md`. Only for a task whose STATUS state is `accepted` (or `hand-tested`) and whose hand test the user
passed in their own words. Not for release markers: dev -> main and tags happen only on the user's explicit go.

1. In the task's folder, check `git status`: only the brief's Files, its `.coord` brief/reports and STATUS may be changed.
   Anything else: ask the user.
2. Commit the feature as one commit `<ID-slug>: <what changed>` (code, selftest, its brief and reports). STATUS and docs go
   in a separate `S<n>-status:` commit, never bundled (repo rule).
3. Switch to dev, `git merge --no-ff feat/<ID-slug>`. Conflicts: stop and tell the user.
4. Re-gate on dev: `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>` (scope step is expected to warn on dev; build,
   selftest and seam must pass; the gate report is not committed). Fail: tell the user, do not push.
5. STATUS: the plan row's State cell `done`, merge hash in "Now", gate line, anything deferred (follow-ups go to the plan or Backlog as `<ID-slug>`
   items). Commit it.
6. `git push origin dev feat/<ID-slug>`. Remove the task worktree if one was used; keep the branch unless the user says to
   delete it.
7. Tell the user in two lines: merged hash, gate result, and the next plan item (offer `/next-task`).
