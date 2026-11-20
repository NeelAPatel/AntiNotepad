// Builds the grouped Settings pages and their live controls.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Platform.Storage;
using AntiNotepad;
using AntiNotepad.Model;
using AntiNotepad.Editor;
using AntiNotepad.Motion;
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
    private const int KeyboardGlyph = 0xf4b8;

    private static readonly (string Id, string Name, int Icon)[] Sections =
    [
        // Fluent UI System Icons: color, paint_brush, document_edit, tap_single, desktop, keyboard, folder, info.
        ("theme", "Theme", 0xf2f5), ("ui", "UI", 0xf592), ("editor", "Editor", 0xf386),
        ("feel", "Feel", 0xec8d), ("system", "System", 0xf359), ("shortcuts", "Shortcuts", KeyboardGlyph),
        ("storage", "Storage", 0xf418), ("about", "About", 0xf4a3),
    ];
    // Remembered while AntiNotepad runs: the last section, and which expanders are open (these start open).
    private static string _section = "theme";
    private static readonly HashSet<string> Expanded = ["light-palette", "backing", "toolbar", "scrolling", "switching", "open-at"];

    private readonly SettingsService _settings;
    private readonly ShortcutMap _shortcuts;
    private readonly ISettingsHost _host;
    private readonly bool _softwareRendering;
    private readonly NotesSession _notes;
    private Palette _p;
    private readonly MotionPreview _preview;
    private readonly SmoothScroll _smoothScroll;
    private readonly ScrollViewer _scroll = new() { HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
    private readonly StackPanel _rail = new() { Spacing = 2 };
    private TextBox _find = null!;
    private Border _tint = null!;
    private string? _appliedMaterial;
    private string? _pendingNotesFolder;
    private string? _notesError;
    private string? _notesSuccess;
    private string? _notesNotice;
    private bool _notesFolderRestartPending;
    private Palette? _stylesPalette;
    private string? _searchQuery;
    private readonly Dictionary<string, SegmentPosition> _segmentPositions = new();
    private Settings S => _settings.Current;

    public SettingsWindow(SettingsService settings, ISettingsHost host, bool softwareRendering, NotesSession notes)
    {
        _settings = settings;
        _shortcuts = new ShortcutMap(settings.Current);
        _smoothScroll = new SmoothScroll(_scroll, Typography.PanelTitle, () => S);
        _host = host;
        _softwareRendering = softwareRendering;
        _notes = notes;
        _p = host.CurrentPalette;
        Title = $"{AppInfo.DisplayName} Settings";
        Icon = host.Icon;
        Width = 900; Height = 720; MinWidth = 640; MinHeight = 460;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowDecorations = WindowDecorations.BorderOnly;
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = 42;
        Background = Brushes.Transparent;
        FontFamily = new FontFamily(Typography.UiFamily);
        TextOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        _preview = new MotionPreview(_p, S, _notes);
        ApplyLook();
        Build();
        Show(_section, animate: false);
        AddHandler(KeyDownEvent, (_, e) =>
        {
            if (e.Key != Key.Escape) return;
            if (HotkeyRecorder.IsRecordingSource(e.Source)) return;
            if (e.Source is TextBox box && !ReferenceEquals(box, _find)) return;
            e.Handled = true;
            if (_find.Text is { Length: > 0 }) _find.Text = "";
            else Close();
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        Deactivated += (_, _) => _host.SettingsLostFocus();
        Closed += (_, _) => { _preview.Release(); _host.SettingsLostFocus(); };
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
        if (Equals(_p, _host.CurrentPalette)) return;
        _p = _host.CurrentPalette;
        Restyle();
    }

    /// <summary>Dev tooling: open a given section without a click.</summary>
    internal void ShowSection(string id) => Show(id, animate: false);

    /// <summary>Dev tooling: click into the preview (starts its caret blink, the path that once leaked the window).</summary>
    internal void FocusPreview() => _preview.FocusEditor();

    /// <summary>Settings were replaced wholesale (restore defaults, undo): rebuild what's on screen.</summary>
    public void Refresh()
    {
        _p = _host.CurrentPalette;
        _shortcuts.Refresh(S);
        Restyle();
    }

    private void Restyle()
    {
        _shortcuts.Refresh(S);
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
        _settings.Update(_ => change());
        AfterSet(rows);
    }

    private void SetLanguageEnabled(string id)
    {
        _settings.SetLanguageEnabled(id, !S.DisabledLanguages.Contains(id));
        AfterSet(rows: true);
    }

    private void AfterSet(bool rows)
    {
        if (!Equals(_p, _host.CurrentPalette)) { _p = _host.CurrentPalette; Restyle(); return; }
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
        var title = Kit.Text($"{AppInfo.Name} Settings", _p.Muted, Typography.Small);
        title.Margin = new Thickness(18, 0, 0, 0);
        title.IsHitTestVisible = false;
        top.Children.Add(title);
        var close = Kit.IconButton(Glyph.Close, "Close (Esc)", _p, Close, 32);
        close.Margin = new Thickness(0, 0, 8, 0);
        Grid.SetColumn(close, 1);
        top.Children.Add(close);
        root.Children.Add(top);

        var body = new Grid { ColumnDefinitions = new ColumnDefinitions("214,*") };
        Grid.SetRow(body, 1);
        var railHost = new StackPanel { Margin = new Thickness(10, 4, 10, 10), Spacing = 12 };
        _find = new TextBox
        {
            PlaceholderText = "Find a setting",
            FontSize = Typography.Body,
            Height = 32,
            CornerRadius = new CornerRadius(Shape.Control),
            Padding = new Thickness(8, 8, 8, 0),
            Foreground = new SolidColorBrush(_p.Ink),
            CaretBrush = new SolidColorBrush(_p.Caret),
            InnerLeftContent = new GlyphIcon(char.ConvertFromUtf32(0xf68f), _p.Muted, Typography.IconInput, 0, Glyph.Fluent) { Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center },
        };
        Avalonia.Automation.AutomationProperties.SetName(_find, "Find a setting");
        // Results build matching rows, so they wait for a pause in typing; clearing the box is instant.
        var findPause = new Avalonia.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        findPause.Tick += (_, _) => { findPause.Stop(); if (!string.IsNullOrWhiteSpace(_find.Text)) ShowResults(_find.Text); };
        _find.TextChanged += (_, _) =>
        {
            findPause.Stop();
            if (string.IsNullOrWhiteSpace(_find.Text)) Show(_section, animate: false);
            else findPause.Start();
        };
        Closed += (_, _) => findPause.Stop();
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
            face.Children.Add(new GlyphIcon(char.ConvertFromUtf32(glyph), on ? IconInk : _p.Muted, Typography.IconLarge, 0, Glyph.Fluent) { VerticalAlignment = VerticalAlignment.Center });
            face.Children.Add(Kit.Text(name, on ? _p.Ink : _p.Muted, Typography.Large, on ? Typography.Strong : Typography.Regular));
            var item = Kit.Plain(face, _p, height: 36, radius: Shape.Popup);
            item.HorizontalAlignment = HorizontalAlignment.Stretch;
            if (on && item.Content is Border b)
            {
                var fill = new SolidColorBrush(_p.Accent, Paint.Selected(_p.Dark));
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
        _smoothScroll.Stop();
        _scroll.Content = page;
        if (keepScroll) Dispatcher.UIThread.Post(() => _scroll.Offset = offset, DispatcherPriority.Loaded);
        else _scroll.Offset = default;
        if (!animate || Anim.Reduced) return;
        // Tab change: the page rises a few pixels as it fades in.
        var shift = new TranslateTransform(0, 10);
        page.RenderTransform = shift;
        page.Opacity = 0;
        Anim.Run(page, MotionTokens.SettingsPage, t => { var e = Anim.EaseOut(t); page.Opacity = e; shift.Y = 10 * (1 - e); }, () => { page.Opacity = 1; page.RenderTransform = null; });
    }

    private StackPanel BuildSection(string section, bool withPreview, string? query = null)
    {
        _searchQuery = query;
        var page = NewPage();
        page.Children.Add(Heading(Sections.First(s => s.Id == section).Name));
        switch (section)
        {
            case "theme": ThemePage(page); break;
            case "ui": UiPage(page); break;
            case "editor": EditorPage(page, withPreview); break;
            case "feel": FeelPage(page, withPreview); break;
            case "system": SystemPage(page); break;
            case "shortcuts": ShortcutsPage(page); break;
            case "storage": StoragePage(page); break;
            default: AboutPage(page); break;
        }
        return page;
    }

    /// <summary>Search: every row whose title or description matches, gathered under its section's name, still live.</summary>
    private void ShowResults(string query)
    {
        BuildRail(null);
        _smoothScroll.Stop();
        _scroll.Content = BuildSearchResults(query);
        _scroll.Offset = default;
    }

    private StackPanel BuildSearchResults(string query)
    {
        query = query.Trim();
        var page = NewPage();
        page.Children.Add(Heading("Results"));
        var any = false;
        foreach (var (id, name, _) in Sections)
        {
            var built = BuildSection(id, withPreview: false, query);
            var matches = built.Children.OfType<Border>().Where(c => Equals(c.Tag, "card"))
                .SelectMany(c => ((StackPanel)c.Child!).Children.OfType<Border>())
                .Where(r => r.Tag is string tag && SearchMatches(tag, query))
                .ToList();
            if (matches.Count == 0) continue;
            any = true;
            // Nested rows come along even when their expander is closed; their indent lines them up with the titles.
            foreach (var row in matches) { ((Panel)row.Parent!).Children.Remove(row); row.IsVisible = true; }
            page.Children.Add(Group(name));
            page.Children.Add(Card([.. matches]));
        }
        if (!any) page.Children.Add(Kit.Text($"No setting matches “{query}”.", _p.Muted, Typography.Body));
        return page;
    }

    private void ThemePage(StackPanel page)
    {
        page.Children.Add(Group("Colors"));
        page.Children.Add(Card([
            Row(() => ModeIcon(), "Mode", "Follow Windows, or stay light or dark",
                () => Seg([("system", "System"), ("light", "Light"), ("dark", "Dark")], S.Theme, v => Set(() => S.Theme = v))),
            .. Expander("light-palette", () => Fl(0xf8a1), "Light mode palette", null, Palette.Get(S.LightPalette, false).Name,
                () => SubRow(null, () => PaletteChips(false, S.LightPalette, v => Set(() => { S.LightPalette = v; if (S.Theme == "dark") S.Theme = "light"; })), wide: true)),
            .. Expander("dark-palette", () => Fl(0xf87d), "Dark mode palette", null, Palette.Get(S.DarkPalette, true).Name,
                () => SubRow(null, () => PaletteChips(true, S.DarkPalette, v => Set(() => { S.DarkPalette = v; if (S.Theme == "light") S.Theme = "dark"; })), wide: true)),
            Row(() => Fl(0xf2f5), "Accent", "Caret, selection, headings and switches", () => AccentDots()),
        ]));

        page.Children.Add(Group("Caret"));
        Control CaretStyles()
        {
            var cells = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
            cells.Children.Add(Tile(CaretPicture(false), "Bar", S.CaretStyle != "block", () => Set(() => S.CaretStyle = "bar", rows: true), 56));
            cells.Children.Add(Tile(CaretPicture(true), "Block", S.CaretStyle == "block", () => Set(() => S.CaretStyle = "block", rows: true), 56));
            return cells;
        }
        page.Children.Add(Card(
            Row(() => CursorIcon("Ab", clock: false), "Caret style", null, CaretStyles),
            Row(() => CursorIcon("A", clock: true), "Blink", null, () => Toggle(S.CaretBlink, v => Set(() => S.CaretBlink = v), "Blink")),
            Row(() => Fl(0xe0cd), "Glide between positions", null, () => Toggle(S.CaretGlide, v => Set(() => S.CaretGlide = v), "Glide between positions"))));

        page.Children.Add(Group("Split view"));
        page.Children.Add(Card(
            Row(() => Fl(0xe8b0), "Side pane palette", "The slot beside your note can look different",
                () => Dropdown([("same", "Same as main"), .. Palette.For(_p.Dark).Select(x => (x.Id, x.Name))], S.SplitPalette, v => Set(() => S.SplitPalette = v)))));
    }

    private void UiPage(StackPanel page)
    {
        page.Children.Add(Group("Window"));
        Control Materials()
        {
            var materials = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
            foreach (var (id, name) in new[] { ("acrylic", "Acrylic"), ("mica", "Mica"), ("solid", "Solid") })
                materials.Children.Add(Tile(MaterialPicture(id), name, S.Material == id, () => Set(() => S.Material = id, rows: true), 80));
            return materials;
        }
        page.Children.Add(Card(
            Row(() => Fl(0xf592), "Background", MaterialHint(), Materials),
            S.Material == "solid" ? null : Row(() => Fl(0xf595), "Tint", "How much of the desktop shows through",
                () => Dropdown([("0.55", "Clear"), ("0.7", "Light"), ("0.82", "Medium"), ("0.92", "Dense")], Closest(S.Tint, 0.55, 0.7, 0.82, 0.92), v => Set(() => S.Tint = Num(v)))),
            Row(() => ScaleIcon(), "Scale", "Toolbar, panels and text together",
                () => Dropdown([("0.9", "90%"), ("1", "100%"), ("1.15", "115%"), ("1.3", "130%"), ("1.5", "150%")], Closest(S.UiScale, 0.9, 1, 1.15, 1.3, 1.5), v => Set(() => S.UiScale = Num(v))))));

        page.Children.Add(Group("Controls"));
        page.Children.Add(Card([
            .. Expander("toolbar", () => ToolbarIcon(), "Toolbar and pager", null, S.Chrome == "fade" ? "Fade while typing" : "Always visible",
                () => SubRow("Show", () => Seg([("fade", "Fade while typing"), ("always", "Always visible")], S.Chrome, v => Set(() => S.Chrome = v, rows: true))),
                S.Chrome != "fade" ? null : () => SubRow("Fade after",
                    () => Dropdown([("800", "0.8 seconds"), ("1600", "1.6 seconds"), ("3000", "3 seconds"), ("6000", "6 seconds")], Closest(S.FadeAfterMs, 800, 1600, 3000, 6000), v => Set(() => S.FadeAfterMs = int.Parse(v))))),
            Row(null, "Bar style", S.Bars switch
                {
                    "overlay" => "Both bars slide in over the note when the pointer reaches the top or bottom edge",
                    "float" => "The top bar slides in at the top; the pager floats just below the window from the bottom edge",
                    _ => "A top bar above the note and the pager below it",
                },
                () => Seg([("docked", "Docked"), ("overlay", "Overlay"), ("float", "Float below")], S.Bars, v => Set(() => S.Bars = v, rows: true))),
            Row(null, "Show Delete button", "Show the Delete button on the bottom bar", () => Toggle(S.ShowDeleteButton, v => Set(() => S.ShowDeleteButton = v), "Show Delete button")),
            Row(null, "Show New note button", "Show the New note button on the bottom bar", () => Toggle(S.ShowNewNoteButton, v => Set(() => S.ShowNewNoteButton = v), "Show New note button")),
            Row(null, "Slot dots", "Choose the appearance of the slot pager", () => Seg([("pill", "Pill"), ("quiet", "Quiet")], S.SlotDots, v => Set(() => S.SlotDots = v))),
            Row(null, "Edge fade", "Text fades into the top and bottom edges where there is more to scroll", () => Toggle(S.EdgeFade, v => Set(() => S.EdgeFade = v), "Edge fade")),
            Row(null, "Menu action row", "Cut, copy, paste and delete as buttons across the top", () => Toggle(S.MenuActionRow, v => Set(() => S.MenuActionRow = v), "Menu action row")),
            Row(null, "Colored menu icons", "Tint a few menu icons with your accent", () => Toggle(S.MenuColorIcons, v => Set(() => S.MenuColorIcons = v), "Colored menu icons")),
            Row(null, "Dim when pinned", "A pinned pad fades back while you work elsewhere", () => Toggle(S.DimWhenPinned, v => Set(() => S.DimWhenPinned = v), "Dim when pinned")),
        ]));
    }

    // Solid switches to software rendering (about 80 MB less memory), which needs a restart either way.
    private string MaterialHint()
    {
        var wantSoftware = S.Material == "solid";
        if (wantSoftware != _softwareRendering) return wantSoftware ? $"Restart {AppInfo.Name} to save about 80 MB" : $"Restart {AppInfo.Name} to get the glass back";
        return "Acrylic and Mica use the GPU (~125 MB)\nSolid uses about 80 MB less";
    }

    private void EditorPage(StackPanel page, bool withPreview)
    {
        if (withPreview) page.Children.Add(Preview(feel: false));
        page.Children.Add(Group("Text"));
        page.Children.Add(Card(
            Row(() => Fl(0xf7e4), "Font", null,
                () => Dropdown([("Cascadia Mono", "Cascadia Mono"), ("Cascadia Code", "Cascadia Code"), ("Consolas", "Consolas"), ("Segoe UI", "Segoe UI")], S.Font, v => Set(() => S.Font = v))),
            Row(() => Fl(0xf7e6), "Font size", null,
                () => Dropdown([("13", "13"), ("14", "14"), ("15", "15"), ("16", "16"), ("18", "18"), ("20", "20")], Closest(S.FontSize, 13, 14, 15, 16, 18, 20), v => Set(() => S.FontSize = Num(v)))),
            Row(null, "Text width", "Wide windows center the column",
                () => Dropdown([("0", "Full width"), ("60", "60 characters"), ("72", "72 characters"), ("88", "88 characters"), ("110", "110 characters")], Closest(S.TextWidth, 0, 60, 72, 88, 110), v => Set(() => S.TextWidth = int.Parse(v)))),
            Row(() => Fl(0xf7f6), "Line spacing", null,
                () => Seg([("1.15", "Tight"), ("1.32", "Normal"), ("1.5", "Relaxed"), ("1.7", "Airy")], Closest(S.LineSpacing, 1.15, 1.32, 1.5, 1.7), v => Set(() => S.LineSpacing = Num(v))))));

        page.Children.Add(Group("Page"));
        Control Backings()
        {
            var backings = new WrapPanel { Orientation = Orientation.Horizontal };
            foreach (var (id, name) in new[] { ("blank", "Blank"), ("lined", "Lined"), ("dotted", "Dotted"), ("grid", "Grid"), ("biggrid", "Large grid") })
                backings.Children.Add(Tile(new PatternSwatch(id, _p) { Height = 40 }, name, S.Paper == id, () => Set(() => S.Paper = id, rows: true), 60));
            return backings;
        }
        page.Children.Add(Card([
            Row(() => Fl(0xf2f9), "Paper tone", null,
                () => Dropdown(Palette.Tones.Select(t => (t.Id, t.Id == "default" ? "From palette" : t.Name)).ToList(), S.Tone, v => Set(() => S.Tone = v))),
            .. Expander("backing", () => Fl(0xf462), "Backing", null, Label(S.Paper),
                () => SubRow(null, Backings, wide: true),
                S.Paper == "blank" ? null : () => SubRow("Pattern weight",
                    () => Seg([("0", "Subtle"), ("1", "Clear"), ("2", "Bold")], S.PaperStrength.ToString(), v => Set(() => S.PaperStrength = int.Parse(v))))),
        ]));

        page.Children.Add(Group("Writing aids"));
        var enabled = Editor.CodeColors.Languages.Count(l => !S.DisabledLanguages.Contains(l.Id));
        page.Children.Add(Card([
            Row(null, "New list lines", "What the list keyword makes",
                () => Seg([("bullet", "Bullets"), ("check", "Checkboxes"), ("number", "Numbers")], S.ListDefault, v => Set(() => S.ListDefault = v))),
            Row(() => Fl(0xf20a), "Answers", "Lines ending in = show their result", () => Toggle(S.InlineMath, v => Set(() => S.InlineMath = v), "Answers")),
            Row(null, "Answer position", null,
                () => Dropdown([("column", "One column"), ("edge", "Right edge")], S.Bubbles, v => Set(() => S.Bubbles = v))),
            Row(() => Fl(0xe7db), "Clean markdown", "Hide ** and # except on the line you're editing", () => Toggle(S.CleanMarkdown, v => Set(() => S.CleanMarkdown = v), "Clean markdown")),
            Row(() => IndentIcon(), "Indent guides", null, () => Toggle(S.IndentGuides, v => Set(() => S.IndentGuides = v), "Indent guides")),
            Row(() => Fl(0xecf4), "Folding", null, () => Toggle(S.Folding, v => Set(() => S.Folding = v), "Folding")),
            Row(() => Fl(0xf4e4), "Short links", "The full address stays in the text", () => Toggle(S.ShortenLinks, v => Set(() => S.ShortenLinks = v), "Short links")),
            .. Expander("code", () => Fl(0xf2ef), "Code languages", "Off: plain text, not offered after ```",
                enabled == Editor.CodeColors.Languages.Length ? $"All {enabled} on" : $"{enabled} of {Editor.CodeColors.Languages.Length} on",
                () => SubRow(null, () => Kit.Toggles(_p, Editor.CodeColors.Languages.Select(l => (l.Id, l.Name)).ToList(),
                    id => !S.DisabledLanguages.Contains(id), SetLanguageEnabled), wide: true)),
        ]));
    }

    private void FeelPage(StackPanel page, bool withPreview)
    {
        if (withPreview) page.Children.Add(Preview(feel: true));
        page.Children.Add(Group("Scrolling"));
        page.Children.Add(Card([
            .. Expander("scrolling", () => Fl(0xe077), "Vertical Scroll Style", ScrollHint(S.Scrolling), ScrollName(S.Scrolling),
                [() => SubRow("Style", () => Seg([("lines", "Lines"), ("free", "Free"), ("rate", "Rate-matched"), ("wheel", "Wheel-driven")], S.Scrolling, v => Set(() => S.Scrolling = v, rows: true))),
                 .. TuningRows()]),
        ]));

        page.Children.Add(Group("Switching between notes"));
        page.Children.Add(Card([
            .. Expander("switching", () => Fl(0xf18d), "Horizontal Scroll Style",
                "Follows input, with projection-based switching and spring edge pulls",
                "Follow-through",
                () => SubRow("Style", () => Seg([("follow", "Follow-through")], S.HorizontalScrollStyle,
                    v => Set(() => S.Swipe = v, rows: true)))),
        ]));

        page.Children.Add(Group("Motion"));
        page.Children.Add(Card(
            Row(() => Fl(0xf103), "Reduce motion", "No glides, coasts, slides or horizontal overscroll", () => Toggle(S.ReduceMotion, v => Set(() => S.ReduceMotion = v), "Reduce motion"))));
    }

    /// <summary>Presets for the chosen scroll style, shown under it.</summary>
    private Func<Border?>[] TuningRows() => S.Scrolling switch
    {
        "lines" => [() => SubRow("Lines per notch", () => Seg([("2", "2"), ("3", "3"), ("4", "4"), ("6", "6")], Closest(S.LinesPerNotch, 2, 3, 4, 6), v => Set(() => S.LinesPerNotch = int.Parse(v))))],
        "free" =>
        [
            () => SubRow("Push per notch", () => Seg([("0.65", "Gentle"), ("1", "Normal"), ("1.5", "Strong")], Closest(S.ScrollDistance, 0.65, 1, 1.5), v => Set(() => S.ScrollDistance = Num(v)))),
            () => SubRow("Coast", () => Seg([("10", "Short"), ("6", "Normal"), ("3.5", "Long")], Closest(S.FreeFriction, 10, 6, 3.5), v => Set(() => S.FreeFriction = Num(v)))),
        ],
        _ =>
        [
            () => SubRow("Distance per click", () => Seg([("0.65", "Short"), ("1", "Normal"), ("1.5", "Long")], Closest(S.ScrollDistance, 0.65, 1, 1.5), v => Set(() => S.ScrollDistance = Num(v)))),
            () => SubRow("Speed boost", () => Seg([("0", "None"), ("0.5", "A little"), ("1", "Normal"), ("1.6", "A lot")], Closest(S.ScrollBoost, 0, 0.5, 1, 1.6), v => Set(() => S.ScrollBoost = Num(v)))),
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

    /// <summary>Builds the page for viewing and changing in-window shortcuts.</summary>
    private void ShortcutsPage(StackPanel page)
    {
        page.Children.Add(Group("Shortcut map"));
        page.Children.Add(Card(Row(() => Fl(KeyboardGlyph), "Reset all shortcuts", "Restore every action to its default key.",
            () => TextButton("Reset all", ResetAllShortcuts))));
        foreach (var section in new[] { "Notes", "Search", "Window", "Slots", "Writing" })
        {
            var definitions = ShortcutMap.Definitions.Where(definition => definition.Section == section).ToArray();
            page.Children.Add(Group(section));
            var rows = definitions.Select(definition => Row(() => Fl(KeyboardGlyph), definition.Name,
                definition.DefaultKey.Length == 0 ? $"No default shortcut. {definition.Help}" : definition.Help,
                () => ShortcutControl(definition))).ToArray();
            page.Children.Add(Card(rows));
        }
    }

    /// <summary>Builds the recorder and reset control for one shortcut action.</summary>
    private Control ShortcutControl(ShortcutDefinition definition)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        row.Children.Add(new HotkeyRecorder(_p,
            () => _shortcuts.GetRecordedValue(definition.Id),
            text => ApplyShortcut(definition, text),
            allowEmpty: true,
            TextButton,
            ShortcutMap.RecorderGestureText,
            text => ShortcutMap.BlockedReason(text, S, definition)).View);
        row.Children.Add(TextButton("Reset", () => ResetShortcut(definition.Id)));
        return row;
    }

    /// <summary>Validates a recorded key and resolves any same-scope ownership conflict.</summary>
    private bool ApplyShortcut(ShortcutDefinition definition, string text)
    {
        if (text.Length == 0)
        {
            SaveShortcut(definition.Id, "");
            return true;
        }
        if (ShortcutMap.BlockedReason(text, S, definition) is not null) return false;
        if (_shortcuts.FindConflict(definition.Id, text) is { } other)
        {
            ConfirmShortcutTake(definition, text, other);
            return true;
        }
        SaveShortcut(definition.Id, text);
        return true;
    }

    /// <summary>Saves one shortcut change and optionally clears the previous owner.</summary>
    private void SaveShortcut(string id, string text, string? clearId = null)
    {
        _settings.Update(settings =>
        {
            if (clearId is not null)
            {
                var cleared = ShortcutMap.Definitions.First(item => item.Id == clearId);
                if (cleared.DefaultKey.Length == 0) settings.ShortcutOverrides.Remove(clearId);
                else settings.ShortcutOverrides[clearId] = "";
            }
            var definition = ShortcutMap.Definitions.First(item => item.Id == id);
            if (text == definition.DefaultKey) settings.ShortcutOverrides.Remove(id);
            else settings.ShortcutOverrides[id] = text;
        });
        _shortcuts.Refresh(S);
        AfterSet(rows: true);
    }

    /// <summary>Removes a saved override so one action returns to its default.</summary>
    private void ResetShortcut(string id)
    {
        _settings.Update(settings => settings.ShortcutOverrides.Remove(id));
        _shortcuts.Refresh(S);
        AfterSet(rows: true);
    }

    /// <summary>Removes every saved override so all actions return to their defaults.</summary>
    private void ResetAllShortcuts()
    {
        _settings.Update(settings => settings.ShortcutOverrides.Clear());
        _shortcuts.Refresh(S);
        AfterSet(rows: true);
    }

    /// <summary>Asks before moving a key from its current action to another action.</summary>
    private async void ConfirmShortcutTake(ShortcutDefinition definition, string text, ShortcutDefinition owner)
    {
        var dialog = new Window
        {
            Title = "Shortcut already in use",
            Width = 420,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = new SolidColorBrush(_p.Canvas),
            Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = $"{text} is assigned to {owner.Name}. Give it to {definition.Name}? The other action will have no shortcut.",
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = new SolidColorBrush(_p.Ink),
                    },
                },
            },
        };
        var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8 };
        actions.Children.Add(TextButton("Cancel", () => dialog.Close(false)));
        actions.Children.Add(TextButton("Take shortcut", () => dialog.Close(true), primary: true));
        ((StackPanel)dialog.Content!).Children.Add(actions);
        if (await dialog.ShowDialog<bool>(this)) SaveShortcut(definition.Id, text, owner.Id);
    }

    private void SystemPage(StackPanel page)
    {
        page.Children.Add(Group("Shortcuts"));
        page.Children.Add(Card(
            Row(() => Fl(0xf4b8), $"Show or hide {AppInfo.Name}", null, () => HotkeyControl(() => _host.HotkeyStatus, text => _host.RegisterHotkey(text) && Store(() => S.Hotkey = text), allowEmpty: false)),
            Row(() => Fl(0xf8bd), "Open on a new note", null, () => HotkeyControl(() => _host.NewNoteHotkeyStatus, text =>
            {
                var value = string.IsNullOrWhiteSpace(text) ? null : text;
                return _host.RegisterNewNoteHotkey(value) && Store(() => S.NewNoteHotkey = value);
            }, allowEmpty: true))));

        page.Children.Add(Group("Opening"));
        Control OpenAtTiles()
        {
            var tiles = new UniformGrid { Columns = 5 };
            foreach (var (id, name) in new[] { ("remember", "Where I left it"), ("center", "Center"), ("mouse", "Mouse"), ("tray", "Above the tray"), ("dropdown", "Drop down") })
                tiles.Children.Add(Tile(new ScreenPicture(id, _p) { Height = 64 }, name, S.OpenAt == id, () => Set(() => S.OpenAt = id, rows: true), 104));
            return tiles;
        }
        page.Children.Add(Card([
            .. Expander("open-at", () => Fl(0xee77), "Open at", null, OpenAtName(S.OpenAt), () => SubRow(null, OpenAtTiles, wide: true)),
            Row(() => Fl(0xf359), "Taskbar icon", "The tray icon stays available while the pad is hidden",
                () => Seg([("visible", "While visible"), ("always", "Always"), ("never", "Never")], S.Taskbar, v => Set(() => S.Taskbar = v))),
            Row(() => Fl(0xf2dd), "Fresh note after", "Away this long, the hotkey opens a new page",
                () => Dropdown([("0", "Never"), ("5", "5 minutes"), ("15", "15 minutes"), ("30", "30 minutes"), ("120", "2 hours")], S.FreshNoteAfterMinutes.ToString(), v => Set(() => S.FreshNoteAfterMinutes = int.Parse(v)))),
        ]));

        page.Children.Add(Group("Window"));
        page.Children.Add(Card(
            Row(() => Fl(0xf601), "Keep on top", null, () => Toggle(S.Pinned, v => Set(() => S.Pinned = v), "Keep on top")),
            Row(() => Fl(0xf165), "Hide when I click away", "Not while kept on top", () => Toggle(S.HideOnBlur, v => Set(() => S.HideOnBlur = v), "Hide when I click away")),
            Row(() => Fl(0xf60e), "Start with Windows", null, () =>
            {
                bool startup = false; try { startup = Startup.IsEnabled; } catch { }
                return Toggle(startup, v => { try { Startup.SetEnabled(v); } catch (Exception ex) { _host.Toast(ex.Message); } }, "Start with Windows");
            })));

        page.Children.Add(Group("Saving"));
        page.Children.Add(Card(Row(null, "Save after I stop typing",
            "Your note is also saved when you hide, switch notes, or quit.",
            () => Dropdown(
                Settings.SaveDelayChoicesMs.Select(ms => (ms.ToString(), (ms / 1000.0).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + " s")).ToArray(),
                S.SaveDelayMs.ToString(), v => Set(() => S.SaveDelayMs = int.Parse(v))))));
    }

    private static string OpenAtName(string id) => id switch
    {
        "center" => "Center", "mouse" => "Mouse", "tray" => "Above the tray", "dropdown" => "Drop down from the top", _ => "Where I left it",
    };

    private bool Store(System.Action change) { _settings.Update(_ => change()); return true; }

    /// <summary>Builds a key-press recorder with the Settings button style.</summary>
    private Control HotkeyControl(Func<string> current, Func<string, bool> apply, bool allowEmpty) =>
        new HotkeyRecorder(_p, current, apply, allowEmpty, TextButton).View;

    private void AboutPage(StackPanel page)
    {
        page.Children.Add(Group(AppInfo.Name));
        page.Children.Add(Card(
            Row(() => Fl(0xf4a3), AppInfo.Label.Length == 0 ? $"{AppInfo.Name} {AppInfo.Version}" : $"{AppInfo.Name} {AppInfo.Version} ({AppInfo.Label})", $"{AppInfo.Description}\n{AppInfo.Copyright}", null),
            Row(() => Fl(KeyboardGlyph), "Keyboard shortcuts", "Every shortcut on one page", () => TextButton("Show", () => { _host.ShowResident(); _host.OpenHelp(); })),
            Row(() => Fl(0xf19f), "Restore default settings", "Undo it from the pad's notice", () => TextButton("Restore", () => _host.ResetSettings()))));
    }

    private void StoragePage(StackPanel page)
    {
        page.Children.Add(Group("Notes folder"));
        Control NotesFolderDetails()
        {
            var details = new StackPanel { Spacing = 8 };
            details.Children.Add(ReadOnlyPath(_notes.DataFolder));

            if (_pendingNotesFolder is { } target)
            {
                var pending = Kit.Text($"New location: {target}", _p.Muted, Typography.Small);
                pending.TextWrapping = TextWrapping.Wrap;
                details.Children.Add(pending);
            }

            var error = _notesError ?? (_notesFolderRestartPending ? null : _notes.NotesLocationError);
            if (!string.IsNullOrWhiteSpace(error)) details.Children.Add(InlineNote(error, _p.Error));
            if (!string.IsNullOrWhiteSpace(_notesSuccess)) details.Children.Add(InlineNote(_notesSuccess, _p.Confirm));
            if (!string.IsNullOrWhiteSpace(_notesNotice)) details.Children.Add(InlineNote(_notesNotice, _p.Muted));

            var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
            if (_pendingNotesFolder is { } destination)
            {
                // The pending folder action: Cancel, then the accent action button at the right edge.
                actions.HorizontalAlignment = HorizontalAlignment.Right;
                actions.Children.Add(TextButton("Cancel", () => { _pendingNotesFolder = null; _notesError = null; _notesNotice = null; _notesFolderRestartPending = false; Refresh(); }));
                var unavailable = _notes.NotesLocationError is not null;
                actions.Children.Add(TextButton(unavailable ? "Use this folder" : "Migrate notes",
                    () => { if (unavailable) UseFolder(destination); else MoveNotes(destination); }, primary: true));
            }
            else
            {
                if (_notes.NotesLocationError is null) actions.Children.Add(TextButton("Open", OpenNotesFolder));
                actions.Children.Add(TextButton(_notes.NotesLocationError is null ? "Change location…" : "Choose folder", ChooseNotesFolder));
            }
            details.Children.Add(actions);
            return details;
        }

        page.Children.Add(Card(Row(() => Fl(0xf418), "Notes folder", "Your notes are plain files here, saved as you type.", null),
            SubRow(null, NotesFolderDetails, wide: true, searchTag: "sub Notes folder location and move notes")));

        page.Children.Add(Group("Other locations"));
        NotesLocation.EnsureRecovered(_notes.DataFolder);
        var recovered = Path.Combine(_notes.DataFolder, "recovered");
        int? recoveredCount = null;
        string? recoveredDescription = null;
        string RecoveryDescription()
        {
            recoveredCount ??= RecoveredFileCount(recovered);
            return recoveredDescription ??= $"{recovered}\n{(recoveredCount is null ? "Unavailable" : recoveredCount == 0 ? "Empty" : $"{recoveredCount} {(recoveredCount == 1 ? "file" : "files")}")}\nNote files the app couldn't place. Open them in any text editor.";
        }
        bool RecoveryDescriptionMatches(string query)
        {
            const string suffix = "\nNote files the app couldn't place. Open them in any text editor.";
            if (recovered.Contains(query, StringComparison.OrdinalIgnoreCase) || suffix.Contains(query, StringComparison.OrdinalIgnoreCase)) return true;
            if (query.Contains('\n') || query.Any(char.IsDigit)
                || "Unavailable".Contains(query, StringComparison.OrdinalIgnoreCase)
                || "Empty".Contains(query, StringComparison.OrdinalIgnoreCase))
                return RecoveryDescription().Contains(query, StringComparison.OrdinalIgnoreCase);
            return false;
        }
        page.Children.Add(Card(
            Row(() => Fl(0xf418), "Recovered notes", null,
                () => TextButton("Open", () => OpenFolder(recovered)),
                RecoveryDescription, RecoveryDescriptionMatches),
            Row(() => Fl(0xf359), "App data", $"{_notes.AppHome}\nLog file and the pointer to your notes folder.",
                () => TextButton("Open", () => OpenFolder(_notes.AppHome)))));
    }

    private Control InlineNote(string text, Color color)
    {
        var label = new TextBlock { Text = text, FontSize = Typography.Small, Foreground = new SolidColorBrush(color), TextWrapping = TextWrapping.Wrap };
        return label;
    }

    private async void ChooseNotesFolder()
    {
        try
        {
            var selected = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Choose notes folder", AllowMultiple = false });
            var folder = selected.FirstOrDefault()?.Path.LocalPath;
            if (string.IsNullOrWhiteSpace(folder)) return;
            folder = Path.GetFullPath(folder);
            if (SamePath(folder, _notes.DataFolder)) return;
            _pendingNotesFolder = folder;
            _notesError = null;
            _notesSuccess = null;
            _notesNotice = null;
            _notesFolderRestartPending = false;
            Refresh();
        }
        catch (Exception ex) { _notesError = "Couldn't choose a notes folder · " + ex.Message; Refresh(); }
    }

    private void MoveNotes(string target)
    {
        _host.Save();
        if (_notes.Store.LastError is { } saveError)
        {
            _notesError = "Couldn't save notes · " + saveError;
            Refresh();
            return;
        }

        if (!_notes.MoveNotes(target, out var message, out var result))
        {
            _notesError = message;
            _notesNotice = null;
            Refresh();
            return;
        }

        _pendingNotesFolder = null;
        _notesError = null;
        _notesSuccess = result!.OldCopy!.Message(result.FileCount);
        _notesNotice = _notes.NotesLocationError;
        Refresh();
    }

    private void UseFolder(string target)
    {
        if (_notes.UseFolderOnNextStart(target, out var message))
        {
            _notesError = null;
            _notesSuccess = message;
            _notesNotice = null;
            _notesFolderRestartPending = true;
        }
        else
        {
            _notesError = message;
            _notesSuccess = null;
            _notesNotice = null;
            _notesFolderRestartPending = false;
        }

        _pendingNotesFolder = null;
        Refresh();
    }

    private static int? RecoveredFileCount(string folder)
    {
        try { return Directory.Exists(folder) ? Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Count() : 0; }
        catch { return null; }
    }

    private static bool SamePath(string left, string right) => string.Equals(
        Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
        Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
        StringComparison.OrdinalIgnoreCase);

    private static bool IsSameOrNestedPath(string parent, string candidate)
    {
        try
        {
            parent = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            candidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return string.Equals(parent, candidate, StringComparison.OrdinalIgnoreCase)
                || candidate.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    private void OpenNotesFolder() => OpenFolder(_notes.DataFolder);

    private void OpenFolder(string path)
    {
        try
        {
            if (!Directory.Exists(path)) { _host.Toast($"Folder is unavailable · {path}"); return; }
            DesktopPlatform.OpenFolder(path);
        }
        catch (Exception ex) { _host.Toast("Couldn't open folder · " + ex.Message); }
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
