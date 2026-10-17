# Launching and messaging agents

| From -> To | How |
|---|---|
| Claude -> Claude | `start_session` (visible in Claude Desktop) / `SendMessage` to `local_...` id |
| Codex -> Codex | `codex exec resume <id>`, `codex queue --thread <id|name> --message "..."`, `codex fork` |
| Claude -> Codex | headless: `codex exec -m gpt-6.1-sol -c model_reasoning_effort=medium -C <dir> --sandbox workspace-write --output-last-message <report> - < task.md > .coord/logs/<task>.log`, run in background. Exit wakes the caller. |
| Codex -> Claude | files only: `.coord/reports/`, `STATUS.md`, `HANDOFF.md`; then exit |

Directors (Codex `gpt-6.1-sol` medium) spawn Luna (`gpt-6-luna` high) subagents. Sonnet 5.5 medium reviews as a visible session.

Limits: write a checkpoint to STATUS.md after every milestone. On a limit stop write HANDOFF.md (task id, session id, earliest resume time). Failover: same agent after reset (`exec resume`) -> other Codex model -> Sonnet session -> CEO if small. Never poll for limits.

Verified recipes: see `docs/comms-tests.md` (fill in the exact working commands).
