# A15-notes-edge-pills: edge pills when scrolling past the first or newest note

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: <set by the item chat> · ui-pills
State: pending
Branch: `feat/A15-notes-edge-pills` in `.claude/worktrees/feat/A15-notes-edge-pills` (created at Approve)

## Request
2026-11-12, user (pasted list, "Standard Notes" section), verbatim:
- Page 1, scroll backwards: no pill "No older Notes"
- Pill: "New Note" --> "New Note Created"
- Pill: Overscroll to right on newly made empty note --> add pill "Already at Latest"

Then: "start 1 only" (first of the running UI/UX/animation list in `.coord/pending.md`, idea notes-edge-pills). Also from the user (2026-11-12): motion cannot be judged by numbers; A14-search-open-fade-up set the Windows 11 values in `docs/decisions.md` (167 ms open, 83 ms close, 16 px rise); reuse them for any pill motion.

## Findings
Tier: Low (ui-pills: UI text plus one small rule; no Store or timers). Own scout, 2026-11-12.
- Pills are toasts: `PadWindow.Layout.cs` `Toast(text, brief: true)` = bottom-centre card, fade in 120 ms (`ToastIn`), 1300 ms hold, fade out 160 ms (`ToastOut`). Not Win11 tokens; no rise.
- All three cases are in `PadWindow.Navigation.cs` `Navigate(direction)`; the swipe (`NoteSwipe`) and arrow keys both call it, once per gesture.
  - Now (a): `target < 0` -> `Bump(-1)` (7 px shake) and nothing else, so no pill.
  - Now (b): created a note -> `Toast("New note", brief: true)`.
  - Now (c): past the newest and `leaving.IsEmpty` -> `Bump(1)`, no pill.
- `MoveCurrent` (Alt+Up/Down) also bumps at the ends; not in the request, left alone.
- A14's tokens (`OverlayIn` 167, `OverlayOut` 83, `OverlayRise` 16) exist, but changing all toasts to them is a wider change; the pills reuse the existing toast motion.

## Plan
After: (a) first note + swipe back -> shake plus pill "No older Notes"; (b) pill text "New Note Created"; (c) empty newest note + swipe forward -> shake plus pill "Already at Latest". All brief (1300 ms), same style as today's pills. A repeat overscroll shows the pill again (each gesture calls Navigate once).
Test: pure helper `EdgeNotice(target, count, leavingEmpty)` returns the pill text or null; `--selftest` checks the 3 cases plus a normal move. UI: hand test in the dev copy.
Open choice for the user: motion (keep toast fade as is, recommended, vs Win11 167/83 plus 16 px rise for all toasts, a separate item).

## Files
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/SelfTest/UI.cs`

## Contracts

## Approve
2026-11-12, user: "Approve", then "*yes use w11 motion*" (overrides the plan's "keep toast fade"): all toasts (the pills included) use the Win11 tokens: in `OverlayIn` 167 ms with `Win11Enter` and `OverlayRise` 16 px, out `OverlayOut` 83 ms with `Win11Exit`. Same files plus `Motion/MotionTokens.cs` if the Toast tokens are retired.
