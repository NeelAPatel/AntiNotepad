using Avalonia.Controls;
using Avalonia.Media;
using Antipad.Model;

namespace Antipad.UI;

/// <summary>
/// Swiping between notes with horizontal input (touchpad, tilt or thumb wheel, Shift+wheel). Two feels, from Settings > Feel:
/// detent: the note follows with rubber-band resistance and, when input stops, commits past a distance or speed, else springs back.
/// progress (picked in the Scroll Lab): the swipe is measured in its own distance, not the window's, so the same movement moves
/// the note the same share of the way at any width. Passing the trigger point changes the note at once and the rest of that
/// gesture (touchpad inertia, more wheel clicks) is ignored. Wheel clicks are paid out smoothly, further when spun faster.
/// The window uses one for its panes and the Settings preview another for its sample notes, so both feel the same.
/// </summary>
internal sealed class NoteSwipe(Control host, Func<Settings> settings, Func<Control> pane, Action<int> navigate)
{
    // detent
    private const double SwipePxPerUnit = 48, SwipeCommitPx = 90, SwipeCommitSpeed = 0.9 /* px per ms */, SwipeFarPx = 200;
    // progress: swipe length (px for a whole note) and the share where it changes come from Settings
    private const double ProgressClickPx = 26, ProgressPxPerUnit = 100;
    private const int ProgressQuietMs = 260, ProgressRearmMs = 110;

    private readonly List<(DateTime At, double Dx)> _samples = [];
    private double _swipe, _progress, _owed, _gap;
    private DateTime _at;
    private bool _fired, _draining;
    private long _click, _frameAt;
    private TimeSpan? _last;
    private Avalonia.Threading.DispatcherTimer? _detentSettle, _progressSettle;

    /// <summary>How far the note is pulled, as a signed share of its width (for live traces).</summary>
    public event Action<double>? Moved;

    /// <summary>One horizontal wheel or touchpad event, in Avalonia's wheel units.</summary>
    public void Input(double units)
    {
        var now = DateTime.Now;
        if (settings().Swipe == "progress") Progress(units, now);
        else Detent(units, now);
    }

    private void Detent(double units, DateTime now)
    {
        if ((now - _at).TotalMilliseconds > 350) { _swipe = 0; _fired = false; _samples.Clear(); }
        _at = now;
        if (_fired) return; // the rest of a committed gesture's inertia
        var dx = units * SwipePxPerUnit;
        _swipe += dx;
        _samples.Add((now, dx));
        _samples.RemoveAll(x => (now - x.At).TotalMilliseconds > 90);
        if (Math.Abs(_swipe) >= SwipeFarPx) { Fire(); return; }
        // Rubber band: the page follows less and less the further it is pulled.
        var shown = _swipe / (1 + Math.Abs(_swipe) / 260);
        Show(shown, 1 - Math.Abs(shown) / 420);
        _detentSettle ??= DetentSettle();
        _detentSettle.Stop(); _detentSettle.Start();
    }

