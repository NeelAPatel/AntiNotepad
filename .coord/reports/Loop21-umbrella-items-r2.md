# Loop21-umbrella-items r2

## 1 Summary
Fixed the three review findings: survival checks now see non-ASCII paths, umbrella catch-up preserves child merge commits, and director start cards use the director naming rule.

## 2 Change list
- `tools/survival.py`: disable Git path quoting for checker commands and add a non-ASCII filename regression case.
- `tools/catch-up.ps1`: add opt-in `-RebaseMerges` and pass `--rebase-merges` to Git.
- `.claude/skills/merge-task/SKILL.md`: direct umbrella catch-up to use `-RebaseMerges`.
- `tools/start-card.ps1`: show the director title rule and assert it in the selftest.

## 3 Behavior changes
Umbrella branches can retain `--no-ff` child merge history when rebased onto dev. The director start card points to `docs/loop-sessions.md` for role/version naming. Survival checks correctly inspect Git diffs for non-ASCII paths.

## 4 Out-of-scope files
No A35 branches or chats changed. The item card was not edited. `tools/catch-up.ps1` is outside the card's declared `Files` list, despite being explicitly required by the r2 request.

## 5 Plan deviations
No implementation deviations. The required gate cannot pass its Scope step unless the item card includes `tools/catch-up.ps1`; the card's Codex instructions prohibit editing that card. Gate was run as requested and failed only at Scope for that file.

## 8 Checks run
- `python -I tools/survival.py --selftest`: PASS, including the non-ASCII filename case.
- `pwsh -NoProfile -File tools/start-card.ps1 -SelfTest`: PASS.
- PowerShell parser check for `tools/catch-up.ps1`: PASS.
- `git diff --check`: PASS (Git emitted line-ending conversion warnings).
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop21-umbrella-items -Offline`: FAIL at Scope (`tools/catch-up.ps1` outside the card Files list). Build (0 warnings/errors), selftest, seam, tokens, format, loop lint, and size steps passed. Loop lint reports existing budget warnings for `docs/loop.md`, `.coord/lessons.md`, and the item card.

## 9 Hand checks
No live umbrella chain was started. Start-card director output is covered by its selftest. Umbrella merge preservation is wired through the explicit `-RebaseMerges` argument; no rebase was run on a live branch.
