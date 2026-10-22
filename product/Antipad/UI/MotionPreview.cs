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
/// Settings preview: a real note editor over three sample notes, scrolled and swiped with the user's own Editor and Feel
/// settings (the same SmoothScroll and NoteSwipe the pad uses), above a live trace of both axes.
/// The sample notes live only in memory and are never saved.
/// </summary>
internal sealed class MotionPreview : StackPanel
{
    private static readonly string[] Texts =
    [
        """
        # Weekend plan
        Saturday: farmers market, then the bike shop
        [ ] pick up the repaired wheel
        [x] book the campsite
        ## Budget
        groceries = 64
        fuel = 38
        campsite = 45
        total = 147 =
        ## Packing
        - tent, poles, pegs
        - two sleeping bags
          - the warm one for her
        - stove and gas
        - headlamps
        ```py
        def split(total, people):
            return round(total / people, 2)
        ```
        ## Route
        - leave by 8 to beat traffic
        - lunch stop at the lake
        - last fuel before the pass
        ## Notes
        Scroll up and down here with your wheel or touchpad.
        Swipe sideways (thumb wheel, two fingers, or Shift + wheel) to change note.
        The trace below shows both: scroll in one colour, note changes in the other.
        - line spacing, text width and backing change this note live
        - so do bubbles, the caret and the font
        ## Later
        [ ] return the borrowed cooler
        [ ] print the parking pass
        [ ] charge the power bank
        end of the sample note
        """,
        """
        # Standup
        shipped: the scroll lab picks
        next: settings previews
        blocked on nothing
        ## Questions
        - is the swipe too eager at 2200 px?
        - should the trash keep 30 days?
        """,
        """
        math
        rent = 1450
        utilities = 180
        internet = 65
        total
        total / 3
        20% of rent
        """,
    ];
    private static Note[]? _samples;
    private static int _index;   // the sample on screen, kept across Settings rebuilds

    private readonly PadView _pad;
    private readonly Trace _trace;
    private readonly NoteSwipe _swipe;
    private double _pull;        // how far the note is pulled right now, as a share of its width
    private readonly DispatcherTimer _sampler = new() { Interval = TimeSpan.FromMilliseconds(16) };

    public MotionPreview(Palette palette, Settings settings)
    {
        Spacing = 6;
        Margin = new Thickness(8, 4, 8, 10);
        _samples ??= Texts.Select(t => new Note { Text = t.Replace("\r\n", "\n") }).ToArray();

        _pad = new PadView(palette, settings, new Timers()) { Height = 230, ClipToBounds = true };
        _pad.Bind(_samples[_index]);
        var frame = new Border
        {
            Child = _pad, CornerRadius = new CornerRadius(10), ClipToBounds = true,
            Background = new SolidColorBrush(palette.Canvas, settings.Material == "solid" ? 1 : .55),
            BorderBrush = new SolidColorBrush(palette.Faint, .4), BorderThickness = new Thickness(1),
        };
        Children.Add(frame);

        _swipe = new NoteSwipe(_pad, () => settings, () => _pad, direction =>
        {
            _index = (_index + direction + _samples.Length) % _samples.Length;
            _pad.Bind(_samples[_index]);
            _pad.Arrive(direction);
        });
        _swipe.Moved += share => _pull = share;
        // Same input rule as the pad: horizontal deltas, or Shift + wheel, swipe; vertical wheel scrolls the note.
        frame.AddHandler(PointerWheelChangedEvent, (_, e) =>
        {
            var horizontal = Math.Abs(e.Delta.X) > 0.001;
            if (!horizontal && !e.KeyModifiers.HasFlag(KeyModifiers.Shift)) return;
            var units = horizontal ? e.Delta.X : e.Delta.Y;
            if (Math.Abs(units) < 0.001) return;
            e.Handled = true;
            _swipe.Input(units);
        }, RoutingStrategies.Tunnel, handledEventsToo: true);

        _trace = new Trace(palette) { Height = 54 };
        Children.Add(_trace);
        var legend = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 14 };
        legend.Children.Add(Key(palette.Accent, "scroll"));
        legend.Children.Add(Key(palette.Number, "note"));
        legend.Children.Add(Kit.Text("last 2 seconds", palette.Muted, 11));
        Children.Add(legend);

        _sampler.Tick += (_, _) =>
        {
            var line = Math.Max(1, _pad.Editor.TextArea.TextView.DefaultLineHeight);
            // Pulling right shows the previous note, so the note position moves against the pull.
            _trace.Add(_pad.Editor.VerticalOffset / line, _index - _pull);
        };
        AttachedToVisualTree += (_, _) => _sampler.Start();
        DetachedFromVisualTree += (_, _) => _sampler.Stop();
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
            dc.DrawRectangle(new SolidColorBrush(p.Ink, p.Dark ? .06 : .045), null, new Rect(0, 0, w, h), 8, 8);
            if (_points.Count < 2 || w <= 0) return;
            var now = Environment.TickCount64;
            Line(dc, x => x.V, 6, p.Accent, now, w, h);
            Line(dc, x => x.H, 1, p.Number, now, w, h);
        }

        private void Line(DrawingContext dc, Func<(long T, double V, double H), double> pick, double minSpan, Color color, long now, double w, double h)
        {
            var lo = _points.Min(pick); var hi = _points.Max(pick);
            if (hi - lo < minSpan) { var mid = (hi + lo) / 2; lo = mid - minSpan / 2; hi = mid + minSpan / 2; }
            const double pad = 6;
            var geometry = new StreamGeometry();
            using (var c = geometry.Open())
            {
                for (var i = 0; i < _points.Count; i++)
                {
                    var pt = new Point((1 - (now - _points[i].T) / Span) * w, pad + (1 - (pick(_points[i]) - lo) / (hi - lo)) * (h - 2 * pad));
                    if (i == 0) c.BeginFigure(pt, false); else c.LineTo(pt);
                }
            }
            dc.DrawGeometry(null, new Pen(new SolidColorBrush(color), 1.8, lineJoin: PenLineJoin.Round), geometry);
        }
    }
}
