# Loop16-poteto-trigger: make poteto mode start on its own in item chats

Item card (`docs/loop.md`). Budget: 4 KB.

Tier: Medium · loop-hooks
State: approved
Branch: `chore/Loop16-poteto-trigger` in `.claude/worktrees/chore/Loop16-poteto-trigger` (created at Approve)

## Request
User, 2026-11-19 (via PStack Manager; number confirmed by the user in Loop Hub 4's chat): "the director starts the item chat with a correct brief, and poteto mode starts on its own. If that fails, the only fallback is the item chat giving the user a command to copy and paste."
- **Goal:** an item chat started by a director runs `/pstack:poteto-mode` with no user typing; if the harness refuses, the chat shows one line and a copyable command and waits.
- **Done check:** (1) a throwaway read-only chat started from the director template prints "Launching skill: pstack:poteto-mode" in its first turn; (2) a director brief that breaks the template is refused before the chat starts; (3) an item chat whose first turn shows neither the skill launch nor a fenced command block is blocked once and told the fallback text.
- **Proof:** the transcript line for (1); the refusal text for (2); the block text for (3); the hook scripts' own self-checks (offline); gate output.
- **What we know:** `.coord/research/poteto-trigger-evidence.md` (ee3b29a): command alone on the brief's first line loaded 10 of 10; inside a sentence refused 11 of 11; the template passed a test on 2026-11-19. The paste-only gate in `.coord/research/pstack-director-playbook.md` section 2 (commit 2f334eb) came from a flawed test and is to be replaced. Observed harness behavior, not documented; an update could change it.
- **Scope (from PStack Manager):**
  1. Playbook section 2, `/next-task` and any other brief template that starts item chats use the template in the evidence note; the paste gate stays only as the fallback line.
  2. Director side: a PreToolUse hook on `mcp__ccd_session__start_session` refuses a `prompt` containing "You are the item chat" unless its first line is exactly `/pstack:poteto-mode` and the explicit Skill-call line follows.
  3. Item-chat side: a Stop hook with a `stop_hook_active` guard; on the first turn it passes only if the transcript shows the skill launch or a fenced block holding the command.
- **Constraints:** Medium tier. Both hooks go in user scope or `.claude/settings.local.json`, never only in the committed settings, so they fire in Master 6, Loop Hub 4 and item chats. Editing user-level settings needs the user's own approval in this chat at Approve; do not touch them before. Hooks fail open if they crash (docs/playbook.md). Prefer a short script plus a text rule; Codex may build, offline only; no live-chat checks by Codex. Findings may end in "drop" or a smaller cut if a hook cannot read the `prompt` field or the transcript.

## Check
Item chat, 2026-11-19.
- (1) Throwaway chat from the template: transcript shows tool_use Skill then "Launching skill: pstack:poteto-mode"; Stop hook on that real transcript logs `first-turn-pass: Launching skill: pstack:poteto-mode`.
- (2) Broken brief ("First run ..." in a sentence) refused before start: "Use the item chat template: /pstack:poteto-mode on line 1 and the explicit Skill tool call on line 2." (hooks in block mode).
- (3) Block text proven offline only (`-SelfTest`: missing launch exits 2 with the fallback line and fenced command; later turn, active guard, bad input pass). Not yet seen live in a chat.
- Miss found by the live check: the skill's injected text is an `isMeta` user message, so the first Stop version always said "past-first-turn". Fixed (commit 3), self-tests added. Caught by: live check; should have caught: self-test built on a hand-made transcript.
- Gate PASS (`tools/gate.ps1 -Offline`). Hooks in `~/.claude/settings.json` now block mode, pointing at this worktree's script; repoint to the main checkout path after merge.
- Codex review round 1: VERDICT rework. Findings: (1) major, lowercase "you are the item chat" (what `/next-task` wrote) skipped the brief check; (2) minor, `/pstack:poteto-mode-unavailable` passed the fenced check. Both fixed in `-r2` (case-insensitive marker, strict command boundary, self-tests added).
- Bounds check: within plan (script, self-tests, playbook, next-task text, docs/playbook row); `-LogOnly` is a reported, justified addition for the log-only rollout; user-settings hook edit was approved at Approve.
- Misses: (a) found at live check, cause `test`: first Stop version counted `isMeta` skill text as a user turn; should have caught: a self-test built from a real transcript. (b) found at review, cause `code`: marker case and fenced-suffix boundary; caught by Codex review. (c) found at my r2 check, cause `code`: the new boundary rejected a bare command with CRLF; fixed with a self-test. Lesson: build Stop self-tests from real transcript shapes (CRLF, isMeta, tool_result).
- Gate PASS after r2. No hand test (loop tooling).
- Noticed: `/review-task` cannot be run by item chats (typed-only); proposal row `idea: review-task-chat-runnable` in `.coord/pending.md`.

## Hand test
None: nothing visible (loop tooling); the live check is the throwaway chat in the Done check, run by the item chat.

## Findings
Scout: this chat (Medium; no Codex audit needed, nothing built yet). Evidence = this chat's own transcript plus the evidence note.
- **Now:** a started chat gets the app's wrapper `Background ...` then `Task:` then the director's `prompt` text (transcript line 3 of this chat). The `/pstack:poteto-mode` line sat right after `Task:` and the Skill call worked (tool_use then "Launching skill: pstack:poteto-mode"). So a template with the bare command first works in practice (10/10, this chat 11/11).
- **Director hook (can do):** `PreToolUse` gets `tool_name` and the whole `tool_input` on stdin, so `prompt` is readable; a matcher `mcp__ccd_session__start_session` is a normal MCP matcher; exit 2 or a `deny` JSON refuses with a message. Not yet proven live that the app's own in-process `ccd_session` tools fire hooks; proof = log-only first run (Plan step 4).
- **Item-chat hook (can do, with one change):** `Stop` gets `transcript_path`, `stop_hook_active`. The transcript is JSONL. The first real turn shows as `assistant tool_use name=Skill input.skill=pstack:poteto-mode` then a `tool_result` "Launching skill: pstack:poteto-mode". Change from the Request: the first user message is NOT the bare brief; the app prepends a system-reminder and the `Background`/`Task:` wrapper, so the hook finds "item chat" by searching the first user message for the line after `Task:` equal to `/pstack:poteto-mode`, not by line 1.
- **Not done by hooks:** a hook cannot make the model call the Skill; it can only refuse a bad brief (director side) and push back once on a bad first turn (child side). Both depend on observed, undocumented behavior.
- **Smaller cut if needed:** director hook only + template. The Stop hook is the part with the most guesswork.

## Plan
After: director brief = template from the evidence note; bad brief refused; item chat with no skill launch and no fenced command in its first turn is blocked once with the fallback text.
- Files: `tools/poteto-gate.ps1` (one script, two modes `-Mode Brief|Stop`, plus `-SelfTest`); `tools/poteto-gate.tests` inside it (offline self-tests with canned JSON stdin and a canned transcript); hooks wiring (user scope, needs your approval); `.coord/research/pstack-director-playbook.md` section 2; `.claude/skills/next-task/SKILL.md` brief template; `docs/playbook.md` one line on the new hook.
- Steps:
  1. Codex writes the script and self-tests (offline): Brief mode refuses prompts with "You are the item chat" unless line 1 is exactly `/pstack:poteto-mode` and line 2 starts "Your first action is a Skill tool call"; Stop mode checks the first turn, honors `stop_hook_active`, fails open on any crash or unreadable transcript, logs to `build/poteto-gate.log`.
  2. I update playbook section 2 and the `/next-task` template (paste gate kept only as the fallback line).
  3. Gate run (loop-lint, self-tests).
  4. With your OK: add both hooks to your user settings in log-only mode, then start one throwaway read-only chat from the template; check the log shows the Brief hook saw the prompt and the Stop hook saw "Launching skill". Then switch to block and re-run the two failure cases (bad brief refused; chat without launch blocked once).
- Tests/gates: script self-tests; the three Done checks above; fail-open check by feeding garbage stdin.
- Tier Medium; Hand test none (as card).
- Needs from you at Approve: (a) OK to edit `C:\Users\Neel\.claude\settings.json` hooks (log-only first); (b) OK to start one throwaway read-only chat for the live check.

## Approve
User, 2026-11-19, in this chat: "Approve" (plan as written, including the user-settings hook edit in log-only mode first and one throwaway read-only chat for the live check).

## Files
- `tools/poteto-gate.ps1`
- `.coord/research/pstack-director-playbook.md`
- `.claude/skills/next-task/SKILL.md`
- `docs/playbook.md`

## Contracts
None (loop tooling; no product contracts).

## For Codex
Role: terra, a script with several cases and self-tests. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then the Files.
- Write only `tools/poteto-gate.ps1` (pwsh 7, style of `tools/guard-bash.ps1`). `-Mode Brief` reads hook JSON on stdin; for a `tool_input.prompt` containing "You are the item chat" it requires line 1 exactly `/pstack:poteto-mode` and line 2 starting "Your first action is a Skill tool call"; else exit 2 with stderr text naming the template. `-Mode Stop` reads `transcript_path` and `stop_hook_active`; if active exit 0; find the first user message and take the line after `Task:`; if it is `/pstack:poteto-mode`, require in the first turn (before the second user message) an assistant tool_use Skill `pstack:poteto-mode` or an assistant text with a fenced block holding `/pstack:poteto-mode`; otherwise exit 2 with the fallback text (one line plus a copyable command). Not an item chat: exit 0. Any exception: log, exit 0. Log to `build/poteto-gate.log`. `-SelfTest` covers the Plan's test cases with canned stdin and a canned transcript.
- The item chat edits docs and playbook files, not you. Offline checks only; never launch AntiNotepad, never edit `.coord/STATUS.md` or this card.
- Done means: `pwsh -NoProfile -File tools/poteto-gate.ps1 -SelfTest` passes; gate passes.
Report `.coord/reports/Loop16-poteto-trigger.md`, short: Summary; Change list; Behavior; Out-of-scope files; Deviations; Checks run.
