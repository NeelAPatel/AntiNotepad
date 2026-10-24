Implemented the six approved findings and added `InputLimitChecks`. Timer keys exclude labels, and stale or completed entries are pruned while the done chip remains available for current lines.

- Release build: **0 warnings, 0 errors** with `NuGetAudit=false`. Without it, restore emitted `NU1900` because nuget.org was unreachable.
- `--selftest`: **SELFTEST PASS**; `git diff --check` passed.
- `build/a9-check` remains because the execution policy rejected recursive deletion. AntiNotepad was not launched or killed.

The per-finding details are in the [A9 report](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad-wt/A9-input-limits-and-timers/.coord/reports/A9-input-limits-and-timers.md>). No commit was made.