# A21b-save-scheduler-extract lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches. No product behavior change found.

- `Model/Note.cs:134`: `ResolveEmptyEdit` preserves the original decision tree from both blocks in `dev:product/AntiNotepad/UI/PadWindow.cs`: nonblank or previously saved returns false without mutation; blank, never saved, write in flight calls `KeepEmptyEditDirty` and returns false; otherwise discards and returns true. The original code also read `SavedAt` before the volatile in-flight flag. No new asynchronous work, state or exception path is introduced.
- Both callers still hold: `UI/PadWindow.cs:424` supplies the editor's blank flag and removes/rearms/returns only when discarded; `UI/PadWindow.cs:452` supplies `note.IsEmpty` and removes/continues only when discarded. The in-flight case continues into scheduling or retry exactly as before. `KeepEmptyEditDirty`, `DiscardUnpersistedEmptyEdit`, and Navigation's two direct discard calls are unchanged.
- `UI/SaveTiming.cs:6`: the complete 49-line class, including comments, matches the original class after removing four spaces of nesting indentation. Only the namespace-level location and its required file header/import were added. Production callers remain `ScheduleNoteSave` (Edit), `SaveImmediately` (Edit/RetryAfterAttempt), `SaveTimerTick` (Tick), and `ArmSaveTimer` (NextDeadline). Selftest callers and the reflected dictionary cast use the relocated type. No old qualified references remain.
- Save triggers remain wired: switch/rebind (`PadWindow.Navigation.cs:34`) and pane focus (`PadWindow.Layout.cs:239`), hide (`PadWindow.cs:264`), minimize (`PadWindow.cs:519`), and deactivation (`PadWindow.cs:113`) call `SaveImmediately`; timer Tick still reaches `SaveTimerTick` and `QueueSave`. Quit (`PadWindow.cs:274`) still synchronously flushes/saves before `CompleteQuit`; session end (`PadWindow.cs:288`) still flushes both panes and synchronously saves. None of these entry points or their failure handling changed.
- The new model checks cover discard, in-flight retention, saved-empty preservation and nonempty preservation. Existing contract checks retain timing, stale write, failure, switch, hide, minimize and tick coverage. The UI check now invokes actual Quit and adds a pending-edit session-end check; its session-ending flag is reset in finally. Test hooks remain in SelfTest. No layering dependency, new subscription in production, or cleanup change was introduced.

## Scope and plan check
In plan. All five changed product files serve the approved Plan's After: move the timing class and adjust test references; consolidate the two empty-edit blocks; add model, Quit and session-end checks. Navigation remains unchanged as permitted. Delays, saved-label behavior, timer/scheduler glue and `_saveSchedules` are preserved. PadWindow is 680 physical lines versus 740 in dev, consistent with the approximate reduction requested. No unrelated product hunks or unlisted implementation deviations found.

The implementer report's uncommitted-work note predates HEAD: the changes are now committed, with the helper commit preceding the separate timing-move/test commit. This does not affect the reviewed product behavior. Design review and slot hand testing remain separate checks.

## Unproven risks
- Live type/wait/switch/hide and actual Windows session-end behavior were not exercised; the approved hand test remains pending.

## Checks run
- Read the item card, implementation report, supplied GATE PASS report, all product diff hunks and surrounding code; traced helper/timing callers and save entry points against dev.
- Exact ordinal comparison of the complete moved SaveTiming block, removing only its former four-space nesting indent: True.
- Offline Debug build using the local NuGet package cache, NuGetAudit=false, AVALONIA_TELEMETRY_OPTOUT=1, with artifacts/output confined to `build/A21b-save-scheduler-extract-review`: Build succeeded; 0 warnings; 0 errors.
- Built executable `--selftest`, waited for process completion: exit 0; CONTRACTS: 32 rows, 31 pass, 1 manual; SELFTEST PASS. Queued-write regression retained the empty revision; split focus/switch saves passed.
- `git diff --check dev...HEAD -- product`: reports only a new blank line at EOF in `UI/SaveTiming.cs:55`; no behavioral implication.
- Removed the isolated review build folder after checks. No product files edited; no branch changes, commits, merges or installed-copy interaction.

VERDICT: accept
