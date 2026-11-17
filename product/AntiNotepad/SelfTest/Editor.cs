// Windowless checks for editor behavior that depends on visual-line layout.
using AntiNotepad.Model;
using AntiNotepad.Theme;
using AntiNotepad.UI;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using System.Reflection;
using System.Reflection.Emit;

namespace AntiNotepad;

internal static partial class SelfTest
{
    private static bool ShortenedLinkChecks(out string report)
    {
        // Exercise the glyph's real press hook without a formatter, window, or browser.
        // Layout needs a platform formatter; dispatch stays headless and uses a stub opener.
        var glyph = (Editor.GlyphElement)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(Editor.GlyphElement));
        var hook = typeof(Editor.GlyphElement).GetMethod("OnPointerPressed", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var calls = 0;
        var sawControl = false;
        typeof(Editor.GlyphElement).GetProperty("Pressed")!.SetValue(glyph,
            (Action<Avalonia.Input.PointerPressedEventArgs>)(e =>
            {
                calls++;
                sawControl = e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control);
                if (sawControl) e.Handled = true;
            }));
        using var pointer = new Avalonia.Input.Pointer(1, Avalonia.Input.PointerType.Mouse, true);
        Avalonia.Input.PointerPressedEventArgs Press(bool left, Avalonia.Input.KeyModifiers modifiers) =>
            new(null, pointer, null!, default, 0,
                new Avalonia.Input.PointerPointProperties(
                    left ? Avalonia.Input.RawInputModifiers.LeftMouseButton : Avalonia.Input.RawInputModifiers.RightMouseButton,
                    left ? Avalonia.Input.PointerUpdateKind.LeftButtonPressed : Avalonia.Input.PointerUpdateKind.RightButtonPressed),
                modifiers);
        var control = Press(true, Avalonia.Input.KeyModifiers.Control);
        hook.Invoke(glyph, [control]);
        var controlPass = calls == 1 && sawControl && control.Handled;
        var plain = Press(true, Avalonia.Input.KeyModifiers.None);
        hook.Invoke(glyph, [plain]);
        var plainPass = calls == 2 && !sawControl && !plain.Handled;
        hook.Invoke(glyph, [Press(false, Avalonia.Input.KeyModifiers.Control)]);
        var rightPass = calls == 2;
        typeof(Editor.GlyphElement).GetProperty("IsLink")!.SetValue(glyph, true);
        calls = 0;
        var tunnel = Press(true, Avalonia.Input.KeyModifiers.Control);
        var tunnelPass = Editor.PadView.TryPressLink(glyph, tunnel) && tunnel.Handled && calls == 1;
        hook.Invoke(glyph, [tunnel]);
        var oncePass = calls == 1 && !Editor.PadView.TryPressLink(glyph, tunnel);
        var plainTunnel = Press(true, Avalonia.Input.KeyModifiers.None);
        var tunnelPlainPass = !Editor.PadView.TryPressLink(glyph, plainTunnel) && !plainTunnel.Handled && calls == 1;
        var tunnelRightPass = !Editor.PadView.TryPressLink(glyph, Press(false, Avalonia.Input.KeyModifiers.Control)) && calls == 1;
        var missingPass = !Editor.PadView.TryPressLink(null, Press(true, Avalonia.Input.KeyModifiers.Control));
        typeof(Editor.GlyphElement).GetProperty("IsLink")!.SetValue(glyph, false);
        var nonLinkPass = !Editor.PadView.TryPressLink(glyph, Press(true, Avalonia.Input.KeyModifiers.Control)) && calls == 1;

