# D13-setup-script-and-config-review (Codex build in this worktree; do not commit)

Role: terra, a script plus a read-and-report review. Sandbox workspace-write.
Inputs: `README.md` (Build from source), `CONTRIBUTING.md` (Build and check), `docs/dev-loop.md`, `tools/dev.ps1`, `tools/dev.sh`, `tools/install.ps1`, `.github/workflows/*`, `.claude/settings.json`, `.claude/rules/*`, `.codex/**`, `.agents/**`, `.gitignore`, `docs/releasing.md`.
Do NOT read or cite anything under `.coord/research/`.

## Owner decisions (fixed)
- Public refs `main` and `dev`. A stranger must be able to clone and build and run easily, especially on `dev`. Windows 11, .NET 10 SDK, PowerShell 7.
- Never touch the installed copy (`%LOCALAPPDATA%\Programs\AntiNotepad`) or real data. Never run `AntiNotepad.exe --quit` without `ANTINOTEPAD_DATA` set. The script must set `ANTINOTEPAD_DATA` to a disposable folder for self-tests and any dev run.
- First person voice in prose ("I"), never "the owner". US spelling, plain words.

## Write
1. `tools/setup.ps1`: one script from a fresh clone to a verified build. Steps with clear messages: check Windows, check `dotnet --version` is 10.x (print the install link if not), check PowerShell 7, `dotnet restore`, `dotnet build product/AntiNotepad -c Debug -o build/setup-check`, run `--selftest` with `ANTINOTEPAD_DATA` pointing at a temp folder, print `SELFTEST PASS` result and the next steps (`tools/dev.ps1` for an isolated dev copy, CONTRIBUTING). Parameter `-SkipSelfTest`. Set `AVALONIA_TELEMETRY_OPTOUT=1`. Exit non-zero with a clear reason on any failure. No network use beyond `dotnet restore`.
2. Update ONLY these existing docs, minimally: `README.md` "Build from source" and `CONTRIBUTING.md` "Build and check" to mention `pwsh -NoProfile -File tools/setup.ps1` as the easy path (keep the manual commands underneath).
3. A report `.coord/reports/D13-setup-script-and-config-review.md` with the review below. Do not edit the files you review.

## Review (read-only; report findings)
- Every file in `.github/workflows/` (including the new `release.yml`): secrets or tokens, permissions, triggers a forked PR could abuse (`pull_request_target`, untrusted input into `run:`), unpinned third-party actions, private repo names, home paths. Say what is safe for a public repo and what must change.
- `.claude/settings.json`, `.claude/rules`, `.codex/**`, `.agents/**`: anything personal or machine-specific (paths, usernames, hooks that call local tools, allowed commands that expose private areas), hooks that would surprise a contributor who opens the repo in Claude Code or Codex. Say what stays, what to scrub, what to document.
- `.gitignore`: confirm `.openchamber/`, `.claude/settings.local.json`, `.coord/logs/` and `build/` are ignored.
- A table: file, finding, severity (must fix / should fix / fine), suggested change.

## Done means (offline)
- `pwsh -NoProfile -File tools/setup.ps1 -SkipSelfTest` parses (use the PowerShell parser); do not run restore or builds if the sandbox blocks NuGet; say so. Claude runs the full script afterward.
- No home paths or private repo names in new text. Never run port, server or browser checks.
- Final message: files written, review table summary, items for the owner.
