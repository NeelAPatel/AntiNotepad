# A4d-shape-tokens-r2: fix the Format gate on the touched lines

Rework round of `A4d-shape-tokens` (`docs/loop.md`). Read the item card `.coord/tasks/A4d-shape-tokens.md` first: its Plan, Files,
Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Low (same as the card)
Role: terra, because it is a mechanical format fix on a value-preserving swap

## Why this round
Round 1 values and DESIGN hash are accepted (DESIGN `8123f5c3...1de55b 16160`, SELFTEST PASS). The online gate fails only on Format:
`Format: FAIL - 35 format problem(s) on changed lines`, all WHITESPACE ("Replace 1 characters with '\r\n' + indent"), on
these lines: `UI/PadWindow.Layout.cs:511,512`, `UI/SearchOverlay.cs:54,55,198`, `UI/SettingsParts.cs:57,58,247,248,314,346,366,387`,
`UI/SettingsWindow.cs:573`. The formatter wants one object-initializer entry per line on every initializer line the swap
touched (the offline gate skipped this step). See `.coord/reports/A4d-shape-tokens.gate.md`, section Format.

Also: delete `build/A4d-shape-tokens-check` if it still exists (it may already be gone).

## Change to the plan
None. Reflow only the touched initializer lines (one entry per line, as `dotnet format` asks; run
`dotnet format whitespace --include <file>` on the four files if it works offline, then check the diff touches only those lines).
No value, token or semantic change. Keep CRLF endings (verify every changed file is 100% CRLF at the end).

## Files
Same as the card; no additions.

## Done means
- `pwsh -NoProfile -File tools/gate.ps1 -Task A4d-shape-tokens-r2 -Base feat/A4-design-tokens -Offline` shows Format PASS
  (if the Format step is skipped offline, run `dotnet format whitespace product/AntiNotepad --verify-no-changes --include <the four files>` and report it).
- `--selftest` still prints `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160` and `SELFTEST PASS`; Build 0 warnings.
- Diff against round 1 is whitespace/line breaks only.

Report: `.coord/reports/A4d-shape-tokens-r2.md`, in the card's report format.
