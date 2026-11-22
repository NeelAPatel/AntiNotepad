# A35d-spacing-tokens lab, round 3 fix: pad and help

Role: terra, fixes to two files after a browser check by Claude.

Read `.coord/tasks/A35d-spacing-tokens.lab3-common.md` in full and follow it. Write in small saved steps.
Your files: `tools/playground/a35d-lab/pad.html` and `tools/playground/a35d-lab/help.js`. Edit only these.
Compare with `build/shots/a35d/pad-AntiNotepad-dev.png`, `pad-slot-*`, `help-top-*`, `help-down-*` and `split-*`.

Fix each finding from the browser check:
1. **Help is see-through.** F1 draws the panel over the note text. In the real app, help covers the editor fully:
   the backdrop shows behind it, and the note text does not (`help-top-*`). Match the panel position, header, section
   captions, key chips and the two text columns to the capture.
2. **Clicking the note turns it into raw markdown** (`#`, backticks, `**`). The real editor keeps its formatting while
   you type. Markers stay dimmed and are shown only on the caret's line, the way `Editor/Colorizers.cs` does it (see the
   dim `#` at the caret in `pad-AntiNotepad-dev.png`). Use one contenteditable view that is formatted as you type. Do
   not switch between a rendered view and a textarea.
3. **Wrapped bullet lines** must indent under the bullet text (hanging indent), as in the captures.
4. **Scrollbar and pill bar show all the time.** In the capture, neither is visible at rest. Copy the code's rules
   for showing them: the scrollbar appears while scrolling, and the bar shows on hover near the bottom per the bar mode.
5. **Backdrop and weight.** Use `LAB.backdrop` on the window, so it has the warm blurred Acrylic look of the captures,
   not flat dark. The note text looks heavier than the capture. Use the code's editor font, size and weight
   (Cascadia Mono 15, regular, line height 1.32) and the code's colors.
6. **Lab controls.** Keep the width, bar mode and save-failure controls above the window, outside the app area.

Report: overwrite `.coord/reports/A35d-spacing-tokens-lab3-pad.md`, adding a "Fix round" section that answers each
numbered item.
