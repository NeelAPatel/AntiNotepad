# A35-ux-pass-s1: token audit

## Summary

- **224 inventory rows:** 190 spacing, 12 timer, 10 font-size, 6 radius, 5 color and 1 opacity. These are scanner categories, not verified design-value counts. [Inventory](.coord/reports/A35-ux-pass-s1-inventory.tsv)
- **0 tween rows** appear in the supplied inventory.
- The audit is **incomplete**: I did not classify every inventory row, finish the missed-value scan, or compare every token. Per-class totals and the five files with the most off-scale values are therefore unverified.
- Several scanner matches require correction before counting: the `GlyphIcon` matches include a tokenized size followed by a numeric weight, and the `Kit.Text` matches at `PadWindow.Layout.cs:753,757` capture button widths rather than font sizes.
- One documented token differs from R: `PagerDotButtonHeight` is 32 today, versus 20 in the scale document. [ChromeTokens.cs:62](product/AntiNotepad/Theme/ChromeTokens.cs:62), [design-scale.md:245](docs/design-scale.md:245)
- The scanner misses drawing values, including 1.8-pixel pens and a 9-pixel radius. Examples are listed below; this is not an exhaustive missed-value inventory.

## Off-scale

Not completed. The missed-value table contains verified examples.

## On-scale literals

Not completed.

## Layout sizes

Not completed.

## Exceptions

| File:lines | Value | Section 6 number |
|---|---|---|
| [Renderers.cs:254](product/AntiNotepad/Editor/Renderers.cs:254) | `DrawChip` font × 0.93 | 5 |
| [Renderers.cs:399](product/AntiNotepad/Editor/Renderers.cs:399), [404–405](product/AntiNotepad/Editor/Renderers.cs:404), [421](product/AntiNotepad/Editor/Renderers.cs:421) | Checkbox font × 0.86, pens 1.6/1.9, radius × 0.28 | 4 |

## Timers

The table identifies the 12 scanner hits. Whether a matching `Theme/` or `Motion/` token already exists was not fully verified.

| File:line | Value | What it times | Token? |
|---|---:|---|---|
| [Motion.cs:262](product/AntiNotepad/Editor/Motion.cs:262) | 530 ms | Caret blink | Unverified |
| [PadView.cs:661](product/AntiNotepad/Editor/PadView.cs:661) | 140 ms | Editor analysis debounce | Unverified |
| [Store.cs:28](product/AntiNotepad/Model/Store.cs:28) | 50 ms | Save coalescing | Unverified |
| [Timers.cs:29](product/AntiNotepad/Model/Timers.cs:29) | 250 ms | Running timer tick | Unverified |
| [TaskbarPresence.cs:15](product/AntiNotepad/Platform/TaskbarPresence.cs:15) | 500 ms | Foreground-loss grace period | Unverified |
| [TaskbarPresence.cs:16](product/AntiNotepad/Platform/TaskbarPresence.cs:16) | 1 s | Post-hide fallback period | Unverified |
| [PadWindow.Keys.cs:255](product/AntiNotepad/UI/PadWindow.Keys.cs:255) | 1.5 s | Delay before trimming closed Settings resources | Unverified |
| [PadWindow.Layout.cs:64](product/AntiNotepad/UI/PadWindow.Layout.cs:64) | 450 ms | Bar sleep delay | Unverified |
| [PadWindow.Layout.cs:65](product/AntiNotepad/UI/PadWindow.Layout.cs:65) | 1600 ms | Initial chrome sleep delay; later reset from settings | Unverified |
| [PadWindow.Layout.cs:66](product/AntiNotepad/UI/PadWindow.Layout.cs:66) | 2600 ms | Initial toast timer; later reset for the toast type | Unverified |
| [SearchOverlay.cs:43](product/AntiNotepad/UI/SearchOverlay.cs:43) | 125 ms | Search debounce | Unverified |
| [SettingsWindow.cs:209](product/AntiNotepad/UI/SettingsWindow.cs:209) | 150 ms | Settings search debounce | Unverified |

## Missed by scanner

Examples only; the full scan and token assignments remain outstanding.

| File:line | Kind | Literal | Class | Nearest step |
|---|---|---:|---|---|
| [MotionPreview.cs:271–272](product/AntiNotepad/UI/MotionPreview.cs:271) | Stroke | 1.8 | Off-scale | Thick 2 |
| [MotionPreview.cs:275](product/AntiNotepad/UI/MotionPreview.cs:275) | Radius | 8 | On-scale | Md 8 |
| [Renderers.cs:101](product/AntiNotepad/Editor/Renderers.cs:101) | Radius | 9 | Off-scale | Md 8 |
| [SettingsParts.cs:609](product/AntiNotepad/UI/SettingsParts.cs:609) | Stroke | 1.1 | Off-scale | Thin 1 |
| [SettingsParts.cs:678](product/AntiNotepad/UI/SettingsParts.cs:678) | Stroke | 1.4 | Off-scale | Thin 1 |
| [SettingsParts.cs:398](product/AntiNotepad/UI/SettingsParts.cs:398) | Color | `#1A1A1A`, `Colors.White` | Not classified | No color ramp established in the supplied scale |

## Tokens vs scale

Partial comparison only.

| Token | Today | R value | Note |
|---|---:|---:|---|
| [ChromeTokens.PagerDotButtonHeight](product/AntiNotepad/Theme/ChromeTokens.cs:62) | 32, via `ButtonSize` | 20 | Differs from [section 3.2](docs/design-scale.md:245). |
| [Paint.DisabledIcon](product/AntiNotepad/Theme/Paint.cs:26) | .5 | Not listed | Additional token |
| [ChromeTokens.PinRingThickness](product/AntiNotepad/Theme/ChromeTokens.cs:16) | 4 | Not listed | Code names it a stroke-scale exception; it is absent from the document’s numbered exceptions. |
| [ChromeTokens.DevBadgeLightTextInkMix](product/AntiNotepad/Theme/ChromeTokens.cs:35) | .62 | Not listed | Additional token |
| [ChromeTokens.PagerCountButtonHeight](product/AntiNotepad/Theme/ChromeTokens.cs:63) | 32, via `ButtonSize` | Not listed | Additional token |
| [ChromeTokens.BarCornerSize](product/AntiNotepad/Theme/ChromeTokens.cs:64) | `PillHeight` | Not listed | Additional token |
| [DevSlotTokens](product/AntiNotepad/Theme/DevSlotTokens.cs:9) | Six slot colors | Not listed | Additional token family |

## Tween

0 inventory hits; out of scope.

## Checks run

Read-only PowerShell `Get-Content` and `Import-Csv` for the brief, scale, rules, inventory and project context; `rg -n` searches for token declarations, timer uses, drawing calls and scanner patterns. No build or app launch.

**READY: no** — the complete row classifications, missed-value inventory, token comparison, category totals and off-scale file ranking remain unfinished.