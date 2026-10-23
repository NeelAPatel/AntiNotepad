# M9b-resident-controller

Completed all four ordered extractions. New shell-agnostic `AntiQuickPad.Controller.ResidentController` partials own debounce, navigation/scroll binding, resident focus/show/hide/quit, lifecycle policy, service ownership, host key handling, and label -> contract ID -> ICommand mapping. One internal `IResidentSurface` is implemented by the resident window. It exposes no window, layout, palette, brush or other control; its only control type is the current editor.

Source changes: new `Controller/` folder plus `Views/Common/ResidentShellWindow.cs`, `.Appearance.cs`, `.Contract.cs`, `.Smoke.cs`. No Core, ViewModel, Quiet/Card/Classic layout, Editor, Carousel, platform implementation, palette/token, docs, Program, App or IShellView edits. No git commands, network, process kills, desktop control, or `--measure`. The explicit write-only scope leaves STATUS and git to the manager; milestone evidence is in this report and the permitted scratch directory.

The existing `FunctionContract.Check` and its smoke invocation are unchanged. The smoke's focus completion source now lives in the controller. Program/Measurement callers still reach the same IShellView members through thin forwards.

Snapshots and gate commands

- Initial sources: `.coord/scratch/M9b/before/`.
- Each successful extraction: `.coord/scratch/M9b/b1/` through `b4/`, each a full `src/AntiQuickPad` copy excluding `bin/` and `obj/`, with sibling `.done` markers.
- Every gate ran from `src/AntiQuickPad`: `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit`; telemetry-opted-out `dotnet build -c Release --no-restore -p:UseSharedCompilation=false`; then the Release DLL with `--selftest` and ANTIQUICKPAD_ALIGNMENT_OUTPUT set to the absolute repository `.coord/scratch/align` path.
- Raw output: `.coord/scratch/M9b/b<N>-quit.log`, `-build.log`, `-selftest.log`. Source audit: `source-diff.txt`. Subscription evidence: `subscriptions-before.txt`, each `b<N>-subscriptions.txt` and `b<N>-subscriptions-moved.txt`.

Gate results

| Extraction | Quit | Release build | Selftest | Quiet focus ms | Card focus ms | Retained Classic focus ms |
| --- | --- | --- | --- | --- | --- | --- |
| b1 | exit 0 | 0 warnings / 0 errors | exit 0, PASS | 2.160, 1.662, 1.764 | 2.248, 2.796, 2.648 | 1.688, 0.915, 1.271 |
| b2 | exit 0 | 0 warnings / 0 errors | exit 0, PASS | 2.152, 2.783, 1.667 | 2.568, 2.784, 1.990 | 1.613, 1.181, 0.828 |
| b3 | exit 0 | 0 warnings / 0 errors | exit 0, PASS | 3.855, 2.151, 2.207 | 2.028, 2.836, 2.370 | 1.729, 1.189, 1.076 |
| b4 | exit 0 | 0 warnings / 0 errors | exit 0, PASS | 2.498, 1.572, 1.715 | 4.091, 1.752, 1.640 | 1.780, 1.158, 0.839 |

Every gate emitted both `CONFORMANCE quiet PASS (24 commands)` and `CONFORMANCE card PASS (24 commands)`, and `SELFTEST PASS: Quiet + Card production shells and retained Classic capability suite.` The existing `window.move` KNOWN-GAP remains unchanged in both shells. Hidden-window assertions passed, with no test-owned activation/foreground events. The captured foreground values were 0x0; these hidden tests establish internal focus timing, not real desktop foreground acquisition or physical latency.

The lifecycle extraction's initial build failed with CS0103 for `WindowDecorationsElementRole` in the visual route adapter. Fix attempt 1 restored its `Avalonia.Input` import. The complete gate then passed; no second fix or rollback was needed. Original failure evidence: `b4-initial-build-failed.log`.

M9b-resident-controller / b1 autosave

