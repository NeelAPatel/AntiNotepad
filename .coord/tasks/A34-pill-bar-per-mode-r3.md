# A34-pill-bar-per-mode-r3: make "Today" and every option behave like the real app

Role: terra, rework of one HTML lab (docs/agents.md). Edit only `tools/playground/a34-pill-bar-lab.html`.
User (2026-11-20): "the interaction for Today is already way inaccurate from what we're doing in the app. Please examine codebase properly."
Read first: `.coord/tasks/A34-pill-bar-per-mode.md`, `product/AntiNotepad/UI/PadWindow.Layout.cs` (BuildPill, lines ~496-533), `PadWindow.Navigation.cs` (Navigate, ShowStack, OpenSlot, PromoteToSlot, CycleSplitSlot, EdgeNotice), `Theme/ChromeTokens.cs` (Pager*, Pill*), `UI/Shortcuts.cs`. The facts below were verified from that code; re-check anything you doubt.

## Today's real behavior (build ONE shared mock model that implements exactly this)
Model: a stack of notes (grows; start with 5, one blank allowed at the end) plus 9 slots (some filled, titles), `viewSlot` (0 = stack), `current`.
Pill (one row, always the same two groups): `[‹] [count] [›] | 9 slot dots`.
- Count text: on the stack `"n / N"` (muted); in a slot view the text is `"stack"` (faint). Count box width = 32 + 14 * digits, tabular numerals.
- Count click: on the stack it opens Search (mock: show a small "Search opens" toast); in a slot view it returns to the stack.
- ‹ / › on the stack: move one note. Before note 1: no wrap, bump and notice "No older Notes". Past the last note: if the last note is empty notice "Already at Latest"; otherwise a new empty note is appended, you land on it, notice "New Note Created". Leaving an empty non-last note drops it ("Empty note removed"). In a slot view, either arrow just returns to the stack (ShowStack).
- Slot dots (always all 9, in BOTH views, today): current slot = wide accent pill (16x8) in a 24px button; filled = solid muted 8px dot; empty = 1px outline dot; buttons 16x20, hover radius 8. Clicking a dot opens that slot; clicking the current slot's dot returns to the stack. Tooltip: "Slot i · title|empty (key label)". Arrows tooltips: "Previous note (Alt+Left)", "Next note · past the last one starts a new note (Alt+Right)".
- Notice (toast) appears briefly above the pill (use a small fading pill, ~1.8 s).
- Pill chrome: radius 12, inset 4, border 1px at 30% alpha, shadow blur 12 y2; arrows 8px hover radius.
- Split view (optional toggle in Today only): right pane has its own slot; the dot of the right slot also shows as current; clicking it activates the right pane; arrows then cycle the right slot.
Show a small "state" readout under each Today bar (viewSlot, current, count) so the user can verify against the app.

## Then
1. Section 0 "Today": exactly that model and behavior, with the real swipe note: "Two-finger swipe, Alt+Left/Right also page".
2. Every proposed option (sections 1, 3, 4) must reuse the same model and keep the same notices and edge rules; only the proposed bar content differs (Standard: pagination only, with the same ‹ count › behavior; Permanent: dots only, with the same open / same-dot-returns behavior). The swap button must switch exactly like ShowStack/OpenSlot: Standard -> Permanent opens the last-used slot (or first filled), Permanent -> Standard returns to the stack at the previous `current`.
3. Where an option removes something the real bar has today (for example the dots in Standard, or the count button's Search/stack role), say so in that card's text: what is lost and where that action goes (for example Search stays on its key and top icon; "stack" return becomes the swap button).
4. Keep: no page-action icons, only the swap button; keyboard-accessible buttons; light and dark.
Done means: valid HTML, JS parses (`node --check` on the extracted script if node exists), no external requests, no top-level identifier named `top`. No other files, no .git. Final message: 6 lines max, including anything in the real code that surprised you.
