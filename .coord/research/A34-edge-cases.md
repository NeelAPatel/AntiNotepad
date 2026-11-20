# A34-pill-bar-per-mode: edge cases and UX pass

Read 2026-11-20 from `PadWindow.Layout.cs` (BuildPill, ApplyBars, FadeChrome), `PadWindow.Navigation.cs`, `PadWindow.Keys.cs` (overlay hides `_pill`), `Shortcuts.cs`, `SelfTest/UI.cs` (reflects `_pill`). Principles named for the later whole-app UX pass: Nielsen heuristics (N), Fitts's law (F), layout stability (L), progressive disclosure (P), common fate (G).

| # | Situation | Decision | Principle |
|---|---|---|---|
| 1 | Split, left Standard active | One bar, pager content; right slot marked nowhere (no dots) | L |
| 2 | Split, right pane active | Dots content, both panes' current slots marked; Delete = "Clear slot N"; New note hidden (corner reserved) | N, L |
| 3 | Swap button in split | Acts on the left pane; icon shows the left pane's view | consistency |
| 4 | All 9 slots empty, swap to Permanent | Opens slot 1 (empty, editable); no extra prompt | N |
| 5 | Swap Permanent to Standard | `ShowStack()`; previous stack note restored (Current kept) | N |
| 6 | Delete a stack note | As today (`DeleteCurrent`): trash, "Moved to trash" + Undo + Ctrl+Z; focus returns to the pane | N user control |
| 7 | Delete twice quickly (double click) | Second click within 500 ms ignored | error prevention, F |
| 8 | Delete on sole blank stack note | Dimmed, no-op (today it silently re-adds a blank) | N status |
| 9 | Delete on empty slot | Dimmed, no-op | N status |
| 10 | Delete in a slot | Tooltip and name "Clear slot N"; text goes to trash; Undo | N match real world |
| 11 | New note while on the trailing blank note | Dimmed (it would land on the same note) | N status |
| 12 | New note in a slot view | Hidden, corner reserved | L |
| 13 | Both toggles off | Corners empty; pager centered exactly as today | consistency |
| 14 | Float style | One pill: Delete, pager, New note inside it, separators on (Settings: none; fixed) | G |
| 15 | Docked chrome "fade" | Whole bar (buttons + pager) fades and stops taking input together | G |
| 16 | Overlay reveal | Buttons are children of the bottom layer: reveal at the 20 px zone with the pager | G |
| 17 | Search overlay open | Whole bar hidden (today `_pill.IsVisible = false`; becomes the bar container) | N |
| 18 | 320 px wide window | Permanent: 42 + dots (about 162) + 42 = 246 px, fits; Float popup is 226 px, fits | F |
| 19 | Window corner radius vs 42 px squares at 4 px inset | Check at hand test; if clipped, inset by the corner radius token | F |
| 20 | Show/hide speed (priority) | Corner buttons built once and updated (enabled, tooltip), only the pager content rebuilt in `UpdateChrome`; no new timers | perf |
| 21 | Dot hit target | Button height 20 to 32 (full bar), width unchanged | F |
| 22 | Count button | Height 20 to 32 | F |
| 23 | Icon-only swap | Tooltip names the destination ("Show slots" / "Show stack") with the bound key; automation name set | recognition over recall |
| 24 | Keyboard | Alt+Left/Right and Ctrl+N unchanged; `swap-view` is bindable; slot-view arrows still return to the stack | N |
| 25 | Existing settings files | New keys default on / Pill; older files load unchanged | compat |
| 26 | Hover colors | Delete glyph error red, New note glyph accent only on hover and focus; never red at rest | N, color as signal |
| 27 | Welcome first run (slot 5 open) | Bar shows dots; swap shows the Permanent state | N |
| 28 | Toast above the bar | Margin uses the strip height (50) unchanged; the 42 px squares fit in it | L |

Open for later items: split's two bars (`split-view-modes`), edge reveal tuning (`bars-edge-reveal`), notice pill redesign (`status-pill-redesign`), Created date and Skip first/last (user: out of scope now).
