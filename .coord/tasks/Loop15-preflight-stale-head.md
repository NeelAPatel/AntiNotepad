# Loop15-preflight-stale-head: stop merge-preflight failing on "stale Head"

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: Low · docs-tools
State: pending
Branch: `chore/Loop15-preflight-stale-head` in `.claude/worktrees/chore/Loop15-preflight-stale-head` (created at Approve)

## Request
User, 2026-11-17/18 (via Master 6): start `idea: preflight-stale-head-again` ("merge-preflight still fails on stale Head at nearly every merge (A2, A5, A7, T16, A21): the item chat edits the card after the last gate run"). The user's caution: the idea may be stale. Number given by Loop Hub 4 on the user's ask.
- **Goal:** merge preflight never fails on "stale Head" for a branch whose newest commits only touch the card, reports or STATUS; or the row is dropped as already solved.
- **Done check:** Findings show, from the last merges (T17, A24, Loop13, Loop14, A22, A23), whether "stale Head" still occurred and why. Fix chosen from: (a) end-of-build step in `.claude/skills/next-task/phases.md` runs the gate after the last card commit; (b) `tools/merge-preflight.ps1` ignores card-only commits after the gate (T5-review-gate-stale-head, 2026-11-01, already added a card/report-only allowance; check why it still trips); (c) drop the row. Pass/fail: a dry run of preflight on a branch with a card-only commit after the gate passes.
- **Proof:** the list of recent merges with the preflight line each saw, the cause, the dry-run output, gate output.
- **What we know:** `tools/merge-preflight.ps1:175` throws `stale Head: re-run the gate` via `Test-GateHead`; preflight for Loop13 and Loop14 printed "commits after the gate touch only the card, reports and STATUS", so the allowance works at least there. Lessons repeat rule applies: name the rule that failed.
- **Constraints:** Low tier, loop tools and docs only; prefer a text rule over new script logic; Codex may build, offline only; Findings first and may end in "drop".

## Findings
- **Kind and why:** process; the row says preflight fails "stale Head" at most merges.
- **Now:** the allowance exists since Loop8-merge-policy (2026-11-06): `Test-GateHead` (`tools/merge-preflight.ps1:28`) passes a clean-Head gate when later commits touch only this item's card, reports and STATUS. A2, A5, A7 (the idea's list) all predate it. After it, Loop13 and Loop14 printed the card-only PASS line; T16 and A21 appear only in one lessons row (2026-11-17) that blames the card edit, which the allowance already covers. For A22, A23, A24, T17 no preflight output survives (gate reports are gitignored and overwritten by the merge gate), so "no stale Head" there is unproven.
- **Residual cause (read from code, then dry-run):** `tools/gate.ps1:43-46` appends `-dirty` to `Head:` when any tracked file other than STATUS, pending, lessons, status-archive or settings.json is modified. The item's own card or reports edited but uncommitted at gate time give `<sha>-dirty`; `Test-GateHead` needs 40 hex digits, so it returns False (dry run here: `-dirty` Head gives False). `/review-task` step 2 gates with whatever card edits are loose, and step 6 re-gates at the tip. Not confirmed on T16/A21 themselves (reports gone).
- **Rule that failed:** `/review-task` steps 2 and 6 have no "clean tree before the gate" line; `docs/dev-loop.md:117` lists only STATUS files as allowed-dirty, so card edits counting as dirty is invisible.
- **Source:** own scout (`.coord/lessons.md`, `git log`, `tools/gate.ps1`).

## Plan
- **After:** (a) text rule: `/review-task` steps 2 and 6 say "commit the card and reports first so the tree is clean, run the gate, then commit only card, reports and STATUS"; `/next-task` phases end-of-build gets the same one line. (b-lite) one message change in preflight: a `-dirty` Head throws `gate ran on a dirty tree: commit the card and re-run the gate` instead of the generic text. The allowed-file set does not change.
- **Expected time:** about 25 minutes request to merge (Codex build about 10).
- **Must not change:** the allowed-file set, per-commit builds, gate behavior.
- **Scope questions:** None.
- **Test plan:** dry runs of `Test-GateHead` (extracted by AST, as T5 did) on a throwaway local branch, never pushed, deleted after: clean Head + card-only commit True; `-dirty` Head False with the new message; clean Head + a `tools/` commit False. Then the gate.
- **Gates:** `tools/gate.ps1` (loop-lint, format, scope).
- **Needs from you:** Approve, or Drop. Alternative: script-only, widen the gate's dirty exemption to the item's card and reports.

## Files
- `.claude/skills/review-task/SKILL.md`
- `.claude/skills/next-task/phases.md`
- `tools/merge-preflight.ps1`

## Contracts
- None (loop text and one script message; no product code).

## Approve
User, 2026-11-18: "Approve"

## For Codex
Role: quick (`docs/agents.md`), because three small edits, no product code. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files; re-read before editing. Offline checks only.
- Build exactly the Plan's "After" for the Files above; no drive-by changes. If the Findings are wrong, stop and report that.
- `/review-task` SKILL.md steps 2 and 6 and `phases.md` end-of-build: add one short line each: commit the card and reports first so the tree is clean, run the gate, then commit only card, reports and STATUS afterwards (a `-dirty` Head always fails preflight).
- `tools/merge-preflight.ps1` line ~175: if the report's `Head:` ends `-dirty`, throw `gate ran on a dirty tree: commit the card and re-run the gate`; otherwise keep `stale Head: re-run the gate`. Do not change the allowed-file set.
- Write the report `.coord/reports/Loop15-preflight-stale-head.md` with dry-run output of `Test-GateHead` (AST-extracted) on a throwaway local branch (never pushed, deleted after): clean Head + card-only commit True; `-dirty` Head False; clean Head + `tools/` commit False.
