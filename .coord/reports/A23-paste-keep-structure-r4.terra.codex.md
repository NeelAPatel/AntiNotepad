Implemented the paste outcome and existing-toast notification. Converted pastes show “Paste Formatting Found”; structured pastes that fall back show “Paste Formatting Fallback,” with code and markdown guards suppressing the notice.

The build completed with **0 warnings**, and **SELFTEST PASS** includes the new outcome checks and earlier paste tests. The requested offline gate **failed its scope check** because the existing branch diff includes `tools/playground/a23-paste-pill-lab.html`, which is absent from the item’s file lists. The gate passed with `-LooseScope`.

The [report](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/feat/A23-paste-keep-structure/.coord/reports/A23-paste-keep-structure-r4.md>) records the results. Live clipboard and toast behavior remain for hand testing.

Automatic execution policy rejected cleanup of `build/A23-check`; the folder remains.