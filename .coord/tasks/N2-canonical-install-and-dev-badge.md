# N2-canonical-install-and-dev-badge

You are the director (gpt-6.1-sol medium); delegate to gpt-6-luna xhigh subagents where useful.
Repo: this folder, branch `feat/N1-antinotepad-identity` (checked out; it already contains N1-antinotepad-identity). Never switch
branch, merge or commit. Read `AGENTS.md`, `docs/dev-loop.md` and `.coord/reports/N1-antinotepad-identity.md` first.

## Goal (user's words, summarized)
One canonical installed AntiNotepad replaces every old copy. Installed = the user's working copy on real data. Launching from the
repo through a script = a dev copy that uses the repo's own build and test data and shows a "dev" badge. No other badges.

## 1. Install / uninstall scripts (`tools/`)
- `tools/install.ps1`: publishes a Release build of `product/AntiNotepad` to the per-user install folder
  `%LOCALAPPDATA%\Programs\AntiNotepad` (self-contained not required; framework-dependent is fine if .NET 10 is present — say
  which you chose and why), creates/updates a Start menu shortcut "AntiNotepad", and keeps the app's own "start with Windows"
  setting pointing at the installed exe. Before replacing files it asks a running installed copy to save and quit
  (`AntiNotepad.exe --quit`) and waits for it to exit; after copying it runs `--selftest` on the installed exe, then starts it.
  Re-running it = upgrade in place. Never touches `%LOCALAPPDATA%\AntiNotepad` (the data).
- `tools/uninstall.ps1`: quits the installed copy, removes the install folder, the Start menu shortcut and the Run-at-login
  value; leaves the data folder unless `-RemoveData` is passed.
- You may write these but must NOT run install/uninstall yourself (no registry, no %LOCALAPPDATA%, no processes). Make
  `install.ps1` support `-WhatIf`-style dry output (`-DryRun`) and run only that.

## 2. Dev launch from the repo
- `tools/dev.ps1` (PowerShell twin of `tools/dev.sh`, same behaviour) and keep `tools/dev.sh`: build Debug to `build/dev`,
  launch with `ANTINOTEPAD_DATA=build/testdata`, hotkey Ctrl+Alt+Shift+D, its own instance id, kill only its own pid.
- The dev copy is identified by that launch route (the env var the scripts set), and only then shows the "dev" badge.

## 3. Badges
- Installed copy: no badge at all (remove "prod").
- Dev copy launched by the repo scripts: "dev" badge only.
- Remove the "claude" badge and whatever detects "launched by Claude" (`Program.LaunchedByClaude` or similar); Claude will use
  the repo scripts too.

## 4. Docs
- Update `docs/dev-loop.md` (installed vs dev copy, install/upgrade/uninstall, dev scripts, badges) and `AGENTS.md` if a line
  changes; the old "swap the user's copy" routine becomes "run tools/install.ps1".

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/n2-check`: 0 errors, 0 warnings;
  `build/n2-check/AntiNotepad.exe --selftest` prints SELFTEST PASS; delete `build/n2-check`.
- `pwsh tools/install.ps1 -DryRun` prints the plan without changing anything. Don't run anything else that installs.
- Report in `.coord/reports/N2-canonical-install-and-dev-badge.md`: scripts and what each does, badge logic, docs changed,
  build/selftest/dry-run output, anything the manager must do by hand.
