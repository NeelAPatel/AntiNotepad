using Avalonia.Controls.Chrome;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Antipad.Editor;
using Antipad.Model;

namespace Antipad.UI;

internal sealed partial class PadWindow
{
    private readonly Border _tint = new() { IsHitTestVisible = false };
    private readonly Grid _content = new() { RowDefinitions = new RowDefinitions("36,*") };
    private readonly Grid _top = new() { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
    private readonly StackPanel _meta = new() { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(20, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false };
    private readonly StackPanel _icons = new() { Orientation = Orientation.Horizontal, Spacing = 2, Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
    private readonly Grid _panes = new() { ColumnDefinitions = new ColumnDefinitions("*,0,0") };
    private readonly Border _divider = new() { Width = 1, Margin = new Thickness(0, 6, 0, 52) };
    private readonly Border _pill = new() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, 10), CornerRadius = new CornerRadius(14), Padding = new Thickness(6, 3) };
    private readonly Border _toast = new() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(16, 0, 16, 48), CornerRadius = new CornerRadius(10), Padding = new Thickness(12, 7), IsVisible = false, MaxWidth = 380 };
    private readonly Panel _overlayHost = new() { IsVisible = false };
    private readonly DispatcherTimer _chromeSleep = new() { Interval = TimeSpan.FromMilliseconds(1600) };
    private readonly DispatcherTimer _toastTimer = new() { Interval = TimeSpan.FromMilliseconds(2600) };
    private bool _chromeAwake = true;
    private Anim.Handle? _chromeAnim;
    private Action? _toastAction;
    /// <summary>Ctrl+Z undoes the toast's action (a delete) until the user types something.</summary>
    private bool _undoArmed;

    private void BuildLayout()
    {
        var drag = new Border { Background = Brushes.Transparent };
        WindowDecorationProperties.SetElementRole(drag, WindowDecorationsElementRole.TitleBar);
        Grid.SetColumnSpan(drag, 2);
        _top.Children.Add(drag);
        _top.Children.Add(_meta);
        Grid.SetColumn(_icons, 1);
        _top.Children.Add(_icons);
        _content.Children.Add(_top);

        _left = NewPane();
        _right = NewPane();
        _active = _left;
        _panes.Children.Add(_left);
        Grid.SetColumn(_divider, 1); _panes.Children.Add(_divider);
        Grid.SetColumn(_right, 2); _panes.Children.Add(_right);
        Grid.SetRow(_panes, 1);
        _content.Children.Add(_panes);

        Grid.SetRow(_overlayHost, 1);
        _content.Children.Add(_overlayHost);

        var root = new Grid();
        root.Children.Add(_tint);
        root.Children.Add(_content);
        root.Children.Add(_toast);
        root.Children.Add(_pill);
        Content = root;

        _pill.PointerEntered += (_, _) => WakeChrome(hold: true);
        _top.PointerEntered += (_, _) => WakeChrome(hold: true);
        _chromeSleep.Tick += (_, _) => { _chromeSleep.Stop(); SleepChrome(soon: false); };
        _toastTimer.Tick += (_, _) => { _toastTimer.Stop(); HideToast(); };
    }

    private PadView NewPane()
    {
        var pane = new PadView(_palette, Settings, _timers);
        pane.Edited += () => { _undoArmed = false; Edited(); };
        pane.ModeChanged += UpdateChrome;
        pane.Toast += t => Toast(t);
        pane.Copy += text => _ = CopyText(text);
        pane.FocusedPane += () => { if (_active != pane) { _active = pane; UpdateChrome(); } };
        return pane;
    }

