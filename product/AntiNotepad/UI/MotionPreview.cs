// In-memory sample editors and motion traces used by Settings.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AntiNotepad.Editor;
using AntiNotepad.Model;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

/// <summary>
/// Settings preview: a real note editor over sample notes, scrolled and switched with the user's own Editor and Feel
/// settings (the same SmoothScroll and NoteSwipe the pad uses), above a live trace of both axes. One instance lives as
/// long as the Settings window and moves between the Editor and Feel pages; a change restyles it in place.
/// Editor shows the writing a note is likely to hold (the showcase); Feel shows long reading to scroll and notes to switch.
/// The samples live only in memory and are never saved.
/// </summary>
internal sealed class MotionPreview : StackPanel
{
    private const string Reading = """
        # Reading list
        The Pragmatic Programmer, chapter 7: the one about estimating, and why "it depends" is a fine answer if you say what it depends on.
        Designing Fluid Interfaces: projection, rubber bands, and why a flick should land where your thumb meant.
        Refactoring UI: spacing first, then type, then color.
        ## Notes from the trip
        - tram 28 before eight, before the crowds
        - LX Factory on Sunday, the bookshop in the old printing hall
        - the long walk up to the castle, then down through Alfama
        - pastéis at the place with the blue door
        ## Still to read
        - Thinking in Systems
        - The Design of Everyday Things, again
        - A Pattern Language, the chapters on windows
        ## Quotes worth keeping
        > Simple things should be simple, complex things should be possible.
        > The best interface is the one you stop noticing.
        ## Ideas
        - a scroll that keeps pace with the wheel
        - notes that switch at a third of the way, at any width
        - settings with a preview right where you change them
        - a showcase page to screenshot after every change
        ## Later
        [ ] return the library books by the 3rd
        [ ] back up the notes folder
        [ ] write up the trip
        end of the reading list
        """;

    private const string Standup = """
        # Standup
        shipped: the scroll picks and the switch feel
        next: the settings window
        blocked on nothing
        ## Questions
        - is the switch too eager in a very wide window?
        - should the trash keep 30 days?
        ## Numbers
        1450 / 3 =
        19.99 * 3 =
        """;

    private const string Budget = """
        math
        rent = 1450
        utilities = 180
        internet = 65
        total
        total / 3
        20% of rent
        5 km in mi
        days until dec 25
        """;

    private static Note[]? _editorSamples, _feelSamples;
    private static int _editorIndex, _feelIndex;

    private readonly Timers _timers;
    private readonly PadView _pad;
    private readonly Border _frame;
    private readonly Trace _trace;
    private readonly StackPanel _legend = new() { Orientation = Orientation.Horizontal, Spacing = Space.L };
    private readonly NoteSwipe _swipe;
    private bool _sampling;      // sampling once per displayed frame, only while something is moving
    private bool _attached;
    private long _movedAt;       // when scroll or note position last changed (ms, high-resolution clock)
    private double _lastV = double.NaN, _lastH = double.NaN;
    private Palette _p;
    private Settings _s;
    private bool _feel;
    private double _pull;        // how far the note is pulled right now, as a share of its width

    private Note[] Samples => _feel
        ? _feelSamples ??= [new Note { Text = Clean(Reading) }, new Note { Text = Clean(Standup) }, new Note { Text = Clean(Budget) }]
        : _editorSamples ??= [new Note { Text = Clean(Showcase.Text) }, new Note { Text = Clean(Standup) }, new Note { Text = Clean(Budget) }];

    private int Index { get => _feel ? _feelIndex : _editorIndex; set { if (_feel) _feelIndex = value; else _editorIndex = value; } }

    private static string Clean(string text) => text.Replace("\r\n", "\n");

