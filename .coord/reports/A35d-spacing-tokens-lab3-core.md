# A35d-spacing-tokens lab 3: core

## 1. Files written
- `tools/playground/a35d-lab/core.css`, `core.js`, `kit-test.html`.
- This report. No product code, status, server, browser, app launch or commit.

## 2. Controls that work
- Settings: heading, group, section, card, row, subRow, expander, toggle, segments (`seg` alias), dropdown, textButton, textBox, readOnlyPath, swatches, tile, pictureChoice, paletteChips, accentDots, caretPicture and materialPicture. Pad: iconButton, pill, toast, badge, metaText, windowFrame (`frame` alias), panelHeader, plain, text and toggles. Fluent button, text field, combo and scrollbar classes are present. Selection and actions update locally; callbacks receive the selected value or state.
- Main signatures: `row(icon, title, description, control)`, `expander(icon, title, description, value, children, open)`, `segments(options, current, pick)`, `dropdown(options, current, pick, minWidth)`, `toggle(on, set, name)`, `swatches(options, current, pick)`, `pictureChoice(options, current, pick)`, `iconButton(glyph, name, click, size)`. Options accept strings, `[value, label]` pairs, or objects with `value` and `label`. Picture objects may supply `picture` and `width`; swatches may supply `color`.
- `setMode`, `onMode`, `snap`, `applySnaps`, `mark`, `setChanges`, `setTheme`, `glyph`, `backdrop`, frame messages and standalone controls are available. Test page has both mode variables scoped side by side. Consumers must call `mark` on their changed elements; core marks its picture tiles.

## 3. Snap rows wired
CSS custom properties on `:root` for both modes; numbers are DIP. Properties are available to other runs, which must attach them to their own elements and mark those elements.

| Row | Element / property suffix | Before | After |
|---|---|---|---|
| 1 | Settings nav `nav-left` | 18 | 16 |
| 2 | nav list `nav-side`, `nav-top`, `nav-bottom` | 10, 4, 10 | 12, 4, 12 |
| 3 | nav item `nav-item-left` | 10 | 12 |
| 4 | page `page-right` | 28 | 32 |
| 5 | Settings stacks `row-gap` | 6 | 8 |
| 6 | dropdown list `item-gap` | 1 | 2 |
| 7 | picture tile label stack `swatch-gap` | 5 | 4 |
| 8 | picture pens `pen-thin`, `pen-thick` | 1.1, 1.4 | 1, 1 |
| 9 | search `overlay-margin` | 14 | 16 |
| 10 | search `field-margin`, `row-margin` | 6, 6 | 8, 8 |
| 11 | search `line-gap` | 3 | 4 |
| 12 | help `side-margin`, `top-margin` | 10, 14 | 12, 16 |
| 13 | help `panel-right` | 14 | 16 |
| 14 | completion `row-gap` | 10 | 12 |
| 15 | save failure `stack-gap`, `inner-gap`, `margin` | 6, 6, 9 | 8, 8, 8 |
| 16 | motion preview `outer-gap`, `middle-gap`, `inner-gap` | 14, 6, 5 | 16, 8, 4 |
| 17 | motion preview `pen` | 1.8 | 2 |
| 18 | folded highlight `radius` | 9 | 8 |
| 19 | Fluent control `radius` | 3 | 4 |

Use e.g. `var(--snap-6-item-gap)`. Core applies rows 6, 7 and 19; remaining consumers wire their own surfaces. Row 8 variables are available for picture art supplied by consumers.

## 4. Captures and remaining differences
- Inspected `settings-Theme-0`, `settings-UI-0`, and `pad-AntiNotepad-dev`. Geometry and colors follow source, but no browser or screenshot comparison was permitted, so pixel equivalence and interaction appearance are unverified. Acrylic is a static gradient approximation; system compositor blur is absent. Settings icons use installed Segoe Fluent Icons rather than the app's bundled Fluent System Icons. Segment highlight changes without the source's sliding animation, and picture art is simplified. Other Settings and pad captures have not been individually compared. Light Fluent control values and primary button contrast are approximations.

## 5. Helpers
- `applySnaps(element, mode)` permits independent before/after sections; `setMode` and `setChanges` allow host control. No dependencies or external assets.

## 6. Checks
- `node --check core.js` and extracted inline `kit-test.html` script: passed. Checked test page's `tests` DOM ID against its markup.
- Node DOM-stub smoke passed for toggle, segments, dropdown, theme and mode; this is not a browser rendering or keyboard interaction check. Visual and real DOM interaction checks remain for a later permitted hand test.
