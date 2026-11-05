# A4n-scale-lab implementation report

## Change list
Recommended values applied to existing tokens and mapped controls; no token renames or scale layer.
ChromeTokens: 25 literal constants change plus derived PagerDotRadius (3.5 -> 4), matching the 26 values in section 3.2.
Top backdrop and all 13 section 6 exceptions retained.

## Role values (sections 3.1 and 3.2)

| Role | File:line | Today -> R |
| --- | --- | --- |
| Typography.Micro | `product/AntiNotepad/Theme/Typography.cs:20` | `11` -> `12` |
| Typography.Caption | `product/AntiNotepad/Theme/Typography.cs:20` | `11.5` -> `12` |
| Typography.Small | `product/AntiNotepad/Theme/Typography.cs:20` | `12` -> `12` |
| Typography.Label | `product/AntiNotepad/Theme/Typography.cs:20` | `12.5` -> `12` |
| Typography.Body | `product/AntiNotepad/Theme/Typography.cs:20` | `13` -> `14` |
| Typography.BodyEmphasis | `product/AntiNotepad/Theme/Typography.cs:21` | `13.5` -> `14` |
| Typography.Large | `product/AntiNotepad/Theme/Typography.cs:21` | `14` -> `14` |
| Typography.Input | `product/AntiNotepad/Theme/Typography.cs:21` | `15` -> `16` |
| Typography.DialogTitle | `product/AntiNotepad/Theme/Typography.cs:21` | `17` -> `18` |
| Typography.PanelTitle | `product/AntiNotepad/Theme/Typography.cs:21` | `18` -> `18` |
| Typography.Display | `product/AntiNotepad/Theme/Typography.cs:21` | `26` -> `28` |
| Typography.Regular | `product/AntiNotepad/Theme/Typography.cs:15` | `FontWeight.Normal` -> `FontWeight.Normal` |
| Typography.Strong | `product/AntiNotepad/Theme/Typography.cs:16` | `FontWeight.SemiBold` -> `FontWeight.SemiBold` |
| Typography.IconBody | `product/AntiNotepad/Theme/Typography.cs:24` | `13` -> `14` |
| Typography.IconSmall | `product/AntiNotepad/Theme/Typography.cs:24` | `14` -> `14` |
| Typography.IconInput | `product/AntiNotepad/Theme/Typography.cs:24` | `15` -> `16` |
| Typography.IconMedium | `product/AntiNotepad/Theme/Typography.cs:24` | `16` -> `16` |
| Typography.IconLarge | `product/AntiNotepad/Theme/Typography.cs:24` | `18` -> `20` |
| Typography.IconXLarge | `product/AntiNotepad/Theme/Typography.cs:24` | `20` -> `20` |
| Shape.Hairline | `product/AntiNotepad/Theme/Shape.cs:8` | `1` -> `2` |
| Shape.Key | `product/AntiNotepad/Theme/Shape.cs:8` | `5` -> `4` |
| Shape.Control | `product/AntiNotepad/Theme/Shape.cs:8` | `7` -> `4` |
| Shape.Popup | `product/AntiNotepad/Theme/Shape.cs:8` | `8` -> `8` |
| Shape.Card | `product/AntiNotepad/Theme/Shape.cs:8` | `10` -> `8` |
| Shape.Dialog | `product/AntiNotepad/Theme/Shape.cs:8` | `12` -> `12` |
| Shape.MenuShadowBlur | `product/AntiNotepad/Theme/Shape.cs:14` | `18` -> `16` |
| Shape.DropdownShadowBlur | `product/AntiNotepad/Theme/Shape.cs:14` | `16` -> `16` |
| Shape.PopupShadowOffsetY | `product/AntiNotepad/Theme/Shape.cs:14` | `4` -> `4` |
| Shape.PopupShadowDarkAlpha | `product/AntiNotepad/Theme/Shape.cs:15` | `110` -> `110` |
| Shape.PopupShadowLightAlpha | `product/AntiNotepad/Theme/Shape.cs:15` | `40` -> `40` |
| MotionTokens.WindowSlide | `product/AntiNotepad/Motion/MotionTokens.cs:9` | `150` -> `160` |
| MotionTokens.ContentReveal | `product/AntiNotepad/Motion/MotionTokens.cs:9` | `140` -> `160` |
| MotionTokens.WindowFade | `product/AntiNotepad/Motion/MotionTokens.cs:9` | `160` -> `160` |
| MotionTokens.OverlayIn | `product/AntiNotepad/Motion/MotionTokens.cs:9` | `130` -> `120` |
| MotionTokens.BarsReveal | `product/AntiNotepad/Motion/MotionTokens.cs:12` | `170` -> `160` |
| MotionTokens.PillFade | `product/AntiNotepad/Motion/MotionTokens.cs:12` | `150` -> `160` |
| MotionTokens.ChromeShift | `product/AntiNotepad/Motion/MotionTokens.cs:12` | `180` -> `200` |
| MotionTokens.Nudge | `product/AntiNotepad/Motion/MotionTokens.cs:12` | `180` -> `200` |
| MotionTokens.ToastIn | `product/AntiNotepad/Motion/MotionTokens.cs:15` | `120` -> `120` |
| MotionTokens.ToastOut | `product/AntiNotepad/Motion/MotionTokens.cs:15` | `160` -> `160` |
| MotionTokens.NoteArrive | `product/AntiNotepad/Motion/MotionTokens.cs:18` | `190` -> `200` |
| MotionTokens.SwipeSpringBack | `product/AntiNotepad/Motion/MotionTokens.cs:18` | `200` -> `200` |
| MotionTokens.CaretGlide | `product/AntiNotepad/Motion/MotionTokens.cs:18` | `80` -> `80` |
| MotionTokens.SettingsPage | `product/AntiNotepad/Motion/MotionTokens.cs:21` | `150` -> `160` |
| Paint.Hover | `product/AntiNotepad/Theme/Paint.cs:8` | `dark ? .09 : .06` -> `dark ? .09 : .06` |
| Paint.Press | `product/AntiNotepad/Theme/Paint.cs:9` | `dark ? .15 : .11` -> `dark ? .15 : .11` |
| Paint.Selected | `product/AntiNotepad/Theme/Paint.cs:10` | `dark ? .22 : .18` -> `dark ? .22 : .18` |
| Paint.RowWash | `product/AntiNotepad/Theme/Paint.cs:15` | `dark ? .03 : .025` -> `dark ? .03 : .03` |
| Paint.HelpWash | `product/AntiNotepad/Theme/Paint.cs:12` | `dark ? .1 : .06` -> `dark ? .09 : .06` |
| Paint.PreviewFrame | `product/AntiNotepad/Theme/Paint.cs:13` | `dark ? .09 : .10` -> `dark ? .09 : .06` |
| Paint.PreviewWell | `product/AntiNotepad/Theme/Paint.cs:14` | `dark ? .06 : .045` -> `dark ? .09 : .06` |
| Paint.CompletionFill | `product/AntiNotepad/Theme/Paint.cs:16` | `dark ? .085 : .075` -> `dark ? .09 : .06` |
| Paint.BadgeFill | `product/AntiNotepad/Theme/Paint.cs:17` | `dark ? .14 : .10` -> `dark ? .15 : .11` |
| Paint.CardSurface | `product/AntiNotepad/Theme/Paint.cs:11` | `dark ? .92 : .9` -> `dark ? .9 : .9` |
| Paint.Toast | `product/AntiNotepad/Theme/Paint.cs:29` | `.92` -> `.9` |
| Paint.PopupBorder | `product/AntiNotepad/Theme/Paint.cs:20` | `.45` -> `.5` |
| Paint.FieldBorder | `product/AntiNotepad/Theme/Paint.cs:21` | `.5` -> `.5` |
| Paint.RestBorder | `product/AntiNotepad/Theme/Paint.cs:27` | `.5` -> `.5` |
| Paint.ResultBorder | `product/AntiNotepad/Theme/Paint.cs:22` | `.25` -> `.3` |
| Paint.ResultCurrent | `product/AntiNotepad/Theme/Paint.cs:23` | `.9` -> `.9` |
| Paint.FocusBorder | `product/AntiNotepad/Theme/Paint.cs:26` | `.9` -> `.9` |
| Paint.AccentPen | `product/AntiNotepad/Theme/Paint.cs:38` | `.9` -> `.9` |
| Paint.HoverFace | `product/AntiNotepad/Theme/Paint.cs:39` | `.86` -> `.9` |
| Paint.CardBorderOn | `product/AntiNotepad/Theme/Paint.cs:40` | `.9` -> `.9` |
| Paint.GridPen | `product/AntiNotepad/Theme/Paint.cs:42` | `.9` -> `.9` |
| Paint.ResultSurface | `product/AntiNotepad/Theme/Paint.cs:24` | `.55` -> `.5` |
| Paint.Dimmed | `product/AntiNotepad/Theme/Paint.cs:25` | `.55` -> `.5` |
| Paint.GlassScrim | `product/AntiNotepad/Theme/Paint.cs:30` | `.55` -> `.5` |
| Paint.GlassPreview | `product/AntiNotepad/Theme/Paint.cs:31` | `.6` -> `.5` |
| Paint.ErrorHint | `product/AntiNotepad/Theme/Paint.cs:28` | `.78` -> `.8` |
| Paint.MenuCaption | `product/AntiNotepad/Theme/Paint.cs:32` | `.8` -> `.8` |
| Paint.MarkerStrong | `product/AntiNotepad/Theme/Paint.cs:36` | `.8` -> `.8` |
| Paint.PreviewPad | `product/AntiNotepad/Theme/Paint.cs:44` | `.8` -> `.8` |
| Paint.MenuSeparator | `product/AntiNotepad/Theme/Paint.cs:33` | `.4` -> `.3` |
| Paint.CaretBlock | `product/AntiNotepad/Theme/Paint.cs:34` | `.45` -> `.5` |
| Paint.MarkerFaint | `product/AntiNotepad/Theme/Paint.cs:35` | `.35` -> `.3` |
| Paint.CardBorderOff | `product/AntiNotepad/Theme/Paint.cs:41` | `.3` -> `.3` |
| Paint.KeyPen | `product/AntiNotepad/Theme/Paint.cs:37` | `.55` -> `.5` |
| ChromeTokens.IconGlyphSize | `product/AntiNotepad/Theme/ChromeTokens.cs:9` | `16` -> `16` |
| ChromeTokens.GlyphIconBoxExtra | `product/AntiNotepad/Theme/ChromeTokens.cs:10` | `2` -> `2` |
| ChromeTokens.BarEdgeInset | `product/AntiNotepad/Theme/ChromeTokens.cs:11` | `4` -> `4` |
| ChromeTokens.BadgeSpacing | `product/AntiNotepad/Theme/ChromeTokens.cs:14` | `4` -> `4` |
| ChromeTokens.PillInset | `product/AntiNotepad/Theme/ChromeTokens.cs:53` | `4` -> `4` |
| ChromeTokens.PillBottomInset | `product/AntiNotepad/Theme/ChromeTokens.cs:57` | `4` -> `4` |
| ChromeTokens.FirstItemInset | `product/AntiNotepad/Theme/ChromeTokens.cs:12` | `10` -> `12` |
| ChromeTokens.BadgeRightInset | `product/AntiNotepad/Theme/ChromeTokens.cs:15` | `6` -> `8` |
| ChromeTokens.MetaSpacing | `product/AntiNotepad/Theme/ChromeTokens.cs:16` | `8` -> `8` |
| ChromeTokens.FloatGap | `product/AntiNotepad/Theme/ChromeTokens.cs:58` | `8` -> `8` |
| ChromeTokens.FloatPagerRevealZone | `product/AntiNotepad/Theme/ChromeTokens.cs:30` | `FloatGap` -> `FloatGap` |
| ChromeTokens.IconSpacing | `product/AntiNotepad/Theme/ChromeTokens.cs:17` | `3` -> `4` |
| ChromeTokens.BadgeCornerRadius | `product/AntiNotepad/Theme/ChromeTokens.cs:18` | `6` -> `4` |
| ChromeTokens.BadgeHorizontalPadding | `product/AntiNotepad/Theme/ChromeTokens.cs:19` | `7` -> `8` |
| ChromeTokens.BadgeDarkFillAlpha | `product/AntiNotepad/Theme/ChromeTokens.cs:22` | `0.18` -> `.15` |
| ChromeTokens.BadgeLightFillAlpha | `product/AntiNotepad/Theme/ChromeTokens.cs:23` | `0.13` -> `.11` |
| ChromeTokens.BadgeFontSize | `product/AntiNotepad/Theme/ChromeTokens.cs:24` | `11.5` -> `12` |
| ChromeTokens.SavedLabelFontSize | `product/AntiNotepad/Theme/ChromeTokens.cs:25` | `12` -> `12` |
| ChromeTokens.RunningTimerFontSize | `product/AntiNotepad/Theme/ChromeTokens.cs:26` | `11.5` -> `12` |
| ChromeTokens.TextEdgeFadeBand | `product/AntiNotepad/Theme/ChromeTokens.cs:27` | `12` -> `12` |
| ChromeTokens.TopIconHoverRadius | `product/AntiNotepad/Theme/ChromeTokens.cs:32` | `8` -> `8` |
| ChromeTokens.PagerArrowHoverRadius | `product/AntiNotepad/Theme/ChromeTokens.cs:33` | `8` -> `8` |
| ChromeTokens.PagerCountHoverRadius | `product/AntiNotepad/Theme/ChromeTokens.cs:34` | `8` -> `8` |
| ChromeTokens.PagerDotHoverRadius | `product/AntiNotepad/Theme/ChromeTokens.cs:35` | `9` -> `8` |
| ChromeTokens.PagerRowSpacing | `product/AntiNotepad/Theme/ChromeTokens.cs:36` | `3` -> `4` |
| ChromeTokens.PagerCountFontSize | `product/AntiNotepad/Theme/ChromeTokens.cs:37` | `11.5` -> `12` |
| ChromeTokens.PagerCountBaseWidth | `product/AntiNotepad/Theme/ChromeTokens.cs:38` | `30` -> `32` |
| ChromeTokens.PagerSeparatorWidth | `product/AntiNotepad/Theme/ChromeTokens.cs:40` | `1` -> `1` |
| ChromeTokens.DividerWidth | `product/AntiNotepad/Theme/ChromeTokens.cs:66` | `1` -> `1` |
| ChromeTokens.PillBorderThickness | `product/AntiNotepad/Theme/ChromeTokens.cs:54` | `1` -> `1` |
| ChromeTokens.PagerSeparatorHeight | `product/AntiNotepad/Theme/ChromeTokens.cs:41` | `14` -> `16` |
| ChromeTokens.PagerSeparatorMargin | `product/AntiNotepad/Theme/ChromeTokens.cs:42` | `5` -> `4` |
| ChromeTokens.PagerSeparatorAlpha | `product/AntiNotepad/Theme/ChromeTokens.cs:43` | `0.5` -> `0.5` |
| ChromeTokens.PagerDotSelectedWidth | `product/AntiNotepad/Theme/ChromeTokens.cs:44` | `16` -> `16` |
| ChromeTokens.PagerDotButtonWidth | `product/AntiNotepad/Theme/ChromeTokens.cs:49` | `16` -> `16` |
| ChromeTokens.PagerDotWidth | `product/AntiNotepad/Theme/ChromeTokens.cs:45` | `7` -> `8` |
| ChromeTokens.PagerDotHeight | `product/AntiNotepad/Theme/ChromeTokens.cs:46` | `7` -> `8` |
| ChromeTokens.PagerDotSelectedButtonWidth | `product/AntiNotepad/Theme/ChromeTokens.cs:48` | `24` -> `24` |
| ChromeTokens.PagerDotButtonHeight | `product/AntiNotepad/Theme/ChromeTokens.cs:50` | `18` -> `20` |
| ChromeTokens.PagerDotBorderThickness | `product/AntiNotepad/Theme/ChromeTokens.cs:51` | `1.2` -> `1` |
| ChromeTokens.PillBorderAlpha | `product/AntiNotepad/Theme/ChromeTokens.cs:55` | `0.35` -> `.3` |
| ChromeTokens.PillCornerRadius | `product/AntiNotepad/Theme/ChromeTokens.cs:56` | `12` -> `12` |
| ChromeTokens.PillShadowBlur | `product/AntiNotepad/Theme/ChromeTokens.cs:61` | `14` -> `12` |
| ChromeTokens.PillShadowOffsetY | `product/AntiNotepad/Theme/ChromeTokens.cs:62` | `3` -> `2` |
| ChromeTokens.PillShadowDarkAlpha | `product/AntiNotepad/Theme/ChromeTokens.cs:63` | `90` -> `90` |
| ChromeTokens.PillShadowLightAlpha | `product/AntiNotepad/Theme/ChromeTokens.cs:64` | `28` -> `28` |
| ChromeTokens.DividerPagerClearance | `product/AntiNotepad/Theme/ChromeTokens.cs:65` | `6` -> `8` |
| ChromeTokens.DividerTopMargin | `product/AntiNotepad/Theme/ChromeTokens.cs:67` | `6` -> `8` |
| ChromeTokens.DividerAlpha | `product/AntiNotepad/Theme/ChromeTokens.cs:68` | `0.45` -> `.5` |
| ChromeTokens.ToastPagerClearance | `product/AntiNotepad/Theme/ChromeTokens.cs:69` | `2` -> `2` |
| Paint.PreviewBar | `product/AntiNotepad/Theme/Paint.cs:43`; `product/AntiNotepad/UI/SettingsParts.cs:532` | .10 -> .09 dark / .06 light (constant and existing call site; no signature change) |

