# A4g-control-groups: control map

This is a source-based map of the inspected instances, not a complete inventory. Values are DIPs unless noted; dark/light pairs are in that order. Unspecified values may be inherited, template-provided, content-sized, or not inspected; they are not necessarily zero. No design dump was run.

## Proposed group names

Text button, icon button, toggle, dropdown, text field, slider, card, preview tile, swatch, popup and menu, badge, pill, overlay panel, toast, text roles.

## Text buttons

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `SettingsParts.TextButton` | `SettingsWindow.HotkeyEditor`, About and storage pages | - | Shape.Control | literal label margin 12,5; button padding 0 | Typography.Body; Typography.Strong / Typography.Regular | Accent alpha literal 1 primary, Paint.HoverFace .86 hover; Line alpha literal .09/.10 secondary, literal border 1 | - |
| `PadWindow.Layout` save failure actions | Retry, Quit anyway | literal 88×34 / 104×34 | literal 7 | literal 0 | Typography.Label; Typography.Strong / Typography.Regular | literal transparent; Paint.Hover / Paint.Press | - |
| `PadWindow.Layout.Toast` action | Undo and other toast actions | - | literal 7 | literal label margin 6,1 | Typography.Label; Typography.Strong | literal transparent; Paint.Hover / Paint.Press | - |

**Disagreements:** `SettingsParts.TextButton` uses `Typography.Body` (13), label margin 12,5 and a secondary .09/.10 fill with a 1 border; `PadWindow.Layout` save-failure actions use `Typography.Label` (12.5), zero padding and transparent fill. `PadWindow.Layout.Toast` actions use label margin 6,1, unlike 12,5 in `SettingsParts.TextButton`.

## Icon buttons

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `Kit.IconButton` | pad top bar, `Kit.PanelHeader` | ChromeTokens.ButtonSize | ChromeTokens.TopIconHoverRadius | literal 0 | ChromeTokens.IconGlyphSize; literal glyph weight 0 | literal transparent; Paint.Hover / Paint.Press | - |
| `Kit.IconButton` | pager previous / next | ChromeTokens.ButtonSize | ChromeTokens.PagerArrowHoverRadius | literal 0 | ChromeTokens.IconGlyphSize; literal glyph weight 0 | literal transparent; Paint.Hover / Paint.Press | - |
| `Kit.IconButton` | `SearchOverlay` tools | literal 32 | ChromeTokens.TopIconHoverRadius | literal 0 | ChromeTokens.IconGlyphSize; literal glyph weight 0 | literal transparent; Paint.Hover / Paint.Press | - |
| `Kit.IconButton` | `SettingsWindow.Build` close | literal 34 | ChromeTokens.TopIconHoverRadius | literal margin 0,0,6,0; padding 0 | ChromeTokens.IconGlyphSize; literal glyph weight 0 | literal transparent; Paint.Hover / Paint.Press | - |

**Disagreements:** `Kit.IconButton` in `SettingsWindow.Build` is 34 square with right margin 6; the `Kit.IconButton` in `SearchOverlay` is 32 square with zero padding. Pager and top-bar radius tokens have different names but both are 8.

## Toggle

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `SettingsParts.Toggle` | Settings switches | literal track 40×20; knob 14×14 | Shape.Popup; Shape.Round(20); Shape.Round(14) | literal face margin 8,4; knob 3,0; spacing 12 | Typography.Body; Typography.Regular | Accent alpha literal 1 on; transparent off, Faint alpha literal 1 border 1.5 off | - |
| `SettingsParts.Seg` | Settings choices | - | Shape.Control | literal label margin 12,5; button 0,0,2,0 | Typography.Body; Typography.Strong / Typography.Regular | Accent Paint.Selected selected; literal transparent otherwise | - |
| `Kit.Toggles` | Settings code languages | - | literal 8 | literal label margin 10,4; button 0,0,4,4 | Typography.Label; Typography.Strong / Typography.Regular | Accent Paint.Selected selected; literal transparent otherwise | - |
| `Editor/Renderers.CheckboxRenderer.Draw` | editor checkbox lines | literal round(Settings.FontSize × .86) | literal size × .28 | - | - | Done alpha literal 1 checked; Accent Paint.AccentPen stroke literal 1.6 unchecked; Canvas alpha literal 1 check stroke 1.9 | - |

