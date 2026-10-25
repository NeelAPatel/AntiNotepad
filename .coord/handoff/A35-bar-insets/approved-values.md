# A35-bar-insets: values locked by the user (2026-10-25)

Mockup: https://claude.ai/artifact/Mo8BBWxPk2f4t8mvChRaQn (source copy: `mockup.html`). All numbers in app px (DIP).

## Locked
```json
{
 "backdrop": "blur",
 "dockedRow": 40,
 "textFade": 12,
 "top": { "edgeA": 0.94, "denseA": 0.86, "dense": 40, "clear": 52, "mid": 50, "blur": 10, "blurTint": 0.5 },
 "pill": { "inset": 2, "btn": 32, "bottom": 4, "gap": 8, "radius": 12 },
 "radii": { "top": 8, "pager": 8, "count": 8, "dot": 9 }
}
```
- Top bar: Hide button 32x32 at 4 top / 4 right; both clusters centered on y 20; badges 20 tall (top 10); leftmost item 10 from
  the left. Docked bar row 40.
- Top backdrop (Overlay, Float): never solid. Blur behind, tint 50% of the palette Canvas, blur 10, dense to 40, clear by 52,
  midpoint 50%. Blur feasibility in Avalonia still to be checked; fallback is the feathered gradient (edge 94%, dense 86%).
- Text edge fade band: 12 (was 40).
- Pill: 32 px buttons, 2 px inset on all sides inside a 1 px border = 38 tall; 4 from the bottom; Float gap 8 under the
  window; pill corner radius 12.
- Hover-fill radii: top bar icons 8, pager arrows 8, pager count 8, pager dots 9.

## Bottom bar change (user, 2026-10-25; mockup v6 awaiting confirmation)
- Docked: the pager gets its own empty strip like the top bar row (4 + 38 + 4 = 46); text never shows in it, only the text
  fade above it.
- Overlay: no full-width bottom shade; the pill itself wears the user's window material (Acrylic/Mica).
- Float below: the pill popup wears the user's window material.
