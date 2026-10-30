---
name: merge-task
description: Master-session end of the AntiNotepad loop - after the user's hand test passes, squash <prefix>/<ID-slug> onto dev together with the item's STATUS edits as one commit, re-run the gate on dev, push, remove the worktree and delete the branch. Use when the user says "/merge-task <ID-slug>", "merge <ID-slug>", or reports a passed hand test for an accepted item.
argument-hint: <ID-slug>
---

# /merge-task <ID-slug>

Phase 8 (Merge) of `docs/loop.md` ("Branches, worktrees and commits"). Only for an item whose card has `## Check` accepted
and, if it had UI checks, `## Hand test` with the user's pass in their own words. Not for release markers: moving `main`
and tagging happen only on the user's explicit go. Every reply opens with the status card.
`<branch>` is `<prefix>/<ID-slug>`; `<wt>` is `.claude/worktrees/<prefix>/<ID-slug>`.

1. In `<wt>`, check the card: `## Request`, `## Plan`, `## Approve` (`## Go` on era-one cards), `## Check` (no open
   "ask the user" line) and `## Hand test` are filled. Missing: stop and tell the user which phase was skipped.
2. Check `git status` and `git diff --name-only dev...<branch>`: only the card's Files and the item's `.coord/tasks` and
   `.coord/reports` files. Anything else (STATUS included): ask the user. Commit anything still uncommitted on the branch.
3. In the master checkout (dev): `git merge --squash <branch>`. Conflicts: stop and tell the user (`git merge --abort`
   leaves dev as it was).
4. Re-gate on dev: `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>` (scope step is expected to warn on dev; build,
   selftest and seam must pass; the gate report is not committed). Fail: tell the user, `git reset --merge`, do not push.
5. STATUS: the plan row's State cell `done`, "Now" updated, gate line, the card's "Noticed" follow-ups as unnumbered
   plan or Backlog rows. `.coord/lessons.md`: copy the card's "Misses" lines, add the item's Scores row (rework rounds,
   gate fails, Codex review proven findings, design review findings, hand-test fails). If a Misses cause and "should
   have caught" pair now appears twice without a rule change, add an unnumbered `idea:` plan row naming the rule or bot
   file to change (e.g. `.agents/skills/lens-review/SKILL.md`) and tell the user; never edit the rule yourself here.
6. Commit once: `git add .coord/STATUS.md .coord/lessons.md` and `git commit`, subject `<ID-slug>: <what changed>` (72
   characters or fewer), body of 3-6 short lines: why, what changed, the gate result, the hand test. This one commit
   carries the item and every STATUS edit since the last commit to dev; it is the only commit dev gets for the item.
7. `git push origin dev`. Remove the worktree with `git worktree remove <wt>` (never `--force`): if it refuses because of
   leftover files, stop, list them for the user and wait. Then delete the branch: `git branch -D <branch>` (a squash does
   not mark it merged) and `git push origin --delete <branch>`.
8. Reply with the status card at Merge ✓: the commit hash, gate result, and the next plan item (offer `/next-task`).