**Disagreements:** `SettingsParts.Toggle` uses a 40×20 round track and `Typography.Body` (13); `SettingsParts.Seg` uses radius 7 and Body text. `Kit.Toggles` uses radius 8 and `Typography.Label` (12.5), versus radius 7 and Body in `SettingsParts.Seg`. `CheckboxRenderer.Draw` sizes its radius as checkbox size × .28 and uses a 1.6 unchecked stroke, unlike the switch and choice controls.

## Dropdown

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `SettingsParts.Dropdown` face | Settings palette, font, scale, timing and other choices | literal height 32, default min width 160 | Shape.Control | literal face margin 12,0,10,0 | Typography.Body; Typography.Regular; chevron Typography.IconSmall | Surface alpha literal 1; Line alpha literal .09/.10, border literal 1 | - |
| `SettingsParts.Dropdown` option | dropdown options | - | literal 6 | literal margin 10,6 | Typography.Body; Typography.Strong / Typography.Regular | Accent Paint.Selected selected; literal transparent otherwise | - |

**Disagreements:** `SettingsParts.Dropdown` face uses radius `Shape.Control` (7), Surface fill and margin 12,0,10,0; its option uses radius 6, selected Accent `Paint.Selected` fill and margin 10,6.

## Text field

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `SearchOverlay` query | search and trash | - | Shape.Card | literal 12,8 | Typography.Input; default | Surface alpha literal 1; Faint Paint.FieldBorder border literal 1; focus style Paint.FocusBorder | - |
| `SettingsWindow.Build` find | Settings search | - | Shape.Popup | literal 8,7,8,0 | Typography.Body; default | Kit.InputStyles: Surface alpha literal 1; Faint Paint.RestBorder / Accent Paint.FocusBorder | - |
| `SettingsParts.ReadOnlyPath` | Settings notes folder | literal min height 38, max height 68 | Shape.Control | literal 10,7 | Typography.Body; default | Kit.InputStyles: Surface alpha literal 1; Faint Paint.RestBorder / Accent Paint.FocusBorder | - |
| `SettingsWindow.HotkeyEditor` | Settings hotkeys | - | Shape.Control | default | Typography.Body; default | Kit.InputStyles: Surface alpha literal 1; Faint Paint.RestBorder / Accent Paint.FocusBorder | - |

**Disagreements:** `SearchOverlay` query uses radius 10, `Typography.Input` (15) and padding 12,8; `SettingsWindow.Build` find uses radius 8, `Typography.Body` (13) and padding 8,7,8,0. `SettingsParts.ReadOnlyPath` uses radius 7 and padding 10,7; `SettingsWindow.HotkeyEditor` also uses radius 7 but default padding. Search uses `Paint.FieldBorder`; the other rows use `Kit.InputStyles` rest and focus borders. Template styling still needs runtime verification.

## Slider

The search `rg -n "\bSlider\b|\bRangeBase\b|\bTrackBar\b" product/AntiNotepad/UI product/AntiNotepad/Editor` found no matches; Settings uses discrete Seg/Dropdown choices. No slider token is proposed without an actual instance.

