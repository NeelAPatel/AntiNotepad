# Loop10-status-split: preview of the new STATUS.md

Proposed file, built from the live rows. Step c replaces `.coord/STATUS.md` with it after the user's nod. Size line is at the end.

---

# STATUS

Updated: 2026-11-12 · Product: AntiNotepad (`product/AntiNotepad`) · Loop v5.3.0 · installed `v0.7.0-rc1` (as STATUS says now; user to confirm)
Only active work lives here. Pending: `.coord/pending.md`. Finished: `.coord/status-archive.md`. Detail: item card in `.coord/tasks/`.

## Working rules right now
- Directors (top-level chats): Master 4 (app, design), Loop Hub 3 (Loop, T, L), Roadmap Hub 1 (roadmap). Notes in `.coord/directors/`.
- Directors route work; they do not do it. Default: Sonnet-or-lower sub-chat per item.
- Updates between directors: need-to-know only; tell after finishing, or ask the user.
- Codex allowed; role by job (`docs/agents.md`); checks offline; no run caps.
- Computer Use off until the user says go; live checks are by hand in the dev copy.
- Never run `AntiNotepad.exe --quit` without `ANTINOTEPAD_DATA` (hits the installed copy).
- Move rules: Approve = pending → STATUS. Merge = STATUS → archive.

## Where things are
- `main` = releases only (tagged); `dev` = integration. Tags: `v0.7.0-rc3` latest, `v0.1.0`, `v0.5.0`, `v0.6.0-rc1`..`rc9`, `v0.7.0-rc1`, `rc2`.
- History rewritten 2026-10-29 (623 commits → 156). Old hashes: tags `archive/full-history-2026-10-29`, `archive/main-2026-10-29`, bundle `AntiNotepad-history-backup-2026-10-29.bundle`.
- Installed copy: `v0.7.0-rc1` per today's STATUS (user to confirm; upgrade only on request, `tools/install.ps1`).
- Locked choices: `docs/decisions.md`. Phases and states: `docs/loop.md`. Files: `docs/loop-sessions.md`.
- Roadmap page: http://localhost:47811 (`/roadmap`); reads STATUS + pending + archive.

## Now (active rows, one line each)
| Item | Goal | Tier | State | Card |
|---|---|---|---|---|
| Loop10-status-split | STATUS holds only active work | Medium | building (a-b done, c waits) | `.coord/tasks/Loop10-status-split.md` |

- No other item in work. Last finished: Loop9-loop-slim (merged dev 2026-11-12).

## Open (needs the user)
- Live check: A30 restart/sign-out saves an edit typed just before.
- N3-release-and-update-plan: proposal tag `proposal/N3-release-plan`; separate chat.
- D1-ui-region-names: paused (user, 2026-11-01); branch d96a11f; name review left.
- Hold: A44b-interop-guards leftover hand checks (Explorer restart, tray right-click menu).

## Next up (from pending.md, user order)
- A5-search-bar-overlap: overlay search covers top bar.
- A6-note-switch-instant-swap: swap text before arrive animation.
- Then the rest of `.coord/pending.md` (41 rows: proposed, needs you, paused, deferred).

## Keep
- This file stays under ~7 KB; budget WARN at 8 KB (`tools/loop-lint.ps1`).
- One line per row. Short bullets, no full sentences.
- The backlog (someday) list moves to the end of `.coord/pending.md`.

---

Preview size: 2674 (the proposed file only) bytes (target 7 KB = 7168). Current STATUS.md after step b: 13485 bytes (56222 before Loop10).

Questions for the user:
- Is "Now" as a one-row table fine, or bullets?
- Should "Next up" show only 2 rows (above) or none (pending.md holds order)?
- Backlog (someday, 4 KB) moves to `pending.md` — ok?
