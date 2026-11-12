# T12-settings-drift-readers: catch-up, merge-preflight and gate ignore settings.json drift

Item card (`docs/loop.md`). Side-task fast lane (T): short card, no audit.

Tier: Low · docs-tools
Branch: `chore/T12-settings-drift-readers` in `.claude/worktrees/chore/T12-settings-drift-readers` (created at Approve)

## Request
User, 2026-11-12: "go ahead and fix it, a heavy plan stage isn't needed for a small fix like this". Follows Loop9a-settings-scope: `approve.ps1` already ignores `.claude/settings.json` drift (an app mod rewrites it); three other scripts do not.

## Findings
- `tools/catch-up.ps1:65`: refuses when tracked changes exist outside `.coord/STATUS.md`.
- `tools/merge-preflight.ps1:156`: throws `dirty item checkout` for the same pattern.
- `tools/gate.ps1:44`: appends `-dirty` to the head SHA in the report for the same pattern, which can break the preflight's "newest gate PASS at this commit" match.
- `tools/approve.ps1:37` already allows both files (reference pattern).

## Plan
- Change the three regexes to also allow `.claude/settings.json`, as in `approve.ps1`. Nothing else.
- Update the refusal text in `catch-up.ps1` if it names only STATUS.
- Test: each script still refuses on any other dirty tracked file; passes with only `settings.json` and/or STATUS modified (a temporary edit in a scratch worktree, reverted).
- Gate offline. No Codex, no hand test. Expected time: about 15 minutes.

## Files
- `tools/catch-up.ps1`
- `tools/merge-preflight.ps1`
- `tools/gate.ps1`
- `.coord/tasks/T12-settings-drift-readers.md`
- `.coord/reports/T12-settings-drift-readers.md`

## Contracts
None (no product code).

## Approve
User, 2026-11-12: "go ahead and fix it, a heavy plan stage isn't needed for a small fix like this".

## Check
- Plan: change three regexes to allow settings.json -> done (catch-up, merge-preflight, gate).
- Plan: update refusal text if STATUS-only -> done (catch-up refusal, preflight CHECK line).
- Plan: test refuse on other dirty file, pass on settings/STATUS -> done (catch-up live, preflight by regex, gate run); see report.
- Plan: gate offline -> GATE PASS.

## Hand test
None, no UI (tooling only).
