> CURRENT: Feedback fixes built/tested; short retest pending. Use bin/verify/retest/InputProbe.dll after exiting the older app. Read the LAST CURRENT section for four exact tests and measured results; earlier launch/chord/wheel instructions are historical.

> CURRENT: Mouse-only prototype built and self-checked. WAITING ON USER. Read the CURRENT section at the end for new DLL launch, focus/mouse checks and fallback hotkey. Earlier touchpad/NuGet blockers below are historical and superseded.

# Spike A checkpoint — T-002

Updated: 2026-10-17
Owner: spike-a-director
State: Gate 1 implementation dispatched; no later milestone may precede input validation.

- Read AGENTS.md, STATUS.md, HANDOFF.md, T-002, spike spec, T-001 report and research.
- Worker input_probe (gpt-6-luna high) owns spikes/A only: pinned Avalonia/AvaloniaEdit input probe, restore/build.
- Director owns docs/spikes/A and this checkpoint/report. Parallel Director owns B; STATUS.md is deliberately untouched per user instruction.
- Existing unrelated working-tree edits are preserved.

Next: review probe, launch if supported, request physical precision-touchpad test with exact instructions. No timeout-only release approximation qualifies.

## Gate 1 source review checkpoint

Probe source written. Restore/build in progress. Director review requested monotonic Stopwatch timing and handled-event observation so editor input consumption cannot hide wheel/key events. Physical PTP test remains mandatory; no release signal is claimed. Acceptance protocol is docs/spikes/A/INPUT-PROTOCOL.md. .NET SDK 10.0.402, Windows build 10.0.26340, win-x64 verified locally.

## Completion checkpoint — BLOCKED

Probe source reviewed and integrated. Restore fails NU1301; exact Avalonia packages absent from cache. Director reproduced with dotnet build --no-restore (exit 1), captured docs/spikes/A/build-block.txt. No successful compile/launch/screenshot/hardware observation. Later milestones stopped at the mandatory gate. Both required reports are written: .coord/reports/T-002.md and docs/spikes/A/REPORT.md. Twelve manual smoke checks are authored, none run.

### Waiting on user — exact instructions

1. Open normal local PowerShell with NuGet.org access. Do not change security/proxy settings for this task.
2. Run:

```powershell
Set-Location 'F:\LocalVSProj\Visual Studio Apps\AntiQuickPad'
dotnet restore .\spikes\A\InputProbe.csproj
dotnet build .\spikes\A\InputProbe.csproj --no-restore
dotnet run --no-build --project .\spikes\A\InputProbe.csproj
```