- Moved the 350 ms DispatcherTimer, ScheduleSave and SaveNotes into `ResidentController.cs`. The controller updates text/caret, calls VM Save followed by the surface window-settings save, clears/records the last error, refreshes status and emits the same `save-error` log. Window geometry/settings capture remains a surface adapter, as required by the interface boundary.
- Initial interface: current Editor, RefreshStatus, SaveWindowSettings and Log, each documented with a one-line comment. The controller's Binding flag suppresses writes during editor construction/binding; the window temporarily forwards it during appearance rebuilding.
- Subscription moved: `_autosave.Tick` -> `InitializeAutosave`, called at the original constructor position. All editor and other subscriptions remained in place at this extraction.
- Gate: successful row above; snapshot `b1/` and marker `b1.done`.
- Limits: the existing 500 ms autosave smoke proves successful debounce persistence, not the exact 350 ms boundary under desktop load. No new resident-specific disk-failure injection was added. Failed-save status/log/save-order preservation was reviewed against the initial sources.

M9b-resident-controller / b2 navigation

- Moved Navigating/Navigated, the departing note ID, per-note scroll dictionary, RememberScroll and BindCurrent into `ResidentController.Navigation.cs`. Carousel receives the same index -> stored-offset delegate via `ScrollFor`.
- Before navigation: cancel motion; SaveNotes captures text/caret and saves; assign cancellation on failure; remember scroll/departing ID only on success. After navigation: close menu; discard deleted note scroll; bind text/caret, clear undo and restore scroll; save unless deleting; focus. This retains actual original order, including closing the menu before binding.
- Interface additions: CancelMotion, CloseMenu, RefreshHint, and a temporary FocusEditor adapter. The last was removed in the next extraction when focus became controller-owned.
- Subscriptions moved: VM Navigating and Navigated -> `InitializeNavigation`, at their original position before the other VM subscriptions and initial BindCurrent.
- Gate: successful row above; snapshot `b2/` and marker `b2.done`.
- Limits: navigation, delete, hint, carousel and appearance-preserved document/caret smoke passed. Exhaustive per-note scroll restoration after long-note wheel/drag navigation and resident failed-save cancellation remain physical/fault-check items.

M9b-resident-controller / b3 show/hide and focus

- Moved Toggle, ShowResident, HideResident, FocusEditor, CompleteFocus, Quit, show timestamps/sources, quit state/source, last show-to-focus value, focus completion source and the 130 ms cue timer into `ResidentController.Residency.cs`.
- Preserved platform Show -> Activate -> Measurement frame -> cue restart -> immediate focus -> Input-priority posted focus. CompleteFocus keeps visibility/editor focus and interactive activation/native-foreground guards, the same Measurement events and log text. Hide still cancels motion and aborts on a failed save. Quit preserves the command-source distinction, failure log/status, successful Close and explicit desktop-lifetime shutdown.
- Interface additions: visibility/activation/editor-focus/foreground observations, RecordFrame, SetFocusCue(active) and Close. Only the window chooses cue brushes. The window's IShellView operations and LastShowToFocusMs are forwards. The smoke directly awaits `controller.FocusCompletion`; its timing assertions are unchanged.
- Subscription moved: cue Tick -> InitializeCue at its original registration position. GotFocus still reached CompleteFocus through a window forward at this extraction; lifecycle window subscriptions remained pending the final extraction.
- Gate: successful row above; snapshot `b3/` and marker `b3.done`.
- Limits: internal show/focus and close/hide smoke passed. The 130 ms interval is preserved in source but visible cue duration, physical foreground focus, activation races and save-failure presentation require desktop verification.

M9b-resident-controller / b4 lifecycle and command map

