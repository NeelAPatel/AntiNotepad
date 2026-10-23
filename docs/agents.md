# Codex agents

## Launch
Headless, in the background; the process exit wakes the manager:

```bash
.coord/tools/codex-run.sh <director|worker|backup> <ID-slug> .coord/tasks/<ID-slug>.md
```

| Tier | Model / effort | Runs per day (local counter) |
|---|---|---|
| director | gpt-6.1-sol medium (delegates to gpt-6-luna xhigh subagents) | 20 |
| worker | gpt-6-luna high | 10 |
| backup | gpt-5.6-luna xhigh | 5 |

- The wrapper adds the only working sandbox route on this machine (`-c 'windows.sandbox="unelevated"'` with
  `--sandbox workspace-write`, never full access).
- Failover on a reached cap or a usage/rate-limit error: director -> worker -> backup -> stop. Other failures are not retried.
- The per-day caps are the wrapper's own safety counter (`.coord/logs/usage.tsv`), not the provider's usage limits. Override with
  `CODEX_MAX_DIRECTOR|WORKER|BACKUP`; `CODEX_TIMEOUT_MIN` (45) caps one run; `CODEX_DRY=1` prints the plan only.
- Output: log `.coord/logs/<ID-slug>.<role>.log`, last message `.coord/reports/<ID-slug>.<role>.codex.md`; the brief asks for a
  short report at `.coord/reports/<ID-slug>.md`.

## Briefs
- One file per task in `.coord/tasks/<ID-slug>.md`: role line, branch, the exact files in scope, the goal, and "Done means".
- Codex cannot write to `.git` in its sandbox: it never commits; the manager reviews and commits.
- Codex cannot see the screen: the manager does the visual check in the dev copy.
- Codex never launches or kills Antipad; it builds into `build/<task>-check`, runs `--selftest` there, then deletes that folder.
- Builds need `AVALONIA_TELEMETRY_OPTOUT=1` inside the sandbox.

## Reporting to the user
When launching, tell the user in one line: working folder, brief file, log and report paths.
