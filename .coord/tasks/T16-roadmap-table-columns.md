# T16-roadmap-table-columns: Tier as a badge, a Type column, narrower branch column (or a column picker)

Item card (`docs/loop.md`). Side-task fast lane (T): short card, no audit, no pstack.

Tier: Low · docs-tools (side tool, does not ship), roadmap portal
Branch: `chore/T16-roadmap-table-columns` in `.claude/worktrees/chore/T16-roadmap-table-columns` (created at Approve)

## Request
User, 2026-11-16, relayed by Master 6 (director) to Roadmap Hub 1:
- Tier column: use the badge style that the ID column already uses.
- Add a Type column between Name and Scope (feat, fix, docs etc., standard GitHub-style prefixes; derive from the branch
  prefix or the row). Narrow the branch/worktree/era column.
- Or, if better: a column-view selector on the table to pick which columns show. Plan should pick one and say why.
- How to run it: "roadmap is not our main project, so pstack and /pstack:poteto-mode are not needed." One Sonnet item
  chat (user allows it for this item) plans; it runs Codex Terra (terra role via `.coord/tools/codex-run.sh`) to update
  the site. All work in a worktree so the user can double-check afterward. Stop at Approve as usual; do not merge:
  the director merges on the user's word. Do not touch the installed copy.

## Findings
- All of it is in `tools/roadmap/index.html`, `planView()` (~line 353-402): header string at 401, row cells at 395, group and
  release rows use `colspan="7"` (lines 379, 387). Tier is plain text `${r.tier} · ${r.effort}` in a `.num` cell. ID badge is
  `badgeId()` (`.b.id`, mono, family colors). Branch column is `badgeBr()` (branch name + worktree path badge) plus an era
  badge, in a flex-wrap `.cell-b`, which is what makes it wide.
- Type is not in the data. Branch prefix exists only for items that have a card or live branch (`branchOf(id)`, e.g.
  `chore/T16-...`); planned rows without a branch have none. So derive: branch prefix if known, else map by ID family
  (A to feat, Loop and L to chore, T to chore, D to docs, C to chore); steps/ideas show nothing. No `roadmap-data.ps1` change.
- The Log tab and Branches tab have their own tables with the same "Branch · worktree · era" column; request is about the
  Plan table, so I leave those.

## Plan
Pick: **badge + Type column** (not a column picker). Why: it answers the request directly, is a ~30-line change, and a
picker adds persistent state, a menu, and colspan/hidden-column bookkeeping for a 7-column table that already fits. A picker
stays a later idea if more columns arrive.
1. Tier cell: split `tier · size` into a Tier badge (`.b tier`, new low/med/high tint using existing tokens, mono weight like
   ID) and keep the size as muted text beside it. Header "Tier · size" stays.
2. New Type column between Name and Scope: a `.b` badge (feat, fix, docs, chore, refactor, perf, test) from `typeOf(id, br)`;
   empty cell for steps/ideas. Colspan 7 to 8 in the group and release rows; searchable text gets the type.
3. Narrow the last column: header "Branch · era"; show the branch badge with the prefix stripped (Type now carries it), era
   badge below, worktree path moved into the badge's tooltip; max-width with ellipsis. Drawer still shows full branch.
4. Legend entry for Type and Tier badges.
Expected time: Codex Terra build about 10 min, my check and test page about 10 min; total about 25 min.
Test page: from the worktree on port 47821 with `-Label dev-worktree-roadmap-table-columns`, never 47811.
Gate: `tools/gate.ps1 -Task T16-roadmap-table-columns -Offline`.

## Files
- `tools/roadmap/index.html`
- `tools/roadmap-data.ps1` (only if Type needs data)

## Approve
Approved by the user in the item chat, 2026-11-16 ("Approve"): Tier badge, Type column, narrower last column; no picker.
Build may start.

## Brief (for Codex)
Role: terra, a one-file UI change across a few functions in `tools/roadmap/index.html`.
Implement the Plan above, Plan tab only (`planView()`, legend, CSS). Details:
- Tier cell: badge `<span class="b tier t-low|t-med|t-high">` from `r.tier` first word (Low/Medium/High; unknown gets plain `.b`),
  mono font like `.b.id`, using existing color tokens; size (`r.effort`) stays as muted text after it.
- New `typeOf(id, br)`: branch prefix before `/` if known (feat, fix, docs, chore, refactor, perf, test); else by ID family
  (A feat, Loop/L/T/C chore, D docs); else ''. New Type column after Name, header `Type`, a `.b type` badge; empty for steps/ideas
  and rows with no id. Add the type to the row search `text`.
- `colspan="7"` becomes 8 (release row, group row, `table(..., 8)`).
- Last column: header `Branch · era`; branch badge with the `prefix/` stripped (full name plus worktree path in `title`), no
  worktree badge in the Plan table, era badge kept; `max-width` plus ellipsis so the column is narrow. Do not change `badgeBr()`
  for the Log/Branches tabs (add a Plan-only variant).
- Legend (`LEG.plan`): add Tier badge and Type badge rows.
- Do not touch other files, port 47811, or the installed copy.
Done means (offline only): `tools/gate.ps1 -Task T16-roadmap-table-columns -Offline` passes; the file parses (no JS syntax error,
e.g. `node --check` on the extracted script if node exists); diff limited to `tools/roadmap/index.html`.
Write a short report to `.coord/reports/T16-roadmap-table-columns.md`.

