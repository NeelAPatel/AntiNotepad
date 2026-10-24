# A21-dev-identity

You are the director (gpt-6.1-sol medium). Plan, split and check the work; delegate implementation and investigation to
gpt-6-luna xhigh subagents as you see fit. If subagent spawning is not available in this run, say so at the top of your report
and do the work yourself.

Repo: this folder, a git worktree on branch `feat/A21-dev-identity`. Never switch branch, merge or commit (.git is read-only);
the manager (a Claude session) reviews and commits. Subagents share this worktree: give each disjoint files.
App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). Read first: `AGENTS.md`, `docs/dev-loop.md` (dev copy vs
installed copy), `docs/decisions.md`. Match the surrounding style. First-party only, no packages.

## The user's request (urgent)
When the installed (prod) copy and the dev copy run at the same time, they look identical in the taskbar and have the same name
in Task Manager. The dev copy must be clearly different:
1. **Name:** "AntiNotepad (dev)" wherever Windows shows the program name for the dev copy: Task Manager (Processes and Details
   "Description"; this comes from the exe's version resource, i.e. AssemblyTitle / FileDescription / Product), the taskbar button
   tooltip / window title (already "(dev)" in `UI/PadWindow.cs` :40), the tray tooltip, and toasts/notifications if they show the
   app name. Find every place and report it.
2. **Icon color:** the dev copy uses a different-colored version of the app icon everywhere it appears: exe icon (taskbar,
   Task Manager, Alt+Tab), window icon, and tray icon. Derive it from the existing `antinotepad.ico` (all sizes) by a hue/tint
   change you script reproducibly (e.g. a small script under `tools/` that writes `antinotepad-dev.ico`), so it stays in sync if the
   main icon changes. Pick a color that's clearly distinct from the current icon at 16 px on light and dark taskbars; say which
   and why. Commit the generated file too.
3. **Prod is untouched:** the installed build (`tools/install.ps1`, a plain Release publish) keeps today's name and icon with no
   extra flags. Only dev builds (`tools/dev.ps1` / `tools/dev.sh`, which build into `build/dev`) get the dev identity. "When it
   migrates to main it will automatically be packaged with the current name and color": the dev identity must come only from
   how the dev scripts build, never be left on in the default build.

## Design constraints
- Prefer a build property set by the dev scripts (e.g. `-p:AntiNotepadDev=true`) that, in the csproj, switches `AssemblyTitle`,
  `Product`/`FileDescription` and `ApplicationIcon`, plus embeds the dev icon as the Avalonia resource used at runtime. Check
  how `AppInfo.Name` is derived (it reads `AssemblyProductAttribute`): changing Product would change `AppInfo.Name`, which also
  names the instance id, data folder, env prefix, run key, log and avares URIs. That must NOT change for dev (the dev instance
  already has its own instance id and data folder). So keep `AppInfo.Name` = "AntiNotepad" and change only the display
  strings and icons, e.g. via a separate assembly attribute or metadata. Verify `avares://` URIs still resolve.
- Runtime: choose window and tray icons from the build flag (or `Program.IsDev` if that's equivalent; explain which and why: a
  dev build running without the dev env should still look dev if it was built as dev; report your choice).
- Other parallel work: A20-taskbar-icon-setting edits `UI/PadWindow*.cs` and `Platform/` for a taskbar setting, and A18 edits
  Settings search. Keep your edits to `UI/PadWindow.cs` / tray code to the icon and title lines only.

## Files in scope
`product/AntiNotepad/AntiNotepad.csproj`, `product/AntiNotepad/AppInfo.cs`, the icon assets, the tray code in `Platform/`,
`UI/PadWindow.cs` (icon/title lines only), `Program.cs` (only if needed), `tools/dev.ps1`, `tools/dev.sh`, a new icon script under
`tools/`, `SelfTest.cs` (ONE new method at the END with one call line in `Run`), `docs/dev-loop.md`.

## Selftest
- the default build reports the normal display name and icon URI; `AppInfo.Name` and the instance/data names are unchanged;
- the dev icon resource exists and loads (in a dev-flag build the check asserts the dev name/icon; in a default build it asserts
  the normal ones).

## Done means
- Default build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a21-check` → 0 errors,
  0 warnings (if NU1900 appears only because nuget.org is unreachable, add `-p:NuGetAudit=false` and say so); `--selftest` PASS.
  Read the exe's FileVersionInfo (FileDescription, ProductName) and show it is the normal name.
- Dev-flag build into `build/a21-dev-check` the same way the dev scripts will build it: 0 warnings, `--selftest` PASS, and
  FileVersionInfo shows "AntiNotepad (dev)". Extract/compare the exe's icon to show it is the dev icon if you can.
- Do NOT run `tools/dev.ps1` / `dev.sh` (they launch the app and stop the running dev copy). Never launch AntiNotepad other than
  `--selftest`, never kill it, never read or write `%LOCALAPPDATA%\AntiNotepad`, `%LOCALAPPDATA%\Programs\AntiNotepad` or the
  real Documents folder. Delete your build check folders if you can.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A21-dev-identity.md`: how you split the work, every place the name/icon now differs, the color pick,
  how prod stays unchanged, checks run, a precise live-check list for a person ("needs a live check": taskbar with both copies
  running, Task Manager names, tray icons), anything not done.
