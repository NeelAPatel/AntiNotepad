# A35-bar-insets-r2

Status: implementation and headless gates complete on `feat/A35-bar-insets`; uncommitted for manager review. Three sequential gpt-6-luna xhigh workers handled routes/tokens, blur, then glyphs; director reviewed each diff and corrected review findings before final gates. Live visual/performance gates remain.

## Removed and retained

Old line numbers below refer to the round-1 product code in `0e5cd2c` (round-2 brief HEAD), not the rewritten files.

| File / old lines | Removal |
|---|---|
| `UI/PadWindow.MaterialLab.cs`, 204–282, 367–404, 516–577, 599–609 | Native acrylic/mica pill routing and fallback, custom overlay popup placement, popup material requests, opacity/placement synchronization, native material observations/logs, top selector and variant tag. Associated state/handlers removed. |
| `Platform/Native.cs`, 81–98 | `ConfigureMaterialPopupWindow` and its Round8 constant (18 lines deleted). |
| `UI/PadWindow.Layout.cs`, 282–299, 346–375 | Feathered palette gradient and native overlay popup slide branch. The old top shade control is also gone. |
| `UI/SettingsWindow.cs`, 365–372; `UI/PadWindow.cs`, 144–145 | Follow material/Acrylic window and top-backdrop Lab choices, plus the obsolete hotkey material tag. Ordinary hotkey-to-focus timing stays. |

Overlay's pill lives in `_bottomLayer` and uses its original slide. Float retains the r6 fade and `PlacementMode.Bottom`, with offset fixed at `ChromeTokens.FloatGap` (8). Its popup requests only transparency, has a transparent fallback, and uses the existing generic popup helper to suppress native rounding/border behind the client-drawn 12 px pill. Every style/Material defaults to opaque palette Surface. Only the dev-only **Pill material: Solid / Blur behind** row remains; the blur choice samples the editor only in Overlay.

`ChromeTokens` now owns bar/pager spacing, glyph box, badges/fonts, count/separator/dots, hover radii, pill border/shadow and coupled clearances. `PillInset=4`, `PillHeight=42`, `PagerStripHeight=50`; the geometry selftest expects these numbers. Blur radius stays 10; Canvas blur tint is now **0.78**.

## Blur replacement and freshness

The top backdrop is one group: **opaque Canvas backing → blurred editor sample → Canvas tint**, with one opacity mask applied to the complete group. The backing occludes the original sharp editor in the fully shown dense band. The mask is opaque through 40, half at 46, clear by 52. Clipping confines the blur to the band. The dev pill uses the same opaque backing/tint and a rounded clip inside its border; padding remains opaque Canvas in this lab choice.

Avalonia 12.1.4 caches VisualBrush source render data and invalidates it on brush property changes. The old scroll path could keep an unchanged crop and reuse stale content. Each dirty refresh now creates a **new VisualBrush** and invalidates the sampling control. Refresh requests coalesce at Render priority after layout. Observers cover both panes' edits, scroll offsets, VisualLinesChanged (folding/document replacement), size/layout, visibility/opacity and arrival transforms, plus the content/top/bottom animation transforms. The actual inner blur control supplies sample coordinates, including pill border/padding and bar translation. Sampling stops while the window/band is hidden; there is no permanent refresh loop. Parent bar opacity still participates in its existing fade animation.

Sources checked: [Avalonia VisualBrush cache](https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.4/src/Avalonia.Base/Media/VisualBrush.cs), [Skia blur effect implementation](https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.4/src/Skia/Avalonia.Skia/DrawingContextImpl.Effects.cs), [raster RenderTargetBitmap implementation](https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.4/src/Skia/Avalonia.Skia/RenderTargetBitmapImpl.cs).

## Software result and measured costs

Offscreen Windows/Avalonia 12.1.4 Skia `RenderTargetBitmap` confirms software BlurEffect works. At radius 10, a pixel adjacent to the synthetic black mark changed from 255 sharp to 210 blurred. A text/translation change produced **7,947 changed pixels**. Software blur is therefore enabled, including Material Solid; no software gradient fallback is selected.

Thirty synthetic **280×90** software raster frames measured **0.60 ms mean / 0.80 ms p95 / 0.82 ms max**. These include rendering the synthetic fixture; they are not live pad, full-size editor, or GPU timings. Probe source/reproduction and evidence: [blur worker report](A35-bar-insets-r2-blur.md), [before](../../build/A35-r2-render-probe/before.png), [after](../../build/A35-r2-render-probe/after.png).

Dev `bar-lab-render` retains activity counts (scroll/edit/visual-lines/transform/layout/bar), mean/max refresh-request CPU and per-control draw-submit CPU, waits for a draw sample and flushes on hide. It labels `visualbrush-source-capture=deferred` and `gpu=unavailable`. **Those runtime fields exclude deferred source capture and blur execution; they are not a complete per-frame blur cost. Actual live scroll/typing frame CPU and hotkey-to-focus timings were not measured**, because launching the pad is prohibited in this brief. The existing hotkey log remains available to the manager.

## Glyph centering and offsets

`GlyphIcon` caches geometry and ink bounds per current render scale, draws the cached geometry with a cached brush, and snaps the translated ink center through the actual visual tree/client origin to device pixels. The box remains 18 DIP inside the 32 DIP button. The source-linked offscreen probe uses the production `Kit.cs` and `Chrome.cs`; the native TopLevel coordinate-conversion path is compiled/reviewed but not exercised by that windowless fixture.

