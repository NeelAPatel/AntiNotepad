Implemented the saved-label transition using the existing Windows 11 motion tokens. The offline build finished with **0 warnings**, and `--selftest` returned **SELFTEST PASS**.

The strict gate **failed its scope check**: the task card lists `product/AntiNotepad/SelfTest/*`, but the gate compares paths literally and rejected `SelfTest/Run.cs`. Its build, selftest, seam, tokens, and format checks passed. The report records the mismatch and the pending independent lens review: [A16-saved-label-fade.md](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/feat/A16-saved-label-fade/.coord/reports/A16-saved-label-fade.md>).

Hand testing remains for you: check next/previous direction, rapid switching, equal labels, typing, and reduced motion.