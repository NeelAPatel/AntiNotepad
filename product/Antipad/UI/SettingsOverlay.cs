using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Antipad.Platform;
using Antipad.Theme;

namespace Antipad.UI;

/// <summary>Settings live inside the pad (no second window): one scrolling sheet, every change applies live.</summary>
internal sealed class SettingsOverlay : Grid
{
    private readonly PadWindow _w;
    private readonly Palette _p;
    private readonly ScrollViewer _scroll;
    public double ScrollOffset => _scroll.Offset.Y;
    private Model.Settings S => _w.Settings;

    public SettingsOverlay(PadWindow window, Palette palette, double scroll)
    {
        _w = window; _p = palette;
        Margin = new Thickness(10, 0, 10, 40);
        var panel = new StackPanel { Spacing = 2, Margin = new Thickness(6, 0, 14, 12) };
        _scroll = new ScrollViewer { Content = panel, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled };
        Children.Add(_scroll);

        panel.Children.Add(Kit.PanelHeader("Settings", _p, _w.CloseOverlay));

        panel.Children.Add(Kit.Section(_p, "Look"));
        panel.Children.Add(Row("Theme", Kit.Segmented(_p, [("system", "System"), ("light", "Light"), ("dark", "Dark")], S.Theme, v => Set(() => S.Theme = v))));
        panel.Children.Add(Row("Light palette", Swatches(false, S.LightPalette, v => Set(() => { S.LightPalette = v; if (S.Theme == "dark") S.Theme = "light"; }))));
        panel.Children.Add(Row("Dark palette", Swatches(true, S.DarkPalette, v => Set(() => { S.DarkPalette = v; if (S.Theme == "light") S.Theme = "dark"; }))));
        panel.Children.Add(Kit.Section(_p, "Make it yours"));
        panel.Children.Add(Row("Accent", AccentSwatches()));
        panel.Children.Add(Row("Paper tone", Kit.Segmented(_p, Palette.Tones.Select(t => (t.Id, t.Name)).ToList(), S.Tone, v => Set(() => S.Tone = v))));
        panel.Children.Add(Row("Split pane", Kit.Segmented(_p, new[] { ("same", "Same as main") }.Concat(Palette.For(_p.Dark).Select(x => (x.Id, x.Name))).ToList(), S.SplitPalette, v => Set(() => S.SplitPalette = v)), "its own palette for the slot beside you"));
        panel.Children.Add(Preview());
        panel.Children.Add(Kit.Section(_p, "Window"));
        panel.Children.Add(Row("Window", Kit.Segmented(_p, [("acrylic", "Acrylic"), ("mica", "Mica"), ("solid", "Solid")], S.Material, v => Set(() => S.Material = v)),
            RenderHint()));
        if (S.Material != "solid")
            panel.Children.Add(Row("Glass tint", Kit.Segmented(_p, [("0.55", "Clear"), ("0.7", "Light"), ("0.82", "Medium"), ("0.92", "Dense")], Closest(S.Tint, 0.55, 0.7, 0.82, 0.92), v => Set(() => S.Tint = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture)))));
        panel.Children.Add(Row("Paper", Kit.Segmented(_p, [("blank", "Blank"), ("lined", "Lined"), ("dotted", "Dotted"), ("grid", "Grid"), ("biggrid", "Large grid")], S.Paper, v => Set(() => S.Paper = v))));
        if (S.Paper != "blank")
            panel.Children.Add(Row("Pattern", Kit.Segmented(_p, [("0", "Subtle"), ("1", "Clear"), ("2", "Bold")], S.PaperStrength.ToString(), v => Set(() => S.PaperStrength = int.Parse(v)))));
        panel.Children.Add(Row("Font", Kit.Segmented(_p, [("Cascadia Mono", "Cascadia Mono"), ("Consolas", "Consolas"), ("Cascadia Code", "Cascadia Code"), ("Segoe UI Variable Text", "Segoe UI")], S.Font, v => Set(() => S.Font = v))));
        panel.Children.Add(Row("UI scale", Kit.Segmented(_p, [("0.9", "90%"), ("1", "100%"), ("1.15", "115%"), ("1.3", "130%"), ("1.5", "150%")], Closest(S.UiScale, 0.9, 1, 1.15, 1.3, 1.5), v => Set(() => S.UiScale = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture))), "everything: chrome, panels and text"));
        panel.Children.Add(Row("Text size", Kit.Segmented(_p, [("13", "13"), ("14", "14"), ("15", "15"), ("16", "16"), ("18", "18"), ("20", "20")], S.FontSize.ToString("0"), v => Set(() => S.FontSize = double.Parse(v)))));
        panel.Children.Add(Row("Controls", Kit.Segmented(_p, [("fade", "Fade while typing"), ("always", "Always visible")], S.Chrome, v => Set(() => S.Chrome = v))));

        panel.Children.Add(Kit.Section(_p, "Behaviour"));
        panel.Children.Add(Row("New note after", Kit.Segmented(_p, [("0", "Never"), ("5", "5 min"), ("15", "15 min"), ("30", "30 min"), ("120", "2 h")], S.FreshNoteAfterMinutes.ToString(), v => Set(() => S.FreshNoteAfterMinutes = int.Parse(v))), "away, the hotkey opens a fresh page"));
        panel.Children.Add(Row("Open at", Kit.Segmented(_p, [("remember", "Where I left it"), ("center", "Centre"), ("mouse", "Mouse"), ("tray", "Above the tray"), ("dropdown", "Drop down from the top")], S.OpenAt, v => Set(() => S.OpenAt = v))));
        panel.Children.Add(Kit.Switch(_p, "Hide when I click away", S.HideOnBlur, v => Set(() => S.HideOnBlur = v), "unless kept on top"));
        panel.Children.Add(Kit.Switch(_p, "Keep on top", S.Pinned, v => Set(() => S.Pinned = v)));
        panel.Children.Add(Kit.Switch(_p, "Fade a pinned pad when I click away", S.DimWhenPinned, v => Set(() => S.DimWhenPinned = v)));
        panel.Children.Add(Kit.Switch(_p, "Answer lines ending in =", S.InlineMath, v => Set(() => S.InlineMath = v), "12 * 4 = shows 48 in any note"));
        panel.Children.Add(Kit.Switch(_p, "Clean markdown", S.CleanMarkdown, v => Set(() => S.CleanMarkdown = v), "hide ** and # except on the line you're editing"));
        panel.Children.Add(Kit.Switch(_p, "Indent guides", S.IndentGuides, v => Set(() => S.IndentGuides = v), "faint lines at each list and code level"));
        panel.Children.Add(Kit.Switch(_p, "Folding", S.Folding, v => Set(() => S.Folding = v), "chevrons to collapse headings, code blocks and nested lists"));
        panel.Children.Add(Kit.Switch(_p, "Shorten long links", S.ShortenLinks, v => Set(() => S.ShortenLinks = v), "the full URL stays in the text"));
        panel.Children.Add(Kit.Switch(_p, "Reduce motion", S.ReduceMotion, v => Set(() => S.ReduceMotion = v)));
        bool startup = false; try { startup = Startup.IsEnabled; } catch { }
        panel.Children.Add(Kit.Switch(_p, "Start with Windows", startup, v => { try { Startup.SetEnabled(v); } catch (Exception ex) { _w.Toast(ex.Message); } Set(() => { }); }));

        panel.Children.Add(Kit.Section(_p, "Shortcut"));
        panel.Children.Add(Hotkey());

        panel.Children.Add(Kit.Section(_p, "About"));
        var about = new TextBlock
        {
            Text = $"Antipad · a quick scratchpad for Windows\nNotes are saved as you type to\n{Program.DataFolder}",
            Foreground = new SolidColorBrush(_p.Muted), FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(8, 0)
        };
        panel.Children.Add(about);
        var help = Kit.Text("Keyboard shortcuts (F1)", _p.Link, 12.5);
        help.Margin = new Thickness(8, 4);
        var helpButton = Kit.Plain(help, _p);
        helpButton.HorizontalAlignment = HorizontalAlignment.Left;
        helpButton.Click += (_, _) => _w.OpenHelp();
        panel.Children.Add(helpButton);
        var reset = Kit.Text("Restore default settings", _p.Muted, 12.5);
        reset.Margin = new Thickness(8, 4);
        var resetButton = Kit.Plain(reset, _p);
        resetButton.HorizontalAlignment = HorizontalAlignment.Left;
        resetButton.Click += (_, _) => _w.ResetSettings();
        panel.Children.Add(resetButton);

        Avalonia.Threading.Dispatcher.UIThread.Post(() => { _scroll.Offset = new Vector(0, scroll); Focus(); }, Avalonia.Threading.DispatcherPriority.Loaded);
        Focusable = true;
    }

    private void Set(Action change) { change(); _w.SettingsChanged(); }

    // Solid switches to software rendering (about 80 MB less memory), which needs a restart either way.
    private string RenderHint()
    {
        var wantSoftware = S.Material == "solid";
        if (wantSoftware == Program.SoftwareRendering) return wantSoftware ? "low-memory mode: no glass, ~40 MB" : "glass uses the GPU, ~125 MB";
        return wantSoftware ? "restart Antipad to save ~80 MB" : "restart Antipad to get the glass back";
    }

    private static string Closest(double value, params double[] options) =>
        options.MinBy(o => Math.Abs(o - value)).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private Control Row(string label, Control control, string? hint = null)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("112,*"), Margin = new Thickness(8, 4) };
        var labels = new StackPanel { Spacing = 1, Margin = new Thickness(0, 4, 8, 0) };
        labels.Children.Add(Kit.Text(label, _p.Ink, 13));
        if (hint is not null) labels.Children.Add(new TextBlock { Text = hint, Foreground = new SolidColorBrush(_p.Muted), FontSize = 11, TextWrapping = TextWrapping.Wrap });
        labels.VerticalAlignment = VerticalAlignment.Top;
        grid.Children.Add(labels);
        Grid.SetColumn(control, 1); grid.Children.Add(control);
        return grid;
    }

    /// <summary>Palette chips that preview each palette's paper, ink and syntax colours.</summary>
    private Control Swatches(bool dark, string current, Action<string> pick)
    {
        var row = new WrapPanel();
        foreach (var palette in Palette.For(dark))
        {
            var selected = palette.Id == current;
            var dots = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 3, HorizontalAlignment = HorizontalAlignment.Center };
            foreach (var c in new[] { palette.Keyword, palette.Identifier, palette.Result })
                dots.Children.Add(new Border { Width = 7, Height = 7, CornerRadius = new CornerRadius(3.5), Background = new SolidColorBrush(c) });
            var face = new StackPanel { Spacing = 4, Children = { new TextBlock { Text = palette.Name, Foreground = new SolidColorBrush(palette.Ink), FontSize = 11.5, HorizontalAlignment = HorizontalAlignment.Center }, dots } };
            var chip = new Border
            {
                Background = new SolidColorBrush(palette.Canvas), CornerRadius = new CornerRadius(9), Padding = new Thickness(10, 7), Margin = new Thickness(0, 0, 6, 6),
                BorderBrush = new SolidColorBrush(selected ? _p.Accent : _p.Faint, selected ? 1 : .5), BorderThickness = new Thickness(selected ? 2 : 1),
                Child = face, Cursor = new Cursor(StandardCursorType.Hand), MinWidth = 84,
            };
            var id = palette.Id;
            chip.PointerPressed += (_, e) => { e.Handled = true; pick(id); };
            Avalonia.Automation.AutomationProperties.SetName(chip, palette.Name);
            row.Children.Add(chip);
        }
        return row;
    }

    private Control AccentSwatches()
    {
        var row = new WrapPanel();
        void Add(string? hex, Color color, string name)
        {
            var selected = S.Accent == hex;
            var dot = new Border
            {
                Width = 22, Height = 22, CornerRadius = new CornerRadius(11), Background = new SolidColorBrush(color), Margin = new Thickness(0, 0, 8, 6),
                BorderBrush = new SolidColorBrush(selected ? _p.Ink : _p.Faint, selected ? .9 : .3), BorderThickness = new Thickness(selected ? 2.5 : 1),
                Cursor = new Cursor(StandardCursorType.Hand),
            };
            ToolTip.SetTip(dot, name);
            Avalonia.Automation.AutomationProperties.SetName(dot, "Accent " + name);
            dot.PointerPressed += (_, e) => { e.Handled = true; Set(() => S.Accent = hex); };
            row.Children.Add(dot);
        }
        var basePalette = Palette.Get(_p.Dark ? S.DarkPalette : S.LightPalette, _p.Dark);
        Add(null, basePalette.Accent, "Palette default");
        foreach (var (hex, name) in Palette.Accents) Add(hex, Color.Parse(hex), name);
        return row;
    }

    /// <summary>Live preview of every colour role in a tiny note, so palette choices are judged on real content.</summary>
    private Control Preview()
    {
        var p = _p;
        TextBlock Line(params (string Text, Color Color)[] runs)
        {
            var t = new TextBlock { FontFamily = new FontFamily(S.Font + ", Cascadia Mono, Consolas"), FontSize = 13, Margin = new Thickness(0, 1) };
            foreach (var (text, color) in runs) t.Inlines!.Add(new Avalonia.Controls.Documents.Run(text) { Foreground = new SolidColorBrush(color) });
            return t;
        }
        var lines = new StackPanel
        {
            Children =
            {
                Line(("math", p.Keyword)),
                Line(("rent", p.Identifier), (" = ", p.Muted), ("1450", p.Number), ("          1,450", p.Result)),
                Line(("// split three ways", p.Comment)),
                Line(("✓ ", p.Done), ("booked the van", p.Done)),
                Line(("see ", p.Ink), ("docs.example.com/…/setup", p.Link)),
            }
        };
        return new Border
        {
            Background = new SolidColorBrush(p.Canvas), BorderBrush = new SolidColorBrush(p.Faint, .4), BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10), Padding = new Thickness(14, 10), Margin = new Thickness(120, 6, 8, 4), Child = lines,
        };
    }

    private Control Hotkey()
    {
        var box = new TextBox
        {
            Text = S.Hotkey ?? GlobalHotkey.DefaultGesture.Label, Width = 190, FontSize = 13,
            Background = new SolidColorBrush(_p.Surface), Foreground = new SolidColorBrush(_p.Ink), BorderBrush = new SolidColorBrush(_p.Faint, .5),
            CornerRadius = new CornerRadius(8), CaretBrush = new SolidColorBrush(_p.Caret),
        };
        Avalonia.Automation.AutomationProperties.SetName(box, "Global shortcut");
        var status = new TextBlock { Text = "Now: " + _w.HotkeyStatus, Foreground = new SolidColorBrush(_p.Muted), FontSize = 11.5, Margin = new Thickness(0, 4, 0, 0), TextWrapping = TextWrapping.Wrap };
        var apply = Kit.Plain(Kit.Text("Apply", _p.Accent, 13, FontWeight.SemiBold), _p, radius: 8);
        if (apply.Content is Border b) b.Padding = new Thickness(12, 5);
        void Apply()
        {
            var text = box.Text?.Trim();
            if (string.IsNullOrEmpty(text)) return;
            if (_w.RegisterHotkey(text)) { S.Hotkey = text; status.Text = "Now: " + _w.HotkeyStatus; _w.SettingsChanged(); }
            else status.Text = $"Couldn't use {text}; kept {_w.HotkeyStatus}.";
        }
        apply.Click += (_, _) => Apply();
        box.KeyDown += (_, e) => { if (e.Key == Key.Enter) { e.Handled = true; Apply(); } };
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { box, apply } };
        return new StackPanel { Margin = new Thickness(8, 2), Children = { Kit.Text("Show / hide from anywhere", _p.Ink, 13), new Border { Height = 6 }, row, status } };
    }
}

