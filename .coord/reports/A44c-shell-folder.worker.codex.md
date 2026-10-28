Completed **A44c-shell-folder**. `SettingsWindow` keeps its folder validation and Toast handling, while `DesktopPlatform.OpenFolder` now owns the Explorer launch. The dev memory metrics remain unchanged.

The new rows are `LAYERS UI->Process.Start 0` and `LAYERS Editor->Process.Start 0`. The temporary probe produced `LAYERS UI->Process.Start 1` and `SELFTEST FAIL`; the source guard also failed on both `Process.Start` and `ProcessStartInfo`. The probe is gone.

Final build: **0 warnings, 0 errors**. Selftest: **PASS**. Source guard: **0 violations in 54 files**. `build/A44c-check` is absent. No commit was made, and `.coord/STATUS.md` was untouched.

Report: [A44c-shell-folder.md](<F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.coord/reports/A44c-shell-folder.md>)

