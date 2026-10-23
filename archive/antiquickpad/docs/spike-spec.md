# Spike spec (identical for A and B)

Build a disposable prototype in `spikes/<A|B>/`. Report in `docs/spikes/<A|B>/REPORT.md`: pass/fail per item, numbers, screenshots, effort estimate, and what blocked you.

Status 2026-10-17: spikes are complete (A validated, B parked). The carousel item below was consolidated from three earlier versions (original touchpad-based gate, then a wheel-only gate, then page-switch clarification); the version here is the current intent. The real app plan supersedes this file.

1. Resident tray app. Global hotkey toggles a frameless window that stays initialized while hidden.
2. Backgrounds: true transparent, Acrylic, Mica. Which work; is text crisp. (De-prioritized: broken in both spikes.)
3. Fixture note: heading colors by level; bold/italic/strike with visible markers; clickable `- [ ]` toggling the text; Tab/Shift+Tab indent on selection; fenced python block highlighted, rest stays markdown.
4. Carousel (MUST).
   - **Input hardware:** the user has no precision touchpad, only a Logitech MX Master 3S with a notched horizontal wheel (one notch = WM_MOUSEHWHEEL delta 120, roughly 80-170 ms apart, no hi-res deltas). Record the mouse model/driver in reports.
   - **Wheel = page switch:** one horizontal notch (or a quick burst, coalesced) moves to the previous/next note with an animated slide. A wheel has no contact or lift, so a burst "ends" after a short idle gap (~80-150 ms, tunable).
   - **Mouse drag = direct manipulation:** content follows the pointer 1:1; on release distance + velocity decide advance or spring back; momentum then snap; rubber-band overscroll past the newest note.
   - **New and blank notes:** overscroll past the newest note (wheel burst or drag beyond a threshold) creates one new note; blank interior notes are pruned.
   - Word-wrapped notes; vertical scroll within a note still works; the horizontal gesture does not fight text selection.
   - **Optional, not a gate:** precision-touchpad two-finger pan, with finger-lift vs inertia distinguished (laptop users later).
5. Autosave to disk, restore on relaunch.
6. Wishlist probes (not blockers): code folding, line numbers. yes/no/effort.
7. Measurements (script where possible): cold launch to editable, hotkey to visible+focused, hide time, idle RAM, dev-loop friction.

Testing: about 10-15 smoke checks, disposable, header `# spike`. No exhaustive suites.