## Card

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `SettingsParts.Card` | Settings pages and search matches | - | Shape.Card | - | - | Surface Paint.CardSurface; Ink literal .09/.10 border literal 1 | - |
| `SettingsParts.Row` | Settings cards | literal min height 62 | - | literal 16,10 | Typography.Large / Typography.Small; default | border assigned by Card: Ink literal .09/.10, literal 0 / 1 | - |
| `SettingsParts.SubRow` | Settings expanders | literal min height 54 | - | literal 52,10,16,10 | Typography.Large; default | Ink Paint.RowWash; border assigned by Card | - |
| `SearchOverlay.Card` | search and trash results | literal grid height 150; width CardWidth(), list content-sized | Shape.Card | literal 12,9; grid margin 0,0,8,8 | Typography.BodyEmphasis Strong; Typography.Small default; Typography.Micro | Surface Paint.ResultSurface or Selection literal 1; Faint Paint.ResultBorder or Accent Paint.ResultCurrent, border literal 1 / 1.5 | - |
| `MotionPreview` frame | Settings motion previews | literal editor height 220 | Shape.Card | literal outer margin 0,0,0,4 | - | Canvas literal 1 / Paint.GlassPreview; Ink Paint.PreviewFrame, border literal 1 | - |

**Disagreements:** `SettingsParts.Card` uses `Paint.CardSurface` and a 1 Ink .09/.10 border; `SearchOverlay.Card` uses `Paint.ResultSurface` or Selection and a 1 or 1.5 Faint/Accent border. `SettingsParts.Row` padding is 16,10 versus 52,10,16,10 in `SettingsParts.SubRow` and 12,9 in `SearchOverlay.Card`. `MotionPreview` frame uses Canvas or `Paint.GlassPreview` and `Paint.PreviewFrame`, unlike the Settings and search cards.

## Preview tile

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `SettingsParts.Tile` | `SettingsWindow`: caret Bar/Block | width 58 literal; picture height 34 literal | Shape.Popup 8; inner 8 minus ring | ring 2 selected / 1.5 otherwise literal; button 4 literal | Typography.Small 12; Strong selected / Regular otherwise | selected Accent; otherwise Line Ink .09/.10 literal; border ring 2/1.5 literal | unspecified |
| `SettingsParts.Tile` | `SettingsWindow`: material | width 78, picture height 46 literal | Shape.Popup 8; inner 8 minus ring | ring 2/1.5 and button 4 literal | Typography.Small 12; Strong/Regular | selected Accent; otherwise Line Ink .09/.10 literal; border ring 2/1.5 literal | unspecified |
| `SettingsParts.Tile` | `SettingsWindow`: paper | width 60, picture height 40 literal | Shape.Popup 8; inner 8 minus ring | ring 2/1.5 and button 4 literal | Typography.Small 12; Strong/Regular | selected Accent; otherwise Line Ink .09/.10 literal; border ring 2/1.5 literal | unspecified |
| `SettingsParts.Tile` | `SettingsWindow`: open location | width 104, picture height 62 literal | Shape.Popup 8; inner 8 minus ring | ring 2/1.5 and button 4 literal | Typography.Small 12; Strong/Regular | selected Accent; otherwise Line Ink .09/.10 literal; border ring 2/1.5 literal | unspecified |
| `SettingsParts.CaretPicture` | `SettingsWindow` via `Tile` | caret 2 or 8 by 16, face height 34 literal | Shape.Hairline | caret left 1 literal | Typography.Large 14; weight unspecified | Canvas; Caret at 1 literal or Paint.CaretBlock | unspecified |
| `SettingsParts.MaterialPicture` | `SettingsWindow` via `Tile` | height 46 literal | unspecified | unspecified | none | Canvas; acrylic gradient Paint.SampleGradientAlpha, mica Paint.SampleFadeAlpha to transparent, solid Canvas | unspecified |

**Disagreements:** `SettingsParts.Tile` for caret, material, paper and open location uses widths 58, 78, 60 and 104 and picture heights 34, 46, 40 and 62 respectively. `SettingsParts.CaretPicture` uses `Shape.Hairline` for its caret, while `SettingsParts.Tile` uses `Shape.Popup` for its frame; `SettingsParts.MaterialPicture` has no specified radius.

