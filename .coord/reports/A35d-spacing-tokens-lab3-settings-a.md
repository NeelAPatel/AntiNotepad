# A35d-spacing-tokens lab 3: settings-a

## 1. Files written
- `tools/playground/a35d-lab/settings.html`, `settings-a.js`, `index.html`; this report.

## 2. Controls that work
- Shell: nav selection and hover, find results across registered pages, close, page transition, scrolling, and loading notes for missing scripts. Theme: mode, palettes, accent, caret style/blink/glide and side pane palette. UI: background, tint, scale, toolbar expansion and timing, bar style, pager and menu toggles. Choices are local mock state, not product settings.
- Host: Before/After/Flip, changes, theme picker, wide side-by-side and narrow tabs, open/close Settings, and frame message forwarding. Other pages are supplied by their own scripts.
- These behaviors are implemented but have not been exercised in a browser under the offline-only instruction.

## 3. Snap rows wired
- 1: nav rail horizontal offset, 18 to 16 (expressed as an offset from the captured 18 baseline).
- 2: rail margins, 10/4/10/10 to 12/4/12/12.
- 3: nav item left padding, 10 to 12.
- 4: page right margin, 28 to 32.
- 5: caret style choice spacing, 6 to 8.
- 6: Theme/UI dropdown list gap, 1 to 2.
- 7: Theme/UI picture tile label gap, 5 to 4.
- 19: Fluent search field radius, 3 to 4; other Fluent controls inherit the shared token.

## 4. Captures and remaining differences
- `settings-Theme-0`: inspected; Acrylic is a static approximation, caret icons and picture artwork are simplified, and exact rendering, layout, hover and type metrics are unverified.
- `settings-Theme-1` through `settings-Theme-5`: inspected; these show the same lower page. Same limitations; browser scroll position and pixel parity are unverified.
- `settings-UI-0`: inspected; the shared material art is simplified; shared control geometry, Acrylic and pixel parity are unverified.
- `settings-UI-1`, `settings-UI-3`, `settings-UI-4`, `settings-UI-5`: inspected; these show the same lower page. Control appearance, scroll positions and pixel parity are unverified.
- `settings-UI-2`: inspected; this file captures the desktop/chat rather than the Settings page, so it cannot serve as a Settings reference.
- `dropdown-tint-popup-light-default.png`: inspected; the shared dropdown uses an approximate light appearance and its face height differs from this older capture. No browser comparison was allowed.

## 5. Helpers
- Local page helpers build Fluent icons, snap-marked dropdowns, and remembered expander states; local shell supplies search, navigation and page context. Host handles frame routing and responsive tabs.

## 6. Checks
- `node --check` on `settings-a.js` and all extracted inline scripts; static ID references checked against the two HTML files. No browser, server, app launch or visual interaction test. Python contact-sheet attempt could not run because Pillow is absent; captures were inspected individually.
