# Loop2-loop-v3-1-catch-up-points: step 3.1c, items catch up with dev before Check and before Merge

Step brief of `Loop2-loop-v3-1` (read the card `.coord/tasks/Loop2-loop-v3-1.md` first). Committed so far: 3.1a
(`tools/catch-up.ps1`, `tools/files-overlap.ps1`), 3.1b (`tools/approve.ps1`, `tools/merge-preflight.ps1`, which already
fails when dev is not an ancestor of the branch).

Tier: Low · docs-tools
Role: director (gpt-6.1-sol medium)

## Why this step
`tools/catch-up.ps1` exists but nothing calls it, so an item meets dev for the first time when `/merge-task` squashes it.
It also merges `origin/dev` when that exists, but dev moves locally first (`/merge-task` commits, then pushes), so a
local dev ahead of origin would be missed.

## After
1. `tools/catch-up.ps1`:
   - Base is the local `-Base` branch (default `dev`). Keep the fetch; when `origin/<Base>` exists and differs from the
     local base, print one `NOTE: origin/<Base> differs from <Base>` line (not a failure). Drop the origin-first choice.
   - After CLEAN (and after UP TO DATE: no incoming files), read the `## Files` list of `.coord/tasks/<Task>.md` in this
     checkout (same parsing as `files-overlap.ps1`) and print `RETEST: yes (<files>)` when any incoming file is in it,
     else `RETEST: no`. On CONFLICT print nothing extra. Exit codes unchanged.
2. `.claude/skills/review-task/SKILL.md` step 2, first: "If `dev` is not an ancestor of HEAD, run
   `pwsh -NoProfile -File tools/catch-up.ps1 -Task <ID-slug>`, push the branch, then run the gate. CONFLICT: stop, leave
   the merge open, and tell the master (conflict routes: step 4a)." Step 5: "A catch-up after acceptance that prints
   `RETEST: yes` repeats the hand test before Merge; `RETEST: no` keeps the earlier result."
3. `.claude/skills/merge-task/SKILL.md` step 1: when the preflight fails with "behind dev", ask the item's session to
   catch up (it runs `/review-task` step 2 again) and wait; never merge dev into an item branch from the master checkout.
4. `docs/loop.md`: one short paragraph "Catching up with dev" in "Branches, worktrees and commits": merge, never rebase
   (branches are pushed); two points (before Check, before Merge); the item's own session runs `tools/catch-up.ps1`, then
   the gate; `RETEST: yes` repeats the hand test. Add `tools/catch-up.ps1`, `tools/files-overlap.ps1`,
   `tools/approve.ps1` and `tools/merge-preflight.ps1` rows to the "Files" table. Nothing else in `docs/loop.md`.

## Files
- `tools/catch-up.ps1`
- `.claude/skills/review-task/SKILL.md`
- `.claude/skills/merge-task/SKILL.md`
- `docs/loop.md`

## Done means
- Probe in a throwaway clone under `build/Loop2-loop-v3-1-probe` (deleted after; never this checkout): local dev ahead
  of origin is merged (CLEAN) with the NOTE line; `RETEST: yes` when incoming touches a card file, `RETEST: no` when not;
  UP TO DATE prints `RETEST: no`; CONFLICT and refusals unchanged. List results in the report.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` passes.

## Constraints
Work in this checkout (`chore/Loop2-loop-v3-1`; no worktree). Never switch branches, merge, commit or push here; never
edit `.coord/STATUS.md`, the card, or anything under `product/`. Never launch or kill AntiNotepad. Keep edits short and
in the existing style (`docs/coding.md`).

Report: `.coord/reports/Loop2-loop-v3-1-catch-up-points.md` with the item template's report sections.
