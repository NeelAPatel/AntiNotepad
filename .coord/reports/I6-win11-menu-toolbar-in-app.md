# I6-win11-menu-toolbar-in-app

Implemented the Icon Lab menu and toolbar design in `product/Antipad`.

- Changed `UI/PadMenu.cs`, `Editor/PadView.cs`, `UI/PadWindow.Layout.cs`, `UI/Kit.cs`, `Model/Store.cs`, and `UI/SettingsWindow.cs`.
- The right-click menu is a pointer-placed popup with the action row, ordered edit/app commands, lab palette colors and typography, grayscale antialiasing, and dimmed shortcuts. It closes on Escape, outside click, and after an enabled item runs. Hide is labelled “Hide Antipad.”
- Added persisted `MenuActionRow` (default on) and `MenuColorIcons` (default off), with the requested rows in UI > Controls.
- Toolbar and pager icon buttons are 32 × 32 px with 4 px hover corners and 16 px weight-0 Segoe Fluent glyphs. The pin keeps its accent fill beneath the outline.
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/Antipad -c Debug -o build/i6-check` succeeded with 0 warnings and 0 errors. `Antipad.exe --selftest` printed `SELFTEST PASS`. The temporary `build/i6-check` directory was removed.
- Not done: no live GUI or screenshot review was run; the app was not launched, as instructed.
