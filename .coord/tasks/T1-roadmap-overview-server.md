# T1-roadmap-overview-server: step T1a, the data builder and the local server

Step brief of `T1-roadmap-overview` (read the card `.coord/tasks/T1-roadmap-overview.md` first, then the approved page
`.coord/tasks/T1-roadmap-overview.mockup.html` and its data `.coord/tasks/T1-roadmap-overview.sample.json`). Committed so
far: the card and those two references. This is the first build step; the page (T1b) and the switch-over (T1c) follow.

Tier: Low · docs-tools
Role: worker (gpt-6-luna xhigh)

## Why this step
The roadmap page today is rewritten by a Claude hook and shows only the STATUS Plan. The new page needs one JSON
document, rebuilt live from the repo, served by a small local server that runs without Claude.

## After
1. `tools/roadmap/eras.json`: `[{"name":"Era 0","until":"8d18f72"},{"name":"Era 1","from":"8d18f72"}]` (Era 1 is
   `8d18f72..dev`; a commit at or before `8d18f72` is Era 0). The data builder reads it; no hard-coded hashes elsewhere.
2. `tools/roadmap-data.ps1` (PowerShell 7, read-only):
   - `-Root <path>` (default: the **main checkout**, found from `git rev-parse --git-common-dir`, so it works when run
     from a worktree too). It reads the working copy there, never the committed file.
   - Builds one object with every top-level key of the sample, same names and shapes: `updated`, `rows`, `now`, `log`,
     `worktrees`, `branches` (incl. `ahead`, `behind`, `fork` vs dev), `tags`, `usage`, `main`, `devAhead`, `cards`,
     `where`, `devLine`, `graph` (`lanes` = local branches: main, dev, then the rest by latest commit, each with `head`,
     `date`, `worktree`; `commits` = commits on any local branch back to the Era 1 start, newest first, each with `in`:
     the lane indexes that contain it, and `tags`). Add `eras` (from the JSON) so the page needs no constants.
   - Sources: `.coord/STATUS.md` (`### Pending work` table rows with their bold group rows, `## Now` bullets,
     `## Where things are`), `.coord/tasks/*.md` cards (title line, `Branch:`, `Tier:`, `## Request`, `## Plan`,
     `## Check`, `## Hand test`), `.coord/logs/usage.tsv` (tab-separated: day label, time, role, model, task, exit, secs),
     and git commands that only read (`log`, `for-each-ref`, `rev-list`, `merge-base`, `worktree list --porcelain`,
     `tag`). A missing or unreadable source gives an empty value and one entry in a new `warnings` array, never a crash.
   - Default output: the JSON on stdout. `-Check`: prints `ROADMAP DATA OK` plus one line of counts (rows, groups, log,
     lanes, worktrees, tags, cards, warnings), or `ROADMAP DATA FAIL: <reason>` and exit 1 when a sample key is missing
     or the Plan row count differs from the STATUS table.
3. `tools/roadmap-serve.ps1` (PowerShell 7, first-party .NET only):
   - `System.Net.HttpListener` on `http://localhost:<Port>/` (`-Port`, default 47811), localhost only, GET only.
   - Routes: `/` serves `tools/roadmap/index.html` (in this step a one-line placeholder page is fine if the file does not
     exist yet), `/data` the JSON from `roadmap-data.ps1` logic (dot-source or a shared function; not a child process
     per request), `/health` `{"app":"antinotepad-roadmap","pid":<n>,"port":<n>}`. Everything else 404.
   - Cache: rebuild `/data` at most every 2 s and only when an input changed (STATUS, cards folder, usage.tsv write
     times, or the git refs/HEAD/worktrees state); send an `ETag` and answer `304` to a matching `If-None-Match`.
   - One instance: before binding, GET `/health`; if our app answers, start nothing (with `-Open`, just open the page) and
     exit 0. If binding fails because the port is taken and `/health` is not ours, print one line naming the port and
     exit 1. Two starts at once must end with exactly one server.
   - `-Open` opens the page in the default browser; `-Stop` asks the running server to stop (a local stop route or the
     pid from `/health`) and waits for it; `-Background` relaunches itself hidden and detached, then returns.
   - Exits on its own after 12 h with no request.
   - Writes nothing in the repo. A short log line per start/stop to `build/roadmap-serve.log` is allowed (`build/` is
     ignored).

## Files
- `tools/roadmap/eras.json`
- `tools/roadmap-data.ps1`
- `tools/roadmap-serve.ps1`

## Done means
- `pwsh -NoProfile -File tools/roadmap-data.ps1 -Check` prints `ROADMAP DATA OK` against the real repo; the counts are
  in the report next to what you counted by hand (Plan rows in STATUS, `git worktree list`, `git branch`).
- A server probe on a spare port (e.g. 47899), from a script you run and delete: start with `-Background`; `/health` and
  `/data` answer; a second and a third start leave one process; `/data` against `-Root` pointing to a throwaway clone
  under `build/T1-probe` (deleted after; never the real STATUS) changes within 3 s after editing that clone's STATUS;
  `ETag` + `304` works; `-Stop` stops it. List each result in the report.
- `pwsh -NoProfile -File tools/gate.ps1 -Task T1-roadmap-overview -Offline` passes.

## Constraints
Work in this worktree (`chore/T1-roadmap-overview`). Never switch branches, merge, commit or push; never edit
`.coord/STATUS.md`, the card, the references, or anything under `product/`. Never launch or kill AntiNotepad. No new
libraries, no network beyond localhost. Comments and formatting follow `docs/coding.md`.

Report: `.coord/reports/T1-roadmap-overview-server.md` with the item template's report sections.