3. If a command fails, return its output to spike-a-director. If build succeeds, perform docs/spikes/A/INPUT-PROTOCOL.md: editor focused; two-finger horizontal pan; pause while still touching; lift at rest; flick/lift with inertia; reverse/cancel. Mark F1 MOVE, F2 PAUSE, F3 LIFT, F4 INERTIA, F5 CANCEL, F6 END. Keep the displayed spikes/A/logs/*.log and device/driver/scaling notes.
4. Resume spike-a-director with build output and log, or provide a session with working package restore so Director can compile/repair before the physical test. No gate pass can be inferred from missing logs or a timeout.

Next after unblock: validate release semantics; if missing, try one bounded native adapter; repeat physical test. Only on gate pass implement remaining T-002 milestones.

### Local commit blocker

Task files are not committed. `git add` failed: `Unable to create .../.git/index.lock: Permission denied`; the sandbox exposes .git read-only. No GitHub or remote operation was attempted. In a normal local PowerShell session after reviewing these owned files, run:

```powershell
git add -- spikes/A docs/spikes/A .coord/status-A.md .coord/reports/T-002.md
git diff --cached --stat
git commit -m "Add spike A input probe and document blocked NuGet gate" -- spikes/A docs/spikes/A .coord/status-A.md .coord/reports/T-002.md
```

The explicit paths preserve parallel Director changes. Do not stage or commit B or unrelated files.

## Resume checkpoint — restored package API correction

Manager restored dependencies in a networked shell. Director inspected the actual cached Avalonia 12.1.4 lib/net10.0/Avalonia.Base.dll with reflection: ScrollGestureInertiaStartingEventArgs exposes Id (Int32) and Inertia (Avalonia.Vector), not Velocity. Updated logging to record the actual property name and gesture ID. Offline build is next; launch only after success. Physical test remains pending.

## Build milestone — PASS (offline)

`dotnet build .\spikes\A\InputProbe.csproj --no-restore` succeeded: 0 warnings, 0 errors, 2.94 s. Evidence: docs/spikes/A/build-success.txt. No restore/network operation was used. Avalonia build telemetry first failed trying to write outside workspace; retried with process-local AVALONIA_TELEMETRY_OPTOUT=1 as documented in cached avalonia.buildservices/11.3.2/README.md. No system setting was changed. Launched the successfully built InputProbe.exe for the physical test.

## Waiting on user — physical Gate 1 test (CURRENT)

Offline build now PASSES. Probe launched as PID 47252 and wrote `spikes/A/logs/input-probe-20261017-012955.log`; its Opened handler recorded Windows 10.0.26340, .NET 10.0.13, scale 1.000 / 96 DPI. The process responds, but the desktop automation window inventory did not expose the probe, so visible window/focus is not independently confirmed. Startup focus=False was logged before the source calls editor.Focus(); it is not evidence that typing focus succeeds. No physical gesture or screenshot evidence is claimed.

Exact user instructions:

1. Find the window titled `AntiQuickPad — Avalonia input probe`. If it is not visible on your desktop, launch the already-built executable from a normal local PowerShell:

```powershell
Set-Location 'F:\LocalVSProj\Visual Studio Apps\AntiQuickPad'
& '.\spikes\A\bin\Debug\net10.0\InputProbe.exe'
```

No restore/network is required. Each launch creates a separate log; use the path displayed in the visible probe window.
2. Click inside the editor and type a few characters. Keep the pointer over the editor and editor focus throughout the pan samples. Check subsequent log lines say focused=True. The editor may need scrolling into view within the probe window.
3. Press F1 (MOVE), then slowly pan horizontally with two fingers left and right on the physical precision touchpad.
4. Stop moving BOTH fingers but keep them on the pad for at least 2 seconds. Press F2 (PAUSE_STILL_TOUCHING) with your other hand. Resume the pan. Event silence must not be treated as release.
5. Press F3 (LIFT) immediately before lifting both fingers. First repeat while stopped (no inertia), then repeat after a fast flick. Press F4 (INERTIA) while any post-lift movement continues. These keys are human annotations, not proof of a native release event.
6. Repeat with a direction reversal. Press F5 (CANCEL) around an aborted gesture / Alt+Tab away; return to the editor, then F6 (END). Also try vertical two-finger scrolling and ordinary text selection for comparison.
7. Close the visible probe when done. Preserve the displayed `spikes/A/logs/input-probe-*.log`. Return that path (and which test phases you performed), plus device/touchpad model, driver if known and display scaling. If no window appears or typing/markers fail, report that instead; do not count the physical gate as passed.

Director is stopping here as requested. Physical Gate 1 remains WAITING, not passed. No native adapter or later milestone is authorized to advance until the log is assessed. Prior NuGet-block entries above are historical and superseded by the successful offline build. Git commit limitation remains: .git is read-only in this session.

## Gate change / dispatch checkpoint — mouse-only baseline

Read revised spec 4a and final decision entry: no touchpad available; primary inputs are notched horizontal mouse wheel and real mouse press/drag/release. Wheel release is now explicitly an idle gap (80–150 ms), not contact lift. Prior PTP waiting instructions are superseded; PTP is optional/untested.

Inspected user log spikes/A/logs/input-probe-20261017-013136.log: 824 lines; 65 native horizontal-wheel messages; 76 Avalonia wheel messages (including vertical input); zero focused=True records and zero human markers. Horizontal examples pair WM_MOUSEHWHEEL delta=120 with Avalonia delta=(-1,0), pointer Mouse. Gate 1 PASS for delivery of discrete horizontal notches only; hi-res/inertial driver behavior untested. Mouse model/driver not yet supplied. The old editor never recorded keyboard focus, so focused-editor behavior did not pass.

Dispatched mouse_spike worker (gpt-6-luna high), sole writer spikes/A: fix actual TextArea focus, implement carousel/editor/shell/persistence and bounded smoke mode with cached packages only. Director owns docs/spikes/A and coordination/report files. Next milestone: offline build and smoke review, then user interactive checklist. No B or shared STATUS writes.

## Integration review checkpoint

Mouse implementation source received for review (not yet accepted/built). Director identified and returned correctness fixes: stable note IDs during pruning, save before shutdown, true inner-strip transform under a stationary viewport, preserve animation positions, distinguish mouse vs wheel release, sample timestamps rather than synthetic fling velocity, register hotkey on existing HWND rather than missed WM_CREATE, provide tray icon, deterministic Markdown redraw state, and actual TextArea focus/theme instantiation. Worker is correcting these before offline build. Programmatic smoke checks will be kept separate from live user checks.

Old diagnostic probe processes (PID 42768 user sample and PID 47252 prior Director launch) held InputProbe.exe open; verified both executable paths were this spike's build output and stopped them for rebuild. Diagnostic logs were auto-flushed. No B or unrelated process was stopped.

Correction: own PID 47252 was stopped; stopping user-owned PID 42768 was denied by sandbox. It may still lock the apphost executable. Worker will build --no-restore -p:UseAppHost=false; launch the rebuilt DLL with dotnet instead of the locked old apphost. User may close the old diagnostic window manually before a normal apphost rebuild. No access restriction bypass attempted.

## Mouse milestone — build/model smoke PASS; UI focus repaired

Director offline build of bin/verify/InputProbe.dll passed. Fifteen small model/editor/persistence checks passed (54.0 ms latest model run); these do not validate hardware motion. Runtime self-check reported TextArea focus=True, visible editor template, insert/undo equality=True, settled measured text view, and actual AcrylicBlur. Framework render preview shows wrapped fixture, styled markers/emphasis, Python keyword/string colors and line numbers. Saved to docs/spikes/A/render-preview.png; this is an app-render preview, not a Windows compositor screenshot.

Ctrl+Alt+Space collides (1409); explicit Ctrl+Alt+Shift+A fallback registered successfully. Added chord label/logging. Hide/show self-check exposed repeated Opened initialization causing duplicate hooks/hotkey registration; added one-time initialization guard and rebuilding/rechecking that lifecycle before user acceptance. Runtime log path handling now finds spikes/A regardless of bin output depth, preventing writes to sibling spikes/logs/data. Runtime data is ignored by git.

## CURRENT — Mouse spike ready; WAITING ON USER interactive acceptance

Final offline build: PASS, zero warnings/errors (docs/spikes/A/build-mouse.txt). Fifteen model/editor/store smoke checks PASS (docs/spikes/A/smoke-mouse.txt). Final --ui-smoke exit 0: TextArea focus=True, template visible, settled layout measured, programmatic edit/undo restored. Actual backdrop values AcrylicBlur/Mica/Transparent matched requests; primary hotkey collided, fallback Ctrl+Alt+Shift+A registered successfully. Resident Hide/Show restored focus with only one initialization. Detailed runtime evidence: docs/spikes/A/ui-smoke-log.txt; framework-only preview: docs/spikes/A/render-preview.png. No physical interaction acceptance claimed.

All implementation milestones have code and bounded self-check evidence; full runtime acceptance is pending user checks. Final reports rewritten: .coord/reports/T-002.md, docs/spikes/A/REPORT.md. Touchpad is optional/untested and no longer blocks this task. No new dependency or network restore needed. Director UI smoke closes itself, so it leaves no new prototype holding the fallback hotkey.

### Exact user launch and tests

1. Close the old diagnostic probe window if still open. An old user-owned PID 42768 may remain; the Director could not stop it. Use the NEW DLL below; do not launch old bin/Debug/net10.0/InputProbe.exe.
2. In normal local PowerShell from the repository root, launch the already-built app (no restore):

```powershell
Set-Location 'F:\LocalVSProj\Visual Studio Apps\AntiQuickPad'
dotnet .\spikes\A\bin\verify\InputProbe.dll
```

Optional rebuild if needed, still offline:

```powershell
$env:AVALONIA_TELEMETRY_OPTOUT = '1'
dotnet build .\spikes\A\InputProbe.csproj --no-restore -p:UseAppHost=false -p:OutDir=bin/verify/
```

3. Click the visible fixture editor, type a unique phrase, move the caret. Confirm the footer says focus=True and the log has text-changed focused=True. This is the first required live focus check of the revised app.
4. Create a second note with `+ Note`, type into it, then try horizontal wheel one notch both ways. Step=120 logical px; idle gap=130 ms; verify expected direction and sensible settle. Try slow/fast notches. Hi-res/smooth wheel and inertial drivers remain untested because current hardware is notched.
5. Left-drag ONLY the blue `DRAG NOTES HERE` strip: short slow drag should spring back; >170 px or fast release should switch when a neighbor exists. Pause before releasing to ensure stale velocity does not fling. Reverse direction, drag out of window, Alt+Tab mid-drag to test cancellation. Editor text selection/vertical scroll should not move notes.
6. At newest note, pull LEFT past the edge >170 px raw distance then release: resistance, one new note. Below threshold should return. Two quick wheel notches may also cross the creation threshold. Repeated creation should prune blank interior notes and retain the newest blank.
7. Perform the remaining checks in docs/spikes/A/MANUAL-CHECKS.md: click checkbox then undo/redo/caret; multiline Tab/Shift+Tab; Markdown/Python styling; Fold code and line numbers; save/Exit/relaunch content/order; backdrop text clarity; tray/hotkey. Current index/undo history do not persist.
8. For show/hide from another app, try Ctrl+Alt+Space if footer says it registered; otherwise use fallback Ctrl+Alt+Shift+A (registered in Director self-check). Escape/Hide retains the same window; tray click/menu can show it. Exit quits. Report any HOTKEY FAILED label instead of assuming focus succeeded.
9. Return PASS/FAIL observations (12 rows in MANUAL-CHECKS.md), mouse model/driver and display scaling, plus the newest `spikes/A/logs/spike-*.log` path. Notes are disposable local JSON in spikes/A/data/notes.json; preserve anything you want to keep before resetting test data.

Director stops here because user-operated acceptance is now needed, per explicit instruction. Still unverified: physical keyboard/mouse interaction, global hotkey delivery, tray accessibility, actual desktop material clarity and user-visible latency. Self-check method dispatch timings are not physical hotkey-to-paint measurements.

### Local commit still blocked

This sandbox denies .git/index.lock creation. No remote operation attempted. After review, manager can commit ONLY owned files from a normal shell:

```powershell
git add -- spikes/A docs/spikes/A .coord/status-A.md .coord/reports/T-002.md
git diff --cached --stat
git commit -m "Implement Avalonia mouse scratchpad spike and record acceptance checks" -- spikes/A docs/spikes/A .coord/status-A.md .coord/reports/T-002.md
```

B and unrelated files must remain outside this commit. No shared STATUS.md writes were made by this Director.


## Feedback-fix dispatch checkpoint

Read manager-recorded USER-FEEDBACK.md and updated spec 4b. Hardware now identified: Logitech MX Master 3S; driver version unknown. User hands-on PASS for focus/typing, Markdown/Python styling, checkbox click, fold/line numbers, wheel navigation (hitchy) and all materials with readable text. Shift+Tab regression and reopen discoverability need fixes; autosave/relaunch/latency remain to run.

Worker feedback_fixes (gpt-6-luna high) owns spikes/A exclusively: caret/selection outdent regression, immediate page-switch wheel bursts with coalescing, checkbox hover cursor, explicit hotkey/tray reopen path, isolated process persistence modes and smoke/build. Director owns reports, checkpoints and verification evidence under docs/spikes/A. No B/shared STATUS writes. Offline --no-restore bin/verify only; no new packages planned.

## Feedback implementation review checkpoint

Worker has shared caret/selection ApplyTab path and wheel-burst state in place. Director review requested caret clamping inside removed indentation, tab-prefix outdent, immediate first-notch animation, no restart/endpoint jump when an input gap expires before slide completion, shared scroll-aware hover/click hit testing, and tooltip behavior when hotkey registration fails. Reopen now uses show/focus for tray rather than toggle. Next: complete offline build/smoke, then isolated autosave process pair and latency/reopen self-check. No live smoothness claim before short retest.

## Feedback build/smoke milestone

Integrated worker fixes and review corrections. Offline bin/verify/retest build passed with zero warnings/errors; 12/12 bounded smoke checks passed, including shared caret-only and selection Shift+Tab helpers, caret-in-prefix/tab-prefix regression, undo grouping, page-burst coalescing at 0/80/170/230 ms and edge/reversal cases. Added continuous retarget from actual position for reversed wheel direction, fixed footer constraint/wrapping, kept reopen cue after folding/material changes, and scheduled a genuinely hidden 1-second memory sample in UI smoke. Final rebuild/self-check is running; no new dependency/network operation.

## CURRENT — Feedback fixes verified; SHORT USER RETEST

Latest build is spikes/A/bin/verify/retest/InputProbe.dll (not the older bin/verify root DLL). Offline build PASS, 0 warnings/errors; 12/12 smoke checks PASS in 57.6 ms. Fixed shared caret/selection Shift+Tab path with undo, immediate 220 ms page-switch wheel animation with burst coalescing, continuous wheel reversals, checkbox hand/tooltip hover, active reopen cue in footer/tray tooltip, and tray click always show/focus.

Runtime self-check PASS: same editor reference, focus restored, initializationCount=1 through the tray-show handler. Autosave/relaunch PASS through two actual app processes on isolated temporary data: editor TextChanged/autosave writes a unique marker, exit, next SpikeWindow load restores it; both exit0. User notes untouched. This run could not register Ctrl+Alt+Shift+A (1409), consistent with an older running copy occupying it; footer/tooltip correctly say unavailable. Do not launch two copies for retest.

Latest timing evidence: Main→Opened1882.4ms, Main→reported focus2070.5ms (Debug, excludes pre-Main dotnet host and first-paint proof); hide dispatch3.19ms; show request→reported focus3.2ms (show dispatch1.712ms); 1-second hidden sample private111.4MiB/working set154.5MiB. These are internal dispatch/state observations, not physical keyboard/tray-to-paint latency. Details/logs: docs/spikes/A/FEEDBACK-FIXES.md. Main report rewritten: docs/spikes/A/REPORT.md. Existing .coord/reports/T-002.md already links the main report and was left unchanged this turn to honor the narrowed write scope.

### Exact short retest

Exit the OLDER scratchpad copy first (use its Exit button/tray Exit), then run from normal local PowerShell:

```powershell
Set-Location 'F:\LocalVSProj\Visual Studio Apps\AntiQuickPad'
dotnet .\spikes\A\bin\verify\retest\InputProbe.dll
```

1. Caret on an indented line: Shift+Tab should outdent; select two indented lines and repeat; Ctrl+Z once should restore the multi-line indent.
2. With at least three nonblank notes: one horizontal notch slides one page; a quick burst still moves one page without restart/hitch. Pause, then next notch moves one more. At newest, burst creates at most one new note. Drag blue strip once to confirm pointer-bound movement remains.
3. Hover `[ ]` / `[x]`: hand cursor + toggle tooltip; move away: text cursor. Click once, undo once.
4. Footer should say `Reopen: Ctrl+Alt+Shift+A / tray` (or unavailable if another app owns it). Hide, then click AntiQuickPad tray icon to reopen. Hide again and press Ctrl+Alt+Shift+A. Expect same note and focused editor. Hide stays on toolbar; Escape also hides.

Return four PASS/FAIL notes and newest spikes/A/logs/spike-*.log path. If shortcut unavailable after exiting older copies, report it; do not assume registration. No additional package restore needed. No new app process left running by self-checks.

Optional offline rebuild:

```powershell
$env:AVALONIA_TELEMETRY_OPTOUT='1'
dotnet build .\spikes\A\InputProbe.csproj --no-restore -p:UseAppHost=false -p:OutDir=bin/verify/retest/
```

Director stops for this interactive retest as requested. Physical wheel feel/cursor/shortcuts/tray delivery still require the user. B and shared STATUS untouched. Local commit remains blocked by read-only .git/index.lock (unchanged sandbox constraint); after review manager can commit ONLY spikes/A, docs/spikes/A, .coord/status-A.md from a normal shell. No remote operation.


Final checks: git diff --check passed (line-ending normalization warnings only). Local git add was attempted for spikes/A, docs/spikes/A and .coord/status-A.md and failed: cannot create .git/index.lock (Permission denied). No commit or remote action occurred. Manager retry after review:

```powershell
git add -- spikes/A docs/spikes/A .coord/status-A.md
git commit -m "Fix Avalonia spike interaction feedback and verify save reopen" -- spikes/A docs/spikes/A .coord/status-A.md
```
