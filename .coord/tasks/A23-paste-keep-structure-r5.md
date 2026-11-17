# A23-paste-keep-structure-r5: review fixes (crash, speed, double paste, emoji width)

Rework round of `A23-paste-keep-structure` (`docs/loop.md`). Read the item card and `-r2` to `-r4` briefs first, then `.coord/reports/A23-paste-keep-structure-r4.review.md` (the proven findings with probes). Plan, Files, Contracts and "For Codex" still apply unless this file says otherwise.

Tier: Medium, same as the card.
Role: terra, because it is four contained fixes in the paste files.

## Why this round
Independent review of r4 returned `VERDICT: rework`; user, 2026-11-17: "fix all 4". The hand test (lists, tables, marks, toast, with A22 merged) passed and its behavior must not change.

## Change to the plan
Fix exactly these, smallest change each:
1. **Stack overflow on deep HTML** (`PasteHtml.cs`): reject nesting deeper than a fixed limit (for example 64 levels) while parsing, before any recursive walk; the engine then returns no conversion (normal paste). Selftest: 20,000 nested `<span>` then `<b>A</b>` returns null, no crash.
2. **Quadratic parse** (`PasteHtml.cs:40`, `html[pos..]` copies the suffix per tag): scan the original string by offset, no suffix copies. Add an input budget (for example HTML over 1 MB, or more than 2,000 table rows, returns no conversion). Selftest: a 4,000-row table converts in well under a second; an over-budget input returns null.
3. **Overlapping Ctrl+V** (`PadView.PasteAsync`): serialize paste requests (a second request waits for the first) and capture document, selection and caret when each request begins its own work, so two quick presses insert twice. Keep the guard that drops a paste when the user moved or edited in the meantime. Selftest only if a pure part can be tested offline; otherwise say so in the report.
4. **Emoji width** (`PasteTable.cs` `Width`): measure grapheme clusters (`StringInfo` text elements) as units; a cluster containing an emoji (regional-indicator pair, skin-tone modifier, VS16 presentation, ZWJ sequence, keycap) is 2 columns. Selftests: `🇺🇸`, `👍🏽`, `❤️`, `👩‍💻`, `🟢` are each width 2; plain letters stay 1.

Also remove the trailing blank lines at EOF in the three new Paste files (`git diff --check`).
Do not change converted output, the toast strings or guards. All earlier paste selftests (captures A to E, tables, marks) must pass unchanged.

## Files
- `product/AntiNotepad/Editor/PasteHtml.cs`
- `product/AntiNotepad/Editor/PasteTable.cs`
- `product/AntiNotepad/Editor/PasteEngine.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/SelfTest/Paste.cs`

## Done means
Offline checks only (`docs/agents.md`, "What Codex can and cannot check").
- The four selftests above pass, and all earlier paste selftests pass.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A23-paste-keep-structure-r5 -Offline` passes.

Report: `.coord/reports/A23-paste-keep-structure-r5.md`, in the card's report format.
