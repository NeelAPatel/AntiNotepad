# C13-dev-loop-kit-r3: rework rounds read all their briefs; contract rows own their checks

Role: worker (gpt-6-luna xhigh). Do the work yourself. Product `product/AntiNotepad`.
Branch: `feat/C13-dev-loop-kit` (C13 and r2 are committed; never switch, merge or commit).
Tier: Medium
Read first: `.coord/tasks/C13-dev-loop-kit.md`, `.coord/tasks/C13-dev-loop-kit-r2.md`, `tools/gate.ps1`,
`product/AntiNotepad/SelfTest/Contracts.cs`, `SelfTest.Run` in `product/AntiNotepad/SelfTest.cs`.

## Goal
Two design-review findings. Product runtime behavior must not change.

## Files
Only these files may change. Touching any other file needs a line under "Out-of-scope files" in the report.
- `tools/gate.ps1`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest.cs` (only the `Run` lines named in item 2)

## Contracts
All six rows keep their result (5 pass, 1 manual).

## To do
1. **Rework scope** (`tools/gate.ps1`, scope step): for a task named `<base>-rN` (N >= 2), the allowed set is the union of
   the backticked `## Files` paths in `.coord/tasks/<base>.md` and every existing `.coord/tasks/<base>-rK.md` with K <= N.
   List which briefs were read in the Scope step lines. A name without `-rN` behaves as today. Prove it: run
   `-Task C13-dev-loop-kit-r3 -Offline` and see no out-of-scope warning for files listed in the C13, r2 or r3 briefs
   (the manager-owned `.claude/**`, `.agents/**`, `.codex/**`, docs files are expected to still warn).
2. **Contract rows own their checks**: `BackgroundSaveAndLogChecks`, `UnavailableStoreStartupChecks`, `FenceCacheChecks`
   and `SettingsChangedChecks` are called both from `Run` and from the CONTRACTS table. Remove their own lines from `Run` so
   each runs once, from the table only. Each failure message must still name the check (keep the old text inside the
   contract failure detail, e.g. `contract fence-cache-bounds: A28-fence-cache: <report>`). Split the two A30 rows so each
   can fail on its own only if `BackgroundSaveAndLogChecks` already reports its cases separately; otherwise keep the shared
   result and say so in the report (no new test code for this).

## Done means
- Item 1 proof as above; a plain name (no `-rN`) unchanged.
- `SELFTEST PASS`, `CONTRACTS: 6 rows, 5 pass, 1 manual`; a temporary broken expectation in `FenceCacheChecks` still fails
  the selftest with the check's name in the message (restore after).
- Gate with `-Offline` PASS, build 0 warnings.

## Report: `.coord/reports/C13-dev-loop-kit-r3.md`
Sections: Summary, Change list, Behavior changes, Out-of-scope files, Checks run.
