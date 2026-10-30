---
name: review-task
description: Run the AntiNotepad Check phase on a finished Codex round - the gates the item card planned, then the bounds check (request vs plan vs done), then a verdict and the hand-test handover. Use when the user or manager says "/review-task <ID-slug>", "review <ID-slug>", or a Codex Build run for an item has just exited.
argument-hint: <ID-slug>
---

# /review-task <ID-slug>

Phase 6 (Check) of `docs/loop.md`, then the hand-over to Hand test. Every reply opens with the status card, with your
role (`reviewer`, then `tester` at the hand-over) on its first line. Keep Claude
reading small: report sections and verdict lines, not whole diffs. Never fix product code here; rework goes back to Codex.

## 1. Read the card
From `.coord/tasks/<ID-slug>.md`: `Tier:`, `## Request`, `## Plan` (After, Must not change, Gates, Test plan), `## Approve`
(`## Go` on era-one cards). Missing Plan or Approve: stop and tell the master the item skipped phases. Missing tier: treat as Medium and say so.
`<round>` is the round just finished: `<ID-slug>` first, `<ID-slug>-rN` after a rework. Check that
`.coord/reports/<round>.md` has the "Change list", "Behavior changes" and "Plan deviations" sections; if not, that is a rework.

## 2. Gate (always)
`pwsh -NoProfile -File tools/gate.ps1 -Task <round>` from the worktree. It writes `.coord/reports/<round>.gate.md` (not
committed) and exits non-zero on failure. Read only its Summary.
- Fail: triage it (step 4a) before anything is relaunched.
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
Compare Request, Plan (and Approve) with what was done: the report's Change list, Behavior changes, Out-of-scope files, Plan
deviations, and `git diff --stat dev...HEAD`. Write `## Check` on the card:
- Gate and review result lines.
- One line per difference: **within plan**, **justified** (why the user need not decide), or **ask the user**.
- Comments and formatting in the changed files follow `docs/coding.md` (on touch); a gap is a `code` rework line.
- "Noticed": side bugs or follow-ups, as proposals only.
Then decide:
- Any review `rework`, a blocker or major finding, a missing Behavior changes entry, or a Plan's "Must not change" broken:
  triage it (4a). Max two rework rounds; a third goes to the user.
- Any "ask the user" line, or a review `ask-user`: ask the user that one question with the card; STATUS `ask-user`.
  A scope change they give becomes a round brief with their words under "Why this round".
- Otherwise accept. Commit the card and reports on the branch (`<ID-slug>: check`). STATUS `accepted`.

## 4a. Triage (every failure, no extra agent)
You already hold the context, so triage here instead of spawning anyone. Name one cause from the failing lines or
findings, and route by it (`docs/loop.md`, "Triage, lessons, scores"):
| Cause | Route |
|---|---|
| `code`: the build does not do what the Plan says | `<ID-slug>-rN` round brief from `.coord/templates/round.md` with exactly the failing lines or proven findings; relaunch Codex; run this skill again when it exits |
| `test`: a selftest, contract or probe is wrong, not the product | round brief limited to the test files; say so under "Why this round" |
| `plan`: Findings or Plan were wrong (wrong cause, missed caller, impossible After) | stop; tell the master with the card; the master re-plans and asks the user to approve again |
| `env`: tooling, build cache, flaky run, sandbox | re-run the gate once; still failing: one-line gate fix if it is the manager's to make, else tell the master |
| `judgment`: only the user can decide | ask the user that one question with the card; STATUS `ask-user` |
Add one line per failure under "Misses" in the card's `## Check`: found at, cause, caught by, which check should have
caught it earlier, a one-line lesson. The master copies these into `.coord/lessons.md` at Merge.

## 5. Hand over
For an accepted item with UI checks: run `tools/dev.ps1` from the worktree immediately before the hand checks. Verify the
printed EXE and DATA paths and the app log under that DATA before reporting ready; if any fails, report that and do not
claim the build is ready. The on-screen dev badge is the user's first hand check while Computer Use is off: give them
the badge text they should see. Never launch during review or rework. Give the user the Plan's UI checks plus the
report's new ones. Record their result verbatim under `## Hand test`, commit it, and tell the master. A failed hand check
is triaged (4a) and gets a "Misses" line. Not visible: say no hand test is needed. Merging is the master's `/merge-task`.
