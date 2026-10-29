# C25-dev-instance-handoff-audit: read-only audit of the C25 brief

Role: director (gpt-6.1-sol medium). Read-only: no edits anywhere except your report.
Branch: `feat/C25-dev-instance-handoff` (checked out; never switch or commit).
Brief under audit: `.coord/tasks/C25-dev-instance-handoff.md`. Read `AGENTS.md`, `docs/dev-loop.md`, `docs/loop.md`.

## Do
1. Confirm or correct the stated cause: `Program.cs` instance id selection, `Platform/SingleInstance.cs` (mutex, show and
   quit signals, `RequestQuit`), and `tools/dev.ps1` / `tools/dev.sh` PID handling. Is a graceful `--quit` with the dev env a
   reliable way to close any running dev copy? What does `RequestQuit` return and how long does quit take (saves)?
2. Confirm or correct the Files list and the Medium tier.
3. List every way the change could touch the user's installed copy, and the guard for each.
4. Riskiest parts (races between quit and relaunch, a hung dev copy, two scripts run at once, `dev.sh` parity, worktrees
   whose `build/` differs) and the hand checks.
5. "Recommendation on handling": scope, split or not, and the mechanism you recommend.

## Report: `.coord/reports/C25-dev-instance-handoff-audit.md`
Sections: Cause, Files and tier, Installed-copy safety, Risks, Hand checks, Recommendation on handling. Keep it short.
