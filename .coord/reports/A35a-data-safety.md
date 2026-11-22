# A35a-data-safety

## 1 Summary

Search deletion and slot-clear Undo now preserve note text. The save-failure prompt sends Enter to the focused button. Offline selftests and the item gate pass.

## 2 Change list

- `UI/SearchOverlay.cs`: query Delete routing, permanent-delete Undo with original trash ordering, corrected trash hint, and accurate slot-skip notices.
- `UI/PadWindow.Navigation.cs` and `UI/PadWindow.Layout.cs`: withdraw a cleared-slot Undo toast when that slot is edited; route save-card Enter to Retry or Quit and refresh both actions on repeat failures.
- `SelfTest/UI.cs` and `SelfTest/Contracts.cs`: four data-safety contracts and focused regression checks, including selected query text and empty-query Ctrl+Backspace.

## 3 Behavior changes

- Plain Delete edits the query. Ctrl+Delete deletes search results only at query end with no selection; elsewhere the query handles it. Empty-query Ctrl+Backspace keeps its existing delete action.
- Permanent trash deletion has a seven-second Undo toast that restores notes at their captured trash indexes.
- Editing the cleared slot hides its Undo toast and clears its action before new text can be overwritten.
- Enter dispatches to the focused save-failure button. Repeated failures refresh both Retry and Quit actions.
- Slot-only deletion says “Slots are cleared in the editor.” Mixed deletion reports moved notes and skipped slots; note-only wording is unchanged.

## 4 Out-of-scope files

`UI/Kit.cs` and `.coord/STATUS.md` were not edited. The item card already differed from HEAD when work began (its Codex role line); that pre-existing change was preserved.

## 5 Plan deviations

No scope deviation. The query-end guard also checks for a selection, since Ctrl+Delete would delete selected query text and therefore would not be a no-op there.

## 6 Contracts

- Added `search-delete-undo`, `slot-clear-undo-withdrawal`, `save-failure-enter-focus`, and `search-delete-toast-count` rows.
- All four execute in `--selftest`; each passed after the fix.

## 7 Lens review

- Findings (proven): none.
- Behavior and scope: matches the approved plan; the existing stack Undo callback, Retry click, Escape, Tab, and editor slot-clear shortcut were not changed.
- Caller check: existing `Toast` callers remain compatible with the optional trailing slot-edit argument.
- Unproven: live focus acquisition and visible toast behavior remain part of the planned dev-copy checks.
- Verdict: accept.

## 8 Checks

### FAIL before

The new contracts were added and run before product changes:

```text
CONTRACTS: 45 rows, 40 pass, 1 manual
SELFTEST FAIL
contract search-delete-undo: plain Delete intercepted; Ctrl+Delete mid-query intercepted; query-end delete had no Undo
contract slot-clear-undo-withdrawal: restore=True; edited text kept=True; action withdrawn=False; undo disarmed=True
contract save-failure-enter-focus: latest retry=True; latest quit=False; focused route missing
contract search-delete-toast-count: slots-only='0 notes moved to trash'; mixed omitted '1 slot skipped'
```

### PASS after

```text
CONTRACTS: 45 rows, 44 pass, 1 manual
SELFTEST PASS
GATE: PASS
Build: 0 warnings, 0 errors
Format: PASS; Scope: PASS; Cleanup: PASS
```

The gate reported a non-blocking Size warning: `SelfTest/UI.cs` grew from 1,094 to 1,328 lines. Offline NuGet audit was disabled as required. The gate removed its `-gate` output.

The separate `build/A35a-data-safety-check` directory still contains copied dependency files. Recursive PowerShell removal was blocked by execution policy after confirming the target was a direct, non-reparse child of `build/`; `dotnet clean` removed project outputs but left those dependencies. Cleanup of that directory is incomplete.

## 9 Hand checks

Not performed. C-1, A-1, and C-2 require the slot dev copy. C-3 was covered by selftest; no save-failure data folder was damaged.
