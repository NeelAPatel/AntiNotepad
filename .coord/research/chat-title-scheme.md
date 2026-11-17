# Chat title scheme (proposal, planning only)

Status: proposal for the user's decision. Nothing here is adopted. Adopting it becomes a Loop item for Loop Hub 4.
Written 2026-11-17 by the title-prefix planning chat.

## 1. Rules found today, and where they live

| # | Rule | Where | Date |
|---|---|---|---|
| R1 | Item chat title starts `[Codexing..] ` while a Codex run it launched is going; it renames itself (`self`), removes it on exit | memory `codexing-title-prefix`; `docs/loop.md:55`; `.coord/research/archive/lv4-shared-brief.md:72` | 2026-11-08 |
| R2 | After merge and cleanup the chat becomes `[Closed] <ID> · merged · <slug>` | same three places (`lv4-shared-brief.md:70`) | 2026-11-08 |
| R3 | Only the top-level chat renames sub-chats; sub-chats ask it | memory `top-level-renames-subchats` | 2026-11-02 |
| R4 | Item chat renames itself `<ID> · review · <slug>` at Check, `<ID> · test · <slug>` at hand test; "the master renames it if it cannot" | `docs/loop-sessions.md:44`; `.claude/skills/next-task/SKILL.md:85`; `docs/wiki/Who-Does-What.md:76,85` | v5.4.0 |
| R5 | Director name `<Role> <N> · LP<vX.Y.Z> · <slug>`; the merging director renames all live directors on a loop version bump; retired: `Master <N> · Loop v<V> · <slug> (closed)` | `docs/loop-sessions.md:26-30,51` | 2026-11-06 |
| R6 | Item step names `<ID> · <step> · <slug>` (scan, audit, build, review, test) | `docs/loop-sessions.md:34-35` | 2026-10-29, updated v5.4.0 |
| R7 | Idea only: a mod showing a per-chat row, progress bar and Codex icon; `[Codexing..]` and `[Closed]` are the stopgap | `.coord/tasks/L16-lv41-agent-progress-panel.md` | 2026-11-08 |

Not found: any title rule in `docs/playbook.md`, `.coord/research/pstack-director-playbook.md`, `docs/loop.md` beyond R1/R2.

## 2. What the live sidebar looks like (2026-11-17)

Eight shapes at once:
- `A23 · paste-keep-structure`, `A22 · wrap-hanging-indent` (R6 without step)
- `T17-dev-staged-wall (standby)` (ID-slug with suffix; matches nothing in docs)
- `A14-search-open-fade-up · hand-tested`, `A13 · search-toggle-off · findings`, `D2 · ui-motion-research · deep research` (free-form phase suffix)
- `Loop11 · build · one-chat-flow`, `A10 · test · hotkey-recorder` (R4/R6)
- `[Closed] A21 · merged · save-delay-setting` (R2)
- `Master 6 · director`, `Loop Hub 4 · loop-tools` (no LP version, R5 not followed), `Roadmap Hub 1 · LPv5.4.0 · roadmap`
- `OLD Master 6 (archive me)`, `[Closed] Master 5 · new director`, `[Closed] Loop Hub 3 · …` (three retired forms)
- Side chats: `btw: investigate why the card got swept in…` (auto titles)

No chat showed `[Codexing..]` at check time, and no title tells waiting-on-you from idle.

## 3. Conflicts

1. **Who renames.** R3 (only the director) vs R1/R2 (self) vs R4 (self, director as fallback). R1/R2 are newer than R3 and the director cannot see a Codex start or exit, so R3 cannot work for state.
2. **Two vocabularies for one thing.** R4/R6 name the role (`build`, `review`, `test`); the user wants state (working, waiting, idle). `review` and `test` are phases, `[Codexing..]` is a state; both want the front of the title.
3. **Bare IDs.** R2, R4, R6 write `A21 · merged · slug`; the repo rule (`AGENTS.md`) is `ID-slug`, never a bare ID.
4. **Retired chats.** Three forms (`OLD … (archive me)`, `[Closed] …`, `… (closed)` in R5), none says whether to archive.
5. **Director version.** R5 requires `LP<vX.Y.Z>`; Master 6 and Loop Hub 4 omit it. It adds length and is only useful to the directors themselves.
6. **Nothing marks "waiting on you".** The state the user most needs is the only one with no rule.

## 4. Proposed scheme

**Principle:** the sidebar truncates and the user scans the left edge. So: one state glyph first, then the name, then the phase last (cut first when truncated).

