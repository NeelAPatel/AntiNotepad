# A35a-data-safety-r2: delete-forever Undo keeps the body; a withdrawn toast button does nothing

Rework round of `A35a-data-safety` (`docs/loop.md`). Read the item card `.coord/tasks/A35a-data-safety.md` first: its Plan, Files, Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium (same)
Role: worker, because two proven findings across two files, and a terra run on this item stopped early. Work through to the report.

## Why this round
Codex lens review `.coord/reports/A35a-data-safety.review.md`, VERDICT: rework. Both findings are proven; read them in full.
1. Delete-forever Undo in Search (`UI/SearchOverlay.cs:381-392`) reinserts the Note but not its body. The next save deletes the body file (`Model/Store.cs:480-484`); Undo leaves the note clean, so no body is rewritten. After reload the text is empty.
2. `HideToast()` clears `_toastAction`, but the toast button stays clickable during its fade, and its click (`UI/PadWindow.Layout.cs:708`) runs the captured `onAction`. A click during the fade still overwrites the new slot text. The selftest fixture forces reduced motion, which hides this window.

## Change to the plan
None. The fixes are inside the approved "After".
- Fix 1: before removing notes from Trash, read each body. On Undo, put each note back at its trash index and set its text again so it is dirty and gets saved. A note whose body is unavailable is restored as metadata only. Never write empty text over it. Do not change `Model/Store.cs`.
- Fix 2: the button click acts only while its toast's action is still current (compare `_toastAction` or a toast generation), and a withdrawn toast cannot run its action at all. Ctrl+Z already checks `_undoArmed`; keep that.
- Tests, each seen failing before its fix: (a) a saved trash note: delete forever, save, Undo, save, reload, text intact. Use the real Store and a temp data folder, as the existing Store selftests do. (b) A withdrawn slot-clear toast's button, invoked after the edit with normal (not reduced) motion, leaves the new text. Extend the existing contract rows rather than adding new ones.

## Files
- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Store.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Done means
Offline checks only (`docs/agents.md`, "What Codex can and cannot check").
- Both findings are fixed and their tests pass. Paste FAIL lines from before each fix, then PASS.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A35a-data-safety-r2 -Offline` passes, including Format.
- Delete `build/A35a-data-safety-check` when done. Never commit, launch AntiNotepad or run `--quit`.

Report: `.coord/reports/A35a-data-safety-r2.md`, in the card's report format.
