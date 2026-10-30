# T1-roadmap-overview-r3 report

Finished by the loop chat (user, 2026-10-30: "Finish r3 myself"), after the user stopped the Codex run at 17:18 (1485 s,
no report). Codex had already written both rules; the loop chat verified them and changed no code.

## Done
- Fork parent: the nearest fork across all other lanes, kept before a branch's own commits when another lane later
  merges them in (`tools/roadmap-data.ps1`, lane loop).
- Landing label: only the newest dev commit per item with no local branch, "squash of" for one commit, "landed from"
  for several (`tools/roadmap/index.html`).

## Checks
- Probe clone (scratchpad, deleted after) with `tmp-x` from dev, `a` from dev with one commit, `b` from `a` with one
  commit, and a `--no-ff` merge of `b` into `a`, served on 47888:
  - **dev:** parent main, fork `8d18f72`.
  - **a:** parent dev, fork `01bb862`.
  - **b:** parent a, fork `078ca77` (a's commit).
  - **tmp-x:** parent dev, fork `01bb862`.
  - The page draws "↳ from a", "↳ from dev" (twice) and "⤶ merged b", with no console errors.
- Real repo on 47887:
  - The page shows exactly one "landed from chore/Loop2-loop-v3-1".
  - Two "squash of" labels, each on a single-commit item.
  - "↳ from dev" on four side branches, and "↳ from main" once.
- The probe servers on 47888 and 47887 were stopped; 47811 was left running. `build/` holds no probe files.
- `tools/roadmap-data.ps1 -Check` printed `ROADMAP DATA OK` (lanes 6, warnings 0).
- `tools/gate.ps1 -Task T1-roadmap-overview -Offline` passed.
