# A55-split-view-clip lens review

## Findings (proven)
None.

## Behavior changes check
Report's list vs the diff: matches.

- `AddPaneHosts` (`UI/PadWindow.Layout.cs:68`) is called only by `BuildLayout` and `SplitPaneClippingChecks`. Both receive stationary Borders containing the original panes in columns 0 and 2, with the divider in column 1. Hosts add no padding or background.
- `ApplyPaneSplit` (`UI/PadWindow.Layout.cs:78`) is called by `BuildLayout`, `BindPanes`, and `SplitPaneClippingChecks`. All callers supply the matching hosts and right pane. Column definitions and pane/divider visibility retain their previous meaning; right-host visibility follows split state, and left-host clipping is disabled in single view. The selftest covers split -> single -> split.
- Host references are initialized during `BuildLayout`, before constructor `BindPanes` and all navigation callers. They remain owned by the window's visual tree; no new subscriptions, timers, caches, persistence, or disposal obligations appear.
- `BindPanes` callers (constructor, Navigate, NewNote, OpenFreshNote, ShowStack, OpenSlot, PromoteToSlot, ToggleSplit, CycleSplitSlot, DeleteCurrent, RestoreFromTrash, SearchChangedData, and JumpTo) retain binding, focus, and arrival behavior. `NoteSwipe.Show`/SpringBack/Fire and `PadView.Arrive` still transform the original PadViews. Focus still targets `_active.FocusText()`.
- Top blur still observes the original panes' visibility, opacity and transforms and uses `TranslatePoint` through their ancestors. `_panes` size and opacity consumers retain the same grid. No consumer found that requires a PadView's immediate parent to be that grid. MotionPreview is unchanged.

## Scope and plan check
In plan. Reviewed every hunk in `git diff dev...HEAD -- product`; working-tree and staged product diffs are empty. The four changed product files exactly match the card's Files list.

- Layout field, host construction, and split-state helper serve the stationary clipping-host and visibility requirements.
- Navigation delegates the existing split layout/visibility updates to the helper.
- The selftest and CONTRACTS row serve the approved structural regression test and manual pixel-containment check.
- No unrelated icons, tokens, styles, defaults, renames, or unlisted implementation deviations. Divider geometry and pager layering, motion distances/timing/fades, binding/undo, top blur source, and MotionPreview remain unchanged. Disabling left-host clipping in single view follows the explicit outer-edge scope decision.

## Unproven risks
- Actual pixel containment during swipe/arrival, caret/selection clipping, and blur appearance require the card's dev-copy hand checks; this review did not launch a GUI.

## Checks run
- Read the item card, implementer report, and gate report; no round briefs present. Applied all nine checklist items to the four changed files and inspected affected callers/consumers.
- Independent isolated build: `dotnet build product/AntiNotepad/AntiNotepad.csproj --artifacts-path build/A55-split-view-clip-review/artifacts -o build/A55-split-view-clip-review/bin -p:NuGetAudit=false --ignore-failed-sources`, with `AVALONIA_TELEMETRY_OPTOUT=1`: Build succeeded; 0 warnings, 0 errors. Intermediate and output paths were under the review folder.
- Isolated executable `--selftest`, with `ANTINOTEPAD_DATA` under the review folder: exit 0; `CONTRACTS: 11 rows, 10 pass, 1 manual`; `SELFTEST PASS`. This includes the structural split-pane-clipping and existing split-divider-above-pager checks; it does not prove rendered pixels.
- Supplied gate report: GATE PASS, seam PASS, scope PASS; advisory SelfTest.cs growth warning (+30 lines). Gate was inspected, not rerun by this review.
- `git diff --check`: PASS. Product working tree remained clean after build/selftest. No product files edited, branch operations performed, or resident app launched/stopped.
- Cleanup attempted for the resolved, exact `build/A55-split-view-clip-review` target with containment and reparse-point guards. Automatic approval review rejected the removal as "blocked by policy" before execution. The review folder remains for manager cleanup; no further deletion attempted.

VERDICT: accept
