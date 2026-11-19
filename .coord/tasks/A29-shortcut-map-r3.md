# A29-shortcut-map-r3: Search key toggles

Rework round of `A29-shortcut-map` (`docs/loop.md`). Read the item card `.coord/tasks/A29-shortcut-map.md` first (see "Search key toggles"); its Plan, Files, Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium (same as the card)
Role: quick, because it is a small change in two known places
Same branch and worktree. Never commit, switch branches, launch or kill AntiNotepad, run `--quit`, or edit `.coord/STATUS.md` or the cards. Offline only. Run every selftest with a 3-minute limit. Delete every `build/A29-shortcut-map-*` folder you create (use `Remove-Item -Recurse -Force` in PowerShell if bash `rm` is refused).

## Why this round
User, 2026-11-19: "Ctrl F should act as a toggle for the global search panel." Today the Search key (and its two unbound aliases) always opens a fresh search, even when search is already open; only the toolbar button toggles (`PadWindow.ToggleSearch` in `UI/PadWindow.Keys.cs`).

## Change to the plan
New behavior: pressing the Search key (and the Search alias keys if bound) opens search, and pressing it again while the search overlay is open closes it, like the toolbar button. The Search key must still work while the search overlay has focus (the overlay's own handler already passes window commands through; check the window handler line that exempts Search, Settings and aliases from the overlay block). Current-note search is not part of this.

## Files
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/Shortcuts.cs`
- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `docs/shortcuts.md`

## Done means
- The key press path calls the toggle; a selftest proves Search open, then the same key closes it, then the key opens it again (no hook in product code; follow how the r2 selftest drives the real handler).
- Help and docs wording still match the map (say "toggles" in the action description if it helps).
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A29-shortcut-map-r3 -Offline` passes.

Report: `.coord/reports/A29-shortcut-map-r3.md`, short: Summary, Change list, Checks run.
