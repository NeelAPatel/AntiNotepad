// Frame-driven animation and easing shared by the UI.
using Avalonia.Controls;

namespace AntiNotepad.Motion;

/// <summary>Frame-driven tweens. Short, interruptible, and skipped entirely with reduced motion.</summary>
internal static class Anim
{
    public static bool Reduced { get; set; }

    public sealed class Handle { public bool Cancelled; public void Cancel() => Cancelled = true; }

    public static Handle Run(Control owner, double ms, Action<double> step, Action? done = null, bool decorative = true, Func<double, double>? easing = null)
    {
        var handle = new Handle();
        if ((Reduced && decorative) || ms <= 0 || TopLevel.GetTopLevel(owner) is not { } top)
        {
            step(1); done?.Invoke(); return handle;
        }
        var start = DateTime.UtcNow;
        void Frame(TimeSpan _)
        {
            if (handle.Cancelled) return;
            var t = Math.Clamp((DateTime.UtcNow - start).TotalMilliseconds / ms, 0, 1);
            step((easing ?? EaseOut)(t));
            if (t < 1) top.RequestAnimationFrame(Frame); else done?.Invoke();
        }
        step(0);
        top.RequestAnimationFrame(Frame);
        return handle;
    }

    /// <summary>Creates a CSS-style easing curve by solving its horizontal coordinate.</summary>
    public static Func<double, double> CubicBezier(double x1, double y1, double x2, double y2)
    {
        static double Coordinate(double t, double a, double b) => 3 * (1 - t) * (1 - t) * t * a + 3 * (1 - t) * t * t * b + t * t * t;
        static double Slope(double t, double a, double b) => 3 * (1 - t) * (1 - t) * a + 6 * (1 - t) * t * (b - a) + 3 * t * t * (1 - b);
        return x =>
        {
            if (x <= 0) return 0;
            if (x >= 1) return 1;
            var low = 0.0;
            var high = 1.0;
            var t = x;
            for (var i = 0; i < 32; i++)
            {
                var error = Coordinate(t, x1, x2) - x;
                if (Math.Abs(error) < 1e-9) break;
                if (error < 0) low = t; else high = t;
                var slope = Slope(t, x1, x2);
                var next = slope > 1e-9 ? t - error / slope : double.NaN;
                t = double.IsNaN(next) || next <= low || next >= high ? (low + high) / 2 : next;
            }
            return Coordinate(t, y1, y2);
        };
    }

    public static double EaseOut(double t) => 1 - Math.Pow(1 - t, 3);
}
