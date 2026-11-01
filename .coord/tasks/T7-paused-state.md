# T7-paused-state: a `paused` State for plan rows, so a started item can wait without a rename (first user: D1-ui-region-names)

Item card (`docs/loop.md`), fast lane (side task, `docs/loop.md` "Rules").

Tier: Low · docs-tools (rules: `docs/loop.md`)
Branch: `chore/T7-paused-state` in `.claude/worktrees/chore/T7-paused-state` (created at Approve)

## Request
User go relayed by Master 2 (2026-11-01): "finish the remaining three Era 1 items, so that only ui-region-names is left". Asked in the Loop Hub 2 chat (2026-11-01): T4-T6 "[No preference]" between "Approve all three" and "Only T5 and T6"; D1 pause: "State 'paused' (Recommended)".

## Findings
- **Kind and why:** process; a `paused` State for plan rows, so a started item can wait without a rename (first user: D1-ui-region-names).
- **Now:** no paused state exists: `docs/loop.md:17` and the STATUS Plan format line list `planned`, `needs you`, loop states, `done`, `dropped`; `tools/loop-card.ps1:23-24` lists in-flight items by state, so an unknown state would show D1 as in flight. Master 2 asked to rename D1 to `D1-PAUSED-...`, which the ID check rejects (`tools/approve.ps1`, lowercase slug only); the user chose a `paused` state.
- **Where:** `docs/loop.md` (states and the "Rules" or phases text), `tools/loop-card.ps1` (skip `paused`). `tools/approve.ps1` cap already ignores unknown states. The STATUS format line and the D1 row are Master 2's.
- **Source:** master scout (Loop Hub 2).

## Plan
- **After:** `docs/loop.md` lists `paused`: a started item set aside by the user; its branch, worktree and card stay; it does not count against the 3-item cap; resuming sets the earlier state back. `tools/loop-card.ps1` does not list paused rows as in flight.
- **Expected time:** about 15 minutes.
- **Must not change:** every other rule, step and behavior of the files below.
- **Scope questions:** None.
- **Test plan, code:** `pwsh -NoProfile -File tools/loop-card.ps1` runs; with a scratch copy of STATUS whose D1 row says `paused`, D1 is not listed. Then the gate (loop-lint).
- **Test plan, UI:** None (not visible).
- **Gates:** `tools/gate.ps1 -Task T7-paused-state`: scope, loop-lint, format.
- **Needs from you:** Nothing.

## Files
- `docs/loop.md`
- `tools/loop-card.ps1`

## Contracts
None.

## Approve
User, 2026-11-01: go relayed by Master 2 ("finish the remaining three Era 1 items"); in this chat "[No preference]" between two options that both build this item, and "State 'paused' (Recommended)" for the D1 pause marker.

## For Codex
Not used: fast lane, a small change; the Claude builder sub-session makes it and says so in its report.

## Check

Fast lane, Claude builder, no Codex. Scope: only the two Files changed. loop-card runs; a scratch STATUS with a `paused` row does not list it. Report: `.coord/reports/T7-paused-state.md`.

## Hand test
None (not visible).
