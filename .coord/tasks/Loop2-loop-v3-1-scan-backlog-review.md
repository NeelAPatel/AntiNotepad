# Loop2-loop-v3-1-scan-backlog-review: lens review of Loop2-loop-v3-1-scan-backlog

Role: reviewer (fresh run, read-only). Scripts and docs only; no product changes.
Branch: `chore/Loop2-loop-v3-1` (already checked out; never switch, merge, commit or edit files outside your report).
Tier: Low

## Inputs
- Item card: `.coord/tasks/Loop2-loop-v3-1.md`; step brief: `.coord/tasks/Loop2-loop-v3-1-scan-backlog.md`
- Implementer report: `.coord/reports/Loop2-loop-v3-1-scan-backlog.md`
- Gate report: `.coord/reports/Loop2-loop-v3-1.gate.md`
- Diff: `git diff -- tools .claude docs .coord/templates` plus untracked `tools/scan-delta.ps1`,
  `.claude/skills/scan-backlog/SKILL.md`, `.coord/templates/scan.md`

## Do
Follow `.agents/skills/lens-review/SKILL.md`, including the plan-bounds item (only the brief's six Files changed; in
`docs/loop.md` only the Backlog scans paragraph and three Files rows). Focus: scan-delta marks (WHERE/NEAR/none) and the
RESCAN rule (more than half of Where files, or a deletion); the first-parent log and merged-in changes; card lookup from
commit subjects; exit codes and git-failure handling (no false "DELTA: none"); the approve.ps1 research-file move
(slug derivation from `<ID>-<slug>`, untracked-only, no-research path, DryRun, STATUS never touched); that the scan-backlog
and next-task skill wording and the scan template match what the scripts do. Do not change files under `tools/`, `.claude/`,
`docs/` or `.coord/templates/`.

## Write: `.coord/reports/Loop2-loop-v3-1-scan-backlog-review.review.md`
In the format the skill gives. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
