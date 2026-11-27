# Loop21-umbrella-items r3

## 1 Summary
Updated STATUS and pending row readers to accept indented item cells and case-insensitive `Child of` scopes. Roadmap parent links normalize short parent IDs such as `A39` to the matching full umbrella ID.

## 2 Change list
- `tools/poteto-gate.ps1`: recognize `&nbsp;`-indented item cells; selftest confirms an existing indented child row passes.
- `tools/title-hint.ps1`: strip allowed indentation before parsing row IDs; selftest confirms the child title hint is emitted.
- `tools/start-card.ps1`: recognize indented child rows and test the umbrella-child role.
- `tools/roadmap-data.ps1`: strip indentation, parse `Child of` without case sensitivity, and resolve short parent prefixes to full row IDs.
- `docs/loop.md`, `AGENTS.md`: document the accepted scope and item-cell indentation convention.

## 3 Behavior changes
Rows like `| &nbsp;&nbsp;A39d-padview-split | Child of A39: ... |` now satisfy brief row checks, produce title hints and umbrella-child start cards, and fold under the full parent item in roadmap data.

## 4 Out-of-scope files
No other files changed. This card was not edited.

## 5 Plan deviations
No implementation deviations. The requested offline gate was run; its build failed because `AvaloniaEdit.dll` was locked by another `dotnet` process (PID 23272, started 2026-11-27 14:12:32). Remaining gate steps shown passed; the gate overall therefore remains failed/unverified for this run.

## 8 Checks run
- `pwsh -NoProfile -File tools/poteto-gate.ps1 -SelfTest`: PASS.
- `pwsh -NoProfile -File tools/title-hint.ps1 -SelfTest`: PASS.
- `pwsh -NoProfile -File tools/start-card.ps1 -SelfTest`: PASS.
- `pwsh -NoProfile -File tools/roadmap-data.ps1 -Check`: PASS (`ROADMAP DATA OK`, 188 rows, 0 warnings).
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop21-umbrella-items -Offline`: FAIL at Build because `AvaloniaEdit.dll` was in use; Scope, seam, tokens, contract, format, LoopLint and size passed; Selftest skipped because no executable was produced.
- `git diff --check`: PASS.

## 9 Hand checks
No live chats or umbrella chain were started. The child-row behaviors are covered by the requested selftests; roadmap parent normalization is included in the data path checked by `-Check`.
