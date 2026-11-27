# Loop21-umbrella-items: umbrella items (sub-items merge into a parent branch) in the loop rules and tools

Item card (`docs/loop.md`). Detail: audit `.coord/research/A35-chat-family-audit.md`; rework briefs r2-r4 `.coord/reports/Loop21-umbrella-items-rework-briefs.md`; findings and mock run in the reports.

Tier: Medium · hooks and gate scripts, new loop concept (loop v5.5.0, minor)
State: hand test
Branch: `chore/Loop21-umbrella-items` in `.claude/worktrees/chore/Loop21-umbrella-items`
Overrides: none.

## Request
User via Loop Hub 4, 2026-11-21: A35 was meant as an UMBRELLA (sub-items merge into it, it merges to dev once) but the loop had no such concept, so A35 and its children had no STATUS row, the title hook was blind and the roadmap could not show them. Build audit fixes 1-5, prove them, merge to dev.

## Findings
Process item. Hooks, brief gate and merge skill assumed one item, one row, dev as the only base. Evidence: the audit above.

## Plan
After: parent and child rows (`Child of <ID>:` Scope, `&nbsp;` indent allowed) folded by the roadmap; the brief gate refuses an item with no row; start card on SessionStart; title hint reads the card and warns on a differing title; need-to-know crosstalk with a `READY:` block; `merge-preflight -Base`, `catch-up -RebaseMerges`, `tools/survival.py`, gate `-Umbrella`; `tools/umbrella-mock.ps1` mock run. Must not change: non-umbrella items, product code, A35 branches. Test plan: every script's `-SelfTest`, `roadmap-data.ps1 -Check`, the mock run, the gate. Gates: gate, Codex review. Needs from you: the merge.

## Files
- `tools/title-hint.ps1`
- `tools/poteto-gate.ps1`
- `tools/start-card.ps1`
- `tools/merge-preflight.ps1`
- `tools/catch-up.ps1`
- `tools/umbrella-mock.ps1`
- `docs/loop-sessions.md`
- `tools/survival.py`
- `tools/gate.ps1`
- `tools/roadmap-data.ps1`
- `tools/roadmap/index.html`
- `.claude/settings.json`
- `.claude/skills/merge-task/SKILL.md`
- `.claude/skills/merge-task/branches.md`
- `.coord/templates/item.md`
- `docs/loop.md`
- `AGENTS.md`

## Approve
User, 2026-11-21: "crosstalk should be on a need to know basis. Major task completed that Parent chat has requested to know about / I the user has requested to send to parent; review/merge ready states: a very crystal clear message that task is ready, go to <chat> and run `/merge-task <slug>` or something for /review in the current chat. All updates dont need to be sent back and forth unless its for a coordination purpose. Other than that, i approve"

## For Codex
Role: terra (rework r2-r3 quick). Read `AGENTS.md`, then the Files. Never switch, merge or commit; never edit STATUS or this card. Offline checks only.

## Check
Gate PASS offline; self-tests pass; Codex review r1 found 3 (fixed r2); chain test (user go 2026-11-21: umbrella to a to b to c, real chats) passed and found the no-nested-starts rule (added) plus smaller frictions (`.coord/research/Loop21t-friction.md`); r3 fixed `&nbsp;` rows found against live A39 rows; r4 adds the mock run (12/12). Live three-level chain is not repeated in r4; the first real use proves it again. Codex review round 2 (2026-11-27): 2 findings (survival with diff.noprefix, autosquash flattening umbrella merges), both fixed in r5 and reproduced; no other proven issue. Within plan.

## Hand test
Not visually testable. Director's choice (2026-11-27): the mock run replaces it, `pwsh -NoProfile -File tools/umbrella-mock.ps1`.
