Implemented all three fixes and wrote [A3-runtime-tidy.md](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad-wt/A3-runtime-tidy/.coord/reports/A3-runtime-tidy.md).

The Release build completed with 0 warnings and 0 errors using process-local `NuGetAudit=false` to avoid the unreachable vulnerability feed. The selftest printed `SELFTEST PASS`, and `git diff --check` passed.

Cleanup is incomplete: workspace policy rejected removing `build/a3-check`, including a direct attempt to remove one generated file, so the directory remains. `.coord/STATUS.md` and the audit document were not changed.