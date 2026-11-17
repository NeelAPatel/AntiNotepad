Implemented r6. The saved-label clock now runs whenever the pad is visible and not minimized, regardless of bar style or visibility. The existing hide and minimize paths stop it; showing or restoring refreshes the label and restarts it.

The updated selftest passed: 26 contract rows, 25 pass, 1 manual, `SELFTEST PASS`. The offline gate passed with 0 build warnings and errors. Its FINAL line was **`GATE: PASS`**. `git diff --check` passed, and the r6 selftest build folder was removed.

Code reading confirms `OnNoteSaved` does not call `UpdateChrome`, `WakeChrome`, or `SlideBar`; it updates the saved text and timer directly. The gate noted size warnings in five large product files.

Report: [.coord/reports/A21-save-delay-setting-r6.md](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/feat/A21-save-delay-setting/.coord/reports/A21-save-delay-setting-r6.md). The requested fade and sliding style hand check remains for the user.