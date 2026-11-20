# A34-pill-bar-per-mode-r4: lab round 4, the extras around the bar

Role: terra, rework of one HTML lab (docs/agents.md). Edit only `tools/playground/a34-pill-bar-lab.html`. Read `.coord/tasks/A34-pill-bar-per-mode.md` and the existing lab (it has a shared interactive model; keep and reuse it).
User (2026-11-20): "I want to add a Last modified timestamp and a delete button too. Definitely leaning towards S1-P1/S3-P3 + T1. But I'm not sure how I want to visually represent the potential items: Delete button, Last modified timestamp, Skip to first/Skip to last note, New note. I'm thinking New Note on bottom right, Delete on bottom left."

## Changes
1. Prune: the main sections show only S1 and S3 (Standard) and P1 and P3 (Permanent) with swap button T1 (icon only). Move the dropped options (S2, S4, P2, P4, T2-T4) into one collapsed `<details>` "Dropped in round 4" at the end, unchanged.
2. New section "Extras layout": 5 interactive layouts, each a full mock window (shared model) in Standard and Permanent (swap button switches both), light/dark toggle once at the page top. Every layout places all four extras: Delete, Last modified, Skip first/last (Standard only: the pill gets `|‹` and `›|`), New note. Layouts:
   - E1 (user's idea): Delete far bottom-left, New note far bottom-right, pill centered, timestamp as small muted text centered under/over the pill or inside the left of the bottom row next to Delete.
   - E2: as E1 but timestamp in the top-left meta area replacing today's "Saved ..." text (say it is separate from Saved).
   - E3: Delete and New note live inside one pill: [delete | ‹ count › | new], skip as double chevrons.
   - E4: two pills: left corner pill (delete + timestamp), center pager pill, right corner pill (new note).
   - E5: skip first/last on long-press/Shift+click of the arrows (no extra buttons), the rest as E1.
   Each layout card: what it adds to width at the 320 px minimum window (show a narrow 320 px mock too), pros/cons, and how it leaves room for edge reveal (bars slide in at the window edge) and a notice pill (toast sits above the bar; show it colliding or not), and split view's two bars (show the layout twice at half width).
3. Behavior must match the real app (verified in `PadWindow.Navigation.cs`):
   - Delete: on the stack, the note goes to the trash, notice "Moved to trash" with an Undo action that restores it at the same index; on a slot, its text is cleared with notice "Slot n cleared" and Undo; an empty slot does nothing. Deleting the last stack note leaves one blank note. After delete, current becomes the previous note (index-1, min 0). Make Undo work in the mock.
   - New note: always opens the stack (viewSlot = 0), landing on the trailing blank note if the last note is empty, otherwise appending one. Flag in the card text: in Permanent view New note leaves the slot view; list this as an open question with two choices (leaves to stack; or hidden in Permanent).
   - Skip first/last: jump to note 1 / note N with no notice; disabled look at the ends.
   - Last modified: show "Modified just now / 5m ago / 14:32 / Nov 18, 14:32" (same formatting thresholds as the app's Saved label: under 10 s just now, under 1 min Ns ago, under 1 h Nm ago, same day HH:mm, else MMM d). Edit the mock note text (a small editable text area in the mock window) updates the timestamp. In Permanent view it shows the slot note's time.
4. Recommendation box updated: pick one layout with reasons, and list the open questions for the user (delete confirmation, whether New note shows in Permanent, whether the timestamp replaces Saved).
5. Keep: no page-action icons, keyboard-accessible buttons, no external requests, no top-level identifier named `top`, `node --check` clean.
Final message: 6 lines max.
