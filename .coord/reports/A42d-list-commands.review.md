# A42d-list-commands lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches. No changed user-visible result, shared mutable state, timer, setting or default found.

- `ListEditing.Continue`, `Renumber` and `IsListLine` are new internal decision helpers. Their production callers are respectively `DocumentCommands.ContinueList`, `Renumber` and `IsListLine`; additional callers are the pure selftests. Existing recognition grammar and number limits are unchanged.
- `DocumentCommands.ContinueList` is called by PadView Enter and document selftests. Its insert callback retains PadView.Insert's insertion followed immediately by BringCaretToView. ClearLine returns true in every mode that reaches it, including List mode, as before. Outdent removes exactly two characters, then performs the inner renumber; PadView still performs the outer renumber.
- `DocumentCommands.Renumber` is called by ContinueList's Outdent branch, PadView Enter/Tab and document selftests. Precomputing decisions is equivalent here: preceding edits replace digits only, preserve line count, and cannot alter a later line's prefix, indent or original number. The count dictionary receives each desired number in the same order as before. Application preserves ascending edit order and obtains each line, match and digit offset from the live document (`DocumentCommands.cs:47-62`), including after digit-width changes. The document-change handler invalidates/rebuilds analysis and raises Edited; it introduces no intervening text rewrite.
- `DocumentCommands.IsListLine` is called by PadView Tab and delegates the original recognition. Existing `DocumentCommands.Indent` remains unchanged; its production Tab caller now calls it directly rather than through the removed PadView wrapper. Existing checkbox callers and behavior are unchanged.
- PadView keeps both RunUpdate scopes, reads CaretLine after continuation/indent, handles successful continuation before returning, and starts the timer only on declined continuation (`PadView.cs:674-694`). Ctrl+Enter and the Enter modifier/selection/completion gates are unchanged. The thin Insert remains at `PadView.cs:752-756`.

## Scope and plan check
In plan. Reviewed the empty committed product diff (`dev...HEAD`) plus all five uncommitted product files; the working tree is the implementation under review. No round briefs were present; the similarly named review brief was also read.

Every hunk serves the approved After or code-test/Contracts plan: decision extraction in ListEditing, live document application in DocumentCommands, PadView delegation and removed implementations/import, pure/document checks, and the new `list-commands-preserved` row. No unrelated edits or unlisted deviations found. Existing checkbox/indent implementation, TextCommands, caches, analysis, save timing and timer commands are untouched. No new lifecycle resources or dependency on UI were introduced. The tests cover text, grouped change notification and one-step Undo; caret anchors are bounded headless evidence.

## Unproven risks
- Live editor caret/scroll behavior and long-list input feel still require the approved dev-copy hand checks; anchor tests do not establish GUI behavior.

## Checks run
- Read item card, implementer report, gate report, review brief, local lens-review skill, relevant loop/dev rules, changed methods and all callers; traced document-change handling.
- `git diff dev...HEAD -- product`: empty; `git diff -- product`: five planned files reviewed; `git diff --check`: passed.
- Independent working-tree Debug build into `build/A42d-list-commands-review`, with AVALONIA_TELEMETRY_OPTOUT=1 and NuGetAudit=false: Build succeeded, 0 warnings, 0 errors. Online NuGet audit was not run.
- Independent executable `--selftest`: exit 0; LIST EDITING 48 checks; LIST COMMANDS 102 checks; CONTRACTS: 13 rows, 12 pass, 1 manual; SELFTEST PASS.
- Supplied offline gate report: GATE: PASS; build/selftest/seam/format/scope/cleanup PASS; size warning for the planned SelfTest.cs growth. Full gate was not repeated.
- Review build cleanup was attempted against the verified absolute task folder, but automatic approval review rejected Remove-Item with "blocked by policy". `build/A42d-list-commands-review` remains and needs manager cleanup. No product source edits, branch operations, commits or GUI launch performed.

VERDICT: accept
