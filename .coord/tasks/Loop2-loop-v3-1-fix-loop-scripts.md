# Loop2-loop-v3-1-fix-loop-scripts: step 3.1a, make the dormant overlap and catch-up scripts work under v3

Step brief of `Loop2-loop-v3-1` (read the card `.coord/tasks/Loop2-loop-v3-1.md` first: Findings, Plan, Approve).

Tier: Low · docs-tools
Role: director (gpt-6.1-sol medium)

## Why this step
v3 names branches `<prefix>/<ID-slug>` with prefix `feat`, `fix`, `refactor`, `docs` or `chore`. Both dormant scripts
assume `feat/` only: `tools/v2.2/files-overlap.ps1` scans only `branch refs/heads/feat/*` worktrees, and
`tools/v2.2/catch-up.ps1` refuses unless HEAD is `feat/<Task>`. Also, under v3.1 an item may run with no worktree (its
branch checked out in the main checkout), where `.coord/STATUS.md` is always edited and uncommitted.

## After
1. Move both scripts to `tools/` (`tools/files-overlap.ps1`, `tools/catch-up.ps1`; delete the `tools/v2.2/` copies and the
   folder). Rewrite their header comments for v3.1 (no "DORMANT" text; say what they do, how to run, exit codes).
2. Branch match in both: `^(feat|fix|refactor|docs|chore)/<ID-slug>$`, with the prefix list defined once at the top of each
   script.
3. `files-overlap.ps1`: consider every checkout in `git worktree list --porcelain`, the main checkout included, whose
   branch matches the pattern and whose `<ID-slug>` is not `-Task`; read that checkout's `.coord/tasks/<ID-slug>.md`.
   Hot files (warn only): `product/AntiNotepad/SelfTest/Contracts.cs`, `product/AntiNotepad/SelfTest.cs`, any
   `product/AntiNotepad/SelfTest/*.cs`, `docs/loop.md`, `docs/glossary.md`, `.claude/skills/*/SKILL.md`. Output and exit
   codes unchanged (PASS/WARN lines, `FILES-OVERLAP: PASS|FAIL`, exit 1 on a shared non-hot file).
4. `catch-up.ps1`: refuse (exit 1) when HEAD is not `<prefix>/<Task>`, or when there are tracked changes other than
   `.coord/STATUS.md` (`git status --porcelain --untracked-files=no`); untracked files do not block. Git's own refusal
   (a dirty STATUS that dev also changed) is reported as `REFUSED: ...` with git's message, exit 1. Rest unchanged:
   fetch, UP TO DATE / CLEAN (incoming files listed) / CONFLICT (hot files marked, merge left open), exit 0/0/2; never
   rebase, never push. Same hot-file list as above.
5. Keep both scripts short and in the style of `tools/gate.ps1` (`Set-StrictMode`, `$ErrorActionPreference = 'Stop'`,
   `docs/coding.md` comment rules).

## Files
- `tools/v2.2/files-overlap.ps1`
- `tools/v2.2/catch-up.ps1`
- `tools/files-overlap.ps1`
- `tools/catch-up.ps1`

## Done means
- `pwsh -NoProfile -File tools/files-overlap.ps1 -Task Loop2-loop-v3-1` prints `FILES-OVERLAP: PASS` (no other items).
- A probe in a throwaway clone under `build/Loop2-loop-v3-1-probe` (deleted after; never in this checkout): two
  branches `fix/T9-a` and `chore/T9-b` with cards sharing one plain file and one hot file prove FAIL and the hot WARN;
  `catch-up.ps1` there proves UP TO DATE, CLEAN, CONFLICT and both refusals (wrong branch; tracked change other than
  STATUS). List the probe results in the report.
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` passes.

## Constraints
Work in this checkout (`chore/Loop2-loop-v3-1` is checked out; no worktree). Never switch branches, merge, commit or
push here; never edit `.coord/STATUS.md`, the card or anything under `product/`. Never launch or kill AntiNotepad.

Report: `.coord/reports/Loop2-loop-v3-1-fix-loop-scripts.md` with the item template's report sections (Summary, Change
list, Behavior changes, Out-of-scope files, Plan deviations, Contracts, Lens review, Checks run, Hand checks).
