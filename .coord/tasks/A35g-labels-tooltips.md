# A35g-labels-tooltips: every control says what it does and its key

Item card (`docs/loop.md`). Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB.

Tier: Medium · settings-section (text plus small Settings behavior: dropdown names, reset Undo, search expanders)
State: ready to merge (gate PASS, hand test PASS; handed back to the A35-ux-pass chat)
Branch: `fix/A35g-labels-tooltips` in `.claude/worktrees/fix/A35g-labels-tooltips` (from `docs/A35-ux-pass`; merges back there)

## Request
User via the A35-ux-pass chat, 2026-11-20 (verbatim): "kick off anything else you want". Batch 6 of `.coord/reports/A35-ux-pass-s3.md`.

## Findings
Source: `.coord/reports/A35-ux-pass-s3.md` and its Codex reads; code re-read here 2026-11-20. Evidence lines are in that report.
- **Fix:** A-6 (the swap button caches by view only, so toggling split must refresh it too), A-8, B-6, B-9 Undo, B-10, C-4, C-5 tooltips, C-7.
- **Partial:** A-7: give Settings `Ctrl+,` (the welcome note already promises it); the welcome note's slot-clear tip names the Clear slot button, not an unbound key; keep other actions unbound. B-3: a matched expander shows all its rows. B-8: description in user words only, because `docs/decisions.md` (A20-overscroll-speed) locks the one-choice row.
- **Defer to `A35e-targets-keyboard`** (targets and keyboard, lab): A-9 Delete/Clear gap, C-5 Delete next to Paste, A-7 hidden-bar cue. **Defer to a later `settings-search` idea:** B-3 option-label search. **Defer to `A35c-design-contract`:** B-9 dimming controls whose prerequisite is off (a visual state; needs a lab).
- **Moved out at Approve:** A-4. The user wants a click on the count to open a note stats popup instead (new idea `note-stats-popup`, routed to the A35 chat); the count stays as it is here.
- **Drop:** C-6 tray keys. Pad shortcuts do not work in the tray menu, so showing them would mislead.
- **Overlap:** A35a, A35b and A35d edit most of these files. A35a and A35b are merged and rebased in; build now. The item chat rebases onto A35d after it lands.

## Plan
- **After:** every row of `.coord/reports/A35g-labels-tooltips-strings.md` (the string table is the lab). Behavior: dropdown names update on pick; Reset and Reset all show an Undo toast that restores the saved overrides; a Settings search that matches an expander's title shows all of its rows.
- **Expected time:** about 2 hours, mostly waiting for siblings to merge.
- **Must not change:** shortcut defaults other than Settings; swap mapping (left pane, A34); save-card buttons and keys; Feel choices.
- **Scope questions:** None.
- **Test plan:** selftests: swap tooltip with split on and off; dropdown name after a pick; Reset all then Undo restores overrides; search "scroll style" shows the expander's rows; Help `list` text follows the setting; Settings default key is `Ctrl+,`. Hand test in the slot dev copy: swap (split on), action row tiles; Settings Feel, Reset all + Undo, search "vertical".
- **Gates:** `tools/gate.ps1 -Task A35g-labels-tooltips -Offline` (build, selftest, format, tokens); Codex review pass (Medium); hand test.
- **Needs from you:** Approve, then the hand test.

## Files
- `product/AntiNotepad/Model/Welcome.cs` (text only)
- `product/AntiNotepad/UI/PadWindow.Bar.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs` (text only; A35a, A35b, A35d also edit)
- `product/AntiNotepad/UI/PadMenu.cs`
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `product/AntiNotepad/UI/Shortcuts.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Design.cs` (design dump baseline only)
- `docs/shortcuts.md` (generated table, for the new Settings key)
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `docs/decisions.md`

## Contracts
- Settings dropdown names carry the row title and current choice.
- Shortcut Reset and Reset all can be undone from the toast.
- A Settings search that matches an expander shows its rows.

## Approve
User, 2026-11-21 (verbatim), on the 15-change list: "> Note count (“2 / 5”): hover says “Search notes (Ctrl+F)”. no need. Let click show a pop up with note stats: # chars, # words, # lines, # paragraphs etc things that are useful for people to keep track of somtimes in word, notepad, etc. approved". Change 1 (A-4) is out; 2-15 approved.

## For Codex
Role: terra (`docs/agents.md`), because these are many small text edits plus three small Settings behaviors across known files. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files.
- Build exactly the Plan's "After" and the string table, word for word, except the two A-4 rows (pager count stays unchanged); no drive-by changes. If the Findings are wrong, stop and report.
- Build into `build/A35g-labels-tooltips-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A35g-labels-tooltips -Offline`.
- Never launch or kill AntiNotepad, never run `--quit`, never edit `.coord/STATUS.md` or this card. Offline only.
- Done: the table's strings appear, the tests pass, 0 warnings, `SELFTEST PASS`, gate passes.

- Continue run: partial edits from an earlier run are in the tree (its notes: `.coord/reports/A35g-labels-tooltips-run2-partial.md`). Review them and finish. Shortcut reset Undo must write through `_settings.Update` (the Layers setter-ownership check). Regenerate the `docs/shortcuts.md` table. Update the design dump baseline only after `--design-dump` shows the diff is just this table's strings; list the changed dump lines in the report.

Report `.coord/reports/A35g-labels-tooltips.md`: 1 Summary; 2 Change list; 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review; 8 Checks; 9 Hand checks.

## Check

## Hand test
Red dev copy, built from 1a33842. User, 2026-11-21 (verbatim): on "Shortcuts page, click Reset all": "i dont see this"; on "Change one shortcut first, then Reset all, then Undo": "i dont see this"; "everythign else works". Cause: the Undo toast is drawn on the pad window, which is out of sight while Settings is open.
User, 2026-11-21 (verbatim), with a screenshot of the Reset all row: "i would expect an undo to show up next to the reset all".
r2, red dev copy built from 1f18301. User, 2026-11-21 (verbatim): "undo works, wrap it up". Hand test PASS.

## Rework r2 (For Codex)
- Reset all and each row's Reset no longer use the pad toast. After a reset that changed something, an `Undo` text button appears right beside that `Reset all` / `Reset` button in Settings (same `TextButton` style). Clicking it restores the saved overrides through `_settings.Update` (as today) and the button disappears.
- The Undo stays until another shortcut change, another reset, or Settings closes. One pending Undo at a time; a new reset replaces it. Row rebuilds (`AfterSet(rows: true)`) must keep it showing.
- A Reset that changed nothing shows no Undo.
- Update the selftests: after Reset all, the Undo button exists next to Reset all; clicking it restores overrides; no pad toast is raised. Same Files list; report as `.coord/reports/A35g-labels-tooltips-r2.md`.
