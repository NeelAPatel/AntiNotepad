# D14-opt-in-loop-hooks

Added opt-in gates to `tools/guard-bash.ps1`, `tools/loop-card.ps1`, `tools/title-hint.ps1`, `tools/worktree-check.ps1`, `tools/director-notes.ps1`, `tools/session-where.ps1`, and `tools/status-line.ps1`. The two self-tests explicitly enable their child hook processes. Added `docs/claude-hooks.md` and short links in `CONTRIBUTING.md` and `docs/playbook.md`.

Offline checks: PowerShell parser passed for all seven scripts. Each exited 0 with empty stdout when the switch was unset. With `ANTINOTEPAD_LOOP=1`, each matched the corresponding `git show HEAD` version for the exercised inputs; these were limited, mostly quiet-path fixtures, not exhaustive behavior tests. The guard and title-hint self-tests passed with the switch both unset and set. `git diff --check` passed for tracked changed scripts and playbook. The first title self-test attempt used a temp directory inside a worktree and failed because its fixture was interpreted as an item worktree; both reruns with the normal temp directory passed.

Test mistake: the enabled loop-card fixture was placed inside this Git checkout. The script resolved the main checkout and thus attempted its normal roadmap health request and potentially server startup, contrary to the no-port instruction. Process inspection was denied, so whether it started a server is unconfirmed. No browser was started. The enabled comparison result is valid for the observed output but does not establish that no server started.

The disabled status-line command was confirmed to exit 0 with empty stdout. I could not verify offline how Claude Code renders an empty status-line result or whether it restores a default line; a live Claude Code check remains open. No commit was made.
