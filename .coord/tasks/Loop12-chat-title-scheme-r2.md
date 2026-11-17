# Loop12-chat-title-scheme-r2: find the item from the session, phase from STATUS

Rework round of `Loop12-chat-title-scheme` (merged to dev as df5d174; its card `.coord/tasks/Loop12-chat-title-scheme.md` and spike log `.coord/research/loop12-title-spike.md` still apply).

Tier: Medium · tooling-hooks
State: approved
Branch: `chore/Loop12-chat-title-scheme-r2` in `.claude/worktrees/chore/Loop12-chat-title-scheme-r2`

## Request
User, 2026-11-17: after the one-time retitle, A22 and A23 showed wrong marks although both wait on a hand test. "Yes, rework" to fixing the script's item and phase lookup.
- **Goal:** the title script gets the item and the phase right for every item chat, wherever the chat runs.
- **Done check:** `chat-title.ps1 -Check` covers the cases below and passes; loop-lint and gate pass offline.
- **Proof:** `-Check` output; report table of case, input, resulting title.

## Findings
Proven today (2026-11-17, `.coord/titles/`, session list, hook output):
1. Item chats run in the main checkout on `dev`, so "item from git branch" finds nothing: the prompt hook prints nothing for them.
2. Phase is read from the card, but an item's card lives in its worktree after Approve; from main the script missed it and printed `request` (A23).
3. `codex-run.sh` passes the task name, which can be a round name `<ID-slug>-rN`; the item must drop `-rN` (and `r N` becomes the rework suffix).
4. `CLAUDE_CODE_SESSION_ID` is set in a chat's Bash tool env and equals the hook stdin `session_id` (verified in this chat).

## Plan
After:
- Registry `<main>/.coord/titles/sessions/<session id>.txt` holds the chat's item. `chat-title.ps1 -Item X` (any run with an explicit item) registers `$env:CLAUDE_CODE_SESSION_ID` when set. New `-SessionId <id>`: item lookup by registry. Item order: `-Item`, registry via `-SessionId` or `$env:CLAUDE_CODE_SESSION_ID`, git branch, else print nothing.
- `tools/loop-card.ps1` reads hook stdin JSON `session_id` and passes `-SessionId` (best effort, unchanged exit codes).
- Phase order when `-Phase` absent: the item's row in `.coord/STATUS.md` (main checkout) State column with the existing regex mapping plus `rework rN` suffix; else the card State line, searched in main and every worktree (`git worktree list --porcelain`); else `request`.
- `-Item` ending `-r<N>` is stripped to the base item and adds the ` · rN` suffix when no phase is given.
- `-Sync` (new, read-only on app files): scan `claude-code-sessions/**/local_*.json`, for each chat whose title contains an `ID-slug` of an existing card or STATUS row (or a bare ID that matches exactly one `ID-slug`), register its `cliSessionId` to that item. Print one line per registration. Never writes app files.
Must not change: output format `SET TITLE: <title>`, dedupe store, glyphs, exit codes of callers.
Gates: `-Check` extended with a temp STATUS fixture, a temp registry, a round-name item, bad session id; loop-lint; gate.

## Files
- `tools/chat-title.ps1`
- `tools/loop-card.ps1`
- `.claude/skills/next-task/SKILL.md`

## Contracts
None (tooling only).

## Approve
Approved by the user, 2026-11-17: "Yes, rework" (item and phase lookup fix, plan above).

## For Codex
Role: terra (`docs/agents.md`), because one script plus a few lines in two files. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then `tools/chat-title.ps1`, `tools/loop-card.ps1` and the Findings above; re-read before editing.
- Build exactly the Plan's After; no drive-by changes. Keep emoji output UTF-8 without BOM.
- `next-task/SKILL.md`: add to the existing title rule line one short sentence: run `tools/chat-title.ps1 -Item <ID-slug> -State working` once at the start so the hook can find this chat.
- Checks (offline): `pwsh -NoProfile -File tools/chat-title.ps1 -Check`, `pwsh -NoProfile -File tools/loop-lint.ps1`, `git diff --check`. Do not run the gate (its selftest launches the app; the Claude session runs it). `-Sync` must be tested only against a temp fixture directory, never the real app data.
- Never launch or kill AntiNotepad, never edit `.coord/STATUS.md`, any card, or app files. Offline only.
- Done means: `-Check` passes with the new cases; callers still exit as before.

Report `.coord/reports/Loop12-chat-title-scheme-r2.md`, short: 1 Summary; 2 Change list; 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 6 Contracts; 7 Lens review (skip); 8 Checks run; 9 Hand checks for the user.

## Check
- Gate: PASS (offline) at the round tip; `chat-title.ps1 -Check` PASS (STATUS-first phase, worktree card, round name, registry, bad session id, sync and ambiguous sync on a fixture); loop-lint PASS (budget warnings only).
- Live (mine): hook run with A22's real session id after `-Sync` printed `SET TITLE: <mark> A22-wrap-hanging-indent · hand test`; `-Sync` registered 63 chats (stale or archived ones too; harmless, a registry entry only acts when that chat sends a prompt).
- Bounds: within plan, 3 files. Codex review skipped, tooling only.
- Misses: found at the user's sidebar after the one-time retitle; cause plan (item from branch assumed, phase from a card that moves); caught by user; should have caught in the spike by running from a main-checkout chat; lesson: test hooks from the chat's real cwd, not from the worktree.
- Noticed: stores hold the last printed title, so any manual test run must be followed by setting the real title (I did this for A22, A23). The old hook in the main checkout keeps running until this round merges.

## Hand test
User, 2026-11-17: pending. Live proof only exists after merge: reply in the A22 chat; its hook should print a SET TITLE line.
