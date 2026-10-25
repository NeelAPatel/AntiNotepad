# A30-save-unavailable-fix

You are the director (gpt-6.1-sol medium). Delegate the implementation to a gpt-6-luna xhigh subagent (or do it yourself if
spawning is unavailable; say so at the top of the report).

Repo: this folder, the git worktree on branch `feat/A30-background-save-and-log`, holding the uncommitted
A30-background-save-and-log change (brief `.coord/tasks/A30-background-save-and-log.md`, report
`.coord/reports/A30-background-save-and-log.md`). Never switch branch, merge or commit.

## Review result
Accepted as is: the snapshot capture, the single background writer and coalescing, the synchronous `Save()` for quit, retry and
session end, revision-checked Dirty clearing, `BackgroundLog`, the log flush points, and the selftest style.

## Regression 1: an unavailable notes folder now looks like a save failure
Before A30, `Store.Save()` returned **true** (with `LastError` set) when the store was unavailable (`!IsAvailable` or the folder
is missing: the A7 state with `NotesLocationError`, where notes open read-only with a hint). That was deliberate: nothing can
be written, and there is nothing to lose. A30 returns false there, so:
- every autosave, hide and deactivation shows "Couldn't save · Notes folder is unavailable…" as a toast;
- Quit (tray, hotkey menu, and `--quit`, which `tools/install.ps1` relies on) is held behind the "Couldn't save your notes"
  prompt, so an install or upgrade times out while the folder is unavailable.
Fix: restore the old meaning. For an unavailable store, `QueueSave`, `Save` and `Flush` report success as a no-op and set
`LastError`; real write failures on an available store keep the A8 behavior (toast, quit blocked, Retry / Quit anyway). The
session-end log line may still record the unavailable state as before (it checks `LastError`).

## Regression 2: one failed save locks folder recovery
`Flush()` returns the sticky `_lastSaveSucceeded`, and `Load()`, `RemoveOrphans()` and `SwitchFolder()` refuse to run when it is
false (`SwitchFolder` throws). So after any failed write, and in the unavailable state, Settings > Storage "Choose folder" /
"Change location…" / Migrate can no longer switch to a working folder, which is exactly the way out of that state (A7, A11).
Fix: those three must drain the writer (wait for in-flight and queued writes to finish) but must not be blocked by an earlier
failure. Keep `Program.MoveNotes`'s pre-move `Store.Save()` gate for an available store (moving an unsaved store would lose
edits); it already skips when the store is unavailable.

## Selftest
Extend the existing `BackgroundSaveAndLogChecks` (no new method): an unavailable store's `QueueSave`/`Save`/`Flush` succeed
with `LastError` set and write nothing; after a failed write, `SwitchFolder` to a fresh temp folder succeeds and the next save
writes there.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a30-check`: 0 errors, 0 warnings;
  `--selftest` prints SELFTEST PASS. Delete `build/a30-check` and `build/A30-measure` if you can.
- Only `Model/Store.cs`, `Program.cs`, `UI/PadWindow.cs` and `SelfTest.cs` change.
- Append a "Rework" section to `.coord/reports/A30-background-save-and-log.md`.
- Never launch or kill AntiNotepad; never touch `%LOCALAPPDATA%\AntiNotepad`, `Programs\AntiNotepad` or real Documents. No
  desktop/computer-use testing.
