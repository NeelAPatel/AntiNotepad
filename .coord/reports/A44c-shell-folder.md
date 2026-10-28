# A44c-shell-folder

Implemented on `reorg/A44c-shell-folder`. No commit was made.

## Changes

- `Platform/DesktopPlatform.cs`: added `OpenFolder(string)` as a facade forward.
- `Platform/Native.cs`: added the Explorer launch using `ProcessStartInfo("explorer.exe")`, `UseShellExecute = true`, `ArgumentList.Add(path)`, and `Process.Start`, with no validation or catch.
- `UI/SettingsWindow.cs`: retained the `Directory.Exists` check, exception handling, and both Toast messages; replaced the launch sequence with `DesktopPlatform.OpenFolder(path)`. Removed `System.Diagnostics`, which was no longer used in this file.
- `SelfTest.cs`: added `OpenFolder` (`void`, `string`) to the DesktopPlatform surface check and the UI/Editor `Process.Start` call guard.
- `tools/check-platform-seam.ps1`: added `Process.Start` and `ProcessStartInfo` to the masked source-token scan. The existing `Process.GetCurrentProcess` metrics are not matched.

The two new clean-tree rows are:

```text
LAYERS UI->Process.Start 0
LAYERS Editor->Process.Start 0
```

All existing LAYERS lines remain unchanged.

## Guard proof

Temporarily added an unused `Process.Start(new ProcessStartInfo("explorer.exe"))` method in `UI/SettingsWindow.cs`. The probe build succeeded with 0 warnings and 0 errors. Selftest printed `LAYERS UI->Process.Start 1`, `LAYERS Editor->Process.Start 0`, and `SELFTEST FAIL`, identifying `AntiNotepad.UI.A44cProcessStartProbe.Unused -> System.Diagnostics.Process.Start`. The source guard failed on both `Process.Start` and `ProcessStartInfo` in the probe. The probe was removed completely before final verification.

## Final verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A44c-check`: succeeded, 0 warnings and 0 errors.
- `build/A44c-check/AntiNotepad.exe --selftest`: `SELFTEST PASS`, with both new rows at 0.
- `pwsh -NoProfile -File tools/check-platform-seam.ps1`: passed, 0 violations in 54 files.
- UI/Editor scan found no `Process.Start`, `ProcessStartInfo`, or Explorer references. The two `Process.GetCurrentProcess` calls in `UI/PadWindow.Keys.cs` remain unchanged.
- `git diff --check`: passed.
- `build/A44c-check` was confirmed absent after verification.

No packages were added, and the installed app was not launched, stopped, or modified. The dev memory-metrics move remains out of scope as directed.
