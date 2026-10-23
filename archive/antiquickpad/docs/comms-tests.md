# Comms tests (results)

| # | Test | Result | Working recipe / note |
|---|---|---|---|
| 1 | Claude -> `codex exec` headless, sandboxed | FAIL (this machine, 2026-10-17) | Sandbox setup refresh error on `node_repl.exe` (os error 32), also from the user's own terminal. Real Windows Restart not yet tried (boot time was still Oct 15). Not using full access. |
| 1b | `codex exec` with per-run `-c 'windows.sandbox="unelevated"'` (user-approved per invocation only, still `--sandbox workspace-write`) | PASS (2026-10-17) | Sol wrote `ping.md` and spawned a `gpt-6-luna` subagent that wrote `sub.md`; exit 0. Workaround, root cause (elevated sandbox provisioning) not fixed. Note: exec picked up repo AGENTS.md when run under the project. |
| 2 | `codex exec resume <id>` | untested | Should work with the 1b flag; not yet run. |
| 3 | `codex queue` into a running app session | PASS | `codex queue --thread <name> --message "..."`. Delivers **between turns**; to deliver mid-turn the user clicks **Steer** in the app. Thread name comes from `~/.codex/session_index.jsonl` (rename session in the app, e.g. `director-test`). |
| 3b | Codex app session can write project files under workspace sandbox | PASS | Director wrote `.coord/STATUS.md` and `queued.md` from inside the app. |
| 4 | Sol Director spawns Luna subagents | not yet | Folded into research task T-001. |
| 5 | Claude spawns visible Sonnet reviewer | not yet | |
| 6 | Cold-start resume from HANDOFF.md + STATUS.md | not yet | |

Lessons: Directors must finish their turn (never "wait" inside a turn). The Codex app is part of the ChatGPT desktop app; its processes run `node_repl`, which is what breaks `codex exec` setup while the app is open.
