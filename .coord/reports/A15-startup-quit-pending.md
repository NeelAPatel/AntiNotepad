# A15-startup-quit-pending

## Cause

The lifetime listener receives show and quit signals on a background thread. Before `Program.Attach` assigned `_window`, `RequestQuit` did nothing. The existing early-show flag also had a check-then-set race with attachment, so an early show could be left pending after the window had already checked the flag.

## Fixes

- Added a lock-protected startup request latch for both show and quit. Requests received before the window is attached are held and dispatched once from `Program.Attach`; requests received after attachment are dispatched immediately.
- Both dispatch paths use `Dispatcher.UIThread.Post`. Quit continues through `PadWindow.Quit`, which flushes editor state and attempts the store save before shutdown. A failed save still opens the existing retry / quit-anyway prompt.
- `SingleInstanceService.RequestQuit` already waits for the primary process to exit before returning success. Confirmation is therefore not sent early; no protocol change was needed in `Platform/SingleInstance.cs`.
- Early show had the same startup issue and is fixed through the same latch.

## Files changed

- `product/AntiNotepad/Program.cs`
- `product/AntiNotepad/SelfTest.cs`
- `.coord/reports/A15-startup-quit-pending.md` (this report)

## Selftest

- Added `StartupRequestLatchChecks`: verifies early show and quit remain pending, are dispatched once when attached, and requests after attachment dispatch directly.

## Build and selftest

- The requested Release build completed with 0 errors but emitted two `NU1900` warnings because the sandbox could not reach nuget.org for vulnerability metadata.
- Release build with `NuGetAudit=false`: 0 warnings, 0 errors.
- `build/a15-check/AntiNotepad.exe --selftest`: `SELFTEST PASS`.
- `git diff --check`: clean.
- Cleanup of `build/a15-check` was attempted after verification, but the shell policy rejected recursive removal; the folder remains.

## needs a live check

- Confirm a startup-time quit request is posted to the UI thread after window/lifetime setup, runs the normal save path, and only reports success after the app exits.
- Confirm a startup-time show request makes the pad visible.
- Confirm the save-failure prompt remains reachable when the latched quit's save fails.

These checks were not run; no app was launched and no second process was started.

## Not done

- Live UI, save-failure interaction, and second-process behavior remain unverified.
