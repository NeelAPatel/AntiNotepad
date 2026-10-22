using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Antipad.Platform;
using Antipad.Theme;

namespace Antipad.UI;

/// <summary>
/// Settings live inside the pad (no second window): a rail of sections on the left, the chosen section on the right.
/// Every change applies live; the overlay is rebuilt after each change and comes back on the same section and scroll.
/// </summary>
internal sealed class SettingsOverlay : Grid
{
    private static readonly (string Id, string Name)[] Tabs =
        [("theme", "Theme"), ("editor", "Editor"), ("ui", "UI"), ("feel", "Feel"), ("system", "System"), ("about", "About")];
    // Remembered for this run, so a rebuild after a change (or reopening Settings) lands on the same section.
    private static string _tab = "theme";

    private readonly PadWindow _w;
    private readonly Palette _p;
    private readonly ScrollViewer _scroll;
    private readonly StackPanel _rail = new() { Spacing = 2, Margin = new Thickness(0, 4, 10, 0) };
    public double ScrollOffset => _scroll.Offset.Y;
    private Model.Settings S => _w.Settings;

    public SettingsOverlay(PadWindow window, Palette palette, double scroll)
    {
        _w = window; _p = palette;
        Margin = new Thickness(10, 14, 10, 40);
        RowDefinitions = new RowDefinitions("Auto,*");
        ColumnDefinitions = new ColumnDefinitions("Auto,*");

        var header = Kit.PanelHeader("Settings", _p, _w.CloseOverlay);
        header.Margin = new Thickness(0, 0, 14, 8);
        Grid.SetColumnSpan(header, 2);
        Children.Add(header);

        Grid.SetRow(_rail, 1);
        Children.Add(_rail);

        _scroll = new ScrollViewer { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled };
        Grid.SetRow(_scroll, 1); Grid.SetColumn(_scroll, 1);
        Children.Add(_scroll);

        Show(_tab, scroll);
        Focusable = true;
    }

    /// <summary>Selects a section: rebuilds the rail highlight and the section's rows.</summary>
    private void Show(string tab, double scroll = 0)
    {
        _tab = tab;
        _rail.Children.Clear();
        foreach (var (id, name) in Tabs)
        {
            var selected = id == tab;
            var label = Kit.Text(name, selected ? _p.Ink : _p.Muted, 13, selected ? FontWeight.SemiBold : FontWeight.Normal);
            label.Margin = new Thickness(12, 6, 18, 6);
            var item = new Border
            {
                Child = label, CornerRadius = new CornerRadius(8),
                Background = selected ? new SolidColorBrush(_p.Accent, _p.Dark ? .22 : .16) : Brushes.Transparent,
            };
            var button = Kit.Plain(item, _p, radius: 8);
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            Avalonia.Automation.AutomationProperties.SetName(button, name + " settings");
            var target = id;
            button.Click += (_, _) => { if (_tab != target) Show(target); };
            _rail.Children.Add(button);
        }

        var panel = new StackPanel { Spacing = 2, Margin = new Thickness(0, 0, 14, 12) };
        switch (tab)
        {
            case "theme": ThemeRows(panel); break;
            case "editor": EditorRows(panel); break;
            case "ui": UiRows(panel); break;
            case "feel": FeelRows(panel); break;
            case "system": SystemRows(panel); break;
            default: AboutRows(panel); break;
        }
        _scroll.Content = panel;
        Avalonia.Threading.Dispatcher.UIThread.Post(() => { _scroll.Offset = new Vector(0, scroll); Focus(); }, Avalonia.Threading.DispatcherPriority.Loaded);
    }

