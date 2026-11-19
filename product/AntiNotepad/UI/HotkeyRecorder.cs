// Records Settings shortcuts as key presses while leaving registration and persistence with the host.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AntiNotepad.Theme;
using Shape = AntiNotepad.Theme.Shape;

namespace AntiNotepad.UI;

/// <summary>Shows the current shortcut and captures a replacement for explicit Save or Cancel.</summary>
internal sealed class HotkeyRecorder
{
    private readonly ContentControl _host = new() { Focusable = true };
    internal Control View => _host;
    private readonly Palette _palette;
    private readonly Func<string> _current;
    private readonly Func<string, bool> _apply;
    private readonly Func<Key, KeyModifiers, string?> _gestureText;
    private readonly Func<string, string?>? _validate;
    private readonly bool _allowEmpty;
    private readonly Func<string, Action, bool, Button> _button;
    private readonly StackPanel _caps = new() { Orientation = Orientation.Horizontal, Spacing = ChromeTokens.BadgeSpacing, VerticalAlignment = VerticalAlignment.Center };
    private TextBlock _hint;
    private Button? _save;
    private bool _recording;
    private string? _gesture;

    /// <summary>Connects the recorder to host-owned settings and the shared Settings button factory.</summary>
    public HotkeyRecorder(Palette palette, Func<string> current, Func<string, bool> apply, bool allowEmpty,
        Func<string, Action, bool, Button> button,
        Func<Key, KeyModifiers, string?>? gestureText = null,
        Func<string, string?>? validate = null)
    {
        _palette = palette;
        _current = current;
        _apply = apply;
        _gestureText = gestureText ?? GestureText;
        _validate = validate;
        _allowEmpty = allowEmpty;
        _button = button;
        _host.Tag = this;
        _hint = Kit.Text("", palette.Muted, Typography.Caption);
        _host.AddHandler(InputElement.KeyDownEvent, RecordKey, RoutingStrategies.Tunnel);
        _host.AddHandler(InputElement.KeyUpEvent, (_, e) => { if (_recording) e.Handled = true; }, RoutingStrategies.Tunnel);
        _host.LostFocus += (_, _) =>
        {
            // Focus can move to an action button before its click is raised.
            Dispatcher.UIThread.Post(() => { if (_recording && !_host.IsKeyboardFocusWithin) ShowCurrent(); });
        };
        ShowCurrent();
    }

    /// <summary>Maps supported modified main keys to the registrar's canonical stored text.</summary>
    internal static string? GestureText(Key key, KeyModifiers modifiers)
    {
        if (modifiers.HasFlag(KeyModifiers.Meta)) return null;
        var main = key switch
        {
            >= Key.A and <= Key.Z => key.ToString(),
            >= Key.D0 and <= Key.D9 => ((int)key - (int)Key.D0).ToString(),
            >= Key.F1 and <= Key.F24 => key.ToString(),
            Key.Space => "Space",
            Key.Escape => "Esc",
            Key.Tab => "Tab",
            Key.Enter => "Enter",
            Key.Insert => "Insert",
            Key.Delete => "Delete",
            _ => null,
        };
        var names = ModifierNames(modifiers);
        return main is null || names.Count == 0 ? null : string.Join('+', names.Append(main));
    }

    /// <summary>Recognizes the two unmodified recorder commands before mapping shortcut keys.</summary>
    internal static bool IsCommand(Key key, KeyModifiers modifiers) =>
        modifiers == KeyModifiers.None && key is Key.Escape or Key.Enter;

    /// <summary>Lets Settings defer its Escape command to an active recorder on the event route.</summary>
    internal static bool IsRecordingSource(object? source) => source is Visual visual
        && visual.GetSelfAndVisualAncestors().OfType<Control>()
            .SelectMany(control => control.GetSelfAndLogicalAncestors()).OfType<ContentControl>()
            .Any(control => control.Tag is HotkeyRecorder recorder && recorder._recording);

    private static bool IsModifier(Key key) => key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
        or Key.LeftShift or Key.RightShift;

    /// <summary>Uses the stored modifier order for both live caps and saved gestures.</summary>
    private static List<string> ModifierNames(KeyModifiers modifiers)
    {
        var names = new List<string>();
        if (modifiers.HasFlag(KeyModifiers.Control)) names.Add("Ctrl");
        if (modifiers.HasFlag(KeyModifiers.Alt)) names.Add("Alt");
        if (modifiers.HasFlag(KeyModifiers.Shift)) names.Add("Shift");
        return names;
    }

    /// <summary>Restores the host's current binding without applying the captured candidate.</summary>
    private void ShowCurrent()
    {
        _recording = false;
        _gesture = null;
        _caps.Children.Clear();
        var label = _current();
        if (label is "off" or "") _caps.Children.Add(Kit.Text("None", _palette.Muted, Typography.Body));
        else AddCaps(ShortcutMap.LabelParts(label), false);
        var row = NewRow();
        row.Children.Remove(_caps);
        foreach (var cap in _caps.Children.ToArray())
        {
            _caps.Children.Remove(cap);
            row.Children.Add(cap);
        }
        row.Children.Add(MakeButton("Change", Begin));
        _host.Content = row;
    }