        // Initialize only windowless layout services; no app lifetime, window, or shell opener.
        AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();
        var viewer = new ScrollViewer();
        typeof(ScrollViewer).GetProperty("Extent")!.SetValue(viewer, new Size(300, 500));
        typeof(ScrollViewer).GetProperty("Viewport")!.SetValue(viewer, new Size(100, 100));
        viewer.Offset = new Vector(40, 0);
        var surface = new Editor.SmoothScroll.ViewerSurface(viewer, Typography.PanelTitle);
        surface.Offset = 250;
        var middle = surface.Offset == 250 && viewer.Offset.X == 40;
        surface.Offset = 900;
        var upper = surface.Offset == 400;
        surface.Offset = -10;
        var lower = surface.Offset == 0;
        var editor = new AvaloniaEdit.TextEditor();
        var editorSurface = new Editor.SmoothScroll.EditorSurface(editor);
        editorSurface.Offset = -10;
        var adaptersPass = middle && upper && lower && surface.LineHeight == Typography.PanelTitle
            && editorSurface.Offset == 0 && editorSurface.Extent == editor.ExtentHeight
            && editorSurface.Viewport == editor.ViewportHeight && editorSurface.LineHeight == editor.TextArea.TextView.DefaultLineHeight;
        Console.WriteLine($"SCROLL SURFACES viewer middle/upper/lower={middle}/{upper}/{lower}; editor={editorSurface.Offset == 0}");
        var pad = (Editor.PadView)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(Editor.PadView));
        typeof(Editor.PadView).GetField("<Editor>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(pad, editor);
        var settings = new Settings { ShortenLinks = true };
        typeof(Editor.PadView).GetProperty("Settings")!.SetValue(pad, settings);
        typeof(Editor.PadView).GetProperty("Palette")!.SetValue(pad, Palette.All[0]);
        var view = editor.TextArea.TextView;
        view.ElementGenerators.Add(new Editor.LinkGenerator(pad));
        const string shortUrl = "https://google.com";
        const string longUrl = "https://example.com/long/path/over/thirtyfour/characters";
        editor.Text = $"prefix {shortUrl} suffix\n{longUrl}\nend";
        void Layout()
        {
            view.Redraw();
            view.Measure(new Size(800, 200));
            view.Arrange(new Rect(0, 0, 800, 200));
            view.EnsureVisualLines();
        }
        Point Inside(int lineNumber, int column) => view.GetVisualPosition(
            new AvaloniaEdit.TextViewPosition(lineNumber, column), AvaloniaEdit.Rendering.VisualYPosition.LineMiddle)
            - view.ScrollOffset + new Vector(1, 0);
        Layout();
        var shortPoint = Inside(1, 10);
        var shortHit = Editor.PadView.PlainUrlAt(view, shortPoint) == shortUrl;
        var outside = Editor.PadView.PlainUrlAt(view, Inside(1, 1)) is null
            && Editor.PadView.PlainUrlAt(view, new Point(799, shortPoint.Y)) is null
            && Editor.PadView.PlainUrlAt(view, new Point(1, 199)) is null
            && Editor.PadView.PlainUrlAt(view, new Point(-1, 1)) is null;
        editor.TextArea.Caret.Line = 2;
        Layout();
        var expandedHit = Editor.PadView.PlainUrlAt(view, Inside(2, 10)) == longUrl;
        editor.TextArea.Caret.Line = 3;
        settings.ShortenLinks = false;
        Layout();
        var disabledHit = Editor.PadView.PlainUrlAt(view, Inside(2, 10)) == longUrl;
        calls = 0;
        string? opened = null;
        void Open(string url) { calls++; opened = url; }
        var urlPress = Press(true, Avalonia.Input.KeyModifiers.Control);
        var urlTunnel = Editor.PadView.TryPressUrl(Editor.PadView.PlainUrlAt(view, shortPoint), urlPress, Open)
            && urlPress.Handled && calls == 1 && opened == shortUrl
            && !Editor.PadView.TryPressUrl(shortUrl, urlPress, Open) && calls == 1;
        var urlPlain = Press(true, Avalonia.Input.KeyModifiers.None);
        var urlRejected = !Editor.PadView.TryPressUrl(shortUrl, urlPlain, Open) && !urlPlain.Handled
            && !Editor.PadView.TryPressUrl(shortUrl, Press(false, Avalonia.Input.KeyModifiers.Control), Open)
            && !Editor.PadView.TryPressUrl(null, Press(true, Avalonia.Input.KeyModifiers.Control), Open) && calls == 1;
        var plainUrlsPass = shortHit && expandedHit && disabledHit && outside && urlTunnel && urlRejected;

        var errors = new List<string>();
        bool Calls(MethodBase method, Type owner, string name)
        {
            var found = false;
            ScanMethod(method, [], errors, (_, opcode, target) =>
            {
                if ((opcode == OpCodes.Call || opcode == OpCodes.Callvirt || opcode == OpCodes.Newobj)
                    && target.DeclaringType == owner && target.Name == name) found = true;
            });
            return found;
        }
        var generator = typeof(Editor.LinkGenerator).GetMethod("ConstructElement")!;
        var generatorPass = Calls(generator, typeof(Editor.GlyphElement), ".ctor")
            && Calls(generator, typeof(Editor.GlyphElement), "set_Hand")
            && Calls(generator, typeof(Editor.GlyphElement), "set_IsLink")
            && Calls(generator, typeof(Editor.GlyphElement), "set_Pressed");
        var query = typeof(Editor.GlyphElement).GetMethod("OnQueryCursor", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var cursorPass = Calls(query, typeof(Avalonia.Input.InputElement), "set_Cursor")
            && Calls(query, typeof(Avalonia.Interactivity.RoutedEventArgs), "set_Handled");
        var moved = typeof(Editor.PadView).GetMethod("OnViewMoved", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var pressed = typeof(Editor.PadView).GetMethod("OnViewPressed", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var routePass = Calls(moved, typeof(Editor.PadView), "GlyphAt")
            && Calls(pressed, typeof(Editor.PadView), "GlyphAt") && Calls(pressed, typeof(Editor.PadView), "TryPressLink")
            && Calls(moved, typeof(Editor.PadView), "PlainUrlAt") && Calls(pressed, typeof(Editor.PadView), "PlainUrlAt")
            && Calls(pressed, typeof(Editor.PadView), "TryPressUrl");
        report = $"glyph Ctrl+left={controlPass}; plain unhandled={plainPass}; right ignored={rightPass}; generator wiring={generatorPass}; cursor handled={cursorPass}; tunnel={tunnelPass}; once={oncePass}; tunnel plain/right/missing/nonlink={tunnelPlainPass && tunnelRightPass && missingPass && nonLinkPass}; hit-test wiring={routePass}";
        Console.WriteLine("SHORTENED LINK " + report);
        report += $"; plain URL short/expanded/off={shortHit}/{expandedHit}/{disabledHit}; outside={outside}; plain URL once/rejected={urlTunnel}/{urlRejected}";
        Console.WriteLine("ANY URL " + report);
        return controlPass && plainPass && rightPass && generatorPass && cursorPass && tunnelPass && oncePass
            && tunnelPlainPass && tunnelRightPass && missingPass && nonLinkPass && routePass && plainUrlsPass && adaptersPass && errors.Count == 0;
    }

    /// <summary>Checks text-column parsing and wrapped layout without opening a window.</summary>
    private static bool WrapIndentChecks(out string report)
    {
        (string Text, int Offset)[] helperCases =
        [
            ("- root", 2), ("    - nested", 6), ("  - [x] checked", 8),
            ("12. numbered", 4), ("\t  indented", 3), ("plain", 0)
        ];
        var helperFailures = helperCases.Where(test => Editor.WrapIndent.TextOffset(test.Text) != test.Offset).ToList();

        try
        {
            if (Application.Current is null)
                AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();

            var editor = new AvaloniaEdit.TextEditor { WordWrap = true };
            editor.Options.IndentationSize = 2;
            var pad = (Editor.PadView)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(Editor.PadView));
            typeof(Editor.PadView).GetProperty("Mode")!.SetValue(pad, NoteMode.Plain);
            typeof(Editor.PadView).GetProperty("Settings")!.SetValue(pad, new Model.Settings());
            typeof(Editor.PadView).GetProperty("Palette")!.SetValue(pad, Palette.All[0]);
            var view = editor.TextArea.TextView;
            ((ILogicalScrollable)view).CanHorizontallyScroll = false;
            view.ElementGenerators.Add(new Editor.CheckboxGenerator(pad));
            view.ElementGenerators.Add(new Editor.BulletGenerator(pad));
            Editor.WrapIndent.Configure(view);

            const string tail = "alpha beta gamma delta epsilon zeta eta theta iota kappa lambda mu nu xi omicron pi rho sigma tau upsilon phi chi psi omega; "
                + "alpha beta gamma delta epsilon zeta eta theta iota kappa lambda mu nu xi omicron pi rho sigma tau upsilon phi chi psi omega";
            (string Kind, string Prefix)[] cases =
            [
                ("top bullet", "- "), ("nested level 1", "  - "),
                ("nested level 2", "    - "), ("nested level 3", "      - "),
                ("checkbox", "  - [ ] "), ("numbered", "   12. "),
                ("plain indented", "    "), ("tab prefix", "\t- "), ("plain unindented", "")
            ];
            var failures = new List<string>();
            if (helperFailures.Count > 0) failures.Add($"text offsets: {helperFailures.Count}");

            foreach (var font in new[] { "Segoe UI", "Consolas" })
            {
                foreach (var fontSize in new[] { 14d, 20d })
                {
                    foreach (var width in new[] { 180d, 240d, 310d })
                    {
                        foreach (var test in cases)
                        {
                            var content = test.Prefix + tail;
                            editor.FontFamily = new FontFamily(font);
                            editor.FontSize = fontSize;
                            editor.Text = content;
                            view.Redraw();
                            view.Measure(new Size(width, 2000));
                            view.Arrange(new Rect(0, 0, width, 2000));
                            view.EnsureVisualLines();

                            var visualLine = view.GetOrConstructVisualLine(editor.Document.GetLineByNumber(1));
                            var textOffset = Editor.WrapIndent.TextOffset(content);
                            var anchorColumn = visualLine.GetVisualColumn(textOffset);
                            var firstTextLine = visualLine.TextLines[0];
                            var expectedX = visualLine.GetTextLineVisualXPosition(firstTextLine, anchorColumn);
                            var wrapped = visualLine.TextLines.Count > 1;
                            var lastRow = visualLine.TextLines[^1];
                            var chipEnd = Editor.ResultRenderer.TextEnd(visualLine);
                            var expectedEnd = visualLine.GetTextLineVisualXPosition(lastRow, visualLine.VisualLength);
                            if (wrapped && (Math.Abs(chipEnd - expectedEnd) > 0.01 || (expectedX > 0 && chipEnd <= lastRow.WidthIncludingTrailingWhitespace)))
                                failures.Add($"{test.Kind}: chip end excludes continuation indent");
                            var aligned = wrapped && visualLine.TextLines.Skip(1).All(textLine =>
                                Math.Abs(visualLine.GetTextLineVisualXPosition(textLine,
                                    visualLine.GetTextLineVisualStartColumn(textLine)) - expectedX) < 0.5);

                            if (!wrapped || !aligned || editor.Text != content)
                            {
                                failures.Add($"{test.Kind}/{font}/{fontSize}/{width}: wrapped={wrapped}, aligned={aligned}, textSame={editor.Text == content}");
                                continue;
                            }

                            var continuation = visualLine.TextLines[1];
                            var startColumn = visualLine.GetTextLineVisualStartColumn(continuation);
                            var position = visualLine.GetTextViewPosition(startColumn);
                            var endPosition = visualLine.GetTextViewPosition(startColumn + 1);
                            var startOffset = editor.Document.GetOffset(position.Location); var endOffset = editor.Document.GetOffset(endPosition.Location);
                            var x0 = visualLine.GetTextLineVisualXPosition(continuation, startColumn);
                            var point = new Point(x0 + Math.Max(0.1, (visualLine.GetTextLineVisualXPosition(continuation, startColumn + 1) - x0) * 0.1),
                                visualLine.GetTextLineVisualYPosition(continuation, VisualYPosition.LineMiddle));
                            var floorHit = view.GetPositionFloor(point); var roundedHit = view.GetPosition(point);

                            editor.TextArea.Caret.Position = position;
                            var caretX = editor.TextArea.Caret.CalculateCaretRectangle().X;

                            var selection = Selection.Create(editor.TextArea, startOffset, endOffset);
                            var selectedSegment = selection.Segments.First();
                            var selectionRect = BackgroundGeometryBuilder.GetRectsForSegment(view, selectedSegment).First();
                            var hitsRoundTrip = floorHit is { } floor
                                && roundedHit is { } rounded
                                && editor.Document.GetOffset(floor.Location) == startOffset
                                && editor.Document.GetOffset(rounded.Location) == startOffset;
                            var geometryMatches = Math.Abs(caretX - x0) < 0.5 && Math.Abs(selectionRect.Left - x0) < 0.5;
                            if (!hitsRoundTrip || !geometryMatches)
                                failures.Add($"{test.Kind}/{font}/{fontSize}/{width}: hit={hitsRoundTrip}, caret/selection={geometryMatches}");
                        }
                    }
                }
            }

            editor.FontFamily = new FontFamily("Consolas");
            editor.FontSize = 20;
            editor.Text = new string(' ', 16) + tail;
            view.Redraw();
            view.Measure(new Size(240, 2000));
            view.Arrange(new Rect(0, 0, 240, 2000));
            view.EnsureVisualLines();
            var wideIndent = view.GetOrConstructVisualLine(editor.Document.GetLineByNumber(1));
            if (wideIndent.TextLines.Count < 2 || Math.Abs(wideIndent.GetTextLineVisualXPosition(wideIndent.TextLines[1], wideIndent.GetTextLineVisualStartColumn(wideIndent.TextLines[1]))) > 0.5)
                failures.Add("over-half-width indent did not fall back to zero");

            editor.Text = "  - " + tail + "\nhidden folded text\nend";
            var folding = AvaloniaEdit.Folding.FoldingManager.Install(editor.TextArea);
            folding.CreateFolding(editor.Document.GetLineByNumber(1).EndOffset - 5, editor.Document.GetLineByNumber(2).EndOffset).IsFolded = true;
            view.Redraw();
            view.Measure(new Size(240, 2000));
            view.Arrange(new Rect(0, 0, 240, 2000));
            view.EnsureVisualLines();
            var folded = view.GetOrConstructVisualLine(editor.Document.GetLineByNumber(1));
            if (folded.LastDocumentLine.LineNumber != 2 || folded.TextLines.Count < 2)
                failures.Add("folded wrapped line was not laid out");
            AvaloniaEdit.Folding.FoldingManager.Uninstall(folding);

            report = $"helper cases={helperCases.Length - helperFailures.Count}/{helperCases.Length}; layouts=9 types x 2 fonts x 2 sizes x 3 widths; folded and wide indent checked; failures={string.Join("; ", failures.Take(8))}";
            Console.WriteLine("WRAP INDENT " + report);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = $"helper cases={helperCases.Length - helperFailures.Count}/{helperCases.Length}; layout exception={ex}";
            Console.WriteLine("WRAP INDENT FAIL " + report);
            return false;
        }
    }

    private static bool SplitDividerFullHeightChecks(out string report)
    {
        try
        {
            var divider = PadWindow.CreateSplitDivider();
            var panes = new Grid { ColumnDefinitions = new ColumnDefinitions("*,0,0") };
            Grid.SetColumn(divider, 1);
            panes.Children.Add(divider);
            var content = new Grid();
            content.Children.Add(panes);

            var root = new Grid();
            root.Children.Add(new Border());
            root.Children.Add(content);
            var topLayer = new Panel();
            var bottomLayer = new Panel();
            var pill = new Border();
            PadWindow.AddChromeLayers(root, topLayer, bottomLayer, new Border(), pill, new Border());

            var contentIndex = root.Children.IndexOf(content);
            var splitDivider = ReferenceEquals(divider.Parent, panes) && ReferenceEquals(panes.Parent, content);
            var dockedPagerAbove = splitDivider && root.Children.IndexOf(pill) > contentIndex;
            root.Children.Remove(pill);
            bottomLayer.Children.Add(pill);
            var overlayPagerAbove = splitDivider && ReferenceEquals(pill.Parent, bottomLayer)
                && root.Children.IndexOf(bottomLayer) > contentIndex;

            report = $"divider bottom={divider.Margin.Bottom}; docked pager above divider={dockedPagerAbove}; overlay pager above divider={overlayPagerAbove}";
            return divider.Margin.Bottom == ChromeTokens.PagerStripHeight + ChromeTokens.DividerPagerClearance && dockedPagerAbove && overlayPagerAbove;
        }
        catch (Exception ex)
        {
            report = ex.GetType().Name + ": " + ex.Message;
            return false;
        }
    }

    private static bool SplitPaneClippingChecks(out string report)
    {
        var left = new Border { RenderTransform = new Avalonia.Media.TranslateTransform() };
        var right = new Border { RenderTransform = new Avalonia.Media.TranslateTransform() };
        var divider = PadWindow.CreateSplitDivider(); var grid = new Grid();
        var (leftHost, rightHost) = PadWindow.AddPaneHosts(grid, left, divider, right);
        PadWindow.ApplyPaneSplit(grid, leftHost, divider, rightHost, right, true);
        grid.Measure(new Size(640, 400)); grid.Arrange(new Rect(0, 0, 640, 400));
        var structure = grid.Children.Count == 3 && ReferenceEquals(grid.Children[0], leftHost) && ReferenceEquals(grid.Children[1], divider) && ReferenceEquals(grid.Children[2], rightHost)
            && ReferenceEquals(leftHost.Parent, grid) && ReferenceEquals(rightHost.Parent, grid) && ReferenceEquals(leftHost.Child, left) && ReferenceEquals(rightHost.Child, right)
            && ReferenceEquals(left.Parent, leftHost) && ReferenceEquals(right.Parent, rightHost)
            && Grid.GetColumn(leftHost) == 0 && Grid.GetColumn(divider) == 1 && Grid.GetColumn(rightHost) == 2 && leftHost.ClipToBounds && rightHost.ClipToBounds
            && leftHost.IsVisible && rightHost.IsVisible && right.IsVisible && divider.IsVisible;
        var leftBounds = leftHost.Bounds; var rightBounds = rightHost.Bounds;
        var separated = leftBounds.Width > 0 && rightBounds.Width > 0 && leftBounds.Right <= divider.Bounds.Left
            && rightBounds.Left >= divider.Bounds.Right && divider.Bounds.Width == ChromeTokens.DividerWidth;
        var leftMotion = (Avalonia.Media.TranslateTransform)left.RenderTransform!; var rightMotion = (Avalonia.Media.TranslateTransform)right.RenderTransform!;
        leftMotion.X = 64; rightMotion.X = -64;
        var towardDividerStable = leftHost.Bounds == leftBounds && rightHost.Bounds == rightBounds;
        leftMotion.X = -64; rightMotion.X = 64;
        var awayFromDividerStable = leftHost.Bounds == leftBounds && rightHost.Bounds == rightBounds;
        var stationary = towardDividerStable && awayFromDividerStable && leftHost.RenderTransform is null && rightHost.RenderTransform is null;
        PadWindow.ApplyPaneSplit(grid, leftHost, divider, rightHost, right, false);
        var single = !leftHost.ClipToBounds && !rightHost.IsVisible && !right.IsVisible && !divider.IsVisible;
        PadWindow.ApplyPaneSplit(grid, leftHost, divider, rightHost, right, true);
        var restored = leftHost.ClipToBounds && rightHost.IsVisible && right.IsVisible && divider.IsVisible;
        report = $"split host structure={structure}; bounds separated by divider={separated}; toward/away translations keep host bounds={stationary}; single view unclipped/hidden={single}; split restored={restored}";
        return structure && separated && stationary && single && restored;
    }

    private static bool MathNotesChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        CheckCase(Editor.MathNotes.BlankLabel("spent 12.50 + 8.25 =") == "      12.50 + 8.25 =",
            "blank prose label through the first number");
        CheckCase(Editor.MathNotes.StripListMarker("- 19.99 * 3 =") == "  19.99 * 3 =",
            "blank bullet marker without shifting math");
        CheckCase(Editor.MathNotes.StripListMarker("2. 19 + 3 =") == "2. 19 + 3 =",
            "keep numbered marker for math evaluation");
        CheckCase(Editor.MathNotes.StripListMarker("  - [x] 3 + 4 =") == "        3 + 4 =",
            "blank checkbox bullet marker without shifting math");

        var evaluated = Editor.MathNotes.EvaluateMath(["math", "lunch 12 + 3 =", "x = 4", "x * 2 ="]);
        CheckCase(evaluated[1]?.Result?.Value == 15, "label line evaluates after retry");
        CheckCase(evaluated[2]?.AssignedName == "x" && evaluated[2]?.Result?.Value == 4
            && evaluated[3]?.Result?.Value == 8, "variables survive label retry");
        CheckCase(evaluated[1]?.Tokens is { Count: > 0 } tokens
            && tokens[0] == new Calc.CalcToken(0, "lunch ".Length, Calc.TokenKind.Comment),
            "label token keeps its comment span");
        CheckCase(Editor.MathNotes.EvaluateMath(["math", "- 2 + 3 ="])[1]?.Result?.Value == 5,
            "bullet math evaluates as positive");

        report = failures.Count == 0 ? "8 checks" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static bool FenceCacheChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        try
        {
            var palette = Theme.Palette.All[0];
            var doc = new AvaloniaEdit.Document.TextDocument("```cs\nint first = 1;\n```\n\n```js\nlet second = 2;\n```");
            var original = Editor.NoteStructure.Build(["```cs", "int first = 1;", "```", "", "```js", "let second = 2;", "```"]);
            var cache = new Editor.FenceCache<object>();
            var copies = 0;
            var builds = 0;
            object Create(string _) { builds++; return new object(); }
            string Copy(string text) { copies++; return text; }

            foreach (var fence in original.Fences)
            {
                var body = fence.Language == "cs" ? "int first = 1;\n" : "let second = 2;\n";
                cache.Get(fence.Open, fence.Close - 1, fence.Closed, doc.Version, fence.Language, palette, () => Copy(body), Create);
            }
            var first = cache.Get(1, 2, true, doc.Version, "cs", palette, () => Copy("int first = 1;\n"), Create);
            for (var i = 0; i < 100; i++)
                CheckCase(ReferenceEquals(first, cache.Get(1, 2, true, doc.Version, "cs", palette, () => Copy("int first = 1;\n"), Create)), "unchanged fence reuses its cached highlight");
            CheckCase(copies == 2 && builds == 2, "one body copy for each unchanged fence across repeated lines");

            var sameBodyNewVersion = new AvaloniaEdit.Document.TextDocument(doc.Text);
            for (var i = 0; i < 100; i++)
                cache.Get(1, 2, true, sameBodyNewVersion.Version, "cs", palette, () => Copy("int first = 1;\n"), Create);
            CheckCase(copies == 3 && builds == 2, "one body copy after a document version change with unchanged fence text");

            var boundsMissed = !cache.TryGet(1, 3, true, sameBodyNewVersion.Version, "cs", palette, out _);
            var changedBounds = cache.Get(1, 3, true, sameBodyNewVersion.Version, "cs", palette,
                () => Copy("int first = 1;\nextra line\n"), Create);
            CheckCase(boundsMissed && !ReferenceEquals(first, changedBounds) && copies == 4 && builds == 3,
                "same-version fence with changed body bounds misses and rebuilds");

            var moved = Editor.NoteStructure.Build(["intro", "```cs", "int first = 1;", "```"]);
            cache.Prune(moved.Fences.Select(f => f.Open));
            CheckCase(cache.Count == 0, "moved and deleted fence keys are pruned");
            var movedDoc = new AvaloniaEdit.Document.TextDocument("intro\n```cs\nint first = 1;\n```");
            cache.Get(moved.Fences[0].Open, 3, moved.Fences[0].Closed, movedDoc.Version, moved.Fences[0].Language, palette,
                () => Copy("int first = 1;\n"), Create);
            cache.Prune(moved.Fences.Select(f => f.Open));
            CheckCase(cache.Count == moved.Fences.Count && cache.TryGet(2, 3, true, movedDoc.Version, "cs", palette, out _)
                && !cache.TryGet(1, 2, true, movedDoc.Version, "cs", palette, out _) && !cache.TryGet(5, 3, true, movedDoc.Version, "js", palette, out _),
                "cache contains only the current moved fence");

            var pens = new Editor.ChevronPens();
            var firstPens = pens.Get(1, palette);
            var reusedPens = pens.Get(1, palette);
            var changedPalettePens = pens.Get(1, Theme.Palette.All[1]);
            var changedScalePens = pens.Get(1.25, Theme.Palette.All[1]);
            CheckCase(ReferenceEquals(firstPens, reusedPens) && !ReferenceEquals(firstPens, changedPalettePens)
                && !ReferenceEquals(changedPalettePens, changedScalePens), "chevron pens reuse by palette and scale");

            var brushes = new Editor.ColorizerBrushCache();
            var firstBrush = brushes.Get(palette.Keyword);
            var reusedBrush = brushes.Get(palette.Keyword);
            var changedBrush = brushes.Get(Theme.Palette.All[1].Keyword);
            CheckCase(ReferenceEquals(firstBrush, reusedBrush) && !ReferenceEquals(firstBrush, changedBrush),
                "colorizer brush reuses the same color and refreshes on palette color change");

            report = failures.Count == 0
                ? "fence cache pruning and body-copy reuse, plus chevron and colorizer brush resource reuse"
                : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex) { report = string.Join("; ", failures.Append(ex.ToString())); return false; }
    }

    private static bool IncrementalAnalysisChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        static string EvalSignature(Calc.LineEval? line)
        {
            if (line is null) return "null";
            var result = line.Result is { } value
                ? $"{value.Display}|{value.Value.ToString("R", System.Globalization.CultureInfo.InvariantCulture)}|{value.Unit}"
                : "-";
            var tokens = string.Join(",", line.Tokens.Select(t => $"{t.Start}:{t.Length}:{t.Kind}"));
            return $"{result}|{line.ErrorReason}|{line.AssignedName}|{tokens}";
        }

        static bool Equivalent(Editor.NoteAnalysis incremental, Editor.NoteAnalysis full, AvaloniaEdit.Document.TextDocument document)
        {
            var structureA = incremental.Structure;
            var structureB = full.Structure;
            if (incremental.Mode != full.Mode || incremental.CodeLanguage != full.CodeLanguage
                || incremental.ModeKeywordLength != full.ModeKeywordLength
                || !incremental.Lines.SequenceEqual(full.Lines)
                || structureA is null || structureB is null
                || !structureA.Fences.SequenceEqual(structureB.Fences)
                || !structureA.Indent.SequenceEqual(structureB.Indent)
                || !structureA.InFence.SequenceEqual(structureB.InFence)
                || incremental.MathLines.Count != full.MathLines.Count
                || !incremental.MathLines.Select(EvalSignature).SequenceEqual(full.MathLines.Select(EvalSignature))) return false;

            var inlineA = incremental.InlineLines.OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}:{EvalSignature(pair.Value)}");
            var inlineB = full.InlineLines.OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}:{EvalSignature(pair.Value)}");
            if (!inlineA.SequenceEqual(inlineB)) return false;

            var timersA = incremental.TimerKeys.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value}");
            var timersB = full.TimerKeys.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value}");
            if (!timersA.SequenceEqual(timersB)) return false;

            var foldsA = structureA.Folds(document, incremental.Lines, incremental.Mode == NoteMode.Code)
                .Select(fold => (fold.StartOffset, fold.EndOffset, fold.Name)).ToArray();
            var foldsB = structureB.Folds(document, full.Lines, full.Mode == NoteMode.Code)
                .Select(fold => (fold.StartOffset, fold.EndOffset, fold.Name)).ToArray();
            return foldsA.SequenceEqual(foldsB);
        }

        static Editor.NoteAnalysis Full(AvaloniaEdit.Document.TextDocument document, bool inlineMath, string? noteId)
        {
            var full = new Editor.NoteAnalysis();
            full.Update(document, 0, document.LineCount - 1, inlineMath, noteId, timers: null, forceFull: true);
            return full;
        }

        static (int First, int Last) Around(AvaloniaEdit.Document.TextDocument document, int lineNumber) =>
            (Math.Max(0, lineNumber - 2), Math.Min(document.LineCount - 1, lineNumber));

        static int FindLine(AvaloniaEdit.Document.TextDocument document, string text)
        {
            for (var number = 1; number <= document.LineCount; number++)
                if (document.GetText(document.GetLineByNumber(number)) == text) return number;
            return 0;
        }

        try
        {
            var document = new AvaloniaEdit.Document.TextDocument(string.Join('\n', new[]
            {
                "# Start", "intro", "```cs", "int value = 1;", "```", "## After",
                "- item one", "  details", "- item two", "total 2 + 3 ="
            }));
            var analysis = new Editor.NoteAnalysis();
            analysis.Update(document, 0, document.LineCount - 1, inlineMath: true, noteId: "a29-note", timers: null, forceFull: true);
            CheckCase(analysis.LastCopiedLineCount == document.LineCount, "initial analysis copies each document line once");
            CheckCase(analysis.InlineLines.Count == 1, "plain note retains its inline result");

            bool Refresh(AvaloniaEdit.Document.TextDocument target, Editor.NoteAnalysis current,
                int first, int last, bool inlineMath, string? noteId, string name)
            {
                current.Invalidate(target, first, last);
                current.Update(target, first, last, inlineMath, noteId, timers: null);
                var equivalent = Equivalent(current, Full(target, inlineMath, noteId), target);
                CheckCase(equivalent, name);
                return equivalent;
            }

            var body = document.GetLineByNumber(4);
            document.Replace(body.Offset, body.Length, "int value = 2;");
            var range = Around(document, 4);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "fence body edit matches full analysis");

            var opening = document.GetLineByNumber(3);
            document.Replace(opening.Offset, opening.Length, "```py");
            range = Around(document, 3);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "fence opening edit matches full analysis");

            var closing = document.GetLineByNumber(5);
            document.Replace(closing.Offset, closing.Length, "tail");
            range = Around(document, 5);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "open fence edit matches full analysis");

            closing = document.GetLineByNumber(5);
            document.Replace(closing.Offset, closing.Length, "```");
            range = Around(document, 5);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "fence close edit matches full analysis");

            document.Insert(0, "## Inserted\n");
            Refresh(document, analysis, 0, 2, true, "a29-note", "heading insertion matches full analysis");
            var insertedHeading = document.GetLineByNumber(1);
            document.Remove(insertedHeading.Offset, insertedHeading.TotalLength);
            Refresh(document, analysis, 0, Math.Min(2, document.LineCount - 1), true, "a29-note", "heading deletion matches full analysis");

            var detailsLine = FindLine(document, "  details");
            if (detailsLine == 0) throw new InvalidOperationException("list child fixture was lost");
            var details = document.GetLineByNumber(detailsLine);
            document.Replace(details.Offset, details.Length, "details");
            range = Around(document, detailsLine);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "list run edit matches full analysis");

            var mathDocument = new AvaloniaEdit.Document.TextDocument("math\nrate = 10\ncost = rate * 3\nlunch 2 + 4");
            var mathAnalysis = new Editor.NoteAnalysis();
            mathAnalysis.Update(mathDocument, 0, mathDocument.LineCount - 1, inlineMath: true, noteId: "a29-math", timers: null, forceFull: true);
            var rate = mathDocument.GetLineByNumber(2);
            mathDocument.Replace(rate.Offset, rate.Length, "rate = 12");
            Refresh(mathDocument, mathAnalysis, 1, 1, true, "a29-math", "later variable use matches full two-pass math analysis");
            CheckCase(mathAnalysis.MathLines[2]?.Result?.Value == 36, "later math line reflects the edited variable");

            var timerDocument = new AvaloniaEdit.Document.TextDocument("timer\n5m tea\n1m water\n5m coffee");
            var timerAnalysis = new Editor.NoteAnalysis();
            timerAnalysis.Update(timerDocument, 0, timerDocument.LineCount - 1, inlineMath: true, noteId: "a29-timers", timers: null, forceFull: true);
            var water = timerDocument.GetLineByNumber(3);
            timerDocument.Replace(water.Offset, water.Length, "5m water");
            range = Around(timerDocument, 3);
            Refresh(timerDocument, timerAnalysis, range.First, range.Last, true, "a29-timers", "timer duration edit matches full parsing");
            CheckCase(timerAnalysis.TimerKeys.TryGetValue(2, out var firstTimer) && firstTimer == "a29-timers:5m:1"
                && timerAnalysis.TimerKeys.TryGetValue(3, out var secondTimer) && secondTimer == "a29-timers:5m:2"
                && timerAnalysis.TimerKeys.TryGetValue(4, out var thirdTimer) && thirdTimer == "a29-timers:5m:3",
                "duplicate timer ordinals remain in document order");

            var burstDocument = new AvaloniaEdit.Document.TextDocument(string.Join("\n",
                Enumerable.Range(0, 5000).Select(index => $"ordinary line {index}")));
            var burst = new Editor.NoteAnalysis();
            burst.Update(burstDocument, 0, burstDocument.LineCount - 1, inlineMath: true, noteId: "a29-burst", timers: null, forceFull: true);
            var firstEdit = burstDocument.GetLineByNumber(2500);
            burstDocument.Replace(firstEdit.Offset, firstEdit.Length, "ordinary line 2499 edited");
            var firstRange = Around(burstDocument, 2500);
            burst.Invalidate(burstDocument, firstRange.First, firstRange.Last);
            var secondEdit = burstDocument.GetLineByNumber(2501);
            burstDocument.Replace(secondEdit.Offset, secondEdit.Length, "ordinary line 2500 edited");
            var secondRange = Around(burstDocument, 2501);
            burst.Invalidate(burstDocument, secondRange.First, secondRange.Last);
            var unionFirst = Math.Min(firstRange.First, secondRange.First);
            var unionLast = Math.Max(firstRange.Last, secondRange.Last);
            burst.Update(burstDocument, unionFirst, unionLast, inlineMath: true, noteId: "a29-burst", timers: null);
            CheckCase(burst.LastCopiedLineCount <= 8 && burst.LastAnalysedLineCount < 32,
                $"5,000-line burst copies {burst.LastCopiedLineCount} and analyses {burst.LastAnalysedLineCount} lines");
            CheckCase(Equivalent(burst, Full(burstDocument, true, "a29-burst"), burstDocument),
                "multiple same-count edits match full analysis");

            report = failures.Count == 0
                ? "fence, heading, list, math, timer, inline and fold equivalence; two-edit 5,000-line burst stayed bounded"
                : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.ToString()));
            return false;
        }
    }
}
