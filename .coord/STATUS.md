# STATUS

Updated: 2026-11-16 · Product: AntiNotepad (`product/AntiNotepad`) · installed `v0.7.0-rc1`
- Only active work lives here. Pending: `.coord/pending.md`. Finished: `.coord/status-archive.md`. Detail: item card in `.coord/tasks/`.

## Working rules right now
- Directors (top-level chats): Master 6 (app, design), Loop Hub 4 (Loop, T, L), Roadmap Hub 1 (roadmap), PStack Manager (pstack). Notes in `.coord/directors/`.
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
- In flight: A29-shortcut-map (building). T15-ffmpeg-frame-capture-trial paused (user, 2026-11-17).
- Last finished: T18-roadmap-parent-rows, Loop20-codex-labs-and-scout, Loop19-review-task-chat-runnable, Loop17-chat-title-selfheal, Loop16-poteto-trigger, A26-tab-at-text-start, A27-recovered-folder-always-open, A21b-save-scheduler-extract, D3-release-checklist, Loop15-preflight-stale-head, A25-page-edge-stuck-after-hide, Loop13-drop-chat-titles, Loop14-codex-closing-files, T17-multi-dev-instances, A24-save-delay-cleanup, A23-paste-keep-structure, A22-wrap-hanging-indent, Loop12-chat-title-scheme-r2, Loop12-chat-title-scheme, A21-save-delay-setting, T16-roadmap-table-columns (dev 2026-11-17), T14-dirty-allowlist-codex-wake, Loop11-one-chat-flow, A19-settings-scroll-feel, A18-settings-segment-animation, A17-settings-toggle-animation.

## Plan
Active rows only (the roadmap script parses this table; same 5 columns as `.coord/pending.md`).

### Pending work
| Item | Scope | Tier | Effort | State |
|---|---|---|---|---|
| A29-shortcut-map | Shortcut table in code and docs; Settings Shortcuts page (record, clear, reset); defaults only for basics; Ctrl+Alt+Shift+1-9/F1-F9 reserved for dev copies. Card in `.coord/tasks/` | Medium | Medium | building |
| A32-selection-convert-menu | Right-click Convert to list ›: Checklist, Bullet list, Numbered list on selected lines (swap prefixes, skip blanks, numbering via existing renumber). Card in `.coord/tasks/` | Medium | Small | hand test |

## Open (needs the user)
- Live check: A30 restart/sign-out saves an edit typed just before.
- N3-release-and-update-plan: proposal tag `proposal/N3-release-plan`; separate chat.
- D1-ui-region-names: paused (user, 2026-11-01); branch d96a11f; name review left.
- Hold: A44b-interop-guards leftover hand checks (Explorer restart, tray right-click menu).

## Next up (user, 2026-11-16: these outrank the rest; rows in `.coord/pending.md`)
- idea: tab-at-text-start: Word's Tab rule for list items.
- idea: list-default-type: Settings default for `list` plus checklist, bullet, numberlist keywords.
- idea: selection-convert-menu: right-click > Convert to list.

## Keep
- STATUS stays under ~7 KB (budget WARN at 8 KB). One line per row; short bullets, no full sentences.
