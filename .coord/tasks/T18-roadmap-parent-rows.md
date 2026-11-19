# T18-roadmap-parent-rows: child rows show under the wrong parent on the Plan table

Item card (`docs/loop.md`). Side-task fast lane (T): short card, no audit, no pstack.

Tier: Low · docs-tools (side tool, does not ship), roadmap portal
Branch: `chore/T18-roadmap-parent-rows` in `.claude/worktrees/chore/T18-roadmap-parent-rows` (created at Approve)

## Request
User, 2026-11-19, to Roadmap Hub 1 (with a screenshot of the Plan table):
- "Some of the roadmap items don't make sense because there's no way to see who's the parent."
- Example: `A32-selection-convert-menu` (feat) is followed by child rows `↳ scale-apply` and `↳ design-plugins-review`,
  so they look like its children, "which makes zero sense". Same pattern lower down: `↳ notes-path-dedupe`,
  `↳ ui-kit-merge`, `↳ ownership-map`, `↳ behavior-timer-names` under `code-ownership`, and `↳ path-validator` ...
  `↳ single-result-message` under `notes-folder-move-flow` (those may be right; check).
- Ask: figure out what's going on (data: how `.coord/pending.md` / STATUS / archive rows mark steps and parents, and how
  `tools/roadmap-data.ps1` + `tools/roadmap/index.html` decide which row is a child and of whom; likely the `↳` step rows are
  attached to whatever row sits above them after sorting/filtering/grouping, or the parent row moved/finished), then
  propose a fix (e.g. name the parent on each child row, keep children glued to their parent under sort/filter/group, show
  orphans plainly).
- How to run it (user): one focused Sonnet item chat; use Codex as much as possible for medium+ reads, writes and the build;
  save Claude usage for planning and discussing. Stop at Approve as usual; do not merge (Roadmap Hub 1 merges on the user's
  word). Do not touch the installed copy or the live roadmap server on port 47811 (test pages on a spare port with `-Label`).

