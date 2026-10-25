# A24-taskbar-click-diagnose

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/A24-taskbar-click-diagnose`. Never switch
branch, merge or commit (.git is read-only). App: `product/AntiNotepad` (Avalonia 12.1.4, .NET 10). Read first: `AGENTS.md`,
`.coord/reports/A22-taskbar-button-fixes.md`, `.coord/reports/A23-taskbar-click-hides.md`, `Platform/TaskbarPresence.cs`,
`UI/PadWindow.cs`. Match the surrounding style.

## Live result of A23 (manager, Windows 11 26340, dev copy rebuilt with A23; verified the dll contains the A23 code)
Mode Always. Pad visible and foreground (GetForegroundWindow == pad HWND, checked right before). Clicking the stand-in's taskbar
button (verified by position) leaves the pad visible and foreground: it does not hide. Clicking the button while the pad is hidden
DOES show it. So the stand-in receives the click, but the decision comes out "focus", or the hide is undone afterwards.
A23's guess (the taskbar becomes foreground first) is unconfirmed: on Windows 11 the taskbar may not take activation at all.

## Task: instrument, then fix
1. Add dev-only logging (`Program.IsDev` or `AppInfo.IsDevBuild`, via `Program.Log`, one line per event, prefix `taskbar:`) for
   every message and decision on this path, with a monotonic ms timestamp: the stand-in's WM_SYSCOMMAND (wParam), WM_ACTIVATE /
   WM_ACTIVATEAPP / WM_NCACTIVATE / WM_SHOWWINDOW / WM_WINDOWPOSCHANGING (flags) on the stand-in; the pad's WM_ACTIVATE
   (state + counterpart HWND + its class name); the snapshot values at decision time (pad visible, GetForegroundWindow + class,
   last foreground-loss time/kind, elapsed ms), the decision (hide/show/focus), and any later show/hide/activation of the pad
   within 1 s (who triggered it: hide-on-blur, restore, activation fallback, etc.).
2. From the code, list the most likely sequences and make the decision robust for each, e.g. capture "pad was foreground" at the
   earliest stand-in message (WM_NCACTIVATE / WM_WINDOWPOSCHANGING / SC_RESTORE, whichever is first), and make sure a hide is not
   followed by an activation fallback that shows it again. Keep A22/A23 behavior otherwise (hidden click shows; inactive visible
   click focuses; Never/While visible/Always button counts).
3. The logging stays in (dev builds only, cheap) so the manager can read `build/testdata/antinotepad.log` after a live click.

## Files in scope
`Platform/TaskbarPresence.cs`, `UI/PadWindow*.cs`, `SelfTest.cs` (extend TaskbarChecks for any new pure decision rule).

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a24-check` (add `-p:NuGetAudit=false` only if
  NU1900 appears because nuget.org is unreachable): 0 errors, 0 warnings; `--selftest` SELFTEST PASS.
- Never launch AntiNotepad except `--selftest`, never kill it, never touch `%LOCALAPPDATA%\AntiNotepad`, Programs or Documents.
  Don't edit STATUS or the audit doc.
- Report at `.coord/reports/A24-taskbar-click-diagnose.md`: the sequences considered, the fix, the log lines to expect for each
  click type, and the selftests.
