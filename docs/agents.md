# Launching and messaging agents

| From -> To | How |
|---|---|
| Claude -> Claude | `start_session` (visible in Claude Desktop) / `SendMessage` to `local_...` id |
| Codex -> Codex | `codex exec resume <id>`, `codex queue --thread <id|name> --message "..."`, `codex fork` |
| Claude -> Codex | headless via `.coord/tools/codex-run.sh` (see Routing below), run in background. Exit wakes the caller. |
| Codex -> Claude | files only: `.coord/reports/`, `STATUS.md`, `HANDOFF.md`; then exit |

Directors (Codex `gpt-6.1-sol` medium) spawn Luna (`gpt-6-luna` high) subagents. Sonnet 5.5 medium reviews as a visible session.

Limits: write a checkpoint to STATUS.md after every milestone. On a limit stop write HANDOFF.md (task id, session id, earliest resume time). Failover: same agent after reset (`exec resume`) -> other Codex model -> Sonnet session -> CEO if small. Never poll for limits.

**Routing (verified 2026-10-21): use `.coord/tools/codex-run.sh`.** `codex exec` works only with `-c 'windows.sandbox="unelevated"'` plus `--sandbox workspace-write` (never full access); the default route fails with "setup refresh had errors". The wrapper adds that flag and does the rest:

```bash
.coord/tools/codex-run.sh <director|worker|backup> <ID-slug> <task-file> [extra codex args]
```

| Tier | Model / effort | Default cap per budget tag |
|---|---|---|
| director | gpt-6.1-sol medium | 3 |
| worker | gpt-6-luna high | 2 |
| backup | gpt-5.6-luna xhigh | 1 |

- Failover chain on cap reached or a usage/rate-limit error: director -> worker -> backup -> STOP (write HANDOFF.md; never loop). Any other failure is not retried; revert or fix by hand.
- Budget tag defaults to today's date (`CODEX_BUDGET_TAG`); caps via `CODEX_MAX_DIRECTOR|WORKER|BACKUP`; per-run wall limit `CODEX_TIMEOUT_MIN` (45); `CODEX_WORKDIR` (repo root); `CODEX_DRY=1` prints the plan only.
- Every run appends to `.coord/logs/usage.tsv` (tag, time, role, model, task, exit, seconds). Log: `.coord/logs/<ID-slug>.<role>.log`. Report: `.coord/reports/<ID-slug>.<role>.codex.md`. Run it in the background; the exit wakes the caller.
- Verified 2026-10-21: all three models reachable; cap, failover and STOP exercised on a trivial task.

Codex app sessions (`codex queue --thread`) remain an option when the user starts one.

Verified recipes: see `docs/comms-tests.md`.

**Rebuild routine:** before any Release build in `src/AntiQuickPad`, run `dotnet bin/Release/net10.0/AntiQuickPad.dll --quit` (saves and exits a running pad; exit 0 if none). Never kill the process.
