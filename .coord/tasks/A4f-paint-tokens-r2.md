# A4f-paint-tokens-r2: fix the Format gate on the touched lines

Rework round of `A4f-paint-tokens` (`docs/loop.md`). Read the item card `.coord/tasks/A4f-paint-tokens.md` first: its Plan, Files,
Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium (same as the card)
Role: terra, because it is a mechanical format fix on a value-preserving swap

## Why this round
Round 1 values and DESIGN hash are accepted (DESIGN `8123f5c3...1de55b 16160`, SELFTEST PASS, Tokens alpha 0 / opacity 2). The online gate
fails only on Format: 10 problems on changed lines, all WHITESPACE ("Replace 1 characters with '\r\n' + indent"), at
`UI/Kit.cs:98`, `UI/PadWindow.Layout.cs:518` (x2), `UI/SearchOverlay.cs:206`, `UI/SettingsParts.cs:132` (x2), `:386`, `:410`, `:421`, `:426`.
The formatter wants one object-initializer entry per line on every initializer line the swap touched (the offline gate skips
this step). See `.coord/reports/A4f-paint-tokens.gate.md`, section Format.

Also: delete `build/A4f-paint-tokens-check` if it still exists.

## Change to the plan
None. Reflow only those initializer lines (one entry per line). Run
`dotnet format whitespace --include <file>` on the four files if it works offline, then check the diff against round 1 touches only those lines
(do not accept reformatting of older lines). No value, token or semantic change. Keep CRLF endings (verify every changed file is 100% CRLF at the end).

## Files
Same as the card; no additions.

## Done means
- `pwsh -NoProfile -File tools/gate.ps1 -Task A4f-paint-tokens -Base feat/A4-design-tokens -Offline` shows Format PASS
  (if skipped offline, run `dotnet format whitespace product/AntiNotepad --verify-no-changes --include <the four files>` and report it).
- `--selftest` prints `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160` and `SELFTEST PASS`; Build 0 warnings.
- Diff against round 1 is line breaks only. Do not touch `.coord/STATUS.md` or the card.

Report: `.coord/reports/A4f-paint-tokens-r2.md`, in the card's report format.
