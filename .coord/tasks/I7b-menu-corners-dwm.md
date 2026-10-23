# I7b-menu-corners-dwm

You are the director (gpt-6.1-sol medium); delegate to a gpt-6-luna xhigh subagent if useful.
Repo: this folder, branch `claude/chrome-overlays` (checked out; never switch or merge). App: `product/Antipad`.

## Bug (still present after I7-menu-corner-artifacts)
The right-click menu (`UI/PadMenu.cs`) still shows light L-shaped marks at the four outer corners of its rounded panel.
I7 set `WindowManagerAddShadowHint = false` on the popup; keep that, but it was not enough.

Likely cause: the popup is its own native window, and Windows 11 DWM gives every top-level window rounded corners plus a 1 px
system border. On our square popup window that system border/rounding shows around our own rounded panel.

## Fix
Once the popup's native window exists (e.g. on the popup's Opened event, via its host `TopLevel.TryGetPlatformHandle()`),
call `DwmSetWindowAttribute` on that HWND:
- `DWMWA_WINDOW_CORNER_PREFERENCE` (33) = `DWMWCP_DONOTROUND` (1)
- `DWMWA_BORDER_COLOR` (34) = `DWMWA_COLOR_NONE` (0xFFFFFFFE)
Put the P/Invoke in `Platform/Native.cs` next to the other Win32 helpers (small, documented, no-op if it fails). Also make sure
the popup window's own background is transparent (TransparencyLevelHint Transparent, Background null/Transparent) so the
square area outside the rounded panel shows the content behind it. Keep the menu otherwise identical.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/Antipad -c Debug -o build/i7b-check`: 0 errors, 0 warnings;
  `build/i7b-check/Antipad.exe --selftest` prints SELFTEST PASS; delete `build/i7b-check`. Don't launch or kill Antipad.
- Don't try to commit. Don't edit `.coord/STATUS.md`.
- Short report in `.coord/reports/I7b-menu-corners-dwm.md`: what you changed, build and selftest result.
