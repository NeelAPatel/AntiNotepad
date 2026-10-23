# N2-canonical-install-and-dev-badge

Status: implementation, review and required build/selftest/dry-run verification complete; check-output cleanup blocked by policy on `feat/N1-antinotepad-identity`. No branch switches, merges or commits.

## Scripts

- `tools/install.ps1 [-DryRun]`: reads product identity from props; publishes Release framework-dependent output to a unique repo staging folder before interrupting the installed app. On upgrade, invokes the installed exe with `--quit`; its existing quit protocol waits for the resident process, and the script enforces a 15-second outer timeout. Copies files into the canonical per-user install, runs the installed selftest (requires exit 0 and SELFTEST PASS), updates the Start menu shortcut, retargets an enabled Run entry, then launches. Both first install and upgrade launch. Installed commands clear inherited ANTINOTEPAD_* variables so dev data/hotkeys/render settings cannot leak in. Staging is cleaned in finally.
- Framework-dependent publishing avoids a duplicate bundled runtime. The machine needs .NET 10; publishing needs the .NET 10 SDK. No external library was added.
- `tools/uninstall.ps1 [-RemoveData] [-DryRun]`: requests graceful installed shutdown and aborts on failure/timeout, removes the canonical install directory, Start menu shortcut and AntiNotepad Run value. Preserves notes unless explicitly asked to remove data. Recursive removal paths are checked against their expected parents; root reparse directories are rejected.
- `tools/dev.ps1 [-Fresh]`: builds Debug to build/dev, launches the exact repo executable with absolute testdata/shots paths, Ctrl+Alt+Shift+D and ANTINOTEPAD_DEV=1. Records the launched PID, executable path and UTC start time in build/dev.pid. Before stopping, verifies both path and start time against that process handle. Termination waits are bounded to five seconds. Fresh cleanup is restricted to testdata, without following child reparse points. Environment overrides belong only to the child process.
- `tools/dev.sh [--fresh]`: validates arguments and delegates to dev.ps1 through pwsh so both routes use the same ownership/build/launch behavior.
## App and documentation

- `Program.cs` removes Claude launch detection. `ANTINOTEPAD_DEV=1` plus a nonempty custom data path identifies a repo dev launch. A data override alone still isolates the instance and bypasses real data/registry migration.
- `PadWindow.Layout.cs` adds only the dev chip for that route, and hides the badge panel otherwise. No prod or Claude badges remain.
- `Platform/Native.cs` makes the Settings start-with-Windows toggle and legacy Run migration always target `%LOCALAPPDATA%\Programs\AntiNotepad\AntiNotepad.exe --hidden`.
- `docs/dev-loop.md` documents installed versus dev copies, installation/upgrades/uninstall, launch scripts, badge environment and one-time retirement of old copies. `AGENTS.md` replaces the old swap/build location with the canonical install workflow.

## Verification

```text
AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/n2-check
  Determining projects to restore...
  All projects are up-to-date for restore.
  AntiNotepad -> F:\LocalVSProj\Visual Studio Apps\AntiQuickPad\build\n2-check\AntiNotepad.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:03.99

build/n2-check/AntiNotepad.exe --selftest
SELFTEST PASS
```

Both commands exited 0. Selftest uses temporary fixtures and returns before desktop, real data or registry initialization.


PowerShell parser checks: tools/install.ps1, tools/uninstall.ps1 and tools/dev.ps1 all PARSE PASS. Bash syntax execution was unavailable: the worker's Git Bash failed with CreateFileMapping Win32 error 5 and WSL failed with Wsl/Service/CreateInstance/E_ACCESSDENIED. Shell wrapper was reviewed statically; no dev launch was executed.

Required install dry run (exit 0; display wrapping normalized):

```text
pwsh -NoProfile -File tools/install.ps1 -DryRun
Task              : Install
Product           : AntiNotepad
Version           : 0.5.0
InstallDirectory  : C:\Users\Neel\AppData\Local\Programs\AntiNotepad
DataDirectory     : C:\Users\Neel\AppData\Local\AntiNotepad
StartMenuShortcut : C:\Users\Neel\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\AntiNotepad.lnk
Publish           : dotnet publish product/AntiNotepad/AntiNotepad.csproj -c Release -o build/<unique-install-stage> --self-contained false
Plan              : Publish to a repository staging directory; on upgrade request a graceful quit from the installed executable; copy staged files; run the installed --selftest; create the Start menu shortcut; retarget the Run entry only when it was enabled; launch the installed app. Preserve the separate data directory.
FirstTransition   : Before first transition, save and exit any legacy Antipad or repo copy through its tray menu. This installer controls only the canonical installed AntiNotepad copy. DryRun does not inspect processes or the registry.
```

DryRun returns before builds, file creation/removal, registry reads/writes, process inspection or app launches. Only identity props and system folder locations are read. No installation state was inspected. `git diff --check` passed at completion.
## Remaining handoff

- Automatic approval review rejected the verified `Remove-Item -LiteralPath <repo>\build\n2-check -Recurse -Force` cleanup with `blocked by policy`. Manager must delete `build/n2-check` (and the previously recorded `build/n1-check`).
- No install/uninstall or dev launch was executed. No running app was quit or killed, and no registry or real-data operations were performed. Live installation, shortcut, startup, hotkey and badge appearance require the manager's user-authorized check.
- A legacy numeric-only build/dev.pid is deliberately rejected. Manager must exit that old dev instance manually and remove build/dev.pid once before using the new launchers. PID/path/start-time mismatches leave the process untouched and abort before fresh cleanup/build/launch.
- Before the first install, save and exit old renamed/repo real-data copies through their own tray menus. Retire obsolete shortcuts/autostart entries/binaries manually after checking the installed copy. Existing legacy data migration is preserved.
- Preexisting `.claude/atlas/4f4ae1d6-e998-4eb9-a1be-e100a230655b.json` changes are untouched by this task.