## Swatch

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `SettingsParts.PaletteChips` | `SettingsWindow`: light and dark palette subrows | content-sized; swatch dots 7 square literal | Shape.Round(18) chip, Shape.Round(7) dots | face 6,8; chip right 8; spacing 5 / 3 literal | Typography.Small 12; weight unspecified | palette Canvas; selected Accent border 2 / Line Ink .09/.10 border 1 literal; dots palette colors | unspecified |
| `SettingsParts.AccentDots` | `SettingsWindow`: Accent row | 18 square literal | Shape.Round(18) | spacing 7 literal | none | color; selected Ink Paint.CardBorderOn border 2.5 / Faint Paint.CardBorderOff border 1 literal | unspecified |

**Disagreements:** `SettingsParts.PaletteChips` uses a selected Accent border 2 and an unselected Line border 1; `SettingsParts.AccentDots` uses a selected Ink `Paint.CardBorderOn` border 2.5 and an unselected Faint `Paint.CardBorderOff` border 1. Palette chips include Small text and 7-square dots, while accent dots are 18-square and have no text.

## Popup and menu

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `SettingsParts.Dropdown` popup | Settings dropdown | literal min width 160 default | Shape.Popup | literal 4 | - | Surface alpha literal 1; Ink literal .09/.10 border literal 1 | Shape.DropdownShadowBlur 16, Shape.PopupShadowOffsetY 4, Shape.PopupShadowDarkAlpha / Shape.PopupShadowLightAlpha 110 / 40 |
| `PadMenu.BuildMenu` | `PadMenu.Show` | - | Shape.Popup | literal 4 | - | Surface alpha literal 1; Faint Paint.PopupBorder border literal 1 | Shape.MenuShadowBlur 18, Shape.PopupShadowOffsetY 4, Shape.PopupShadowDarkAlpha / Shape.PopupShadowLightAlpha 110 / 40 |
| `PadMenu.MenuButton` | `BuildMenuRow` | literal height 32 | literal 4 | literal 8,0 | Typography.Large / Typography.Body; default | literal transparent; Paint.Hover / Paint.Press; shortcut Paint.MenuCaption .8, disabled opacity .42 | - |
| `PadMenu.MenuButton` | `BuildActionRow` | literal height 52 | literal 4 | literal 0; margin 0 / 2 | Typography.Large; default | literal transparent; Paint.Hover / Paint.Press | - |

**Disagreements:** `SettingsParts.Dropdown` popup uses Ink .09/.10 border and `Shape.DropdownShadowBlur` (16); `PadMenu.BuildMenu` uses `Paint.PopupBorder` (.45) and `Shape.MenuShadowBlur` (18). `PadMenu.MenuButton` in `BuildMenuRow` is height 32 with padding 8,0; in `BuildActionRow` it is height 52 with padding 0.

