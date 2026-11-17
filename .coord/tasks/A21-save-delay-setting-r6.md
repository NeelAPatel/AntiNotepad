# A21-save-delay-setting-r6: label clock runs whenever the pad is visible

Rework round of `A21-save-delay-setting`. Read the card (its Hand test section, last entry), `.coord/tasks/A21-save-delay-setting-r3.md` (spec) and `.coord/reports/A21-save-delay-setting-r5.md`. Do not edit any card or brief.

Tier: High (same)
Role: quick, a small change in known places (one gate condition plus tests).

## Why
User hand test: the "Saved ..." label shows "just now" and never counts up. Cause (read from code): `ShouldRunSavedLabelClock` in `UI/PadWindow.Layout.cs` requires the top bar to be "shown" (`_topShown`), which only the sliding bar styles set; in fade mode the clock never starts. User, verbatim: "the visibility of the bars doesn't matter. the text should update regardless."

## Fix
- The once-a-second label clock runs whenever the pad window is visible and not minimized, independent of bar style or bar visibility. No clock while hidden or minimized (show/hide speed); refresh once when the pad is shown again.
- Remove the bar-visibility term from the clock rule and from `UpdateSavedLabelTimer`'s callers that only existed for it (`SlideBar`); keep the timer restarted on save, note switch, show, restore from minimize.
- Tests: the clock rule with bars hidden or in fade mode is true when the window is visible; false when hidden or minimized; a visible pad with the label's note saved 3 s ago reaches "Saved 3s ago" without any bar event. Update the existing selftest and CONTRACTS row that asserted the bar term.
- Also confirm in the report, with a test or code reading, that a save completion never calls `UpdateChrome`, `WakeChrome` or `SlideBar` (user reported bars flashing on saves earlier; say what you found).

## Files
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Done means
Offline only; finish inside 25 minutes. Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A21-save-delay-setting-r6 -Offline`; quote its FINAL line. Draft the report after the first green selftest. Remove your own `build/A21-save-delay-setting-r6-*` folders; if a test process hangs, report PID and path and do not leave it running.

Report: `.coord/reports/A21-save-delay-setting-r6.md`, card report format plus "Hand checks for the user" (type once, stop: label counts up in fade and slide styles).
