# A29-shortcut-map-r2: fix the proven review findings

Rework round of `A29-shortcut-map` (`docs/loop.md`). Read the item card `.coord/tasks/A29-shortcut-map.md` first: its Plan, Files, Contracts and "For Codex" section still apply unless this file says otherwise. Also read `.coord/reports/A29-shortcut-map.review.md` (the proven findings, with probes).

Tier: Medium (same as the card)
Role: worker, because the fixes touch shared key-routing state and its tests
Same branch and worktree. Never commit, switch branches, launch or kill AntiNotepad, run `--quit`, or edit `.coord/STATUS.md` or the cards. Offline only. Run every selftest with a 3-minute limit (a hung selftest cost a whole earlier run).

## Why this round
The Codex review (verdict: rework) proved:
1. **Major, `UI/Shortcuts.cs` BlockedReason:** `Shift+Backspace` is recordable; AvaloniaEdit binds it to Backspace, so a New note override would steal character deletion. Reserve it, and add a selftest case. Check the same way for the other keys AvaloniaEdit binds twice (`third_party/AvaloniaEdit/Editing/EditingCommandHandler.cs`: for example Shift+Delete, Ctrl+Insert, Shift+Insert, Ctrl+Shift+Z) and reserve any that are missing.
2. **Major, `UI/Shortcuts.cs` Refresh:** overrides are validated one by one in table order after the overridden defaults were removed. `new-note = Ctrl+F` with `search = Ctrl+F` (or `search = garbage`) leaves Search unbound and Ctrl+N dead. Resolve the whole override set together before committing routes: a bad or duplicate override falls back to that action's default (or unassigned if the default is taken by a valid override), valid swaps and explicit clears still work, nothing gets lost silently. Add both paired cases to the selftest.
3. **Minor, `UI/PadWindow.Navigation.cs` PromoteToSlot:** the toast says `Ctrl+{slot} opens it`, but Open slot has no default key. Build the hint from the effective Open slot binding and omit it when unassigned.
4. **Minor, `UI/PadWindow.Keys.cs` `ShortcutDispatchOverride`:** a test hook in product code. Remove it. Test dispatch without running side effects another way (for example resolve the route for each default key through the same lookup the handler uses and assert the action, plus a few safe real actions); no hook field in product classes.
5. **Minor, coding standard:** add one-line `///` summaries to `ShortcutsPage`, `ShortcutControl`, `ApplyShortcut`, `SaveShortcut`, `ResetShortcut`, `ResetAllShortcuts`, `ConfirmShortcutTake` (`UI/SettingsWindow.cs`) and the collision and routing helpers in `UI/Shortcuts.cs`.

## Change to the plan
None. Behavior removed versus dev is unchanged (the review listed it).

## Files
- `product/AntiNotepad/UI/Shortcuts.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `docs/shortcuts.md`

## Done means
- Findings 1 to 5 fixed, each with a selftest case where the finding was behavioral; `docs/shortcuts.md` table still equals the map.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A29-shortcut-map-r2 -Offline` passes. Delete `build/A29-shortcut-map-*` folders after.

Report: `.coord/reports/A29-shortcut-map-r2.md`, in the card's report format (sections 1 to 9).
