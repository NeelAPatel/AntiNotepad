# A35c-design-contract-r4: finish the contract doc

Rework round of `A35c-design-contract`. Read the card, `-r2.md`, `-r3.md` and the r3 report first.

Tier: Medium, same as the card
Role: terra, doc work across several files

## Why this round
Selftests now exist (r3 report). The doc is still "(draft)", its Rules are one-liners with no numbers, and `docs/design-scale.md` is still there.

## Change to the plan
None.

## Files
- `docs/design-contract.md`
- `docs/design-scale.md`
- `docs/decisions.md`
- `tools/check-design-contract.ps1`

## Done means
- Contract title has no "draft"; the top note about retaining `design-scale.md` is gone.
- Rules gain the concrete values and reasons that still apply, taken from `design-scale.md` (sections 2, 3.2, 6) and the A35 reports (`.coord/reports/A35-ux-pass-s3.md`, `A35d-*`, `A35e-*`, `A35f-*`, `A35g-*`): spacing steps (Nudge 2, Xs 4, S 8, M 12, L 16, Xl 24, Xxl 32), 32 px control target, corner steps, type ramp, stroke steps, icon sizes, focus ring rule, contrast minima (4.5 text, 3 marks) with the modeled pairs the selftest sweeps, label and tooltip rules. Every number must match the code values in the generated table; where design-scale.md disagrees with the code, the code wins.
- Add a short "How to extend" section: new token (add const, run `-Write`), new one-off (add a row to `tools/design-exceptions.tsv` with a reason, run `-Write`), new custom control (declare it in Custom controls), new screen (add it to the walk in `SelfTest/Design.cs`).
- Add a "Known gaps" section listing: markdown marker text under 3:1 on six palettes (reported by selftest, not changed; user decision pending), the Clear-tint hint 1.24:1 exception, B-9 dimming (planned, lab first), product files over 600 lines (PadView, SettingsWindow, PadWindow.Layout, SettingsParts, PadWindow; planned split or named).
- `docs/design-scale.md` deleted; `docs/decisions.md` and every non-archive doc point at `docs/design-contract.md` instead (do not edit `.coord/` history files).
- `pwsh tools/check-design-contract.ps1` and `pwsh tools/check-design-tokens.ps1` exit 0; `tools/gate.ps1 -Task A35c-design-contract-r4 -Offline -Base docs/A35-ux-pass` passes.

Report: `.coord/reports/A35c-design-contract-r4.md`, short.
