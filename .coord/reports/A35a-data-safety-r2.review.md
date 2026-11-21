# A35a-data-safety-r2 lens review

## Findings (proven)

None. Both major findings from the prior review are fixed.

## Behavior changes check

Report's list vs the diff: matches. The original report describes the combined behavior changes; the r2 report describes the persistence and stale-button corrections. No missing behavior change found.

- **Search word editing:** `product/AntiNotepad/UI/SearchOverlay.cs:326-344` is the query's tunnel handler, registered at line 76, shared by Stack/Trash and list/grid. Plain Delete passes through. Ctrl+Delete passes through whenever the caret is before query end or query text is selected. Ctrl+Backspace passes through for every nonempty query, including caret-at-start and selection cases. Exact modifier matching also leaves Shift/Alt combinations outside these destructive routes. Only query-end Ctrl+Delete with no selection and empty-query Ctrl+Backspace call `DeleteSelected`, as approved. `UI/PadWindow.Keys.cs:32-40` returns from the overlay branch without dispatching editor delete shortcuts. `HasCurrentResults` (`SearchOverlay.cs:441`) blocks destructive actions against pending or stale results.
- **Permanent-delete Undo ordering and persistence:** `SearchOverlay.cs:381-389` reads every body before removing any note, captures availability after that read, and sorts original trash indexes. Undo at lines 393-399 reinserts in ascending index order with clamping and assigns captured available text through `Note.Text`. The setter (`Model/Note.cs:54-58`) marks it dirty and advances its search revision even when the body string is unchanged. The initial read retains an available body in `_text`, so the setter does not need to read a body file that the deletion save has removed. `Model/Store.cs:406-420` then captures the dirty body for persistence; revision matching in `ApplyCompletedSaves` prevents an older acknowledgement from clearing this new revision. An unavailable body is reinserted without a text assignment or empty replacement. `SearchChangedData` schedules or immediately queues persistence, and search restarts only for the current overlay. This fixes prior finding 1.
- **Slot clear and withdrawal:** `UI/PadWindow.Navigation.cs:248-252` changes the document before installing the new toast. The synchronous Edited notification (`Editor/PadView.cs:658`) therefore cannot withdraw that new toast during the clear itself. Later edits reach `OnPaneEdited` (`UI/PadWindow.Layout.cs:268-274`) and withdraw only the toast tracking that exact slot. Toast replacement resets the tracked slot; hide clears it. Undo's own document assignment can withdraw the toast while its callback is already running, but does not interrupt the callback: the trash copy is removed, text restored, and saving scheduled. The button checks generation and current action before invocation (`Layout.cs:712`); hide increments the generation and clears the action synchronously (`:735-736`). A retained button cannot act during the fade or after replacement. The post-callback generation check also avoids hiding a replacement toast. This fixes prior finding 2.
- **Save-card Enter:** `Layout.cs:145` installs the window tunnel handler. At lines 810-817 it requires the active save overlay, marks Enter handled, and chooses Quit for the focused Quit button or a Quit button event source; otherwise it chooses Retry. This prevents a second Button Enter action. Repeated prompts update both stored actions (`:746-750`). Both production callers (`UI/PadWindow.cs:283,318`) supply the same `RetryQuit` and `QuitAnyway` bound methods. Escape, Tab, Retry clicks, and quit/save logic retain their paths.
- **Counts and preserved Undo:** slot-only and mixed notices count actual stack notes and skipped slots separately (`SearchOverlay.cs:404-434`). The stack Undo callback retains its previous operations. The editor slot-clear shortcut is unchanged.

Changed-contract caller audit (paths relative to `product/AntiNotepad`):

- `Toast`'s optional trailing argument is used only by `UI/PadWindow.Navigation.cs:251`. Existing calls remain compatible: Navigation lines 94, 97, 106, 107, 169, 182, 235, 266, 394, 410, 436, 439, 458, 467; Keys lines 104, 126, 318; Layout line 235; PadWindow lines 74, 554, 559, 572, 678; Program line 236. SearchOverlay lines 369, 391, 421 preserve restore notices or implement the planned new Undo/count behavior. The explicit `ISettingsHost.Toast` adapter remains compatible with SettingsWindow lines 664, 865, 868. Existing actionable toasts retain valid current-button activation; stale activation is intentionally revoked.
- `OnPaneEdited` is reached by both panes through `NewPane` (`Layout.cs:233`); existing save and chrome effects remain. `HideToast` callers are the timer (`Layout.cs:194`), current toast button (`:714`), slot edit (`:271`), and overlay/editor Ctrl+Z (`Keys.cs:39,75`). Each correctly revokes the current action. Both Ctrl+Z routes still require `_undoArmed` and `_toastAction`.
- `ShowSaveFailurePrompt` has only the two production callers listed above, plus the selftest. `OnSaveFailureKeyDown` remains registered once by `BuildLayout`; its changed Enter dispatch does not change the other key routes.

## Scope and plan check

In plan. Every product hunk serves the approved Search key safety/Undo/hint, slot-clear Undo withdrawal, save-card Enter routing, delete counts, or their tests. The r2 body capture and button-generation guard implement the unchanged approved After behaviors and the explicit rework instructions. The query-selection guard is disclosed in the original report's Plan deviations and follows the approved no-word-editing condition. No unrelated icons, styles, defaults, renames, dependencies, or model changes.

The Must not change paths are preserved: stack-view Undo callback, editor slot-clear shortcut, Retry click, Escape, and Tab. Four CONTRACTS rows are wired to selftests; r2 extends the existing rows with real Store delete-save/Undo-save/reload and unavailable-body checks, and a retained-button activation check with reduced motion disabled. Test hooks remain in SelfTest. New tracking resets on toast replacement/hide; no permanent latch, new event/timer lifetime problem, or upward layer dependency found. Captured note bodies are bounded by the current Undo toast callback and released when its toast content is replaced or the window is released.

## Unproven risks

- Visible focus acquisition, pointer activation during an actual fade, and dev-copy hand checks were not exercised in this offline review; selftests invoke handlers and retained buttons directly.
- Repeated save prompts retain their original Button Click closures while Enter uses refreshed actions; all current production callers pass the same bound methods, so no present functional failure is established.

## Checks run

- Read the item card, r2 brief, both implementer reports, prior review, supplied r2 gate report, current startup/role/coding guidance, and complete `git diff 6902a44...HEAD -- product`; inspected changed hunks, surrounding paths, persistence acknowledgements, and callers.
- Reviewed HEAD `185db6bd7b3131f2350bbd16ad6c32146d8e5db1`: six product files, 375 insertions and 12 deletions. Initial working tree was clean.
- `git diff --check 6902a44...HEAD -- product`: exit 0, no output.
- Build/selftest/gate were not rerun. Supplied r2 gate evidence: `GATE: PASS`, build 0 warnings/0 errors, `CONTRACTS: 45 rows, 44 pass, 1 manual`, `SELFTEST PASS`, Format/Scope/Cleanup PASS. That report identifies an earlier dirty revision; these are supplied implementation-run results, not independently reproduced results at review HEAD.
- No temporary probe/build folder was created. No product files changed; no installed data access, app launch, `--quit`, network operation, branch switch, merge, or commit.

VERDICT: accept
