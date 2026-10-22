# M8a-quiet-variant-renders (M8-antinote-ui-overhaul, design step)

Director: Codex (tier per codex-run.sh). Do it yourself.
Branch: feat/ui-overhaul (the manager handles ALL git; you run no git commands that write).
Naming: always `<ID>-<slug>`, never a bare ID. Finish in ONE turn. End when the report is written, or when a gate fails twice.

## Sandbox limit (learned on attempt 1, 2026-10-21)
Your sandbox cannot read the screen: GDI `BitBlt` from the screen DC fails with error 5 (access denied). So **you build and verify the renderer; the manager runs the capture outside the sandbox.** Do not revert your work because capture is denied. Make capture failure a clear error message (`CAPTURE DENIED: run outside the sandbox`) with a non-zero exit, and add `--no-capture` which does everything except reading pixels (opens backdrop + windows, asserts AcrylicBlur, writes the manifest without files). The glass check (tint 0 vs 100) runs in the manager's run too: implement it as `--glass-check <outdir>` so the manager can run it first.

## Compile discipline (learned on attempt 2, 2026-10-21)
Attempt 2 stopped on compile errors alone. Write the renderer in small pieces and build after each piece. Compile errors while writing are normal work, NOT gate failures: fix them until the build is clean. The "two fix attempts" rule applies only to the full gate once the code compiles. Known pitfalls in this project: `Path` is ambiguous (use `System.IO.Path`); shapes live in `Avalonia.Controls.Shapes`; `GetVisualDescendants` needs `using Avalonia.VisualTree;`; `TemplatedControl` is in `Avalonia.Controls.Primitives`; `Canvas.SetLeft/SetTop` are static setters, not initializer members; `Screens` is an instance (`window.Screens.Primary`). If you do stop and restore, restore EVERY file you touched (attempt 2 left DesignEntry.cs half-edited and the build broken).

## Why
The user picks the overhaul's look from real renders before anything is built (decisions log 2026-10-18). Decided so far (decisions log 2026-10-21):
- **Reference:** AntiNote (Mac). Editor first; everything else stays hidden until needed. See `docs/design/antinote-reference-2026-10-21.md` (UI targets table).
- **Identity:** chrome-free window, top and bottom **hover bars** (hidden when idle, shown on hover), **Acrylic** background. User's live pick: Acrylic, Quiet light, tint 90 %. Not 100 % Mac finish; no traffic lights.
- **Density** is the main known complaint about today's Quiet (960 x 800 window, 90 DIP insets, see `Views/Quiet/QuietLayout.cs`); everything may change. The direction (more compact vs more air) is not decided: the variants must span it.
- **Base:** Quiet, light + dark. Card is being retired: do not use it.
- Translucency rule (F25 finding): every layer above the backdrop is transparent except ONE tint layer. The editor and its template must not paint a full-area background.

These are design mock-ups, not a shell. They show what each variant would look like; the chosen one is built later as a real UI task.

## Read
AGENTS.md, docs/function-contract.md (Ownership; "Building a new shell"), docs/design/antinote-reference-2026-10-21.md, docs/design/m6/README.md, `.coord/reports/F25a-resident-backdrop-probe.md`, `src/AntiQuickPad/Design/*.cs` (BackdropProbeWindow shows how layers were made transparent; DesignWindow/DesignSpec show the M6 renderer and its fixture), `Views/Quiet/QuietLayout.cs`, `Views/Common/ShellPalette.cs`, `Look.cs`, `Editor/EditorFactory.cs`.

## Write only
`src/AntiQuickPad/Design/` (new files are fine), `docs/design/m8/` (renders + README), `.coord/reports/M8a-quiet-variant-renders.md`, scratch under `.coord/scratch/M8a/`.
NOT: Controller, ViewModels, Contract, Core, Editor/Behaviour, Editor/Presentation, Carousel, Shell, Views, Look/ShellPalette values, Program.cs (unless the argument route truly must change; say why). No git, no network, never kill processes. No new packages (P/Invoke to user32/gdi32/dwmapi is fine).

## Build: `--render-variants <outdir> [--only <variantId>]`
A Design/ entry, isolated like `--probe-backdrop`: no notes store, hotkey, tray or single-instance channel.

### Backdrop (privacy and reproducibility, mandatory)
Captures read real screen pixels, so the user's own desktop must never appear. Before any capture, open a borderless, non-activating backdrop window that covers the capture area with a fixed, generated "wallpaper": large soft colour fields (warm and cool), some high-contrast shapes, and a few lines of large text, so the blur and tint are visible. Same image for every capture. Close it at the end. Each capture rectangle must lie fully inside it; assert that and fail otherwise. Z-order: the backdrop window and every variant window are topmost, the variant above the backdrop, so Acrylic samples only the synthetic wallpaper and no toast or other window can leak into a capture.

