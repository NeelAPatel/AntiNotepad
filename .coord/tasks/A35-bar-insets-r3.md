# A35-bar-insets-r3: the overlay/float top gap as virtual whitespace, not a taller line 1

Role: director (gpt-6.1-sol medium). Delegate to one gpt-6-luna xhigh worker; review its diff yourself.
Branch: `feat/A35-bar-insets` (this worktree; rounds 1-2 committed, HEAD `7c01071`+). The round-1 brief
`.coord/tasks/A35-bar-insets.md` constraints and build/selftest rules still apply. You cannot commit.

## Problem (user, live, screenshots in `.coord/handoff/A35-bar-insets/lab2/`)
In overlay and float, `Editor/Elements.cs` `TopGapGenerator` makes room under the sliding top bar by putting a tall
invisible inline object at the end of line 1 (Gap = `ChromeTokens.DockedRowHeight`, 40). Line 1 becomes 40 px taller, so:
- the selection highlight (and current-line/other line backgrounds) on line 1 is a huge box covering the gap;
- the fold chevron for line 1 (a heading) sits in the wrong place, high up in the gap instead of beside the text.

The user's model: the gap is **whitespace before the document**, like the blank "lines" VS Code adds after the end of a file
so the end can be scrolled to eye level. It is not part of line 1 and has no line height.

## Work
1. Replace `TopGapGenerator` with a real top inset of the scrollable content in overlay and float (and in no other style or
   in private/Settings previews, same as today): at scroll offset 0 the first line's top sits `DockedRowHeight` (40) below the
   editor's top; scrolling moves the gap away with the text; line 1 keeps its normal height and every per-line thing
   (selection, current-line highlight, search highlights, caret, fold chevrons and the fold gutter, bullets/renumbering,
   paper/grid layer, hit testing, mouse selection drag from the gap, PageUp/Home/Ctrl+Home, caret-into-view on typing) treats
   line 1 as a normal line. Find the cleanest AvaloniaEdit 12 way (e.g. a top margin/offset in the TextView's scroll extent
   or the scroll viewer's content, a custom margin in layout, or an AvaloniaEdit extension point); no reflection hacks, no
   forked AvaloniaEdit. Report which route you chose and why.
2. Keep everything that currently depends on the gap correct: `PadView.UpdateEdgeFade` (fade only when scrolled past content),
   the blur-behind top band sampling (`UI/PadWindow.MaterialLab.cs`), the paper layer (`Editor/Renderers.cs`), and the
   selftest check that names `TopGapGenerator.Gap` (point it at the new token).
3. Gap value stays a `ChromeTokens` token (40).

## Done means
- 0 warnings, SELFTEST PASS (same procedure; leave the check folder if policy blocks deletion and say so).
- Report `.coord/reports/A35-bar-insets-r3.md`: the route, files changed, how each per-line feature above was verified (or
  what needs the manager's live look), and any behavior that changed at scroll offset 0.
