# Loop2-loop-v3-1-preflights: step 3.1b, scripted Approve and Merge preflights

Step brief of `Loop2-loop-v3-1` (read the card `.coord/tasks/Loop2-loop-v3-1.md` first). Step 3.1a is committed:
`tools/files-overlap.ps1` and `tools/catch-up.ps1` exist; reuse their branch pattern and style.

Tier: Low · docs-tools
Role: director (gpt-6.1-sol medium)

## Why this step
The cap of 3 items past Approve and "Files not overlapping" are checked by eye (`.claude/skills/next-task/SKILL.md` step 4),
and `/merge-task` steps 1-2 are a checklist a session reads, not a check that runs. Nothing proves a branch was caught up
with dev or that its gate passed on the commit being merged, or that the shared checkout has no merge half done.

## After
1. **`tools/approve.ps1 -Task <ID-slug> -Prefix <feat|fix|refactor|docs|chore> [-NoWorktree] [-Cap 3] [-DryRun]`**, run
   from the main checkout:
   - Refuse unless the main checkout is on `dev`, has no tracked changes other than `.coord/STATUS.md`, and has no merge
     in progress (`git diff --cached --quiet`; no `SQUASH_MSG` or `MERGE_HEAD` under `git rev-parse --git-path`).
   - The card `.coord/tasks/<Task>.md` exists untracked here and its `## Approve` section is filled (not empty, not a
     `<...>` placeholder line).
   - **Cap:** count Plan rows in `.coord/STATUS.md` (section `## Plan`, table rows as `tools/loop-card.ps1` parses them)
     whose State is past Approve: `approved`, `building`, `rework r<N>`, `checking`, `accepted`, `ask-user`, `hand test`,
     `hand-tested`; ignore the row for `-Task`. Count >= Cap: `APPROVE: FAIL (cap)`.
   - **Overlap:** run `tools/files-overlap.ps1 -Task <Task>`; its FAIL is `APPROVE: FAIL (overlap)`.
   - Then create: default `git worktree add .claude/worktrees/<Prefix>/<Task> -b <Prefix>/<Task> dev`, move the card and
     any untracked `.coord/tasks/<Task>-*.md` and `.coord/reports/<Task>-*` into the worktree, commit them there as
     `<Task>: card`. `-NoWorktree`: `git switch -c <Prefix>/<Task> dev` in the main checkout (STATUS travels uncommitted),
     commit the same files as `<Task>: card`.
   - Never edit STATUS (the master sets `approved`); never push. `-DryRun` prints every check and the commands, changes
     nothing. Output lines end with `APPROVE: PASS|FAIL (<reason>)`; exit 0/1.
2. **`tools/merge-preflight.ps1 -Task <ID-slug>`**, run from the main checkout:
   - Exactly one local branch `<prefix>/<Task>` (prefix list as in 3.1a).
   - The card on that branch (`git show <branch>:.coord/tasks/<Task>.md`): `## Request`, `## Plan`, `## Approve` (or
     `## Go` on era-one cards), `## Check` and `## Hand test` present and filled (no `<...>` placeholder line); no line in
     `## Check` that contains `ask the user` unless the same line contains `answered`.
   - The branch's checkout (its worktree, or the main checkout for a `-NoWorktree` item) has no tracked changes other
     than `.coord/STATUS.md`.
   - `dev` is an ancestor of the branch (`git merge-base --is-ancestor dev <branch>`); else `FAIL (behind dev: run
     tools/catch-up.ps1 -Task <Task>)`.
   - Gate on file: in that checkout, the newest `.coord/reports/<Task>.gate.md` or `<Task>-r<N>.gate.md` says
     `GATE: PASS` and its `Head:` line equals the branch tip.
   - The main checkout has no merge in progress (as in approve). Print whether the main checkout is on `dev` (squash
     can run) or on the item branch (`-NoWorktree`: switch to dev first).
   - Output lines end with `MERGE-PREFLIGHT: PASS|FAIL (<reason>)`; exit 0/1. Read-only: changes nothing.
3. **`tools/gate.ps1`**: the report's Summary gets one line `Head: <full sha of HEAD>` right after `GATE:` (with
   `-dirty` appended when the checkout had tracked changes other than `.coord/STATUS.md`). Nothing else changes.
4. **`tools/loop-card.ps1`**: when more than 3 items are past Approve (same state list), add one line
   `WARNING: <n> items past Approve (cap 3).` ASCII only, as today.
5. **Skills**: `.claude/skills/next-task/SKILL.md` step 4 runs `tools/approve.ps1` (with `-NoWorktree` only when the
   user said so) instead of the hand-written `git worktree add` and move; it stops on FAIL and tells the user the reason.
   `.claude/skills/merge-task/SKILL.md` steps 1-2 become "run `tools/merge-preflight.ps1 -Task <ID-slug>`; FAIL: stop and
   tell the user the reason"; keep the "anything outside Files: ask the user" diff check. Do not touch other steps.

## Files
- `tools/approve.ps1`
- `tools/merge-preflight.ps1`
- `tools/gate.ps1`
- `tools/loop-card.ps1`
- `.claude/skills/next-task/SKILL.md`
- `.claude/skills/merge-task/SKILL.md`

## Done means
- In a throwaway clone under `build/Loop2-loop-v3-1-probe` (deleted after; never this checkout) with a small STATUS and
  cards: approve PASS with a worktree and with `-NoWorktree`; FAIL on cap (3 rows past Approve), on overlap, on a
  missing Approve section, on a dirty checkout; merge-preflight PASS on a caught-up branch with a matching gate report
  (write a fake report with the right `Head:`), FAIL when behind dev, on a stale `Head:`, on an open "ask the user" line,
  on a staged merge in the main checkout. List the results in the report.
- `pwsh -NoProfile -File tools/loop-card.ps1` prints its normal lines on this checkout.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` passes and its report has a `Head:` line.

## Constraints
Work in this checkout (`chore/Loop2-loop-v3-1` checked out; no worktree). Never switch branches, merge, commit or push
here; never edit `.coord/STATUS.md`, the card, or anything under `product/`. Never launch or kill AntiNotepad. Keep the
scripts short, in the style of `tools/files-overlap.ps1` and `docs/coding.md`.

Report: `.coord/reports/Loop2-loop-v3-1-preflights.md` with the item template's report sections.