### Format
- Item chat: `<state><badge> <ID-slug> · <phase>` (badge only while Codex runs)
- Director: `<Role> <N> · LP<vX.Y.Z> · <slug>` (no glyph, pinned; LP version kept per user 2026-11-17, so a loop version bump still renames the live directors; Master 6 and Loop Hub 4 currently omit it and must be brought into line)
- Finished: `[Merged] <ID-slug>` (merged items) or `[Closed] <name>` (any other retired chat), then archived

### Two slots: `<state><Codex badge>`
Codex running is a separate axis from the chat's state, so it is a badge, not a state.

| Slot | Glyph | Meaning |
|---|---|---|
| State | 💤 | Standby: idle, nothing owed either way |
| State | 🔨 | Working: the chat itself is doing things (reading, editing, checking) |
| State | ⏳ | Waiting on background: something will wake the chat without the user (Codex run, background command, gate or CI run) |
| State | ❓ | Needs the user: Approve, a decision, hand test |
| State | ⛔ | Blocked or stuck: gate failed, Codex errored or hit its usage limit, waits on another item or director |
| State | 🅿️ | Parked: a step is done and reported (e.g. investigation finished), the task is still open, nothing is owed by the user (confirmed, user, 2026-11-17) |
| Badge | 🤖 | A Codex run is going (set at launch, cleared when the chat has the result) |
| Badge | 🧊 | Cold: untouched over an hour, prompt cache expired. Only a sweeper (the L16 mod or a scheduled task) can set it, because an idle chat cannot rename itself (proposed) |

No ✅ (user, 2026-11-17). A finished chat gets a text prefix instead:
- `[Merged] <ID-slug>`: the item is merged into dev and cleaned up.
- `[Closed] <name>`: any other chat that is retired (old directors, dropped items, side chats). Both are then archived.

- ⏳ only for things that wake the chat on their own. Roadmap open, an idle dev server and similar do not count.
- Priority when several apply: ⛔ > ❓ > ⏳ > 🔨 > 🅿️ > 💤. Anything that needs the user wins, and the 🤖 badge never hides it.
- Rework round goes in the phase text (`build · r2`), not a glyph.
- Stale ⏳ (a crashed run) is not detectable from a title; the L16 mod handles it from the Codex log time.
- The app's own unread dot overlaps ❓ but is cleared once opened; ❓ stays until answered.
- More stacked glyphs only if a real need shows up.

### Phase suffix (after `·`, short words)
`request`, `findings`, `plan`, `build`, `check`, `hand test`, `merged`. Updated at the same rename as the glyph; no extra renames.

### Who renames
- **Item chats rename themselves** (`session_id self`) at five moments only: Codex launch, Codex exit, asking the user, user answers, merge cleanup.
- **Directors rename only:** their own name; retired chats (`🗄`) and archiving them; any item chat whose self-rename was refused.
- This supersedes R3 for state and R4's phase-role names. R3 survives for retired chats and for chats that cannot rename themselves.
- A stale 🔨 after a crash is the known weakness; the L16 mod would read the Codex log mtime to catch it (L16 card, row "Detect Codex running").

### Sidebar examples
Today:
```
A23 · paste-keep-structure
A22 · wrap-hanging-indent
T17-dev-staged-wall (standby)
[Closed] A21 · merged · save-delay-setting
OLD Master 6 (archive me)
```
Proposed:
```
Master 6 · LPv5.4.0 · app-design           (pinned)
Loop Hub 4 · LPv5.4.0 · loop-tools         (pinned)
⏳🤖 A22-wrap-hanging-indent · build
❓ A23-paste-keep-structure · hand test
🔨 T15-ffmpeg-frame-capture-trial · check
⛔ A24-example-item · build · r2
🅿️🧊 T17-dev-staged-wall · findings
💤 A25-example-item · request
[Merged] A21-save-delay-setting
[Closed] Master 5 · new director
```

## 4b. Legend (the one table to copy into the roadmap and docs)

Title = `<state><badge> <ID-slug> · <phase>`. Glyphs at the left edge; the highest-priority state shows.

