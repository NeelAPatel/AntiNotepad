# A21-save-delay-setting-r4

## 1 Summary

Recovered the uncommitted r3 changes and fixed the failing save-delay contracts, the reflection call for `Edited`, and a concurrent Store acknowledgement race. The final offline gate passed. The main A21 item card was not changed.

## 2 Change list

- `product/AntiNotepad/Model/Note.cs:114,122` | Keep never-saved blank edits clean and make the blank-discard check explicit | r3 spec edge 1.
- `product/AntiNotepad/Model/Store.cs:40,146,497` | Load label times from durable bodies and serialize completion acknowledgements so `Save()` callers observe the matching revision | r3 spec edges 2, 5, 13.
- `product/AntiNotepad/UI/PadWindow.cs:421,446,469,516,527,577` | Track independent monotonic deadlines, idle and repeated safety writes, retry failed writes, save immediately at boundaries and on minimize, and update only the label after a durable save | r3 spec edges 2–13.
- `product/AntiNotepad/UI/PadWindow.Keys.cs:329` | Flush pending edits when the save delay changes | r3 spec edge 12.
- `product/AntiNotepad/UI/PadWindow.Navigation.cs:30,62,228,235,289` | Save note transitions and cancel deadlines for notes removed or trashed | r3 spec edges 8, 11.
- `product/AntiNotepad/UI/PadWindow.Layout.cs:209,229,238,293,306,365` | Schedule from current document state before model notifications catch up; format the saved label and run its aligned clock only when visible | r3 spec edges 1, 7.
- `product/AntiNotepad/Theme/ChromeTokens.cs:7,11,12` | Centralize save and saved-label timing values | r3 spec edges 5, 7.
- `product/AntiNotepad/SelfTest/Store.cs:499,570,580,593`, `SelfTest/UI.cs:277,341,543`, `SelfTest/Contracts.cs:77,83,89,125,156`, `SelfTest/Design.cs:22,80` | Cover storage, timing, boundaries, label ages, and the intended design snapshot; fix the reflection invocation to pass both parameters | r3 spec checks and contracts.

## 3 Behavior changes

The 400 ms default keeps its trailing-edge debounce. Longer delays save N seconds after the last edit, with a one-second idle gate and repeating safety writes while typing continues. Each note has its own deadline. A blank, never-saved note shows `-` and does not write an empty body. Switching, hiding, minimizing, clicking away, quitting, sign-out, and shutdown save pending edits immediately. A save completion updates only the saved-label text and does not wake the bars.

## 4 Out-of-scope files

No product files outside the r3 `## Files` list changed. I added that inherited `## Files` list to the separate r4 continuation brief because the gate requires one on each rework brief. The main item card `.coord/tasks/A21-save-delay-setting.md` remains unchanged.

## 5 Plan deviations

No product-scope deviations. The design dump still matches the intended Saving-row snapshot, hash `7c2c76482fc04e6e4e3c933b72e6d5e42774f6a702844eb41cbf66db181ce607`; its expected hash did not need another update.

## 6 Contracts

`save-delay-settings-persist`, `save-delay-state-machine`, `saved-label-age-and-clock`, and `save-delay-timer-and-boundaries` pass. The selftest reports 23 contract rows: 22 automated passes and one manual check.

## 7 Lens review

Not run in this worker continuation.

## 8 Checks run

- Debug build: 0 warnings, 0 errors.
- `AntiNotepad.exe --selftest`: exit 0, `SELFTEST PASS`; design dump hash matches the expected value.
- Large-note timing sample: 1,000,000 characters, enqueue `0.1 ms`, completion wait `64.0 ms`; the write completes through the background queue after the 50 ms coalescing window.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A21-save-delay-setting-r4 -Offline` — final line: `GATE: PASS`. Build, Selftest, Seam, Tokens, Format, Scope, and Cleanup pass. Offline mode disabled NuGet audit for this build.
- Gate cleanup removed its gate build/data folders. Removal of the separate direct-check folders was blocked by workspace policy; they remain under `build/A21-save-delay-setting-r4-check` and `build/A21-save-delay-setting-r4-check-data`.
- Size warning retained as requested: `SelfTest/Store.cs` 496 → 609 lines; `SelfTest/UI.cs` 389 → 712; `UI/PadWindow.cs` 529 → 731; `UI/PadWindow.Layout.cs` 696 → 750. No files were split.

## 9 Hand checks for the user

- A new blank note shows `-`.
- With the delay set to 5 seconds, type and confirm the save timing.
- Keep typing without a one-second pause and confirm a save occurs every N seconds.
- Switch notes, hide, and minimize; each saves immediately.
- Confirm the bars stay still when a save completes.
- Confirm the label reads `Saved just now`, then `Saved 1s ago`, `Saved 2s ago`, and continues counting.
