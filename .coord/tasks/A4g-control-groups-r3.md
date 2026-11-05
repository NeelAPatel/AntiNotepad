# A4g-control-groups-r3: finish the control map, file by file

Rework round of `A4g-control-groups` (`docs/loop.md`). Read the card `.coord/tasks/A4g-control-groups.md` and `.coord/tasks/A4g-control-groups-r2.md`
first; Part 1 only, report only, no product file changes.

Tier: Medium
Role: terra, because it is careful reading across many UI files

## Why this round
Rounds 1 and 2 both stopped and declared the map "incomplete" (no per-instance tables for most groups, no Disagreements per
group). Do not stop early again: this round must finish. "Complete" here is bounded and mechanical, so you can finish it:

1. Work through these files in order, and for EACH one grep for every control-building site (`new Border`, `new Button`,
   `new TextBox`, `new TextBlock`, `CornerRadius`, `FontSize`, `BoxShadow`, `Padding`, `Margin`, factory methods):
   `UI/Kit.cs`, `UI/SettingsParts.cs`, `UI/SettingsWindow.cs`, `UI/PadMenu.cs`, `UI/SearchOverlay.cs`, `UI/HelpOverlay.cs`,
   `UI/PadWindow.Layout.cs`, `UI/PadWindow.cs`, `UI/PadWindow.Navigation.cs`, `UI/NoteSwipe.cs`, `UI/MotionPreview.cs`,
   `Editor/Renderers.cs`. Skip files with no visible control.
2. For every group in the card, give a table with one row per factory or distinct call site (not per call with identical
   arguments; list those callers in one cell). Columns: instance (file:method), callers, size, radius, padding/margin,
   font size/weight, fill and border (alpha), shadow. Each cell: the token name or `literal <value>`. A cell the code does
   not set is `-`. Template-inherited Avalonia defaults may be `default`; do not investigate templates further.
3. Under each table, a "Disagreements" list: the instances and values that differ.
4. Keep the proposed group-names list, the group-token table (cover every group) and the ChromeTokens alias table
   (already verified correct by Check: keep as is).
5. Replace the draft disclaimer with a one-line scope note. "Gaps" lists only things genuinely not covered (name them); an
   empty or short Gaps is the goal. Fold the "Additional inspected instances" table into the right groups.
6. A design dump is optional; skip it unless a value cannot be read from source.

## Change to the plan
None.

## Files
- `.coord/reports/A4g-control-groups-map.md`

## Done means
- Every group has a per-instance table and a Disagreements list; the map is not labelled a draft.
- No product file changed; `pwsh -NoProfile -File tools/gate.ps1 -Task A4g-control-groups-r3 -Base feat/A4-design-tokens -Offline` passes.

Report: `.coord/reports/A4g-control-groups-r3.md`, in the card's report format. If you still cannot finish, say exactly which
files remain, but write the map as far as you got before reporting.
