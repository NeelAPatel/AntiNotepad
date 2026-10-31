# Dev loop (per item)

Who does what, what triggers each phase, and what it leaves behind. Model routing and the Codex wrapper: `docs/agents.md`.
Terms used here: `docs/glossary.md`. How code is written: `docs/coding.md`.
Every item has one **item card**, `.coord/tasks/<ID-slug>.md` (from `.coord/templates/item.md`), that grows phase by
phase: the request, what was found, the approved plan, and the check of what was done against both. Anyone can open the
card and see why the item exists, what was agreed and where it stands.

## Phases
`Request → Findings → Plan → Approve → Build → Check → Hand test → Merge`

| # | Phase | Who | Leaves on the card | STATUS state |
|---|---|---|---|---|
| 1 | **Request**: the user's words, verbatim, and what they want changed | master | `## Request` | `request` |
| 2 | **Findings**: why the item exists (bug, feature, polish, process), where the change lands, what it does **now** with evidence; bugs need a reproduction | `auditor` sub-session running a Codex audit (Medium, High); the master's own short scout (Low) | `## Findings`, audit report | `findings` |
| 3 | **Plan**: **after** state against "now", scope questions, test plan (code and UI), the gates this item runs and what each catches, what is needed from the user, Files, tier | master | `## Plan` | `plan ready` |
| 4 | **Approve**: the user approves, changes or drops the plan; on approval the branch and worktree are created | user | `## Approve` with the user's words | `approved` / `dropped` |
| 5 | **Build**: Codex implements; the director spawns `lens_reviewer` before reporting | `builder` sub-session runs Codex | code on `<prefix>/<ID-slug>`, report | `building` / `rework rN` |
| 6 | **Check**: the planned gates, then the bounds check (request vs plan vs done) | `reviewer` (`/review-task`; usually the builder session renamed) | `## Check` | `checking` / `accepted` / `ask-user` |
| 7 | **Hand test**: the sub-session opens its build; the user runs the plan's UI checks | user, with the `tester` | `## Hand test` with the user's words | `hand test` / `hand-tested` |
| 8 | **Merge**: squash onto dev with the item's STATUS edits, gate on dev, push, delete the branch, remove the worktree | master (`/merge-task`) | the item's commit on dev | `done` |

"Approve" was "Go" before loop v3; old cards keep `## Go`.

### 2. Findings: what they must answer
- **Kind and why**: bug, feature, polish or process; what about today's behavior justifies the item.
- **Now**: what the app does today, with evidence: a code path with file:line, a selftest or probe result, or the user's
  description or screenshot. For anything visible, say what dev does today in plain words; if that already matches what
  the user asked for, stop and ask (A53-split-divider-height lost three commits to this).
- **Bugs: reproduce first.** A reproduction (probe, failing selftest, exact code path, or the user's steps confirmed in
  the dev copy) is required before building. No reproduction: stop and ask the user; never build a guess (A54 r1 could not
  reproduce and claimed no fix).
- **Where**: the files and callers the change reaches, shared state, the riskiest part.
- **Size**: a file the change touches that is over 600 lines gets the "one job?" check from `docs/coding.md`; a split it
  calls for is proposed as its own item, not done in passing.
- **Tier**: findings confirm or raise the tier. A bug whose cause is not proven by the findings is at least Medium.
- Low items get the master's own scout (a few `rg` and reads, at most about ten tool calls). Anything that needs more
  becomes a Codex audit, and the item is at least Medium.

**Backlog scans:** `/scan-backlog [slug ...]` runs one read-only Haiku scout per idea in a shared detached dev snapshot
and proposes an order for the user. Reports stay untracked in `.coord/research/<slug>.md`, stamped with the full scanned
SHA, until Approve moves and commits them with the card. At task time Findings reads the scan plus `tools/scan-delta.ps1`:
commits, changed Where files and their neighbors, and the related cards. More than half the Where files changed, or any
Where file deleted, requires a fresh scout or audit; Medium/High audits still run with the scan and delta as inputs.

### 3. Plan: what the user approves
- **After**: what it will do, written next to "Now" so the difference is the change.
- **Expected time**: request to merge, from similar runs in `.coord/logs/usage.tsv` and past cards. Over an hour is said
  plainly, with a smaller cut offered, even when the user wants the full scope (T1-roadmap-overview: expected 30
  minutes, took 4 h 45 min, never called).