    /// <summary>Recreates chrome controls (palette changes), then refreshes their content.</summary>
    private void RebuildChrome()
    {
        var p = _palette;
        _icons.Children.Clear();
        _icons.Children.Add(Kit.IconButton(Glyph.Search, "Search notes (Ctrl+F)", p, () => OpenSearch()));
        _icons.Children.Add(Kit.IconButton(Glyph.Split, "Split view with a slot (Ctrl+\\)", p, ToggleSplit));
        _icons.Children.Add(Kit.IconButton(Settings.Pinned ? Glyph.Pinned : Glyph.Pin, Settings.Pinned ? "Unpin (Ctrl+T)" : "Keep on top (Ctrl+T)", p, TogglePin));
        _icons.Children.Add(Kit.IconButton(Glyph.Settings, "Settings (Ctrl+,)", p, OpenSettings));
        _icons.Children.Add(Kit.IconButton(Glyph.Hide, "Hide (Esc)", p, HideResident));
        if (Settings.Pinned && _icons.Children[2] is Button pin && pin.Content is Border { Child: TextBlock glyph }) glyph.Foreground = new SolidColorBrush(p.Accent);
        _divider.Background = new SolidColorBrush(p.Faint, .45);
        _pill.Background = new SolidColorBrush(p.Surface, p.Dark ? .86 : .9);
        _pill.BorderBrush = new SolidColorBrush(p.Faint, .35); _pill.BorderThickness = new Thickness(1);
        _pill.BoxShadow = new BoxShadows(new BoxShadow { Blur = 14, OffsetY = 3, Color = Color.FromArgb(p.Dark ? (byte)90 : (byte)28, 0, 0, 0) });
        _toast.Background = new SolidColorBrush(p.Ink, .92);
        _toast.BoxShadow = _pill.BoxShadow;
        _overlayHost.Background = new SolidColorBrush(p.Canvas, Settings.Material == "solid" ? 1 : .55);
        UpdateChrome();
    }

    /// <summary>Refreshes metadata text, the pager and slot dots.</summary>
    private void UpdateChrome()
    {
        if (_left is null || _active?.Note is not { } note) return;
        var p = _palette;
        _meta.Children.Clear();
        if (note.Slot > 0) _meta.Children.Add(Chip($"slot {note.Slot}", p.Accent));
        var label = Modes.Label(_active.Mode, _active.CodeLanguage);
        if (label.Length > 0) _meta.Children.Add(Chip(label, p.Keyword));
        _meta.Children.Add(Kit.Text(Kit.Ago(note.Modified), p.Faint, 11.5));
        if (_timers.AnyRunning && _active.Mode != NoteMode.Timer) _meta.Children.Add(Kit.Text("● timer running", p.Number, 11.5));
        BuildPill();
    }

    private Border Chip(string text, Color color) => new()
    {
        Background = new SolidColorBrush(color, _palette.Dark ? .18 : .13), CornerRadius = new CornerRadius(6), Padding = new Thickness(7, 1, 7, 2),
        Child = Kit.Text(text, color, 11.5, FontWeight.SemiBold), VerticalAlignment = VerticalAlignment.Center
    };

    private void BuildPill()
    {
        var p = _palette;
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        var onStack = Data.ViewSlot == 0;
        row.Children.Add(Kit.IconButton(Glyph.Back, "Previous note (Ctrl+[)", p, () => Navigate(-1), 24));
        var count = Kit.Text(onStack ? $"{Data.Current + 1} / {Data.Stack.Count}" : "stack", onStack ? p.Muted : p.Faint, 11.5, FontWeight.SemiBold);
        count.Margin = new Thickness(4, 0); count.MinWidth = 38; count.TextAlignment = TextAlignment.Center;
        var countButton = Kit.Plain(count, p, height: 22);
        countButton.Click += (_, _) => { if (!onStack) ShowStack(); else OpenSearch(); };
        Avalonia.Automation.AutomationProperties.SetName(countButton, onStack ? $"Note {Data.Current + 1} of {Data.Stack.Count}" : "Back to the stack");
        row.Children.Add(countButton);
        row.Children.Add(Kit.IconButton(Glyph.Forward, "Next note · past the last one starts a new note (Ctrl+])", p, () => Navigate(1), 24));
        row.Children.Add(new Border { Width = 1, Height = 14, Background = new SolidColorBrush(p.Faint, .5), Margin = new Thickness(5, 0) });
        for (var i = 1; i <= 9; i++)
        {
            var slot = i;
            var filled = !Data.Slots[i - 1].IsEmpty;
            var current = Data.ViewSlot == i || (Settings.Split && Settings.SplitSlot == i);
            var dot = new Border
            {
                Width = current ? 16 : 7, Height = 7, CornerRadius = new CornerRadius(3.5),
                Background = current ? new SolidColorBrush(p.Accent) : filled ? new SolidColorBrush(p.Muted) : Brushes.Transparent,
                BorderBrush = new SolidColorBrush(current ? p.Accent : p.Faint), BorderThickness = new Thickness(filled || current ? 0 : 1.2),
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
            };
            var b = Kit.Plain(dot, p, current ? 24 : 16, 22, 6);
            b.Click += (_, _) => OpenSlot(slot);
            var title = Data.Slots[i - 1].IsEmpty ? "empty" : Data.Slots[i - 1].Title;
            ToolTip.SetTip(b, $"Slot {i} · {title} (Ctrl+{i})");
            Avalonia.Automation.AutomationProperties.SetName(b, $"Slot {i}");
            row.Children.Add(b);
        }
        _pill.Child = row;
    }

