# Design contract

This is the product design contract. The background studies are [design systems](design-systems.md) and [Fluent](design-fluent.md). Visible changes are lab-first and need user approval.

## Rules

Values are device-independent pixels unless stated otherwise. Use the named roles; the generated registry below is authoritative for individual constants.

- **Spacing and targets:** `Space` steps are Nudge 2, Xs 4, S 8, M 12, L 16, Xl 24 and Xxl 32. Nudge handles optical alignment and small clearances; free layout sizes follow the 4 px grid where practical. Use the 32 px `ChromeTokens.ButtonSize` control target, with stable, non-overlapping hit areas. Preserve the editor text insets (36 left, 30 right) to avoid rewrapping notes, and the named chrome measurements: 40 px docked row, 4 px pill inset and bottom gap, 8 px float gap, 12 px text fade, and derived 42 px pill and 50 px pager strip. The 24 px pager dot target width is a deliberate compact exception; its height is 32 px.
- **Typography:** the UI ramp is 12 px (Micro, Caption, Small, Label), 14 px (Body, BodyEmphasis, Large), 16 px Input, 18 px DialogTitle and PanelTitle, and 28 px Display. Use regular and strong weights for chrome; bold is for editor content. Editor text follows the user's font settings; the result chip scales with that text.
- **Corners and strokes:** use `Shape.Hairline` 2 px for the smallest corner, `Control` and `Key` 4, `Card` and `Popup` 8, and `Dialog` 12; round elements derive their radius from their size. Use `Shape.Thin` 1 px and `Shape.Thick` 2 px strokes. The 8 px icon hover radius distinguishes the hit area; the prominent search input also uses 8 px. Checkbox pens 1.6 and 1.9 scale with drawn editor geometry.
- **Color:** use palette roles and `Paint` for chrome, not literal colors or opacity. User-selected tint is a setting. Preserve the lab-tuned top backdrop: stops 40 and 52, blur 10 and tint 0.4.
- **Icons:** use the typed `Icons` registry and its specified font. `Typography` sizes are 14 px for IconBody and IconSmall, 16 px for IconInput and IconMedium, and 20 px for IconLarge and IconXLarge. Choose a glyph whose meaning matches the action.
- **Focus:** keep a visible focus ring, using the 2 px thick stroke. Field focus uses the accent if it reaches 3:1 against Surface, otherwise Ink; focus adorners must remain visible within their controls.
- **Contrast:** model at least 4.5:1 for text and 3:1 for non-text marks. The selftest sweeps eight palettes with each default and eight accent overrides: primary text on accent; field rest and focus on Surface; toast text/action on its composited background; failure detail on Surface; empty-note hint on Surface and Canvas; markdown marker text on Canvas; placeholder text on Surface; toggle off/on on the Settings card composited over Canvas; and pager empty, filled and selected marks on Surface and Canvas. Hint and marker limitations are listed below. Clear tint over the desktop is modeled, not measured live.
- **Labels and tooltips:** use concise, consistent US English and name the action or setting, including the current value where relevant to accessible dropdown names. Icon-only actions need an accessible name and an action-identifying tooltip; include a shortcut when one exists. Describe context that changes an action, such as the pane affected by swap, and explain disabled dependencies where applicable.
- **Motion:** use `MotionTokens` and preserve fast show/hide; timing is outside the visual-literal ratchet.
- **Size:** keep product C# files to 600 lines; named exceptions and their maximums are in `tools/size-exceptions.tsv`.

## How to extend

- **New token:** add the public constant, then run `pwsh tools/check-design-contract.ps1 -Write`.
- **New one-off:** add a row with a reason to `tools/design-exceptions.tsv`, then run `pwsh tools/check-design-contract.ps1 -Write`.
- **New custom control:** declare it in Custom controls with its tokens and geometry.
- **New screen:** add it to the walk in `product/AntiNotepad/SelfTest/Design.cs`.

## Known gaps

- Markdown marker text is below 3:1 on six palettes; the selftest reports it without failing. A user decision is pending.
- The Clear-tint empty-note hint has a named 1.24:1 minimum exception, retained by the user; it is not changed here.
- B-9 dependent-control dimming is planned and requires a lab first.
- Product C# files over 600 lines are named with current maximums in `tools/size-exceptions.tsv`; the check rejects new oversized files and growth past each maximum. Splits are a follow-up refactor.

### File-size exceptions

<!-- design-size-exceptions:start -->
| File | Max lines | Reason |
| --- | ---: | --- |
| `product/AntiNotepad/Editor/PadView.cs` | 1075 | Holds the editor view, its setup, and input handling. |
| `product/AntiNotepad/SelfTest/Editor.cs` | 635 | Groups editor behavior checks and their shared test helpers. |
| `product/AntiNotepad/SelfTest/Store.cs` | 697 | Groups store behavior checks and their shared test helpers. |
| `product/AntiNotepad/SelfTest/UI.cs` | 1485 | Groups UI behavior checks and their shared test helpers. |
| `product/AntiNotepad/UI/PadWindow.Layout.cs` | 903 | Builds and wires the pad window layout and its controls. |
| `product/AntiNotepad/UI/PadWindow.cs` | 683 | Owns pad window behavior and coordinates its partial implementations. |
| `product/AntiNotepad/UI/SettingsParts.cs` | 698 | Builds shared settings controls and their presentation helpers. |
| `product/AntiNotepad/UI/SettingsWindow.cs` | 1016 | Owns settings window navigation, page construction, and interaction. |
<!-- design-size-exceptions:end -->

