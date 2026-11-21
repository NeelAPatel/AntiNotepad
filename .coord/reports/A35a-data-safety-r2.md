# A35a-data-safety-r2

## 1 Summary

Permanent-delete Undo now retains available note bodies through save and reload. A toast button can run only while its toast action is current, so edits cannot be overwritten by a withdrawn Undo. Offline selftests and the r2 gate pass.

## 2 Change list

- `UI/SearchOverlay.cs`: capture each trashed note body before removal; restore available text through `Note.Text` during Undo.
- `UI/PadWindow.Layout.cs`: associate each toast button with a generation and current action; stale button clicks return without invoking the captured action.
- `SelfTest/UI.cs`, `SelfTest/Store.cs`, and `SelfTest/Contracts.cs`: extend the existing search-delete and slot-clear contract checks. The Store check exercises a real temporary Store through delete-save-Undo-save-reload and verifies unavailable bodies remain metadata-only.

## 3 Behavior changes

- Undo restores notes to their original trash positions and marks available bodies dirty so the next save rewrites them.
- Notes with unavailable bodies return as metadata; Undo does not replace them with empty text.
- A withdrawn or replaced toast button cannot run its captured action. Ctrl+Z still uses `_undoArmed`.

## 4 Out-of-scope files

`Model/Store.cs`, `.coord/STATUS.md`, and both task cards were not edited. No interactive app instance was launched and no commit was created.

## 5 Plan deviations

No plan deviation. The existing contract rows were extended; no contract row was added. The body persistence check also covers an unavailable body to guard the metadata-only rule.

## 6 Contracts

- `search-delete-undo`: query routing, trash ordering, saved-body persistence across delete save, Undo save, and reload; unavailable bodies remain metadata-only. PASS.
- `slot-clear-undo-withdrawal`: before-edit Undo still restores the slot; after editing, the existing button is invoked with reduced motion off and leaves the new text intact. PASS.

## 7 Lens review

- The two proven findings in `.coord/reports/A35a-data-safety.review.md` are addressed.
- The search Undo captures text before removal and assigns it only when `BodyUnavailable` is false.
- The toast handler checks both its generation and current action. Manual diff review found no additional issue in these changes.

## 8 Checks

### FAIL before

Before Fix 1, the test-only build reproduced the saved-body loss:

```text
CONTRACTS: 45 rows, 42 pass, 1 manual
SELFTEST FAIL
contract search-delete-undo: search delete safety: permanent-delete Undo must preserve a saved body across delete save, Undo save, and reload
```

After Fix 1 and before Fix 2, the normal-motion button test still reproduced the stale action:

```text
CONTRACTS: 45 rows, 43 pass, 1 manual
SELFTEST FAIL
contract slot-clear-undo-withdrawal: slot clear undo safety: restore=True; normal motion=True; edited text kept=False; action withdrawn=True; undo disarmed=True; stale button present=True; toast visible=False
```

### PASS after

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
CONTRACTS: 45 rows, 44 pass, 1 manual
SELFTEST PASS
GATE: PASS
Format: PASS; Scope: PASS; Cleanup: PASS
```

`git diff --check` passed. The gate reported a non-blocking Size warning for the long selftest files (`Store.cs` 648 -> 697 lines; `UI.cs` 1094 -> 1334 lines, measured against the gate base). Offline NuGet audit was disabled as required. `build/A35a-data-safety-check` was deleted and confirmed absent.

## 9 Hand checks

Not performed; this round was restricted to offline checks. The normal-motion button contract invokes the retained button in selftest, but visible fade and pointer behavior remain for the item chat's dev-copy hand check.
