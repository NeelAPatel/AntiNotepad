# A4c-type-tokens-r2: format the lines round 1 touched

Rework round of `A4c-type-tokens` (`docs/loop.md`). Read the item card `.coord/tasks/A4c-type-tokens.md` first: its Plan, Files,
Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Low, same as the card
Role: terra, because it is a small mechanical edit across five files that must not change any value

## Why this round
`tools/gate.ps1 -Task A4c-type-tokens -Base feat/A4-design-tokens` fails Format: 35 `WHITESPACE` problems on lines round 1
touched (the formatter wants one object-initializer entry per line; the old lines had this problem too, but a touched line
counts as new). Build, Selftest (DESIGN hash unchanged), Seam, Tokens, Scope and Size pass. Flagged lines (file:line, the gate
also counts the line after):
- `UI/HelpOverlay.cs`: 38
- `UI/PadWindow.Layout.cs`: 217, 242, 486
- `UI/SearchOverlay.cs`: 51, 181, 182, 301
- `UI/SettingsParts.cs`: 26, 31
- `UI/SettingsWindow.cs`: 185, 186, 547, 548, 560

## Change to the plan
None. Layout only: split those initializer lines so each entry that `dotnet format` flags starts its own line, with the
indent the formatter asks for. No value, token or ordering change.

## Files
Files added for this round only: none; the card's Files apply (only the five UI files above change).
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`

## Done means
- From `product/AntiNotepad`: `dotnet format AntiNotepad.csproj --verify-no-changes --verbosity quiet --include <each of the five files>`
  reports no problem on any line round 1 touched (lines containing `Typography.`, or the line before). Do NOT run
  `dotnet format` in write mode on whole files: older problems elsewhere stay as they are.
- Keep CRLF line endings in every file (the worktree files are CRLF; write them back as CRLF).
- Build 0 warnings, `SELFTEST PASS` with `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160`,
  `pwsh -NoProfile -File tools/gate.ps1 -Task A4c-type-tokens-r2 -Offline -Base feat/A4-design-tokens` passes.

Report: `.coord/reports/A4c-type-tokens-r2.md`, in the card's report format.
