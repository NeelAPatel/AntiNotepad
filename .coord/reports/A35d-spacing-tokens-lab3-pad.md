# A35d-spacing-tokens lab 3: pad

## 1. Files written
- `tools/playground/a35d-lab/pad.html`, `tools/playground/a35d-lab/help.js`, and this report.

## 2. Controls
- Width, bar mode, save-failure prompt, note and slot selection, split, menus, overlays, heading folds, checklist toggles, timer indicators and completion remain wired in the lab. Math results and timers are sample indications, not app functionality.
- The editor uses one contenteditable surface with source text retained in its DOM. Formatting, selection, editing, undo and browser-specific input need a browser hand check; they are a lab approximation of AvaloniaEdit.

## 3. Snap rows wired
- 12: help top and side margins, 14/10 to 16/12 px; 13: help panel right margin, 14 to 16 px.
- 14: completion row gap, 10 to 12 px; 15: save-failure spacing and margins, 6/6/9 to 8/8/8 px; 18: folded highlight radius, 9 to 8 px. These elements are marked for Show changes.

## 4. Captures and remaining differences
- Inspected `pad-AntiNotepad-dev`, `pad-slot-AntiNotepad-dev`, `help-top-AntiNotepad-dev`, `help-down-AntiNotepad-dev`, and `split-AntiNotepad-dev`. No browser rendering was permitted, so exact geometry, wrapping, colours and interaction parity against every capture remain unverified. The shared backdrop is a static gradient, not system Acrylic blur. Float mode remains inside the lab window rather than in a separate popup.
- The editor approximates Markdown and does not implement all AvaloniaEdit syntax and editing behavior; composition, complex selections, clipboard and undo require hand testing. Timer state can reset on redraw. Help and split geometry also require a browser comparison.

## 5. Helpers
- Local editable Markdown styling, caret offset mapping, selection restoration, simple undo, scrollbar activity and bottom-zone reveal helpers in `pad.html`; help uses a local backdrop cover.

## 6. Checks run
- `node --check` on `help.js` and the extracted inline `pad.html` script passed; all IDs used by the pad's `$` helper exist. No browser, server, AntiNotepad launch or commit, per shared rules.

## Fix round
1. Help now covers the editor with an opaque shared-backdrop surface below the top bar. Its content starts below the chrome and retains source-derived margins, section captions, key chips and text columns. Capture parity remains unverified.
2. Focus no longer switches to raw text. One contenteditable retains Markdown characters, dims markers on the caret line and hides them on other lines while styling headings and inline spans. Complex Markdown and browser editing behavior remain approximate.
3. Bullet lines use a two-character hanging indent; nested indentation and checkbox wrapping still need visual checking.
4. The editor scrollbar is transparent at rest and appears temporarily on scroll. The pager is hidden at rest and revealed near the bottom (20 px, or 8 px for float) or while interacting with it. The separate float popup and exact fade timing are approximations.
5. `LAB.backdrop` now takes effect on the window; editor typography is Cascadia Mono 15 regular at 1.32 line height, with source heading hues and palette text colors. The backdrop remains the core's Acrylic approximation.
6. Width, bar mode and save-failure controls remain above and outside the app window.
