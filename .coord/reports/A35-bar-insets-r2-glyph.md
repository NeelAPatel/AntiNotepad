# A35-bar-insets-r2 glyph centering

## Implementation

- `UI/Kit.cs`: `GlyphIcon` caches `FormattedText.BuildGeometry(default)` and its ink bounds for the current render scale. It aligns the geometry bounds center to the icon-box center, translates that center through the actual visual tree and client origin, snaps the target in device pixels, then translates back to local coordinates. It draws the cached outline geometry with a cached immutable brush; faux-weight copies keep their symmetric offsets.
- `UI/Chrome.cs`: added `GlyphIconBoxExtra = 2`, so the 16 DIP bar/pager glyph remains in its existing 18 DIP box, centered 7 DIP inside a 32 DIP button.
- No material, bar layout, or other setting changed. `GlyphIcon` is also used in settings and menus; those instances follow the same ink-centering requirement.

## Offsets

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

## Probe and limits

- Bounded source-linked probe: `build/A35-r2-glyph-probe/`. It links the production `Kit.cs` and `Chrome.cs`, uses `AppBuilder.Configure<ProbeApp>().UsePlatformDetect().SetupWithoutStarting()`, and renders through software `RenderTargetBitmap` at 96/120/144 DPI. No `Window` or AntiNotepad process was started.
- Raw output/table: `build/A35-r2-glyph-probe/results.txt`. Before/after montages: `montage-100.png`, `montage-125.png`, and `montage-150.png`; Hide-only raster captures: `hide-100.png`, `hide-125.png`, and `hide-150.png`.
- The montage shows the glyphs remain legible and crisp in the software renderer. Geometry drawing may differ slightly from `DrawText` font hinting/antialiasing; this offscreen check does not establish the live Windows appearance or system font fallback. The actual TopLevel/client-origin conversion path was reasoned against the 480x500 layout and compiled, but the probe has no native TopLevel. The manager's live dev-copy check remains necessary.
- Product Debug build: `dotnet build product/AntiNotepad -c Debug -o build/A35-bar-insets-check -p:AntiNotepadDev=true -p:NuGetAudit=false` succeeded with 0 warnings and 0 errors. No selftest or live visual check was run by this worker.
