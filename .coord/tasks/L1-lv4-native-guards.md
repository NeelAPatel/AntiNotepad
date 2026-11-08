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
