// Recommended spacing steps and stable exceptions used by interface surfaces.
namespace AntiNotepad.Theme;

/// <summary>Names spacing steps and measurements that cannot move without changing content layout.</summary>
internal static class Space
{
    /// <summary>The recommended gap scale, with a small alignment nudge.</summary>
    public const double Nudge = 2, Xs = 4, S = 8, M = 12, L = 16, Xl = 24, Xxl = 32;

    /// <summary>Editor insets preserve the user's text position and line wrapping.</summary>
    public const double EditorTextInsetLeft = 36; // Moving this inset changes the user's text position.
    public const double EditorTextInsetRight = 30; // A 32 px inset would rewrap existing lines.

    /// <summary>Editor hint insets keep the hint on the first text line.</summary>
    public const double EditorHintOffset = 1; // Aligns the hint glyph with editor text.
    public const double EditorHintTopOffset = 3; // Keeps the hint on the editor's first text line.
    public const double EditorHintLeftInset = EditorTextInsetLeft + EditorHintOffset;
    public const double EditorHintTopInset = Xs + EditorHintTopOffset;

    /// <summary>Small control and illustration measurements kept at their current geometry.</summary>
    public const double ToggleKnobInset = 3; // Centers the knob in the 40 by 20 track.
    public const double CaretBarWidth = 2; // Draws a real caret in the style sample.
    public const double CaretSampleMargin = 1; // Centers the real caret in its sample.

    /// <summary>Widths and sizes tied to text or sample artwork rather than the spacing scale.</summary>
    public const double CompletionModeColumnWidth = 70; // Fits the mode keyword text.
    public const double CompletionCommandColumnWidth = 86; // Fits the command keyword text.
    public const double HelpKeyColumnWidth = 150; // Fits shortcut names without wrapping.
    public const double MotionSampleHeight = 46; // Preserves the preview's sample chart proportions.
    public const double MotionSampleBarWidth = 14; // Preserves the preview's sample chart proportions.
    public const double MotionSampleBarHeight = 3; // Preserves the preview's sample chart proportions.
}