- **Scope questions**: anything the request leaves open, asked now (A54 found "all URLs or only shortened ones?" only at
  the hand test).
- **Test plan**: selftest or `CONTRACTS` rows in code, and the exact UI checks for the hand test.
- **Gates**: which checks run and what each one catches for this item (menu below). The tier sets the defaults; the plan
  may add gates, never drop the tier's defaults.
- **Size**: more than about 400 lines of product code (generated files, tests and docs not counted) means lettered steps
  (`A1a`, `A1b`), each its own loop run and commit.
- **Needs from you**: decisions, screenshots, a live check, or "Nothing".
- **Files**: exact paths (the gate reads `## Files`).
The master asks one question: approve, change, or drop. Nothing is built, and no branch exists, before approval.

### Gates menu
| Gate | Catches | Default for |
|---|---|---|
| `tools/gate.ps1` (build 0 warnings, selftest, LAYERS, CONTRACTS, seam, format, scope, loop-lint, size) | compile errors, broken promises, layering leaks, new format problems on changed lines (rules: `.editorconfig`), files outside the plan (FAIL on item branches; WARN on dev, CI or `-LooseScope`), inconsistent loop files, god-file growth | every tier |
| Codex fresh review (`lens-review`) | logic, behavioral coupling, shared state and latches, timing and caches, missed inputs, cleanup | Medium, High |
| Claude `design-reviewer` | responsibility, boundaries, coupling between features, future cost | High |
| New `CONTRACTS` row | the fixed bug or new promise coming back unseen | every bug fix |
| Reproduction test | the bug is real and is the one fixed | every bug |
| Lab build | a design change the user has not approved yet | design changes (`AGENTS.md`) |
| Hand test | what only the screen shows | anything visible; always for High |

### 6. Check: plan vs done (bounds check)
After the gates, the reviewer compares three things and writes `## Check` on the card:
- **Requested** (`## Request`), **approved** (`## Plan`, `## Approve`), **done** (the report's Change list, Behavior
  changes and Out-of-scope files, and `git diff --stat dev...HEAD`).
- Every difference gets one line: **within plan**, **justified** (why, and why the user does not need to decide), or
  **ask the user** (it changes behavior or scope the user did not approve). Ask-the-user items stop the item until answered.
- Comments and formatting in the changed files follow `docs/coding.md` (on touch); a gap is a rework line, not a pass.
- Side bugs noticed on the way go under "Noticed" as proposed follow-ups, never fixed in passing.
- A scope change from the user mid-item (A54 r3) is recorded on the card with the user's words and goes through Plan again
  for that round.

### Triage, lessons, scores
Strictness is cheap by design: scripts first (zero model tokens), model judgment only on exceptions.
- **Triage on every failure** (gate fail, review rework, hand-test fail), done by the Check session that already has the
  context, no extra agent: one cause, `code`, `test`, `plan`, `env`, `conflict` or `judgment`, routed by the table in
  `/review-task` step 4a. Only `code` and `test` go back to Codex; `plan` goes to the master, `judgment` to the user.
- **Lessons:** each failure leaves one "Misses" line on the card; the master copies them into `.coord/lessons.md` at Merge
  with one Scores row per item (what each check found). Master-owned on dev, like STATUS.
- **Self-improvement, user-approved:** when the same cause and "should have caught" pair appears twice without a rule
  change, the master proposes an unnumbered `idea:` row naming the rule or bot file to change. Rules change only through
  an item the user approves.
- **Reviewer scores:** the Scores table shows whether each review earns its run. Empty reviews on items that later had
  misses mean that review's checklist needs a change (an `idea:` row), not more reviewers.
- **Cost:** a green item adds no model calls (format and the ledger are scripts); a failure adds a few lines in a
  session already open; the retro is one row written at Merge.

