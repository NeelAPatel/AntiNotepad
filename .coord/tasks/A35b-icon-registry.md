# A35b-icon-registry: every icon is the one its name says, in the right font, and a check fails if not

Tier: Medium · ui-shared-state
State: hand test
Branch: `feat/A35b-icon-registry` in `.claude/worktrees/feat/A35b-icon-registry` (from `docs/A35-ux-pass`)

## Request
Via A35-ux-pass chat, 2026-11-20: "Yes both". Additions: "we dont need faux-icon-bolding if theres no native heavier weight icon"; "There is also Microsoft Codicons (or something like that)".

## Findings
- **Kind and why:** polish + bug class. A29 drew a wrong icon silently: 32 of 55 codes exist in both icon fonts.
- **Now:** raw PUA strings and ints, no font tie, no check; `.coord/reports/A35b-icon-registry-findings.md`.
- **Risk:** menu colors and pinned fill compare icon constants.

## Plan
- **After:** `Icons.cs` (root): `enum IconFont { Segoe, Fluent }`, `sealed record Icon(string Name, IconFont Font, int Code, string? FontGlyphName, string Meaning)`, fields, `Icons.All`. `GlyphIcon(Icon, Color, double size)` takes the font from the icon; weight parameter, offset path and `ChromeTokens.PlainIconWeight` deleted. `Kit.IconButton`, `MenuEntry.Icon` (`Icon?`), `Fl(Icon)`, sidebar tuples take `Icon`. `Glyph` (codes), `MenuGlyphs`, `KeyboardGlyph` deleted in the same change. Menu colors and pinned fill use reference checks against `Icons.*`.
- **Look:** five lab swaps (findings). No Codicons. No lesson; checks encode it (parent, 11-20).
- **Must not change:** every other icon's code and size, menu icon colors, pinned fill layer, dev badge.
- **Test plan:** selftest `Icons`: each `Icons.All` code exists in its font; Fluent entries' `post` glyph name equals `FontGlyphName` (parse the bundled font asset bytes); Segoe entries existence only. CONTRACTS row for A29. Scanner self-check fixtures: PUA char / `` / `ConvertFromUtf32(0xF6ED)` outside `Icons.cs` fail; `0xF120`, `0xFFFF`, `ContainsGlyph(0xE0A0)`, `AppInfo.cs` dev badge pass. Hand: Settings rows, search box, chevrons, pad bar, menus look as the lab.
- **Gates:** selftest (codes, names); scanner `icon-code` = 0 (reads string contents); gate -Offline; Codex review.

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

## Contracts
- New: A29-icon-codes: every icon code is in its own font with its glyph name.

## Approve
"Approve" (user, 2026-11-20, after lab)

## For Codex
Role: terra, typed migration over ~15 files.
Read `AGENTS.md`, coding rules, findings, Files. Never switch, merge, commit.
- Build exactly the Plan "After". If the Findings are wrong, stop and report.
- Build into `build/A35b-icon-registry-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, delete it. Gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A35b-icon-registry -Offline`.
- Never launch or kill AntiNotepad, never `--quit`, never edit STATUS or this card. Offline only.
- Done means: no `Glyph.`/`MenuGlyphs`/`Fl(0x`/PUA literal outside `Icons.cs`; GlyphIcon has no weight; selftest checks pass; scanner fixtures behave; 0 warnings; gate passes.

Report `.coord/reports/A35b-icon-registry.md` with the template's 9 sections.

## Check
Item chat accepted. r2 review finding fixed by the item chat, not re-reviewed; r3 not Codex-reviewed (code swaps; diff read). `.coord/reports/A35b-icon-registry-check.md`.

## Hand test
"All good" (user, 2026-11-21, r3).
