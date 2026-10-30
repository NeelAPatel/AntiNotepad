# Loop2-loop-v3-1-gate-and-audit-hardening

## Summary
Scope violations fail by default; dev/base, CI and explicit loose scope warn. Findings audits request a read-only sandbox and return their report as the final message for the auditor to copy.

## Change list
| File / block | Change | Plan |
|---|---|---|
| tools/gate.ps1:6,54,399-530 | LooseScope, default strict Scope, per-brief rework validation and CI warnings | After 1 |
| .coord/tools/codex-run.sh:6-17,40-51 | Sandbox validation, PLAN field, argument and capacity failover | After 2 |
| .coord/templates/findings.md:3-24 | No file writes; final-message report; describe build probes | After 3 |
| .claude/skills/next-task/SKILL.md:39-43 | Read-only invocation and report copy | After 4 |
| docs/agents.md:20-22 | Audit sandbox versus build/review default | After 5 |
| docs/loop.md:62 | Scope gate severity exceptions | After 5 |

## Behavior changes
- Scope errors fail on item branches; missing briefs, Files sections and path lists include rework briefs. Each existing round brief is validated individually, and the requested round must exist.
- CI evaluates Scope with WARN severity instead of skipping it. Current branch equal to Base and LooseScope also warn; LooseScope summaries identify loose scope. StrictScope is removed.
- CODEX_SANDBOX defaults to workspace-write; only workspace-write/read-only are valid. Invalid values exit 2 before ledger/report setup. PLAN includes the sandbox, which is passed to codex exec.
- Capacity failures follow the existing rate-limit failover chain.
- Audits never write files or run builds; their final output is the report. The auditor copies the wrapper's captured last message into the existing audit report path.

## Out-of-scope files
None. This requested report is a coordination artifact. Pre-existing manager edits to STATUS/card and unrelated untracked coordination files were left alone.

## Plan deviations
Wrapper execution checks are blocked by Git Bash startup: CreateFileMapping, Win32 error 5, before script execution. The observed bogus-run process exit was 256, not the script's intended 2; neither requested wrapper assertion is claimed as passed. Real non-dry read-only Codex execution remains the manager's separate check. Automatic approval review rejected recursive and enumerated clone cleanup commands with "blocked by policy"; the disposable clone build/Loop2-loop-v3-1-probe and its sibling .bundle remain for manager cleanup.

## Contracts
None (no product changes).

## Lens review
Fresh lens_reviewer: no proven findings; behavior list matches the diff, all implementation hunks in plan, VERDICT: accept. Wrapper execution remains unverified.

## Checks run
- Offline gate: PASS; Build exit 0, 0 warnings/errors; SELFTEST PASS; CONTRACTS 13 rows, 12 pass, 1 manual; seam PASS; format PASS; strict Scope PASS (14 changed files against the whole item card); size/cleanup PASS.
- Disposable clone scope matrix: item stray FAIL; LooseScope stray WARN with loose scope summary; CI stray WARN; dev stray WARN; missing item brief FAIL. Missing Files section FAIL; no backticks FAIL; missing requested rework brief FAIL; rework with no backticked paths FAIL even when base paths allow all changed files; same malformed rework with LooseScope WARN.
- Clone isolation: clone from a local bundle because normal local clone also invokes blocked MSYS sh. Sparse checkout contains tools only, so build cannot find the product project and selftest skips. Only Scope output is evidence; overall probe gates exit 1. Clone-local dev reset to clone HEAD removes prior item commits from the fixture comparison.
- Scoped git diff --check: PASS; no remaining StrictScope callers in tools/skills.
- Bash dry, bogus and syntax invocations cannot start due to the host sandbox error above. Static diff inspection only for sandbox validation and capacity grep.

## Hand checks for the user
None (not visible). Manager: run the wrapper dry read-only / invalid sandbox checks and the separately planned real read-only Codex run in a working Git Bash environment.
