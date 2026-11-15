// Horizontal paging and edge-pull feedback shared by the pad and Settings preview.
using Avalonia.Controls;
using Avalonia.Media;
using AntiNotepad.Editor;
using AntiNotepad.Motion;

namespace AntiNotepad.UI;

/// <summary>Tracks horizontal input, commits projected swipes, and springs edge pulls home.</summary>
internal sealed class NoteSwipe(Control host, Func<Control> pane, Func<int, bool> canNavigate, Action<int> navigate)
{
    private readonly List<(long At, double Dx)> _events = [];
    private readonly EdgeBand _edge = new();
    private readonly NotchGainClock _notches = new();
    private double _pull, _springVelocity;
    private long _lastInputDelta, _lastWheelLock;
    private double _lastDelta;
    private bool _spent, _returning, _frameRunning;
    private TimeSpan? _lastFrame;
    private int _frameGeneration;
    private Avalonia.Threading.DispatcherTimer? _quiet;

    /// <summary>How far the note is pulled, as a signed share of its width (for live traces).</summary>
    public event Action<double>? Moved;

    internal bool HasRunningTimer => _quiet?.IsEnabled == true || _frameRunning;
    internal bool HasPendingProgress => _pull != 0 || _edge.Active;
    internal double OwedMotion => _edge.Pull;

    /// <summary>One horizontal wheel or touchpad event, in Avalonia's wheel units.</summary>
    public void Input(double units)
    {
        if (Math.Abs(units) < 0.001) return;
        var now = Environment.TickCount64;
        var dx = units * MotionTokens.TouchpadPixelsPerUnit;
        if (IsWheelClick(units))
        {
            var gain = _notches.Next(now);
            var direction = -Math.Sign(units);
            if (canNavigate(direction))
            {
                if (now < _lastWheelLock) return;
                _lastWheelLock = now + (long)MotionTokens.HorizontalWheelLock;
                Commit(direction, 0);
            }
            else if (!Anim.Reduced)
            {
                _edge.Push(Math.Sign(dx) * MotionTokens.HorizontalWheelNudge * gain, Width, wheel: true, now);
                _pull = _edge.Pull;
                _returning = false;
                Show(_edge.Shown);
                RestartQuiet(MotionTokens.EdgeWheelHold);
                StartFrame();
            }
            return;
        }

        if (_spent)
        {
            var fresh = Math.Abs(dx) > Math.Abs(_lastDelta) * 1.6 && Math.Abs(dx) > 4
                || Math.Sign(dx) != Math.Sign(_lastDelta) && Math.Abs(dx) > 2;
            var gap = now - _lastInputDelta;
            _lastDelta = dx;
            _lastInputDelta = now;
            if (gap > MotionTokens.HorizontalSpentQuiet || fresh) _spent = false;
            else return;
        }
        _lastDelta = dx;
        _lastInputDelta = now;

        if (_edge.Active)
        {
            var next = _edge.CurrentPull(Width) + dx;
            if (Math.Abs(next) > 0.01 && canNavigate(-Math.Sign(next)))
            {
                _edge.Reset();
                _pull = next;
                Show(_pull);
            }
            else
            {
                _edge.Push(dx, Width, wheel: false, now);
                _pull = _edge.Pull;
                Show(_edge.Shown);
            }
        }
        else
        {
            _pull += dx;
            if (Math.Abs(_pull) > 0.01 && !canNavigate(-Math.Sign(_pull)))
            {
                _edge.Push(_pull, Width, wheel: false, now);
                _pull = _edge.Pull;
                Show(_edge.Shown);
            }
            else Show(_pull);
        }

        _returning = false;
        _events.Add((now, dx));
        _events.RemoveAll(e => now - e.At > 200);
        RestartQuiet(MotionTokens.HorizontalQuiet);
        if (!_edge.Active && IsDecelerating() && ShouldCommit(_pull, ReleaseVelocity(now), Width))
        {
            var direction = -Math.Sign(_pull);
            if (canNavigate(direction)) Commit(direction, ReleaseVelocity(now));
        }
    }

    private double Width => Math.Max(1, pane().Bounds.Width);

    private static bool IsWheelClick(double units) => Math.Abs(units - Math.Round(units)) < 0.01;

    /// <summary>Applies the locked projection rule to the displayed position and release velocity.</summary>
    internal static bool ShouldCommit(double x, double velocity, double width)
    {
        var projected = x + velocity * MotionTokens.HorizontalProjection;
        return width > 0 && Math.Sign(projected) == Math.Sign(x)
            && Math.Abs(projected) >= width * MotionTokens.HorizontalCommitShare;
    }

    /// <summary>Starts the quiet check after the last touchpad event.</summary>
    private void RestartQuiet(double milliseconds)
    {
        _quiet ??= new Avalonia.Threading.DispatcherTimer();
        _quiet.Interval = TimeSpan.FromMilliseconds(milliseconds);
        _quiet.Tick -= OnQuiet;
        _quiet.Tick += OnQuiet;
        _quiet.Stop();
        _quiet.Start();
    }

    private void OnQuiet(object? sender, EventArgs e)
    {
        _quiet?.Stop();
        if (_edge.Active) { StartFrame(); return; }
        if (_pull == 0) return;
        var now = Environment.TickCount64;
        var velocity = ReleaseVelocity(now);
        var direction = -Math.Sign(_pull);
        if (!Anim.Reduced && canNavigate(direction) && ShouldCommit(_pull, velocity, Width)) Commit(direction, velocity);
        else StartReturn(Anim.Reduced ? 0 : velocity);
    }

