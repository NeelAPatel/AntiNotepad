// Shared corner radii, strokes and popup shadows for interface shapes.
namespace AntiNotepad.Theme;

/// <summary>Names the shape values used throughout the interface.</summary>
internal static class Shape
{
    /// <summary>Corner radii for caret samples, keys, controls, popups, cards and dialogs.</summary>
    public const double Hairline = 2, Key = 4, Control = 4, Popup = 8, Card = 8, Dialog = 12;

    /// <summary>Strokes for borders and drawn interface marks.</summary>
    public const double Thin = 1, Thick = 2;

    /// <summary>Checkbox drawing values stay proportional to the editor text size.</summary>
    public const double CheckboxSizeScale = 0.86; // The checkbox mark follows editor text size.
    public const double CheckboxRadiusScale = 0.28; // The checkbox curve follows its drawn size.
    public const double CheckboxAccentPen = 1.6; // Checkbox artwork scales with editor text size.
    public const double CheckboxCheckPen = 1.9; // Checkbox artwork scales with editor text size.

    /// <summary>Radius for fully round controls and marks.</summary>
    public static double Round(double size) => size / 2;

    /// <summary>Shadow geometry and alpha for menus and dropdown popups.</summary>
    public const double PopupShadowOffsetY = 4, MenuShadowBlur = 16, DropdownShadowBlur = 16;
    public const byte PopupShadowDarkAlpha = 110, PopupShadowLightAlpha = 40;
}
