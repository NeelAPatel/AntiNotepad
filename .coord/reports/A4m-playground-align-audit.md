# A4-design-tokens: alignment audit (read-only)

Worktree: `.claude/worktrees/feat/A4-design-tokens`. Short paths below are relative to it. No product or doc file was edited.
Sources: `docs/design-scale.md` (DS), `docs/playground/tokens.json` (TJ), `docs/playground/measure.md` (MM),
`tools/playground/index.html` (PG), product `product/AntiNotepad/...` (PS).

The product "today" values were checked one by one: every numeric `today` in TJ roles matches `Theme/ChromeTokens.cs`,
`Typography.cs`, `Shape.cs`, `Paint.cs` and `Motion/MotionTokens.cs`. **No role has a wrong numeric today value.** The
problems are in the scale-entry `today` arrays, in the formatting of a few role strings, in the mappings that only cover R,
and in literals hard-coded in the page.

## Findings

| # | Area | Where (file:line) | What is wrong | Correct value + evidence | Severity |
| --- | --- | --- | --- | --- | --- |
| 1 | D | PG:914 (`setPreset`), TJ `replaces` lists | **Alt preset can't reproduce DS Alt.** Each role is bound to one scale entry, and that binding is the R mapping. A null Alt step sends roles back to today. | Under Alt: (a) `Corner.Sm`=4 drives Control and Badge radii, but DS wants 6 (Mid) (DS:39,45). (b) `Space.S`=6 also shrinks the 8s and 7s (MetaSpacing, FloatGap, dots 7 to 6), but DS keeps 8 as S2 (DS:139-140,147). (c) `Motion.Fast` null leaves ToastIn and OverlayIn at 120/130, but DS folds them into Base 160 (DS:111). (d) `Strength.Strong` null leaves .78/.8, but DS folds it into Full .9 (DS:132). (e) `Stroke.Mid` 1.5 drives nothing, so focus goes to 2 where DS Alt keeps the half step (DS:87,90). Fix: add a per-role Alt step (e.g. `stepsAlt`) and have `setPreset` read it. | **blocks review** (Alt comparison is wrong) |
| 2 | D | PG:395 `dotBtnH = 18` | PagerDotButtonHeight is a literal. The R preset can't show 18 to 20, one of the 22 locked-row changes the user must judge (DS:245,331). | Should follow the role: 18 today, 20 R. TJ role `ChromeTokens.PagerDotButtonHeight` is kind `size` with `steps: []`, so it needs a token or a per-preset size. | **blocks review** (lock question) |
| 3 | D | PG:579 `fsA = T("Text.S", 12.5)` | Menu action labels (Cut, Copy, Paste, Delete) are drawn as Label 12.5 / Text.S. | Typography.Large 14, which is Text.M (PS `UI/PadMenu.cs:115`). Today is 1.5 too small, and R shows 12 where it should show 14. | cosmetic (visible mis-render) |
| 4 | D | PG:576 `rRow = T("Corner.Sm", 6)`, `data-r` "Shape.Control" (PG:581) | Menu row and action hover radius: the today value is 6 and the role is Control 7. | Literal 4 (PS `UI/PadMenu.cs:120,159` `MenuButton(..., 52, 4)` / `(..., 32, 4)`). R 4 coincides, but Today is wrong. | cosmetic |
| 5 | D | PG:566 `y += hgt + 3` | Gap between search result cards is 3. | `StackPanel Spacing = 2` (PS `UI/SearchOverlay.cs:87`). MM:183's own 349 to 354 at 2x is 2.5. | cosmetic |
| 6 | D | PG:558 `aSurf` for all cards | The current result card uses the .55 surface. | The current or picked card is opaque, alpha 1 (PS `UI/SearchOverlay.cs:251`). | cosmetic |
| 7 | D | PG:564 10 px spacer | Search meta spacing is 10. | `Spacing = 8` (PS `UI/SearchOverlay.cs:195`). | cosmetic |
| 8 | D | PG:490 `rgba(var(--mutec),.7)` | The empty-dot ring is at alpha .7. | `p.Faint` at full alpha (PS `UI/PadWindow.Layout.cs:289`). | cosmetic |
| 9 | D | PG:87 `.toast>div{padding:7px 12px}` | Toast padding is CSS. No preset moves it. | Today 12 x 7 (PS `UI/PadWindow.Layout.cs:34`). DS section 4 R is 12 x 8 (DS:281). | cosmetic |
| 10 | D | PG:64 `.box` 14 px, 1.9 pen, radius 4 | The checkbox is drawn with fixed literals. | DS exception 4: size = font x .86, about 12.9 at 15 px; radius x .28, about 3.6; pens 1.6 / 1.9. MM:134 measured "about 13 wide". | cosmetic |
| 11 | D | PG:442 badge glyph span | The badge branch glyph inherits weight 600 and uses `margin-right:4px`. | The product run is `Typography.Regular`, and the gap is the space character in `value[1..]` (PS `UI/PadWindow.Layout.cs:241-246`). | cosmetic |
| 12 | D | PG:400 `sFocus = T("Stroke.Thick", 2)` | Today focus ring is 2. | Code today is 1.5 (PS `UI/SearchOverlay.cs:253`, current ring). Pixels show 2 (MM:177,182,194). The page follows the pixels. Note it; no change needed unless Today must equal code. | cosmetic |
| 13 | A | TJ:1061-1066 `Space.2XL` today `[28, 32]` | The only role is PagerCountBaseWidth 30, and 30 is missing. The card sample, slider thumb and "today" ticks sit at 28, while the canvas draws 30 (PG:394). | `[30]` (PS `Theme/ChromeTokens.cs:38`; DS:235). 28 is a free margin (DS:148-149). 32 is ButtonSize, which is a size and not in replaces (DS:213). | cosmetic |
| 14 | A | TJ:859 `Strength.Mid` today includes .4; TJ:836 `Strength.Faint` lacks .4 | .4 is in the wrong family. | MenuSeparator .4 maps to Faint .3 (DS:201; PS `Theme/Paint.cs:33`). No Mid role is .4. TopBackdrop tint .4 is an exception. Faint should be `[.25,.3,.35,.4]` and Mid `[.45,.5,.55,.6]`. | cosmetic |
| 15 | A | TJ:341 `Icon.S` today `[13,14,15]` | 15 belongs to IconInput, which is Icon.M (DS:169). | `[13, 14]` (PS `Theme/Typography.cs:24`). | cosmetic |
| 16 | A | TJ:105 `Corner.Md` today `[8,10]` | PagerDotHoverRadius 9 is in replaces but missing from today. | `[8, 9, 10]` (PS `Theme/ChromeTokens.cs:35`; DS:232). | cosmetic |
| 17 | A | TJ:414-416 `Stroke.Thin` replaces | PagerDotBorderThickness is listed twice, and "focus ring" is listed under Thin. | The focus ring goes to Thick 2. Only the unselected tile ring and the off-toggle go to Thin (DS:88,319). Remove "focus ring" and the duplicate. | cosmetic |
| 18 | A | TJ:400 `Stroke.Thin` today `[1,1.2]`; TJ:442 `Stroke.Thick` `[2,2.5]` | 1.5 is missing from both, though both absorb 1.5 roles. | Thin today: 1, 1.2, 1.5 (tile ring, off-toggle). Thick today: 1.5, 2, 2.5 (focus, search current 1.5 at PS `UI/SearchOverlay.cs:253`). | cosmetic |
| 19 | A | TJ:720,742 `Wash.Hover.*`; TJ:764,783 `Wash.Press.*` | Today lists only the Paint.Hover or Press value. | Hover dark: .06, .085, .09, .10. Hover light: .045, .06, .075, .10 (HelpWash, PreviewFrame/Well, CompletionFill, PreviewBar; PS `Theme/Paint.cs:12-16,43`). Press dark: .14, .15, .18. Press light: .10, .11, .13 (BadgeFill `Paint.cs:17`, `ChromeTokens.cs:22-23`). | cosmetic |
| 20 | A | TJ:703 `Wash.Faint.Light` today `[0.03]` | Wrong today value. | .025 (PS `Theme/Paint.cs:15`; DS:187 "0 / +.005"). | cosmetic |
| 21 | B | TJ:1654 (and the Press and Selected rows after it) | `Paint.Hover`, `Paint.Press` and `Paint.Selected` each have `today` set to the whole combined row ".09/.06, .15/.11, .22/.18". This string flows into saved picks through `ROLE_TODAY` (PG:681,1095). | Hover ".09/.06", Press ".15/.11", Selected ".22/.18" (PS `Theme/Paint.cs:8-10`). | cosmetic (picks carry a wrong today) |
| 22 | B | TJ:2728, 2738 | `PillHeight` and `PagerStripHeight` both have today/new "42, 50". | PillHeight 42 and PagerStripHeight 50, each on its own row (PS `Theme/ChromeTokens.cs:59-60`). | cosmetic |
| 23 | B | TJ Full roles (ResultCurrent ... GridPen, around 1840-1920) | All six show change "0 (HoverFace +.04)". | HoverFace "+.04" (.86 to .9), the others "0" (DS:197). | cosmetic |
| 24 | B | TJ `Paint.SampleFadeAlpha` | Today is "40 (byte)" and its siblings are plain numbers. | "40". The DS row suffix applies to all four. | cosmetic |
| 25 | B | TJ `ChromeTokens.ButtonSize` | New is "32 (size, 2XL)", but steps is `[]`. Space.2XL never moves buttons. | Consistent with DS ("size", DS:213). The "2XL" label only hints at it. No change needed; noted so nobody expects the 2XL slider to move buttons. | cosmetic |
| 26 | C | TJ (no entry), PG (not drawn) | Exceptions 4 (checkbox), 5 (DrawChip), 7 (toast and dialog action pad 8 x 2) and 9 (accent dot ring is Ink) have no TJ record. 5, 7 and 9 are not on the canvas. 4 is drawn with literals (#10). | DS:298-303. Add an `exceptions` list to TJ (13 rows, each with its literal) so the page can show them read-only. | cosmetic |
| 27 | C | PG (no backdrop element) | Exception 3, the TopBackdrop gradient (stops 40/52, blur 10, tint .4, mask 1/.5/0), is in TJ roles but not drawn. | PS `Editor/PadView.cs:350-378`, `ChromeTokens.cs:73-97`. | cosmetic |
| 28 | C | PG:50 `.badge{align-items:center;height:20px}` | Exception 1 (badge padding 1 top / 2 bottom, optical) is not applied. The page centers the text instead. | PS `UI/PadWindow.Layout.cs:215`; DS:295. | cosmetic |
| 29 | E | MM:100-101 (pill y 590) and MM:129 ("pill top 593") | The pill y is wrong. | **597** client: 643 − PillBottomInset 4 − PillHeight 42 (PS `ChromeTokens.cs:57-59`). The strip top is 593 (MM:103), so pill = 593 + 4. Window rows 598..639 are client 597..638 (PG:607,463). MM:129 should read "strip top 593". | cosmetic (doc) |
| 30 | E | MM:107,108,110 (prev 111, count 146, next 193); MM:111-113 (separator x 157, dots 170/189/208, small grab) | These x values are **window** coordinates (+8), though MM:27 says coordinates are client. | Client: prev **103.5** (pill 98.5 + border 1 + inset 4), count **138.5**, next **185.5**, separator **225.5**, first dot **234.5** (stack view, today tokens; same formula as PG:469-491). Subtract 8 from the small-grab x values. | cosmetic (doc) |
| 31 | E | MM:18 ("x = 8, y = 0"), MM:71 ("window y = client y + 4") | The grab-origin offsets contradict each other and the page. | Window = client + (8, 1). Pill rows 598..639 against client 597..638, and PG:607,610 use `dx -8, dy -1`. MM:80's hover y 5..36 for buttons at client 4.5..36.5 also gives about +0.5 to 1, not +4. | cosmetic (doc) |
| 32 | E | MM:80 | The "Search button" hover wash x 443..474 (window) is the settings button. | 443 − 8 = 435, the settings x (MM:82). The search hover would be at window 338..369. | cosmetic (doc) |
| 33 | E | MM:64 | The card border is placed at window x 7 and 514. That conflicts with the client origin x = 8 and client width 506. | The border should be at window x 8 and 513. Either the frame is 7 or the run read is off by 1. | cosmetic (doc) |
| 34 | E | MM:101 | Slot-view pill: "w 317" and "= 316" on the same row. | Derived 309 + 8 = 317. The 2x run 189..821 is 316. State which one is used. | cosmetic (doc) |
| 35 | E | MM:119 | The pager sum lists four explicit "+ 3" and then "12 spacings of 3", which double-counts. | 275 + 8 dot gaps x 3 = 299 (12 spacings in total). | cosmetic (doc) |
| 36 | E | TJ:315-330 `Weight.Bold` recommended null | **Refuted as a slip.** The R scale has no chrome Bold. Editor Bold is exception 10. | DS:66-70,168,304. With R, `setPreset` leaves Bold null, so the editor keeps 700 (PG:397,423). This is correct. | none |
| 37 | E | PG:347 `BADGE_BRANCH = "⎇"` (U+2387) | **Refuted for this machine.** The product draws U+E0A0 only if a font named exactly "Cascadia Mono" or "Cascadia Code" contains it, else U+2387 (PS `UI/PadWindow.Layout.cs:229-242`). The installed `C:\Windows\Fonts\CascadiaMono.ttf` and `CascadiaCode.ttf` have no U+E0A0 (cmap check). The user font folder has no Cascadia. | U+2387 matches the dev copy here. The remaining gaps (weight and spacing) are #11. | none |

## D. Preview-blind tokens (eye button only flashes the sample)

Found by script: TJ scale names minus every `T("...")` read in PG.

- **Never read in any view:** Corner.Mid, Corner.Round (dots use `dotH/2`, PG:489), Text.XL, Text.2XL, Icon.S, Icon.L,
  Stroke.Mid, Motion.Quick, Motion.Slow, Wash.Faint.Dark/Light, Wash.Selected.Dark/Light, Space.S2, Space.XL (the stack
  view has no selected dot).
- **Read only in one view:** Text.M, Text.L and Strength.Full (search, menu, toast); Shadow.High.* and Strength.Strong (menu);
  Space.Nudge (toast).
- **Off-theme halves:** each `*.Light` token is blind in dark and each `*.Dark` token is blind in light.
- **Roles that never draw** even though their token is "on canvas": DividerWidth, DividerTopMargin, DividerPagerClearance and
  DividerAlpha (the split-view divider, PS `UI/PadWindow.Layout.cs:64-65,179`, is not drawn); RunningTimerFontSize;
  GlyphIconBoxExtra; PagerDotSelectedButtonWidth; MetaSpacing and BadgeSpacing (no slot or mode badges in the stack view);
  TopBackdrop.*; Paint IconInkAlpha / PinnedGlyphAlpha (icons are drawn at full ink).
- **The tag is misleading:** `used` is filled during the draw before the cuts (PG:497-501). Pager tokens therefore count as
  "on canvas" in search view and in hidden overlay or float. Motion.Fast and Base are read in every view (PG:512-513), even
  though they only act in the toast view.

## C. The 13 exceptions at a glance

| # | In TJ | On canvas |
| --- | --- | --- |
| 1 Badge pad 1/2 | yes (roles, kind exception) | no (centered, #28) |
| 2 PagerCountDigitWidth 14 | yes | yes (PG:395 literal) |
| 3 TopBackdrop | yes | no (#27) |
| 4 Checkbox | no | literals, off by about 1 px (#10) |
| 5 DrawChip | no | no |
| 6 Nudge 2 | yes (Space.Nudge) | toast only |
| 7 Action pad 8 x 2 | no | no |
| 8 Search query radius Md 8 | implicit (Corner.Md) | yes (PG:527) |
| 9 Accent dot ring Ink | no | no |
| 10 Editor Bold | yes (Typography.Bold, Weight.Bold) | yes (PG:423) |
| 11 Material picture / pill 42 / strip 50 | derived rows | yes (computed, PG:461-463) |
| 12 Byte alphas | yes (4 roles) | no |
| 13 Hover radius Md 8 | implicit (Corner.Md) | yes |
