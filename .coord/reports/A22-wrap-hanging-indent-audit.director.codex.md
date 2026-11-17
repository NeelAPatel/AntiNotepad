## 1. Kind and why

**Bug: missing visual hanging indentation.** The requested behavior is explicit: every wrapped bullet continuation must align with its text, at every nesting level. Document text and editing behavior must remain unchanged. [Request:12–14](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.coord/tasks/A22-wrap-hanging-indent.md:12>).

## 2. Now

**Code-derived behavior on dev; no app launched or screenshot independently inspected:**

| Line type | First visual line | Soft-wrapped continuations |
|---|---|---|
| Nested bullets, levels 1–3 | Leading whitespace, then `•` and text | Editor text-area left edge |
| Top-level bullet | `•` and text | Left edge, underneath the marker column |
| Plain indented line | Leading whitespace and text | Left edge |
| Numbered list | Literal number/punctuation and text | Left edge |
| Checkbox | Leading whitespace, checkbox replacement and text | Left edge |

Wrapping is enabled in [PadView.cs:93](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/Editor/PadView.cs:93>). Bullets replace **only one marker character**; whitespace remains ordinary text ([Elements.cs:86–102](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/Editor/Elements.cs:86>)). Checkboxes replace their marker span with a glyph element ([Elements.cs:62–69](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/Editor/Elements.cs:62>)); numbered prefixes are ordinary text ([ListEditing.cs:9](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/Editor/ListEditing.cs:9>)).

**The screenshot description is consistent with broken library indentation, not lost leading spaces.** Both library barriers are established below.

## 3. Regression?

**No earlier hanging-indent implementation or removal found.** All-ref `git log -S` searches for `WordWrapIndentation`, `InheritWordWrapIndentation`, and `HangingIndent` returned no commits, including archived history. Broader editor wrapping/paragraph searches found wrapping setup, without a hanging-indent implementation.

The archive tag’s `PadView.cs` also has `WordWrap = true` and no wrap-indent configuration. Following the csproj through renames found AvaloniaEdit **12.0.0** already present in `870460d`’s `product/Antipad/Antipad.csproj:19`; the archive and current project retain that version ([AntiNotepad.csproj:43](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/AntiNotepad.csproj:43>)).

**No dependency-version rollback or app change undoing a fix was identified.** “Previously fixed” remains unsubstantiated; this is not proof that every historical executable behaved identically.

## 4. Cause

**Two independently proven library barriers.** Cached packages lack source/PDB/XML, so binary evidence uses method tokens and IL offsets instead of invented source line numbers. Cached DLLs match `build/dev` by SHA-256.

**AvaloniaEdit 12.0.0: indentation calculation is unreachable.** Equivalent code reconstructed from IL:

```csharp
// BuildVisualLine, token 0x0600051D, IL_014E onward
if (paragraph.firstLineInParagraph)
{
    paragraph.firstLineInParagraph = false;
    // Compute inherited whitespace width plus WordWrapIndentation.
}
```

`VisualLineTextParagraphProperties`’ constructor (`0x06000624`) never initializes that Boolean; `CreateParagraphProperties` (`0x0600051C`) never sets it. The sole field write anywhere in this DLL sets it **false**. It therefore starts false and skips the calculation.

Meanwhile, `TextEditorOptions`’ constructor (`0x06000114`, IL_006F–0071) sets **`InheritWordWrapIndentation = true`**. The scout’s implication that an omitted option explains everything is incorrect.

The intended calculation scans `VisualLine.Elements` using `IsWhitespace` (`GetIndentationVisualColumn`, `0x0600051E`), then measures the resulting column with `GetDistanceFromCharacterHit`. Thus leading spaces **should** inherit their measured width in that intended path; it would still stop before the bullet.

**Avalonia 12.1.4: paragraph indent is not consumed.** In `Avalonia.Base.dll`, calls to `get_Indent` occur only while copying paragraph properties. `TextFormatterImpl.FormatLine` (`0x0600280C`) and `TextLineImpl.GetParagraphOffsetX` (`0x0600288E`) do not apply it; left alignment returns zero.

