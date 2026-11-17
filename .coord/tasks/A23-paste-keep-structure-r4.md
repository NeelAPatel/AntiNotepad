# A23-paste-keep-structure-r4: paste notice in the existing toast

Rework round of `A23-paste-keep-structure` (`docs/loop.md`). Read the item card, `-r2.md` and `-r3.md` first: Plan, Files, Contracts and "For Codex" still apply unless this file says otherwise.

Tier: Medium, same as the card.
Role: terra, because the engine's result shape changes and the hook and selftests follow.

## Why this round
User, 2026-11-17: after a copy that held no HTML pasted flat with no sign why, "lets add a pill bubble then on paste for the user to know. Paste Formatting Found / Paste Formatting Not Found / Paste Formatting Fallback". After the lab (`tools/playground/a23-paste-pill-lab.html`): "Use the pill notification setup already in the code for things like deleted note, empty note, etc." That is the pad toast: `PadView.RaiseToast(string)` (`Editor/PadView.cs`) feeds `PadWindow.Toast` (`UI/PadWindow.Layout.cs`). Position, motion, timing and look come from it unchanged; no new control.

## Change to the plan
1. `PasteEngine.Upgrade` keeps returning note text or null, plus a second public result so the hook knows why: add `PasteEngine.Analyze(ClipSnapshot)` returning `PasteOutcome { string? Text; bool StructureSeen }` (a small record or struct), with `Upgrade` implemented on it. `StructureSeen` is true when the HTML holds `li` or `table` structure (after the pre/white-space guard? no: report it also for guarded code HTML only if `li`/`table` is present, but never show the notice for the `pre` guard; see rules).
2. Hook (`PasteAsync` in `PadView.cs`) after deciding: converted gives `RaiseToast("Paste Formatting Found")`; not converted and `StructureSeen` and not preformatted gives `RaiseToast("Paste Formatting Fallback")`; everything else (plain copy, simple HTML text, code, markdown source) shows nothing, so ordinary pastes never flash a notice.
3. Wording is exactly those two strings, as constants next to the hook.
4. No change to `PadWindow*`, Settings, or the toast code.

Selftests (`SelfTest/Paste.cs`): outcome for capture A (converted), table capture D (converted), HTML with `li` that fails the word guard (fallback: `StructureSeen` true, text null), simple `<p>` HTML (no notice), plain-only snapshot (no notice), `pre` with `li` (no notice), markdown source (no notice). Test the outcome values; the toast itself is checked in the hand test.

## Files
- `product/AntiNotepad/Editor/PasteEngine.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/SelfTest/Paste.cs`

## Done means
Offline checks only (`docs/agents.md`, "What Codex can and cannot check").
- Outcome selftests pass; all earlier paste selftests pass.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A23-paste-keep-structure-r4 -Offline` passes.

Report: `.coord/reports/A23-paste-keep-structure-r4.md`, in the card's report format.
