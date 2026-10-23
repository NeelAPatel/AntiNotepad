# A4-tooling-tidy

- `tools/press-hotkey.ps1` validates `-Key` as exactly one ASCII letter or digit before sending input. Key-up events for the key and Ctrl/Alt/Shift now run in `finally`. The hotkey script was not run.
- The worker effort is now `gpt-6-luna xhigh` in `.coord/tools/codex-run.sh` and `docs/agents.md`; other tiers and caps are unchanged.

## Checks

- PowerShell parser: passed with no parse errors.
- `bash -n .coord/tools/codex-run.sh`: unable to run. Git Bash exits before reading the file with `CreateFileMapping ... Win32 error 5`; WSL is also denied by the environment.
- `CODEX_DRY=1 bash .coord/tools/codex-run.sh worker X /dev/null`: unable to reach the wrapper for the same Git Bash startup error, so no PLAN line was emitted. Expected from the edited tier and empty usage ledger (not observed): `PLAN: role=worker model=gpt-6-luna effort=xhigh run=1/10 workdir=<repo-root> task=/dev/null log=<repo-root>/.coord/logs/X.worker.log report=<repo-root>/.coord/reports/X.worker.codex.md timeout=45m`.