/// <summary>F1 cheat sheet. Everything here works without the mouse.</summary>
internal sealed class HelpOverlay : ScrollViewer
{
    private static readonly (string Section, (string Keys, string What)[] Rows)[] Sheet =
    [
        ("Anywhere", [("Ctrl+Alt+Shift+S", "Show / hide Antipad (change in Settings)"), ("Esc", "Hide (or close a panel)")]),
        ("Notes", [("Ctrl+N", "New note"), ("Ctrl+]  ·  Alt+→  ·  swipe", "Next note; past the last one starts a new note"), ("Ctrl+[  ·  Alt+←", "Previous note (in a split pane: the previous slot)"),
            ("Ctrl+Shift+Backspace", "Delete note (to trash, undoable)"), ("Alt+Shift+↑ / ↓", "Move note earlier / later"), ("Ctrl+F", "Search · Ctrl+G grid · Ctrl+T trash")]),
        ("Slots", [("Ctrl+1 … 9", "Open a permanent slot (again to go back)"), ("Ctrl+Shift+1 … 9", "Keep this note in a slot"), ("Ctrl+0", "Back to the stack"), ("Ctrl+\\", "Split: a slot beside your note")]),
        ("Writing", [("Ctrl+Enter", "Toggle a checkbox (or start a timer)"), ("Tab / Shift+Tab", "Indent list items"), ("/", "Commands: /date /sort /json /ocr …"),
            ("Ctrl+Shift+C", "Copy the whole note"), ("Ctrl+Shift+O", "Read text from a screen snip"), ("Ctrl+ + / −", "Text size"), ("Ctrl+T", "Keep on top")]),
        ("Line 1 modes", [("math", "Live answers, variables, units, %, sum"), ("list", "Every line is a checkbox"), ("timer", "25m focus · Enter starts"), ("code py", "Syntax colours"), ("clipboard", "Collects what you copy")]),
    ];

