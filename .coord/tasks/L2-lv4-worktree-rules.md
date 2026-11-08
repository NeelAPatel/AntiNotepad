# L2-lv4-worktree-rules: worktree naming and base-on-dev rules checked by Claude Code config

Item card (`docs/loop.md`). Level 4 item 2 of 7 (shared brief: `.coord/research/lv4-shared-brief.md`).

Tier: Medium · config plus one script, no app code
Branch: `chore/L2-lv4-worktree-rules` in `.claude/worktrees/chore/L2-lv4-worktree-rules` (created at Approve)

## Request
Loop Hub 2, 2026-11-08: make the worktree rules checked by config. (a) native worktree base on `dev` if the docs support it;
(b) a logging-only hook that flags a worktree not at `.claude/worktrees/<prefix>/<ID-slug>` (lowercase slug, branch of the
same name) or not based on dev; (c) decide in writing whether `tools/approve.ps1` stays the only way items get a worktree.
Done when: base setting verified or rejected with reason, the check logs on a throwaway worktree, undo is one line, and the
card lists verified versus assumed.

## Findings
- **Kind and why:** process. The rules live in `docs/loop.md` prose and in `approve.ps1`; nothing notices a worktree made another way (native `--worktree`, the app's own, a hand `git worktree add`).
- **Now:** `approve.ps1` makes `.claude/worktrees/<prefix>/<ID-slug>` from `dev`. Native worktrees go to `.claude/worktrees/<name>` on branch `worktree-<name>` from the remote default branch (origin/HEAD = `main` here). The app-made `claude/dreamy-raman-7c3f1c` sits at the v0.7.0-rc1 marker commit, which is an ancestor of both `main` and `dev`, so its base cannot be told from the git data.
- **Where:** `.claude/settings.json` (shared with L1, L4, L5: expect a rebase), new `tools/worktree-check.ps1`, log in `.coord/logs/` (gitignored).
- **Risk:** a `WorktreeCreate` hook would REPLACE git's creation, not add to it (docs); so none is used.
- **Source:** docs read 2026-11-08 by a helper agent (urls below); local git facts checked by me.

### Verified vs assumed (so far)
| Claim | State | Evidence |
|---|---|---|
| `worktree.baseRef` accepts only `"fresh"` (default, remote default branch) or `"head"` (current local HEAD); a branch name such as `dev` is not allowed | **Verified in docs** (helper agent, https://code.claude.com/docs/en/worktrees.md, settings-reference.md) | not yet seen in a run |
| `baseRef` applies to `--worktree` and `isolation: worktree` subagents | Verified in docs | no run yet |
| `baseRef` applies to app-made (desktop) worktrees | **Unknown**: desktop page is silent | experiment cannot reach the app's button |
| `.worktreeinclude` (gitignore syntax) copies only ignored files into native worktrees | Verified in docs | nothing in this repo needs it yet |
| Native `--worktree chore/L2-x` slash names work | **Unknown**: docs silent; branch would be `worktree-chore/L2-x` | test in Build |
| `WorktreeCreate` replaces default creation | Verified in docs | not used |
| `SessionStart` cannot block, gets `cwd` and `source` | Verified in docs | test in Build |
| Installed Claude Code is 2.1.312; docs give no minimum version for `baseRef` | Verified (`claude --version`); version gap is **assumed fine** | |
| Origin HEAD is `main`, so native default base is main | Verified (`refs/remotes/origin/HEAD`) | |

## Plan
- **After:**
  - (a) `.claude/settings.json` gets `"worktree": {"baseRef": "head"}`. The main checkout stays on dev, so native worktrees started from it branch from dev. `dev` cannot be named: that part is **rejected with the reason above**. Caveat: started from an item worktree it branches from that item's HEAD.
  - (b) `SessionStart` hook runs `tools/worktree-check.ps1` (PowerShell). It skips the main checkout. In a linked worktree it checks: path is `.claude/worktrees/<prefix>/<ID-slug>` (prefix in feat/fix/refactor/docs/chore, same pattern as `approve.ps1`); branch equals `<prefix>/<ID-slug>`; HEAD is not simply on the main tip while dev has moved (heuristic for "based on main"). It appends one line per problem to `.coord/logs/worktree-check.log` and prints nothing, so it adds no context. Always exits 0.
  - (c) Written comparison below; decision: `approve.ps1` stays the only item path. No change to it.
  - A tiny experiment (Build): one throwaway `claude -p --model haiku --worktree l2-probe` run to see branch name, path and base with `baseRef: head`, then remove the worktree and branch.
- **Expected time:** about 45 minutes request to merge (small script, one setting, one experiment).
- **Must not change:** `tools/approve.ps1`, app code, gate scripts, user-global settings, `docs/loop.md` (L6 owns the split).
- **Scope questions:** none; one decision at Approve (go or change).
- **Test plan, code:** script check, not selftest: run `worktree-check.ps1` with a stdin JSON against (1) a good nested worktree, (2) a throwaway `claude/…`-style flat worktree, (3) the main checkout; expect log lines only for (2).
- **Test plan, UI:** None (not visible). The user's hand test: read the log file after opening a throwaway native worktree session.
- **Gates:** `tools/gate.ps1 -Task L2-lv4-worktree-rules` (scope: files list; format on the script). Build and selftest unaffected (no product change).
- **Needs from you:** Approve.

### (c) approve.ps1 versus native worktrees
| | `tools/approve.ps1` | Native `--worktree` / app |
|---|---|---|
| Path | `.claude/worktrees/<prefix>/<ID-slug>` | `.claude/worktrees/<name>` (slashes unverified) |
| Branch | `<prefix>/<ID-slug>` | `worktree-<name>` or app's `claude/<name>` |
| Base | `dev` | main by default; `head` possible, dev not nameable |
| Checks cap (3), file overlap, filled Approve, card move and commit | yes | no |
| Hook can add them | n/a | only by replacing creation (`WorktreeCreate`), which would re-implement approve.ps1 |
**Decision:** approve.ps1 stays the only item path; native worktrees stay for scratch work, and the hook only reports them.

## Files
Only these files may change:
- `.claude/settings.json`
- `tools/worktree-check.ps1`

## Contracts
None (no product code).

## Approve
User, 2026-11-08: "head is fine, i need worktree to spawn from dev branch because im parallelly building from it. im happy to hand test." and "plan is approved, start the worktree, branch and build this out. Once built, debrief me and wait for additional corrections or green light to prep for merging."

## For Codex
Role: terra (medium) (`docs/agents.md`), because it is a hook script with git logic. Product `product/AntiNotepad` is not touched. The branch is already
checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`, `docs/coding.md`, `tools/director-notes.ps1` (hook style), then the files above.

Constraints:
- Build exactly the Plan's "After" (a) and (b). No edits to approve.ps1 or other files.
- Hook entry in settings.json copies the style of the existing `PreToolUse` guard-bash entry (`"shell": "powershell"`, `pwsh -NoProfile -File \"$env:CLAUDE_PROJECT_DIR/tools/worktree-check.ps1\"`, timeout 15), as a second object in `SessionStart` with `"matcher": "startup"`, one entry, so undo is deleting it. Keep every existing entry. Also add `"worktree": {"baseRef": "head"}` as a top-level key.
- Script reads the hook JSON on stdin (`cwd`), never throws, always exits 0, writes only to the log, no stdout.
- Never launch or kill AntiNotepad. Never run `--quit`. Never edit `.coord/STATUS.md` or this card. No ports, servers or browser.

Done means (offline checks only):
- The setting and hook exist; the script logs for a flat or wrong-branch worktree and stays silent for a correct one and for the main checkout.
- Gate passes offline: `pwsh -NoProfile -File tools/gate.ps1 -Task L2-lv4-worktree-rules -Offline`.

Report: `.coord/reports/L2-lv4-worktree-rules.md`, short, sections: Summary, Change list, Behavior changes, Out-of-scope files, Plan deviations, Contracts, Lens review, Checks run, Hand checks for the user.

## Check
Gate PASS (online, 2026-11-08): build, selftest, seam, tokens, scope 2 files, loop-lint.
Review by me (no Codex review; Medium, config only). Fixes after Codex, mine, small: log goes to the main checkout's `.coord/logs` (Codex wrote to the worktree's own); base test uses merge-base with dev (Codex compared HEAD to main tip, which missed a main-based branch that already had commits). Codex wrote no separate report (brief limited it to 2 files); its last message is `.coord/reports/L2-lv4-worktree-rules.terra.codex.md`.
Bounds: within plan.

Verified (run, 2026-11-08):
- `claude -p --worktree` with `baseRef: head` branches from the current HEAD of the checkout it starts in (probe HEAD = item branch HEAD), path `.claude/worktrees/<name>`, branch `worktree-<name>`.
- The SessionStart hook fires in a real native session and logged path and branch problems for it; silent stdout; exit 0 on bad input.
- Fixtures: main checkout and a nested good worktree log nothing; flat main-based worktree logs 3 lines; app-made `claude/dreamy-raman-7c3f1c` logs 2 lines.
Not verified: slash names in `--worktree`; whether `baseRef` applies to the desktop app's worktree button; minimum Claude Code version for `baseRef`; base heuristic is a heuristic (a branch cut at main's tip when dev == main passes).

Misses: None.

## Hand test
Hand test: user opens a throwaway worktree from the app button, then reads `.coord/logs/worktree-check.log` in the main checkout. Result (user, 2026-11-08): "Accept, test after merge" after the folder-specific test proved fiddly. The app button's `baseRef` behavior is checked after the merge: press the app's worktree button in a session on dev and read `.coord/logs/worktree-check.log` in the main checkout (flat path and branch lines expected, no base-on-main line if `head` is honored).
