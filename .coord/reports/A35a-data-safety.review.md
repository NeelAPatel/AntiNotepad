# A35a-data-safety lens review

## Findings (proven)

1. [major] product/AntiNotepad/UI/SearchOverlay.cs:381 - Permanent-delete Undo restores metadata without preserving or rewriting the note body. The removal captures only `(Note, Index)` and removes the note from `Trash`; Undo at lines 389-392 only reinserts it and calls `SearchChangedData()`. For a previously saved note, `Dirty` is false. `SearchChangedData()` schedules persistence (`UI/PadWindow.Navigation.cs:300-314`); once that save runs, `Model/Store.cs:480-484` deletes the removed body's file. Undo does not make the note dirty, so `Store.cs:406-420` supplies no replacement body. Search reads bodies into its separate cache (`Model/Search/Store.Search.cs:39-46`), not into the retained Note, and can briefly conceal the loss. Proven with an offline probe linking the unchanged model sources, using a saved/reloaded trash note, search, the exact removal/reinsertion operations, and saves before and after Undo. Output: `delete save=True; body exists=False`, `Undo save=True; dirty=False; body exists=False`, `RELOAD text=''; unavailable=True`, `PROBE FAIL: Undo restored metadata but lost the persisted body`. All saves returned success. Smallest fix: capture an available body before removal and restore it as a dirty body on Undo, or defer physical deletion until Undo expires. Handle an unavailable body without replacing it with empty text. Add a save/delete-save/Undo-save/reload check; the current ordering-only test uses new in-memory dirty notes and never crosses the deletion persistence boundary.

2. [major] product/AntiNotepad/UI/PadWindow.Layout.cs:270 - With normal motion, withdrawing slot-clear Undo still leaves an actionable button during the fade, so new slot text can still be overwritten. Exact path: editing the cleared slot synchronously reaches `OnPaneEdited` and calls `HideToast()` (lines 267-270); `HideToast` clears `_toastAction` but leaves the toast visible and enabled until its animation completion (lines 726-732). `Motion/MotionTokens.cs:14-15` makes that fade 83 ms; `Motion/Anim.cs:21-31` schedules completion on later frames for a control attached to a window. The button handler at `Layout.cs:708` invokes its captured `onAction` without checking the cleared field. A click during that interval therefore executes `UI/PadWindow.Navigation.cs:251`, assigning the old body over the new text and removing the trash copy. Smallest fix: revoke button activation synchronously when hiding (and restore it on the next Toast), or guard the button callback against the current toast action/generation. Cover a withdrawn toast's button activation; the current fixture forces reduced motion (`SelfTest/UI.cs:45-46`), which hides the toast synchronously and masks this interval.

## Behavior changes check

Report's list vs the diff: matches the intended changes; the persistence and complete action-withdrawal claims are not fulfilled because of findings 1 and 2. No additional undocumented behavior change found.

- Search query routing (`SearchOverlay.cs:326-344`): in both Stack and Trash, plain Delete passes through; Ctrl+Delete passes through when a selection exists or the caret is before query end. Ctrl+Backspace passes through for every nonempty query, including caret-at-start and selected-text cases. Only empty-query Ctrl+Backspace retains note deletion, as expressly approved. Exact modifier matching leaves Shift/Alt combinations outside these delete routes. List and grid share this handler. `HasCurrentResults` prevents destruction against pending/stale query results.
- Permanent-delete Undo sorts captured indexes ascending and reinserts in that order, with clamping. That preserves the original positions for a batch when the intervening trash contents are unchanged. Its seven-second duration comes from the existing actionable-toast timer. Both toast-button and Ctrl+Z paths reach the same Undo callback; both have finding 1.
- Slot clear changes the document before arming the new toast, so its own synchronous Edited event does not withdraw that new toast. Undo's document assignment can call HideToast again, but its already-running callback still completes, removes the copy, and schedules the restored body. Editing another note does not match the tracked slot. A subsequent Toast replaces the tracked note; HideToast clears it. Finding 2 concerns activation after withdrawal.
- Save-card Enter runs in the window tunnel before the Button handles the key, marks the event handled, and chooses Quit by focus or button source; otherwise it invokes Retry (`Layout.cs:800-809`). Repeat prompts refresh both stored actions. Both production callers (`PadWindow.cs:283,318`) provide the same `RetryQuit`/`QuitAnyway` methods. Escape, Tab, Retry clicks, and the quit/save logic remain unchanged.
- Slot-only and mixed messages count skipped slots separately; the stack Undo callback remains unchanged for actual notes.

