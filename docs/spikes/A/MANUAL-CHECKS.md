# spike — Interactive acceptance checklist

Current hardware gate: mouse horizontal notches and mouse drag. Touchpad optional/untested. Run the newly built default app, not `--probe`. Keep returned logs and note mouse model, driver and Windows scaling.

| # | User action | Pass evidence |
|---|---|---|
| 1 | Click note and type a unique phrase, move caret | Visible text/caret; logs record actual focus and text change |
| 2 | Rotate horizontal wheel one notch in each direction | Immediate animated page switch in expected direction; one notch or quick burst means one page |
| 3 | Rotate several notches with short and long pauses | No animation restart/hitch, stuck transform or uncontrolled skips; pause then next burst moves one page |
| 4 | Drag dedicated grab strip slowly, reverse, release short of threshold | 1:1 drag while held; release springs back; text selection remains available inside editor |
| 5 | Drag/release across threshold, then flick quickly | Distance/velocity choose sensible note; no lost capture; leaving app cancels safely |
| 6 | At newest note pull beyond end and release; repeat below threshold | Rubber-band resistance; exactly one new note only beyond threshold; blank interior notes pruned |
| 7 | Vertically scroll a long wrapped note; select text | Vertical scroll and selection do not move carousel |
| 8 | Toggle fixture checkbox, undo, redo; inspect caret | Source changes `[ ]`/`[x]`, markers remain, undo/redo reversible and caret stable |
| 9 | Select multiple lines and Tab / Shift+Tab; inspect fixture styles | Indent/outdent works; visible Markdown markers, heading levels, emphasis, Python fence distinction |
| 10 | Edit, wait for save, quit, relaunch | Same note content/order restored (current index/undo history do not persist); no lost edits |
| 11 | Try tray show/hide/exit and global hotkey from another app | Same resident window, visibly focused editor on show; registration failures clearly logged |
| 12 | Switch Transparent/Acrylic/Mica and line-number/folding controls | Record actual material fallback/text clarity; folding/line numbers yes/no |

Record PASS/FAIL and what happened for each, especially focus and carousel direction. Build/model smoke results cannot replace these checks. Real hotkey-to-visible/focused and hide latency require user-visible observation; a stopwatch around a method call measures only dispatch cost.
