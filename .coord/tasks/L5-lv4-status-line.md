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
- `tools/session-where.ps1` (new; replaces the plan's director-notes edit, so the compact hook stays untouched)
- `.claude/settings.json` (two added entries: `statusLine`, and a `SessionStart` matcher `startup|resume` running session-where)

## Contracts
None.

## Approve
User, 2026-11-08, in the L5 chat: "plan is approved, start the worktree, branch and build this out." Debrief after build,
then wait for corrections or a green light to prep for merge.

## For Codex
Role: quick (gpt-6-luna medium), three small PowerShell scripts and two settings entries. Offline checks only; never run
the app, never start a server or open a port. Do not commit. Edit only the files under `## Files`. Follow `docs/coding.md`
(comment header, formatting). ASCII-only output from hook scripts (console code page).

Build:
1. `tools/status-line.ps1`: reads Claude Code status-line JSON from stdin (fields: `cwd`, `workspace.current_dir`,
   `context_window.used_percentage` (may be null), `cost.total_cost_usd`, `model.display_name`). Use ONE git call
   (`git -C <cwd> rev-parse --abbrev-ref HEAD --git-dir --git-common-dir`) to get branch and worktree state; a worktree is
   when git-dir differs from git-common-dir. Item = branch text after the first `/` (`chore/L5-x` -> `L5-x`), else the leaf of
   a `.claude/worktrees/...` cwd, else empty. Line 1: `branch | item | WT | ctx 42% | $1.23` (omit parts that are absent;
   `WT` only in a worktree). Line 2: `in flight: <IDs>` from STATUS, read from the MAIN checkout (git-common-dir parent),
   using the same Plan-table parsing as `tools/loop-card.ps1` (state not in done, dropped, planned, needs you, proposed, paused,
   -), item ids only, `none` when empty. Light ANSI color is optional. Any error: print the plain branch (or `-`) and exit 0.
   No network, no process start. Must be fast (target under 300 ms).
2. `tools/session-where.ps1`: SessionStart hook. From the current branch (git, from `$env:CLAUDE_PROJECT_DIR` or script
   root) derive the item slug; read the card `.coord/tasks/<slug>.md` in the current checkout, falling back to the main
   checkout. Phase = the last of these headings that has non-empty body text: Request, Findings, Plan, Approve, Build,
   Check, Hand test, Merge. Next action by phase: Request/Findings -> write the Plan; Plan -> wait for user approval;
   Approve -> build with Codex; Build -> run the gate and Check; Check -> hand test; Hand test -> wait for merge; Merge -> done.
   Also print the STATUS row state for the item if found. Output at most 4 plain lines: `Item: <slug>`, `Phase: <phase>`,
   `Next: <action>`, `Card: .coord/tasks/<slug>.md`. On `dev`, `main` or an unknown branch, print nothing. Exit 0 always.
3. `tools/loop-card.ps1`: remove only the long reminder line (the output style already carries it) and its header-comment
   mention. Keep the in-flight line, the over-cap warning and the roadmap auto-start unchanged. Update the file's header
   comment to match.
4. `.claude/settings.json`: add exactly `"statusLine": {"type": "command", "command": "pwsh -NoProfile -File \"$CLAUDE_PROJECT_DIR/tools/status-line.ps1\""}`
   and one new `SessionStart` entry (matcher `startup|resume`, command `pwsh -NoProfile -File "$CLAUDE_PROJECT_DIR/tools/session-where.ps1"`,
   timeout 15) beside the existing ones. Touch nothing else; keep the file valid JSON.

Done means:
- Each script parses (`[System.Management.Automation.Language.Parser]::ParseFile`); `settings.json` parses (`ConvertFrom-Json`).
- `status-line.ps1` fed mock JSON on stdin (with and without context/cost, null percentage, empty input) prints the expected
  lines and exits 0, run from this worktree; copy the commands and output into the report.
- `session-where.ps1` run in this worktree prints the four lines for `L5-lv4-status-line`.
- `loop-card.ps1` still prints the in-flight line (run with empty stdin; report the character count before 487 and after).
- `tools/gate.ps1 -Task L5-lv4-status-line -Offline` passes.
- Report at `.coord/reports/L5-lv4-status-line.md` with sections: Changed, Checks run, Open questions.

## Check
Codex quick (Luna medium) built it; report `.coord/reports/L5-lv4-status-line.md`. Gate `-Offline`: GATE PASS.
Verified (run here, 2026-11-08):
- Status line with mock JSON in the worktree (`chore/... | item | WT | ctx 42% | $1.23` plus an in-flight line), with null context and cost, and with empty input (prints `-`, exit 0); in the main checkout it prints `dev` and the in-flight line, no `WT`.
- `session-where` prints item, phase, next action and card path in the worktree; prints nothing on `dev`.
- Per-turn hook text: 487 -> 279 characters (-43%), in-flight line, cap warning and roadmap auto-start kept.
- Speed: bare `pwsh` start 264 ms; status-line 358 ms (Codex) and 473 ms (me, machine busy with other sessions). Claude runs it in the background after each message, so it never blocks typing.
- Docs (statusline.md, 2026-11-08): fields, Windows support, refresh events, multi-line output.
Assumed, not verified: the live Claude Code render (needs a restart of a session in this branch); `$CLAUDE_PROJECT_DIR` expands in the `statusLine` command (the other hooks rely on it, docs do not say); workspace trust must be accepted or the line stays blank; the `STATUS: planned` text in `session-where` reads the main checkout's STATUS, which can lag the branch.
