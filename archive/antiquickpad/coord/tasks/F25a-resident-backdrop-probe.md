# F25a-resident-backdrop-probe (for F25-translucency-live-look, M8-antinote-ui-overhaul)

Director: Codex gpt-6.1-sol medium. Do it yourself.
Branch: feat/ui-overhaul (the manager handles ALL git; you run no git commands that write).
Naming: always `<ID>-<slug>`, never a bare ID. Finish in ONE turn. End when the report is written, or when a gate fails twice.

## Why
The overhaul's identity is a chrome-free window with a translucent main background. The user will judge translucency live in a 5-minute check. Today's `--probe-backdrop` tests the wrong thing: a window with standard decorations (`WindowDecorations.Full`) and a plain text panel. The real pad (`Views/Common/ResidentShellWindow.cs`) uses `WindowDecorations.BorderOnly`, `ExtendClientAreaToDecorationsHint = true`, `ExtendClientAreaTitleBarHeightHint = 58`, and its layers are opaque all the way up (window `Background`, Quiet root `Grid` `Background = c.Canvas`, editor `Background = look.Background` in `Editor/Presentation/EditorPresentation.cs`). Hypothesis to check, not assume: any opaque layer above the backdrop hides it completely, which may explain the 2026-10-17 verdict "transparency effectively broken".

Make the probe look and behave like the real pad window, with live controls, so the user judges what would ship.

## Read
AGENTS.md, docs/function-contract.md (Ownership), docs/design/m6/README.md (probe section), `src/AntiQuickPad/Design/DesignEntry.cs`, `Views/Common/ResidentShellWindow.cs` + `.Appearance.cs`, `Views/Common/ShellLayoutContext.cs`, `Views/Common/ShellPalette.cs`, `Views/Quiet/QuietLayout.cs`, `Look.cs`, `Editor/EditorFactory.cs`, `Editor/Presentation/EditorPresentation.cs`.

## Write only
`src/AntiQuickPad/Design/` (new files are fine, e.g. `Design/BackdropProbeWindow.cs`), `docs/design/m6/README.md` (probe section only), `.coord/reports/F25a-resident-backdrop-probe.md`, scratch under `.coord/scratch/F25a/`.
NOT: Controller, ViewModels, Contract, Core, Editor/Behaviour, Editor/Presentation, Carousel, Shell, Views, Look/ShellPalette values, Program.cs (unless the usage line truly must change; say why). No git, no network, never kill processes. No new packages.

## Build: `--probe-backdrop <none|mica|acrylic|blur> [--theme light|dark] [--tint 0-100] [--report path] [--close-after-ms ms]`
Keep the existing arguments working (the starting material; `--report`/`--close-after-ms` as today). The window:
1. **Decorations as the resident pad:** `WindowDecorations.BorderOnly`, `ExtendClientAreaToDecorationsHint = true`, `ExtendClientAreaTitleBarHeightHint = 58`, 960 x 800 like the pad's default, centred. A transparent 58 DIP top strip that is a real drag region (same `WindowDecorationsElementRole.TitleBar` mechanism `ShellLayoutContext.DragRegion` uses), so the window can be moved and edge-resized like the pad.
2. **Editor-like layers:** a real AvaloniaEdit editor from `EditorFactory.Create(look, null, new TextDocument(sample))` using the Quiet shell's own Look/palette for the chosen theme (same colours as the shipping Quiet light/dark), filled with a realistic sample note: H1, H2, a paragraph, a bullet list, checkboxes (one checked), struck text, and a fenced python block. Never open the notes store, hotkey, tray or single-instance channel. Make every layer above the backdrop transparent on this instance only (window, root, editor, its TextArea/TextView, scroll viewer) and put ONE tint layer (the Quiet canvas colour at the chosen tint alpha) directly over the backdrop. Do not edit Editor/Presentation to do this; set properties on the created instance. If some layer cannot be made transparent without touching forbidden folders, say exactly which in the report.
3. **Live keys** (editor keeps normal typing; use a modifier-free key only when the editor does not have focus, or use Ctrl+Alt+<key>; pick one scheme, show it in the HUD):
   - material: none / mica / acrylic / blur (switch `TransparencyLevelHint` at runtime; show requested and `ActualTransparencyLevel`);
   - theme: light / dark (Quiet palettes);
   - tint alpha down / up in steps 0, 15, 30, 45, 60, 75, 90, 100 %;
   - hide/show the HUD (for a clean look);
   - Esc or the window close: write the report and exit.
4. **HUD:** small, bottom-left, unobtrusive: material requested -> actual, theme, tint %, key help, and a one-line hint: "Check: bright + dark wallpaper behind, click another window (inactive look), move, resize, text sharpness."
5. **Report** (to `--report` when given; always also to stdout): OS, Avalonia version, start state, every state change with timestamp and actual level, and the final state at close (material, actual level, theme, tint). The manager reads the final state as the user's pick.

## Gate (all must pass)
From src/AntiQuickPad: `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit` (never kill); `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build -c Release --no-restore -p:UseSharedCompilation=false` = 0 warnings / 0 errors; `--selftest` (env `ANTIQUICKPAD_ALIGNMENT_OUTPUT` = `.coord/scratch/align`) exit 0 with `CONFORMANCE quiet PASS`, `CONFORMANCE card PASS`, `BOUNDARY PASS`, all pixel proofs PASS, unchanged expected values. Then exactly two short visible probe runs (they pop a window for about 1.5 s; keep it to these two): `--probe-backdrop acrylic --theme dark --tint 45 --report .coord/scratch/F25a/probe-acrylic.txt --close-after-ms 600` and the same with `mica` and `--theme light`. Both exit 0 and report an actual level. Do NOT run `--measure`.
- Gate passes: copy `src/AntiQuickPad/Design/` to `.coord/scratch/F25a/Design/` and write `.coord/scratch/F25a/done` with a one-line summary.
- Gate fails: up to two fix attempts; then restore the start state, write `.coord/scratch/F25a/failed`, and STOP.

## Report `.coord/reports/F25a-resident-backdrop-probe.md`
What was built, the key scheme, which layers were made transparent and how, anything that could not be made transparent, gate output, the two probe reports, and the exact commands the manager gives the user for the live check (one per material, plus how to read the final state).
