# A6-note-switch-instant-swap-r2

## Summary
Locked in the overlapping snapshot departure and immediate arrival as the only swipe behavior, including release builds. The requested offline gate is blocked by its scope check: the round-2 brief has no backticked paths under Files. The brief and card were not edited.

## Change list
- `product/AntiNotepad/UI/NoteSwipe.cs`: stage the displayed pull without an environment selector.
- `product/AntiNotepad/Editor/PadView.cs`: retain snapshot departure and immediate arrival, remove sequential and continuous variants, and retain fallback and cleanup paths.
- `product/AntiNotepad/Motion/MotionTokens.cs`: describe the retained transition.
- `product/AntiNotepad/SelfTest/UI.cs` and `SelfTest/Contracts.cs`: exercise and describe the single path.

## Behavior changes
A committed swipe binds synchronously, captures the old page when possible, and overlaps its 160 ms departure with the 200 ms arrival. No top level, zero bounds, reduced motion, or supported snapshot failures retain normal arrival. Its default remains 34 px and 0.25 opacity. Boundary staging, interruption, binding, release, and detach cleanup remain in place. Preview and split/slot callers continue through SwitchTo.

## Out-of-scope files
None. This report and the gate report are coordination artifacts.

## Plan deviations
The product-wide no-lab-word condition cannot be met within the stated file list: existing unrelated Scroll Lab comments remain in `product/AntiNotepad/Model/Settings.cs:32` and `product/AntiNotepad/Editor/Motion.cs:21`. No note-switch lab selector or wording remains. The required gate fails its scope check because `.coord/tasks/A6-note-switch-instant-swap-r2.md` has no backticked Files paths; the user explicitly prohibited editing cards. The check-build folder remains because sandbox policy rejected its literal-path deletion.

## Contracts
`note-switch-settles` retains the settled-pane and repeat-navigation contract; headless checks cover normal and reduced-motion no-frame completion, seeded departure interruption, and detached cancellation. Displayed frames and snapshot pixels require hand testing.

## Lens review
Ran the lens-review checklist myself: reviewed scope, callers, state, timing, fallback, cleanup, failure paths, layering and checks. No proven code findings. SwitchTo callers in navigation and preview retain synchronous binding; Arrive has no external callers. The remaining risks are snapshot rendering and first reparent behavior on the user's actual renderer and DPI, which offline checks cannot establish. This is a self-review, not an independent review.

## Checks run
- Explicit offline build: 0 warnings, 0 errors; `--selftest`: `SELFTEST PASS`, 14 contract rows, 13 pass and 1 manual.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A6-note-switch-instant-swap-r2 -Offline`: FAIL solely at Scope due to the round-2 brief's missing backticked Files list. Build, Selftest, Seam, Tokens and Format passed; Cleanup skipped on failure; size warning for PadView.
- `git diff --check`: pass. Product search found no note-switch selector, and the two unrelated out-of-scope comments noted above.
- Check-build cleanup attempted; execution policy rejected the command. No app UI was launched or killed, and no git writes were made.

## Hand checks for the user
In the dev copy, check touchpad and Shift+wheel both directions, fast repeats and reversals, boundary bumps, split and slots, Settings preview, reduced motion, clipping, focus and high DPI. Confirm the old image departs while the newly bound page arrives and no visual residue remains.
