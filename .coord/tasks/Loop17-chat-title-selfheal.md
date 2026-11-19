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
Own scout, no Codex audit (6 small files, all read).
- **Now:** one committed `UserPromptSubmit` hook exists (`tools/loop-card.ps1`: ASCII, fail-open). `docs/loop.md:57` says titles are free-form. STATUS rows start at Approve; before that the item is a `pending.md` row.
- **Chat sees:** its phase changes, user answers, Codex launch and exit notice. Not director edits, crashes or merge.
- **Hook reads:** row state; `.coord/logs/<ID-slug>*.log` mtime; `usage.tsv` (row at run exit, `exit=N`). Running = log newer than the item's last row; failed = last `exit` not 0; stalled = running, log silent 20 min. Stdin `cwd` names a worktree chat's item; a main-checkout chat gets all in-flight rows.
- **Settled:** committed scope, a second command in the same group (no user-settings edit). Stall 20 min, hint only. Owed reason does not replace the phase. Me marker stays `⁉️`; colour is the hand test.

## Plan
- **After:** hook prints `title-hint <ID-slug>: me=needs-you|none codex=codexing|failed|stalled|none dev=paused|parked|merged|dropped|none phase=findings|plan|build|check|hand-test|none`; the chat sets that title first in its turn. Rule: set or clear in the same step as the STATUS row.
- **Must not change:** `loop-card.ps1` output; no retitling script; director titles.
- **Tests:** `tools/title-hint.ps1 -SelfTest` with canned rows, logs, ledger (running, stalled, failed, paused resume, garbage in fails open). No CONTRACTS rows.
- **Gates:** `tools/gate.ps1 -Offline`; Codex terra builds, sol reviews.
- **Needs from you:** at Approve, OK for one throwaway read-only chat (live heal check).

## Files
- `tools/title-hint.ps1`, `.claude/settings.json`, `docs/loop.md`, `docs/loop-sessions.md`
- `.claude/skills/next-task/SKILL.md`, `.claude/skills/merge-task/SKILL.md`

## Approve
User, 2026-11-19, in this chat: "Approve" (plan as written, including one throwaway read-only chat for the live check, run only on a further OK).

## Hand test
Look at the throwaway chat's title in the sidebar: `⁉️` must draw in colour; if grey, say so and it becomes `❓`. Nothing else is visible.
