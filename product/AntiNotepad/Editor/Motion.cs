using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using AntiNotepad.Motion;
using AntiNotepad.Theme;

namespace AntiNotepad.Editor;

/// <summary>
/// Wheel and touchpad scrolling with four vertical styles (Settings > Feel):
/// lines: each notch glides 3 lines; consecutive notches extend the same glide.
/// free: each notch adds momentum that coasts and decays; a touchpad tracks 1:1 in pixels.
/// rate: slow clicks glide a short way; faster spinning makes every click travel further and the glide tighter.
/// wheel: the click rate sets the sheet's speed directly, and it stops almost as soon as the wheel does.
/// The last two were picked in the Scroll Lab; they listen to how fast clicks arrive, since a wheel sends whole clicks.
/// </summary>
internal sealed class SmoothScroll
{
    private const double PxPerUnit = 50;          // touchpad: pixels per wheel unit
    private const double Impulse = 700;           // free: px/s added per notch (about 115 px coasted alone at friction 6)
    private const double MaxSpeed = 9000;
    private enum Motion { None, Glide, Coast, Spring, Fly }
    internal interface ISurface
    {
        Control Host { get; }
        double Extent { get; }
        double Viewport { get; }
        double Offset { get; set; }
        double LineHeight { get; }
    }

    internal sealed class EditorSurface(TextEditor editor) : ISurface
    {
        public Control Host => editor;
        public double Extent => editor.ExtentHeight;
        public double Viewport => editor.ViewportHeight;
        public double Offset { get => editor.VerticalOffset; set => SetOffset(editor, value); }
        public double LineHeight => editor.TextArea.TextView.DefaultLineHeight;
    }

    internal sealed class ViewerSurface(ScrollViewer viewer, double lineHeight) : ISurface
    {
        public Control Host => viewer;
        public double Extent => viewer.Extent.Height;
        public double Viewport => viewer.Viewport.Height;
        public double Offset
        {
            get => viewer.Offset.Y;
            set => viewer.Offset = new Vector(viewer.Offset.X, Math.Clamp(value, 0, Math.Max(0, Extent - Viewport)));
        }
        public double LineHeight => lineHeight;
    }

    private readonly ISurface _surface;
    private readonly Func<Model.Settings> _settings;
    private Motion _motion;
    private double _target = double.NaN;
    private double _velocity, _position, _omega, _flyGoal;
    private long _lastClick, _holdUntil;
    private double _gap;
    private TimeSpan? _last;
    private bool _running;
    private int _generation;

    public SmoothScroll(TextEditor editor, Func<Model.Settings> settings) : this(new EditorSurface(editor), settings) { }
    public SmoothScroll(ScrollViewer viewer, double lineHeight, Func<Model.Settings> settings) : this(new ViewerSurface(viewer, lineHeight), settings) { }

    private SmoothScroll(ISurface surface, Func<Model.Settings> settings)
    {
        _surface = surface; _settings = settings;
        surface.Host.AddHandler(InputElement.PointerWheelChangedEvent, OnWheel, RoutingStrategies.Tunnel);
    }

    private double Max => Math.Max(0, _surface.Extent - _surface.Viewport);
    private double Line => _surface.LineHeight;

    private void OnWheel(object? sender, PointerWheelEventArgs e)
    {
        // A nested editor or scroll viewer owns its wheel input (including the Feel preview).
        if (_surface is ViewerSurface && e.Source is Visual source)
            for (var visual = source; visual is not null && visual != _surface.Host; visual = visual.GetVisualParent())
                if (visual is ScrollViewer or TextEditor) return;
        if (e.KeyModifiers != KeyModifiers.None || Math.Abs(e.Delta.Y) < 0.001 || Math.Abs(e.Delta.X) > Math.Abs(e.Delta.Y)) return;
        var max = Max;
        if (max <= 0) return;
        e.Handled = true;
        var delta = e.Delta.Y;
        var mode = _settings().Scrolling;
        if (mode == "lines") { Lines(delta, max); return; }
        // Precision touchpads send fractions of a notch and already carry their own inertia: follow them exactly.
        var touchpad = Math.Abs(delta - Math.Round(delta)) > 0.01;
        if (touchpad || Anim.Reduced)
        {
            _velocity = 0; _motion = Motion.None; _target = double.NaN;
            SetSurfaceOffset(Math.Clamp(_surface.Offset - delta * (touchpad ? PxPerUnit : Line * 3), 0, max));
            return;
        }
        if (mode == "rate") Rate(delta, max);
        else if (mode == "wheel") Fly(delta);
        else Free(delta);
    }

