---
name: merge-task
description: Master-session end of the AntiNotepad loop - after the user's hand test passes, merge feat/<ID-slug> into dev with --no-ff, re-run the gate on dev, make the one STATUS commit, push and remove the worktree. Use when the user says "/merge-task <ID-slug>", "merge <ID-slug>", or reports a passed hand test for an accepted item.
argument-hint: <ID-slug>
---

# /merge-task <ID-slug>

Phase 8 (Merge) of `docs/loop.md` ("Branches, worktrees and commits"). Only for an item whose card has `## Check` accepted
and, if it had UI checks, `## Hand test` with the user's pass in their own words. Not for release markers: dev -> main and
tags happen only on the user's explicit go. Every reply opens with the status card.

1. In `.claude/worktrees/<ID-slug>`, check the card: `## Request`, `## Plan`, `## Go`, `## Check` (no open "ask the user"
   line) and `## Hand test` are filled. Missing: stop and tell the user which phase was skipped.
2. Check `git status` and `git diff --name-only dev...feat/<ID-slug>`: only the card's Files and the item's
   `.coord/tasks` and `.coord/reports` files. Anything else (STATUS included): ask the user. Commit anything still
   uncommitted on the branch as `<ID-slug>: <what changed>`.
3. In the master checkout (dev): `git merge --no-ff feat/<ID-slug>`. Conflicts: stop and tell the user.
4. Re-gate on dev: `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>` (scope step is expected to warn on dev; build,
   selftest and seam must pass; the gate report is not committed). Fail: tell the user, do not push.
5. STATUS: the plan row's State cell `done`, merge hash in "Now", gate line, the card's "Noticed" follow-ups as unnumbered
   plan or Backlog rows. `.coord/lessons.md`: copy the card's "Misses" lines, add the item's Scores row (rework rounds,
   gate fails, Codex review proven findings, design review findings, hand-test fails). If a Misses cause and "should
   have caught" pair now appears twice without a rule change, add an unnumbered `idea:` plan row naming the rule or bot
   file to change (e.g. `.agents/skills/lens-review/SKILL.md`) and tell the user; never edit the rule yourself here.
   Commit STATUS and lessons once as `S<n>-status: <ID-slug> merged`; it carries every STATUS edit since the last
   merge. This and the merge are the only commits dev gets.
6. `git push origin dev feat/<ID-slug>`. Remove the worktree (`git worktree remove .claude/worktrees/<ID-slug>`); keep the
   branch until the next release marker.
7. Reply with the status card at Merge ✓: merged hash, gate result, and the next plan item (offer `/next-task`).
