# M9b-resident-controller (M9-function-contract-separation, part b)

Director: Codex gpt-6.1-sol medium. Do it yourself; a Luna subagent only for a clearly separable read/check.
Branch: feat/function-contract-separation (the manager handles ALL git; you run no git commands that write).
Naming: always `<ID>-<slug>`, never a bare ID. Finish in ONE turn (90 min wall limit). End when the report is written, all four sub-steps are done, or a gate fails twice on a sub-step.

## Goal
Move resident behaviour out of `Views/Common/ResidentShellWindow*.cs` into a shell-agnostic controller, so a later UI task can replace layouts, palettes and the window look without touching behaviour. Shells (and the resident window) only place controls, apply looks and bind commands. **Observable behaviour must not change**: same timings (350 ms debounce, 130 ms focus cue), same event order, same log lines, same save-failure handling, same focus path, same hotkey/tray/second-launch behaviour.

## Read
AGENTS.md, docs/function-contract.md (contract + "Guarantees a shell must not break"), docs/architecture.md, `.coord/reports/M9-baseline.md`, `.coord/reports/M9d-conformance-check.md`, all of `src/AntiQuickPad/Views/Common/`, `Views/IShellView.cs`, `Contract/FunctionContract.cs`, `ViewModels/MainViewModel.cs`, `Shell/PlatformServices.cs`, `Shell/HiddenWindowPlatform.cs`, `Measurement.cs` (only how it calls the shell), `Program.cs`, `App.cs`.

## Write only
- New folder `src/AntiQuickPad/Controller/` (namespace `AntiQuickPad.Controller`).
- `src/AntiQuickPad/Views/Common/*.cs`, `Views/IShellView.cs`, `App.cs` (composition only).
- `.coord/reports/M9b-resident-controller.md`, snapshots under `.coord/scratch/M9b/`.
NOT: Core, ViewModels, Views/Quiet, Views/Card, Views/Classic (Classic keeps its own lifecycle; out of scope), Editor, Carousel, Design, Shell/*Platform* implementations, ShellPalette/Look tokens, docs/. No git, no network, never kill processes.

## Design rules
- The controller may use Avalonia threading (DispatcherTimer, Dispatcher), the AvaloniaEdit `TextEditor` handed to it, `IWindowPlatform`, and `ShellServices`. It must NOT reference layouts, `ShellLayoutContext`, `ShellPalette`, `Look`, brushes, colours, or any control other than the editor.
- The window talks to the controller, and the controller talks back through ONE small interface (for example `IResidentSurface`) implemented by `ResidentShellWindow`. It covers: the current editor, the carousel cancel, setting the focus cue on/off (the window decides the brush), closing the menu, refreshing hint/status, saving window settings, IsVisible/IsActive/IsKeyboardFocusWithin, and logging. Keep it minimal and documented with one-line comments.
- Shells keep calling the same ViewModel commands; menu/toolbar wiring stays where it is unless a sub-step moves the command mapping (b4).
- Keep the code style of the surrounding files (dense, short members, few comments).

## Sub-steps (in order; each ends with the gate and a snapshot)
- **b1 autosave**: the 350 ms debounce timer, ScheduleSave, SaveNotes (including the window-snapshot save and save-failure status/logging) move to the controller.
- **b2 navigation**: the Navigating/Navigated protocol (cancel motion, save, remember scroll, failed save cancels; after: close menu, drop the deleted note's scroll, bind current, save except after delete, focus), the per-note scroll memory, and BindCurrent (text, caret, clear undo, restore scroll) move to the controller. The carousel keeps getting scroll offsets through the same delegate.
- **b3 show/hide and focus**: Toggle, ShowResident, HideResident, FocusEditor, CompleteFocus (including Measurement events/foreground check and `_focusCompletion` used by the selftest), the 130 ms focus cue timer, and Quit (save-failure abort, command source, shutdown) move to the controller. `IShellView` members stay on the window as thin forwards so Program/Measurement/tray callers are unchanged.
- **b4 lifecycle and command map**: hotkey and tray installation, Opened/Activated/Deactivated/Closing/Closed handling, the Esc / Ctrl+N / Ctrl+Shift+Delete key map, and `CommandFor(label)` (contract ID -> ICommand) move to the controller. The window keeps layout/menu building and asks the controller for commands. Re-point `LiveRoutes(id)` (the conformance resolver from part d) to read routes through the controller; the check itself must not need rewriting.

## Gate after EVERY sub-step (all must pass)
From src/AntiQuickPad: `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit` (never kill); `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build -c Release --no-restore -p:UseSharedCompilation=false` = 0 warnings / 0 errors; `--selftest` (env `ANTIQUICKPAD_ALIGNMENT_OUTPUT` = `.coord/scratch/align`) exit 0, with `CONFORMANCE quiet PASS`, `CONFORMANCE card PASS`, and selftest show-to-focus < 100 ms. Also diff the event subscriptions: before you start, save `grep -n "+=" src/AntiQuickPad/Views/Common/*.cs` to `.coord/scratch/M9b/subscriptions-before.txt`. After each sub-step, list every subscription that moved and where it went, so nothing was dropped (Activated, Deactivated and Closing are not covered by the selftest). Do NOT run `--measure`; the manager runs it on the desktop.
- Gate passes: copy `src/AntiQuickPad` (excluding bin/ and obj/) to `.coord/scratch/M9b/b<N>/` and write `.coord/scratch/M9b/b<N>.done` with a one-line summary.
- Gate fails: up to two fix attempts. If it still fails, restore the sources from the previous snapshot (or from the files as they were when you started, for b1), write `.coord/scratch/M9b/b<N>.failed` with the reason, and STOP. Do not start the next sub-step.

## Report `.coord/reports/M9b-resident-controller.md`
Per sub-step: what moved (members, subscriptions), the interface surface added, gate output (warnings, selftest result, conformance lines, show-to-focus numbers), any behaviour risk you could not prove by test (for example Activated -> focus, Deactivated -> cancel, Closing -> hide), and a list for the user's physical check.
