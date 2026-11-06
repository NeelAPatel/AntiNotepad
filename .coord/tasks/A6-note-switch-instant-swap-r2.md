# A6-note-switch-instant-swap-r2: lock in variant 2 and remove the lab

Rework round of `A6-note-switch-instant-swap` (`docs/loop.md`). Read the item card `.coord/tasks/A6-note-switch-instant-swap.md` first
and the round-1 report `.coord/reports/A6-note-switch-instant-swap.md`: Plan, Files, Contracts and "For Codex" still apply unless this file says otherwise.

Tier: Medium (same)
Role: terra, because it is a careful removal of lab branches in timing-sensitive code.

## Why this round
The lab ended. User, 2026-11-06: "Variant 2 is best. lock it in. Ensure worktree is ready to merge with variant 2's solution".
Variant 2 = the old page is frozen as an image and slides out (160 ms, from its displayed pull) while the new, already-bound content arrives at once over 200 ms.

## Change to the plan
Variant 2 becomes the only behavior; variants 1 and 3 and the `ANTINOTEPAD_NOTE_SWITCH_LAB` switch are deleted.
- Remove the environment variable read and all variant selection in `UI/NoteSwipe.cs` and `Editor/PadView.cs`; no dev-only gating, variant 2 runs in release too.
- Delete code only variants 1 and 3 used (sequential-reveal logic, continuous-start parameters if unused, their tokens in `Motion/MotionTokens.cs`). Keep the fallbacks (no top level, zero bounds, reduced motion, snapshot failure -> normal arrival) and all cancellation/cleanup paths.
- Keep `Arrive`'s default behavior (34 px, 0.25 opacity) for the fallback and other callers.
- Keep Settings preview and split/slot paths working through the same single path.
- Update selftests and the `note-switch-settles` CONTRACTS row to variant 2 only; remove checks that exercise deleted variants. Update comments/headers to describe the final behavior (no "lab" wording).
- Do not touch `AppInfo.cs` (the temporary badge edit was already reverted).

## Files
Files added for this round only: none beyond the card's list.
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/UI/NoteSwipe.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Done means
Offline checks only.
- No reference to `NOTE_SWITCH_LAB` or "lab" remains in product code; one swipe path, variant 2.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A6-note-switch-instant-swap-r2 -Offline` passes.
- Run the lens-review checklist yourself and say so.

Report: `.coord/reports/A6-note-switch-instant-swap-r2.md`, in the card's report format.
Never commit, never launch or kill AntiNotepad, never edit STATUS or the cards. Delete your build check folder if the sandbox allows; else say so.
