# T12-settings-drift-readers: report

Changed (commit fefb773):
- `tools/catch-up.ps1`: dirty-checkout regex now also allows `.claude/settings.json`; refusal text names both files.
- `tools/merge-preflight.ps1`: same regex for `dirty item checkout`; CHECK line says "except STATUS and settings.json drift".
- `tools/gate.ps1`: same regex, so settings.json drift no longer appends `-dirty` to the report SHA.

Tests:
- Regex check on sample porcelain lines: allows ` M .claude/settings.json`, ` M .coord/STATUS.md`, `M  .coord/STATUS.md`; rejects `tools/gate.ps1`, `settings.json.bak`, `x/.claude/settings.json`.
- `catch-up.ps1` live: with only settings.json modified -> UP TO DATE; with README.md modified -> REFUSED (new text).
- `merge-preflight.ps1` only runs from the main checkout, so it was covered by the regex check, not run live.
- `gate.ps1 -Task T12-settings-drift-readers -Offline`: GATE PASS (report `.coord/reports/T12-settings-drift-readers.gate.md`).
- Temporary edits reverted; tree clean.
