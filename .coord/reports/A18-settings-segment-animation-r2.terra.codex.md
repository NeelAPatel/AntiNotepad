**A18 rework is implemented.** Rebuilt selectors retain their recent highlight position and glide to the new selection; reduced motion snaps. The decision is covered by selftest, including fresh, stale and missing positions.

The offline build completed with **0 warnings**, `SELFTEST PASS`, and the requested gate passed. The design snapshot hash was unchanged. The gate removed its own folders, but sandbox policy rejected cleanup of `build/A18-settings-segment-animation-check`, so that folder remains.

The [report](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/feat/A18-settings-segment-animation/.coord/reports/A18-settings-segment-animation-r2.md>) includes the checks and hand-test cases. Live animation behavior and an independent review remain for the item chat.