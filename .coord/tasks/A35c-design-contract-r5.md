# A35c-design-contract-r5: name the oversized files and ratchet them

Rework round of `A35c-design-contract`. Read the card and the r4 report first.

Tier: Medium, same as the card
Role: quick, a small script and one table

## Why this round
Five product C# files are over 600 lines: `Editor/PadView.cs` 1075, `UI/SettingsWindow.cs` 1016, `UI/PadWindow.Layout.cs` 903, `UI/SettingsParts.cs` 698, `UI/PadWindow.cs` 683. Splitting them is its own refactor (item chat decision, 2026-11-21): name them now and make them a ratchet so none grows and no new file crosses 600.

## Change to the plan
Plan item 7 file splits become named exceptions plus a ratchet; the splits are a follow-up item.

## Files
- `tools/size-exceptions.tsv`
- `tools/check-design-contract.ps1`
- `docs/design-contract.md`

## Done means
- `tools/size-exceptions.tsv`: columns file, max lines (today's count), reason (one honest line each, e.g. PadView holds the editor view and its input handling).
- `check-design-contract.ps1` also fails when a `product/**/*.cs` file over 600 lines is not listed, or a listed file exceeds its max, and says "lower the max" when a file shrank. Count lines the way `gate.ps1` `Get-FileLines` does.
- Contract: Known gaps item for the files points at the TSV; add a "Size" rule line (600 lines; exceptions named in the TSV).
- `pwsh tools/check-design-contract.ps1` exit 0; `tools/gate.ps1 -Task A35c-design-contract-r5 -Offline -Base docs/A35-ux-pass` passes.

Report: `.coord/reports/A35c-design-contract-r5.md`, short.
