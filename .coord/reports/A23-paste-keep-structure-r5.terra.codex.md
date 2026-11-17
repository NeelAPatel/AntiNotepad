Implemented the four r5 fixes: bounded HTML depth and input size, offset-based tag scanning, queued paste requests, and grapheme-aware emoji width. The existing paste selftests pass unchanged.

The Release build completed with **0 warnings**, the full selftest reported **SELFTEST PASS**, and the requested offline gate reported **GATE PASS**. The report is at [A23-paste-keep-structure-r5.md](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/feat/A23-paste-keep-structure/.coord/reports/A23-paste-keep-structure-r5.md>).

Live overlapping pastes were not tested under the offline-only constraint. The existing normal-paste fallback still delegates to AvaloniaEdit’s asynchronous command.

Automatic execution policy rejected cleanup of `build/A23-check`, so that build folder remains. The gate cleaned its own build folder.