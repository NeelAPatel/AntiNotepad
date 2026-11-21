# A35e lab addendum: float bar

Role: terra, new HTML section in the existing lab. Edit only `tools/playground/a35ef-lab/` (add `floatbar.js`, `floatbar.css`, one link in `index.html`). Do not touch product code or other docs. Never commit. Read `product/AntiNotepad/UI/PadBar.cs`, `Theme/ChromeTokens.cs`, the lab's existing pad mock and `comparison.js`.

The user's hand test found the float bar (floating center pill, `BarStyle.Float`) badly spaced:
- Permanent (slots) view: poor spacing between Delete/Clear and the slot dots, and a stray separator at the far end of the pill.
- Standard (stack) view: [Delete] [<] [2 / 2] [>] [+ New] are jammed with uneven gaps and the New button hugs the next arrow; poorly designed overall.
Add a "Float bar" section showing, for both views and both themes (Paper, Ink), Before (reproduce today exactly from PadBar.cs: pill with 4 px inset, separator width 1, margins, 32 px targets) and three After options:
- A: one pill; groups Delete | pager | New; a separator only between two present groups; 8 px each side of every separator; all targets 32 x 32; 4 px between buttons in a group; slot dots keep 24 px targets, pitch 24.
- B: no separators; Delete and New become separate round 32 px buttons outside the pager pill, 8 px away from it; the pill holds only the pager (arrows + count, or the nine dots).
- C: like A but Delete/Clear is the only item left of the pager and New moves into the pager as the "next" of the last note (no separate New button in the pill).
Include Delete disabled, New hidden and "Delete off" combinations, a Show changes toggle with measured gaps in px, Tab focus rings (2 px Ink at 90%, 2 px gap), and print the pill width at the 320 px pad width and whether it fits. State your recommendation in one line. Compute the printed gaps from the DOM.
Done means: opens offline from `index.html`; report `.coord/reports/A35e-targets-keyboard-lab2.md`: what each option shows, the recommendation, anything that does not fit at 320 px.
