# A35d-spacing-tokens lab, round 3, part pad

Role: terra, one window with many states.

First read `.coord/tasks/A35d-spacing-tokens.lab3-common.md` in full and follow it. Write in small saved steps.

**Your files:** `tools/playground/a35d-lab/pad.html`
**Source to copy:** `UI/PadWindow*.cs`, `UI/PadBar.cs`, `UI/PadMenu.cs`, `UI/NoteSwipe.cs`, `Editor/PadView.cs`, `Editor/Renderers.cs`, `Editor/Colorizers.cs`, `Editor/Completion.cs`.
**Captures to match:** `pad-*`, `split-*`, `docs/playground/shots/pad-*`, `menu-*`, `toast-*`.
**Snap rows you wire:** 14 (completion), 15 (save-failure card), 18 (fold highlight radius).

**Must work:** the top bar buttons (stack, search, split, keep on top, settings, hide) with hover; an editable editor with the demo notes and their markdown, math, checklist, timer and fold rendering; pill bar, pager dots and count, slots and stack, docked, overlay and float modes; split view with two panes; the menu on right click and from the menu button; completion on `/`; toast with Undo; a lab-only 'Show save failure' button outside the window; width switch 460 or 354 outside the window. Load `help.js` and `search.js`, open help on F1 and search on Ctrl+F or the search button, and close both on Esc. Call `LAB.overlays.help.mount(host, ctx)` and `LAB.overlays.search.mount(host, ctx)`, where `ctx = {notes, close(), openNote(i), palette}`.

Report: `.coord/reports/A35d-spacing-tokens-lab3-pad.md`.
