# A36-drop-pill-lab

- Removed the dev-only Settings Lab row and pill blur sampling. The pager child is the row again; the existing `RebuildChrome` palette `Surface` fill keeps the pill solid in every style and material.
- Replaced `PadWindow.MaterialLab.cs` with `PadWindow.TopBlur.cs`. The top-bar blur, Canvas tint, editor opacity-mask cooperation, freshness observers, software-rendered blur, and split-pane samples remain. Removed the pill sampler, render telemetry and unused `BlurTintAlpha` token.
- The renamed source is 320 lines versus 535 before (215 fewer); also removed 9 Settings lines, 7 net Layout lines, and 2 token lines. Updated `.coord/STATUS.md`.
- Verification: Debug build to `build/A36-drop-pill-lab-check` succeeded with 0 warnings and 0 errors. `--selftest` with data under that build directory printed `SELFTEST PASS`.
- The check output remains at `build/A36-drop-pill-lab-check`: after verifying the exact worktree path, workspace policy rejected its recursive cleanup.
- No implementation uncertainties. The app was not launched, so live visual behavior was not manually inspected under the task constraint.
