# Scale proposal: a few values per category, every role and control mapped

Paper only; nothing here is applied and **no product file, token or test changes** (A4j-scale-proposal). The next step is a
lab build to compare the look, then apply. Built from `docs/design-systems.md` (tiers, step-count aims),
`docs/design-fluent.md` (Windows 11 values), `.coord/reports/A4g-control-groups-map.md` (15 control kinds), today's tokens
in `product/AntiNotepad/Theme/` and `Motion/MotionTokens.cs`, and the margins, paddings and gaps read from `UI/` and `Editor/`.
Values are DIPs; dark/light pairs are in that order; "R" = recommended, "Alt" = the one alternative.

**Shape of the system** (from `design-systems.md` section 5): **scale** (few raw steps) > **role** (existing names such as
`Typography.Body`, `Shape.Control`, each naming one step) > **control** (15 kinds citing roles). Names stay; only values move.
A literal at the control is an exception (section 6) and never edits a scale.

## 1. Where we are today, and the recommended count

| Category | Today (distinct values) | R steps | Alt steps | Fluent / Windows reference |
| --- | --- | --- | --- | --- |
| Corners | 1, 5, 6, 7, 8, 10, 12 + round (7) | 5 (2, 4, 8, 12, round) | 5 (4, 6, 10, 14, round) | Windows 0/4/8; Fluent 2 0/2/4/8/12/circle |
| Text size | 11, 11.5, 12, 12.5, 13, 13.5, 14, 15, 17, 18, 26 (11) | 5 (12, 14, 16, 18, 28) | 5 (12, 13, 15, 18, 26) | ramp 12/14/18/20/28, all even |
| Weight | Regular, SemiBold, Bold (3) | 2 (Regular, Strong) | 3 (keep Bold for editor only, see 6) | Regular + Semibold only |
| Icon size | 13, 14, 15, 16, 18, 20 (6) | 3 (14, 16, 20) | 3 (14, 16, 18) | not published |
| Stroke | 1, 1.2, 1.5, 1.6, 1.9, 2, 2.5 (7) | 2 (1, 2) | 3 (1, 1.5, 2) | Fluent 2: 1 to 4 |
| Shadow | blur 14/16/18, offset 3/4, 2 alpha pairs (5 loose numbers) | 2 levels (Low, High) | 2 levels, today's numbers | Windows 2/8/16/32/128 |
| Motion | 14 tokens, 10 distinct values 80 to 200 ms | 4 (80, 120, 160, 200) | 3 (80, 160, 200) | Windows 83/167/250 |
| Transparency | 10 wash pairs + 13 distinct strengths (.25 to .92) | 4 washes + 4 strengths | 4 washes + 3 strengths | Smoke 30%; layers 25 to 70% |
| Spacing | 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20, 22, 24 (17) | 6 (4, 8, 12, 16, 24, 32) + Nudge 2 | 6 (4, 6, 8, 12, 16, 24) + Nudge 2 | 4 px base, ramp 0/2/4/6/8/10/12/16/20/24/32 |

Aim from the study was 5/5/2/3/2/2/4/5/6; the two that overshoot are transparency (two families) and weight Alt.

## 2. Scales: recommended and alternative

Rounding rule for every mapping below: **nearest step; a tie goes up**; a value already on the 4 px grid is kept.

### 2.1 Corners

| Step | R | Alt | R reason |
| --- | --- | --- | --- |
| Xs | 2 | - | Fluent 2: shapes under 32 px use 2; caret samples |
| Sm | 4 | 4 | Windows 11 `ControlCornerRadius`; every in-page control |
| (mid) | - | 6 | Alt keeps controls softer than Windows |
| Md | 8 | 10 | Windows `OverlayCornerRadius`; popups, cards, hover radii (locked at 8) |
| Lg | 12 | 14 | Fluent 2 popovers and button sheets; pill and dialog (both 12 today) |
| Round | size / 2 | size / 2 | unchanged |

- **R:** the Windows 11 pairs 4 / 8 plus the two ends Fluent 2 adds. Seven values fall to five. Biggest look change (7 to 4).
- **Alt:** controls 6 instead of 4, popups and cards 10, dialog and pill 14. Closer to today's soft feel (7/10/12), but 6 and 10 are not Windows steps and the hover radius lock (8) falls off the scale.

### 2.2 Text size

