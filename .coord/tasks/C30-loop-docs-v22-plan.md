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
- **Must not change:** `docs/loop.md` rules (user, 2026-10-29 follow-up: v2.2 stays dormant; only a one-line pointer to `docs/v2.2/` is added), skills, scripts, STATUS, lessons, product code; nothing is pushed to the wiki repo.
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
- `docs/loop.md`
- `docs/v2.2/README.md`
- `docs/v2.2/DISCUSSION.md`
- `docs/v2.2/CHANGELOG.md`
- `docs/v2.2/ACTIVATION.md`
- `tools/v2.2/files-overlap.ps1`
- `tools/v2.2/catch-up.ps1`

## Contracts
None (docs).

## Go
User, 2026-10-29: the request above, relayed as this item's task by the master (Request, Findings, Plan and Go in one).

## Check
- **Gate (`tools/gate.ps1 -Task C30-loop-docs-v22-plan`):** `GATE: PASS`; build 0 warnings, `SELFTEST PASS`, `CONTRACTS: 11 rows, 10 pass, 1 manual`, seam 0 violations, Format skipped (no C# changed), Scope PASS (12 files, all allowed), Size PASS.
- **Bounds check:**
  - Requested: v2.2 plan (parallelism, catch-up, conflicts, lettered steps) and readable docs with diagrams. Done: `docs/loop-v2.2-plan.md` plus 10 wiki pages and `_Sidebar.md`: within plan.
  - `docs/loop.md`, skills, scripts, STATUS, lessons untouched: within plan.
  - Mermaid diagrams were not rendered here (no renderer offline); written conservatively (flowchart, sequenceDiagram, gitGraph): justified, noted in the report.
  - Wiki links use `[text](Page-Name)` without `.md`, so they work in the wiki but not when browsing `docs/wiki/` in the repo: justified (the wiki is the target).
- **Noticed (proposals, none fixed):** `docs/loop.md` contradictions listed in the final report (v2 attribution, gate row, rework counting, parallel items vs one dev copy).
- **Misses:** None.

## Check, follow-up round (dormant drop; user, 2026-10-29: "dev and main will remain on v2.1 until i explicitly trigger the v2.2 changes ... committed all the way to master but remain dormant")
- **Gate (after catching up with dev, `catch-up.ps1` CLEAN, 29 incoming files):** `GATE: PASS`; build 0 warnings, `SELFTEST PASS`, `CONTRACTS: 13 rows, 12 pass, 1 manual`, Scope PASS (19 files, all allowed).
- **Bounds check:**
  - Added `docs/v2.2/{README,DISCUSSION,CHANGELOG,ACTIVATION}.md`, `tools/v2.2/{files-overlap,catch-up}.ps1`: within the user's follow-up (docs + dormant scripts, chosen by the user).
  - `docs/loop.md`: two pointer lines under "Loop version" saying v2.2 is dormant; no rule changed: justified (discoverability after the user's rebase cleanup).
  - Scripts live in `tools/v2.2/` and are called by no gate, skill or hook: within plan. `files-overlap.ps1` ran PASS (0 others in flight); `catch-up.ps1` ran for real on this branch (CLEAN; `git diff dev...HEAD` stayed limited to this item's files, confirming the scope finding).
  - Not tried: `catch-up.ps1` CONFLICT path and `files-overlap.ps1` FAIL/WARN path (no second card in flight): left for the `step: rehearsal`.
- **Misses:** None.

## Hand test
None (not visible).
