# F37-hidden-selftest-windows

Completed 2026-10-21. Release build: 0 warnings, 0 errors. Full `--selftest` and `--render-shell quiet light` exit 0, including hidden-window assertions. Additional `--render-design quiet` and `--probe-backdrop mica` checks exit 0.

## Files changed

- `src/AntiQuickPad/Shell/HiddenWindowPlatform.cs` (new): hidden adapter, pre-show native cloaking/placement, foreground-event guard and geometry/taskbar assertions.
- `src/AntiQuickPad/App.cs`: inject hidden adapter only for isolated CLI modes; register window before desktop lifetime can show it.
- `src/AntiQuickPad/Program.cs`: select hidden modes before startup; assert at process completion; put isolated runtime data/proofs under `.coord/scratch/hidden-runs/`.
- `src/AntiQuickPad/Design/DesignEntry.cs`: register both design/probe windows before startup; invisible backdrop probes close after reporting and any requested delay.
- `src/AntiQuickPad/Views/Common/ResidentShellWindow.Smoke.cs`: capture Classic child stdout/stderr so its assertions appear in the parent log.
- `src/AntiQuickPad/Views/Classic/ClassicShellWindow.Smoke.cs`: remove maximize and measurement invocation; surface quit-resident child's assertions.
- `.coord/reports/F37-hidden-selftest-windows.md`: this report.

No Core, ViewModel, production window adapter, production focus methods, or Measurement.cs changes. No git commands, network requests, process kills, docs, STATUS or TRACKER writes. No subagent needed.

## Each window path

| Path | How hidden / verified |
| --- | --- |
| `--selftest` Quiet/Card resident | Hidden adapter's Show places outside all screens; Activate is a no-op. App registers native protection before desktop lifetime's initial Show. Full suite verifies both shells in the same window. |
| `--selftest --shell classic` child | Same injection and pre-show registration, covering automatic startup, close/hide, toggles, bounds restore, pin changes, second-launch show, and save-failure recovery. Child assertions propagate through its exit code and captured output. |
| `--selftest-quit-resident` child | Independently selects hidden startup, even without `--selftest`; initially shown only off-screen/cloaked, then hidden by its existing startup path. Guard checks before that Hide and asserts on process completion. |
| `Measurement.Smoke -> --measure -> --measure-child` | Removed from selftest; no measurement child is launched by the smoke suite. See Decisions. |
| `--render-shell` | Real resident host uses the same adapter and pre-show protection. Direct editor focus stays managed; captures still produce 14 PNGs. Exit includes assertions. |
| `--render-design` | Design window registered before assignment to MainWindow; same native protection and assertions without resident services. Produces 10 PNGs. |
| `--probe-backdrop` | Probe window registered before assignment to MainWindow; same native protection and assertions. Reports requested/actual transparency then closes, honoring an optional close delay. |

All protected windows have `ShowActivated=false`, `ShowInTaskbar=false`, manual startup placement and normal state. Placement is beyond maximum screen right/bottom plus 4096 pixels. Native `WM_WINDOWPOSCHANGING` interception forces that position before application, sets `SWP_NOACTIVATE`, and also covers resizing/pin/native startup operations. DWM cloaking is applied before the first Show as an additional visibility barrier. Mouse activation is suppressed. These hooks apply only to registered isolated windows.

## Decisions

1. Measurement option **(b)**: omit `Measurement.Smoke` from selftest. Real `--measure`, its foreground requirement, SendInput, hotkey registration and measurement children are unchanged. Manager runs real measurements separately on the desktop.
2. Drop Classic smoke's maximize/restore step. Native maximize is a desktop behavior and should be checked physically; ordinary bounds round trip and pin persistence remain tested.
3. The suggested Avalonia-only off-screen placement was insufficient: the initial probe latched a native rectangle on the 3840x2160 monitor; Classic reported `3024,1473,3840,2112`. Added native placement interception and pre-show cloaking rather than weakening the rectangle assertion.
4. Backdrop probe now exits after reporting by default, since its former physical close interaction is inaccessible while hidden. `--close-after-ms` remains an additional delay. Actual visual backdrop evaluation requires a separately authorized visible diagnostic.

## Focus probe

The first adapter probe retained the exact existing FocusEditor/CompleteFocus methods. Quiet/Card completed real `IsKeyboardFocusWithin` and focus task completion in about 1.7–2.5 ms, although the geometry guard correctly rejected native startup placement. With native placement protection and cloaking, Classic passed too. No test-only focus substitute was necessary.

Final complete run: Quiet 2.212, 2.033, 1.708 ms; Card 2.341, 1.877, 1.635 ms; Classic 1.558, 0.821, 3.155 ms. Existing <100 ms assertions remain intact and measure actual managed editor focus completion. These are internal timings, not foreground/hotkey-to-paint measurements.

## Assertion coverage

