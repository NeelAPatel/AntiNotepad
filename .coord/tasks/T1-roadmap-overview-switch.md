# T1-roadmap-overview-switch: step T1c, switch the roadmap to the live server

Step brief of `T1-roadmap-overview` (read the card `.coord/tasks/T1-roadmap-overview.md` first: Plan row T1c, "Repeat
starts", Approve). Committed so far: T1a (data builder and server, d9e2acb) and T1b (the page, f453e9e), both checked by
the loop chat (live refresh, state kept across a data change, offline recovery, themes, 375 px).

Tier: Low · docs-tools
Role: worker (gpt-6-luna xhigh)

## Why this step
The new server and page work, but every chat still uses the old roadmap: two Claude hooks run `tools/roadmap.ps1` on
every Edit/Write and turn end, `/roadmap` opens `.coord/roadmap.html`, and the docs describe that page.

## After
1. `tools/loop-card.ps1` (UserPromptSubmit hook, every prompt): after its existing output, it makes sure the roadmap
   server is up. It already resolves the main checkout as `$Root`; use `$Root/tools/roadmap-serve.ps1` (skip silently
   when that file does not exist). GET `http://localhost:47811/health` with a short timeout (about 300 ms); if our app
   does not answer, start the server with `Start-Process` hidden and detached **without waiting** (not `-Background`,
   which waits for health). Any failure is swallowed: the hook's output, exit code and speed for the status card stay as
   they are (well under 1 s added in the worst case). The server's own single-instance check handles races.
2. `.claude/settings.json`: remove the PostToolUse `Edit|Write|MultiEdit` and Stop entries that run `tools/roadmap.ps1`.
   Every other setting and hook stays byte-for-byte; the JSON stays valid.
3. Delete `tools/roadmap.ps1`.
4. `tools/roadmap-data.ps1`: add `-Snapshot <file>`: writes `tools/roadmap/index.html` with the current data inlined so
   the page works opened from disk or as an Artifact (no fetch; the live pill says "snapshot <updated>"). Prefer a small
   hook in `index.html` (for example a `window.ROADMAP_DATA` script block the page uses instead of fetching when present)
   over a second copy of the page. Writes only the given file.
5. `.claude/skills/roadmap/SKILL.md`: `/roadmap` runs `pwsh -NoProfile -File tools/roadmap-serve.ps1 -Background -Open`
   from the main checkout (it starts the server or reuses the running one, then opens `http://localhost:47811`). Keep
   rules 2 and 3 (STATUS is the source of truth; at most three lines in chat). The phone copy: on the user's request,
   `tools/roadmap-data.ps1 -Snapshot <scratch file>` and publish that file.
6. `README.md` (new; the repo has none): a short top section naming the app in one sentence (from `AGENTS.md`), then
   "Roadmap (project overview)": what it is in one line, the commands as code blocks
   (`pwsh -NoProfile -File tools/roadmap-serve.ps1 -Open`, `pwsh -NoProfile -File tools/roadmap-serve.ps1 -Stop`), the
   URL `http://localhost:47811`, and one line that the server starts on its own with the first Claude prompt and stops
   after 12 h unused. Nothing else in the README.
7. Docs, only the roadmap lines: `AGENTS.md` "Read when" line (the plan: `/roadmap`, local server page
   `http://localhost:47811`, live from STATUS and git); `docs/loop.md` lines on the plan table (around line 162), the
   "Roadmap:" session bullet (around 215), the Files table row for the roadmap scripts and the `.claude/settings.json`
   row (hooks: status card plus roadmap auto-start; no roadmap rebuild); `docs/glossary.md` only if it defines the
   roadmap. `tools/loop-lint.allow` only if loop-lint flags a new line.

## Files
- `tools/loop-card.ps1`
- `.claude/settings.json`
- `tools/roadmap.ps1`
- `tools/roadmap-data.ps1`
- `tools/roadmap/index.html`
- `.claude/skills/roadmap/SKILL.md`
- `README.md`
- `AGENTS.md`
- `docs/loop.md`
- `docs/glossary.md`
- `tools/loop-lint.allow`

## Done means
- `echo {} | pwsh -NoProfile -File tools/loop-card.ps1` prints the same status-card text as before, with the server
  stopped and running; time both (report the numbers); with it stopped, the server is up a few seconds later; run it
  three times quickly: still one server process. Stop the server after.
- `tools/roadmap-data.ps1 -Snapshot build/T1-snapshot.html` writes a file that contains the data and no `fetch` path
  is taken (read the code); delete the file after.
- `.claude/settings.json` parses (`ConvertFrom-Json`) and no longer mentions `roadmap.ps1`; `git grep roadmap.ps1`
  finds only history files (cards, reports, archives).
- `pwsh -NoProfile -File tools/loop-lint.ps1` and `pwsh -NoProfile -File tools/gate.ps1 -Task T1-roadmap-overview -Offline`
  pass.

## Constraints
Work in this worktree (`chore/T1-roadmap-overview`). Never switch branches, merge, commit or push; never edit
`.coord/STATUS.md`, the card, the references, or anything under `product/`. Never launch or kill AntiNotepad. Stop any
server you start (`-Stop`; port 47811 is the real one, so use `-Port 47891` for anything except the loop-card test).
No new libraries. Comments and formatting follow `docs/coding.md`; keep doc edits short and in the existing style.

Report: `.coord/reports/T1-roadmap-overview-switch.md` with the item template's report sections.
