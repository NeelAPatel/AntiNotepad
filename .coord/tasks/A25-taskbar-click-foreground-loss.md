# A25-taskbar-click-foreground-loss

You are the director (gpt-6.1-sol medium). This is a focused fix: do it yourself or with one gpt-6-luna xhigh subagent.
Repo: this folder, a git worktree on branch `feat/A25-taskbar-click-foreground-loss`. Never switch branch, merge or commit (.git is
read-only). App: `product/AntiNotepad` (Avalonia 12.1.4, .NET 10). Read first: `AGENTS.md`,
`.coord/reports/A24-taskbar-click-diagnose.md`, `Platform/TaskbarPresence.cs`, `UI/PadWindow.cs`. Match the surrounding style.

## Live trace (manager, Windows 11 26340, dev copy with A24, Taskbar icon = Always)
The pad was visible and foreground (the manager had just clicked into it); then the manager clicked the stand-in's taskbar button.
Expected: hide. Actual: decision `focus`, the pad stays visible. The A24 trace of that click (wall clock removed):

```text
taskbar: t=179394.9ms pad WM_NCACTIVATE active=False lParam=0x0 visible=True cause=api-show age=179191.2ms
taskbar: t=179395.5ms pad WM_ACTIVATE state=WA_INACTIVE wParam=0x0 counterpart=0x0/(none) visible=True active=True cause=api-show age=179191.7ms
taskbar: t=179396.0ms pad foreground-loss kind=Other at=179395.6ms counterpart=0x0/(none) included=True
taskbar: t=179396.5ms pad Avalonia.Deactivated visible=True active=False cause=api-show age=179192.7ms
taskbar: t=179399.3ms pad WM_ACTIVATEAPP active=False thread=13492 visible=True cause=api-show age=179195.5ms
taskbar: t=179399.6ms proxy hwnd=0x1311062 WM_ACTIVATEAPP active=False thread=13492
taskbar: t=179412.0ms proxy hwnd=0x1311062 WM_WINDOWPOSCHANGING flags=0x00000003 names=SWP_NOSIZE|SWP_NOMOVE internal=False
taskbar: t=179412.4ms click snapshot source=WM_WINDOWPOSCHANGING padVisible=True padActive=False fg=0x1311062/Avalonia-fcb0ad15-15d8-4a3a-be90-46a3b6cd7c8a lossKind=Other lossAt=179395.6ms lossAge=16.7ms padWasForeground=False
taskbar: t=179413.3ms proxy hwnd=0x1311062 WM_ACTIVATEAPP active=True thread=13492
taskbar: t=179413.6ms pad WM_ACTIVATEAPP active=True thread=13492 visible=True cause=api-show age=179209.8ms
taskbar: t=179414.0ms proxy hwnd=0x1311062 WM_NCACTIVATE active=True lParam=0x0
taskbar: t=179414.3ms proxy hwnd=0x1311062 WM_ACTIVATE state=WA_ACTIVE minimized=True counterpart=0x0/(none) wParam=0x200001
taskbar: t=179415.2ms decision source=WM_ACTIVATE result=focus padVisible=True padActive=False fg=0x1311062/Avalonia-fcb0ad15-15d8-4a3a-be90-46a3b6cd7c8a previousForeground=0x0/(none) lossKind=Other lossAt=none lossAge=na recentTaskbarLoss=False earlySnapshotPadVisible=True earlySnapshotPadActive=False earlySnapshotForeground=0x
taskbar: t=179415.6ms proxy Avalonia.Activated fallback
taskbar: t=179415.7ms decision duplicate-suppressed source=Avalonia.Activated reason=activation-pending
taskbar: t=179416.2ms proxy hwnd=0x1311062 WM_SYSCOMMAND wParam=0xF120 command=0xF120 name=SC_RESTORE lParam=0x0
taskbar: t=179416.3ms click snapshot source=WM_SYSCOMMAND/SC_RESTORE padVisible=True padActive=False fg=0x1311062/Avalonia-fcb0ad15-15d8-4a3a-be90-46a3b6cd7c8a lossKind=Other lossAt=none lossAge=na padWasForeground=False
taskbar: t=179416.4ms decision duplicate-suppressed source=SC_RESTORE reason=activation-pending
taskbar: t=179558.3ms decision dispatch source=WM_ACTIVATE result=focus padVisible=True
taskbar: t=179558.6ms pad taskbar callback source=WM_ACTIVATE action=focus visible=True
taskbar: t=179558.7ms pad action=show-begin cause=taskbar-click:WM_ACTIVATE visible=True
taskbar: t=179559.0ms proxy hwnd=0x1311062 WM_NCACTIVATE active=True lParam=0x118162A
taskbar: t=179559.2ms proxy hwnd=0x1311062 WM_ACTIVATE state=WA_INACTIVE minimized=True counterpart=0x118162A/Avalonia-1ddd4f6a-a2a5-491a-9ab6-ed73b13f06ef wParam=0x200000
taskbar: t=179561.3ms pad WM_NCACTIVATE active=True lParam=0x1311062 visible=True cause=taskbar-click:WM_ACTIVATE age=2.6ms
taskbar: t=179561.5ms pad WM_ACTIVATE state=WA_ACTIVE wParam=0x1 counterpart=0x1311062/Avalonia-fcb0ad15-15d8-4a3a-be90-46a3b6cd7c8a visible=True active=False cause=taskbar-click:WM_ACTIVATE age=2.8ms
taskbar: t=179561.6ms pad Avalonia.Activated visible=True active=False cause=taskbar-click:WM_ACTIVATE age=2.9ms
taskbar: t=179562.0ms pad action=show-end cause=taskbar-click:WM_ACTIVATE visible=True
```

