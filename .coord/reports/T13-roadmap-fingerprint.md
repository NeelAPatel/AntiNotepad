# T13-roadmap-fingerprint report

## Changed
- `tools/roadmap-serve.ps1`: `Get-RoadmapFingerprint` now also stats `.coord/pending.md` and `.coord/status-archive.md` (paths defined like `$statusPath`; missing files already fall back to `|missing`).

## Tests
- Ran the server from this worktree on spare port 47931 (never 47811) with `-Background -Root`.
- Appended a line to `pending.md`: `/data` ETag changed within 5 s; reverted, ETag changed again.
- Same for `status-archive.md`: ETag changed within 5 s on edit and on revert.
- Edits reverted (`git status` shows only the script); server stopped with `-Stop`.
- `pwsh -NoProfile -File tools/gate.ps1 -Task T13-roadmap-fingerprint`: GATE PASS (offline). Warnings are existing doc budgets.