Offsets are physical pixels `(x,y)` from each 32 DIP button center, using a 480x500 DIP pad with a zero client origin. Top-bar icon centers follow the actual right cluster (`Search=320`, `Split=355`, `Pin=390`, `Settings=425`, `Hide=460`; y=20). Pager centers use the stack-page layout with a one-digit count and nine unselected dots (`Back=106.5`, `Forward=188.5`; y=475). The old values are the analytic geometry-bounds center after the former local linebox-origin rounding; the new values are the global device-pixel snap offsets. Lists are ordered 100%, 125%, 150%. Direct old/new Hide raster-centroid measurements follow below.

| Glyph | Before `(x,y)` | After `(x,y)` |
|---|---|---|
| Hide | `(0,-0.801)` / `(-0.250,-1.251)` / `(+0.500,-0.701)` | `(0,0)` / `(0,0)` / `(0,0)` |
| Search | `(0,0)` / `(-0.250,-0.250)` / `(+0.500,+0.500)` | `(0,0)` / `(0,0)` / `(0,0)` |
| Split | `(0,0)` / `(-0.250,-0.250)` / `(+0.500,+0.500)` | `(0,0)` / `(+0.250,0)` / `(+0.500,0)` |
| Pin | `(-0.133,+0.113)` / `(-0.416,-0.108)` / `(+0.301,+0.670)` | `(0,0)` / `(+0.500,0)` / `(0,0)` |
| PinFilled | `(+0.367,-0.387)` / `(+0.209,-0.733)` / `(+1.051,-0.080)` | `(0,0)` / `(+0.500,0)` / `(0,0)` |
| Settings | `(0,0)` / `(-0.250,-0.250)` / `(+0.500,+0.500)` | `(0,0)` / `(-0.250,0)` / `(+0.500,0)` |
| Back | `(-0.250,0)` / `(-0.562,-0.250)` / `(+0.125,+0.500)` | `(+0.500,0)` / `(-0.125,+0.250)` / `(+0.250,+0.500)` |
| Forward | `(+0.250,0)` / `(+0.063,-0.250)` / `(+0.875,+0.500)` | `(+0.500,0)` / `(+0.375,+0.250)` / `(+0.250,+0.500)` |

The Hide glyph geometry is `(0,6.398,16,1.602)` in the 18 DIP box, so the old linebox center placed its vector ink bounds center high by `0.801`, `1.251`, and `0.701` px. In the actual offscreen text/geometry raster comparison, Hide's old darkness-weighted center was high by `0.521`, `0.670`, and `0.766` px at 100%, 125%, and 150%; the new offsets were `0.000`, `0.000`, and `+0.002` px. The new raster ink-bounds midpoint is exactly `20`, `25`, and `30` device pixels. The small `+0.002` weighted-center residual is antialias coverage, while the geometric and raster-bounds centers are exact.

The director inspected the before/after montages at all three scales; no obvious shape/alignment regression appeared in the synthetic comparison. Geometry drawing can change font hinting/antialiasing relative to DrawText. Pager half-pixel centers retain up to half a device pixel of rounding; the table makes those offsets explicit. Live crispness, font fallback, monitor/DPI transitions and other GlyphIcon users (settings/menus) still need a look. [Glyph report](A35-bar-insets-r2-glyph.md), [raw measurements](../../build/A35-r2-glyph-probe/results.txt), [100% montage](../../build/A35-r2-glyph-probe/montage-100.png), [125% montage](../../build/A35-r2-glyph-probe/montage-125.png), [150% montage](../../build/A35-r2-glyph-probe/montage-150.png).

## Final gates and remaining live look

- Final director build: `AVALONIA_TELEMETRY_OPTOUT=1`, `dotnet build product/AntiNotepad -c Debug -o build/A35-bar-insets-check -p:AntiNotepadDev=true -p:NuGetAudit=false`: **0 warnings, 0 errors**. Audit was disabled for this command only, consistent with round 1; project configuration unchanged. [Build log](../../build/A35-bar-insets-r2-build.log).
- Isolated `AntiNotepad.exe --selftest`, with `ANTINOTEPAD_DATA` under `build/A35-bar-insets-check/selftestdata`: **SELFTEST PASS**, exit 0. [Selftest log](../../build/A35-bar-insets-r2-selftest.log).
- No AntiNotepad GUI launch/kill, installed-data access, pad frame/DWM/corner/shadow/show-hide path changes, dependency additions, commit or tag. The ordinary Material setting's existing transparency routing remains unchanged.
- Manager live checks: top contrast/glow and sharp-text suppression; wheel/drag/keyboard scroll, typing/folding, paging/split and bar slide freshness; Solid/Acrylic/Mica in dark/light; float placement/corners/fade/focus; glyph appearance at 100/125/150 and UI scale. The retained dev pill Blur behind option is implemented/probed, pending that visual gate.
- Cleanup attempted using native PowerShell against only the task check output. **Automatic approval review rejected recursive deletion as "blocked by policy". `build/A35-bar-insets-check` remains for manager cleanup.** Probe folders and logs remain under `build/` as evidence.
