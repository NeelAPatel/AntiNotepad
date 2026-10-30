# T1-roadmap-overview-r3: fork parent from the nearest branch, one landing label per item

Rework round of `T1-roadmap-overview` (`docs/loop.md`). Read the item card `.coord/tasks/T1-roadmap-overview.md` and
`.coord/tasks/T1-roadmap-overview-r2.md` first; they still apply unless this file says otherwise. r2's code is in the
working tree (not committed); build on it.

Tier: Low · docs-tools
Role: worker (gpt-6-luna xhigh)

## Why this round
Loop chat browser check of r2 (2026-10-30) on r2's own probe clone: fork curves, the merge curve and "⤶ merged" draw
correctly, no console errors. Two faults:
1. `probe-r2-source` and `probe-r2-target` were made from `chore/T1-roadmap-overview` (head `c796e1e`) but show
   "↳ from dev" at `01bb862`. r2's brief said "merge-base with dev for side branches"; that rule is wrong (the brief's
   fault, not the code's) whenever a branch starts from another feature branch, which is how the user works.
2. "⤶ squash of chore/Loop2-loop-v3-1" appears on all 12 Loop2 commits. Loop2's step commits were fast-forwarded onto
   dev (the branch commits themselves), so the label belongs on one row only.
Also, r2's probe left `build/T1-r2-probe/` and `build/T1-r2-probe.bundle` behind (the loop chat deleted them).

## Change to the plan
1. **Parent lane:** for each branch B other than main, its fork is the newest commit that B shares with any other lane
   (compare `git merge-base B L` for every other lane L and take the one nearest B's head; ties go to the earlier lane
   in lane order). Its parent lane is that commit's home lane. dev's parent stays main when main is an ancestor of dev.
   `graph.lanes[].parent` and `fork` follow this rule.
2. **Landing label:** for each item on dev that no longer has a local branch, label only its newest dev commit:
   "⤶ squash of <branch>" when the item has exactly one dev commit, "⤶ landed from <branch>" when it has several
   (fast-forward). No label on its other commits.
3. **Cleanup:** the probe deletes its clone and any bundle or temp file it creates, also when a step fails
   (`try`/`finally`), and stops its server.

## Files
Files added for this round only (the gate allows the card's Files plus these).
- `.coord/tasks/T1-roadmap-overview-r3.md`

## Done means
- Probe on a throwaway clone under `build/T1-r3-probe` served on port 47888: `tmp-x` from dev with no commits (parent
  dev), branch `a` from dev with one commit, branch `b` from `a` with one commit (parent `a`, fork at `a`'s commit), a
  `--no-ff` merge of `b` into `a`. Report each lane's `parent`/`fork` from `/data` and which rows carry "↳" and "⤶"
  labels. Afterwards `build/` has no `T1-r3-probe*` left and no server on 47888.
- On the real repo, `/data` (port 47887, `-Root` default) gives exactly one "landed from chore/Loop2-loop-v3-1" row
  (read from the page code against `/data`); stop that server.
- `tools/roadmap-data.ps1 -Check` OK; `tools/gate.ps1 -Task T1-roadmap-overview -Offline` passes.
- Port 47811 is the user's live test server: never stop it.

Report: `.coord/reports/T1-roadmap-overview-r3.md`, in the card's report format.
