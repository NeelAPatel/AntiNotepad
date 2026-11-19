# Loop20-codex-labs-and-scout: give lab pages and read-only inventories to Codex

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: Medium · loop-docs
State: approved (user, 2026-11-19)
Branch: `chore/Loop20-codex-labs-and-scout` in `.claude/worktrees/chore/Loop20-codex-labs-and-scout` (created at Approve)

## Request
User, 2026-11-19 (via PStack Manager, "Yes, send it"): offload more item-chat work to Codex without losing Claude's inline visuals or browser checks.
- **Goal:** item chats hand lab-page writing and big read-and-grep inventories to Codex, and keep serving, browser checks, user talk and decisions.
- **Done check:** (1) `.coord/research/pstack-director-playbook.md` section 3 Lab row says Claude writes a short lab brief, a Codex quick or terra run writes the HTML in the item worktree, Claude serves it (`tools/playground-serve.ps1`), checks it in the browser pane and shows it; with a size threshold (new page or about 150+ lines to Codex; a few small tweaks stay with Claude). (2) Findings row allows a read-only Codex quick run (`CODEX_SANDBOX=read-only`, short report format) for inventories, floor of about 5 files or fewer stays with Claude; or folded into the scout-role item if one exists (check first). (3) "Keep with Claude" list written: inline `show_widget` visuals, serving and browser-pane checks, user talk, decisions, card and pending edits. (4) `docs/agents.md` updated if roles change; `.coord/research/pstack-user-files` mirrors refreshed if any pstack file changes. (5) `tools/loop-lint.ps1` and gate clean.
- **Proof:** the diff of the playbook table and docs; gate output; one lab brief example in the playbook; a real item chat (next lab-first item) shown to follow it, or a stated reason none ran yet.
- **What we know:** read-only review of Master 7's item chats A29-shortcut-map, A31-list-default-type, A32-selection-convert-menu (2026-11-19): all follow the playbook; Codex built A32. Claude itself wrote the A29 lab page (about 11k characters), a 5k key inventory gathered with greps, the A31 lab page (5k plus 8 small edits), and read about 10 files per chat for Findings. Cause: playbook section 3 "Claude builds the lab page" and Findings stays with Claude until the Codex scout role exists (listed under "Not yet in place"). The Codex sandbox cannot bind ports, so serving stays with Claude.
- **Constraints:** Medium tier. Docs and playbook text only; no product code. Check `docs/agents.md` and STATUS for an existing scout-role item before adding one. Never brief Codex with port, server or browser checks. Codex may build, offline only. Stop at Approve.

## Findings
Own scout, 2026-11-19 (5 files, below the Codex floor). Process item, Medium confirmed. No scout-role item exists (only Loop Hub 4 notes), so Findings is folded in here. `quick`/`terra` and read-only sandbox already exist; no new role. Mirrors unchanged. Codex cannot serve, so Claude serves labs.

## Plan
Branch `chore/Loop20-codex-labs-and-scout` (created at Approve). Files: `.coord/research/pstack-director-playbook.md`, `docs/agents.md`. One commit if both build clean.
Edit playbook section 3 (Lab and Findings rows, Keep-with-Claude list, lab brief example, Not-yet-in-place) and add a short note to `docs/agents.md`. Verify with loop-lint and gate.
**Proof plan:** diff, gate output, brief example. Real-item proof: none has run yet; the next lab-first item (A29/A31 already built labs) will show it, so I state that reason.
**Your call at Approve:** approve, change or drop.

## Approve
User, 2026-11-19, in this chat: "Approve" (plan: playbook Lab and Findings rows, Keep-with-Claude list, lab brief example, `docs/agents.md` note).

## Check
Gate offline PASS (build, selftest, seam, tokens, scope, loop-lint, size). Diff scope: the playbook, `docs/agents.md` and this card only. Commit review done: one tidy commit `71baddc`.

## Hand test
None: nothing visible (loop docs).

## Files
- `.coord/research/pstack-director-playbook.md`
- `docs/agents.md`
- `.coord/tasks/Loop20-codex-labs-and-scout.md`
