---
name: review-task
description: Check a finished AntiNotepad round against its plan, run gates, and hand over accepted work.
disable-model-invocation: true
shell: powershell
allowed-tools: PowerShell(git *)
argument-hint: <ID-slug>
---

# /review-task <ID-slug>

Phase detail: `.claude/skills/review-task/phases.md`.

## Live facts

!`git branch --show-current`
!`git rev-list --count dev..HEAD`
!`git status --short`
!`git grep -h -E "^(Tier|Branch):" -- ".coord/tasks/$ARGUMENTS.md"`

Branch, commits ahead of dev and changed files are above. A missing card stops this skill.

Phase 6 (Check) of `docs/loop.md`, then the hand-over. Run by the item chat that built it. Every reply opens with the status card, role `reviewer` (then
`tester` at the hand-over). Read report sections and verdict lines, not whole diffs. Never fix product code here; rework goes
back to Codex.

## 1. Read the card
From `.coord/tasks/<ID-slug>.md`: `Tier:`, `## Request`, `## Plan` (After, Must not change, Gates, Test plan), `## Approve`.
Missing Plan or Approve: stop, tell the director the item skipped phases. Missing tier: treat as Medium, say so.
`<round>` is the round just finished: `<ID-slug>`, or `<ID-slug>-rN` after a rework. `.coord/reports/<round>.md` needs
"Change list", "Behavior changes" and "Plan deviations"; if not, that is a rework.

## 2. Gate (always)
If `dev` is not an ancestor of HEAD, run `pwsh -NoProfile -File tools/catch-up.ps1 -Task <ID-slug>` (rebase only; no push,
no force-push), then the gate. CONFLICT: stop, leave the rebase open, tell the director (step 4a).

`pwsh -NoProfile -File tools/gate.ps1 -Task <round>` from the worktree writes `.coord/reports/<round>.gate.md` (not
committed); read only its Summary. Fail: triage (4a) before relaunching anything. Warnings (scope, size): carry to step 4.

## 3. The card's other gates
Run every gate the Plan lists (at least the tier's defaults):
- **Codex review** (Medium, High): fill `.coord/templates/review.md` into `.coord/tasks/<round>-review.md`, launch
  `bash .coord/tools/codex-run.sh reviewer <round>-review .coord/tasks/<round>-review.md` as a tracked background call you own (`docs/agents.md` "Launch"), tell the user
  in one line (folder, brief, log, report `.coord/reports/<round>.review.md`), end the turn and resume on the exit notice.
- **Design review** (High): after the Codex review exists, spawn the `design-reviewer` agent with "Design review of
  <ID-slug>, latest round <round>."
- **Contracts / reproduction test**: confirm the named rows and checks are in the diff and pass in the gate's selftest.

## 4. Bounds check and verdict
Compare Request, Plan and Approve with what was done (the report's Change list, Behavior changes, Out-of-scope files, Plan
deviations; `git diff --stat dev...HEAD`). For a large diff a forked `design-reviewer` pass may do the comparison: give it
the card path, round and report path, ask for one line per difference. You still write `## Check` and ask the user.
`## Check` holds:
- Gate and review result lines.
- One line per difference: **within plan**, **justified** (why the user need not decide), or **ask the user**.
- Comments and formatting follow `docs/coding.md` (on touch); a gap is a `code` rework line.
- "Noticed": side bugs and follow-ups as proposals; tell the director each in one line now (`docs/loop.md`, "Proposals go
  into `.coord/pending.md` at once").
Then:
- Review `rework`, a blocker or major finding, a missing Behavior changes entry, or a broken "Must not change": triage (4a).
  Rounds `-r2` and `-r3` run without asking; `-r4` needs the user's go.
- Any "ask the user" line or review `ask-user`: ask that one question with the card; STATUS `ask-user`. A scope change
  becomes a round brief quoting their words under "Why this round".
- Otherwise accept: commit card and reports (`<ID-slug>: check`); STATUS `accepted`.

## 4a. Triage (every failure, no extra agent)
Name one cause and route by it (`.claude/skills/review-task/phases.md`, Triage, lessons, scores):
| Cause | Route |
|---|---|
| `code`: build misses the Plan | `<ID-slug>-rN` brief from `.coord/templates/round.md` with exactly the failing lines or proven findings; relaunch Codex; run this skill again on exit |
| `test`: a selftest, contract or probe is wrong | round brief limited to the test files; say so under "Why this round" |
| `plan`: Findings or Plan were wrong | stop; write the corrected plan as the round brief, ask the user to approve it, tell the director |
| `env`: tooling, cache, flaky run, sandbox | re-run the gate once; still failing: one-line gate fix if it is the manager's, else tell the director |
| `conflict`: catch-up stopped on conflicts | resolve on the branch keeping both sides, `git add`, `git rebase --continue`, re-run the gate; a semantic clash: `git rebase --abort` and route as `plan` |
| `judgment`: only the user can decide | ask that one question with the card; STATUS `ask-user` |
Add one "Misses" line per failure in `## Check` (found at, cause, caught by, should have caught, lesson); the director copies
them to `.coord/lessons.md` at Merge.

## 5. Hand over
A catch-up after acceptance printing `RETEST: yes` repeats the hand test before Merge; `RETEST: no` keeps the result.
With UI checks: run `tools/dev.ps1` from the worktree just before the hand checks, verify the printed EXE and DATA paths
and the app log, and report not-ready if any fails. Give the user your slot's dev badge text to expect, the Plan's UI checks and the
report's new ones; record their result verbatim under `## Hand test`, commit it, tell the director. A failed check is
triaged (4a) with a "Misses" line. Not visible: say no hand test is needed. Never launch during review or rework.

## 6. Commit review (before suggesting the merge)
After the hand test passes (at once if not visible), tidy the branch as `/merge-task` step 0 says: fold tiny and
back-and-forth commits, make every message say what changed, drop lab and debug code, keep each commit buildable. Then
`tools/catch-up.ps1 -Task <ID-slug>`, re-run the gate at the tip, tell the director it is ready to merge, and record the
tidy-up as one line under `## Check`.
