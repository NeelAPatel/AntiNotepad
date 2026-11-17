# A22-wrap-hanging-indent: wrapped lines of a bullet stay under its text

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: High (raised 2026-11-17: vendored dependency) · editor-feature (bug without a proven cause; item chat confirms in Findings)
State: request
Branch: `fix/A22-wrap-hanging-indent` in `.claude/worktrees/fix/A22-wrap-hanging-indent` (created at Approve, from dev's tip)

## Request
User, 2026-10-29: when an indented bullet line soft-wraps, the wrapped part should start under the bullet's text, not under the bullet; wanted: the same alignment as a typed new line, just without the bullet. User, 2026-11-16 (screenshot, nested bullets): wrapped lines of "the multiple subc..." start at the left edge ("background subtasks re...", "response ready") instead of staying indented under the bullet's text; "we fixed this before somewhere I think". Numbered A22 on the user's go, 2026-11-16.

- **Goal:** a soft-wrapped continuation of a bullet line, at any indent level, lines up under the first character of that bullet's text.
- **Done check:** (1) nested bullets at levels 1-3 wrap with every continuation line at the text column of its bullet, at several window widths; (2) top-level bullets still wrap correctly; (3) numbered lists, checkboxes and plain indented lines also wrap to their text column (user, 2026-11-17); (4) caret movement, selection and the text itself are unchanged (only the visual wrap).
- **Proof:** a before and after screenshot of the user's example text at one narrow width; a selftest check on the indent computation if the logic is testable; the Findings answer to "was this fixed before, and what undid it".
- **What we know:** reported 2026-10-29 and again 2026-11-16; the user thinks it was fixed once, so look for a regression first (git history of the editor's wrapping/indent code); unknown cause, no theory. Editor code lives in `Editor/` (PadView, colorizers, renderers).
- **Constraints:** Medium tier; lab-first only if the look is a design choice (a pure bug fix may skip the lab: say so in the Plan); dev copy only; one Codex run unless the plan says more. Branch from dev's latest commit (`git rev-parse dev` at Approve). This item merges only after `A21-save-delay-setting` has merged to dev (director merges in that order); catch up to dev before merge.

## Findings
Codex audit `.coord/reports/A22-wrap-hanging-indent-audit.director.codex.md` (read-only, from the library binaries).
- **Kind:** bug. Wrapped lines of every bullet go to the editor's left edge.
- **Fixed before?** No. No wrap-indent code or setting in dev or the pre-rewrite history; AvaloniaEdit 12.0.0 was always used. Nothing undid a fix.
- **Cause (proven, two library barriers):** (1) AvaloniaEdit 12.0.0 never sets the flag that lets its wrap-indent calculation run. (2) Avalonia 12.1.4's text formatter ignores the paragraph indent. Our own indent option is already on, so setting options alone cannot fix it.
- **No supported hook:** the paragraph code is private/internal; the smallest safe integration is unproven.
- **Risk:** the indent must reach drawing, caret, selection and hit-testing together, and use the real measured width of the bullet glyph (fonts are configurable).

## Plan
Fork (user, 2026-11-17: "Go", self-documented and maintained). Size and patch points: `.coord/reports/A22-wrap-hanging-indent-forksize.worker.codex.md`.
Rule (all line types): a soft-wrapped continuation starts where a typed new line's text would. Bullet/checkbox: after the marker. Numbered: after `1. `. Plain indented: own indent. Unindented plain text: unchanged. Parsing reuses `Editor/ListEditing.cs`.
- **Done in commit 69a2ecf:** AvaloniaEdit 12.0.0 vendored verbatim in `third_party/AvaloniaEdit/` (`UPSTREAM.txt`).
- **A22b (Codex worker):** (1) csproj `ProjectReference` replaces the package (no signing, vendored warnings silenced, build 0 warnings). (2) Patch `TextView.BuildVisualLine` (continuation width minus indent, per-line indent) and the X helpers Codex listed: draw, `GetTextLineVisualXPosition`, `GetVisualColumn` (all overloads incl. floor), caret and selection follow. (3) Seam: a `Func` on `TextView` that returns the indent text offset for a visual line; AntiNotepad's `Editor/WrapIndent.cs` supplies it from `ListEditing` and measures with the real glyph widths. (4) `third_party/AvaloniaEdit/PATCHES.md` (version, commit, licence, why, each patched method and reason), `// AntiNotepad patch A22:` markers, `docs/dependencies.md` upgrade recipe, selftest that fails if markers or pinned version drift. (5) Selftests: pure helper, windowless layout check (continuation X = text X at several widths and fonts; text unchanged; caret/selection/hit-test round trip).
No lab: "same as typing" is the design. Avoids `PadWindow*`, Settings, Store (A21).

## Files
All changed files, exact paths (the gate matches paths, not directories). `third_party/AvaloniaEdit/` is the vendored library.
- `docs/dependencies.md`
- `product/AntiNotepad/AntiNotepad.csproj`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/Editor/Renderers.cs`
- `product/AntiNotepad/Editor/WrapIndent.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Editor.cs`
- `third_party/AvaloniaEdit/.gitignore`
- `third_party/AvaloniaEdit/AvaloniaEdit.csproj`
- `third_party/AvaloniaEdit/AvaloniaEditCommands.cs`
- `third_party/AvaloniaEdit/CodeCompletion/CompletionAcceptAction.cs`
- `third_party/AvaloniaEdit/CodeCompletion/CompletionList.cs`
- `third_party/AvaloniaEdit/CodeCompletion/CompletionList.xaml`
- `third_party/AvaloniaEdit/CodeCompletion/CompletionListBox.cs`
- `third_party/AvaloniaEdit/CodeCompletion/CompletionTipContentControl.cs`
- `third_party/AvaloniaEdit/CodeCompletion/CompletionTipContentControl.xaml`
- `third_party/AvaloniaEdit/CodeCompletion/CompletionWindow.cs`
- `third_party/AvaloniaEdit/CodeCompletion/CompletionWindowBase.cs`
- `third_party/AvaloniaEdit/CodeCompletion/ICompletionData.cs`
- `third_party/AvaloniaEdit/CodeCompletion/IOverloadProvider.cs`
- `third_party/AvaloniaEdit/CodeCompletion/InsightWindow.cs`
- `third_party/AvaloniaEdit/CodeCompletion/InsightWindow.xaml`
- `third_party/AvaloniaEdit/CodeCompletion/OverloadInsightWindow.cs`
- `third_party/AvaloniaEdit/CodeCompletion/OverloadViewer.cs`
- `third_party/AvaloniaEdit/CodeCompletion/PopupWithCustomPosition.cs`
- `third_party/AvaloniaEdit/Document/DataObjectCopyingEventArgs.cs`
- `third_party/AvaloniaEdit/Document/DocumentChangeEventArgs.cs`
- `third_party/AvaloniaEdit/Document/DocumentChangeOperation.cs`
- `third_party/AvaloniaEdit/Document/DocumentChangedEventArgs.cs`
- `third_party/AvaloniaEdit/Document/DocumentLine.LineNode.cs`
- `third_party/AvaloniaEdit/Document/DocumentLine.cs`
- `third_party/AvaloniaEdit/Document/DocumentLineTree.cs`
- `third_party/AvaloniaEdit/Document/DocumentTextWriter.cs`
- `third_party/AvaloniaEdit/Document/IDocument.cs`
- `third_party/AvaloniaEdit/Document/ILineTracker.cs`
- `third_party/AvaloniaEdit/Document/ITextAnchor.cs`
- `third_party/AvaloniaEdit/Document/ITextSource.cs`
- `third_party/AvaloniaEdit/Document/IUndoableOperation.cs`
- `third_party/AvaloniaEdit/Document/LineManager.cs`
- `third_party/AvaloniaEdit/Document/NewLineFinder.cs`
- `third_party/AvaloniaEdit/Document/OffsetChangeMap.cs`
- `third_party/AvaloniaEdit/Document/RopeTextSource.cs`
- `third_party/AvaloniaEdit/Document/SimpleSegment.cs`
- `third_party/AvaloniaEdit/Document/TextAnchor.cs`
- `third_party/AvaloniaEdit/Document/TextAnchorNode.cs`
- `third_party/AvaloniaEdit/Document/TextAnchorTree.cs`
- `third_party/AvaloniaEdit/Document/TextDocument.cs`
- `third_party/AvaloniaEdit/Document/TextDocumentWeakEventManager.cs`
- `third_party/AvaloniaEdit/Document/TextLocation.cs`
- `third_party/AvaloniaEdit/Document/TextSegment.cs`
- `third_party/AvaloniaEdit/Document/TextSegmentCollection.cs`
- `third_party/AvaloniaEdit/Document/TextSourceVersionProvider.cs`
- `third_party/AvaloniaEdit/Document/TextUtilities.cs`
- `third_party/AvaloniaEdit/Document/UndoOperationGroup.cs`
- `third_party/AvaloniaEdit/Document/UndoStack.cs`
- `third_party/AvaloniaEdit/Document/WeakLineTracker.cs`
- `third_party/AvaloniaEdit/Editing/AbstractMargin.cs`
- `third_party/AvaloniaEdit/Editing/Caret.cs`
- `third_party/AvaloniaEdit/Editing/CaretLayer.cs`
- `third_party/AvaloniaEdit/Editing/CaretNavigationCommandHandler.cs`
- `third_party/AvaloniaEdit/Editing/CaretWeakEventHandler.cs`
- `third_party/AvaloniaEdit/Editing/DottedLineMargin.cs`
- `third_party/AvaloniaEdit/Editing/DragDropException.cs`
- `third_party/AvaloniaEdit/Editing/EditingCommandHandler.cs`
- `third_party/AvaloniaEdit/Editing/EmptySelection.cs`
- `third_party/AvaloniaEdit/Editing/HotkeyConfiguration.cs`
- `third_party/AvaloniaEdit/Editing/IReadOnlySectionProvider.cs`
- `third_party/AvaloniaEdit/Editing/LineNumberMargin.cs`
- `third_party/AvaloniaEdit/Editing/NoReadOnlySections.cs`
- `third_party/AvaloniaEdit/Editing/RectangleSelection.cs`
- `third_party/AvaloniaEdit/Editing/Selection.cs`
- `third_party/AvaloniaEdit/Editing/SelectionColorizer.cs`
- `third_party/AvaloniaEdit/Editing/SelectionLayer.cs`
- `third_party/AvaloniaEdit/Editing/SelectionMouseHandler.cs`
- `third_party/AvaloniaEdit/Editing/SelectionSegment.cs`
- `third_party/AvaloniaEdit/Editing/SimpleSelection.cs`
- `third_party/AvaloniaEdit/Editing/TextArea.cs`
- `third_party/AvaloniaEdit/Editing/TextArea.xaml`
- `third_party/AvaloniaEdit/Editing/TextAreaDefaultInputHandlers.cs`
- `third_party/AvaloniaEdit/Editing/TextAreaInputHandler.cs`
- `third_party/AvaloniaEdit/Editing/TextSegmentReadOnlySectionProvider.cs`
- `third_party/AvaloniaEdit/Folding/FoldingElementGenerator.cs`
- `third_party/AvaloniaEdit/Folding/FoldingManager.cs`
- `third_party/AvaloniaEdit/Folding/FoldingMargin.cs`
- `third_party/AvaloniaEdit/Folding/FoldingMarginMarker.cs`
- `third_party/AvaloniaEdit/Folding/FoldingSection.cs`
- `third_party/AvaloniaEdit/Folding/NewFolding.cs`
- `third_party/AvaloniaEdit/Folding/XmlFoldingStrategy.cs`
- `third_party/AvaloniaEdit/Highlighting/DocumentHighlighter.cs`
- `third_party/AvaloniaEdit/Highlighting/HighlightedLine.cs`
- `third_party/AvaloniaEdit/Highlighting/HighlightedSection.cs`
- `third_party/AvaloniaEdit/Highlighting/HighlightingBrush.cs`
- `third_party/AvaloniaEdit/Highlighting/HighlightingColor.cs`
- `third_party/AvaloniaEdit/Highlighting/HighlightingColorizer.cs`
- `third_party/AvaloniaEdit/Highlighting/HighlightingDefinitionInvalidException.cs`
- `third_party/AvaloniaEdit/Highlighting/HighlightingDefinitionTypeConverter.cs`
- `third_party/AvaloniaEdit/Highlighting/HighlightingEngine.cs`
- `third_party/AvaloniaEdit/Highlighting/HighlightingManager.cs`
- `third_party/AvaloniaEdit/Highlighting/HighlightingRule.cs`
- `third_party/AvaloniaEdit/Highlighting/HighlightingRuleSet.cs`
- `third_party/AvaloniaEdit/Highlighting/HighlightingSpan.cs`
- `third_party/AvaloniaEdit/Highlighting/HtmlClipboard.cs`
- `third_party/AvaloniaEdit/Highlighting/HtmlOptions.cs`
- `third_party/AvaloniaEdit/Highlighting/HtmlRichTextWriter.cs`
- `third_party/AvaloniaEdit/Highlighting/IHighlighter.cs`
- `third_party/AvaloniaEdit/Highlighting/IHighlightingDefinition.cs`
- `third_party/AvaloniaEdit/Highlighting/IHighlightingDefinitionReferenceResolver.cs`
- `third_party/AvaloniaEdit/Highlighting/Resources/ASPX.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/Boo.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/CPP-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/CSS-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/CSharp-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/Coco-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/HTML-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/Java-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/JavaScript-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/Json.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/MarkDown-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/MarkDownWithFontSize-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/ModeV1.xsd`
- `third_party/AvaloniaEdit/Highlighting/Resources/ModeV2.xsd`
- `third_party/AvaloniaEdit/Highlighting/Resources/PHP-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/Patch-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/PowerShell.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/Python-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/Resources.cs`
- `third_party/AvaloniaEdit/Highlighting/Resources/TSQL-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/Tex-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/VB-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/XML-Mode.xshd`
- `third_party/AvaloniaEdit/Highlighting/Resources/XmlDoc.xshd`
- `third_party/AvaloniaEdit/Highlighting/RichText.cs`
- `third_party/AvaloniaEdit/Highlighting/RichTextColorizer.cs`
- `third_party/AvaloniaEdit/Highlighting/RichTextModel.cs`
- `third_party/AvaloniaEdit/Highlighting/RichTextModelWriter.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/HighlightingLoader.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/IXshdVisitor.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/SaveXshdVisitor.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/V1Loader.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/V2Loader.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/XmlHighlightingDefinition.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/XshdColor.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/XshdElement.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/XshdImport.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/XshdKeywords.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/XshdProperty.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/XshdReference.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/XshdRule.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/XshdRuleSet.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/XshdSpan.cs`
- `third_party/AvaloniaEdit/Highlighting/Xshd/XshdSyntaxDefinition.cs`
- `third_party/AvaloniaEdit/Indentation/CSharp/CSharpIndentationStrategy.cs`
- `third_party/AvaloniaEdit/Indentation/CSharp/DocumentAccessor.cs`
- `third_party/AvaloniaEdit/Indentation/CSharp/IndentationReformatter.cs`
- `third_party/AvaloniaEdit/Indentation/DefaultIndentationStrategy.cs`
- `third_party/AvaloniaEdit/Indentation/IIndentationStrategy.cs`
- `third_party/AvaloniaEdit/LICENSE`
- `third_party/AvaloniaEdit/PATCHES.md`
- `third_party/AvaloniaEdit/Properties/InternalsVisibleTo.cs`
- `third_party/AvaloniaEdit/Rendering/BackgroundGeometryBuilder.cs`
- `third_party/AvaloniaEdit/Rendering/CollapsedLineSection.cs`
- `third_party/AvaloniaEdit/Rendering/ColorizingTransformer.cs`
- `third_party/AvaloniaEdit/Rendering/ColumnRulerRenderer.cs`
- `third_party/AvaloniaEdit/Rendering/CurrentLineHighlightRenderer.cs`
- `third_party/AvaloniaEdit/Rendering/DocumentColorizingTransformer.cs`
- `third_party/AvaloniaEdit/Rendering/FormattedTextElement.cs`
- `third_party/AvaloniaEdit/Rendering/GlobalTextRunProperties.cs`
- `third_party/AvaloniaEdit/Rendering/HeightTree.cs`
- `third_party/AvaloniaEdit/Rendering/HeightTreeLineNode.cs`
- `third_party/AvaloniaEdit/Rendering/HeightTreeNode.cs`
- `third_party/AvaloniaEdit/Rendering/IBackgroundRenderer.cs`
- `third_party/AvaloniaEdit/Rendering/ITextRunConstructionContext.cs`
- `third_party/AvaloniaEdit/Rendering/ITextViewConnect.cs`
- `third_party/AvaloniaEdit/Rendering/IVisualLineTransformer.cs`
- `third_party/AvaloniaEdit/Rendering/InlineObjectRun.cs`
- `third_party/AvaloniaEdit/Rendering/Layer.cs`
- `third_party/AvaloniaEdit/Rendering/LayerPosition.cs`
- `third_party/AvaloniaEdit/Rendering/LinkElementGenerator.cs`
- `third_party/AvaloniaEdit/Rendering/PointerHoverLogic.cs`
- `third_party/AvaloniaEdit/Rendering/SimpleTextSource.cs`
- `third_party/AvaloniaEdit/Rendering/SingleCharacterElementGenerator.cs`
- `third_party/AvaloniaEdit/Rendering/TextLayer.cs`
- `third_party/AvaloniaEdit/Rendering/TextView.cs`
- `third_party/AvaloniaEdit/Rendering/TextViewCachedElements.cs`
- `third_party/AvaloniaEdit/Rendering/TextViewWeakEventManager.cs`
- `third_party/AvaloniaEdit/Rendering/VisualLine.cs`
- `third_party/AvaloniaEdit/Rendering/VisualLineConstructionStartEventArgs.cs`
- `third_party/AvaloniaEdit/Rendering/VisualLineElement.cs`
- `third_party/AvaloniaEdit/Rendering/VisualLineElementGenerator.cs`
- `third_party/AvaloniaEdit/Rendering/VisualLineElementTextRunProperties.cs`
- `third_party/AvaloniaEdit/Rendering/VisualLineLinkText.cs`
- `third_party/AvaloniaEdit/Rendering/VisualLineText.cs`
- `third_party/AvaloniaEdit/Rendering/VisualLineTextParagraphProperties.cs`
- `third_party/AvaloniaEdit/Rendering/VisualLineTextSource.cs`
- `third_party/AvaloniaEdit/Rendering/VisualLinesInvalidException.cs`
- `third_party/AvaloniaEdit/Rendering/VisualYPosition.cs`
- `third_party/AvaloniaEdit/RoutedCommand.cs`
- `third_party/AvaloniaEdit/SR.Designer.cs`
- `third_party/AvaloniaEdit/SR.resx`
- `third_party/AvaloniaEdit/SR.zh-Hans.resx`
- `third_party/AvaloniaEdit/Search/ISearchStrategy.cs`
- `third_party/AvaloniaEdit/Search/RegexSearchStrategy.cs`
- `third_party/AvaloniaEdit/Search/SearchCommands.cs`
- `third_party/AvaloniaEdit/Search/SearchPanel.cs`
- `third_party/AvaloniaEdit/Search/SearchPanel.xaml`
- `third_party/AvaloniaEdit/Search/SearchResultBackgroundRenderer.cs`
- `third_party/AvaloniaEdit/Search/SearchStrategyFactory.cs`
- `third_party/AvaloniaEdit/Snippets/IActiveElement.cs`
- `third_party/AvaloniaEdit/Snippets/InsertionContext.cs`
- `third_party/AvaloniaEdit/Snippets/Snippet.cs`
- `third_party/AvaloniaEdit/Snippets/SnippetAnchorElement.cs`
- `third_party/AvaloniaEdit/Snippets/SnippetBoundElement.cs`
- `third_party/AvaloniaEdit/Snippets/SnippetCaretElement.cs`
- `third_party/AvaloniaEdit/Snippets/SnippetContainerElement.cs`
- `third_party/AvaloniaEdit/Snippets/SnippetElement.cs`
- `third_party/AvaloniaEdit/Snippets/SnippetEventArgs.cs`
- `third_party/AvaloniaEdit/Snippets/SnippetInputHandler.cs`
- `third_party/AvaloniaEdit/Snippets/SnippetReplaceableTextElement.cs`
- `third_party/AvaloniaEdit/Snippets/SnippetSelectionElement.cs`
- `third_party/AvaloniaEdit/Snippets/SnippetTextElement.cs`
- `third_party/AvaloniaEdit/TextEditor.cs`
- `third_party/AvaloniaEdit/TextEditor.xaml`
- `third_party/AvaloniaEdit/TextEditorComponent.cs`
- `third_party/AvaloniaEdit/TextEditorOptions.cs`
- `third_party/AvaloniaEdit/TextViewPosition.cs`
- `third_party/AvaloniaEdit/Themes/Base.xaml`
- `third_party/AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml`
- `third_party/AvaloniaEdit/Themes/Simple/AvaloniaEdit.xaml`
- `third_party/AvaloniaEdit/UPSTREAM.txt`
- `third_party/AvaloniaEdit/Utils/BusyManager.cs`
- `third_party/AvaloniaEdit/Utils/CallbackOnDispose.cs`
- `third_party/AvaloniaEdit/Utils/CharRope.cs`
- `third_party/AvaloniaEdit/Utils/CompressingTreeList.cs`
- `third_party/AvaloniaEdit/Utils/Constants.cs`
- `third_party/AvaloniaEdit/Utils/DataObjectEx.cs`
- `third_party/AvaloniaEdit/Utils/DelayedEvents.cs`
- `third_party/AvaloniaEdit/Utils/Deque.cs`
- `third_party/AvaloniaEdit/Utils/ExtensionMethods.cs`
- `third_party/AvaloniaEdit/Utils/FileReader.cs`
- `third_party/AvaloniaEdit/Utils/IFreezable.cs`
- `third_party/AvaloniaEdit/Utils/IServiceContainer.cs`
- `third_party/AvaloniaEdit/Utils/NullSafeCollection.cs`
- `third_party/AvaloniaEdit/Utils/ObserveAddRemoveCollection.cs`
- `third_party/AvaloniaEdit/Utils/PixelSnapHelpers.cs`
- `third_party/AvaloniaEdit/Utils/PropertyChangedWeakEventManager.cs`
- `third_party/AvaloniaEdit/Utils/RichTextWriter.cs`
- `third_party/AvaloniaEdit/Utils/Rope.cs`
- `third_party/AvaloniaEdit/Utils/RopeNode.cs`
- `third_party/AvaloniaEdit/Utils/RopeTextReader.cs`
- `third_party/AvaloniaEdit/Utils/StringSegment.cs`
- `third_party/AvaloniaEdit/Utils/TextFormatterFactory.cs`
- `third_party/AvaloniaEdit/Utils/ThrowUtil.cs`
- `third_party/AvaloniaEdit/Utils/WeakEventManagerBase.cs`
- `third_party/AvaloniaEdit/XmlnsAttributes.cs`
- `third_party/AvaloniaEdit/upstream/Directory.Build.props.txt`
- `third_party/AvaloniaEdit/upstream/Directory.Packages.props.txt`

## Contracts
- All existing rows keep passing.
- New `wrap-indent-text-column` (text unchanged; continuation X = first text X per line type; unindented unchanged; caret/selection/hit-test round trip).
- New `avaloniaedit-fork-documented`: PATCHES.md lists every `AntiNotepad patch A22` marker and the pinned upstream version.

## Approve
User, 2026-11-17: "Approve" (scope: all line types), then "Go" on the documented AvaloniaEdit fork. Plan above is the revised one; user to confirm before the build.

## For Codex
Role: worker (gpt-6-luna xhigh): library internals, many files. Branch checked out: never switch, merge or commit. Read `AGENTS.md`, `docs/coding.md`, this card, `.coord/reports/A22-wrap-hanging-indent-forksize.worker.codex.md`, `third_party/AvaloniaEdit/UPSTREAM.txt`.

Constraints:
- Edit upstream files only at the listed patch points, each marked `// AntiNotepad patch A22: <why>`. No reformatting, no drive-by changes in `third_party/`.
- Build into `build/A22-check` with `AVALONIA_TELEMETRY_OPTOUT=1`; run `--selftest` there; delete the folder. Offline only; no ports, browser or launching the app.
- If `tools/gate.ps1` fails only because `third_party/` is treated as product code, report it; do not edit the gate.
- Do not touch `PadWindow*`, Settings or Store files. Never edit STATUS or this card.

Done means: build 0 warnings, `SELFTEST PASS` with the new checks and CONTRACTS rows, `tools/gate.ps1 -Task A22-wrap-hanging-indent -Offline` passes (or the one reported exception).

Report `.coord/reports/A22-wrap-hanging-indent.md` (overwrite): Summary; Change list; Behavior changes; Out-of-scope files; Plan deviations; Contracts; Checks run; Hand checks for the user.

## Check
- **Gate (online, at the final tip):** see the gate report; Build 0 warnings, SELFTEST PASS (contracts incl. `wrap-indent-text-column`, `wrapped-result-chip-text-end`, `avaloniaedit-fork-documented`), Seam, Tokens, Format PASS. Size WARN: `SelfTest/Editor.cs` 490 to 635 lines (planned layout checks). Scope: the Files section now lists every vendored path, so Scope matches exact paths (earlier runs failed only because the entry was a directory).
- **Independent review (reviewer agent, 2026-11-17):** layout math sound, vendored copy unmodified at the first commit, 12 patch markers all listed in `PATCHES.md`. Findings fixed in r2: Format failure, result chips on wrapped lines, inline-object indent, per-line regex cost (now only when a line wraps), extra layout cases. Open, accepted: RTL and tab-stop restart on continuation rows (LTR app).
- **Bounds check:** request (wrapped continuations under their text, all line types) vs plan (vendored AvaloniaEdit fork, Func seam, `WrapIndent`) vs done (same; text, undo and caret keys untouched; only `third_party/AvaloniaEdit` 4 files + csproj + `WrapIndent.cs` + `PadView` one line + `Renderers.cs` chip fix): within plan; `Renderers.cs` was added by the r2 brief.
- **Catch-up:** rebased on dev 18ff078, RETEST: no (incoming changes are loop and tooling files only).

## Hand test
- **Build checked:** dev copy from this worktree, rebased on dev e2bb364 (before the last catch-up).
- **Result (user, 2026-11-17):** "Looks good" (rebased dev copy). Earlier pass on r2: "it works pretty well!"
- **Separate request, not part of A22:** Tab changes level only at the text start (Word's rule): idea row `tab-at-text-start` (Master 6).
- **Misses:** none.
