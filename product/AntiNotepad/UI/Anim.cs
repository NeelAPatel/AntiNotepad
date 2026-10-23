using Avalonia.Controls;

namespace AntiNotepad.UI;

/// <summary>Frame-driven tweens. Short, interruptible, and skipped entirely with reduced motion.</summary>
internal static class Anim
{
    public static bool Reduced { get; set; }

    public sealed class Handle { public bool Cancelled; public void Cancel() => Cancelled = true; }

    public static Handle Run(Control owner, double ms, Action<double> step, Action? done = null, bool decorative = true)
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
            step(EaseOut(t));
            if (t < 1) top.RequestAnimationFrame(Frame); else done?.Invoke();
        }
        step(0);
        top.RequestAnimationFrame(Frame);
        return handle;
    }

    public static double EaseOut(double t) => 1 - Math.Pow(1 - t, 3);
}
