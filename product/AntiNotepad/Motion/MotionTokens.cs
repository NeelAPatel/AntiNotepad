// Durations (milliseconds) and geometry for the app's animations.

using Avalonia.Animation.Easings;

namespace AntiNotepad.Motion;

/// <summary>Named durations and geometry for each animation role.</summary>
internal static class MotionTokens
{
    /// <summary>Pad window and overlay transitions.</summary>
    public const double WindowSlide = 160, ContentReveal = 160, WindowFade = 160;

    // Microsoft Learn: Timing and easing, Windows 11 control durations and curves.
    public const double Win11Faster = 83, Win11Fast = 167, Win11Normal = 250;
    public const double OverlayIn = Win11Fast, OverlayOut = Win11Faster, OverlayRise = 16;
    public static readonly Func<double, double> Win11Enter = Anim.CubicBezier(0, 0, 0, 1);
    public static readonly Func<double, double> Win11Exit = Anim.CubicBezier(1, 0, 1, 1);
    /// <summary>The same enter curve as an Avalonia easing, for property transitions.</summary>
    public static readonly Easing Win11EnterEasing = new SplineEasing(0, 0, 0, 1);

    /// <summary>Chrome and feedback transitions.</summary>
    public const double BarsReveal = 160, PillFade = 160, ChromeShift = 200, Nudge = 200;

    /// <summary>Pinned window ring fade in either direction.</summary>
    public const double PinRingFade = 250;

    /// <summary>Editor and note transitions.</summary>
    public const double NoteArrive = 200, CaretGlide = 80;

    /// <summary>Horizontal paging and edge-pull timing.</summary>
    public const double EdgeBandRatio = 0.55, EdgeSpringPeriod = 420, EdgeSpringDamping = 1;
    public const double EdgeWheelGlide = 70, EdgeWheelHold = 220, EdgeTouchQuiet = 90;
    public const double HorizontalProjection = 499, HorizontalCommitShare = 0.5, HorizontalQuiet = EdgeTouchQuiet;
    public const double HorizontalVelocityWindow = 60, HorizontalVelocityMinimumSpan = 16, HorizontalVelocityStale = 80;
    public const double HorizontalWheelLock = 260;
    public const double HorizontalSpentQuiet = 150, HorizontalWheelNudge = 192, HorizontalPageGap = 16;
    public const double TouchpadPixelsPerUnit = 50;

    /// <summary>Programmatic note-arrival geometry.</summary>
    public const double NoteArrivalShift = 34, NoteArrivalOpacity = 0.25;

    /// <summary>Settings page transitions.</summary>
    public const double SettingsPage = 160;
    public const double ToggleRest = 12, ToggleHover = 14, TogglePressedWidth = 17;
}
