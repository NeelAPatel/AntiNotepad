# Loop2-loop-v3-1-fix-loop-scripts-review lens review

## Findings (proven)
1. [major] tools/files-overlap.ps1:39 - A failed worktree enumeration still prints FILES-OVERLAP: PASS. The native Git exit code is never checked: when worktree list fails without stdout, the loop does nothing, $others stays empty, and line 59 announces PASS. Set-StrictMode and ErrorActionPreference do not turn a native nonzero exit into a terminating error under the normal PowerShell defaults. Read-only fault injection ran the actual script with a Git function returning the real checkout root for rev-parse and no output plus LASTEXITCODE=128 for worktree list; it printed "FILES-OVERLAP: PASS (0 other item(s) in flight, 0 hot file warning(s))", and the child PowerShell command completed with exit 0. This can approve an overlap check without inspecting any other checkout. This defect is retained from the dormant script, rather than introduced by the prefix change, but is material to promoting it into the loop's safety check and the requested false-PASS review. Smallest fix: capture enumeration output, check LASTEXITCODE immediately, print FILES-OVERLAP: FAIL and exit 1 on error; explicitly exit 0 on the successful path. Add one failure-injection capability check.

## Behavior changes check
Report's list vs the diff: matches.
- Both scripts use one prefix list and anchored, case-sensitive item-name matching, consistent with gate.ps1's task-name grammar.
- Overlap reads cards from their own checkout paths, includes the main checkout, excludes the requested task, and treats the requested hot patterns as warnings.
- Catch-up allows tracked STATUS changes, rejects other tracked changes, ignores untracked files, checks fetch failure, and distinguishes merge refusals from conflicts.
- Base and HotFiles parameters remain. No rebase or push was added.
- Callers: rg found no active script or skill invoking either old path. References under docs/v2.2 are dormant activation/history documentation; updating those is reserved for later steps.
- The report does not claim to handle enumeration failure; finding 1 is an uncovered retained failure path.

## Scope and plan check
In plan: the tools diff contains exactly the brief's four paths: deletion of tools/v2.2/files-overlap.ps1 and tools/v2.2/catch-up.ps1, addition of tools/files-overlap.ps1 and tools/catch-up.ps1. Each implementation hunk serves After 1-5; no unrelated script edits, product changes, or unlisted implementation deviations found.
Existing STATUS and coordination artifacts were present before this review and are outside its script diff. This review wrote only this report. No branch switch, merge, commit, fetch, application launch, or product execution occurred.

## Unproven risks
- None beyond finding 1. Real merge/conflict persistence and preservation of dirty STATUS were not independently repeated; those are supported by the implementer's fixture report, not by this review's mocked Git probes.

## Checks run
- Read the item card, step brief, implementer report, gate report, review skill, coding standard, complete old/new scripts, and reference callers.
- Live branch: chore/Loop2-loop-v3-1. Live worktree inventory: main checkout only.
- Live files-overlap.ps1 -Task Loop2-loop-v3-1: FILES-OVERLAP: PASS (0 other item(s) in flight, 0 hot file warning(s)); exit 0.
- git diff --check: no whitespace errors; Git emitted its existing STATUS line-ending warning.
- In-memory Git stubs executed the actual scripts in child PowerShell processes; no fixture files or repositories were created:
  - Catch-up accepts all five prefixes: UP TO DATE, exit 0.
  - Rejects dev, detached HEAD, extra prefix text, extra slash suffix, different task, and wrong task-name case: REFUSED, exit 1.
  - Modified or staged STATUS alone: UP TO DATE, exit 0. Modified docs/loop.md: REFUSED, exit 1.
  - CLEAN: incoming.txt listed, exit 0. CONFLICT: docs/loop.md marked [hot], exit 2. Fetch failure and merge failure without unmerged paths: REFUSED, exit 1.
  - Conflict exit 2 was separately verified with explicit child-process exit propagation; the initial PowerShell command wrapper normalized nonzero script exits to 1.
  - Overlap main-checkout record using the existing C30-loop-docs-v22-plan card: 17 plain overlaps FAIL, docs/loop.md WARN, exit 1. Following detached, bare, and extra-slash branch records were ignored without reading their nonexistent paths.
  - Enumeration failure injection: false PASS reproduced as described in finding 1. Explicit propagation of LASTEXITCODE gives 128 instead, which also fails the requested 0/1 contract; no failure check or explicit successful exit exists in the script.
- Existing gate report: GATE: PASS; build 0 warnings/0 errors, SELFTEST PASS, CONTRACTS 13 rows (12 pass, 1 manual). Read as supplied evidence; gate not rerun because this review authorizes only its report to be written.

VERDICT: rework