    private Avalonia.Threading.DispatcherTimer DetentSettle()
    {
        var timer = new Avalonia.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(130) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            if (_fired) return;
            var span = _samples.Count > 1 ? Math.Max(16, (_samples[^1].At - _samples[0].At).TotalMilliseconds) : 90;
            var speed = Math.Abs(_samples.Sum(x => x.Dx)) / span;
            if (Math.Abs(_swipe) >= SwipeCommitPx || (speed >= SwipeCommitSpeed && Math.Abs(_swipe) >= SwipeCommitPx / 2)) { Fire(); return; }
            SpringBack();
        };
        return timer;
    }

    private void Progress(double units, DateTime now)
    {
        // After a change, a short pause re-arms the next swipe; before one, slow wheel clicks keep adding up for longer.
        if ((now - _at).TotalMilliseconds > (_fired ? ProgressRearmMs : ProgressQuietMs)) { _progress = 0; _owed = 0; _fired = false; }
        _at = now;
        if (_fired) return;
        var click = Math.Abs(units - Math.Round(units)) < 0.01;
        if (!click) Advance(units * ProgressPxPerUnit);
        else
        {
            var tick = Environment.TickCount64;
            var gap = _click == 0 ? 1000 : tick - _click;
            _click = tick;
            _gap = gap >= 400 || _gap <= 0 ? Math.Min(gap, 400) : _gap * 0.4 + gap * 0.6;
            var gain = 1 + 2 * Math.Clamp((1000 / _gap - 6) / 24, 0, 1);
            _owed += units * ProgressClickPx * gain;
            // A hidden window gets no frames, so a pay-out cut off by hiding is restarted rather than trusted.
            if (!_draining || tick - _frameAt > 150) { _draining = true; _last = null; _frameAt = tick; Frame(); }
        }
        _progressSettle ??= ProgressSettle();
        _progressSettle.Stop(); _progressSettle.Start();
    }

    private void Frame()
    {
        if (TopLevel.GetTopLevel(host) is not { } top) { _draining = false; return; }
        top.RequestAnimationFrame(time =>
        {
            var dt = _last is { } last ? Math.Clamp((time - last).TotalSeconds, 0, 0.05) : 1 / 60.0;
            _last = time;
            _frameAt = Environment.TickCount64;
            if (_fired || Math.Abs(_owed) < 0.01) { _owed = 0; _draining = false; return; }
            var part = _owed * (1 - Math.Exp(-22 * dt));
            if (Math.Abs(_owed - part) < 0.5) part = _owed;
            _owed -= part;
            Advance(part);
            Frame();
        });
    }

    private void Advance(double px)
    {
        if (_fired) return;
        var s = settings();
        _progress += px / Math.Max(60, s.SwipeLength);
        if (Math.Abs(_progress) >= Math.Clamp(s.SwipeFiresAt, 0.1, 0.9)) { _swipe = _progress; _owed = 0; Fire(); return; }
        // Nearly 1:1 for the first half note, then a rubber band (never reached before it fires at the defaults).
        var a = Math.Abs(_progress);
        var share = Math.Sign(_progress) * (a <= 0.5 ? a : 0.5 + (1 - 1 / ((a - 0.5) * 0.55 / 0.5 + 1)) * 0.5);
        Show(share * pane().Bounds.Width, 1 - Math.Min(0.7, Math.Abs(share) * 1.2));
    }

    private Avalonia.Threading.DispatcherTimer ProgressSettle()
    {
        var timer = new Avalonia.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ProgressQuietMs) };
        timer.Tick += (_, _) =>
        {
            if (Math.Abs(_owed) > 0.5) return;     // still paying out the last clicks
            timer.Stop();
            if (_fired) return;
            _progress = 0;
            SpringBack();
        };
        return timer;
    }

    private void Show(double x, double opacity)
    {
        var target = pane();
        if (target.RenderTransform is not TranslateTransform follow) target.RenderTransform = follow = new TranslateTransform();
        follow.X = x;
        target.Opacity = opacity;
        Moved?.Invoke(x / Math.Max(1, target.Bounds.Width));
    }

    private void SpringBack()
    {
        var target = pane();
        var from = (target.RenderTransform as TranslateTransform)?.X ?? 0;
        var opacity = target.Opacity;
        _swipe = 0;
        Anim.Run(target, 200, t =>
        {
            if (target.RenderTransform is TranslateTransform tt) tt.X = from * (1 - t);
            target.Opacity = opacity + (1 - opacity) * t;
            Moved?.Invoke(from * (1 - t) / Math.Max(1, target.Bounds.Width));
        }, () => { target.RenderTransform = null; target.Opacity = 1; Moved?.Invoke(0); }, decorative: false);
    }

    private void Fire()
    {
        _fired = true;
        _detentSettle?.Stop();
        var target = pane();
        target.RenderTransform = null; target.Opacity = 1;
        Moved?.Invoke(0);
        navigate(_swipe > 0 ? -1 : 1);
    }
}
