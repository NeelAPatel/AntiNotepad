using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AntiNotepad;
using AntiNotepad.Platform;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

/// <summary>
/// Settings in a window of their own (Ctrl+,), laid out like Windows Settings but in AntiNotepad's look: a rail of sections
/// with a search box, then each section's rows grouped in cards. Created when opened, released when closed.
/// A change applies to the pad at once. The open page is rebuilt only when a change adds or removes rows, and the
/// window restyles only when the palette itself changes, so clicking an option never makes anything flash.
/// </summary>
internal sealed partial class SettingsWindow : Window
{
    private static readonly (string Id, string Name, int Icon)[] Sections =
    [
        // Fluent UI System Icons: color, paint_brush, document_edit, tap_single (a finger touching), desktop, info.
        ("theme", "Theme", 0xf2f5), ("ui", "UI", 0xf592), ("editor", "Editor", 0xf386),
        ("feel", "Feel", 0xec8d), ("system", "System", 0xf359), ("about", "About", 0xf4a3),
    ];
    // Remembered while AntiNotepad runs: the last section, and which expanders are open (these start open).
    private static string _section = "theme";
    private static readonly HashSet<string> Expanded = ["light-palette", "backing", "toolbar", "scrolling", "switching", "open-at"];

    private readonly PadWindow _pad;
    private Palette _p;
    private readonly MotionPreview _preview;
    private readonly ScrollViewer _scroll = new() { HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
    private readonly StackPanel _rail = new() { Spacing = 2 };
    private TextBox _find = null!;
    private Border _tint = null!;
    private string? _appliedMaterial;
    private Palette? _stylesPalette;
    private Model.Settings S => _pad.Settings;

    public SettingsWindow(PadWindow pad)
    {
        _pad = pad;
        _p = pad.CurrentPalette;
        Title = $"{AppInfo.Name} Settings";
        Icon = pad.Icon;
        Width = 900; Height = 720; MinWidth = 640; MinHeight = 460;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowDecorations = WindowDecorations.BorderOnly;
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = 42;
        Background = Brushes.Transparent;
        FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI");
        TextOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        _preview = new MotionPreview(_p, S);
        ApplyLook();
        Build();
        Show(_section, animate: false);
        AddHandler(KeyDownEvent, (_, e) =>
        {
            if (e.Key != Key.Escape) return;
            if (e.Source is TextBox box && !ReferenceEquals(box, _find)) return;
            e.Handled = true;
            if (_find.Text is { Length: > 0 }) _find.Text = "";
            else Close();
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        Deactivated += (_, _) => _pad.SettingsLostFocus();
        Closed += (_, _) => { _preview.Release(); _pad.SettingsLostFocus(); };
    }

    // ---------- look ----------

    private void ApplyLook()
    {
        RequestedThemeVariant = _p.Dark ? Avalonia.Styling.ThemeVariant.Dark : Avalonia.Styling.ThemeVariant.Light;
        if (_appliedMaterial != S.Material) { TransparencyLevelHint = Kit.Material(S.Material); _appliedMaterial = S.Material; }
        TransparencyBackgroundFallback = new SolidColorBrush(_p.Canvas);
        if (!Equals(_stylesPalette, _p)) { Kit.InputStyles(Styles, _p); _stylesPalette = _p; }
        // A little denser than the pad: settings are read, not written over the desktop.
        if (_tint is not null) _tint.Background = new SolidColorBrush(_p.Canvas, S.Material == "solid" ? 1 : Math.Max(0.86, S.Tint));
    }

    /// <summary>The pad's palette changed (a setting here, or Windows switching light and dark): dress to match.</summary>
    public void SyncLook()
    {
        if (Equals(_p, _pad.CurrentPalette)) return;
        _p = _pad.CurrentPalette;
        Restyle();
    }

    /// <summary>Dev tooling: open a given section without a click.</summary>
    internal void ShowSection(string id) => Show(id, animate: false);

    /// <summary>Dev tooling: click into the preview (starts its caret blink, the path that once leaked the window).</summary>
    internal void FocusPreview() => _preview.FocusEditor();

    /// <summary>Settings were replaced wholesale (restore defaults, undo): rebuild what's on screen.</summary>
    public void Refresh()
    {
        _p = _pad.CurrentPalette;
        Restyle();
    }

    private void Restyle()
    {
        ApplyLook();
        _preview.ApplyLook(_p, S);
        var query = _find.Text;
        Build();
        if (string.IsNullOrWhiteSpace(query)) Show(_section, animate: false, keepScroll: true);
        else { _find.Text = query; ShowResults(query); }
    }

    /// <summary>Applies a change to the pad. Only <paramref name="rows"/> changes (an option that shows or hides rows) rebuild the page.</summary>
    private void Set(System.Action change, bool rows = false)
    {
        change();
        _pad.SettingsChanged();
        if (!Equals(_p, _pad.CurrentPalette)) { _p = _pad.CurrentPalette; Restyle(); return; }
        ApplyLook();
        _preview.ApplyLook(_p, S);
        if (!rows) return;
        if (_find.Text is { Length: > 0 } query) ShowResults(query);
        else Show(_section, animate: false, keepScroll: true);
    }

    // ---------- frame ----------

    private void Build()
    {
        var root = new Grid { RowDefinitions = new RowDefinitions("42,*") };
        _tint = new Border();
        Grid.SetRowSpan(_tint, 2);
        root.Children.Add(_tint);
        ApplyLook();

        var top = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        var drag = new Border { Background = Brushes.Transparent };
        WindowDecorationProperties.SetElementRole(drag, WindowDecorationsElementRole.TitleBar);
        Grid.SetColumnSpan(drag, 2);
        top.Children.Add(drag);
        var title = Kit.Text($"{AppInfo.Name} Settings", _p.Muted, 12);
        title.Margin = new Thickness(18, 0, 0, 0);
        title.IsHitTestVisible = false;
        top.Children.Add(title);
        var close = Kit.IconButton(Glyph.Close, "Close (Esc)", _p, Close, 34);
        close.Margin = new Thickness(0, 0, 6, 0);
        Grid.SetColumn(close, 1);
        top.Children.Add(close);
        root.Children.Add(top);

        var body = new Grid { ColumnDefinitions = new ColumnDefinitions("214,*") };
        Grid.SetRow(body, 1);
        var railHost = new StackPanel { Margin = new Thickness(10, 4, 10, 10), Spacing = 12 };
        _find = new TextBox
        {
            PlaceholderText = "Find a setting", FontSize = 13, Height = 34, CornerRadius = new CornerRadius(8), Padding = new Thickness(8, 7, 8, 0),
            Foreground = new SolidColorBrush(_p.Ink), CaretBrush = new SolidColorBrush(_p.Caret),
            InnerLeftContent = new GlyphIcon(char.ConvertFromUtf32(0xf68f), _p.Muted, 15, 0, Glyph.Fluent) { Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center },
        };
        Avalonia.Automation.AutomationProperties.SetName(_find, "Find a setting");
        _find.TextChanged += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_find.Text)) Show(_section, animate: false);
            else ShowResults(_find.Text);
        };
        railHost.Children.Add(_find);
        // The rail and the scroller outlive a rebuild (theme change): take them off the old frame first.
        if (_rail.Parent is Panel oldRail) oldRail.Children.Remove(_rail);
        railHost.Children.Add(_rail);
        body.Children.Add(railHost);
        Grid.SetColumn(_scroll, 1);
        if (_scroll.Parent is Panel old) old.Children.Remove(_scroll);
        body.Children.Add(_scroll);
        root.Children.Add(body);
        Content = root;
    }

