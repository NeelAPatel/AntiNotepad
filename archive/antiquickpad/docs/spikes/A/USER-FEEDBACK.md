# Spike A: user hands-on feedback (2026-10-17, manager-recorded)

Tested by the user with a Logitech MX Master 3S on the Director's `bin/verify` build. Screenshots reviewed: Transparent, AcrylicBlur and Mica modes all render; text stays readable in each; `focus=True` in the first.

| Area | Result | Note |
|---|---|---|
| Editor focus, typing | PASS | footer showed focus=True |
| Decorated markdown (headings, bold/italic/strike markers visible) | PASS | |
| Python fence highlighting | PASS | keywords and strings colored |
| Clickable `[ ]` / `[x]` checkbox | PASS, rough | works; no hover/pointer feedback that it is clickable |
| Code folding | PASS, different than envisioned | works on the fenced block gutter |
| Line numbers | PASS | |
| Horizontal wheel switching notes | PASS, rough | "works as intended", needs cleanup of hitchiness |
| Transparent / Acrylic / Mica | PASS | all three apply |
| Hide / reopen | NOT TESTED | user saw no way to reopen; hotkey `Ctrl+Alt+Shift+A` or the tray icon is supposed to reopen (user may not have known) |
| Tab/Shift+Tab, autosave/relaunch, tray, latency numbers | NOT YET RUN | |

User verdict: "For a spike, yeah every basic thing works. Works surprisingly better than I thought."

Follow-ups for the real app, not blockers: checkbox hover/cursor affordance; smooth wheel animation (page-switch, one notch = one note); a visible reopen cue after Hide.

## Retest 2026-10-17 (build bin/verify/retest)
All four checks PASS: Shift+Tab (caret and multi-line, undo), wheel page switch with burst coalescing plus drag, checkbox hover/click/undo, Hide then reopen via tray and Ctrl+Alt+Shift+A. User: scrolling animation feels a bit weird, but fine for the spike. Follow-up for the real app: tune the page-switch animation (easing/duration), see demos/ui-variants for the visual direction.
