# C13-dev-loop-kit-r2: gate must never pass without a selftest pass; offline switch; quieter size warning

Role: worker (gpt-6-luna xhigh). Do the work yourself. Product `product/AntiNotepad`.
Branch: `feat/C13-dev-loop-kit` (already checked out, all of C13 uncommitted; never switch, merge or commit).
Tier: Medium
Read first: `.coord/tasks/C13-dev-loop-kit.md`, `.coord/reports/C13-dev-loop-kit.md`, `.coord/reports/C13-dev-loop-kit.review.md`.

## Goal
Fix the one proven review finding and two loop frictions in `tools/gate.ps1`. Nothing else changes.

## Files
Only these files may change. Touching any other file needs a line under "Out-of-scope files" in the report.
- `tools/gate.ps1`
- `.coord/templates/brief.md` (only the one Constraints line in item 2)

## Contracts
None (tooling only).

## To do
1. **Review finding (major)**: `tools/gate.ps1:307`: a successful build with no `AntiNotepad.exe` gives Selftest SKIP and
   GATE PASS. Gate success must require Selftest PASS. A missing executable after a successful build is Selftest FAIL.
   SKIP after an already-failed build stays as is. Prove it with the reviewer's probe (process-local `UseAppHost=false`):
   GATE FAIL, exit 1.
2. **`-Offline` switch**: the Codex sandbox has no network, so NuGet audit warning NU1900 fails the build there. Add
   `-Offline`, which sets `NuGetAudit=false` for the build only and adds a WARN step line `Offline: NuGet audit off (sandbox)`
   to the report, so the manager's online gate stays the one that counts. Without `-Offline` nothing changes. In
   `.coord/templates/brief.md` Constraints, add one line: inside the Codex sandbox run the gate as
   `pwsh -NoProfile -File tools/gate.ps1 -Task <ID-slug> -Offline`; the manager re-runs it online.
3. **Size warning noise**: today any growth of a file already over `-SizeWarn` warns, so every task touching `SelfTest.cs`
   (2,646 lines) warns. Warn only when a file crosses `-SizeWarn` in this diff, or grows by more than `-SizeGrowth` lines
   (new parameter, default 25) while already over it. Keep the old -> new counts in the line.

## Done means
- The UseAppHost=false probe gives GATE FAIL, exit 1; the normal gate (with `-Offline` inside the sandbox) gives GATE PASS.
- `-Offline` shows its WARN line; without it, no NuGetAudit change.
- SelfTest.cs at +1 line no longer warns; a probe of +30 lines on an over-limit file does (restore it after).
- Build 0 warnings, `SELFTEST PASS`.

## Report: `.coord/reports/C13-dev-loop-kit-r2.md`
Sections: Summary, Change list, Behavior changes, Out-of-scope files, Checks run.
