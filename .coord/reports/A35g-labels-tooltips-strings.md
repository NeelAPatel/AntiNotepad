# A35g-labels-tooltips: string table (the lab for Approve)

Every new or changed piece of text this step adds. `{keys}` is the action's current shortcut label; with no key the
text shows without the parentheses, as today. Approving the card approves this table.

| ID | Where | Now | After |
|---|---|---|---|
| ~~A-4~~ (out at Approve) | Pager count, tooltip | (none) | `Search notes ({keys})` (the Search action's key, Ctrl+F by default) |
| ~~A-4~~ (out at Approve) | Pager count, screen-reader name | `Note 2 of 5` | `Note 2 of 5 · search notes` |
| A-6 | Swap button, tooltip and name, split on | `Show slots` / `Show stack` | `Show slots in the left pane` / `Show stack in the left pane` |
| A-6 | Swap button, split off | `Show slots` / `Show stack` | unchanged |
| A-7 | Settings action default key | none | `Ctrl+,` (the Windows and VS Code convention); pad tooltips then show it |
| A-7 | Welcome note, Help list | `To clear a demo, open its slot and press `Ctrl+Shift+Backspace`; the note moves to Trash.` (that key does nothing by default) | `To clear a demo, open its slot and click **Clear slot** at the bottom; the note moves to Trash.` |
| A-8 | Dev badge, tooltip and screen-reader name | (none) | the full badge text, the same text hover shows today |
| B-6 | Every Settings dropdown, screen-reader name | the first value only, never updated | `<row title>, <current choice>`, for example `Answer position, Right edge`; updates on pick |
| B-8 | Feel, switching row description | `Follows input, with projection-based switching and spring edge pulls` | `Swipe sideways to change notes. The page follows your swipe and springs back at the first and last note.` |
| B-9 | Toast after Reset all | (none) | `Shortcuts reset to defaults` with action `Undo` |
| B-9 | Toast after one Reset | (none) | `<action name> reset to its default` with action `Undo` |
| B-10 | Feel rows | `Vertical Scroll Style`, `Horizontal Scroll Style` | `Vertical scroll style`, `Horizontal scroll style` |
| C-4 | Save-failure card, body | the raw error, or `The notes could not be written to disk.` | `Your latest edits aren't on disk yet. Retry tries again. Quit anyway closes AntiNotepad, and those edits may be lost.` |
| C-4 | Save-failure card, second line (muted) | (none) | `Details: <raw error>` (only when there is one) |
| C-5 | Menu action row tiles, tooltips | (none) | `Cut (Ctrl+X)`, `Copy (Ctrl+C)`, `Paste (Ctrl+V)`, `Delete (Del)`: each tile's own key text |
| C-7 | Help, `list` row | `Every line is a checkbox` | follows Settings: `Every line gets a bullet` / `Every line gets a checkbox` / `Every line is numbered` |

Checked against tests and docs: no selftest asserts the old strings. `docs/decisions.md` (A20-overscroll-speed) names
the Feel rows in title case; the Codex run updates those two names to sentence case.
