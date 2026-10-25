# A30-background-save-and-log

Implemented in the supplied worktree on `feat/A30-background-save-and-log`. No branch switch, merge, commit, STATUS edit, or audit edit. Build and capability checks pass. The requested few-ms shutdown target is not established: the synthetic latest-edit Save core measured 10–12 ms median here; live session-end delivery and latency remain unverified.

## Split and files changed

- Luna xhigh `store_writer`: `product/AntiNotepad/Model/Store.cs` — snapshots, serialized writer, coalescing, flush and recovery/move synchronization.
- Luna xhigh `log_and_checks`: `product/AntiNotepad/Program.cs` — background logging, exit/crash flush, and folder-move save gate; `product/AntiNotepad/SelfTest.cs` — one method appended at the end and one call in Run; temporary measurement harness.
- Sol director: `product/AntiNotepad/UI/PadWindow.cs` — save call sites, queued-save completion/error handling and exit flushes; reviewed worker changes, built and ran selftest, and wrote this report.
- This report is the only retained file outside the listed product scope. `Program.MoveNotes` has a save gate because copying/renaming a notes folder must happen after the writer finishes. No dependencies were added.

## Design as built

`Store.QueueSave()` captures metadata, a copy of Settings (including its mutable language list), and dirty bodies already in memory on the caller/UI thread. It does not load clean or unavailable bodies or serialize JSON on that thread. Metadata is independent of the live model; body strings are immutable. The writer never invokes an editor delegate or reads live note state.

One background writer owns disk saves in sequence. A pending batch keeps the latest snapshot and shares its completion task across superseded requests. The coalescing window is 50 ms; an active write finishes before the next batch writes. Dirty body snapshots are retained for retry, and successful completions clear Dirty only when the note's edit revision still matches. The acknowledgment also restores the title/mode caches computed by the worker, so a later metadata-only save preserves them. Caller-context completions acknowledge saves; synchronous Save/Flush also apply acknowledgments without waiting for UI callbacks.

`Store.Save()` captures the current state, forces the pending batch immediately, waits for its raw writer completion and returns success/failure. `Store.Flush()` forces and drains pending writes without capturing another snapshot. Neither waits for the coalescing timer. The index/body file names, JSON model, temporary-file replacement, index backup and recovery rules are preserved. Unavailable stores now return failure from Save rather than an ambiguous successful no-op, so Quit remains blocked. Unavailable note bodies remain unwritten.

Autosave, deactivation and hide queue work. Hide continues immediately after snapshot capture and does not wait for storage. Their asynchronous UI-context completion shows the existing failure toast; successful completion permits document eviction. Quit and Retry retain the synchronous Save gate and the existing “Couldn't save your notes” prompt with Retry / Quit anyway. An ordinary quit cannot exit while its save is pending or failed; explicit Quit anyway still drains in-flight work before discarding the failure.

The session-end path stops timers, flushes both panes, captures the latest edit through Save and drains the writer before returning to Avalonia. It retains A26's no-prompt, accept-session-end behavior, including on failure. Its status line is queued and the log is flushed before the handler returns. No deliberate debounce wait is added to shutdown. Filesystem stalls still have no hard millisecond ceiling; actual Windows delivery and end-to-end latency need the live check below.

Settings retains its synchronous pane/save step. `Program.MoveNotes` also calls Save and aborts on failure before verified copying and backup/rename begin. This runs synchronously on the UI thread, so no autosave can enqueue during the move. `SwitchFolder`, `Load`, and `RemoveOrphans` drain first; a failed drain prevents folder switching or a recovery sweep. The existing pointer, verification, old-folder backup and recovered-file behavior is unchanged.

Logging uses a separate ordered background queue. Log captures the timestamp and queues a line; directory creation, 64 KB rotation and append happen on its one consumer. Flush is an ordered barrier and waits until prior appends have been attempted. Logging remains best effort, as before. Flushes cover normal quit, session end, crash handlers, process exit and lifetime disposal. A flush after disposal waits for consumer completion rather than enqueuing an unreachable barrier.

