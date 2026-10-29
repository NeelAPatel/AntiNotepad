# C25-dev-instance-handoff: launching the dev copy from any checkout replaces whatever dev copy is running

Role: director (gpt-6.1-sol medium). Repo tooling; product `product/AntiNotepad` is not changed.
Branch: `feat/C25-dev-instance-handoff` (already checked out; never switch, merge or commit: you cannot write to `.git`).
Tier: Medium (rules: `docs/loop.md`; tooling, but it stops processes)
Read first: `AGENTS.md`, `docs/dev-loop.md`, then the files below. Line numbers are from the current tree; re-read before editing.

## Goal
User: "when we work on these A##, it does not seem to be aware another instance, possibly from another worktree's dev build
is running. We need to ensure that when some task/change is completed, it launches ITS version automatically instead of me
wasting cycles on prompting it to close old ones and restart."

Cause (manager's reading, confirm it): every dev run uses the same single-instance id, `AppInfo.DevInstanceId`
(`AntiNotepad-dev`), chosen in `Program.cs:41-49` whenever `ANTINOTEPAD_DATA` is set. But `tools/dev.ps1` only tracks the
process it started itself, in its own checkout's `build/dev.pid`. A worktree's `dev.ps1` sees no record, builds, launches; the
new process finds the dev mutex taken, signals the running (older) dev copy to show, and exits. The user sees the old build.

Wanted:
1. `tools/dev.ps1` (and `tools/dev.sh`, kept in parity) closes any running dev copy, whichever checkout started it, then
   launches this checkout's build and confirms the launched process is the primary instance (still running after a short
   wait, image path = this checkout's `build/dev/AntiNotepad.exe`). Fail loudly if the old copy cannot be closed.
2. Preferred mechanism, if it holds: after the build, run this checkout's freshly built exe with `--quit` and the dev env
   (`ANTINOTEPAD_DEV=1`, `ANTINOTEPAD_DATA` set to this checkout's `build/testdata`), which signals whoever holds the
   `AntiNotepad-dev` mutex to quit gracefully (it saves its notes); then wait until the dev mutex is free (bounded, about 5 s).
   Only if that fails: a machine-wide record of the running dev copy (pid, exe path, start time), kept in the MAIN checkout's
   `build/` (resolve it with `git rev-parse --git-common-dir`), verified the same way the current `build/dev.pid` record is
   verified before any kill. Keep the per-checkout record logic working or replace it; no orphaned records.
3. The installed copy must never be touched: `--quit` without `ANTINOTEPAD_DATA` targets it (`docs/dev-loop.md`), so the
   script must set `ANTINOTEPAD_DATA` on every `--quit` call and never kill a process whose image is not a repo
   `build/dev/AntiNotepad.exe`.
4. Loop docs: the implementation sub-session starts its own dev copy (`tools/dev.ps1` from the worktree) right after the gate
   passes, so the build the user is asked to hand test is already open. Update the rows/lines that say how the hand test is
   run.

Must not change: product code, selftest, data cleanup behavior of `-Fresh`, the dev hotkey, the dev data folder per checkout.

## Files
Only these files may change. Touching any other file needs a line under "Out-of-scope files" in the report.
- `tools/dev.ps1`
- `tools/dev.sh`
- `docs/dev-loop.md`
- `docs/loop.md`
- `.claude/skills/next-task/SKILL.md`
- `.claude/skills/review-task/SKILL.md`

## Contracts
None of the product `CONTRACTS` rows change. New promise (documented in `docs/dev-loop.md`, not a selftest row): running
`tools/dev.ps1` from any checkout leaves exactly one dev copy running, and it is this checkout's build.

## Constraints
- Smallest change that meets the goal. No drive-by refactors of the script's other parts.
- Never launch or kill AntiNotepad yourself from the Codex sandbox, and never run `--quit` (sandboxed process control is
  unreliable and the installed copy must not be touched). Verify by reading and with `pwsh -NoProfile -Command` parse checks
  (`[System.Management.Automation.Language.Parser]::ParseFile`) and `bash -n tools/dev.sh`. The manager runs the live check.
- Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task C25-dev-instance-handoff -Offline` inside the sandbox.

## Done means
- From worktree B, with a dev copy from checkout A running: `tools/dev.ps1` closes A's copy and B's build is the one running
  (its badge/log path proves it). Run twice from the same checkout: still one copy, the newest build.
- With no dev copy running: launches normally. The installed copy is untouched in every case.
- Docs say the sub-session launches the dev copy when the gate passes.
- Gate passes.

## Before you report (director only)
Spawn the `lens_reviewer` agent on your diff (it reads `.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why
not.

## Report: `.coord/reports/C25-dev-instance-handoff.md`
Keep it short. Use exactly these sections:
1. **Summary**: two or three sentences, including the confirmed cause.
2. **Change list**: one row per changed block: `file:line-range | what changed | which goal line it serves`.
3. **Behavior changes**: every changed script behavior, file location, timing or safety check. "None" only if true.
4. **Out-of-scope files**: file and why, or "None".
5. **Contracts**: rows added or changed, or "None".
6. **Lens review**: findings and what you did with each.
7. **Checks run**: parse checks, gate result lines.
8. **Hand checks for the user**: the live two-checkout check, step by step.
