# Dev loop

## Installed and dev copies
| | Installed user's copy | Repo dev copy |
|---|---|---|
| Build | `%LOCALAPPDATA%\Programs\AntiNotepad` (Release publish) | `build/dev` (Debug) |
| Instance id | `AntiNotepad` | `AntiNotepad-dev` |
| Display name | `AntiNotepad` | `AntiNotepad (dev)` |
| Icon | Primary AntiNotepad icon | Distinct dev icon |
| App home | `%LOCALAPPDATA%\AntiNotepad` (log and `location.json`) | `build/testdata` (throwaway log and pointer) |
| Notes folder | `Documents\AntiNotepad` by default; selected path is recorded in app home's `location.json` | `build/testdata` (`ANTINOTEPAD_DATA` bypasses Documents and the pointer) |
| Hotkey | Ctrl+Alt+Shift+S (default; configurable in Settings) | Ctrl+Alt+Shift+D |
| Badge in the top bar | None | dev |

The installed copy is the user's working copy. Never type into or stop it during development. Upgrade it only when the user
asks, through `tools/install.ps1`. Dev work uses the separate repo scripts below. There are no prod or Claude badges.

## Install, upgrade and uninstall
- Preview safely: `pwsh -NoProfile -File tools/install.ps1 -DryRun`. This prints the plan without building, writing files,
  inspecting processes, accessing the registry or launching the app.
- Install or upgrade: `pwsh -NoProfile -File tools/install.ps1`. It publishes Release, requests the installed copy to save and
  quit with `--quit`, waits for exit, and moves the old install to a timestamped sibling backup before copying new files.
  It runs the installed `--selftest`, updates the Start menu shortcut `AntiNotepad`, preserves whether start with Windows is
  enabled while pointing an enabled entry at the installed exe, and starts the app. Any failure after the swap restores the
  old files; the old app is relaunched only if it was running before the upgrade. A failed first install removes its partial
  files. A successful upgrade keeps only the latest previous-version backup beside the install. A quit timeout aborts
  replacement; no force kill. The success message prints the notes folder from `%LOCALAPPDATA%\AntiNotepad\location.json`,
  falling back to app home if the pointer cannot be read.
- Installer rollback check: `pwsh -NoProfile -File tools/install.ps1 -InstallRoot <repo-build-folder> -TestFailSelfTest` runs
  the publish and copy flow against a direct `build/A16-staged-upgrade-rollback-<suffix>` child, injects a selftest failure,
  and must restore the fixture's old files. With `-InstallRoot`, the installer skips installed-process checks and `--quit`,
  all Start menu shortcut and Run key access or changes, and app launch. This mode accepts no path outside that named build
  child. It is for disposable repo fixtures only.
- Publishing is framework-dependent to avoid shipping a duplicate runtime. The machine needs .NET 10; building needs the
  .NET 10 SDK. The installed exe and the Settings toggle always use the canonical install path for start with Windows.
- Uninstall: `pwsh -NoProfile -File tools/uninstall.ps1`. It quits the installed copy, removes previous-install backups,
  installed files, shortcut and Run-at-login entry. It reads `location.json`, prints the notes folder, and keeps it.
  `-RemoveData` removes only the app home (`%LOCALAPPDATA%\AntiNotepad`), never the notes folder.
- The first normal startup keeps the existing `DefaultDataFolderMigration` as its first step, then moves any app-home notes to
  `Documents\AntiNotepad`. The pointer in app home selects the notes folder; the product log always stays in app home.
  `Documents` is resolved with `SpecialFolder.MyDocuments`, including OneDrive redirection. If a pointer's target is unavailable,
  the app does not create it or save into it. `ANTINOTEPAD_DATA` bypasses this startup resolution for dev and selftest fixtures.
- First transition: save and exit any old Antipad/AntiQuickPad or repo-built real-data copy through its own tray menu before
  installing. The old identity cannot be controlled by the new executable. Retire its old shortcuts/autostart entries and
  remove obsolete binary folders (such as `build/antipad` or `build/antinotepad`) manually after verifying the installed copy.
  Preserve real data and the repo dev copy. Do not launch old binaries against real notes again.

## Dev copy
- `pwsh -NoProfile -File tools/dev.ps1 [-Fresh]` or `bash tools/dev.sh [--fresh]` builds Debug into `build/dev` with the
  compile-time property `AntiNotepadDev=true`, then launches that executable with `ANTINOTEPAD_DATA=build/testdata`
  (absolute path), `ANTINOTEPAD_HOTKEY=Ctrl+Alt+Shift+D`, `ANTINOTEPAD_DEV=1` and `ANTINOTEPAD_SHOTS=build/shots`.
  Both routes share the same implementation. The dev build displays as `AntiNotepad (dev)` and uses its distinct dev icon.
- The compile-time dev identity is separate from the runtime dev badge and dev-only UI. `AppInfo.IsDevBuild`,
  `AppInfo.DisplayName` and the selected `AppInfo.IconUri` follow the compile-time identity; stable `AppInfo.Name` remains
  `AntiNotepad`, even when the assembly's Product display name is `AntiNotepad (dev)`. The compile-time identity is independent
  of the runtime `ANTINOTEPAD_DEV=1` marker, which works with a custom data path to gate the dev badge and dev-only UI.
- The dev script sets `AntiNotepadDev=true`; ordinary Debug/Release builds and the installed Release publish leave it unset
  and retain the default `AntiNotepad` identity.
- The scripts stop only their recorded dev PID after checking executable path and process start time; they never stop the
  installed copy. Fresh deletes only repo test data. Logs and PID state live under `build`.
- On the first use after upgrading these scripts, a legacy numeric-only `build/dev.pid` is rejected. Exit the old dev copy
  manually and remove that PID file once, then retry. A mismatched PID is left untouched and the launcher aborts.
- The explicit dev marker plus a custom data path enables the dev badge and dev-only UI. A custom data path alone still uses
  the separate instance and bypasses real-data/registry migration, but has no badge. Merely inheriting `CLAUDECODE` does nothing.
- Headless Codex must not run these launch scripts; build to `build/<task>-check` and use `--selftest` only.
- `pwsh tools/press-hotkey.ps1 -Key D` presses the dev hotkey (the first press after launch may only prewarm).
- Dev-only switches: `ANTINOTEPAD_DEBUG_SETTINGS_CYCLES=n` opens/closes Settings n times and logs memory; F12 / Shift+F12 /
  Ctrl+F12 write captures to `ANTINOTEPAD_SHOTS` (captures miss the Acrylic backdrop).
- App log: `<app home>/antinotepad.log` (hotkey timings, memory lines, crashes as `crash ...`).

## Checks
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/<task>-check`: 0 errors, 0 warnings.
- `<build>/AntiNotepad.exe --selftest` prints `SELFTEST PASS` (exit 0). Selftest never touches real data.
- Delete only the task's check output after verification.

## Other tools
- `tools/make-icon.ps1` rebuilds `product/AntiNotepad/antinotepad.ico`.
- `tools/make-dev-icon.ps1` derives every size in the distinct dev icon from the primary icon. Rerun it whenever the primary
  icon changes.
- Computer use: only act in the dev copy; the installed copy holds real user data.

## Product identity
- `product/AntiNotepad/AppInfo.props` owns the name, version, pre-release label, description and copyright. Runtime identity
  comes from generated assembly attributes through `AppInfo`.
- The default data folder migrates once from the previous identity at normal startup. Custom `ANTINOTEPAD_DATA` bypasses
  migration; failed migration retains the old folder.
- A name change updates runtime identities and assembly naming from props. Source namespaces and checked-in project/tool
  paths remain source layout choices.
