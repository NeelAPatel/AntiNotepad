# Loop2-loop-v3-1-preflights-review: lens review of Loop2-loop-v3-1-preflights

Role: reviewer (fresh run, read-only). Scripts and skill docs only; no product changes.
Branch: `chore/Loop2-loop-v3-1` (already checked out; never switch, merge, commit or edit files outside your report).
Tier: Low

## Inputs
- Item card: `.coord/tasks/Loop2-loop-v3-1.md`; step brief: `.coord/tasks/Loop2-loop-v3-1-preflights.md`
- Implementer report: `.coord/reports/Loop2-loop-v3-1-preflights.md`
- Gate report: `.coord/reports/Loop2-loop-v3-1.gate.md`
- Diff: `git diff -- tools .claude` plus untracked `tools/approve.ps1` and `tools/merge-preflight.ps1`

## Do
Follow `.agents/skills/lens-review/SKILL.md`, including the plan-bounds item (only the brief's six Files changed).
Focus: a false PASS in either preflight; STATUS state parsing and the cap count (states past Approve, ignoring -Task's
row); card section parsing (filled vs `<...>` placeholder, `## Go` fallback, `ask the user` / `answered`); git failure
handling (unchecked native exit codes); `Head:` and `-dirty` in gate.ps1; that the edited skill steps still read
correctly and other steps are untouched. Do not change files under `tools/`, `.claude/` or `product/`.

## Write: `.coord/reports/Loop2-loop-v3-1-preflights-review.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