## Badge

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PadWindow.Layout.Chip` | mode, slot and development badges | ChromeTokens.BadgeHeight | ChromeTokens.BadgeCornerRadius | ChromeTokens.BadgeHorizontalPadding / BadgeTopPadding / BadgeBottomPadding | ChromeTokens.BadgeFontSize; Typography.Strong | color ChromeTokens.BadgeDarkFillAlpha / BadgeLightFillAlpha; no border | - |
| `Editor/Renderers.ResultRenderer.DrawChip` | results and timers | literal text width + 14, height + 4; timer adds text height × .62 + 6 to width | literal 7 | literal inset 7,2 | Settings.FontSize × .93; Settings font / Typography.Strong | result color Paint.BadgeFill; no border | - |

**Disagreements:** `PadWindow.Layout.Chip` uses radius 6, font 11.5, padding 7,1,7,2 and fill .18/.13; `ResultRenderer.DrawChip` uses radius 7, font `Settings.FontSize` × .93, inset 7,2 and `Paint.BadgeFill` .14/.10.

## Pill

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PadWindow.Layout` pill | pager | - | ChromeTokens.PillCornerRadius | ChromeTokens.PillInset; ChromeTokens.PillBottomInset | - | Surface alpha literal 1; Faint ChromeTokens.PillBorderAlpha, ChromeTokens.PillBorderThickness | ChromeTokens.PillShadowBlur 14 / PillShadowOffsetY 3 / PillShadowDarkAlpha 90 / PillShadowLightAlpha 28 |
| `PadWindow.Layout` count | pager count | ChromeTokens.PagerCountBaseWidth + digits × PagerCountDigitWidth; ChromeTokens.BadgeHeight | ChromeTokens.PagerCountHoverRadius | literal 0 | ChromeTokens.PagerCountFontSize; Typography.Strong | literal transparent; Paint.Hover / Paint.Press | - |
| `PadWindow.Layout` dots | pager dots | ChromeTokens.PagerDotWidth / PagerDotSelectedWidth, PagerDotHeight | ChromeTokens.PagerDotRadius; hit ChromeTokens.PagerDotHoverRadius | literal 0 | - | palette color; literal transparent hit surface | - |

**Disagreements:** `PadWindow.Layout` pager pill uses radius 12 and Surface fill; its count uses radius 8 and transparent fill, while its dots use radius 3.5 and a radius 9 hit area. Count height is 20 versus dot height 7.

## Overlay panel

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PadWindow.Layout` overlay host | search, help and save-failure dialog | - | - | - | - | Canvas literal 1 solid / Paint.GlassScrim otherwise | - |
| `HelpOverlay` key | shortcut sheet | literal min width 150 | Shape.Key | literal 7,2 | Typography.Caption; default | Ink Paint.HelpWash .1/.06; mono font | - |
| `PadWindow.Layout` save failure card | save failure | - | Shape.Dialog | literal margin 12; padding 18 | Typography.DialogTitle Strong; Typography.Label | Surface alpha literal 1; Faint Paint.PopupBorder, border literal 1 | ChromeTokens.PillShadowBlur 14 / PillShadowOffsetY 3 / PillShadowDarkAlpha 90 / PillShadowLightAlpha 28 |

**Disagreements:** `PadWindow.Layout` overlay host uses Canvas or `Paint.GlassScrim`; its save-failure card uses Surface, radius 12 and padding 18. `HelpOverlay` key uses radius 5, `Paint.HelpWash` and padding 7,2, unlike the save-failure card.

## Toast

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PadWindow.Layout` toast | `Toast` | literal max width 380 | Shape.Card | literal padding 12,7; margin 16,0,16,ChromeTokens.ToastBottomMargin | Typography.Label; default / Typography.Strong action | Ink Paint.Toast .92; no border; Canvas message and Accent action foreground | ChromeTokens.PillShadowBlur 14 / PillShadowOffsetY 3 / PillShadowDarkAlpha 90 / PillShadowLightAlpha 28 |

**Disagreements:** No disagreement is identifiable from the single `PadWindow.Layout` toast row; its message and optional action use regular and strong weight respectively.

## Text roles

| Instance (file:method) | Callers | Size | Radius | Padding / margin | Font size / weight | Fill and border (alpha) | Shadow |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `SettingsParts.Heading` | Settings pages | - | - | literal margin 2,0,0,10 | Typography.Display; Typography.Strong | - | - |
| `SettingsParts.Group` | Settings pages | - | - | literal margin 2,20,0,8 | Typography.Body; Typography.Strong | - | - |
| `SettingsParts.Texts` | Settings Row / SubRow | - | - | - | Typography.Large / Typography.Small; default | - | - |
| `Kit.Section` | HelpOverlay | - | - | literal margin 8,22,0,2 | Typography.Micro; Typography.Strong | - | - |
| `Kit.PanelHeader` | HelpOverlay | - | - | literal margin 8,0,0,0 | Typography.PanelTitle; Typography.Strong | - | - |
| `SearchOverlay.Card` text | result title, snippet, metadata | - | - | - | Typography.BodyEmphasis Strong; Typography.Small default; Typography.Micro Strong / Regular | - | - |
| `PadWindow.Layout` metadata | saved, running timer | - | - | - | ChromeTokens.SavedLabelFontSize / RunningTimerFontSize; Typography.Regular | - | - |

