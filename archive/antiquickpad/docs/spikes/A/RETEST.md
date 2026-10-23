# Short retest — feedback fixes

Exit the older spike first, then run `dotnet .\spikes\A\bin\verify\retest\InputProbe.dll` from the repository root. Current shortcut is Ctrl+Alt+Shift+A; the footer/tray tooltip reports whether registration succeeded.

1. In a note, indent one line with Tab; press Shift+Tab with only the caret on that line. Select two indented lines, Shift+Tab, then Ctrl+Z once. Expect outdent in both cases and one undo restores the multi-line indentation.
2. Have at least three nonblank notes. One horizontal wheel notch should immediately slide by one page. A quick burst should still move only one page, with no restart/hitch or runaway skipping. After stopping briefly, the next notch should move one more page. At newest, a burst creates at most one trailing note. Drag the blue strip once to confirm it still follows the pointer directly.
3. Hover `[ ]` or `[x]`: expect a hand cursor and toggle affordance; move away: normal text cursor. Click once and undo once.
4. Read the active hotkey in the footer. Click Hide, then click the AntiQuickPad tray icon to reopen. Hide again and use the displayed hotkey (normally Ctrl+Alt+Shift+A). Expect the same note and a focused editor. Report any failure plus newest spike log path.

Return four short PASS/FAIL notes; smoothness, physical shortcuts and tray discovery need your observation. The Director separately exercises isolated autosave/relaunch and method-level timing; those results do not replace physical keyboard-to-paint latency.
