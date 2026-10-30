# T1-roadmap-overview: the roadmap becomes the user's central overview, served live from STATUS and git

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Low · docs-tools (rules: `docs/loop.md`); a visible design, so lab-first (`AGENTS.md`)
Branch: `chore/T1-roadmap-overview` in `.claude/worktrees/chore/T1-roadmap-overview` (created at Approve)

## Request
User, 2026-10-30 (verbatim), in the loop chat (Loop Hub 1, master for `Loop` and `T` items):
"I need Roadmap to become my central overview of the project. * Work log * Era dropdown * List of worktrees, branches, and
associated work items with each of them.the work log table should have columns/badges/tags for the worktrees and
branches, so i can search for any item in any column and find it. What else would be useful to have on Roadmap.html?"

Picked from the loop chat's list (question card): Now strip, Item drawer, Branch health flags, Releases, Codex run
counter, Hand-test queue. With them: "i dont want to waste tokens on updating the site everytime theres a new update to
Status.md. How can we make it so that the roadmap is a front end, but all the data is served purely from a couple .md
files where if they change, the website changes without intervention or AI needing to refresh the page? i.e we make the
UI and layout and structure of the html site, but data is served from .mds". Asked "local server" or "data file, no
server", the user chose: "Local server". Then: "Yes, start roadmap".

## Findings
- **Kind and why:** tooling feature. The roadmap shows only the STATUS Plan, as one long list; the user wants one place
  for the plan, the work done, and where every branch and worktree stands.
- **Now (master scout, 2026-10-30):**
  - `tools/roadmap.ps1` (129 lines) reads only `## Plan` of `.coord/STATUS.md` and writes the whole page,
    `.coord/roadmap.html`. It runs as a Claude hook on every Edit/Write and at each turn's end (`.claude/settings.json`,
    PostToolUse and Stop). Zero model tokens already; the cost is a rewrite per edit.
  - The page, opened from disk, reloads itself every 3 s (`location.reload`), so any search or filter state would be
    lost on each refresh; it keeps only the scroll position.
  - Git changes outside a Claude Edit/Write (a new worktree, a deleted branch, a push) never trigger the hook.
  - There is no era split, work log, branch list, search or item detail. STATUS has one `### Pending work` table with
    bold group rows; 70 rows today (37 needs you, 16 proposed, 13 done, 4 planned).
  - Data that exists for the new panels: `git log dev` (169 commits; Era 1 is `8d18f72..dev`), `git worktree list
    --porcelain`, branches with ahead/behind dev, tags (34 local, incl. old `antinotepad-*` names deleted on GitHub per
    STATUS), `.coord/logs/usage.tsv` (Codex runs per role and day), card `Branch:`/`Tier:` lines and sections in
    `.coord/tasks/*.md`, STATUS "Now" and "Where things are" (installed copy version).
  - Probe: `[System.Net.HttpListener]` with prefix `http://localhost:47811/` starts without admin rights in pwsh 7.6.9
    on this machine (first-party, no new library).
  - `/roadmap` (`.claude/skills/roadmap/SKILL.md`) runs `tools/roadmap.ps1 -Open`; `tools/roadmap.url` holds an
    Artifact copy published on request.
- **Risk:** low for the product (no `product/` change). The hook and `/roadmap` are used by every chat, so the switch
  must not leave a stale or broken roadmap; the old page stays until the server works.
- **Source:** master scout (this card).

## Design (lab-first, approved)
Mockup rounds 1-3 in the Artifact https://claude.ai/artifact/61incWqKacnMkRBjvn9uYs; the user's comments are on it. Round 3
approved by the user, 2026-10-30: "Yes, write the plan". The approved page is
`.coord/tasks/T1-roadmap-overview.mockup.html` (data injected at `/*DATA*/`) and its data shape is
`.coord/tasks/T1-roadmap-overview.sample.json` (a real snapshot). Both are references, not shipped files.
What the user asked for across the rounds, and the build must keep:
- Top bar: era select (Era 1 / Era 0 / All), theme button (System > Light > Dark, kept per browser), a narrow search
  (`/` focuses it; every word must match somewhere in the row), Legend button whose content follows the tab.
