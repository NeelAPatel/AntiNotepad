# F37-hidden-selftest-windows

Director: Codex gpt-6.1-sol medium. Do it yourself (small); spawn a Luna subagent only if it clearly helps.
Branch: feat/function-contract-separation (the manager handles ALL git; you run no git commands that write).
Naming: always `<ID>-<slug>` (F37-hidden-selftest-windows), never a bare ID.
Finish in ONE turn. End only when the report is written or you are blocked.

## Read
AGENTS.md, docs/architecture.md, src/AntiQuickPad/Program.cs, App.cs, Shell/PlatformServices.cs, Shell/AvaloniaWindowPlatform.cs, Views/Common/ResidentShellWindow*.cs, Views/Classic/ClassicShellWindow*.cs, Views/Common/ShellRender.cs, Design/DesignEntry.cs, Design/DesignWindow.cs, Measurement.cs.

## Write only
src/AntiQuickPad/ (test/render/design startup paths, Shell/, Views/*Smoke*, Measurement.cs as needed), `.coord/reports/F37-hidden-selftest-windows.md`. Scratch output only under `.coord/scratch/`. No Core or ViewModel changes. No docs/, no STATUS/TRACKER, no git, no network, never kill processes.

## Problem
`--selftest` (Quiet/Card resident), its child `--selftest --shell classic`, that child's `--selftest-quit-resident` child (shown at startup, then hidden), the measurement smoke (`Measurement.Smoke` -> `--measure` -> `--measure-child`), `--render-shell`, `--render-design` and `--probe-backdrop` all show real windows on the user's desktop and can take foreground/focus from the user's running pad. `CreateNoWindow` only hides consoles.

## Goal
None of those runs may show a window on any screen, appear in the taskbar, or take foreground/activation. Production (normal launch, hotkey, tray, real `--measure`) must behave exactly as before.

## Approach (suggested; adapt if a probe disproves it)
- Seam: `ShellServices.CreateWindowPlatform` is injected in App. In test/render/design modes give it a hidden platform: Show places the window off every screen (outside the union of `Screens.All`), `ShowActivated = false`, `ShowInTaskbar = false`; Activate does nothing. Normal launches keep `AvaloniaWindowPlatform` unchanged.
- Bypasses to cover: the classic desktop lifetime shows `desktop.MainWindow` at startup before any platform call (set position/ShowActivated/ShowInTaskbar on the window before it is shown in those modes); grep `Views/` and `Design/` for direct `Show()`, `Activate()`, `WindowState = Maximized` and similar, and route or neutralise them in test modes. A maximized window lands on a monitor: if the Classic smoke's maximize step cannot stay off-screen, drop that one step and say so.
- PROBE FIRST: check early whether keyboard focus (`IsKeyboardFocusWithin`) still lands in a non-activated, off-screen window and whether show-to-focus completes. If not, add a test-only focus path that leaves the production focus path byte-for-byte unchanged in behaviour. Keep the < 100 ms show-to-focus assertions meaningful.
- Measurement smoke inside selftest: the measure child refuses to complete focus unless it is the foreground window. Choose ONE and record it in the report under "Decisions": (a) give the smoke a hidden variant that skips the foreground check (plumbing check only), or (b) drop it from selftest (the manager runs real `--measure` separately). The real `--measure` path must stay untouched.
- Assertions (nobody is watching tonight, so the test must prove it): record `GetForegroundWindow()` at selftest start; assert no test window ever becomes foreground and the foreground window is unchanged at the end (tolerate it changing for reasons outside the test only if it is not one of ours); assert every test window's rect intersects no screen in `Screens.All`; assert `ShowInTaskbar == false`. Cover the main selftest, the Classic child and render-shell at least.

## Build and test
From src/AntiQuickPad: `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit` (never kill), then with `AVALONIA_TELEMETRY_OPTOUT=1`: `dotnet build -c Release --no-restore -p:UseSharedCompilation=false`. Run `--selftest` with env `ANTIQUICKPAD_ALIGNMENT_OUTPUT` pointing at `.coord/scratch/align` (so no tracked PNGs change). Also run `--render-shell quiet light .coord/scratch/render` once.

## Done when
Release build 0 warnings / 0 errors; `--selftest` exit 0 including the new no-window/no-foreground assertions; render-shell exit 0 with the same assertions; report `.coord/reports/F37-hidden-selftest-windows.md` lists: files changed, each window path and how it is hidden, the measurement-smoke decision, probe result on focus, selftest output tail, anything the user must check physically.
