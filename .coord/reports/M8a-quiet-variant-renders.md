# M8a-quiet-variant-renders

## Built

Added an isolated `Design/` renderer for five Quiet Acrylic design variants. It uses the production `EditorFactory.Create` with the existing `DesignWindow.Fixture`, the Quiet light/dark `ShellPalette`, monospaced editor rendering, Segoe Fluent Icons and transparent editor/template layers. Each variant has a single 90% canvas tint layer, real route-shaped controls, idle/forced-hover states, and an Acrylic assertion. `Program.cs` and `DesignEntry.cs` now route `--render-variants` and `--glass-check` into this isolated design lifetime; this argument route was necessary because the existing CLI only recognized `--render-design` and `--probe-backdrop`.

The backdrop is a real borderless, topmost, non-activating Avalonia window covering the primary screen with a fixed generated wallpaper: warm/cool fields, high-contrast shapes, lines and large text. Variant windows are topmost, `WindowDecorations.BorderOnly`, extended into the client area, `ShowActivated=false`, and `TransparencyLevelHint=AcrylicBlur`. The runner waits 650 ms after `Opened`, asserts `ActualTransparencyLevel == AcrylicBlur`, and closes each window before the next.

Screen capture uses native physical window rectangles plus a 24-pixel margin. It asserts the rectangle is fully inside the synthetic backdrop, captures through GDI `BitBlt`, writes PNGs, and records physical pixels, DIP size, actual transparency and DPI in `render-manifest.json`. A denied screen read reports `CAPTURE DENIED: run outside the sandbox` and exits non-zero. The separate glass check captures V1 light-idle at tint 0% and 100%, computes mean RGB-channel difference and changed-pixel fraction, and selects inactive capture or foreground activation/restoration based on that measured result.

## Variants

- `V1-compact-pill`: 480 × 560 DIP, 14 DIP text, 24 DIP insets; icon-only top hover bar and small bottom count/dot pill; trades writing room for the closest AntiNote-like compactness; needs hover reveal/fade timing as a function task.
- `V2-roomy-calm`: 760 × 680 DIP, 15 DIP text, 48 DIP insets; full-width hairline-separated bars; trades compactness for generous reading room; needs bar reveal/fade and inactive-window policy as a function task.
- `V3-paper-grid`: 480 × 560 DIP, 14 DIP text, 24 DIP insets; V1 bars plus sparse dotted texture above the tint; trades some quiet for tactile paper orientation; needs paper preference and contrast/reduced-motion behavior as a function task.
- `V4-edge-strips`: 560 × 640 DIP, 14 DIP text, 32 DIP insets; full-width strips with title/date above and count/save below; trades overlay subtlety for explicit context; needs strip fade, inactive dimming and date-state behavior as a function task.
- `V5-bare`: 520 × 600 DIP, 14 DIP text, 28 DIP insets; faint `3 / 8` idle marker and compact bars over the text on forced hover; trades discoverability for the least furniture; needs hover/keyboard discoverability and auto-hide behavior as a function task.

## Gate output

- `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit`: exit 0.
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build -c Release --no-restore -p:UseSharedCompilation=false`: exit 0, 0 warnings, 0 errors.
- `ANTIQUICKPAD_ALIGNMENT_OUTPUT=F:\LocalVSProj\Visual Studio Apps\AntiQuickPad\.coord\scratch\M8a\align dotnet bin/Release/net10.0/AntiQuickPad.dll --selftest`: exit 0. Output included `BOUNDARY PASS (250 types checked)`, `CONFORMANCE quiet PASS (24 commands)`, `CONFORMANCE card PASS (24 commands)`, all existing Quiet/Card raster, heading, checkbox, strike, guide and active-line pixel proofs PASS, and `SELFTEST PASS`. The existing `window.move` known-gap diagnostic remained informational and did not fail the gate.
- Final required no-capture run: `dotnet bin/Release/net10.0/AntiQuickPad.dll --render-variants ../../.coord/scratch/M8a/nocap --only V1-compact-pill --no-capture` from `src/AntiQuickPad`: exit 0. All four states reported 480 × 560 DIP, 530 × 610 pixels at DPI 1, `actual=AcrylicBlur`; no screen pixels or PNGs were read. Manifest: `.coord/scratch/M8a/nocap/render-manifest.json` with `file: null` entries.
- Completion snapshot: `.coord/scratch/M8a/Design/`; marker: `.coord/scratch/M8a/done`.

No PNGs were inspected in the sandbox because GDI screen reads are denied by design. Therefore there is no sandbox visual defect claim; the manager's external run is the source of the real PNG review. The synthetic backdrop, forced hover states, and actual wallpaper/GPU behavior remain limits of these mock-ups.

## Manager capture commands

Run these from `F:\LocalVSProj\Visual Studio Apps\AntiQuickPad\src\AntiQuickPad`, in this order, outside the Codex sandbox:

```powershell
dotnet bin/Release/net10.0/AntiQuickPad.dll --glass-check ../../.coord/scratch/M8a/glass-check
dotnet bin/Release/net10.0/AntiQuickPad.dll --render-variants ../../docs/design/m8
```

The first command is the objective tint 0%/100% proof. The second writes all 20 PNGs under `docs/design/m8/<variantId>/<state>.png` and the final `docs/design/m8/render-manifest.json`, choosing inactive or activated capture from its measured glass decision and restoring the prior foreground window after any activated capture.

Re-render one variant:

```powershell
dotnet bin/Release/net10.0/AntiQuickPad.dll --render-variants ../../docs/design/m8 --only V3-paper-grid
```
