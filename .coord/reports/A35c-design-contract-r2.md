# A35c-design-contract-r2: Run report

## Summary

Partial; not ready for Check. The generated contract table now includes source expressions and icon font/code, and the named exception table is generated and checked against the TSV. Custom control declarations have been expanded. The rendered-tree membership selftest and contrast additions remain unwritten; no off-token failure was demonstrated. The scale document remains in place and the contract remains draft because reconciliation is incomplete. Computed constants are shown as expressions, not evaluated values. No design dump hash change was made.

## Files changed

- `tools/check-design-contract.ps1`: generate and verify value expressions and TSV exception rows.
- `docs/design-contract.md`: generated table and expanded custom-control descriptions.
- The run 1 changes remain in the worktree; see `.coord/reports/A35c-design-contract.md`.

## Exceptions list with reasons

The TSV rows are unchanged:

```tsv
file	category	count	reason
product/AntiNotepad/Editor/Motion.cs	radius	2	Caret artwork uses a small radius proportional to block or bar geometry.
product/AntiNotepad/Editor/PadView.cs	color	1	Black RGB is the alpha mask for the editor edge gradient.
product/AntiNotepad/Editor/Renderers.cs	radius	4	Drawn pause glyph uses small rounded bars; badge uses the shape token.
product/AntiNotepad/Editor/Renderers.cs	stroke	1	Drawn editor artwork uses a proportional 1.6 px pen.
product/AntiNotepad/UI/NoteSwipe.cs	spacing	1	Minimum denominator guards swipe calculation; not a layout token.
product/AntiNotepad/UI/PadBar.cs	color	1	Black RGB is the shadow color; alpha uses the shadow tokens.
product/AntiNotepad/UI/PadMenu.cs	color	1	Black RGB is the shadow color; alpha uses the shadow tokens.
product/AntiNotepad/UI/PadMenu.cs	spacing	1	Conditional spacing uses Space.Nudge; numbers are array indices.
product/AntiNotepad/UI/PadWindow.Layout.cs	font-size	2	Scanner matches button widths after tokenized text, not font sizes.
product/AntiNotepad/UI/PadWindow.Navigation.cs	spacing	1	Window width is calculated from screen bounds and scale.
product/AntiNotepad/UI/PadWindow.cs	opacity	1	Content reveal animation interpolates its starting opacity.
product/AntiNotepad/UI/PadWindow.cs	spacing	1	Split window width is derived from the current width.
product/AntiNotepad/UI/SettingsParts.cs	color	1	Black RGB is the popup shadow color; alpha uses the shadow tokens.
product/AntiNotepad/UI/SettingsParts.cs	font-size	1	Scanner matches thickness after tokenized icon size, not a font size.
product/AntiNotepad/UI/SettingsParts.cs	radius	9	Round swatch and drawn sample artwork use geometry-specific radii.
product/AntiNotepad/UI/SettingsWindow.cs	font-size	1	Scanner matches thickness after tokenized icon size, not a font size.
*	timer	12	Timing and motion durations are outside the visual token ratchet.
*	tween	0	Timing and motion durations are outside the visual token ratchet.
```

The Clear-tint hint remains the documented contrast exception.

## Checks run

- `pwsh tools/check-design-tokens.ps1`: pass.
- `pwsh tools/check-design-contract.ps1 -Write` and normal check: pass.
- `tools/gate.ps1 -Task A35c-design-contract-r2 -Offline -Base docs/A35-ux-pass`: pass, including build with 0 warnings and `SELFTEST PASS`, Scope and Cleanup. This gate does not prove the missing checks exist. See `.coord/reports/A35c-design-contract-r2.gate.md`.
- Requested isolated build and deliberate off-token failure were not run.
