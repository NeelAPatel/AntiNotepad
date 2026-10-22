using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace AntiQuickPad.Editor;

internal static class StrikeProof
{
    public static async Task Run(TextEditor editor, string output, Look? palette = null)
    {
        var look = palette ?? Look.Default;
        editor.Text = "plain ~~strike~~ **~~bold strike~~** *~~italic strike~~*";
        editor.CaretOffset = 0;
        await Task.Delay(100);
        editor.UpdateLayout();
        var view = editor.TextArea.TextView;
        view.EnsureVisualLines();
        var size = new PixelSize((int)Math.Ceiling(editor.Bounds.Width), (int)Math.Ceiling(editor.Bounds.Height));
        byte[] Capture(string? path)
        {
            using var bitmap = new RenderTargetBitmap(size, new Vector(96, 96));
            bitmap.Render(editor);
            if (path is not null) bitmap.Save(path, PngBitmapEncoderOptions.Default);
            var bytes = new byte[size.Width * size.Height * 4];
            var ptr = Marshal.AllocHGlobal(bytes.Length);
            try { bitmap.CopyPixels(new PixelRect(size), ptr, bytes.Length, size.Width * 4); Marshal.Copy(ptr, bytes, 0, bytes.Length); }
            finally { Marshal.FreeHGlobal(ptr); }
            return bytes;
        }
        var visual = view.VisualLines.Single();
        foreach (var (offset, weight, style) in new[] {
            (0, FontWeight.Normal, FontStyle.Normal), (8, FontWeight.Normal, FontStyle.Normal),
            (20, FontWeight.Bold, FontStyle.Normal), (39, FontWeight.Normal, FontStyle.Italic) })
        {
            var props = visual.Elements.Single(e => e.RelativeTextOffset <= offset && e.RelativeTextOffset + e.DocumentLength > offset).TextRunProperties;
            if (props.Typeface.Weight != weight || props.Typeface.Style != style)
                throw new Exception($"Strike typeface mismatch at {offset}: {props.Typeface}");
        }
        editor.TextArea.Caret.Hide();
        Console.WriteLine("T007 typefaces PASS: plain/strike Regular; bold strike Bold; italic strike Italic.");
        var overlay = Capture(output);
        var strike = view.BackgroundRenderers.OfType<StrikeRenderer>().Single();
        var origin = view.TranslatePoint(default, editor)!.Value;
        var expectedBands = strike.StrokeBounds.Select(r => r.Translate(new Vector(origin.X, origin.Y))).ToArray();
        byte[] control;
        view.BackgroundRenderers.Remove(strike); view.InvalidateLayer(KnownLayer.Selection);
        try { await Task.Delay(80); control = Capture(null); }
        finally { view.BackgroundRenderers.Add(strike); view.InvalidateLayer(KnownLayer.Selection); }
        Rect Region(int start, int length) => BackgroundGeometryBuilder.GetRectsForSegment(view, new SimpleSegment(start, length)).Single().Translate(new Vector(origin.X, origin.Y));
        var plain = Region(0, 5); var struck = Region(8, 6);
        bool Different(int x, int y)
        {
            var i = (y * size.Width + x) * 4;
            return Math.Abs(overlay[i] - control[i]) + Math.Abs(overlay[i + 1] - control[i + 1]) + Math.Abs(overlay[i + 2] - control[i + 2]) > 8;
        }
        var rows = new HashSet<int>(); var maxHeight = 0;
        for (var x = (int)Math.Ceiling(struck.Left); x < (int)struck.Right; x++)
        {
            var count = 0;
            for (var y = (int)struck.Top; y < (int)Math.Ceiling(struck.Bottom); y++)
                if (Different(x, y)) { count++; rows.Add(y); }
            maxHeight = Math.Max(maxHeight, count);
        }
        // Compare mean glyph ink coverage above 10%, excluding the strike rows.
        // Allow 0.10 difference for the distinct letter shapes in plain versus strike.
        double Ink(Rect rect)
        {
            var values = new List<double>();
            for (var y = (int)Math.Ceiling(rect.Top); y < (int)rect.Bottom; y++)
            for (var x = (int)Math.Ceiling(rect.Left); x < (int)rect.Right; x++)
            {
                if (rows.Contains(y)) continue;
                var i = (y * size.Width + x) * 4;
                var coverage = Math.Abs(overlay[i + 1] - look.Background.G) / (double)Math.Abs(look.Foreground.G - look.Background.G);
                if (coverage > .1) values.Add(coverage);
            }
            return values.Average();
        }
        var plainInk = Ink(plain); var strikeInk = Ink(struck);
        var difference = Math.Abs(plainInk - strikeInk);
        var outside = 0;
        // Compare all glyphs (including bold/italic) against the identical renderer-off control.
        // Bounds are predicted by the renderer geometry, not inferred from changed pixels.
        for (var y = 0; y < size.Height; y++)
        for (var x = 0; x < size.Width; x++)
            if (!expectedBands.Any(r => x + 1 > r.Left && x < r.Right && y + 1 > r.Top && y < r.Bottom) && Different(x, y)) outside++;
        Console.WriteLine($"T007 pixels: scale=1 size={size.Width}x{size.Height} strikeHeight={maxHeight}px affectedRows={string.Join(',', rows.Order())} plainInk={plainInk:F4} strikeInk={strikeInk:F4} delta={difference:F4} tolerance=0.10 glyphDifferenceOutsideStrike={outside}; PNG={output}");
        if (maxHeight < 2 || rows.Count > 4 || difference > .10 || outside != 0)
            throw new Exception("Strike bitmap thickness/glyph coverage check failed.");
    }
}
