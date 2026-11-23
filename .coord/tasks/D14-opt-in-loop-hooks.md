# D14-opt-in-loop-hooks (Codex build in this worktree; do not commit)

Role: terra, small edits across seven scripts plus one doc, with hook semantics to respect. Sandbox workspace-write.
Inputs: `.claude/settings.json`, `tools/guard-bash.ps1`, `tools/loop-card.ps1`, `tools/title-hint.ps1`, `tools/worktree-check.ps1`, `tools/director-notes.ps1`, `tools/session-where.ps1`, `tools/status-line.ps1`, `.coord/reports/D13-setup-script-and-config-review.md` (hook findings), `docs/loop.md`, `docs/playbook.md`, `CONTRIBUTING.md`.
Do NOT read anything under `.coord/research/`. Do NOT edit `AGENTS.md`, `docs/loop.md`, product code, or `.claude/settings.json` structure beyond what is stated below.

## Owner decision (fixed)
Keep the hooks and document them, but make my loop-specific hooks OPT-IN so contributors who open the repo in Claude Code are not forced into my dev loop. In prose use first person ("I", "my"), never "the owner". Plain words, US spelling.

## The switch
- Environment variable `ANTINOTEPAD_LOOP`. Hooks do their work only when it equals `1`. Otherwise each script exits 0 immediately with NO output (a UserPromptSubmit or SessionStart hook's stdout is added to the model's context, so silence matters), and writes no files and starts no processes (loop-card must not start the roadmap server when off).
- Put the gate right after any `param(...)` / `[CmdletBinding()]` block (a `param` block must stay the first statement; comments before it are fine) and before any other work, including reading stdin is acceptable only if needed to avoid a broken pipe: read and discard stdin when the hook receives JSON, then exit 0.
- Keep `-SelfTest` runs of `guard-bash.ps1` and `title-hint.ps1` working with the variable unset (the gate must not block `-SelfTest`).
- `status-line.ps1` when off: print nothing and exit 0 (Claude Code then shows the default line). Check that an empty output from a statusLine command is harmless; say what you found.
- Add one shared tiny comment in each script: `# Opt-in: part of my dev loop. Set ANTINOTEPAD_LOOP=1 to enable (see docs/claude-hooks.md).`

## Write
1. Gate edits in the seven scripts.
2. `docs/claude-hooks.md`: one page. Why the hooks exist, how to turn them on (`ANTINOTEPAD_LOOP=1` in the user's Claude settings `env`, or in the shell), and a table with one row per hook: event, script, what it reads, what it writes or starts, what it prints, and what happens when the switch is off. Also list the non-hook project settings a contributor will notice (`outputStyle`, `attribution`, `worktree.baseRef`, the two `ask` rules) and how to override them in `.claude/settings.local.json`. Note that `.codex` and `.agents` are optional for contributors. Link the page from `CONTRIBUTING.md` under "Build and check" with one bullet, and add `ANTINOTEPAD_LOOP` guidance to `docs/playbook.md` in the smallest possible edit (one short note where hooks are first mentioned).
3. A report `.coord/reports/D14-opt-in-loop-hooks.md`: files changed, what you ran, results.

## Done means (offline)
- Each script parses (PowerShell parser).
- For each of the seven scripts run it twice with the sandbox's PowerShell: with `ANTINOTEPAD_LOOP` unset and with it `1`. Unset: exit code 0 and empty stdout (feed `{}` or a plausible JSON on stdin where the hook reads stdin). Set to 1: behavior matches before your change (compare against `git stash`-free means: read the diff; do not stash). Run `pwsh -NoProfile -File tools/guard-bash.ps1 -SelfTest` and `tools/title-hint.ps1 -SelfTest` with it unset and set; both must pass.
- Do not start the roadmap server, browsers or anything on a port. Do not touch the installed app or any data folder.
- No home paths or private repo names in new text.
- Final message: files changed, results table (script, unset result, set result), open items for me.
