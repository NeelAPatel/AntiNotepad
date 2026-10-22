using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Antipad.Editor;
using Antipad.Model;
using Antipad.Theme;

namespace Antipad.UI;

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

    private readonly PadView _pad;
    private readonly Border _frame;
    private readonly Trace _trace;
    private readonly StackPanel _legend = new() { Orientation = Orientation.Horizontal, Spacing = 14 };
    private readonly NoteSwipe _swipe;
    private readonly DispatcherTimer _sampler = new() { Interval = TimeSpan.FromMilliseconds(16) };
    private Palette _p;
    private Settings _s;
    private bool _feel;
    private double _pull;        // how far the note is pulled right now, as a share of its width

    private Note[] Samples => _feel
        ? _feelSamples ??= [new Note { Text = Clean(Reading) }, new Note { Text = Clean(Standup) }, new Note { Text = Clean(Budget) }]
        : _editorSamples ??= [new Note { Text = Clean(Showcase.Text) }, new Note { Text = Clean(Standup) }, new Note { Text = Clean(Budget) }];

    private int Index { get => _feel ? _feelIndex : _editorIndex; set { if (_feel) _feelIndex = value; else _editorIndex = value; } }

    private static string Clean(string text) => text.Replace("\r\n", "\n");

    public MotionPreview(Palette palette, Settings settings)
    {
        _p = palette; _s = settings;
        Spacing = 6;
        Margin = new Thickness(0, 0, 0, 4);
        // Same size and place on both pages, so switching tabs never moves it.
        _pad = new PadView(palette, settings, new Timers()) { Height = 220, ClipToBounds = true };
        _frame = new Border { Child = _pad, CornerRadius = new CornerRadius(10), ClipToBounds = true, BorderThickness = new Thickness(1) };
        Children.Add(_frame);

        _swipe = new NoteSwipe(_pad, () => _s, () => _pad, direction =>
        {
            Index = (Index + direction + Samples.Length) % Samples.Length;
            _pad.Bind(Samples[Index]);
            _pad.Arrive(direction);
        });
        _swipe.Moved += share => _pull = share;
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

        _trace = new Trace(palette) { Height = 46 };
        Children.Add(_trace);
        Children.Add(_legend);
        ApplyLook(palette, settings);

        _sampler.Tick += (_, _) =>
        {
            var line = Math.Max(1, _pad.Editor.TextArea.TextView.DefaultLineHeight);
            // Pulling right shows the previous note, so the note position moves against the pull.
            _trace.Add(_pad.Editor.VerticalOffset / line, Index - _pull);
        };
        AttachedToVisualTree += (_, _) => _sampler.Start();
        DetachedFromVisualTree += (_, _) => _sampler.Stop();
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
        _frame.Background = new SolidColorBrush(palette.Canvas, settings.Material == "solid" ? 1 : .6);
        _frame.BorderBrush = new SolidColorBrush(palette.Ink, palette.Dark ? .09 : .10);
        _trace.Palette = palette;
        _legend.Children.Clear();
        _legend.Children.Add(Key(palette.Accent, "scroll"));
        _legend.Children.Add(Key(palette.Number, "note"));
        _legend.Children.Add(Kit.Text("last 2 seconds", palette.Muted, 11.5));
        var hint = Kit.Text(_feel ? "Scroll up and down, or sideways to switch notes" : "Type, scroll or switch notes here", palette.Muted, 11.5);
        hint.Margin = new Thickness(12, 0, 0, 0);
        _legend.Children.Add(hint);
        _trace.InvalidateVisual();
    }

    private static Control Key(Color color, string label) => new StackPanel
    {
        Orientation = Orientation.Horizontal, Spacing = 5,
        Children =
        {
            new Border { Width = 14, Height = 3, CornerRadius = new CornerRadius(1.5), Background = new SolidColorBrush(color), VerticalAlignment = VerticalAlignment.Center },
            Kit.Text(label, color, 11.5),
        }
    };

    /// <summary>Both axes over the last two seconds, each scaled to its own range (scroll in lines, note in whole notes).</summary>
    private sealed class Trace(Palette p) : Control
    {
        private const double Span = 2000;
        private readonly List<(long T, double V, double H)> _points = [];
        public Palette Palette { get; set; } = p;

        public void Add(double v, double h)
        {
            var now = Environment.TickCount64;
            _points.Add((now, v, h));
            _points.RemoveAll(x => now - x.T > Span);
            InvalidateVisual();
        }

        public override void Render(DrawingContext dc)
        {
            var w = Bounds.Width; var h = Bounds.Height;
            dc.DrawRectangle(new SolidColorBrush(Palette.Ink, Palette.Dark ? .06 : .045), null, new Rect(0, 0, w, h), 8, 8);
            if (_points.Count == 0 || w <= 0) return;
            var now = Environment.TickCount64;
            // At rest each line keeps its own lane (scroll high, note low) so one never hides the other.
            Line(dc, x => x.V, 6, 0.3, Palette.Accent, now, w, h);
            Line(dc, x => x.H, 1, 0.7, Palette.Number, now, w, h);
        }

        private void Line(DrawingContext dc, Func<(long T, double V, double H), double> pick, double minSpan, double lane, Color color, long now, double w, double h)
        {
            var lo = _points.Min(pick); var hi = _points.Max(pick);
            if (hi - lo < minSpan) { var mid = (hi + lo) / 2; lo = mid - (1 - lane) * minSpan; hi = lo + minSpan; }
            const double pad = 6;
            double Y(double v) => pad + (1 - (v - lo) / (hi - lo)) * (h - 2 * pad);
            var geometry = new StreamGeometry();
            using (var c = geometry.Open())
            {
                // Always from the left edge: before the first sample the value was what it first reads.
                c.BeginFigure(new Point(0, Y(pick(_points[0]))), false);
                foreach (var point in _points) c.LineTo(new Point((1 - (now - point.T) / Span) * w, Y(pick(point))));
                c.LineTo(new Point(w, Y(pick(_points[^1]))));
            }
            dc.DrawGeometry(null, new Pen(new SolidColorBrush(color), 1.8, lineJoin: PenLineJoin.Round), geometry);
        }
    }
}
