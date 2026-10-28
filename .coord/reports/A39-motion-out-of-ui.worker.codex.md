**A39-motion-out-of-ui** is complete on `reorg/A39-motion-out-of-ui`; no commit was made.

`Anim` now lives in `AntiNotepad.Motion`, `ChromeTokens` in `AntiNotepad.Theme`, and editor menu data and glyph IDs in `AntiNotepad.Editor`. `PadView` sends menu requests to `PadWindow` through a delegate; `PadMenu` still builds the popup with the same entries, settings, and rendering behavior. The Editor UI search returned no matches.

The layer selftest reports **0 refs for all eight rules**. I temporarily added an Editor reference to `AntiNotepad.UI.PadWindow`; the selftest reported `Editor->UI 1 refs` and named the offending types, then passed after I removed the probe.

The final build had **0 warnings and 0 errors**, and `--selftest` printed `SELFTEST PASS`. I removed `build/A39-check`. I did not launch the live GUI, so its visual check remains for the manager.

Report: [A39-motion-out-of-ui.md](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.coord/reports/A39-motion-out-of-ui.md>)  
Status: [STATUS.md](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.coord/STATUS.md>)

