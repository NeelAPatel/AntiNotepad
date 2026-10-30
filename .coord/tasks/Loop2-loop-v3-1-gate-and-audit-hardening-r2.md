# Loop2-loop-v3-1-gate-and-audit-hardening-r2: audit report path after failover, and a too-broad capacity grep

Rework round of `Loop2-loop-v3-1-gate-and-audit-hardening` (`docs/loop.md`). Read the item card
`.coord/tasks/Loop2-loop-v3-1.md` and the step brief `.coord/tasks/Loop2-loop-v3-1-gate-and-audit-hardening.md` first.

Tier: Low · docs-tools
Role: director

## Why this round
Proven review findings (`.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening-review.review.md`):
1. `.claude/skills/next-task/SKILL.md` step 2 (~line 41) copies `<ID-slug>-audit.director.codex.md`, but after a failover
   the wrapper writes `<ID-slug>-audit.worker.codex.md` (or `.backup.`) and exits 0, so the copy fails or copies a stale
   director report.
2. `.coord/tools/codex-run.sh:50` the new bare `at capacity` alternative also matches unrelated failures such as
   `write failed: disk is at capacity`, which then advance the model tier instead of failing.

## Change to the plan
None. Fix within the step's files only:
- next-task step 2: the auditor copies the report named on the wrapper's final `DONE via <role>. Report: <path>` line
  (any role) to `.coord/reports/<ID-slug>-audit.md`; if there is no `DONE` line (the wrapper stopped or failed), it stops and
  ends its turn with the wrapper's last lines instead of copying anything. It must not copy a report from an earlier run.
- codex-run.sh: replace the bare `at capacity` with a model/provider-specific pattern, e.g. `model is at capacity` (covers
  "Selected model is at capacity"); keep the other alternatives and the return-4 chain unchanged.
- If `docs/agents.md` or `.coord/templates/findings.md` names the director-only path, make it agree.

## Files
Only files in the step brief's Files list (expected: `.claude/skills/next-task/SKILL.md`, `.coord/tools/codex-run.sh`, and
`docs/agents.md` / `.coord/templates/findings.md` only if they name the path).

## Done means
- `CODEX_DRY=1 CODEX_SANDBOX=read-only bash .coord/tools/codex-run.sh director Loop2-loop-v3-1-probe-audit AGENTS.md` still
  prints `sandbox=read-only`; `CODEX_SANDBOX=bogus` exits 2; `bash -n .coord/tools/codex-run.sh` passes (say if Git Bash
  cannot start in your sandbox).
- A grep probe: `Selected model is at capacity` matches; `write failed: disk is at capacity` does not.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` passes.

Constraints: no commit, push, branch switch or merge here; never edit STATUS, the card, product. Leave no probe artifacts.
Report: `.coord/reports/Loop2-loop-v3-1-gate-and-audit-hardening-r2.md` (short, step report format).
