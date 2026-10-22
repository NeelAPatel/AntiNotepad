# F54-measure-heavy-index-crash

Done 2026-10-21 by the Claude manager (user: prioritise Claude usage today). Function task.

## Cause
`MainViewModel.PopulateMeasurementFixture` created up to 19 notes on the session, then selected and updated each, and refreshed the VM's projected note list (`_notes`) only at the end. `NotesSession.UpdateCurrent` raises `SaveStateChanged(Pending)` synchronously; the VM turns that into `Status` changes and `SaveLineText` notifications; bound readers (the save line, the empty hint) read `IsCurrentEmpty -> Current -> _notes[CurrentIndex]` with the session index (up to 19) against the stale one-item list: `ArgumentOutOfRangeException`. The exception escaped the measure control tick, so `heavy` was never acked; the VM was then left with a stale list, so a later wheel event crashed the child (manager repro).

Normal paths are not affected: `Navigate` (create/delete/select) refreshes before raising navigation events, and the only notification raised inside a session action (`DeleteCurrent -> Save`) happens when the stale list is longer than the session's, so the index stays in range.

## Fix
- `PopulateMeasurementFixture` refreshes the VM list right after creating the fixture notes, before the select/update loop.
- Measure harness: a control command that throws now logs an `error` event (command, exception type, message) instead of killing the tick silently; the parent reports that error as the gap. The `heavy` ack wait is 30 s (was the shared 3 s).

## Proof
- New smoke in `ViewModels/ProjectionSmoke.cs` (runs in `--selftest`): a fresh VM whose PropertyChanged/Status handlers read `SaveLineText`, `Current`, `PositionText` and `PagerDots` on every notification, populates the 20 x 200 fixture, then Previous / Select(0) / Next; asserts position, content and navigation.
- Fail before fix: `SELFTEST FAIL: System.ArgumentOutOfRangeException` (`.coord/scratch/F54/selftest-before-fix.log`).
- Pass after fix: `F54-measure-heavy-index-crash PASS` (`.coord/scratch/F54/selftest.log`).

## Gate
Release build 0 warnings / 0 errors; `--selftest` exit 0: BOUNDARY PASS (258 types), CONFORMANCE quiet PASS (23), REVEAL PASS, PRESENTATION PASS (both themes), alignment rasters PASS, unchanged expected values. No view, layout or token change. `--measure` not re-run yet (needs user OK).
