# AvaloniaEdit fork patches

Upstream version: 12.0.0  
Upstream commit: `86fdebec4cff7affb0e14c7885df71d28edce777`  
License: MIT; the upstream `LICENSE` remains beside the source and ships as `Licenses/AvaloniaEdit-LICENSE.txt`.

The Avalonia 12 formatter ignores paragraph indentation. A22 reduces the available width for each continuation line and applies the same measured offset to drawing and coordinate conversion, so text, caret, selection, and hit testing share one layout.

The vendored project targets `net10.0`, matching AntiNotepad. Its `Avalonia` project dependency is pinned to the host's existing `12.1.4`; assembly signing is disabled for the local source project. The local `.gitignore` keeps project build outputs out of the vendored source changes.

## Patched methods

- `Rendering/TextView.cs` — `WrapIndentationProvider` supplies the source offset for the first text character. `BuildVisualLine` maps that offset after visual element generation, measures its actual width, and formats continuation lines in the remaining width. `CreateAndMeasureVisualLines` includes the offset in the measured line width.
- `Rendering/VisualLine.cs` — stores the measured continuation offset. `GetTextLineVisualXPosition` applies it to text positions; `GetVisualColumn` and `GetVisualColumnFloor` subtract it before hit testing; `VisualLineDrawingVisual.Render` draws each continuation at that offset.
- `Editing/Caret.cs` — `CalcCaretOverstrikeRectangle` shifts text bounds that bypass the shared X helper.
- `Rendering/BackgroundGeometryBuilder.cs` — `ProcessTextLines` shifts selected text bounds and the continuation extent used when selections cross a wrapped line.

## Source markers

Every `AntiNotepad patch A22` source marker is listed here and checked by the `avaloniaedit-fork-documented` selftest contract.

```text
// AntiNotepad patch A22: provide the source text offset that continuation lines should align with.
// AntiNotepad patch A22: include the continuation offset in the measured line width.
// AntiNotepad patch A22: resolve the text anchor after generators have mapped source text to visual columns.
// AntiNotepad patch A22: reserve measured prefix width before formatting each continuation.
// AntiNotepad patch A22: keep continuation layout and coordinate mapping on the same measured offset.
// AntiNotepad patch A22: translate caret, selection and renderer positions with drawn text.
// AntiNotepad patch A22: undo the continuation translation before resolving a hit.
// AntiNotepad patch A22: floor hit testing uses the same translated origin as drawing.
// AntiNotepad patch A22: draw wrapped lines at the measured text anchor.
// AntiNotepad patch A22: overstrike bounds come from unshifted TextLine coordinates.
// AntiNotepad patch A22: selection bounds use unshifted TextLine coordinates.
// AntiNotepad patch A22: include continuation origin when a selection extends past wrapped text.
```

Upgrade instructions are in [`docs/dependencies.md`](../../docs/dependencies.md).
