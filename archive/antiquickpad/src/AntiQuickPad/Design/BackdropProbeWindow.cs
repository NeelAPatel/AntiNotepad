using AntiQuickPad.Editor;
using AntiQuickPad.Views.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Document;

namespace AntiQuickPad.Design;

internal sealed record BackdropProbeOptions(string Material, string Theme, int Tint, string? Report, int? CloseAfterMs)
{
    public const string Usage = "Usage: --probe-backdrop <none|mica|acrylic|blur> [--theme light|dark] [--tint 0-100] [--report path] [--close-after-ms ms]";
    public static BackdropProbeOptions Parse(string[] args)
    {
        if (args.Length < 2 || !new[] { "none", "mica", "acrylic", "blur" }.Contains(args[1])) throw new ArgumentException(Usage);
        var theme = "dark"; var tint = 60; string? report = null; int? close = null;
        for (var i = 2; i < args.Length; i += 2)
        {
            if (i + 1 >= args.Length) throw new ArgumentException(Usage);
            var value = args[i + 1];
            switch (args[i])
            {
                case "--theme" when value is "light" or "dark": theme = value; break;
                case "--tint" when int.TryParse(value, out var alpha) && alpha is >= 0 and <= 100: tint = alpha; break;
                case "--report": report = Path.GetFullPath(value); break;
                case "--close-after-ms" when int.TryParse(value, out var ms) && ms >= 0: close = ms; break;
                default: throw new ArgumentException(Usage);
            }
        }
        return new(args[1], theme, tint, report, close);
    }
}

// Independent, in-memory surface: no resident controller, settings, notes, tray or singleton.
internal sealed class BackdropProbeWindow : Window
{
    private static readonly int[] TintSteps = [0, 15, 30, 45, 60, 75, 90, 100];
    private const string Sample = """
        # A little room to think

        A quiet place for the thoughts that arrive between everything else. Try typing here,
        then move this note over a bright window and a dark part of the wallpaper.

        ## Today, at an easy pace

        - Capture the idea before it disappears
        - Leave enough space to see what matters
        - Compare the text against light and dark content

        - [ ] Check the inactive window appearance
        - [x] Make room for a fresh thought

        ~~Keep every thought in my head~~ Write it down and carry on.

        ```python
        def small_steps(ideas):
            # Keep the useful ones close.
            for idea in ideas:
                print(f"Next: {idea}")
        ```
        """;
    private readonly BackdropProbeOptions _options;
    private readonly Grid _root = new() { Background = Brushes.Transparent, ClipToBounds = true };
    private readonly Border _tint = new() { IsHitTestVisible = false };
    private readonly TextBlock _hud = new() { FontSize = 11, Margin = new Thickness(20, 0, 16, 14),
        HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Bottom,
        IsHitTestVisible = false };
    private readonly TextDocument _document = new(Sample);
    private TextEditor? _editor;
    private string _material;
    private string _theme;
    private int _alpha;
    private StreamWriter? _report;
    private bool _closed;

    public BackdropProbeWindow(BackdropProbeOptions options)
    {
        _options = options; _material = options.Material; _theme = options.Theme; _alpha = options.Tint;
        Title = "AntiQuickPad · F25a-resident-backdrop-probe";
        Width = 960; Height = 800; MinWidth = 600; MinHeight = 480;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowDecorations = WindowDecorations.BorderOnly;
        ExtendClientAreaToDecorationsHint = true; ExtendClientAreaTitleBarHeightHint = 58;
        CanResize = true; Background = Brushes.Transparent;
        // This is deliberately behind the root/editor, the only full-window painted layer.
        _root.Children.Add(_tint);
        Content = _root;
        ApplyTheme();
        var drag = new Border { Height = 58, VerticalAlignment = VerticalAlignment.Top, Background = Brushes.Transparent };
        WindowDecorationProperties.SetElementRole(drag, WindowDecorationsElementRole.TitleBar);
        _root.Children.Add(drag); _root.Children.Add(_hud);
        ApplyMaterial(); RefreshHud();
        if (options.Report is { } path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            _report = new StreamWriter(path, false) { AutoFlush = true };
        }
        Write($"OS={System.Runtime.InteropServices.RuntimeInformation.OSDescription}; Avalonia={typeof(Window).Assembly.GetName().Version}");
        Record("start");
        PropertyChanged += (_, e) =>
        {
            if (e.Property == ActualTransparencyLevelProperty && !_closed) { RefreshHud(); Record("actual-level-changed"); }
        };
        AddHandler(KeyDownEvent, OnProbeKey, RoutingStrategies.Tunnel, true);
        Opened += async (_, _) =>
        {
            ClearEditorBackgrounds(); _editor!.Focus();
            await Task.Delay(800);
            if (_closed) return;
            Record("settled");
            if (_options.CloseAfterMs is { } ms)
            {
                await Task.Delay(ms);
                if (!_closed) Close();
            }
        };
        Closed += (_, _) =>
        {
            _closed = true; Record("final");
            _report?.Dispose(); _report = null;
            if (_editor is { } editor) EditorFactory.Release(editor);
        };
    }

