# A4-tooling-tidy

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/A4-tooling-tidy`. Never switch branch,
merge or commit (.git is read-only for you). Context: `docs/audit/A1-codebase-audit.md`, findings A1-18 and A1-26.

## Goal
1. **A1-18-hotkey-helper-held-modifiers** (`tools/press-hotkey.ps1`): validate `-Key` before any key goes down. Accept
   exactly one letter A-Z or digit 0-9; otherwise throw a clear error without pressing anything. Wrap the press in
   `try/finally` so the key and the Ctrl/Alt/Shift releases always run. Default `D` and behavior for valid keys are unchanged.
   Don't run the script: it sends real system-wide input.
2. **A1-26-worker-effort-mismatch**: AGENTS.md is the policy, so workers are `gpt-6-luna xhigh`. Update `.coord/tools/codex-run.sh`
   (the `worker)` tier line: `high` -> `xhigh`) and the worker row in `docs/agents.md` to match. Leave the director and
   backup tiers, caps and everything else alone. Check with `CODEX_DRY=1 bash .coord/tools/codex-run.sh worker X /dev/null`
   that the PLAN line shows `effort=xhigh`. The dry run writes nothing but may create `.coord/logs`, which is gitignored.
Files in scope: `tools/press-hotkey.ps1`, `.coord/tools/codex-run.sh`, `docs/agents.md`. Nothing else.

## Done means
- `pwsh -NoProfile -Command "$null = [System.Management.Automation.Language.Parser]::ParseFile('tools/press-hotkey.ps1', [ref]$null, [ref]$e); $e"`
  prints no parse errors, and `bash -n .coord/tools/codex-run.sh` passes.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Write a short report at `.coord/reports/A4-tooling-tidy.md` covering the changes, the dry-run PLAN line and the parse-check
  results.
