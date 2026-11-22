# A35d-spacing-tokens lab, round 3 fix: Settings, backdrop and host

Role: terra, fixes after a browser check by Claude.

Read `.coord/tasks/A35d-spacing-tokens.lab3-common.md` in full and follow it. Write in small saved steps.
Your files: `tools/playground/a35d-lab/core.css`, `core.js` (backdrop only; keep every API name), `settings.html`,
`settings-a.js`, `settings-b.js`, `settings-c.js` and `index.html`. Do not edit `pad.html` or `help.js`; another run
is fixing them at the same time.

Fix each finding from the browser check:
1. **Backdrop is flat dark.** Every Settings capture (`build/shots/a35d/settings-*`) shows the Acrylic look: a dark
   tint over a blurred, warm, uneven background, lighter at the top-left edge. Make `LAB.backdrop` produce that. Use
   layered radial gradients behind the code's tint alpha, with no image files. Apply it to the Settings window.
2. **Missing row icons.** Some rows have no icon, though the captures show one: Caret style, Blink, Glide between
   positions, Text width, and others. Go through every page against its captures. Give each row the same glyph, from
   the same font family (`Glyph.Fluent` uses the bundled font; see the shared brief).
3. **Mode row icon** differs from the capture (a half-filled circle). Check every row icon for this.
4. **Host page (`index.html`)** squeezes both windows into small boxes with their own scrollbars. Size each iframe to
   its window, at full size (pad 460 x 560 plus its lab controls; Settings 900 x 720). Place them side by side when the
   screen is wide enough, and stack them with the page scrolling when it is not. Never nest scrollbars, and never
   scale the windows.
5. Pass the Settings pages over once more against every capture (text, order, control kinds, defaults), and fix what
   still differs.

Report: `.coord/reports/A35d-spacing-tokens-lab3-fix-settings.md`, with the shared report sections plus one answer per
numbered item.
