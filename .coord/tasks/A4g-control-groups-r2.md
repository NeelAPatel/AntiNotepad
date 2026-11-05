# A4g-control-groups-r2: complete the control map

Rework round of `A4g-control-groups` (`docs/loop.md`). Read the item card `.coord/tasks/A4g-control-groups.md` first: its Plan, Files,
Contracts and "For Codex" section still apply (Part 1 only, report only, no product file changes) unless this file says otherwise.

Tier: Medium (same as the card)
Role: terra, because completing the map is careful reading across many files

## Why this round
Round 1 (`.coord/reports/A4g-control-groups.md`) says its own map is an incomplete draft: "does not yet enumerate every
instance and value, particularly Settings previews and editor drawing", and no design dump was run. Check accepted the
ChromeTokens alias table (values match `Theme/ChromeTokens.cs`, `Shape.cs`, `Typography.cs`, `Paint.cs`). The rest fails the
card's Done means: one section per group with the sites, the values with token or literal, and the disagreements.

## Change to the plan
None. Rewrite `.coord/reports/A4g-control-groups-map.md` in place, keeping the draft's good content. Required:
- Per group (text button, icon button, toggle, dropdown, text field, slider, card, popup and menu, badge, pill, overlay panel,
  toast, text roles): a table with one row per instance (file:method, who calls it) and columns size, radius, padding/margin,
  font size and weight, fill and border (alpha), shadow, each cell marked with its token or `literal`. Cover Settings
  preview controls, `PadMenu`, `SearchOverlay`, `HelpOverlay`, `PadWindow.*.cs`, `Editor/Renderers.cs` and any other
  control factory you find (grep for `new Border`, `new Button`, `CornerRadius`, `FontSize` in `UI/` and `Editor/`).
- A "Disagreements" list per group naming the instances and values that differ.
- Build into `build/A4g-control-groups-check` and run `--design-dump` to confirm real values if useful, then delete the folder.
- Slider: state the grep that found none.
- Keep the proposed group-token table (name | points at | used by), now covering every group, and the alias table.
- Add a short "Proposed group names" section at the top for the user: the group names in one list.
- Remove the "draft / not complete" disclaimer only if the map really is complete; list any residual gaps under "Gaps".

## Files
- `.coord/reports/A4g-control-groups-map.md`

## Done means
- Map complete as above; no product file changed; gate passes.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A4g-control-groups-r2 -Base feat/A4-design-tokens -Offline` passes.

Report: `.coord/reports/A4g-control-groups-r2.md`, in the card's report format.