## Control values (section 4)

| Role | File:line | Today -> R |
| --- | --- | --- |
| PreviewBar dark strength | `product/AntiNotepad/Theme/Paint.cs:43` | `PreviewBar = .10` -> `PreviewBar = .09` |
| Text button Control | `product/AntiNotepad/UI/Kit.cs:111` | `double radius = 7` -> `double radius = 4` |
| Toggle Body | `product/AntiNotepad/UI/Kit.cs:140` | `Text(label, on ? p.Ink : p.Faint, Typography.Label` -> `Text(label, on ? p.Ink : p.Faint, Typography.Body` |
| Toggle padding | `product/AntiNotepad/UI/Kit.cs:141` | `new Thickness(10, 4)` -> `new Thickness(8, 4)` |
| Toggle Control | `product/AntiNotepad/UI/Kit.cs:142` | `Plain(text, p, radius: 8)` -> `Plain(text, p, radius: 4)` |
| Section margin | `product/AntiNotepad/UI/Kit.cs:171` | `new Thickness(8, 22, 0, 2)` -> `new Thickness(8, 24, 0, 2)` |
| Field focus Thick | `product/AntiNotepad/UI/Kit.cs:190` | `state == ":focus" ? 1.5 : 1` -> `state == ":focus" ? 2 : 1` |
| Hover borders | `product/AntiNotepad/UI/SettingsParts.cs:19` | `_p.Dark ? .09 : .10` -> `_p.Dark ? .09 : .06` |
| Heading margin | `product/AntiNotepad/UI/SettingsParts.cs:30` | `new Thickness(2, 0, 0, 10)` -> `new Thickness(2, 0, 0, 12)` |
| Group margin | `product/AntiNotepad/UI/SettingsParts.cs:39` | `new Thickness(2, 20, 0, 8)` -> `new Thickness(2, 24, 0, 8)` |
| Row size | `product/AntiNotepad/UI/SettingsParts.cs:103` | `MinHeight = 62` -> `MinHeight = 64` |
| Row padding | `product/AntiNotepad/UI/SettingsParts.cs:103` | `new Thickness(16, 10)` -> `new Thickness(16, 12)` |
| Row title Strong | `product/AntiNotepad/UI/SettingsParts.cs:109` | `FontSize = Typography.Large, Foreground` -> `FontSize = Typography.Large, FontWeight = Typography.Strong, Foreground` |
| SubRow size | `product/AntiNotepad/UI/SettingsParts.cs:132` | `MinHeight = 54` -> `MinHeight = 56` |
| SubRow padding | `product/AntiNotepad/UI/SettingsParts.cs:133` | `new Thickness(52, 10, 16, 10)` -> `new Thickness(48, 12, 16, 12)` |
| Expander row compensation | `product/AntiNotepad/UI/SettingsParts.cs:160` | `new Thickness(-16, -10)` -> `new Thickness(-16, -12)` |
| Text button and Seg padding | `product/AntiNotepad/UI/SettingsParts.cs:198` | `new Thickness(12, 5)` -> `new Thickness(12, 4)` |
| Toggle knob | `product/AntiNotepad/UI/SettingsParts.cs:217` | `Width = 14, Height = 14, CornerRadius = new CornerRadius(Shape.Round(14)), Margin = new Thickness(3, 0)` -> `Width = 12, Height = 12, CornerRadius = new CornerRadius(Shape.Round(12)), Margin = new Thickness(4, 0)` |
| Toggle off Thin | `product/AntiNotepad/UI/SettingsParts.cs:226` | `on ? 0 : 1.5` -> `on ? 0 : 1` |
| Dropdown face padding | `product/AntiNotepad/UI/SettingsParts.cs:242` | `new Thickness(12, 0, 10, 0)` -> `new Thickness(12, 12)` |
| Dropdown face min size | `product/AntiNotepad/UI/SettingsParts.cs:246` | `Height = 32, MinWidth = minWidth` -> `Height = 48, MinWidth = minWidth` |
| Dropdown popup border | `product/AntiNotepad/UI/SettingsParts.cs:255` | `BorderBrush = Line,
                BorderThickness` -> `BorderBrush = new SolidColorBrush(_p.Faint, Paint.PopupBorder),
                BorderThickness` |
