# Loop12-chat-title-scheme: chat titles show state and Codex badge, set by hooks and scripts

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: Medium · tooling-hooks (spike result unknown; hooks run in every chat)
State: approved
Branch: `chore/Loop12-chat-title-scheme` in `.claude/worktrees/chore/Loop12-chat-title-scheme` (created at Approve by `tools/approve.ps1`, cut from the dev tip)

## Request
User, 2026-11-17: adopt the approved title scheme from `.coord/research/chat-title-scheme.md` (sections 4, 4b, 4c, 5): `<state><badge> <ID-slug> · <phase>`; states ⛔ ❓ ⏳ 🔨 🅿️ 💤; badges 🤖 (Codex running), 🧊 (idle over an hour); `[Merged]` / `[Closed]` for retired chats; director names keep `<Role> <N> · LP<vX.Y.Z> · <slug>`. Hook and script driven, no AI tokens; AI self-rename only as fallback. "Ensure we handle this with pstack and our dev loop in mind so that these rules are followed as deterministically and at minimal usage % cost as possible." Master 6 merges it into dev when done, then a one-time pass retitles the frequently touched chats.

- **Goal:** every chat's title shows its state and Codex badge, updated by hooks/scripts, at zero AI token cost.
- **Done check:** (1) spike result with evidence (done: fallback); (2) the script flips 🔨 ⏳ ❓ 🅿️ 🤖 ⛔ on a test chat at the right moments; (3) `[Merged]` by `/merge-task`; (4) gate and `-Check` pass offline.
- **Proof:** spike log, event/hook/title table, gate output.
- **What we know:** titles are held by the app; see Findings.
- **Constraints:** no model calls in hooks; scripts and text rules over new gates; installed copy untouched; Codex builds, checks offline only.
- **Scope:** `tools/chat-title.ps1`, hook line in `loop-card.ps1`, `codex-run.sh` and `gate.ps1` lines, `/merge-task`; docs `loop.md:55`, `loop-sessions.md`, next-task `SKILL.md:85`, `Who-Does-What.md`; roadmap legend. Memory notes are the user's call; the one-time retitle is Master 6's.

## Findings
Spike done (log: `.coord/research/loop12-title-spike.md`). **No script or hook can retitle a chat in the desktop app.** Editing the session file: ignored, then overwritten at the next turn. A `UserPromptSubmit` hook `sessionTitle` runs but the sidebar does not change (known app issue). Only the app's `set_session_title` tool works, from the chat itself or a director. So the scheme is kept but set by one cheap tool call, with a script computing the exact title (fallback in research 4c).

## Plan
**Goal:** every title follows `<state><badge> <ID-slug> · <phase>` with the fewest model tokens: a script decides the title, the chat makes one `set_session_title` call only when the title changes.
**Script:** `tools/chat-title.ps1 -State -Codex -Phase`: builds the title (phase from the item card), compares with the last title stored in `.coord/titles/<cliSessionId>.txt` (gitignored), prints `SET TITLE: <title>` only if different, else nothing. `-Check` runs offline.
| Event | Hook or script | Chat does |
|---|---|---|
| User sends a message | `UserPromptSubmit` (`loop-card.ps1`) | 🔨 line, only if the stored title differs |
| Codex launch / exit | `codex-run.sh` prints the line (🤖, ⏳ / clear; ⛔ on failure) | one call |
| Gate fails | `gate.ps1` prints the line (⛔) | one call |
| Asks the user (Approve, decision, hand test) | text rule: call before the reply, ❓ | one call |
| Report done, task open | text rule 🅿️ | one call |
| Merge cleanup | `/merge-task` prints `[Merged] <ID-slug>` | one call |
Dropped: 🧊 sweeper (nothing can set it without the tool); `Stop`/`Notification` hooks (cannot retitle). Retired chats: director calls the tool.
**Expected time:** about 2 h build, 1 chat. **Tests:** `chat-title.ps1 -Check` (table above, no-change prints nothing, bad input); `loop-lint`; gate. **Hand test:** one item chat shows 🔨, ❓, 🤖 at the right moments.

## Files
- `tools/chat-title.ps1`
- `tools/loop-card.ps1`
- `.coord/tools/codex-run.sh`
- `tools/gate.ps1`
- `.claude/skills/merge-task/SKILL.md`
- `.claude/skills/next-task/SKILL.md`
- `docs/loop.md`
- `docs/loop-sessions.md`
- `docs/wiki/Who-Does-What.md`
- `tools/roadmap/index.html`
- `.gitignore`
## Contracts
## Approve
Approved by the user, 2026-11-17: fallback plan above (script computes title, chat makes one rename call).
## For Codex
## Check
## Hand test
