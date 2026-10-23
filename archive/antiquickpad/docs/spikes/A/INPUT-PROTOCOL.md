> HISTORICAL: Original probe/tuning notes. Current wheel requirement is spec 4b page switching; touchpad is optional/untested. Use REPORT.md and RETEST.md for the current build.

# Spike A — Gate 1 acceptance protocol

Run on the Windows precision touchpad that will be used for AntiQuickPad. A remote desktop, touchscreen, ordinary wheel, or injected scroll cannot substitute for this test.

1. Start the probe using its README. Keep the pointer above the editable note and click the editor. Confirm ordinary typing works.
2. Mark MOVE with F1, then pan horizontally with two fingers slowly in each direction. Observe deltas.
3. Stop moving but keep both fingers touching for at least two seconds. Use F2 to mark PAUSE without lifting either finger (another hand can press the key).
4. Resume movement, then press F3 immediately before lifting both fingers. Observe whether a release signal arrives promptly; key timing is a human annotation, never the release signal itself.
5. Repeat with a fast horizontal flick. Mark LIFT at release and F4 while inertia continues. Does release precede the final inertial delta?
6. Repeat without a flick; release from rest. A release signal must also exist with no inertia.
7. Repeat after reversing direction mid-pan. Try vertical two-finger scrolling and text selection to expose arbitration problems.
8. During a gesture, switch away using Alt+Tab (F5 marks CANCEL if practicable). Return and mark END with F6.
9. Close the probe so its log is flushed. Retain the .log file and note the device model, touchpad driver, Windows settings, display scaling, and what you observed.

Acceptance: identify an actual contact-end/cancel event distinct from paused contact and post-release OS inertia while the editor owns focus. An idle timer, last wheel sample, or human marker alone fails this requirement. If Avalonia/native messages do not provide this, one bounded Windows native adapter is the next experiment. No adapter has passed until the same physical protocol is repeated.

Background: [Microsoft Direct Manipulation input processing](https://learn.microsoft.com/en-us/windows/win32/directmanipulation/directmanipulation-processing-input-with-directmanipulation) explains that consumed manipulations can withhold later pointer messages; observing a pointer hook is not equivalent to owning a Direct Manipulation viewport. [Microsoft overview](https://learn.microsoft.com/en-us/windows/win32/directmanipulation/direct-manipulation-portal) describes movement continuing into inertia after contact lift. These sources guide the next adapter; they do not prove this machine's behavior.