**Consequently, neither setting the options nor repairing the Boolean alone is sufficient.** Versions: [AntiNotepad.csproj:41–43](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/AntiNotepad.csproj:41>).

## 5. Where and smallest fix

**Required behavior:** identify an eligible bullet’s text-start document offset, map it to its generated visual column, and measure that position on the first formatted line. This includes leading whitespace, the actual `•` advance, and following whitespace. Do not multiply character count by a nominal space width: fonts are configurable ([PadView.cs:189–190](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/Editor/PadView.cs:189>)), and the replacement bullet has its own formatted width ([Elements.cs:34–39,101](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/Editor/Elements.cs:34>)).

Likely product files:

- New `Editor/WrapIndent.cs`: eligibility and text-start computation.
- `Editor/PadView.cs`: registration.
- `SelfTest/Editor.cs` and `SelfTest/Contracts.cs`: checks and contract.
- `AntiNotepad.csproj` plus a bounded dependency patch **if** that is the selected integration route.

**A supported product-only paragraph hook was not found.** AvaloniaEdit’s `CreateParagraphProperties` and `BuildVisualLine` are private, nonvirtual; its paragraph-properties type is internal/sealed. The smallest maintainable integration is therefore **not yet proven**. A bounded formatting patch or adapter needs a probe before fixing the Files list.

That integration must reduce continuation wrapping width and expose the offset through drawing, hit-testing, caret positions, and selection geometry. A drawing-only translation would be unsafe: existing caret and link handling consume those coordinates ([Motion.cs:302–303](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/Editor/Motion.cs:302>), [PadView.cs:769–788](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/Editor/PadView.cs:769>)).

`IndentGuideRenderer` should retain its existing nesting columns and full visual-line height ([Structure.cs:458–475](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/Editor/Structure.cs:458>)). Measure after element generation and colorizers, respecting font-size transformations; heading scaling currently changes run size ([Colorizers.cs:69–79](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/Editor/Colorizers.cs:69>)).

## 6. Testability

**Partly pure, partly layout-dependent.**

A pure helper can return the eligible bullet’s text-start offset. Check `-`, `*`, `+`; nesting levels 1–3; tabs and multiple spaces; empty items; checkbox exclusion; numbered/plain-line exclusion; and code/fence exclusions.

Actual proportional-font width and coordinate agreement require a **windowless layout capability check**, not a pure test. Existing precedent measures and arranges a TextView in [SelfTest/Editor.cs:86–95](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/product/AntiNotepad/SelfTest/Editor.cs:86>).

The probe should verify every continuation’s X equals the first text character’s X at several widths/fonts/sizes, with unchanged document text and correct position/hit-test round trips. Include adjacent bullets with different nesting to catch indentation leaking between paragraphs.

## 7. Risk and tier

**Confirm Medium**, with greater implementation uncertainty than an option-only fix. This remains an editor feature under [docs/loop.md:32](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/docs/loop.md:32>).

Principal risks are geometry disagreement, excessive indentation at narrow widths, and changing unrelated line types. A dependency patch requires explicit review and the layout probe before implementation approval.

## 8. Recommendation

Keep **bullets only** as the default scope; preserve numbered lists, checkboxes, and plain indented lines until the user explicitly includes them. That matches [Request:13](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.coord/tasks/A22-wrap-hanging-indent.md:13>). Explicitly correct top-level bullets too: they currently lack text-column alignment.

Before Plan/Approve, prove a bounded formatting integration that handles wrapping width and all coordinates together. The requested bullet alignment can be treated as a bug correction; expanding other line types introduces a scope/design decision ([Request:16](</F:/LocalVSProj/Visual Studio Apps/AntiQuickPad/.coord/tasks/A22-wrap-hanging-indent.md:16>)).

**READY: no - cause is proven; the smallest maintainable formatting integration still needs a windowless layout probe.**