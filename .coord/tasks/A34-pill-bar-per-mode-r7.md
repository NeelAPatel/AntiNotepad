# A34-pill-bar-per-mode-r7: lab round 7, trim scope and apply agreed UX decisions

Role: quick, a surgical edit of one existing HTML lab (docs/agents.md). Edit only `tools/playground/a34-pill-bar-lab.html`. Keep everything else working. Same hygiene: single file, offline, no top-level identifier named `top`, `node --check` clean.
User (2026-11-20): "im likely going to move skip buttons out of scope", "lets move it [Created date] out of scope then no need for date at this time", and agreed to these:
1. Hide New note in Permanent but keep its corner empty (no layout shift when swapping views). In Corners the right corner stays reserved and empty; in the one-pill Float layout the pill keeps its width stable if possible (reserve the space, invisible, not collapsed) or note the shift in the caption. Remove the "New note in Permanent" setting row.
2. Delete honest in Permanent: tooltip and accessible name read "Clear slot N" (not "Delete note"); dimmed and non-clickable (`disabled`) on an empty slot; the Standard tooltip stays "Delete note".
3. No delete confirmation (keep the Undo toast).
4. Delete icon is the normal muted color at rest and turns the palette error red only on hover/focus (already so; make sure nothing is red at rest). In the one-pill Float layout separators default ON (already).
7. Corner buttons (Delete, New note) and the pager belong to the same bar layer: in Overlay and Float they reveal and hide together with the pager (one reveal group), never separately.
8. Dots (8 px) get a taller invisible hit area (about the full 32 px bar height, 16 px wide as today; keep the existing 24 px selected-dot width), so clicking just above or below a dot still hits it.

## Remove
- Skip first/last: setting row, buttons, state, disabled logic.
- Created date: setting row, state, formatting, the debug "soft save" strip and the speed-up checkbox, the seeded dates, and the top-left "after/instead of Saved" variants. The top-left keeps only "Saved 1:37 PM". Remove Created text from the "What this removes" panel and add one line: "Out of scope for A34: Skip first/last, Created date."
- Update the caption and the short "What this removes from today's bar" list to match.

## Done means
Valid HTML; script parses; no external requests; console clean. Final message, 5 lines max.
