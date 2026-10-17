# STATUS (update at every dispatch / milestone / completion)

Phase: setup -> cross-chat tests (BLOCKED on Codex sandbox) -> research
Updated: 2026-10-17

## Running now
Nothing dispatched. Manager session: Claude Sonnet 5.5 medium.

## Last result
- Repo scaffolded and committed (AGENTS.md, CLAUDE.md, .coord/, docs/).
- Comms test 1 FAILED twice: `codex exec --sandbox workspace-write` cannot start shell/file tools.
  Cause (from `~/.codex/.sandbox/sandbox.2026-10-17.log`): sandbox setup refresh cannot validate
  `...\OpenAI\Codex\runtimes\cua_node\...\node_repl.exe` because it is in use (os error 32). Running
  processes holding it: node_repl (x2) from the Codex desktop app. `--disable computer_use -c notify=[]` did not help.
- After user closed the Codex app (no codex/node_repl processes left), test 1 still failed twice with the same os error 32 on node_repl.exe; a plain user-level exclusive open of that file succeeds, so the holder is likely the elevated `codex-windows-sandbox-service` (pid 31524 at the time) or Defender. Untried: reboot, restart that service (admin), Defender exclusions for Codex helpers (doctor's own advice, user-owned security setting).
- Not using `danger-full-access` (user rule: folder-limited sandbox).

## Next action
Ask user to close the Codex desktop app (and its node_repl processes), then rerun test 1:
`codex exec -m gpt-6.1-sol -c model_reasoning_effort=low -C .coord/scratch/t1 --sandbox workspace-write --skip-git-repo-check - < prompt > .coord/logs/t1.log`
Pull the session id from the log header (`session id:`) for test 2 (`codex exec resume <id>`).
If it passes, record the dispatch rule in docs/agents.md. Then tests 2-6, then research Director.

## Blockers / waiting on user
- Close Codex desktop app so headless sandbox can start (or approve another route).
- Rust + MSVC install for spike B (approved, install after research).

## Test Director queued-message check
- 2026-10-17: Test Director active; waiting for queued messages. Will record received text in .coord/scratch/t1/queued.md.
- 2026-10-17: Queued message received (mango-42) and appended verbatim to .coord/scratch/t1/queued.md; queued-message test complete.
