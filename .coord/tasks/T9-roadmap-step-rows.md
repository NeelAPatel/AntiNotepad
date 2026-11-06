# T9-roadmap-step-rows: roadmap shows umbrella step rows in their era, reads one-line card headers, and parks paused rows

Item card (`docs/loop.md`), fast lane (side task, `docs/loop.md` "Rules").

Tier: Low · side tool (does not ship), roadmap portal
Branch: `chore/T9-roadmap-step-rows` in `.claude/worktrees/chore/T9-roadmap-step-rows` (created at Approve)

## Request
User to Master 4, 2026-11-06: "update the roadmap site end to end". Master 4 found two page/data defects in the
roadmap tools and passed them to Loop Hub 2 (owner of `tools/roadmap*`), plus one optional question about paused rows.

## Findings
- **Kind and why:** bug; finished design-token steps vanish from the "Era 1 (now)" view, and two step cards show empty
  drawers.
- **Now:**
  - `tools/roadmap/index.html` `rowEra()` matches a done row to log items only when one ID starts with the other. Step
    IDs such as `A4a` and the umbrella `A4` (log item `A4-design-tokens`) never match, so the row falls to Era 0.
  - `tools/roadmap-data.ps1` `Get-RoadmapCards` needs a line starting `Branch:`. `A4m-playground-align.md` and
    `A4n-scale-lab.md` keep the branch on the `Tier:` line (`Tier: Medium · Branch: ...`), so both cards are skipped.
  - `stKind()` sends `paused` to the active kind, so D1-ui-region-names shows under "Moving without you".
- **Where:** those two files only.
- **Risk:** low; a wrong era match would show a done row in a newer era than it belongs to.
- **Source:** Master 4 report, confirmed by master scout.

## Plan
- **After:**
  - `rowEra()`: when no log item matches, strip the step letter (`A4a` -> `A4`) and match log items with that ID.
  - `Get-RoadmapCards`: also take the branch from a `Branch:` part inside the `Tier:` line, and trim the tier at ` · `.
  - `stKind()`: `paused` counts with deferred rows, not active ones (decided by Loop Hub 2: a paused item is not moving).
- **Expected time:** about 15 minutes, request to merge.
- **Must not change:** other row kinds, era rules for rows that already match, other card fields.
- **Scope questions:** None.
- **Test plan, code:** `pwsh -NoProfile -File tools/roadmap-data.ps1` summary shows cards up by 2; the JSON has
  `A4m-playground-align` and `A4n-scale-lab` cards with branch and tier set.
- **Test plan, UI:** None by the user; the page logic change is checked by reading.
- **Gates:** `tools/gate.ps1 -Task T9-roadmap-step-rows`: scope, loop-lint.
- **Needs from you:** Nothing.

## Files
- `tools/roadmap/index.html`
- `tools/roadmap-data.ps1`

## Contracts
None.

## Approve
User to Master 4, 2026-11-06: "update the roadmap site end to end" (relayed by Master 4 as this fix request; fast lane).

## For Codex
Not used: fast lane, three small edits; Loop Hub 2 edits them itself.

## Check
- Gate `tools/gate.ps1 -Task T9-roadmap-step-rows`: PASS (LoopLint failed once on a sample branch name in a comment; reworded).
- Code test: `roadmap-data.ps1` now emits 64 cards; `A4m-playground-align` (branch `chore/A4m-playground-align (off
  feat/A4-design-tokens)`, tier Medium) and `A4n-scale-lab` (branch `feat/A4n-scale-lab (off ...)`, tier Medium, hand
  test filled) are present.
- Era fallback run in node against log items `A4-design-tokens` and `A3-calc-split`: `A4a-token-scan` and
  `A4n-scale-lab` land in era 1; rows that already matched are unchanged.
- `paused` now hits the deferred kind first, so D1-ui-region-names leaves "Moving without you".
- Bounds check: request (two defects plus the paused question) = plan = done; two files, 11 lines added, 2 removed.
- Noticed: card headers vary (`Branch:` line vs inside `Tier:`); the parser now takes both. A4l's branch still carries its
  worktree text, as before (not in scope).
- Misses: None.

## Hand test
None (checked by Master 4 against the live page after merge).
