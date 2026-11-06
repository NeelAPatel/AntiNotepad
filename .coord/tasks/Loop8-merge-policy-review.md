# Loop8-merge-policy-review: fresh read-only review of the preflight and catch-up change

Role: reviewer, fresh read-only lens review. Read `AGENTS.md`, then `.agents/skills/lens-review/SKILL.md`, then the card
`.coord/tasks/Loop8-merge-policy.md` (Plan "After") and run `git diff dev...HEAD -- tools/` yourself.

Focus on `tools/merge-preflight.ps1` (new `Test-CommitBuilds`, widened `Test-GateHead`, merge-commit check) and
`tools/catch-up.ps1` (rebase, `-Push`). Look for: a path where the preflight can pass with a commit that does not build
or does not pass `--selftest`; a way it can modify the item branch, dev, the item checkout or leave the scratch worktree
behind; stale-Head rules that are too loose (a code commit slipping past the gate) or too tight; env vars leaking after
the run; catch-up pushing anything but the item branch, or rebasing dev; behavior after a failed or conflicting rebase.
Known and already verified by the manager on scratch repos: PASS, docs-only skip, stale-Head card/STATUS pass and code
commit fail, broken commit named, merge-commit fail, catch-up UP TO DATE/CLEAN/-Push/CONFLICT/in-progress refusal.

Offline, read-only. No servers, no builds needed (you may read only). Report to the last message: verdict
(accept | rework), then findings, each with `file:line`, what breaks, and a concrete input, most severe first. Do not
report style nits.