### Variant windows
Each variant is a real Avalonia window shown on screen: `WindowDecorations.BorderOnly`, `ExtendClientAreaToDecorationsHint = true`, title-bar height hint matching its top bar, `TransparencyLevelHint = AcrylicBlur`. Assert `ActualTransparencyLevel == AcrylicBlur` before capturing. Content: the real editor from `EditorFactory.Create` with the existing `DesignWindow.Fixture` text and the Quiet light or dark palette (`ShellPalette.Get("quiet", dark)`), all layers transparent except one tint layer (Quiet canvas colour, tint 90 %). Monospaced editor font as today. Icons: Segoe Fluent Icons glyphs as today. Mock content only: buttons look real but bind nothing.

Every variant places a visible route (button, bar item or menu entry) for each control today's Quiet exposes: new note, More menu, pin, close/hide, previous/next, pager dots, position text and the save line, so the chosen one can pass `CONFORMANCE` when built.

Five variants, each a distinct, coherent answer. Starting points; refine within the identity:
- **V1-compact-pill:** AntiNote-like size (about 480 x 560), 14 px text, about 24 DIP insets. Top hover bar: thin, icons only (pin, new, more, close). Bottom: small floating pill with "3 of 8" and dots.
- **V2-roomy-calm:** today's proportions slimmed (about 760 x 680), 15 px text, about 48 DIP insets, generous line height. Bars span the width with hairline separators.
- **V3-paper-grid:** V1's size with a subtle dotted paper texture drawn above the tint (still transparent in between), 14 px.
- **V4-edge-strips:** about 560 x 640; full-width top and bottom strips that fade in, holding title/date at the top and count + save state at the bottom.
- **V5-bare:** about 520 x 600; nothing visible when idle except a faint "3 / 8" in the bottom-right corner; on hover, compact floating bars overlay the text edges.

### States and captures
Per variant: `light-idle`, `light-hover` (both bars forced visible; no real hover needed), `dark-idle`, `dark-hover`. 5 x 4 = 20 PNGs. Show each window, wait until Acrylic has settled (at least 600 ms after Opened), capture the window's on-screen rectangle plus a 24 px margin (so the corners and shadow show) with GDI BitBlt from the screen DC in physical pixels, close it, then the next. Do NOT register windows with `HiddenWindows`, cloak them, or move them off-screen (a cloaked window is not composited and the capture would show only the backdrop). Show them on screen with `ShowActivated = false` (or `WS_EX_NOACTIVATE`).

**Glass check first (objective, mandatory):** before the full run, capture V1 light-idle at tint 0 and at tint 100 with the window NOT activated (save to `.coord/scratch/M8a/glass-check/`). Compare pixels over the backdrop's colour fields. If they do not differ clearly, inactive Acrylic is drawing its solid fallback: then activate each variant window just before its capture and restore the previous foreground window right after. Report which path was used and the measured difference. Use the same tint-0 vs tint-100 comparison as your "backdrop shows through" proof, not judgment by eye. Total visible time under 40 s. Report DPI scale and the actual pixel size of each capture.

Write `docs/design/m8/<variantId>/<state>.png`, a `docs/design/m8/render-manifest.json` (variant, state, size DIP and pixels, actual transparency level, file), and `docs/design/m8/README.md`: one short paragraph per variant (size, text size, insets, bars, what it trades, and any NEW behaviour it would need, e.g. hover-reveal timing, fade when inactive, auto-hide; each becomes a function task before the UI build), how the captures were made (inactive or activated path), and the limits (backdrop is synthetic; real wallpaper varies; hover is forced).

## Gate (all must pass)
From src/AntiQuickPad: `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit` (never kill); `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build -c Release --no-restore -p:UseSharedCompilation=false` = 0 warnings / 0 errors; `--selftest` (env `ANTIQUICKPAD_ALIGNMENT_OUTPUT` = `.coord/scratch/M8a/align`) exit 0 with `CONFORMANCE quiet PASS`, `CONFORMANCE card PASS`, `BOUNDARY PASS`, all pixel proofs PASS with unchanged expected values. Then ONE `--render-variants .coord/scratch/M8a/nocap --only V1-compact-pill --no-capture` run: exit 0 and a manifest entry reporting `AcrylicBlur`. Do NOT attempt real captures (they are denied in your sandbox). In the report give the exact commands the manager runs, in order: glass check, then full render to `docs/design/m8`. Do NOT run `--measure`.
- Gate passes: copy `src/AntiQuickPad/Design/` to `.coord/scratch/M8a/Design/` and write `.coord/scratch/M8a/done` with a one-line summary.
- Gate fails: up to two fix attempts; then restore the start state, write `.coord/scratch/M8a/failed`, and STOP.

## Report `.coord/reports/M8a-quiet-variant-renders.md`
What was built, how the backdrop and capture work, the five variants (one line each), gate output, render run output, anything that looked wrong in the PNGs, and the command to re-render one variant.
