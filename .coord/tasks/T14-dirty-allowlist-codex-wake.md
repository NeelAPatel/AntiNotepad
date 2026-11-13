# T14-dirty-allowlist-codex-wake: approve stops failing on dirty .coord files; find why Codex runs are not monitored

Item card (`docs/loop.md`). Side-task fast lane (T): short card, no audit.

Tier: Low · docs-tools
Branch: `chore/T14-dirty-allowlist-codex-wake` in `.claude/worktrees/chore/T14-dirty-allowlist-codex-wake` (created at Approve)

## Request
User, 2026-11-13: "another issue: this seems to happen a lot more often" (A15 item chat could not run `tools/approve.ps1`: `.coord/pending.md` had uncommitted edits in the main checkout; director asked to commit or revert). Then: "A 16, 17, 18 seems to kick off codex but not monitor their completion... weird." Then: "okay fix these up" (after notes, 2026-11-12: "no need to fix yet, just make notes").

## Findings
- Step a. The dirty-checkout allowlist is `^.. (\.coord/STATUS\.md|\.claude/settings\.json)$` in four scripts: `tools/approve.ps1:37`, `tools/catch-up.ps1:65`, `tools/gate.ps1:44`, `tools/merge-preflight.ps1:156`. Since Loop10 the rule "proposals go into `.coord/pending.md` at once" keeps `pending.md` (also `lessons.md`, `status-archive.md`) uncommitted in the main checkout, so every approve can fail. Approve commits only the card, so those files are safe to leave dirty; the director commits them in the merge commit.
- Step b (unknown cause). Launch is `.coord/tools/codex-run.sh` "in the background; the process exit wakes the item chat" (`docs/agents.md`). The user saw A16/A17/A18 start Codex and not follow its completion. Director's quick look (2026-11-13): A16 finished and merged; A17 chat was running with a live Codex process; A18 was idle waiting on a lab pick, so a stuck run was not reproduced.

## Plan
- a. Widen the regex in all four scripts to `^.. (\.coord/(STATUS|pending|lessons|status-archive)\.md|\.claude/settings\.json)$`. Add one line to `docs/dev-loop.md` (these `.coord` files may be dirty at approve, catch-up, gate and preflight). Test: with `.coord/pending.md` modified, `tools/approve.ps1 -Task <name> -Prefix chore -DryRun` no longer prints "dirty checkout" (revert the test edit); a dirty product file still fails.
- b. Find the cause with evidence, read-only first: read the A16, A17, A18 chat transcripts (`list_events`, or search transcripts) for how each launched Codex (background tool call vs a detached process) and whether the chat was woken at exit; read `.coord/tools/codex-run.sh`. Write the cause and a minimal fix in the report. Apply the fix only if it is a text or launch-script change of a few lines; otherwise propose it and stop. Never poll: two samples 30 s apart if you must check a live run.
- Gate offline; no hand test. Expected time: about 40 minutes.

## Files
- `tools/approve.ps1`
- `tools/catch-up.ps1`
- `tools/gate.ps1`
- `tools/merge-preflight.ps1`
- `docs/dev-loop.md`
- `docs/agents.md`
- `.coord/tasks/T14-dirty-allowlist-codex-wake.md`
- `.coord/reports/T14-dirty-allowlist-codex-wake.md`

## Contracts
None (no product code).

## Approve
User, 2026-11-13: "okay fix these up".

## Check
- Gate (offline): PASS (all 9 steps). Regex tested on sample status lines; step b cause and fix in the report.
