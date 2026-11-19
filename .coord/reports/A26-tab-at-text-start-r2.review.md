# A26-tab-at-text-start-r2 lens review

## Findings (proven)
1. [major] product/AntiNotepad/Editor/DocumentCommands.cs:31 - Selected-list indentation repeatedly scans the entire list and blocks the editor for seconds on a modest note. `PadView.cs:867` calls this synchronously from its key handler; after indenting the selected lines, this loop invokes `Renumber` for every one. `ListEditing.cs:127-134` finds and traverses the whole contiguous block on every invocation, even for bullets without numbers. A Release console probe linked the current `DocumentCommands.cs`, `ListEditing.cs` and `Modes.cs`, using the local dev copy's AvaloniaEdit.dll: selecting all 1,000 `- item` lines took 0.632 seconds; 4,000 lines (about 28 KB) took 6.895 seconds. Both produced correct indentation and one Undo restored the input. These timings exclude editor rendering and callbacks. Smallest fix: collect affected contiguous list blocks after the indentation pass and renumber each block once, preserving fresh-number behavior for moved numbered groups; add a bounded large-selection check that catches repeated block scans.

## Behavior changes check
Report's list vs the diff: matches. Plain Ctrl+brackets now reach the editor; they edit touched list lines and leave an all-plain selection unhandled. Ctrl+Shift+brackets retain the existing fold path. Tab decisions and Ctrl+Tab insertion match the approved rules. The repeated renumbering cost is the reliability defect above.

Caller check: new `IndentListLines` callers are `PadView.OnKeyDown` (brackets at line 867, single-line Tab at line 879) and `SelfTest/Lists.cs:197,201`. Single-line Tab retains the old indent/renumber sequence; selected edits retain one undo group but introduce the finding above. `TabDecision` is called only by PadView and list selftests; `IsAtTextStart` only by that decision and its selftests. Existing `Indent`, `Renumber`, continuation and checkbox callers keep their signatures and behavior. No new shared state, deferred work, cleanup ownership, native calls or layer dependencies were introduced.

## Scope and plan check
In plan. Every product hunk serves the card's After, tests/contract/help, or the r2 shortcut amendment; the design hash follows the help change. `docs/decisions.md` records the explicitly authorized navigation decision. No unrelated hunks or unlisted implementation deviations found. Wrap hanging indent, Enter continuation and Ctrl+Enter paths are unchanged. Renumbering remains functionally the existing algorithm; its repeated invocation for selected lines causes the finding. The report explicitly defers routed-key verification to hand checks, as allowed by r2.

## Unproven risks
- Actual routed keys, completion interaction, checkbox marker caret positions and selection retention still need the stated dev-slot hand checks; no UI was launched.

## Checks run
- Read the card, r2/review briefs, both implementer reports, supplied gate report, complete product diff and changed-file context; searched callers and checked editor/window key routing and AvaloniaEdit caret/selection behavior.
- `git diff --check`: pass. No uncommitted product changes were present at review start.
- Temporary Release probe of current source: pass for indentation and one-Undo restoration at 1,000 and 4,000 bullet lines; responsiveness failure measured at 0.632 s and 6.895 s respectively. No product files were edited.
- Supplied r2 gate: PASS, build 0 warnings/errors, SELFTEST PASS (34 contracts: 33 pass, 1 manual). These are supplied gate results, not a full build/selftest rerun by this reviewer.
- Cleanup blocked: sandbox policy rejected both guarded and literal-path PowerShell deletion of the verified `build/A26-tab-at-text-start-review` folder. The temporary probe folder remains and requires cleanup outside this run.

VERDICT: rework
