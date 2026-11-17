# A21-save-delay-setting-r2 lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches across the original implementation report and r2 report.

- `Settings.SaveDelayMs` normalizes unsupported integers to 400, survives `CopyForSave()` and JSON persistence, and is classified as persistence-only. Its production consumers are the Settings dropdown, `PadWindow.SaveDelayInterval()` (constructor and `ScheduleSave()`), and the completion log's detached settings snapshot. `HandleSettingsChanged()` already calls `ScheduleSave()` for persistence-only changes, so changing the delay restarts the timer without applying unrelated appearance policies.
- `BindPanes()` now queues saves on changes to either displayed note or split state. Reviewed its callers: constructor, Navigate, NewNote, OpenFreshNote, ShowStack, OpenSlot, PromoteToSlot, ToggleSplit, CycleSplitSlot, DeleteCurrent, RestoreFromTrash, SearchChangedData and JumpTo. Initial binding is excluded; changed bindings capture the current bodies before `SwitchTo()` detaches them. Same-note focus changes do not require another body snapshot.
- `Store.ApplyCompletedSaves()` now sets memory-only `SavedAt` and raises `NoteSaved` after a matching revision clears `Dirty`. Its callers remain `Save()`, `DrainSaveQueue()` and `CompleteQueuedSaveAsync()`. The result and exception contracts of `Save`, `QueueSave` and `Flush` are unchanged. Reviewed their production consumers: PadWindow's queued save, synchronous save, session-end and quit paths; NotesSession's folder move; and the Settings host save before a move. No caller requires an incompatible result or side effect.
- `OnNoteSaved()` is the sole production event subscriber; it logs the captured delay and elapsed time and refreshes chrome only for the active note. It is removed on window close. `SavedText()` is consumed by chrome updates and periodic saved-label refresh, both now using `SavedAt ?? Modified`. Saves do not assign `Modified`; failed or stale writes cannot advance `SavedAt` through the new acknowledgement guard.
- R2 removes duplicate Avalonia setup, orders the UI contract after `shortened-link-opens`, and updates the design hash. These change checks only, as reported.

## Scope and plan check
In plan. All 13 changed product files serve the approved After or its checks. The additional `SelfTest/Design.cs` edit is explicitly authorized by the r2 brief. File header comments accompany touched files and satisfy the coding rules; no unrelated icon, style, rename or default changes were found. No unlisted plan deviation found.

Checked the bounds stated in the card: 400 ms default; exactly the four approved choices; Saving group and approved hint; persistence-only settings reaction; queued transition saves; existing hide/blur/quit/session-end paths; memory-only save timestamp with Modified preserved; revision matching; completion refresh and logging. The reported use of the synchronous quit save method in selftest, with real quit left to hand testing, is disclosed in the original report.

The two new CONTRACTS rows cover settings/storage acknowledgements and timer/save boundaries. Their checks include all four persisted choices, invalid input, stale acknowledgement rejection, unchanged Modified, nonserialization, timer interval changes, save logs, active label refresh, and navigation/hide/quit save boundaries. New test orchestration remains in SelfTest. No new Model/Editor dependency on UI or native call outside Platform was introduced.

## Unproven risks
- Real elapsed debounce timing and a second keystroke restarting the wait remain hand checks: the selftest inspects timer configuration and forces writes rather than waiting through each interval.
- Actual Windows sign-out/shutdown remains the explicitly open A30 live check; this review does not establish it.

## Checks run
- Read the item card, r2 brief, both implementation reports, r2 gate report, changed hunks and surrounding code; traced affected callers, settings flow, acknowledgements, failure paths and cleanup.
- `git diff dev...HEAD -- product`: empty; the reviewed implementation is in the uncommitted product diff. `git diff --cached --stat -- product`: empty.
- `git diff -- product`: reviewed all 13 changed product files. `git diff --check`: pass (exit 0).
- Supplied `.coord/reports/A21-save-delay-setting-r2.gate.md`: GATE PASS; build 0 warnings/0 errors; SELFTEST PASS; seam, tokens, format, scope and size pass. These are supplied gate results, not independently rerun build/selftest results.
- No build, application launch or probe was needed for a proven finding. No review build folder was created. Product files, branch, index and STATUS were not changed by this review.

VERDICT: accept
