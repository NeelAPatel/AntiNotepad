# A21-save-delay-setting

## 1 Summary

Implementation is in the item worktree. Final verification is pending: the first offline gate build succeeded, but its selftest is blocked in the earlier test binary. The source was corrected and the item build passes.

## 2 Change list

- `Model/Settings.cs:22-28` | Add the 400 ms default and normalize unsupported values | Plan: persisted setting, invalid value -> 400.
- `Model/SettingsService.cs:94` | Classify delay changes as persistence-only | Plan: persistence-only flags.
- `Model/Note.cs:37` | Add nonserialized `SavedAt` | Plan: in-memory save time.
- `Model/Store.cs:50,439-452,489-497,514` | Timestamp successful body writes; apply only matching revisions and notify the UI | Plan: durable completion and actual save time.
- `UI/SettingsWindow.cs:536-544` | Add the Saving dropdown below Window | Plan: layout A control and hint.
- `UI/PadWindow.cs:30,72,393-409` | Use the setting for timer interval; refresh the active label and log delay/elapsed ms | Plan: adjustable timer and save completion label.
- `UI/PadWindow.Navigation.cs:22-53` | Queue a snapshot before pane or split transitions | Plan: immediate transition save via `QueueSave`.
- `UI/PadWindow.Layout.cs:275` | Display `SavedAt ?? Modified` | Plan: label follows real save time.
- `Theme/ChromeTokens.cs:7-10` | Define save delay interval options | Plan: timer values in Theme/Motion tokens.
- `SelfTest/Store.cs:499-559`, `SelfTest/UI.cs:277-405`, `SelfTest/Contracts.cs:77-87` | Add storage, timer, transition, timestamp and contract checks | Plan: selftests and CONTRACTS rows.

## 3 Behavior changes

Settings now offers 0.4, 5, 10 and 30 seconds. The timer restarts with the selected value. Pane and split changes queue the current snapshot before detaching the old editor. Successful matching body writes set in-memory `SavedAt`, refresh the active label and log `save delay=...ms actual=...ms`.

## 4 Out-of-scope files

No product files outside the card were changed. `PadWindow.Keys.cs` was not needed: its existing persistence-only handler already restarts the save timer. The task card and `.coord/STATUS.md` are untouched.

## 5 Plan deviations

No product behavior deviation. The UI selftest exercises `PadWindow.Save()`, the synchronous save called by Quit; the user hand check still covers a real quit and window hide.

## 6 Contracts

- Pending edits queue a save before note, slot, stack, search, split, hide or quit save boundaries.
- `SavedAt` changes only after a durable write with a matching revision clears `Dirty`; `Modified` is unchanged and `SavedAt` is not serialized.

## 7 Lens review

Static Codex review found no proven source issue. Transition detection runs before `SwitchTo`; stale acknowledgements fail the revision check; the save event is raised only after `Dirty` clears. Final review remains subject to the selftest and gate completing.

## 8 Checks run

- `dotnet build product/AntiNotepad/AntiNotepad.csproj -o build/A21-save-delay-setting-check --no-restore` with `AVALONIA_TELEMETRY_OPTOUT=1`: pass, 0 warnings, 0 errors.
- `git diff --check`: pass.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A21-save-delay-setting -Offline`: build pass, 0 warnings; selftest has not returned, so `SELFTEST PASS` and gate result are pending. The running test used an earlier build that blocked after its first save log while synchronously waiting on a queued task; the source check now uses `Store.Save()`.

## 9 Hand checks for the user

In the dev copy, select each delay, type twice to confirm the wait restarts, and confirm the saved label changes only after the write. Check the choice after restart, then hide, switch notes, open Settings and quit with a pending edit. Keep sign-out and Windows shutdown as the open A30 live check.
