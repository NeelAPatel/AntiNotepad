# L1-lv4-native-guards

## Summary

Implemented the offline guard in log mode. Hook installation and ask rules remain with the manager.

## Change list

- `tools/guard-bash.ps1`: merge and installed-copy checks, fail-open logging, block mode and self-tests.
- `tools/guard.mode`: defaults to `log`.

## Checks run

- PowerShell 7 and Windows PowerShell 5.1: `-SelfTest` passed, including temporary log and block mode checks.
- Hook JSON in this worktree: `git merge x` and `git push origin HEAD:dev` exited 0 and logged merge BLOCK; `echo hi` exited 0 without a log entry.
- `tools/gate.ps1 -Task L1-lv4-native-guards -Offline`: PASS, including scope and loop lint.

## Open questions

- Claude's hook shell, blocking exit behavior and ask prompts still need the planned live hand test after the manager installs the settings.
- The installed-copy check intentionally flags `--quit` even in quoted commit messages and echo text. Shell parsing is deliberately limited; errors fail open and nested shell commands are not inspected.