Caller/contract audit of the changed `Toast` signature (all paths relative to `product/AntiNotepad`):

- `UI/PadWindow.Navigation.cs:94,97,106,107,169,182,235,394,410,436,439,458,467`: notice-only calls keep their previous defaults. `:266`: stack delete Undo keeps its previous behavior. `:251`: the sole new slot argument opts into withdrawal, subject to finding 2.
- `UI/SearchOverlay.cs:369`: restore notice unchanged. `:387`: new permanent-delete Undo has finding 1. `:414`: stack-delete Undo retains its callback; slots-only calls now have no Undo action, matching the plan.
- `UI/PadWindow.Layout.cs:234`, `UI/PadWindow.Keys.cs:104,126,318`, `UI/PadWindow.cs:74,554,559,572,678`, `Program.cs:236`: existing forwarding, notices, settings Undo, and startup/save messages remain compatible with the optional trailing argument. The explicit `ISettingsHost.Toast` adapter still implements the unchanged interface. Its `UI/SettingsWindow.cs:664,865,868` callers remain compatible.
- `OnPaneEdited` is called by both panes' Edited subscriptions (`Layout.cs:232`); neither caller's save/chrome effects change. `HideToast` is also called by the timer, toast click, and both Ctrl+Z routes (`Layout.cs:193,708`; `Keys.cs:39,75`); clearing the new tracking field fits each caller, but does not revoke the captured button delegate.

## Scope and plan check

In plan. Every product hunk serves C-1 Search delete routing/Undo/hint, A-1 slot Undo withdrawal, C-3 save-card Enter/repeated-action refresh, C-2 accurate delete notices, or their four contract/selftest rows. The added query-selection guard is justified by the approved no-word-editing condition and is disclosed in Plan deviations. No unrelated icons, styles, defaults, dependencies, model changes, or layering changes.

The stack-view Undo body, editor slot-clear shortcut, Retry click, Escape, and Tab retain their original code paths. Four CONTRACTS rows exist and are wired to checks. Test hooks remain in SelfTest. The checks do not cover the two failing boundaries above. New toast tracking resets on replacement/hide; no permanent latch found. No new event/timer lifetime issue beyond the activation interval in finding 2 was found.

## Unproven risks

- Real focus acquisition and pointer behavior were not exercised in a visible window; review was offline only.
- Repeated save prompts keep the original button Click closures even though keyboard actions refresh; current production callers supply identical bound methods, so no present functional failure is established.

## Checks run

- Read the complete item card, implementer report, supplied GATE: PASS report, and `git diff 6902a44...HEAD -- product`; inspected every changed hunk, surrounding code, and callers. Five product files, 304 insertions and 11 deletions.
- Offline model persistence probe: `dotnet run --project build/A35a-data-safety-review/Probe.csproj --configfile build/A35a-data-safety-review/NuGet.Config`. Package sources were cleared; no external packages, app startup, or installed data were used. Probe compiled unchanged model sources into the review folder. Result: `PROBE FAIL: Undo restored metadata but lost the persisted body` (the diagnostic executable itself exited 0).
- Finding 2 proved by the exact synchronous-edit -> HideToast -> deferred fade completion -> captured button callback path quoted above; no visible-app reproduction claimed.
- Full product build/selftest/gate were not rerun. The supplied gate reports 0 warnings, 0 errors, `CONTRACTS: 45 rows, 44 pass, 1 manual`, and `SELFTEST PASS`; those checks do not disprove either finding.
- Deleted the temporary `build/A35a-data-safety-review` folder after the probe. No product source was edited; no branch switch, commit, merge, app launch, or `--quit` was performed.

VERDICT: rework
