using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AvaloniaEdit;
namespace AntiQuickPad.Editor;
internal static class ActiveGuideProof
{
    public static async Task Run(TextEditor editor, string output, Look? palette = null)
    {
        var look = palette ?? Look.Default;
        editor.Text = "- root\n    - child\n        - nested caret\n            - deeper\n        - sibling\n    - outside\n\n```python\ndef f():\n    if True:\n        return 1\n        return 2\n    return 0\n```\nend";
        editor.CaretOffset = 0; editor.ScrollToHome(); await Task.Delay(100); editor.UpdateLayout();
        var view = editor.TextArea.TextView; view.EnsureVisualLines();
        var guide = view.BackgroundRenderers.OfType<IndentGuideRenderer>().Single();
        var size = new PixelSize((int)Math.Ceiling(editor.Bounds.Width), (int)Math.Ceiling(editor.Bounds.Height));
        byte[] Capture()
        {
            using var bitmap = new RenderTargetBitmap(size, new Vector(96,96)); bitmap.Render(editor);
            var bytes = new byte[size.Width * size.Height * 4]; var ptr = Marshal.AllocHGlobal(bytes.Length);
            try { bitmap.CopyPixels(new PixelRect(size), ptr, bytes.Length, size.Width * 4); Marshal.Copy(ptr,bytes,0,bytes.Length); }
            finally { Marshal.FreeHGlobal(ptr); }
            return bytes;
        }
        editor.TextArea.Caret.Hide(); var baseline = Capture();
        var origin = view.TranslatePoint(default, editor)!.Value; var shots = new List<byte[]>();
        foreach (var f in new[] { (Line:3, First:3, Last:5), (Line:11, First:11, Last:12) })
        {
            editor.CaretOffset = editor.Document.GetLineByNumber(f.Line).EndOffset;
            await Task.Delay(80); editor.TextArea.Caret.Hide();
            var active = guide.ActiveBlock(editor.TextArea.Caret.Line);
            if (active != (2, f.First, f.Last)) throw new Exception($"Active block incorrect: {active}");
            var raster = Capture();
            var font = f.Line == 3 ? look.EditorFont : look.CodeFont;
            var spaces = new FormattedText("        ", System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, new Typeface(new FontFamily(font)), editor.FontSize, Brushes.White).WidthIncludingTrailingWhitespace;
            var xGuide = (int)Math.Floor(origin.X + spaces);
            var ys = view.VisualLines.Where(v => v.FirstDocumentLine.LineNumber >= f.First && v.FirstDocumentLine.LineNumber <= f.Last).ToArray();
            var y0 = (int)Math.Floor(origin.Y + ys[0].VisualTop - view.VerticalOffset);
            var y1 = (int)Math.Ceiling(origin.Y + ys[^1].VisualTop + ys[^1].Height - view.VerticalOffset);
            int changed = 0, other = 0, bright = 0;
            for (var y=0;y<size.Height;y++) for(var x=0;x<size.Width;x++)
            {
                var i=(y*size.Width+x)*4;
                if (raster.AsSpan(i,4).SequenceEqual(baseline.AsSpan(i,4))) continue;
                if (Math.Abs(x-xGuide)>1 || y<y0 || y>=y1) { other++; continue; }
                changed++;
                if ((raster[i]-baseline[i]) * (look.IndentGuideActive.B-look.IndentGuide.B) > 0 && (raster[i+1]-baseline[i+1]) * (look.IndentGuideActive.G-look.IndentGuide.G) > 0 && (raster[i+2]-baseline[i+2]) * (look.IndentGuideActive.R-look.IndentGuide.R) > 0) bright++;
            }
            if(changed==0 || bright!=changed || other!=0) throw new Exception($"Active pixels line={f.Line}: changed={changed}, brighter={bright}, outside={other}");
            var sample=((int)(origin.Y+ys[0].VisualTop-view.VerticalOffset+8)*size.Width+xGuide)*4;
            Console.WriteLine($"T009 active line={f.Line} column=8 block={f.First}-{f.Last} x={xGuide}: brighter={bright} pixels, other-guide/outside changes={other}; RGB {baseline[sample+2]},{baseline[sample+1]},{baseline[sample]} -> {raster[sample+2]},{raster[sample+1]},{raster[sample]}.");
            editor.TextArea.Caret.Show(); await Task.Delay(40); shots.Add(Capture()); editor.TextArea.Caret.Hide();
        }
        editor.CaretOffset=editor.Document.GetLineByNumber(15).Offset; await Task.Delay(80); editor.TextArea.Caret.Hide();
        if (guide.ActiveBlock(15).Level!=0 || !Capture().SequenceEqual(baseline)) throw new Exception("Unindented caret left active pixels.");
        using var combined = new WriteableBitmap(new PixelSize(size.Width*2,size.Height),new Vector(96,96),PixelFormat.Bgra8888,AlphaFormat.Premul);
        using (var buffer=combined.Lock())
            for (var y=0;y<size.Height;y++) for(var shot=0;shot<2;shot++)
                Marshal.Copy(shots[shot],y*size.Width*4,buffer.Address+y*buffer.RowBytes+shot*size.Width*4,size.Width*4);
        combined.Save(output, PngBitmapEncoderOptions.Default);
        Console.WriteLine($"T009 active PASS: unindented reset, exact block extent, unchanged other pixels; proof={output} size={size.Width*2}x{size.Height}.");
    }
}
