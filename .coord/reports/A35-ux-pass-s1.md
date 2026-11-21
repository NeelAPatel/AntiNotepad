# A35-ux-pass-s1: token audit (stage 1)

Written by the item chat, 2026-11-20. Target: `docs/design-scale.md` R column (applied by A4n-scale-lab) and
`.claude/rules/coding.md` "Design values". Report only; no product file changed.

**How it was made.** `tools/check-design-tokens.ps1 -Inventory` produced 224 hits
([inventory](A35-ux-pass-s1-inventory.tsv)). [`A35-ux-pass-s1-classify.py`](A35-ux-pass-s1-classify.py) split each hit
into its literal numbers and classed each one against the scale ([output](A35-ux-pass-s1-classified.tsv); rerun:
`python -I A35-ux-pass-s1-classify.py A35-ux-pass-s1-inventory.tsv out.tsv`). Two Codex terra runs read the code for
values the scanner misses and compared tokens with the doc ([run A](A35-ux-pass-s1.codex-a.md),
[run B](A35-ux-pass-s1.codex-b.md)); both stopped partway (READY: no). Their confirmed rows are kept; the item chat
checked pens and drawing calls with its own search.

## Summary
- **The scale exists only as a document for spacing.** `Theme/` has no spacing tokens (no XS to 2XL class).
  97 literal spacings at control sites already equal a scale step, but each is a bare number, so nothing stops drift.
- **39 spacing literals are off the scale**, plus 20 layout sizes off the 4 grid. Most are 6 (should be 8), 10 (12),
  14 (16), 3 and 5 (4), 7 and 9 (8).
- Top files by off-scale or off-grid literals: `SettingsWindow.cs` 9, `SearchOverlay.cs` 7, `SettingsParts.cs` 7,
  `PadView.cs` 6, `HelpOverlay.cs` 6, `MotionPreview.cs` 6.
- **Radius, font size, color:** nearly clean. All 10 font-size hits are scanner false positives (the size is a token;
  the number is a weight or a width). Radius hits are `Shape.Round(size)`, fine. One literal color pair:
  `SettingsParts.cs:398` (`#1A1A1A` / white for text on the accent swatch).
- **Strokes the scanner cannot see:** 1.8 (`MotionPreview.cs:271-272`), 1.1 (`SettingsParts.cs:609`), 1.4
  (`SettingsParts.cs:678`); radius 9 at `Renderers.cs:101`. Checkbox pens 1.6 / 1.9 are exception 4.
- **One opacity literal:** `PadWindow.cs:344`, `0.35 + 0.65 * t` (a fade ramp; motion-adjacent, noted only).
- **Tokens vs doc:** one value drifted on purpose (`PagerDotButtonHeight` 20 to 32 in A34: the doc is stale); 10 newer
  tokens are not in the doc. No token found that breaks the scale by mistake.
- **Timers:** 12 literals (debounces, sleep and toast delays). Behavior timing, not motion; the toast and bar-sleep
  delays sit next to settings values. Listed for a later token family; not a visual finding.