The guard records `GetForegroundWindow()` before Avalonia startup, hooks process-filtered `EVENT_SYSTEM_FOREGROUND`, latches any foreground event or Avalonia activation, and checks foreground ownership periodically and at completion. It tolerates external foreground changes only when the final owner is not the test process. Each window-owning child installs its own guard.

At Opened, subsequent visible position/size changes, adapter Show and 10 ms timer intervals, it checks native `GetWindowRect()` against every `Screens.All` rectangle, native DWM cloaking, `ShowInTaskbar=false` and `ShowActivated=false`. Failures latch and force process exit 1 even after a capability suite otherwise passes. Pre-native-show size/position notifications are excluded; the HWND is cloaked and its pending placement constrained before initial Show. The quit-resident child gets two checks even though it immediately hides at startup.

## Validation and logs

From `src/AntiQuickPad`, ran the requested graceful `--quit`, then with `AVALONIA_TELEMETRY_OPTOUT=1`:

```text
dotnet build -c Release --no-restore -p:UseSharedCompilation=false
Build succeeded. 0 Warning(s), 0 Error(s).
dotnet bin/Release/net10.0/AntiQuickPad.dll --selftest
exit 0; ANTIQUICKPAD_ALIGNMENT_OUTPUT=<repo>/.coord/scratch/align
dotnet bin/Release/net10.0/AntiQuickPad.dll --render-shell quiet light ../../.coord/scratch/render
exit 0; 73 native rect/cloak/taskbar checks
dotnet bin/Release/net10.0/AntiQuickPad.dll --render-design quiet ../../.coord/scratch/design
exit 0; 60 native rect/cloak/taskbar checks
dotnet bin/Release/net10.0/AntiQuickPad.dll --probe-backdrop mica --report ../../.coord/scratch/F37-hidden-selftest-windows-backdrop.txt
exit 0; actual=Mica; 52 native rect/cloak/taskbar checks
```

Full logs: `.coord/scratch/F37-hidden-selftest-windows-{build,selftest,render,design,backdrop}.log`. Earlier investigation logs: `F37-hidden-selftest-windows-probe.log` and `F37-hidden-selftest-windows-classic.log` in the same scratch folder. All fixture data, PNGs and runtime logs created during this task are under `.coord/scratch/`.

Selftest output tail (selected final lines):

```text
F37-hidden-selftest-windows PASS: quit-resident child; windows=1; native rect/cloak/taskbar checks=2; no activation/foreground events; foreground start=0x0 end=0x0 (unchanged).
T009 command quit PASS: isolated resident exit=0, pending edit saved, source=command start/finish, no Show signal; elapsed=1340ms.
T009 timeout PASS: exit=1 after 5092ms; no-resident exit=0.
F37-hidden-selftest-windows: real --measure omitted; manager runs it separately on the desktop.
M4 PASS: integrated notch/burst; 1:1 drag; long/short snap; caret/scroll; cancellation; delete keeps one note.
show-to-focus internal ms=1.558
show-to-focus internal ms=0.821
show-to-focus internal ms=3.155
T006 PASS: heading boundaries; list Enter/undo; quit failure restores visible status and logs reason.
SELFTEST PASS: same resident editor; toggle/focus; debounced save; next-process reload; second-process signal/show; notes round trip.
T006 PASS: successful quit start/finish logs.
F37-hidden-selftest-windows PASS: selftest; windows=1; native rect/cloak/taskbar checks=784; no activation/foreground events; foreground start=0x0 end=0x0 (unchanged).
SELFTEST PASS: Quiet + Card production shells and retained Classic capability suite.
T006 PASS: successful quit start/finish logs.
F37-hidden-selftest-windows PASS: selftest; windows=1; native rect/cloak/taskbar checks=1475; no activation/foreground events; foreground start=0x0 end=0x0 (unchanged).
```

## Physical checks still needed

This runner returned foreground handle `0x0` throughout. Automated checks establish native off-screen geometry, cloaking, flags and absence of reported activation/foreground events in this execution environment; they do not establish preservation of a specific foreground app on the user's interactive desktop. Re-run selftest/render with another app focused and confirm no flash, taskbar item, or focus theft across the user's actual monitor layout. Normal launch/hotkey/tray behavior, physical maximize/snap, real `--measure`, and subjective backdrop appearance were not exercised. Their production adapter/focus/measurement implementation remains unchanged.

## Manager follow-up (2026-10-21)
`--probe-backdrop` is excluded from hidden mode again and keeps its original close behaviour. It is the interactive window for the F25-translucency-live-look check, so hiding it defeated its purpose. `--render-design`, `--render-shell` and all selftest paths stay hidden.

## Live verification (2026-10-21, user awake)
The manager clicked into the user's running pad (Card shell) and ran `--selftest`. Three screenshots of the primary monitor during the run showed no new window. The selftest reported the foreground window 0x606C6 unchanged from start to end, and it belongs to the resident AntiQuickPad.exe (PID 9364). Exit 0; conformance quiet/card PASS. The second monitor was not screenshotted; the selftest's rect assertion covers every screen.