## Measurements

A standalone Release .NET console harness copied baseline Store/Modes from worktree HEAD and the implemented Store/logger into separate namespaces. It used disposable fixture folders under `build/A30-measure`, with no GUI or real data. Warmup preceded nine repetitions. Note bodies contained 1,000 lines (roughly 68 KB); assignment/text construction was outside the measured save call. These are calling-thread measurements of the autosave/log work, **not a live Avalonia UI measurement**. Pane flushing, editor string extraction/copy costs, UI scheduling, and rendered latency were not measured.

| Operation | Before median / p95 | After median / p95 |
| --- | --- | --- |
| One 1,000-line autosave, caller time | Save: 18.046 / 20.338 ms | QueueSave: 0.060 / 0.090 ms |
| One log call, amortized over 1,000 calls | 0.075959 / 0.093028 ms | 0.000406 / 0.000865 ms |

The log measurement used the original append algorithm versus a warmed `Program.Log`-equivalent path/lock/logger wrapper; its per-run 1,000-call totals were 75.959 / 93.028 ms before and 0.406 / 0.865 ms after. The extracted logger's Write loop alone measured 0.376 / 0.716 ms. Per-call numbers are amortized, not individual-event percentiles; p95 across nine repetitions is the largest sample.

Disk time moves to the writer/flush: immediately flushing a queued 1,000-line save took 17.147 / 19.042 ms median/p95. Flushing the 1,000 queued log lines took 78.006 / 96.400 ms. These costs are excluded from the after caller times, deliberately, because autosave/hide/log calls no longer wait for them.

For the shutdown/quit core, the harness queued a live-note snapshot, changed its text/revision immediately, and measured synchronous Save of that latest edit. One line measured 10.508 / 11.258 ms; 1,000 lines measured 12.415 / 14.121 ms. This bypasses the 50 ms coalescing window but does **not** meet a few-ms target in this environment. It excludes the session-end log flush and Avalonia/Windows work. No hard deadline is imposed on disk writes, which must finish before replying. A live dev-copy shutdown measurement is still required. Measurements preceded the final acknowledgment-only title/mode cache preservation fix; the caller snapshot and disk-writing algorithms measured are unchanged.

## Verification

Final required commands:

```text
AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a30-check
Build succeeded. 0 Warning(s), 0 Error(s). Time Elapsed 00:00:04.22
build/a30-check/AntiNotepad.exe --selftest
SELFTEST PASS (exit 0)
git diff --check
exit 0
```

The one new `BackgroundSaveAndLogChecks` method uses temporary folders only. It checks:

- Forty queued snapshots share a coalesced task; disk holds the last snapshot and the index backup remains the baseline revision.
- Flush persists queued work before returning.
- Synchronous Save replaces a pending snapshot with the latest live-editor text/revision.
- A mutable Settings language list changed after queueing cannot alter the captured snapshot.
- An index path obstructed by a directory returns failure through QueueSave, Flush and the exact synchronous Save predicate used by Quit. Removing the obstruction permits Retry and retains the note text.
- Sixty-four log entries retain their captured timestamps and order after Flush.

It uses no reflection or test-only product hooks. The headless failure check exercises the Quit save predicate; it does not instantiate the prompt or prove actual process/IPC behavior. The existing unavailable-body, backup fallback, recovery and session-close checks also passed in the full selftest.

Automatic approval review rejected both recursive cleanup commands as “blocked by policy,” before execution. `build/a30-check` and the standalone source/binaries in `build/A30-measure` remain for manager cleanup; no alternate deletion route was attempted. No harness code was added to the product project.

## needs a live check

