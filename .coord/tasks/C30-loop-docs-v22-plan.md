# C30-loop-docs-v22-plan: plan loop v2.2 (parallel items, catch-up, conflicts) and write the loop as a readable wiki

Item card (`docs/loop.md`). Done by Claude directly (docs only; no product code, no Codex).

Tier: Low (docs/tools, no product code) (rules: `docs/loop.md`)
Branch: `feat/C30-loop-docs-v22-plan` in `.claude/worktrees/C30-loop-docs-v22-plan`

## Request
User, 2026-10-29 (verbatim): "plan out what we can update in a future v2.2 dev loop for parallelism, merge conflict
resolution, and catch up with dev portions as you mentioned. We should also start making documentation of the dev loop,
maybe with GH Wiki features perhaps. This way i can also read my own documentation and pick up on how things was planned
out, both with graphics/visualizations and a easy to interpret guide for the potentially non tech savy."

## Findings
- **Kind and why:** process. Parallel items are allowed "only if Files lists do not overlap" but nothing checks it; no step
  merges the latest dev into a branch before Check or Merge; `/merge-task` step 3 stops on any conflict; the loop's only
  documentation is one dense runbook (`docs/loop.md`) written for the master and the sub-sessions, not for the user.
- **Now (evidence):**
  - `tools/gate.ps1:215-222` computes changed files as `git diff dev...HEAD` (three-dot, from the merge base) plus working
    changes, and ignores `.coord/`. So merging dev into a branch does not make dev's files look out of scope.
  - The gate builds and selftests the branch alone, never "branch + latest dev"; a semantic clash between two items first
    shows at `/merge-task` step 4, after the merge commit exists on local dev.
  - `docs/dev-loop.md` and C25: `tools/dev.ps1` replaces any running dev copy from any checkout, so only one item can be in
    hand test at a time. `docs/loop.md` does not say this next to the parallel-items rule.
  - Codex caps are shared across worktrees since C28 (`docs/agents.md`); STATUS and `.coord/lessons.md` are master-owned.
- **Where:** `docs/loop-v2.2-plan.md` and `docs/wiki/*` only. `docs/loop.md` rules are not edited.
- **Risk:** the docs drift from `docs/loop.md`; the plan reads as a rule change. Mitigated by quoting loop.md and listing
  contradictions in the report instead of settling them.
- **Source:** master scout (loop.md, three skills, gate.ps1, A55 card, git log).

## Plan
- **After:** `docs/loop-v2.2-plan.md` (a proposal with Now / Proposed / cost / touches for parallelism, catch-up with dev,
  conflict resolution, and lettered unnumbered `step:` names) and `docs/wiki/` (GitHub Wiki pages with Mermaid diagrams, a
  glossary, a history page, and a worked example following A55-split-view-clip), ready to publish on the user's word.
- **Must not change:** `docs/loop.md`, skills, scripts, STATUS, lessons, product code; nothing is pushed to the wiki repo.
- **Scope questions:** None (publishing to the wiki is asked in the final report).
- **Test plan, code:** None (docs).
- **Test plan, UI:** None (not visible).
- **Gates:** `tools/gate.ps1 -Task C30-loop-docs-v22-plan` (catches scope drift: only the Files below); bounds check.
  No Codex review (docs only), no hand test.
- **Needs from you:** the publish decision (commands in the report); whether any v2.2 step is big enough to make it v3.

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list).
- `docs/loop-v2.2-plan.md`
- `docs/wiki/Home.md`
- `docs/wiki/_Sidebar.md`
- `docs/wiki/Dev-Loop-Overview.md`
- `docs/wiki/Who-Does-What.md`
- `docs/wiki/Worked-Example-A55.md`
- `docs/wiki/Tiers-and-Gates.md`
- `docs/wiki/Triage-and-Lessons.md`
- `docs/wiki/Where-Files-Live.md`
- `docs/wiki/Loop-History.md`
- `docs/wiki/Glossary.md`
- `docs/wiki/Loop-v2.2-Plan.md`

## Contracts
None (docs).

## Go
User, 2026-10-29: the request above, relayed as this item's task by the master (Request, Findings, Plan and Go in one).

## Check
<Filled after the gate.>

## Hand test
None (not visible).
