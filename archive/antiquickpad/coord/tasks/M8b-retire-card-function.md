# M8b-retire-card-function (M8-antinote-ui-overhaul, function step before M8c-v1-compact-shell)

Director: Codex gpt-6.1-sol medium. Do it yourself.
Branch: feat/ui-overhaul (the manager handles ALL git; you run no git commands that write).
Naming: always `<ID>-<slug>`, never a bare ID. Finish in ONE turn. End when the report is written, or when a gate fails twice.
Compile discipline: build after each small change. Compile errors while writing are normal work, not gate failures. If you stop and restore, restore EVERY file you touched.

## Why
The user retired the Card shell (decisions log 2026-10-21) and picked the V1-compact-pill design (`docs/design/m8/`, 480 x 560 DIP window). The next task (M8c-v1-compact-shell, UI only) rebuilds Quiet as V1 and deletes the dead Card view code. Before that, the function side must stop offering Card and must not restore window bounds saved under the old 960 x 800 layout. This is a function task: no geometry, colour, token or layout changes.

## Read
AGENTS.md, docs/function-contract.md (whole file), docs/architecture.md, `App.cs`, `Contract/FunctionContract.cs`, `Controller/ResidentController.Commands.cs` (+ other partials as needed), `ViewModels/MainViewModel.cs`, `src/AntiQuickPad.Core/SettingsStore.cs`, `Views/Common/ResidentShellWindow.cs` (`RestoreWindowSnapshot`, `SaveWindowSnapshot`), `Views/Common/ResidentShellWindow.Appearance.cs` (menu item list, line ~111), `Views/Common/ResidentShellWindow.Smoke.cs`, `Views/Common/ResidentShellWindow.Contract.cs`, `Views/Classic/ClassicShellWindow.Smoke.cs`.

## Do
1. **Retire the shell.card command:** remove it from `FunctionContract`, the controller's command map and `SwitchShell` accepted IDs, `App` shell selection, and the Commands table in `docs/function-contract.md` (also fix the `note.select` route text that says "pager dot (Card)": Quiet has pager dots; say what is true now). Remove the Card entry from the More menu's shell list (the one-line join in `Appearance.cs`; touch nothing else there). If the shell list then holds only Quiet, keep the Quiet entry (the contract still lists `shell.quiet`).
2. **Migrate saved settings:** a saved `ShellId` of `card` loads as `quiet`; a saved `LookId` of `Card` (or any Card-only look ID) maps to the Quiet equivalent (system/light/dark preference unchanged). Never crash on old settings files.
3. **Drop old window bounds once:** add a layout version to `WindowSettings` (e.g. `int LayoutVersion = 0`, JSON back-compatible) and a single constant for the current version owned by the function side (e.g. on `MainViewModel`, value 1 = M8 layout). When saved settings carry an older version, ignore their Width/Height/X/Y (the window uses its own defaults and startup location) but keep Pinned, ShellId and LookId. Saving writes the current version. A minimal change in `RestoreWindowSnapshot`/`SaveWindowSnapshot` is allowed for this; no size values change in this task (M8c sets the new default size).
4. **Selftest:** shell-switch smokes and conformance run for Quiet only (Card proofs and the Quiet<->Card switching loop go). Keep the Classic capability suite working (its ShellId persistence check may use another valid ID; keep its intent). Add a smoke: old settings JSON with `ShellId: "card"`, `LookId: "Card"`, no LayoutVersion, 960 x 800 bounds -> loads as quiet, bounds ignored, Pinned kept; re-save writes the current LayoutVersion; a current-version file restores its bounds.
5. Leave `Views/Card/`, `CardLayout`, Card palette branches and `_shellId == "card"` branches in Views alone (dead code until M8c deletes it), unless the build forces a change; then make the smallest one and list it.

## Write only
`App.cs`, `Contract/`, `Controller/`, `ViewModels/`, `src/AntiQuickPad.Core/` (settings record + store only), `Views/Common/ResidentShellWindow.cs` (bounds restore/save only), `Views/Common/ResidentShellWindow.Appearance.cs` (the Card menu entry only), `Views/Common/ResidentShellWindow.Smoke.cs`, `Views/Common/ResidentShellWindow.Contract.cs`, `Views/Classic/ClassicShellWindow.Smoke.cs`, `Program.cs` (only if `--render-shell card` must be rejected now; say so), `docs/function-contract.md`, `.coord/reports/M8b-retire-card-function.md`, scratch under `.coord/scratch/M8b/`.
NOT: `Views/Quiet/`, `Views/Card/`, `ShellPalette`, `Look`, `Editor/`, `Carousel/`, `Design/`, any geometry/colour/token. No git, no network, never kill processes, no new packages.

## Gate (all must pass)
From src/AntiQuickPad: `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit` (never kill); `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build -c Release --no-restore -p:UseSharedCompilation=false` = 0 warnings / 0 errors; `--selftest` (env `ANTIQUICKPAD_ALIGNMENT_OUTPUT` = `.coord/scratch/M8b/align`) exit 0 with `CONFORMANCE quiet PASS`, `BOUNDARY PASS`, Quiet pixel proofs PASS with unchanged expected values, and the new settings smoke PASS. Render `--render-shell quiet light .coord/scratch/M8b/before` first and `.coord/scratch/M8b/after` at the end; the Quiet PNGs must be byte-identical except the More menu state if it is captured (explain any difference). Do NOT run `--measure`.
- Gate passes: copy `src/AntiQuickPad` and `src/AntiQuickPad.Core` (excluding bin/ and obj/) to `.coord/scratch/M8b/snapshot/` and write `.coord/scratch/M8b/done` with a one-line summary.
- Gate fails: up to two fix attempts; then restore every touched file, write `.coord/scratch/M8b/failed`, and STOP.

## Report `.coord/reports/M8b-retire-card-function.md`
What was removed, the settings migration rules, the layout-version rule and where the constant lives, the selftest changes (removed and added checks), dead Card code left for M8c (file list), gate output, render comparison.
