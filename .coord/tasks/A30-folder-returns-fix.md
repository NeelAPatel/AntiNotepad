# A30-folder-returns-fix

You are the implementation worker (gpt-6-luna xhigh). Do the work yourself.

Repo: this folder, the git worktree on branch `feat/A30-background-save-and-log`, holding the uncommitted A30 change plus its
A30-save-unavailable-fix rework (see `.coord/reports/A30-background-save-and-log.md`). Never switch branch, merge or commit.

## Review result
The rework is accepted except one point. `Store.IsStoreUnavailable()` used to set `IsAvailable = false` whenever the notes
folder was missing at save time. That made a brief absence permanent: if the folder comes back (OneDrive Documents not ready
yet at sign-in, an external or network drive reconnecting), every later save stays a silent no-op and quit reports success, so
edits are lost. Before A30, each save simply re-checked the folder. The manager has already removed that assignment in
`Model/Store.cs` (`IsStoreUnavailable`, with a one-line comment); keep that change.

## To do
1. `SelfTest.cs` `BackgroundSaveAndLogChecks` now fails on "missing folder becomes unavailable and QueueSave, Save, and Flush
   write nothing", because it asserts `IsAvailable` turns false. Change that case to the intended behavior: while the folder is
   missing, `QueueSave`/`Save`/`Flush` succeed with `LastError` set, write nothing and keep Dirty, and `IsAvailable` is not
   changed by a save.
2. Add to the same method: recreate the folder, then `Save()` writes the still-dirty text there and clears Dirty.
3. Check nothing else in `Model/Store.cs` relied on the removed assignment (e.g. code that reads `IsAvailable` after a save);
   if something did, fix it in the smallest way and say so in the report.
4. Remove the two no-op `_ = IsStoreUnavailable();` statements in `Save()` and `Flush()` if they do nothing useful (if they
   restore `LastError` after a drain, keep the effect but make it explicit and readable).

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a30-check`: 0 errors, 0 warnings;
  `--selftest` prints SELFTEST PASS. Delete `build/a30-check` if you can.
- Only `Model/Store.cs` and `SelfTest.cs` change.
- Append a "Rework 2" section to `.coord/reports/A30-background-save-and-log.md`.
- Never launch or kill AntiNotepad; never touch `%LOCALAPPDATA%\AntiNotepad`, `Programs\AntiNotepad` or real Documents. No
  desktop/computer-use testing.