    private void Lines(double delta, double max)
    {
        var step = Line * Math.Clamp(_settings().LinesPerNotch, 1, 10);
        var from = _running && _motion == Motion.Glide && !double.IsNaN(_target) ? _target : _surface.Offset;
        _velocity = 0;
        _target = Math.Clamp(from - delta * step, 0, max);
        if (Anim.Reduced) { SetSurfaceOffset(_target); return; }
        _motion = Motion.Glide;
        Start();
    }

    private void Free(double delta)
    {
        _target = double.NaN;
        // Spinning the other way stops the coast first.
        if (_motion != Motion.Coast || Math.Sign(_velocity) == Math.Sign(delta)) _velocity = 0;
        if (_motion != Motion.Coast || !_running) _position = _surface.Offset;
        _velocity = Math.Clamp(_velocity - delta * Impulse * _settings().ScrollDistance, -MaxSpeed, MaxSpeed);
        _motion = Motion.Coast;
        Start();
    }

    /// <summary>Clicks per second, from a smoothed gap between clicks; a pause of 400 ms or more starts slow again.</summary>
    private double ClickRate()
    {
        var now = Environment.TickCount64;
        var gap = _lastClick == 0 ? 1000 : now - _lastClick;
        _lastClick = now;
        _gap = gap >= 400 || _gap <= 0 ? Math.Min(gap, 400) : _gap * 0.4 + gap * 0.6;
        return 1000 / _gap;
    }

    // Under 6 clicks a second: the base distance; at 30 a second: the full boost.
    private static double Boost(double rate, double boost) => 1 + boost * Math.Clamp((rate - 6) / 24, 0, 1);

    private void Rate(double delta, double max)
    {
        var s = _settings();
        var gain = Boost(ClickRate(), 2.5 * s.ScrollBoost);
        var offset = _surface.Offset;
        if (_motion != Motion.Spring || !_running) { _position = offset; _velocity = 0; _target = offset; }
        // Turning back starts from where the sheet is, not from where it was heading.
        if (Math.Sign(_target - offset) != Math.Sign(-delta) && Math.Abs(_target - offset) > 1) _target = offset;
        _target = Math.Clamp(_target - delta * Line * 40 / 24 * s.ScrollDistance * gain, 0, max);   // 40 px a slow click at a 24 px line
        _omega = 2.4 / (Math.Clamp(_gap * 0.9, 45, 160) / 1000);                    // glide no longer than the gap between clicks
        _motion = Motion.Spring;
        Start();
    }

    private void Fly(double delta)
    {
        var rate = ClickRate();
        var s = _settings();
        var gain = Boost(rate, 1.5 * s.ScrollBoost);
        if (_motion != Motion.Fly || !_running) { _position = _surface.Offset; _velocity = 0; }
        if (_velocity != 0 && Math.Sign(_velocity) != Math.Sign(-delta)) _velocity = 0;
        _flyGoal = Math.Sign(-delta) * rate * Math.Abs(delta) * Line * 60 / 24 * s.ScrollDistance * gain;   // px/s matching the click rate
        _holdUntil = Environment.TickCount64 + (long)Math.Clamp(_gap * 1.6, 80, 350);    // hold it until the next click is overdue
        _motion = Motion.Fly;
        Start();
    }

    /// <summary>Moves the editor's own ScrollViewer (TextEditor.ScrollToVerticalOffset doesn't apply reliably here).</summary>
    public static void SetOffset(TextEditor editor, double y)
    {
        if (editor.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault() is { } viewer)
            viewer.Offset = new Vector(viewer.Offset.X, Math.Max(0, y));
        else editor.ScrollToVerticalOffset(y);
    }

    private void SetSurfaceOffset(double y) => _surface.Offset = y;

