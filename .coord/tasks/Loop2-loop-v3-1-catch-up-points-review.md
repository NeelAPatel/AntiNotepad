# Loop2-loop-v3-1-catch-up-points-review: lens review of Loop2-loop-v3-1-catch-up-points

Role: reviewer (fresh run, read-only). Scripts and docs only; no product changes.
Branch: `chore/Loop2-loop-v3-1` (already checked out; never switch, merge, commit or edit files outside your report).
Tier: Low

## Inputs
- Item card: `.coord/tasks/Loop2-loop-v3-1.md`; step brief: `.coord/tasks/Loop2-loop-v3-1-catch-up-points.md`
- Implementer report: `.coord/reports/Loop2-loop-v3-1-catch-up-points.md`
- Gate report: `.coord/reports/Loop2-loop-v3-1.gate.md`
- Diff: `git diff -- tools .claude docs`

## Do
Follow `.agents/skills/lens-review/SKILL.md`, including the plan-bounds item (only the brief's four Files changed; in
`docs/loop.md` only the catch-up paragraph and four Files-table rows). Focus: the local base and the NOTE line (when
origin differs, missing origin ref, missing local base, no false CLEAN/UP TO DATE); RETEST file matching (Files-section
parsing parity with `tools/files-overlap.ps1`, path normalization, no false `RETEST: no` when an incoming file is listed,
nothing printed on CONFLICT/refusal, exit codes 0/0/2 and 1); git failure handling; that the review-task, merge-task and
loop.md wording matches what the scripts actually do. Do not change files under `tools/`, `.claude/` or `docs/`.

## Write: `.coord/reports/Loop2-loop-v3-1-catch-up-points-review.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
