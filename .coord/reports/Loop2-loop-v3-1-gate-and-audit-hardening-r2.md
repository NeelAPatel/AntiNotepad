# Loop2-loop-v3-1-gate-and-audit-hardening-r2

## Summary
Audit handoff follows the successful role's report path from the current invocation's final DONE line. Capacity failover now requires model context instead of matching unrelated disk-capacity failures.

## Change list
| File / block | Change | Plan |
|---|---|---|
| .claude/skills/next-task/SKILL.md:41-47 | Copy Report path from current DONE line, any role; stop without copying when absent; prohibit earlier-run evidence | Rework audit handoff |
| .coord/tools/codex-run.sh:50 | Replace bare at capacity with model is at capacity; other alternatives and return-4 chain unchanged | Rework capacity match |

## Behavior changes
- Auditor accepts director, worker or backup success; strips the Log suffix from the reported path. A stopped/failed invocation with no DONE line ends with wrapper diagnostics and copies nothing, even if older report files exist.
- Model-capacity diagnostics still trigger failover; disk-capacity text alone no longer does.

## Out-of-scope files
None. This requested report is a coordination artifact. Pre-existing edits are preserved; STATUS, card and product sources were not edited. Neither docs/agents.md nor .coord/templates/findings.md names a director-only audit path, so neither needed an r2 edit.

## Plan deviations
Git Bash cannot start in this sandbox: CreateFileMapping, Win32 error 5. Dry read-only, bogus sandbox and bash -n each returned process exit 256 before script execution; sandbox output, intended exit 2 and Bash syntax are unverified. The extracted grep expression was probed with rg instead (same regex constructs for these inputs). No probe files were created.

## Contracts
None (no product changes).

## Lens review
Fresh project lens_reviewer: no proven findings, VERDICT accept. Behavior list matches the two r2 changes; scope and optional-doc checks pass. Bash verification limitation remains explicit.

## Checks run
- Offline gate: GATE PASS, exit 0; Build 0 warnings/errors; SELFTEST PASS; CONTRACTS 13 rows, 12 pass, 1 manual; seam, format, strict Scope (14 changed files), size and cleanup PASS.
- Extracted grep expression via rg -qi: Selected model is at capacity matches (exit 0); write failed: disk is at capacity does not (exit 1).
- Scoped git diff --check: PASS. Static inspection: other grep alternatives, return 4 and failover chain unchanged; DONE output includes the role-specific report followed by two spaces and Log.
- Requested Bash probes blocked as described above. No live provider run; no probe artifacts left. Gate removed its task build/data folders.

## Hand checks for the user
None (not visible). Manager: rerun dry read-only, bogus sandbox and bash -n checks in working Git Bash.
