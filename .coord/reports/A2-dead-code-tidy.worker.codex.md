Removed all requested dead members, made GlyphIcon snapping unconditional, updated its summary, and removed the redundant fence local. The requested report is at [.coord/reports/A2-dead-code-tidy.md](F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad-wt/A2-dead-code-tidy/.coord/reports/A2-dead-code-tidy.md).

The Release build passed with **0 warnings and 0 errors** when run with command-scoped `NuGetAudit=false`; the initial build had two `NU1900` warnings because the vulnerability metadata endpoint was unreachable. Self-test printed **SELFTEST PASS**.

`build/a2-check` remains because automatic approval review rejected both recursive deletion attempts as blocked by policy. `.coord/STATUS.md` and the audit doc are unchanged.