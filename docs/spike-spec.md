# Spike spec (identical for A and B)

Build a disposable prototype in `spikes/<A|B>/`. Report in `docs/spikes/<A|B>/REPORT.md`: pass/fail per item, numbers, screenshots, effort estimate, and what blocked you.

1. Resident tray app. Global hotkey toggles a frameless window that stays initialized while hidden.
2. Backgrounds: true transparent, Acrylic, Mica. Which work; is text crisp.
3. Fixture note: heading colors by level; bold/italic/strike with visible markers; clickable `- [ ]` toggling the text; Tab/Shift+Tab indent on selection; fenced python block highlighted, rest stays markdown.
4. Carousel (MUST): position bound to the gesture (direct manipulation).
   - Content follows drag 1:1 (mouse drag, precision-touchpad two-finger horizontal pan, horizontal wheel delta).
   - On release distance + velocity decide advance or spring back; momentum then snap.
   - Rubber-band overscroll past newest note; release beyond threshold creates a new note; blank interior notes pruned.
   - Word-wrapped notes; vertical scroll within a note still works; horizontal gesture does not fight text selection.
5. Autosave to disk, restore on relaunch.
6. Wishlist probes (not blockers): code folding, line numbers. yes/no/effort.
7. Measurements (script where possible): cold launch to editable, hotkey to visible+focused, hide time, idle RAM, dev-loop friction.

Testing: about 10-15 smoke checks, disposable, header `# spike`. No exhaustive suites.
