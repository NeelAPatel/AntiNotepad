# Loop2-loop-v3-1-gate-and-audit-hardening-r2-review: lens review of Loop2-loop-v3-1-gate-and-audit-hardening-r2

Role: reviewer (fresh run, read-only).
Branch: `chore/Loop2-loop-v3-1` (already checked out; never switch, merge, commit or edit files outside your report).
Tier: Low

## Inputs
- Round brief: `.coord/tasks/Loop2-loop-v3-1-gate-and-audit-hardening-r2.md`; step brief `...-hardening.md`
- Prior findings: `.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening-review.review.md`
- Implementer report: `.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening-r2.md`
- Code: `.claude/skills/next-task/SKILL.md` step 2 (Medium/High audit) and `.coord/tools/codex-run.sh` (the capacity grep and
  the `DONE via` line)

## Do
Follow `.agents/skills/lens-review/SKILL.md`. Focus: does the auditor copy path now work for director, worker and backup
success (the wrapper's exact `DONE via <role>. Report: <path>  Log: <path>` format, path may contain spaces), stop when
there is no DONE line, and never copy an earlier run's report; does the capacity grep still match
"Selected model is at capacity" and the other limit messages and no longer unrelated "disk is at capacity"; nothing else
in the wrapper or skill changed. Do not change files.

## Write: `.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening-r2-review.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
