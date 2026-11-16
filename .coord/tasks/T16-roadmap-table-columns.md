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
