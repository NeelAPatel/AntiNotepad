# A25-page-edge-stuck-after-hide: page-edge pull stays stuck after the pad hides

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: Medium (confirm in Findings)
State: building
Branch: `fix/A25-page-edge-stuck-after-hide` in `.claude/worktrees/fix/A25-page-edge-stuck-after-hide` (created at Approve, from dev's tip)

## Request
Director (Master 6), 2026-11-18, user's go to start Findings (pending: `idea: page-edge-stuck-after-hide`, from the pstack trial, 2026-11-16).
- **Goal:** after the pad hides in the middle of a page-edge pull, showing it again leaves no stuck edge offset.
- **Done check:** (1) a selftest shows hide during a pull clears the frame state; (2) a 10-second hand check in your slot: pull the page edge, hide, show; the edge is at rest.
- **Proof:** selftest plus the hand check; before/after of the reset path.
- **What we know:** bug in merged `A20-overscroll-speed`: `StartFrame` returns early while `_frameRunning` is true; only the frame callback or `StopFrames` clears it, and nothing resets it on hide. Notes: `.coord/research/pstack-trial-log.md` step 3, risk 3. Code in `Editor/` (overscroll and frame code).
- **Stale check (user, 2026-11-18):** ideas age fast. Findings first re-checks that the bug still exists on dev's tip (read the code, reproduce by selftest if possible). If it is gone or the code moved on, say so and stop with a recommendation to drop the row. Weigh pros and cons of fixing now vs dropping in the Findings.
- **Constraints:** smallest fix; no visible redesign; Codex offline only; follow `.claude/rules/coding.md`; hand test in your slot's dev copy only (`docs/dev-loop.md`), never the installed copy. Self-test first; ask the user only for the final quick check.

## Findings
Chat: item chat, 2026-11-18, own read of dev tip e626863. Tier: **Low** (one wiring line in `PadWindow.HideResident`, selftest; no visible change), down from Medium.
- **Stale check: the bug still exists.** `PadWindow.HideResident` (`UI/PadWindow.cs`) never calls `NoteSwipe.Cancel()`; only `MotionPreview` does. `_noteSwipe` (`PadWindow.Navigation.cs`) is created on first wheel and never cancelled.
- **Mechanism:** the hidden window gets no animation frames, so a pending `_edge`/spring in `NoteSwipe` stops mid-pull. `_frameRunning` stays true, so `StartFrame` returns early. On show the pane keeps its `TranslateTransform` offset; the next wheel input continues from the stale `_edge`. (Reading only; not reproduced live, see Plan.)
- **Cancel is safe to call:** it stops timers, resets edge/pull/spring, sets transform null and opacity 1. Existing selftest covers it.
- **Fix vs drop:** fix = +1 line, no extra hide cost (null-safe, only when a swipe was ever used); risk near zero. Drop = a rare visible glitch (hide mid-pull) stays. Recommend **fix**.
- Not a split candidate: `PadWindow.cs` was not touched beyond one line.

## Plan
1. `PadWindow.Navigation.cs`: add `internal void CancelSwipe() => _noteSwipe?.Cancel();` with a one-line `///`.
2. `PadWindow.HideResident`: call `CancelSwipe()` before `Hide()`.
3. Selftest (`SelfTest/UI.cs`, `NoteSwipeChecks`): seed `_frameRunning = true` by reflection (stuck frame), `Input(1)` leaves the pane pulled; `Cancel()` then restores the pane and `HasRunningTimer` is false; a following `Input(1)` pulls again. Before/after: the check fails without the Cancel reset of `_frameRunning`, passes with it. The wiring line itself is covered by the hand check (`PadWindow` is not built in selftests).
4. Hand check (slot dev copy): pull the page edge with a horizontal wheel click at the last note, hide at once, show; the page is at rest.
Codex: steps 1-3, offline. Gates: gate.ps1.

## Files
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/SelfTest/UI.cs`

## Approve
User, 2026-11-18, in the item chat: "Approve"

## For Codex
Role: quick, because it is a one-line wiring change plus one selftest case. The branch is checked out: never switch, merge or commit. Read `AGENTS.md` and `.claude/rules/coding.md`, then the Files; re-read before editing.
- Build exactly the Plan steps 1-3; no drive-by changes. If the Findings are wrong, stop and report that.
- Step 3 detail: in `NoteSwipeChecks`, seed `_frameRunning = true` on a fresh `NoteSwipe` by reflection, `Input(1)` (pane pulled, frame stuck), `Cancel()`, then assert pane transform null, opacity 1, `HasRunningTimer` false, and a following `Input(1)` pulls again. Add a CONTRACTS row only if neighbouring swipe checks have one.
- Build into `build/A25-page-edge-stuck-after-hide-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete the folder. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A25-page-edge-stuck-after-hide -Offline`.
- Never launch or kill AntiNotepad, never run `--quit`, never edit `.coord/STATUS.md` or this card. Offline checks only.
- Done means: HideResident calls the cancel; the new selftest case exists and passes; build 0 warnings, `SELFTEST PASS`, gate passes. Show the new case failing without the reset (temporarily skipping `_frameRunning = false` in `Cancel`), then passing.

Report `.coord/reports/A25-page-edge-stuck-after-hide.md`, short: 1 Summary; 2 Change list; 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review (n/a); 8 Checks run; 9 Hand checks for the user.

## Check
Low tier: gate only. Gate PASS on the item branch (build 0 warnings, SELFTEST PASS, seam, tokens, format, scope, size). Diff read by the item chat: 3 files, 7 product lines plus one selftest case; matches Plan steps 1-3. Mutation check by Codex: the new case fails without the `_frameRunning` reset. Bounds: within plan. Noticed: none. Misses: none.

## Hand test
Dev copy running from the item worktree. User, 2026-11-18: "yes the page resets to normal once i do step 4 to show it again" (pull, hide, show: page at rest).
