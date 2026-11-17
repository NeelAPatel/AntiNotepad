# Loop12-chat-title-scheme-r2 report

## 1 Summary
Implemented session-based item lookup and STATUS-first phase lookup.

## 2 Change list
- `tools/chat-title.ps1`: session registry, worktree card fallback, round-name handling, fixture-tested sync and expanded `-Check`.
- `tools/loop-card.ps1`: pass hook stdin session ID to the title script.
- `.claude/skills/next-task/SKILL.md`: register the item once at chat start.

## 3 Behavior changes
Explicit item takes precedence over session registry, then branch; STATUS takes precedence over worktree cards. Sync reads session JSON and writes only the registry.

| Case | Input | Resulting title or result |
|---|---|---|
| STATUS overrides card | A22-wrap-hanging-indent; hand test | 🔨 A22-wrap-hanging-indent · hand test |
| STATUS rework | A23-paste-keep-structure; rework r4 | 🔨 A23-paste-keep-structure · build · r4 |
| Worktree card | A24-fallback; checking | 🔨 A24-fallback · check |
| Missing state | A25-missing | 🔨 A25-missing · request |
| Round name | A22-wrap-hanging-indent-r2; hand test | 🔨 A22-wrap-hanging-indent · hand test · r2 |
| Registry | session-123 mapped to A22 | 🔨 A22-wrap-hanging-indent · hand test |
| Bad session ID | ../bad | No registry item |
| Sync | exact A22 and unique bare A23 | Both registered |
| Sync ambiguous | bare T9 with two items | No registration |

## 4 Out-of-scope files
None changed, except this requested report.

## 5 Plan deviations
None. Gate and live app-data sync were intentionally not run, per the brief.

## 6 Contracts
None specified (tooling only).

## 7 Lens review
Skipped as requested.

## 8 Checks run
- `pwsh -NoProfile -File tools/chat-title.ps1 -Check`: PASS, including sync against temporary files.
- `pwsh -NoProfile -File tools/loop-lint.ps1`: PASS; existing budget warnings for `docs/loop.md`, `.coord/lessons.md`, and the item card.
- `git diff --check`: PASS; Git reported line-ending conversion warnings.

## 9 Hand checks for the user
In an item chat, register its item once, then submit a prompt and confirm the emitted title has the expected item and STATUS phase. Run `-Sync` only when ready to register existing chats, then verify representative titles and ambiguous IDs.
