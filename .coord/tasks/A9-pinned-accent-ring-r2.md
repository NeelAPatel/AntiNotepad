# A9-pinned-accent-ring-r2: fix the stalled ring selftest and refresh the design baseline

Rework round of `A9-pinned-accent-ring` (`docs/loop.md`). Read the item card `.coord/tasks/A9-pinned-accent-ring.md` first: its Plan, Files,
Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium, same as the card.
Role: worker, because the cause of a selftest stall is unknown (shared UI state, timing).

## Why this round
Round 1 code (commit "pinned accent ring (round 1...)") builds with 0 warnings, but `--selftest` never finishes: output stops after the `A18-search-speed timing` line and CPU is idle. The same selftest on the parent commit passes (`SELFTEST PASS`), so the hang is new. The prime suspect is `PinRingChecks` in `SelfTest/UI.cs` (run through the new `pinned-accent-ring` row in `SelfTest/Contracts.cs`): it builds a `PadWindow` differently from `DesignDump` in `SelfTest/Design.cs` (that one installs the Fluent theme first, uses `Store(path)` then `store.Load()`, seeds with `Welcome.SeedIfNew`, and deletes its temp folder). Make the check follow that working pattern, and find the real cause if that is not it. Also: `DesignDump` captures the whole logical tree, so the new ring Border changes `DesignHash`; after confirming the only change is the ring Border (compare with `--design-dump <file>` output from the parent commit), update `DesignHash` in `SelfTest/Design.cs`.
Do not weaken the ring code: 4 px, `Shape.Card` radius, palette accent, `Paint.FocusBorder` opacity, 250 ms fade, click-through.

## Change to the plan
None. Files outside the card's list are approved for this round (see below).

## Files
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `tools/playground/a9-ring-lab.html` (already committed lab page; listed only so gate Scope passes; do not edit it)

## Done means
Offline checks only (`docs/agents.md`, "What Codex can and cannot check"). Use a hard timeout (about 5 min) when running `--selftest`; if it stalls again, report where, do not retry blindly.
- `--selftest` prints `SELFTEST PASS`, including the `pinned-accent-ring` contract row and `DESIGN dump` with the new hash.
- Build 0 warnings, `pwsh -NoProfile -File tools/gate.ps1 -Task A9-pinned-accent-ring-r2 -Offline` passes (or lists only failures you explain).
- Delete `build/A9-check`, gate output and data folders you created.

Report: `.coord/reports/A9-pinned-accent-ring-r2.md`, in the card's report format.
