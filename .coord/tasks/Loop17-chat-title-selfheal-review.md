# Loop17-chat-title-selfheal-review: lens review of Loop17-chat-title-selfheal

Role: reviewer (fresh run, read-only). No product code changes in this item (tooling and docs only).
Branch: `chore/Loop17-chat-title-selfheal` (already checked out; never switch, merge, commit or edit files except the report).
Tier: Medium

## Inputs
- Item card `.coord/tasks/Loop17-chat-title-selfheal.md` (Request, Plan, Files, Approve, Check); report `.coord/reports/Loop17-chat-title-selfheal.md`; gate `.coord/reports/Loop17-chat-title-selfheal.gate.md`
- Diff: `git diff dev...HEAD -- tools .claude docs` (script `tools/title-hint.ps1`, settings entry, docs rule, two skill lines)

## Do
Follow `.agents/skills/lens-review/SKILL.md`, including the plan-bounds item. Focus: hook fails open and prints ASCII only; running/failed/stalled logic against `.coord/tools/codex-run.sh` log and `usage.tsv` shapes; phase and me-marker map vs `docs/loop.md`; STATUS and pending parsing vs `tools/loop-card.ps1`; doc rule says title and row change together; no retitling script; self-tests cover real shapes. Run `pwsh -NoProfile -File tools/title-hint.ps1 -SelfTest` only (offline; no ports).

## Write: `.coord/reports/Loop17-chat-title-selfheal.review.md`
In the skill's format. End with one verdict line: `VERDICT: accept`, `VERDICT: rework` or `VERDICT: ask-user`.
