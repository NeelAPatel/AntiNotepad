# Loop2-loop-v3-1-gate-and-audit-hardening-review: lens review of Loop2-loop-v3-1-gate-and-audit-hardening

Role: reviewer (fresh run, read-only). Scripts and docs only; no product changes.
Branch: `chore/Loop2-loop-v3-1` (already checked out; never switch, merge, commit or edit files outside your report).
Tier: Low

## Inputs
- Item card: `.coord/tasks/Loop2-loop-v3-1.md`; step brief: `.coord/tasks/Loop2-loop-v3-1-gate-and-audit-hardening.md`
- Implementer report: `.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening.md`
- Gate report: `.coord/reports/Loop2-loop-v3-1.gate.md`
- Diff: `git diff -- tools .coord/tools .coord/templates .claude docs`

## Do
Follow `.agents/skills/lens-review/SKILL.md`, including the plan-bounds item (only the brief's six Files changed). Focus:
strict vs WARN conditions in `tools/gate.ps1` (item-branch stray file FAIL; WARN on `-Ci`, on HEAD == `-Base`, on
`-LooseScope`; detached HEAD; rework-round briefs; missing brief / Files section / no backticked paths) and that nothing
else in the gate changed (other checks, exit codes, report format); the `CODEX_SANDBOX` validation and the sandbox flag
actually passed to `codex exec` in `.coord/tools/codex-run.sh` (exit 2 before ledger/report setup; no behavior change for the
default); the "at capacity" failover grep (no false positives, same chain as rate limits); that `.coord/templates/findings.md`
and the next-task step 2 copy path (`<ID-slug>-audit.director.codex.md` -> `<ID-slug>-audit.md`) agree with what the wrapper
writes; docs/agents.md and the loop.md Gates row wording. Do not change files.

## Write: `.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening-review.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
