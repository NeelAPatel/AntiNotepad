# Test log: settings window + pin fix (2026-10-22)

Build: `feat/ui-overhaul` at bbb2c2b. Pad is running (Ctrl+Alt+Shift+A). Mark **P / F / ?** with a note.

| # | Do | Expect | Result | Note |
| --- | --- | --- | --- | --- |
| 1 | Click Pin, keep the pointer on it, then move away | Filled green pin as soon as it is pinned (even while hovered); outline pin when unpinned | | |
| 2 | Click ⋯ (now "Settings") | Glass settings window opens beside the pad (left if no room on the right), same look | | |
| 3 | Esc in settings | Settings closes, typing goes to the note | | |
| 4 | Open settings, press the hotkey | Pad and settings both hide; hotkey again shows only the pad | | |
| 5 | Open settings, Minimize (in settings) | Pad minimises, settings closes and does not come back on restore | | |
| 6 | Theme: System / Light / Dark | Pad and settings switch together | | |
| 7 | Background: Acrylic / Mica / Solid | Pad material changes at once; Solid is opaque | | |
| 8 | Tint slider | Glass gets stronger/weaker after you let go briefly; no flicker of the note | | |
| 9 | Bars: Always, then On hover | Always keeps top bar and pill visible; On hover returns to fading | | |
| 10 | Hotkey > Change, press e.g. Ctrl+Alt+Shift+Q | Label shows the new shortcut; it now toggles the pad; old one no longer does | | |
| 11 | Hotkey > Change, press a shortcut already used by another app | Message says it is not available; old shortcut keeps working | | |
| 12 | Set the hotkey back to Ctrl+Alt+Shift+A | Works again | | |
| 13 | Start with Windows on, then off | Toggle sticks (optional: check Task Manager > Startup apps shows AntiQuickPad while on) | | |
| 14 | Font size, Marker dimness, Strike thickness sliders | Note text updates after a short pause; settings stays open and focused | | |
| 15 | New note / Delete note in settings | Work on the pad | | |
| 16 | Quit in settings, relaunch | All choices above are remembered | | |
