using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AntiQuickPad.Design;

internal sealed class WallpaperWindow : Window
{
    internal WallpaperWindow()
    {
        Title = "AntiQuickPad synthetic capture backdrop";
        WindowDecorations = WindowDecorations.None;
        WindowStartupLocation = WindowStartupLocation.Manual;
        ShowActivated = false;
        ShowInTaskbar = false;
        Topmost = true;
        CanResize = false;
        Background = new SolidColorBrush(Color.Parse("#18232C"));
        Content = new WallpaperSurface();
    }

    internal NativeRect NativeBounds => ScreenCapture.WindowRect(this);

    internal void FitToPrimary()
    {
        var screen = Screens.Primary ?? Screens.All.FirstOrDefault();
        if (screen is null) throw new InvalidOperationException("No primary screen is available for the synthetic backdrop.");
        Width = screen.Bounds.Width / screen.Scaling;
        Height = screen.Bounds.Height / screen.Scaling;
        Position = new PixelPoint(screen.Bounds.X, screen.Bounds.Y);
    }
}

internal sealed class WallpaperSurface : Control
{
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var w = Bounds.Width; var h = Bounds.Height;
        context.DrawRectangle(new SolidColorBrush(Color.Parse("#18232C")), null, new Rect(0, 0, w, h));
        context.DrawEllipse(new SolidColorBrush(Color.Parse("#D37D5F"), .78), null, new Point(w * .18, h * .31), w * .34, h * .28);
        context.DrawEllipse(new SolidColorBrush(Color.Parse("#397A92"), .80), null, new Point(w * .84, h * .24), w * .37, h * .32);
        context.DrawEllipse(new SolidColorBrush(Color.Parse("#B29A55"), .70), null, new Point(w * .67, h * .85), w * .32, h * .28);
        context.DrawRectangle(new SolidColorBrush(Color.Parse("#F2D5A4"), .76), null, new Rect(w * .04, h * .70, w * .24, h * .12));
        context.DrawRectangle(new SolidColorBrush(Color.Parse("#102F43"), .90), new Pen(new SolidColorBrush(Color.Parse("#85D1DD")), 4),
            new Rect(w * .53, h * .15, w * .23, h * .21), 10, 10);
        context.DrawLine(new Pen(new SolidColorBrush(Color.Parse("#F9E4BD"), .82), 3), new Point(w * .06, h * .12), new Point(w * .43, h * .49));
        context.DrawLine(new Pen(new SolidColorBrush(Color.Parse("#A2D7D0"), .80), 3), new Point(w * .78, h * .62), new Point(w * .96, h * .92));
        context.DrawLine(new Pen(new SolidColorBrush(Color.Parse("#EFA98B"), .78), 2), new Point(w * .34, h * .86), new Point(w * .62, h * .55));
        var type = new Typeface(new FontFamily("Segoe UI Variable Text"), FontStyle.Normal, FontWeight.SemiBold);
        DrawText(context, "SYNTHETIC / WALLPAPER", type, 34, new Point(w * .08, h * .18), Color.Parse("#FFF0D0"));
        DrawText(context, "ANTIQUICKPAD", type, 22, new Point(w * .58, h * .76), Color.Parse("#D9F1ED"));
        DrawText(context, "colour fields · lines · shapes", type, 16, new Point(w * .08, h * .88), Color.Parse("#D7E8E7"));
    }

    private static void DrawText(DrawingContext context, string text, Typeface typeface, double size, Point origin, Color color)
    {
        var formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, size,
            new SolidColorBrush(color));
        context.DrawText(formatted, origin);
    }
}
