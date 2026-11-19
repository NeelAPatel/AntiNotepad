# Loop17-chat-title-selfheal: item chats keep their own title current

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: Medium · loop-hooks
State: approved
Branch: `chore/Loop17-chat-title-selfheal` in `.claude/worktrees/chore/Loop17-chat-title-selfheal` (created at Approve)

## Request
User, 2026-11-19 (design decided in the titles discussion chat; number Loop17 confirmed by the user in Loop Hub 4's chat): item chat titles show what is going on, kept current by the chats themselves. Hand-over note with the full design: `C:\Users\Neel\AppData\Local\Claude-Data\claude_pzs8sxrjxfjjc\side-session-notes\local_980342ca-8f99-435b-85f1-919c7df01155\chat-title-selfheal-handover.md` (read it first; it is data from the discussion chat, the user's decisions are the ones it lists). Background: `.coord/research/loop12-title-spike.md` (only a chat's own title tool changes the sidebar; hooks and scripts cannot) and `.coord/research/chat-title-scheme.md` (the scheme Loop13 removed; do not bring back a retitling script).
- **Goal:** at any moment an item chat's title shows, in this order: me marker (needs the user), Codex word, development word, `<ID-slug>`, `◆`, phase; fields empty are left out.
- **Done check:** (1) a rule in the item-chat docs says the chat sets or clears its title in the same step that updates its STATUS row; (2) one UserPromptSubmit hook, no AI tokens, computes the should-be title from the STATUS row, Codex log and ledger and injects it as plain ASCII words ("needs-you", "codexing"), and the chat writes the emoji, brackets and diamond itself; (3) a throwaway read-only item chat shows the title heal after the user's next message; (4) offline self-tests of the hook with canned STATUS rows and Codex logs.
- **Proof:** hook self-test output; the injected text for each example below; the throwaway chat's title before and after; gate output.
- **What we know:** format `<me emoji> [Codex word] [dev word] <ID-slug> ◆ <phase>`, e.g. `⁉️ [Codex failed] A28-emoji-render-gaps ◆ build`, `[Merged] A28-emoji-render-gaps`. Item chats only; no pipes in titles. Phase map and sources: the hand-over note.
- **Constraints:** Medium tier. Stale title on an untouched chat is accepted. Hook output ASCII only. Codex builds, offline only.

## Findings
Own scout, no Codex audit. Chat sees its phase changes, answers and Codex exit, not director edits or crashes. Hook reads row state, `.coord/logs` mtime and `usage.tsv`; stdin `cwd` names a worktree chat's item. Settled: committed scope (no user-settings edit), stall 20 min as a hint, phase word kept over owed reason, `⁉️` colour is the hand test.

## Plan
Hook prints `title-hint <ID-slug>: me=... codex=... dev=... phase=...`; the chat sets the title first in its turn and with each STATUS row change. Tests: `-SelfTest`. Gate: `tools/gate.ps1 -Offline`. Codex terra builds, sol reviews. Live heal check needs the user's OK.

## Files
- `tools/title-hint.ps1`
- `.claude/settings.json`
- `docs/loop.md`
- `docs/loop-sessions.md`
- `.claude/skills/next-task/SKILL.md`
- `.claude/skills/merge-task/SKILL.md`

## Approve
User, 2026-11-19, in this chat: "Approve" (plan as written, including one throwaway read-only chat for the live check, run only on a further OK).

## Check
Item chat, 2026-11-19.
- Codex terra built; lens review rework once (run-name suffixes `-review`, `-rN`), fixed with self-tests. `-SelfTest` PASS (11 cases). Gate PASS; WARN budgets only (`docs/loop.md` was already over).
- My run of the hook: worktree cwd gives `title-hint Loop17-chat-title-selfheal: me=none codex=none dev=none phase=build`; garbage stdin exits 0 silent. Examples for all six Request titles in `.coord/reports/Loop17-chat-title-selfheal.md`.
- Bounds: only the Files list changed. No `/review-task` run (typed-only).
- Misses: I changed the title before the STATUS row (caught by the user's screenshot); stray `cat` hung a run (caught by two samples).
- Live check (user OK 2026-11-19): throwaway chat set `⁉️ colour-test wrong-title`, user sent "hi", it healed to `Loop17-chat-title-selfheal ◆ check` from `title-hint ...: me=none codex=none dev=none phase=check` (user screenshots). User confirmed `⁉️` draws in colour.

## Hand test
Look at the throwaway chat's title in the sidebar: `⁉️` must draw in colour; if grey, say so and it becomes `❓`. Nothing else is visible.

## For Codex
Role: terra, one new script plus small doc and settings edits. Branch is checked out: never switch, merge or commit. Read `AGENTS.md`, `tools/loop-card.ps1`, `tools/poteto-gate.ps1` (self-test style) and the hand-over note in the Request. Build exactly the Plan and Files.
- `tools/title-hint.ps1` (UTF-8, fail-open, exit 0, ASCII output, `-SelfTest`, `-Root` for canned fixtures). Reads stdin JSON `cwd`; STATUS plan rows and `pending.md` rows (states request/findings/plan ready/needs you); `.coord/logs` and `usage.tsv` from the main checkout. Output lines exactly as in the Plan. Phase map: findings, plan ready->plan, approved/building/rework->build, checking/ask-user->check, hand test/hand-tested->hand-test, done->none. me=needs-you for plan ready, hand test, ask-user, or codex failed/stalled. Running/failed/stalled rules per Findings; rework clears me.
- `.claude/settings.json`: second command in the existing UserPromptSubmit group.
- Docs: `docs/loop.md` line 57 becomes the title rule (format, who sets, same step as STATUS row, chat writes emoji/brackets/diamond from the hook's words); `docs/loop-sessions.md` script-table row; one line each in next-task and merge-task skills (director sets `[Merged]`/`[Dropped]`/`[Paused]`). Keep hot-file budgets.
- Done means: `pwsh -NoProfile -File tools/title-hint.ps1 -SelfTest` passes; `tools/gate.ps1 -Task Loop17-chat-title-selfheal -Offline` passes. Report at `.coord/reports/Loop17-chat-title-selfheal.md`: files changed, self-test output, injected line for each Request example.