    // ---------- chrome visibility: present when the mouse moves, gone while typing ----------

    private void WakeChrome(bool hold = false)
    {
        _chromeSleep.Stop();
        if (!_chromeAwake) { _chromeAwake = true; FadeChrome(1); }
        if (!hold && Settings.Chrome == "fade") _chromeSleep.Start();
    }

    private void SleepChrome(bool soon)
    {
        if (Settings.Chrome != "fade" || _overlayOpen) return;
        if (_pill.IsPointerOver || _top.IsPointerOver) { if (soon) return; }
        if (soon) { _chromeSleep.Stop(); _chromeSleep.Start(); return; }
        if (!_chromeAwake) return;
        _chromeAwake = false;
        FadeChrome(0);
    }

    private void FadeChrome(double to)
    {
        _chromeAnim?.Cancel();
        var from = _icons.Opacity;
        _pill.IsHitTestVisible = to > 0; _icons.IsHitTestVisible = to > 0;
        _chromeAnim = Anim.Run(this, to > from ? 90 : 220, t =>
        {
            var v = from + (to - from) * t;
            _icons.Opacity = v; _pill.Opacity = v;
        }, decorative: false);
    }

    // ---------- toast ----------

    internal void Toast(string text, string? action = null, Action? onAction = null)
    {
        var p = _palette;
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        row.Children.Add(new TextBlock { Text = text, Foreground = new SolidColorBrush(p.Canvas), FontSize = 12.5, TextWrapping = TextWrapping.Wrap, MaxWidth = 300, VerticalAlignment = VerticalAlignment.Center });
        _toastAction = onAction;
        _undoArmed = onAction is not null;
        if (action is not null && onAction is not null)
        {
            var label = Kit.Text(action, p.Accent, 12.5, FontWeight.SemiBold);
            label.Margin = new Thickness(6, 1);
            var button = Kit.Plain(label, p with { Ink = p.Canvas });
            button.Click += (_, _) => { onAction(); HideToast(); };
            row.Children.Add(button);
        }
        _toast.Child = row;
        _toast.IsVisible = true;
        Anim.Run(_toast, 120, t => _toast.Opacity = t);
        _toastTimer.Stop();
        _toastTimer.Interval = TimeSpan.FromMilliseconds(action is null ? 2400 : 7000);
        _toastTimer.Start();
    }

    private void HideToast()
    {
        _toastAction = null; _undoArmed = false;
        Anim.Run(_toast, 160, t => _toast.Opacity = 1 - t, () => _toast.IsVisible = false);
    }

    /// <summary>A tiny shake: there is nothing further in that direction.</summary>
    private void Bump(int direction)
    {
        var shift = new TranslateTransform();
        _active.RenderTransform = shift;
        Anim.Run(_active, 180, t => shift.X = direction * 7 * Math.Sin(t * Math.PI), () => _active.RenderTransform = null);
    }
}