- Now strip: **Your turn** (only active items whose state is the user's move: approve, ask, "with you", hand test),
  **Moving without you** with "n of 3 past Approve", **Hand-test queue**, **Recent Codex runs** (last 6: time, task,
  role, duration). No run caps or meters anywhere (the user: only the token usage % limits Codex).
- Plan tab: columns ID | Name | Scope | Tier · size | State | Branch · worktree · era. ID only for confirmed items;
  `step:` rows indented under their idea; numbered groups in STATUS order; release markers as a bright orange
  "⚑ Release point" band; 3px row stripe by state; ID badge tinted by family (A, Loop, T, C, D). Filters in this order
  with counts: All, Active, Planned, ✦ Proposed, Ideas, Deferred, Done.
- Work log tab: what landed on dev, newest first, consecutive commits of one item folded into one expandable row.
- Branches & worktrees tab: a health table of every local branch and worktree (detached worktrees too), then a lane
  table (lanes | commit | label | message | date): one pinned line per local branch, main, dev, then the others by
  latest commit; dot = commit in branch, ring = head, dashed = branch exists but not at this commit. Local only.
- Releases tab: main, dev ahead of main, installed copy (STATUS "Where things are"), every tag with its kind.
- Drawer: phase strip, facts grid (tier, branch, commits, gate, hand test, Codex runs), card sections as one-line
  previews that open to the full text, commits, runs, files.
- Light and dark themes; no horizontal page scroll at 375 px.
Mockup-only parts that do not ship: the yellow mockup bar and the "busy day" example button.

## Plan
- **Now:** `tools/roadmap.ps1` rewrites `.coord/roadmap.html` (the Plan table only) on every Claude Edit/Write and turn
  end; the page reloads itself every 3 s and loses any state; git changes made outside Claude never show.
- **After** (three steps, one commit each on the branch, in order):
  | Step | After |
  |---|---|
  | T1a-roadmap-server | `tools/roadmap-data.ps1` builds the `/data` JSON in the shape of the sample file from the **main checkout's working copy** (`.coord/STATUS.md`, `.coord/tasks/*.md`, `.coord/logs/usage.tsv`) and read-only git (log of dev and every local branch back to the Era 1 start, worktree list, branches with ahead/behind/fork point vs dev, tags, lane membership), eras from `tools/roadmap/eras.json`. `tools/roadmap-serve.ps1`: .NET `HttpListener` on `http://localhost:47811/`, read-only, routes `/` (the page), `/data`, `/health`; JSON rebuilt at most every 2 s and only when an input changed; one instance per port; `-Open` opens the browser, `-Stop` stops it; exits after 12 h without a request. `roadmap-data.ps1 -Check` prints `ROADMAP DATA OK` with counts, or what is missing |
  | T1b-roadmap-page | `tools/roadmap/index.html`: the approved mockup minus the mockup-only parts, fetching `/data` every 3 s; redraws only when the data changed and keeps tab, era, filter, search, expanded rows, open drawer and scroll; shows "offline" in the live pill when the server is gone and recovers when it is back |
  | T1c-roadmap-switch | `/roadmap` starts the server when it is not running (hidden, detached) and opens the page; the two roadmap hooks leave `.claude/settings.json`; `tools/roadmap.ps1` is deleted; `tools/roadmap.url` stays, and `roadmap-data.ps1 -Snapshot <file>` writes a static page with the data injected for an Artifact copy on request; `tools/loop-card.ps1` (the status-card hook, every prompt) pings `/health` and starts the server hidden when it is down, so the first message after a restart brings it up (zero tokens); a new `README.md` gets a short Roadmap section with the CLI commands (`pwsh -NoProfile -File tools/roadmap-serve.ps1 -Open`, `-Stop`, and `http://localhost:47811`); `AGENTS.md`, `docs/loop.md` (plan table, Roadmap line, Files table, settings row) and the skill describe the server |
- **Must not change:** STATUS format or content (the page only reads it); `product/`; the status card text and its
  in-flight list (`tools/loop-card.ps1` only gains the server ping, which never delays or fails the prompt); the main checkout's files (the server writes nothing anywhere except the `-Snapshot` file).
- **Scope questions** (my recommendation in bold; say if you want another):
  1. Port: **fixed 47811** (probed free, no admin rights needed), with `-Port` to override.
  2. Server lifetime: **hidden background process started by `/roadmap`, stops after 12 h without a page request**, or
     a visible console window you close yourself.
  3. Phone copy: **keep a static snapshot Artifact on request** (`-Snapshot`), or drop `tools/roadmap.url`.
  4. Era list: **`tools/roadmap/eras.json`** (Era 0 = up to `8d18f72`, Era 1 = after), instead of a new STATUS section.
- **Test plan, code:** `roadmap-data.ps1 -Check` against the real repo (every sample key present; rows = Plan rows in
  STATUS; lanes = local branches; worktrees = `git worktree list`); a script probe that starts the server on a spare port,
  fetches `/health` and `/data`, edits a scratch copy of STATUS (via `-Root` on a temp clone, never the real file) and sees
  `/data` change within 3 s, then `-Stop`. Results in each step's report.
- **Test plan, UI (hand test, you):**
  1. `/roadmap` opens the page; the Now strip shows T1-roadmap-overview under Your turn.
  2. Type a search, pick a filter and open a drawer; edit a STATUS row in your editor; within ~3 s the row changes and
     your search, filter and drawer stay.
  3. Create a throwaway branch with `git branch tmp-x` and delete it: it appears in, then leaves, the Branches tab
     without any Claude action.
  4. Theme button cycles and is remembered after a reload; Legend changes with the tab; phone width (DevTools) has no
     sideways scroll.
  5. Stop the server (`tools/roadmap-serve.ps1 -Stop`): the pill says offline; `/roadmap` brings it back.
  6. Run `/roadmap` three times, also from two chats: still one server process (Task Manager shows one
     `roadmap-serve` pwsh); each run only opens a tab.
- **Repeat starts (user asked, 2026-10-30):** every start first calls `/health`; if our server answers, it starts
  nothing and only opens the page. Two starts at the same moment: only one can bind the port, the other sees it taken,
  finds our `/health` and backs off. If something else holds 47811, the start says so in one line and starts nothing.
- **Gates:** `tools/gate.ps1 -Task T1-roadmap-overview` on each step (scope against Files; loop-lint once T1c touches loop
  files); the `-Check` and server probe above; bounds check at Check; hand test (visible).
- **Size:** tooling only, no product code; about 900-1,100 lines in all (page ~550, data ~300, server ~150), so three
  step commits.
- **Coordination:** T1c changes hooks every chat uses, so before Merge this chat tells Master 2 in one line (role rule).
- **Needs from you:** Approve, plus answers to the four scope questions (or "defaults").

## Files
- `tools/roadmap-data.ps1`
- `tools/roadmap-serve.ps1`
- `tools/roadmap/index.html`
- `tools/roadmap/eras.json`
- `tools/roadmap.ps1`
- `tools/roadmap.url`
- `.claude/skills/roadmap/SKILL.md`
- `.claude/settings.json`
- `tools/loop-card.ps1`
- `README.md`
- `docs/wiki/Roadmap-Portal.md`
- `docs/wiki/_Sidebar.md`
- `docs/wiki/Getting-Started.md`
- `docs/wiki/Who-Does-What.md`
- `docs/wiki/Where-Files-Live.md`
- `AGENTS.md`
- `docs/loop.md`
- `docs/glossary.md`
- `tools/loop-lint.allow`
- `.coord/tasks/T1-roadmap-overview.mockup.html`
- `.coord/tasks/T1-roadmap-overview.sample.json`

## Contracts
None (no product code).

## Approve
User, 2026-10-30: "i approve this", then after the auto-start proposal and the repeat-start and CLI answers: "okay sounds
good, approved. add this instruction line to readme.md as well". So: approved with the four default scope answers (port
47811, hidden server that stops after 12 h idle, snapshot Artifact on request, `tools/roadmap/eras.json`), the auto-start
from the status-card hook, and a new `README.md` with the CLI commands (the repo had no README).

