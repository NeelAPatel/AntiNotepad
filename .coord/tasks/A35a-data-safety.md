# A35a-data-safety: no pad action silently destroys note text

Item card (`docs/loop.md`). Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB.

Tier: Medium · bug-fix (key and toast handlers only; the quit path gets key routing, its logic is untouched)
State: building
Branch: `fix/A35a-data-safety` in `.claude/worktrees/fix/A35a-data-safety` (from `docs/A35-ux-pass`; merges back there)

## Request
User via the A35-ux-pass chat, 2026-11-20 (verbatim): "Yes both" (stage 4 batch 1, data safety, with batch 2, icons). Batch 1 = C-1, A-1, C-3, C-2.

## Findings
Source: `.coord/reports/A35-ux-pass-s3.md` (Codex audit), code re-read here 2026-11-20.
- **C-1 🔴** `UI/SearchOverlay.cs:337`: in trash view plain Delete in the focused query box runs `DeleteSelected()`, deleting forever with no Undo (`:376-379`).
- **A-1 🔴** `UI/PadWindow.Navigation.cs:251`: edits clear `_undoArmed` (`Layout.cs:267`), which gates Ctrl+Z, but the toast button (`Layout.cs:704`) ignores it and overwrites the new slot text. `Edited` fires synchronously (`Editor/PadView.cs:658`), so the clear itself lands before the toast.
- **C-3 🔴** `UI/PadWindow.Layout.cs:794`: Enter always retries, even with Quit anyway focused.
- **C-2 🟡** `UI/SearchOverlay.cs:386-394`: slots are skipped but the toast counts them out ("0 notes moved to trash").

## Plan
- **After:**
  - C-1: plain Delete always edits the query. Ctrl+Delete deletes (trash view: forever; stack view: to trash) only when the caret is at the end of the query, where it would delete no word; elsewhere it deletes a word. Ctrl+Backspace on an empty query stays. Delete forever gets an **Undo** toast (7 s) that puts notes back at their trash positions. Trash hint: "Enter restores · Ctrl+Delete removes forever".
  - A-1: editing a cleared slot hides its Undo toast, so nothing can overwrite new text. Other toasts unchanged.
  - C-3: Enter runs the focused button; a repeat prompt also refreshes the quit action.
  - C-2: slots alone: "Slots are cleared in the editor"; mixed: "2 notes moved to trash · 1 slot skipped"; notes only: unchanged.
- **Lab:** not lab-worthy (one key rule, three toast strings).
- **Must not change:** stack-view Undo; editor slot clear (Ctrl+Shift+Backspace); Retry, Esc, Tab on the save card.
- **Test plan:** repro selftests seen failing on the base, landing with their fix: (1) trash view, query "abc", caret mid-query: Delete and Ctrl+Delete leave Trash unchanged; caret at end: Ctrl+Delete removes, Undo restores; (2a) clear slot, Undo with no edit restores old text; (2b) clear slot, type, toast action gone, new text kept; (3) Quit focused + Enter calls quit, not retry; (4) slot-only and mixed toast texts. Four `CONTRACTS` rows.
- **Gates:** `tools/gate.ps1 -Task A35a-data-safety -Offline`; Codex review pass; hand test of C-1, A-1, C-2 in the slot dev copy. C-3 is selftest only (forcing a save failure needs a broken data folder).
- **Needs from you:** Approve, then the hand test.

## Files
- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Contracts
- Search: Delete edits the query; Ctrl+Delete deletes only at query end; delete forever has Undo.
- Slot-clear Undo is withdrawn once the slot is edited.
- Save-failure card: Enter runs the focused button.
- Search delete toast counts only notes moved and names skipped slots.

## Approve
User, 2026-11-20 (verbatim): "Approve"

## For Codex
Role: terra (`docs/agents.md`), because four clear fixes across a few files. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files.
- Build exactly the Plan's "After"; no drive-by changes. If the Findings are wrong, stop and report.
- Write each repro selftest first, run it on unchanged code, paste FAIL lines; then fix and paste PASS.
- Build into `build/A35a-data-safety-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A35a-data-safety -Offline`.
- Never launch or kill AntiNotepad, never run `--quit`, never edit `.coord/STATUS.md` or this card, never edit `UI/Kit.cs`. Offline only.
- Done: the After behaviors, the tests pass, 0 warnings, `SELFTEST PASS`, gate passes.

Report `.coord/reports/A35a-data-safety.md`: 1 Summary; 2 Change list; 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review; 8 Checks (FAIL before, PASS after); 9 Hand checks.

## Check

## Hand test