    public MotionPreview(Palette palette, Settings settings, INotesLocation notesLocation)
    {
        _p = palette; _s = settings;
        Spacing = Space.S;
        Margin = new Thickness(0, 0, 0, Space.Xs);
        // Same size and place on both pages, so switching tabs never moves it.
        _timers = new Timers();
        _pad = new PadView(palette, settings, _timers, notesLocation) { Height = ChromeTokens.Size.MotionPreviewPadHeight, ClipToBounds = true, Private = true };
        _frame = new Border { Child = _pad, CornerRadius = new CornerRadius(Shape.Card), ClipToBounds = true, BorderThickness = new Thickness(Shape.Thin) };
        Children.Add(_frame);

        _swipe = new NoteSwipe(_pad, () => _pad,
            direction => Index + direction >= 0 && Index + direction < Samples.Length,
            direction =>
        {
            Index += direction;
            _pad.SwitchTo(Samples[Index], direction);
            Wake();
        });
        _swipe.Moved += share => { _pull = share; Wake(); };
        // Same input rule as the pad: horizontal deltas, or Shift + wheel, switch notes; the vertical wheel scrolls.
        _frame.AddHandler(PointerWheelChangedEvent, (_, e) =>
        {
            var horizontal = Math.Abs(e.Delta.X) > 0.001;
            if (!horizontal && !e.KeyModifiers.HasFlag(KeyModifiers.Shift)) return;
            var units = horizontal ? e.Delta.X : e.Delta.Y;
            if (Math.Abs(units) < 0.001) return;
            e.Handled = true;
            _swipe.Input(units);
        }, RoutingStrategies.Tunnel, handledEventsToo: true);

        _trace = new Trace(palette) { Height = Space.MotionSampleHeight };
        Children.Add(_trace);
        Children.Add(_legend);
        ApplyLook(palette, settings);

        // Scrolling (wheel, keys, dragging the bar, typing past the edge) is what wakes the trace.
        _pad.Editor.TextArea.TextView.ScrollOffsetChanged += (_, _) => Wake();
        AttachedToVisualTree += (_, _) => { _attached = true; Wake(); };
        DetachedFromVisualTree += (_, _) => { _attached = false; _sampling = false; _swipe.Cancel(); };
    }

    /// <summary>Something moved: run the trace until its whole two-second window has gone still again.</summary>
    private void Wake()
    {
        _movedAt = Trace.Now;
        if (_attached) StartSampling();
    }

    /// <summary>
    /// One sample per displayed frame (the display's own rate, 60 to 120 Hz or more), so the trace moves as smoothly
    /// as the scroll it shows. Once nothing has moved for the whole two-second window the line is flat end to end,
    /// so sampling stops entirely: an idle preview costs no frames and no memory.
    /// </summary>
    private void StartSampling()
    {
        if (_sampling) return;
        _sampling = true;
        Frame();
    }

    private void Frame()
    {
        if (!_sampling || TopLevel.GetTopLevel(this) is not { } top) { _sampling = false; return; }
        top.RequestAnimationFrame(_ =>
        {
            if (!_sampling) return;
            var line = Math.Max(1, _pad.Editor.TextArea.TextView.DefaultLineHeight);
            // Pulling right shows the previous note, so the note position moves against the pull.
            var pull = (_pad.Editor.TextArea.RenderTransform as TranslateTransform)?.Y ?? 0;
            var v = (_pad.Editor.VerticalOffset - pull) / line;
            var h = Index - _pull;
            if (v != _lastV || h != _lastH) { _movedAt = Trace.Now; _lastV = v; _lastH = h; }
            _trace.Add(v, h);
            if (Trace.Now - _movedAt > Trace.Span + 100) { _sampling = false; return; }
            Frame();
        });
    }

    /// <summary>Dev tooling: put the caret in the preview, as a click would.</summary>
    public void FocusEditor() => _pad.FocusText();

    /// <summary>The Settings window closed: stop sampling and let go of the editor's documents.</summary>
    public void Release()
    {
        _sampling = false;
        _swipe.Cancel();
        _pad.Release();
        _timers.Stop();
    }

    /// <summary>Editor samples (what you write) or Feel samples (long reading to scroll, notes to switch).</summary>
    public void UseSamples(bool feel)
    {
        if (_feel == feel && _pad.Note is not null) return;
        _feel = feel;
        _pad.Bind(Samples[Index]);
    }

    /// <summary>Restyles in place after a change (no new editor, so the trace and scroll position carry on).</summary>
    public void ApplyLook(Palette palette, Settings settings)
    {
        _p = palette; _s = settings;
        _pad.ApplyLook(palette, settings);
        _frame.Background = new SolidColorBrush(palette.Canvas, settings.Material == "solid" ? 1 : Paint.GlassPreview);
        _frame.BorderBrush = new SolidColorBrush(palette.Ink, Paint.PreviewFrame(palette.Dark));
        _trace.Palette = palette;
        _legend.Children.Clear();
        _legend.Children.Add(Key(palette.Accent, "scroll"));
        _legend.Children.Add(Key(palette.Number, "note"));
        _legend.Children.Add(Kit.Text("last 2 seconds", palette.Muted, Typography.Caption));
        var hint = Kit.Text(_feel ? "Scroll or pull past either edge; swipe sideways between notes" : "Type, scroll or switch notes here", palette.Muted, Typography.Caption);
        hint.Margin = new Thickness(Space.M, 0, 0, 0);
        _legend.Children.Add(hint);
        _trace.InvalidateVisual();
    }

