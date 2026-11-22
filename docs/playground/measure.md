# Playground measurements: the dev copy to the pixel (A4k-playground-measure)

Input for the token playground page (A4l-playground-page). Nothing here changes a product file, token or test.
Every box has two sources: **px** (measured on a capture in `shots/`) and **code** (the token or XAML value, or
"derived: a + b"). A disagreement is listed in section 9 and not fixed.

## 1. Frame

| Item | Value | Source |
| --- | --- | --- |
| Display | 1920 x 1080, 100% (96 dpi), one monitor | px: `GetSystemMetrics`, `GetDpiForWindow` = 96 |
| App `Scale` setting | 100% | px: Settings > UI > Scale (`dropdown-tint-popup-light-default.png`) |
| DIP to pixel | 1 DIP = 1 px in window grabs; 2 px in the `-2x` captures | px: display scale 100% |
| Build | dev copy, Debug, `AntiNotepad (dev)`, dev badge shown, test data `build/testdata` | code: `docs/dev-loop.md` |
| Default window | native rect 522 x 652 (client 506 x 643, 8 px frame left and right, 1 px bottom shadow) | px: `GetWindowRect` + F12 render 1012 x 1286 |
| Small window | native rect 370 x 499 (client 354 x 490) | px: `GetWindowRect` after a corner drag |
| Client origin in a window grab | x = 8, y = 1 | px: pill and text edges agree between grabs and F12 |
| Backdrop | Acrylic (default), Mica and Solid captured; tint Light; theme System (dark OS) then forced Light, then Dark | px: Settings > UI |

How the numbers were taken: grabs are exact-pixel screen copies of the dev window (`shots/<view>-<state>-<theme>-<size>.png`,
window and popup pixels, layered children included). The `-2x` files are the dev capture key (F12, `ANTINOTEPAD_SHOTS`):
a 2x render of the window content without the OS backdrop, so edges are clean and half pixels (11.5, 1.2, 3.5) resolve.
Limits of the F12 render: the pager and the toast text come out empty, and popups (menu, dropdown) are not included, so
those use grabs. Run lengths were read with row and column scans; bounding-box clusters over an Acrylic backdrop were
discarded where the backdrop bled into them.

Coordinates below are **client DIPs** (x right, y down, origin top-left of the 506 x 643 content) unless marked "window".

## 2. Capture index (49 files)

Sizes: `default` 522 x 652 window, `small` 370 x 499, `-2x` = F12 render. Theme `dark` is the Ink palette.

