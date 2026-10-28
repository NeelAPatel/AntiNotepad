# A43b-notes-session-r2

## Root cause

`Store.Normalize` supplies the normal loaded-store selection invariant, but the unavailable exits in `Store.Load` return before normalization. A missing notes folder therefore leaves the initial `StoreData` with an empty `Stack` and `Current == -1`. The constructor-fallback route has the same state: `MarkUnavailable` set availability and the error but did not repair the in-memory selection. `PadWindow.LeftNote` then indexed `Data.Stack[Data.Current]` during pane binding and crashed before `PadView.Bind` could show the location error.

## Changes

- `product/AntiNotepad/Model/Store.cs`: added `EnsureUsableSelection`, which keeps or creates a stack note, removes null entries, and clamps `Current`. It runs when `MarkUnavailable` is called and before each unavailable early return in `Load`. It does not run on a successfully loaded store path.
- `product/AntiNotepad/SelfTest.cs`: added the single `UNAVAILABLE STORE startup` capability line. The check covers a missing-folder `Load`, the Program constructor-fallback sequence (`createDirectories: false`, `MarkUnavailable`, `Load`), no-write `Save` behavior, and `SwitchFolder` followed by `Load` replacing the placeholder model with data from a valid folder. No UI/window is constructed.
- `.coord/STATUS.md`: recorded the r2 work and completion.

## Disk safety and folder recovery

The placeholder is only added to the in-memory `Data.Stack`. `QueueSave` exits as a successful no-op when unavailable; `Save` enters the unavailable check, drains queued work, and returns without capturing a new snapshot. On the missing-folder path, the test verifies that `Save` leaves the folder absent. `SwitchFolder` changes the Store paths without writing the model; a subsequent `Load` replaces `Data` with the target folder's loaded data. The selftest verifies that the loaded note is the target note and that the placeholder `StoreData` reference was replaced.

Source review found that Settings' existing recovery route is still gated for an unavailable source: `SettingsWindow.MoveNotes` checks `Store.LastError` after saving, and `NotesSession.MoveNotes` refuses when `Store.IsAvailable` is false. I left both untouched as required. Thus this change makes startup and Settings construction reachable, but does not change that pre-existing migration policy.

## Selection readers checked

- `product/AntiNotepad/UI/PadWindow.Navigation.cs`: `LeftNote` and the navigation, new-note, fresh-note, promote, and reorder paths read `Data.Current` or index `Data.Stack`. These need a nonempty stack and an in-range current selection.
- `product/AntiNotepad/UI/PadWindow.Layout.cs`: pager text and automation names use `Data.Current` and stack count.
- `product/AntiNotepad/UI/SearchOverlay.cs`: reads the current note with `ElementAtOrDefault`, so it is already bounds-safe; it changes `Current` only when a note is found.
- `product/AntiNotepad/UI/PadWindow.cs`: the timer predicate enumerates the stack and slots without indexing the current selection.
- Other `Data.Stack[0]` readers found by the product-wide search are in `SelfTest.cs` and run against stores loaded from available fixture folders.

## Gate

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A43b-r2-check`: succeeded, 0 warnings and 0 errors.
- `build/A43b-r2-check/AntiNotepad.exe --selftest`: exit 0, `SELFTEST PASS`; all 89 existing `LAYERS` lines remain, and the new line printed `UNAVAILABLE STORE startup missing-folder load, constructor fallback, no-write Save, and valid-folder reload`.
- Cleanup: the resolved output path was confirmed inside the workspace, but recursive `Remove-Item` was refused by tool policy. `build/A43b-r2-check` remains.
- No app GUI or process was launched.
