# A17-tool-script-checks

## Changes

- `.coord/tools/codex-run.sh`: validates the task name before creating directories or touching the usage ledger. The accepted pattern is:

  ```regex
  ^[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+$
  ```

  It requires an uppercase ASCII ID start and one or more hyphen-separated lowercase ASCII slug segments. Invalid input prints an error and exits 2.
- `tools/press-hotkey.ps1`: uses `$Key.ToUpperInvariant()` to avoid culture-dependent key conversion.

## Existing task-name inventory

The pattern was applied to every non-underscore file name in `.coord/tasks` (with `.md` removed). All 18 names passed; none failed:

```text
A1-codebase-audit
A10-ocr-repeat-and-target
A11-storage-settings-tab
A12-ocr-always-snip-and-lists
A13-timer-max-and-currency-error
A2-dead-code-tidy
A3-runtime-tidy
A4-tooling-tidy
A5-audit-refresh
A7-notes-folder-and-recovery
A8-save-failure-quit-and-x64
A9-input-limits-and-timers
I6-win11-menu-toolbar-in-app
I7-menu-corner-artifacts
I7b-menu-corners-dwm
N1-antinotepad-identity
N2-canonical-install-and-dev-badge
N3-release-and-update-plan
```

`.coord/logs/usage.tsv` and `.coord/logs/` were absent in this checkout, so there were no column-5 ledger task names to test.

## Checks and limits

- PowerShell parser check for `tools/press-hotkey.ps1`: passed. The script was not executed because it sends real keystrokes.
- Regex check: `A17-tool-script-checks` passed; `../evil` was rejected.
- `bash -n .coord/tools/codex-run.sh` and both requested dry-run invocations were attempted, but Git Bash could not start. It failed with `CreateFileMapping ... Win32 error 5` before parsing or executing the script. The WSL fallback also reported access denied while enumerating distributions. Therefore Bash syntax and dry-run output/rejection are unverified in this environment. The failed attempts created no `.coord/logs` path.
- By inspection, a successful `CODEX_DRY=1` path returns before the ledger append, so it adds no ledger line. The launcher currently creates `.coord/logs` and touches `usage.tsv` before reaching that dry-run return; a valid dry run would leave an empty ledger file. No build or selftest was needed for this task.
- No visual/manual product check was identified. Runtime hotkey dispatch remains unverified because the helper was not run, as instructed.
