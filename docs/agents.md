# Launching and messaging agents

| From -> To | How |
|---|---|
| Claude -> Claude | `start_session` (visible in Claude Desktop) / `SendMessage` to `local_...` id |
| Codex -> Codex | `codex exec resume <id>`, `codex queue --thread <id|name> --message "..."`, `codex fork` |
| Claude -> Codex | headless: `codex exec -m gpt-6.1-sol -c model_reasoning_effort=medium -C <dir> --sandbox workspace-write --output-last-message <report> - < task.md > .coord/logs/<task>.log`, run in background. Exit wakes the caller. |
| Codex -> Claude | files only: `.coord/reports/`, `STATUS.md`, `HANDOFF.md`; then exit |

Directors (Codex `gpt-6.1-sol` medium) spawn Luna (`gpt-6-luna` high) subagents. Sonnet 5.5 medium reviews as a visible session.

Limits: write a checkpoint to STATUS.md after every milestone. On a limit stop write HANDOFF.md (task id, session id, earliest resume time). Failover: same agent after reset (`exec resume`) -> other Codex model -> Sonnet session -> CEO if small. Never poll for limits.

**Second route (user-approved, per invocation only, never global):** add `-c 'windows.sandbox="unelevated"'` to `codex exec` (keep `--sandbox workspace-write`; never full access). Verified for file write + Luna subagent. Optional tool, app sessions stay primary.

**Current working route (2026-10-17):** `codex exec` fails here (sandbox setup error). Use Codex **app sessions** instead: the user starts a session in this project (gpt-6.1-sol medium) and names it; the manager sends work with `codex queue --thread <name> --message "..."`. Delivery happens between turns (user clicks Steer for mid-turn). Directors run all milestones in ONE turn, writing STATUS.md checkpoints as they go, and end the turn only when the report is written or they are blocked. They never wait inside a turn and must not end a turn mid-task (the manager is not polling; an ended turn means idle until queued). Point them at a task file (`.coord/tasks/T-###.md`) instead of long messages.

Verified recipes: see `docs/comms-tests.md`.
