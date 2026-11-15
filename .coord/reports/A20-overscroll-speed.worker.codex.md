Implemented Follow-through horizontal switching and shared edge-band overscroll across all four vertical scroll styles. Settings and defaults are updated, with locked values recorded in [scroll-feel.md](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/feat/A20-overscroll-speed/docs/scroll-feel.md).

Build completed with 0 warnings, and selftests passed. The exact gate command reports `FAIL` on scope because its parser misses the second path on a bullet and treats `SelfTest/` as an exact file path. With `-LooseScope`, the gate reports `PASS`; details are in the [A20 report](/F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.claude/worktrees/feat/A20-overscroll-speed/.coord/reports/A20-overscroll-speed.md). The requested build-check folder remains because sandbox review blocked its removal.

Please hand-test on the dev copy: wheel and touchpad at the first and last notes; the top and bottom of a long note in all four vertical styles; Reduce motion; and the Settings Feel preview. Check that edge pulls return smoothly and caret and selection scrolling still work. I did not launch the app.

