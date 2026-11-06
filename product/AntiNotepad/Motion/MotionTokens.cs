// Durations (milliseconds) and geometry for the app's animations.

namespace AntiNotepad.Motion;

/// <summary>Named durations and geometry for each animation role.</summary>
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

    /// <summary>Note-switch departure and arrival geometry.</summary>
    public const double NoteExit = 160, NoteArrivalShift = 34, NoteArrivalOpacity = 0.25;

    /// <summary>Settings page transitions.</summary>
    public const double SettingsPage = 160;
}