    private void ApplyTheme()
    {
        RequestedThemeVariant = _theme == "dark" ? ThemeVariant.Dark : ThemeVariant.Light;
        var palette = ShellPalette.Get("quiet", _theme == "dark");
        var look = palette.Look with { ModernEditor = true, ActiveLine = palette.ActiveLine };
        var caret = _editor?.CaretOffset ?? 0;
        var scroll = _editor?.TextArea.TextView.ScrollOffset ?? default;
        var hadFocus = _editor?.IsKeyboardFocusWithin == true;
        if (_editor is { } previous) { _root.Children.Remove(previous); EditorFactory.Release(previous); }
        _editor = EditorFactory.Create(look, null, _document);
        _editor.Margin = new Thickness(90, 90, 90, 90);
        _editor.CaretOffset = Math.Min(caret, _document.TextLength);
        _root.Children.Insert(1, _editor);
        _hud.Foreground = new SolidColorBrush(look.Foreground);
        FontFamily = new FontFamily(look.InterfaceFont);
        TransparencyBackgroundFallback = new SolidColorBrush(palette.Canvas);
        ApplyTint(); ClearEditorBackgrounds();
        // Templates create the actual scroll viewer after the editor joins the visual tree.
        _editor.Loaded += (_, _) => ClearEditorBackgrounds();
        Dispatcher.UIThread.Post(() =>
        {
            if (_closed) return;
            ClearEditorBackgrounds();
            EditorViewport.Restore(_editor, scroll);
            if (hadFocus) _editor.Focus();
        }, DispatcherPriority.Loaded);
    }

    private void ClearEditorBackgrounds()
    {
        if (_editor is null) return;
        _editor.Background = Brushes.Transparent;
        _editor.TextArea.Background = Brushes.Transparent;
        // TextView is a drawing Control with no Background property; its container is cleared below.
        foreach (var visual in _editor.GetVisualDescendants())
        {
            // Local values override theme/template backgrounds on this instance only.
            if (visual is Panel panel) panel.Background = Brushes.Transparent;
            if (visual is Border border) border.Background = Brushes.Transparent;
            if (visual is TemplatedControl control) control.Background = Brushes.Transparent;
            if (visual is Avalonia.Controls.Presenters.ContentPresenter presenter) presenter.Background = Brushes.Transparent;
        }
    }

    private void ApplyTint()
    {
        var canvas = ShellPalette.Get("quiet", _theme == "dark").Canvas;
        _tint.Background = new SolidColorBrush(Color.FromArgb((byte)Math.Round(_alpha * 255d / 100), canvas.R, canvas.G, canvas.B));
    }
    private void ApplyMaterial() => TransparencyLevelHint = new[] { _material switch
    {
        "mica" => WindowTransparencyLevel.Mica, "acrylic" => WindowTransparencyLevel.AcrylicBlur,
        "blur" => WindowTransparencyLevel.Blur, _ => WindowTransparencyLevel.None
    } };
    private void OnProbeKey(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { e.Handled = true; Close(); return; }
        if (e.KeyModifiers != (KeyModifiers.Control | KeyModifiers.Alt)) return;
        var change = "";
        switch (e.Key)
        {
            case Key.D1: _material = "none"; change = "material"; break;
            case Key.D2: _material = "mica"; change = "material"; break;
            case Key.D3: _material = "acrylic"; change = "material"; break;
            case Key.D4: _material = "blur"; change = "material"; break;
            case Key.T: _theme = _theme == "dark" ? "light" : "dark"; ApplyTheme(); change = "theme"; break;
            case Key.Down: _alpha = TintSteps.LastOrDefault(x => x < _alpha); ApplyTint(); change = "tint"; break;
            case Key.Up: _alpha = TintSteps.FirstOrDefault(x => x > _alpha, 100); ApplyTint(); change = "tint"; break;
            case Key.H: _hud.IsVisible = !_hud.IsVisible; change = "hud"; break;
            default: return;
        }
        e.Handled = true;
        if (change == "material") ApplyMaterial();
        RefreshHud(); Record(change);
    }
    private void RefreshHud() => _hud.Text = $"{_material} -> {ActualTransparencyLevel}  ·  {_theme}  ·  tint {_alpha}%\n" +
        "Ctrl+Alt: 1 none / 2 mica / 3 acrylic / 4 blur · T theme · ↓/↑ tint · H HUD · Esc close\n" +
        "Check: bright + dark wallpaper behind, click another window (inactive look), move, resize, text sharpness.";
    private void Record(string kind) => Write($"{DateTimeOffset.UtcNow:O} {kind}: material={_material}; requested={TransparencyLevelHint[0]}; actual={ActualTransparencyLevel}; theme={_theme}; tint={_alpha}; hud={_hud.IsVisible}");
    private void Write(string line) { Console.WriteLine(line); _report?.WriteLine(line); }
}