| Step | R | Alt | R reason |
| --- | --- | --- | --- |
| S | 12 | 12 | Fluent Caption and the Regular floor; absorbs 11, 11.5, 12, 12.5 |
| M | 14 | 13 | R: Fluent Body 14; Alt: keeps our body at 13 |
| L | 16 | 15 | only the search input uses it (15 today) |
| XL | 18 | 18 | Fluent Body Large; panel and dialog titles |
| 2XL | 28 | 26 | R: Fluent Title 28; Alt: today's Display |

- **R:** even sizes on the Fluent ramp; 7 of 11 sizes move, all by 0.5 to 2. Body 13 to 14 is the visible one.
- **Alt:** fewest moves (Display, Input and PanelTitle stay) but body stays 13 and Large 14 drops to 13; 13 and 15 are still off the ramp.

### 2.3 Weight

| Step | R | Alt |
| --- | --- | --- |
| Regular | 400 | 400 |
| Strong | 600 (SemiBold) | 600 |
| Bold | none in chrome | 700, **editor markdown only** (`Colorizers.cs`) |

- **R:** Windows publishes Regular and Semibold only. `Bold` is used only by the markdown colorizer on the user's own
  text (headings, `**bold**`), so R keeps that as an exception (section 6) and the chrome scale stays 2.
- **Alt:** counts Bold as a named third step instead of an exception. Same pixels.

### 2.4 Icon size

| Step | R | Alt | R reason |
| --- | --- | --- | --- |
| S | 14 | 14 | pairs with text M; chevrons, `IconBody`, `IconSmall` |
| M | 16 | 16 | `ChromeTokens.IconGlyphSize`; bar and menu glyphs |
| L | 20 | 18 | R: one large size (merges 18 and 20); Alt: pairs with XL text 18 |

- **R:** all even; 13 to 14, 15 to 16, 18 to 20 (mode-tile faces get 2 bigger). **Alt:** `IconXLarge` 20 to 18 instead.

### 2.5 Stroke

| Step | R | Alt | R reason |
| --- | --- | --- | --- |
| Thin | 1 | 1 | Fluent 2 thin; borders, dividers, separators |
| (mid) | - | 1.5 | Alt keeps today's focus and tile half-step |
| Thick | 2 | 2 | Fluent 2 thick; focus ring, selected ring, selected swatch |

- **R:** whole pixels only (Fluent). 1.2, 1.5, 2.5 snap; the checkbox pen (1.6, 1.9) is an exception. **Alt:** fewest moves, one fractional step stays.

### 2.6 Shadow

