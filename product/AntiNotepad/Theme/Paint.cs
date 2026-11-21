// Paint strengths shared by UI and editor drawing.
namespace AntiNotepad.Theme;

/// <summary>Provides paint strengths by visual role.</summary>
internal static class Paint
{
    /// <summary>Theme-dependent paint strengths.</summary>
    public static double Hover(bool dark) => dark ? .09 : .06;
    public static double Press(bool dark) => dark ? .15 : .11;
    public static double Selected(bool dark) => dark ? .22 : .18;
    public static double CardSurface(bool dark) => dark ? .9 : .9;
    public static double HelpWash(bool dark) => dark ? .09 : .06;
    public static double PreviewFrame(bool dark) => dark ? .09 : .06;
    public static double PreviewWell(bool dark) => dark ? .09 : .06;
    public static double RowWash(bool dark) => dark ? .03 : .03;
    public static double CompletionFill(bool dark) => dark ? .09 : .06;
    public static double BadgeFill(bool dark) => dark ? .15 : .11;

    /// <summary>Paint strengths for fixed roles.</summary>
    public const double PopupBorder = .5;
    public const double FieldBorder = .5;
    public const double ResultBorder = .3;
    public const double ResultCurrent = .9;
    public const double ResultSurface = .5;
    public const double Dimmed = .5;
    public const double DisabledIcon = .5;
    public const double FocusBorder = .9;
    public const double RestBorder = .55;
    public const double ErrorHint = .8;
    public const double Toast = .9;
    public const double GlassScrim = .5;
    public const double GlassPreview = .5;
    public const double MenuCaption = .8;
    public const double MenuSeparator = .3;
    public const double CaretBlock = .5;
    public const double MarkerFaint = .3;
    public const double MarkerStrong = .8;
    public const double KeyPen = .5;
    public const double AccentPen = .9;
    public const double HoverFace = .9;
    public const double CardBorderOn = .9;
    public const double CardBorderOff = .3;
    public const double GridPen = .9;
    public const double PreviewBar = .09;
    public const double PreviewPad = .8;

    /// <summary>Alpha values for glyph and sample colors.</summary>
    public const byte IconInkAlpha = 220;
    public const byte PinnedGlyphAlpha = 110;
    public const byte SampleGradientAlpha = 90;
    public const byte SampleFadeAlpha = 40;
}
