## 1. Kind and why

**Feature, with a save-status correctness fix.** Today the delay is fixed at 400 ms; there is no setting. “Saved…” reports the last edit time before that edit necessarily reaches disk. Longer delays make both that misleading label and missing save-on-navigation behavior significant.

Audited `dev` at `8414bf03cd84`. No files changed, builds/tests run, or app processes launched/stopped.

## 2. Now

All product paths below are relative to `product/AntiNotepad`.

**Verified timer:** `UI/PadWindow.cs:30,88,379,386` implements a restarting 400 ms timer: `Edited()` → `ScheduleSave()` → timer tick → `QueueSave()`. Store then adds a **50 ms coalesce window**, plus actual I/O time (`Model/Store.cs:27,348`). Thus 400 ms is the enqueue delay, not an exact disk-completion deadline.

**Pending-edit paths:**

| Action | Current behavior | Consequence at 5/10/30 s |
|---|---|---|
| Hide: hotkey, resident close/X/Alt+F4, blur, taskbar hide | `HideResident()` calls asynchronous `QueueSave()` before hiding; no synchronous drain (`UI/PadWindow.cs:114,144,238,350,371`) | Bypasses selected delay, but save finishes after hide |
| Deactivate | Calls asynchronous `QueueSave()` before the Settings-window exemption (`UI/PadWindow.cs:96`) | Bypasses selected delay; still subject to Store coalescing |
| Quit/retry | Flushes both pane positions, calls synchronous `Store.Save()`; failure offers retry/quit anyway; completion drains queued work (`UI/PadWindow.cs:252,289,297,410`) | Already bypasses timer |
| Shutdown/restart/sign-out | `ShutdownRequested` → `PrepareForSessionEnd()`; OSShutdown closing is a fallback. Stops timer, flushes pane positions, synchronously saves (`Program.cs:219`; `UI/PadWindow.cs:116,266`) | Already bypasses timer; live A30 verification remains open |
| Stack navigation/new note | Pane `Flush()`, change selection, bind, then `Edited()` (`UI/PadWindow.Navigation.cs:62,87`) | **No immediate disk save; restarts the delay** |
| Stack/slot/search-result selection | `ShowStack`, `OpenSlot`, `JumpTo` similarly schedule; split-slot cycling schedules through settings (`UI/PadWindow.Navigation.cs:112,122,184,264`) | **Same gap** |
| Promote/restore/split changes | Schedule through `Edited()` (`UI/PadWindow.Navigation.cs:137,155,229`) | Pending edits remain delayed |
| Automatic fresh note after absence | `OpenFreshNote()` binds without explicitly scheduling (`UI/PadWindow.Navigation.cs:103`) | No explicit save boundary; normally follows an earlier hide save |
| Notes-folder migration | Settings host saves first; `NotesSession.MoveNotes()` saves again before moving; `SwitchFolder()` drains existing queue (`UI/SettingsWindow.cs:650`; `NotesSession.cs:24`; `Model/Store.cs:525`) | Already bypasses timer |
| Choose unavailable folder for next startup | Writes the location pointer; does not migrate/save current notes (`NotesSession.cs:51`) | Existing unavailable-store behavior, not a flush guarantee |
| Open Settings | No explicit save in `OpenSettings()`; normal pad deactivation queues one (`UI/PadWindow.Keys.cs:208`; `UI/PadWindow.cs:96`) | Relies on deactivation |
| Crash/forced termination | Exception handlers log only (`Program.cs:32,37`) | Unsaved edits can be lost for the longer interval; crash recovery is outside the Request |

**Important distinction:** `PadView.Flush()` only copies caret and scroll position into the model; it does **not** write text (`Editor/PadView.cs:447`). Dirty text survives editor detachment in memory (`Model/Note.cs:119`), so navigation does not itself discard it—but does not make it durable.

**Async dirty clearing works.** `Store.QueueSave()` returns `CompleteQueuedSaveAsync()`’s task; that awaits writer completion and calls `ApplyCompletedSaves()` (`Model/Store.cs:225,320,330`). The latter clears `Dirty` only for acknowledged durable bodies whose current revision matches the saved revision (`:473`). An older completion cannot clear a newer edit.

**Visible label today:** edits immediately update chrome; `SavedText()` uses `Note.Modified`, updated by `Touched()` on edits (`UI/PadWindow.cs:379`; `UI/PadWindow.Layout.cs:275`; `Model/Note.cs:112`). It therefore displays an edit age as a save age. Save completion itself does not refresh the label (`UI/PadWindow.cs:393,410`). Refreshes occur during chrome updates, top-bar show, and chrome wake (`UI/PadWindow.Layout.cs:257,316,501,526`); there is no periodic age-refresh timer.

## 3. Reproduction

Code-path reproduction, not a live run:

1. Edit a note: document change calls `Touched()` and raises `Edited` (`Editor/PadView.cs:218,624,657`).
2. `Edited()` restarts the timer and updates “Saved…” immediately (`UI/PadWindow.cs:379`).
3. Before the timer fires, navigate or create a note.
4. Navigation calls pane `Flush()`, binds another note, then restarts the timer (`UI/PadWindow.Navigation.cs:62,87`).
5. No Store save was requested by that transition.

