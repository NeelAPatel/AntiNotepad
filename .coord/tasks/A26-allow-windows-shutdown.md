# A26-allow-windows-shutdown

You are the director (gpt-6.1-sol medium). Focused fix: do it yourself or with one gpt-6-luna xhigh subagent. Repo: this folder,
a git worktree on branch `feat/A26-allow-windows-shutdown`. Never switch branch, merge or commit (.git is read-only).
App: `product/AntiNotepad` (Avalonia 12.1.4, .NET 10). Read first: `AGENTS.md`, `docs/decisions.md`, `UI/PadWindow*.cs`,
`Platform/TaskbarPresence.cs`, `Program.cs`, and A8's save-failure quit handling (the "Couldn't save your notes" prompt).

## Bug (seen live 2026-10-24 23:07:44)
When Windows began a shutdown/restart/sign-out, the Application event log recorded, for both the installed and the dev copy:
`Microsoft-Windows-Winsrv 10001: The following application attempted to veto the shutdown: AntiNotepad.exe.` Then both processes
ended (the dev log shows `pad action=hide ... cause=hide` and nothing after; no save logged).
Cause in code: `UI/PadWindow.cs:85` `Closing += (_, e) => { if (!_quitting) { e.Cancel = true; HideResident(); } };` and
`Platform/TaskbarPresence.cs:137` (`e.Cancel = true` on the taskbar stand-in) cancel the close that Windows' end-session triggers.

## Fix
- On Windows session end (WM_QUERYENDSESSION / WM_ENDSESSION, or Avalonia's lifetime `ShutdownRequested` with its reason; check
  what Avalonia 12.1.4 raises and cite it), AntiNotepad must **not veto**: flush and save all notes synchronously and quickly,
  then let the session end. Never show the save-failure prompt or any UI during session end (log the failure instead); never
  block for more than a moment.
- Normal close of the pad (X, Alt+F4) keeps today's behavior (hide to tray). The stand-in keeps refusing ordinary closes, but
  must not block session end.
- Make sure a dev copy and the installed copy each handle it independently.

## Files in scope
`UI/PadWindow*.cs`, `Platform/TaskbarPresence.cs`, `Program.cs`, `Platform/` (a small session-end helper if needed),
`SelfTest.cs` (ONE new method at the end, logic-level: the close decision for normal close vs session end), `docs/decisions.md`
(one line).

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a26-check` (add `-p:NuGetAudit=false` only if
  NU1900 appears because nuget.org is unreachable): 0 errors, 0 warnings; `--selftest` SELFTEST PASS.
- Never launch AntiNotepad except `--selftest`, never kill it, never touch `%LOCALAPPDATA%\AntiNotepad`, Programs or Documents.
  Don't edit STATUS or the audit doc.
- Report at `.coord/reports/A26-allow-windows-shutdown.md`: the session-end path as built (with source evidence), what is saved
  and how fast, selftests, and how the manager can verify without restarting the PC (e.g. a dev-only way to simulate
  WM_QUERYENDSESSION to the dev copy's window, if safe).
