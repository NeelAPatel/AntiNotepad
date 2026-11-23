Implemented A21 in the worktree: the Settings dropdown, configurable restart timer, saves before pane and split transitions, and completion-based “Saved” labels with delay and elapsed-time logging. Added selftests and CONTRACTS rows.

The item build passes with 0 warnings and 0 errors; `git diff --check` passes. The first offline gate build also passed, but its selftest is still running in an earlier build and stopped after the first save log. I fixed the blocking wait in the source. The required selftest and final gate result remain pending.

The report is at [A21-save-delay-setting.md](F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/feat/A21-save-delay-setting/.coord/reports/A21-save-delay-setting.md).

The item card prohibits killing AntiNotepad. May I stop only the temporary selftest process, PID <PID> at `build/A21-save-delay-setting-gate/AntiNotepad.exe`, so I can rerun the selftest and gate? The installed copy remains untouched.

