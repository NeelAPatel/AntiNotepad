# A43b-notes-session-r3: in-app recovery from a missing notes folder (A53-unavailable-folder-recovery, folded into A43b-notes-session by the user)

Role: worker (gpt-6-luna xhigh). **Implementation, small.** Product `product/AntiNotepad`. Branch `reorg/A43b-notes-session` (already
checked out; do not switch branches, do not commit: you cannot write to `.git`). Read `AGENTS.md`, `.coord/reports/A43b-notes-session.md` and
`.coord/reports/A43b-notes-session-r2.md` first. Line numbers are from the current tree; re-read before editing.

## The problem (seen live by the manager)
With `location.json` pointing at a missing folder the app now starts (r2) and shows "Error: Notes folder not found ... Choose a folder in
Settings > Storage", and Settings > Storage offers "Choose folder". But picking a folder and pressing **Migrate notes** fails with
"Couldn't save notes · Notes folder is unavailable: ...": `SettingsWindow.MoveNotes` (`UI/SettingsWindow.cs` ~679) first calls
`_host.Save()` and checks `Store.LastError`, and `NotesSession.MoveNotes` refuses while the store is unavailable. There is nothing to
migrate from an unavailable source, so recovery needs a different action: **point the app at a folder** (an existing notes folder, or an
empty one to start fresh). Today's only way out is hand-editing `location.json`.

## Design (keep it small and safe; do NOT live-reload the store)
Replacing `Store.Data` while the window, panes, timers and tray hold it is the risky part, so recovery takes effect **on the next start**:
1. **`NotesSession.UseFolderOnNextStart(string folder, out string message)`** (new; `NotesSession.cs`). Allowed only when
   `NotesLocationError is not null` (the unavailable/invalid-pointer state). Otherwise returns false with a message and changes nothing.
   Validates: full path, `Directory.Exists`, enumerable (like `NotesLocation.FromPointer`), not equal to / nested in a folder in a way that
   would be wrong (keep it simple: the folder must exist and be readable; it may contain an existing `index.json` to adopt, or be empty).
   Never creates the folder. Then writes the pointer through `NotesLocation` (make `WritePointer` `internal` if it is private; reuse its
   exact format; no change to its behavior) at `AppHome`. Returns true with message
   "Notes folder set to <folder>. Quit and start AntiNotepad again to load it." It does **not** touch `Store`, `DataFolder`,
   `NotesLocationError`, and does not save. `MoveNotes` and every available-store path stay byte-for-byte unchanged.
2. **`SettingsWindow`**: when `_notes.NotesLocationError is not null` and a folder was chosen (`_pendingNotesFolder`), the primary button
   reads **"Use this folder"** (instead of "Migrate notes") and calls a new `UseFolder(destination)` that skips `_host.Save()` and the
   migrate preflight, calls `_notes.UseFolderOnNextStart`, shows the message as the existing green success line on success (error line
   `_notesError` on failure), clears `_pendingNotesFolder`, and `Refresh()`. While that restart-pending success line is shown, do not also
   show the stale red session error line (keep a small private flag; the error line returns on the next Settings open since the session is
   unchanged). When `NotesLocationError is null` everything (button text, `MoveNotes`, preflight, backup text) stays exactly as today.
   Keep the "Choose folder" button as is.
3. **`SelfTest.cs`**: add one capability line `NOTES SESSION use-folder` (print like `NOTES SESSION move`), temp root, no UI: (a) an unavailable
   session (store `createDirectories: false` on a missing folder, `NotesLocationError` set) + `UseFolderOnNextStart(existingFolderWithIndex)`
   returns true, `NotesLocation.ReadPointer(appHome)` equals the folder, session `DataFolder`/`NotesLocationError`/`Store` references are
   unchanged, nothing created; (b) a fresh start from that pointer resolves with `NotesLocation.Resolve` to the folder with no error;
   (c) an **available** session returns false and leaves the pointer untouched; (d) a missing folder or a file path returns false, creates
   nothing, pointer untouched. Existing LAYERS rows (89) and all other lines unchanged; add LAYERS rows only if one of the existing
   rules is naturally extended (not required).
4. Do not touch `Store`, `NotesLocation.Move/Resolve` behavior, `ISettingsHost`, `PadWindow`, the recovered-folder Open row (A51), or the
   old-folder rename warning (A52).

## Rules
- Behavior-neutral for every available-store path. US spelling. Never launch or kill AntiNotepad (never `--quit`). Do not commit.
- Build: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A43b-r3-check` (0 warnings, 0 errors), then
  `build/A43b-r3-check/AntiNotepad.exe --selftest` must print `SELFTEST PASS` with all existing lines plus `NOTES SESSION use-folder`.
  Delete `build/A43b-r3-check` afterwards (if the tool refuses recursive removal, say so in the report).

## Done means
- Build 0 warnings 0 errors; `SELFTEST PASS`.
- Report `.coord/reports/A43b-notes-session-r3.md`: files changed, exact wording of the new button/message, confirmation that the
  available-store `MoveNotes` path and `SettingsWindow` preflight are untouched, selftest lines.
