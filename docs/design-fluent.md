# Fluent study: Windows 11 sizes beside ours

Notes only; nothing here is applied. Read 2026-11-03 from Microsoft's own pages (each value carries its link). A value the
pages do not give says **not published**; none is filled in from memory. The point is to find patterns Fluent follows
(even sizes, a 4 px grid, radius pairs) and list which of our tokens break them, so the user can decide in
design-questions.

**Verdict key:** match = same number; near = within 1 step of the Fluent value or on the same grid; breaks = off the
pattern. Tokens live in `Theme/Typography.cs`, `Shape.cs`, `ChromeTokens.cs`, `Paint.cs`, `Theme/Palette.cs` and
`Motion/MotionTokens.cs`.

## Sources
- [Typography](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/typography) (Windows 11 type ramp)
- [Geometry](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/geometry) (corner radius)
- [Content layout and spacing](https://learn.microsoft.com/en-us/windows/apps/design/basics/content-basics) (spacing, gutters)
- [Layering and elevation](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/layering) (elevation, stroke)
- [Timing and easing](https://learn.microsoft.com/en-us/windows/apps/design/motion/timing-and-easing) (durations, curves)
- [Materials](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/materials), [Acrylic](https://learn.microsoft.com/en-us/windows/apps/design/style/acrylic), [Mica](https://learn.microsoft.com/en-us/windows/apps/design/style/mica), [In-app acrylic](https://learn.microsoft.com/en-us/windows/apps/develop/ui/in-app-acrylic)
- [Fluent 2 layout](https://fluent2.microsoft.design/layout) (spacing ramp), [Fluent 2 shapes](https://fluent2.microsoft.design/shapes) (radius, stroke), [Fluent 2 motion](https://fluent2.microsoft.design/motion)
- [WinUI theme resources](https://raw.githubusercontent.com/microsoft/microsoft-ui-xaml/main/controls/dev/CommonStyles/Common_themeresources_any.xaml) (layer and smoke fills, animation durations), [Button resources](https://raw.githubusercontent.com/microsoft/microsoft-ui-xaml/main/controls/dev/CommonStyles/Button_themeresources.xaml)

## 1. Type ramp
Fluent sizes are all **even** and sit in a fixed ramp (size/line height, effective pixels).
Source for the whole ramp: [Typography, type ramp](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/typography#type-ramp).

| Fluent | Ours | Token | Verdict |
|---|---|---|---|
| Caption 12/16 | 12 | `Typography.Small` | match |
| Caption 12/16 | 11, 11.5 | `Micro`, `Caption` | breaks (below the 12 floor; 11.5 is a half size) |
| Body 14/20 | 14 | `Large`, `IconSmall` | match |
| Body 14/20 | 13, 13.5, 12.5 | `Body`, `BodyEmphasis`, `Label` | breaks (our body is 13, not 14) |
| Body 14/20 | 15 | `Input` | breaks (odd; between Body 14 and Body Large 18) |
| Body Large 18/24 | 18 | `PanelTitle`, `IconLarge` | match |
| Body Large 18/24 | 17 | `DialogTitle` | near (odd, one under 18) |
| Subtitle 20/28 | 20 | `IconXLarge` | match (icon only; no text at 20) |
| Title 28/36 | 26 | `Display` | near (even, but not on the ramp) |
| Title Large 40/52, Display 68/92 | none | none | not used (not needed for a scratchpad) |
| Weights: Regular, Semibold only; "Bold ... not part of the Windows type ramp" ([source](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/typography#type-ramp)) | Regular, SemiBold, Bold | `Regular`, `Strong`, `Bold` | match / match / breaks (Bold) |
| Font: Segoe UI Variable ([source](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/typography)) | Segoe UI Variable Text, Segoe UI | `UiFamily` | match |
| Minimum: 12 px Regular, 14 px Semibold ([source](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/typography#typography-best-practices-in-windows-11)) | 11 and 11.5 Regular; 13 and 13.5 emphasis | `Micro`, `Caption`, `BodyEmphasis` | breaks (11, 11.5 under the Regular floor; 13.5 emphasis under the Semibold floor, if it is semibold) |
| Line height: ramp pairs size with line height (12/16, 14/20, 18/24) | not tokenized | none | not published as a rule beyond the pairs; ours is unset |
| Icon glyph sizes: not published on these pages | 13, 14, 15, 16, 18, 20 | `Icon*`, `ChromeTokens.IconGlyphSize` 16 | not comparable (13 and 15 are odd) |

Other type tokens in `ChromeTokens`: `BadgeFontSize` 11.5, `SavedLabelFontSize` 12, `RunningTimerFontSize` 11.5,
`PagerCountFontSize` 11.5. 11.5 appears in four tokens; it breaks both the floor and the even rule.

## 2. Spacing grid and control sizes
Fluent's base unit is **4 px**; "gutter widths should be a multiple of the base unit"
([Fluent 2 layout](https://fluent2.microsoft.design/layout)). The ramp is 0, 2, 4, 6, 8, 10, 12, 16, 20, 24, 28, 32, 36, 40, 48, 52, 56; 2, 6 and 10 exist only so Fluent icons align to the 4 px grid
([same page](https://fluent2.microsoft.design/layout)).

Windows gaps ([Content layout and spacing](https://learn.microsoft.com/en-us/windows/apps/design/basics/content-basics)): 8 between buttons, button and
flyout, control and header; 12 between control and label and between content areas; 16 between surface and edge text.
Touch target: 40x40 px (7.5 mm at 135 PPI), "typically" for WinUI controls
([Targeting](https://learn.microsoft.com/en-us/windows/apps/develop/input/guidelines-for-targeting)). Button padding
11,5,11,6 and a 1 px border ([Button resources](https://raw.githubusercontent.com/microsoft/microsoft-ui-xaml/main/controls/dev/CommonStyles/Button_themeresources.xaml)).
Control heights (for example a 32 px button) are **not published** on the pages read.

| Fluent | Ours | Token | Verdict |
|---|---|---|---|
| 40 touch target | 40 bar row | `DockedRowHeight` | match |
| 4 base unit | 4 | `BarEdgeInset`, `PillInset`, `BadgeSpacing`, `PillBottomInset` | match |
| 8 gap | 8 | `MetaSpacing`, `FloatGap`, `TopIconHoverRadius`, `PagerArrowHoverRadius` | match |
| 12 (ramp) | 12 | `TextEdgeFadeBand` | match |
| 16 (ramp) | 16 | `IconGlyphSize`, `PagerDotSelectedWidth`, `PagerDotButtonWidth` | match |
| 20, 24, 32, 36 (ramp) | 20, 24, 32 | `BadgeHeight`, `OverlayPagerRevealZone`, `PagerDotSelectedButtonWidth`, `ButtonSize` | match |
| 52 (ramp) | 52 | `TopBackdrop.ClearStop` | match |
| 6, 10 (ramp, icon padding) | 6, 10 | `BadgeRightInset`, `DividerPagerClearance`, `DividerTopMargin`, `FirstItemInset`, `BadgeCornerRadius` | near (on the ramp, but off the 4 px multiples) |
| Multiple of 4 | 3 | `IconSpacing`, `PagerRowSpacing` | breaks |
| Multiple of 4 | 5 | `PagerSeparatorMargin` | breaks |
| Multiple of 4 | 7 | `BadgeHorizontalPadding`, `PagerDotWidth`, `PagerDotHeight` | breaks (odd) |
| Multiple of 4 | 9 | `PagerDotHoverRadius` | breaks |
| Multiple of 4 | 14 | `PagerSeparatorHeight`, `PagerCountDigitWidth`, `PillShadowBlur` | breaks |
| Multiple of 4 | 18 | `PagerDotButtonHeight` | breaks |
| Multiple of 4 | 30 | `PagerCountBaseWidth` | breaks |
| Multiple of 4 | 40 / 52 stops, 10 blur | `TopBackdrop.DenseStop`, `BlurRadius` | match (40, 52); near (10) |
| Multiple of 4 | 1, 2 | `BadgeTopPadding`, `BadgeBottomPadding`, `ToastPagerClearance` | near (1 is under the smallest step, 2 is on the ramp) |
| derived | 42 (32 + 2 x 4 + 2 x 1) | `PillHeight` | breaks (the 1 px border makes it 42, not 40 or 44) |
| derived | 50 (42 + 2 x 4) | `PagerStripHeight` | breaks |

## 3. Corner radii
Windows 11 uses three levels, set by two global resources, `ControlCornerRadius` 4 and `OverlayCornerRadius` 8
([Geometry](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/geometry)):
- 8 px: top-level containers such as app windows, flyouts and dialogs.
- 4 px: in-page controls (Button, CheckBox, ComboBox, TextBox, ListView) and bar shapes (ProgressBar, ScrollBar, Slider). ToolTip is the one overlay at 4.
- 0 px: straight edges that touch other straight edges; maximized or snapped windows.

Fluent 2 adds a finer scale: None 0, Small 2 (small badges), Medium 4 (buttons, dropdowns), Large 8, X-Large 12
(button sheets, popovers), Circle 50%; "for shapes smaller than 32 pixels, the corner angle is reduced to 2 pixels"
([Fluent 2 shapes](https://fluent2.microsoft.design/shapes)). The two sources are not identical: Windows names dialogs
at 8, Fluent 2 names popovers at 12.

| Fluent | Ours | Token | Verdict |
|---|---|---|---|
| 4 controls | 7 | `Shape.Control` | breaks |
| 4 controls | 5 | `Shape.Key` | breaks (also off the even pattern) |
| 8 flyouts, menus | 8 | `Shape.Popup`, hover radii | match |
| 8 dialogs (Windows) / 12 popovers (Fluent 2) | 12 | `Shape.Dialog`, `PillCornerRadius` | near (matches Fluent 2, not Windows) |
| 8 containers, nothing at 10 | 10 | `Shape.Card` | breaks (no 10 step exists) |
| 2 small badges (shapes under 32 px) | 6 | `BadgeCornerRadius` (20 px high badge) | breaks |
| 0 or 2 (smallest step) | 1 | `Shape.Hairline` | breaks |
| Circle 50% | size / 2 | `Shape.Round`, `PagerDotRadius` | match |

Our radii are 1, 5, 6, 7, 8, 10, 12: seven values for Fluent's four (0, 2, 4, 8) plus 12.

## 4. Borders and strokes
| Fluent | Ours | Token | Verdict |
|---|---|---|---|
| Stroke thin 1, thick 2, thicker 3, thickest 4 ([Fluent 2 shapes](https://fluent2.microsoft.design/shapes)) | 1 | `PillBorderThickness`, `DividerWidth`, `PagerSeparatorWidth` | match |
| Stroke 1 on every elevation level ([Layering](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/layering)) | 1 | same | match |
| Button border 1 ([Button resources](https://raw.githubusercontent.com/microsoft/microsoft-ui-xaml/main/controls/dev/CommonStyles/Button_themeresources.xaml)) | 1.2 | `PagerDotBorderThickness` | breaks (no fractional step) |
| Focused text box bottom border 2 (Default/Light), 2 all sides (High Contrast) ([Common resources](https://raw.githubusercontent.com/microsoft/microsoft-ui-xaml/main/controls/dev/CommonStyles/Common_themeresources.xaml)) | not tokenized as a thickness | `Paint.FocusBorder` is an alpha (.9) | not comparable |

## 5. Elevation and shadow
Windows 11 elevation values, each with a 1 px stroke
([Layering and elevation](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/layering)):
Window 128, Dialog 128, Flyout 32, Tooltip 16, Card 8, Control 2 (rest and hover; pressed 1), Layer 1. The page says
"the higher the elevation, the larger and softer the shadow", but gives **no blur, offset or alpha** per level
(**not published**), so a number-for-number match cannot be made.

| Fluent | Ours | Token | Verdict |
|---|---|---|---|
| Flyout 32 (blur, offset, alpha not published) | blur 18 / 16, offset 4, alpha 110 dark / 40 light | `MenuShadowBlur`, `DropdownShadowBlur`, `PopupShadowOffsetY`, `PopupShadowDarkAlpha` | not comparable |
| Card 8, Control 2 (not published) | pill: blur 14, offset 3, alpha 90 / 28 | `PillShadowBlur`, `PillShadowOffsetY`, `PillShadow*Alpha` | not comparable |
| Levels are a ramp 1, 2, 8, 16, 32, 128 | two levels (menu, pill), values differ slightly (18/16/14, 4/3) | the five shadow tokens above | breaks the idea (one elevation scale; ours are one-off numbers) |

Shadow alpha differs by theme in Fluent too ("the intensity of the rendered shadow changes depending on the theme at
parity of value" ([same page](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/layering))), so our dark/light pair is in the spirit.

## 6. Motion
WinUI standard durations: Normal 250 ms, Fast 167 ms, Faster 83 ms
([Timing and easing](https://learn.microsoft.com/en-us/windows/apps/design/motion/timing-and-easing); same values in
[Common resources](https://raw.githubusercontent.com/microsoft/microsoft-ui-xaml/main/controls/dev/CommonStyles/Common_themeresources_any.xaml)).
Button color changes use 83 ms ([Button resources](https://raw.githubusercontent.com/microsoft/microsoft-ui-xaml/main/controls/dev/CommonStyles/Button_themeresources.xaml)).
Easing: entering UI uses `cubic-bezier(0, 0, 0, 1)` (Fast Out, Slow In); exiting UI uses `cubic-bezier(1, 0, 1, 1)`
(Slow Out, Fast In) ([Timing and easing](https://learn.microsoft.com/en-us/windows/apps/design/motion/timing-and-easing)).
Fluent 2 gives no millisecond tokens; it only says to "use quick, natural durations" and to give larger elements more
time ([Fluent 2 motion](https://fluent2.microsoft.design/motion)). Neither source gives a "multiples of" rule; the
three durations are 83, 167 and 250, about 1/12, 1/6 and 1/4 of a second (our arithmetic, not stated by Microsoft).

| Fluent | Ours | Token | Verdict |
|---|---|---|---|
| Faster 83 | 80 | `CaretGlide` | near |
| Fast 167 | 170, 160, 150, 140, 130, 120 | `BarsReveal`, `ToastOut`/`WindowFade`, `WindowSlide`/`PillFade`/`SettingsPage`, `ContentReveal`, `OverlayIn`, `ToastIn` | near (170) / breaks (the other five scatter 120 to 160) |
| Fast 167 (or Normal 250) | 180, 190, 200 | `ChromeShift`, `Nudge`, `NoteArrive`, `SwipeSpringBack` | breaks (between Fast and Normal) |
| Normal 250 | none | none | not used |
| Enter curve 0,0,0,1; exit 1,0,1,1 | durations only here; curves are outside `MotionTokens` | none | not checked in this study (curves are not in the token file) |

Our 12 durations span 80 to 200 ms in 10 ms steps; Fluent has three buckets.

## 7. Materials and tint ranges
Windows 11 uses Mica (opaque, tinted by the wallpaper; base layer), Acrylic (translucent frosted glass; "only for
transient, light-dismiss surfaces such as flyouts and context menus") and Smoke (always translucent black behind modals)
([Materials](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/materials)).
Mica "falls back to a solid color" when transparency is off, the window deactivates, or on low-end hardware
([Mica](https://learn.microsoft.com/en-us/windows/apps/design/style/mica)). Acrylic guidance: "Don't put desktop acrylic
on large background surfaces", avoid stacking acrylic panes, "keep the impact on legibility in mind" when changing tint
or opacity ([Acrylic](https://learn.microsoft.com/en-us/windows/apps/design/style/acrylic)).

Published layer alphas (WinUI source, [Common resources](https://raw.githubusercontent.com/microsoft/microsoft-ui-xaml/main/controls/dev/CommonStyles/Common_themeresources_any.xaml); percent is our conversion of the hex alpha byte):

| Fluent resource | Dark | Light |
|---|---|---|
| `LayerFillColorDefault` | #4C3A3A3A (30%) | #80FFFFFF (50%) |
| `LayerOnMicaBaseAltFillColorDefault` | #733A3A3A (45%) | #B3FFFFFF (70%) |
| `CardBackgroundFillColorDefault` | #0DFFFFFF (5%) | #B3FFFFFF (70%) |
| `LayerOnAcrylicFillColorDefault` | #09FFFFFF (3.5%) | #40FFFFFF (25%) |
| `SmokeFillColorDefault` | #4D000000 (30%) | #4D000000 (30%) |

| Fluent | Ours | Token | Verdict |
|---|---|---|---|
| Acrylic `TintOpacity` / `TintLuminosityOpacity` recommended range: **not published**. The one example on the page is 0.8 / 0.5, with no claim it is a recommendation ([In-app acrylic](https://learn.microsoft.com/en-us/windows/apps/develop/ui/in-app-acrylic)) | window tint 0.55, 0.7 (default), 0.82, 0.92; code clamps to 0.3 to 1 | `Settings.Tint` (`Model/Settings.cs`), presets in `UI/SettingsWindow.cs`, clamp in `UI/PadWindow.cs` | not comparable |
| Smoke 30% | scrim .55, dimmed .55 | `Paint.GlassScrim`, `Paint.Dimmed` | breaks (denser than Smoke) |
| Layer over Mica 30% dark, 50% light | card surface .92 dark / .90 light; toast .92 | `Paint.CardSurface`, `Paint.Toast` | breaks (far denser than any published layer) |
| Card 5% dark, 70% light | `BadgeFill` .14 dark / .10 light; `RowWash` .03 / .025 | `Paint.BadgeFill`, `Paint.RowWash` | near for the washes (small alphas); a card on a light theme is not compared |

**The user's concern: the tint range is narrow (about 0.55 to 0.92).**
- The four presets (Clear 0.55, Light 0.7, Medium 0.82, Dense 0.92) span 0.37. Code already allows 0.3 to 1, so the narrowness is the **menu**, not the clamp.
- Fluent publishes no recommended tint range for Acrylic, so there is no Microsoft number to widen to. What it does publish is that its own layers are lighter than our floor: 30% dark and 25% to 50% light for the layers above Mica or Acrylic, 30% for Smoke.
- Mica is opaque by definition, so "Dense" is the Mica end of the scale and "Clear" is the Acrylic end. Fluent also keeps the two apart by use (Mica for the window base, Acrylic for transient surfaces); we put one tint slider across both.
- Fluent's legibility warning applies to the low end: at low tint text sits on whatever is behind the window. High Contrast already falls back to solid in both Windows and our palettes.
- Gap to name for the user: we sample 4 stops; nothing below 0.55 is offered, though the clamp allows 0.3. Whether lower is wanted is a feel decision, not a Fluent one.

## Patterns Fluent follows, and which of ours break each
1. **A 4 px grid for sizes and gaps** (multiples of 4; 2, 6, 10 only as icon padding).
   - Break: 3, 5, 7, 9, 14, 18, 30 and the derived 42 and 50 (see section 2).
2. **Even type sizes on a fixed ramp** (12, 14, 18, 20, 28, 40, 68).
   - Break: 11.5 (four tokens), 12.5, 13, 13.5, 15, 17, 11; 7 of our 11 text sizes.
   - Break: 26 is even but off the ramp (Title is 28).
3. **Two or three radii only: 4 for controls, 8 for overlays** (plus 0, and 2 or 12 in Fluent 2).
   - Break: 5, 6, 7, 10, 1; seven radius values where Fluent uses four or five.
4. **Floors on text size** (12 Regular, 14 Semibold).
   - Break: 11 and 11.5 text; possibly 13.5 emphasis.
5. **No Bold; Semibold is the emphasis weight.**
   - Break: `Typography.Bold`.
6. **Whole-pixel strokes, 1 or 2.**
   - Break: 1.2 pager dot border.
7. **A small set of named durations** (83, 167, 250) with directional easing.
   - Break: 12 durations from 80 to 200; ours are not bucketed.
8. **One elevation scale** (1, 2, 8, 16, 32, 128) tied to shadow softness.
   - Break: our shadows are separate one-off blur and offset numbers.
9. **Material by role** (Mica base, Acrylic transient, Smoke behind modals, layer fills at low alpha).
   - Break (partly): one tint slider for the whole window; surface and scrim alphas (.55 to .92) denser than the published layers.

## Candidates for design-questions
Proposals only. Nothing is applied; each needs the user's yes and a lab view first.
1. **Snap the odd spacing tokens to the 4 px grid** (3 to 4, 5 to 4 or 6, 7 to 8, 14 to 16, 18 to 20, 30 to 32). Visual cost: the pager shifts by 1 to 2 px.
2. **Collapse radii to Fluent's pairs**: `Control` 7 to 4, `Key` 5 to 4, `Card` 10 to 8, `Badge` 6 to 2 or 4, `Dialog` 12 stay (Fluent 2) or go to 8 (Windows). Biggest visible change in the whole study.
3. **Round the type sizes** to 12 / 14 / 18, with 13 to 14 for Body and 15 to 14 or 16, 17 to 18, 11 and 11.5 to 12. Needs a width check on the pad and the pager count.
4. **Drop `Bold`**; use `Strong` (Semibold) only, or keep Bold for the editor body (decision for the user, since Fluent's rule is for UI chrome).
5. **Make the pager dot border 1 px.**
6. **Bucket the motion**: three or four named steps (for example 80, 160, 200) instead of 12 values; the user chooses the numbers; Fluent's own are 83 / 167 / 250.
7. **Widen the tint menu**: add one lower stop (0.4 or 0.45) and perhaps a numeric slider 0.3 to 1.0, keeping the four names. Fluent gives no number to copy, so the value is the user's taste.
8. **Name an elevation scale**: two shadow levels (pill, menu) with one rule for blur and offset, instead of five loose tokens.
9. **Reconsider the scrim and card surface alphas** (.55 to .92) against the lighter published layers (25% to 50%), only if the user wants a less dense feel.

## Not covered
- Control heights (32 px buttons), list item heights, text box heights: **not published** on the pages read.
- Fluent 2 duration tokens: none given on the motion page.
- Exact `AcrylicBrush` default tint and luminosity values: not published on the pages read.
- Easing curves in our code: not in `MotionTokens`; outside this study.
- Hover, press and selection alphas (`Paint.Hover`, `Press`, `Selected`): no Fluent number was found, so no comparison is made.
