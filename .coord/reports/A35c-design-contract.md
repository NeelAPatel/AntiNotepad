# A35c-design-contract: Run 1 report

## Summary

Partial implementation; not ready for Check. The design contract is a draft. The rendered-tree scale check, extended contrast checks, complete custom-control declarations, token values and full reconciliation of the scale document remain outstanding. The original scale document remains in place to avoid losing its detail. No B-9 or file splits were attempted.

## Files changed

- `docs/design-contract.md`: draft rules, named historical exceptions and generated name registry.
- `tools/check-design-contract.ps1`, `tools/gate.ps1`: registry generator/check and gate step.
- `tools/check-design-tokens.ps1`, `tools/design-exceptions.tsv`, removed `tools/design-tokens-baseline.tsv`: reasoned, per-file literal ratchet with motion category allowances.
- `product/AntiNotepad/Editor/Renderers.cs`, `Editor/Structure.cs`, `UI/MotionPreview.cs`, `UI/SettingsParts.cs`: exact-value substitutions with existing tokens.
- `tools/playground/a35d-lab/core.css`, removed lab font copy: reference product font.

## Exceptions list with reasons

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

## Checks run

- Offline isolated build: pass, 0 warnings and errors; `--selftest`: SELFTEST PASS. Design dump hash remains `ac08aea9779b8398a054f60b94cc833a9d28df467850dd5e9d662d7b3f012c22`.
- Token scanner selfcheck and normal check: pass. Contract check: pass.
- Survival: failed with the 23 previously identified missing lines.
- Gate: sequential rerun passed Build, Selftest, Seam, Tokens, Contract, Format, LoopLint and Size; failed Scope because the item card Files section contains no backticked file paths. The card is outside Codex's permitted edits. The first run had a build file lock caused by concurrent builds. See `.coord/reports/A35c-design-contract.gate.md`.
- The requested isolated build output was deleted.
