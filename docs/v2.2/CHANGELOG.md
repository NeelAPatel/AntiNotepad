# Loop v2.2: change log (planned; nothing applied)

State key: **not applied** = described only; **built, dormant** = exists in `tools/v2.2/`, called by nothing.
Format per step: what changes, which files, state.

## v2.1 -> v2.2

### step: files-overlap-check
- Adds: overlap check between the `## Files` lists of items in flight; hot files warn, other shared files fail.
- Files: `tools/v2.2/files-overlap.ps1` -> `tools/files-overlap.ps1`; one step in `tools/gate.ps1`; `/next-task` step 3.
- State: script **built, dormant**; gate step and skill change **not applied**.

### step: in-flight-limit-and-board
- Adds: limit of 2 items past Go; STATUS "In flight" table; rule that Request, Findings and Plan may overlap another item's Build.
- Files: `docs/loop.md`, `tools/loop-card.ps1`, STATUS layout.
- State: **not applied**.

### step: hand-test-queue
- Adds: one hand test at a time (single dev copy); "your turn" note from the master; no-UI items skip the queue.
- Files: `.claude/skills/review-task/SKILL.md` step 5, `docs/loop.md`.
- State: **not applied**.

### step: catch-up-script
- Adds: `catch-up.ps1`: merge dev into the branch, report UP TO DATE / CLEAN / CONFLICT, mark hot files.
- Files: `tools/v2.2/catch-up.ps1` -> `tools/catch-up.ps1`.
- State: **built, dormant** (tried on `feat/C30-loop-docs-v22-plan`: clean merge of 29 incoming files; gate scope unaffected).

### step: catch-up-points
- Adds: catch up before Check and before Merge; the build/review session runs it; gate re-runs after; user decides re-test.
- Files: `/review-task` step 2, `/merge-task` step 2, `docs/loop.md`.
- State: **not applied**.

### step: conflict-triage
- Adds: triage cause `conflict` with four kinds (mechanical, small textual, semantic, judgment) and their routes; Misses wording.
- Files: `/review-task` step 4a, `docs/loop.md`, `.coord/lessons.md` header.
- State: **not applied**.

### step: conflict-codex-round
- Adds: round brief limited to conflict-marker files; review checklist line for merge diffs.
- Files: `.coord/templates/round.md`, `.agents/skills/lens-review/SKILL.md`.
- State: **not applied**.

### step: rehearsal
- Adds: one practice run with two throwaway docs items and a forced conflict.
- State: **not applied**.

## Version rule
Steps that change session rules (limit, queue, catch-up points) may make this v3 under `docs/loop.md`'s version rule:
the user decides at activation.

## Delivered in the dormant drop (C30-loop-docs-v22-plan)
`docs/loop-v2.2-plan.md`, `docs/v2.2/*`, `docs/wiki/*` (also on the GitHub wiki), `tools/v2.2/*`, one pointer line in `docs/loop.md`.
