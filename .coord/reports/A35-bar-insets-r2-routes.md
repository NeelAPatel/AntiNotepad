# A35-bar-insets: pill routes and chrome tokens

## Area 1 result

- The pill inset is 4 px. Derived pill height is 42 px and the docked pager strip is 50 px; the headless geometry assertion now expects these values.
- `ChromeTokens` now owns the bar and pager spacing, badge, count, separator, dot, divider, border, shadow, hover-radius and font measurements used in `PadWindow.Layout.cs` and `Kit.IconButton`.
- Every style and window Material defaults to an opaque palette Surface pill. The dev-only Lab selector has only Solid and Blur behind. Blur is used only for Overlay on GPU rendering; all other cases keep the opaque Surface.
- Overlay places the pill in `_bottomLayer`, so its existing slide animation applies to the pill itself. Float keeps the `Bottom` popup, its 8 px `VerticalOffset`, and the existing pill opacity fade. The popup requests Transparent, uses a transparent fallback, and uses `Native.ConfigurePopupWindow` to keep the client-drawn 12 px corners clear of native popup rounding/borders.
- Native acrylic/mica pill requests, native material fallback routing, custom overlay popup placement, popup opacity synchronization, `Native.ConfigureMaterialPopupWindow`, top-backdrop selector, and A35 blur/hotkey lab telemetry were removed. The ordinary hotkey-to-focus log remains.
- Top blur is the default for sliding bars. Software rendering uses an opaque Canvas brush through the same feather mask; this fallback is provisional for the next blur pass. The existing VisualBrush sampling/update mechanism was kept intact for that pass.

## Files changed

- `product/AntiNotepad/UI/Chrome.cs`, `PadWindow.Layout.cs`, `PadWindow.MaterialLab.cs`, `Kit.cs`, `SettingsWindow.cs`, `PadWindow.cs`
- `product/AntiNotepad/Platform/Native.cs`, `product/AntiNotepad/SelfTest.cs`
- `.coord/STATUS.md`

## Checks and remaining limits

- `git diff --check` reported no whitespace errors. No build, selftest, app launch, or live visual check was run in this area; the manager owns those gates.
- Software fallback appearance, blurred glyph behavior/freshness, popup corners/focus/z-order, and live placement/fade still need the manager's dev-copy review. The blur sampling implementation was intentionally left for the next worker.
