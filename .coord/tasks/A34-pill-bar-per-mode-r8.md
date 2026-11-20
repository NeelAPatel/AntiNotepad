# A34-pill-bar-per-mode-r8: bigger corner buttons, taller count target

Role: quick, a surgical edit of one existing HTML lab (docs/agents.md). Edit only `tools/playground/a34-pill-bar-lab.html`. Same hygiene: single file, offline, no top-level identifier named `top`, `node --check` clean. Keep everything else.
User (2026-11-20), verbatim:
- "Lets increase the square height and width of the delete/new note buttons to match the height of the pill bar. This would allow for same visual consistency as the top right despite being a bit of a bigger target to click."
- "Lets increase the height of the page number pill target as well to match its left/right button neighbours." (An MSPaint mockup shows the hover square of the "1 / 3" count button being shorter than the hover squares of the ‹ and › buttons beside it; they must all be the same height, so the three hover squares read as one row of equal squares/rounded rects.)

## Changes
1. Corners layout (Docked and Overlay): the Delete and New note buttons become square, width and height equal to the pill's outer height (`T.pillHeight`, 42 px: button 32 + 2 x inset 4 + 2 x border 1; the source is `ChromeTokens.PillHeight`), with the same hover wash and hover radius family as the other icon buttons (scale the radius sensibly, the hover shape stays a rounded square), glyph size unchanged (16). Vertically centered with the pill so their top and bottom edges line up with the pill's outer edges. Delete hover: glyph pastel red; New note hover: glyph accent (unchanged).
2. In the one-pill (Float) layout the buttons are inside the pill, so they keep the inner button size (32) because the pill itself is the 42 px container; do not enlarge them there.
3. Count button ("1 / 3", the Search target): its hover/press target height equals its arrow neighbours' height (32, `T.button`), not 20. Width unchanged (`32 + 14 x digits`). Make sure the count text stays centered and the three hover shapes (‹ count ›) share one top and bottom edge.
4. The dots keep their taller hit area; nothing else changes.
5. Check 320 px width and Split: the bigger corner buttons (42 each) plus the pill must still fit; if not, say what you did (for example the pill shrinks its gaps) in the final message.

## Done means
Valid HTML; script parses; no external requests; console clean. Final message, 5 lines max.
