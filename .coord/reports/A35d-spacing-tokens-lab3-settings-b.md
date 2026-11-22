# A35d-spacing-tokens lab 3: settings-b

## 1. Files written
- `tools/playground/a35d-lab/settings-b.js` and this report.

## 2. Controls that work
- Editor dropdowns, segments, toggles, backing and language expanders, pattern tiles and language choices update their local selection; backing reveals pattern weight. Font, size, width, line spacing and backing update the preview.
- Feel expanders, scroll-style selection, the style-specific tuning selections and Reduce motion update locally. The editable sample scrolls; horizontal wheel, Shift+wheel or a sideways pointer gesture switches among the three source notes. The scroll and note traces animate for two seconds after movement.
- These are lab approximations: tuning selections do not reproduce the app's scroll physics, and several writing-aid, tone and pattern-weight settings update their controls but do not restyle the preview. Switching notes uses a short CSS arrival rather than the app's swipe engine. Preview editing and source markdown rendering are simplified.

## 3. Snap rows wired
- 5: backing tile stack gap, 6 → 8 px, marked.
- 6: dropdown list gap, 1 → 2 px, via core and marked.
- 8: drawn indent-guide icon pen, 1.4 → 1 px, marked. The 1.1 → 1 px clock pen belongs to the Theme page, not these pages. Paper swatches use the source's fixed 1 px pattern pen.
- 16: preview outer legend gap 14 → 16, preview stack gap 6 → 8, legend key gap 5 → 4 px, marked.
- 17: preview trace pens 1.8 → 2 px, marked; mode changes wake a redraw.
- 19: no Fluent controls are built on these two pages; the shared shell's search field is marked by its owner. Their SettingsParts dropdowns, segments, toggles and picture tiles are custom controls with their own radii.

## 4. Captures and remaining differences
- `settings-Editor-0` through `settings-Editor-5`: inspected individually. Sample content comes from Showcase, but the browser approximation lacks the real editor's full markdown, links, checkboxes, answer bubbles, folding chevrons, fade and syntax highlighting; scrolling positions and trace shapes depend on interaction. Shared kit geometry, glyphs, colors and Acrylic also need visual comparison.
- `settings-Feel-0` through `settings-Feel-5`: inspected individually. Reading text comes from MotionPreview, but editor formatting, folding chevrons, checkboxes, edge fade, motion physics and exact trace timing differ. Shared kit geometry, glyphs, colors and Acrylic also need visual comparison.
- No rendered comparison was possible under the offline/no-browser brief; pixel equivalence is unverified for every capture.

## 5. Local helpers
- Source-shaped SVG paper swatches and indent icon; local editable sample, note switch, trace canvas, legend and local page state. Core has no MotionPreview or pattern-swatch builder.

## 6. Checks
- `node --check tools/playground/a35d-lab/settings-b.js` passed. Confirmed the shell calls page builders with a root, and checked selector class names against core builders. This file uses no DOM IDs. No browser, server, app launch, product edits or commit. Live visual and interaction verification remains open.
