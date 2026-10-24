# A7-notes-folder-and-recovery

## Changed files

- `product/AntiNotepad/Platform/NotesLocation.cs` (new): app-home pointer resolution, verified copy, pointer update, and old-copy backup.
- `product/AntiNotepad/Platform/DefaultDataFolderMigration.cs`: left unchanged; it remains the first startup migration step.
- `product/AntiNotepad/Program.cs`: separates app home from notes folder, keeps logs in app home, handles unavailable locations, and switches the Store after a move.
- `product/AntiNotepad/Model/Store.cs`: validates index candidates, distinguishes unavailable bodies, recovers unknown files, and removes bodies for known note deletions.
- `product/AntiNotepad/Editor/PadView.cs`: binds unavailable notes read-only and displays the recovery hint using the existing hint style.
- `product/AntiNotepad/UI/SettingsWindow.cs`: shows both paths and adds Open, Change/Choose folder, Move/Cancel, and conditional Open recovered actions.
- `product/AntiNotepad/SelfTest.cs`: adds `NotesLocationChecks` and its single call from `Run`.
- `tools/uninstall.ps1`: reads the notes path from `location.json`, preserves it, and guards against deleting notes inside app home or the install folder.
- `docs/dev-loop.md`: documents app home, notes folder, migration, and uninstall behavior.
- `docs/decisions.md`: records decision 1.
- `.coord/reports/A7-notes-folder-and-recovery.md`: this report.

## Resolution rules built

- `DefaultDataFolderMigration.Resolve` still runs first. Its resolved folder is app home, which holds the product log and `location.json`.
- Normal startup gets Documents from `SpecialFolder.MyDocuments` and uses `Documents\AntiNotepad` by default.
- With `ANTINOTEPAD_DATA`, the supplied folder is both app home and notes folder. Startup skips the Documents lookup, pointer resolution, and first-run move.
- An existing pointer selects its absolute path. If the target is missing or cannot be enumerated/read, the Store is disabled; startup does not create it, seed welcome notes, or save.
- Without a pointer, app-home `index.json` or `notes\` triggers a first-run move. If the Documents target already has `index.json`, the app stays in app home, writes no pointer, logs the conflict, and displays one startup toast.
- With no app-home notes, startup creates the Documents folder and writes the pointer.

## Move steps

1. Settings flushes both panes and saves the Store before calling the move operation.
2. The move refuses a missing source, an existing target `index.json`, a target with a colliding copied path, the current folder, and either folder containing the other.
3. It copies `index.json`, `index.json.bak`, and all files under `notes\` and `recovered\` without overwriting. It compares copied file counts and lengths and verifies each file with SHA-256.
4. After verification it updates app-home `location.json`. The live Store then points to the target without restarting; body loads continue by note ID.
5. App-home data is moved into `backup-before-move-<timestamp>`. A user notes folder is renamed to `<name> (moved yyyy-MM-dd HHmm)`. A backup rename failure leaves the old folder and reports that the new folder is active.
6. A copy, verification, or pointer failure cleans up only the partial copies created by this attempt and leaves the old location active.

## Recovery and validation

- `RemoveOrphans` moves unknown `.md` files from `notes\` into `recovered\`; collisions get a numbered suffix. It no longer deletes unknown files.
- Normal saves detect known note IDs removed from the model and delete those bodies after the index write. Expired trash bodies are deleted directly only when the primary index loaded successfully. Recovery loads from `.bak`, legacy v1, or defaults do not delete bodies; unknown bodies are moved to `recovered\`.
- `index.json` and then `index.json.bak` are accepted only when Settings, Stack, Slots, and Trash are present and non-null. The legacy v1 path remains supported.
- A missing body for a nonblank note and an I/O read failure return an unavailable marker without caching empty text or marking the note dirty. The editor binds read-only and shows: “This note couldn't be loaded. Its text may be in your recovered folder — the path is in Settings › About.” Later body accesses retry the read.
- Settings shows the notes path and recovered path. Open recovered appears only when that folder contains files. The folder picker asks “Move your notes to <path>?” before Move and Cancel are shown.

## User-visible error states

- Existing first-run target index: app home stays active; a one-time startup toast explains the conflict.
- Missing or inaccessible pointer target: the pad hint names the path and points to Settings › About. The editor is read-only, saving and welcome seeding are disabled, and the settings action reads “Choose folder.” A move is refused while the source remains unavailable, leaving the pointer unchanged.
- Missing or unreadable note body: the note is read-only with the recovery hint; Settings shows the recovered path and offers Open recovered when files exist.
- Move refusal or copy/verification failure: a Settings toast gives the reason, and the old location remains active.
- Old-folder backup rename failure after a verified move: the new folder remains active, the old folder stays in place, and the toast explains the rename failure.

## Selftest checks

`NotesLocationChecks` uses temporary folders only and covers:

- Fresh resolution writes a pointer to Documents.
- First-run copy verifies contents and leaves an app-home backup.
- A target containing `index.json` is refused.
- A missing pointer target is not created.
- An unknown body is moved into `recovered\`.
- Delete-forever removes its own body.
- `Settings: null` in `index.json` falls back to `.bak`.
- A missing body for a nonblank note remains unavailable, ignores text/touch writes, and is not recreated by Save.
- `ANTINOTEPAD_DATA` bypasses Documents and first-run movement.

## Verification and remaining limits

- Release build: 0 warnings, 0 errors.
- `build/a7-check/AntiNotepad.exe --selftest`: `SELFTEST PASS`.
- NuGet's online vulnerability audit was disabled only in the build shell because nuget.org was unreachable; project and package settings were not changed.
- The task check output remains in `build/a7-check`; recursive removal was blocked by the tool policy after its in-worktree path was verified. No installed app, real app-home folder, or real Documents folder was opened or changed. The GUI, Explorer launch, folder picker, and uninstaller were not run.
- The unavailable-body selftest verifies the model's unavailable/unsaved behavior. The Avalonia read-only binding and visual hint were not exercised in a live window.