    private void Start()
    {
        if (_running) return;
        _running = true; _last = null; Frame();
    }

    /// <summary>Stops motion and invalidates any frame queued before a content change.</summary>
    public void Stop() { _running = false; _motion = Motion.None; _velocity = 0; _target = double.NaN; _generation++; }

    private void Frame()
    {
        if (TopLevel.GetTopLevel(_surface.Host) is not { } top) { Stop(); return; }
        var generation = _generation;
        top.RequestAnimationFrame(time =>
        {
            if (!_running || generation != _generation) return;
            var dt = _last is { } last ? Math.Clamp((time - last).TotalSeconds, 0, 0.05) : 1 / 60.0;
            _last = time;
            if (_motion == Motion.Glide ? GlideStep() : PhysicsStep(dt)) Frame();
        });
    }

    private bool GlideStep()
    {
        var now = _surface.Offset;
        var gap = _target - now;
        if (Math.Abs(gap) < 0.5) { SetSurfaceOffset(_target); Stop(); return false; }
        // Exponential approach: quick start, soft landing, about 140 ms for a notch.
        SetSurfaceOffset(now + gap * 0.28);
        return true;
    }

    private bool PhysicsStep(double dt)
    {
        // Something else moved the view (typing, the scrollbar): let it win.
        if (Math.Abs(_surface.Offset - _position) > 2) { Stop(); return false; }
        var max = Max;
        switch (_motion)
        {
            case Motion.Coast:
                _position = Math.Clamp(_position + _velocity * dt, 0, max);
                _velocity *= Math.Exp(-_settings().FreeFriction * dt);
                if (_position <= 0 || _position >= max) _velocity = 0;
                SetSurfaceOffset(_position);
                if (Math.Abs(_velocity) < 15) { Stop(); return false; }
                return true;
            case Motion.Spring:
                {
                    // Critically damped spring toward the target, in small substeps for stability.
                    var k = _omega * _omega; var c = 2 * _omega;
                    var n = Math.Max(1, (int)Math.Ceiling(dt / 0.004)); var h = dt / n;
                    for (var i = 0; i < n; i++) { _velocity += (k * (_target - _position) - c * _velocity) * h; _position += _velocity * h; }
                    _position = Math.Clamp(_position, 0, max);
                    SetSurfaceOffset(_position);
                    if (Math.Abs(_target - _position) < 0.3 && Math.Abs(_velocity) < 4) { SetSurfaceOffset(_target); _position = _target; Stop(); return false; }
                    return true;
                }
            case Motion.Fly:
                {
                    var holding = Environment.TickCount64 < _holdUntil;
                    _velocity = holding ? _velocity + (_flyGoal - _velocity) * (1 - Math.Exp(-25 * dt)) : _velocity * Math.Exp(-14 * dt);
                    _position += _velocity * dt;
                    if (_position <= 0 || _position >= max) { _position = Math.Clamp(_position, 0, max); _velocity = 0; _holdUntil = 0; holding = false; }
                    SetSurfaceOffset(_position);
                    if (!holding && Math.Abs(_velocity) < 5) { Stop(); return false; }
                    return true;
                }
        }
        Stop();
        return false;
    }
}

/// <summary>
/// A drawn caret that glides between positions (about 80 ms) and only blinks while idle.
/// AvaloniaEdit's own caret is made transparent; IME and selection still use the real caret position.
/// </summary>
internal sealed class SmoothCaret : IBackgroundRenderer
{
    private readonly PadView _pad;
    private readonly TextEditor _editor;
    private readonly DispatcherTimer _blink = new() { Interval = TimeSpan.FromMilliseconds(530) };
    private Rect _from, _to;           // document coordinates (scroll applied at draw time)
    private DateTime _movedAt = DateTime.MinValue;
    private bool _on = true;
    private bool _animating;

    // The caret layer only repaints with the built-in caret; the selection layer always does.
    public KnownLayer Layer => KnownLayer.Selection;

