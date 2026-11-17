# A21-save-delay-setting-r4 lens review

## Findings (proven)

1. [major] product/AntiNotepad/Model/Note.cs:119 - Clearing a new note during its first queued write loses the deletion and resurrects the old text on reload. Queue a nonempty live note, clear its document before the 50 ms coalesced write completes, then quit/save. Touched clears Dirty because SavedAt is still null; ScheduleNoteSave also removes the deadline (UI/PadWindow.cs:423). The captured nonempty body still writes. Its stale completion cannot set SavedAt (Model/Store.cs:510), and subsequent snapshots skip the now-clean empty document (Model/Store.cs:405). Even quit reports success without persisting the deletion. Probe output: `first=True, quit=True, liveLength=0, dirty=False, savedAt=, reloadedText=[text deleted during first save]`. Smallest safe fix: distinguish a pristine blank from a note with a queued/in-flight or previously durable body; retain a dirty empty revision and its deadline until it supersedes any older write. Cover pending writes and already-written-but-stale completions.

2. [major] product/AntiNotepad/UI/PadWindow.Navigation.cs:136 - Switching to the already-visible split note does not save the departing note immediately. With split view open, edit the left note and press Ctrl+the right slot. OpenSlot changes _active and calls BindPanes, but its transition predicate compares only bound notes and split mode (Navigation.cs:25); these are unchanged, so no save is queued. Clicking the other pane has the same omission: FocusedPane only changes _active and refreshes chrome (UI/PadWindow.Layout.cs:224; raised by Editor/PadView.cs:148). Probe after OpenSlot and Store.Flush: `activeIsRight=True, departingDirty=True, latestTextOnDisk=False`. This misses the brief's every-note-switch boundary. Smallest fix: centralize active-pane changes and request SaveImmediately when leaving a different active note, including the focus-only slot path; test mouse focus and Ctrl+slot.

3. [minor] product/AntiNotepad/UI/PadWindow.Layout.cs:312 - A backward wall-clock change can suspend the visible saved-label clock for the entire time difference. With SavedAt one hour ahead of now, SavedLabelRefreshInterval returns one hour plus one second; UpdateSavedLabelTimer assigns that interval at :370. If the clock is corrected forward shortly afterward, the label remains "Saved just now" until that long dispatcher interval elapses. This violates r3 edge 7's once-a-second visible refresh and delays recovery from edge 5's clock changes. The formula at :308-312 proves the interval; SelfTest/Design.cs currently asserts that hour-long interval as expected behavior. Cap future-time refresh intervals at one second and update that check.

## Behavior changes check

Report's list vs the diff: missing the unsafe blank-discard consequence in finding 1 and the future-time refresh suspension in finding 3. The claimed immediate save on every note switch is incomplete (finding 2). The remaining listed changes match: normalized/persisted setting, independent monotonic schedules, idle and repeating safety attempts, minimize boundary, durable body timestamps, and label-only completion refresh.

Changed shared methods and callers checked:

- Note.Touched: production caller is PadView.DocumentFor's document TextChanged handler; Notes/Store selftests also call it. The new clean-blank meaning fails with an outstanding body write (finding 1).
- DiscardUnpersistedEmptyEdit: ScheduleNoteSave, SaveImmediately, DropIfAbandoned, DeleteCurrent. Its guard does not represent pending-write state (finding 1).
- Store.ApplyCompletedSaves: Save, DrainSaveQueue (Flush and SwitchFolder), CompleteQueuedSaveAsync. Revision matching remains intact; NoteSaved subscribers are PadWindow.OnNoteSaved and the selftest counter.
- Edited: pane edits now use OnPaneEdited; clipboard append, OCR and slot-undo pass the affected note; zoom, settings appearance, reorder and search metadata changes use the data deadline. Reflection selftests pass the updated parameters.
- BindPanes: construction, Navigate, NewNote, OpenFreshNote, ShowStack, OpenSlot, PromoteToSlot, ToggleSplit, CycleSplitSlot, DeleteCurrent, RestoreFromTrash, SearchChangedData, JumpTo. Rebinding transitions queue a snapshot; active-pane-only switches escape that rule (finding 2).
- QueueSave: SaveImmediately and SaveTimerTick. The latter reads live text without flushing panes. Completion calls SetSavedText and label-timer scheduling rather than UpdateChrome. Save/quit and session-end callers retain synchronous Store persistence.

## Scope and plan check

In plan: all 14 changed product files serve the original approved After or the r2/r3/r4 overrides. Later briefs explicitly replace Modified fallback, delay-change restart and original label formatting; these are authorized changes. Design.cs is allowed by r2/r3/r4. Header comments follow the on-touch coding rule. No unrelated visual, icon, rename or default changes found. The card has no separate "Must not change" list; its explicit default-delay, Modified, serialization, undo and installed-copy constraints were checked.

Pure timing checks cover default debounce, 5/10/30-second idle/safety deadlines, retries and delayed ticks. Store checks cover failure retention and stale acknowledgements. Hidden-pad checks cover per-note saving, normal rebinding transitions, minimize/hide, saved-empty bodies and undo. They miss the interleavings and focus-only boundary above. The future-clock test codifies finding 3.

## Unproven risks

- Actual visible bar behavior and real IME composition still require the user's dev-copy hand checks; offline UI checks use hidden windows.
- Real sign-out/shutdown remains the existing A30 hand check.

## Checks run

- Reviewed `git diff dev...HEAD -- product` and `git diff -- product`, changed-file context and callers; branch feat/A21-save-delay-setting, HEAD db91931, no uncommitted product changes.
- Original source copied under build/A21-save-delay-setting-review/source; offline build with --no-restore and NuGetAudit=false: 0 warnings, 0 errors. No product files edited.
- Temporary probes added only to that copy; rebuilt with 0 warnings/errors. Captured selftest: exit 0, `CONTRACTS: 23 rows, 22 pass, 1 manual`, `SELFTEST PASS`. Probe results above demonstrate failures absent from existing assertions.
- Supplied r4 gate: `GATE: PASS`; size warnings retained for SelfTest/Store.cs, SelfTest/UI.cs, UI/PadWindow.cs and UI/PadWindow.Layout.cs.
- `git diff --check`: pass.
- Removed the review build, probes and fixture data; verified the review directory no longer exists. No app launched interactively, stopped, installed or changed.

VERDICT: rework