    private static Control Key(Color color, string label) => new StackPanel
    {
        Orientation = Orientation.Horizontal,
        Spacing = Space.Xs,
        Children =
        {
            new Border { Width = Space.MotionSampleBarWidth, Height = Space.MotionSampleBarHeight, CornerRadius = new CornerRadius(Shape.Round(Space.MotionSampleBarHeight)), Background = new SolidColorBrush(color), VerticalAlignment = VerticalAlignment.Center },
            Kit.Text(label, color, Typography.Caption),
        }
    };

    /// <summary>
    /// Both axes over the last two seconds, each scaled to its own range (scroll in lines, note in whole notes).
    /// Samples live in a fixed ring and the pens are made once per palette, so drawing a frame allocates nothing.
    /// </summary>
    private sealed class Trace(Palette p) : Control
    {
        public const double Span = 2000;
        // Milliseconds from a high-resolution clock (TickCount64 moves in ~16 ms steps).
        public static long Now => System.Diagnostics.Stopwatch.GetTimestamp() * 1000 / System.Diagnostics.Stopwatch.Frequency;

        // Two seconds at up to 500 frames a second; past that the oldest samples are simply overwritten.
        private readonly long[] _t = new long[1024];
        private readonly double[] _v = new double[1024], _h = new double[1024];
        private int _head, _count;
        private Palette _palette = p;
        private IBrush _well = null!;
        private Pen _scrollPen = null!, _notePen = null!;

        public Palette Palette
        {
            get => _palette;
            set { _palette = value; _well = null!; }
        }

        public void Add(double v, double h)
        {
            var now = Now;
            var i = (_head + _count) % _t.Length;
            if (_count == _t.Length) _head = (_head + 1) % _t.Length; else _count++;
            _t[i] = now; _v[i] = v; _h[i] = h;
            // Drop what has scrolled out of the window.
            while (_count > 1 && now - _t[_head] > Span) { _head = (_head + 1) % _t.Length; _count--; }
            InvalidateVisual();
        }

        public override void Render(DrawingContext dc)
        {
            if (_well is null)
            {
                _well = new SolidColorBrush(_palette.Ink, Paint.PreviewWell(_palette.Dark));
                _scrollPen = new Pen(new SolidColorBrush(_palette.Accent), Shape.Thick, lineCap: PenLineCap.Round);
                _notePen = new Pen(new SolidColorBrush(_palette.Number), Shape.Thick, lineCap: PenLineCap.Round);
            }
            var w = Bounds.Width; var h = Bounds.Height;
            dc.DrawRectangle(_well, null, new Rect(0, 0, w, h), 8, 8);
            if (_count == 0 || w <= 0) return;
            var now = Now;
            // At rest each line keeps its own lane (scroll high, note low) so one never hides the other.
            Line(dc, _v, 6, 0.3, _scrollPen, now, w, h);
            Line(dc, _h, 1, 0.7, _notePen, now, w, h);
        }

        private void Line(DrawingContext dc, double[] values, double minSpan, double lane, Pen pen, long now, double w, double h)
        {
            double lo = double.MaxValue, hi = double.MinValue;
            for (var k = 0; k < _count; k++) { var v = values[(_head + k) % _t.Length]; if (v < lo) lo = v; if (v > hi) hi = v; }
            if (hi - lo < minSpan) { var mid = (hi + lo) / 2; lo = mid - (1 - lane) * minSpan; hi = lo + minSpan; }
            const double pad = 6;
            double Y(double v) => pad + (1 - (v - lo) / (hi - lo)) * (h - 2 * pad);
            // Always from the left edge: before the first sample the value was what it first reads; and on to the right edge.
            var previous = new Point(0, Y(values[_head]));
            for (var k = 0; k < _count; k++)
            {
                var i = (_head + k) % _t.Length;
                var point = new Point((1 - (now - _t[i]) / Span) * w, Y(values[i]));
                dc.DrawLine(pen, previous, point);
                previous = point;
            }
            dc.DrawLine(pen, previous, new Point(w, previous.Y));
        }
    }
}
