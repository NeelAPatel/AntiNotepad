# A35-bar-insets-r2: lab round 2 (the user's verdict on the material lab)

Role: director (gpt-6.1-sol medium). Delegate to gpt-6-luna xhigh subagents one area at a time; review their diffs yourself.
Branch: `feat/A35-bar-insets` (this worktree). Round 1 is committed (`503696e`); read `.coord/tasks/A35-bar-insets.md` (the
round-1 brief: spec, constraints, build/selftest rules, all still apply) and your round-1 report
`.coord/reports/A35-bar-insets.md`. You cannot commit; the manager reviews and commits.

## What the user saw (dark mode, live, round-1 lab)
Screenshots: `.coord/handoff/A35-bar-insets/lab1/` (lab1-01 docked full pad, lab1-02 overlay top bar with blur behind,
lab1-03 float pill with acrylic window, lab1-04 overlay top bar, lab1-05 top bar alignment, lab1-06 pill alignment,
lab1-07 Windows' own caption buttons for comparison, lab1-08 pill back-arrow zoom).
- Docked: looks good.
- Acrylic window pill (overlay and float): glitchy slide animation, sometimes out of position, corner artifacts. **Rejected.**
- Top bar blur behind (overlay, float): "glowy", contrast not dark enough to be useful; sharp text still shows through.
  **Bug**: scrolling while the top bar is visible leaves stale artifacts of what used to be under the bar.
- Feathered gradient top backdrop: same problem as the original. **Rejected.**
- Hide button glyph (`Glyph.Hide`, U+E921) sits 1-2 px above center. Windows' own minimize glyph (lab1-07) is centered.
- Pill inset 2 feels tight: make it **4**.

## Decisions
- Pill material: **Solid** (palette Surface, fully opaque) in all three bar styles and every Material setting. Blur behind for
  the pill stays only as a dev Lab option, and only if the fixes below make it clean.
- Top backdrop in overlay and float: **blur behind only**, fixed properly. No feathered gradient option.

## Work
1. Remove the Acrylic-window pill route entirely: native popup material requests/fallbacks, custom overlay popup placement,
   popup opacity/placement sync, `Native.ConfigureMaterialPopupWindow`, related lab state and telemetry. The overlay pill
   lives in-window again (in `_bottomLayer`, sliding as before r1); the float pill stays in `_floatPager` exactly as in r6
   (fade in/out, `PlacementMode.Bottom`, offset = `FloatGap`), with a Solid background. Float popup corners must render
   clean (no square/acrylic corners behind the 12 px pill).
2. Remove the feathered-gradient top backdrop and its lab toggle.
3. Fix blur behind (top band in overlay/float, and the pill's lab option in overlay):
   - No sharp text visible through the dense part (0..40): the original editor content under the band must be hidden there
     and only the blurred copy shown, then feathered back to the sharp text by 52 (midpoint 46).
   - Contrast: the bar's icons and text must read clearly on it. Darken: make the tint a token and raise it (start around
     Canvas at 0.75-0.8 in the dense part); no glow from bright glyphs bleeding up.
   - Never stale: it must stay correct while scrolling (wheel, drag, keyboard), typing, folding, note paging, split view,
     and during the bar's slide animation. Re-sample on every change of what lies beneath; no artifacts after scroll.
   - Works with software rendering (Material "solid" uses Avalonia's software renderer): check whether `BlurEffect` renders
     there; if not, the dense part uses an opaque Canvas band with the same 12 px feather, never a crash or an empty band.
   - Measure and log the per-frame CPU cost of the blur while scrolling (keep the existing `bar-lab-render` line).
4. Hide glyph centering: `GlyphIcon` centers by the text line box, not the glyph's ink. Center icon glyphs by their ink bounds
   (e.g. `FormattedText.BuildGeometry(...).Bounds`, computed once per glyph/scale), then snap to device pixels. Verify the
   minimize line lands on the exact vertical center of its 32 px button at 100%, 125%, 150% scaling, and that the other bar
   and pager icons don't shift visibly for the worse (report their before/after offsets).
5. Tokens: `PillInset` 4 (pill = 32 + 2*4 + 2*1 = 42, docked pager strip = 50). Update the selftest numbers. Add the blur tint
   token(s). Keep every bar/pager number in `ChromeTokens`.
6. Lab rows left (dev only): "Pill material: Solid / Blur behind". Nothing else.

## Done means
- 0 warnings, SELFTEST PASS (same build/selftest procedure; delete the check folder if policy allows, else say so).
- Report `.coord/reports/A35-bar-insets-r2.md`: what was removed (files/lines), how blur behind now hides the sharp text and
  stays fresh, software-render result, measured costs, glyph offsets before/after, and anything that still needs a live look.
