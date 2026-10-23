# Dev loop

## Two copies, side by side
| | User's copy | Dev copy |
|---|---|---|
| Build | `build/antipad` (Release) | `build/dev` (Debug) |
| Instance id | `Antipad` | `Antipad-dev` |
| Data | `%LOCALAPPDATA%\Antipad` (real notes) | `build/testdata` (throwaway) |
| Hotkey | Ctrl+Alt+Shift+S (default; the user can change it in Settings) | Ctrl+Alt+Shift+D (`ANTIPAD_HOTKEY`) |
| Badge in the top bar | prod | dev |

A custom `ANTIPAD_DATA` makes an instance the dev one (its own single-instance id), so both run at once.
Never type into, kill or rebuild the user's copy outside the swap routine.

## Dev copy
- `tools/dev.sh [--fresh]` kills only the dev instance it started (`build/dev.pid`), builds Debug into `build/dev`, launches it on
  `build/testdata` with hotkey Ctrl+Alt+Shift+D. `--fresh` wipes the test data first.
- `pwsh tools/press-hotkey.ps1 -Key D` presses the dev hotkey from a script (the first press after a launch may only prewarm).
- Icon Lab (design spike): run the dev build with `--iconlab`.
- Dev-only switches: `ANTIPAD_DEBUG_SETTINGS_CYCLES=n` opens/closes Settings n times and logs memory; F12 / Shift+F12 /
  Ctrl+F12 write captures to `ANTIPAD_SHOTS` (captures miss the Acrylic backdrop).
- Log: `<data>/antipad.log` (hotkey timings, memory lines, crashes as `crash ...`).

## Checks
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/Antipad -c Debug -o build/<task>-check`: 0 errors, 0 warnings.
- `<build>/Antipad.exe --selftest` prints `SELFTEST PASS` (exit 0). Selftest never touches real data.

## Swap the user's copy to the current code (only when the user asks)
1. `build/antipad/Antipad.exe --quit` with `ANTIPAD_DATA` unset (saves and exits the real copy).
2. `dotnet build product/Antipad -c Release -o build/antipad`.
3. `build/antipad/Antipad.exe --selftest`.
4. Relaunch `build/antipad/Antipad.exe`.

## Other tools
- `tools/make-icon.ps1` rebuilds `product/Antipad/antipad.ico`.
- Computer use: the grant covers `build\dev\Antipad.exe` and `build\antipad\Antipad.exe`; only ever act in the dev copy.