## Findings
Read by the item chat (small, two files; no Codex read needed). Evidence from `tools/roadmap-data.ps1` output and `tools/roadmap/index.html` `planView()`.
- Row files carry no explicit parent. A parent is an `idea:` row and a child is a `step:` row; the link is only "the nearest `idea:` row above it in the same group" (`parentByGroup`, index.html ~L431). The link is used only to decide whether the parent may be hidden; the table never draws it.
- Display order is the file order, then a stable sort by bucket (Current / Next / Backlog / Done, `bucketOf`, ~L447). The state filter, search and column filters then drop rows one by one, so a step ends up beside whatever row survives above it. The child row shows only `↳ name`, no parent name.
- Why `↳ scale-apply` sits under `A32-selection-convert-menu`: in `pending.md` group "Design system and ownership" the steps `scale-apply`, `design-plugins-review`, `timer-names` come first and have **no `idea:` row above them** (their parent, design-questions, is gone). They are true orphans. With the Planned filter, rows 6-17 (paused/proposed) drop out, so the last surviving row above them is A32. Data dump order: rows 5 A32, then 18-20 the orphans.
- The other groups are right: `notes-path-dedupe`, `ui-kit-merge`, `ownership-map`, `behavior-timer-names` follow `idea: code-ownership`; `path-validator` ... `single-result-message` follow `idea: notes-folder-move-flow`. They only look like orphans when the parent is filtered out or its bucket differs (e.g. `step: timer-names` is deferred, so it can leave its parent's bucket).
- Root cause: the child-to-parent link is implicit and not shown or kept under bucket/filter, plus 3 genuine orphans in the data.

## Plan
Chosen: page-only change in `tools/roadmap/index.html` (no row-file edits, no data-script change).
1. Compute each step's parent once (same rule) and keep a child glued right after its parent: sort order within a bucket follows the parent's position, and a child takes its parent's bucket.
2. Show the parent on every child: `↳ parent-name › step-name` (parent in muted text). Orphans show `↳ no parent › step-name` in a warning color with a tooltip. If the parent is filtered out, the name still shows.
3. Search and column filters match the parent name too, so a child is found by its parent's name.
4. Legend line updated.
Why this one: the data already has the info; showing it fixes every case, and it needs no change to the user's row files.
Not done, your call later: give the 3 orphans a parent by adding an `idea: design-system` row in `pending.md` (a row-file edit, needs your OK).
Rough time: about 1 hour (Codex terra build, then Claude gate and test page on port 47822).

## Rework r2 (user, 2026-11-19, after the preview)
"If a parent is found i want it to be collapsable/foldable. If there's no parent then it stands on its own."
- A parent that has children gets a fold toggle (▾/▸) in its Name cell and a child count; folded hides its steps. State saved like `planCollapsed` (own key, per parent item name). Search or column filters active: show matching steps unfolded, as groups do.
- A step with no parent renders as a normal standalone row: no `↳`, no "no parent" label, no warning color.
- Children keep `parent › step` naming and stay glued under the parent. A child whose parent is filtered out still names it.
- Same single file, `tools/roadmap/index.html`. Update the legend (fold arrow; remove orphan line).

## Rework r3 (user, 2026-11-19, with a sidebar screenshot)
User: five groups in this order: 1 Active (items launched / with a sub-chat), 2 Upcoming (what the user said is upcoming), 3 Paused/Deferred, 4 Backlog, 5 Done. "Have some sort of cache too so I can save the column widths I set and expand/fold status across the board."
- Replace BUCKETS (Current/Next/Backlog/Done) in `planView()`. Precedence per row (a step follows its parent's bucket as in r1): Done (state done) > Paused/Deferred (stKind 'def') > Active (state you/act, or the item has a live branch or worktree: `wtByBranch`) > Upcoming (group name starts "Next"; that is the "Next up" group in `pending.md`) > Backlog. Rename the headers accordingly; the collapse state keys follow the new names.
- Careful: `stKind()` returns 'act' for any unknown state, so "plan ready" and "request" rows count as 'act' today (that is why Current holds 35 rows). Do not use stKind 'act' alone for Active. Active = a live item branch/worktree (not done, not paused/deferred), or a state that is clearly in flight (approved, building, lab, check, hand test, needs you from the STATUS In-flight table). "plan ready", "request", "proposed", "planned" are NOT Active. Check the result against the data dump: only items with a worktree (A29, A32, T18, etc.) land in Active.
- Limit: the page only sees STATUS rows, branches and worktrees, not Claude chats. Chat-aware Active needs Loop Hub to write it into STATUS (follow-up, not here).
- Shared prefs: widths, hidden columns, group folds, parent folds, hidden rows are saved today in `localStorage`, which is per port, so the preview port and the live port do not share them. Add `GET /prefs` and `POST /prefs` to `tools/roadmap-serve.ps1` (localhost only, body capped at 64 KB, valid JSON object only) reading/writing `roadmap-prefs.json` in the MAIN checkout's `.coord/` (resolve via `git rev-parse --git-common-dir`); add that file to `.gitignore`. The page loads prefs at start (falls back to `localStorage` when the server has none or fails) and writes both on each change.
- Files for r3 add `tools/roadmap-serve.ps1` and `.gitignore`.

## Rework r4 (user, 2026-11-19, the four boxes at the top)
User: "Take a look again at the 4 containers at the top... do they make sense? I would rather have it be: Needs You, Working, Hand-test queue, and Recent Codex Runs. The top 4 boxes should only be making use of Active items only." Correction, same day: "*Active and upcoming" (the boxes use the Active and Upcoming buckets).
Findings: `renderNow()` builds the boxes from `ACTIVE(state)` = stKind 'you' or 'act', and stKind returns 'act' for any unknown state, so "plan ready" ideas fill "Moving without you" (about 100 rows). The Plan table's Active bucket (r3) uses a different rule. Two rules for "active" is the bug.
- Make one helper `bucketOf` reuse: `isActiveRow(r)` and `isUpcomingRow(r)` (the r3 Active and Upcoming rules) feed both the Plan buckets and the boxes; rows in Paused/Deferred, Backlog and Done never appear in the boxes. Was: `isActiveRow(r)` (the r3 Active rule: live branch/worktree, or an in-flight state; never done, paused/deferred, plan ready, request, proposed, planned) and use it in both `bucketOf()` and `renderNow()`.
- Boxes, in this order and with these names: **Needs you** (Active or Upcoming rows waiting on the user: approve, answer, `plan ready`/`request`/`needs you` in Upcoming means waiting for the user's go; not hand test), **Working** (Active rows that are not needs-you, not hand test; count text drops "past Approve" unless cheap to keep), **Hand-test queue** (Active rows whose state is hand test), **Recent Codex runs** (last 6 runs whose task is an Active or Upcoming item; if none, the last 6 overall with the heading note "no active item runs").
- Update the page header comment, the legend and any copy that says "Your turn"/"Moving without you". Empty states stay plain.
- Limit: chat self-rename titles (e.g. "[Codexing..] A29-shortcut-map ◆ Lab") are in Claude's own session list, not in any file the page can read. Phase comes from STATUS and cards. A follow-up for Loop Hub: write the chat phase into the STATUS row (or `.coord/` state file) so the page can read it.

## Files
- `tools/roadmap/index.html`
- `tools/roadmap-serve.ps1` (r3)
- `.gitignore` (r3)

## Approve
User, 2026-11-19: "approved for now, make a worktree'd version of roadmap with this change so i can preview here before going live" (page-only fix; orphans not edited).

## For Codex
Role: terra (`docs/agents.md`), because one file, normal reasoning. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then `tools/roadmap/index.html` (`planView()`).
- Build exactly the Plan above in `tools/roadmap/index.html` only; no drive-by changes, no row-file or data-script edits.
- Offline checks only: `pwsh -NoProfile -File tools/roadmap-data.ps1 -Check`, and a JS syntax check of the page script (extract the `<script>` and run `node --check` if node exists). No ports, servers, browser.
- Never edit `.coord/STATUS.md` or this card. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task T18-roadmap-parent-rows -Offline`.
- Rework r4: build the section "Rework r4" on top of the existing diff, same file only (`tools/roadmap/index.html`). Offline checks only.
- Rework r3: build the section "Rework r3" on top of the existing diff. Offline checks only; do not start the server (the item chat tests `/prefs` live). Read-only data script stays untouched.
- Rework r2: build the section "Rework r2" on top of the existing diff (it supersedes the orphan label from the Plan). Report to `.coord/reports/T18-roadmap-parent-rows.md` (overwrite).
- Done means: children follow their parent under bucket sort and filters, show `parent › step`, orphans show `no parent`, search matches parent names, legend updated; `-Check` passes.

Report `.coord/reports/T18-roadmap-parent-rows.md`, short: 1 Summary; 2 Change list; 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 8 Checks run; 9 Hand checks.

## Check
Gate PASS offline (Scope 1 file, SELFTEST PASS); `roadmap-data.ps1 -Check` PASS; Codex terra report `.coord/reports/T18-roadmap-parent-rows.md`. Bounds: within plan. Live probe on 47822 (item chat): children show `parent › step`, orphans `no parent › scale-apply` and `design-plugins-review`, ordering holds. Noticed: `step: timer-names` is deferred and shows in another bucket; orphans stay in the Current bucket after the last row above them.

## Hand test
Preview: http://localhost:47822/ (label dev-worktree-roadmap-parent-rows, worktree page). r2-r4 (fold, five groups, shared prefs, right-aligned Columns) checked by item chat; User result: pending.