    /// <summary>Opens an empty recording session and focuses it after it is attached.</summary>
    private void Begin()
    {
        _recording = true;
        _gesture = null;
        _caps.Children.Clear();
        var waiting = new StackPanel { Orientation = Orientation.Horizontal, Spacing = ChromeTokens.MetaSpacing };
        waiting.Children.Add(new Ellipse
        {
            Width = ChromeTokens.PagerDotWidth,
            Height = ChromeTokens.PagerDotHeight,
            Fill = new SolidColorBrush(_palette.Accent),
            VerticalAlignment = VerticalAlignment.Center,
        });
        waiting.Children.Add(Kit.Text("Press the new shortcut", _palette.Ink, Typography.Small));
        _caps.Children.Add(new Border
        {
            BorderBrush = new SolidColorBrush(_palette.Accent),
            BorderThickness = new Thickness(ChromeTokens.PillBorderThickness),
            CornerRadius = new CornerRadius(Shape.Key),
            Padding = new Thickness(ChromeTokens.MetaSpacing, ChromeTokens.BadgeBottomPadding),
            Child = waiting,
        });
        var row = NewRow();
        _save = MakeButton("Save", () => { if (_gesture is { } text) Apply(text); }, true);
        _save.IsEnabled = false;
        row.Children.Add(_save);
        if (_allowEmpty) row.Children.Add(MakeButton("Clear", () => Apply("")));
        row.Children.Add(MakeButton("Cancel", ShowCurrent));
        _hint = Kit.Text("", _palette.Muted, Typography.Caption);
        Hint();
        _host.Content = new StackPanel { Spacing = ChromeTokens.BadgeSpacing, Children = { row, _hint } };
        Dispatcher.UIThread.Post(() => { if (_recording) _host.Focus(); });
    }

    /// <summary>Moves the caps into the current action row without retaining an old parent.</summary>
    private StackPanel NewRow()
    {
        if (_caps.Parent is Panel parent) parent.Children.Remove(_caps);
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ChromeTokens.BadgeSpacing,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { _caps },
        };
    }

    /// <summary>Creates a Settings action with the existing separation from its key caps.</summary>
    private Button MakeButton(string text, Action action, bool primary = false)
    {
        var button = _button(text, action, primary);
        button.Margin = new Thickness(ChromeTokens.MetaSpacing, 0, 0, 0);
        return button;
    }

    /// <summary>Captures key downs before focus navigation; releases preserve the last displayed candidate.</summary>
    private void RecordKey(object? sender, KeyEventArgs e)
    {
        if (!_recording) return;
        e.Handled = true;
        if (e.Key is Key.LWin or Key.RWin || e.KeyModifiers.HasFlag(KeyModifiers.Meta))
        {
            Hint("The Windows key can't be used. Use Ctrl, Alt or Shift.");
            return;
        }
        if (IsCommand(e.Key, e.KeyModifiers))
        {
            if (e.Key == Key.Escape) ShowCurrent();
            else if (_gesture is { } text) Apply(text);
            return;
        }
        var gesture = _gestureText(e.Key, e.KeyModifiers);
        if (gesture is null && !IsModifier(e.Key))
        {
            Hint("That key can't be a shortcut. Use A-Z, 0-9, F1-F24, Space, Tab, Enter, Insert or Delete.");
            return;
        }
        var error = gesture is null ? null : _validate?.Invoke(gesture);
        _gesture = gesture;
        _save!.IsEnabled = gesture is not null && error is null;
        _caps.Children.Clear();
        AddCaps(gesture is null ? ModifierNames(e.KeyModifiers).ToArray() : ShortcutMap.LabelParts(gesture), true);
        if (gesture is null) _caps.Children.Add(GhostCap());
        Hint(error);
    }

    /// <summary>Applies only an explicit Save or Clear, retaining the session when Windows refuses.</summary>
    private void Apply(string text)
    {
        if (_apply(text)) ShowCurrent();
        else Hint($"Windows won't give {AppInfo.Name} {text}; another app may use it.");
    }

    /// <summary>Shows cancellation guidance or a capture or registration error.</summary>
    private void Hint(string? error = null)
    {
        _hint.Text = error ?? "Esc or Cancel cancels" + (_allowEmpty ? " · Clear removes the shortcut" : "");
        _hint.Foreground = new SolidColorBrush(error is null ? _palette.Muted : _palette.Error);
        _hint.HorizontalAlignment = HorizontalAlignment.Right;
    }

    /// <summary>Uses the existing key-cap geometry with an accent border during capture.</summary>
    private void AddCaps(IEnumerable<string> keys, bool live)
    {
        foreach (var key in keys)
            _caps.Children.Add(new Border
            {
                BorderBrush = new SolidColorBrush(live ? _palette.Accent : _palette.Ink, live ? 1 : Paint.Hover(_palette.Dark)),
                BorderThickness = new Thickness(ChromeTokens.PillBorderThickness, ChromeTokens.PillBorderThickness,
                    ChromeTokens.PillBorderThickness, ChromeTokens.BadgeBottomPadding),
                CornerRadius = new CornerRadius(Shape.Key),
                Padding = new Thickness(ChromeTokens.MetaSpacing, ChromeTokens.BadgeBottomPadding),
                Background = new SolidColorBrush(_palette.Surface),
                Child = Kit.Text(key, _palette.Ink, Typography.Small),
            });
    }

    /// <summary>Draws a dashed placeholder until a supported main key is pressed.</summary>
    private Control GhostCap() => new Panel
    {
        Children =
        {
            new Rectangle
            {
                Stroke = new SolidColorBrush(_palette.Faint), StrokeThickness = ChromeTokens.PillBorderThickness,
                StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { Shape.Key, Shape.Key },
                RadiusX = Shape.Key, RadiusY = Shape.Key,
            },
            new Border
            {
                Padding = new Thickness(ChromeTokens.MetaSpacing, ChromeTokens.BadgeBottomPadding),
                Child = Kit.Text("key", _palette.Faint, Typography.Small),
            },
        },
    };
}
