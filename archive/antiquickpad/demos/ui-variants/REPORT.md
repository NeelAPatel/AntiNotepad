# UI variants: what Avalonia + AvaloniaEdit can do

All images in `out/` are real Avalonia 12.1.4 / AvaloniaEdit 12.0.0 renders (Skia, `RenderTargetBitmap` of the window's visual tree). No web mockups, no image editing. Same fixture in all (H1-H3, bold/italic/strike with visible markers, `- [ ]`/`- [x]`, python fence, one long wrapped line; plus a `#` comment line in the fence).

## Honest limits (apply to every image)
- **Backdrop is a plain gradient brush I painted behind the panel** to stand in for a desktop. It is not acrylic/Mica/blur; that path needs the real compositor and is out of scope (de-prioritized). Panels are opaque or near-opaque.
- **Rounded 12px window corners are a `Border` + `ClipToBounds` inside a window** (shadow is a real `BoxShadow`). On a real borderless window the OS window rectangle is still square; a true rounded window needs a transparent window (`TransparencyLevelHint = Transparent`), which is the part both spikes found flaky. Not verified here.
- **Heading "gradient" = per-level colors** (H1 warm to H3 cool), via `DocumentColorizingTransformer`. A per-glyph smooth gradient inside one heading was not attempted (Avalonia can use a `LinearGradientBrush` as foreground, but it is a per-run brush, so it would not give clean text gradients across a line).
- **Line height / leading could not be changed.** I tried `TextBlock.LineHeight` on the editor: no effect, AvaloniaEdit lays lines out at the font's natural height. The Slate "airy" feel comes from font, size and padding only. Real leading would need a custom `TextView` change (hard).
- Interaction (checkbox click hit-testing, fold toggling) was not exercised: these are still frames. The checkbox graphic is an inline control, so spike A's click-to-toggle would need re-pointing at it (easy-medium).
- Code panel is inset only 2px, text sits flush to its left edge: AvaloniaEdit has no per-line padding, so no inner padding in the panel (rough, not fixable cheaply).
- Fixed-colour palettes, one machine (renderScaling 1), no other DPI checked.

## Variants

| Variant | Palette / type | What was done and which mechanism | Effort | Rough / not possible |
|---|---|---|---|---|
| **Ink** (`ink.png`) | Dark slate, warm to cool headings. Segoe UI Variable Text 17 + Cascadia Code in fence | Heading colors/weights, dimmed markers: `DocumentColorizingTransformer`. Code panel: `IBackgroundRenderer` on Background layer. Gutter: custom `AbstractMargin` (11px, 30% white, right aligned, 1px separator). 12px panel + 16px padding: `Border`/`Padding` | Colors easy; custom gutter medium; code panel medium | Checkboxes still `[ ]` text (accent colored). No leading control. |
| **Paper** (`paper.png`) | Warm cream, brown headings. Georgia 19 + Cascadia Mono in fence | Line numbers off (`ShowLineNumbers=false`, no custom margin). Real rounded-square checkboxes with checked state + checkmark: `VisualLineElementGenerator` returning `InlineObjectElement` with a `Border`/`Path`. | Colors easy; checkbox graphic medium | The inline object raises the line height of checkbox rows slightly (spacing uneven); dash bullet baseline sits a bit above the box. Needs hit-testing rewiring. |
| **Terminal** (`terminal.png`) | Black, saturated headings (yellow/cyan/magenta). Cascadia Mono everywhere | Prominent 15px numbers + current-line row highlight (custom gutter row fill plus `Options.HighlightCurrentLine`/`CurrentLineBackground`). Fold markers via `FoldingManager` (default square +/- box, brush `FoldingMarkerBrush` set; guide lines come from the stock margin). Distinct code panel (`IBackgroundRenderer`). | Medium (gutter and fold) | Fold marker styling is limited to brushes; stock box shape remains (white box looks harsh, hard to restyle without replacing `FoldingMargin`). |
| **Slate** (`slate.png`) | Cool blue-grey, low-contrast, monochrome-blue headings. Segoe UI Variable Display 18 | Same machinery as Ink with different `Look`; no gutter rule, 24px padding, subdued numbers. | Easy | No large line height (see limits), so it is "calm" more than "airy". |
| **Midnight** (`midnight.png`) | Near-black, single muted gold accent (headings, bullets, keywords). Cascadia Code 15 | Same machinery; hairline gutter (10px numbers at 18% white), accent reused for H1-H6 in fading gold. | Easy | Very low contrast markers/numbers: legibility on a real display unverified. |
| **Carousel** (`carousel.png`) | Ink look, two notes | Two `TextEditor`s in cards on a `Canvas`; current card offset by -120px, next card at 72% opacity peeking in from the right, 10px rounded cards. Static frame of the offset; the slide animation is spike A's `TranslateTransform` timer. | Easy (geometry); animation already exists in spike A | Frame is composed by setting the offset, not by capturing a live animation. |

Fonts used (all ship with Windows 11): Segoe UI Variable Text/Display, Georgia, Cascadia Code, Cascadia Mono.

## Swapping a theme
All palette, type and feature switches live in one record, `Look` (`Looks.cs`). A theme is one entry in `Look.All`; the window, colorizer, gutter, code panel and checkbox generator all read from the chosen `Look` (`--render <name>`). Switching theme at runtime means rebuilding the editors' transformers/margins with another `Look` (small), or moving the colors to a resource dictionary (`DynamicResource`) for hot swap, which I did not do.

## Verdict
Polished, typography-led, chrome-free looks are achievable with styles/transformers/background renderers/custom margins without leaving the stack. Hard walls: leading, per-line inner padding in the code panel, real rounded transparent window, restyling the stock fold margin beyond brushes.