## Scope up (user, in the item chat, 2026-11-16) - Brief 2 for Codex (role: terra, same files plus the data script)
Build on the committed work. Files now: `tools/roadmap/index.html`, `tools/roadmap-data.ps1`.
1. **Collapsible group headers** (Plan tab): the "1. Current / 2. Next / 3. Backlog / 4. Done N rows" header rows toggle their rows
   (chevron, click or Enter/Space, aria-expanded). Default: all expanded; remember per header in localStorage (try/catch like
   `roadmap-hidden`). While searching, show everything.
2. **Remove release points**: Release marker rows (two in `.coord/pending.md`, plus the legend line and the release row code in
   `planView`) no longer show anywhere in the Plan tab or count. Do NOT edit `.coord/*.md` (director owns them); skip rows whose
   item is 'Release marker' in the page and remove the "Release point" legend row. Releases tab stays.
3. **Entered timestamp**: new column right after Name, header "Entered", format `Nov 16 2:35p` (month day, 12-hour, a/p, local
   time). Source: `tools/roadmap-data.ps1` adds `entered` (ISO or epoch, per row) = commit time when the row's item text first
   appeared in `.coord/STATUS.md`, `.coord/pending.md` or `.coord/status-archive.md`, found with ONE `git log --reverse -p` pass
   over those files (not a git call per row), matched on the row's item key. Blank when not found. Keep data build fast (cache by HEAD
   if needed). Colspan becomes 9 (header rows span all visible columns, compute it).
4. **Column tools for the Plan table**: (a) resizable columns: drag handle on each header's right edge, widths saved in
   localStorage, double-click resets; (b) a filter per column: a small text box row under the header (or per-header popover) that
   narrows rows by that column's text (State, Tier, Type, Branch, Name, Scope, Entered); combined with the global search; a clear
   button; (c) a Columns picker button (near the filter chips) with checkboxes to show/hide each column, saved in localStorage,
   Reset link. Hidden columns also drop their header, cells and colspan. ID and Name cannot be hidden.
Done means (offline): gate passes; JS parses; `roadmap-data.ps1` still outputs valid JSON via its own check mode if one exists
(see its header); diff limited to the two files. Update `.coord/reports/T16-roadmap-table-columns.md`.

## Rework 3 (user, 2026-11-16): Excel-style column filters - Brief 3 for Codex (role: terra, `tools/roadmap/index.html` only)
The per-column text boxes are rejected ("really poor"). Replace them with Excel-style header filters:
- Remove the filter-input row. Each filterable column header gets a small filter button (funnel icon) beside its title; it shows
  an active state (dot or accent) when that column has a filter. Columns: Name, Entered, Type, Scope, Tier, State, Branch/era
  (ID too if easy). Keep resize handles and the Columns picker working.
- Click opens a popup anchored under the button (closes on outside click or Esc; keyboard reachable; one popup at a time):
  a search box at top (filters the value list as you type), then "(Select all)" / "Clear all" actions (acting on the visible,
  searched values), then a scrollable checkbox list of the column's distinct values with row counts, then OK/Cancel or apply-live
  (live is fine) and a "Clear filter" link. Blank values appear as "(Blanks)". Sorting A-Z/Z-A inside the popup is a bonus
  (state kept simple: one active sort).
- Filter model: per column a Set of selected values (null = no filter). Rows show if every active column filter matches, ANDed
  with the global search and state chips. Value lists reflect the other active filters only if simple; otherwise all values.
- Entered column filters by date label text (e.g. "Nov 12"), not the time.
- Save filters in memory only (not localStorage). Show a "Clear all filters" button near the Columns picker when any is active.
- Done means: gate offline passes; JS parses; diff limited to `tools/roadmap/index.html`; update the report.

## Check
Item chat, 2026-11-16. Tier Low (side tool, does not ship).
- Gate: `tools/gate.ps1 -Task T16-roadmap-table-columns -Offline` PASS after each Codex run (last on head 0db7c29): build 0 warnings, selftest, seam, tokens, scope (only `tools/roadmap/index.html` and `tools/roadmap-data.ps1` allowed). Offline NuGet audit WARN is the sandbox, expected.
- Bounds check, request vs plan vs done:
  - Request (Tier badge, Type column between Name and Scope, narrower branch/worktree/era column, or a picker): done as planned (badge + Type + narrow column); picker was also delivered later at the user's word.
  - Scope growth, all asked by the user in this chat on 2026-11-16: collapsible group headers; Release marker rows hidden in the page (the two rows remain in `.coord/pending.md`, director-owned); Entered timestamp column (from the first git commit that carries the row, 145 of 151 rows filled, rest blank); resizable columns; Columns show/hide picker; per-column filters, reworked once to Excel-style popups (search, select all, clear all, value counts) after the first text-box version was rejected.
  - Nothing beyond that: no installed copy, live server 47811 or product code touched; `roadmap-data.ps1` only adds the `entered` field.
- Behavior checked on a test page (port 47821, now stopped): no console errors; collapse 117 to 50 rows and back; State popup opens by real click, unticking "done" 117 to 51 rows, Esc closes, Clear all filters restores 117. Not clicked through: every other column's popup.
- Known cosmetic: State filter values come straight from STATUS text and can be long.
- Step-0 commit review: 8 commits, each docs/tools only so each builds; card, brief and scope commits are separate from the code commits (3 code commits). Left as is, no rewrite.
- Verdict: accepted.

## Hand test
User, 2026-11-16, on the test page (http://localhost:47821/, label dev-worktree-roadmap-table-columns):
- After the first build: "yes test page looks right, but still do these new scope values".
- After the Excel-style filters: "Looks good".
