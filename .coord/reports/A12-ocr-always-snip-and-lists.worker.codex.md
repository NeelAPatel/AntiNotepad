Implemented **A12-ocr-always-snip-and-lists**. The list-mode root cause was Enter handling: `PadView.OnKeyDown` used tunnel routing and `ContinueList` marked Enter handled before the completion window could accept a slash command. The handler now yields while completion is open; normal list continuation is unchanged.

`/ocr` now always starts a fresh screen snip. I removed the existing-clipboard image path and its sequence-number tracking, preserved insertion into the captured note, and updated the command summary.

Build completed with 0 warnings and 0 errors, with `NuGetAudit=false` in the environment; without it, NuGet’s unreachable vulnerability service produced `NU1900` warnings. Selftest passed. Manager manual steps and the root-cause details are in the [A12 report](<F:\LocalVSProj\Visual Studio Apps\AntiQuickPad-wt\A12-ocr-always-snip-and-lists\.coord\reports\A12-ocr-always-snip-and-lists.md>). I did not run the manual UI or snipping checks.

The generated `build/a12-check` directory remains because automatic workspace policy rejected its recursive cleanup command.