| Level | R (blur / offset-y / dark alpha / light alpha) | Alt (today's numbers) | Used by |
| --- | --- | --- | --- |
| Low | 12 / 2 / 90 / 28 | 14 / 3 / 90 / 28 | pill, toast, save-failure card |
| High | 16 / 4 / 110 / 40 | 18 / 4 / 110 / 40 | menu, dropdown popup |

- **R:** two named levels on even numbers, one rule (higher level: bigger blur and offset, heavier alpha), as Windows 11 ties softness to elevation. Windows gives no blur or offset numbers, so these are ours.
- **Alt:** pick the two most used current sets; only the dropdown (16 to 18) moves. Less tidy, near-zero visible change.

### 2.7 Motion

| Step | R | Alt | R reason |
| --- | --- | --- | --- |
| Quick | 80 | 80 | Windows Faster 83; caret glide |
| Fast | 120 | - | toast in, overlay in; Alt drops this step |
| Base | 160 | 160 | Windows Fast 167; almost every transition (140 to 170 today) |
| Slow | 200 | 200 | between Windows Fast and Normal 250; nudge, note arrive, spring back |

- **R:** 40 ms ladder (80, 120, 160, 200). **Alt:** three buckets like Windows; Fast folds into Base (toast in +40, overlay in +30). Show/hide speed is the priority: R moves `WindowSlide` +10 and `WindowFade` 0; lab must time both.

### 2.8 Transparency

Two families, because wash roles differ by theme and strength roles do not.

| Washes (ink over surface) | R dark / light | Used by |
| --- | --- | --- |
| Faint | .03 / .03 | row wash |
| Hover | .09 / .06 | hover, help key, preview frame and well, completion fill, light borders |
| Press | .15 / .11 | press, badges |
| Selected | .22 / .18 | selected |

| Strengths (one value, both themes) | R | Alt | Used by |
| --- | --- | --- | --- |
| Faint | .3 | .3 | resting card border, result border, faint markers, pill border, menu separator |
| Mid | .5 | .6 | popup, field and rest borders, dividers, scrims, dimmed, caret block, key pen |
| Strong | .8 | - | menu caption, strong marker, error hint, preview pad |
| Full | .9 | .9 | focus, current, accent pen, hover face, card and toast surface |

- **R:** keeps today's four wash steps (they already follow a pattern) and snaps 25 strengths to 4; scrim .55 to .5 is also closer to Windows Smoke (30%) without going there.
- **Alt:** three strengths (.3, .6, .9): fewer numbers, but Mid moves +.1 and Strong folds into Full (+.1). The user's tint presets are data (section 6).

### 2.9 Spacing

| Step | R | Alt | R reason |
| --- | --- | --- | --- |
| XS | 4 | 4 | Fluent base unit; bar edge, pill inset, badge gap |
| S | 8 | 6 | Windows gap between buttons and controls |
| (S2) | - | 8 | Alt keeps a 6 nudge step |
| M | 12 | 12 | Windows control-to-label gap; content gap |
| L | 16 | 16 | surface-to-edge text; row side padding |
| XL | 24 | 24 | group gaps |
| 2XL | 32 | - | button size, section gap |
| Nudge | 2 (allowed, not a step) | 2 | hairline gaps and optical adjustments |

- **R:** pure multiples of 4 (Fluent's rule); 6 and 10 become 8 and 12. **Alt:** Fluent's own 6 nudge stays, so fewer pixels move; 10 and 14/18 still snap.
- Today's gaps seen in code: Spacing 1, 2 (x5), 3 (x2), 4 (x3), 5 (x3), 6 (x4), 7, 8 (x2), 9, 10 (x2), 12 (x5), 14. Margins/padding mix 1 to 24 plus 28, 30, 36, 37, 46, 52.
- **Mapping used below:** 1 to 0 or Nudge 2 (optical), 2 Nudge, 3 to 4, 5 to 4, 6 to 8, 7 to 8, 9 to 8, 10 to 12, 14 to 16, 18 to 16, 20 to 24, 22 to 24, 28 to 32. A tie such as 6 or 10 goes up.

Strokes: `Shape.Thin` 1 and `Shape.Thick` 2. Spacing: `Space.Nudge` 2, `Xs` 4, `S` 8, `M` 12, `L` 16, `Xl` 24, `Xxl` 32.

## 3. Roles to step (every token)

### 3.1 Typography, Shape, Motion, Paint

| Role | Today | New (R) | Change |
| --- | --- | --- | --- |
| Typography.Micro | 11 | S 12 | +1 |
| Typography.Caption | 11.5 | S 12 | +0.5 |
| Typography.Small | 12 | S 12 | 0 |
| Typography.Label | 12.5 | S 12 | -0.5 |
| Typography.Body | 13 | M 14 | +1 |
| Typography.BodyEmphasis | 13.5 | M 14 (Strong) | +0.5 |
| Typography.Large | 14 | M 14 | 0 |
| Typography.Input | 15 | L 16 | +1 |
| Typography.DialogTitle | 17 | XL 18 | +1 |
| Typography.PanelTitle | 18 | XL 18 | 0 |
| Typography.Display | 26 | 2XL 28 | +2 |
| Typography.Regular / Strong / Bold | 400 / 600 / 700 | Regular / Strong / exception (editor only) | 0 / 0 / chrome use none |
| Typography.IconBody / IconSmall / IconInput | 13 / 14 / 15 | S 14 / S 14 / M 16 | +1 / 0 / +1 |
| Typography.IconMedium / IconLarge / IconXLarge | 16 / 18 / 20 | M 16 / L 20 / L 20 | 0 / +2 / 0 |
| Shape.Hairline | 1 | Xs 2 | +1 |
| Shape.Key | 5 | Sm 4 | -1 |
| Shape.Control | 7 | Sm 4 | -3 |
| Shape.Popup | 8 | Md 8 | 0 |
| Shape.Card | 10 | Md 8 | -2 |
| Shape.Dialog | 12 | Lg 12 | 0 |
| Shape.Round | size / 2 | Round | 0 |
| Shape.MenuShadowBlur / DropdownShadowBlur | 18 / 16 | High 16 / 16 | -2 / 0 |
| Shape.PopupShadowOffsetY | 4 | High 4 | 0 |
| Shape.PopupShadowDark / LightAlpha | 110 / 40 | High 110 / 40 | 0 |
| MotionTokens.WindowSlide / ContentReveal / WindowFade / OverlayIn | 150 / 140 / 160 / 130 | Base 160 / Base 160 / Base 160 / Fast 120 | +10 / +20 / 0 / -10 |
| MotionTokens.BarsReveal / PillFade / ChromeShift / Nudge | 170 / 150 / 180 / 180 | Base 160 / Base 160 / Slow 200 / Slow 200 | -10 / +10 / +20 / +20 |
| MotionTokens.ToastIn / ToastOut | 120 / 160 | Fast 120 / Base 160 | 0 / 0 |
| MotionTokens.NoteArrive / SwipeSpringBack / CaretGlide | 190 / 200 / 80 | Slow 200 / Slow 200 / Quick 80 | +10 / 0 / 0 |
| MotionTokens.SettingsPage | 150 | Base 160 | +10 |
| Paint.Hover / Press / Selected | .09/.06, .15/.11, .22/.18 | Hover / Press / Selected | 0 |
| Paint.RowWash | .03/.025 | Faint .03/.03 | 0 / +.005 |
| Paint.HelpWash | .10/.06 | Hover .09/.06 | -.01 / 0 |
| Paint.PreviewFrame | .09/.10 | Hover .09/.06 | 0 / -.04 |
| Paint.PreviewWell | .06/.045 | Hover .09/.06 | +.03 / +.015 |
| Paint.CompletionFill | .085/.075 | Hover .09/.06 | +.005 / -.015 |
| Paint.BadgeFill | .14/.10 | Press .15/.11 | +.01 / +.01 |
| Paint.CardSurface | .92/.90 | Full .9 | -.02 / 0 |
| Paint.Toast | .92 | Full .9 | -.02 |
| Paint.PopupBorder / FieldBorder / RestBorder | .45 / .5 / .5 | Mid .5 | +.05 / 0 / 0 |
| Paint.ResultBorder | .25 | Faint .3 | +.05 |
| Paint.ResultCurrent / FocusBorder / AccentPen / HoverFace / CardBorderOn / GridPen | .9 / .9 / .9 / .86 / .9 / .9 | Full .9 | 0 (HoverFace +.04) |
| Paint.ResultSurface / Dimmed / GlassScrim | .55 | Mid .5 | -.05 |
| Paint.GlassPreview | .6 | Mid .5 | -.1 |
| Paint.ErrorHint / MenuCaption / MarkerStrong / PreviewPad | .78 / .8 / .8 / .8 | Strong .8 | +.02 / 0 / 0 / 0 |
| Paint.MenuSeparator | .4 | Faint .3 | -.1 |
| Paint.CaretBlock | .45 | Mid .5 | +.05 |
| Paint.MarkerFaint / CardBorderOff | .35 / .3 | Faint .3 | -.05 / 0 |
| Paint.KeyPen | .55 | Mid .5 | -.05 |
| Paint.PreviewBar | .10 | Hover wash .09/.06 | -.01 / -.04 |
| Paint.IconInkAlpha / PinnedGlyphAlpha / SampleGradientAlpha / SampleFadeAlpha | 220 / 110 / 90 / 40 (byte) | exceptions (section 6) | 0 |

### 3.2 ChromeTokens (the locked set; **bold** = value changes)

| Value | Today | New (R) | Change |
| --- | --- | --- | --- |
| DockedRowHeight, TopScrollableWhitespace | 40 | 40 (size) | 0 |
| ButtonSize | 32 | 32 (size, 2XL) | 0 |
| IconGlyphSize | 16 | Icon M 16 | 0 |
| GlyphIconBoxExtra | 2 | Nudge 2 | 0 |
| BarEdgeInset, BadgeSpacing, PillInset, PillBottomInset | 4 | XS 4 | 0 |
| **FirstItemInset** | 10 | M 12 | +2 |
| BadgeHeight | 20 | 20 (size) | 0 |
| **BadgeRightInset** | 6 | S 8 | +2 |
| MetaSpacing, FloatGap, FloatPagerRevealZone | 8 | S 8 | 0 |
| **IconSpacing** | 3 | XS 4 | +1 |
| **BadgeCornerRadius** | 6 | Sm 4 | -2 |
| **BadgeHorizontalPadding** | 7 | S 8 | +1 |
| BadgeTopPadding / BadgeBottomPadding | 1 / 2 | exception (optical) | 0 |
| **BadgeDarkFillAlpha / BadgeLightFillAlpha** | .18 / .13 | Press .15 / .11 | -.03 / -.02 |
| **BadgeFontSize** | 11.5 | S 12 | +0.5 |
| SavedLabelFontSize | 12 | S 12 | 0 |
| **RunningTimerFontSize** | 11.5 | S 12 | +0.5 |
| TextEdgeFadeBand | 12 | M 12 | 0 |
| TopRevealZone, OverlayPagerRevealZone | 20 | 20 (size) | 0 |
| TopIconHoverRadius, PagerArrowHoverRadius, PagerCountHoverRadius | 8 | Md 8 | 0 |
| **PagerDotHoverRadius** | 9 | Md 8 | -1 |
| **PagerRowSpacing** | 3 | XS 4 | +1 |
| **PagerCountFontSize** | 11.5 | S 12 | +0.5 |
| **PagerCountBaseWidth** | 30 | 2XL 32 | +2 |
| PagerCountDigitWidth | 14 | exception (text fit) | 0 |
| PagerSeparatorWidth, DividerWidth, PillBorderThickness | 1 | Thin 1 | 0 |
| **PagerSeparatorHeight** | 14 | L 16 | +2 |
| **PagerSeparatorMargin** | 5 | XS 4 | -1 |
| PagerSeparatorAlpha | .5 | Mid .5 | 0 |
| PagerDotSelectedWidth, PagerDotButtonWidth | 16 | L 16 | 0 |
| **PagerDotWidth / PagerDotHeight** | 7 / 7 | S 8 / 8 | +1 / +1 |
| **PagerDotRadius** | 3.5 (height / 2) | Round 4 (height / 2) | +0.5 |
| PagerDotSelectedButtonWidth | 24 | XL 24 | 0 |
| **PagerDotButtonHeight** | 32 | 32 (ButtonSize) | 0 |
| PagerCountButtonHeight | 32 | 32 (ButtonSize) | 0 |
| BarCornerSize | 42 | 42 (PillHeight) | 0 |
| PinRingThickness | 4 | exception (stroke) | 0 |
| DevBadgeLightTextInkMix | .62 | exception (ink mix) | 0 |
| DeleteDebounceMs | 500 ms | timer | 0 |
| PlainIconWeight | 0 | exception (font weight) | 0 |
| **PagerDotBorderThickness** | 1.2 | Thin 1 | -0.2 |
| **PillBorderAlpha** | .35 | Faint .3 | -.05 |
| PillCornerRadius | 12 | Lg 12 | 0 |
| PillHeight, PagerStripHeight | 42, 50 (derived) | 42, 50 (derived) | 0 |
| **PillShadowBlur / PillShadowOffsetY** | 14 / 3 | Low 12 / 2 | -2 / -1 |
| PillShadowDarkAlpha / LightAlpha | 90 / 28 | Low 90 / 28 | 0 |
| **DividerPagerClearance / DividerTopMargin** | 6 / 6 | S 8 / S 8 | +2 / +2 |
| **DividerAlpha** | .45 | Mid .5 | +.05 |
| ToastPagerClearance | 2 | Nudge 2 | 0 |
| DividerBottomMargin, ToastBottomMargin | derived | derived (unchanged: strip stays 50) | 0 |
| TopBackdrop DenseStop / ClearStop | 40 / 52 | exception (lab-tuned lock) | 0 |
| TopBackdrop BlurRadius / TintAlpha | 10 / .4 | exception (lab-tuned lock) | 0 |
| TopBackdrop mask alphas | 1 / .5 / 0 | exception (gradient stops) | 0 |

Values that change: **22 rows** (the bold rows; 26 individual constants, since some rows hold a pair).

## 4. Controls (15 kinds from the A4g map)

R only. "Role" cites the section 3 names. Spacing is `vertical x horizontal` where a margin or padding exists.

| Kind | Radius | Font | Padding / margin | Fill | Border | Shadow | A4g disagreement, resolved |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Text button | Control 4 | Body 14, Strong / Regular | label 12 x 4 (was 12 x 5) | accent primary; Hover / Press wash | Thin 1, Hover wash | none | Save-failure actions (Label, pad 0, radius 7, 34 high) and toast action (pad 6 x 1) become the same role: Body, Control 4, 32 high (ButtonSize), transparent + Hover / Press; the toast action keeps a compact pad 8 x 2 (exception) |
| Icon button | Md 8 (hover) | Icon M 16 | 0 | transparent; Hover / Press | none | none | Settings close 34 and right margin 6 become ButtonSize 32 and margin 8; search tools already 32; top bar and pager radius both Md 8 |
| Toggle | track Round; Seg and Kit.Toggles Control 4 | Body 14 (Seg and Kit.Toggles) | Seg label 12 x 4, gap Nudge 2; Kit.Toggles 8 x 4, gap 4 | Selected wash; accent on | Thin 1 off (was 1.5) | none | Kit.Toggles radius 8 / Label to Control 4 / Body, matching Seg; knob 14 to 12 in the 40 x 20 track (margin 4); checkbox is an exception |
| Dropdown | face Control 4; option Sm 4 | Body 14; chevron Icon S 14 | face 12 x 12 (was 12,0,10,0); option 12 x 8 (was 10 x 6) | Surface; Selected wash | Thin 1, Hover wash | none (popup: High) | Option radius 6 becomes 4 (same as face) |
| Text field | Control 4; search query Md 8 | Body 14; search Input L 16 | 12 x 8 (settings find 8,8,8,0; path 12 x 8) | Surface | Thin 1 rest Mid .5; focus Thick 2 Full .9 | none | Search `FieldBorder` merges with `RestBorder` (Mid); search query keeps the larger Md 8 as a named exception (large input, own font step) |
| Slider | none today | - | - | - | - | - | No instance; nothing proposed |
| Card | Md 8 | title Body 14 Strong / Small S 12 | Row 16 x 12 (was 16 x 10); SubRow 48,12,16,12 (was 52,10,16,10); search card 12 x 8 | CardSurface Full .9; search ResultSurface Mid .5 | Thin 1 Hover wash; search Faint .3, current Thick 2 Full .9 | none | Search card border and current ring stay a different color role but share Thin / Thick; `MotionPreview` frame uses Hover wash + Mid glass (was Preview ink, .6) |
| Preview tile | Md 8 (inner = Md minus ring) | Caption S 12, Strong when selected | button pad 4; picture sizes snap to 4 grid (see 6) | selected accent; else Hover wash | ring Thick 2 selected / Thin 1 otherwise (was 1.5) | none | Tile widths and heights stay layout sizes; caret picture Hairline 1 becomes Xs 2; material picture has no radius (exception, none) |
| Swatch | Round | Caption S 12 | chip face 8 x 8, chip gap 8, dot gap 4 (was 5 / 3), accent-dot gap 8 (was 7) | palette / accent color | selected Thick 2; unselected Thin 1 Faint .3 | none | Chip selected is Accent, dot selected is Ink (the dot is itself accent color; named exception); thickness unified at 2 (was 2 vs 2.5); unselected unified at Faint .3; dots 7 to 8, accent dots 18 to 20 |
| Popup and menu | Md 8; item Sm 4 | Body 14 | popup pad XS 4; row 8 x 0 in a 32 high row; action row 52 high (on grid) | Surface; Hover / Press | Thin 1, Mid .5 (`PopupBorder`) | High 16 / 4 / 110 / 40 | Dropdown popup border (Ink .09/.10) becomes `PopupBorder` Mid; dropdown 16 and menu 18 blur become one High 16 |
| Badge | Sm 4 | Caption S 12 Strong | 8 x (1, 2 optical) | Press .15 / .11 | none | none | Chip (radius 6, 11.5, .18/.13) and `DrawChip` (radius 7, .93 x font, .14/.10) both become Sm 4 and Press; `DrawChip` font stays proportional to the editor (exception) |
| Pill | Lg 12; count Md 8; dots Round | count Caption S 12 Strong | inset 4, bottom 4; count 20 high | Surface; count transparent + Hover / Press | Thin 1, Faint .3 | Low 12 / 2 / 90 / 28 | Radii 12 / 8 / round stay three roles on purpose (container, hit area, mark); dots 8 and hover 8 |
| Overlay panel | save-failure card Lg 12; help key Sm 4 | card title XL 18, body Body 14; key Caption S 12 | card pad L 16 (was 18), margin 12; key 8 x 2 | Surface; scrim Mid .5; key Hover wash | Thin 1 Mid .5 | Low (card) | Help key radius 5 to 4; scrim .55 to .5; card pad 18 to 16 |
| Toast | Md 8 (was Card 10) | Caption S 12 (was Label 12.5); action Strong | 12 x 8 (was 12 x 7), margin 16 | Full .9 (was .92) | none | Low | One instance, nothing to resolve |
| Text roles | none | Heading 2XL 28 Strong; Group Body 14 Strong; Section S 12 Strong; PanelHeader XL 18; card text M / S / S; meta S 12 | Heading 2,0,0,12; Group 2,24,0,8; Section 8,24,0,2; header 8,0,0,0 | none | none | none | Micro, Caption, Small and Label now share 12 and differ by weight and color; roles keep their names so a later merge is optional |

## 5. Alias keys and untouched sizes

- `ChromeTokens` keeps its names (bar measurement lock); each value in 3.2 becomes an alias of a scale step, or a stated exception.
- **Layout sizes are not scale steps.** Free sizes snap to the 4 grid when edited (62 to 64, 54 to 56, 38 to 40, 34 to 32, 150 to 152,
  tile widths 58 / 78 to 56 / 80, picture heights 46 / 62 to 48 / 64); windows (780, 900, 640 min) and panel widths (380 toast max) are not touched.
- The user's tint presets (.55, .7, .82, .92) are a Settings choice, not a Paint token; unaffected here.

## 6. Exceptions (a literal kept at the control, each with a reason)

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

## 7. Visible changes, biggest first (where to look in the lab build)

1. **Corners.** Every button, field, dropdown face and segmented choice goes 7 to 4 (sharper); cards, search, toast 10 to 8;
   badges 6 to 4; help keys 5 to 4. The whole Settings window and the bar will look squarer.
2. **Text.** Body and row titles 13 / 13.5 to 14; the small labels (11, 11.5, 12.5) all to 12; search input 15 to 16; display
   title 26 to 28. Check Settings width and the pager count at 12 px.
3. **Spacing.** Row and field padding 10 to 12 (rows about 4 px taller inside their min heights), group gap 20 to 24, toggle
   knob 14 to 12, swatch and tile snaps (2 to 4 px each). Bar: first item 10 to 12, badge right inset 6 to 8, dividers 6 to 8.
4. **Pager.** Dots 7 to 8, dot hit height 18 to 20, separator 14 to 16, count base 30 to 32, hover radius 9 to 8, border 1.2 to 1.
5. **Shadows.** Pill, toast and failure card shadow 14 / 3 to 12 / 2 (a touch tighter); menu 18 to 16 blur.
6. **Strokes.** Focus ring 1.5 to 2; selected swatch 2.5 to 2; unselected tile ring and off-toggle 1.5 to 1.
7. **Transparency.** Scrim and dimmed .55 to .5; glass preview .6 to .5; toast and card surface .92 to .9; menu separator .4 to .3;
   badge fill slightly lighter (.18 to .15); light-theme preview frame .10 to .06. Small, mostly felt in the tint.
8. **Motion.** Window slide 150 to 160, content reveal 140 to 160, bars reveal 170 to 160, chrome shift and nudge 180 to 200.
   Time the show/hide on the lab build; speed of show/hide is the priority.
9. **Icons.** Mode-tile faces 18 to 20; 13 to 14 and 15 to 16 glyphs get 1 px larger.

## 8. Lock question (one)

`docs/decisions.md`, "Bar insets and chrome tokens", fixes the top bar and pager numbers. Under the recommended scales, **22
ChromeTokens rows (26 constants) change** (section 3.2, bold): first item inset 10 to 12, badge right inset 6 to 8, icon spacing 3 to 4,
badge radius 6 to 4, badge padding 7 to 8, three 11.5 font sizes to 12, badge fill .18 / .13 to .15 / .11, dot hover radius 9
to 8, pager row spacing 3 to 4, count base 30 to 32, separator height 14 to 16 and margin 5 to 4, dots 7 to 8 (and radius 3.5 to
4), dot hit height 18 to 20, dot border 1.2 to 1, pill border alpha .35 to .3, pill shadow 14 / 3 to 12 / 2, divider clearance
and top margin 6 to 8, divider alpha .45 to .5. The 40 bar, 32 buttons, 20 badges, 8 hover radii, 42 pill, 4 insets, 8 gap, 12
fade and the backdrop do **not** change.

**Your call:** lift the bar-and-pager lock for these values so the lab build applies them, or keep them (the lab build then
leaves ChromeTokens as it is and applies the scales to everything else)?
