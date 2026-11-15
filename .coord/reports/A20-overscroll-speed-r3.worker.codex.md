Vertical scrolling is restored to `dev` behavior: all four styles stop at note ends, and touchpad input remains 1:1. Horizontal Follow-through and the Feel settings are preserved. `Motion.cs` differs from `dev` only in its comment.

The build succeeded with 0 warnings, selftest passed (19 contracts: 18 automated, 1 manual), and the offline gate passed. The gate reported the inherited `PadView.cs` size warning. No live hand test was performed.

The [nine-section report](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/feat/A20-overscroll-speed/.coord/reports/A20-overscroll-speed-r3.md>) is complete. The standalone check output remains because automatic approval review blocked its removal with `blocked by policy`; the gate cleaned its own temporary folders.