# Loop12-chat-title-scheme: chat titles show state and Codex badge, set by hooks and scripts

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: Medium · tooling-hooks (spike result unknown; hooks run in every chat)
State: hand test
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
- `.coord/research/loop12-title-spike.md`
## Contracts
None (no product code; tooling and docs only).
## Approve
Approved by the user, 2026-11-17: fallback plan above (script computes title, chat makes one rename call).
## For Codex
Role: terra (`docs/agents.md`), because one new PowerShell script plus small edits across a few scripts and docs. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files; re-read before editing. Read `.coord/research/chat-title-scheme.md` (4, 4b, 4c) and `.coord/research/loop12-title-spike.md` first.
- Build exactly the Plan above for the Files above; no drive-by changes.
- `tools/chat-title.ps1` contract: params `-Item <ID-slug>` (default: derived from the current git branch `prefix/ID-slug`; none found: print nothing, exit 0), `-State blocked|needs-you|waiting|working|parked|standby`, `-Codex` (adds the robot badge), `-Phase` (default: from the card's `State:` line mapped to request, findings, plan, build, check, hand test; rework round adds ` · r2`), `-Merged`, `-Closed -Name <text>`, `-Check`. Glyphs per research 4b: blocked U+26D4, needs-you U+2753, waiting U+23F3, working U+1F528, parked U+1F17F U+FE0F, standby U+1F4A4, Codex U+1F916. Title `<state><badge> <ID-slug> · <phase>`; `[Merged] <ID-slug>`; `[Closed] <name>`. It stores the last title in `<main checkout>/.coord/titles/<ID-slug>.txt` (add `.coord/titles/` to `.gitignore`) and prints one line `SET TITLE: <title>` only when it differs from the stored one (it then updates the store), otherwise prints nothing. Set `[Console]::OutputEncoding` to UTF-8 without BOM before printing so emoji survive a hook pipe (the spike showed `??` otherwise). `-Check` runs fully offline on a temp store: every state, the badge, no-change prints nothing, merged and closed forms, bad state fails, UTF-8 bytes round-trip.
- Callers: `tools/loop-card.ps1` runs it with `-State working` after reading hook stdin; `codex-run.sh` prints the line at launch (`-State waiting -Codex`) and at exit (success: `-State working`; failure or usage limit: `-State blocked`); `tools/gate.ps1` prints `-State blocked` when the gate fails (only when it runs in an item branch; never changes its pass/fail logic); `.claude/skills/merge-task/SKILL.md` gains a final step: after cleanup run `-Merged` and have the director rename the item chat with the printed title. All callers are best effort: a missing script or error must never change an exit code.
- Docs (text rules, short): replace R1/R2/R4/R5/R6 title rules in `docs/loop.md:55`, `docs/loop-sessions.md`, `next-task/SKILL.md:85`, `docs/wiki/Who-Does-What.md` with the legend and the rule "when a hook or script prints `SET TITLE: ...`, call `set_session_title` with `self` once; at asking the user set needs-you before the reply, after a report set parked". Roadmap legend: collapsible block in `tools/roadmap/index.html`. Say in the docs that hooks cannot retitle (spike), so the chat makes the call.
- Build checks: no product code changes, so no app build is needed; run `pwsh -NoProfile -File tools/chat-title.ps1 -Check`, `pwsh -NoProfile -File tools/loop-lint.ps1`, and `pwsh -NoProfile -File tools/gate.ps1 -Task Loop12-chat-title-scheme -Offline`.
- Never launch or kill AntiNotepad, never run `--quit`, never edit `.coord/STATUS.md` or this card. Offline checks only (no ports, servers, browser).
- Done means: `chat-title.ps1 -Check` passes; loop-lint and gate pass offline; all edited callers still exit as before.

Report `.coord/reports/Loop12-chat-title-scheme.md`, short: 1 Summary; 2 Change list (`file:lines | what | Plan line`); 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review (skip, not a director run); 8 Checks run; 9 Hand checks for the user.

## Check
- Gate: PASS (offline; selftest PASS, scope 10 files allowed, loop-lint PASS). Warnings: card 7994 B over its 4096 B budget, `loop.md` and `lessons.md` over budget (lessons is not this item's).
- Codex review: skipped, no product code; I read the 79-line script, all callers and the doc diffs myself. `chat-title.ps1 -Check` PASS, `bash -n codex-run.sh` OK, live hook run printed `SET TITLE:` in UTF-8 once and nothing on repeat.
- Bounds: within plan. Dropped as planned: sweeper and Stop/Notification hooks. Report notes the gate and live caller tests were not run in Codex's sandbox; I ran the gate and one live hook call.
- Code gaps noticed: the phase comes from the card's `State:` line, so the chat must keep it current (documented). The store records what was printed, not what the app accepted.
- Misses: none.
- Noticed (proposals): a sweeper for 🧊 would need an app API we have not found; the `[Merged]` step is untested until a real merge.

## Hand test