## Tiers
Set at Plan; findings may raise it. When unsure, pick the higher tier.
| Tier | Touches | Findings | Default gates |
|---|---|---|---|
| Low | UI text, theme tokens, docs/tools, Calc rules with selftests, pure moves with LAYERS rows | master scout | gate; hand test if visible |
| Medium | Editor features (input, cursor, selection, rendering), Settings sections, a new setting or shared state, any bug without a proven cause | Codex audit | gate + Codex review; hand test if visible |
| High | `Store`/`NotesSession`/notes folder, startup and quit, `Platform/` window styles and taskbar, timers and deferred work, save paths, memory | Codex audit | gate + Codex review + design review; hand test always |

## Status card (every reply about an item)
Every Claude reply about project work, master and sub-sessions alike, opens with this card. Keep it to these lines;
detail goes below it.
```
**>> <ID-slug>** · <Tier> tier · ● <phase>
✓ Request | ✓ Findings | ● Plan | ○ Approve | ○ Build | ○ Check | ○ Hand test | ○ Merge
**Tier:** <Tier> · <reason slug>
**Goal:** <one line>
**Now:** <what is happening, one or two lines>

---

**Waiting on you:** <the one decision or check, or "nothing">; anything else below
```
- `✓` done, `●` current, `○` not reached, `–` skipped by tier (Findings is never skipped). Rework shows as
  `● Build (r2)`.
- **Sub-sessions add their role** to the first line: `**>> A1-list-commands** · Medium tier · ● Build · builder`. The
  master may leave it out.
- The tier reason is a slug from the Tiers table's "Touches" column: Low `ui-text`, `theme-tokens`, `docs-tools`,
  `calc-rules`, `pure-move`; Medium `editor-feature`, `settings-section`, `new-setting`, `shared-state`,
  `bug-unproven-cause`; High `store-notes`, `startup-quit`, `platform-window`, `timers-deferred`, `save-paths`, `memory`.
- Markdown: one line per field (each renders on its own line), and a blank line before `---` (without it the line above
  turns into a heading).
- Several items in flight: one card each, the one the reply is about first.
- **Quiet relays:** a sub-session note that changes nothing ("still running", a launcher exit already reported) gets at
  most one plain line from the master, with no card and no tool calls. A card is for a phase change or a question.
- A `UserPromptSubmit` hook (`tools/loop-card.ps1`) reminds every turn and lists the items in flight from STATUS.

## Widgets (Claude app features)
Priority (user, 2026-10-29): good development and a good understanding of the codebase first, then saving usage. Wasted
usage is back-and-forth to get things right, so use whatever cuts that:
- **Question cards** (`AskUserQuestion`) for decisions with clear options: scope questions, Approve, rework or accept.
- **Next-step box** (`wrap_up`) at the end of every reply that did work: a short recap and one question with buttons.
- **Task list** (`update_plan`) for the steps of the current phase.
- **Inline visuals** where a picture helps: layouts, flows, lab-first designs, Findings that are hard to say in words.
- The Markdown **status card** above stays text: it costs about the same as plain lines.

## Rules
- A failed gate, a `rework` verdict or a user scope change goes back to Build as `<ID-slug>-r2`, `-r3` (STATUS `rework rN`),
  with the round's brief listing what changed and why, written by the item's own sub-session (it saw the failure).
  Counting: the first build is round 1 (no suffix), so two rework rounds (`-r2`, `-r3`) run without asking; a fourth
  round (`-r4`) needs the user's go. A round's gate and review run under the round's name (`-Task <ID-slug>-rN`): the gate allows the Files of the base card and every round's brief, and the
  reviews read that round's report plus the base report.
- Gate reports (`.coord/reports/*.gate.md`) are regenerated on every run and not committed; the result lines go into the
  card's `## Check`.
- Every bug fixed adds a `CONTRACTS` row, so it cannot come back unseen.
- Every Codex run happens in its own linked, visible sub-session that reports back to the master, named and modeled as in
  "Sessions" below. The master never launches Codex directly.
- Sub-sessions own the card's Build, Check and Hand test sections; the master owns Request, Findings, Plan, Approve and
  STATUS. A sub-session that needs a user decision asks it with the card, and records the answer verbatim.
