# A16-staged-upgrade-rollback

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


## Finding: A1-17-upgrade-has-no-rollback
`tools/install.ps1` (~:132) copies the new build over the installed copy, then runs its selftest. A failed copy or selftest
leaves a half-upgraded install with no way back. Notes are not affected (they live in the notes folder).

## Fix
- Keep publishing to the staging folder. Before touching the install folder, move the current install to a sibling backup,
  e.g. `%LOCALAPPDATA%\Programs\AntiNotepad.previous-<stamp>` (the app has quit by then).
- Put the new files in place, run the installed `--selftest`. On any failure after the swap began: restore the backup into place,
  relaunch the old version only if it was running before, and print a clear message.
- On success remove the backup (or keep only the latest one; say which in the report).
- First install (nothing installed before): no backup; on failure remove the partial install folder.
- Fix the stale success message "Data remains at $dataDirectory": since A7 the notes live in the folder named in
  `%LOCALAPPDATA%\AntiNotepad\location.json` (default `Documents\AntiNotepad`). Print that path when it can be read, else the
  app home.
- Update `tools/uninstall.ps1` only if it must know about the backup folder; update the install notes in `docs/dev-loop.md`.

## Hard safety rule (overrides the standard Done means)
You must NOT run `tools/install.ps1` for real: it would quit and overwrite the user's real installed copy. Allowed checks:
- PowerShell parse check (`[System.Management.Automation.Language.Parser]::ParseFile`) of each changed script;
- `-DryRun`, only if it touches nothing;
- for a real end-to-end check, add an optional test-only parameter (e.g. `-InstallRoot <folder>`) that also skips the `--quit`
  request, the Start menu shortcut, the Run key and the launch, and run it only against a folder under the repo's `build/`.
  Also simulate a failing selftest there (a test-only switch) and show the rollback restored the old files. Say exactly what the
  test parameter skips.
No C# change is expected, so no app build is needed unless you change C#.

## Files in scope
`tools/install.ps1`, `tools/uninstall.ps1`, `docs/dev-loop.md`.