| View | State | Theme | Backdrop | Size | File |
| --- | --- | --- | --- | --- | --- |
| Pad, saved slot 5 | no bar (fade while typing) | dark | Acrylic | default | `pad-slot5-nobar-dark-default.png` |
| Pad, saved slot 5 (welcome, 4 line groups) | bar + pager always visible | dark | Acrylic | default | `pad-slot5-bar-dark-default.png` |
| Saved slot 1 (markdown), 2 (math), 3 (checklist) | docked | dark | Acrylic | default | `pad-slot1-markdown-…`, `pad-slot2-math-…`, `pad-slot3-checklist-dark-default.png` |
| Saved slot 4 (timer note) | idle | dark | Acrylic | default | `pad-slot4-timers-dark-default.png` |
| Saved slot 4 | one timer done | dark | Solid | default, 2x | `pad-slot4-timerdone-solid-dark-default-2x.png` |
| Saved slot 4 | one timer running (`● 0:58` chip) | dark | Solid | small | `pad-slot4-timer-running-dark-small.png` |
| Saved slots 6 (empty, then plain text), 7 (list) | docked | dark | Acrylic | default | `pad-slot6-empty-…`, `pad-slot6-plain-…`, `pad-slot7-list-dark-default.png` |
| Stack page 1/2 (showcase), 2/2 (math), 1/2 (empty first run) | docked | dark | Acrylic | default | `pad-stack1-current-…`, `pad-stack2-math-…`, `pad-stack1-empty-dark-default.png` |
| Stack page | docked | dark | Solid | default, 2x, small | `pad-stack-docked-solid-dark-default(-2x).png`, `pad-stack1-docked-solid-dark-small.png` |
| Stack page | docked, Mica | dark | Mica | default | `pad-stack1-docked-mica-dark-default.png` |
| Stack page | docked, Solid | dark | Solid | default | `pad-stack1-docked-solid-dark-default.png` |
| Stack page | docked | light | Acrylic | default | `pad-stack1-docked-light-default.png` |
| Bar icon hover | search, settings | dark | Acrylic | default | `pad-stack1-hover-search-…`, `pad-stack1-hover-settings-dark-default.png` |
| Pager hover | prev, count, dot (Acrylic); next (Solid) | dark | both | default | `pad-stack1-hover-pagerprev-…`, `…-pagercount-…`, `…-pagerdot-…`, `pad-stack-hover-pagernext-solid-dark-default.png` |
| Overlay mode | bar hidden, bar shown (hover top), pager reveal attempt | light, dark | Acrylic | default | `pad-stack1-overlay-light-…`, `pad-stack1-overlay-bar-light-…`, `pad-stack1-overlay-pager-light-…`, `pad-stack1-overlay-dark-…`, `pad-stack1-overlay-bar-dark-default.png` |
| Float below mode | bar hidden, bar shown, pager reveal attempt | light, dark | Acrylic | default | `pad-stack1-float-light-…`, `pad-stack1-float-bar-light-…`, `pad-stack1-float-pager-light-…`, `pad-stack1-float-bar-dark-default.png` |
| Right-click menu | open, row hover | dark | Acrylic, Solid | default (popup pad 150) | `menu-contextmenu-dark-default.png`, `menu-contextmenu-hover-row-dark-default.png`, `menu-contextmenu-hover-row-solid-dark-default.png` |
| Search | results, current result 1 and 2, field focus ring | dark | Acrylic, Solid | default, 2x | `search-results-dark-default.png`, `search-current2-dark-default.png`, `search-results-solid-dark-default(-2x).png` |
| Toast | "Copied the whole note" | dark | Acrylic, Solid | default, 2x | `toast-copied-dark-default.png`, `toast-copied-solid-dark-default(-2x).png` |
| Dropdown popup | Tint list (Settings) | light | Acrylic | Settings window | `dropdown-tint-popup-light-default.png` |
| Focus ring | keyboard focus on a search tool button (user capture, 100%) | dark | Solid | crop 537 x 181 | `search-focus-toolbutton-dark-default.png` |
| Focus | Tab from the editor | dark | Solid | default, 2x | `pad-stack-focus-bar-solid-dark-default(-2x).png` (Tab only indents the editor; see 9) |

Pages covered: saved slots 1 to 7 (4 demo notes, one empty, one plain, one list) and stack pages 1/2 and 2/2.

## 3. Window and card

| Box | x | y | w | h | px source | code source |
| --- | --- | --- | --- | --- | --- | --- |
| Card (content) | 0 | 0 | 506 | 643 | F12 render 1012 x 1286 at 2x; border at window x 8 and 513 | window width 522 - 2 x 8 frame |
| Card border | | | 1 | | px: expected at 8 and 513; the scan is off by 1 in the row scan | `BorderThickness` 1 (palette border; value not tokenized) |
| Card shadow / frame | | | 8 left and right, 1 bottom | | px: window rect minus card | window chrome (outside the redraw) |
| Content right edge in small | | | 354 | 490 | px: window 370 x 499 | same rule |

## 4. Top bar (docked)

Reference: Acrylic grab `pad-stack1-current-dark-default.png` (window y = client y + 1 there) and the Solid 2x render.
Row height 40, items centered; icon glyph ink is 16 x 16 (search ink 14 x 14, minimize 16 x 2).

