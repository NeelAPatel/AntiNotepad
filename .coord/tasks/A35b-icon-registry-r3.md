# A35b-icon-registry-r3: five icon picks from the hand test

Rework round of `A35b-icon-registry` (`docs/loop.md`). Read the item card `.coord/tasks/A35b-icon-registry.md` first: its Plan, Files,
Contracts and "For Codex" section still apply unless this file says otherwise. r2 is built and committed; build on it.

Tier: Medium
Role: quick, because these are registry code swaps, two new entries and one ternary, in known places.

## Why this round
Hand test of r2 (user, 2026-11-21, verbatim): "Doesnt quite hit the mark: * Folding * Inconsistent Split view icon *
Storage Icon * Slots/Stack. Everything else is good enough" and "Trash icon (not delete) can it be recycle bin?"
Lab `build/a35b-icon-lab4.html` (not committed). User on the picks, 2026-11-21: "recycle arrows makes sense. everything
else is good"

## Change to the plan
In `product/AntiNotepad/Icons.cs` (confirm every Fluent name with
`python -I .coord/reports/A35-ux-pass-s2-fontcodes.py product/AntiNotepad/Assets/Fonts/FluentSystemIcons-Regular.ttf <HEX>`):
- `Folding`: Fluent `0xE32F` `ic_fluent_chevron_up_down_20_regular`.
- `Split`: Segoe `0xE90D` (two-pane window, divider right). Pad bar and pad menu already use `Icons.Split`.
- `SidePane`: Fluent `0xE8BD` `ic_fluent_panel_right_20_regular` (matches the pad's split picture).
- `StorageTab`: Fluent `0xE466` `ic_fluent_database_20_regular`. `NotesFolder` keeps the folder.
- New `Stack`: Segoe `0xE81E` (stacked layers), meaning "Show the stack view".
- New `OpenTrash`: Fluent `0xEF12` `ic_fluent_recycle_20_regular`, meaning "Open trash". `Delete` meaning becomes
  "Delete a note or selection". Add both new entries to `Icons.All`.

Callers:
- `UI/PadWindow.Bar.cs` swap button shows where it goes, matching its tooltip: "Show slots" draws `Icons.Grid`,
  "Show stack" draws `Icons.Stack` (today it draws `List`/`Grid`, the current view). Both the initial
  `BuildSwapButton` icon and the `Data.ViewSlot` ternary change.
- `UI/SearchOverlay.cs`: the button that opens trash draws `Icons.OpenTrash` (the `trash ? Back : Delete` choice
  becomes `trash ? Back : OpenTrash`). Change nothing else in this file (a sibling item edits its behavior).
- The search overlay's list/grid results toggle keeps `List`/`Grid`.

Must not change: every other icon, sizes, tooltips, colors, behavior.

## Files
- `product/AntiNotepad/Icons.cs`
- `product/AntiNotepad/UI/PadWindow.Bar.cs`
- `product/AntiNotepad/UI/SearchOverlay.cs`

## Done means
Offline checks only (`docs/agents.md`, "What Codex can and cannot check").
- The registry holds the six code changes and two new entries with confirmed glyph names; callers draw them as above.
- Build 0 warnings, `SELFTEST PASS` (icon contract passes), `pwsh -NoProfile -File tools/gate.ps1 -Task A35b-icon-registry-r3 -Offline` passes.

Report: `.coord/reports/A35b-icon-registry-r3.md`, in the card's report format.
