// WCAG contrast calculations for opaque theme colors and their composited paints.
using Avalonia.Media;

namespace AntiNotepad.Theme;

/// <summary>Chooses colors that remain legible on a known surface.</summary>
internal static class Contrast
{
    public const double Text = 4.5;
    public const double Mark = 3;
    private static readonly Color DarkText = Color.Parse("#111111");

    /// <summary>Calculates WCAG relative luminance from sRGB channels.</summary>
    public static double Luminance(Color color)
    {
        static double Linear(byte value)
        {
            var channel = value / 255.0;
            return channel <= .04045 ? channel / 12.92 : Math.Pow((channel + .055) / 1.055, 2.4);
        }
        return .2126 * Linear(color.R) + .7152 * Linear(color.G) + .0722 * Linear(color.B);
    }

    /// <summary>Calculates the contrast ratio of two opaque colors.</summary>
    public static double Ratio(Color a, Color b)
    {
        var first = Luminance(a); var second = Luminance(b);
        return (Math.Max(first, second) + .05) / (Math.Min(first, second) + .05);
    }

    /// <summary>Composites an opaque foreground with a paint opacity over an opaque background.</summary>
    public static Color Over(Color fg, double alpha, Color bg) => Color.FromRgb(
        (byte)Math.Round(fg.R * alpha + bg.R * (1 - alpha)),
        (byte)Math.Round(fg.G * alpha + bg.G * (1 - alpha)),
        (byte)Math.Round(fg.B * alpha + bg.B * (1 - alpha)));

    /// <summary>Chooses the stronger of white and near-black text on a fill.</summary>
    public static Color BestText(Color fill) => Ratio(Colors.White, fill) >= Ratio(DarkText, fill) ? Colors.White : DarkText;

    /// <summary>Moves a foreground toward the opposing ink extreme until it meets the requested ratio.</summary>
    public static Color EnsureOn(Color fg, Color bg, double min)
    {
        var extreme = Luminance(bg) > .179 ? Colors.Black : Colors.White;
        for (var step = 0; step <= 100; step++)
        {
            var candidate = Over(extreme, step / 100.0, fg);
            if (Ratio(candidate, bg) >= min) return candidate;
        }
        return extreme;
    }

    /// <summary>Uses the accent for a field focus ring when it contrasts with the field.</summary>
    public static Color FieldFocus(Palette p) => Ratio(p.Accent, p.Surface) >= Mark ? p.Accent : p.Ink;

    /// <summary>Models the settings card over its canvas backing.</summary>
    public static Color SettingsCard(Palette p) => Over(p.Surface, Paint.CardSurface(p.Dark), p.Canvas);
}
