# A35-ux-pass-s3: rendered-tree checks (design dump)

Item chat, 2026-11-20. Source: `AntiNotepad.exe --design-dump` from a Debug build of this branch (`build/a35-check`;
headless, temp data, no window shown). The dump lists resolved properties for the pad, every Settings section, Help,
Search and the pad menu in all 8 palettes (18,048 lines). It has no bounds, so hit sizes come from the Codex code reads.

Counts below are for the `snow` palette (every palette has the same tree, 2,256 nodes).

| # | Finding | Evidence | Principle | Sev |
|---|---|---|---|---|
| R-1 | **Fluent theme default radius 3 leaks into ~60 buttons**: dropdown option buttons in popups and the wrap-panel toggle buttons in Settings (Editor, UI, Feel, System) render with radius 3, the Avalonia Fluent theme default, not `Shape.Control` 4. The token scanner cannot see it: no code sets it. | 235 nodes `CornerRadius=3` under `.../Popup/Border/StackPanel/Button` and `.../WrapPanel/Button` | V3, N4 | 🟢 |
| R-2 | Settings window tree holds Avalonia's drawn caption buttons at 10 px text (6 per section). Need a live check whether they ever show. | `.../WindowDrawnDecorations/.../StackPanel/Button` FontSize=10 | V2 (12 px floor) | 🟢 if hidden |
| R-3 | Rendered text sizes: 12 (810), 14 (354 + 238 semibold), 28 semibold (14), 18 semibold (2), 16 (2), 15 (editor, user setting). All on the ramp except the user's own editor size. | dump | V2 | no issue |
| R-4 | Rendered radii besides 3: 4, 8, 12 on scale; 10, 9 (round of 20 and 18: toggle track, swatch), 7 and 6 (inner radius of an 8 tile minus its 1 or 2 px ring), 1.5 (caret sample, round of 3). All explained. | dump | V3 | no issue |
| R-5 | Rendered strokes: 1 and 2 on scale; one 4 (`PinRingThickness`, a named token). | dump | V4 | no issue |
