# Loop5-reply-style: report

Builder: Claude, fast lane, no Codex. Edited by hand.

- Added `.claude/output-styles/antinotepad.md` (57 lines) and `"outputStyle": "AntiNotepad"` in `.claude/settings.json`.
- `docs/loop.md` "Status card": one-line default, full card on phase change / first reply / decision, "Waiting on you" is the decision block or "nothing", pointer to the style.
- `docs/glossary.md`, `docs/wiki/Getting-Started.md`, `tools/loop-card.ps1`: match the new card.
- Gate: PASS (`.coord/reports/Loop5-reply-style.gate.md`). Hook output checked.
- Hand test: user reads a reply from a session started after the merge.
