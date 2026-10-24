# A14-swipe-close-cancel

## Changed

- `product/AntiNotepad/UI/NoteSwipe.cs`
- `product/AntiNotepad/UI/MotionPreview.cs`
- `product/AntiNotepad/SelfTest.cs`
- `.coord/reports/A14-swipe-close-cancel.md`

## Finding and fix

The progress settle timer kept running while owed wheel motion waited for frames. A Settings preview released during that wait had no explicit swipe teardown. `NoteSwipe.Cancel()` now stops both settle timers and any spring-back animation, clears pending motion, invalidates queued frame callbacks, and resets an attached pane. Queued callbacks use weak references so they cannot keep a released swipe or pane alive. The preview cancels on close and visual detachment.

The progress settle tick now drops motion that has not changed for 1 second and settles the pane. Normal swipe thresholds, rubber banding, and animation timings are unchanged.

## Selftest checks

- A progress swipe with owed motion and no attached frames has an active timer; `Cancel()` stops it and clears pending progress.
- A supplied-clock settle tick waits while motion is recent, then stops the timer and drops stalled motion at the 1-second bound.

## Verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a14-check -p:NuGetAudit=false`: 0 warnings, 0 errors. NuGet audit was disabled because the default audit request could not reach `api.nuget.org` and emitted `NU1900` warnings.
- `build/a14-check/AntiNotepad.exe --selftest`: `SELFTEST PASS`.

## Needs a live check

- Exercise a progress swipe in the Settings preview, then close Settings while it is paying out. Confirm the pane resets and that normal swipe feel remains unchanged. The GUI was not launched for this worker task.

## Not done

- No live UI check was performed.
- The automatic policy blocked recursive deletion of `build/a14-check`; the verified task output folder remains.
