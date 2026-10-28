# A43b-notes-session-r3

Implementation on `reorg/A43b-notes-session`; no commit made.

## Changes

- `product/AntiNotepad/NotesSession.cs`: added `UseFolderOnNextStart`. It only runs while `NotesLocationError` is set; it requires an absolute path to an existing, enumerable directory and refuses the unavailable folder or an overlapping path. It updates `location.json` through `NotesLocation.WritePointer` without changing the session, `Store`, or selected folder and never creates the chosen folder.
- `product/AntiNotepad/Platform/NotesLocation.cs`: made `WritePointer` internal so the session can reuse the existing pointer format and write behavior.
- `product/AntiNotepad/UI/SettingsWindow.cs`: when the notes folder is unavailable and a destination is pending, the primary button reads **Use this folder**. It skips the save and migration preflight and reports success in the green line; a small per-window flag hides the stale session error until Settings is reopened. The pending selection is cleared on success or failure.
- `product/AntiNotepad/SelfTest.cs`: added the `NOTES SESSION use-folder` capability check for successful adoption and fresh resolution, unchanged references/state, available-session refusal, and missing/file target refusal without creation or pointer changes.

The success message is: “Notes folder set to <folder>. Quit and start AntiNotepad again to load it.”

The available-store `NotesSession.MoveNotes` method and `SettingsWindow.MoveNotes` save preflight are unchanged. The normal available-store button and migration path retain their behavior. `Store`, `PadWindow`, recovered-folder behavior, and old-folder rename behavior were not changed.

## Gate

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A43b-r3-check`: succeeded with 0 warnings and 0 errors.
- `build/A43b-r3-check/AntiNotepad.exe --selftest`: exit 0, `SELFTEST PASS`; `NOTES SESSION use-folder passed`; all 89 existing LAYERS rows remain unchanged.
- The requested recursive cleanup of `build/A43b-r3-check` was rejected by tool policy; the directory remains. No app GUI or process was launched.
