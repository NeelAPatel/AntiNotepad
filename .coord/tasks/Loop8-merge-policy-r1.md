# Loop8-merge-policy-r1: script changes for rebase plus a merge commit per item

Codex brief for the script part of `Loop8-merge-policy`. Read the item card `.coord/tasks/Loop8-merge-policy.md` first
(Request, Findings, Plan), then `AGENTS.md`, `docs/coding.md` and the three scripts below.

Tier: Medium · docs-tools
Role: terra, because three PowerShell scripts need reasoning about git behavior across a few files.

## What to build
New merge rule: item branches are tidied into meaningful commits, rebased onto dev, and land with one
`git merge --no-ff` commit. No squash. Edit only the three scripts; the manager writes docs and skills.

### 1. `tools/merge-preflight.ps1`
- **Every commit builds (new check).** After the gate-Head check, list the non-merge commits of `dev..<branch>` oldest
  first (`git rev-list --reverse --no-merges dev..<branch>`). For each commit that touches product code (any changed path
  under `product/`, via `git diff-tree --no-commit-id --name-only -r --root <sha>`), build it and run `--selftest`.
  Skip commits that touch nothing under `product/` and print `SKIP <short sha> <subject>: no product code`.
  - Do this in a scratch **detached** worktree under `<root>/build/<Task>-preflight` (create with
    `git worktree add --detach`, move it with `git checkout --detach <sha>` per commit, remove it in a `finally` with
    `git worktree remove --force`; `build/` is gitignored). Never touch the item checkout, its index, dev or any branch.
    The scratch build output goes to a folder inside that worktree's own `build/`-like temp dir you remove afterwards.
  - Build exactly as `tools/gate.ps1` does: `dotnet build product/AntiNotepad -c Debug -o <out>` with
    `AVALONIA_TELEMETRY_OPTOUT=1`, require exit 0; then run `<out>/AntiNotepad.exe --selftest` with
    `ANTINOTEPAD_DATA=<scratch data dir>` set and `ANTINOTEPAD_DEV` unset, require exit 0 and a line `SELFTEST PASS`.
    Use the same project path gate.ps1 uses (read it). Do not check warnings; that is the gate's job.
  - A failure throws `commit <short sha> "<subject>" does not build` or `... fails --selftest`, so the final line is
    `MERGE-PREFLIGHT: FAIL (...)` naming the commit.
  - Add switches `-Offline` (adds `-p:NuGetAudit=false` to the build, as gate.ps1 does) and `-SkipBuilds` (skips this
    check, prints `CHECK: per-commit builds skipped (-SkipBuilds)`; for scratch tests only).
  - Print `CHECK: N commit(s) build and pass --selftest (M skipped, no product code)` on success; with zero product
    commits print that all were skipped. Real `dotnet` / `AntiNotepad.exe` runs are fine offline (no ports, no browser);
    the selftest is the same one the gate runs.
- **Stale Head fix.** In `Test-GateHead`, widen the allowed set: commits after the gate may change only this item's card
  and reports (as today) **or** `.coord/STATUS.md`. Compare the file list of `git diff --name-only --no-renames
  <gateHead> <tip>`; this already covers a catch-up that brings only those files, since it diffs trees. Update the
  function comment and the success message `CHECK: commits after the gate touch only the card, reports and STATUS`.
- **Merge commits on the branch.** Fail with `branch has merge commits: rebase onto dev (tools/catch-up.ps1)` if
  `git rev-list --merges dev..<branch>` is non-empty.
- **Wording.** Header comment and messages no longer say squash: "Run from the main checkout before merging an item onto
  dev with `--no-ff`." Rename the `SQUASH_MSG` marker loop so it checks only `MERGE_HEAD` (a squash is no longer used).
  Message `Main checkout on dev: merge can run`.

### 2. `tools/catch-up.ps1`
- Rebases the item branch onto local `dev` instead of merging. Replace the `git merge --no-ff ...` call with
  `git rebase refs/heads/$baseRef`. Update the header comment ("Loop v5.2.0: fetches dev and rebases an item checkout onto
  it; pushes only with -Push, and only the item branch with --force-with-lease").
- Add `[switch]$Push`. After a clean rebase (and in the UP TO DATE case, when `-Push` is given) run
  `git push --force-with-lease origin $branch` (this checkout's current branch only, never dev). On push failure print
  `REFUSED: push failed: <output>` and exit 1. Without `-Push`, print nothing about pushing except in the CLEAN line hint:
  `re-run tools/gate.ps1 -Task <Task>; push with -Push or git push --force-with-lease origin <branch>`.
- A conflict now leaves a rebase open: keep exit 2, keep the hot-file marking, and say
  `CONFLICT: ... resolve, git add, git rebase --continue (or git rebase --abort)`. The conflict list still comes from
  `git diff --name-only --diff-filter=U`.
- Keep REFUSED checks. Add one: refuse when a rebase is already in progress (`rebase-merge` or `rebase-apply` under
  `git rev-parse --git-path`), `REFUSED: a rebase is already in progress`.
- Keep the RETEST report exactly as is.

### 3. `tools/approve.ps1`
- The worktree is the default and stays so. Do not change behavior. Only update the header comment and the
  `-NoWorktree` parameter comment so they say: worktree by default; `-NoWorktree` only when the user says so. Nothing
  suggests it for docs or tooling items. If the script has no such suggestion, change nothing but the comment.

## Files
- `tools/merge-preflight.ps1`
- `tools/catch-up.ps1`
- `tools/approve.ps1`

## Done means
Offline checks only (`docs/agents.md`, "What Codex can and cannot check"). Never start servers, open ports or a browser.
- `pwsh -NoProfile -Command "[scriptblock]::Create((Get-Content -Raw tools/merge-preflight.ps1)) | Out-Null"` (and the
  other two) parse without error.
- `tools/catch-up.ps1` on a scratch branch you create inside a scratch clone (`git clone` of this repo into `build/`,
  then delete it) behaves: UP TO DATE; CLEAN rebase with RETEST line; CONFLICT leaves a rebase open with exit 2. If
  the sandbox cannot do this, say so in the report rather than skipping silently. Do not touch this repo's dev or main.
- Code follows `docs/coding.md` (guide-style comments on each function, 4-space indent, `.editorconfig`).
- Never run `git commit`, `git rebase` or `git push` in this worktree; never edit the card or STATUS; never launch
  AntiNotepad except `--selftest` from a build output folder.

Report: `.coord/reports/Loop8-merge-policy-r1.md`, short, with exactly the sections from the card's "For Codex" report
format (Summary, Change list, Behavior changes, Out-of-scope files, Plan deviations, Contracts, Lens review, Checks run,
Hand checks for the user).
