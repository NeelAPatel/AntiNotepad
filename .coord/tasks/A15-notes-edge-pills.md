# A15-notes-edge-pills: edge pills when scrolling past the first or newest note

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: <set by the item chat> · ui-pills
State: hand test
Branch: `feat/A15-notes-edge-pills` in `.claude/worktrees/feat/A15-notes-edge-pills` (created at Approve)

## Request
2026-11-12, user (pasted list, "Standard Notes" section), verbatim:
- Page 1, scroll backwards: no pill "No older Notes"
- Pill: "New Note" --> "New Note Created"
- Pill: Overscroll to right on newly made empty note --> add pill "Already at Latest"

Then: "start 1 only" (first of the running UI/UX/animation list in `.coord/pending.md`, idea notes-edge-pills). Also from the user (2026-11-12): motion cannot be judged by numbers; A14-search-open-fade-up set the Windows 11 values in `docs/decisions.md` (167 ms open, 83 ms close, 16 px rise); reuse them for any pill motion.

## Findings
Tier: Low (ui-pills: UI text plus one small rule; no Store or timers). Own scout.
- Pills are toasts: `PadWindow.Layout.cs` `Toast(text, brief: true)` = bottom-centre card, fade in 120 ms (`ToastIn`), 1300 ms hold, fade out 160 ms (`ToastOut`). Not Win11 tokens; no rise.
- All three cases are in `PadWindow.Navigation.cs` `Navigate(direction)`; the swipe (`NoteSwipe`) and arrow keys both call it, once per gesture.
  - Now (a): `target < 0` -> `Bump(-1)` (7 px shake) and nothing else, so no pill.
  - Now (b): created a note -> `Toast("New note", brief: true)`.
  - Now (c): past the newest and `leaving.IsEmpty` -> `Bump(1)`, no pill.
- A14's tokens (`OverlayIn` 167, `OverlayOut` 83, `OverlayRise` 16) exist; the user chose them for all toasts.

## Plan
- **After:** (a) swipe back on the first note: shake plus pill "No older Notes"; (b) pill "New Note Created"; (c) swipe forward on the empty newest note: shake plus pill "Already at Latest". Pills stay 1300 ms and show again on each overscroll.
- **Motion (user, 2026-11-12):** every toast uses the Win11 tokens: in `OverlayIn` with `Win11Enter` plus `OverlayRise`, out `OverlayOut` with `Win11Exit`; old `ToastIn/Out` tokens removed.
- **Must not change:** other toasts' text and timing; Alt+Up/Down end bump; Bump shake.
- **Test plan:** pure `EdgeNotice(target, count, leavingEmpty)` returns the pill text or null; `--selftest` covers the 3 cases plus a normal move. UI hand test: swipe at both ends, create a note by swiping, toast motion.
- **Gates:** gate offline (build, selftest, tokens, format).

## Files
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/SelfTest/UI.cs`

## Contracts
- none new (feature, no bug)

## Approve
2026-11-12, user: "Approve", then "*yes use w11 motion*" (overrides the plan's "keep toast fade"): all toasts (the pills included) use the Win11 tokens: in `OverlayIn` 167 ms with `Win11Enter` and `OverlayRise` 16 px, out `OverlayOut` 83 ms with `Win11Exit`. Same files plus `Motion/MotionTokens.cs` if the Toast tokens are retired.

## For Codex
Role: terra (docs/agents.md), because a few files of UI logic plus a motion change. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files; re-read before editing.
- Build exactly the Plan's "After" and "Motion"; no drive-by changes. Pills reuse `Toast(text, brief: true)`; `Navigate` shows them via `EdgeNotice`.
- Build into `build/A15-notes-edge-pills-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A15-notes-edge-pills -Offline`.
- Never launch or kill AntiNotepad, never edit STATUS or this card. Offline checks only.
- Done means: the 3 pill texts come from `EdgeNotice`; toasts use the Win11 tokens; build 0 warnings, `SELFTEST PASS`, gate passes.

Report `.coord/reports/A15-notes-edge-pills.md`, short: 1 Summary; 2 Change list; 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review; 8 Checks run; 9 Hand checks.

## Check
Gate offline PASS (build 0 warnings, SELFTEST PASS, seam, tokens, format, scope, size). Diff within plan; no deviations. No lens review (Low tier). Noticed: tokens baseline can be lowered (-Update), left alone.

## Hand test
Dev copy running from this worktree (pid 25492, build/dev, test data build/testdata). Result: pending.
