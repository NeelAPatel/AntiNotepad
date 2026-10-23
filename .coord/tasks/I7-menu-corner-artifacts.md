# I7-menu-corner-artifacts

You are the director (gpt-6.1-sol medium); delegate to a gpt-6-luna xhigh subagent if useful.
Repo: this folder, branch `claude/chrome-overlays` (checked out; never switch or merge). App: `product/Antipad`.

## Bug
The new right-click menu (`UI/PadMenu.cs`, from I6-win11-menu-toolbar-in-app) shows small light-grey L-shaped marks at all four
outer corners of its rounded 8 px panel, in dark mode. Something square behind the rounded panel is drawn: likely the popup host
(PopupRoot / Flyout presenter / an outer Border) has a non-transparent background, border or default template chrome, or the
popup window itself isn't transparent, so its square corners show around the rounded panel.

## Fix
Make everything outside the rounded panel fully transparent (popup root/presenter background, border, padding, any default
theme chrome or shadow border), so only the rounded panel with its own 1 px border is visible. Check light and dark palettes in
code. Keep everything else about the menu identical (sizes, insets, colours, behaviour). Only touch `UI/PadMenu.cs` unless the
cause is elsewhere; say where if so.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/Antipad -c Debug -o build/i7-check`: 0 errors, 0 warnings;
  `build/i7-check/Antipad.exe --selftest` prints SELFTEST PASS; delete `build/i7-check`. Don't launch or kill Antipad.
- Don't try to commit. Don't edit `.coord/STATUS.md`.
- Short report in `.coord/reports/I7-menu-corner-artifacts.md`: the cause you found, the fix, build and selftest result.
