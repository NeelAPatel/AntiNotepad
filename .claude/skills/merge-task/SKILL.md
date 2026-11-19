---
name: merge-task
description: Merge an accepted AntiNotepad item to dev, re-gate, push, and clean up its branch and worktree.
disable-model-invocation: true
shell: powershell
allowed-tools: PowerShell(git *)
argument-hint: <ID-slug>
---

# /merge-task <ID-slug>

Phase detail: `.claude/skills/merge-task/branches.md`.

## Live facts

!`git branch --show-current`
!`git rev-list --count dev..HEAD`
!`git status --short`

Branch, commits ahead of dev and changed files are above; steps below need not re-run them.

Phase 8 (Merge) of `docs/loop.md` (.claude/skills/merge-task/branches.md). Only for an item whose card has `## Check` accepted
and, if it had UI checks, `## Hand test` with the user's pass in their own words. Not for release markers: moving `main`
and tagging happen only on the user's explicit go. Every reply opens with the status card.
`<branch>` is `<prefix>/<ID-slug>`; `<wt>` is `.claude/worktrees/<prefix>/<ID-slug>`.

The director sets the item chat title to `[Merged] <ID-slug>` when merging.

0. **Commit review (the item's session, before it asks for this merge).** Ask the item's session whether it has done it;
   if not, it does it now: `git log --oneline dev..<branch>`, fold tiny and back-and-forth commits into the commit they
   belong to (`git commit --fixup <sha>`, then `GIT_SEQUENCE_EDITOR=: git rebase -i --autosquash dev`), reword vague
   messages to `<ID-slug>: <what changed>`, drop lab variants and debug code, then `tools/catch-up.ps1 -Task <ID-slug>` (rebase only; merge runs from the local branch, no force-push) and re-run the gate at the new tip. No fixed
   count: one commit per meaningful change. Never rebase or force-push from the master checkout.
1. From the master checkout, run `pwsh -NoProfile -File tools/merge-preflight.ps1 -Task <ID-slug>`. It also builds
   each commit of `dev..<branch>` that touches product code and runs `--selftest`; a failure names the commit, and the
   item's session fixes or folds it (step 0). FAIL with "behind dev": ask the item's session to catch up (it runs
   `/review-task` step 2 again) and wait; never rebase or merge dev into an item branch from the master checkout. Other
   FAIL: stop and tell the user the reason. For a `-NoWorktree` item, switch to dev before step 3 as the script directs.
2. Check `git diff --name-only dev...<branch>`: only the card's Files and the item's `.coord/tasks` and
   `.coord/reports` files. Anything else (STATUS included): ask the user.
3. In the master checkout (dev): `git merge --no-ff --no-commit <branch>`. Conflicts: stop and tell the user
   (`git merge --abort` leaves dev as it was).
4. Re-gate on dev: `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>` (scope step is expected to warn on dev; build,
   selftest and seam must pass; the gate report is not committed). Fail: tell the user, `git merge --abort`, do not push.
5. STATUS: move the item's plan row from STATUS to the "Finished rows" table in `.coord/status-archive.md` (State `done`); remove its "Now" line (STATUS keeps active items only), gate line, any of the card's "Noticed" follow-ups not
   already in STATUS as unnumbered plan or Backlog rows. `.coord/lessons.md`: copy the card's "Misses" lines, add the item's Scores row (rework rounds,
   gate fails, Codex review proven findings, design review findings, hand-test fails). If a Misses cause and "should
   have caught" pair now appears twice without a rule change, add an unnumbered `idea:` plan row naming the rule or bot
   file to change (e.g. `.agents/skills/lens-review/SKILL.md`) and tell the user; never edit the rule yourself here.
6. Commit once: `git add .coord/STATUS.md .coord/lessons.md` and `git commit`, subject `<ID-slug>: <what changed>` (72
   characters or fewer), body of 3-6 short lines: why, what changed, the gate result, the hand test. This merge commit
   carries every STATUS edit since the last commit to dev; the branch's own commits stay as they are, and dev gets no
   separate `chore: status` commit.
7. `git push origin dev`. In the worktree, delete any untracked `.coord/reports/*.codex.md` files (known leftovers from
   the older wrapper); tracked files are never touched. Remove the worktree with `git worktree remove <wt>` (never
   `--force`): if it refuses because of any other leftover files, stop, list them for the user and wait. Then delete the branch: `git branch -d <branch>` (it works
   because the merge marks the branch merged) and `git push origin --delete <branch>`.
8. **Version bump:** if this item bumped the loop version (`docs/loop.md`, .claude/skills/merge-task/branches.md), the merging director also (a)
   updates the STATUS "Loop vX.Y.Z is live" line and the three director names in STATUS "Working rules", and (b) renames
   the three live director chats (Master 4, Loop Hub 2, Roadmap Hub 1) to the new `LP<vX.Y.Z>`.
9. Reply with the status card at Merge ✓: the merge commit hash, gate result, and the next plan item (offer `/next-task`).
