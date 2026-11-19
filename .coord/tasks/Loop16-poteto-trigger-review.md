# Loop16-poteto-trigger-review: lens review of Loop16-poteto-trigger

Role: reviewer (fresh run, read-only). This item is loop tooling, no product code. Branch `chore/Loop16-poteto-trigger` (checked out; never switch, merge, commit or edit files). Tier: Medium

## Inputs
- Card `.coord/tasks/Loop16-poteto-trigger.md` (Plan, Files, Approve), report `.coord/reports/Loop16-poteto-trigger.md`
- Diff: `git diff dev...HEAD` (script `tools/poteto-gate.ps1`, playbook, next-task skill text, docs/playbook.md row)

## Do
Follow `.agents/skills/lens-review/SKILL.md` where it applies to a script, including the plan-bounds item. Offline only: run `pwsh -NoProfile -File tools/poteto-gate.ps1 -SelfTest`; you may feed the script canned stdin. Look for: a hook that could block normal chats (false blocks), crash paths that do not fail open, wrong first-turn detection (isMeta user messages, tool_result user messages, a second real user message), `stop_hook_active` handling, brief checks that are too strict or too loose (CRLF, leading blank lines), log path surprises, text in the playbook/skill that disagrees with the script. Do not run live chats or touch `~/.claude`.

## Write: `.coord/reports/Loop16-poteto-trigger.review.md`
Format from the skill; one finding per line with file and line. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