| Box | x | y (Solid) | w | h | px source | code source |
| --- | --- | --- | --- | --- | --- | --- |
| Slot badge `slot 4` | 10 | 6 | 42.5 | 20 | F12 2x: 20..105 and 12..52 | `FirstItemInset` 10 (left), `BadgeHeight` 20; derived: text + 2 x `BadgeHorizontalPadding` 7 |
| Mode badge `timer` | 61 | 6 | 42 | 20 | F12 2x: 122..206 | same; gap to previous = `BadgeSpacing` 4 + width |
| Saved text | 111 | center 16.5 | | | F12 2x: text starts at 222 | `FirstItemInset` + badges + `MetaSpacing` 8; font `SavedLabelFontSize` 12 |
| Dev badge (branch + time) | 176 | 6 | 148 | 20 | F12 2x: 352..648 | `BadgeHeight` 20; right edge 324 |
| Search button (hit box) | 330 | 0.5 | 32 | 32 | px: hover wash x 338..369, y 5..36 in the Acrylic grab = 32 x 32 | `ButtonSize` 32 |
| Pager-style button | 365 | | 32 | 32 | icon centres 346, 381, 416, 451, 486 (F12 2x: 692, 762, 832, 902, 972) | pitch 35 = `ButtonSize` 32 + `IconSpacing` 3 |
| Pin / settings / minimize | 400 / 435 / 470 | | 32 | 32 | same row; minimize ink 16 x 2 | same |
| Right edge | 502 | | | | px: minimize centre 486 + 16 = 502, card edge 506 | `BarEdgeInset` 4 |
| Icon glyph | | | 16 | 16 | px: pin 416..431 x 13..28 in the Acrylic grab | `IconGlyphSize` 16 |
| Hover wash | | | 32 | 32 | px: +15 per channel over (48,51,44), about 9% ink | `Paint.Hover` dark .09, radius `TopIconHoverRadius` 8 |
| Badge corner / fill | | | | | px: rounded rectangle, fill about 18% of the badge colour | `BadgeCornerRadius` 6, `BadgeDarkFillAlpha` .18, light .13 |
| Badge text | | | | | px: x-height fits 11.5 px strong | `BadgeFontSize` 11.5, `Typography.Strong` (600) |
| Row height | | 0 | | 40 | px: first text line top at 44.5 = 40 + editor margin 4 + 0.5 | `DockedRowHeight` 40 = `TopScrollableWhitespace` |
| "● timer running" meta | | | | | not captured as meta in default size (the running chip is in the text) | `RunningTimerFontSize` 11.5 (code only) |

Vertical note: in **Acrylic and Mica** the bar content is centred at y = 20.5 (buttons y 4.5..36.5, as `DockedRowHeight` 40
predicts); in **Solid** (grab and F12) it is centred at y = 16.5, four DIP higher. See 9, item 1.

## 5. Pager pill (docked)

Reference: `pad-stack1-current-dark-default.png`, `pad-stack1-docked-solid-dark-small.png`.