**Disagreements:** `SettingsParts.Heading` uses `Typography.Display` (26) strong; `SettingsParts.Group` uses Body (13) strong. `Kit.Section` uses Micro (11) strong versus `Kit.PanelHeader` PanelTitle (18) strong. `SearchOverlay.Card` uses BodyEmphasis (13.5), Small (12) and Micro (11); `PadWindow.Layout` metadata uses SavedLabelFontSize (12) and RunningTimerFontSize (11.5).

## Proposed group tokens (names for discussion only)

One shared token set per group is proposed for applicable radius, font, fill, border and padding. A dash means the property has no common value proposed, not that it is zero. Yes means choosing a single value for at least one listed property would change an inspected rendered value; those choices wait for design-questions, not Part 2. Listed values are candidates and do not resolve those disagreements.

| Group | Radius | Font | Fill | Border | Padding | Instances disagree? |
| --- | --- | --- | --- | --- | --- | --- |
| Text button | Shape.Control | Typography.Body | - | - | - | Yes |
| Icon button | ChromeTokens.TopIconHoverRadius | ChromeTokens.IconGlyphSize | transparent | none | 0 | No (size and margin differ) |
| Toggle | - | - | - | - | - | Yes |
| Dropdown | - | Typography.Body | - | - | - | Yes |
| Text field | - | - | Surface | - | - | Yes |
| Slider | - | - | - | - | - | No instance found |
| Card | Shape.Card | - | - | - | - | Yes |
| Preview tile | - | - | - | - | - | Yes |
| Swatch | Shape.Round(18) | - | - | - | - | Yes |
| Popup and menu | - | - | - | - | - | Yes |
| Badge | - | - | - | none | - | Yes |
| Pill | - | - | - | - | - | Yes |
| Overlay panel | - | - | - | - | - | Yes |
| Toast | Shape.Card | Typography.Label | Paint.Toast | none | 12,7 | No (one instance) |
| Text roles | - | - | - | - | - | Yes |

## Gaps

- The Settings navigation row, menu separator, help text, motion trace and other distinct text and drawing sites are not mapped.
- Callers, state-dependent values and final template styling have not been fully cross-checked. No design dump or runtime visual verification was performed.
- The slider search found no instance in UI or Editor; no slider values are proposed.

## ChromeTokens alias candidates

Names remain in ChromeTokens, preserving the bar measurement lock. Only exact matches are candidates; equality alone does not establish shared semantics.

| Name | Value | Role token it equals | Same value: yes/no |
| --- | --- | --- | --- |
| BadgeFontSize | 11.5 | Typography.Caption | yes |
| RunningTimerFontSize | 11.5 | Typography.Caption | yes |
| PagerCountFontSize | 11.5 | Typography.Caption | yes |
| SavedLabelFontSize | 12 | Typography.Small | yes |
| IconGlyphSize | 16 | Typography.IconMedium | yes |
| TopIconHoverRadius | 8 | Shape.Popup | yes |
| PagerArrowHoverRadius | 8 | Shape.Popup | yes |
| PagerCountHoverRadius | 8 | Shape.Popup | yes |
| PillCornerRadius | 12 | Shape.Dialog | yes |
| BadgeDarkFillAlpha | .18 | Paint.BadgeFill(true) = .14 | no |
| BadgeLightFillAlpha | .13 | Paint.BadgeFill(false) = .10 | no |
