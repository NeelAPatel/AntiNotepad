# C13-dev-loop-kit: gate script, reviewer tier, CONTRACTS selftest and a manual CI workflow

Role: director (gpt-6.1-sol medium). Delegate implementation to one gpt-6-luna xhigh worker; review its diff yourself.
Product `product/AntiNotepad`. Branch: `feat/C13-dev-loop-kit` (already checked out; never switch, merge or commit: you
cannot write to `.git`).
Tier: Medium (rules: `docs/loop.md`, being written by the manager in parallel; do not edit docs).
Read first: `AGENTS.md`, `docs/agents.md`, `.coord/tools/codex-run.sh`, `tools/check-platform-seam.ps1`, `tools/dev.ps1`
(how it builds), `product/AntiNotepad/SelfTest.cs` (`Run`, `UnavailableStoreStartupChecks`, `FenceCacheChecks`,
`BackgroundSaveAndLogChecks`, the settings Changed and LAYERS checks). The manager already wrote, uncommitted on this branch:
`.coord/templates/brief.md`, `.coord/templates/review.md`, `.agents/skills/lens-review/SKILL.md`,
`.codex/agents/lens-reviewer.toml`, `.claude/**`. Do not edit those.

## Goal
Build the scripted part of a new dev loop so every task gets the same zero-token checks, a fresh Codex reviewer can be
launched by tier, and cross-feature behavior promises are tested. Product behavior must not change at all.

## Files
Only these files may change. Touching any other file needs a line under "Out-of-scope files" in the report.
- `tools/gate.ps1` (new)
- `.coord/tools/codex-run.sh`
- `product/AntiNotepad/SelfTest/Contracts.cs` (new)
- `product/AntiNotepad/SelfTest.cs` (make the class `partial`, one call line in `Run`; nothing else)
- `.github/workflows/gate.yml` (new)

## 1. `tools/gate.ps1`
PowerShell 7, `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug> [-Base dev] [-StrictScope] [-SizeWarn 600] [-Keep] [-Ci]`.
Runs from the repo root or any worktree (`git rev-parse --show-toplevel`). Validate `-Task` like codex-run.sh does
(`^[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+$`); `-Ci` makes `-Task` optional (default `ci`) and skips the scope step.
Steps, each recorded as PASS / FAIL / WARN / SKIP with its key output lines:
1. **Build**: Debug build of `product/AntiNotepad/AntiNotepad.csproj` into `build/<Task>-gate` with
   `AVALONIA_TELEMETRY_OPTOUT=1`, the same way `tools/dev.ps1` builds but without the dev identity property. FAIL on any
   error or any warning (count them).
2. **Selftest**: run the built exe with `--selftest` only, with `ANTINOTEPAD_DATA` set to a fresh
   `build/<Task>-gate-data` folder (absolute path). FAIL unless exit code 0 and output contains `SELFTEST PASS`. Capture the
   `LAYERS` and `CONTRACTS` summary lines. Never start the app any other way and never pass `--quit`.
3. **Seam**: run `tools/check-platform-seam.ps1`; FAIL on non-zero.
4. **Scope**: changed files = `git diff --name-only <Base>...HEAD` plus uncommitted and untracked files, excluding
   `.coord/**`. Allowed = the backticked paths in the bullet list under `## Files` in `.coord/tasks/<Task>.md`. Any changed
   file not allowed = WARN listing each (FAIL with `-StrictScope`). No brief or no `## Files` section = WARN.
5. **Size**: each changed `product/**/*.cs` file over `-SizeWarn` lines that grew versus `<Base>` = WARN with old -> new
   line counts.
Write `.coord/reports/<Task>.gate.md`: first a `## Summary` section (one line `GATE: PASS|FAIL`, then one line per step),
then a section per step. Also print the summary. Delete `build/<Task>-gate` and `-gate-data` at the end unless `-Keep`
(keep them on FAIL). Exit 0 on PASS (warnings allowed), 1 on FAIL. Refuse to delete anything outside `build/`.

