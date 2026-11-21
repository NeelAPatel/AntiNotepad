# A35d-spacing-tokens lab 3: search

## 1. Files written
- `tools/playground/a35d-lab/search.js` and this report.

## 2. Controls that work
- Query filters all terms against note title and text, highlights snippet matches, and preserves the original `ctx.notes` index for `ctx.openNote(i)`. Up/down, grid left/right, hover, Enter and click move or open the current result. Ctrl+click selects results and updates the count.
- Grid/list and trash/back toggle, close and Escape work. Footer updates its count and hints. `ctx.trashNotes` or an array in `ctx.trash` supplies trash; the current pad context supplies neither, so its trash view is empty. Opening a trash result also calls `ctx.openNote(i)` with its trash-array index; restoration requires a host callback.
- Ctrl+Delete, Delete, Ctrl+Space, drag reorder, and persistence are not implemented; the source footer describes real app actions, not these lab behaviors.

## 3. Snap rows wired
- 9: overlay top and side inset, 14 → 16 px.
- 10: search tool group left margin and empty-result row side margin, 6 → 8 px.
- 11: each result title/snippet/meta stack gap, 3 → 4 px.

## 4. Capture comparisons and differences
- Inspected `search-empty-AntiNotepad-dev.png` and `search-results-AntiNotepad-dev.png`: source geometry, text styling and states are represented. The current pad supplies only its slot notes, including no showcase stack note or timestamps, so content/count differ. Background is a static gradient instead of compositor Acrylic; text rasterization, wrapping, scrolling and exact pixels remain unverified without a browser.
- Inspected `search-current2-dark-default.png`, `search-focus-toolbutton-dark-default.png`, `search-results-dark-default.png`, `search-results-solid-dark-default.png` and `search-results-solid-dark-default-2x.png`: selected and focus outlines and results follow the source; their older data/window/backdrop and the 2x scale are not reproduced as separate presets. Tool-button focus uses the shared core style. No browser screenshot comparison was allowed, so visual equivalence remains unverified.

## 5. Helpers
- Local note parsing, relative time, safe match highlighting and result rendering; local CSS provides the search-specific cards and panel. Core has no search builder.

## 6. Checks run
- `node --check tools/playground/a35d-lab/search.js` passed.
- Offline Node DOM-stub smoke passed filtering, highlighting, original result index, arrow/Enter, grid, trash, close and unmount. It does not verify browser layout or pointer rendering. No DOM IDs are used by this file.
