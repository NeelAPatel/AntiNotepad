# L5-lv4-status-line: a free status line, a smaller per-turn hook, and a SessionStart "where am I"

Role: sub-session (Sonnet medium). Codex role: Luna medium. Tier: Low. Expected time: about 45 minutes.
Branch: `chore/L5-lv4-status-line` (created at Approve). Shared brief: `.coord/research/lv4-shared-brief.md`.

## Request
Loop Hub 2 (2026-11-08): Level 4 item 5 of 7. Project status line (branch, item, in-flight items, context %, cost, worktree
marker) at zero model tokens; trim the per-turn hook; SessionStart prints phase and next action for the current branch.

## Findings
Verified against https://code.claude.com/docs/en/statusline.md (2026-11-08):
- `statusLine` = `{type:"command", command, padding?, refreshInterval?}`; runs locally, no tokens; refresh on each assistant
  message, /compact, mode change, debounced 300 ms; a new trigger cancels a slow run; output shows only on exit 0.
- Windows: commands run via Git Bash if installed, else PowerShell; use forward slashes; script file recommended.
- stdin JSON: `cwd`, `workspace.current_dir/project_dir/git_worktree`, `worktree.name/path/branch` (Claude-made worktree
  sessions only), `model.display_name`, `context_window.used_percentage` (may be null early), `cost.total_cost_usd`,
  `session_name`, `output_style.name`. Multi-line and ANSI colors work.
- Workspace trust must be accepted or the line stays blank.
Not verified: a numeric timeout; whether `worktree.*` appears for `.claude/worktrees/` checkouts made by approve.ps1 (docs tie
it to Claude worktree sessions; `workspace.git_worktree` should still be set). Plan: derive branch and worktree marker
from `git`/`cwd` and use the JSON fields only as extras.
Measured today (before): the UserPromptSubmit hook prints 487 chars (about 120 tokens) every turn and takes about 557 ms.
Of that, the reminder line is about 300 chars and duplicates the output style; the in-flight line is the rest.

## Plan
1. `tools/status-line.ps1`: reads stdin JSON; line 1 = branch, item (from `<prefix>/<ID-slug>` or worktree path), worktree
   marker, context %, cost; line 2 = in-flight items from STATUS (short ids). No network, no roadmap start. Fail-safe: any
   error prints a plain branch name and exits 0.
2. `.claude/settings.json`: add `statusLine` (`pwsh -NoProfile -File "$CLAUDE_PROJECT_DIR/tools/status-line.ps1"`).
3. `tools/loop-card.ps1`: drop the reminder line (output style covers it); keep roadmap auto-start, the in-flight line
   (the status line shows it too but only on screen, not to the model) and the over-cap warning.
4. `tools/director-notes.ps1` plus a `SessionStart` entry for startup and resume (no matcher change for `compact`): print
   phase and next action for the current branch from `.coord/tasks/<ID-slug>.md` (last filled section) and the STATUS row.
5. Gates: `tools/gate.ps1 -Task L5-lv4-status-line -Offline`; render test in main and in a worktree with mock JSON; time
   10 runs; measure per-turn hook text before and after.

## Files
- `tools/status-line.ps1` (new)
- `tools/loop-card.ps1`
- `tools/director-notes.ps1`
- `.claude/settings.json` (small edit; expect a rebase against L1-L4)

## Contracts
None.

## Approve
User, 2026-11-08, in the L5 chat: "plan is approved, start the worktree, branch and build this out." Debrief after build,
then wait for corrections or a green light to prep for merge.
