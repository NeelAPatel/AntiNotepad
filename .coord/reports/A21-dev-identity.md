# A21-dev-identity

Implemented the scoped dev build identity. Default and dev builds pass all headless checks. One additional Windows title was found outside the allowed files: `UI/SettingsWindow.cs:54` still says `AntiNotepad Settings`; a scope question was sent, and that file is unchanged pending an answer. Manager should include its single title-line change to `AppInfo.DisplayName` if desired, coordinating with A18-search-speed.

## Work split

- Director: inspected identity/notification paths, integrated the hotkey tooltip update in Program.cs, added the selftest, reviewed diffs, built and tested both variants, verified executable resources, and wrote this report.
- gpt-6-luna xhigh `a21_identity`: csproj, AppInfo.cs, PadWindow title line and Tray display strings; then a temporary read-only Win32 executable icon verifier.
- gpt-6-luna xhigh `a21_icon`: reproducible tools/make-dev-icon.ps1 and generated antinotepad-dev.ico.
- gpt-6-luna xhigh `a21_scripts`: tools/dev.ps1, dev.sh and docs/dev-loop.md; static PowerShell parsing passed.

Files were disjoint. No branch switch, merge, commit, .git write, STATUS edit or audit-doc edit. The generated ICO is present for the manager to commit.

## Name and icon inventory

| Surface | Default | Dev-flag build | Source |
|---|---|---|---|
| Exe FileDescription / Task Manager description | AntiNotepad | AntiNotepad (dev) | AssemblyTitle |
| Exe ProductName | AntiNotepad | AntiNotepad (dev) | Product |
| Main window title / taskbar tooltip / Alt+Tab label | AntiNotepad | AntiNotepad (dev) | PadWindow title uses AppInfo.DisplayName |
| Exe icon / Task Manager / shell | Amber primary ICO | Blue dev ICO | ApplicationIcon |
| Main window icon / Alt+Tab / taskbar | Primary resource | Dev resource | AppInfo.IconUri; existing PadWindow icon loader |
| Tray icon | Primary resource | Dev resource | Existing Tray icon loader through AppInfo.IconUri |
| Tray tooltip, initial and after hotkey-status updates | AntiNotepad + status | AntiNotepad (dev) + status | Tray.cs and Program.cs |
| Tray Show / Quit menu items | AntiNotepad | AntiNotepad (dev) | AppInfo.DisplayName |
| Settings window icon | Primary, inherited from pad | Dev, inherited from pad | Existing SettingsWindow Icon = pad.Icon |

The current checkout has ShowInTaskbar=false; taskbar appearance must also be checked with the parallel A20-taskbar-icon-setting change when the button is enabled. No taskbar behavior was changed here.

Notification investigation: the app uses its own in-window Toast overlay, with message text and no app-name header; no Windows toast/balloon notification or AppUserModelID registration code was found. No notification display name requires changing in current code.

Outside the scoped Windows identity changes, SettingsWindow's title remains the base name as noted above. In-app Settings/About/help/context-menu and seeded note prose still use the stable base product name. The exe filename / Task Manager Details **Name** remains AntiNotepad.exe; the requested **Description** changes through FileDescription.

## Color and reproducibility

The primary page is amber #F2B84B. The dev page is saturated cool blue #4BA7F2 (HSL hue 207 degrees), a clear warm/cool distinction even in the 16 px frame, with dark ink retained for contrast. Light/dark taskbar appearance needs a live check; no visual Windows claim is made from the resource checks.

Run `pwsh -NoProfile -File tools/make-dev-icon.ps1` after changing the primary icon. It reads every existing frame, tints colored pixels, preserves near-neutral ink, alpha, geometry and frame metadata, and writes the generated ICO. Current sizes are 256, 64, 48, 32 and 16 px. Worker verification found zero per-pixel alpha differences and 6,177 unchanged exact #2B2A27 ink pixels. Repeated generation produced the same SHA-256:

`D43C02E2B577DAF5EEB21F8C75FF58807130F728DF48829095B08CC1C934A4E9`

## Build identity and production isolation

