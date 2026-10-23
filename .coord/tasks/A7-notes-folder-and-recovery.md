# A7-notes-folder-and-recovery

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/A7-notes-folder-and-recovery`. Never switch
branch, merge or commit (.git is read-only for you). App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10).
Read first: `AGENTS.md`, `docs/dev-loop.md`, `docs/audit/A1-codebase-audit.md` (findings A1-1, A5-1, A5-2, decision 1).
Match the surrounding code style, naming and comment density. First-party .NET/Avalonia only, no new packages.

## The user's decision (decision 1)
- Notes live in a user-visible **notes folder**, by default `Documents\AntiNotepad`. It holds `index.json`, `index.json.bak`, `notes\`
  and a new `recovered\` folder.
- Settings shows the folder and lets the user pick a new location; the current notes **move** there.
- A note that can't be recovered shows a hint (styled like the empty-note hint) telling the user to check the recovered folder,
  whose path is shown in Settings.
- Unknown note files are never deleted: they go to `recovered\`.

## Design (follow it; report anything that doesn't fit)
1. **App home vs notes folder.** App home = `%LOCALAPPDATA%\AntiNotepad` (from `DefaultDataFolderMigration`, which stays as the first step).
   It keeps the log (`Program.Log` must write to the app home, never the notes folder) and a pointer file `location.json`
   `{ "NotesFolder": "<absolute path>" }`. Settings can't hold the path because Settings live in `index.json` inside the notes folder.
2. **Resolution** (new `Platform/NotesLocation.cs` or similar, pure and testable: take app home and Documents paths as parameters,
   like `DefaultDataFolderMigration.Resolve`). Use `Environment.GetFolderPath(SpecialFolder.MyDocuments)`, which follows OneDrive
   redirection.
   - `ANTINOTEPAD_DATA` set (dev copy, tests): app home = notes folder = that path. No Documents, no first-run move. The pointer for
     a later in-app move lives in that folder. Dev and selftest must never touch the real app home or Documents.
   - Pointer exists: use it. If that folder is **missing or unreachable** (unplugged drive, offline sync), **do not create it, do not
     seed welcome notes, do not save**. Start in a clear error state: the pad shows a hint naming the missing path and Settings
     (offer "Choose folder"), keep the pointer, and log it.
   - No pointer, and the app home holds notes (`index.json` or `notes\`): **first-run move** to `Documents\AntiNotepad` (step 3).
     If that target already holds an `index.json`, don't merge or overwrite: keep using the app home, write no pointer, log it, and
     show a one-time toast.
   - No pointer and no notes (fresh install): use `Documents\AntiNotepad`, write the pointer.
3. **Moving notes (first run and Settings use the same code).**
   - Flush and save, then **copy** `index.json`, `index.json.bak`, `notes\*` and `recovered\*` to the target.
   - **Verify** the file count and byte length (or a hash) of each file. Then write the pointer, then switch.
   - Never delete or overwrite. Keep the old copy as a named backup: from the app home, move the data into
     `<app home>\backup-before-move-<yyyyMMdd-HHmmss>\`; from a user folder, rename it to `<name> (moved <yyyy-MM-dd HHmm>)`.
     If that rename fails, leave the old folder as it is and say so.
   - Refuse a target that already has an `index.json`, is the current folder, or is inside it (or contains it). On any failure, stay
     on the old location with nothing changed, and show why.
   - In Settings the move happens **live**, without a restart: make the Store's folder paths switchable after a full save. Bodies
     are loaded by id, so pointing `Store` at the new folder is enough once everything is saved and verified.
   - `--selftest` must never trigger the first-run move or touch the pointer: it runs on the installed copy during install.
4. **Settings row** (`UI/SettingsWindow.cs` ~:527, the existing "Notes folder" row). Show the path. Add buttons using the existing
   Settings helpers and style: **Open** (Explorer at the folder), **Change…** (Avalonia `StorageProvider.OpenFolderPickerAsync`,
   then a confirm line "Move your notes to <path>?" with Move and Cancel), and **Open recovered**, shown only when `recovered\` has
   files. Description: "Saved as you type, one file per note. Notes the app can't place go to the recovered folder."
5. **Recovery, never deletion (A1-1).**
   - Today `RemoveOrphans` deletes body files the index doesn't list, and that sweep is also how delete-forever, the 30-day trash
     expiry and dropped empty notes get removed. Make those removals delete their **own** body file directly, at the point they
     happen.
   - Then the sweep **moves** truly unknown `.md` files to `recovered\` (on a name clash, add a suffix; never overwrite).
   - When the load came from `index.json.bak`, the legacy file or defaults (recovery mode), do **no deletions at all**, only
     moves to `recovered\`.
6. **Invalid index falls back (A5-2).** Validate each candidate (`index.json`, then `.bak`) as a complete model, with non-null
   Settings, Stack, Slots and Trash, before accepting it. Don't let `Normalize` throw on a parsed-but-incomplete file. Keep the
   existing legacy v1 path.
7. **Missing or unreadable notes (A5-1).**
   - Distinguish three cases: valid empty text; **missing** (no body file but the index says the note isn't blank); and
     **unreadable** (an I/O error).
   - For missing and unreadable notes:
     - never cache `""` into the note's text;
     - never mark it Dirty or save it;
     - open the editor read-only;
     - `PeekText` and search must not treat it as empty.
   - Unreadable notes retry on the next access.
   - The editor shows the hint in the empty-note hint style (`Editor/PadView.cs` `_hint`): "This note couldn't be loaded. Its
     text may be in your recovered folder — the path is in Settings › <the section holding Notes folder>." Use the real section
     name.
8. **Uninstall** (`tools/uninstall.ps1`): `-RemoveData` removes only the app home. Print where the notes folder is, from
   `location.json`, and say it was kept. Never delete it.
9. **Docs:** update `docs/dev-loop.md` (data table, install notes, `-RemoveData`), and note the location rule in `docs/decisions.md`.

## Files in scope
`Model/Store.cs`, `Program.cs`, `Platform/DefaultDataFolderMigration.cs`, a new `Platform/NotesLocation.cs`, `Editor/PadView.cs`
(the hint, read-only state and bind only), `UI/SettingsWindow.cs`, `UI/SettingsParts.cs` (only if a helper is needed), `SelfTest.cs`,
`tools/uninstall.ps1`, `docs/dev-loop.md`, `docs/decisions.md`. Leave everything else alone. Other workers are editing PadView
(list and timer code), PadWindow and the OCR code in parallel, so keep PadView edits local to the hint and bind.

## Selftest (add a `NotesLocationChecks` method at the END of SelfTest.cs with one call line in Run)
Use temp folders only. Each item is a check:
- fresh install resolves to Documents and writes the pointer;
- a first-run move copies, verifies and leaves a backup;
- a target that already has notes is refused;
- a missing pointer target does not create anything;
- an unknown body file goes to `recovered\`, not deleted;
- delete-forever removes its own body;
- an index with `"Settings": null` falls back to `.bak`;
- a missing body for a non-blank note stays read-only and is not saved;
- `ANTINOTEPAD_DATA` bypasses everything.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a7-check`: 0 errors, 0 warnings.
  `build/a7-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Delete `build/a7-check` if you can.
- Never launch or kill AntiNotepad otherwise, and never read or write `%LOCALAPPDATA%\AntiNotepad` or the real Documents folder.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A7-notes-folder-and-recovery.md`:
  - files changed;
  - the resolution rules as built;
  - the move steps;
  - what the user sees in each error state;
  - the selftest list;
  - anything not done.
