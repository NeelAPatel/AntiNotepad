---
name: review-task
description: Run the AntiNotepad review pipeline on a finished Codex task - gate script, then the reviews its tier calls for, then a verdict. Use when the user or manager says "/review-task <ID-slug>", "review <ID-slug>", or a Codex implementation run for a task has just exited.
argument-hint: <ID-slug>
---

# /review-task <ID-slug>

Runs steps 6-7 of the loop in `docs/loop.md`. Keep Claude reading small: read report sections and verdict lines, not whole
diffs. Never fix product code here; rework goes back to Codex with a `<ID-slug>-rN` brief.

## 1. Read the tier
From `.coord/tasks/<ID-slug>.md`, the `Tier:` line (Low, Medium, High). Missing tier: treat as Medium and say so.
Check that `.coord/reports/<ID-slug>.md` exists and has the "Change list" and "Behavior changes" sections; if not, that is a
rework (the report format is part of the task).

## 2. Gate (all tiers)
`<round>` is the name of the round just finished: `<ID-slug>` for the first, `<ID-slug>-rN` after a rework. Run
`pwsh -NoProfile -File tools/gate.ps1 -Task <round>` from the worktree that holds the change. It writes
`.coord/reports/<round>.gate.md` (not committed) and exits non-zero on failure. Read only its Summary section.
- Fail: write `.coord/tasks/<ID-slug>-rN.md` (next N) from `.coord/templates/brief.md` quoting the failing lines, relaunch
  Codex (`docs/agents.md`), update STATUS (`rework rN`), stop.
- Warnings (scope, size): carry them into step 4.

## 3. Reviews by tier
- **Low**: none. Go to step 4.
- **Medium**: fresh Codex review. Fill `.coord/templates/review.md` into `.coord/tasks/<ID-slug>-review.md` (base branch
  `dev` unless the brief says otherwise), then launch in the background:
  `bash .coord/tools/codex-run.sh reviewer <ID-slug>-review .coord/tasks/<ID-slug>-review.md`
  Tell the user in one line: working folder, brief file, log `.coord/logs/<ID-slug>-review.reviewer.log`, report
  `.coord/reports/<ID-slug>.review.md`. Wait for the exit notification; do not poll.
  After a rework, name the review `<round>-review`, list both the base report and `.coord/reports/<round>.md` as inputs,
  and point it at `.coord/reports/<round>.gate.md`.
- **High**: the Medium step, then spawn the `design-reviewer` agent (Agent tool, `subagent_type: design-reviewer`) with the
  prompt "Design review of <ID-slug>, latest round <round>." after the Codex review report exists.

## 4. Verdict
Read the `VERDICT:` line and "Findings (proven)" of the Codex review, and the design reviewer's reply.
- Any `rework`, a blocker/major finding, or a report "Behavior changes" entry the review says is missing: write the rework
  brief `<ID-slug>-rN` with exactly those findings and relaunch Codex; when it exits, run this skill again for that round.
  Max two rework rounds; a third goes to the user.
- `ask-user`: put the one question to the user.
- Otherwise accept. Append to `.coord/reports/<ID-slug>.md` a short `## Manager verdict` (tier, gate result, review verdicts,
  scope/size warnings accepted and why).

## 5. Hand over
Update `.coord/STATUS.md` (state `accepted`, `rework rN`, or `ask-user`). For an accepted task give the user the report's
"Hand checks for the user" list for the dev copy (`tools/dev.ps1`), or say none is needed (Low tier and not visible).
Merging into dev is the master session's step 9, after the user's hand test passes.
