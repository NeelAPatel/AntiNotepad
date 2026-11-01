# T5-review-gate-stale-head: merge preflight accepts commits made after the gate when they touch only the item's card and reports

Item card (`docs/loop.md`), fast lane (side task, `docs/loop.md` "Rules").

Tier: Low · docs-tools (rules: `docs/loop.md`)
Branch: `chore/T5-review-gate-stale-head` in `.claude/worktrees/chore/T5-review-gate-stale-head` (created at Approve)

## Request
User go relayed by Master 2 (2026-11-01): "finish the remaining three Era 1 items, so that only ui-region-names is left". Asked in the Loop Hub 2 chat (2026-11-01): T4-T6 "[No preference]" between "Approve all three" and "Only T5 and T6"; D1 pause: "State 'paused' (Recommended)".

## Findings
- **Kind and why:** process; merge preflight accepts commits made after the gate when they touch only the item's card and reports.
- **Now:** `tools/merge-preflight.ps1:65-67` requires the newest gate report's `Head:` to equal the branch tip. `/review-task` step 4 (`.claude/skills/review-task/SKILL.md:50`) commits the card and reports after the step-2 gate, so the tip always moves: preflight failed "stale Head: re-run the gate" on A1-selftest-split and A2-store-split (Master 2, 2026-11-01), costing a full gate re-run each time.
- **Where:** `tools/merge-preflight.ps1` only. Riskiest part: letting a code change slip in after the gate; the allow-list must be exact.
- **Source:** master scout (Loop Hub 2).

## Plan
- **After:** if the report's Head differs from the tip, preflight passes only when that Head is an ancestor of the tip and `git diff --name-only <Head> <tip>` lists only `.coord/tasks/<ID-slug>*` and `.coord/reports/<ID-slug>*` paths; it prints `CHECK: commits after the gate touch only the card and reports`. Anything else still fails "stale Head: re-run the gate".
- **Expected time:** about 20 minutes.
- **Must not change:** every other rule, step and behavior of the files below.
- **Scope questions:** None.
- **Test plan, code:** script parses; a scratch check on a throwaway local branch (never pushed, deleted after): card-only commit after a gate PASS -> preflight PASS; a `tools/` or `product/` commit after it -> FAIL stale Head. Then the gate.
- **Test plan, UI:** None (not visible).
- **Gates:** `tools/gate.ps1 -Task T5-review-gate-stale-head`: scope, loop-lint, format.
- **Needs from you:** Nothing.

## Files
- `tools/merge-preflight.ps1`

## Contracts
None.

## Approve
User, 2026-11-01: go relayed by Master 2 ("finish the remaining three Era 1 items"); in this chat "[No preference]" between two options that both build this item, and "State 'paused' (Recommended)" for the D1 pause marker.

## For Codex
Not used: fast lane, a small change; the Claude builder sub-session makes it and says so in its report.

## Check
- Gate: `tools/gate.ps1 -Task T5-review-gate-stale-head` run last at the branch tip; result in `.coord/reports/T5-review-gate-stale-head.gate.md`.
- Bounds: Request vs Plan vs done: within plan. Only `tools/merge-preflight.ps1` changed (plus card and report); all other preflight rules untouched. Plan deviations None.
- Tests: Head logic tested directly on a throwaway local worktree (tip, card+report only, other item card, `tools/` change, unknown Head); details in `.coord/reports/T5-review-gate-stale-head.md`. No end-to-end preflight run.
- Built by the Claude builder, no Codex (fast lane). Misses: None.

## Hand test
None (not visible).