## Custom controls

Drawn controls use the same roles as equivalent controls. Their geometry may scale with content or depict sample artwork. The literal exceptions in `tools/design-exceptions.tsv` record the file, category, count and reason; exceptions do not authorize unrelated new literals. 
| Control | Tokens and geometry |
| --- | --- |
| Checkbox renderer | `Shape.CheckboxSizeScale`, `CheckboxRadiusScale`, `CheckboxAccentPen`, `CheckboxCheckPen`; scales with editor text. |
| Result chip | `Typography.EditorChipScale`, `Shape.Control` and palette roles; tracks editor font size. |
| Caret artwork | `Space.CaretBarWidth`, `Space.CaretSampleMargin`; proportional rounded geometry. |
| Motion preview | `Space.MotionSampleHeight`, `MotionSampleBarWidth`, `MotionSampleBarHeight`; illustrative geometry. |
| Settings swatches and sample artwork | `Shape` and `Paint` roles; round swatches and sample shapes use geometry-specific radii. |
| Pager and pill | `ChromeTokens` pager and pill sizes, radii and strokes; round dots derive their radius from their size. |
| Editor structural marks | `Shape.Thin`, `Paint.MarkerFaint` and `Paint.MarkerStrong`; drawn marks follow content geometry. |


## Token and icon registry

The following names are generated from public constants in `Theme/*.cs` and registered icons. Run `pwsh tools/check-design-contract.ps1 -Write` after an intentional registry change.

