# A35-bar-insets-r2-blur

## Changes

- `PadWindow.MaterialLab.cs`: the top backdrop uses one opacity-masked group containing opaque Canvas, a fresh VisualBrush sample, and the Canvas tint. The mask stays dense through 40 px, reaches half at 46 px, and clears by 52 px. The group clips blur output to the band. Avalonia 12.1.4 software blur is enabled after the offscreen probe passed.
- The dev-only pill Blur behind choice remains optional and applies in Overlay only. It uses an opaque Canvas backing, a rounded clip inside the border, and the same tint token. Solid remains the default.
- Dirty refreshes coalesce at Render priority and allocate a new VisualBrush each time. Sources include scroll, edit, VisualLinesChanged, pane transform/opacity/visibility, split/layout size changes, and content/top/bottom transforms. Sampling pauses when the window or blur region is hidden.
- Dev `bar-lab-render` logs refresh-request CPU and per-control draw-submit CPU means/maxima, activity counts, and `gpu=unavailable`; VisualBrush source capture remains deferred and is not counted as draw time. Metrics wait for a draw sample and flush when the blur groups hide or the window closes.
- `Chrome.cs`: blur tint alpha token is `0.78`. `PadWindow.Layout.cs`: the pill host composes the backdrop group beneath pager controls.

## Evidence and limits

- Probe source: [Program.cs](../../build/A35-r2-render-probe/Program.cs); rendered evidence: [before.png](../../build/A35-r2-render-probe/before.png), [after.png](../../build/A35-r2-render-probe/after.png). It ran on Windows with `UsePlatformDetect().SetupWithoutStarting()` and offscreen Skia `RenderTargetBitmap`; no window was created. Reproduce with PowerShell:

  ```powershell
  $env:AVALONIA_TELEMETRY_OPTOUT = '1'
  dotnet restore build/A35-r2-render-probe/A35-r2-render-probe.csproj --ignore-failed-sources -p:NuGetAudit=false
  dotnet run --project build/A35-r2-render-probe/A35-r2-render-probe.csproj --no-restore
  ```

  At blur radius 10, a sharp sample beside the synthetic black line was `(255,255,255)` and the blurred sample was `(210,210,210)`. A synthetic text/translation update changed 7,947 pixels. Across 30 software raster frames, mean was 0.60 ms, p95 0.80 ms, and max 0.82 ms. These are synthetic software raster CPU timings, not live pad or GPU costs.
- Product build: `dotnet build product/AntiNotepad -c Debug -o build/A35-bar-insets-check -p:AntiNotepadDev=true -p:NuGetAudit=false` succeeded with 0 warnings and 0 errors.
- No selftest or live pad run was performed. The dev `bar-lab-render` log measures VisualBrush rebind/invalidation CPU and per-control `DrawRectangle` submission CPU; source capture and blur execution are deferred, and GPU cost is unavailable. Thus it does not measure full-frame latency during real scrolling or typing. Live freshness and visual appearance remain for the director's final gates.
