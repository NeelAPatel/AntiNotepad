using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

/// <summary>
/// The Settings window's building blocks, in AntiNotepad's look on Windows Settings' bones: cards of rows
/// (icon, title, one line of description, control on the right), expanders that open nested rows, and small pictures
/// where a word would be vaguer than an image. Controls update their own look when clicked, so a change never needs
/// the page rebuilt just to show itself.
/// </summary>
internal sealed partial class SettingsWindow
{
    private SolidColorBrush Line => new(_p.Ink, _p.Dark ? .09 : .10);
    private Color IconInk => Color.FromArgb(220, _p.Ink.R, _p.Ink.G, _p.Ink.B);

    // ---------- page structure ----------

    private Control Heading(string text) => new TextBlock
    {
        Text = text, FontSize = 26, FontWeight = FontWeight.SemiBold, Foreground = new SolidColorBrush(_p.Heading), Margin = new Thickness(2, 0, 0, 10)
    };

    private Control Group(string text) => new TextBlock
    {
        Text = text, FontSize = 13, FontWeight = FontWeight.SemiBold, Foreground = new SolidColorBrush(_p.Ink), Margin = new Thickness(2, 20, 0, 8)
    };

    /// <summary>A rounded card; rows inside are divided by hairlines.</summary>
    private Border Card(params Border?[] rows)
    {
        var stack = new StackPanel();
        var first = true;
        foreach (var row in rows)
        {
            if (row is null) continue;
            row.BorderBrush = Line;
            row.BorderThickness = first ? default : new Thickness(0, 1, 0, 0);
            first = false;
            stack.Children.Add(row);
        }
        return new Border
        {
            Background = new SolidColorBrush(_p.Surface, _p.Dark ? .92 : .9), BorderBrush = Line, BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10), ClipToBounds = true, Child = stack, Tag = "card",
        };
    }

    /// <summary>Icon, title and an optional description on the left; the control on the right.</summary>
    private Border? Row(Func<Control?>? icon, string title, string? description, Func<Control?>? control,
        Func<string?>? describe = null, Func<string, bool>? descriptionMatches = null)
    {
        var shownDescription = description;
        var descriptionResolved = describe is null;
        string? ResolveDescription()
        {
            if (!descriptionResolved) { shownDescription = describe!() ?? shownDescription; descriptionResolved = true; }
            return shownDescription;
        }
        if (_searchQuery is { } query && !SearchMatches(title + " " + description, query))
        {
            if (descriptionMatches is not null)
            {
                if (!descriptionMatches(query)) return null;
            }
            else
            {
                shownDescription = ResolveDescription();
                if (!SearchMatches(title + " " + shownDescription, query)) return null;
            }
        }
        if (describe is not null) shownDescription = ResolveDescription();

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto") };
        grid.Children.Add(new Border { Width = 20, Height = 20, Margin = new Thickness(0, 0, 16, 0), Child = icon?.Invoke(), VerticalAlignment = VerticalAlignment.Center });
        var text = Texts(title, shownDescription);
        Grid.SetColumn(text, 1); grid.Children.Add(text);
        var right = control?.Invoke();
        if (right is not null)
        {
            right.Margin = new Thickness(16, 0, 0, 0);
            right.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(right, 2); grid.Children.Add(right);
        }
        return new Border { MinHeight = 62, Padding = new Thickness(16, 10), Child = grid, Tag = title + " " + shownDescription };
    }

    private StackPanel Texts(string title, string? description)
    {
        var text = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        text.Children.Add(new TextBlock { Text = title, FontSize = 14, Foreground = new SolidColorBrush(_p.Ink), TextWrapping = TextWrapping.Wrap });
        if (!string.IsNullOrEmpty(description))
            text.Children.Add(new TextBlock { Text = description, FontSize = 12, Foreground = new SolidColorBrush(_p.Muted), TextWrapping = TextWrapping.Wrap });
        return text;
    }

    /// <summary>A nested row under an expander, indented to the titles above. Wide controls take the whole row.</summary>
    private Border? SubRow(string? title, Func<Control> control, bool wide = false, string? searchTag = null)
    {
        var tag = searchTag ?? "sub " + title;
        if (!SearchMatches(tag, _searchQuery)) return null;
        var content = control();
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        if (wide) { Grid.SetColumnSpan(content, 2); grid.Children.Add(content); }
        else
        {
            grid.Children.Add(Texts(title ?? "", null));
            content.Margin = new Thickness(16, 0, 0, 0);
            content.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(content, 1); grid.Children.Add(content);
        }
        return new Border
        {
            MinHeight = 54, Padding = new Thickness(52, 10, 16, 10), Background = new SolidColorBrush(_p.Ink, _p.Dark ? .03 : .025),
            Child = grid, Tag = tag,
        };
    }

    /// <summary>A row that opens the rows under it. Which ones are open is remembered while AntiNotepad runs.</summary>
    private Border?[] Expander(string key, Func<Control?>? icon, string title, string? description, string value, params Func<Border?>?[] subs)
    {
        var open = Expanded.Contains(key);
        GlyphIcon? chevron = null;
        var kids = subs.Where(s => s is not null).Select(s => s!()).Where(s => s is not null).Cast<Border>().ToArray();
        var row = Row(icon, title, description, () =>
        {
            chevron = new GlyphIcon(char.ConvertFromUtf32(0xf2a3), _p.Muted, 16, 0, Glyph.Fluent) { RenderTransform = new RotateTransform(open ? 180 : 0), VerticalAlignment = VerticalAlignment.Center };
            var right = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            right.Children.Add(Kit.Text(value, _p.Muted, 13));
            right.Children.Add(chevron);
            return right;
        });
        if (row is null) return [.. kids];
        foreach (var kid in kids) kid.IsVisible = open;
        // The whole row toggles, with the soft hover every other clickable surface has.
        var face = row.Child!;
        row.Child = null;
        var button = Kit.Plain(face, _p, radius: 0);
        button.HorizontalAlignment = HorizontalAlignment.Stretch;
        button.Margin = new Thickness(-16, -10);
        if (button.Content is Border inner) inner.Padding = new Thickness(16, 10);
        button.Click += (_, _) =>
        {
            open = !open;
            if (open) Expanded.Add(key); else Expanded.Remove(key);
            foreach (var kid in kids) kid.IsVisible = open;
            ((RotateTransform)chevron!.RenderTransform!).Angle = open ? 180 : 0;
        };
        Avalonia.Automation.AutomationProperties.SetName(button, title);
        row.Child = button;
        return [row, .. kids];
    }

    internal static bool SearchMatches(string? tag, string? query) => query is null
        || tag is not null && tag.Replace("sub ", "").Contains(query, StringComparison.OrdinalIgnoreCase);

    // ---------- controls ----------

    /// <summary>Two to four choices as pills; the pill moves at once, then the setting applies.</summary>
    private Control Seg(IReadOnlyList<(string Value, string Label)> options, string current, Action<string> pick)
    {
        var row = new WrapPanel { Orientation = Orientation.Horizontal };
        var items = new List<(string Value, Border Face, TextBlock Text)>();
        void Paint(string selected)
        {
            foreach (var (value, face, text) in items)
            {
                var on = value == selected;
                face.Background = on ? new SolidColorBrush(_p.Accent, _p.Dark ? .22 : .18) : Brushes.Transparent;
                text.Foreground = new SolidColorBrush(on ? _p.Ink : _p.Muted);
                text.FontWeight = on ? FontWeight.SemiBold : FontWeight.Normal;
            }
        }
        var chosen = current;
        foreach (var (value, label) in options)
        {
            var text = Kit.Text(label, _p.Muted, 13);
            text.Margin = new Thickness(12, 5);
            var button = Kit.Plain(text, _p, radius: 7);
            button.Margin = new Thickness(0, 0, 2, 0);
            var face = (Border)button.Content!;
            items.Add((value, face, text));
            button.PointerExited += (_, _) => Paint(chosen);
            var v = value;
            button.Click += (_, _) => { chosen = v; Paint(v); pick(v); };
            Avalonia.Automation.AutomationProperties.SetName(button, label);
            row.Children.Add(button);
        }
        Paint(current);
        return row;
    }

    /// <summary>A Windows-style toggle with its On/Off word, flipping itself before the setting applies.</summary>
    private Control Toggle(bool on, Action<bool> set, string name)
    {
        var word = Kit.Text(on ? "On" : "Off", _p.Muted, 13);
        var knob = new Border { Width = 14, Height = 14, CornerRadius = new CornerRadius(7), Margin = new Thickness(3, 0) };
        var track = new Border { Width = 40, Height = 20, CornerRadius = new CornerRadius(10), Child = knob, VerticalAlignment = VerticalAlignment.Center };
        void Paint()
        {
            word.Text = on ? "On" : "Off";
            knob.HorizontalAlignment = on ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            knob.Background = new SolidColorBrush(on ? _p.Surface : _p.Muted);
            track.Background = on ? new SolidColorBrush(_p.Accent) : Brushes.Transparent;
            track.BorderBrush = new SolidColorBrush(_p.Faint);
            track.BorderThickness = new Thickness(on ? 0 : 1.5);
        }
        Paint();
        var face = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Margin = new Thickness(8, 4) };
        face.Children.Add(word);
        face.Children.Add(track);
        var button = Kit.Plain(face, _p, radius: 8);
        button.Click += (_, _) => { on = !on; Paint(); set(on); };
        Avalonia.Automation.AutomationProperties.SetName(button, name);
        return button;
    }

    /// <summary>A dropdown in the pad's own colours: the current choice and a chevron, opening a short list.</summary>
    private Control Dropdown(IReadOnlyList<(string Value, string Label)> options, string current, Action<string> pick, double minWidth = 160)
    {
        var label = Kit.Text(options.FirstOrDefault(o => o.Value == current).Label ?? current, _p.Ink, 13);
        var face = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(12, 0, 10, 0) };
        face.Children.Add(label);
        var chevron = new GlyphIcon(char.ConvertFromUtf32(0xf2a3), _p.Muted, 14, 0, Glyph.Fluent) { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
        Grid.SetColumn(chevron, 1); face.Children.Add(chevron);
        var box = new Border { Height = 32, MinWidth = minWidth, CornerRadius = new CornerRadius(7), BorderBrush = Line, BorderThickness = new Thickness(1), Background = new SolidColorBrush(_p.Surface), Child = face };
        var button = Kit.Plain(box, _p, radius: 7);
        var list = new StackPanel { Spacing = 1 };
        var popup = new Popup
        {
            PlacementTarget = button, Placement = PlacementMode.BottomEdgeAlignedLeft, IsLightDismissEnabled = true, VerticalOffset = 4,
            Child = new Border
            {
                Background = new SolidColorBrush(_p.Surface), BorderBrush = Line, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8),
                Padding = new Thickness(4), MinWidth = minWidth, Child = list,
                BoxShadow = new BoxShadows(new BoxShadow { Blur = 16, OffsetY = 4, Color = Color.FromArgb(_p.Dark ? (byte)110 : (byte)40, 0, 0, 0) }),
            },
        };
        foreach (var (value, text) in options)
        {
            var item = Kit.Text(text, value == current ? _p.Ink : _p.Muted, 13, value == current ? FontWeight.SemiBold : FontWeight.Normal);
            item.Margin = new Thickness(10, 6);
            var row = Kit.Plain(item, _p, radius: 6);
            row.HorizontalAlignment = HorizontalAlignment.Stretch;
            if (value == current && row.Content is Border b) b.Background = new SolidColorBrush(_p.Accent, _p.Dark ? .22 : .18);
            var v = value;
            row.Click += (_, _) => { popup.IsOpen = false; label.Text = text; pick(v); };
            list.Children.Add(row);
        }
        button.Click += (_, _) => popup.IsOpen = !popup.IsOpen;
        Avalonia.Automation.AutomationProperties.SetName(button, label.Text);
        return new Panel { Children = { button, popup } };
    }

    /// <summary>A small text button (Change, Restore, Show). <paramref name="primary"/> fills it with the accent for the one action that changes something.</summary>
    private Button TextButton(string text, System.Action click, bool primary = false)
    {
        // Dark text on a light accent, white on a dark one.
        var a = _p.Accent;
        var onAccent = 0.2126 * a.R + 0.7152 * a.G + 0.0722 * a.B > 150 ? Color.FromRgb(0x1A, 0x1A, 0x1A) : Colors.White;
        var label = Kit.Text(text, primary ? onAccent : _p.Ink, 13, primary ? FontWeight.SemiBold : FontWeight.Normal);
        label.Margin = new Thickness(12, 5);
        var face = primary
            ? new Border { Background = new SolidColorBrush(a), CornerRadius = new CornerRadius(7), Child = label }
            : new Border { BorderBrush = Line, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(7), Child = label };
        var button = Kit.Plain(face, _p, radius: 7);
        if (primary)
        {
            // The filled face hides Plain's hover tint, so it lightens a little instead.
            button.PointerEntered += (_, _) => face.Opacity = 0.86;
            button.PointerExited += (_, _) => face.Opacity = 1;
        }
        button.Click += (_, _) => click();
        return button;
    }

    /// <summary>A palette-styled, selectable path field that cannot be edited.</summary>
    private TextBox ReadOnlyPath(string path) => new()
    {
        Text = path,
        IsReadOnly = true,
        AcceptsReturn = true,
        TextWrapping = TextWrapping.Wrap,
        MinHeight = 38,
        MaxHeight = 68,
        FontSize = 13,
        Padding = new Thickness(10, 7),
        Foreground = new SolidColorBrush(_p.Ink),
        CaretBrush = new SolidColorBrush(_p.Caret),
        CornerRadius = new CornerRadius(7),
    };

    // ---------- pictures ----------

    /// <summary>A picture with its label under it; the chosen one wears an accent ring.</summary>
    private Control Tile(Control picture, string label, bool selected, System.Action click, double width)
    {
        var ring = selected ? 2 : 1.5;
        var frame = new Border
        {
            Width = width, CornerRadius = new CornerRadius(8), Padding = new Thickness(ring),
            BorderBrush = selected ? new SolidColorBrush(_p.Accent) : Line, BorderThickness = new Thickness(ring),
            // The picture is clipped to the ring's inner curve, so its corners never show outside the frame.
            Child = new Border { CornerRadius = new CornerRadius(8 - ring), ClipToBounds = true, Child = picture },
        };
        var stack = new StackPanel { Spacing = 5, HorizontalAlignment = HorizontalAlignment.Center };
        stack.Children.Add(frame);
        var text = Kit.Text(label, selected ? _p.Ink : _p.Muted, 12, selected ? FontWeight.SemiBold : FontWeight.Normal);
        text.HorizontalAlignment = HorizontalAlignment.Center;
        text.TextAlignment = TextAlignment.Center;
        stack.Children.Add(text);
        var button = Kit.Plain(stack, _p, radius: 8);
        if (button.Content is Border b) b.Padding = new Thickness(4);
        button.Click += (_, _) => click();
        Avalonia.Automation.AutomationProperties.SetName(button, label);
        return button;
    }

    private Control PaletteChips(bool dark, string current, Action<string> pick)
    {
        var grid = new UniformGrid { Columns = 4 };
        foreach (var palette in Palette.For(dark))
        {
            var dots = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 3, HorizontalAlignment = HorizontalAlignment.Center };
            foreach (var c in new[] { palette.Keyword, palette.Identifier, palette.Result })
                dots.Children.Add(new Border { Width = 7, Height = 7, CornerRadius = new CornerRadius(3.5), Background = new SolidColorBrush(c) });
            var face = new StackPanel { Spacing = 5, Margin = new Thickness(6, 8) };
            face.Children.Add(new TextBlock { Text = palette.Name, FontSize = 12, Foreground = new SolidColorBrush(palette.Ink), HorizontalAlignment = HorizontalAlignment.Center });
            face.Children.Add(dots);
            var selected = palette.Id == current;
            var chip = new Border
            {
                Background = new SolidColorBrush(palette.Canvas), CornerRadius = new CornerRadius(9), Margin = new Thickness(0, 0, 8, 0), Child = face,
                BackgroundSizing = BackgroundSizing.InnerBorderEdge,
                BorderBrush = selected ? new SolidColorBrush(_p.Accent) : Line, BorderThickness = new Thickness(selected ? 2 : 1), Cursor = new Cursor(StandardCursorType.Hand),
            };
            var id = palette.Id;
            chip.PointerPressed += (_, e) => { e.Handled = true; pick(id); };
            Avalonia.Automation.AutomationProperties.SetName(chip, palette.Name);
            grid.Children.Add(chip);
        }
        return grid;
    }

    private Control AccentDots()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7 };
        void Add(string? hex, Color color, string name)
        {
            var selected = S.Accent == hex;
            var dot = new Border
            {
                Width = 18, Height = 18, CornerRadius = new CornerRadius(9), Background = new SolidColorBrush(color), Cursor = new Cursor(StandardCursorType.Hand),
                BorderBrush = new SolidColorBrush(selected ? _p.Ink : _p.Faint, selected ? .9 : .3), BorderThickness = new Thickness(selected ? 2.5 : 1),
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

    /// <summary>"a" followed by a bar or a block caret, as it looks in the editor.</summary>
    private Control CaretPicture(bool block)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        row.Children.Add(new TextBlock { Text = "a", FontFamily = new FontFamily("Cascadia Mono, Consolas"), FontSize = 14, Foreground = new SolidColorBrush(_p.Ink), VerticalAlignment = VerticalAlignment.Center });
        row.Children.Add(new Border
        {
            Width = block ? 8 : 2, Height = 16, Margin = new Thickness(1, 0, 0, 0), CornerRadius = new CornerRadius(1),
            Background = new SolidColorBrush(_p.Caret, block ? .45 : 1), VerticalAlignment = VerticalAlignment.Center,
        });
        return new Border { Height = 34, Background = new SolidColorBrush(_p.Canvas), Child = row };
    }

    private Control MaterialPicture(string material)
    {
        IBrush fill = material switch
        {
            "acrylic" => new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative), EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops = { new GradientStop(Color.FromArgb(90, _p.Keyword.R, _p.Keyword.G, _p.Keyword.B), 0), new GradientStop(Color.FromArgb(90, _p.Identifier.R, _p.Identifier.G, _p.Identifier.B), 1) },
            },
            "mica" => new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative), EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops = { new GradientStop(Color.FromArgb(40, _p.Ink.R, _p.Ink.G, _p.Ink.B), 0), new GradientStop(Colors.Transparent, 1) },
            },
            _ => new SolidColorBrush(_p.Canvas),
        };
        return new Border { Height = 46, Background = new SolidColorBrush(_p.Canvas), Child = new Border { Background = fill } };
    }

    // ---------- icons ----------

    /// <summary>A Fluent UI System Icons glyph by codepoint (the bundled font, regular weight at its 20 px design size).</summary>
    private Control Fl(int codepoint) => new GlyphIcon(char.ConvertFromUtf32(codepoint), IconInk, 20, 0, Glyph.Fluent)
    {
        HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
    };

    /// <summary>An icon the Segoe set doesn't have, drawn in the same 1.4 px line on a 20 px square.</summary>
    private Control Drawn(Action<DrawingContext, Pen, IBrush> draw) => new DrawnIcon(draw, IconInk);

    private Control ModeIcon() => Drawn((dc, pen, ink) =>
    {
        dc.DrawEllipse(null, pen, new Point(10, 10), 7, 7);
        var half = new StreamGeometry();
        using (var c = half.Open()) { c.BeginFigure(new Point(10, 3), true); c.ArcTo(new Point(10, 17), new Size(7, 7), 0, false, SweepDirection.Clockwise); c.EndFigure(true); }
        dc.DrawGeometry(ink, null, half);
    });

    private Control ScaleIcon() => Drawn((dc, pen, _) =>
    {
        var g = new StreamGeometry();
        using (var c = g.Open())
        {
            c.BeginFigure(new Point(3, 7), false); c.LineTo(new Point(3, 5.5)); c.ArcTo(new Point(5.5, 3), new Size(2.5, 2.5), 0, false, SweepDirection.Clockwise); c.LineTo(new Point(7, 3)); c.EndFigure(false);
            c.BeginFigure(new Point(13, 3), false); c.LineTo(new Point(14.5, 3)); c.ArcTo(new Point(17, 5.5), new Size(2.5, 2.5), 0, false, SweepDirection.Clockwise); c.LineTo(new Point(17, 7)); c.EndFigure(false);
            c.BeginFigure(new Point(17, 13), false); c.LineTo(new Point(17, 14.5)); c.ArcTo(new Point(14.5, 17), new Size(2.5, 2.5), 0, false, SweepDirection.Clockwise); c.LineTo(new Point(13, 17)); c.EndFigure(false);
        }
        dc.DrawGeometry(null, pen, g);
        dc.DrawRectangle(null, pen, new Rect(3, 10, 7, 7), 1.8, 1.8);
    });

    private Control ToolbarIcon() => Drawn((dc, pen, _) =>
    {
        dc.DrawRectangle(null, pen, new Rect(2.7, 4.7, 14.6, 10.6), 2.2, 2.2);
        dc.DrawLine(pen, new Point(2.7, 11.6), new Point(17.3, 11.6));
    });

    private Control IndentIcon() => Drawn((dc, pen, _) =>
    {
        var g = new StreamGeometry();
        using (var c = g.Open()) { c.BeginFigure(new Point(5, 5.5), false); c.LineTo(new Point(9.5, 10)); c.LineTo(new Point(5, 14.5)); c.EndFigure(false); }
        dc.DrawGeometry(null, pen, g);
        dc.DrawLine(pen, new Point(14.5, 4.5), new Point(14.5, 15.5));
    });

    /// <summary>A text cursor before letters, like Windows' "Text cursor" icons; the blink one adds a small clock.</summary>
    private Control CursorIcon(string letters, bool clock) => Drawn((dc, pen, ink) =>
    {
        dc.DrawRectangle(ink, null, new Rect(1.5, 3, 1.8, 14), 0.9, 0.9);
        var text = new FormattedText(letters, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface("Segoe UI Variable Text, Segoe UI", FontStyle.Normal, FontWeight.SemiBold), 12.5, ink);
        dc.DrawText(text, new Point(4.5, 10 - text.Height / 2));
        if (!clock) return;
        var thin = new Pen(ink, 1.1);
        dc.DrawEllipse(new SolidColorBrush(_p.Surface), thin, new Point(15, 14.5), 3.7, 3.7);
        dc.DrawLine(thin, new Point(15, 12.6), new Point(15, 14.6));
        dc.DrawLine(thin, new Point(15, 14.6), new Point(16.3, 15.4));
    });

    /// <summary>Backing patterns as they draw on the page.</summary>
    private sealed class PatternSwatch(string kind, Palette p) : Control
    {
        public override void Render(DrawingContext dc)
        {
            var w = Bounds.Width; var h = Bounds.Height;
            dc.DrawRectangle(new SolidColorBrush(p.Canvas), null, new Rect(0, 0, w, h));
            var pen = new Pen(new SolidColorBrush(p.Paper, kind == "biggrid" ? 1 : .9), 1);
            switch (kind)
            {
                case "lined": for (var y = 8.5; y < h; y += 8) dc.DrawLine(pen, new Point(0, y), new Point(w, y)); break;
                case "dotted":
                    var dot = new SolidColorBrush(p.Faint);
                    for (var y = 4.0; y < h; y += 8) for (var x = 4.0; x < w; x += 8) dc.DrawEllipse(dot, null, new Point(x, y), 0.9, 0.9);
                    break;
                case "grid":
                case "biggrid":
                    var step = kind == "grid" ? 8 : 16;
                    for (var y = step + 0.5; y < h; y += step) dc.DrawLine(pen, new Point(0, y), new Point(w, y));
                    for (var x = step + 0.5; x < w; x += step) dc.DrawLine(pen, new Point(x, 0), new Point(x, h));
                    break;
            }
        }
    }

    /// <summary>A little screen with the pad where that Open at choice puts it.</summary>
    private sealed class ScreenPicture(string where, Palette p) : Control
    {
        public override void Render(DrawingContext dc)
        {
            var w = Bounds.Width; var h = Bounds.Height;
            dc.DrawRectangle(new SolidColorBrush(p.Surface), null, new Rect(0, 0, w, h));
            dc.DrawRectangle(new SolidColorBrush(p.Ink, .10), null, new Rect(0, h - 7, w, 7));
            var pad = new SolidColorBrush(p.Accent, .8);
            Rect r = where switch
            {
                "center" => new Rect(w / 2 - 13, h / 2 - 13, 26, 20),
                "mouse" => new Rect(w * 0.46, h * 0.38, 26, 20),
                "tray" => new Rect(w - 32, h - 7 - 24, 26, 20),
                "dropdown" => new Rect(w / 2 - 13, 0, 26, 20),
                _ => new Rect(12, 10, 26, 20),
            };
            dc.DrawRectangle(pad, null, r, where == "dropdown" ? 0 : 4, where == "dropdown" ? 0 : 4);
            if (where != "mouse") return;
            var arrow = new StreamGeometry();
            using (var c = arrow.Open())
            {
                var o = new Point(r.X - 2, r.Y - 3);
                c.BeginFigure(o, true);
                foreach (var (x, y) in new[] { (0, 10.0), (3, 7), (5.5, 11.5), (7, 10.7), (4.6, 6.4), (8.5, 6.4) }) c.LineTo(new Point(o.X + x, o.Y + y));
                c.EndFigure(true);
            }
            dc.DrawGeometry(new SolidColorBrush(p.Ink), new Pen(new SolidColorBrush(p.Surface), 1), arrow);
        }
    }

    private sealed class DrawnIcon(Action<DrawingContext, Pen, IBrush> draw, Color ink) : Control
    {
        protected override Size MeasureOverride(Size availableSize) => new(20, 20);

        public override void Render(DrawingContext dc)
        {
            var brush = new SolidColorBrush(ink);
            draw(dc, new Pen(brush, 1.4, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round), brush);
        }
    }
}
