# Loop2-loop-v3-1-fix-loop-scripts-review: lens review of Loop2-loop-v3-1-fix-loop-scripts

Role: reviewer (fresh run, read-only). Scripts only; no product changes in this step.
Branch: `chore/Loop2-loop-v3-1` (already checked out; never switch, merge, commit or edit files outside your report).
Tier: Low

## Inputs
- Item card: `.coord/tasks/Loop2-loop-v3-1.md`; step brief: `.coord/tasks/Loop2-loop-v3-1-fix-loop-scripts.md`
- Implementer report: `.coord/reports/Loop2-loop-v3-1-fix-loop-scripts.md`
- Gate report: `.coord/reports/Loop2-loop-v3-1.gate.md`
- Diff: `git diff -- tools` plus untracked `tools/files-overlap.ps1` and `tools/catch-up.ps1`

## Do
Follow `.agents/skills/lens-review/SKILL.md`, including the plan-bounds item (only the brief's four Files changed).
Focus: branch matching (`^(feat|fix|refactor|docs|chore)/<ID-slug>$`, no partial matches), worktree parsing incl. the main
checkout and detached/bare entries, the STATUS exception in catch-up, any false PASS, exit codes (overlap 0/1, catch-up
0/0/2, refusals 1). Do not change files under `tools/` or `product/`.

## Write: `.coord/reports/Loop2-loop-v3-1-fix-loop-scripts-review.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
