# N1-antinotepad-identity

Status: implementation, director review, build and selftest complete; two completion exceptions are recorded below. Branch `feat/N1-antinotepad-identity`; no branch switches, merges or commits.

## Scope and evidence boundary

No running product instance was launched, quit or killed. No real user data or registry operations were performed. Only temporary selftest fixtures may exercise migration. Historical task briefs/reports, archive contents and git tags are preserved.

## Identity design

`AppInfo.props` holds AppName, AppVersion `0.5.0`, AppVersionLabel `pre-release`, description and copyright (`Copyright © 2026`). It feeds Product, Title, AssemblyName, RootNamespace, Version, AssemblyVersion/FileVersion `0.5.0.0`, InformationalVersion `0.5.0-pre-release`, Description and Copyright. `AppInfo.cs` reads the generated metadata and derives runtime identities, environment variable names and resource URIs. Source namespaces and physical asset/project paths are source layout, while executable naming and resource assembly paths follow the product name.

## Rename inventory

| Old | New |
|---|---|
| `product/Antipad/` | `product/AntiNotepad/` |
| `Antipad.csproj` | `AntiNotepad.csproj` |
| AssemblyName / RootNamespace `Antipad` | `AntiNotepad`, fed from props |
| `namespace Antipad.*`, `using Antipad.*` | `AntiNotepad.*` |
| `Antipad.exe` | `AntiNotepad.exe` |
| `antipad.ico` | `antinotepad.ico` |
| `avares://Antipad/…` | runtime URI derived from AppInfo |
| Product text / window titles / tray / menus / help / notes / About | AppInfo-derived AntiNotepad text |
| Instance `Antipad`, `Antipad-dev` | `AntiNotepad`, `AntiNotepad-dev` (and derived mutex/pipe names) |
| Default data folder `%LOCALAPPDATA%\Antipad` | `%LOCALAPPDATA%\AntiNotepad` |
| Run value `Antipad` | `AntiNotepad` |
| `antipad.log` | `antinotepad.log` (legacy log preserved) |
| Temp prefix `antipad-selftest-` | `antinotepad-selftest-` |
| `ANTIPAD_DATA` | `ANTINOTEPAD_DATA` |
| `ANTIPAD_HOTKEY` | `ANTINOTEPAD_HOTKEY` |
| `ANTIPAD_SHOTS` | `ANTINOTEPAD_SHOTS` |
| `ANTIPAD_RENDER` | `ANTINOTEPAD_RENDER` |
| `ANTIPAD_GPU_CACHE_MB` | `ANTINOTEPAD_GPU_CACHE_MB` |
| `ANTIPAD_TRIM_SECONDS` | `ANTINOTEPAD_TRIM_SECONDS` |
| `ANTIPAD_DEBUG_SETTINGS_CYCLES` | `ANTINOTEPAD_DEBUG_SETTINGS_CYCLES` |
| User build `build/antipad` | `build/antinotepad` |
| Future tag convention `antipad-vN` | `antinotepad-vN` (existing tags untouched) |