    private void ThemeRows(StackPanel panel)
    {
        panel.Children.Add(Preview());
        panel.Children.Add(Row("Theme", Kit.Segmented(_p, [("system", "System"), ("light", "Light"), ("dark", "Dark")], S.Theme, v => Set(() => S.Theme = v))));
        panel.Children.Add(Row("Light palette", Swatches(false, S.LightPalette, v => Set(() => { S.LightPalette = v; if (S.Theme == "dark") S.Theme = "light"; }))));
        panel.Children.Add(Row("Dark palette", Swatches(true, S.DarkPalette, v => Set(() => { S.DarkPalette = v; if (S.Theme == "light") S.Theme = "dark"; }))));
        panel.Children.Add(Row("Accent", AccentSwatches(), "caret, selection, top headings, checkboxes, switches, pin and the current-note dot"));
        panel.Children.Add(Row("Caret", Kit.Segmented(_p, [("bar", "Bar"), ("block", "Block")], S.CaretStyle, v => Set(() => S.CaretStyle = v))));
        panel.Children.Add(Kit.Switch(_p, "Caret blinks", S.CaretBlink, v => Set(() => S.CaretBlink = v), "after a moment of stillness"));
        panel.Children.Add(Kit.Switch(_p, "Caret glides", S.CaretGlide, v => Set(() => S.CaretGlide = v), "slides to its new spot instead of jumping"));
        panel.Children.Add(Row("Split pane", Kit.Segmented(_p, new[] { ("same", "Same as main") }.Concat(Palette.For(_p.Dark).Select(x => (x.Id, x.Name))).ToList(), S.SplitPalette, v => Set(() => S.SplitPalette = v)), "its own palette for the slot beside you"));
    }