## For Codex
Per step: a step brief `.coord/tasks/T1-roadmap-overview-<step>.md` names the step's After, its files (a subset of Files
above) and its Done means. Read this whole card, the mockup and the sample JSON first; `docs/coding.md` applies to every
file. Role: worker (gpt-6-luna xhigh) builds each step. PowerShell 7, first-party .NET only (`HttpListener`,
`System.Text.Json` or `ConvertTo-Json`), no new libraries, no CDN in the page. Read-only toward the repo: git commands
that only read; never write STATUS, cards or anything outside Files. Never commit, never switch branches, never edit
STATUS or this card, never touch `product/`, never start or stop the installed AntiNotepad. Report
`.coord/reports/T1-roadmap-overview-<step>.md` in the item template's sections.

## Check
Builder sub-session (Sonnet) ran each Codex step and its checks; the loop chat re-checked every step before committing.
- **T1a-roadmap-server** (d9e2acb; worker round 1 + r2): gate PASS; `-Check` OK (rows 70, groups 8, log 169, lanes 5,
  worktrees 4, tags 34, cards 37, warnings 0); health 0.1 s, `/data` 1.5 s cold and 0.1 s cached; live probe on a
  throwaway clone: a STATUS edit showed in `/data` after 3.3 s; ETag 304; `-Background` returns in 3 s; three starts
  leave one process; `-Stop` works. Round 1's `-Background` hung; r2 replaced it.
