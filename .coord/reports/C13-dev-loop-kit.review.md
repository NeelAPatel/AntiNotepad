# C13-dev-loop-kit-review lens review

## Findings (proven)
1. [major] tools/gate.ps1:307 - A successful build without `AntiNotepad.exe` skips the required selftest and still passes. Proven with the unchanged gate and process-local `UseAppHost=false`, `NuGetAudit=false`, `-Task C13-dev-loop-kit-review-probe`: Build PASS (exit 0, 0 warnings/errors), Selftest SKIP, Seam PASS, GATE PASS, process exit 0. MSBuild inherits UseAppHost and builds a DLL without the executable. Lines 287-307 select SKIP; lines 231-233 fail only FAIL steps, so success requires no selftest result. Smallest fix: mark the missing executable as Selftest FAIL after an otherwise successful build, or require Selftest PASS for gate success. Skip after an already-failed build is harmless.

## Behavior changes check
Report's list vs the diff: matches. Product runtime behavior is unchanged. Tooling adds the gate, reviewer routing and manual CI; selftest adds CONTRACTS. The false-pass case above violates the gate's required selftest promise.

Reviewed every requested hunk and surrounding code. Existing delegates retain their original callers in SelfTest.Run and are additionally called by ContractsChecks through RunExistingCheck. The two A30 rows share memoization local to each table run; SettingsChangedChecks, UnavailableStoreStartupChecks and FenceCacheChecks run independently. Failed rows and exceptions append to Run's failure list, producing SELFTEST FAIL and exit 1. Gate lines 294-300 require exit 0 and SELFTEST PASS and preserve failure output. No changed product method contract, persistent shared state, layering dependency or runtime timing was found. Reviewer routing adds its tier and backup mapping; workflow dispatch and always-upload match the brief.

## Scope check
In scope: tools/gate.ps1, .coord/tools/codex-run.sh, product/AntiNotepad/SelfTest/Contracts.cs, the partial keyword and one Run call in SelfTest.cs, and .github/workflows/gate.yml. Manager-owned surrounding changes were excluded from the implementation review.

Scope parsing works with the current .coord/templates/brief.md: a temporary brief copied from the template recognized the SelfTest.cs bullet despite preceding prose and trailing parenthetical text. SelfTest.cs was excluded from out-of-scope output; Contracts.cs and gate.ps1 were correctly listed.

## Unproven risks
- Reviewer Bash dry run and GitHub-hosted Actions execution were not performed in this fresh review; routing and workflow were checked statically.

## Checks run
- Read skill, STATUS, brief, implementer/gate reports, loop rules, brief template, complete requested diff/new files, existing contract delegates and Program's early --selftest return.
- Missing-apphost probe: unchanged gate with process-local UseAppHost=false and NuGetAudit=false; exit 0, Build PASS, Selftest SKIP, GATE PASS. No product/tools edits needed.
- Normal gate with only process-local NuGetAudit=false: exit 0; build 0 warnings/errors; SELFTEST PASS; CONTRACTS: 6 rows, 5 pass, 1 manual; all 104 LAYERS rows clean; Seam 0 violations in 55 files. Scope/Size WARN as expected. This verifies the offline-context build, not hosted CI.
- Cleanup probe used the actual functions extracted through the PowerShell AST: removed a fixture containing a junction, preserved its target sentinel, and refused .coord outside build/. Reviewed normalized direct-child checks, reparse-point avoidance and recursive deletion; no proven outside-build deletion path in ordinary operation.
- PowerShell parser PASS; git diff --check PASS (STATUS line-ending advisory only).
- Review-created build directories and temporary probe brief/report cleaned up. No branch changes, commits, product/tools edits, installed-copy interaction or normal GUI launches.

VERDICT: rework
