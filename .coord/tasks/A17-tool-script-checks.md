# A17-tool-script-checks

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


## Findings: A5-4-worker-output-slug-unchecked and A5-5-hotkey-case-depends-on-language
1. `.coord/tools/codex-run.sh`: the task argument goes into log and report paths unchecked (`../` escapes `.coord/`). Accept only
   `<ID>-<slug>`. Before you settle the regex, run it against every task name in `.coord/logs/usage.tsv` (column 5) and every file
   name in `.coord/tasks/` (without `.md`, ignoring names starting with `_`), and make sure all pass; list any that don't and why.
   Reject with a clear message and a non-zero exit before anything is written (no ledger line, no log).
2. `tools/press-hotkey.ps1`: `$Key.ToUpper()` uses the current culture (Turkish `i` -> `İ`). Use `ToUpperInvariant()`.

## Files in scope
`.coord/tools/codex-run.sh`, `tools/press-hotkey.ps1`. Nothing else.

## Done means (replaces the standard one)
- `bash -n .coord/tools/codex-run.sh` passes. `CODEX_DRY=1 .coord/tools/codex-run.sh worker A17-tool-script-checks x` prints a
  PLAN; `CODEX_DRY=1 .coord/tools/codex-run.sh worker ../evil x` is rejected and writes nothing. Check whether CODEX_DRY adds a
  ledger line today; if it does, report it, don't change it.
- PowerShell parse check of `press-hotkey.ps1`. Do NOT run press-hotkey.ps1 (it sends real keystrokes).
- No build needed. Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A17-tool-script-checks.md`: the regex, the names tested, results.

