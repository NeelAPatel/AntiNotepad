# Loop21-umbrella-items: umbrella items (sub-items merge into a parent branch) in the loop rules and tools

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: Medium · hooks and gate scripts, new loop concept (loop v5.5.0, minor)
State: approved
Branch: `chore/Loop21-umbrella-items` in `.claude/worktrees/chore/Loop21-umbrella-items` (created at Approve)
Overrides: none yet. The chain test (Plan, step 7) starts chats; AGENTS.md forbids that, so it needs the user's go there.

## Request
User via Loop Hub 4, 2026-11-21, after the audit `.coord/research/A35-chat-family-audit.md`: A35 was meant as an UMBRELLA (sub-items A35a... merge into A35, A35 merges to dev once). The loop has no such concept, so A35 and its children had no STATUS row, the title hook was blind, and the roadmap could not show them. Build audit fixes 1-5 in a worktree, prove them with a three-level chat chain, merge to dev, then the A35 chat pulls.

## Findings
- **Kind and why:** process; A35 ran 15 hours with 5 children, no row, titles frozen, about 30 messages (16 avoidable).
- **Now:** `tools/title-hint.ps1` builds hints only from STATUS/pending rows (`Get-Hints`); `loop-card.ps1` prints "In flight: none". `tools/poteto-gate.ps1` Brief mode checks only line 1 and 2 of an item brief. `director-notes.ps1` prints only for directors, only on compact; item chats get nothing at start. `merge-task` and `merge-preflight.ps1` assume the base is `dev` (`dev..$branch`, "behind dev") and edit STATUS. `a35-survival.py` exists only on `docs/A35-ux-pass` (46 lines, not in `tools/`, not in the gate).
- **Where / Risk:** `tools/*.ps1`, `.claude/settings.json` hooks, `.claude/skills/merge-task/`, `docs/loop.md` (ask-permission file), `AGENTS.md`, `.coord/templates/item.md`, `tools/roadmap-data.ps1` (T18 folds by nearest `idea:` row). Riskiest: a hook that crashes or prints wrongly in every chat (all hooks must fail open) and a gate that blocks legitimate briefs (ship it log-only first, per `guard.mode`).
- **Source:** own scout (audit report plus the files above). Does not touch `A35-ux-pass` branches or chats.

## Plan
- **After:** (1) Umbrella rule: parent row plus a child row per sub-item at dispatch (child Scope starts `child of <ID>:`; `roadmap-data.ps1` folds those under the parent); umbrella card gets a `## Children` list and an `Overrides:` line (template); the item brief template and `poteto-gate.ps1 -Mode Brief` refuse a brief naming an item with no row. (2) New `tools/start-card.ps1` on SessionStart startup, resume, compact: role (item, child, umbrella, director), title rule, merge rule, Codex split, files to read. (3) `title-hint.ps1`: with no row but a card, prints `title-hint <ID>: NO ROW (card phase=...)`; every turn, warns when the real title (read from the session file, fail open) differs from the computed one. (4) Crosstalk is need-to-know only (user, 2026-11-21): no acks, no progress FYIs. A child messages its parent only for coordination, for a major result the parent asked to hear about, or on the user's request. At review/merge ready the chat prints one unmistakable block: `READY: <ID-slug>. Go to <chat> and run /merge-task <ID-slug>` (or `/review-task` here). Rule lines in `docs/loop.md` and `AGENTS.md`; children rebase when ready; shared board `.coord/tasks/<umbrella>-board.md` (queue, order, landed) replaces messages; one-level-chat rule gets an umbrella exception with the user's go. (5) `merge-task`: "into an umbrella" variant (`merge-preflight.ps1 -Base <branch>`, commit review, no STATUS edits) and the umbrella-to-dev merge; `tools/survival.py` promoted from `a35-survival.py`, optional gate step (`-Umbrella <branch>`).
- **Expected time:** about 3 h build, plus the chain test.
- **Must not change:** non-umbrella items behave as today; no product code; A35 branches and chats.
- **Scope questions:** none; child row convention (Scope prefix) is my choice, the alternative is a new State word.
- **Test plan:** each script has `-SelfTest` (title-hint, poteto-gate, start-card, preflight base case, survival on a temp repo); `roadmap-data.ps1 -Check`; gate. Step 7 chain test: throwaway umbrella `Loop21t-chain` starts child, grandchild, great-grandchild; check rows, titles at each level, start card, merges up the chain.
- **Gates:** `tools/gate.ps1 -Offline`, each `-SelfTest`, Codex review (Medium).
- **Needs from you:** Approve; the go for the chain test (starts 3 chats).

## Files
- `tools/title-hint.ps1`
- `tools/poteto-gate.ps1`
- `tools/start-card.ps1`
- `tools/merge-preflight.ps1`
- `tools/survival.py`
- `tools/gate.ps1`
- `tools/roadmap-data.ps1`
- `.claude/settings.json`
- `.claude/skills/merge-task/SKILL.md`
- `.claude/skills/merge-task/branches.md`
- `.coord/templates/item.md`
- `docs/loop.md`
- `AGENTS.md`

## Contracts
None (tooling, no product code).

## Approve
User, 2026-11-21: "crosstalk should be on a need to know basis. Major task completed that Parent chat has requested to know about / I the user has requested to send to parent; review/merge ready states: a very crystal clear message that task is ready, go to <chat> and run `/merge-task <slug>` or something for /review in the current chat. All updates dont need to be sent back and forth unless its for a coordination purpose. Other than that, i approve"

## For Codex
Role: terra, because it spans several scripts with selftests. Branch is checked out: never switch, merge or commit. Build exactly the Plan's After for the Files above; offline checks only; never edit STATUS or this card. Report `.coord/reports/Loop21-umbrella-items.md`.
