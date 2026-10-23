using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace AntiQuickPad.Views.Common;

// M11b-settings-window: glass settings beside the pad. Presentation only: every control binds a command ID.
internal sealed class SettingsWindow : Window
{
    internal sealed record State(string Theme, string Material, double Tint, string Bars, string Hotkey, string? HotkeyError,
        bool HotkeyLocked, bool Startup, double FontSize, double MarkerOpacity, double StrikeEm);

    private readonly Func<string, ICommand> _command;
    private readonly Func<State> _state;
    private readonly Dictionary<string, string> _routes = [];
    private ShellPalette _palette;
    private bool _capturing;
    private TextBlock? _hotkeyText, _hotkeyNote;
    private Button? _hotkeyButton;
    private readonly Dictionary<string, DispatcherTimer> _debounce = [];

    internal IReadOnlyDictionary<string, string> Routes => _routes;

    internal SettingsWindow(ShellPalette palette, Func<string, ICommand> command, Func<State> state)
    {
        _palette = palette; _command = command; _state = state;
        Title = "AntiQuickPad settings"; ShowInTaskbar = false; CanResize = false;
        Width = 320; Height = palette.Tokens.Window.Height;
        WindowDecorations = WindowDecorations.BorderOnly;
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = palette.Tokens.TopBar;
        WindowStartupLocation = WindowStartupLocation.Manual;
        Background = Brushes.Transparent;
        AddHandler(KeyDownEvent, OnKey, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        Rebuild(palette);
    }

    internal void Rebuild(ShellPalette palette)
    {
        _palette = palette;
        var t = palette.Tokens; var c = palette.Colors; var look = palette.Look; var s = _state();
        RequestedThemeVariant = c.Canvas.R + c.Canvas.G + c.Canvas.B < 384 ? ThemeVariant.Dark : ThemeVariant.Light;
        TransparencyLevelHint = look.Material switch
        {
            "acrylic" => [WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Mica, WindowTransparencyLevel.None],
            "mica" => [WindowTransparencyLevel.Mica, WindowTransparencyLevel.None],
            _ => [WindowTransparencyLevel.None]
        };
        TransparencyBackgroundFallback = new SolidColorBrush(c.Canvas);
        // Fluent sliders and toggles read the system accent; give them the token accent instead of the Windows colour.
        foreach (var key in new[] { "SystemAccentColor", "SystemAccentColorDark1", "SystemAccentColorDark2", "SystemAccentColorDark3",
            "SystemAccentColorLight1", "SystemAccentColorLight2", "SystemAccentColorLight3" }) Resources[key] = c.Accent;
        foreach (var key in new[] { "SliderTrackValueFill", "SliderTrackValueFillPointerOver", "SliderTrackValueFillPressed",
            "SliderThumbBackground", "SliderThumbBackgroundPointerOver", "SliderThumbBackgroundPressed",
            "ToggleSwitchFillOn", "ToggleSwitchFillOnPointerOver", "ToggleSwitchFillOnPressed", "ToggleSwitchStrokeOn" })
            Resources[key] = new SolidColorBrush(c.Accent);
        Foreground = new SolidColorBrush(c.Ink); FontFamily = new FontFamily(t.InterfaceFont);
        _routes.Clear();
        var ink = new SolidColorBrush(c.Ink); var muted = new SolidColorBrush(c.Muted); var selectedFill = new SolidColorBrush(look.CodePanel);

        var list = new StackPanel { Spacing = 6, Margin = new Thickness(t.Inset, 0, t.Inset, t.Inset) };
        TextBlock Heading(string text) => new() { Text = text.ToUpperInvariant(), FontSize = t.MenuHeadingSize, Foreground = muted, Margin = new Thickness(0, 14, 0, 2) };
        Button Action(string label, string id, object? parameter = null, bool selected = false, string? text = null)
        {
            var b = new Button
            {
                Content = new TextBlock { Text = text ?? label, VerticalAlignment = VerticalAlignment.Center },
                Command = _command(id), CommandParameter = parameter, Height = t.MenuRow, MinWidth = t.TargetMin,
                Padding = new Thickness(t.MenuItem, 0), CornerRadius = new CornerRadius(t.MenuItemRadius), BorderThickness = new Thickness(0),
                Background = selected ? selectedFill : Brushes.Transparent, Foreground = ink, HorizontalContentAlignment = HorizontalAlignment.Center
            };
            AutomationProperties.SetName(b, label); ToolTip.SetTip(b, label);
            ShellLayoutContext.FocusRing(b, look, t);
            _routes[label] = id;
            return b;
        }
        Control Segments(params Button[] buttons)
        {
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions(string.Join(',', buttons.Select(_ => "*"))) };
            for (var i = 0; i < buttons.Length; i++) { buttons[i].HorizontalAlignment = HorizontalAlignment.Stretch; Grid.SetColumn(buttons[i], i); grid.Children.Add(buttons[i]); }
            return grid;
        }
        Control SliderRow(string label, string id, double min, double max, double step, double value, Func<double, string> format)
        {
            var valueText = new TextBlock { Text = format(value), Foreground = muted, FontSize = t.StatusSize, VerticalAlignment = VerticalAlignment.Center };
            var slider = new Slider { Minimum = min, Maximum = max, Value = value, SmallChange = step, LargeChange = step * 4, TickFrequency = step,
                IsSnapToTickEnabled = true, Height = t.TargetMin + 8, VerticalAlignment = VerticalAlignment.Center };
            AutomationProperties.SetName(slider, label);
            var command = _command(id);
            // Debounced: one preference write (and at most one appearance rebuild) after the slider settles.
            slider.PropertyChanged += (_, e) =>
            {
                if (e.Property != Slider.ValueProperty) return;
                valueText.Text = format(slider.Value);
                Debounce(id, () => command.Execute(slider.Value));
            };
            _routes[label] = id;
            var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            header.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center });
            Grid.SetColumn(valueText, 1); header.Children.Add(valueText);
            return new StackPanel { Spacing = 0, Children = { header, slider } };
        }

        // Header: title and close (drag strip behind it).
        var title = new TextBlock { Text = "Settings", FontSize = t.MenuLabelSize + 2, FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center };
        var close = new Button { Content = new TextBlock { Text = "", FontFamily = new FontFamily(t.IconFont), FontSize = t.IconSize - 3,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
            Width = t.Button.Width, Height = t.Button.Height, Padding = new Thickness(0), Background = Brushes.Transparent, BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(t.ButtonRadius), Foreground = ink, Command = _command("settings.toggle") };
        AutomationProperties.SetName(close, "Close settings"); ToolTip.SetTip(close, "Close settings (Esc)"); ShellLayoutContext.FocusRing(close, look, t);
        var header = new Grid { Height = t.TopBar, ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(t.Inset, 0, t.Inset - 8, 0) };
        header.Children.Add(title); Grid.SetColumn(close, 1); header.Children.Add(close);

        list.Children.Add(Heading("Note"));
        list.Children.Add(Segments(Action("New note", "note.create"), Action("Delete note", "note.delete")));

        list.Children.Add(Heading("Theme"));
        list.Children.Add(Segments(Action("System", "theme.system", selected: s.Theme == "system"),
            Action("Light", "theme.light", selected: s.Theme == "light"), Action("Dark", "theme.dark", selected: s.Theme == "dark")));

        list.Children.Add(Heading("Background"));
        list.Children.Add(Segments(Action("Material: Acrylic", "settings.material", "acrylic", s.Material == "acrylic", "Acrylic"),
            Action("Material: Mica", "settings.material", "mica", s.Material == "mica", "Mica"),
            Action("Material: Solid", "settings.material", "solid", s.Material == "solid", "Solid")));
        _routes["Material"] = "settings.material";
        list.Children.Add(SliderRow("Tint", "settings.tint", .5, 1, .05, s.Tint, v => $"{v * 100:0} %"));

        list.Children.Add(Heading("Bars"));
        list.Children.Add(Segments(Action("Bars: show on hover", "settings.bars", "hover", s.Bars == "hover", "On hover"),
            Action("Bars: always", "settings.bars", "always", s.Bars == "always", "Always")));
        _routes["Bars"] = "settings.bars";

        list.Children.Add(Heading("Hotkey"));
        _hotkeyText = new TextBlock { Text = s.Hotkey, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 8, 0) };
        _hotkeyButton = new Button { Content = new TextBlock { Text = "Change", VerticalAlignment = VerticalAlignment.Center }, Height = t.MenuRow,
            Padding = new Thickness(t.MenuItem, 0), CornerRadius = new CornerRadius(t.MenuItemRadius), BorderThickness = new Thickness(0),
            Background = selectedFill, Foreground = ink, IsEnabled = !s.HotkeyLocked };
        AutomationProperties.SetName(_hotkeyButton, "Hotkey"); ShellLayoutContext.FocusRing(_hotkeyButton, look, t);
        _hotkeyButton.Click += (_, _) => SetCapture(!_capturing);
        _routes["Hotkey"] = "settings.hotkey"; _routes["Hotkey capture"] = "settings.hotkey-capture";
        var hotkeyRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        hotkeyRow.Children.Add(_hotkeyText); Grid.SetColumn(_hotkeyButton, 1); hotkeyRow.Children.Add(_hotkeyButton);
        list.Children.Add(hotkeyRow);
        _hotkeyNote = new TextBlock { Text = s.HotkeyLocked ? "Set by ANTIQUICKPAD_HOTKEY." : s.HotkeyError ?? "", FontSize = t.StatusSize,
            Foreground = s.HotkeyError is null ? muted : new SolidColorBrush(look.Accent), TextWrapping = TextWrapping.Wrap, IsVisible = s.HotkeyLocked || s.HotkeyError is not null };
        list.Children.Add(_hotkeyNote);
        var startup = new ToggleSwitch { IsChecked = s.Startup, OnContent = "Start with Windows", OffContent = "Start with Windows" };
        AutomationProperties.SetName(startup, "Start with Windows");
        var startupCommand = _command("settings.startup");
        startup.IsCheckedChanged += (_, _) => startupCommand.Execute(startup.IsChecked == true);
        _routes["Start with Windows"] = "settings.startup";
        list.Children.Add(startup);

        list.Children.Add(Heading("Editor"));
        list.Children.Add(SliderRow("Font size", "settings.font-size", 11, 22, 1, s.FontSize, v => $"{v:0} px"));
        list.Children.Add(SliderRow("Marker dimness", "settings.marker-dim", .2, 1, .05, s.MarkerOpacity, v => $"{v * 100:0} %"));
        list.Children.Add(SliderRow("Strike thickness", "settings.strike", .05, .25, .01, s.StrikeEm, v => $"{v:0.00} em"));

        list.Children.Add(Heading("Window"));
        list.Children.Add(Segments(Action("Minimize", "window.minimize"), Action("Quit AntiQuickPad", "app.quit", text: "Quit")));

        var tint = new Border { Background = new SolidColorBrush(c.Canvas, look.TintAlpha), IsHitTestVisible = false };
        var drag = new Border { Height = t.TopBar, Background = Brushes.Transparent, VerticalAlignment = VerticalAlignment.Top };
        WindowDecorationProperties.SetElementRole(drag, WindowDecorationsElementRole.TitleBar);
        var body = new DockPanel();
        DockPanel.SetDock(header, Dock.Top); body.Children.Add(header);
        body.Children.Add(new ScrollViewer { Content = list, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled });
        Content = new Grid { Children = { tint, drag, body } };
    }

    // Only the tint layer and material change for these; no rebuild, no focus change.
    internal void ApplyGlass(ShellPalette palette)
    {
        if (Content is Grid { Children: [Border tint, ..] }) tint.Background = new SolidColorBrush(palette.Colors.Canvas, palette.Look.TintAlpha);
    }

    internal void RefreshHotkey()
    {
        var s = _state();
        if (_hotkeyText is not null) _hotkeyText.Text = _capturing ? "Press the new shortcut…" : s.Hotkey;
        if (_hotkeyNote is not null)
        {
            _hotkeyNote.Text = s.HotkeyLocked ? "Set by ANTIQUICKPAD_HOTKEY." : s.HotkeyError ?? "";
            _hotkeyNote.IsVisible = s.HotkeyLocked || s.HotkeyError is not null;
        }
        if (_hotkeyButton?.Content is TextBlock label) label.Text = _capturing ? "Cancel" : "Change";
    }

    internal void PlaceBeside(Window owner)
    {
        if (Shell.HiddenWindows.Enabled) return;
        var scale = owner.RenderScaling;
        var ownerWidth = (int)Math.Round(owner.Bounds.Width * scale);
        var width = (int)Math.Round(Width * scale);
        var gap = (int)Math.Round(8 * scale);
        var screen = owner.Screens.ScreenFromWindow(owner)?.WorkingArea;
        var x = owner.Position.X + ownerWidth + gap;
        if (screen is { } area && x + width > area.Right) x = Math.Max(area.X, owner.Position.X - width - gap);
        Height = owner.Bounds.Height;
        Position = new PixelPoint(x, owner.Position.Y);
    }

    private void SetCapture(bool capturing)
    {
        _capturing = capturing;
        _command("settings.hotkey-capture").Execute(capturing);
        RefreshHotkey();
    }

    private void OnKey(object? sender, KeyEventArgs e)
    {
        if (_capturing)
        {
            e.Handled = true;
            if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None) { SetCapture(false); return; }
            if (e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin) return;
            var key = KeyName(e.Key);
            var parts = new List<string>();
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) parts.Add("Ctrl");
            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt)) parts.Add("Alt");
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) parts.Add("Shift");
            if (e.KeyModifiers.HasFlag(KeyModifiers.Meta)) parts.Add("Win");
            parts.Add(key ?? e.Key.ToString());
            _capturing = false;
            // The controller validates, registers and stores; on failure it keeps (and restores) the old hotkey.
            _command("settings.hotkey").Execute(string.Join('+', parts));
            RefreshHotkey();
            return;
        }
        if (e.Key == Key.Escape) { _command("settings.toggle").Execute(null); e.Handled = true; }
    }

    private static string? KeyName(Key key) => key switch
    {
        >= Key.A and <= Key.Z => key.ToString(),
        >= Key.D0 and <= Key.D9 => ((int)(key - Key.D0)).ToString(),
        >= Key.F1 and <= Key.F24 => key.ToString(),
        Key.Space => "Space", Key.Tab => "Tab", Key.Enter => "Enter", Key.Insert => "Insert", Key.Delete => "Delete",
        _ => null
    };

    private void Debounce(string id, Action action)
    {
        if (!_debounce.TryGetValue(id, out var timer))
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            var owned = timer;
            owned.Tick += (_, _) => { owned.Stop(); (owned.Tag as Action)?.Invoke(); };
            _debounce[id] = timer;
        }
        timer.Tag = action; timer.Stop(); timer.Start();
    }

    // Selftest: flush any pending slider value immediately.
    internal void FlushDebounce() { foreach (var timer in _debounce.Values.Where(t => t.IsEnabled)) { timer.Stop(); (timer.Tag as Action)?.Invoke(); } }
}