    private void EditorRows(StackPanel panel)
    {
        panel.Children.Add(new MotionPreview(_p, S));
        panel.Children.Add(Row("Paper tone", Kit.Segmented(_p, Palette.Tones.Select(t => (t.Id, t.Name)).ToList(), S.Tone, v => Set(() => S.Tone = v))));
        panel.Children.Add(Row("Backing", Kit.Segmented(_p, [("blank", "Blank"), ("lined", "Lined"), ("dotted", "Dotted"), ("grid", "Grid"), ("biggrid", "Large grid")], S.Paper, v => Set(() => S.Paper = v))));
        if (S.Paper != "blank")
            panel.Children.Add(Row("Pattern weight", Kit.Segmented(_p, [("0", "Subtle"), ("1", "Clear"), ("2", "Bold")], S.PaperStrength.ToString(), v => Set(() => S.PaperStrength = int.Parse(v)))));
        panel.Children.Add(Row("Editor font", Kit.Segmented(_p, [("Cascadia Mono", "Cascadia Mono"), ("Consolas", "Consolas"), ("Cascadia Code", "Cascadia Code"), ("Segoe UI Variable Text", "Segoe UI")], S.Font, v => Set(() => S.Font = v))));
        panel.Children.Add(Row("Font size", Kit.Segmented(_p, [("13", "13"), ("14", "14"), ("15", "15"), ("16", "16"), ("18", "18"), ("20", "20")], S.FontSize.ToString("0"), v => Set(() => S.FontSize = double.Parse(v)))));
        panel.Children.Add(Row("Text width", Kit.Segmented(_p, [("0", "Full"), ("60", "60"), ("72", "72"), ("88", "88"), ("110", "110")], Closest(S.TextWidth, 0, 60, 72, 88, 110), v => Set(() => S.TextWidth = int.Parse(v))),
            "characters per line; a wider pad centres the column"));
        panel.Children.Add(Row("Line spacing", Kit.Segmented(_p, [("1.15", "Tight"), ("1.32", "Normal"), ("1.5", "Relaxed"), ("1.7", "Airy")], Closest(S.LineSpacing, 1.15, 1.32, 1.5, 1.7), v => Set(() => S.LineSpacing = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture)))));
        panel.Children.Add(Row("Answer bubbles", Kit.Segmented(_p, [("column", "One column"), ("edge", "Right edge")], S.Bubbles, v => Set(() => S.Bubbles = v)),
            S.Bubbles == "edge" ? "pinned to the right edge of the text" : "lined up just past the longest line"));
        panel.Children.Add(Kit.Section(_p, "Writing"));
        panel.Children.Add(Kit.Switch(_p, "Answer lines ending in =", S.InlineMath, v => Set(() => S.InlineMath = v), "12 * 4 = shows 48 in any note"));
        panel.Children.Add(Kit.Switch(_p, "Clean markdown", S.CleanMarkdown, v => Set(() => S.CleanMarkdown = v), "hide ** and # except on the line you're editing"));
        panel.Children.Add(Kit.Switch(_p, "Indent guides", S.IndentGuides, v => Set(() => S.IndentGuides = v), "faint lines at each list and code level"));
        panel.Children.Add(Kit.Switch(_p, "Folding", S.Folding, v => Set(() => S.Folding = v), "chevrons to collapse headings, code blocks and nested lists"));
        panel.Children.Add(Kit.Switch(_p, "Shorten long links", S.ShortenLinks, v => Set(() => S.ShortenLinks = v), "the full URL stays in the text"));
        panel.Children.Add(Kit.Section(_p, "Code languages"));
        panel.Children.Add(Row("Highlight in code blocks", Kit.Toggles(_p, Editor.CodeColors.Languages.Select(l => (l.Id, l.Name)).ToList(),
            id => !S.DisabledLanguages.Contains(id), id => Set(() => { if (!S.DisabledLanguages.Remove(id)) S.DisabledLanguages.Add(id); })),
            "off: plain text, not offered after ```, never loaded"));
    }

    private void UiRows(StackPanel panel)
    {
        panel.Children.Add(Row("Background", Kit.Segmented(_p, [("acrylic", "Acrylic"), ("mica", "Mica"), ("solid", "Solid")], S.Material, v => Set(() => S.Material = v)),
            RenderHint()));
        if (S.Material != "solid")
            panel.Children.Add(Row("Tint level", Kit.Segmented(_p, [("0.55", "Clear"), ("0.7", "Light"), ("0.82", "Medium"), ("0.92", "Dense")], Closest(S.Tint, 0.55, 0.7, 0.82, 0.92), v => Set(() => S.Tint = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture)))));
        panel.Children.Add(Row("UI scale", Kit.Segmented(_p, [("0.9", "90%"), ("1", "100%"), ("1.15", "115%"), ("1.3", "130%"), ("1.5", "150%")], Closest(S.UiScale, 0.9, 1, 1.15, 1.3, 1.5), v => Set(() => S.UiScale = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture))), "everything: chrome, panels and text"));
        panel.Children.Add(Row("Controls", Kit.Segmented(_p, [("fade", "Fade while typing"), ("always", "Always visible")], S.Chrome, v => Set(() => S.Chrome = v))));
        if (S.Chrome == "fade")
            panel.Children.Add(Row("Fade after", Kit.Segmented(_p, [("800", "0.8 s"), ("1600", "1.6 s"), ("3000", "3 s"), ("6000", "6 s")], Closest(S.FadeAfterMs, 800, 1600, 3000, 6000), v => Set(() => S.FadeAfterMs = int.Parse(v))),
                "once the mouse stops"));
        panel.Children.Add(Kit.Switch(_p, "Fade a pinned pad when I click away", S.DimWhenPinned, v => Set(() => S.DimWhenPinned = v)));
    }

    private void FeelRows(StackPanel panel)
    {
        panel.Children.Add(new MotionPreview(_p, S));
        panel.Children.Add(Row("Scrolling", Kit.Segmented(_p, [("lines", "Lines"), ("free", "Free"), ("rate", "Rate-matched"), ("wheel", "Wheel-driven")], S.Scrolling, v => Set(() => S.Scrolling = v)),
            ScrollHint(S.Scrolling)));
        TuningRows(panel);
        panel.Children.Add(Row("Swipe between notes", Kit.Segmented(_p, [("detent", "Detent"), ("progress", "Progress")], S.Swipe, v => Set(() => S.Swipe = v)),
            S.Swipe == "progress" ? "same feel at any window width; changes as you pass a third of the way" : "follows with resistance; decides when you stop"));
        if (S.Swipe == "progress")
        {
            panel.Children.Add(Row("Swipe length", Kit.Segmented(_p, [("150", "Short"), ("220", "Normal"), ("320", "Long")], Closest(S.SwipeLength, 150, 220, 320), v => Set(() => S.SwipeLength = Num(v))),
                "how far you swipe for a whole note"));
            panel.Children.Add(Row("Changes at", Kit.Segmented(_p, [("0.25", "A quarter"), ("0.35", "A third"), ("0.5", "Halfway")], Closest(S.SwipeFiresAt, 0.25, 0.35, 0.5), v => Set(() => S.SwipeFiresAt = Num(v)))));
        }
        panel.Children.Add(Kit.Switch(_p, "Reduce motion", S.ReduceMotion, v => Set(() => S.ReduceMotion = v), "no glides, coasts or slides"));
    }

    private static double Num(string v) => double.Parse(v, System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Presets for the chosen scrolling feel, shown under it.</summary>
    private void TuningRows(StackPanel panel)
    {
        switch (S.Scrolling)
        {
            case "lines":
                panel.Children.Add(Row("Lines per notch", Kit.Segmented(_p, [("2", "2"), ("3", "3"), ("4", "4"), ("6", "6")], Closest(S.LinesPerNotch, 2, 3, 4, 6), v => Set(() => S.LinesPerNotch = int.Parse(v)))));
                break;
            case "free":
                panel.Children.Add(Row("Push per notch", Kit.Segmented(_p, [("0.65", "Gentle"), ("1", "Normal"), ("1.5", "Strong")], Closest(S.ScrollDistance, 0.65, 1, 1.5), v => Set(() => S.ScrollDistance = Num(v)))));
                panel.Children.Add(Row("Coast", Kit.Segmented(_p, [("10", "Short"), ("6", "Normal"), ("3.5", "Long")], Closest(S.FreeFriction, 10, 6, 3.5), v => Set(() => S.FreeFriction = Num(v)))));
                break;
            case "rate":
            case "wheel":
                panel.Children.Add(Row("Distance per click", Kit.Segmented(_p, [("0.65", "Short"), ("1", "Normal"), ("1.5", "Long")], Closest(S.ScrollDistance, 0.65, 1, 1.5), v => Set(() => S.ScrollDistance = Num(v))),
                    "when you click slowly"));
                panel.Children.Add(Row("Speed boost", Kit.Segmented(_p, [("0", "None"), ("0.5", "A little"), ("1", "Normal"), ("1.6", "A lot")], Closest(S.ScrollBoost, 0, 0.5, 1, 1.6), v => Set(() => S.ScrollBoost = Num(v))),
                    "extra distance when you spin fast"));
                break;
        }
    }

    private static string ScrollHint(string mode) => mode switch
    {
        "free" => "each notch adds speed that coasts",
        "rate" => "slow clicks glide a little; spin faster to go further",
        "wheel" => "moves with the wheel, stops when it stops",
        _ => "each notch glides 3 lines",
    };

    private void SystemRows(StackPanel panel)
    {
        panel.Children.Add(Hotkey());
        panel.Children.Add(NewNoteHotkey());
        panel.Children.Add(Row("Open at", Kit.Segmented(_p, [("remember", "Where I left it"), ("center", "Center"), ("mouse", "Mouse"), ("tray", "Above the tray"), ("dropdown", "Drop down from the top")], S.OpenAt, v => Set(() => S.OpenAt = v))));
        panel.Children.Add(Row("New note after", Kit.Segmented(_p, [("0", "Never"), ("5", "5 min"), ("15", "15 min"), ("30", "30 min"), ("120", "2 h")], S.FreshNoteAfterMinutes.ToString(), v => Set(() => S.FreshNoteAfterMinutes = int.Parse(v))), "away, the hotkey opens a fresh page"));
        panel.Children.Add(Kit.Switch(_p, "Hide when I click away", S.HideOnBlur, v => Set(() => S.HideOnBlur = v), "unless kept on top"));
        panel.Children.Add(Kit.Switch(_p, "Keep on top", S.Pinned, v => Set(() => S.Pinned = v)));
        bool startup = false; try { startup = Startup.IsEnabled; } catch { }
        panel.Children.Add(Kit.Switch(_p, "Start with Windows", startup, v => { try { Startup.SetEnabled(v); } catch (Exception ex) { _w.Toast(ex.Message); } Set(() => { }); }));
    }

    private void AboutRows(StackPanel panel)
    {
        var about = new TextBlock
        {
            Text = $"Antipad · a quick scratchpad for Windows\nNotes are saved as you type to\n{Program.DataFolder}",
            Foreground = new SolidColorBrush(_p.Muted), FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(8, 6, 8, 0)
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

    /// <summary>Label above its control, full width: options don't wrap into orphans in a narrow pad.</summary>
    private Control Row(string label, Control control, string? hint = null)
    {
        var stack = new StackPanel { Spacing = 6, Margin = new Thickness(8, 6, 8, 8) };
        var head = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        head.Children.Add(Kit.Text(label, _p.Ink, 13));
        if (hint is not null) head.Children.Add(new TextBlock { Text = hint, Foreground = new SolidColorBrush(_p.Muted), FontSize = 11.5, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis });
        stack.Children.Add(head);
        stack.Children.Add(control);
        return stack;
    }

    /// <summary>Palette chips that preview each palette's paper, ink and syntax colours.</summary>
    private Control Swatches(bool dark, string current, Action<string> pick)
    {
        // An even grid: three chips per row, every chip the same width.
        var row = new Avalonia.Controls.Primitives.UniformGrid { Columns = 3 };
        foreach (var palette in Palette.For(dark))
        {
            var selected = palette.Id == current;
            var dots = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 3, HorizontalAlignment = HorizontalAlignment.Center };
            foreach (var c in new[] { palette.Keyword, palette.Identifier, palette.Result })
                dots.Children.Add(new Border { Width = 7, Height = 7, CornerRadius = new CornerRadius(3.5), Background = new SolidColorBrush(c) });
            var face = new StackPanel { Spacing = 4, Children = { new TextBlock { Text = palette.Name, Foreground = new SolidColorBrush(palette.Ink), FontSize = 11.5, HorizontalAlignment = HorizontalAlignment.Center }, dots } };
            var chip = new Border
            {
                Background = new SolidColorBrush(palette.Canvas), CornerRadius = new CornerRadius(9), Padding = new Thickness(8, 7), Margin = new Thickness(0, 0, 6, 6),
                BorderBrush = new SolidColorBrush(selected ? _p.Accent : _p.Faint, selected ? 1 : .5), BorderThickness = new Thickness(selected ? 2 : 1),
                Child = face, Cursor = new Cursor(StandardCursorType.Hand),
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
            CornerRadius = new CornerRadius(10), Padding = new Thickness(14, 10), Margin = new Thickness(8, 4, 8, 8), Child = lines,
        };
    }

    /// <summary>Optional second hotkey: empty and Apply turns it off.</summary>
    private Control NewNoteHotkey()
    {
        var box = new TextBox
        {
            Text = S.NewNoteHotkey ?? "", PlaceholderText = "e.g. Ctrl+Alt+Shift+N", Width = 190, FontSize = 13,
            Background = new SolidColorBrush(_p.Surface), Foreground = new SolidColorBrush(_p.Ink), BorderBrush = new SolidColorBrush(_p.Faint, .5),
            CornerRadius = new CornerRadius(8), CaretBrush = new SolidColorBrush(_p.Caret),
        };
        Avalonia.Automation.AutomationProperties.SetName(box, "New note shortcut");
        var status = new TextBlock { Text = "Now: " + _w.NewNoteHotkeyStatus, Foreground = new SolidColorBrush(_p.Muted), FontSize = 11.5, Margin = new Thickness(0, 4, 0, 0), TextWrapping = TextWrapping.Wrap };
        var apply = Kit.Plain(Kit.Text("Apply", _p.Accent, 13, FontWeight.SemiBold), _p, radius: 8);
        if (apply.Content is Border b) b.Padding = new Thickness(12, 5);
        void Apply()
        {
            var text = box.Text?.Trim();
            var value = string.IsNullOrEmpty(text) ? null : text;
            if (_w.RegisterNewNoteHotkey(value)) { S.NewNoteHotkey = value; status.Text = "Now: " + _w.NewNoteHotkeyStatus; _w.SettingsChanged(); }
            else status.Text = $"Couldn't use {text}; it may belong to another app.";
        }
        apply.Click += (_, _) => Apply();
        box.KeyDown += (_, e) => { if (e.Key == Key.Enter) { e.Handled = true; Apply(); } };
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { box, apply } };
        return new StackPanel { Margin = new Thickness(8, 6, 8, 8), Children = { Kit.Text("New note hotkey", _p.Ink, 13), Kit.Text("opens Antipad on a fresh note · leave empty for none", _p.Muted, 11.5), new Border { Height = 6 }, row, status } };
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
        return new StackPanel { Margin = new Thickness(8, 6, 8, 8), Children = { Kit.Text("Hotkey", _p.Ink, 13), Kit.Text("show / hide from anywhere", _p.Muted, 11.5), new Border { Height = 6 }, row, status } };
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
        ("Line 1 modes", [("math", "Live answers, variables, units, %, sum"), ("list", "Every line is a checkbox"), ("timer", "25m focus · Enter starts"), ("code py", "Syntax colors"), ("clipboard", "Collects what you copy")]),
    ];

    public HelpOverlay(PadWindow window, Palette p)
    {
        Margin = new Thickness(10, 14, 10, 40);
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
