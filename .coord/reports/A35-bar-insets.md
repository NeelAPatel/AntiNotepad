# A35-bar-insets

Status: implementation and headless gates complete; visual/material/performance gates pending the manager. Working tree only, manager commits.

## Validation
- No app launch/shutdown or installed-data access performed. Pad frame/DWM/corner/shadow/show-hide implementation unchanged; only popup attributes added.
- Live Windows visual/material/focus/z-order/move-resize and hotkey timings require the manager's dev copy. No runtime measurements claimed by headless checks.

## Reference techniques
- Avalonia 12.1.4 native PopupImpl shows without activation, handles WM_MOUSEACTIVATE with MA_NOACTIVATE, and uses an owned WS_POPUP / WS_EX_TOOLWINDOW with no taskbar button: https://raw.githubusercontent.com/AvaloniaUI/Avalonia/12.1.4/src/Windows/Avalonia.Win32/PopupImpl.cs
- VisualBrush and BlurEffect are first-party APIs: https://docs.avaloniaui.net/docs/graphics-animation/brushes and https://docs.avaloniaui.net/docs/graphics-animation/effects
- DWM corner preferences expose system rounding choices, not a custom 12 DIP native radius: https://learn.microsoft.com/windows/win32/api/dwmapi/ne-dwmapi-dwm_window_corner_preference

## Geometry review
- Added `UI/Chrome.cs` (`ChromeTokens`) as the shared source. Values: row 40, button 32, edge 4, left item 10, badge 20; text fade 12; top/pager/count/dot hover radii 8/8/8/9; pill inset 2, border 1, radius 12, bottom 4, float gap 8; derived pill 38 and docked strip 46; top edge/dense alpha .94/.86, dense/clear 40/52, midpoint .5 (46), blur radius 10, tint .5. Divider/toast offsets and reveal zones also live here.
- Geometry files: `UI/PadWindow.Layout.cs`, `UI/Kit.cs`, `Editor/PadView.cs`, `Editor/Elements.cs`, `Editor/Renderers.cs`, `SelfTest.cs`.
- Reviewed: top items center in the 40 row, right edge 4, badges height 20, docked editor/paper stop above strip, full-width bottom shade removed, feather fallback 40/46/52, float gap 8. Headless check covers derived dimensions and the 480-wide Hide x=444 formula; actual rendered placement remains a visual gate.
- Initial prescribed build passed with 0 errors and 2 NU1900 warnings (NuGet vulnerability index unreachable). No compiler warnings.

## Material lab and limits
Additional files: new `UI/PadWindow.MaterialLab.cs`; changes in `UI/SettingsWindow.cs`, `UI/PadWindow.cs` (appearance integration and timing tag only), and `Platform/Native.cs` (separate guarded popup helper). No dependency added. Lab selectors are runtime-only under `Program.IsDev`; default routing also runs without dev UI and follows the existing Material setting.

| Variant | Result | Technique and limits |
|---|---|---|
| Acrylic window / Follow Acrylic | Partly: implemented; live result unverified | Owned native non-overlay Popup, AcrylicBlur > Mica > Transparent requests. Overlay custom placement at bottom inset; Float below uses gap 8. Popup-only DWM Round request (nominal 8); client pill remains 12 with 1 DIP border and hover fills. Native/client contours may differ. Actual material and fallbacks logged. |
| Follow Mica | Partly: implemented; Mica availability unverified | Mica > AcrylicBlur > Transparent; actual transparency drives effective routing. Mica is a system material, not guaranteed to blur note text. If unavailable, Overlay attempts editor blur; Float uses opaque Surface. |
| Blur behind (pill / top band) | Partly | Cropped VisualBrush of `_panes`, BlurEffect radius 10, Canvas tint .5. Top mask opaque through 40, half at 46, clear at 52. Pane edit/scroll and layout/finite animation changes refresh samples. Original sharp glyphs may remain visible under the translucent blurred copy: this is not proven true backdrop replacement. Float/docked explicit blur choice falls back to Surface because no editor lies underneath. Software rendering falls back. |
| Solid | Implemented; visual result unverified | Fully opaque palette Surface; client radius 12 and 1 DIP border. Overlay stays in-window; Float retains native popup positioning. |
| Feathered top fallback | Implemented; visual result unverified | Canvas .94 at edge, .86 at 40, .43 at 46, zero at 52. No solid full-width bottom shade. |

Native focus, owner z-order, Alt+Tab/taskbar absence, show-fade synchronization, hover/border appearance, move/resize lag, scale/multi-monitor alignment and native material appearance are **not live-verified**. Library source supports nonactivation/tool-window behavior; code observes visibility, opacity and transforms without rewriting pad lifecycle paths. Native opacity may not fade DWM's backdrop identically to client content. Screen-edge clipping remains possible to preserve the specified geometry.

## Costs and recommendation
- Hotkey-to-focus: **not measured for any variant** (app launch prohibited). Existing dev timing line now includes requested/effective pill and top variants.
- Typing/scroll blur cost: **not measured**. `bar-lab-render` logs event counts, render-call counts, total/max CPU drawing-command submission milliseconds for pill/top, and explicitly `gpu-render-ms=unavailable`. Divide total by render calls for mean CPU submission per call. This excludes actual GPU blur/completion and is not a full frame-time measurement.
- No permanent animation loop or bitmap capture was added; source observers detach while hidden and telemetry runs only after activity.
- Provisional shipped routing: Acrylic => native Acrylic (then Mica); Mica => native Mica (then Acrylic); Solid => opaque Surface. Native failure => sampled blur in Overlay where possible, Surface for Float. Top => sampled blur on GPU, feathered gradient on software. These defaults are wired, but native performance and the sampled-blur ghosting issue need the manager's visual gate before release.
- After the user picks: remove Lab rows/setters/override state and telemetry from `PadWindow.MaterialLab.cs`; retain shared tokens, chosen routing, visibility/placement observers needed by the chosen native pill, and top fallback. If blur loses, delete LabBackdrop/VisualBrush source-refresh and pane-observer code; if native loses, delete popup material routing and `Native.ConfigureMaterialPopupWindow` (Float still needs its popup).

## Final checks
- Final Debug build: 0 warnings, 0 errors, using the prescribed output/property plus `-p:NuGetAudit=false` because the initial prescribed build could not reach NuGet vulnerability data. No project audit configuration changed.
- Isolated executable with `ANTINOTEPAD_DATA` under `build/`: SELFTEST PASS (exit 0), including the single geometry check.
- Director reviewed both sequential worker diffs and corrected popup routing/guards, rounding helper, mask, gravity/offset, and Avalonia API compatibility. No commit made.
- Cleanup blocked: automatic approval review rejected recursive deletion of the verified task check-output folder with "blocked by policy". `build/A35-bar-insets-check` remains for manager cleanup.
