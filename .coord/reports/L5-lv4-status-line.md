# L5-lv4-status-line

## Changed
- Added `tools/status-line.ps1` for branch, item, worktree, context percentage, cost, and in-flight IDs.
- Added `tools/session-where.ps1` for current item phase, next action, and STATUS state.
- Removed the reminder line from `tools/loop-card.ps1`; the in-flight and over-cap lines and roadmap auto-start remain.
- Added the status line command and `startup|resume` SessionStart hook in `.claude/settings.json`.

## Checks run
- PowerShell parser: PASS for `tools/status-line.ps1`, `tools/session-where.ps1`, and `tools/loop-card.ps1`.
- `settings.json` `ConvertFrom-Json`: PASS.
- Status line command and output, run in this worktree:
  - `'{"cwd":"<worktree>","context_window":{"used_percentage":42.4},"cost":{"total_cost_usd":1.234},"model":{"display_name":"Claude"}}' | pwsh -NoProfile -File tools/status-line.ps1`
  - `chore/L5-lv4-status-line | L5-lv4-status-line | WT | ctx 42% | $1.23`
  - `in flight: L5-lv4-status-line, L6-lv4-docs-diet, L7-lv4-playbook`
  - With null context and cost:
    `chore/L5-lv4-status-line | L5-lv4-status-line | WT`
    `in flight: L5-lv4-status-line, L6-lv4-docs-diet, L7-lv4-playbook`
  - With omitted context and cost: same output as null fields.
  - Empty input: `-` (exit 0).
- `tools/session-where.ps1` output:
  - `Item: L5-lv4-status-line`
  - `Phase: Approve (STATUS: planned)`
  - `Next: build with Codex`
  - `Card: .coord/tasks/L5-lv4-status-line.md`
- Empty-input `tools/loop-card.ps1`: in-flight line present; 279 output characters (before: 487).
- Status-line runtime: 10 process launches averaged 358.3 ms.
- `tools/gate.ps1 -Task L5-lv4-status-line -Offline`: `GATE: PASS`; build 0 warnings/errors, selftest PASS, seam PASS, tokens PASS, format PASS, scope PASS, loop lint PASS, size PASS. Offline NuGet audit warning only.

## Open questions
- The status-line process averaged 358.3 ms across 10 invocations, above the 300 ms target. This includes launching `pwsh` for each measurement; the hook process behavior in Claude Code has not been measured.
- Output follows the approved examples. In-flight list contains the three currently building Level 4 items.


## Rework r2

### Changed
- `tools/status-line.ps1` now forwards the raw stdin JSON to `$HOME/.claude/statusline-command.mjs` through `node` when available, then adds one `loop:` line with worktree marker, branch, item, and in-flight IDs. `STATUSLINE_GLOBAL` overrides the global script path for local checks.
- Removed context, cost, and model details from the loop line. Global-script failure or missing path is silent; status-line errors exit 0.

### Checks run
- PowerShell parser: PASS for `tools/status-line.ps1`.
- Worktree mock command: `$mockJson | pwsh -NoProfile -File tools/status-line.ps1`
  - `GLOBAL {"cost":{"total_cost_usd":1.23},"context_window":{"used_percentage":42},"cwd":"F:\\LocalVSProj\\Visual Studio Apps\\AntiQuickPad\\.claude\\worktrees\\chore\\L5-lv4-status-line"}`
  - `loop: WT | chore/L5-lv4-status-line | item L5-lv4-status-line | in flight L5-lv4-status-line, L6-lv4-docs-diet, L7-lv4-playbook`
- Main-checkout mock command: `$mainJson | pwsh -NoProfile -File tools/status-line.ps1`; global output was forwarded. Output: `GLOBAL`, then `loop: dev | in flight L5-lv4-status-line, L6-lv4-docs-diet, L7-lv4-playbook`. The main checkout currently has these items in flight.
- Empty stdin: global script received empty stdin and printed `GLOBAL`; loop line reflected the default script-root worktree.
- Missing global script command: `$mockJson | pwsh -NoProfile -File tools/status-line.ps1` with `STATUSLINE_GLOBAL` set to a nonexistent file. Output was only the `loop:` line above; exit 0.
- Timed 10 fresh `pwsh` invocations with the mock global script: 427.7, 430.0, 433.6, 429.9, 449.2, 454.4, 486.4, 441.4, 419.4, 409.3 ms; average 438.1 ms.
- `tools/gate.ps1 -Task L5-lv4-status-line -Offline`: `GATE: PASS` (build, selftest, seam, tokens, format, scope, loop lint, size).

### Open questions
- The live Claude status-line render and ANSI appearance remain unverified; the mock wrapper verified stdin forwarding and output composition.
- Runtime above includes a fresh `pwsh` launch and a local mock Node script. Claude Code's status-line scheduling was not measured.

