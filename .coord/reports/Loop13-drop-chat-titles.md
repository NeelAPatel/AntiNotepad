# Loop13-drop-chat-titles build report

## Changed

- Deleted `tools/chat-title.ps1`; the local `.coord/titles/` directory was already absent.
- Removed title calls from `tools/gate.ps1`, `tools/loop-card.ps1`, `.coord/tools/codex-run.sh`, `.claude/skills/merge-task/SKILL.md`, and `.claude/skills/next-task/SKILL.md`.
- Removed the title scheme and legend from `docs/loop.md`, `docs/loop-sessions.md`, `docs/wiki/Who-Does-What.md`, and `tools/roadmap/index.html`; `docs/loop.md` states that titles are free-form and the status card carries the phase.
- Removed `.coord/titles/` from `.gitignore`; updated sample slugs and expected badge text in `product/AntiNotepad/SelfTest/App.cs` and `tools/playground/t17-badge-lab.html`.
- This report: `.coord/reports/Loop13-drop-chat-titles.md`.

## Tests run

- Local-cache-only restore: succeeded. An initial `--no-restore` build failed because this worktree had no assets file; restored from the local NuGet cache and rebuilt.
- `dotnet build product/AntiNotepad -c Debug -o build/Loop13-drop-chat-titles-check --no-restore`: `Build succeeded. 0 Warning(s), 0 Error(s)`.
- Built `AntiNotepad.exe --selftest`: exit 0, `CONTRACTS: 32 rows, 31 pass, 1 manual`, `SELFTEST PASS`.
- `pwsh -NoProfile -File tools/gate.ps1 -Offline -Task Loop13-drop-chat-titles`: `GATE: PASS`. Tail: Format, Scope, LoopLint, Size and Cleanup PASS; LoopLint warns about the existing lessons and item-card budgets and docs/loop.md at 8198 bytes versus 8192. Full gate output: `.coord/reports/Loop13-drop-chat-titles.gate.md`.
- `git diff --check`: pass. The requested build directory was removed after checks.

## Grep output

`git grep -il chat-title` (all matches are permitted history or task cards):

```text
.coord/STATUS.md
.coord/lessons.md
.coord/pending.md
.coord/reports/Loop12-chat-title-scheme-r2.md
.coord/reports/Loop12-chat-title-scheme-r2.terra.codex.md
.coord/reports/Loop12-chat-title-scheme.md
.coord/reports/Loop12-chat-title-scheme.terra.codex.md
.coord/research/loop12-title-spike.md
.coord/status-archive.md
.coord/tasks/Loop12-chat-title-scheme-r2.md
.coord/tasks/Loop12-chat-title-scheme.md
.coord/tasks/Loop13-drop-chat-titles.md
```

## Risks

Loop14 also edits `merge-task` and `codex-run.sh`; the second merger must rebase. No live chat retitling or UI hand test was performed.