    public HelpOverlay(PadWindow window, Palette p)
    {
        Margin = new Thickness(10, 0, 10, 40);
        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled;
        var panel = new StackPanel { Margin = new Thickness(8, 0, 14, 12) };
        panel.Children.Add(Kit.PanelHeader("Shortcuts", p, window.CloseOverlay));
        foreach (var (section, rows) in Sheet)
        {
            panel.Children.Add(Kit.Section(p, section));
            foreach (var (keys, what) in rows)
            {
                var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*"), Margin = new Thickness(8, 3) };
                var key = new Border
                {
                    Background = new SolidColorBrush(p.Ink, p.Dark ? .1 : .06), CornerRadius = new CornerRadius(5), Padding = new Thickness(7, 2), MinWidth = 150,
                    Child = new TextBlock { Text = keys, FontFamily = new FontFamily("Cascadia Mono, Consolas"), FontSize = 11.5, Foreground = new SolidColorBrush(p.Ink) }
                };
                var text = new TextBlock { Text = what, Foreground = new SolidColorBrush(p.Muted), FontSize = 12.5, Margin = new Thickness(12, 0, 0, 0), TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
                grid.Children.Add(key);
                Grid.SetColumn(text, 1); grid.Children.Add(text);
                panel.Children.Add(grid);
            }
        }
        Content = panel;
        Focusable = true;
        Avalonia.Threading.Dispatcher.UIThread.Post(() => Focus());
    }
}
