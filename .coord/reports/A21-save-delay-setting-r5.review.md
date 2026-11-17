# A21-save-delay-setting-r5 lens review

## Findings (proven)

1. [minor] product/AntiNotepad/Model/Store.cs:538 - Deleted notes can leave permanent entries in `_unsupersededBodyWrites`. Reproduction: edit a fresh stack note; queue its first body; clear the document; let the stale first write finish; delete the blank note; save and drain. The body is correctly deleted, but the tracking dictionary still contains its ID and revision, and `HasPendingBodyWrite` still returns true. Independent probe output: `delete-stale tracking: retainedIds=1, deletedIdRetained=True, pending=True`. A second probe blocked the writer after taking its snapshot, cleared/deleted the note while that write was in progress, then released it and saved: `delete-inflight tracking: retainedIds=1, deletedIdRetained=True, pending=True`. The writer adds the stale revision at Store.cs:398-400; the revision guard at :527-528 prevents its removal; snapshot cleanup at :418-420 prunes only `_pendingBodies`. Once the note is absent from Data.All, no later snapshot can produce its matching acknowledgement. Repeating this lifecycle retains another dictionary/hash-set entry per deleted note for the Store's lifetime. No body resurrection or lost edit was observed. Smallest fix: retire tracking for removed IDs after the serialized writer has finished their earlier writes and durably applied the deletion; include a regression for deletion after stale completion and during an in-progress write.

## Behavior changes check

Report's list vs the diff: matches the intended changes; missing the minor retained tracking state in finding 1. The three r4 fixes pass fresh independent probes:

- Clearing during a queued or in-progress first write preserves the empty revision; synchronous quit-save acknowledges it and reload returns empty text. Clearing after an already-completed stale write also passes.
- Ctrl+slot to the visible split pane and the pane-focus handler both save the departing note immediately. The actual editor GotFocus event remains wired to that handler.
- Future SavedAt values produce a 1000 ms refresh interval.

Shared contracts and callers checked:

- `Note.Touched`: PadView.DocumentFor's TextChanged handler and Notes/Store selftests. The pending-body callback now preserves dirty empty revisions; ordinary pristine blanks remain clean. Store's constructor and SwitchFolder assign the callback. `HasPendingBodyWrite` is also read by ScheduleNoteSave and SaveImmediately; those guards hold in the fresh clear/write probes.
- `DiscardUnpersistedEmptyEdit`: ScheduleNoteSave, SaveImmediately, DropIfAbandoned and DeleteCurrent. The first two check pending writes; the latter two remove abandoned notes. Durable deletion succeeds, with the tracking cleanup exception above. `KeepEmptyEditDirty` is used only by the two scheduling paths.
- `Store.ApplyCompletedSaves`: Save, DrainSaveQueue (used by Flush, Load, RemoveOrphans and SwitchFolder), and CompleteQueuedSaveAsync. Revision matching protects Dirty and SavedAt; matching completions leave Modified unchanged. NoteSaved subscribers are PadWindow.OnNoteSaved and selftest counters. The added completion lock has no reverse lock acquisition in the reviewed paths.
- `Edited`: pane edits use OnPaneEdited; clipboard append, OCR and slot-undo identify the affected note; geometry/settings/reorder/search metadata use the separate data deadline. Reflection checks match the changed signature.
- `BindPanes`: construction, Navigate, NewNote, OpenFreshNote, ShowStack, OpenSlot, PromoteToSlot, ToggleSplit, CycleSplitSlot, DeleteCurrent, RestoreFromTrash, SearchChangedData and JumpTo. Rebinding transitions save; ActivatePane covers focus-only changes from OpenSlot, HandlePaneFocused and OCR. Calls suppressing its save are followed by the binding transition save.
- `QueueSave`: SaveImmediately and SaveTimerTick; timer saves read documents without flushing editor state. `Save` remains synchronous for quit, retry and session end; Flush drains existing work. Queued and in-progress quit probes persist the newest revision. Save completion updates label text and label scheduling without calling UpdateChrome or waking bars.

## Scope and plan check

In plan. Reviewed all 14 changed product files against the approved After and r2-r5 overrides. Settings normalization/persistence and the approved Saving dropdown serve the original plan. Per-note monotonic timing, safety writes, minimize, empty-note handling, label formatting/visibility and label-only completion updates serve r3. The first-write tracking, active-pane save boundary and future-clock cadence serve r5. Selftests, CONTRACTS and the design snapshot serve those promises. No unrelated icon, style, rename or default changes, or unlisted scope deviations found.

The card has no separate Must not change list. Its explicit constraints were checked: default 400 ms; Modified remains last-edit time; SavedAt stays out of serialization; body/index layout unchanged; save copies text without changing undo; offline review and installed-copy isolation preserved. The later briefs explicitly override the original label fallback and delay-change behavior. Test hooks exist only in the temporary review copy. Existing selftests cover the new promises except the cleanup lifecycle in finding 1.

## Unproven risks

- Real visible-bar behavior and active IME composition still require the user's dev-copy hand checks; offline UI probes use hidden windows.
- Real sign-out/shutdown remains the existing A30 live check.

## Checks run

- Read the item card, r2-r5 briefs, original/r4/r5 implementation reports, r4 review and supplied r5 gate; reviewed `git diff dev...HEAD -- product`, `git diff -- product`, changed-file context and callers. Branch feat/A21-save-delay-setting, HEAD 74d0bf7; no uncommitted product changes.
- Copied source under build/A21-save-delay-setting-review/source. Offline Debug builds with `--no-restore -p:NuGetAudit=false`: 0 warnings, 0 errors. Product files were never edited.
- Added independent probes only in that copy. Final run exited 0: `CONTRACTS: 26 rows, 25 pass, 1 manual`, `SELFTEST PASS`. Design dump remained `7c2c76482fc04e6e4e3c933b72e6d5e42774f6a702844eb41cbf66db181ce607`.
- Fresh probes passed clear-during-first-write, stale acknowledgement followed by clear, saved-empty body reload and subsequent edit, split shortcut/focus save, future-clock cadence, and latest-body quit persistence. Queued and deliberately blocked in-progress writes were both exercised. Deleted bodies stayed deleted; tracking inspection proved finding 1.
- Supplied r5 gate: `GATE: PASS`, with the existing five-file Size warning. Gate was not rerun; this review independently built and ran the full selftest with probes.
- `git diff --check`: pass. Removed the review build, copied source, probes, logs and fixture data; verified the review directory no longer exists. No interactive app launch, process termination, installation or git mutation.

VERDICT: accept
