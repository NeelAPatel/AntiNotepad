# A15-startup-quit-pending

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


## Finding: A1-15-startup-quit-is-dropped
A `--quit` request from a second instance (the installer runs `AntiNotepad.exe --quit`) that arrives before `_window` is attached
is dropped (`Program.cs` and `Platform/SingleInstance.cs`). The installer then waits and aborts the upgrade.

## Fix
- Remember a pending quit when it arrives before the window exists, and act on it as soon as the window/lifetime is ready, via
  the normal quit path (flush and save, including the A8 save-failure handling; never skip saving).
- The second instance must still get its confirmation only after the first one really saved and exited (keep the existing
  protocol; if confirmation is sent early today, fix that and say so).
- Check whether an early show request has the same bug; fix it the same way if so, and report either way.
- The request arrives on a pipe/background thread: marshal to the UI thread.

## Files in scope
`Program.cs`, `Platform/SingleInstance.cs`, `SelfTest.cs`. (`Model/Welcome.cs` and `Model/Store.cs` are being edited by others.)

## Selftest
Logic only, without a second process: the pending-quit latch is set by an early request and consumed once when the window becomes
ready; a request after ready goes straight through.

Build check folder id: `a15`.