<!-- design-tokens:start -->
| Name | Value (source expression) | Source |
| --- | --- | --- |
| `ChromeTokens.AccentDotSize` | `20` | `ChromeTokens.cs` |
| `ChromeTokens.BadgeBottomPadding` | `2` | `ChromeTokens.cs` |
| `ChromeTokens.BadgeCornerRadius` | `Shape.Key` | `ChromeTokens.cs` |
| `ChromeTokens.BadgeDarkFillAlpha` | `.15` | `ChromeTokens.cs` |
| `ChromeTokens.BadgeFontSize` | `12` | `ChromeTokens.cs` |
| `ChromeTokens.BadgeHeight` | `20` | `ChromeTokens.cs` |
| `ChromeTokens.BadgeHorizontalPadding` | `Space.S` | `ChromeTokens.cs` |
| `ChromeTokens.BadgeLightFillAlpha` | `.11` | `ChromeTokens.cs` |
| `ChromeTokens.BadgeRightInset` | `Space.S` | `ChromeTokens.cs` |
| `ChromeTokens.BadgeSpacing` | `Space.Xs` | `ChromeTokens.cs` |
| `ChromeTokens.BadgeTopPadding` | `1` | `ChromeTokens.cs` |
| `ChromeTokens.BarCornerSize` | `PillHeight` | `ChromeTokens.cs` |
| `ChromeTokens.BarEdgeInset` | `Space.Xs` | `ChromeTokens.cs` |
| `ChromeTokens.BlurRadius` | `10` | `ChromeTokens.cs` |
| `ChromeTokens.ButtonSize` | `32` | `ChromeTokens.cs` |
| `ChromeTokens.CaretBlockWidth` | `8` | `ChromeTokens.cs` |
| `ChromeTokens.CaretPictureHeight` | `32` | `ChromeTokens.cs` |
| `ChromeTokens.CaretSampleHeight` | `16` | `ChromeTokens.cs` |
| `ChromeTokens.ClearMaskAlpha` | `0` | `ChromeTokens.cs` |
| `ChromeTokens.ClearStop` | `52` | `ChromeTokens.cs` |
| `ChromeTokens.CompletionWindowMinWidth` | `300` | `ChromeTokens.cs` |
| `ChromeTokens.CompletionWindowWidth` | `340` | `ChromeTokens.cs` |
| `ChromeTokens.DeleteDebounceMs` | `500` | `ChromeTokens.cs` |
| `ChromeTokens.DenseMaskAlpha` | `1` | `ChromeTokens.cs` |
| `ChromeTokens.DenseStop` | `40` | `ChromeTokens.cs` |
| `ChromeTokens.DevBadgeLightTextInkMix` | `.62` | `ChromeTokens.cs` |
| `ChromeTokens.DividerAlpha` | `.5` | `ChromeTokens.cs` |
| `ChromeTokens.DividerBottomMargin` | `PagerStripHeight + DividerPagerClearance` | `ChromeTokens.cs` |
| `ChromeTokens.DividerPagerClearance` | `Space.S` | `ChromeTokens.cs` |
| `ChromeTokens.DividerTopMargin` | `Space.S` | `ChromeTokens.cs` |
| `ChromeTokens.DividerWidth` | `Shape.Thin` | `ChromeTokens.cs` |
| `ChromeTokens.DockedRowHeight` | `40` | `ChromeTokens.cs` |
| `ChromeTokens.FirstItemInset` | `Space.M` | `ChromeTokens.cs` |
| `ChromeTokens.FloatGap` | `Space.S` | `ChromeTokens.cs` |
| `ChromeTokens.FloatPagerRevealZone` | `FloatGap` | `ChromeTokens.cs` |
| `ChromeTokens.GlyphIconBoxExtra` | `Space.Nudge` | `ChromeTokens.cs` |
| `ChromeTokens.HelpOverlayBottomMargin` | `DockedRowHeight` | `ChromeTokens.cs` |
| `ChromeTokens.HiddenTaskbarHelperSize` | `1` | `ChromeTokens.cs` |
| `ChromeTokens.IconGlyphSize` | `16` | `ChromeTokens.cs` |
| `ChromeTokens.IconSpacing` | `Space.Xs` | `ChromeTokens.cs` |
| `ChromeTokens.MaterialPictureHeight` | `48` | `ChromeTokens.cs` |
| `ChromeTokens.MenuActionRowHeight` | `52` | `ChromeTokens.cs` |
| `ChromeTokens.MenuIconColumnWidth` | `28` | `ChromeTokens.cs` |
| `ChromeTokens.MenuPopupMinWidth` | `224` | `ChromeTokens.cs` |
| `ChromeTokens.MenuRowHeight` | `32` | `ChromeTokens.cs` |
| `ChromeTokens.MetaSpacing` | `Space.S` | `ChromeTokens.cs` |
| `ChromeTokens.Midpoint` | `0.5` | `ChromeTokens.cs` |
| `ChromeTokens.MidpointMaskAlpha` | `0.5` | `ChromeTokens.cs` |
| `ChromeTokens.MotionPreviewPadHeight` | `220` | `ChromeTokens.cs` |
| `ChromeTokens.OverlayPagerRevealZone` | `20` | `ChromeTokens.cs` |
| `ChromeTokens.PadWindowMinHeight` | `260` | `ChromeTokens.cs` |
| `ChromeTokens.PadWindowMinWidth` | `320` | `ChromeTokens.cs` |
| `ChromeTokens.PagerArrowHoverRadius` | `8` | `ChromeTokens.cs` |
| `ChromeTokens.PagerCountBaseWidth` | `32` | `ChromeTokens.cs` |
| `ChromeTokens.PagerCountButtonHeight` | `ButtonSize` | `ChromeTokens.cs` |
| `ChromeTokens.PagerCountDigitWidth` | `14` | `ChromeTokens.cs` |
| `ChromeTokens.PagerCountFontSize` | `12` | `ChromeTokens.cs` |
| `ChromeTokens.PagerCountHoverRadius` | `8` | `ChromeTokens.cs` |
| `ChromeTokens.PagerDotBorderThickness` | `Shape.Thin` | `ChromeTokens.cs` |
| `ChromeTokens.PagerDotButtonHeight` | `ButtonSize` | `ChromeTokens.cs` |
| `ChromeTokens.PagerDotHeight` | `8` | `ChromeTokens.cs` |
| `ChromeTokens.PagerDotHoverRadius` | `8` | `ChromeTokens.cs` |
| `ChromeTokens.PagerDotRadius` | `PagerDotHeight / 2` | `ChromeTokens.cs` |
| `ChromeTokens.PagerDotSelectedWidth` | `16` | `ChromeTokens.cs` |
| `ChromeTokens.PagerDotTargetWidth` | `24` | `ChromeTokens.cs` |
| `ChromeTokens.PagerDotWidth` | `8` | `ChromeTokens.cs` |
| `ChromeTokens.PagerRowSpacing` | `Space.Xs` | `ChromeTokens.cs` |
| `ChromeTokens.PagerSeparatorAlpha` | `0.5` | `ChromeTokens.cs` |
| `ChromeTokens.PagerSeparatorHeight` | `Space.L` | `ChromeTokens.cs` |
| `ChromeTokens.PagerSeparatorMargin` | `Space.S` | `ChromeTokens.cs` |
| `ChromeTokens.PagerSeparatorWidth` | `Shape.Thin` | `ChromeTokens.cs` |
| `ChromeTokens.PagerStripHeight` | `PillHeight + 2 * PillBottomInset` | `ChromeTokens.cs` |
| `ChromeTokens.PaletteChipWidth` | `144` | `ChromeTokens.cs` |
| `ChromeTokens.PaletteDotSize` | `8` | `ChromeTokens.cs` |
| `ChromeTokens.PatternPictureHeight` | `40` | `ChromeTokens.cs` |
| `ChromeTokens.PillBorderAlpha` | `.3` | `ChromeTokens.cs` |
| `ChromeTokens.PillBorderThickness` | `Shape.Thin` | `ChromeTokens.cs` |
| `ChromeTokens.PillBottomInset` | `Space.Xs` | `ChromeTokens.cs` |
| `ChromeTokens.PillCornerRadius` | `12` | `ChromeTokens.cs` |
| `ChromeTokens.PillHeight` | `ButtonSize + 2 * PillInset + 2 * PillBorderThickness` | `ChromeTokens.cs` |
| `ChromeTokens.PillInset` | `Space.Xs` | `ChromeTokens.cs` |
| `ChromeTokens.PillShadowBlur` | `12` | `ChromeTokens.cs` |
| `ChromeTokens.PillShadowDarkAlpha` | `90` | `ChromeTokens.cs` |
| `ChromeTokens.PillShadowLightAlpha` | `28` | `ChromeTokens.cs` |
| `ChromeTokens.PillShadowOffsetY` | `2` | `ChromeTokens.cs` |
| `ChromeTokens.PillShadowReach` | `PillShadowBlur + PillShadowOffsetY` | `ChromeTokens.cs` |
| `ChromeTokens.PinRingThickness` | `4` | `ChromeTokens.cs` |
| `ChromeTokens.RevealHandleHeight` | `2` | `ChromeTokens.cs` |
| `ChromeTokens.RevealHandleInset` | `3` | `ChromeTokens.cs` |
| `ChromeTokens.RevealHandleWidth` | `24` | `ChromeTokens.cs` |
| `ChromeTokens.RunningTimerFontSize` | `12` | `ChromeTokens.cs` |
| `ChromeTokens.SaveDelayDefaultMs` | `400` | `ChromeTokens.cs` |
| `ChromeTokens.SaveDelayFiveSecondsMs` | `5_000` | `ChromeTokens.cs` |
| `ChromeTokens.SaveDelayTenSecondsMs` | `10_000` | `ChromeTokens.cs` |
| `ChromeTokens.SaveDelayThirtySecondsMs` | `30_000` | `ChromeTokens.cs` |
| `ChromeTokens.SavedLabelFontSize` | `12` | `ChromeTokens.cs` |
| `ChromeTokens.SavedLabelMinuteMs` | `60_000` | `ChromeTokens.cs` |
| `ChromeTokens.SavedLabelRefreshMs` | `SaveIdleGateMs` | `ChromeTokens.cs` |
| `ChromeTokens.SavedLabelTimestampMs` | `300_000` | `ChromeTokens.cs` |
| `ChromeTokens.SaveFailureCardMaxWidth` | `420` | `ChromeTokens.cs` |
| `ChromeTokens.SaveFailureCardMinWidth` | `240` | `ChromeTokens.cs` |
| `ChromeTokens.SaveFailureReasonMaxWidth` | `380` | `ChromeTokens.cs` |
| `ChromeTokens.SaveIdleGateMs` | `1_000` | `ChromeTokens.cs` |
| `ChromeTokens.ScreenPictureHeight` | `64` | `ChromeTokens.cs` |
| `ChromeTokens.SearchOverlayBottomMargin` | `PagerStripHeight - PillInset` | `ChromeTokens.cs` |
| `ChromeTokens.SearchResultCardHeight` | `152` | `ChromeTokens.cs` |
| `ChromeTokens.SettingsDropdownHeight` | `48` | `ChromeTokens.cs` |
| `ChromeTokens.SettingsIconBoxSize` | `20` | `ChromeTokens.cs` |
| `ChromeTokens.SettingsPageMaxWidth` | `780` | `ChromeTokens.cs` |
| `ChromeTokens.SettingsPrimaryRowMinHeight` | `64` | `ChromeTokens.cs` |
| `ChromeTokens.SettingsSearchFieldHeight` | `32` | `ChromeTokens.cs` |
| `ChromeTokens.SettingsSecondaryRowMinHeight` | `56` | `ChromeTokens.cs` |
| `ChromeTokens.SettingsSubrowIndent` | `48` | `ChromeTokens.cs` |
| `ChromeTokens.SettingsTextBoxMaxHeight` | `68` | `ChromeTokens.cs` |
| `ChromeTokens.SettingsTextBoxMinHeight` | `40` | `ChromeTokens.cs` |
| `ChromeTokens.SettingsWindowHeight` | `720` | `ChromeTokens.cs` |
| `ChromeTokens.SettingsWindowMinHeight` | `460` | `ChromeTokens.cs` |
| `ChromeTokens.SettingsWindowMinWidth` | `640` | `ChromeTokens.cs` |
| `ChromeTokens.SettingsWindowWidth` | `900` | `ChromeTokens.cs` |
| `ChromeTokens.ShortcutDialogWidth` | `420` | `ChromeTokens.cs` |
| `ChromeTokens.TextEdgeFadeBand` | `Space.M` | `ChromeTokens.cs` |
| `ChromeTokens.TintAlpha` | `0.4` | `ChromeTokens.cs` |
| `ChromeTokens.ToastBottomMargin` | `PagerStripHeight + ToastPagerClearance` | `ChromeTokens.cs` |
| `ChromeTokens.ToastMaxWidth` | `380` | `ChromeTokens.cs` |
| `ChromeTokens.ToastPagerClearance` | `Space.Nudge` | `ChromeTokens.cs` |
| `ChromeTokens.ToastTextMaxWidth` | `300` | `ChromeTokens.cs` |
| `ChromeTokens.ToggleTrackHeight` | `20` | `ChromeTokens.cs` |
| `ChromeTokens.ToggleTrackWidth` | `40` | `ChromeTokens.cs` |
| `ChromeTokens.TopIconHoverRadius` | `8` | `ChromeTokens.cs` |
| `ChromeTokens.TopRevealZone` | `DockedRowHeight / 2` | `ChromeTokens.cs` |
| `ChromeTokens.TopScrollableWhitespace` | `DockedRowHeight` | `ChromeTokens.cs` |
| `Contrast.Mark` | `3` | `Contrast.cs` |
| `Contrast.Text` | `4.5` | `Contrast.cs` |
| `Icons.About` | `Fluent 0xF4A3` | `Icons.cs` |
| `Icons.Accent` | `Fluent 0xF2F5` | `Icons.cs` |
| `Icons.Answers` | `Fluent 0xF20A` | `Icons.cs` |
| `Icons.AppData` | `Fluent 0xE542` | `Icons.cs` |
| `Icons.Back` | `Segoe 0xE76B` | `Icons.cs` |
| `Icons.Background` | `Fluent 0xF592` | `Icons.cs` |
| `Icons.Backing` | `Fluent 0xF462` | `Icons.cs` |
| `Icons.CaretGlide` | `Fluent 0xE0CD` | `Icons.cs` |
| `Icons.CleanMarkdown` | `Fluent 0xE7DB` | `Icons.cs` |
| `Icons.Close` | `Segoe 0xE711` | `Icons.cs` |
| `Icons.CodeLanguages` | `Fluent 0xF2EF` | `Icons.cs` |
| `Icons.Copy` | `Segoe 0xE8C8` | `Icons.cs` |
| `Icons.Cut` | `Segoe 0xE8C6` | `Icons.cs` |
| `Icons.DarkMode` | `Fluent 0xF87D` | `Icons.cs` |
| `Icons.Delete` | `Segoe 0xE74D` | `Icons.cs` |
| `Icons.EditorTab` | `Fluent 0xF386` | `Icons.cs` |
| `Icons.FeelTab` | `Fluent 0xEC8D` | `Icons.cs` |
| `Icons.Folding` | `Fluent 0xE32F` | `Icons.cs` |
| `Icons.Font` | `Fluent 0xF7E4` | `Icons.cs` |
| `Icons.FontSize` | `Fluent 0xF7E6` | `Icons.cs` |
| `Icons.Forward` | `Segoe 0xE76C` | `Icons.cs` |
| `Icons.FreshNote` | `Fluent 0xF2DD` | `Icons.cs` |
| `Icons.Grid` | `Segoe 0xF0E2` | `Icons.cs` |
| `Icons.Hide` | `Segoe 0xE921` | `Icons.cs` |
| `Icons.HideWhenClickAway` | `Fluent 0xEBD0` | `Icons.cs` |
| `Icons.HorizontalScroll` | `Fluent 0xF18D` | `Icons.cs` |
| `Icons.KeepOnTop` | `Segoe 0xE718` | `Icons.cs` |
| `Icons.KeepOnTopFilled` | `Segoe 0xE842` | `Icons.cs` |
| `Icons.KeepOnTopSetting` | `Fluent 0xF601` | `Icons.cs` |
| `Icons.Keyboard` | `Fluent 0xF4B8` | `Icons.cs` |
| `Icons.LightMode` | `Fluent 0xF8A1` | `Icons.cs` |
| `Icons.LineSpacing` | `Fluent 0xF7F6` | `Icons.cs` |
| `Icons.List` | `Segoe 0xEA37` | `Icons.cs` |
| `Icons.NewNote` | `Segoe 0xE710` | `Icons.cs` |
| `Icons.NewNoteHotkey` | `Fluent 0xF8BD` | `Icons.cs` |
| `Icons.NotesFolder` | `Fluent 0xF418` | `Icons.cs` |
| `Icons.OpenAt` | `Fluent 0xEE77` | `Icons.cs` |
| `Icons.OpenTrash` | `Fluent 0xEF12` | `Icons.cs` |
| `Icons.PaperTone` | `Fluent 0xF2F9` | `Icons.cs` |
| `Icons.Paste` | `Segoe 0xE77F` | `Icons.cs` |
| `Icons.RecoveredNotes` | `Fluent 0xF47E` | `Icons.cs` |
| `Icons.Redo` | `Segoe 0xE7A6` | `Icons.cs` |
| `Icons.ReduceMotion` | `Fluent 0xF103` | `Icons.cs` |
| `Icons.RestoreDefaults` | `Fluent 0xF19F` | `Icons.cs` |
| `Icons.Search` | `Segoe 0xE721` | `Icons.cs` |
| `Icons.SelectAll` | `Segoe 0xE8B3` | `Icons.cs` |
| `Icons.Settings` | `Segoe 0xE713` | `Icons.cs` |
| `Icons.SettingsChevron` | `Fluent 0xF2A2` | `Icons.cs` |
| `Icons.SettingsSearch` | `Fluent 0xEA7C` | `Icons.cs` |
| `Icons.ShortLinks` | `Fluent 0xF4E4` | `Icons.cs` |
| `Icons.SidePane` | `Fluent 0xE8BD` | `Icons.cs` |
| `Icons.Split` | `Segoe 0xE90D` | `Icons.cs` |
| `Icons.Stack` | `Segoe 0xE81E` | `Icons.cs` |
| `Icons.StartWithWindows` | `Fluent 0xF60E` | `Icons.cs` |
| `Icons.StorageTab` | `Fluent 0xE466` | `Icons.cs` |
| `Icons.System` | `Fluent 0xF359` | `Icons.cs` |
| `Icons.TaskbarIcon` | `Fluent 0xF359` | `Icons.cs` |
| `Icons.ThemeTab` | `Fluent 0xF2F5` | `Icons.cs` |
| `Icons.Tint` | `Fluent 0xF595` | `Icons.cs` |
| `Icons.UiTab` | `Fluent 0xF592` | `Icons.cs` |
| `Icons.Undo` | `Segoe 0xE7A7` | `Icons.cs` |
| `Icons.VerticalScroll` | `Fluent 0xE077` | `Icons.cs` |
| `Paint.AccentPen` | `.9` | `Paint.cs` |
| `Paint.CardBorderOff` | `.3` | `Paint.cs` |
| `Paint.CardBorderOn` | `.9` | `Paint.cs` |
| `Paint.CaretBlock` | `.5` | `Paint.cs` |
| `Paint.Dimmed` | `.5` | `Paint.cs` |
| `Paint.DisabledIcon` | `.5` | `Paint.cs` |
| `Paint.ErrorHint` | `.8` | `Paint.cs` |
| `Paint.FieldBorder` | `.5` | `Paint.cs` |
| `Paint.FocusBorder` | `.9` | `Paint.cs` |
| `Paint.GlassPreview` | `.5` | `Paint.cs` |
| `Paint.GlassScrim` | `.5` | `Paint.cs` |
| `Paint.GridPen` | `.9` | `Paint.cs` |
| `Paint.HoverFace` | `.9` | `Paint.cs` |
| `Paint.IconInkAlpha` | `220` | `Paint.cs` |
| `Paint.KeyPen` | `.5` | `Paint.cs` |
| `Paint.MarkerFaint` | `.3` | `Paint.cs` |
| `Paint.MarkerStrong` | `.8` | `Paint.cs` |
| `Paint.MenuCaption` | `.8` | `Paint.cs` |
| `Paint.MenuSeparator` | `.3` | `Paint.cs` |
| `Paint.PinnedGlyphAlpha` | `110` | `Paint.cs` |
| `Paint.PopupBorder` | `.5` | `Paint.cs` |
| `Paint.PreviewBar` | `.09` | `Paint.cs` |
| `Paint.PreviewPad` | `.8` | `Paint.cs` |
| `Paint.RestBorder` | `.55` | `Paint.cs` |
| `Paint.ResultBorder` | `.3` | `Paint.cs` |
| `Paint.ResultCurrent` | `.9` | `Paint.cs` |
| `Paint.ResultSurface` | `.5` | `Paint.cs` |
| `Paint.SampleFadeAlpha` | `40` | `Paint.cs` |
| `Paint.SampleGradientAlpha` | `90` | `Paint.cs` |
| `Paint.Toast` | `.9` | `Paint.cs` |
| `Shape.Card` | `8` | `Shape.cs` |
| `Shape.CheckboxAccentPen` | `1.6` | `Shape.cs` |
| `Shape.CheckboxCheckPen` | `1.9` | `Shape.cs` |
| `Shape.CheckboxRadiusScale` | `0.28` | `Shape.cs` |
| `Shape.CheckboxSizeScale` | `0.86` | `Shape.cs` |
| `Shape.Control` | `4` | `Shape.cs` |
| `Shape.Dialog` | `12` | `Shape.cs` |
| `Shape.DropdownShadowBlur` | `16` | `Shape.cs` |
| `Shape.Hairline` | `2` | `Shape.cs` |
| `Shape.Key` | `4` | `Shape.cs` |
| `Shape.MenuShadowBlur` | `16` | `Shape.cs` |
| `Shape.Popup` | `8` | `Shape.cs` |
| `Shape.PopupShadowDarkAlpha` | `110` | `Shape.cs` |
| `Shape.PopupShadowLightAlpha` | `40` | `Shape.cs` |
| `Shape.PopupShadowOffsetY` | `4` | `Shape.cs` |
| `Shape.Thick` | `2` | `Shape.cs` |
| `Shape.Thin` | `1` | `Shape.cs` |
| `Space.CaretBarWidth` | `2` | `Space.cs` |
| `Space.CaretSampleMargin` | `1` | `Space.cs` |
| `Space.CompletionCommandColumnWidth` | `86` | `Space.cs` |
| `Space.CompletionModeColumnWidth` | `70` | `Space.cs` |
| `Space.EditorHintLeftInset` | `EditorTextInsetLeft + EditorHintOffset` | `Space.cs` |
| `Space.EditorHintOffset` | `1` | `Space.cs` |
| `Space.EditorHintTopInset` | `Xs + EditorHintTopOffset` | `Space.cs` |
| `Space.EditorHintTopOffset` | `3` | `Space.cs` |
| `Space.EditorTextInsetLeft` | `36` | `Space.cs` |
| `Space.EditorTextInsetRight` | `30` | `Space.cs` |
| `Space.HelpKeyColumnWidth` | `150` | `Space.cs` |
| `Space.L` | `16` | `Space.cs` |
| `Space.M` | `12` | `Space.cs` |
| `Space.MotionSampleBarHeight` | `3` | `Space.cs` |
| `Space.MotionSampleBarWidth` | `14` | `Space.cs` |
| `Space.MotionSampleHeight` | `46` | `Space.cs` |
| `Space.Nudge` | `2` | `Space.cs` |
| `Space.S` | `8` | `Space.cs` |
| `Space.ToggleKnobInset` | `3` | `Space.cs` |
| `Space.Xl` | `24` | `Space.cs` |
| `Space.Xs` | `4` | `Space.cs` |
| `Space.Xxl` | `32` | `Space.cs` |
| `Typography.Body` | `14` | `Typography.cs` |
| `Typography.BodyEmphasis` | `14` | `Typography.cs` |
| `Typography.Bold` | `FontWeight.Bold` | `Typography.cs` |
| `Typography.CapsLetterSpacing` | `0.8` | `Typography.cs` |
| `Typography.Caption` | `12` | `Typography.cs` |
| `Typography.DialogTitle` | `18` | `Typography.cs` |
| `Typography.Display` | `28` | `Typography.cs` |
| `Typography.EditorChipScale` | `0.93` | `Typography.cs` |
| `Typography.EditorFallback` | `", Cascadia Mono, Consolas, monospace"` | `Typography.cs` |
| `Typography.IconBody` | `14` | `Typography.cs` |
| `Typography.IconInput` | `16` | `Typography.cs` |
| `Typography.IconLarge` | `20` | `Typography.cs` |
| `Typography.IconMedium` | `16` | `Typography.cs` |
| `Typography.IconSmall` | `14` | `Typography.cs` |
| `Typography.IconXLarge` | `20` | `Typography.cs` |
| `Typography.Input` | `16` | `Typography.cs` |
| `Typography.Label` | `12` | `Typography.cs` |
| `Typography.Large` | `14` | `Typography.cs` |
| `Typography.Micro` | `12` | `Typography.cs` |
| `Typography.MonoFamily` | `"Cascadia Mono, Consolas"` | `Typography.cs` |
| `Typography.PanelTitle` | `18` | `Typography.cs` |
| `Typography.Regular` | `FontWeight.Normal` | `Typography.cs` |
| `Typography.Small` | `12` | `Typography.cs` |
| `Typography.Strong` | `FontWeight.SemiBold` | `Typography.cs` |
| `Typography.UiFamily` | `"Segoe UI Variable Text, Segoe UI"` | `Typography.cs` |
<!-- design-tokens:end -->

