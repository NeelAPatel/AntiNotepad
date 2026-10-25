# A30-background-save-and-log

You are the director (gpt-6.1-sol medium). You plan, split and check the work; delegate the implementation to gpt-6-luna xhigh
subagents (how many, and how to split, is your call). If subagent spawning is not available to you in this run, say so at the
top of your report and do the work yourself.

Repo: this folder, a git worktree on branch `feat/A30-background-save-and-log` (off `dev`). Never switch branch, merge or
commit (.git is read-only for you and your subagents); the manager (a Claude session) reviews the diff, sends fixes back, and
integrates. Subagents share this one worktree, so give each one a disjoint set of files. Two other Codex runs work in parallel
in other worktrees (A29-incremental-analysis: editor analysis; A31-no-windows-show-animation: pad show/hide in
`UI/PadWindow.cs` and `Platform/Native.cs`); stay out of their areas and keep your `UI/PadWindow.cs` edits to the save calls.
App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). Read first: `AGENTS.md`, `docs/dev-loop.md`,
`docs/decisions.md`, finding A1-14 plus A1-1, A1-2, A5-1, A5-2 in `docs/audit/A1-codebase-audit.md`, and the A7, A8, A15 and
A26 reports in `.coord/reports/`. Match the surrounding style, naming and comment density. First-party .NET only.

## Finding: A1-14-synchronous-save-and-log (effort L)
`Model/Store.cs` `Save()` serializes the index and writes note files synchronously on the UI thread (autosave while typing,
hide, quit), and `Program.Log` writes the log file synchronously too.
Direction (refine it): snapshot what must be saved on the UI thread (cheap: ids, metadata, the dirty bodies' text), write it on
one background writer that serializes saves in order and coalesces bursts; same for log lines (a background queue, flushed in
order). Never read live mutable note/editor state off the UI thread.

## Must keep working (already set up, verified by the user)
- A26: Windows shutdown/restart/sign-out saves at once (including an edit typed just before) and lets the session end; this
  path must flush everything pending and finish before replying, still within a few ms.
- A8: a failed save shows "Couldn't save your notes" with Retry and Quit anyway; Quit never exits after a failed or still-
  pending save. Save failures must still reach the UI (marshal back).
- A15 and `--quit` (used by `tools/install.ps1`): quit saves and waits for the writer before exiting.
- A7/A11: notes folder pointer, Change location/Migrate notes (verified copy + backup) must not race the writer (pause or
  drain it during a move); recovered folder handling (A1-1) unchanged.
- A5-1/A5-2: unavailable bodies are never written; an invalid index still falls back to the backup.
- The store file format, atomic write/backup behavior and file names stay exactly the same. Prod data must load unchanged.
- Hide stays instant (hiding must not wait on disk).

## Scope
Files: `Model/Store.cs` (save path), `Program.cs` (logging and quit/shutdown flush only), `UI/PadWindow.cs` (save call sites
and the save-failure prompt only), `Platform/SessionEnd*` if the shutdown flush lives there, `SelfTest.cs` (ONE new method at
the END plus one call line in `Run`). Anything else: say why in the report.

## Selftest (small capability checks; temp folders only; no reflection, no test-only hooks in product classes)
- a burst of saves is coalesced and the files on disk equal the last snapshot;
- a flush after queued saves writes everything before returning (the shutdown/quit path);
- a failing write is reported back as a failure, and quit is still blocked;
- log lines are written in order after a flush.

## Measure
UI-thread time for one autosave of a 1000-line note and for a log call, before and after, in the report (temporary harness,
not left in product code).

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a30-check`: 0 errors, 0 warnings;
  `build/a30-check/AntiNotepad.exe --selftest` prints SELFTEST PASS. Delete `build/a30-check` if you can.
- Never launch AntiNotepad any other way, never kill it, never read or write `%LOCALAPPDATA%\AntiNotepad`,
  `%LOCALAPPDATA%\Programs\AntiNotepad` or the real Documents folder. No desktop/computer-use testing.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A30-background-save-and-log.md`: split, files changed, design as built (writer, coalescing,
  flush points, failure path, folder-move interaction), before/after numbers, selftest list, what the user must check by
  hand in the dev copy (marked "needs a live check"), anything not done.
