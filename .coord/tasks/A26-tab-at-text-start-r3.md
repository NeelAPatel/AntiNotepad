# A26-tab-at-text-start-r3: renumber once per block, not once per selected line

Rework round of `A26-tab-at-text-start` (`docs/loop.md`). Read the item card `.coord/tasks/A26-tab-at-text-start.md`, the r2 brief and the review `.coord/reports/A26-tab-at-text-start-r2.review.md` first: Plan, Files, Contracts and "For Codex" still apply unless this file says otherwise.

Tier: Medium, same as the card
Role: terra, because it is one targeted fix in `DocumentCommands.IndentListLines`

## Why this round
Review verdict rework, one proven major finding (cause: `code`): `IndentListLines` calls `Renumber` once per selected list line, and each call scans the whole contiguous block. Measured: 1,000 selected bullet lines 0.63 s, 4,000 lines 6.9 s, on the UI thread.

## Change to the plan
None for behavior. Renumber each affected contiguous block once after all indents (preserving today's fresh-number rule for a moved numbered group: a single-line Tab or Ctrl+] on a numbered line still restarts it at 1, and results equal to r2 for every existing selftest case). Cost for N selected lines must be about linear in the block size, not N times it.
- Add a bounded large-selection selftest, for example 2,000 selected bullet lines indented in well under a second (use a generous bound so it is not flaky, plus a result check), so repeated scans are caught.

## Files
- `product/AntiNotepad/Editor/DocumentCommands.cs`
- `product/AntiNotepad/Editor/ListEditing.cs`
- `product/AntiNotepad/SelfTest/Lists.cs`

## Done means
- The above, with all earlier list selftests unchanged and passing. Build 0 warnings (`AVALONIA_TELEMETRY_OPTOUT=1`, into `build/A26-tab-at-text-start-r3-check`), `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A26-tab-at-text-start-r3 -Offline` passes. Never launch the app, never commit, never edit the card. If cleanup of your build folder is blocked, say so; do not retry.

Report: `.coord/reports/A26-tab-at-text-start-r3.md`, in the card's report format.
