// Shared corner radii and popup shadows for interface shapes.
namespace AntiNotepad.Theme;

/// <summary>Names the shape values used throughout the interface.</summary>
internal static class Shape
{
    /// <summary>Corner radii for caret samples, keys, controls, popups, cards and dialogs.</summary>
    public const double Hairline = 2, Key = 4, Control = 4, Popup = 8, Card = 8, Dialog = 12;

    /// <summary>Radius for fully round controls and marks.</summary>
    public static double Round(double size) => size / 2;

    /// <summary>Shadow geometry and alpha for menus and dropdown popups.</summary>
    public const double PopupShadowOffsetY = 4, MenuShadowBlur = 16, DropdownShadowBlur = 16;
    public const byte PopupShadowDarkAlpha = 110, PopupShadowLightAlpha = 40;
}
