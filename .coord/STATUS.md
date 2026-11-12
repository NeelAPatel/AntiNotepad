# STATUS

Updated: 2026-11-12 · Product: AntiNotepad (`product/AntiNotepad`) · installed `v0.7.0-rc1`
- Only active work lives here. Pending: `.coord/pending.md`. Finished: `.coord/status-archive.md`. Detail: item card in `.coord/tasks/`.

## Working rules right now
- Directors (top-level chats): Master 4 (app, design), Loop Hub 3 (Loop, T, L), Roadmap Hub 1 (roadmap). Notes in `.coord/directors/`.
- Directors route work; they do not do it. Default: Sonnet-or-lower sub-chat per item.
- Updates between directors: need-to-know only; tell after finishing, or ask the user.
- Codex allowed; role by job (`docs/agents.md`); checks offline; no run caps.
- Computer Use off until the user says go; live checks are by hand in the dev copy.
- Never run `AntiNotepad.exe --quit` without `ANTINOTEPAD_DATA` (hits the installed copy).
- Move rules: Approve = pending -> STATUS. Merge = STATUS -> archive.

## Where things are
- `main` = releases only (tagged); `dev` = integration. Tags: `v0.7.0-rc1` to `rc3`, `v0.6.0-rc1` to `rc9`, `v0.5.0`, `v0.1.0`.
- History rewritten 2026-10-29 (623 commits to 156). Old hashes: tags `archive/full-history-2026-10-29`, `archive/main-2026-10-29`, bundle `AntiNotepad-history-backup-2026-10-29.bundle`.
- Installed copy: `v0.7.0-rc1` (upgrade only on request, `tools/install.ps1`).
- Locked choices: `docs/decisions.md`. Phases and states: `docs/loop.md`. Files: `docs/loop-sessions.md`.
- Loop version: `docs/loop.md`.
- Roadmap page: http://localhost:47811 (`/roadmap`); reads STATUS + pending + archive.

## Now
- Nothing in work. A14-search-open-fade-up is next (plan ready; approve now that A13 is merged). Last finished: A13-search-toggle-off (dev 2026-11-12).

## Plan
Active rows only (the roadmap script parses this table; same 5 columns as `.coord/pending.md`).

### Pending work
| Item | Scope | Tier | Effort | State |
|---|---|---|---|---|

## Open (needs the user)
- Live check: A30 restart/sign-out saves an edit typed just before.
- N3-release-and-update-plan: proposal tag `proposal/N3-release-plan`; separate chat.
- D1-ui-region-names: paused (user, 2026-11-01); branch d96a11f; name review left.
- Hold: A44b-interop-guards leftover hand checks (Explorer restart, tray right-click menu).

## Next up (user order; rest in `.coord/pending.md`)
- A5-search-bar-overlap: overlay search covers top bar.
- A6-note-switch-instant-swap: swap text before arrive animation.

## Keep
- STATUS stays under ~7 KB (budget WARN at 8 KB). One line per row; short bullets, no full sentences.
