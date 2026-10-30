# T1-roadmap-overview-r2: the lane table shows where each branch starts and where it joins

Rework round of `T1-roadmap-overview` (`docs/loop.md`). Read the item card `.coord/tasks/T1-roadmap-overview.md` first
(Design, Plan, Hand test round 1): its Plan, Files and "For Codex" still apply unless this file says otherwise.

Tier: Low · docs-tools
Role: worker (gpt-6-luna xhigh)

## Why this round
Hand test round 1 (user, 2026-10-30), after `git branch tmp-x` from dev: "While theres no commits on the tmp-x branch, i
expect it to show that a new branch has been made FROM whatever branch it makes from, and same with merges. Everthing
else passes". Today the Branches tab draws a dot on every lane that contains a commit, so shared history looks like
parallel copies, `tmp-x` shows only a head ring on dev's commit, and nothing shows where a branch starts or joins.

## Change to the plan
Lane table drawing only (Branches & worktrees tab); the health table above it, the other tabs and the server stay.
1. **Home lane:** each commit is drawn once, on its home lane: the first lane in lane order (main, dev, then the others
   by latest commit) that contains it. Other lanes that contain it show no dot there.
2. **Parent and fork:** each branch other than main has a parent lane: the lane that is home to its fork commit
   (`git merge-base` with dev for side branches; dev's parent is main when main is an ancestor of dev). At the fork
   commit's row, draw a curve from the parent lane's dot into the branch's lane. The branch lane is solid from that row
   up to its head row, and dashed (as today) above its head and below its fork.
3. **Branch with no own commits** (head equals the fork commit, like `tmp-x`): at that row draw the curve from the
   parent lane ending in the branch's head ring on its own lane, and add "↳ from dev" (the parent lane's name) next to
   its label pill. A branch with own commits gets the same "↳ from <parent>" text at its fork row.
4. **Merges:** a commit with two parents whose second parent is home to another lane draws a curve from that lane into
   the commit's dot, with "⤶ merged <branch>" in Label. Squash merges leave no second parent: when a dev commit's subject
   starts with an item whose card has a `Branch:` line and that branch no longer exists locally, show
   "⤶ squash of <branch>" in Label (no curve; the branch is gone).
5. **Legend** (Branches tab): add rows for the fork curve, the merge curve, and the "↳ from" and "⤶" labels; keep the
   dashed-line row.
6. **Data:** `tools/roadmap-data.ps1` adds to `graph.commits[]` a `parents` array (short hashes) and to each
   `graph.lanes[]` entry `parent` (lane index or null) and `fork` (short hash or null); squash detection may be done in
   the page from `cards` and `log`. `-Check` still passes; the sample keys stay.
Drawing: SVG or CSS inside the existing 30 px rows, curves at most one row tall; the lanes stay pinned columns; no
horizontal page scroll at 375 px; both themes.

## Files
Files added for this round only (the gate allows the card's Files plus these).
- `.coord/tasks/T1-roadmap-overview-r2.md`

## Done means
- A probe on a throwaway clone under `build/T1-r2-probe` (deleted after; never the real repo), served with
  `tools/roadmap-serve.ps1 -Port 47890 -Root <clone>`: create in the clone `tmp-x` from dev (no commits), a branch with
  two commits, and a real merge commit (`git merge --no-ff`) back into a test branch; read `/data` and check `parents`,
  `parent` and `fork` for each, and list in the report what each row of the lane table will draw (read the page code
  against it). Stop the server.
- `pwsh -NoProfile -File tools/roadmap-data.ps1 -Check` prints `ROADMAP DATA OK`.
- `pwsh -NoProfile -File tools/gate.ps1 -Task T1-roadmap-overview -Offline` passes.
The loop chat then checks the drawing in a browser before the user's hand test round 2.

Report: `.coord/reports/T1-roadmap-overview-r2.md`, in the card's report format.
