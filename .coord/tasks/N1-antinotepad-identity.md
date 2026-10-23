# N1-antinotepad-identity

You are the director (gpt-6.1-sol medium); delegate to gpt-6-luna xhigh subagents where useful (e.g. one for the rename sweep,
one for the identity file and migration), and review their work.
Repo: this folder. Branch `feat/N1-antinotepad-identity` is checked out: work here, never switch branch or merge.
Read `AGENTS.md` and `docs/dev-loop.md` first.

## 1. Rename: Antipad -> AntiNotepad, everywhere
From now on the product is called **AntiNotepad** (exact casing). Leave no stone unturned, outside `archive/` and `.git`:
- Folder and project: `product/Antipad/` -> `product/AntiNotepad/`, `Antipad.csproj` -> `AntiNotepad.csproj`,
  `AssemblyName` and `RootNamespace` AntiNotepad (exe becomes `AntiNotepad.exe`), every `namespace Antipad...` / `using Antipad...`,
  `avares://Antipad/...` resource URIs, `antipad.ico` -> `antinotepad.ico` (update csproj, Tray, tools/make-icon.ps1).
- User-visible text: window titles, tray tooltip/menu, "Hide Antipad" -> "Hide AntiNotepad", Settings/About, welcome and
  showcase notes, help overlay, toasts, tooltips, selftest output. Badges stay "dev"/"prod".
- Identity strings: single-instance ids (`Antipad` -> `AntiNotepad`, `Antipad-dev` -> `AntiNotepad-dev`), mutex/pipe names, the
  Run-at-login registry value name, log file `antipad.log` -> `antinotepad.log`, selftest temp prefix, the data folder
  `%LOCALAPPDATA%\Antipad` -> `%LOCALAPPDATA%\AntiNotepad`.
- Env vars: `ANTIPAD_*` -> `ANTINOTEPAD_*` (DATA, HOTKEY, SHOTS, RENDER, GPU_CACHE_MB, TRIM_SECONDS, DEBUG_SETTINGS_CYCLES, ...).
- Comments, docs and tooling: `AGENTS.md`, `docs/*.md`, `.coord/STATUS.md`, `tools/dev.sh` (build folders become
  `build/antinotepad` for the user's copy and `build/dev` stays for dev; pid file stays), `tools/press-hotkey.ps1`,
  `.coord/tools/codex-run.sh` if it mentions it. Past task briefs/reports in `.coord/tasks` and `.coord/reports` (I6, I7, I7b)
  are history: leave them as they are. Do not touch `archive/` or git tags.
- Afterwards `grep -ri antipad` outside archive/.git/build/.coord/tasks/.coord/reports must find nothing except the
  migration code that names the old identity on purpose (comment it as such).

## 2. Migration (the user has real notes and a running copy)
- On startup, if `%LOCALAPPDATA%\AntiNotepad` does not exist and `%LOCALAPPDATA%\Antipad` does, move (or copy, then rename the
  old one to `Antipad.migrated`) its whole contents across once, including notes, index, settings, backups and the log. Never
  lose data: if anything fails, keep using the old folder and log it.
- Run at login: if the old `Antipad` Run value exists, replace it with the new `AntiNotepad` value pointing at the new exe path.
- Only the default data folder migrates; a custom `ANTINOTEPAD_DATA` is used as is.
- Add a `--selftest` check for the migration using temp folders (never the real `%LOCALAPPDATA%`).

## 3. One file for the app's identity
Create a single source of truth for the product's identity: name, version, pre-release label, short description (the About
line, today "A quick scratchpad for Windows"), copyright/author line, and the derived ids (instance id, data folder name,
Run value name, log file name, env var prefix). Recommended shape:
- `product/AntiNotepad/AppInfo.props` (MSBuild, imported by the csproj) holds Name, Version `0.5.0`, VersionLabel `pre-release`
  (informational version `0.5.0-pre-release`), Description and Copyright, and feeds the assembly attributes (Product, Title,
  Version, FileVersion, InformationalVersion, Description, Copyright).
- `product/AntiNotepad/AppInfo.cs`: a static class that reads those assembly attributes and exposes Name, Version, Label,
  Description, Copyright, plus the derived identity strings. Everything in the app uses AppInfo (window titles, tray, About page,
  instance ids, data folder, Run value, log name, env vars) instead of literals.
Changing the name or version must need exactly one edit, in AppInfo.props. If a cleaner single-file design exists, use it and
explain why in the report. Settings > About shows "AntiNotepad 0.5.0 (pre-release)" and the description.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/n1-check`: 0 errors, 0 warnings;
  `build/n1-check/AntiNotepad.exe --selftest` prints SELFTEST PASS (including the migration check); delete `build/n1-check`.
- Do not launch, quit or kill any running Antipad/AntiNotepad; don't touch `%LOCALAPPDATA%` or the registry outside selftest
  temp folders. Don't try to commit (.git is read-only for you).
- Report in `.coord/reports/N1-antinotepad-identity.md`: the identity-file design, the full list of renamed identifiers
  (old -> new), the migration behaviour, the final `grep -ri antipad` result, build and selftest output, anything left undone.