| Box | x | y | w | h | px source | code source |
| --- | --- | --- | --- | --- | --- | --- |
| Pill, stack view | centred (98.5) | 597 | 309 | 42 | window row/col runs: x 106..414, y 598..639 (window), border at 598 and 639 | derived: `ButtonSize` 32 + 2 x `PillInset` 4 + 2 x `PillBorderThickness` 1 = `PillHeight` 42; width 2 + 8 + 299 |
| Pill, slot view | centred (94.5) | 597 | 317 | 42 | derived width 317 is used; F12 2x x 189..821 measures 316 | derived: stack width + 8 (selected dot 24 instead of 16) |
| Pill bottom inset | | | 4 | | px: pill bottom 639, card border 644 (4 px fill between) | `PillBottomInset` 4 |
| Strip height | | 593 | | 50 | | derived: `PillHeight` 42 + 2 x `PillBottomInset` 4 = `PagerStripHeight` 50 |
| Pill corner | | | | | px: radius visible at 12 | `PillCornerRadius` 12 |
| Pill border | | | 1 | | px: border pixel (69,63,55) over fill (38,37,34) | `PillBorderThickness` 1, `PillBorderAlpha` .35 |
| Pill shadow | | | | | px: dark halo about 3 px below, blur not isolated | `PillShadowBlur` 14, `PillShadowOffsetY` 3, alpha 90 / 28 |
| Prev arrow button | 103.5 (pill + 5) | | 32 | 32 | px: chevron ink 8 x 12 centred at pill + 21 | `Kit.IconButton` size `ButtonSize` 32, radius `PagerArrowHoverRadius` 8 |
| Count button | 138.5 | | 44 (stack mode, 1 digit) | 20 | row scan: count text between chevrons; stack view `1 / 2` | derived: `PagerCountBaseWidth` 30 + digits 1 x `PagerCountDigitWidth` 14; height `BadgeHeight` 20 |
| Count text | | | | | px: 11.5 strong, tabular figures | `PagerCountFontSize` 11.5, `Typography.Strong` |
| Next arrow button | 185.5 | | 32 | 32 | px: symmetric to prev | as prev |
| Separator | 225.5 | | 1 | 14 | px small grab: window x 157, y 459..472 (client x 149) | `PagerSeparatorWidth` 1, `PagerSeparatorHeight` 14, margin `PagerSeparatorMargin` 5 each side |
| Dot (resting) | | | 7 | 7 | px: runs of 7 at y 463..469 (small grab) | `PagerDotWidth` 7, `PagerDotHeight` 7, radius `PagerDotRadius` 3.5 |
| Dot hit box | 234.5 (first, stack view) | | 16 | 18 | px: pitch 19 between dots (small grab client x 162, 181, 200) | `PagerDotButtonWidth` 16 + `PagerRowSpacing` 3; height `PagerDotButtonHeight` 18 |
| Selected dot | | | 16 | 7 | px: wide yellow pill in `pad-slot5-bar-…` | `PagerDotSelectedWidth` 16, hit box `PagerDotSelectedButtonWidth` 24 |
| Empty dot ring | | | | | px: 1 px ring, faint | `PagerDotBorderThickness` 1.2 (2x render shows about 1.2) |
| Item spacing | | | 3 | | px: arrow to count to arrow | `PagerRowSpacing` 3 |
| Hover washes (arrow, count, dot) | | | | | px: lighter fill matching the 32 x 32 / 44 x 20 / 16 x 18 boxes (`…-hover-pager…png`) | `Paint.Hover` .09 dark, radii `PagerArrowHoverRadius` 8, `PagerCountHoverRadius` 8, `PagerDotHoverRadius` 9 |

Sum check: 32 + 3 + 44 + 3 + 32 + 3 + (5 + 1 + 5) + 3 + 9 x 16 = 275; adding 8 dot gaps x 3 = 299 (12 spacings total) inner width, matches the
measured 309 outer with border and inset.

## 6. Text content (editor)

| Box | Value | px source | code source |
| --- | --- | --- | --- |
| Editor left | 36 | F12 2x: text starts at 72 | `PadView` margin 36 |
| Editor right | 30 | derived from wrap width (about 440 wide text area) | `PadView` margin right 30 |
| Editor top | 40 + 4 | first line box starts at 44.5 | `TopScrollableWhitespace` 40 + margin top 4 |
| Editor bottom | 50 | strip top 593 | `PagerStripHeight` 50 |
| Line pitch | 23 | F12 2x: 157 to 203 | `Settings.FontSize` 15 x line spacing (setting, not a token) |
| Body font | 15, Cascadia Mono | px: cap height and pitch | `Settings.FontSize` default 15, `Typography.MonoFamily` |
| Heading groups | orange bold, group gap 1 blank line | px | colorizer (editor exception: Bold) |
| Timer chip | h 20, x 157.5, pitch 23 | F12 2x: 315..440 x 138..178 | `DrawChip` (radius 7, font x .93, fill .14 / .10, editor exception) |
| Checkbox | about 13 wide, 1.9 pen | F12 2x | editor exception, drawn in proportion to the font |

## 7. Toast

Reference: `toast-copied-solid-dark-default(-2x).png`, `toast-copied-dark-default.png`.

