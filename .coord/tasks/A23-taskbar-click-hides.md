# A23-taskbar-click-hides

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/A23-taskbar-click-hides`. Never switch
branch, merge or commit (.git is read-only). App: `product/AntiNotepad` (Avalonia 12.1.4, .NET 10). Read first: `AGENTS.md`,
`.coord/reports/A22-taskbar-button-fixes.md`, `Platform/TaskbarPresence.cs`, `UI/PadWindow.cs`. Match the surrounding style.

## Live result of A22 (manager, computer use, Windows 11, dev copy)
Pass: While visible (one button only while the pad is visible), Always (one stable stand-in in both states), Never (none),
click while hidden shows and focuses the pad, the Settings row icon, no stale hotkey-to-focus log.
**Fail:** with the pad visible AND foreground (verified with GetForegroundWindow == pad HWND just before), clicking its taskbar
button leaves it visible and foreground. It should hide (same as the hotkey). Reproduced in While visible and Always.

Likely cause: a taskbar click first makes Explorer's taskbar (`Shell_TrayWnd`, explorer.exe) the foreground window, so the pad
deactivates with the taskbar, not the stand-in, as the WM_ACTIVATE counterpart; by the time the stand-in's SC_RESTORE/activation
arrives, the "pad was focused" snapshot is false and the action becomes "focus". Verify this with the code and fix it robustly:
e.g. record the time the pad last lost foreground and treat "pad was foreground within the last ~500 ms and the foreground went
to the taskbar or the stand-in" as focused at click time. Keep A22's other behavior unchanged.

## Files in scope
`Platform/TaskbarPresence.cs`, `UI/PadWindow*.cs`, `SelfTest.cs` (extend the existing TaskbarChecks with the decision rule,
e.g. a pure function taking last-foreground-loss time, now, counterpart class/kind).

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a23-check` (add `-p:NuGetAudit=false` only if
  NU1900 appears because nuget.org is unreachable): 0 errors, 0 warnings; `--selftest` SELFTEST PASS.
- Never launch AntiNotepad except `--selftest`, never kill it, never touch `%LOCALAPPDATA%\AntiNotepad`, Programs or Documents.
  Don't edit STATUS or the audit doc.
- Report at `.coord/reports/A23-taskbar-click-hides.md`: cause as confirmed in code, the fix, selftests, what the manager should
  click to verify.
