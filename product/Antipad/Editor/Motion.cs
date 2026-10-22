using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using Antipad.UI;

namespace Antipad.Editor;

/// <summary>
/// Wheel and touchpad scrolling that glides instead of jumping. Two feels:
/// lines: each notch glides 3 lines; consecutive notches extend the same glide.
/// free: each notch adds momentum that coasts and decays, so distance follows how fast you spin; a touchpad tracks 1:1 in pixels.
/// </summary>
internal sealed class SmoothScroll
{
    private const double PxPerUnit = 50;          // touchpad: pixels per wheel unit
    private const double Impulse = 700;           // free: px/s added per notch (about 115 px coasted alone)
    private const double Friction = 6;            // free: velocity decays by e^-6 per second
    private const double MaxSpeed = 9000;
    private readonly TextEditor _editor;
    private readonly Func<bool> _free;
    private double _target = double.NaN;
    private double _velocity, _position;
    private TimeSpan? _last;
    private bool _running;

    public SmoothScroll(TextEditor editor, Func<bool> free)
    {
        _editor = editor; _free = free;
        editor.AddHandler(InputElement.PointerWheelChangedEvent, OnWheel, RoutingStrategies.Tunnel);
    }

    private double Max => Math.Max(0, _editor.ExtentHeight - _editor.ViewportHeight);

    private void OnWheel(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.None || Math.Abs(e.Delta.Y) < 0.001 || Math.Abs(e.Delta.X) > Math.Abs(e.Delta.Y)) return;
        var max = Max;
        if (max <= 0) return;
        e.Handled = true;
        if (_free()) { Free(e.Delta.Y, max); return; }
        _velocity = 0;
        var step = _editor.TextArea.TextView.DefaultLineHeight * 3;
        var from = _running && !double.IsNaN(_target) ? _target : _editor.VerticalOffset;
        _target = Math.Clamp(from - e.Delta.Y * step, 0, max);
        if (Anim.Reduced) { SetOffset(_editor, _target); return; }
        Start();
    }

    private void Free(double delta, double max)
    {
        _target = double.NaN;
        // Precision touchpads send fractions of a notch and already carry their own inertia: follow them exactly.
        var touchpad = Math.Abs(delta - Math.Round(delta)) > 0.01;
        if (touchpad || Anim.Reduced)
        {
            _velocity = 0;
            SetOffset(_editor, Math.Clamp(_editor.VerticalOffset - delta * (touchpad ? PxPerUnit : _editor.TextArea.TextView.DefaultLineHeight * 3), 0, max));
            return;
        }
        // Spinning the other way stops the coast first.
        if (Math.Sign(_velocity) == Math.Sign(delta)) _velocity = 0;
        if (!_running) _position = _editor.VerticalOffset;
        _velocity = Math.Clamp(_velocity - delta * Impulse, -MaxSpeed, MaxSpeed);
        Start();
    }

    /// <summary>Moves the editor's own ScrollViewer (TextEditor.ScrollToVerticalOffset doesn't apply reliably here).</summary>
    public static void SetOffset(TextEditor editor, double y)
    {
        if (editor.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault() is { } viewer)
            viewer.Offset = new Vector(viewer.Offset.X, Math.Max(0, y));
        else editor.ScrollToVerticalOffset(y);
    }

    private void Start()
    {
        if (_running) return;
        _running = true; _last = null; Frame();
    }

    private void Frame()
    {
        if (TopLevel.GetTopLevel(_editor) is not { } top) { _running = false; return; }
        top.RequestAnimationFrame(time =>
        {
            var dt = _last is { } last ? Math.Clamp((time - last).TotalSeconds, 0, 0.05) : 1 / 60.0;
            _last = time;
            if (!double.IsNaN(_target))
            {
                var now = _editor.VerticalOffset;
                var gap = _target - now;
                if (Math.Abs(gap) < 0.5) { SetOffset(_editor, _target); _running = false; return; }
                // Exponential approach: quick start, soft landing, about 140 ms for a notch.
                SetOffset(_editor, now + gap * 0.28);
            }
            else
            {
                // Something else moved the view (typing, the scrollbar): let it win.
                if (Math.Abs(_editor.VerticalOffset - _position) > 2) { _velocity = 0; _running = false; return; }
                var max = Max;
                _position = Math.Clamp(_position + _velocity * dt, 0, max);
                _velocity *= Math.Exp(-Friction * dt);
                if (_position <= 0 || _position >= max) _velocity = 0;
                SetOffset(_editor, _position);
                if (Math.Abs(_velocity) < 15) { _velocity = 0; _running = false; return; }
            }
            Frame();
        });
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
        _editor.TextArea.GotFocus += (_, _) => { _on = true; _blink.Start(); Invalidate(); };
        _editor.TextArea.LostFocus += (_, _) => { _blink.Stop(); Invalidate(); };
        _blink.Tick += (_, _) =>
        {
            // Stay solid for a moment after any movement, then blink.
            if ((DateTime.Now - _movedAt).TotalMilliseconds < 600) { _on = true; return; }
            _on = !_on; Invalidate();
        };
    }

    private Rect? Target()
    {
        var view = _editor.TextArea.TextView;
        if (!view.VisualLinesValid || _editor.Document is null) return null;
        var pos = _editor.TextArea.Caret.Position;
        var top = view.GetVisualPosition(pos, VisualYPosition.TextTop);
        var bottom = view.GetVisualPosition(pos, VisualYPosition.TextBottom);
        if (double.IsNaN(top.X) || double.IsNaN(top.Y)) return null;
        return new Rect(top.X, top.Y, 2, Math.Max(4, bottom.Y - top.Y));
    }

    private void Moved()
    {
        _movedAt = DateTime.Now; _on = true;
        if (!_editor.TextArea.TextView.VisualLinesValid || Target() is not { } target) { Invalidate(); return; }
        if (Anim.Reduced || _to == default) { _from = _to = target; Invalidate(); return; }
        _from = Current(); _to = target;
        if (!_animating) { _animating = true; Frame(); }
    }

    private Rect Current()
    {
        var t = Math.Clamp((DateTime.Now - _movedAt).TotalMilliseconds / 80, 0, 1);
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

    public void Draw(TextView view, DrawingContext dc)
    {
        if (!_editor.TextArea.IsFocused || !_on && !_animating) return;
        // Outside a glide, re-read the caret here (during render the visual lines are valid; asking during layout is not safe).
        if (!_animating && Target() is { } fresh) _from = _to = fresh;
        var r = _animating ? Current() : _to;
        if (r == default) return;
        var rect = new Rect(r.X - view.HorizontalOffset, r.Y - view.VerticalOffset, r.Width, r.Height);
        dc.DrawRectangle(new SolidColorBrush(_pad.Palette.Caret), null, rect, 1, 1);
    }
}
