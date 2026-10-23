using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;

namespace AntiQuickPad.Editor;

internal static class AlignmentProof
{
    public static async Task Run(TextEditor editor, string output, Look? palette = null)
    {
        var look = palette ?? Look.Default;
        var view = editor.TextArea.TextView;
        PixelSize size = default;
        byte[] Capture(string? name)
        {
            size = new PixelSize((int)Math.Ceiling(editor.Bounds.Width), (int)Math.Ceiling(editor.Bounds.Height));
            using var bitmap = new RenderTargetBitmap(size, new Vector(96, 96)); bitmap.Render(editor);
            if (name != null) bitmap.Save(Path.Combine(output, name), PngBitmapEncoderOptions.Default);
            var bytes = new byte[size.Width * size.Height * 4]; var ptr = Marshal.AllocHGlobal(bytes.Length);
            try { bitmap.CopyPixels(new PixelRect(size), ptr, bytes.Length, size.Width * 4); Marshal.Copy(ptr, bytes, 0, bytes.Length); }
            finally { Marshal.FreeHGlobal(ptr); }
            return bytes;
        }
        async Task Layout(string text)
        {
            editor.Text = text; editor.CaretOffset = 0; editor.ScrollToHome(); await Task.Delay(100);
            editor.UpdateLayout(); view.EnsureVisualLines(); editor.TextArea.Caret.Hide();
        }
        Rect Bounds(byte[] pixels, Rect region, Func<byte, byte, byte, bool> test)
        {
            var points = new List<Point>();
            for (var y = Math.Max(0, (int)Math.Floor(region.Top)); y < Math.Min(size.Height, Math.Ceiling(region.Bottom)); y++)
            for (var x = Math.Max(0, (int)Math.Floor(region.Left)); x < Math.Min(size.Width, Math.Ceiling(region.Right)); x++)
            { var i = (y * size.Width + x) * 4; if (test(pixels[i + 2], pixels[i + 1], pixels[i])) points.Add(new Point(x + .5, y + .5)); }
            if (points.Count == 0) throw new Exception("No fixture ink found.");
            return new Rect(new Point(points.Min(p => p.X) - .5, points.Min(p => p.Y) - .5), new Point(points.Max(p => p.X) + .5, points.Max(p => p.Y) + .5));
        }
        await Layout("- [ ] open\n- [x] done\n- plain\n    - [ ] nested\n        - [x] deep\n    - plain");
        var pixels = Capture("T-009-checkbox.png");
        var origin = view.TranslatePoint(default, editor)!.Value;
        var boxes = view.BackgroundRenderers.OfType<CheckboxRenderer>().Single().Boxes.ToArray();
        bool Accent(byte r, byte g, byte b) => Math.Abs(r - look.Accent.R) < 20 && Math.Abs(g - look.Accent.G) < 20 && Math.Abs(b - look.Accent.B) < 20;
        var bulletCentres = new List<double>();
        foreach (var item in boxes)
        {
            var box = item.Bounds.Translate(new Vector(origin.X, origin.Y));
            var isChecked = editor.Document.GetCharAt(item.Offset + 1) == 'x';
            var color = isChecked ? look.Accent : look.Muted;
            var ink = Bounds(pixels, box.Inflate(2), (r,g,b) => Math.Abs(r-color.R)<20 && Math.Abs(g-color.G)<20 && Math.Abs(b-color.B)<20);
            var line = editor.Document.GetLineByOffset(item.Offset);
            var visual = view.VisualLines.Single(v => v.FirstDocumentLine == line);
            var dashColumn = editor.Document.GetText(line).IndexOf('-');
            var dash = BackgroundGeometryBuilder.GetRectsForSegment(view, new AvaloniaEdit.Document.SimpleSegment(line.Offset + dashColumn, 1)).Single().Translate(new Vector(origin.X, origin.Y));
            var dashInk = Bounds(pixels, dash, Accent);
            bulletCentres.Add(dashInk.Center.Y);
            var delta = Math.Abs(ink.Center.Y - dashInk.Center.Y);
            if (delta > 1) throw new Exception($"Checkbox/bullet centre mismatch: {delta}");
            if (editor.Document.GetCharAt(item.Offset + 1) == 'x')
            {
                var check = Bounds(pixels, box.Deflate(2), (r,g,b) => Math.Abs(r-look.CheckboxCheck.R)<20 && Math.Abs(g-look.CheckboxCheck.G)<20 && Math.Abs(b-look.CheckboxCheck.B)<20);
                var dx = Math.Abs(check.Center.X - ink.Center.X); var dy = Math.Abs(check.Center.Y - ink.Center.Y);
                Console.WriteLine($"T008 checkbox line={line.LineNumber}: check centre delta=({dx:F2},{dy:F2})px box/bullet deltaY={delta:F2}px row={visual.Height:F3}");
                if (dx > 1 || dy > 1) throw new Exception("Check ink not centred.");
            }
            else Console.WriteLine($"T008 open line={line.LineNumber}: box/bullet deltaY={delta:F2}px row={visual.Height:F3}");
        }
        var heights = view.VisualLines.Select(v => v.Height).ToArray();
        if (heights.Max() - heights.Min() > .01) throw new Exception("Checkbox changed row height.");
        // Hit-test the rendered rectangle and exercise the same toggle operation used by pointer input.
        var hit = boxes[0]; var caret = editor.CaretOffset;
        EditorFactory.UpdateCursor(editor, hit.Bounds.Center);
        if (!ReferenceEquals(editor.Cursor, EditorFactory.CheckboxHand) || !EditorFactory.TryCheckboxAt(editor, hit.Bounds.Center, out var hitOffset) || hitOffset != hit.Offset)
            throw new Exception("Checkbox hover/hit failed.");
        EditorFactory.UpdateCursor(editor, new Point(view.Bounds.Width - 10, view.Bounds.Height - 10));
        if (ReferenceEquals(editor.Cursor, EditorFactory.CheckboxHand)) throw new Exception("Checkbox hover did not clear.");
        if (!hit.Bounds.Contains(hit.Bounds.Center) || EditorOperations.ToggleCheckbox(editor.Document, hit.Offset, ref caret) != "[x]") throw new Exception("Checkbox hit/toggle failed.");
        editor.Document.UndoStack.Undo();

        // A plain-row control compares actual bullet ink positions, rather than trusting layout heights.
        await Layout("- plain\n- plain\n- plain\n    - plain\n        - plain\n    - plain");
        var plainPixels = Capture(null);
        var controlDelta = 0.0;
        var iBullet = 0;
        foreach (var n in new[] {1,2,4,5})
        {
            var line = editor.Document.GetLineByNumber(n);
            var dashColumn = editor.Document.GetText(line).IndexOf('-');
            var region = BackgroundGeometryBuilder.GetRectsForSegment(view, new AvaloniaEdit.Document.SimpleSegment(line.Offset + dashColumn, 1)).Single().Translate(new Vector(origin.X,origin.Y));
            var y = Bounds(plainPixels, region, Accent).Center.Y;
            controlDelta = Math.Max(controlDelta, Math.Abs(y - bulletCentres[iBullet++]));
        }
        if (controlDelta != 0) throw new Exception("Checkbox/plain row pixel positions differ.");
        Console.WriteLine($"T008 row pixel control: max bullet-centre displacement vs all-plain rows={controlDelta:F2}px; hover/hit/undo PASS.");

        await Layout("- root\n    - child\n        - grandchild\n\n        - continued\n\n- outside\n    ```python\n    def f():\n        if True:\n            return 1\n\n        return 0\n    ```\n\n\t- tab\n\t\t- two tabs\n\nend");
        var guide = view.BackgroundRenderers.OfType<IndentGuideRenderer>().Single();
        var on = Capture("T-009-guides.png");
        view.BackgroundRenderers.Remove(guide); view.InvalidateLayer(KnownLayer.Background); await Task.Delay(80);
        var off = Capture(null);
        view.BackgroundRenderers.Add(guide); view.InvalidateLayer(KnownLayer.Background);
        origin = view.TranslatePoint(default, editor)!.Value;
        int[] depths = [0,1,2,2,2,0,0,1,1,2,3,2,2,1,0,1,2,0,0];
        foreach (var visual in view.VisualLines)
        {
            var n = visual.FirstDocumentLine.LineNumber; var expected = depths[n - 1];
            var columns = new HashSet<int>();
            var y0 = (int)Math.Ceiling(origin.Y + visual.VisualTop - view.VerticalOffset);
            for (var y = y0 + 2; y < y0 + (int)visual.Height - 2; y++)
            for (var x = (int)origin.X; x < size.Width; x++)
            { var i = (y * size.Width + x) * 4; if (Math.Abs(on[i] - off[i]) + Math.Abs(on[i+1] - off[i+1]) + Math.Abs(on[i+2] - off[i+2]) > 8) columns.Add(x); }
            var text = editor.Document.GetText(visual.FirstDocumentLine);
            // Independent font measurement for expected four-space stops, including the code face.
            var font = new Avalonia.Media.Typeface(new Avalonia.Media.FontFamily(n is >= 8 and <= 14 ? look.CodeFont : look.EditorFont));
            var spaces = new Avalonia.Media.FormattedText("    ", System.Globalization.CultureInfo.InvariantCulture, Avalonia.Media.FlowDirection.LeftToRight, font, editor.FontSize, Avalonia.Media.Brushes.White).WidthIncludingTrailingWhitespace;
            var xs = Enumerable.Range(1, expected).Select(k => (int)Math.Floor(origin.X + k * spaces)).ToArray();
            if (guide.Depth(n) != expected || columns.Count != expected || xs.Any(x => !columns.Any(c => Math.Abs(c - x) <= 1)))
                throw new Exception($"Guide pixels line {n}: expected {string.Join(',',xs)} actual {string.Join(',',columns)} depth={guide.Depth(n)}");
            Console.WriteLine($"T008 guides line={n} indentColumns={string.Join(',', Enumerable.Range(1,expected).Select(k=>k*4))} pixelX={string.Join(',',columns.Order())}");
        }
        // Off setting is exercised through the actual renderer using the same visual fixture.
        view.BackgroundRenderers.Remove(guide);
        var disabled = new IndentGuideRenderer(look with { ShowIndentGuides = false }, new MarkdownScanner(editor.Document));
        view.BackgroundRenderers.Add(disabled); view.InvalidateLayer(KnownLayer.Background); await Task.Delay(80);
        if (!Capture(null).SequenceEqual(off)) throw new Exception("Disabled guides still draw.");
        view.BackgroundRenderers.Remove(disabled); view.BackgroundRenderers.Add(guide);
        Console.WriteLine($"T008 PASS: checkbox rows all {heights[0]:F3} DIP; guide columns, blank continuation/end, tabs, code font and off setting verified; PNGs={output}");
    }
}