| Mark | Slot | Name | Means | Set by | Cleared when |
|---|---|---|---|---|---|
| ⛔ | State | Blocked | Gate failed, Codex errored or hit its usage limit, or waits on another item or director | the chat | the blocker is resolved |
| ❓ | State | Needs you | Approve, a decision or a hand test is waiting on the user | the chat, when it asks and ends its turn | the user answers |
| ⏳ | State | Waiting | Something will wake the chat on its own: Codex run, background command, gate or CI | the chat, at launch | the result is back |
| 🔨 | State | Working | The chat itself is reading, editing or checking | the chat | work stops |
| 🅿️ | State | Parked | A step is finished and reported; the task is still open; nothing is owed by the user | the chat, after reporting | work resumes |
| 💤 | State | Standby | Not started, or idle with nothing owed either way | the chat or director | work starts |
| 🤖 | Badge | Codex running | A Codex run is going, whatever the state | the chat, at launch | Codex exits and the chat has the result |
| 🧊 | Badge | Cold | Untouched over an hour, prompt cache expired (proposed; sweeper-only, so not in the first adoption) | sweeper (L16 mod or scheduled task) | the chat's next rename |
| `[Merged]` | Prefix | Merged | Item merged into dev and cleaned up; archive it | the chat, after cleanup | never |
| `[Closed]` | Prefix | Closed | Any other retired chat (old director, dropped item, side chat); archive it | the director | never |

Priority: ⛔ > ❓ > ⏳ > 🔨 > 🅿️ > 💤. The 🤖 and 🧊 badges never hide a state. Phase words (after `·`): `request`, `findings`, `plan`, `build`, `check`, `hand test`; rework round as `r2`.
Roadmap: add the legend as a collapsible block and show each chat's mark beside its row (a Loop item, after approval).

## 4c. Hook-driven renaming (user, 2026-11-17: hooks and no-AI as much as possible)

Instead of the AI chat calling the title tool, a script sets the title from hook events. No tokens spent.

| Event | Hook | Sets |
|---|---|---|
| User sends a message | `UserPromptSubmit` | 🔨 (clears ❓ and 🅿️) |
| Codex launches | `PreToolUse` on Bash when the command runs `codex-run.sh` | 🤖 badge |
| Chat ends its turn | `Stop` | ⏳ if a Codex run or background task is live; ❓ if the last reply holds the style's `**Your call:**` block or a decision card; else 🅿️ |
| Permission or input prompt | `Notification` | ❓ |
| Codex exits | `codex-run.sh` itself (it already writes the log and ledger) | clears 🤖; sets ⛔ on a failed or usage-limit exit |
| Gate fails | `tools/gate.ps1` exit | ⛔ |
| Phase word | read from the item card (same logic as `tools/session-where.ps1`) | text after `·` |
| Merge cleanup | `/merge-task` script | `[Merged]` |
| Retire a chat | director runs the script | `[Closed]` |
| Over an hour idle | a scheduled sweeper over the same session files | 🧊 (so 🧊 moves into the first adoption) |

Unverified, and the make-or-break question: **how a script may change a title.** The tool the AI uses (`set_session_title`) is an app tool, not callable from a hook. The titles live in `Claude-Data/claude-code-sessions/<id>/<id>/<session>.json`. Writing that file from outside may be ignored or overwritten by the running app, and the sidebar may not refresh. A short spike must prove it before anything else. Fallback if it fails: the chat keeps renaming itself, but through a short checklist driven by the same hook events (hook prints the title to set; the AI does one tool call). Hooks need the session id from stdin; map it to the file by `cliSessionId`.

## 5. If adopted: what changes (for the Loop item, not done here)
- `docs/loop.md:55` and `docs/loop-sessions.md` (R2, R4, R5, R6): replace with this section.
- `.claude/skills/next-task/SKILL.md:85`, `docs/wiki/Who-Does-What.md`: phase renames use the new format.
- Memory notes `codexing-title-prefix`, `top-level-renames-subchats`: update or retire (user's call; not touched here).
- `.coord/research/archive/lv4-shared-brief.md` stays as history.
- Roadmap page: show the legend and each chat's mark (`tools/roadmap/index.html`).
- One-time pass: retitle the existing chats (directors can do it with `set_session_title`; the existing sidebar tool `mark_completed` may also help for ✅).

## 6. Open decisions (one at a time)
1. ~~Glyphs vs text~~ Decided (user, 2026-11-17): emoji glyphs, two slots (state + 🤖 badge), 🔨 working, ⏳ waiting on background, six states.
2. Who renames: changed 2026-11-17 to hook and script driven (section 4c), AI self-rename only as a fallback. Decided (user, 2026-11-17): spike first, as step one of the Loop item; it touches only a throwaway test chat and proves whether a script can retitle a chat and the sidebar refreshes. Old text: Self-rename for item chats at five moments (accept the stale-🔨 risk)?
3. ~~Drop LP version~~ Decided (user, 2026-11-17): keep `LP<vX.Y.Z>` in director names.
4. ~~Retired chats~~ Decided (user, 2026-11-17): `[Merged]` for merged items, `[Closed]` for other retired chats, no ✅ and no 🗄.
5. 🅿️ for "step done, task open" and 🧊 as a sweeper-only cold badge (waiting on user).
