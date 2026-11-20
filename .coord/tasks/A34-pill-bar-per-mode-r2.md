# A34-pill-bar-per-mode-r2: make the lab bars interactive

Role: terra, a rework of one existing HTML lab (docs/agents.md). Edit only `tools/playground/a34-pill-bar-lab.html`.
Read `.coord/tasks/A34-pill-bar-per-mode.md` first (Findings and "For Codex"). User reaction (2026-11-20): "this is a good start but I need them interactive and clickable. No need to show the page actions but the bar should be interactive."

## Changes
1. Every bottom pill in the page (sections 1, 3, 4, and Today) is a working mock with its own state:
   - Standard: prev/next arrows change the page number (wraps or stops at ends, say which); the count label updates; the slot-dot row, where kept, is clickable.
   - Permanent: clicking a dot selects that slot (selected dot moves); empty and filled dots stay distinguishable; hover and pressed states.
   - Each pill has a small shared mock note state (8 slots, some filled) so Standard and Permanent previews stay coherent.
2. The swap button switches the bar between Standard and Permanent in place (pill content changes with it), in every top-right option and every combined preview. Selected/current state visible.
3. Remove the Search, Split, Pin, Settings and Hide icons from all top-right mockups: show only the swap button (right-aligned, with a faint empty slot area to the left to keep the "room for later" idea; section 4 may still show dashed reserved boxes for future items).
4. Keep the recommendation and options text; update copy that mentions the removed icons.
5. Keyboard: arrow buttons and dots are real `<button>`s, focus ring visible, Enter/Space work.
Done means: file valid, JS has no syntax errors (check with `node --check` on the extracted script if node exists), no external requests, no identifier named `top` declared at script top level (it broke round 1). No other files, no .git.
Report: your final message, 6 lines max.
