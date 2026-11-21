# A35d-spacing-tokens lab 3: help

## 1. Files written
- `tools/playground/a35d-lab/help.js` and this report.

## 2. Controls that work
- Scrollable, keyboard-focusable help list; shared panel-header close button has hover and press states and calls `ctx.close()` (also accepts `ctx.closeOverlay()`). `unmount()` removes the overlay. Default assigned shortcut and fixed-key rows and all line 1 modes are included. Shortcut overrides are not modeled.

## 3. Snap rows wired
- 12: scroll viewport side/top margins, 10/14 px before, 12/16 px after; bottom remains 40 px.
- 13: panel right margin, 14 px before, 16 px after.

## 4. Captures and remaining differences
- `help-top-AntiNotepad-dev.png`: transcribed geometry, text and colors against the capture and source. Browser rendering, font rasterization, scrollbar, acrylic and exact pixels remain unverified under the offline/no-browser rule.
- `help-down-AntiNotepad-dev.png`: identical SHA-256 to the top capture, so it does not show the scrolled state. Lower content was transcribed from source and its appearance remains unverified.

## 5. Helpers
- Local row and key-chip construction; shared core has no help row builder. The host must provide a positioned containing block and a close callback in `ctx.close` or `ctx.closeOverlay` for its overlay state to close.

## 6. Checks
- `node --check tools/playground/a35d-lab/help.js` passed. No DOM IDs used. Capture files compared by SHA-256. No browser, server, app launch or commit.