- **T1b-roadmap-page** (f453e9e; one round): gate PASS; browser check by the loop chat: no console errors on any tab
  or era, empty and no-match search; a live STATUS edit kept tab, search text and focus, filter, scroll and the open
  drawer (drawer showed the new text); offline pill and recovery; Light/Dark/System; no sideways scroll at 375 px.
- **T1c-roadmap-switch** (1c489c7; one round): gate PASS; loop-lint PASS (38 files); `settings.json` parses, only the
  UserPromptSubmit hook remains; status card text unchanged, the hook takes ~0.4 s; auto-start skips silently until
  the server script exists in the main checkout (after Merge).
- **Bounds (requested, approved, done):**
  - Panels, design, server, auto-start, README, snapshot: within plan.
  - The server serves the page from its own folder (T1b): justified, so a worktree can serve its own page while reading
    the main checkout's data; no behavior change for the user.
  - `.gitignore` still names `.coord/roadmap.html` (the old page stays ignored on disk): justified, harmless.
- **Noticed (proposed follow-ups, not fixed):** `docs/wiki/Who-Does-What.md` (lines 184, 203) still describes
  `tools/roadmap.ps1`, so it goes with the next wiki republish; `tools/approve.ps1` moves only `.md` planning files
  (the mockup and sample were moved by hand); `docs/agents.md` and `.coord/tools/codex-run.sh` still treat per-role run
  counts as caps with failover, though the only real Codex limit is the usage %.
- **Misses:** T1a's builder skipped the live refresh check from Done means; the loop chat ran it. Cause `judgment`.
- **Verdict:** accept; hand test next.

## Hand test
Round 1, 2026-10-30, from the branch (`tools/roadmap-serve.ps1 -Background -Open` in the worktree, real STATUS and git),
checks 1-5 of the Plan. The user, with a screenshot of the Branches tab after `git branch tmp-x`: "While theres no
commits on the tmp-x branch, i expect it to show that a new branch has been made FROM whatever branch it makes from, and
same with merges. Everthing else passes". So checks 1, 2, 4, 5 pass; check 3 fails on how a fork (and a merge) is
drawn. Rework round `T1-roadmap-overview-r2` (cause `plan`: the approved lane design drew shared history as dots on
every lane and never drew where a branch starts or joins). Checks 6 and the auto-start run after Merge.

Round 2, 2026-10-30, after r3 (the user's Codex run was stopped at 17:18 and the loop chat finished r3; see
`.coord/reports/T1-roadmap-overview-r3.md`). The user repeated check 3 on 47811: "okay it works". All of checks 1-5 pass.
The user then asked for the wiki page `docs/wiki/Roadmap-Portal.md` (launch, shut down, manage), linked from
`README.md`, and to merge to dev. Check 6 and the auto-start run after Merge.
