# Test log: V1 shell + design tokens (2026-10-21)

Build: `feat/ui-overhaul` at cabc71a (M8c-v1-compact-shell + M6c-design-tokens + F54). Pad is running.
Mark each line **P** (pass), **F** (fail) or **?** (unsure) and add a note if useful. Anything F goes into TRACKER.md.

## 1. Show, hide, focus
| # | Do | Expect | Result | Note |
| --- | --- | --- | --- | --- |
| 1.1 | Press **Ctrl+Alt+Shift+A** | Pad appears instantly, centred, about 480 x 560 (first run after the layout change) | | |
| 1.2 | Start typing right away | Text goes into the note, no click needed | | |
| 1.3 | Press **Esc** | Pad hides; the app you were in gets focus back | | |
| 1.4 | Hotkey again | Same note, caret where you left it | | |

## 2. Glass and colours
| # | Do | Expect | Result | Note |
| --- | --- | --- | --- | --- |
| 2.1 | Put a bright, colourful window or wallpaper behind the pad | Blurred colour shows faintly through (90 % tint) | | |
| 2.2 | Then something dark behind it | Still readable; secondary text ("3 of 8", "All changes saved") easy to read | | |
| 2.3 | More menu > Light, then Dark | Both themes look right; glass in both | | |
| 2.4 | Look at the bottom dots | Idle dots solid (not faded); current dot longer and green | | |
| 2.5 | Code block and the line with the caret | Tinted, but the glass still reads through them | | |
| 2.6 | Text sharpness on the glass | Acceptable | | |

## 3. Hover bars
| # | Do | Expect | Result | Note |
| --- | --- | --- | --- | --- |
| 3.1 | Move the pointer away from the pad | Top icons and bottom pill fade out shortly after | | |
| 3.2 | Move the pointer onto the pad | Both fade in quickly | | |
| 3.3 | Open the More menu, move the pointer off the pad | Bars and menu stay until the menu closes | | |
| 3.4 | Press **Tab** a few times (pointer off the pad) | Bars appear; a green ring marks the focused button | | |
| 3.5 | Click Pin with the mouse | No green ring after the click (ring is keyboard-only) | | |

## 4. Window
| # | Do | Expect | Result | Note |
| --- | --- | --- | --- | --- |
| 4.1 | Drag by the top strip (bars shown or hidden) | Window moves; snaps to screen edges like normal Windows | | |
| 4.2 | Resize from an edge and a corner | Resizes smoothly down to about 360 x 420; glass keeps up | | |
| 4.3 | Pin, then click another app | Pad stays on top; unpin returns to normal | | |
| 4.4 | Click another app without pinning | Note what the glass does when the pad is inactive | | |

## 5. Notes and editor
| # | Do | Expect | Result | Note |
| --- | --- | --- | --- | --- |
| 5.1 | **Ctrl+N** (or the + icon) | New empty note; hint line sits exactly where the first text will go | | |
| 5.2 | Horizontal wheel one notch each way | Slides one note per notch | | |
| 5.3 | Click a dot | Jumps to that note | | |
| 5.4 | Previous / Next arrows in the pill | Move one note | | |
| 5.5 | Long note: scroll to the end | Last line fully visible above the pill | | |
| 5.6 | Markdown: headings, `- [ ]` checkbox click, list Enter, Tab / Shift+Tab, fold chevron | All work as before | | |
| 5.7 | More > Delete note | Note removed (no undo yet; known) | | |

## 6. Persistence
| # | Do | Expect | Result | Note |
| --- | --- | --- | --- | --- |
| 6.1 | Type something, wait 2 s, tray > Quit, relaunch | Text, note position, theme and window size restored | | |

## Anything else
Free notes:

## Results (user, 2026-10-22)
Everything passed except:
- 3.1-3.3 hover bars: work; make the reveal a user setting later (F56).
- 4.3 Pin: works, but the icon did not show the pinned state (F57, fixed by the manager, awaiting re-check).
- 3.4 Tab ring: could not trigger, because Tab is the editor's indent key (F58).
- Next: replace the More menu with a settings window owned by the pad (M11).