| Dropdown option padding | `product/AntiNotepad/UI/SettingsParts.cs:267` | `new Thickness(10, 6)` -> `new Thickness(12, 8)` |
| Dropdown option radius | `product/AntiNotepad/UI/SettingsParts.cs:268` | `Kit.Plain(item, _p, radius: 6)` -> `Kit.Plain(item, _p, radius: 4)` |
| Path min size | `product/AntiNotepad/UI/SettingsParts.cs:309` | `MinHeight = 38` -> `MinHeight = 40` |
| Path padding | `product/AntiNotepad/UI/SettingsParts.cs:312` | `new Thickness(10, 7)` -> `new Thickness(12, 8)` |
| Tile rest Thin | `product/AntiNotepad/UI/SettingsParts.cs:323` | `selected ? 2 : 1.5` -> `selected ? 2 : 1` |
| Palette chip gap (tile stays 5) | `product/AntiNotepad/UI/SettingsParts.cs:355` | `Spacing = 5` -> `Spacing = 8` |
| Swatch dot gap | `product/AntiNotepad/UI/SettingsParts.cs:351` | `Spacing = 3` -> `Spacing = 4` |
| Swatch dot size | `product/AntiNotepad/UI/SettingsParts.cs:353` | `Width = 7, Height = 7, CornerRadius = new CornerRadius(Shape.Round(7))` -> `Width = 8, Height = 8, CornerRadius = new CornerRadius(Shape.Round(8))` |
| Swatch face padding | `product/AntiNotepad/UI/SettingsParts.cs:354` | `new Thickness(6, 8)` -> `new Thickness(8, 8)` |
| Swatch rest Faint (palette chip only) | `product/AntiNotepad/UI/SettingsParts.cs:329` | `selected ? new SolidColorBrush(_p.Accent) : Line` -> `selected ? new SolidColorBrush(_p.Accent) : new SolidColorBrush(_p.Faint, Paint.CardBorderOff)` |
| Accent dot gap | `product/AntiNotepad/UI/SettingsParts.cs:377` | `Spacing = 7` -> `Spacing = 8` |
| Accent dot size | `product/AntiNotepad/UI/SettingsParts.cs:383` | `Width = 18,
                Height = 18` -> `Width = 20,
                Height = 20` |
