# A35-bar-insets: handoff

The user's spec for the pad's top bar and bottom pager (pill), in their words, tidied. Applies to **all three bar styles**
(Settings > UI > Controls > Bar style: Docked, Overlay, Float below). Screenshots are in this folder.

## Spec
Consistency matters: every item sits inside an even inset from the window edge.

**Top bar**
- The Hide button (top-right `—`; the user calls it "minimize") is **4 px from the top edge and 4 px from the right edge**.
  Today the insets are uneven (see `03-hide-button-uneven-inset-zoom.png`, `05-hide-button-inset-zoom.png`, red/green marks).
- Its total size is **32 px** high.
- As a consequence, the left and right clusters of the top bar move **down 1 px**, so they center-align with the Hide button.
- As a consequence, the badges (slot, mode such as timer/list, dev, and the "Saved …" text) are **10 px from the top edge**,
  and the leftmost item (a badge or the Saved text) is **10 px from the left edge** (see `04-top-edge-10px-note.webp`,
  `06-top-bar-annotated-spec.webp`).
- In Overlay, the bar's solid area ends **4 px below the Hide button**, so the whole top bar reads as having a 4 px inset with
  every button/item inside it at its own padding.

**Bottom pager (pill)**
- Same rules: total height **32 px including its 1 px border** (today 36 px).
- **4 px from the bottom edge**.

**Gradients / feathering / fall-off**
- The fade between bar and note text happens **within 15 px of height** (see the 15 px mark in `06-top-bar-annotated-spec.webp`;
  today's dark bands in `01-current-top-bar.png` and `02-current-pager.png` fall off over a much taller area).

## Files
- `01-current-top-bar.png`, `02-current-pager.png`: current r6 dev copy, Docked style.
- `03-hide-button-uneven-inset-zoom.png`, `05-hide-button-inset-zoom.png`: zoomed Hide button; red/green ticks show the uneven
  top/right insets.
- `04-top-edge-10px-note.webp`: the 10 px top-edge target for the bar items.
- `06-top-bar-annotated-spec.webp`: the user's annotated top bar (10 px, 4 px, 15 px).
- `07-broken-A34-full-window.png`: for contrast only, the reverted A34 lab build (thick native border); not the target.
