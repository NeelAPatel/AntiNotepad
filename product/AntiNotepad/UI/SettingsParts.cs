using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using AntiNotepad.Motion;
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
    private SolidColorBrush Line => new(_p.Ink, _p.Dark ? .09 : .06);
    private Color IconInk => Color.FromArgb(Paint.IconInkAlpha, _p.Ink.R, _p.Ink.G, _p.Ink.B);

    // ---------- page structure ----------

    private Control Heading(string text) => new TextBlock
    {
        Text = text,
        FontSize = Typography.Display,
        FontWeight = Typography.Strong,
        Foreground = new SolidColorBrush(_p.Heading),
        Margin = new Thickness(2, 0, 0, 12)
    };

    private Control Group(string text) => new TextBlock
    {
        Text = text,
        FontSize = Typography.Body,
        FontWeight = Typography.Strong,
        Foreground = new SolidColorBrush(_p.Ink),
        Margin = new Thickness(2, 24, 0, 8)
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
            Background = new SolidColorBrush(_p.Surface, Paint.CardSurface(_p.Dark)),
            BorderBrush = Line,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(Shape.Card),
            ClipToBounds = true,
            Child = stack,
            Tag = "card",
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
        return new Border { MinHeight = 64, Padding = new Thickness(16, 12), Child = grid, Tag = title + " " + shownDescription };
    }

    private StackPanel Texts(string title, string? description)
    {
        var text = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        text.Children.Add(new TextBlock { Text = title, FontSize = Typography.Large, FontWeight = Typography.Strong, Foreground = new SolidColorBrush(_p.Ink), TextWrapping = TextWrapping.Wrap });
        if (!string.IsNullOrEmpty(description))
            text.Children.Add(new TextBlock { Text = description, FontSize = Typography.Small, Foreground = new SolidColorBrush(_p.Muted), TextWrapping = TextWrapping.Wrap });
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
            MinHeight = 56,
            Padding = new Thickness(48, 12, 16, 12),
            Background = new SolidColorBrush(_p.Ink, Paint.RowWash(_p.Dark)),
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
            chevron = new GlyphIcon(Icons.SettingsChevron, _p.Muted, Typography.IconMedium) { RenderTransform = new RotateTransform(open ? 180 : 0), VerticalAlignment = VerticalAlignment.Center };
            var right = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            right.Children.Add(Kit.Text(value, _p.Muted, Typography.Body));
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
        button.Margin = new Thickness(-16, -12);
        if (button.Content is Border inner) inner.Padding = new Thickness(16, 12);
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

    /// <summary>Returns the measured face bounds in the selector's coordinate space.</summary>
    internal static Rect SegmentTarget(Point origin, Size size) => new(origin, size);

    internal sealed record SegmentPosition(string Value, Rect Bounds, long Time, object Owner);

    /// <summary>Chooses a recent previous highlight as the starting point for a rebuilt selector.</summary>
    internal static Rect SegmentStart(SegmentPosition? previous, string current, Rect target, long now, bool reduced) =>
        !reduced && previous is not null && previous.Value != current && now >= previous.Time && now - previous.Time <= 1000
            ? previous.Bounds : target;

    /// <summary>Choices share a moving highlight while each pill fades its hover wash.</summary>
    private Control Seg(IReadOnlyList<(string Value, string Label)> options, string current, Action<string> pick)
    {
        var row = new WrapPanel { Orientation = Orientation.Horizontal };
        var highlight = new Border { Background = new SolidColorBrush(_p.Accent, AntiNotepad.Theme.Paint.Selected(_p.Dark)), CornerRadius = new CornerRadius(Shape.Control), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, IsHitTestVisible = false, IsVisible = false };
        var root = new Grid { Children = { highlight, row } };
        var items = new List<(string Value, Border Face, TextBlock Text)>();
        var signature = string.Join("|", options.Select(option => option.Value));
        var owner = new object();
        var chosen = current;
        Rect position = default, target = default;
        var initialized = false;
        Anim.Handle? slide = null;
        void Place(Rect bounds)
        {
            position = bounds;
            highlight.Margin = new Thickness(bounds.X, bounds.Y, 0, 0);
            highlight.Width = bounds.Width;
            highlight.Height = bounds.Height;
            highlight.IsVisible = true;
            if (!_segmentPositions.TryGetValue(signature, out var saved) || ReferenceEquals(saved.Owner, owner))
                _segmentPositions[signature] = new SegmentPosition(chosen, bounds, Environment.TickCount64, owner);
        }
        void UpdateTarget()
        {
            var face = items.FirstOrDefault(item => item.Value == chosen).Face;
            if (face is null || face.Bounds.Width <= 0 || face.TranslatePoint(default, root) is not { } origin) return;
            var next = SegmentTarget(origin, face.Bounds.Size);
            if (!initialized)
            {
                initialized = true;
                _segmentPositions.TryGetValue(signature, out var previous);
                var start = SegmentStart(previous, chosen, next, Environment.TickCount64, Anim.Reduced);
                _segmentPositions[signature] = new SegmentPosition(chosen, start, Environment.TickCount64, owner);
                Place(start);
                if (start == next) { target = next; return; }
            }
            if (next == target) return;
            target = next;
            slide?.Cancel();
            var from = position;
            slide = Anim.Run(root, MotionTokens.Win11Fast, t => Place(new Rect(
                from.X + (next.X - from.X) * t, from.Y + (next.Y - from.Y) * t,
                from.Width + (next.Width - from.Width) * t, from.Height + (next.Height - from.Height) * t)), easing: MotionTokens.Win11Enter);
        }
        void Paint(string selected)
        {
            foreach (var (value, _, text) in items)
            {
                var on = value == selected;
                text.Foreground = new SolidColorBrush(on ? _p.Ink : _p.Muted);
                text.FontWeight = on ? Typography.Strong : Typography.Regular;
            }
        }
        foreach (var (value, label) in options)
        {
            var text = Kit.Text(label, _p.Muted, Typography.Body);
            text.Margin = new Thickness(12, 4);
            var button = Kit.Plain(text, _p, radius: Shape.Control);
            button.Margin = new Thickness(0, 0, 2, 0);
            var face = (Border)button.Content!;
            items.Add((value, face, text));
            var hover = new Border { Background = new SolidColorBrush(_p.Ink, AntiNotepad.Theme.Paint.Hover(_p.Dark)), CornerRadius = new CornerRadius(Shape.Control), Opacity = 0, IsHitTestVisible = false };
            face.Child = null;
            face.Child = new Grid { Children = { hover, text } };
            Anim.Handle? fade = null;
            void Hover(bool on)
            {
                face.Background = Brushes.Transparent;
                fade?.Cancel();
                var from = hover.Opacity;
                var to = on ? 1.0 : 0.0;
                fade = Anim.Run(button, on ? MotionTokens.Win11Fast : MotionTokens.Win11Faster,
                    t => hover.Opacity = from + (to - from) * t, easing: on ? MotionTokens.Win11Enter : MotionTokens.Win11Exit);
            }
            button.PointerEntered += (_, _) => Hover(true);
            button.PointerExited += (_, _) => Hover(false);
            button.AddHandler(InputElement.PointerPressedEvent, (_, _) => face.Background = Brushes.Transparent, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
            button.AddHandler(InputElement.PointerReleasedEvent, (_, _) => face.Background = Brushes.Transparent, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
            var v = value;
            button.Click += (_, _) =>
            {
                if (v != chosen) { slide?.Cancel(); target = default; }
                if (initialized) _segmentPositions[signature] = new SegmentPosition(chosen, position, Environment.TickCount64, owner);
                chosen = v; Paint(v); pick(v);
            };
            Avalonia.Automation.AutomationProperties.SetName(button, label);
            row.Children.Add(button);
        }
        Paint(current);
        root.LayoutUpdated += (_, _) => UpdateTarget();
        return root;
    }

    /// <summary>A Windows-style toggle with its On/Off word, flipping itself before the setting applies.</summary>
    private Control Toggle(bool on, Action<bool> set, string name)
    {
        var word = Kit.Text(on ? "On" : "Off", _p.Muted, Typography.Body);
        var knob = new Border { Width = MotionTokens.ToggleRest, Height = MotionTokens.ToggleRest, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };
        var outline = new Border { CornerRadius = new CornerRadius(Shape.Round(20)), BorderBrush = new SolidColorBrush(_p.Faint), BorderThickness = new Thickness(1), Opacity = on ? 0 : 1 };
        var fill = new Border { CornerRadius = new CornerRadius(Shape.Round(20)), Background = new SolidColorBrush(_p.Accent), Opacity = on ? 1 : 0 };
        var track = new Grid { Width = 40, Height = 20, VerticalAlignment = VerticalAlignment.Center, Children = { outline, fill, knob } };
        var position = on ? 1.0 : 0.0;
        var hovering = false;
        var pressed = false;
        Anim.Handle? slide = null, resize = null, fade = null;
        void Place()
        {
            knob.Margin = new Thickness(3 + (36 - knob.Width - 3) * position, 0, 0, 0);
            knob.CornerRadius = new CornerRadius(Shape.Round(knob.Height));
        }
        void Resize()
        {
            resize?.Cancel();
            var width = Anim.Reduced ? MotionTokens.ToggleRest : pressed ? MotionTokens.TogglePressedWidth : hovering ? MotionTokens.ToggleHover : MotionTokens.ToggleRest;
            var height = Anim.Reduced ? MotionTokens.ToggleRest : pressed || hovering ? MotionTokens.ToggleHover : MotionTokens.ToggleRest;
            var fromWidth = knob.Width; var fromHeight = knob.Height;
            resize = Anim.Run(track, MotionTokens.Win11Faster, t =>
            {
                knob.Width = fromWidth + (width - fromWidth) * t;
                knob.Height = fromHeight + (height - fromHeight) * t;
                Place();
            }, easing: MotionTokens.Win11Enter);
            NoFill();
        }
        knob.Background = new SolidColorBrush(on ? _p.Surface : _p.Muted);
        Place();
        var face = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Margin = new Thickness(8, 4) };
        face.Children.Add(word);
        face.Children.Add(track);
        var button = Kit.Plain(face, _p, radius: Shape.Popup);
        // Plain paints a grey hover fill on pointer events; the toggle keeps the hit area but not the fill.
        var plainFill = (Border)button.Content!;
        void NoFill() => plainFill.Background = Brushes.Transparent;
        button.PointerEntered += (_, _) => { hovering = true; Resize(); };
        button.PointerExited += (_, _) => { hovering = false; pressed = false; Resize(); };
        button.AddHandler(InputElement.PointerPressedEvent, (_, e) => { if (e.GetCurrentPoint(button).Properties.IsLeftButtonPressed) { pressed = true; Resize(); } NoFill(); }, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        button.AddHandler(InputElement.PointerReleasedEvent, (_, _) => { pressed = false; Resize(); }, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        button.PointerCaptureLost += (_, _) => { pressed = false; Resize(); };
        button.Click += (_, _) =>
        {
            on = !on;
            word.Text = on ? "On" : "Off";
            knob.Background = new SolidColorBrush(on ? _p.Surface : _p.Muted);
            slide?.Cancel(); fade?.Cancel();
            var from = position; var target = on ? 1.0 : 0.0; var fromFill = fill.Opacity;
            slide = Anim.Run(track, MotionTokens.Win11Fast, t => { position = from + (target - from) * t; Place(); }, easing: MotionTokens.Win11Enter);
            fade = Anim.Run(track, MotionTokens.Win11Faster, t => { fill.Opacity = fromFill + (target - fromFill) * t; outline.Opacity = 1 - fill.Opacity; }, easing: MotionTokens.Win11Enter);
            set(on);
            if (Anim.Reduced)
            {
                slide.Cancel(); fade.Cancel(); resize?.Cancel();
                position = target; fill.Opacity = target; outline.Opacity = 1 - target;
                knob.Width = knob.Height = MotionTokens.ToggleRest;
                Place();
            }
        };
        Avalonia.Automation.AutomationProperties.SetName(button, name);
        return button;
    }

    /// <summary>A dropdown in the pad's own colours: the current choice and a chevron, opening a short list.</summary>
    private Control Dropdown(IReadOnlyList<(string Value, string Label)> options, string current, Action<string> pick, double minWidth = 160)
    {
        var label = Kit.Text(options.FirstOrDefault(o => o.Value == current).Label ?? current, _p.Ink, Typography.Body);
        var face = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(12, 12) };
        face.Children.Add(label);
        var chevron = new GlyphIcon(Icons.SettingsChevron, _p.Muted, Typography.IconSmall) { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
        Grid.SetColumn(chevron, 1); face.Children.Add(chevron);
        var box = new Border { Height = 48, MinWidth = minWidth, CornerRadius = new CornerRadius(Shape.Control), BorderBrush = Line, BorderThickness = new Thickness(1), Background = new SolidColorBrush(_p.Surface), Child = face };
        var button = Kit.Plain(box, _p, radius: Shape.Control);
        var list = new StackPanel { Spacing = 1 };
        var popup = new Popup
        {
            PlacementTarget = button, Placement = PlacementMode.BottomEdgeAlignedLeft, IsLightDismissEnabled = true, VerticalOffset = 4,
            Child = new Border
            {
                Background = new SolidColorBrush(_p.Surface),
                BorderBrush = new SolidColorBrush(_p.Faint, Paint.PopupBorder),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(Shape.Popup),
                Padding = new Thickness(4),
                MinWidth = minWidth,
                Child = list,
                BoxShadow = new BoxShadows(new BoxShadow { Blur = Shape.DropdownShadowBlur, OffsetY = Shape.PopupShadowOffsetY, Color = Color.FromArgb(_p.Dark ? Shape.PopupShadowDarkAlpha : Shape.PopupShadowLightAlpha, 0, 0, 0) }),
            },
        };
        foreach (var (value, text) in options)
        {
            var item = Kit.Text(text, value == current ? _p.Ink : _p.Muted, Typography.Body, value == current ? Typography.Strong : Typography.Regular);
            item.Margin = new Thickness(12, 8);
            var row = Kit.Plain(item, _p, radius: 4);
            row.HorizontalAlignment = HorizontalAlignment.Stretch;
            if (value == current && row.Content is Border b) b.Background = new SolidColorBrush(_p.Accent, Paint.Selected(_p.Dark));
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
        var label = Kit.Text(text, primary ? onAccent : _p.Ink, Typography.Body, primary ? Typography.Strong : Typography.Regular);
        label.Margin = new Thickness(12, 4);
        var face = primary
            ? new Border { Background = new SolidColorBrush(a), CornerRadius = new CornerRadius(Shape.Control), Child = label }
            : new Border { BorderBrush = Line, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(Shape.Control), Child = label };
        var button = Kit.Plain(face, _p, radius: Shape.Control);
        if (primary)
        {
            // The filled face hides Plain's hover tint, so it lightens a little instead.
            button.PointerEntered += (_, _) => face.Opacity = Paint.HoverFace;
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
        MinHeight = 40,
        MaxHeight = 68,
        FontSize = Typography.Body,
        Padding = new Thickness(12, 8),
        Foreground = new SolidColorBrush(_p.Ink),
        CaretBrush = new SolidColorBrush(_p.Caret),
        CornerRadius = new CornerRadius(Shape.Control),
    };

    // ---------- pictures ----------

    /// <summary>A picture with its label under it; the chosen one wears an accent ring.</summary>
    private Control Tile(Control picture, string label, bool selected, System.Action click, double width)
    {
        var ring = selected ? 2 : 1;
        var frame = new Border
        {
            Width = width,
            CornerRadius = new CornerRadius(Shape.Popup),
            Padding = new Thickness(ring),
            BorderBrush = selected ? new SolidColorBrush(_p.Accent) : Line, BorderThickness = new Thickness(ring),
            // The picture is clipped to the ring's inner curve, so its corners never show outside the frame.
            Child = new Border { CornerRadius = new CornerRadius(Shape.Popup - ring), ClipToBounds = true, Child = picture },
        };
        var stack = new StackPanel { Spacing = 5, HorizontalAlignment = HorizontalAlignment.Center };
        stack.Children.Add(frame);
        var text = Kit.Text(label, selected ? _p.Ink : _p.Muted, Typography.Small, selected ? Typography.Strong : Typography.Regular);
        text.HorizontalAlignment = HorizontalAlignment.Center;
        text.TextAlignment = TextAlignment.Center;
        stack.Children.Add(text);
        var button = Kit.Plain(stack, _p, radius: Shape.Popup);
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
            var dots = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, HorizontalAlignment = HorizontalAlignment.Center };
            foreach (var c in new[] { palette.Keyword, palette.Identifier, palette.Result })
                dots.Children.Add(new Border { Width = 8, Height = 8, CornerRadius = new CornerRadius(Shape.Round(8)), Background = new SolidColorBrush(c) });
            var face = new StackPanel { Spacing = 8, Margin = new Thickness(8, 8) };
            face.Children.Add(new TextBlock { Text = palette.Name, FontSize = Typography.Small, Foreground = new SolidColorBrush(palette.Ink), HorizontalAlignment = HorizontalAlignment.Center });
            face.Children.Add(dots);
            var selected = palette.Id == current;
            var chip = new Border
            {
                Background = new SolidColorBrush(palette.Canvas),
                CornerRadius = new CornerRadius(Shape.Round(18)),
                Margin = new Thickness(0, 0, 8, 0),
                Child = face,
                BackgroundSizing = BackgroundSizing.InnerBorderEdge,
                BorderBrush = selected ? new SolidColorBrush(_p.Accent) : new SolidColorBrush(_p.Faint, Paint.CardBorderOff),
                BorderThickness = new Thickness(selected ? 2 : 1),
                Cursor = new Cursor(StandardCursorType.Hand),
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
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        void Add(string? hex, Color color, string name)
        {
            var selected = S.Accent == hex;
            var dot = new Border
            {
                Width = 20,
                Height = 20,
                CornerRadius = new CornerRadius(Shape.Round(20)),
                Background = new SolidColorBrush(color),
                Cursor = new Cursor(StandardCursorType.Hand),
                BorderBrush = new SolidColorBrush(selected ? _p.Ink : _p.Faint, selected ? Paint.CardBorderOn : Paint.CardBorderOff),
                BorderThickness = new Thickness(selected ? 2 : 1),
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
        row.Children.Add(new TextBlock { Text = "a", FontFamily = new FontFamily(Typography.MonoFamily), FontSize = Typography.Large, Foreground = new SolidColorBrush(_p.Ink), VerticalAlignment = VerticalAlignment.Center });
        row.Children.Add(new Border
        {
            Width = block ? 8 : 2,
            Height = 16,
            Margin = new Thickness(1, 0, 0, 0),
            CornerRadius = new CornerRadius(Shape.Hairline),
            Background = new SolidColorBrush(_p.Caret, block ? Paint.CaretBlock : 1),
            VerticalAlignment = VerticalAlignment.Center,
        });
        return new Border { Height = 32, Background = new SolidColorBrush(_p.Canvas), Child = row };
    }

    private Control MaterialPicture(string material)
    {
        IBrush fill = material switch
        {
            "acrylic" => new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops = { new GradientStop(Color.FromArgb(Paint.SampleGradientAlpha, _p.Keyword.R, _p.Keyword.G, _p.Keyword.B), 0), new GradientStop(Color.FromArgb(Paint.SampleGradientAlpha, _p.Identifier.R, _p.Identifier.G, _p.Identifier.B), 1) },
            },
            "mica" => new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops = { new GradientStop(Color.FromArgb(Paint.SampleFadeAlpha, _p.Ink.R, _p.Ink.G, _p.Ink.B), 0), new GradientStop(Colors.Transparent, 1) },
            },
            _ => new SolidColorBrush(_p.Canvas),
        };
        return new Border { Height = 48, Background = new SolidColorBrush(_p.Canvas), Child = new Border { Background = fill } };
    }

    // ---------- icons ----------

    /// <summary>Draws a registered Fluent icon in the Settings row color.</summary>
    private Control Fl(Icon icon) => new GlyphIcon(icon, IconInk, Typography.IconXLarge)
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
            new Typeface(Typography.UiFamily, FontStyle.Normal, Typography.Strong), Typography.Label, ink);
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
            var pen = new Pen(new SolidColorBrush(p.Paper, kind == "biggrid" ? 1 : Paint.GridPen), 1);
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
            dc.DrawRectangle(new SolidColorBrush(p.Ink, p.Dark ? Paint.PreviewBar : Paint.Hover(false)), null, new Rect(0, h - 7, w, 7));
            var pad = new SolidColorBrush(p.Accent, Paint.PreviewPad);
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
