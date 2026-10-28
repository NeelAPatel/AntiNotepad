# A35-bar-insets-r4 report

## Implementation

- Each `PadView` now uses one editor `OpacityMask` for both the existing 12 px edge fade and the moving top-band mask. The
  mask combines their opacity factors into a single gradient, so there are no stacked masks. The top band is positioned from
  the actual top-layer and editor origins, follows pane and bar transforms, and follows the bar opacity; at opacity zero it
  removes the top mask. This stays in editor coordinates while the 40 px logical leading gap scrolls, so partially visible
  whitespace does not shift the mask away from the text.
- The blur band now has one source per pane, each sampling its `TextView` directly. This keeps the copy outside the masked
  `TextEditor` ancestor, so the band still blurs text after the editor mask hides the sharp copy. Both blur copies and the
  Canvas tint share the 40/46/52 feather. The top blur group has no Canvas backing.
- The pill blur lab retains its existing 0.78 Canvas tint. The top band has its own `ChromeTokens.TopBackdrop.TintAlpha`
  token at **0.4**. With the window already tinted toward Canvas, this pulls the band modestly toward Canvas instead of
  applying the prior 0.78 overlay; this should reduce the wallpaper-dependent step while preserving icon contrast. The
  actual light/dark wallpaper balance still needs the manager's live look.

## Files changed

- `product/AntiNotepad/Editor/PadView.cs` — compose the edge fade and top-band opacity into one mask.
- `product/AntiNotepad/UI/PadWindow.MaterialLab.cs` — screen-align each pane's unmasked TextView sample, follow slide and
  pane transforms, and remove the opaque band backing.
- `product/AntiNotepad/UI/Chrome.cs` — add the top-band tint token and feather-alpha helper.
- `product/AntiNotepad/SelfTest.cs` — check the 0/40/46/52 feather values and 0.4 tint token.
- `.coord/STATUS.md` — mark A35-bar-insets-r4 ready for the manager's visual review.

## Verification and manager look

- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A35-bar-insets-check -p:AntiNotepadDev=true -p:NuGetAudit=false` — **0 warnings, 0 errors**.
- Selftest: `--selftest` with `ANTINOTEPAD_DATA` under `build/A35-bar-insets-check` — **SELFTEST PASS**.
- `git diff --check` passes. The required recursive cleanup of `build/A35-bar-insets-check` was blocked by policy; the check
  folder remains.
- Manager live look needed: light and dark wallpapers in Overlay and Float; Acrylic, Mica, and Solid material; split view;
  top-band tint and icon readability; text freshness while typing, scrolling through the partial 40 px gap, folding, paging,
  and sliding; confirm line-1 selection and gutter chevrons remain intact. No live window was launched in this worker pass.

No commit made; the manager owns review and commit.
