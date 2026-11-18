# A25-page-edge-stuck-after-hide

## 1. Summary
Hiding the pad now cancels any active page-edge swipe, clearing its frame and restoring the pane.

## 2. Change list
- Added `PadWindow.CancelSwipe()` and called it from `HideResident` before `Hide()`.
- Added a `NoteSwipeChecks` regression case that seeds `_frameRunning`, pulls, cancels, checks clean state, and verifies a later pull works.

## 3. Behavior changes
The pane no longer retains a mid-pull transform when the hidden window stops receiving animation frames.

## 4. Out-of-scope files
None in the final diff. `NoteSwipe.cs` was temporarily changed for the requested mutation check and restored.

## 5. Plan deviations
None.

## 6. Contracts
No new CONTRACTS row; neighboring swipe cleanup checks do not define one.

## 7. Lens review
n/a

## 8. Checks run
- Selftest passed with the reset present.
- Mutation check: temporarily removed `_frameRunning = false` from `StopFrames`; selftest failed on “cancel after a stuck frame clears the pull and restores the pane”. Restored the line and reran the gate.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A25-page-edge-stuck-after-hide -Offline`: PASS; build had 0 warnings and 0 errors, selftest printed `SELFTEST PASS`, format/scope/seam/tokens passed. Offline NuGet audit was disabled by the gate.
- Gate removed the task build and data folders.

## 9. Hand checks for the user
Pending: in this worktree's dev slot, pull the page edge at the last note, hide immediately, then show; confirm the edge is at rest. No live app was launched by Codex.
