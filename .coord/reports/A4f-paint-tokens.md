# A4f-paint-tokens

## Summary
Centralized the planned paint strengths by role and replaced their inline uses with the same values. The two motion start opacities remain inline as planned.

## Change list
| File | Literals swapped (count) | Tokens used |
|---|---:|---|
| Theme/Paint.cs | 0 | Defines the planned paint tokens |
| UI/SettingsParts.cs | 14 | IconInkAlpha, CardSurface, RowWash, Selected, HoverFace, CardBorderOn, CardBorderOff, CaretBlock, SampleGradientAlpha, SampleFadeAlpha, GridPen, PreviewBar, PreviewPad |
| UI/SearchOverlay.cs | 6 | FieldBorder, ResultSurface, ResultBorder, Dimmed, ResultCurrent |
| UI/Kit.cs | 6 | PinnedGlyphAlpha, Hover, Press, Selected, FocusBorder, RestBorder |
| UI/PadMenu.cs | 5 | PopupBorder, MenuCaption, Hover, Press, MenuSeparator |
| UI/PadWindow.Layout.cs | 3 | Toast, GlassScrim, PopupBorder |
| UI/MotionPreview.cs | 3 | GlassPreview, PreviewFrame, PreviewWell |
| UI/SettingsWindow.cs | 1 | Selected |
| UI/HelpOverlay.cs | 1 | HelpWash |
| Editor/Renderers.cs | 4 | CompletionFill, BadgeFill, KeyPen, AccentPen |
| Editor/PadView.cs | 2 | ErrorHint |
| Editor/Structure.cs | 2 | MarkerFaint, MarkerStrong |
| Editor/Motion.cs | 1 | CaretBlock |
| tools/design-tokens-baseline.tsv | 0 | Refreshed counts |

## Behavior changes
None.

## Out-of-scope files
None.

## Plan deviations
The check build folder remains: automatic approval review rejected the recursive cleanup command. The gate cleaned its own task folders.

## Contracts
None.

## Lens review
Self-reviewed using the lens-review checklist as the implementation worker. Compared every changed value and both halves of each pair, including hover, press, focus, editor drawing and save dialog; reviewed scope, state, timing, edge cases, cleanup, failure paths, layering and tests. No proven findings; behavior and scope match the plan. Qualified two references to avoid existing `Paint` name collisions.

## Checks run
- Offline build: 0 warnings, 0 errors. An initial build before resolving a local name collision failed and attempted NuGet audit; final build disabled audit for offline operation.
- Selftest: `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160`; `SELFTEST PASS`; Theme to UI and Editor: 0 refs.
- Tokens: alpha 5 to 0; opacity 45 to 2. The remaining sites are PadView note-arrival and PadWindow content-reveal animation starts.
- Gate: PASS (Build, Selftest, Seam, Tokens, Format, Scope, Size and gate Cleanup; LoopLint skipped). See gate report. First run failed Scope due to temporary inventory files; removed before rerun.

## Hand checks for the user
In a dev build, in dark and light themes: hover and press a menu item and Settings button; tab into a text box; open search and arrow through results; inspect a Settings card, theme cards and caret preview; open help; show a toast; inspect bracket guides and block caret in a note. Confirm nothing looks different.
