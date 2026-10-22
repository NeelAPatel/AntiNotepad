# AntiNote reference for M8-antinote-ui-overhaul (2026-10-21)

Source: ChatGPT research notes on AntiNote 2.1.x, pasted by the user 2026-10-21. Not verified on a Mac; window size and timings are proposals, not measurements. AntiNote 2.0 (2026-08-20) changed the layout (9 permanent slots, split view, new search/settings, Liquid Glass), so 1.x screenshots show identity, not current geometry.

Core principle: editor first, note management second. Everything else stays hidden until a gesture, shortcut, keyword or context needs it.

## UI targets (in scope for the overhaul)

| Area | Target | Note |
| --- | --- | --- |
| Window | One rounded, floating, resizable window; whole surface is the editor; no sidebar, ribbon, toolbar or tabs | Matches user identity: chrome-free + top/bottom hover bars |
| Size | ~440 x 540 DIP start, user-resizable | Proposal only |
| Metadata | Optional small status/date/note-count indicators | Subtle third text level |
| Type | Monospaced, 14-16 px, line height ~1.4x; 3 levels: content, emphasis, metadata | We already use mono only (F28) |
| Colour roles | Separate tokens for background/chrome, fg/muted, keywords, identifiers, numbers/results, links, selection, completed items, error/confirm, paper texture, caret. Not one accent for all | Feeds M6c-design-tokens |
| Surface | Light + dark palettes; native translucent material; high-contrast and opaque fallbacks; optional paper (blank, lined, dotted, small/large grid; subtle/clear/bold) | F25-translucency-live-look first |
| Bottom bar | Floating bar that fades when inactive (2.x slot selector) | Fits our bottom hover bar |
| Motion | Open/close ~100-170 ms; note navigation ~160-230 ms; hover feedback < 100 ms; search filter immediate; checkbox toggle immediate; reduced motion drops decorative transforms | Feeds M6d-motion-system; must not hurt show-to-focus (median <= 30 ms) |
| Windows fit | Use native Windows backdrop and shortcuts; do not copy traffic lights or Liquid Glass literally | |
| Quality | Visible focus and selection, screen reader labels, high DPI / mixed monitors, IME, text/caret/syntax/paper/blur stay aligned while resizing | Overlaps held F42, F43, F48, F51 |

## Function ideas (NOT UI work; candidates for later function tasks, user decides)

- Overscroll past the newest note creates a new note; abandoned empty notes disappear.
- Recoverable trash for deleted non-empty notes (overlaps F44-delete-placement-and-undo).
- Search surface: filter as you type, list and card grid, keyboard select, batch ops, drag reorder (M10-search-across-notes).
- Nine permanent slots plus split view (slot beside an ephemeral note).
- First-line modes: math (variables, units, coloured results), `code: py` (we chose fenced blocks first, decisions log 2026-10-17).
- Link shortening (display only), caret-side command/keyword completion.
- Utilities: timer, OCR, clipboard capture.
- Per-note caret and scroll restore; per-note undo/redo; crash recovery.

## Motion and video references

- AntiNote 2.0 launch video: https://www.youtube.com/watch?v=-tyOgBTZbxw (current 2.x layout and transitions; most important)
- Exceptional Mac Apps, AntiNote (2026-09): https://www.youtube.com/watch?v=OxbXh5I2a48
- Snazzy Labs segment (1.x identity): https://www.youtube.com/watch?v=npxlJdxSv4A&t=927s
- Official feature gallery (short looping clips per interaction): https://antinote.io/features
- Developer beta demos (swipe, math, variables, checklists): https://www.reddit.com/r/macapps/comments/1id97zc/
- 2.0 developer announcement: https://www.reddit.com/r/macapps/comments/1vx486e/

Advice from the notes: capture live 2.x frames before fixing exact window size, bar geometry or curves.