| Box | x | y | w | h | px source | code source |
| --- | --- | --- | --- | --- | --- | --- |
| Toast | centred (178.5) | 560 | 149.5 | 31 | F12 2x: x 357..656, y 1120..1182 (empty render) | derived: text + 2 x 12 padding; height 7 + about 17 + 7 |
| Toast bottom gap | | | | 52 | px: toast bottom 591, card bottom 643 | `ToastBottomMargin` = `PagerStripHeight` 50 + `ToastPagerClearance` 2 |
| Padding | 12 x 7 | | | | px: text inset | `Padding` 12, 7 |
| Corner | 10 | | | | px: radius 10 | `Shape.Card` 10 |
| Text | 12.5 | | | | px: x-height | `Typography.Label` 12.5 |
| Surface | light, about 92% | px: fill (218,213,203) over dark | `Paint.Toast` .92 |
| Max width | 380 | not reached | `MaxWidth` 380, margin 16 each side |

## 8. Menus, search

### 8.1 Right-click menu

Reference: `menu-contextmenu-hover-row-solid-dark-default.png` (popup, window grab origin offset 543, 71).

| Box | Value | px source | code source |
| --- | --- | --- | --- |
| Popup | w 234 x h 377 (window x 380..613, y 456..832) | border runs 380 / 613 and 456 / 832 | derived: `MinWidth` 224 + 2 x padding 4 + 2 x border 1; height 2 + 8 + 52 + 3 x 9 + 9 rows x 32 |
| Padding | 4 | first content row starts 4 px inside | `Padding` 4 |
| Border / corner | 1 / 8 | border pixel (60,57,53) | `BorderThickness` 1, `Shape.Popup` 8 |
| Action row (cut, copy, paste, delete) | h 52, 4 equal columns of 56 | rows 457..516 incl. gaps | `Height` 52, 4 columns |
| Row | h 32 | row pitch 32 (Undo 538, Redo 570, Select all 602) | `Height` 32, padding 8 |
| Separator | 1 px, margin 4 above and below (9 total) | lines at y 517, 622, 759 | `Height` 1, `Margin` 4 |
| Row icon column | 28 | glyph centre x about 28 inside the row | grid column "28,*,Auto" |
| Row font | 14 (label), 13 (shortcut) | px: label cap height | `Typography.Large` 14, `Typography.Body` 13 |
| Shortcut inset | 24 left margin, 8 right | right-aligned grey text | `Margin` 24, 0, 8, 0 |
| Shadow | blur 18, offset y 4, alpha 110 | soft halo below the popup | `Shape.MenuShadowBlur` 18, `PopupShadowOffsetY` 4, `PopupShadowDarkAlpha` 110 |

### 8.2 Search overlay

Reference: `search-results-solid-dark-default-2x.png` (client coordinates), `search-results-dark-default.png` (grab).

| Box | x | y | w | h | px source | code source |
| --- | --- | --- | --- | --- | --- | --- |
| Overlay margin | 14 | 14 | | | field left at 28 / 2 | `Margin` 14, 14, 14, 46 |
| Query field | 14 | 54 | 372 | 38 | F12 2x: x 28..772, y 108..184 | derived: padding 12 x 8 + line height of `Typography.Input` 15 + border |
| Field border (focus) | | | 2 | | F12 2x: 4 px ring, gold | code: `BorderThickness` 1 (rest); focus width not set in the file read |
| Field corner | 10 | | | | px: radius 10 | `Shape.Card` 10 |
| Field font | 15 | | | | px | `Typography.Input` 15 |
| Tool buttons (grid, trash, close) | right of the field, pitch 34 | F12 2x: icons at x 816, 884, 952 | `Spacing` 2 between 32 boxes |
| Result card (current) | 14 | 100 | 477 | 75 | F12 2x: x 28..984, y 200..350 | padding 12 x 9, corner `Shape.Card` 10 |
| Current ring | | | 2 | | F12 2x: 4 px gold | `BorderThickness` 1.5 current / 1 other |
| Result card gap | | | | 3 | F12 2x: 349 to 354 | list spacing (not read) |
| Result title | | | | | px: bold 15-ish | row title font (`Typography.Large`) |
| Result body | | | | | px: mono 12 | `Typography.Small` 12 |
| Tool button focus ring | | | 32 | 32 | user capture: white ring, 2 px (rows 25..26 and 55..56, columns 409..410 and 439..440), fill inside darker (11,11,10), corners near square | system default focus visual, not a token; sits inside the 32 x 32 box (`ButtonSize`) |
| Footer hint | | | | | px: grey 12 at the bottom ("Searching…", then "4 notes · Enter opens…") | `Typography.Small` 12; `_status` margin 4, 8, 0, 0 |

