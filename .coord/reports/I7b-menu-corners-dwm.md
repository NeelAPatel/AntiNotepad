# I7b-menu-corners-dwm

- Updated `product/Antipad/UI/PadMenu.cs` to make the popup host transparent after opening, keep `WindowManagerAddShadowHint = false`, and apply the native DWM popup-window cleanup.
- Added a guarded `DwmSetWindowAttribute` helper in `product/Antipad/Platform/Native.cs` using `DWMWCP_DONOTROUND` and `DWMWA_COLOR_NONE`; missing/unsupported DWM is a no-op.
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/Antipad -c Debug -o build/i7b-check` — 0 warnings, 0 errors.
- Selftest: `build/i7b-check/Antipad.exe --selftest` — `SELFTEST PASS`.
- Cleanup: deleted `build/i7b-check` after verification.