- In the dev copy, type and hide immediately; verify hide latency and the latest note after reopening/restarting.
- Simulate A26's shutdown query after an immediate edit and while autosave is pending. Verify reply acceptance, latest text, log completion and actual elapsed time. Then verify restart/sign-out if desired.
- Make the dev notes destination unwritable, try tray/hotkey/`--quit`, and verify the prompt keeps the process alive. Restore access and Retry; separately verify explicit Quit anyway.
- Move notes while a queued autosave exists; verify the active pointer/new folder and retained old-folder backup, and that subsequent edits save in the new location.
- Verify backup-index recovery and unavailable-body behavior with dev fixtures, plus UI error-toast/prompt appearance and focus.

These are unverified live behaviors. No desktop/computer-use testing was performed. AntiNotepad was invoked only with `--selftest`; no installed process was launched/stopped, and no real app-home, installed-copy or Documents data was accessed. The installer/second-process quit protocol was not run.

## Rework

A30-save-unavailable-fix (2026-10-25): Sol director delegated implementation to a gpt-6-luna xhigh subagent, reviewed the result and ran verification. This section supersedes the unavailable-save and recovery-blocking behavior described above.

- Unavailable stores now return success from QueueSave, Save and Flush with LastError set, without writing files or clearing Dirty. This covers both a forced-unavailable store and a missing folder. Save and Flush still drain outstanding writer work; the writer also treats an unavailable destination as a successful no-op with no durable-body acknowledgments.
- Load, RemoveOrphans and SwitchFolder drain the writer without treating a previous failed save as a veto. Switching folders resets the previous save status for the new destination. Real write failures on available stores still return false, preserving the existing toast, quit gate and Retry behavior. Program.MoveNotes retains its pre-move Save gate unchanged; no UI changes were needed.
- Rework changes only Model/Store.cs and SelfTest.cs, plus this report. The existing Program.cs and UI/PadWindow.cs changes from A30-background-save-and-log remain intact. No STATUS/audit changes, branch operations, merges or commits were made.
- Extended the existing BackgroundSaveAndLogChecks method: forced-unavailable and missing-folder QueueSave/Save/Flush succeed with LastError, write nothing and retain Dirty; after a real failed write, SwitchFolder to a fresh temporary folder succeeds and the original dirty text is saved there. The existing real-failure and Retry checks still pass.

Verification: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a30-check` succeeded with 0 warnings and 0 errors (4.64 seconds). `build/a30-check/AntiNotepad.exe --selftest` printed `SELFTEST PASS`; `git diff --check` passed.

Automatic approval review rejected the native PowerShell recursive cleanup of build/a30-check and build/A30-measure as "blocked by policy" before execution. Both directories remain for manager cleanup; no alternate deletion route was attempted. No GUI was launched or process killed, and no real app-home, installed-copy or Documents data was touched. Live unavailable-folder quit/install and Settings recovery behavior still needs a live check.

## Rework 2

A30-folder-returns-fix (2026-10-25): updated `BackgroundSaveAndLogChecks` to cover a transiently missing notes folder. `QueueSave`, `Save`, and `Flush` succeed without creating files, leave `LastError` set and the note dirty, and leave `IsAvailable` unchanged. After the fixture recreates the folder, `Save()` writes the retained text and clears Dirty.

Removed the redundant post-`Flush` availability recheck in `Save()`. Kept `Flush()`'s post-drain recheck with a comment explaining that a successful writer completion can clear `LastError`; the recheck restores the missing-folder error when the folder is still absent. Reviewed all `IsAvailable` references: startup and folder switching use the initialized/load state, and save snapshot writing still uses the explicit store state; no caller depended on a save changing it. No additional Store changes were needed.

Verification: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a30-check` succeeded with 0 warnings and 0 errors. `build/a30-check/AntiNotepad.exe --selftest` printed `SELFTEST PASS`. `git diff --check` passed. Cleanup of `build/a30-check` was blocked by policy; the directory remains, and no alternate deletion route was attempted. No app GUI or installed process was launched or stopped, and no real app-home, installed-copy, or Documents data was accessed.
