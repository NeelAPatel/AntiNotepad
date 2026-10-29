# A42c-checkbox-indent lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches. Existing command behavior is preserved; the new internal helpers and regex ownership are disclosed.

- `PadView.ToggleCheckbox` callers: checkbox press in `Editor/Elements.cs:72` and Ctrl+Enter in `Editor/PadView.cs:672`. Both still use the facade. The adapter preserves the invalid-line no-op, relative checkbox-state offset, bullet replacement span, and insertion operation. No caret or selection setter was added.
- `PadView.Indent` has one caller, the Tab branch at `Editor/PadView.cs:685`. It still executes synchronously inside the existing `RunUpdate`, followed by `Renumber(CaretLine, fresh: !shift)` using the post-edit caret. Inserts and removals retain their original offsets, lengths and document APIs. The new adapter also ignores invalid indent line numbers; this is unreachable through the existing caller, whose preceding `IsListLine(CaretLine)` already obtains a valid document line.
- `ListEditing.Checkbox` readers: `Elements.cs:51,62` (hit-testing and glyph length), `Renderers.cs:409` (box position/state), `Colorizers.cs:80` (Done coloring), and the extracted toggle helper. Pattern, options and capture groups are unchanged; no `CheckboxGenerator.Box` references remain.
- New helper callers are confined to `DocumentCommands`, the PadView facades and selftests. No new mutable shared state, deferred work, event subscriptions, persistence paths or UI dependency was introduced. Existing timer routing, modifier handling, selection gating and normal Tab fall-through are unchanged.

## Scope and plan check
In plan. Reviewed every hunk in the seven modified tracked product files and the untracked `Editor/DocumentCommands.cs`; `git diff dev...HEAD -- product` is empty. No round briefs exist.

All eight product files are in the card's Files list. Pure edit decisions, document application, the two thin facades, regex ownership/readers, selftests, LAYERS guards and the CONTRACTS row each serve the approved After/test plan. `IsListLine`, `ContinueList`, `Renumber`, `Insert` and TextCommands grammar remain in their original scope. No unrelated styling, defaults, renames or unlisted implementation deviations were found. The report discloses its remaining check-build directory and offline audit limitation.

The tests cover pure decisions, real TextDocument text/edit count/single Undo, nonzero offsets, LF/CRLF, invalid checkbox lines, whitespace variants and regex captures. The new contract invokes the document checks; pure checks remain invoked by the selftest runner. Caret, selection, click geometry and combined indent-plus-renumber Undo are explicitly assigned to the planned hand checks rather than claimed as automated evidence.

## Unproven risks
- The planned dev-copy hand checks remain outstanding: caret/selection, click geometry, timer routing and combined indent-plus-renumber Undo have not been exercised live by this review.

## Checks run
- Read the lens-review skill, item card, implementer report and supplied gate report; inspected both requested diffs, the untracked adapter, surrounding code and all command/regex callers. Applied all nine checklist items.
- `git diff --check`: PASS (no output).
- `pwsh -NoProfile -File tools/check-platform-seam.ps1`: PASS; `PLATFORM-SEAM source guard: 0 violations in 50 files`.
- Supplied gate evidence (not rerun here): `GATE: PASS`; build 0 warnings/errors; `SELFTEST PASS`; `CONTRACTS: 12 rows, 11 pass, 1 manual`. Size warning is the planned SelfTest growth.
- No build, executable, UI session or probe was run by this review; no review build folder was created. Product files, branch, task card and STATUS were not changed.

VERDICT: accept
