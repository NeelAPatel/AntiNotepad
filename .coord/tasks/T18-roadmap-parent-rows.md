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

## Files
- `tools/roadmap/index.html`

## Approve
User, 2026-11-19: "approved for now, make a worktree'd version of roadmap with this change so i can preview here before going live" (page-only fix; orphans not edited).

## For Codex
Role: terra (`docs/agents.md`), because one file, normal reasoning. The branch is checked out: never switch, merge or commit. Read `AGENTS.md`, then `tools/roadmap/index.html` (`planView()`).
- Build exactly the Plan above in `tools/roadmap/index.html` only; no drive-by changes, no row-file or data-script edits.
- Offline checks only: `pwsh -NoProfile -File tools/roadmap-data.ps1 -Check`, and a JS syntax check of the page script (extract the `<script>` and run `node --check` if node exists). No ports, servers, browser.
- Never edit `.coord/STATUS.md` or this card. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task T18-roadmap-parent-rows -Offline`.
- Done means: children follow their parent under bucket sort and filters, show `parent › step`, orphans show `no parent`, search matches parent names, legend updated; `-Check` passes.

Report `.coord/reports/T18-roadmap-parent-rows.md`, short: 1 Summary; 2 Change list; 3 Behavior changes; 4 Out-of-scope files; 5 Plan deviations; 8 Checks run; 9 Hand checks.

## Check
Gate PASS offline (Scope 1 file, SELFTEST PASS); `roadmap-data.ps1 -Check` PASS; Codex terra report `.coord/reports/T18-roadmap-parent-rows.md`. Bounds: within plan. Live probe on 47822 (item chat): children show `parent › step`, orphans `no parent › scale-apply` and `design-plugins-review`, ordering holds. Noticed: `step: timer-names` is deferred and shows in another bucket; orphans stay in the Current bucket after the last row above them.

## Hand test
Preview: http://localhost:47822/ (label dev-worktree-roadmap-parent-rows, worktree page). User result: pending.