| Accent dot radius | `product/AntiNotepad/UI/SettingsParts.cs:385` | `CornerRadius = new CornerRadius(Shape.Round(18)),
                Background = new SolidColorBrush(color)` -> `CornerRadius = new CornerRadius(Shape.Round(20)),
                Background = new SolidColorBrush(color)` |
| Accent ring Thick | `product/AntiNotepad/UI/SettingsParts.cs:389` | `selected ? 2.5 : 1` -> `selected ? 2 : 1` |
| Caret picture size | `product/AntiNotepad/UI/SettingsParts.cs:416` | `Height = 34` -> `Height = 32` |
| Material picture size | `product/AntiNotepad/UI/SettingsParts.cs:437` | `Height = 46` -> `Height = 48` |
| PreviewBar light strength | `product/AntiNotepad/UI/SettingsParts.cs:532` | `p.Ink, Paint.PreviewBar` -> `p.Ink, p.Dark ? Paint.PreviewBar : Paint.Hover(false)` |
| Close size | `product/AntiNotepad/UI/SettingsWindow.cs:174` | `_p, Close, 34` -> `_p, Close, 32` |
| Close margin | `product/AntiNotepad/UI/SettingsWindow.cs:175` | `new Thickness(0, 0, 6, 0)` -> `new Thickness(0, 0, 8, 0)` |
| Settings find size | `product/AntiNotepad/UI/SettingsWindow.cs:187` | `Height = 34` -> `Height = 32` |
| Settings field Control | `product/AntiNotepad/UI/SettingsWindow.cs:188` | `CornerRadius = new CornerRadius(Shape.Popup)` -> `CornerRadius = new CornerRadius(Shape.Control)` |
| Settings find padding | `product/AntiNotepad/UI/SettingsWindow.cs:189` | `new Thickness(8, 7, 8, 0)` -> `new Thickness(8, 8, 8, 0)` |
| Caret tile width | `product/AntiNotepad/UI/SettingsWindow.cs:333` | `rows: true), 58)` -> `rows: true), 56)` |
| Material tile width | `product/AntiNotepad/UI/SettingsWindow.cs:355` | `rows: true), 78)` -> `rows: true), 80)` |
| Screen picture size | `product/AntiNotepad/UI/SettingsWindow.cs:510` | `Height = 62` -> `Height = 64` |
| Search card padding | `product/AntiNotepad/UI/SearchOverlay.cs:203` | `new Thickness(12, 9)` -> `new Thickness(12, 8)` |
| Search card size | `product/AntiNotepad/UI/SearchOverlay.cs:208` | `GridView ? 150 :` -> `GridView ? 152 :` |
| Search current Thick | `product/AntiNotepad/UI/SearchOverlay.cs:253` | `current ? 1.5 : 1` -> `current ? 2 : 1` |
| Help key padding | `product/AntiNotepad/UI/HelpOverlay.cs:40` | `new Thickness(7, 2)` -> `new Thickness(8, 2)` |
| Toast padding | `product/AntiNotepad/UI/PadWindow.Layout.cs:34` | `new Thickness(12, 7)` -> `new Thickness(12, 8)` |
| Toast Caption | `product/AntiNotepad/UI/PadWindow.Layout.cs:450` | `FontSize = Typography.Label, TextWrapping = TextWrapping.Wrap, MaxWidth = 300` -> `FontSize = Typography.Caption, TextWrapping = TextWrapping.Wrap, MaxWidth = 300` |
| Toast action padding | `product/AntiNotepad/UI/PadWindow.Layout.cs:456` | `new Thickness(6, 1)` -> `new Thickness(8, 2)` |
| Toast action size | `product/AntiNotepad/UI/PadWindow.Layout.cs:457` | `Kit.Plain(label, p with { Ink = p.Canvas })` -> `Kit.Plain(label, p with { Ink = p.Canvas }, height: 32)` |
| Save failure Body | `product/AntiNotepad/UI/PadWindow.Layout.cs:492` | `FontSize = Typography.Label,
            TextWrapping` -> `FontSize = Typography.Body,
            TextWrapping` |
