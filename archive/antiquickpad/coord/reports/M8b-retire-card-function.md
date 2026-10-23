# M8b-retire-card-function

Completed on 2026-10-21. All gates passed on the first run; no gate fixes or restoration required. No git commands, network, packages, process killing or --measure runs.

## Removed routes

- Removed `shell.card` from FunctionContract, the controller command map, accepted SwitchShell IDs, App resident-shell selection and the documented Commands table.
- Removed only the Card tuple from the More menu shell list; Quiet remains.
- Updated `note.select` to `pager dot (Quiet)` in the doc, code mirror and live-route resolver.
- Program.cs changed only to reject `--render-shell card` before startup (exit 2); render usage now names Quiet. Ordinary unknown startup shell IDs retain the existing Classic fallback.

## Settings migration

SettingsStore.Load normalizes saved `card` (case insensitive) to `quiet`. Legacy `Card` / `Card System` looks map to `system`, `Card Light` to `light`, and `Card Dark` to `dark`; spelling normalization also accepts joined, hyphenated and underscored forms, case insensitive. Existing system/light/dark preferences are preserved. Null IDs have safe fallbacks and malformed JSON returns no snapshot. Migration does not write on load; the next normal save persists normalized IDs.

WindowSettings and WindowSnapshot gained optional `LayoutVersion = 0`, preserving old JSON compatibility. The single current-version constant is `MainViewModel.CurrentLayoutVersion = 1` (M8-antinote-ui-overhaul). Every MainViewModel.SaveSettings writes that version. Resident RestoreWindowSnapshot restores pin first, then ignores Width/Height/X/Y when the saved version is older. It leaves the window defaults and startup location alone. Current-version bounds restore through the existing platform path. No default/minimum dimensions, positions, colours, tokens or layout values changed.

## Selftest

- Conformance and light/dark production proofs run only for Quiet (23 commands); Card proofs, the Quiet-to-Card switching loop and Card-only peek/dot checks were removed.
- Quiet selection retains note/caret/document checks; theme, menu, navigation, persistence, pin, autosave, carousel and hidden focus checks remain.
- Added explicit rejection of the retired command and switch, and absence of the Card menu entry.
- Added legacy JSON smoke with card/Card, no version, 960 x 800 bounds and pin true. It exercises the resident restore/save methods, proves old bounds/startup unchanged and pin kept, checks re-save version/appearance, then restores current-version bounds. It also checks theme preference preservation, Card Light/Dark migration and malformed JSON tolerance. Fixtures and window state are restored afterward.
- Classic capability suite still passes; its appearance-persistence check now uses quiet/dark to retain the same bounds-save preservation intent.
- Quiet pixel-proof expected values and all proof implementation files are untouched.

The hidden platform deliberately keeps X/Y off-screen; this is scripted bounds/persistence evidence, not physical monitor-placement validation.

## Dead Card code retained for M8c-v1-compact-shell

- `src/AntiQuickPad/Views/Card/CardLayout.cs`
- Card layout selection, decoration and `_shellId == "card"` branches in `Views/Common/ResidentShellWindow.Appearance.cs` (only its menu tuple changed).
- `Views/Common/ShellPalette.cs` Card palettes.
- `Views/Common/ResidentShellWindow.Alignment.cs` Card alignment expectations.
- Card design fixtures in `Design/DesignSpec.cs`, `Design/DesignEntry.cs`, `Design/DesignWindow.cs`.

No build-forced exception to the protected view/layout files was needed. The standalone `AntiQuickPad.Core.Smoke/Program.cs` still has its old card/Card round-trip expectation; it is outside the write allowlist and requested gate, so it was neither changed nor run. The manager should adjust that separate disposable check when it is next used.

## Gate evidence

From src/AntiQuickPad:

- `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit`: exit 0, no process kill.
- Each of the three small implementation steps compiled: 0 warnings, 0 errors.
- Final `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build -c Release --no-restore -p:UseSharedCompilation=false`: exit 0; 0 warnings, 0 errors. Log: `.coord/scratch/M8b/build.log`.
- `--selftest`, with ANTIQUICKPAD_ALIGNMENT_OUTPUT pointing to the absolute repository `.coord/scratch/M8b/align`: exit 0. Log: `.coord/scratch/M8b/selftest.log`.
- `BOUNDARY PASS (250 types checked)`.
- `CONFORMANCE quiet PASS (23 commands)`; existing documented window.move route-kind gap remains unchanged.
- `M8b-retire-card-function SETTINGS PASS` and `M8b-retire-card-function COMMAND PASS`.
- Quiet light/dark raster, heading, checkbox, strike and active-guide proofs PASS, unchanged expectations.
- Retained Classic capability suite PASS; hidden-window checks PASS with foreground unchanged.
- Before and after Quiet light render invocations exit 0; hidden render checks PASS.
- Card render rejection independently checked: exit 2, usage printed, no Card render directory produced.

## Render comparison

Captured `.coord/scratch/M8b/before` before editing and `.coord/scratch/M8b/after` after implementation. Compared all 14 PNGs by bytes and SHA-256. Eleven are byte-identical: main, bars-hover, carousel, fold-hover and empty at 1x/1.5x, plus fold-hover closeup. Only menu-open at 1x/1.5x and its closeup differ, as permitted: removing the Card row shortens the menu and moves the following separator/Quit entry. No menu geometry/token code changed. Full hashes: `.coord/scratch/M8b/render-comparison.txt`.

Source snapshot: `.coord/scratch/M8b/snapshot/AntiQuickPad` and `snapshot/AntiQuickPad.Core`, excluding bin/obj. SHA-256 manifest: `.coord/scratch/M8b/snapshot-sha256.txt`. Review diff and original touched-file backups are under `.coord/scratch/M8b/`. Completion marker: `.coord/scratch/M8b/done`.

STATUS.md and TRACKER.md were left to the manager because this task's explicit write-only list excludes them.
