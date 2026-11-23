# A21-save-delay-setting-r5

## 1 Summary

Fixed the three r4 review findings. The new offline regressions reproduce each finding before the fix and pass after it. No card or brief was changed.

## 2 Change list

- `product/AntiNotepad/Model/Note.cs` | Keep a blank revision dirty when Store reports an older body write may still reach disk.
- `product/AntiNotepad/Model/Store.cs` | Track queued, writing, and stale durable body revisions until the current revision is durable.
- `product/AntiNotepad/UI/PadWindow.cs` | Preserve the empty revision and its deadline while an older body needs replacement.
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`, `product/AntiNotepad/UI/PadWindow.Layout.cs` | Save all pending edits when Ctrl+slot or pane focus changes the active note in split view.
- `product/AntiNotepad/SelfTest/UI.cs`, `product/AntiNotepad/SelfTest/Design.cs`, `product/AntiNotepad/SelfTest/Contracts.cs` | Add the three regression checks and CONTRACTS rows.

## 3 Behavior changes

Clearing a never-saved blank note still cancels its timer when no body write exists. If an older body is queued or has reached disk with a stale acknowledgement, the empty revision stays dirty and scheduled until the newer empty body is durable. Split-view active-note changes save the departing note immediately. Future `SavedAt` values keep the label clock at a one-second cadence.

## 4 Out-of-scope files

No card or brief changes. No files outside the supplied product file list changed.

## 5 Plan deviations

None.

## 6 Contracts

Added `save-delay-empty-first-write`, `save-delay-split-active-switch`, and `saved-label-future-clock-cadence`. Each new contract points to the focused regression run shared with the existing save-delay or saved-label check.

Before fix:

```text
SAVE DELAY REGRESSION blank queued write: first=True, queuedBeforeClear=True, clearDirty=False, clearScheduled=False, staleDirty=False, staleScheduled=False, quit=True, liveLength=0, savedAt=, reloadedText=[text deleted during first save]
SAVE DELAY REGRESSION split active note: shortcutActiveRight=True, shortcutSaved=False, focusActiveLeft=True, focusSaved=False
SAVE DELAY REGRESSION future label refresh: savedAtInFuture=True intervalMs=3601000
```

After fix:

```text
SAVE DELAY REGRESSION blank queued write: first=True, queuedBeforeClear=True, clearDirty=True, clearScheduled=True, staleDirty=True, staleScheduled=True, quit=True, liveLength=0, savedAt=2026-11-16T22:40:30.8332822-05:00, reloadedText=[]
SAVE DELAY REGRESSION split active note: shortcutActiveRight=True, shortcutSaved=True, focusActiveLeft=True, focusSaved=True
SAVE DELAY REGRESSION future label refresh: savedAtInFuture=True intervalMs=1000
CONTRACTS: 26 rows, 25 pass, 1 manual
SELFTEST PASS
```

## 7 Lens review

Not run in this worker round.

## 8 Checks run

- Baseline regression build: 0 warnings, 0 errors; the three new checks failed with the reproduction lines above.
- Final direct Debug build: 0 warnings, 0 errors; final direct `--selftest` exited 0 with 26 CONTRACTS rows, 25 pass, 1 manual, `SELFTEST PASS`.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A21-save-delay-setting-r5 -Offline` — FINAL line: `GATE: PASS`. Build, Selftest, Seam, Tokens, Format, Scope, and Cleanup passed; LoopLint skipped. The gate reported its existing Size warning for five product C# files.
- The first probe version synchronously waited on `QueueSave()` from the UI selftest and hung. PID <PID>, path `<repo root>\Visual Studio Apps\AntiQuickPad\.claude\worktrees\feat\A21-save-delay-setting\build\A21-save-delay-setting-r5-baseline\AntiNotepad.exe`. It was left running per the inherited r3 instruction not to stop AntiNotepad. The probe now uses `Store.Flush()` and passes.
- Requested Bash cleanup was rejected by the sandbox, so the remaining task folders are listed here: `build/A21-save-delay-setting-r5-baseline`, `build/A21-save-delay-setting-r5-baseline2`, and `build/A21-save-delay-setting-r5-check`. The gate removed its own task build and data folders.

## 9 Hand checks for the user

- Open split view, switch the active pane with an unsaved edit, then quit; confirm the departing note was saved.
- Repeat the active-pane switch by Ctrl+slot and by clicking the other pane.