## Named exceptions

<!-- design-exceptions:start -->
| File | Category | Count | Reason |
| --- | --- | --- | --- |
| `product/AntiNotepad/Editor/Motion.cs` | `radius` | 2 | Caret artwork uses a small radius proportional to block or bar geometry. |
| `product/AntiNotepad/Editor/PadView.cs` | `color` | 1 | Black RGB is the alpha mask for the editor edge gradient. |
| `product/AntiNotepad/Editor/Renderers.cs` | `radius` | 4 | Drawn pause glyph uses small rounded bars; badge uses the shape token. |
| `product/AntiNotepad/Editor/Renderers.cs` | `stroke` | 1 | Drawn editor artwork uses a proportional 1.6 px pen. |
| `product/AntiNotepad/UI/NoteSwipe.cs` | `spacing` | 1 | Minimum denominator guards swipe calculation; not a layout token. |
| `product/AntiNotepad/UI/PadBar.cs` | `color` | 1 | Black RGB is the shadow color; alpha uses the shadow tokens. |
| `product/AntiNotepad/UI/PadMenu.cs` | `color` | 1 | Black RGB is the shadow color; alpha uses the shadow tokens. |
| `product/AntiNotepad/UI/PadMenu.cs` | `spacing` | 1 | Conditional spacing uses Space.Nudge; numbers are array indices. |
| `product/AntiNotepad/UI/PadWindow.Layout.cs` | `font-size` | 2 | Scanner matches button widths after tokenized text, not font sizes. |
| `product/AntiNotepad/UI/PadWindow.Navigation.cs` | `spacing` | 1 | Window width is calculated from screen bounds and scale. |
| `product/AntiNotepad/UI/PadWindow.cs` | `opacity` | 1 | Content reveal animation interpolates its starting opacity. |
| `product/AntiNotepad/UI/PadWindow.cs` | `spacing` | 1 | Split window width is derived from the current width. |
| `product/AntiNotepad/UI/SettingsParts.cs` | `color` | 1 | Black RGB is the popup shadow color; alpha uses the shadow tokens. |
| `product/AntiNotepad/UI/SettingsParts.cs` | `font-size` | 1 | Scanner matches thickness after tokenized icon size, not a font size. |
| `product/AntiNotepad/UI/SettingsParts.cs` | `radius` | 9 | Round swatch and drawn sample artwork use geometry-specific radii. |
| `product/AntiNotepad/UI/SettingsWindow.cs` | `font-size` | 1 | Scanner matches thickness after tokenized icon size, not a font size. |
| `*` | `timer` | 12 | Timing and motion durations are outside the visual token ratchet. |
| `*` | `tween` | 0 | Timing and motion durations are outside the visual token ratchet. |
<!-- design-exceptions:end -->