Only tools/dev.ps1 passes `-p:AntiNotepadDev=true`; dev.sh delegates to it. The csproj defaults the flag to false. Plain Debug/Release builds and tools/install.ps1's unchanged plain Release publish retain today's name and amber icon, regardless of branch. Both runtime icon resources are embedded, but only the selected icon is used.

Assembly metadata carries stable AppName separately from the display Product. AppInfo.Name stays AntiNotepad, assembly name stays AntiNotepad, and instance IDs (AntiNotepad / AntiNotepad-dev), data-folder name, Run key name, env prefix, log name, font URI and avares assembly root remain unchanged.

AppInfo.IsDevBuild, DisplayName and IconUri use build metadata, rather than Program.IsDev. A dev-flag binary therefore keeps its dev display identity without a dev environment. Program.IsDev still separately controls dev UI/badge via the existing environment and custom data-path gate. Build identity does not change runtime data/instance routing; normal app launches must still use the dev scripts' environment.

## Checks run

All builds used AVALONIA_TELEMETRY_OPTOUT=1. NuGet restore succeeded; no NuGetAudit override was needed.

| Check | Result |
|---|---|
| `dotnet build product/AntiNotepad -c Release -o build/a21-check` | Exit 0, 0 warnings, 0 errors |
| `dotnet build product/AntiNotepad -c Debug -o build/a21-dev-check -p:AntiNotepadDev=true` | Exit 0, 0 warnings, 0 errors |
| Default exe `--selftest` | Exit 0, SELFTEST PASS; normal display name and avares://AntiNotepad/antinotepad.ico |
| Dev exe `--selftest` | Exit 0, SELFTEST PASS; dev display name and avares://AntiNotepad/antinotepad-dev.ico |
| Default FileVersionInfo | FileDescription = AntiNotepad; ProductName = AntiNotepad |
| Dev FileVersionInfo | FileDescription = AntiNotepad (dev); ProductName = AntiNotepad (dev) |
| Plain Release rebuilt after dev build, then selftested | 0 warnings/errors; exit 0, SELFTEST PASS; FileDescription/ProductName restored to AntiNotepad |
| Exe raw icon resources | Win32 LoadLibraryEx as data/image resource, EnumResourceNames/FindResource/LoadResource: all 5 RT_ICON payloads in each exe match every corresponding source ICO frame byte-for-byte |
| `git diff --check` | Passed |

SelfTest.cs has one new method at the end and one new call line in Run. The existing AppInfoSelfTest comparison was minimally adjusted to compare Product/Title to DisplayName, since those intentionally differ from the stable Name in dev builds. The new method checks explicit expected display/icon identities and unchanged storage/resource identities, opens both embedded icons with StandardAssetLoader, and validates their ICO headers. It does not initialize a desktop session.

Neither dev launch script was run. AntiNotepad was executed only with --selftest; no existing app was stopped. No installed app, real app-home/data or real Documents folder was read or written. Cleanup was attempted with verified workspace paths, then explicit literal paths; automatic approval review rejected both commands as "blocked by policy." Consequently build/a21-check, build/a21-dev-check and build/a21-icon-verify.ps1 remain for the manager to remove. No cleanup succeeded.

## Needs a live check

1. Manager launches the dev copy through tools/dev.ps1 or dev.sh with the installed copy already running. Do not type into or stop the installed copy.
2. With the taskbar button enabled through A20-taskbar-icon-setting, confirm blue dev versus amber installed icon at 16 px on both light and dark taskbars; hover each and confirm the dev title suffix. Check Alt+Tab icons and labels too.
3. In Task Manager Processes confirm the dev display name/icon, then Details Description column and exe Properties > Details confirm AntiNotepad (dev). Details Name should still be AntiNotepad.exe for both.
4. Hover both tray icons: confirm blue versus amber and dev suffix, including after a dev hotkey status change; confirm the dev Show/Quit labels.
5. Open dev Settings and confirm it inherits the blue icon. Its title remains the documented out-of-scope gap until the manager approves that one-line change.

No live GUI or installed publish/install check was performed. Manager reviews and commits the scoped implementation and generated icon.