## 9. Mismatches (not fixed)

| # | Where | px says | Code or doc says | Note |
| --- | --- | --- | --- | --- |
| 1 | Top bar, Solid backdrop | bar content centred at y 16.5 (F12 and grab) | `DockedRowHeight` 40, centred, gives 20; Acrylic matches at 20.5 | Solid sits 4 DIP higher than Acrylic and Mica; the redraw must pick one |
| 2 | Search focus and current ring | 2 DIP thick (4 px at 2x) | `BorderThickness` 1.5 current, 1 rest; `design-scale` calls today's focus 1.5 | focus ring may be set in a style; the redraw should use 2 for both |
| 3 | Dev badge, small window and slot + mode badges | dev badge overlaps the `Saved …` text ("Saved 59m ago" clipped) | meta stack has no collision rule | real layout overlap, also at default size with `slot` + `timer` badges |
| 4 | Pager count width | 44 (1 digit) matches | `design-scale` 3.2 proposes `PagerCountBaseWidth` 32 (+2) | consistent, but the proposed width changes with digits |
| 5 | Keyboard focus | Tab from the editor only inserts a tab. The user's capture shows a focus ring on a search tool button: white, 2 px, 32 x 32, square corners | `Kit.Plain` sets `Focusable = true`; no focus token exists | the ring is the platform default (white), while the field and result rings are gold and 2 px: two focus looks, none from a token |
| 6 | F12 render | pager and toast text are empty | the live window shows both | dev capture limit, not an app bug; grabs used instead |
| 7 | Overlay / float bar reveal | bar slides in on a hover in the top 20 px; the pager did not reveal on a synthetic move to the bottom 8 to 20 px | `OverlayPagerRevealZone` 20, `FloatPagerRevealZone` 8 | pager reveal in those modes is not captured (needs real pointer travel); bar reveal is |
| 8 | Window top | card border sits at window y 1, with a 1 px top offset | left, right, bottom have an 8 / 8 / 1 px margin | asymmetric native frame; irrelevant to the HTML redraw |

## 9b. For the design language (observations, no action taken)

- **Two focus looks:** a white 2 px ring on buttons (platform default) and a gold 2 px ring on the search field, the
  current result and selected Settings tiles. A focus role (`Stroke.Thick` + one colour role) would unify them.
- **Selected state is already 2 DIP gold** on Settings tiles (caret style, background), the search field and the current
  result; today's code says 1.5 in places. This supports one `Stroke.Thick` 2.
- **Washes are one family:** hover is the same +15 per channel step on the bar icon, the pager parts and the menu rows,
  so `Wash.Hover` can drive all three.
- **Corners are four values in view:** 8 (hover boxes, popups), 10 (cards, toast, field), 12 (pill), 6 (badges).
  This matches the `Corner.Md` / `Lg` / `Sm` merge in `../design-contract.md`.
- **Spacing rhythm already runs on 3 and 4** in the pager (pitch 19 = 16 + 3) and 2 around menu rows; `Space.XS` 4
  absorbs the 3.

## 10. Not captured (and screenshots seen but not saved)

The user also sent, in chat, five dark-theme crops that were not saved as files and are described here only: pin button
lit gold (on state); Settings caret-style tile selected with a gold ring (Bar); Settings background tiles with Solid
selected by a gold ring; the overlay pager sitting over the note's last lines; the float pager below the window.

- Settings window (out of scope for v1, only the Tint dropdown for the dropdown popup).
- Focus rings on bar icons, pager parts and menu rows (one tool-button ring exists, see 9, item 5).
- Overlay and float pager as files (see 9, item 7; described below), the `● timer running` meta text, the save-failure card.
- Light theme beyond the docked and the overlay and float bars (menu, search, toast were captured in dark). Light is a lower priority than the token consolidation (user, 2026-11-04).