| Save retry Body and size | `product/AntiNotepad/UI/PadWindow.Layout.cs:497` | `p.Accent, Typography.Label, Typography.Strong), p, 88, 34` -> `p.Accent, Typography.Body, Typography.Strong), p, 88, 32` |
| Save quit Body and size | `product/AntiNotepad/UI/PadWindow.Layout.cs:500` | `p.Muted, Typography.Label), p, 104, 34` -> `p.Muted, Typography.Body), p, 104, 32` |
| Save card padding | `product/AntiNotepad/UI/PadWindow.Layout.cs:515` | `new Thickness(18)` -> `new Thickness(16)` |
| Badge radius | `product/AntiNotepad/Editor/Renderers.cs:295` | `null, rect, 7, 7` -> `null, rect, 4, 4` |
| Toast action Body | `product/AntiNotepad/UI/PadWindow.Layout.cs:455` | Label -> Body 14 Strong |
| Save prompt compact padding | `product/AntiNotepad/UI/PadWindow.Layout.cs:498`; `product/AntiNotepad/UI/PadWindow.Layout.cs:502` | 0 -> 8 x 2 on existing button content borders |

## Behavior changes
Only the approved visual values change: typography, corners, strokes, transparency, spacing, shadows and transition durations.
Settings dropdown face height grows from 32 to 48 to fit the specified 12 x 12 insets and Body text without clipping.
Row titles now use Strong as mapped in section 4. Save prompt and toast actions use Body with 32-high hit areas.
No input routing, saving, hotkeys, settings defaults, palette colors, or motion algorithms changed.

