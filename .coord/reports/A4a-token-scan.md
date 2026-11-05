# A4a-token-scan report

## Summary
The scanner inventories brush opacity arguments and literal byte alpha arguments alongside the existing design values. The regenerated baseline and inventory contain 412 hits.

## Change list
| File and lines | Change | Plan |
|---|---|---|
| `tools/check-design-tokens.ps1:10-27` | Brush opacity and alpha patterns | Scan categories |
| `tools/check-design-tokens.ps1:320-329` | Brush, alpha, token and endpoint fixtures | SelfCheck |
| `tools/design-tokens-baseline.tsv:1` | Regenerated counts | Baseline |
| `.coord/reports/A4a-token-scan-inventory.tsv:1` | Regenerated hits | Inventory |
| `tools/gate.ps1:342-350` | Tokens gate step | Gate integration |
| `docs/loop.md:67` and `docs/loop.md:317-318` | Gate menu and files | Documentation |
| `docs/coding.md:47-48` | Design values guidance | Documentation |

Counts: radius 25, font-family 7, font-size 60, font-weight 38, color 8, shadow 4, opacity 45, alpha 7, tween 13, timer 14, spacing 191.

Top ten files, relative to `product/AntiNotepad/`: `UI/SettingsParts.cs` 124; `UI/PadWindow.Layout.cs` 51; `UI/SettingsWindow.cs` 49; `UI/SearchOverlay.cs` 38; `UI/PadMenu.cs` 31; `UI/Kit.cs` 28; `UI/MotionPreview.cs` 18; `Editor/PadView.cs` 12; `UI/PadWindow.cs` 11; `UI/HelpOverlay.cs` 11.

## Behavior changes
The scanner and gate ratchet count brush opacity and byte alpha; named tokens and endpoints are excluded. Five alpha calls retain their pre-existing color hits: `UI/Kit.cs:99`, `UI/SettingsParts.cs:20`, both calls at `:392`, and `:397`. The two conditional alpha calls were not color hits in r2. No product behavior or settings changed.

## Out-of-scope files
`.coord/reports/A4a-token-scan.md`: required report. The gate writes its ignored report and temporary build outputs.

## Plan deviations
The dedicated build folder is retained per the r2 brief. Counts are regex-based occurrences rather than a full semantic inventory: arbitrary positional font sizes in helpers other than `Text` and `GlyphIcon`, target-typed font constructors, and numeric geometry in general drawing primitives are omitted because their meaning cannot be identified reliably by these patterns. `Colors.Name` is excluded as requested. Brush detection handles simple argument expressions and conditional literals, not nested calls in arguments. The tween pattern covers duration properties and `Anim.Run`/`Tween` arguments, including direct `TimeSpan.From...` calls.

## Contracts
None.

## Lens review
Self-review followed the lens checklist; no proven remaining findings. The scanner retains r2 color classification and counts each qualifying conditional as one alpha hit. No agent was spawned.

## Checks run
- Dedicated build with `NuGetAudit=false`: 0 warnings, 0 errors; dedicated `--selftest`: `SELFTEST PASS`, `CONTRACTS: 13 rows, 12 pass, 1 manual`.
- `-SelfCheck`: `TOKENS SELFCHECK PASS`; default: `TOKENS PASS`; baseline hash unchanged by a second `-Update`.
- Temporary `new CornerRadius(5)` product probe: exit 1 with file, category, counts and guidance; original bytes restored and hash verified.
- Offline gate: `GATE: PASS`, including Build, Selftest, Seam, Tokens, Format, Scope, LoopLint and Size.

## Hand checks for the user
None (not visible).