Dev build `build/dev`, PID file `build/dev.pid` and dev/prod badges stay unchanged. Updated AGENTS.md, docs/*.md and tools scripts. Wrapper had no old-name reference.

## Migration

Normal startup takes the new identity single-instance lock before resolving data. A nonempty custom data override is returned verbatim and bypasses default-folder and Run-entry migration. For default data, an existing destination is used without merging. Otherwise the whole old directory is moved to the new sibling with Directory.Move; notes, index, settings, backups, legacy log and other files travel together. There is no copy/delete cleanup phase. The old log is preserved as `antipad.log`; new logging uses `antinotepad.log`.

Path inspection or move exceptions return the old folder plus an error, and Program logs the error there before loading Store. A destination file also selects the old path with an error. Logging is best effort if the old path itself is inaccessible. The legacy v1 `antipad.json` reader is intentionally retained so those notes still migrate to the current store format.

Only normal primary startup with default data opens the HKCU Run key. If its old value exists, the new value is written with the new executable path and `--hidden` before deleting the old value. Exceptions are logged and do not stop startup; a failed write preserves the old value. Real registry migration remains untested by design.

`--selftest` returns before instance coordination, normal startup handlers, data-folder resolution or registry access. Temp tests exercise whole-tree migration including nested notes, index, settings, backup and legacy log; existing destination/no merge; custom override; and injected move failure with the intact old tree selected. Identity checks compare assembly metadata/derived identifiers and open both bundled assets through StandardAssetLoader without initializing the desktop platform.

## Validation and remaining work

Final required command (AVALONIA_TELEMETRY_OPTOUT=1):

```text
dotnet build product/AntiNotepad -c Debug -o build/n1-check
  Determining projects to restore...
  Restored F:\LocalVSProj\Visual Studio Apps\AntiQuickPad\product\AntiNotepad\AntiNotepad.csproj (in 219 ms).
  AntiNotepad -> F:\LocalVSProj\Visual Studio Apps\AntiQuickPad\build\n1-check\AntiNotepad.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:03.92
```

```text
build/n1-check/AntiNotepad.exe --selftest
SELFTEST PASS
```

Selftest process exit: 0. Build exit: 0. Additional single-source verification with MSBuild global properties `-p:AppName=N1IdentityProbe -p:AppVersion=0.5.1` also built with 0 warnings/errors and printed SELFTEST PASS, including metadata and resource URI checks. The final required build above restored the default identity; AppInfo.props was never changed for the probe.

The first selftest attempt printed `SELFTEST FAIL / app identity: Unable to locate 'Avalonia.Platform.IAssetLoader'.` Director fixed the test to use StandardAssetLoader directly; final passes include that fix. Director also reviewed and corrected the migration fixture directory check and assembly-version metadata inspection before final validation. No desktop app initialization was needed.

`git diff --check` passes for tracked changes (Git prints line-ending normalization notices). Branch remains `feat/N1-antinotepad-identity`. No commit attempted.

### Final case-insensitive search

Equivalent of the requested recursive grep, including hidden/ignored files:

```powershell
rg -l -i --hidden --no-ignore antipad . --glob '!**/archive/**' --glob '!**/.git/**' --glob '!**/build/**' --glob '!**/.coord/tasks/**' --glob '!**/.coord/reports/**'
```

Files with hits:

```text
.claude/atlas/966c1dc8-0742-4d57-af39-7b41c8396fd3.json
.claude/atlas/4f4ae1d6-e998-4eb9-a1be-e100a230655b.json
.coord/logs/I3-win11-icon-menu-lab.director.log
.coord/logs/I4-lab-menu-tweaks.director.log
.coord/logs/I5-lab-menu-width.director.log
.coord/logs/I6-win11-menu-toolbar-in-app.worker.log
.coord/logs/I7-menu-corner-artifacts.worker.log
.coord/logs/I7b-menu-corners-dwm.backup.log
.coord/logs/N1-antinotepad-identity.director.log
product/AntiNotepad/Model/Store.cs
product/AntiNotepad/SelfTest.cs
product/AntiNotepad/Platform/Native.cs
product/AntiNotepad/Platform/DefaultDataFolderMigration.cs
```

The four source files contain only intentional, commented legacy folder/Run-value/v1-store/log-fixture references. AGENTS.md, docs, tooling and current STATUS have no old-name hits. The literal broad grep requirement is not fully satisfied: historical conversation snapshots and logs retain old identity text. These records were preserved; one atlas file was already modified before work began, and the director log is actively written by the manager wrapper. Historical briefs/reports remain unchanged.

### Remaining / handoff

- `build/n1-check` remains. Automatic approval review rejected the cleanup action, first recursive removal and then a safer verified, bounded per-file/empty-directory removal, with only `blocked by policy` as the reason. No escalation is available. Manager must delete this check output.
- Old generated obj contents were safely moved from the renamed project into `build/n1-previous-obj` before building, so stale assembly artifacts are outside the source scan. Current obj contents are regenerated for AntiNotepad. Manager may discard the previous generated cache with other build artifacts.
- Historical snapshots/logs above are the remaining nonmigration grep exceptions.
- Real data migration, registry migration, GUI appearance, hotkeys and live swap are intentionally unverified. No user/dev product instance was launched, quit or killed. The manager must use the documented first-rename swap procedure when separately authorized; do not start both identities against real notes.
- Archive files, existing tags, past task briefs/reports and the preexisting atlas change were not edited by this task. New task briefs appearing during the run were also left alone.
