# A29-shortcut-map-r4: four hand-test fixes

Rework round of `A29-shortcut-map` (`docs/loop.md`). Read the item card `.coord/tasks/A29-shortcut-map.md` first; its Plan, Files, Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium (same as the card)
Role: worker, because the fixes touch key routing, labels, Settings and their tests
Same branch and worktree. Never commit, switch branches, launch or kill AntiNotepad (a dev copy from `build/dev` is running: build only into `build/A29-shortcut-map-check`, never touch `build/dev`), run `--quit`, or edit `.coord/STATUS.md` or the cards. Offline only. Run every selftest with a 3-minute limit. Delete every `build/A29-shortcut-map-*` folder you create (PowerShell `Remove-Item -Recurse -Force` if bash `rm` is refused; say so in the report if it is refused).

## Why this round
The user hand-tested the dev copy on 2026-11-19 and asked for these four changes (round 4, approved by the user):
1. **F1 does not toggle Help.** F1 opens Help; pressing F1 again while the Help overlay is open must close it (like the Search key and `ToggleSearch`). The Help action is bound to F1 by default, and any key the user binds to Help toggles the same way. The key must still reach the window handler while Help has focus. Add a selftest that opens, closes and reopens Help through the real window handler (follow how the r3 Search-toggle selftest does it).
2. **Text size up shows only "Ctrl".** The label for the Ctrl++ default (Help sheet, app menu, tooltips, Settings row) loses its plus sign. Find where key text is split or joined (`ShortcutMap` label code, `GetLabel`) and fix it so a key named `+` shows as `Ctrl++` everywhere; check every other default for the same loss (`Ctrl+-`, `Ctrl+\`, `Ctrl+Shift+[`, `Ctrl+Shift+]`). Add a selftest that every default's label round-trips (label text parses back to the same gesture).
3. **Checkbox or timer must not have a default key.** Remove the Ctrl+Enter default; the action stays in the table and stays bindable in Settings (empty default, like Settings or Delete note). Ctrl+Enter then does nothing special by default. Update the Help sheet, `docs/shortcuts.md`, the card-listed defaults in tests and the contract row text. Plain Enter list continuation and Tab behavior must not change.
4. **Wrong icon for Shortcuts.** The Settings sidebar tab and the Shortcuts page rows use glyph 0xf6ed, which was a guess. The app already has a keyboard glyph for the System page row "Keyboard shortcuts" (0xf4b8 in `UI/SettingsWindow.cs`). Use that glyph for the Shortcuts tab (and any icon on that page). Verify in the icon font / `Glyph` helper that it really is a keyboard; if no real keyboard glyph exists, remove the icons from the Shortcuts tab and page rather than guess. Do not change any other existing icon.

## Change to the plan
Defaults list in the card loses "Checkbox or timer Ctrl+Enter" (user, 2026-11-19). Help toggles on F1.

## Files
- `product/AntiNotepad/UI/Shortcuts.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `docs/shortcuts.md`

## Done means
- The four changes work; selftest covers 1, 2 and 3 (and the glyph check if cheap).
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A29-shortcut-map-r4 -Offline` passes (update the pinned `DesignHash` only if the layout dump changes for these reasons).

Report: `.coord/reports/A29-shortcut-map-r4.md`: Summary, Change list, Behavior changes, Checks run.
