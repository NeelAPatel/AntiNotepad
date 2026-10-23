# A8-save-failure-quit-and-x64

- `PadWindow.Save()` now returns whether the store saved. Its normal failure toast remains in place for autosave, hide and deactivation saves. A failed quit suppresses the transient toast and keeps the resident app open.
- The save prompt is a compact centered card over the pad, using the current palette's surface, ink, muted and accent colors, the existing `Kit` button helper, and the pad's shadow. It shows “Couldn't save your notes” and the store error reason. Retry receives focus; Enter retries, Tab moves between the two named buttons, Esc cancels, and clicking outside the card or away from the pad cancels. A failed retry updates the reason in the same prompt. “Quit anyway” closes explicitly.
- Quit routes:
  - **Hotkey (`Ctrl+Shift+Q`):** calls `Quit()`; a failed save shows the prompt and keeps the app running.
  - **In-pad menu:** the menu has a “Hide AntiNotepad” item, not a Quit item. It still calls `HideResident()`; a failed save uses the ordinary toast and the resident app hides without exiting.
  - **Tray Quit:** calls `Quit()`; if the pad was hidden and saving fails, the pad is shown with the prompt.
  - **`--quit` (installer/uninstaller):** sends the existing quit request to the same `Quit()` path. On save failure the resident stays open with the prompt; the requester gets the existing timeout/nonzero result and aborts. `SingleInstance.cs` and the scripts' quit protocol were not changed.
- `tools/install.ps1` now publishes with `-r win-x64 --self-contained false`; the dry-run plan shows the same flags. No project file was changed.

## Publish

- Hand-run publish command: `dotnet publish product/AntiNotepad/AntiNotepad.csproj -c Release -o build/a8-publish -r win-x64 --self-contained false -p:NuGetAudit=false`. The extra NuGet property avoided an online vulnerability lookup unavailable in this environment; it does not change the runtime target or self-contained setting.
- Before publish, the RID-less `build/a8-check/runtimes` output had 17 top-level runtime folders (13 Linux, 1 macOS, 3 Windows), 24 `.so` files and 1 `.dylib`.
- After publish, `build/a8-publish` had no `runtimes` folder, no `runtimes/linux*` or `runtimes/osx*` folders, and no `.so` or `.dylib` files.
- The published output contains `Microsoft.Windows.SDK.NET.dll` and `WinRT.Runtime.dll`; its deps file references `Microsoft.Windows.SDK.NET.Ref/10.0.19041.57`.

## Checks

- NuGet vulnerability lookup was unavailable from this environment (`NU1900`, `api.nuget.org` service index). After restoring with NuGet audit disabled, `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a8-check --no-restore -p:NuGetAudit=false` passed with 0 warnings and 0 errors.
- `build/a8-check/AntiNotepad.exe --selftest`: `SELFTEST PASS`.
- `build/a8-publish/AntiNotepad.exe --selftest`: `SELFTEST PASS`.
- The GUI was not launched, so the prompt's rendered appearance and live pointer/focus behavior remain for the manager's dev-copy gate.
- `build/a8-check` and `build/a8-publish` remain because the environment blocked the requested recursive cleanup.
