using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace AntiQuickPad.Views.Common;

// Centre visible glyph ink, rather than the font's asymmetric line box.
internal sealed class OpticalText : TextBlock
{
    // Quarter-DIP coverage correction is verified at 1x/1.5x against the real footer pixels.
    public double OpticalOffset { get; set; }
    private string? _key;
    private double _inkCentre;
    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        var key = $"{Text}|{FontFamily}|{FontSize}|{FontWeight}";
        if (_key != key)
        {
            var probe = new TextBlock { Text = Text, FontFamily = FontFamily, FontSize = FontSize,
                FontWeight = FontWeight, Foreground = Brushes.White };
            probe.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            probe.Arrange(new Rect(probe.DesiredSize));
            var size = new PixelSize(Math.Max(1, (int)Math.Ceiling(probe.Bounds.Width * 6)), Math.Max(1, (int)Math.Ceiling(probe.Bounds.Height * 6)));
            using var bitmap = new RenderTargetBitmap(size, new Vector(576, 576)); bitmap.Render(probe);
            var bytes = new byte[size.Width * size.Height * 4]; var ptr = Marshal.AllocHGlobal(bytes.Length);
            try { bitmap.CopyPixels(new PixelRect(size), ptr, bytes.Length, size.Width * 4); Marshal.Copy(ptr, bytes, 0, bytes.Length); }
            finally { Marshal.FreeHGlobal(ptr); }
            var top = size.Height; var bottom = -1;
            for (var y = 0; y < size.Height; y++) for (var x = 0; x < size.Width; x++)
                if (bytes[(y * size.Width + x) * 4 + 3] >= 128) { top = Math.Min(top, y); bottom = Math.Max(bottom, y); }
            _inkCentre = bottom < 0 ? probe.Bounds.Height / 2 : (top + bottom + 1) / 12d;
            _key = key;
        }
        RenderTransform = new TranslateTransform(0, finalSize.Height / 2 - _inkCentre + OpticalOffset);
        return result;
    }
}