## What it shows
- On Windows 11 a taskbar click deactivates the pad with **no counterpart** (`WM_ACTIVATE WA_INACTIVE counterpart=0x0`: the
  window being activated belongs to another thread, i.e. Explorer's taskbar). A23/A24 only count losses to `Shell_TrayWnd`,
  `Shell_SecondaryTrayWnd` or the stand-in, so this loss is `kind=Other` and is ignored.
- ~17 ms later the stand-in gets `WM_WINDOWPOSCHANGING`, then `WM_ACTIVATE WA_ACTIVE`, then `SC_RESTORE`. The snapshot taken at
  that first message already has `padWasForeground=False`, and the decision at `WM_ACTIVATE` even shows `lossAt=none` although the
  loss was recorded 17 ms before (check why: consumed/cleared too early?).

## Fix
- Treat "the pad lost foreground within the grace window (500 ms) before the click's first stand-in message" as
  `padWasForeground=True`, for counterparts `none` (0x0 / other thread), taskbar classes, and the stand-in. A loss to a *different
  window of another app* that the user clicked (non-null counterpart of another class) should still not count, if the counterpart
  is known. Explain the rule you choose and why in the report.
- Make sure the loss recorded just before the click is still available at decision time (the `lossAt=none` above).
- Keep everything else from A22–A24 (button counts per mode; hidden click shows; inactive visible click focuses; post-hide
  fallback suppression; dev-only `taskbar:` trace).

## Files in scope
`Platform/TaskbarPresence.cs`, `UI/PadWindow*.cs`, `SelfTest.cs` (extend TaskbarChecks: a loss with null counterpart 17 ms before
the first stand-in message => hide; an old loss => focus/show as before).

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a25-check` (add `-p:NuGetAudit=false` only if
  NU1900 appears because nuget.org is unreachable): 0 errors, 0 warnings; `--selftest` SELFTEST PASS.
- Never launch AntiNotepad except `--selftest`, never kill it, never touch `%LOCALAPPDATA%\AntiNotepad`, Programs or Documents.
  Don't edit STATUS or the audit doc.
- Report at `.coord/reports/A25-taskbar-click-foreground-loss.md`: the rule, why `lossAt=none` happened, selftests, and the trace
  lines the manager should now see for a focused-pad click.
