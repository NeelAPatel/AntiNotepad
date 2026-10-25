# A35-bar-insets-r4: top blur band must not paint over the window material

Role: worker (gpt-6-luna xhigh). Branch `feat/A35-bar-insets` (this worktree; rounds 1-3 committed). The round-1 brief
`.coord/tasks/A35-bar-insets.md` constraints and build/selftest rules still apply. You cannot commit.

## Problem (user, live; `.coord/handoff/A35-bar-insets/lab2/lab2-04-*` light wallpaper, `lab2-05-*` dark wallpaper)
In overlay and float, the top bar's blur-behind band (`UI/PadWindow.MaterialLab.cs`) hides the sharp note text under the bar
by drawing an **opaque Canvas backing**, then the blurred copy and a Canvas tint (0.78). The pad window itself is translucent
(Acrylic/Mica: `_tint` is Canvas at `Settings.Tint`, about 0.7, over the system material). Over a dark wallpaper the opaque
band matches the window body; over a light wallpaper it reads as a heavy dark strip with a visible step to the lighter body.
The pill's lab "Blur behind" option is fine; leave it.

## Work
1. Hide the sharp text in the band by masking the **editor content itself**, not by painting over it: the pane's text is fully
   transparent in the dense part (0..40 from the pad's top edge), ramps back (half at 46) and is fully visible by 52, following
   the top band's slide (bar hidden = no mask). Combine this with the existing edge-fade opacity mask in
   `PadView.UpdateEdgeFade` into one mask; do not stack two masks that fight. Both panes in split view.
2. The band then draws only: the blurred copy of the text + a Canvas tint, both masked with the same 40/46/52 feather, and no
   opaque backing, so the window's own material shows through exactly as in the body. Make sure the blurred copy samples the
   text **without** the new mask (e.g. sample a child such as the TextView while the mask sits on an ancestor), or the band
   will blur nothing.
3. Tint: keep it a `ChromeTokens` token. Choose it so the band is only modestly darker than the body on both light and dark
   wallpapers (start around Canvas at 0.35-0.45 on top of the window's own tint) while the bar icons still read; report the
   value and reasoning. Software rendering (Material solid) must still work: there the window is opaque, so the result is
   simply the blurred text on Canvas.
4. Keep r2/r3 behavior: freshness on scroll/typing/fold/paging/split/slide, no stale artifacts, the r3 top gap behavior,
   selection/chevrons on line 1 untouched. The r3 top gap is `Editor/ScrollableTopInset.cs` (a logical-scroll adapter):
   sample and mask in the coordinates it actually arranges, including while the 40 px leading whitespace is partly visible.
   User's rule: line 1 has the same height in every bar style, and the 40 px top margin keeps text clear of the bar.

## Done means
- 0 warnings, SELFTEST PASS (leave the check folder if policy blocks deletion and say so).
- Report `.coord/reports/A35-bar-insets-r4.md`: the masking route, files changed, the tint value, what needs the manager's
  live look (light vs dark wallpaper, Acrylic/Mica/Solid, split view).
