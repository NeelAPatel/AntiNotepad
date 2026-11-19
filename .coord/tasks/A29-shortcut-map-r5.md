# A29-shortcut-map-r5: built-in example notes no longer name Ctrl+Enter

Rework round of `A29-shortcut-map` (`docs/loop.md`). Read the item card `.coord/tasks/A29-shortcut-map.md` and `.coord/reports/A29-shortcut-map-r4.md` first.

Tier: Medium (same as the card)
Role: quick, because it is two text lines plus any test that pins them
Same branch and worktree. Never commit, switch branches, launch or kill AntiNotepad (a dev copy from `build/dev` runs: never touch it), run `--quit`, or edit `.coord/STATUS.md` or the cards. Offline only. 3-minute limit on every selftest. Build only into `build/A29-shortcut-map-check` and delete it after (a .NET `Directory.Delete` call works if `Remove-Item` is refused).

## Why this round
Round 4 removed the default Ctrl+Enter key for Checkbox or timer (user, 2026-11-19). Two built-in example notes still tell users to press it. Clicking a checkbox's box already toggles it (`Editor/Elements.cs`), so the examples should say so:
- `product/AntiNotepad/Model/Welcome.cs` line about "Press `Ctrl+Enter` to check off this example." -> "Click the box to check off this example." (same list item style, keep the `- [ ]` form).
- `product/AntiNotepad/Model/Showcase.cs` line "[ ] Ctrl+Enter ticks the line you're on" -> "[ ] Click a box to tick it" (keep the same shape and length class).
Check that nothing else (selftest, docs, Help) pins the old wording; update the pinned text where a test compares these notes. Do not change any other line.

## Change to the plan
None beyond the above.

## Files
- `product/AntiNotepad/Model/Welcome.cs`
- `product/AntiNotepad/Model/Showcase.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Design.cs`

## Done means
- The two lines are reworded; `Ctrl+Enter` appears nowhere in built-in note text.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A29-shortcut-map-r5 -Offline` passes.

Report: `.coord/reports/A29-shortcut-map-r5.md`: short Summary, Change list, Checks run.