- Moved hotkey/tray creation, callbacks, logs and disposal; Opened/Activated/Deactivated/Closing/Closed policy; tunnel Esc/Ctrl+N/Ctrl+Shift+Delete map; operation/status/pin VM handling; and editor event wiring into `ResidentController.Lifecycle.cs`. Test/render service suppression is preserved; the host supplies only a rendering flag and native-host adapters.
- Native event subscriptions remain on the Window as thin forwards in the original order. OnActivated still posts focus at Input priority. OnClosing sets Cancel before HideResident unless quitting. OnDeactivated cancels motion. OnClosed preserves motion cancellation -> autosave/cue stop -> hotkey/tray disposal -> theme disposal -> conditional quit-finish log -> log disposal.
- `ResidentController.Commands.cs` maps existing toolbar/menu labels through contract IDs to the same VM commands or controller actions. The controller now orchestrates shell-switch and theme-save behavior, menu toggle/focus and pin persistence; the surface applies visual rebuilds/theme/minimize/menu focus. Menu/toolbar layout and styling are unchanged.
- `LiveRoutes(id)` now forwards to the controller. Host keys, service capabilities, system routes and debounce read controller state. Toolbar/menu/visual gesture inspection stays behind HasPresentationRoute, preserving the original checks without bringing controls/layouts into the controller. The conformance check and negative probe were not rewritten; the negative probe was not rerun in this task.
- Interface additions: native-host hotkey factory, measurement attachment, retained smoke callback, separate appearance/log disposal, pin/minimize/menu presentation, shell ID/application, theme application and visual route inspection. These are the necessary one-line adapters to existing window-bound APIs; the controller never receives the native window, shell layout or palette.
- Subscriptions moved: OperationFailed, VM PropertyChanged, Status.PropertyChanged -> InitializeViewModel; editor TextChanged, caret PositionChanged, TextArea GotFocus and editor PointerPressed -> AttachEditor at their original build position. Window Opened/Activated/Closing/Deactivated/Closed and tunnel KeyDown now target controller handlers. SizeChanged/PositionChanged remain geometry capture. Theme change, pin binding, carousel hint, layout bar and menu presentation subscriptions remain on their existing surfaces.
- Subscription audit: 31 `+=` occurrences before; 31 across Views/Common plus Controller after. Every moved subscription and retained adapter is listed in the per-step subscription files; source comparison was inspected as well as counts.
- Gate: corrected on fix attempt 1, then successful row above; snapshot `b4/` and marker `b4.done`.
- Limits: selftest intentionally skips actual hotkey/tray installation and does not establish physical Activated/Deactivated behavior. Closing -> hide has a scripted smoke; real close, foreground transfer and service callback delivery remain desktop checks. The window retains bounds/settings capture, native event adapters, visual rebuilding and menu navigation/presentation.

Physical checks for the user/manager

- Run the manager's desktop `--measure` comparison against M9-baseline: median visible-and-focused <=27 ms and p95 <=30 ms, with the baseline's rerun rule. No new desktop numbers are claimed here.
- In both Quiet and Card, show/hide repeatedly with the physical global hotkey; confirm immediate typing focus and the brief focus cue. Switch away/back to exercise Activated -> posted editor focus.
- Exercise tray toggle, icon click/show and quit; second launch/show; command `--quit` with pending edits. Confirm persistence and existing quit-start/quit-finish logs.
- During carousel wheel/drag motion, deactivate the window and confirm cancellation. Use Esc with the menu open (closes/focuses editor), then Esc without the menu (saves/hides). Use native Close and confirm hide rather than exit.
- Exercise Ctrl+N and Ctrl+Shift+Delete; navigate among long notes with distinct caret/scroll positions; delete a scrolled note. Confirm note selection, restored offsets, cleared undo after navigation and no stale scroll after deletion.
- Switch shell/theme while editing; confirm document/undo, caret and scroll survive. Check pin persistence, minimize, window bounds and relaunch restore.
- With an isolated disposable data directory, force a save failure: verify autosave error/status/log, navigation cancellation, hide staying visible, tray/menu quit abort and show, and command quit preserving the existing no-forced-show behavior. Remove the fault and confirm saving/quit recovers. Resident-specific failures were reviewed in source, not newly exercised by this hidden suite.
