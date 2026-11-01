# T6-gate-format-split-baseline: the gate's Format step treats lines moved verbatim into a new file as old code, not as changed lines

Item card (`docs/loop.md`), fast lane (side task, `docs/loop.md` "Rules").

Tier: Low · docs-tools (rules: `docs/loop.md`)
Branch: `chore/T6-gate-format-split-baseline` in `.claude/worktrees/chore/T6-gate-format-split-baseline` (created at Approve)

## Request
User go relayed by Master 2 (2026-11-01): "finish the remaining three Era 1 items, so that only ui-region-names is left". Asked in the Loop Hub 2 chat (2026-11-01): T4-T6 "[No preference]" between "Approve all three" and "Only T5 and T6"; D1 pause: "State 'paused' (Recommended)".

## Findings
- **Kind and why:** process; the gate's Format step treats lines moved verbatim into a new file as old code, not as changed lines.
- **Now:** `tools/gate.ps1:355-361`: a changed `.cs` file not present at the base revision gets `$set = $null`, so every format problem in it counts as new. A pure split (A3-calc-split) therefore had to reformat moved code, breaking "verbatim" (Master 2, 2026-11-01).
- **Where:** `tools/gate.ps1` Format step only (lines ~342-397).
- **Source:** master scout (Loop Hub 2).

## Plan
- **After:** for a new file, a line counts as touched only when its text (trailing whitespace and CR ignored) is not among the lines the same change removed from product `.cs` files at the base revision (`git diff -U0 <base>` `-` lines). Lines moved verbatim become "older" problems (counted, not failing); new or edited lines still fail. Tracked files keep today's hunk logic.
- **Expected time:** about 30 minutes.
- **Must not change:** every other rule, step and behavior of the files below.
- **Scope questions:** None.
- **Test plan, code:** script parses; scratch check on a throwaway local branch (never pushed, deleted after): move a method with a known format problem verbatim into a new file -> Format PASS with an older count; add a new badly formatted line in that file -> Format FAIL. Then the gate.
- **Test plan, UI:** None (not visible).
- **Gates:** `tools/gate.ps1 -Task T6-gate-format-split-baseline`: scope, loop-lint, format.
- **Needs from you:** Nothing.

## Files
- `tools/gate.ps1`

## Contracts
None.

## Approve
User, 2026-11-01: go relayed by Master 2 ("finish the remaining three Era 1 items"); in this chat "[No preference]" between two options that both build this item, and "State 'paused' (Recommended)" for the D1 pause marker.

## For Codex
Not used: fast lane, a small change; the Claude builder sub-session makes it and says so in its report.

## Check

Fast lane, built by the Claude builder (no Codex). Scratch test on throwaway local branches (deleted, never pushed): class moved verbatim into a new file -> Format PASS, 6 older ignored; new badly formatted class added to that file -> Format FAIL on the new lines only. Report: `.coord/reports/T6-gate-format-split-baseline.md`. Final gate result below.

## Hand test
None (not visible).
