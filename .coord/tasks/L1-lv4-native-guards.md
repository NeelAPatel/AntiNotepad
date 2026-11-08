# L1-lv4-native-guards: hooks and ask rules that turn loop rules into walls

Item card (`docs/loop.md`). Level 4 item 1 of 7; shared brief `.coord/research/lv4-shared-brief.md`.

Tier: Medium · config and scripts only, no app code
Branch: `chore/L1-lv4-native-guards`

## Request
Loop Hub 2, 2026-11-08: project hooks and permission rules in `.claude/settings.json` plus small scripts under `tools/`
that enforce (a) merge guard, (b) installed-copy wall, (c) loop-file protection. Log-only first, one-line switch to blocking.

## Findings
- `.claude/settings.json` has two hooks today (`loop-card.ps1`, `director-notes.ps1`); no `permissions` block.
- Worktrees live in `.claude/worktrees/<prefix>/<slug>`; `build/` and `.claude/worktrees/` are git-ignored.
- Docs claims (hook `shell` field, exit 2, Edit rules scope) were checked by a helper on the live docs: see Verified below.

## Plan
- **After:**
  - `tools/guard-bash.ps1`: reads the hook JSON on stdin, parses the command (strips `git -C <dir>` and `-c k=v`,
    splits on `;` `&&` `||` `|`), and checks two things.
    - Merge guard, matched to the v5.2.0 merge policy (`merge-task`, `docs/loop.md`). It looks at where the command runs
      (linked worktree = git dir differs from common dir) and which ref it targets, never at `--no-ff`:
      - Item worktree: ALLOW `git push` / `--force-with-lease` of the item's own branch (bare push, or a refspec naming
        `chore/<ID-slug>` or the current branch), `git rebase`, `git commit`. BLOCK `git merge`, `git checkout|switch`
        of `main` or `dev`, and any push whose target is `dev` or `main` (`HEAD:dev`, `--all`, `--mirror`, deletes).
      - Main checkout on dev: ALLOW `git merge --no-ff [--no-commit]`, `git merge --abort`, `git push origin dev`,
        `git push origin --delete <item branch>`. BLOCK `git rebase` and any force push (`--force`, `-f`,
        `--force-with-lease`); BLOCK push of `main` (releases only, on the user's go).
      - Checked against `merge-task` steps 0-7 and `catch-up.ps1`: the scripts run their git calls inside pwsh, so the
        hook sees only the outer command. Edge: a `-NoWorktree` item runs in the main checkout on a non-dev branch;
        the guard only judges the main checkout when it is on dev, otherwise it logs "unknown context" and allows.
    - Installed-copy wall: command contains `--quit`, `Programs\AntiNotepad` or `%LOCALAPPDATA%\AntiNotepad` and no
      `ANTINOTEPAD_DATA` in the command or environment.
  - Mode lives in `tools/guard.mode` (one word: `log` or `block`). `log` writes a line to `build/guard.log` and exits 0;
    `block` also prints the reason to stderr and exits 2. Undo: delete the hook entry (one line group) in settings.
  - `permissions.ask` for `Edit`/`Write` on `.coord/STATUS.md`, `.coord/lessons.md`, `docs/loop.md`. Ask-only, never deny.
- **Expected time:** about 45 minutes with Codex (Terra medium for the script).
- **Must not change:** the two existing hooks, app code, gate scripts, STATUS.
- **Test plan, code:** `tools/guard-bash.ps1 -SelfTest` feeds sample JSON and checks log lines and exit codes. Cases:
  worktree: `git push --force-with-lease origin chore/X` allow, `git -C . push origin HEAD:dev` block, `git merge x` block,
  `git switch main` block, `git rebase dev` allow; main on dev: `git merge --no-ff --no-commit b` allow, `git merge --abort`
  allow, `git push origin dev` allow, `git rebase dev` block, `git push --force origin dev` block, `git push origin main`
  block; `--quit` with and without `ANTINOTEPAD_DATA`; installed path with and without it.
- **Test plan, live:** in the worktree session, run a harmless command that trips each guard in log mode; then flip to
  `block` once on a throwaway command (`git merge --abort` inside the worktree), confirm the block, flip back.
- **Gates:** `tools/gate.ps1 -Task L1-lv4-native-guards -Offline`: scope, loop-lint.
- **Files touched that siblings also touch:** `.claude/settings.json` (L2, L4, L5 expected); keep the diff to one added
  `PreToolUse` group and one `permissions` block.
- **Needs from you:** approval of this plan; a hand test of the three guards.

## Verified vs assumed
Final table filled at Check. Read in the live docs 2026-11-08 (code.claude.com/docs/en/hooks, /permissions), not yet run here:
- `"shell": "powershell"` is accepted; default here is bash because Git Bash is installed, so set it explicitly.
- Exit 2 on `PreToolUse` blocks the call and sends stderr to Claude. Stdin JSON has `cwd`, `tool_name`, `tool_input.command`.
- `Edit(path)` rules cover the built-in edit tools plus Bash redirects and `tee`; other Bash writes (`sed -i`, `cp`) are not covered.
- Hooks also fire inside subagents; `cwd` is the worktree root once a worktree is entered.
Still assumed (test at Build): the shell setting and exit 2 behave this way on this machine; a session whose project dir is
the main checkout but cwd is a worktree is detected by the git-dir check; `ask` actually prompts in a sub-session.

## Files
- `.claude/settings.json`
- `tools/guard-bash.ps1`
- `tools/guard.mode`

## Contracts
None.

## Approve
User, 2026-11-08, in the L1 chat: "approved, start the worktree, branch and build this out" (plan including the
merge-policy-aware guard). Routing: Codex Terra medium for the script.

## For Codex
Role: terra (`docs/agents.md`), because a command parser with many allow/block cases needs care. Product is not touched.
The branch is already checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`,
`docs/coding.md`, then `tools/catch-up.ps1` for script style.

Constraints:
- Build exactly the Plan's "After" for the Files above. Do not edit anything else, and do not add the `.claude/settings.json`
  hook or `permissions` block: the manager does that after testing.
- `tools/guard-bash.ps1` must run under both Windows PowerShell 5.1 and `pwsh` 7 (no 7-only syntax). Hook JSON arrives on
  stdin (`cwd`, `tool_name`, `tool_input.command`). Read the project root from `$env:CLAUDE_PROJECT_DIR`, else the
  script's parent folder.
- Mode: first word of `tools/guard.mode` (`log` or `block`; missing file or anything else means `log`). The shipped file
  contains `log`. `log`: append one line (`ISO time<TAB>mode<TAB>guard<TAB>verdict<TAB>cwd<TAB>command`) to
  `<root>/build/guard.log` (create the folder), exit 0, print nothing. `block`: for a BLOCK verdict also write the reason to
  stderr and exit 2. Allowed commands log nothing in `log` mode except guards that flagged (log flagged only, to keep the log small).
- Fail open: any internal error or unparsable input logs `guard-error` and exits 0.
- Parse the command: split on `;` `&&` `||` `|` and newlines (respect simple quotes), skip env-var prefixes, find `git`,
  consume `-C <dir>` (resolve the effective directory against `cwd`; may be relative) and `-c k=v`, then the subcommand.
  Handle `git.exe`. A quoted or heredoc body is not a command.
- Context of a directory: `worktree` when `git rev-parse --git-dir` differs from `--git-common-dir`; `main-dev` when they
  are equal and the current branch is `dev`; otherwise `other` (log "unknown context", allow).
- Merge guard rules (decide on directory and target ref, never on `--no-ff`):
  - `worktree`: ALLOW `git push` / `--force-with-lease` for the item's own branch (bare push, or a refspec naming the
    current branch or a `chore|feat|fix|refactor|docs/<slug>` branch), `git rebase`, `git commit`, `git fetch`. BLOCK `git merge`
    (any args), `git checkout` / `git switch` to `main` or `dev`, and any push whose target is `dev` or `main`
    (`origin dev`, `HEAD:dev`, `HEAD:refs/heads/main`, `--all`, `--mirror`, `--delete dev|main`, `:dev`).
  - `main-dev`: ALLOW `git merge --no-ff [--no-commit] <branch>`, `git merge --abort`, `git push origin dev` and
    `git push origin --delete <non-main, non-dev branch>`, `git branch -d`. BLOCK `git rebase`, any force push (`--force`,
    `-f`, `--force-with-lease`, `+refspec`), and any push of `main`.
- Installed-copy wall: BLOCK when the text contains `--quit`, `Programs\AntiNotepad` or `%LOCALAPPDATA%\AntiNotepad`
  (either slash direction, case-insensitive) and neither the command text nor the environment has `ANTINOTEPAD_DATA`.
  `--quit` inside a quoted `git commit -m` message or `echo` is still flagged in `log` mode (fine; note it in the report).
- `-SelfTest` switch: runs table-driven cases on a pure function `Get-GuardVerdict -Command -Context -HasData`
  (context passed in, so no git repo is needed) and prints `GUARD SELFTEST PASS` or fails with the case name, exit 1.
  Cases: every ALLOW and BLOCK in the rules above, `git -C . push origin HEAD:dev` in `worktree` = block,
  `git push --force-with-lease origin chore/X` in `worktree` = allow, `git merge --no-ff --no-commit b` in `main-dev` = allow,
  `git rebase dev` in `main-dev` = block, `git switch main` in `worktree` = block, `--quit` with and without data env,
  installed path with and without it, `a && git merge x` in `worktree` = block, `echo hi` = allow.
- Also add the mode-switch test: with a temp root, mode `log` gives exit 0 and a log line for a BLOCK case; mode `block`
  gives exit 2 and a stderr reason.
- Follow `docs/coding.md` (header comment, formatting). No ports, no servers, no AntiNotepad launch, never `--quit`.

Done means (offline checks only):
- `pwsh -NoProfile -File tools/guard-bash.ps1 -SelfTest` prints `GUARD SELFTEST PASS`; same under `powershell -NoProfile`.
- Piping sample hook JSON to the script behaves as the rules say (show 3 examples in the report).
- `tools/guard.mode` exists containing `log`.
- `pwsh -NoProfile -File tools/gate.ps1 -Task L1-lv4-native-guards -Offline` passes.

Report: `.coord/reports/L1-lv4-native-guards.md`, short, sections: Summary, Change list, Checks run, Open questions.

## Check
Codex (terra) built `tools/guard-bash.ps1` and `tools/guard.mode`; the manager added the hook and `permissions.ask` to
`.claude/settings.json`. Gate: `GATE: PASS` (offline).

| Claim | Status | Evidence |
|---|---|---|
| `-SelfTest` passes on `pwsh` 7 and Windows PowerShell 5.1 (33 cases + log/block mode) | Verified | both print `GUARD SELFTEST PASS` |
| Hook runs with `"shell": "powershell"` and `$env:CLAUDE_PROJECT_DIR` on this machine | Verified | `claude -p` in the worktree wrote `build/guard.log` lines |
| Merge guard logs `git merge` and `git push ... HEAD:dev` in a worktree | Verified | 2 log lines, `merge BLOCK`, mode `log`, commands still ran (harmless: missing ref, `--dry-run`) |
| Installed-copy wall logs `--quit` without `ANTINOTEPAD_DATA` | Verified | log line `installed-copy BLOCK` |
| Control command `echo hi` logs nothing | Verified | no log line |
| `block` mode stops the call, reason reaches Claude (exit 2) | Verified once | `git merge --no-ff nonexistent-xyz` refused with "Merging in an item worktree is blocked."; mode set back to `log` |
| `permissions.ask` on `Edit(.coord/lessons.md)` prompts in a worktree session | Verified | headless run reported the permission request and the file stayed unchanged |
| Main-checkout rules (`main-dev` context) | Self-test only | not exercised live; would need a run in the main checkout |
| Hooks inside subagents, `ask` for STATUS.md and loop.md paths | Assumed | same rule form as lessons.md; docs say hooks fire in subagents |
| Plain-text `--quit` in a commit message or echo is flagged (false positive) | Known | log-only; note for block mode |
| `git checkout dev -- <file>` in a worktree is flagged (false positive) | Known | log-only; note for block mode |

Concern: `permissions.ask` has no per-session switch, so a director editing STATUS.md in the main checkout is also asked.
Undo (one block each): delete the `PreToolUse` entry; delete the `permissions` block. Mode switch: `tools/guard.mode`.
