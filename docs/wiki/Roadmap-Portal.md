**In short**

The roadmap portal is a local page at http://localhost:47811. It shows the plan, the work log, branches and worktrees,
releases and recent Codex runs, and it updates live from STATUS, item cards, the Codex usage log and read-only Git. It
costs no AI tokens. A hidden PowerShell server serves it; the server starts on its own and stops after 12 hours unused
(`tools/roadmap-serve.ps1`; `tools/loop-card.ps1`).

# Roadmap portal

## Launch

Run every command from the main checkout folder.

| Goal | Command |
|---|---|
| Start the server and open the page | `pwsh -NoProfile -File tools/roadmap-serve.ps1 -Open` |
| Start it hidden, with no console window | `pwsh -NoProfile -File tools/roadmap-serve.ps1 -Background` |
| Start hidden and open the page | `pwsh -NoProfile -File tools/roadmap-serve.ps1 -Background -Open` |
| From a Claude chat | `/roadmap` (runs the line above) |

You rarely need to start it yourself. The status-card hook (`tools/loop-card.ps1`, on every Claude prompt) checks
`/health` and starts the server hidden when it is down. Running a start command again while the server is up only opens
the page: one server per port, never two.

Without `-Background`, the server runs in the current console until you stop it or close that window.

## Shut down

| Goal | Command |
|---|---|
| Stop the server | `pwsh -NoProfile -File tools/roadmap-serve.ps1 -Stop` |
| Stop a server on another port | `pwsh -NoProfile -File tools/roadmap-serve.ps1 -Stop -Port <port>` |

The server also stops by itself after 12 hours with no requests. A page left open keeps it alive, since the page asks for
fresh data every 3 seconds. When the server is down, the page shows an offline marker, keeps the last data, and
recovers on its own when the server comes back.

## Check and manage

| Goal | Command or address |
|---|---|
| Is it running? | http://localhost:47811/health (returns app name, process ID and port) |
| Check that the data reads cleanly | `pwsh -NoProfile -File tools/roadmap-data.ps1 -Check` (prints `ROADMAP DATA OK` and counts) |
| Raw data the page uses | http://localhost:47811/data |
| One-file copy for another device | `pwsh -NoProfile -File tools/roadmap-data.ps1 -Snapshot <file.html>` (opens offline, no server) |
| Serve on another port | `-Port <port>` on any start or stop command |
| Serve another clone | `-Root <folder>` (for tests; the default is the main checkout) |

- **What it reads:** `.coord/STATUS.md`, `.coord/tasks/*.md`, `.coord/logs/usage.tsv` and Git (branches, worktrees, tags,
  log). It reads the main checkout's working copy, even when started from a worktree. It never writes to the repository.
- **Eras:** `tools/roadmap/eras.json` names the history eras and the commit each starts from; the page's era picker
  reads it. Add an entry when a new era begins.
- **Page:** `tools/roadmap/index.html` is the fixed page; its look changes only through a loop item (design is lab-first).
- **Only local:** the server answers `localhost` requests only.

## Fixes

| Symptom | Fix |
|---|---|
| Page shows offline | Run a start command, or send any Claude prompt (the hook starts it). |
| Page shows old code after an update | Stop, then start again: the server loads its scripts once. |
| "could not bind port" | Another program holds 47811. Stop it, or use `-Port`. |
| A count looks wrong | Fix STATUS or the card; the page only shows what the files say. Run `-Check` for warnings. |

## Sources

- `tools/roadmap-serve.ps1` — start, stop, single instance, 12-hour idle stop, routes `/`, `/data`, `/health`.
- `tools/roadmap-data.ps1` — data sources, `-Check`, `-Snapshot`.
- `tools/loop-card.ps1` — auto-start on each prompt.
- `.claude/skills/roadmap/SKILL.md` — `/roadmap`.
- `.coord/tasks/T1-roadmap-overview.md` — design, plan and hand test.
