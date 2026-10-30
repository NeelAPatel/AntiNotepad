# Loop2-loop-v3-1-preflights-r2-review: lens review of Loop2-loop-v3-1-preflights-r2

Role: reviewer (fresh run, read-only). Scripts only; no product changes.
Branch: `chore/Loop2-loop-v3-1` (already checked out; never switch, merge, commit or edit files outside your report).
Tier: Low

## Inputs
- Round brief: `.coord/tasks/Loop2-loop-v3-1-preflights-r2.md`; step brief `.coord/tasks/Loop2-loop-v3-1-preflights.md`
- Prior review finding: `.coord/reports/Loop2-loop-v3-1-preflights-review.review.md`
- Implementer report: `.coord/reports/Loop2-loop-v3-1-preflights-r2.md`
- Code: untracked `tools/approve.ps1` and `tools/merge-preflight.ps1` (the round changed only their section/placeholder helper and its call sites)

## Do
Follow `.agents/skills/lens-review/SKILL.md`. Focus: does the placeholder rule now reject the real `.coord/templates/item.md`
Plan/Approve/Request/Check/Hand test placeholder lines (labeled, bulleted, standalone) and still accept normal filled
text (including filled lines containing `<` later, inline code); any false PASS or false FAIL on a real filled card such as
`.coord/tasks/Loop2-loop-v3-1.md`; both scripts' helpers identical; no other behavior changed. Do not change files under `tools/`.

## Write: `.coord/reports/Loop2-loop-v3-1-preflights-r2-review.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
