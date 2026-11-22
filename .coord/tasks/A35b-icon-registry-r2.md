# A35b-icon-registry-r2: build the approved plan (run 1 stopped without changes)

Rework round of `A35b-icon-registry` (`docs/loop.md`). Read the item card `.coord/tasks/A35b-icon-registry.md` first: its Plan, Files,
Contracts and "For Codex" section still apply unless this file says otherwise. Then read
`.coord/reports/A35b-icon-registry-findings.md` (the five icon swaps, the constant comparisons, the non-icon hex values).

Tier: Medium
Role: worker, because run 1 (terra) read everything, confirmed the findings, then stopped with no edits and no reason.

## Why this round
Run 1 wrote a report saying "Not implemented" and changed no file. The plan is approved by the user; implement it now.
A status-only report is a failed run. If one part truly blocks you, build every other part, and say exactly what blocked
and what you tried.

## Change to the plan
None. Concrete guidance for the parts that are most work:
- **Order:** add `UI/Icons.cs`, switch `GlyphIcon`/`Kit.IconButton`/`MenuEntry`/`Fl` to `Icon`, migrate callers, delete
  `Glyph` codes, `MenuGlyphs`, `KeyboardGlyph`, the weight parameter, offset path and `ChromeTokens.PlainIconWeight`;
  build. Then selftest, then scanner. Build after each step.
- **Font families** (`Glyph.Font`, `Glyph.Fluent`) may move into `Icons.cs` (for example `Icons.FontFamily(IconFont)`).
- **Glyph names:** you do not need fontTools. Read the bundled font bytes (`AssetLoader.Open` on
  `AppInfo.FontUri`) and parse the TrueType `cmap` (format 4 or 12) and `post` table version 2.0 yourself (glyph
  index -> name index -> 258 standard Mac names or the Pascal strings that follow). The item chat's working Python
  reader, `.coord/reports/A35-ux-pass-s2-fontcodes.py`, shows the exact layout to port. For Segoe (system font,
  `C:/Windows/Fonts/SegoeIcons.ttf` or Avalonia's `GlyphTypeface` for "Segoe Fluent Icons") check existence only.
- **Registry glyph names** for every Fluent entry: take them from the s2 TSV plus the findings report swaps; confirm each
  with the Python script (`python -I .coord/reports/A35-ux-pass-s2-fontcodes.py <ttf> <HEX ...>`).
- **Scanner:** add category `icon-code` to `tools/check-design-tokens.ps1` that scans the raw file text (string
  contents included) for literal U+E000-U+F8FF characters, `\uE...`/`\uF...` escapes and `ConvertFromUtf32(`, skipping
  `UI/Icons.cs` and the `AppInfo.cs` dev badge line. Bare hex integers are not icon codes. Baseline 0. Add the fixtures
  the card's Test plan lists to `-SelfCheck`. Update the baseline TSV for any count the weight removal changes.

## Files
- `product/AntiNotepad/Icons.cs`
- `product/AntiNotepad/AppInfo.cs`
- `product/AntiNotepad/UI/Kit.cs`
- `product/AntiNotepad/UI/PadBar.cs`
- `product/AntiNotepad/UI/PadMenu.cs`
- `product/AntiNotepad/UI/PadWindow.Bar.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/Editor/MenuEntry.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/SelfTest/Icons.cs`
- `product/AntiNotepad/SelfTest/Run.cs`
- `product/AntiNotepad/SelfTest/Bar.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `tools/check-design-tokens.ps1`
- `tools/design-tokens-baseline.tsv`

## Done means
Offline checks only (`docs/agents.md`, "What Codex can and cannot check").
- The card's "Done means", all of it.
- `pwsh -NoProfile -File tools/check-design-tokens.ps1 -SelfCheck` passes with the new fixtures.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A35b-icon-registry-r2 -Offline` passes.

Report: `.coord/reports/A35b-icon-registry-r2.md`, in the card's report format.
