# A32-selection-convert-menu-r2: close the submenu when another menu row is hovered

Rework round of `A32-selection-convert-menu` (`docs/loop.md`). Read the item card `.coord/tasks/A32-selection-convert-menu.md` first: its Plan, Files, Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium, same as the card.
Role: terra, because one small UI fix in one file.

## Why this round
Proven review finding (`.coord/reports/A32-selection-convert-menu.review.md`, major, `UI/PadMenu.cs` ~178): the "Convert to list" submenu opens on hover but never closes when the pointer moves to another parent row (Undo, Settings, and so on); it only closes on a child choice, detach, or Left/Esc. Smallest fix: the parent menu tracks the open submenu and closes it when a sibling row is entered, while the pointer may still travel from the opener into the submenu popup (do not close on leaving the opener alone). Keep the Right/Left/Esc behavior and the native-popup light-dismiss behavior unchanged.

## Change to the plan
None.

## Files
- `product/AntiNotepad/UI/PadMenu.cs`

## Done means
- Hovering a sibling row closes an open submenu; moving from the opener into the submenu keeps it open.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A32-selection-convert-menu-r2 -Offline` passes.

Report: `.coord/reports/A32-selection-convert-menu-r2.md`, in the card's report format.