- Release markers in the STATUS plan: the master asks the user before moving `main`. Only on the user's go.
- Design changes stay lab-first (`AGENTS.md`): Build to Hand test run on a lab build, and the user approves before the real change.
- **Side tasks take the fast lane** (user, 2026-10-31: side tasks such as roadmap work may skip the pipeline "as its not
  a critical piece of the project"). `T` items and tooling that does not ship in the product skip whatever does not help:
  the card can be a few lines or none, no audit, no Codex when the Claude session can do it in minutes, and only the
  checks the change touches. A master still lands it as one commit on dev and gives the user a short change list.
- `Loop` and `D` items keep a short card and a branch (they change how everyone works); Findings can be one paragraph.
- **Codex is paused** (user, 2026-10-31: "For the foreseeable time codex is not allowed"): until the user lifts it, the builder sub-session implements
  itself and says so in its report; the Codex rules below wait for its return.
- **Who builds:** every item, `Loop`, `D` and `T` included, is built by Codex in a builder sub-session, with the role
  picked by the job (`docs/agents.md`); a docs-only change of a few lines may be made by the builder session itself and
  says so in the report. Codex "Done means" lists offline checks only; live servers, browser and screen checks belong to
  the Claude session (`docs/agents.md`, "What Codex can and cannot check").
- **Progress checks need evidence:** "is it done?" is answered from two samples about 30 seconds apart, never from one
  timestamp (`docs/agents.md`).
- **Rule overrides:** when the user asks for something a rule forbids, the chat names the rule once; if the user still
  says go, it proceeds and logs the override in the item's `.coord/lessons.md` row, so repeated overrides show up as a
  rule worth changing.
- **Proposals go into STATUS at once:** every item or idea a chat names (a follow-up, a "Noticed" side bug, a backlog
  idea) becomes an unnumbered `idea:` or `proposed` plan row when it is named, not later at Merge.
- **Merging:** only a master merges to dev or moves `main`. A sub-session suggests the merge to its master (or the user
  tells the master directly) and merges only into branches it created itself, such as catching up from dev.
- **Loop freeze** (user, 2026-10-31): lifted for `Loop4-retro-fixes`; after it, loop rules change only when
  `.coord/lessons.md` shows the same miss twice (the existing self-improvement rule) or the user asks.
- The plan table: the local server page `http://localhost:47811` (`/roadmap` opens it), live from STATUS and Git. The
  `UserPromptSubmit` hook starts the server when needed; the Artifact (`tools/roadmap.url`) is published only on request.
  `/next-task` and `/merge-task` keep State current.

## IDs and numbering (user, 2026-10-29)
| Prefix | Covers | Example |
|---|---|---|
| `A` | App: product code and behavior, refactors and file splits included | `A1-list-commands` |
| `Loop` | The dev loop and agent processes | `Loop1-process-v3` |
| `D` | Documentation: code comments, wiki, `.md` file management | `D1-ui-region-names` |
| `T` | Tooling and process that is not AI | `T1-<slug>` |
- An item that fits more than one prefix takes the prefix of the area it affects most.
- **Giving a number brings an item into existence:** only the user's confirmation of an idea and its scope assigns one.
  Until then it is an unnumbered plan row `idea: <slug>` with `step: <slug>` rows under it (or a Backlog line).
- One ID = one idea. Its steps are lettered in run order, `A1a-<slug>`, `A1b-<slug>`, each one loop run with its own card,
  branch and commit, lettered when confirmed. A small idea is a single run with no steps; more than about 8 steps means
  split the idea. Review follow-ups become steps of the idea they serve. Rework rounds add `-r<N>`.
- **Eras:** counters can be reset and reprioritized at any time; each reset ends an era, and an ID only means one item
  within its era. Era one (2026-10-17 to 2026-10-29) used `A`, `C`, `N`, `P` and `S<n>-status`, all retired; old cards
  and reports keep their old meaning. Within an era a number is never reused, even if dropped.
- Always write `<ID>-<slug>` plus its scope, never a bare ID or a range.

## Sessions: master, sub-sessions, names (user, 2026-10-29)
**Loop version: v3.2** (2026-10-31, `Loop4-retro-fixes`: Codex roles by job and no run caps, offline-only Codex checks,
expected time in the plan, a side-task fast lane, masters per area with sub-sessions per phase, master notes after
compaction. v3.1 was 2026-10-30, `Loop2-loop-v3-1`: approve and merge preflights, catch-up with dev, strict scope,
read-only audits, backlog scans with a delta at task time, loop-lint; the dormant v2.2 drop absorbed). v3 was
`Loop1-process-v3`: Approve phase, A/Loop/D/T IDs and eras, type-prefixed branches and nested worktrees, squash merges
with STATUS inside, roles, widgets, `docs/coding.md`. v2 was C26-item-card-loop, v2.1
C28-loop-cheap-checks; v1 was C13-dev-loop-kit through C25. v2.2 was never activated; v3.1 absorbed it (its notes were deleted on 2026-10-31; git history keeps them).
A merged item that changes the phases, the roles or who runs which phase bumps the major version (new master at the next
handover); new checks, scripts, routes or research steps bump the minor version (v3.1) and need no new master.

- **The picture** (user, 2026-10-30): the user is the CEO; masters are directors, one per area (today the app master and
  the loop chat for `Loop` and `T`); sub-sessions are managers who hire Codex for one item or phase and let it go when
  done. It is a guide, not a wall: the user talks to either level directly. Testing an item or changing its scope goes
  to its sub-session; backlog and cross-item matters go to the master. Assume the master always finds out; it may give
  its view through the sub-session but does not override what the user asked there.
- **A master** owns its area's STATUS rows and the roadmap, runs Request to Approve and Merge, checks Files overlap
  before dispatching (`tools/files-overlap.ps1`), and starts sub-sessions. It never runs Codex, builds or hand-test
  launches itself; its own reading is the Low-tier scout. Masters of different areas tell each other in one line before
  a merge when both are active.
- **Master notes:** each master keeps `.coord/directors/<slug>.md` (gitignored; slug from its name): its role, open
  threads, decisions waiting on the user, linked sub-sessions, and where it left off. It updates the file when a
  decision is made, not later. After a context compaction a `SessionStart` hook (`tools/director-notes.ps1`) prints the
  notes back into the chat, so the master resumes from its own notes rather than the summary alone.
- **Master name:** `Master <N> · Loop v<V> · <slug>`, N one higher than the last master (STATUS "Working rules" names the
  current one), V the loop version above (written with "v" so it never reads as a `Loop` item ID), the slug the
  remaining work it was opened for. Example: `Master 2 · Loop v3 · era-two-start`.
- **Sub-session per step, well scoped:** each has one item, one step, one worktree, and a brief that names its done
  condition. Several sub-sessions per item, one per phase, is normal and often better (`A55` ran an audit chat, then a
  test chat); the item card is what they share. A sub-session records the user's scope changes on its card in the
  user's words and gets a design approval from the user itself. Name: `<ID> · <step> · <slug>`, for example `A1 · build · list-commands`. Its **role** is shown in its
  status card.
  | Step | Role | When | Model | Does |
  |---|---|---|---|---|
  | `scan` | `scout` | rarely: a Low scout too big for the master | Haiku | read-only findings, no Codex |
  | `audit` | `auditor` | Findings for Medium, High | Haiku | runs the Codex findings audit, leaves it untracked until Approve, reports |
  | `build` | `builder` | after Approve | Sonnet medium | runs Codex, then Check (`/review-task`) and the hand-test handover |
  | `review` | `reviewer` | Check; a fresh one only when the build session stalled | Sonnet medium | `/review-task` |
  | `test` | `tester` | the hand test; a separate one only for a re-test the user asked for | Sonnet medium | launches the build, gives the UI checks, records the result |
  A build session moving into Check or Hand test renames itself to that step (`A1 · review · list-commands`,
  `A1 · test · list-commands`) and changes its role, so the sidebar shows where it is; the master renames it if it
  cannot. The Codex role for each run is picked by the job (`docs/agents.md`).
- **Roadmap:** every master uses the same local server page `http://localhost:47811` (`/roadmap`), served live from STATUS
  and Git.
- **Handing over to a new master** (the chat grew long, or the loop version changed): no sub-session mid-step (or each one
  named in STATUS with its state), STATUS "Now" says where every item stands, dev clean and pushed. Retitle the old master
  `Master <N> · Loop v<V> · <slug> (closed)`, stop and archive it. The user opens the new master with:
  > You are `Master <N+1> · Loop v<V> · <slug>` for AntiNotepad. Read `AGENTS.md`, `docs/loop.md` (all of it, it is the
  > law for how you run items and start Claude and Codex sub-sessions) and `.coord/STATUS.md`. Rename this chat to that
  > name, update STATUS "Working rules" with it, open `/roadmap`, then reply with the status card for the next plan item
  > and wait for my approval. Computer Use only when I say so.

## Branches, worktrees and commits (user, 2026-10-29)
- **`main`** holds releases only. At a release, on the user's go, it fast-forwards to `dev` (no merge commits) and gets
  a plain semver tag: `v0.7.0`, with `-rc1`, `-rc2` for builds not yet hand-tested. No pull requests for now; they come
  back with the release pipeline.
- **`dev`** is integration: one commit per item (or per lettered step), nothing else.
- **Every item gets a branch** `<prefix>/<ID-slug>` off the latest dev, created at **Approve** (not before), including
  docs and tooling items. The prefix follows the kind of change: `feat/`, `fix/` or `refactor/` for A, `docs/` for D,
  `chore/` for Loop and T. Rework rounds stay on the same branch.
- **Worktree per item** in a folder that mirrors the branch: `.claude/worktrees/<prefix>/<ID-slug>`
  (`git worktree add .claude/worktrees/feat/A1-list-commands -b feat/A1-list-commands dev`; also fix/, refactor/, docs/, chore/). The master's checkout stays
  on dev. Sub-sessions share the master's folder (`use_worktree: false`) and work only inside that path, with absolute
  paths; Codex runs from the worktree's own `.coord/tools/codex-run.sh`. **At most 3 items** past Approve at once, and
  only if their Files lists do not overlap. After Check accepts, the implementation sub-session runs `tools/dev.ps1`
  from the worktree immediately before the hand checks; it verifies the dev badge and EXE/DATA/log paths. One dev copy
  runs at a time, so up to 3 items can build in parallel but hand tests take turns, oldest accepted item first.
  Without worktrees there is no parallel work at all (user, 2026-10-30).
- **No worktree** (only when the user says so, docs or tooling items): the branch is checked out in the main checkout
  (`tools/approve.ps1 -NoWorktree`) and nothing else runs until it merges.
- **Before Approve** there is no branch: the card and any audit brief and report live untracked in the master checkout;
  the auditor's Codex run is read-only there. At Approve they move into the new worktree and are committed on the branch.
- **What lands on the branch:** the card, audit, reports and code, committed there as often as useful (Codex cannot
  commit: the sub-session does). Push the branch to origin freely. Branches never edit STATUS.
- **What lands on dev: one squashed commit per item.** Between merges the master edits `.coord/STATUS.md` on dev and
  leaves it uncommitted (the roadmap reads the working copy). At Merge, `git merge --squash <prefix>/<ID-slug>`, then the
  item's commit carries its code, card, reports, its STATUS edits and its lessons. STATUS rides whole with whichever
  item's commit lands next, or one `chore: status` commit at the end of the day if nothing lands.
  No other direct commits to dev; urgent one-line fixes also go through a branch.
- **Commit message:** subject `<ID-slug>: <what changed>`, 72 characters or fewer; body of 3-6 short lines: why, what
  changed, the gate result, the hand test. No Claude attribution lines (`.claude/settings.json`).
- **Clean-up at Merge:** remove the worktree (`git worktree remove`, never `--force`: leftover files stop the merge and
  are listed for the user), then delete the branch locally (`git branch -D`, since a squash does not mark it merged) and
  on origin.

**Catching up with dev:** merge, never rebase (item branches are pushed). Catch up at two points: before Check and before
Merge when dev is not an ancestor of HEAD. The item's own session runs `tools/catch-up.ps1`, pushes the branch, then runs
the gate. `RETEST: yes` repeats the hand test before Merge; `RETEST: no` keeps the earlier result.

## Triggers in practice
Three commands drive the loop: `/next-task` (Request to Approve, then starts Build; master), `/review-task <ID-slug>`
(Check; sub-session), `/merge-task <ID-slug>` (Merge; master).
- **User, in the master session:** a request or `/next-task` (or "next"); the answer at Approve; the hand-test result;
  then `/merge-task <ID-slug>`; "release" at a marker.
- **Master, automatically:** writes the card, gets Findings (scout or audit sub-session), writes the Plan and asks; after
  approval, creates the branch and worktree and starts the Build sub-session.
- **Build sub-session, automatically:** launches Codex; when it exits runs `/review-task <ID-slug>` (gates, bounds check),
  relaunches rework itself, then, only after acceptance, starts its worktree dev copy with `tools/dev.ps1`, verifies the
  dev badge and EXE/DATA/log paths, hands the user the plan's UI checks and tells the master. Never launch during review or rework.
- **Any time, any terminal:** `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug>` (gate only).
- **CI (manual for now):** GitHub Actions workflow `gate`, run from the Actions tab; it runs `tools/gate.ps1 -Ci`.

## Files
| File | For |
|---|---|
| `AGENTS.md` (loaded by `CLAUDE.md` and by Codex) | project rules; points here |
| `docs/coding.md` | comments, formatting, file and folder rules |
| `docs/glossary.md` | one line per term |
| `.editorconfig` | formatting rules the gate's Format step checks |
| `.coord/templates/item.md` | the item card: Request, Findings, Plan, Approve, Codex brief, Check, Hand test |
| `.coord/templates/findings.md` | brief for the read-only Codex audit (Findings) |
| `.coord/templates/scan.md` | brief for one read-only Haiku backlog scout |
| `.coord/templates/review.md` | brief for the fresh Codex review |
| `.coord/templates/round.md` | brief for a rework round |
| `tools/gate.ps1` | build, selftest (LAYERS, CONTRACTS), seam, format, scope, loop-lint, size |
| `tools/loop-lint.ps1` | read-only consistency check of live loop files and their file references |
| `tools/loop-lint.allow` | path, rule and line-substring exceptions for historical mentions |
| `tools/catch-up.ps1` | merge local dev into an item branch; report conflicts and whether to repeat the hand test |
| `tools/files-overlap.ps1` | compare an item's Files with other item checkouts |
| `tools/scan-delta.ps1` | compare a backlog scan SHA and Where files with dev; flag rescans and cards to read |
| `tools/approve.ps1` | check the item cap and overlap, then create the branch and worktree |
| `tools/merge-preflight.ps1` | check the card, gate, branch and dev before Merge |
| `product/AntiNotepad/SelfTest/Contracts.cs` | the `CONTRACTS` table of cross-feature promises |
| `.agents/skills/lens-review/SKILL.md` | Codex review checklist (logic, coupling, reliability, plan bounds, proof rule) |
| `.codex/agents/lens-reviewer.toml` | Codex role the director spawns before reporting |
| `.coord/tools/codex-run.sh` | Codex wrapper: roles by job, failover on usage limits, ledger (`docs/agents.md`) |
| `.claude/agents/design-reviewer.md` | Claude design reviewer (Opus, read-only) |
| `.claude/skills/next-task/SKILL.md` | `/next-task [ID-slug]`: Request, Findings, Plan, Approve, start Build |
| `.claude/skills/scan-backlog/SKILL.md` | `/scan-backlog [slug ...]`: snapshot scouts and proposed backlog order |
| `.claude/skills/review-task/SKILL.md` | `/review-task <ID-slug>`: Check (gates, bounds check, verdict), hand over |
| `.claude/skills/merge-task/SKILL.md` | `/merge-task <ID-slug>`: Merge |
| `tools/loop-card.ps1` | `UserPromptSubmit` hook: status-card reminder and the items in flight |
| `.coord/lessons.md` | misses (cause, caught by, should have caught) and per-item reviewer scores; master-owned |
| `.github/workflows/gate.yml` | CI gate, manual trigger |
| `tools/roadmap-data.ps1`, `tools/roadmap-serve.ps1`, `tools/roadmap/index.html`, `tools/roadmap.url`, `.claude/skills/roadmap/SKILL.md` | live roadmap from STATUS and Git (`/roadmap`); snapshot Artifact on request |
| `tools/director-notes.ps1` | `SessionStart` hook after compaction: prints the master's `.coord/directors/<slug>.md` |
| `.claude/settings.json` | hooks: status card plus roadmap auto-start (UserPromptSubmit), master notes after compaction (SessionStart); attribution off |
