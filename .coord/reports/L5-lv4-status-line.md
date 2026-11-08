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