## Left as is
- All 13 exceptions in design-scale.md section 6: badge optical top/bottom; digit measurement; entire TopBackdrop; proportional checkbox geometry and pens; proportional DrawChip font and widths; Nudge 2; compact toast/dialog action padding; search query radius 8; accent dot Ink ring; editor Bold 700; material picture no radius and derived pill/strip sizes; byte sample/glyph alphas; icon hover radius 8.
- Palette.cs: palette colors, customization mixes (.06/.6), selection alpha bytes and Marker mix .35 have no mapped R replacement.
- MotionPreview.cs already consumes the mapped Shape.Card, Paint.GlassPreview and Paint.PreviewFrame values. Legend gap 14, stack gap 6, trace height 46 and drawn legend mark 14 x 3 have no section 4 mapping.
- PadMenu.cs disabled opacity .42 has no mapped R replacement. It already satisfies mapped popup/item radii, 32/52 row heights, 8 x 0 row padding, High shadow, PopupBorder and typography roles. Its custom glyph geometry (18 box, 1.6 pen) has no mapped stroke role.
- Editor DrawChip: inset 7/2, width +14, timer icon factor .62 and gap 6, text offset 10 and edge 8 remain under the proportional-width exception; only radius/fill change. Checkbox and timer glyph path coordinates/pens stay literal.
- Completion.cs: popup 300/340 widths, row spacing 10 and label widths 70/86 have no section 4 mapping.
- SettingsWindow.cs: 900 x 720 and 640 x 460 minimum window; title margin 18; rail margins 10/4/10/10; rail item height 36 and radius 8; page 16/2/28/32 margin; tile widths already on-grid (60/104); editor font/line spacing, tint, UI-scale and scroll presets; debounce 150 are unmapped layout, user data or behavior.
- SettingsParts.cs: dropdown min width 160, option list spacing 1, path maximum height 68, tile widths already on-grid, swatch capsule Shape.Round(18), caret optical left margin 1, custom drawing coordinates and DrawnIcon pen 1.4 have no mapped replacement.
- SearchOverlay.cs: overall 14/14/14/46 margin, tools inset 6, empty-state inset 6, card content gap 3, card geometry/width calculation, result-list gap 2 have no mapped replacement.
- HelpOverlay.cs: overall 10/14/10/40 margin, panel right inset 14, key row margin 8/3 and key min width 150 have no mapped replacement.
- PadWindow.Layout.cs: prompt spacing 6/9, widths 88/104/240/420, toast max 380/message max 300 and dismissal timers have no mapped replacement.
- Other Editor/UI literals outside these mapped control instances remain unchanged.

