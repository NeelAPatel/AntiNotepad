# L16-lv41-agent-progress-panel

Tier: Low (loop tooling, no app code) · Owner: sub-chat `L16`, Sonnet medium · Builder: Codex Luna medium · Branch (at Approve): `chore/L16-lv41-agent-progress-panel`

## Request
Level 4.1 (user, 2026-11-08, via "Agentic Levels 4+5 · native features"): a Claude Code mod with a panel next to the director chat. One row per linked sub-chat: item, phase, progress bar, and an icon while Codex runs for it. It also shows the loop info Desktop's missing status line would (worktree, item, in-flight items, phase). Reuse `tools/status-line.ps1` and `tools/session-where.ps1` logic where it fits. Stopgap today: the `[Codexing..] ` and `[Closed]` title rules.

## Findings (checked 2026-11-08 against the live `plugin-authoring` types and files)
| Question | Answer | Status |
|---|---|---|
| Can a mod draw a panel beside the chat? | Yes: `$.ui.open({id,title})` plus a `ui.render` hook on `Pane`. `placement: 'dock'` = beside the transcript (terminal fullscreen 110+ columns); remote surfaces (Desktop) that report fullscreen get a dock. | Verified in types; Desktop drawing **not yet seen live** |
| Can a mod list linked sessions or their state? | **No.** `$.session` only reads the mod's own session (id, cwd, messages, model...). No noun for other chats, titles or cards. | Verified (types) |
| Can a mod read files and run commands? | Yes: `$.fs.read/list` (absolute paths, 4 MiB), `$.process.run([argv])`, `$.clock` timers, `$.store` across sessions. So it can read STATUS, cards, `.coord/logs`, `build/`. | Verified (types) |
| Where do chat titles and state live? | Desktop keeps one JSON per chat under `%LOCALAPPDATA%\Claude chat data\<app>\claude-code-sessions\...\local_<id>.json` with `title`, `cwd`, `lastActivityAt`, `isArchived`, `completedTurns`, `postTurnSummary`. It is an app-internal format, not a documented API: it may change. Read-only use. | Seen on disk; stability **assumed** |
| Detect Codex running | Best signal: `.coord/tools/codex-run.sh` writes `.coord/logs/<ID-slug>.<role>.log` (log mtime fresh, ledger row appears at exit). Title prefix `[Codexing..] ` is a second hint. Process name: only `codex-windows-sandbox-service.exe` was present at idle; the name of the run process while working is **unverified**. | Log signal verified in script; process name unverified |
| Where do mod files live? | Repo-shipped mod (folder in the repo, e.g. `tools/progress-panel/`, with `.claude-plugin/plugin.json` and `hooks/`) can merge to dev like any tool. To load it, the user adds `CLAUDE_CODE_PLUGIN_DIRS` to the `env` block of `~/.claude/settings.json` (user-global; I never edit it) or adds the folder as a local marketplace and installs it (`/plugin`). The session's scratch mods folder is user-config, not mergeable. | Verified in reference.md |
| Test without disturbing other chats? | Yes: a mod is per session; a hot-reloaded folder only affects the session that loads it. The pane draws only in that chat. A `claude plugin validate` and `claude plugin test` (headless, with fixture files) touch no chat. | Verified (docs); live Desktop test needs one chat of the user's choosing |
| Does Desktop run the mod at all? | Reference says desktop sessions load plugins and `ui.render` raises on every attached surface. | Verified in docs, **assumed** for this install |

Consequence: "linked sub-chats" cannot come from the mod's API. Rows come from three file sources: STATUS `building`/`checking` rows (item, state), item cards (phase), Claude chat data chat JSON (title, last activity, archived), plus Codex logs.

## Plan
Two cuts. The user picks cut 1 first; cut 2 follows only after cut 1 is seen working.

**Cut 1 (read-only rows, no live Codex): about 55 min.**
- Mod `tools/progress-panel/` (repo): pane `L16 panel`, refreshed by `$.clock` every 10 s.
- Reader helper `tools/panel-state.ps1` (reuses `status-line.ps1` STATUS parsing and `session-where.ps1` phase logic) prints JSON: in-flight items (STATUS state), card phase (0-7 of 8 for the bar), worktree and branch of the main checkout, and the Codex icon from the newest `.coord/logs/*.log` modified in the last 90 s with no matching ledger row.
- Pane draws one row per in-flight item: `ID-slug  ● Phase  [████░░░░]  🟡 Codex` and a header line `dev | main checkout | 3 in flight`.
- `/panel` command to open or close it.
- Selftest-style check: `tools/panel-state.ps1 -Check` against fixture STATUS and card files.

**Cut 2 (chat-aware): about 45 min more.** Add title, last activity and archived from the Claude chat data JSON, so each row shows the real chat name, an "idle for N min" mark, and the `[Codexing..]` prefix as a second Codex hint; drop rows for `[Closed]` chats.

**Gates:** `claude plugin validate`; `tsc -p` on the mod; `claude plugin test` (fixture state to expected rows); `panel-state.ps1 -Check`; `tools/gate.ps1 -Offline` unchanged (no app code touched). Hand test: user loads the mod in one chat, looks at the pane, says pass or fail.

## Files
New: `tools/progress-panel/.claude-plugin/plugin.json`, `tools/progress-panel/hooks/hooks.json`, `tools/progress-panel/hooks/register.tsx`, `tools/progress-panel/types/index.d.ts`, `tools/progress-panel/hooks/register.test.tsx`, `tools/panel-state.ps1`, `docs/panel.md` (10 lines: load it, undo it). One line in `docs/playbook.md`.
Unchanged: app code, gate scripts, `.claude/settings.json`, hooks, `tools/status-line.ps1` and `tools/session-where.ps1` (read, not edited; shared parsing copied or dot-sourced).
Overlap check: expect none with building items (all new paths).

## Risks
- Claude chat data JSON format can change (cut 2 only; panel degrades to cut 1 rows).
- Desktop may not dock the pane (then it draws inline above the prompt; still works).
- Codex run process name unknown; we use the log signal, so a stalled-but-fresh log shows as running.
- Mod load needs one user-global setting that only the user can add.

Undo: remove the `CLAUDE_CODE_PLUGIN_DIRS` entry (or uninstall); nothing else depends on it.

## Expected time
Cut 1 about 55 min, cut 2 about 45 min more; the full item is **over an hour (about 1 h 40 min)**, so cut 1 first.

## Unverified (carry into Build)
1. Pane docks on this Desktop install. 2. Process name of a running Codex job. 3. Claude chat data JSON stable across app updates. 4. `$.fs` may read outside the project folder in Desktop (types say absolute paths allowed).

## Approve

## Build

## Check

## Hand test

## Merge
