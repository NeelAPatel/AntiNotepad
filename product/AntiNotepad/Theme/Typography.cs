// Shared font families, weights and sizes for the app's text and icons.
using Avalonia.Media;

namespace AntiNotepad.Theme;

/// <summary>Names the typography values used throughout the app.</summary>
internal static class Typography
{
    /// <summary>Font families for interface text, monospaced text and editor fallback.</summary>
    public const string UiFamily = "Segoe UI Variable Text, Segoe UI";
    public const string MonoFamily = "Cascadia Mono, Consolas";
    public const string EditorFallback = ", Cascadia Mono, Consolas, monospace";

    /// <summary>Weights for regular, emphasized and bold text.</summary>
    public const FontWeight Regular = FontWeight.Normal;
    public const FontWeight Strong = FontWeight.SemiBold;
    public const FontWeight Bold = FontWeight.Bold;

    /// <summary>Text sizes from small labels through dialog and panel titles.</summary>
    public const double Micro = 12, Caption = 12, Small = 12, Label = 12, Body = 14,
        BodyEmphasis = 14, Large = 14, Input = 16, DialogTitle = 18, PanelTitle = 18, Display = 28;

    /// <summary>Glyph sizes used by interface icons.</summary>
    public const double IconBody = 14, IconSmall = 14, IconInput = 16, IconMedium = 16, IconLarge = 20, IconXLarge = 20;
}