## Design-scale vs tokens.json cross-check
No Recommended-value disagreement found in sections 3.1/3.2. JSON role new/steps and recommended scale values agree with the document, including PreviewBar theme pair and PagerDotButtonHeight preset 20. JSON covers token roles, not the section 4 literal map.
Source naming: JSON explicitly maps document Shape.PopupShadowDark to the actual PopupShadowDarkAlpha symbol; same value 110.
Both sources label DividerBottomMargin as derived and unchanged, but increasing DividerPagerClearance 6 -> 8 necessarily changes its result 56 -> 58. Preserved its formula, strip 50 and ToastBottomMargin 52. This is an internal source inconsistency, not a JSON/document disagreement.

## Plan deviations
- Existing borders receive compact 8 x 2 padding for mapped save actions; row title weight is set to Strong. These assignments are needed to realize the specified roles, without new controls or token structure.
- Dropdown height 32 -> 48 accommodates the approved 12 x 12 padding.
- Scope brief has prose paths rather than the explicit backticked bullet paths the gate requires; Codex does not edit its item card (docs/agents.md).

## Checks
Final gate: `pwsh -NoProfile -File tools/gate.ps1 -Task A4n-scale-lab -Base feat/A4-design-tokens -Offline`: overall FAIL.
- Build PASS: 0 warnings, 0 errors. Restore used only C:/Users/Neel/.nuget/packages.
- Selftest PASS: exit 0, SELFTEST PASS; CONTRACTS 13 rows, 12 pass / 1 manual. Design dump hashes agree across both captures: `1032fadd9067c61f4ce6299ad46b414464a0fb383a95827eaa79960cdb5c716c`, 16,160 lines. Preserved deterministic equality and refreshed the prior design baseline.
- Format PASS: 13 changed C# files, no format problems on changed lines.
- Seam PASS: 0 violations; Size PASS; git diff --check PASS.
- Scope FAIL: brief Files is prose, not explicit backticked bullets. This is a plan-format failure; Codex may not edit its own brief.
- Tokens FAIL: PadWindow.Layout spacing literal count 24 -> 26, exactly the two required compact 8 x 2 assignments on existing save-action content borders. This is an intentional mapping, but a baseline increase needs its reason on the item card. Baseline not changed.
- Fresh lens review: product mapping accepted after restoring unmapped Tile gap 5; no further proven product defects or JSON disagreements. Required checks still need rework for Scope/Tokens.
- Final offline dev-identity build is prepared separately with AntiNotepadDev=true and branch stamp feat/A4n-scale-lab; no GUI was launched.

