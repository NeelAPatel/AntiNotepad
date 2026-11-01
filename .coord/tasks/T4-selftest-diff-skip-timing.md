# T4-selftest-diff-skip-timing: verbatim-move checks compare `--selftest` output without wall-clock timing lines

Item card (`docs/loop.md`), fast lane (side task, `docs/loop.md` "Rules").

Tier: Low · docs-tools (rules: `docs/loop.md`)
Branch: `chore/T4-selftest-diff-skip-timing` in `.claude/worktrees/chore/T4-selftest-diff-skip-timing` (created at Approve)

## Request
User go relayed by Master 2 (2026-11-01): "finish the remaining three Era 1 items, so that only ui-region-names is left". Asked in the Loop Hub 2 chat (2026-11-01): T4-T6 "[No preference]" between "Approve all three" and "Only T5 and T6"; D1 pause: "State 'paused' (Recommended)".

## Findings
- **Kind and why:** process; verbatim-move checks compare `--selftest` output without wall-clock timing lines.
- **Now:** A2-store-split's card wrote the rule ad hoc ("`--selftest` output equal to dev's (timing lines such as `A18-search-speed timing` excepted)", `.coord/tasks/A2-store-split.md:34`); `docs/coding.md` "How to split" (lines 59-63) says nothing about how to prove a move is verbatim, so the next split could diff timing lines and fail spuriously.
- **Where:** `docs/coding.md` "How to split" only.
- **Source:** master scout (Loop Hub 2).

## Plan
- **After:** one bullet under "How to split": prove a pure move by comparing `--selftest` output with dev's, skipping lines that report wall-clock times (they contain `timing` or `ms`, e.g. `A18-search-speed timing`), plus the moved-line count (sorted lines equal). No script.
- **Expected time:** about 10 minutes.
- **Must not change:** every other rule, step and behavior of the files below.
- **Scope questions:** None.
- **Test plan, code:** `tools/gate.ps1` loop-lint and scope pass.
- **Test plan, UI:** None (not visible).
- **Gates:** `tools/gate.ps1 -Task T4-selftest-diff-skip-timing`: scope, loop-lint, format.
- **Needs from you:** Nothing.

## Files
- `docs/coding.md`

## Contracts
None.

## Approve
User, 2026-11-01: go relayed by Master 2 ("finish the remaining three Era 1 items"); in this chat "[No preference]" between two options that both build this item, and "State 'paused' (Recommended)" for the D1 pause marker.

## For Codex
Not used: fast lane, a small change; the Claude builder sub-session makes it and says so in its report.

## Check
- Diff is one added bullet in `docs/coding.md`; nothing else changed.
- `tools/gate.ps1 -Task T4-selftest-diff-skip-timing`: run last, result in the builder's reply.

## Hand test
None (not visible).
