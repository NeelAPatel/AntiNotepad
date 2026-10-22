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
/// Wheel and touchpad scrolling that glides to its target instead of jumping three lines at a time.
/// Consecutive notches extend the same glide, so fast spinning stays fluid.
/// </summary>
internal sealed class SmoothScroll
{
    private readonly TextEditor _editor;
    private double _target = double.NaN;
    private bool _running;

    public SmoothScroll(TextEditor editor)
    {
        _editor = editor;
        editor.AddHandler(InputElement.PointerWheelChangedEvent, OnWheel, RoutingStrategies.Tunnel);
    }

    private void OnWheel(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.None || Math.Abs(e.Delta.Y) < 0.001 || Math.Abs(e.Delta.X) > Math.Abs(e.Delta.Y)) return;
        var max = Math.Max(0, _editor.ExtentHeight - _editor.ViewportHeight);
        if (max <= 0) return;
        e.Handled = true;
        var step = _editor.TextArea.TextView.DefaultLineHeight * 3;
        var from = _running && !double.IsNaN(_target) ? _target : _editor.VerticalOffset;
        _target = Math.Clamp(from - e.Delta.Y * step, 0, max);
        if (Anim.Reduced) { SetOffset(_editor, _target); return; }
        if (!_running) { _running = true; Frame(); }
    }

    /// <summary>Moves the editor's own ScrollViewer (TextEditor.ScrollToVerticalOffset doesn't apply reliably here).</summary>
    public static void SetOffset(TextEditor editor, double y)
    {
        if (editor.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault() is { } viewer)
            viewer.Offset = new Vector(viewer.Offset.X, Math.Max(0, y));
        else editor.ScrollToVerticalOffset(y);
    }

    private void Frame()
    {
        if (TopLevel.GetTopLevel(_editor) is not { } top) { _running = false; return; }
        top.RequestAnimationFrame(_ =>
        {
            var now = _editor.VerticalOffset;
            var gap = _target - now;
            if (Math.Abs(gap) < 0.5) { SetOffset(_editor, _target); _running = false; return; }
            // Exponential approach: quick start, soft landing, about 140 ms for a notch.
            SetOffset(_editor, now + gap * 0.28);
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