## Out-of-scope files
`product/AntiNotepad/SelfTest/Design.cs:20`: refreshed only the existing DesignHash after generating and inspecting the 16,160-line dump. This necessary verification update is outside the original product-file list and is included in the exact Scope handoff below. No assertions were removed or weakened. STATUS and this report are required coordination deliverables.

## Size review
SettingsWindow.cs exceeds 600 lines and continues its existing Settings-page composition job; this value-only lab adds no new responsibility. No split in this item.

## Hand test
Not run. Codex must not launch or kill the app (docs/agents.md and docs/dev-loop.md). Manager runs tools/dev.ps1 after acceptance and verifies badge, EXE, DATA and log; user checks section 7 items 1-9 and show/hide timing.

## Noticed
Snapshot refresh completed. Owning session must enumerate the files below in the brief and record the reason for the intentional token-literal increase before rerunning the gate. It must include the snapshot file as a justified verification addition.

## Exact Scope handoff

The owning session can replace the prose Files section with this explicit list (no scope expansion beyond the mapped controls and necessary snapshot refresh):

- `product/AntiNotepad/Editor/Renderers.cs`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/Theme/Paint.cs`
- `product/AntiNotepad/Theme/Shape.cs`
- `product/AntiNotepad/Theme/Typography.cs`
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `product/AntiNotepad/UI/Kit.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`

The gate automatically allows STATUS and `.coord/reports/A4n-scale-lab.md`; do not use LooseScope to hide the missing manifest.

## Bounds check

Requested and approved: all Recommended values, including the locked chrome, in an offline lab build. Done: all mapped values, preserved exceptions, exact file/value tables, no JSON disagreements, clean build and passing selftest/Format. Verification additions: snapshot hash only. Remaining: strict Scope manifest, recorded token-baseline reason/update, review acceptance after these checks, manager dev launch and user visual/timing hand test. No commits, pushes, merges or installed-copy actions.

Offline dev artifact: `build/A4n-scale-lab-check/AntiNotepad.exe` (Debug, AntiNotepadDev=true, branch metadata feat/A4n-scale-lab). Retained for this requested lab build; never launched. Canonical manager launch still uses tools/dev.ps1 and isolated disposable data.
