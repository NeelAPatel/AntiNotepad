# Spike B: user hands-on feedback (2026-10-17, manager-recorded)

Tested by the user (Logitech MX Master 3S) via `measure-spike.ps1`. Only partial run; the user is waiting for both spikes to finish before further tests.

- **Shell error:** header showed `SHELL ERROR: tray/hotkey unavailable (see runtime log)`. Tray and Ctrl+Alt+Space did not start. Director must diagnose from `docs/spikes/B/runtime.tsv` (likely hotkey/tray thread or binding failure). Hide/reopen untested for that reason.
- **Animation:** visually much smoother than A, with some stutter.
- **GPU:** Task Manager showed ~4-5% GPU while swiping; the user has no baseline. Needs an idle vs. animating comparison (and ideally the same for A) before judging.
- **Memory seen in Task Manager:** about 20 MB working set for `antiquickpad-spike-b.exe` (screenshot, one sample, not the scripted metric).
- **Text rendering:** better and nicer than A overall, though the user prefers some elements of A's text rendering (specifics to collect).
- **Features:** expected features work (markdown styling, fenced python color, notes, wrapped checkbox lines, Tab/Shift+Tab fixture). User verdict for both spikes: nothing is "acceptable-great" yet beyond colorization/decoration working; optimization needed.
- **Transparency/Mica/Blur:** effectively broken or invisible in both spikes. De-prioritized for now.

Product goal noted by the user: fit and finish comparable to a typical native Mac AppKit/Swift app.