This proves the missing save boundary and premature label from source. The proposed 30 s behavior needs implementation before it can be exercised live.

## 4. Where

Likely change surface:

- `Model/Settings.cs`: persisted delay with 400 ms default.
- `Model/SettingsService.cs`: explicit persistence-only mapping, avoiding its unlisted-property `Appearance | Persistence` fallback.
- `UI/SettingsWindow.cs`: Storage control and search text.
- `UI/PadWindow.cs`, `.Keys.cs`, `.Navigation.cs`: timer policy, settings reaction, navigation save boundaries, completion refresh.
- `Model/Note.cs`, `Model/Store.cs`, `UI/PadWindow.Layout.cs`: distinct save timestamp and label.
- `SelfTest/Store.cs`, `UI.cs`, `Contracts.cs`, and test registration as needed.

**Other debounce:** settings and geometry changes share `ScheduleSave()` (`UI/PadWindow.Keys.cs:313`; `UI/PadWindow.cs:448`). Settings live inside `index.json`, not a separate settings file (`Model/Store.cs:16,411,450`). Simply changing the timer delays both. The editor’s 140 ms analysis debounce does not defer `Edited()` (`Editor/PadView.cs:648,657,660`).

**Keep `Modified` as last edit.** Consumers include persisted metadata (`Model/Store.cs:418`), search-cache identity/stale-result protection (`Model/Search/Store.Search.cs:19,77,85`), search-result age (`UI/SearchOverlay.cs:198`), trash-copy metadata and latest clipboard-note selection (`UI/PadWindow.Navigation.cs:208,319`). Stack ordering is explicit list order, not a `Modified` sort.

**`SavedAt` placement:** capture the timestamp in the writer for an acknowledged body write; carry it through completion and apply it beside the revision-checked acknowledgement. Do not timestamp enqueue or delayed UI continuation, and do not treat unavailable-store “success” as a body save. If persisted, include it in detached metadata before index serialization. Refresh the active label after completion and synchronous saves. Existing files need an explicit unknown/legacy fallback.

Size check: `SettingsWindow.cs` is 746 lines and `PadWindow.Layout.cs` 696. Both contain distinct responsibilities; flag their extraction candidates on the card as separate follow-up items, per coding rules.

## 5. Risk

**Highest risk:** acknowledging the wrong revision, losing a pending edit at a transition, or falsely reporting a save.

`Store.Flush()` drains already queued snapshots; it **does not capture edits still waiting in the UI timer** (`Model/Store.cs:260`). A required boundary must capture current data through `Save()`/`QueueSave()`, not merely call `Flush()`.

Gates from `.claude/skills/next-task/phases.md`:

- Offline gate/selftests + CONTRACTS: defaults, persistence, revision acknowledgements, failures and transition saves.
- Fresh lens review: timer resets, completion ordering, missed navigation routes.
- Design review: settings/body scheduling coupling and timestamp ownership.
- Approved lab + hand test: control, pending/saved label, motion, actual timing and lifecycle behavior.

“Durable” currently means successful atomic file replacement; `Atomic()` does not explicitly flush hardware buffers (`Model/Store.cs:549`). Do not broaden the promise to power-loss durability.

## 6. Tier

**Confirm High — save paths, timers, Store acknowledgements, quit/session-end coupling.** This directly matches `docs/loop.md`’s High criteria.

## 7. Recommendation

Keep one item. Add the four validated delays, preserve the existing 400 ms default and Store coalescing, add explicit save boundaries for every actual note transition, and separate last-save time from `Modified`.

Recommended plan decisions:

- Keep hide/deactivation saves asynchronous to preserve show/hide speed.
- Keep settings/geometry persistence prompt; explicitly document that their shared Store snapshot may save pending note text early.
- Define changing the delay while a timer is pending.
- Define never-saved, pending, failed and legacy-note label states in the lab; none should imply the current edit is saved.

Existing test homes:

- `SelfTest/Store.cs:320`, `BackgroundSaveAndLogChecks`, local `CheckCase` and temporary-store/reload fixtures: coalescing, queued flush, latest live text before quit, settings snapshot, failure/retry and folder switching.
- `SelfTest/UI.cs:178`, `SettingsChangedChecks`: settings notifications.
- `SelfTest/Platform.cs:326`, `SessionEndCloseChecks`: close-policy checks only; **not proof that shutdown saved text**.
- `SelfTest/Contracts.cs:19`: existing save checks are integrated through a memoized helper.

Add checks for legacy/default/invalid settings, each round trip, restart-after-edit scheduling, timer-pending boundary capture, async-only dirty clearing, stale completion, save timestamp round trip, unavailable/failing writes, and unchanged `Modified`. Exercise the actual scheduling/boundary policy, not only Store methods.

Hand-test every delay with a second keystroke restarting the wait; verify body-write timing logs and completion-based label refresh. Test immediate hide, all note-switch routes/new note, Settings, folder migration and clean quit/restart. Retain explicit user-run shutdown/sign-out checks.

READY: yes