## 2. `.coord/tools/codex-run.sh`: reviewer tier
Add role `reviewer`: model `gpt-6.1-sol`, effort `${CODEX_REVIEW_EFFORT:-medium}`, cap `${CODEX_MAX_REVIEWER:-10}`.
Failover reviewer -> backup. Update the usage comment at the top and the role error text. Keep everything else identical.
Check with `CODEX_DRY=1 bash .coord/tools/codex-run.sh reviewer C13-dev-loop-kit-review .coord/tasks/C13-dev-loop-kit.md`.

## 3. `CONTRACTS` selftest (`SelfTest/Contracts.cs`)
A `partial` part of `SelfTest` with one table of cross-feature behavior promises, run from one call line in `Run`, like
`LAYERS`. Each row: id (`<area>-<slug>`), the promise in one sentence, the incident it comes from, and a check delegate, or
`Manual` with the hand check to do. Reuse the existing check methods where they already prove the promise (call them; do not
duplicate their code). Start with these rows:
- `store-unavailable-save-noop` (A30): an unavailable store's `QueueSave`/`Save`/`Flush` succeed as a no-op with `LastError`
  set and write nothing.
- `store-folder-returns` (A30): a missing folder that comes back is saved to again; a save never makes the store unavailable.
- `startup-unavailable-store` (A43b): startup with a missing notes folder does not throw and keeps a valid selection.
- `fence-cache-bounds` (A28): a fence cache hit requires the same body bounds, not only the document version.
- `settings-changed-notifies` (A40): changing a setting raises the settings Changed event for its consumers.
- `window-style-bits` (A32/A34): the pad window's style flags are unchanged. Only if a pure, non-windowing check is possible
  without any product change; otherwise `Manual` ("pad keeps rounded corners and shadow in all three bar styles").
If a promise cannot be tested without changing product code, make it `Manual` and propose the seam in the report. Print one
line `CONTRACTS: <n> rows, <p> pass, <m> manual`; each failing row adds a failure `contract <id>: <detail>`.

## 4. `.github/workflows/gate.yml`
Name `gate`. Trigger `workflow_dispatch` only (the user decides later whether to add push). `windows-latest`,
`actions/checkout@v4` (fetch-depth 0), `actions/setup-dotnet@v4` with `dotnet-version: 10.0.x`, then
`pwsh -NoProfile -File tools/gate.ps1 -Ci`, then upload `.coord/reports/ci.gate.md` as an artifact (`actions/upload-artifact@v4`)
even on failure.

## Constraints
- Smallest change that meets the goal. No product behavior change, no refactor of existing selftests beyond the partial
  keyword and the call line.
- Never launch or kill AntiNotepad except the gate's own `--selftest` run into `build/`. Never run `--quit`.

## Done means
- `pwsh -NoProfile -File tools/gate.ps1 -Task C13-dev-loop-kit` exits 0, writes `.coord/reports/C13-dev-loop-kit.gate.md`,
  shows Build/Selftest/Seam PASS and the CONTRACTS line; its Scope step lists the manager's files as out of scope (expected:
  they are not in this brief's Files list) and that is only a WARN.
- A deliberately broken probe proves the gate fails: temporarily break one CONTRACTS row's expectation, see GATE: FAIL,
  restore it, see PASS. Say so in the report.
- The reviewer dry run prints a PLAN line with `role=reviewer model=gpt-6.1-sol`.
- Build 0 warnings, `SELFTEST PASS`.

## Before you report (director only)
Spawn the `lens_reviewer` agent on your diff (it reads `.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why
not. If the agent role is not available to you, say so at the top of the report and run the checklist yourself.

## Report: `.coord/reports/C13-dev-loop-kit.md`
Keep it short. Use exactly these sections:
1. **Summary**
2. **Change list**: `file:line-range | what changed | which goal line it serves`
3. **Behavior changes**: product behavior must be "None"; list tooling behavior here.
4. **Out-of-scope files**
5. **Contracts**: the rows, each pass/manual
6. **Lens review**: whether `lens_reviewer` was spawned, findings and what you did
7. **Checks run**
8. **Hand checks for the user**: "None (not visible)" unless something is.