### Design rationale

(a literal kept at the control, each with a reason)

| # | Exception | Reason |
| --- | --- | --- |
| 1 | `BadgeTopPadding` 1, `BadgeBottomPadding` 2 | Optical centering of 12 px text in a 20 px badge; not a gap |
| 2 | `PagerCountDigitWidth` 14 | Width of a digit in the count text, a font measurement, not a spacing |
| 3 | `TopBackdrop` stops 40 / 52, blur 10, tint .4, mask alphas | Lab-tuned and locked in `decisions.md`; part of one gradient |
| 4 | Checkbox renderer: size x .86, radius x .28, pens 1.6 / 1.9 | Drawn in proportion to the editor font size |
| 5 | `DrawChip` font x .93 and widths | Tracks the editor font size, not the UI scale |
| 6 | Nudge 2 (box extra, toast clearance, Seg gap, line gaps) | Hairline gaps between 0 and 4; Fluent keeps 2 for icon alignment |
| 7 | Toast and dialog action pad 8 x 2 | Compact text buttons sit in a bar of 32 high; full 12 x 4 would crowd the toast |
| 8 | Search query radius Md 8 | Prominent 16 px input; matches the overlay card it sits on |
| 9 | Accent dot selected ring colored Ink, not Accent | The dot is itself the accent color; the ring has to differ |
| 10 | Editor `Bold` weight (markdown headings, `**bold**`) | The user's content, not chrome; Fluent's no-Bold rule is for UI |
| 11 | Material picture (no radius), pager `PillHeight` 42 and strip 50 | Sample art; derived from button 32 + inset + border, kept so the lock holds |
| 12 | Drawn sample and glyph alphas as bytes (220, 110, 90, 40) | Painted into bitmaps and glyph color, not UI washes |
| 13 | Icon button hover radius Md 8 on a 32 square | Locked hover radii (8, and 9 to 8 under R); bigger than Control 4 on purpose |
| 14 | `Space.EditorTextInsetLeft` 36, `EditorTextInsetRight` 30 | Moves the user's text position; 30 to 32 would rewrap existing lines |
| 15 | `Space.EditorHintOffset` 1, `EditorHintTopOffset` 3 | Derives hint placement from editor text inset and top alignment |
| 16 | `Space.ToggleKnobInset` 3 | Centers the knob in the 40 x 20 track |
| 17 | `Space.CaretBarWidth` 2, `CaretSampleMargin` 1 | Draws and centers a real caret in the style sample |
| 18 | `Space.CompletionModeColumnWidth` 70, `CompletionCommandColumnWidth` 86 | Fits the completion keywords without wrapping |
| 19 | `Space.HelpKeyColumnWidth` 150 | Fits shortcut names without wrapping |
| 20 | `Space.MotionSampleHeight` 46, `MotionSampleBarWidth` 14, `MotionSampleBarHeight` 3 | Preserves motion preview sample proportions |
| 21 | `Typography.CapsLetterSpacing` .8 | Extra tracking separates all-caps section labels |
| 22 | `ChromeTokens.GlyphIconBoxExtra` 2 | Adds optical clearance around the 16 px glyph |
| 23 | `ChromeTokens.Size` window, panel, popup, row and sample dimensions | Named layout sizes preserve surface geometry on the 4 px grid |
| 24 | `Paint.DisabledIcon` .5 | Keeps disabled glyphs subdued |
| 25 | `Typography.UiFamily`, `MonoFamily`, `EditorFallback` | Font fallback lists select installed UI and editor fonts |

The machine-checked literal exceptions and their reasons are in [design-exceptions.tsv](../tools/design-exceptions.tsv). File-size exceptions and maximums are in [size-exceptions.tsv](../tools/size-exceptions.tsv). The Clear-tint hint exception is described above.
