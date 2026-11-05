# A4c-type-tokens report

## Summary
Added shared typography constants and replaced matching font values at the approved call sites. The design dump remains unchanged; visual hand checks remain for the user.

## Change list
Counts are token references replacing literals; baseline and new catalog have no swaps.

| File | Literals swapped (count) | Tokens used |
|---|---:|---|
| Theme/Typography.cs | 0 | All planned family, weight, text and icon constants defined |
| UI/SettingsParts.cs | 31 | Body, Display, IconMedium, IconSmall, IconXLarge, Label, Large, MonoFamily, Regular, Small, Strong, UiFamily |
| UI/SettingsWindow.cs | 15 | Body, Caption, IconInput, IconLarge, Large, Regular, Small, Strong, UiFamily |
| UI/SearchOverlay.cs | 12 | BodyEmphasis, Caption, Input, Micro, MonoFamily, Small, Strong |
| UI/Kit.cs | 10 | Body, IconBody, Label, Micro, PanelTitle, Regular, Strong |
| UI/PadWindow.Layout.cs | 12 | DialogTitle, Label, Regular, Strong |
| UI/PadWindow.cs | 1 | UiFamily |
| UI/PadMenu.cs | 5 | Body, IconMedium, Large, UiFamily |
| UI/MotionPreview.cs | 3 | Caption |
| UI/HelpOverlay.cs | 3 | Caption, Label, MonoFamily |
| Editor/Colorizers.cs | 6 | Bold, Strong |
| Editor/Completion.cs | 4 | Small, Strong |
| Editor/Renderers.cs | 2 | MonoFamily, Strong |
| Editor/Elements.cs | 1 | Regular |
| Editor/PadView.cs | 1 | EditorFallback |
| tools/design-tokens-baseline.tsv | 0 | Updated inventory baseline |

Product paths above are relative to product/AntiNotepad.

## Behavior changes
None.

## Out-of-scope files
None for product changes. This requested report and the gate-generated report are coordination artifacts outside the Files list.

## Plan deviations
The inventory retains 10 false-positive font-size matches: Kit.cs lines 93 and 99 (existing ChromeTokens size followed by glyph weight 0); PadMenu.cs line 204; SettingsParts.cs lines 132, 229, 407; SettingsWindow.cs lines 187, 219 (token size followed by glyph weight 0); PadWindow.Layout.cs lines 491 and 494 (token size followed by button widths 88 and 104). No font-size literal is represented by those matches. Icon font resource and system-family identifiers in Kit.cs and glyph-discovery family names in PadWindow.Layout.cs remain literal: the approved catalog has no equivalent constants and changing these identifiers would require additional tokens. Settings font-size choices and user-adjustment bounds are settings data, not applied fixed typography sizes.

## Contracts
None.

## Lens review
The reviewer found the scanner missed PadMenu's target-typed UI family; replaced it with UiFamily. The reviewer found no value changes in a token-expansion comparison of all call-site files, and noted the retained icon and discovery identifiers documented above.

## Checks run
- Build: PASS, 0 warnings and 0 errors, offline restore with NuGet audit disabled. An initial no-restore attempt failed because assets were absent; offline restore resolved it.
- Selftest: `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160`; `SELFTEST PASS`.
- Offline gate: Build, Selftest, Seam, Tokens, Format, Scope, Size, Cleanup PASS; LoopLint SKIP; `GATE: PASS`. Theme-to-UI and Theme-to-Editor both 0 references.
- Inventory before -> after: font-size 60 -> 10 (false positives above); font-weight 38 -> 0; font-family 7 -> 0. Baseline updated.

## Hand checks for the user
In the dev build, inspect pad text and headings, a math/list/code note, completion popup, search (Ctrl+F), help, pad menu, and three Settings sections. Confirm nothing looks different.