    public SmoothCaret(PadView pad)
    {
        _pad = pad; _editor = pad.Editor;
        _editor.TextArea.Caret.PositionChanged += (_, _) => Moved();
        _editor.TextArea.GotFocus += (_, _) => { _on = true; UpdateBlink(); Invalidate(); };
        _editor.TextArea.LostFocus += (_, _) => { _blink.Stop(); Invalidate(); };
        _blink.Tick += (_, _) =>
        {
            // Stay solid for a moment after any movement, then blink (or never, if blinking is off).
            if (!_pad.Settings.CaretBlink || (DateTime.Now - _movedAt).TotalMilliseconds < 600) { if (!_on) { _on = true; Invalidate(); } return; }
            _on = !_on; Invalidate();
        };
    }

    public void SettingsChanged()
    {
        UpdateBlink();
        Invalidate();
    }

    private void UpdateBlink()
    {
        if (_editor.TextArea.IsFocused && _pad.Settings.CaretBlink) _blink.Start();
        else { _blink.Stop(); _on = true; }
    }

    private Rect? Target()
    {
        var view = _editor.TextArea.TextView;
        if (!view.VisualLinesValid || _editor.Document is null) return null;
        var pos = _editor.TextArea.Caret.Position;
        var top = view.GetVisualPosition(pos, VisualYPosition.TextTop);
        var bottom = view.GetVisualPosition(pos, VisualYPosition.TextBottom);
        if (double.IsNaN(top.X) || double.IsNaN(top.Y)) return null;
        // A block covers one character cell; a bar is 2 px.
        var cell = view.WideSpaceWidth;
        var width = _pad.Settings.CaretStyle == "block" && double.IsFinite(cell) && cell > 2 ? cell : 2;
        return new Rect(top.X, top.Y, width, Math.Max(4, bottom.Y - top.Y));
    }

    private void Moved()
    {
        // Read where the caret is drawn right now before restarting the clock: Current() measures from _movedAt, so doing it after
        // would return the stale start of the previous glide (during a drag, every move jumped back to the last pause).
        var shown = _animating ? Current() : _to;
        _movedAt = DateTime.Now; _on = true;
        if (!_editor.TextArea.TextView.VisualLinesValid || Target() is not { } target) { Invalidate(); return; }
        if (Anim.Reduced || !_pad.Settings.CaretGlide || _to == default) { _from = _to = target; Invalidate(); return; }
        _from = shown; _to = target;
        if (!_animating) { _animating = true; Frame(); }
    }

    private Rect Current()
    {
        var t = Math.Clamp((DateTime.Now - _movedAt).TotalMilliseconds / MotionTokens.CaretGlide, 0, 1);
        var e = Anim.EaseOut(t);
        return new Rect(_from.X + (_to.X - _from.X) * e, _from.Y + (_to.Y - _from.Y) * e, _to.Width, _from.Height + (_to.Height - _from.Height) * e);
    }

    private void Frame()
    {
        if (TopLevel.GetTopLevel(_editor) is not { } top) { _animating = false; return; }
        top.RequestAnimationFrame(_ =>
        {
            Invalidate();
            if ((DateTime.Now - _movedAt).TotalMilliseconds < 90) Frame();
            else { _animating = false; _from = _to; Invalidate(); }
        });
    }

    private void Invalidate() => _editor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);

    /// <summary>
    /// Stops blinking for good (the editor is going away). A running timer is held by the dispatcher, and through its
    /// handler it would keep this caret, its editor and the whole window alive.
    /// </summary>
    public void Stop() { _blink.Stop(); _animating = false; }

    public void Draw(TextView view, DrawingContext dc)
    {
        if (!_editor.TextArea.IsFocused || !_on && !_animating) return;
        // Outside a glide, re-read the caret here (during render the visual lines are valid; asking during layout is not safe).
        if (!_animating && Target() is { } fresh) _from = _to = fresh;
        var r = _animating ? Current() : _to;
        if (r == default) return;
        var rect = new Rect(r.X - view.HorizontalOffset, r.Y - view.VerticalOffset, r.Width, r.Height);
        // The block is translucent so the character under it stays readable.
        var block = _pad.Settings.CaretStyle == "block";
        dc.DrawRectangle(new SolidColorBrush(_pad.Palette.Caret, block ? Paint.CaretBlock : 1), null, rect, block ? 2 : 1, block ? 2 : 1);
    }
}
