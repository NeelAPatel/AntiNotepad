# I7-menu-corner-artifacts

- Cause: Avalonia's popup host was allowed to add its native shadow around the rectangular popup window. This appeared at the rounded panel's four outer corners; the menu already draws its own rounded shadow.
- Fix: Set `Popup.WindowManagerAddShadowHint = false` in `UI/PadMenu.cs`. The rounded panel, its 1 px border, and its existing light/dark palette-specific shadow remain unchanged.
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/Antipad -c Debug -o build/i7-check` succeeded with 0 warnings and 0 errors.
- Selftest: `build/i7-check/Antipad.exe --selftest` printed `SELFTEST PASS`.
- Cleanup: Deleted `build/i7-check` after verification.
