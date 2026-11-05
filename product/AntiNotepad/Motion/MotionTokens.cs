// Durations for the app's animations, in milliseconds.

namespace AntiNotepad.Motion;

/// <summary>Named durations for each animation role.</summary>
internal static class MotionTokens
{
    /// <summary>Pad window and overlay transitions.</summary>
    public const double WindowSlide = 160, ContentReveal = 160, WindowFade = 160, OverlayIn = 120;

    /// <summary>Chrome and feedback transitions.</summary>
    public const double BarsReveal = 160, PillFade = 160, ChromeShift = 200, Nudge = 200;

    /// <summary>Toast transitions.</summary>
    public const double ToastIn = 120, ToastOut = 160;

    /// <summary>Editor and note transitions.</summary>
    public const double NoteArrive = 200, SwipeSpringBack = 200, CaretGlide = 80;

    /// <summary>Settings page transitions.</summary>
    public const double SettingsPage = 160;
}
