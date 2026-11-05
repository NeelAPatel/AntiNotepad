# A4g-control-groups-r3

## Summary
Added per-instance tables for twelve groups, including the editor checkbox and motion-preview frame, to the existing map. The inventory is still incomplete and remains labelled a draft; it is not ready for the Part 2 checkpoint.

## Change list
| File | Change |
| --- | --- |
| `.coord/reports/A4g-control-groups-map.md` | Added source-based instance tables and identified remaining gaps. |
| `.coord/reports/A4g-control-groups-r3.md` | This report. |

## Behavior changes
None (report only).

## Out-of-scope files
None.

## Plan deviations
Did not complete the required file-by-file inventory. Remaining detailed cross-checks and rows are in `UI/Kit.cs`, `UI/SettingsParts.cs`, `UI/SettingsWindow.cs`, `UI/PadMenu.cs`, `UI/SearchOverlay.cs`, `UI/HelpOverlay.cs`, `UI/PadWindow.Layout.cs`, `UI/PadWindow.cs`, `UI/MotionPreview.cs`, and `Editor/Renderers.cs`. `UI/PadWindow.Navigation.cs` and `UI/NoteSwipe.cs` have no visible control-building sites in the search performed. Preview instances remain in the separate additional table; the slider needs its empty table, each disagreement needs an explicit instance comparison, and callers and state values need a full cross-check. No design dump was run.

## Contracts
None.

## Lens review
Not run; the requested map remains incomplete.

## Checks run
- Offline gate: **PASS**; Build, Selftest, Seam, Tokens, Format, Scope, Size and Cleanup passed; LoopLint skipped.
- `git diff --name-only feat/A4-design-tokens -- product/AntiNotepad`: no output.
- Slider search (`rg -n '\bSlider\b|\bRangeBase\b|\bTrackBar\b' product/AntiNotepad/UI product/AntiNotepad/Editor`): no matches.

## Hand checks for the user
None (report only).
