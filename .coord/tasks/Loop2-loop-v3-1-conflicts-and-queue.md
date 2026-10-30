# Loop2-loop-v3-1-conflicts-and-queue: step 3.1d, one conflict route and one hand-test turn rule

Step brief of `Loop2-loop-v3-1` (read the card `.coord/tasks/Loop2-loop-v3-1.md` first, including "Round 2 scope
change": this step was trimmed on purpose; add nothing beyond the After below).

Tier: Low · docs-tools
Role: worker (gpt-6-luna xhigh)

## Why this step
`tools/catch-up.ps1` (3.1a, 3.1c) can end in CONFLICT, and `/review-task` step 2 now says "conflict routes: step 4a",
but step 4a has no conflict row. Hand tests share one dev copy and nothing says who goes first.

## After
1. `.claude/skills/review-task/SKILL.md` step 4a table, one new row:
   `conflict`: a catch-up stopped on conflicts | resolve on the branch in the item's session (both sides kept as the two
   plans intend), commit the merge, re-run the gate; if the other item changed something this item's Plan relies on
   (semantic clash), abort the merge (`git merge --abort`) and route it as `plan` instead |
2. `docs/loop.md`:
   - In "Triage, lessons, scores", the cause list reads `code`, `test`, `plan`, `env`, `conflict` or `judgment`.
   - In "Branches, worktrees and commits", the sentence "One dev copy runs at a time, so hand tests take turns." becomes
     "One dev copy runs at a time, so hand tests take turns, oldest accepted item first."
3. `.coord/lessons.md` header: `conflict` added to the cause list. No table rows change.

## Files
- `.claude/skills/review-task/SKILL.md`
- `docs/loop.md`
- `.coord/lessons.md`

## Done means
- Only those lines change (`git diff --stat` shows three files, a few lines each).
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` passes.

## Constraints
Work in this checkout (`chore/Loop2-loop-v3-1`; no worktree). Never switch branches, merge, commit or push; never edit
`.coord/STATUS.md`, the card, or `product/`. Never launch or kill AntiNotepad.

Report: `.coord/reports/Loop2-loop-v3-1-conflicts-and-queue.md` with the item template's report sections.
