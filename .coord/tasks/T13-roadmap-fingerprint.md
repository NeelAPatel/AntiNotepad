# T13-roadmap-fingerprint: live reload watches pending.md and status-archive.md

Item card (`docs/loop.md`). Side-task fast lane (T): short card, no audit.

Tier: Low · docs-tools
Branch: `chore/T13-roadmap-fingerprint` in `.claude/worktrees/chore/T13-roadmap-fingerprint` (created at Approve)

## Request
User, 2026-11-12: "Merge T12, then start T13. T13 can be done solo, no need for me. Let me know when T13 is complete." Raised by Roadmap Hub 1 as a gap left by Loop10-status-split.

## Findings
- `Get-RoadmapFingerprint` in `tools/roadmap-serve.ps1` (around line 97) stats only `.coord/STATUS.md`, `usage.tsv`, `eras.json` and `roadmap-data.ps1`.
- The roadmap now also reads `.coord/pending.md` and `.coord/status-archive.md` (Loop10), so edits to them reach the live page only after a STATUS edit or commit. That breaks the auto-refresh from `T10-roadmap-live-reload`.
- Missing files already fall back to `|missing` in the fingerprint.

## Plan
- Add the two paths to the `foreach` list. Nothing else.
- Test on a spare port, never 47811 (the user's live roadmap): start the server on another port, edit `.coord/pending.md` (touch a copy in a scratch checkout or revert after), see `/data` change within about 5 s; revert the edit; stop your server.
- Gate offline. No Codex, no hand test. Expected time: about 15 minutes.

## Files
- `tools/roadmap-serve.ps1`
- `.coord/tasks/T13-roadmap-fingerprint.md`
- `.coord/reports/T13-roadmap-fingerprint.md`

## Contracts
None (no product code).

## Approve
User, 2026-11-12: "Merge T12, then start T13".

## Hand test
None, no UI change (tooling only).
