# A35d-spacing-tokens lab 3: Settings fix

## 1. Files written
- `tools/playground/a35d-lab/core.css`, `settings.html`, `settings-a.js`, `index.html`; this report. `core.js` and the other assigned page scripts needed no changes.

## 2. Controls that work
- Existing Settings interactions remain wired; tint selection now updates the backdrop, including solid mode. Host mode, theme, change highlights and open/close Settings messaging remain wired. Pad frame tracks pad and control dimensions, including split width.

## 3. Snap rows wired
- Existing Settings rows: 1 nav offset 18/16, 2 rail side 10/12 and bottom 10/12, 3 item inset 10/12, 4 page right 28/32, 5 control spacing 6/8, 6 dropdown item gap 1/2, 7 picture labels 5/4, 8 picture pens 1.1/1 and 1.4/1, 16 preview gaps 14/16, 6/8, 5/4, 17 preview pen 1.8/2, 19 Fluent radius 3/4. Existing wiring retained; no new snap values.

## 4. Capture comparison and numbered findings
1. Acrylic: radial warm background with lighter top left and the Settings source's `max(0.86, tint)` canvas overlay; solid is opaque. It is a static approximation of the compositor. The gradient and tint still need a browser comparison against the captures.
2. Caret style and Blink now reproduce the source's drawn icons; Glide already uses the source's `0xe0cd` bundled Fluent glyph. Text width is intentionally blank in both `settings-Editor-1` and `SettingsWindow.cs`, as are other `Row(null, ...)` rows; adding glyphs there would differ from the capture. Fluent row codepoints were checked against source.
3. Mode now draws the half-filled circle; Scale and Toolbar also use the source's drawn geometry instead of unrelated font glyphs. Exact antialiasing remains unverified.
4. Host displays both windows together, wraps them on narrow screens and uses page scrolling; the Settings iframe is 900 x 720, and the pad iframe includes its 460 x 562 current lab window, 24 px body padding and lab controls, with resize observation for split mode. Browser sizing and scrollbar behavior remain unverified.
5. Compared Settings row text, order, control choices and defaults with `SettingsWindow.cs` and `Settings.cs`; no further edits made. Capture inspection covered `settings-Theme-0`, `settings-Theme-1` (also identical Theme 2–5), `settings-Editor-1`, and `settings-About-0`. Their remaining visual differences include compositor and drawn-icon antialiasing, and the preview is an approximation. Other capture images were not individually inspected; full visual matching, all capture states, and interactive behavior are unverified.

## 5. Helpers
- Local SVG `drawn` helper recreates Mode, caret, Blink, Scale and Toolbar from `SettingsParts.cs`. Host uses `ResizeObserver` to accommodate the pad controls and split mode.

## 6. Checks run
- `node --check` passed for `core.js`, `settings-a.js`, `settings-b.js`, `settings-c.js` and extracted inline scripts in `index.html` and `settings.html`. DOM IDs used by the changed host and Settings code were checked against markup. No browser, server or app was launched. Python fontTools and Pillow are unavailable; no font inspection or generated contact sheet was performed.
