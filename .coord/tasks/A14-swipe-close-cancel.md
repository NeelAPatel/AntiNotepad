# A14-swipe-close-cancel

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/<this task's ID-slug>`. Never switch
branch, merge or commit (.git is read-only for you). App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10).
Read first: `AGENTS.md`, `docs/dev-loop.md`, and in `docs/audit/A1-codebase-audit.md` the finding(s) named in the task.
Match the surrounding code style, naming and comment density. First-party .NET/Avalonia only, no new packages.
Other workers edit other files in parallel: stay inside "Files in scope". Add selftest checks as ONE new method at the END of
`SelfTest.cs` with one call line in `Run` (other workers do the same; keep it easy to merge).

## Standard "Done means" (unless the task says otherwise)
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/<id>-check`: 0 errors, 0 warnings.
  `build/<id>-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Delete `build/<id>-check` if you can.
- Never launch AntiNotepad any other way, never kill it, never send it `--quit`, and never read or write
  `%LOCALAPPDATA%\AntiNotepad`, `%LOCALAPPDATA%\Programs\AntiNotepad` or the real Documents folder.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/<ID-slug>.md`: files changed, what was wrong, the fix, the selftest checks added, anything a person
  must check by eye or by hand (the manager cannot do live UI tests this round; mark those "needs a live check"), and anything
  not done.


## Finding: A1-13-progress-swipe-close-retention
`UI/NoteSwipe.cs` `ProgressSettle()` returns before `timer.Stop()` while `_owed` is not ~0. If frames stop arriving (Settings
closed mid-swipe, the pad hidden, the pane detached), the settle timer can tick forever and keep its owner alive.
The Settings motion preview (`UI/MotionPreview.cs`) hosts a NoteSwipe too.

## Fix
- Give NoteSwipe an explicit cancel/release path (e.g. `Cancel()` or `IDisposable`) that stops every timer it owns, clears
  `_owed`/`_progress`, unhooks any frame callbacks, and restores the pane (transform X 0, opacity 1) if it is still attached.
- Call it when the preview is released / the Settings window closes (MotionPreview's existing teardown).
- Make the settle timer self-terminating anyway: if `_owed` has not changed for a bounded time (about 1 s), drop it and settle.
- Keep normal swipe feel unchanged (thresholds, rubber band, timings).

## Files in scope
`UI/NoteSwipe.cs`, `UI/MotionPreview.cs`, `SelfTest.cs`.

## Selftest (logic only, no visible windows)
- a swipe with owed motion and no frames: after cancel, no timer is running;
- the settle timer gives up on its own after the bound if `_owed` never drains (drive it without real time if you can, e.g. an
  internal method that runs one tick with a supplied clock).

Build check folder id: `a14`.

