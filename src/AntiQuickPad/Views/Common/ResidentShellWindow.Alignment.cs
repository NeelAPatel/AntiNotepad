using System.Runtime.InteropServices;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;

namespace AntiQuickPad.Views.Common;

public sealed partial class ResidentShellWindow
{
    private async Task AlignmentPixels(string output)
    {
        Directory.CreateDirectory(output);
        while (_vm.Notes.Count < 8) _vm.CreateCommand.Execute(null);
        for (var i = 0; i < _vm.Notes.Count; i++) { _vm.Select(i); _editor.Text = "# Same note\nkeep this caret\n- [ ] task"; }
        _vm.Select(2); SaveNotes();
        _barReveal?.CaptureState(true);
        await Task.Delay(80); UpdateLayout();
        var rootSize = _root.Bounds.Size;
        _root.Width = rootSize.Width; _root.Height = rootSize.Height; _frame.Child = null;
        var capture = new Viewbox { Stretch = Stretch.Fill, Child = _root }; Content = capture;
        var results = new List<object>();
        foreach (var scale in new[] { 1d, 1.5d })
        {
            var width = (int)Math.Ceiling(_root.Bounds.Width * scale);
            var height = (int)Math.Ceiling(_root.Bounds.Height * scale);
            byte[] Capture()
            {
                capture.Measure(new Size(width, height)); capture.Arrange(new Rect(0, 0, width, height));
                using var bitmap = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));
                bitmap.Render(capture);
                var bytes = new byte[width * height * 4]; var ptr = Marshal.AllocHGlobal(bytes.Length);
                try { bitmap.CopyPixels(new PixelRect(new PixelSize(width, height)), ptr, bytes.Length, width * 4); Marshal.Copy(ptr, bytes, 0, bytes.Length); }
                finally { Marshal.FreeHGlobal(ptr); }
                return bytes;
            }
            Rect Region(Control c) { var p = c.TranslatePoint(default, _root)!.Value; return new Rect(p, c.Bounds.Size); }
            Rect Difference(byte[] on, byte[] off, Rect region, int threshold = 80)
            {
                var left = width; var top = height; var right = -1; var bottom = -1;
                for (var y = Math.Max(0, (int)Math.Floor(region.Top * scale)); y < Math.Min(height, Math.Ceiling(region.Bottom * scale)); y++)
                for (var x = Math.Max(0, (int)Math.Floor(region.Left * scale)); x < Math.Min(width, Math.Ceiling(region.Right * scale)); x++)
                {
                    var i = (y * width + x) * 4;
                    if (Math.Abs(on[i] - off[i]) + Math.Abs(on[i+1] - off[i+1]) + Math.Abs(on[i+2] - off[i+2]) < threshold) continue;
                    left = Math.Min(left, x); right = Math.Max(right, x); top = Math.Min(top, y); bottom = Math.Max(bottom, y);
                }
                Check(right >= left, "T-015-alignment-fixes: no raster difference");
                return new Rect(left / scale, top / scale, (right - left + 1) / scale, (bottom - top + 1) / scale);
            }
            Rect Ink(Control c)
            {
                var on = Capture(); var opacity = c.Opacity; c.Opacity = 0; var off = Capture(); c.Opacity = opacity;
                return Difference(on, off, Region(c).Inflate(20), c is Border ? 8 : 80);
            }
            void Crop(byte[] bytes, Rect rect, string name)
            {
                var x = Math.Max(0, (int)Math.Floor(rect.X * scale)); var y = Math.Max(0, (int)Math.Floor(rect.Y * scale));
                var w = Math.Min(width - x, (int)Math.Ceiling(rect.Width * scale)); var h = Math.Min(height - y, (int)Math.Ceiling(rect.Height * scale));
                using var bitmap = new WriteableBitmap(new PixelSize(w,h), new Vector(96 * scale,96 * scale), Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Premul);
                using (var buffer = bitmap.Lock()) for (var row = 0; row < h; row++) Marshal.Copy(bytes, ((y + row) * width + x) * 4, buffer.Address + row * buffer.RowBytes, w * 4);
                bitmap.Save(Path.Combine(output, name + (scale == 1 ? "@1x.png" : "@1.5x.png")), PngBitmapEncoderOptions.Default);
            }
            var count = Ink(_layout.Count);
            var previous = Ink((Control)_layout.Buttons["Previous"].Content!);
            var next = Ink((Control)_layout.Buttons["Next"].Content!);
            var save = Ink(_layout.Save);
            var reference = previous.Center.Y;
            Console.WriteLine($"T-015-alignment-fixes {_shellId} @{scale} count={count.Center.Y} prev={previous.Center.Y} next={next.Center.Y} save={save.Center.Y} pager={reference}");
            var delta = Math.Abs(count.Center.Y - reference);
            Check(delta <= .5, $"T-015-alignment-fixes {_shellId} @{scale}: count centre {count.Center.Y} vs reference {reference} delta {delta}");
            Check(Math.Abs(next.Center.Y - previous.Center.Y) <= .5, "Footer arrow ink mismatch");
            results.Add(new { scale, countY = count.Center.Y, previousY = previous.Center.Y, nextY = next.Center.Y, saveY = save.Center.Y, pagerY = reference, delta });
            // M11b-settings-window: the in-tree menu (and its raster proofs) moved to the settings window.
            Crop(Capture(), new Rect(0, _root.Bounds.Height - 78, _root.Bounds.Width, 78), "footer");
        }
        File.WriteAllText(Path.Combine(output, "measurements.json"), JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"T-015-alignment-fixes {_shellId}/{(_theme.IsDark ? "dark" : "light")} raster PASS: {output}");
        capture.Child = null; _root.Width = _root.Height = double.NaN; _frame.Child = _root; Content = _frame; UpdateLayout();
        _barReveal?.CaptureState(null);
    }
}











