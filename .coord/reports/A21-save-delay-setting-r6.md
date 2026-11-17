# A21-save-delay-setting-r6

## 1 Summary

The saved-label clock now runs whenever the pad is visible and not minimized, independent of bar style or bar visibility. The first offline selftest is green. No card or brief was changed.

## 2 Change list

- `product/AntiNotepad/UI/PadWindow.Layout.cs` | Removed top-bar visibility from the clock rule; sliding bar events no longer start or stop the timer.
- `product/AntiNotepad/SelfTest/Design.cs` | Updated saved-label regression for visible, hidden, minimized and three-second age behavior independent of a bar event.
- `product/AntiNotepad/SelfTest/Contracts.cs` | Updated the saved-label clock contract for window visibility behavior.

## 3 Behavior changes

While visible and not minimized, the saved label advances each second even in fade mode or when sliding bars are hidden. Hiding or minimizing stops the timer; showing or restoring refreshes the current age and restarts it. Save completion still refreshes only the saved text and timer.

Code reading of `OnNoteSaved` confirms it calls `SetSavedText` and `UpdateSavedLabelTimer`, then rearms save scheduling. It has no call to `UpdateChrome`, `WakeChrome`, or `SlideBar`, so save completion does not directly wake or show bars.

## 4 Out-of-scope files

No card, brief, or files outside the requested product files and report were changed by this round. Existing untracked r5 review reports were left untouched.

## 5 Plan deviations

None.

## 6 Contracts

Updated `saved-label-age-and-clock`. Regression checks verify the clock rule accepts visible state without bar visibility, rejects hidden/minimized state, and formats a save from three seconds ago as `Saved 3s ago` without a bar event.

## 7 Lens review

Not run in this worker round.

## 8 Checks run

- Direct Debug build to `build/A21-save-delay-setting-r6-selftest`: 0 warnings, 0 errors.
- Direct `--selftest`: 26 CONTRACTS rows, 25 pass, 1 manual; `SELFTEST PASS`.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A21-save-delay-setting-r6 -Offline` — FINAL line: `GATE: PASS`. Build, selftest, seam, tokens, format, scope and cleanup passed; LoopLint skipped. The gate noted existing size warnings in five large product C# files. Full output: `.coord/reports/A21-save-delay-setting-r6.gate.md`.

## 9 Hand checks for the user

- Type once, stop, and confirm the saved label counts up in fade style and in both sliding styles.