    private void BuildRail(string? selected)
    {
        _rail.Children.Clear();
        foreach (var (id, name, glyph) in Sections)
        {
            var on = id == selected;
            var face = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Margin = new Thickness(12, 0), VerticalAlignment = VerticalAlignment.Center };
            face.Children.Add(new GlyphIcon(char.ConvertFromUtf32(glyph), on ? IconInk : _p.Muted, 18, 0, Glyph.Fluent) { VerticalAlignment = VerticalAlignment.Center });
            face.Children.Add(Kit.Text(name, on ? _p.Ink : _p.Muted, 14, on ? FontWeight.SemiBold : FontWeight.Normal));
            var item = Kit.Plain(face, _p, height: 36, radius: 8);
            item.HorizontalAlignment = HorizontalAlignment.Stretch;
            if (on && item.Content is Border b)
            {
                var fill = new SolidColorBrush(_p.Accent, _p.Dark ? .22 : .18);
                b.Background = fill;
                item.PointerExited += (_, _) => b.Background = fill;
            }
            var target = id;
            item.Click += (_, _) =>
            {
                if (_find.Text is { Length: > 0 }) _find.Text = "";
                if (_section != target || _scroll.Content is null) Show(target);
            };
            Avalonia.Automation.AutomationProperties.SetName(item, name + " settings");
            _rail.Children.Add(item);
        }
    }

    // ---------- pages ----------

    private StackPanel NewPage() => new() { Margin = new Thickness(16, 2, 28, 32), MaxWidth = 780, HorizontalAlignment = HorizontalAlignment.Stretch };

    private void Show(string section, bool animate = true, bool keepScroll = false)
    {
        _section = section;
        BuildRail(section);
        var offset = _scroll.Offset;
        var page = BuildSection(section, withPreview: true);
        _scroll.Content = page;
        if (keepScroll) Dispatcher.UIThread.Post(() => _scroll.Offset = offset, DispatcherPriority.Loaded);
        else _scroll.Offset = default;
        if (!animate || Anim.Reduced) return;
        // Tab change: the page rises a few pixels as it fades in.
        var shift = new TranslateTransform(0, 10);
        page.RenderTransform = shift;
        page.Opacity = 0;
        Anim.Run(page, 150, t => { var e = Anim.EaseOut(t); page.Opacity = e; shift.Y = 10 * (1 - e); }, () => { page.Opacity = 1; page.RenderTransform = null; });
    }

    private StackPanel BuildSection(string section, bool withPreview)
    {
        var page = NewPage();
        page.Children.Add(Heading(Sections.First(s => s.Id == section).Name));
        switch (section)
        {
            case "theme": ThemePage(page); break;
            case "ui": UiPage(page); break;
            case "editor": EditorPage(page, withPreview); break;
            case "feel": FeelPage(page, withPreview); break;
            case "system": SystemPage(page); break;
            default: AboutPage(page); break;
        }
        return page;
    }

    /// <summary>Search: every row whose title or description matches, gathered under its section's name, still live.</summary>
    private void ShowResults(string query)
    {
        BuildRail(null);
        var page = NewPage();
        page.Children.Add(Heading("Results"));
        var any = false;
        foreach (var (id, name, _) in Sections)
        {
            var built = BuildSection(id, withPreview: false);
            var matches = built.Children.OfType<Border>().Where(c => Equals(c.Tag, "card"))
                .SelectMany(c => ((StackPanel)c.Child!).Children.OfType<Border>())
                .Where(r => r.Tag is string tag && tag.Replace("sub ", "").Contains(query.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (matches.Count == 0) continue;
            any = true;
            // Nested rows come along even when their expander is closed; their indent lines them up with the titles.
            foreach (var row in matches) { ((Panel)row.Parent!).Children.Remove(row); row.IsVisible = true; }
            page.Children.Add(Group(name));
            page.Children.Add(Card([.. matches]));
        }
        if (!any) page.Children.Add(Kit.Text($"No setting matches “{query.Trim()}”.", _p.Muted, 13));
        _scroll.Content = page;
        _scroll.Offset = default;
    }

    private void ThemePage(StackPanel page)
    {
        page.Children.Add(Group("Colors"));
        page.Children.Add(Card([
            Row(ModeIcon(), "Mode", "Follow Windows, or stay light or dark",
                Seg([("system", "System"), ("light", "Light"), ("dark", "Dark")], S.Theme, v => Set(() => S.Theme = v))),
            .. Expander("light-palette", Fl(0xf8a1), "Light mode palette", null, Palette.Get(S.LightPalette, false).Name,
                SubRow(null, PaletteChips(false, S.LightPalette, v => Set(() => { S.LightPalette = v; if (S.Theme == "dark") S.Theme = "light"; })), wide: true)),
            .. Expander("dark-palette", Fl(0xf87d), "Dark mode palette", null, Palette.Get(S.DarkPalette, true).Name,
                SubRow(null, PaletteChips(true, S.DarkPalette, v => Set(() => { S.DarkPalette = v; if (S.Theme == "light") S.Theme = "dark"; })), wide: true)),
            Row(Fl(0xf2f5), "Accent", "Caret, selection, headings and switches", AccentDots()),
        ]));

        page.Children.Add(Group("Caret"));
        var cells = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        cells.Children.Add(Tile(CaretPicture(false), "Bar", S.CaretStyle != "block", () => Set(() => S.CaretStyle = "bar", rows: true), 58));
        cells.Children.Add(Tile(CaretPicture(true), "Block", S.CaretStyle == "block", () => Set(() => S.CaretStyle = "block", rows: true), 58));
        page.Children.Add(Card(
            Row(CursorIcon("Ab", clock: false), "Caret style", null, cells),
            Row(CursorIcon("A", clock: true), "Blink", null, Toggle(S.CaretBlink, v => Set(() => S.CaretBlink = v), "Blink")),
            Row(Fl(0xe0cd), "Glide between positions", null, Toggle(S.CaretGlide, v => Set(() => S.CaretGlide = v), "Glide between positions"))));

        page.Children.Add(Group("Split view"));
        page.Children.Add(Card(
            Row(Fl(0xe8b0), "Side pane palette", "The slot beside your note can look different",
                Dropdown([("same", "Same as main"), .. Palette.For(_p.Dark).Select(x => (x.Id, x.Name))], S.SplitPalette, v => Set(() => S.SplitPalette = v)))));
    }

    private void UiPage(StackPanel page)
    {
        page.Children.Add(Group("Window"));
        var materials = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        foreach (var (id, name) in new[] { ("acrylic", "Acrylic"), ("mica", "Mica"), ("solid", "Solid") })
            materials.Children.Add(Tile(MaterialPicture(id), name, S.Material == id, () => Set(() => S.Material = id, rows: true), 78));
        page.Children.Add(Card(
            Row(Fl(0xf592), "Background", MaterialHint(), materials),
            S.Material == "solid" ? null : Row(Fl(0xf595), "Tint", "How much of the desktop shows through",
                Dropdown([("0.55", "Clear"), ("0.7", "Light"), ("0.82", "Medium"), ("0.92", "Dense")], Closest(S.Tint, 0.55, 0.7, 0.82, 0.92), v => Set(() => S.Tint = Num(v)))),
            Row(ScaleIcon(), "Scale", "Toolbar, panels and text together",
                Dropdown([("0.9", "90%"), ("1", "100%"), ("1.15", "115%"), ("1.3", "130%"), ("1.5", "150%")], Closest(S.UiScale, 0.9, 1, 1.15, 1.3, 1.5), v => Set(() => S.UiScale = Num(v))))));

        page.Children.Add(Group("Controls"));
        page.Children.Add(Card([
            .. Expander("toolbar", ToolbarIcon(), "Toolbar and pager", null, S.Chrome == "fade" ? "Fade while typing" : "Always visible",
                SubRow("Show", Seg([("fade", "Fade while typing"), ("always", "Always visible")], S.Chrome, v => Set(() => S.Chrome = v, rows: true))),
                S.Chrome != "fade" ? null : SubRow("Fade after",
                    Dropdown([("800", "0.8 seconds"), ("1600", "1.6 seconds"), ("3000", "3 seconds"), ("6000", "6 seconds")], Closest(S.FadeAfterMs, 800, 1600, 3000, 6000), v => Set(() => S.FadeAfterMs = int.Parse(v))))),
            Row(null, "Bar style", S.Bars switch
                {
                    "overlay" => "Both bars slide in over the note when the pointer reaches the top or bottom edge",
                    "float" => "The top bar slides in at the top; the pager floats just below the window from the bottom edge",
                    _ => "A top bar above the note and the pager below it",
                },
                Seg([("docked", "Docked"), ("overlay", "Overlay"), ("float", "Float below")], S.Bars, v => Set(() => S.Bars = v, rows: true))),
            Row(null, "Edge fade", "Text fades into the top and bottom edges where there is more to scroll", Toggle(S.EdgeFade, v => Set(() => S.EdgeFade = v), "Edge fade")),
            Row(null, "Menu action row", "Cut, copy, paste and delete as buttons across the top", Toggle(S.MenuActionRow, v => Set(() => S.MenuActionRow = v), "Menu action row")),
            Row(null, "Colored menu icons", "Tint a few menu icons with your accent", Toggle(S.MenuColorIcons, v => Set(() => S.MenuColorIcons = v), "Colored menu icons")),
            Row(null, "Dim when pinned", "A pinned pad fades back while you work elsewhere", Toggle(S.DimWhenPinned, v => Set(() => S.DimWhenPinned = v), "Dim when pinned")),
        ]));
    }

    // Solid switches to software rendering (about 80 MB less memory), which needs a restart either way.
    private string MaterialHint()
    {
        var wantSoftware = S.Material == "solid";
        if (wantSoftware != Program.SoftwareRendering) return wantSoftware ? $"Restart {AppInfo.Name} to save about 80 MB" : $"Restart {AppInfo.Name} to get the glass back";
        return "Acrylic and Mica use the GPU (~125 MB)\nSolid uses about 80 MB less";
    }

    private void EditorPage(StackPanel page, bool withPreview)
    {
        if (withPreview) page.Children.Add(Preview(feel: false));
        page.Children.Add(Group("Text"));
        page.Children.Add(Card(
            Row(Fl(0xf7e4), "Font", null,
                Dropdown([("Cascadia Mono", "Cascadia Mono"), ("Cascadia Code", "Cascadia Code"), ("Consolas", "Consolas"), ("Segoe UI", "Segoe UI")], S.Font, v => Set(() => S.Font = v))),
            Row(Fl(0xf7e6), "Font size", null,
                Dropdown([("13", "13"), ("14", "14"), ("15", "15"), ("16", "16"), ("18", "18"), ("20", "20")], Closest(S.FontSize, 13, 14, 15, 16, 18, 20), v => Set(() => S.FontSize = Num(v)))),
            Row(null, "Text width", "Wide windows center the column",
                Dropdown([("0", "Full width"), ("60", "60 characters"), ("72", "72 characters"), ("88", "88 characters"), ("110", "110 characters")], Closest(S.TextWidth, 0, 60, 72, 88, 110), v => Set(() => S.TextWidth = int.Parse(v)))),
            Row(Fl(0xf7f6), "Line spacing", null,
                Seg([("1.15", "Tight"), ("1.32", "Normal"), ("1.5", "Relaxed"), ("1.7", "Airy")], Closest(S.LineSpacing, 1.15, 1.32, 1.5, 1.7), v => Set(() => S.LineSpacing = Num(v))))));

        page.Children.Add(Group("Page"));
        var backings = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var (id, name) in new[] { ("blank", "Blank"), ("lined", "Lined"), ("dotted", "Dotted"), ("grid", "Grid"), ("biggrid", "Large grid") })
            backings.Children.Add(Tile(new PatternSwatch(id, _p) { Height = 40 }, name, S.Paper == id, () => Set(() => S.Paper = id, rows: true), 60));
        page.Children.Add(Card([
            Row(Fl(0xf2f9), "Paper tone", null,
                Dropdown(Palette.Tones.Select(t => (t.Id, t.Id == "default" ? "From palette" : t.Name)).ToList(), S.Tone, v => Set(() => S.Tone = v))),
            .. Expander("backing", Fl(0xf462), "Backing", null, Label(S.Paper),
                SubRow(null, backings, wide: true),
                S.Paper == "blank" ? null : SubRow("Pattern weight",
                    Seg([("0", "Subtle"), ("1", "Clear"), ("2", "Bold")], S.PaperStrength.ToString(), v => Set(() => S.PaperStrength = int.Parse(v))))),
        ]));

        page.Children.Add(Group("Writing aids"));
        var enabled = Editor.CodeColors.Languages.Count(l => !S.DisabledLanguages.Contains(l.Id));
        page.Children.Add(Card([
            Row(Fl(0xf20a), "Answers", "Lines ending in = show their result", Toggle(S.InlineMath, v => Set(() => S.InlineMath = v), "Answers")),
            Row(null, "Answer position", null,
                Dropdown([("column", "One column"), ("edge", "Right edge")], S.Bubbles, v => Set(() => S.Bubbles = v))),
            Row(Fl(0xe7db), "Clean markdown", "Hide ** and # except on the line you're editing", Toggle(S.CleanMarkdown, v => Set(() => S.CleanMarkdown = v), "Clean markdown")),
            Row(IndentIcon(), "Indent guides", null, Toggle(S.IndentGuides, v => Set(() => S.IndentGuides = v), "Indent guides")),
            Row(Fl(0xecf4), "Folding", null, Toggle(S.Folding, v => Set(() => S.Folding = v), "Folding")),
            Row(Fl(0xf4e4), "Short links", "The full address stays in the text", Toggle(S.ShortenLinks, v => Set(() => S.ShortenLinks = v), "Short links")),
            .. Expander("code", Fl(0xf2ef), "Code languages", "Off: plain text, not offered after ```",
                enabled == Editor.CodeColors.Languages.Length ? $"All {enabled} on" : $"{enabled} of {Editor.CodeColors.Languages.Length} on",
                SubRow(null, Kit.Toggles(_p, Editor.CodeColors.Languages.Select(l => (l.Id, l.Name)).ToList(),
                    id => !S.DisabledLanguages.Contains(id), id => Set(() => { if (!S.DisabledLanguages.Remove(id)) S.DisabledLanguages.Add(id); }, rows: true)), wide: true)),
        ]));
    }

    private void FeelPage(StackPanel page, bool withPreview)
    {
        if (withPreview) page.Children.Add(Preview(feel: true));
        page.Children.Add(Group("Scrolling"));
        page.Children.Add(Card([
            .. Expander("scrolling", Fl(0xe077), "Scroll style", ScrollHint(S.Scrolling), ScrollName(S.Scrolling),
                [SubRow("Style", Seg([("lines", "Lines"), ("free", "Free"), ("rate", "Rate-matched"), ("wheel", "Wheel-driven")], S.Scrolling, v => Set(() => S.Scrolling = v, rows: true))),
                 .. TuningRows()]),
        ]));

        page.Children.Add(Group("Switching between notes"));
        page.Children.Add(Card([
            .. Expander("switching", Fl(0xf18d), "Switch style",
                S.Swipe == "progress" ? "Same feel at any window width" : "Follows with resistance, decides when you stop",
                S.Swipe == "progress" ? "Progress" : "Detent",
                SubRow("Style", Seg([("detent", "Detent"), ("progress", "Progress")], S.Swipe, v => Set(() => S.Swipe = v, rows: true))),
                S.Swipe != "progress" ? null : SubRow("Distance to switch",
                    Seg([("150", "Short"), ("220", "Normal"), ("320", "Long")], Closest(S.SwipeLength, 150, 220, 320), v => Set(() => S.SwipeLength = Num(v)))),
                S.Swipe != "progress" ? null : SubRow("Changes at",
                    Seg([("0.25", "A quarter"), ("0.35", "A third"), ("0.5", "Halfway")], Closest(S.SwipeFiresAt, 0.25, 0.35, 0.5), v => Set(() => S.SwipeFiresAt = Num(v))))),
        ]));

        page.Children.Add(Group("Motion"));
        page.Children.Add(Card(
            Row(Fl(0xf103), "Reduce motion", "No glides, coasts or slides", Toggle(S.ReduceMotion, v => Set(() => S.ReduceMotion = v), "Reduce motion"))));
    }

    /// <summary>Presets for the chosen scroll style, shown under it.</summary>
    private Border?[] TuningRows() => S.Scrolling switch
    {
        "lines" => [SubRow("Lines per notch", Seg([("2", "2"), ("3", "3"), ("4", "4"), ("6", "6")], Closest(S.LinesPerNotch, 2, 3, 4, 6), v => Set(() => S.LinesPerNotch = int.Parse(v))))],
        "free" =>
        [
            SubRow("Push per notch", Seg([("0.65", "Gentle"), ("1", "Normal"), ("1.5", "Strong")], Closest(S.ScrollDistance, 0.65, 1, 1.5), v => Set(() => S.ScrollDistance = Num(v)))),
            SubRow("Coast", Seg([("10", "Short"), ("6", "Normal"), ("3.5", "Long")], Closest(S.FreeFriction, 10, 6, 3.5), v => Set(() => S.FreeFriction = Num(v)))),
        ],
        _ =>
        [
            SubRow("Distance per click", Seg([("0.65", "Short"), ("1", "Normal"), ("1.5", "Long")], Closest(S.ScrollDistance, 0.65, 1, 1.5), v => Set(() => S.ScrollDistance = Num(v)))),
            SubRow("Speed boost", Seg([("0", "None"), ("0.5", "A little"), ("1", "Normal"), ("1.6", "A lot")], Closest(S.ScrollBoost, 0, 0.5, 1, 1.6), v => Set(() => S.ScrollBoost = Num(v)))),
        ],
    };

    private static string ScrollName(string mode) => mode switch { "free" => "Free", "rate" => "Rate-matched", "wheel" => "Wheel-driven", _ => "Lines" };

    private static string ScrollHint(string mode) => mode switch
    {
        "free" => "Each notch adds speed that coasts",
        "rate" => "Slow clicks glide a little; spin faster to go further",
        "wheel" => "Moves with the wheel, stops when it stops",
        _ => "Each notch glides a few lines",
    };

    private void SystemPage(StackPanel page)
    {
        page.Children.Add(Group("Shortcuts"));
        page.Children.Add(Card(
            Row(Fl(0xf4b8), $"Show or hide {AppInfo.Name}", null, HotkeyEditor(() => _pad.HotkeyStatus, text => _pad.RegisterHotkey(text) && Store(() => S.Hotkey = text), allowEmpty: false)),
            Row(Fl(0xf8bd), "Open on a new note", null, HotkeyEditor(() => _pad.NewNoteHotkeyStatus, text =>
            {
                var value = string.IsNullOrWhiteSpace(text) ? null : text;
                return _pad.RegisterNewNoteHotkey(value) && Store(() => S.NewNoteHotkey = value);
            }, allowEmpty: true))));

        page.Children.Add(Group("Opening"));
        var tiles = new UniformGrid { Columns = 5 };
        foreach (var (id, name) in new[] { ("remember", "Where I left it"), ("center", "Center"), ("mouse", "Mouse"), ("tray", "Above the tray"), ("dropdown", "Drop down") })
            tiles.Children.Add(Tile(new ScreenPicture(id, _p) { Height = 62 }, name, S.OpenAt == id, () => Set(() => S.OpenAt = id, rows: true), 104));
        page.Children.Add(Card([
            .. Expander("open-at", Fl(0xee77), "Open at", null, OpenAtName(S.OpenAt), SubRow(null, tiles, wide: true)),
            Row(Fl(0xf2dd), "Fresh note after", "Away this long, the hotkey opens a new page",
                Dropdown([("0", "Never"), ("5", "5 minutes"), ("15", "15 minutes"), ("30", "30 minutes"), ("120", "2 hours")], S.FreshNoteAfterMinutes.ToString(), v => Set(() => S.FreshNoteAfterMinutes = int.Parse(v)))),
        ]));

        page.Children.Add(Group("Window"));
        bool startup = false; try { startup = Startup.IsEnabled; } catch { }
        page.Children.Add(Card(
            Row(Fl(0xf601), "Keep on top", null, Toggle(S.Pinned, v => Set(() => S.Pinned = v), "Keep on top")),
            Row(Fl(0xf165), "Hide when I click away", "Not while kept on top", Toggle(S.HideOnBlur, v => Set(() => S.HideOnBlur = v), "Hide when I click away")),
            Row(Fl(0xf60e), "Start with Windows", null, Toggle(startup, v => { try { Startup.SetEnabled(v); } catch (Exception ex) { _pad.Toast(ex.Message); } }, "Start with Windows"))));
    }

    private static string OpenAtName(string id) => id switch
    {
        "center" => "Center", "mouse" => "Mouse", "tray" => "Above the tray", "dropdown" => "Drop down from the top", _ => "Where I left it",
    };

    private bool Store(System.Action change) { change(); _pad.SettingsChanged(); return true; }

    /// <summary>The shortcut as key caps with a Change button; Change swaps in a box (Enter applies, Esc cancels).</summary>
    private Control HotkeyEditor(Func<string> current, Func<string, bool> apply, bool allowEmpty)
    {
        var host = new ContentControl();
        void ShowCaps()
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
            var label = current();
            if (label is "off" or "") row.Children.Add(Kit.Text("None", _p.Muted, 13));
            else
                foreach (var key in label.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                    row.Children.Add(new Border
                    {
                        BorderBrush = Line, BorderThickness = new Thickness(1, 1, 1, 2), CornerRadius = new CornerRadius(5), Padding = new Thickness(8, 2),
                        Background = new SolidColorBrush(_p.Surface), Child = Kit.Text(key, _p.Ink, 12),
                    });
            var change = TextButton("Change", ShowBox);
            change.Margin = new Thickness(8, 0, 0, 0);
            row.Children.Add(change);
            host.Content = row;
        }
        void ShowBox()
        {
            var label = current();
            var box = new TextBox
            {
                Text = label is "off" ? "" : label, Width = 200, FontSize = 13, PlaceholderText = allowEmpty ? "Empty for none" : "e.g. Ctrl+Alt+Shift+S",
                Foreground = new SolidColorBrush(_p.Ink), CaretBrush = new SolidColorBrush(_p.Caret), CornerRadius = new CornerRadius(7),
            };
            var status = Kit.Text("Enter to apply, Esc to cancel", _p.Muted, 11.5);
            box.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Escape) { e.Handled = true; ShowCaps(); }
                if (e.Key != Key.Enter) return;
                e.Handled = true;
                var text = box.Text?.Trim() ?? "";
                if (text.Length == 0 && !allowEmpty) return;
                if (apply(text)) ShowCaps();
                else status.Text = $"Windows won't give {AppInfo.Name} {text}; another app may use it.";
            };
            host.Content = new StackPanel { Spacing = 4, Children = { box, status } };
            Dispatcher.UIThread.Post(() => { box.Focus(); box.SelectAll(); });
        }
        ShowCaps();
        return host;
    }

    private void AboutPage(StackPanel page)
    {
        page.Children.Add(Group(AppInfo.Name));
        page.Children.Add(Card(
            Row(Fl(0xf4a3), $"{AppInfo.Name} {AppInfo.Version} ({AppInfo.Label})", $"{AppInfo.Description}\n{AppInfo.Copyright}", null),
            Row(Fl(0xf418), "Notes folder", Program.DataFolder + "\nSaved as you type, one file per note", null),
            Row(Fl(0xf4b8), "Keyboard shortcuts", "Every shortcut on one page", TextButton("Show", () => { _pad.ShowResident(); _pad.OpenHelp(); })),
            Row(Fl(0xf19f), "Restore default settings", "Undo it from the pad's notice", TextButton("Restore", _pad.ResetSettings))));
    }

    /// <summary>The one preview, moved to whichever page shows it, so its trace and scroll survive tab changes.</summary>
    private Control Preview(bool feel)
    {
        if (_preview.Parent is Panel old) old.Children.Remove(_preview);
        _preview.UseSamples(feel);
        return _preview;
    }

    private static string Label(string paper) => paper switch { "lined" => "Lined", "dotted" => "Dotted", "grid" => "Grid", "biggrid" => "Large grid", _ => "Blank" };

    private static double Num(string v) => double.Parse(v, System.Globalization.CultureInfo.InvariantCulture);

    private static string Closest(double value, params double[] options) =>
        options.MinBy(o => Math.Abs(o - value)).ToString(System.Globalization.CultureInfo.InvariantCulture);
}
