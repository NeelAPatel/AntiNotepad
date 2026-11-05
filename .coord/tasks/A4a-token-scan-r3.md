# A4a-token-scan-r3: count brush opacity and byte alpha

Same item card (`.coord/tasks/A4a-token-scan.md`) and the r2 brief; keep everything that works. Fix these gaps in
`tools/check-design-tokens.ps1` (plus regenerated baseline and inventory):

1. Opacity currently has 4 hits; A59 had ~39 brush-alpha and ~16 color-construction rows. Add and count, as category
   `opacity` (brush opacity literal, e.g. `.45`, `0.3`) and a new category `alpha` (byte alpha literal):
   - `new SolidColorBrush(<color>, .45)` (UI/PadMenu.cs lines ~84, 151, 228)
   - `Renderers.Brush(<color>, 0.x)` and any other brush helper taking a literal opacity argument (grep Editor/ and UI/ for
     helpers named `*Brush*` with a trailing literal 0<x<1 argument)
   - `Color.FromArgb(<literal byte>, x.R, x.G, x.B)` and `Color.FromArgb(cond ? (byte)110 : (byte)40, ...)` (UI/SettingsParts.cs
     lines ~20, 241, 397): count each as one `alpha` hit (not also as `color`, unless it already was in r2 as color; avoid
     double counting and say which).
   - literal 0 and 1 and 255 endpoints are not counted.
2. One SelfCheck fixture per form above (brush ctor, brush helper, FromArgb literal, FromArgb ternary), plus a no-count
   fixture for a named token / endpoint.
3. Regenerate baseline (must equal `-Update`) and inventory; re-run SelfCheck, default check, scratch-edit probe (revert),
   dedicated build + selftest, offline gate. Update `docs/coding.md` / `docs/loop.md` wording only if they list categories.
4. Overwrite `.coord/reports/A4a-token-scan.md` (same sections, final state, counts per category and top 10 files).
Do not touch dev, STATUS, product files or check-platform-seam.ps1. Do not commit.