## Off-scale spacing (39 literals; full list in the classified TSV)
| File:line | Literal(s) | Nearest step |
|---|---|---|
| `UI/SettingsWindow.cs:195` | margin 10, 4, 10, 10 | M 12 |
| `UI/SettingsWindow.cs:205` | margin 10 | M 12 |
| `UI/SettingsWindow.cs:184` | margin 18 | L 16 |
| `UI/SettingsWindow.cs:260` | margin 28 (right) | 2XL 32 |
| `UI/SettingsWindow.cs:348, 532, 715` | spacing 6 | S 8 |
| `UI/SearchOverlay.cs:47` | margin 14, 14, 14 (bottom 46) | L 16 |
| `UI/SearchOverlay.cs:78, 172` | margin 6 | S 8 |
| `UI/SearchOverlay.cs:201` | spacing 3 | XS 4 |
| `UI/HelpOverlay.cs:20` | margin 10, 14, 10 (bottom 40) | M 12, L 16 |
| `UI/HelpOverlay.cs:22` | margin 14 (right) | L 16 |
| `UI/SettingsParts.cs:298` | toggle knob offset 3 | XS 4 |
| `UI/SettingsParts.cs:446` | spacing 5 | XS 4 |
| `UI/SettingsParts.cs:361, 526` | spacing / margin 1 | 0 or Nudge 2 |
| `UI/PadWindow.Layout.cs:763-766` | spacing 6, margin 6, spacing 9 (save-failure card) | S 8 |
| `Editor/PadView.cs:99, 130` | margins 36, 30, 37, 7 (editor text inset, hint) | 30 to 2XL 32; 7 to S 8; 36/37 layout |
| `Editor/PadView.cs:1039` | margin `left + 1`, 7 | Nudge, S 8 |
| `Editor/Completion.cs:116, 143` | spacing 10 | M 12 |
| `UI/MotionPreview.cs:85, 107, 223` | spacing 14, 6, 5 | L 16, S 8, XS 4 |
| `UI/PadMenu.cs:140` | menu action gap 2 or 0 (counted 1 by the script: false hit on `- 1`) | on scale (Nudge) |

## Layout sizes off the 4 grid (20)
`Completion.cs:119, 146` widths 70, 86; `HelpOverlay.cs:35, 61` 150; `MotionPreview.cs:135, 226` 46, 14 x 3;
`PadMenu.cs:273` icon box 18 x 18; `PadView.cs:130` 37; `SearchOverlay.cs:47` bottom 46. `TaskbarPresence.cs` 1 x 1 (a
hidden helper window) and arithmetic (`Width / 2`, `Math.Max(1, ...)`) are not design values.

## Strokes and drawing (missed by the scanner)
| File:line | Literal | Class |
|---|---|---|
| `UI/MotionPreview.cs:271-272` | pen 1.8 | off-scale, Thick 2 |
| `UI/SettingsParts.cs:609` | pen 1.1 | off-scale, Thin 1 (Settings picture art) |
| `UI/SettingsParts.cs:678` | pen 1.4 | off-scale, Thin 1 (picture art) |
| `UI/SettingsParts.cs:560-680` | ellipse sizes 7, 3.7, 0.9 | sample art, exception 11 |
| `Editor/Renderers.cs:101` | radius 9 | off-scale, Md 8 |
| `Editor/Renderers.cs:399-424` | checkbox x.86, pens 1.6 / 1.9, radius x.28 | exception 4 |
| `Editor/Renderers.cs:254` | chip font x.93 | exception 5 |
| `UI/Kit.cs:238` | letter spacing 0.8 | typography; no token family |

## Tokens vs doc
| Token | Today | Doc R | Stale side |
|---|---|---|---|
| `ChromeTokens.PagerDotButtonHeight` | 32 | 20 | doc (A34 raised it for Fitts) |
| `PinRingThickness` 4, `DevBadgeLightTextInkMix` .62, `PagerCountButtonHeight` 32, `BarCornerSize` 42, `PlainIconWeight` 0, `DeleteDebounceMs` 500, `Paint.DisabledIcon` .5, `UiFamily`, `MonoFamily`, `EditorFallback` | as named | not in doc | doc predates them |

## What this means for stage 4 (proposal only)
1. Add the spacing scale as tokens (`Space.Xs` to `Space.Xxl`, `Space.Nudge`) and move the 97 on-scale literals to it.
   Mechanical; no pixel moves.
2. Snap the 39 off-scale literals (visible: 1 to 4 px per site; Settings, search and help most). Lab-first.
3. Teach the gate scanner pens (`new Pen(`) and drawing radii, and add a stroke token pair (Thin 1, Thick 2).
4. Update `design-scale.md` 3.2 for the 10 newer tokens and the A34 dot height.

## Checks run
`pwsh tools/check-design-tokens.ps1` (TOKENS PASS); `-Inventory`; the classify script; `rg` for `Pen(`, `RoundedRect(`,
`DrawEllipse(`, `Colors.`, `Brushes.`, `Color.Parse` in `UI/`, `Editor/`, `Motion/`. Two Codex terra runs, read-only.
