**A35-bar-insets-r4** is implemented on `feat/A35-bar-insets`; no commit was made. The editor uses one combined mask for the moving top band and existing edge fade. The blur samples each pane’s `TextView` outside that mask. The top band has no opaque Canvas backing and uses a `ChromeTokens` tint of **0.4**; the pill’s existing blur tint is unchanged.

Build passed with **0 warnings**, `--selftest` passed, and `git diff --check` passed. Cleanup of `build/A35-bar-insets-check` was blocked by policy, so the folder remains.

The report is [A35-bar-insets-r4.md](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.claude/worktrees/A35-bar-insets/.coord/reports/A35-bar-insets-r4.md>). The manager’s live look is still needed for light and dark wallpapers, Acrylic/Mica/Solid, split view, and line‑1 selection and chevrons.