    private bool IsDecelerating()
    {
        if (_events.Count < 6) return false;
        var recent = _events.TakeLast(6).Select(e => Math.Abs(e.Dx)).ToArray();
        var first = (recent[0] + recent[1] + recent[2]) / 3;
        var last = (recent[3] + recent[4] + recent[5]) / 3;
        var sign = Math.Sign(_events[^1].Dx);
        return last < first * 0.92 && _events.TakeLast(6).All(e => Math.Sign(e.Dx) == sign);
    }

    private double ReleaseVelocity(long now)
    {
        if (_events.Count == 0 || now - _events[^1].At > MotionTokens.HorizontalVelocityStale) return 0;
        var first = now - MotionTokens.HorizontalVelocityWindow;
        var samples = _events.Where(e => e.At > first && e.At <= now).ToArray();
        if (samples.Length == 0) return 0;
        var start = samples.Min(e => e.At);
        var span = Math.Max(MotionTokens.HorizontalVelocityMinimumSpan, samples[^1].At - start);
        return samples.Sum(e => e.Dx) / span;
    }

    /// <summary>Schedules frames while an edge glide or spring is active.</summary>
    private void StartFrame()
    {
        if (_frameRunning) return;
        _frameRunning = true;
        _lastFrame = null;
        Frame();
    }

    private void Frame()
    {
        if (!_frameRunning || TopLevel.GetTopLevel(host) is not { } top) { _frameRunning = false; _lastFrame = null; return; }
        var generation = _frameGeneration;
        var weak = new WeakReference<NoteSwipe>(this);
        top.RequestAnimationFrame(time =>
        {
            if (!weak.TryGetTarget(out var swipe) || generation != swipe._frameGeneration || !swipe._frameRunning) return;
            if (Anim.Reduced)
            {
                swipe._edge.Reset();
                swipe._pull = 0;
                swipe._returning = false;
                swipe.Show(0);
                swipe.StopFrames();
                return;
            }

            var dt = swipe._lastFrame is { } last ? Math.Clamp((time - last).TotalSeconds, 0, 0.05) : 1 / 60.0;
            swipe._lastFrame = time;
            if (swipe._edge.Active)
            {
                swipe._edge.Step(Environment.TickCount64, dt, swipe.Width);
                swipe._pull = swipe._edge.Active ? swipe._edge.Pull : 0;
                swipe.Show(swipe._edge.Active ? swipe._edge.Shown : 0);
                if (swipe._edge.Active) swipe.Frame(); else swipe.StopFrames();
                return;
            }

            if (swipe._returning)
            {
                swipe._pull = EdgeBand.SpringStep(swipe._pull, ref swipe._springVelocity, 0, dt);
                swipe.Show(swipe._pull);
                if (Math.Abs(swipe._pull) < 0.3 && Math.Abs(swipe._springVelocity) < 10)
                {
                    swipe._pull = 0;
                    swipe._returning = false;
                    swipe.Show(0);
                    swipe.StopFrames();
                }
                else swipe.Frame();
                return;
            }

            swipe.StopFrames();
        });
    }

    private void StopFrames()
    {
        _frameGeneration++;
        _frameRunning = false;
        _lastFrame = null;
    }

    private void StartReturn(double velocity)
    {
        if (Anim.Reduced)
        {
            _edge.Reset();
            _pull = 0;
            Show(0);
            return;
        }
        _returning = true;
        _springVelocity = velocity * 1000;
        StartFrame();
    }

    /// <summary>Gives the gesture ownership of the pane instead of competing with an earlier animation.</summary>
    private void Show(double x)
    {
        var target = pane();
        if (target is PadView pad) pad.StopNoteMotion();
        if (target.RenderTransform is not TranslateTransform follow) target.RenderTransform = follow = new TranslateTransform();
        follow.X = x;
        target.Opacity = 1;
        Moved?.Invoke(x / Math.Max(1, target.Bounds.Width));
    }

    /// <summary>Commits the current displayed pull and release speed to the note transition.</summary>
    private void Commit(int direction, double velocity)
    {
        if (!canNavigate(direction)) { StartReturn(0); return; }
        _quiet?.Stop();
        StopFrames();
        _spent = true;
        _lastInputDelta = Environment.TickCount64;
        _edge.Reset();
        var target = pane();
        var x = (target.RenderTransform as TranslateTransform)?.X ?? 0;
        if (target is PadView pad) pad.PrepareSwipe(x, 1, velocity);
        target.RenderTransform = null;
        target.Opacity = 1;
        _pull = 0;
        _events.Clear();
        Moved?.Invoke(0);
        try { navigate(direction); }
        finally { if (target is PadView prepared) prepared.DiscardSwipe(); }
    }

    /// <summary>Stops pending motion when its host is released and restores even a detached pane.</summary>
    public void Cancel()
    {
        _quiet?.Stop();
        StopFrames();
        _edge.Reset();
        _notches.Reset();
        _pull = _springVelocity = 0;
        _lastInputDelta = _lastWheelLock = 0;
        _lastDelta = 0;
        _events.Clear();
        _spent = _returning = false;

        var target = pane();
        if (target is PadView pad) pad.StopNoteMotion();
        target.RenderTransform = null;
        target.Opacity = 1;
        Moved?.Invoke(0);
    }
}
