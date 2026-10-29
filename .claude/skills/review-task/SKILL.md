---
name: review-task
description: Run the AntiNotepad Check phase on a finished Codex round - the gates the item card planned, then the bounds check (request vs plan vs done), then a verdict and the hand-test handover. Use when the user or manager says "/review-task <ID-slug>", "review <ID-slug>", or a Codex Build run for an item has just exited.
argument-hint: <ID-slug>
---

# /review-task <ID-slug>

Phase 6 (Check) of `docs/loop.md`, then the hand-over to Hand test. Every reply opens with the status card. Keep Claude
reading small: report sections and verdict lines, not whole diffs. Never fix product code here; rework goes back to Codex.

## 1. Read the card
From `.coord/tasks/<ID-slug>.md`: `Tier:`, `## Request`, `## Plan` (After, Must not change, Gates, Test plan), `## Go`.
Missing Plan or Go: stop and tell the master the item skipped phases. Missing tier: treat as Medium and say so.
`<round>` is the round just finished: `<ID-slug>` first, `<ID-slug>-rN` after a rework. Check that
`.coord/reports/<round>.md` has the "Change list", "Behavior changes" and "Plan deviations" sections; if not, that is a rework.

## 2. Gate (always)
`pwsh -NoProfile -File tools/gate.ps1 -Task <round>` from the worktree. It writes `.coord/reports/<round>.gate.md` (not
committed) and exits non-zero on failure. Read only its Summary.
- Fail: write `.coord/tasks/<ID-slug>-rN.md` (next N) from `.coord/templates/round.md` quoting the failing lines, relaunch
  Codex (`docs/agents.md`), STATUS `rework rN`, stop.
- Warnings (scope, size): carry them into step 4.

## 3. The card's other gates
Run every gate the Plan lists (at least the tier's defaults):
- **Codex review** (Medium, High): fill `.coord/templates/review.md` into `.coord/tasks/<round>-review.md`, launch
  `bash .coord/tools/codex-run.sh reviewer <round>-review .coord/tasks/<round>-review.md` in the background, tell the user
  in one line (folder, brief, log, report `.coord/reports/<round>.review.md`), wait for the exit notification.
- **Design review** (High): after the Codex review exists, spawn the `design-reviewer` agent (Agent tool,
  `subagent_type: design-reviewer`) with "Design review of <ID-slug>, latest round <round>."
- **Contracts / reproduction test**: confirm the named rows and checks exist in the diff and pass in the gate's selftest.

## 4. Bounds check and verdict
Compare Request, Plan (and Go) with what was done: the report's Change list, Behavior changes, Out-of-scope files, Plan
deviations, and `git diff --stat dev...HEAD`. Write `## Check` on the card:
- Gate and review result lines.
- One line per difference: **within plan**, **justified** (why the user need not decide), or **ask the user**.
- "Noticed": side bugs or follow-ups, as proposals only.
Then decide:
- Any review `rework`, a blocker or major finding, a missing Behavior changes entry, or a Plan's "Must not change" broken:
  write the `<ID-slug>-rN` round brief with exactly those findings and relaunch Codex; when it exits, run this skill again.
  Max two rework rounds; a third goes to the user.
- Any "ask the user" line, or a review `ask-user`: ask the user that one question with the card; STATUS `ask-user`.
  A scope change they give becomes a round brief with their words under "Why this round".
- Otherwise accept. Commit the card and reports on the branch (`<ID-slug>: check`). STATUS `accepted`.

## 5. Hand over
For an accepted item with UI checks: run `tools/dev.ps1` from the worktree immediately before the hand checks. Verify the
dev badge, the printed EXE and DATA paths, and the app log under that DATA before reporting ready; if any fails, report
that and do not claim the build is ready. Never launch during review or rework. Give the user the Plan's UI checks plus
the report's new ones, and the badge text they should see. Record their result verbatim under `## Hand test`, commit it,
and tell the master. Not visible: say no hand test is needed. Merging is the master's `/merge-task`.
