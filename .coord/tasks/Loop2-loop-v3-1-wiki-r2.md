# Loop2-loop-v3-1-wiki-r2: four qualification fixes from the accuracy review

Rework round of `Loop2-loop-v3-1-wiki`. Read `.coord/tasks/Loop2-loop-v3-1-wiki.md` first; it still applies unless this file
says otherwise.

Tier: Low · docs-tools
Role: director

## Why this round
Proven findings in `.coord/reports/Loop2-loop-v3-1-wiki-review.review.md` (verdict accept, four minor findings):
1. `docs/wiki/Dev-Loop-Overview.md:117` and `docs/wiki/Tiers-and-Gates.md:29,33`: "coordination files" are not excluded from the
   scope check; only paths beginning with `.coord/` are (`tools/gate.ps1:225-232`). A stray `docs/agents.md` failed Scope in the
   rehearsal (`.coord/reports/Loop2-loop-v3-1-rehearsal.md`, check 2a). Say "`.coord/` paths" wherever that exclusion is meant.
2. `docs/wiki/Dev-Loop-Overview.md:113`: "a fresh Sol medium Codex reviewer" omits that effort is `CODEX_REVIEW_EFFORT`
   (default medium) per `docs/agents.md` and `.coord/tools/codex-run.sh:30`.
3. `docs/wiki/Where-Files-Live.md:32`: "uncommitted edits ride the next item commit" omits the fallback in `docs/loop.md`
   ("Branches, worktrees and commits"): a `chore: status` commit at the end of the day when nothing lands.
4. `docs/wiki/Who-Does-What.md:55`: the wiki resolves a contradiction inside `docs/loop.md` (Sessions table says the auditor
   "commits it"; the Branches section and `.claude/skills/next-task/SKILL.md` step 2 say nothing is committed before Approve)
   without saying so. State it plainly as an inconsistency in the rule files, name both lines, and say the wiki follows the
   skill's step 2.

## Change to the plan
None.

## Files
Only the wiki pages named above (all on the step brief's Files list), plus the round report.

## Done means
- The four statements are corrected as described; no other wording changes.
- The brief's backticked-path check prints nothing; `git diff --check` clean.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1-wiki -Offline` passes (Scope against the brief's Files; the round
  brief's Files count too).

Constraints: no commit, push, branch switch or merge; edit only `docs/wiki/*.md` and the report; never touch STATUS, the card or product.
Report: `.coord/reports/Loop2-loop-v3-1-wiki-r2.md` (short, step report format).
