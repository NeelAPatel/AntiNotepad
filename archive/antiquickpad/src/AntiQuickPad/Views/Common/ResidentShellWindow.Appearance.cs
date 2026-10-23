using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Controls.Primitives;
using AntiQuickPad.Carousel;
using AntiQuickPad.Editor;
using AntiQuickPad.ViewModels;

namespace AntiQuickPad.Views.Common;

public sealed partial class ResidentShellWindow
{
    private Quiet.QuietBarReveal? _barReveal;
    private string _shellId = "quiet";
    private readonly ThemeService _theme;
    private ShellPalette _palette = null!;
    private ShellLayoutContext _layout = null!;
    private Grid _root = null!;
    private SettingsWindow? _settings;
    private Core.Preferences? _appliedPreferences;
    private Look CurrentLook => _palette?.Look ?? Look.Default;
    private Tokens.DesignTokens T => Tokens.DesignTokens.Quiet;
    internal string ShellId => _shellId;
    internal ThemeService ThemeService => _theme;
    internal ICommand Command(string id) => _controller.Command(id);
    internal void SwitchShell(string id) => _controller.SwitchShell(id);
    private void SetTheme(string preference) => _controller.SetTheme(preference);
    private bool SettingsOpen => _settings is { IsVisible: true };
    private void RebuildAppearance()
    {
        _barReveal?.Dispose();
        var hadEditor = _editor is not null;
        var caret = hadEditor ? _editor!.CaretOffset : _vm.Current.CaretOffset;
        var text = hadEditor ? _editor!.Text : _vm.Current.Text;
        var scroll = hadEditor ? _editor!.TextArea.TextView.ScrollOffset : default;
        var document = hadEditor ? _editor!.Document : null;
        _carousel?.Cancel(); _binding = true;
        if (hadEditor) EditorFactory.Release(_editor!);
        _appliedPreferences = _vm.Preferences;
        _palette = ShellPalette.Get(_shellId, _theme.IsDark, _vm.Preferences);
        var look = _palette.Look with { ModernEditor = true, ActiveLine = _palette.ActiveLine };
        _palette = _palette with { Look = look }; _binding = true;
        _editor = EditorFactory.Create(look, _vm, document);
        Avalonia.Automation.AutomationProperties.SetName(_editor, "Note text");
        if (!hadEditor) _editor.Text = text;
        _editor.CaretOffset = Math.Clamp(caret, 0, text.Length); _binding = false;
        _controller.AttachEditor();
        _carousel = new CarouselController(_editor, look, _vm, _controller.ScrollFor, ShowIndicator,
            null, null);
        _carousel.MotionChanged += RefreshHint;
        _layout = new ShellLayoutContext(this, _palette, _carousel.View);
        Background = Brushes.Transparent;
        ApplyMaterial(look);
        Foreground = _layout.Ink; FontFamily = new FontFamily(look.InterfaceFont);
        _root = (Grid)Quiet.QuietLayout.Build(_layout);
        _frame.Child = _root; Content = _frame;
        _barReveal = new Quiet.QuietBarReveal(_root, () => SettingsOpen, _layout.Bars) { Always = _vm.Preferences.Bars == "always" };
        _pin.IsChecked = _vm.Pinned;
        if (!hadEditor) _pin.IsCheckedChanged += (_, _) => _vm.Pinned = _pin.IsChecked == true;
        ApplyScrollbarStyle(); UpdateStatus();
        _settings?.Rebuild(_palette);
        // Never pull focus back from an open settings window.
        if (hadEditor) { UpdateLayout(); EditorViewport.Restore(_editor, scroll); if (!SettingsOpen) FocusEditor(); }
    }
    private void ApplyMaterial(Look look)
    {
        TransparencyLevelHint = look.Material switch
        {
            "acrylic" => [WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Mica, WindowTransparencyLevel.None],
            "mica" => [WindowTransparencyLevel.Mica, WindowTransparencyLevel.None],
            _ => [WindowTransparencyLevel.None]
        };
        TransparencyBackgroundFallback = new SolidColorBrush(_palette.Canvas);
    }
    // M11b-settings-window: material, tint and bars apply in place; font, marker and strike need a rebuild.
    private void ApplyPreferencesNow()
    {
        var before = _appliedPreferences; var now = _vm.Preferences;
        if (before is null || before.FontSize != now.FontSize || before.MarkerOpacity != now.MarkerOpacity || before.StrikeEm != now.StrikeEm)
        { RebuildAppearance(); return; }
        _appliedPreferences = now;
        var fresh = ShellPalette.Get(_shellId, _theme.IsDark, now);
        var look = CurrentLook with { Material = fresh.Look.Material, TintAlpha = fresh.Look.TintAlpha };
        _palette = _palette with { Look = look };
        ApplyMaterial(look);
        if (_layout.Tint is { } tint) tint.Background = new SolidColorBrush(_palette.Canvas, look.TintAlpha);
        if (_barReveal is not null) _barReveal.Always = now.Bars == "always";
        if (_settings is not null) { _settings.Rebuild(_palette); _settings.ApplyGlass(_palette); }
    }
    private SettingsWindow.State SettingsState()
    {
        var p = _vm.Preferences; var t = T;
        return new(_theme.Preference, p.Material ?? t.Material, p.Tint ?? t.TintAlpha, p.Bars ?? "hover", _controller.HotkeyLabel,
            _controller.HotkeyError, _controller.HotkeyFromEnvironment, p.StartWithWindows, p.FontSize ?? t.BodySize,
            p.MarkerOpacity ?? 1, p.StrikeEm ?? t.StrikeEm);
    }
    private SettingsWindow EnsureSettings()
    {
        if (_settings is not null) return _settings;
        _settings = new SettingsWindow(_palette, Command, SettingsState);
        if (Shell.HiddenWindows.Enabled) Shell.HiddenWindows.Register(_settings);
        _settings.Closing += (_, e) => { if (!_quitting) { e.Cancel = true; _settings.Hide(); } };
        return _settings;
    }
    private void OpenSettingsWindow()
    {
        var settings = EnsureSettings();
        settings.Rebuild(_palette);
        settings.Topmost = Topmost;
        settings.PlaceBeside(this);
        if (!settings.IsVisible) settings.Show(this);
        // F37-hidden-selftest-windows: hidden test windows are never activated.
        if (!Shell.HiddenWindows.Enabled) settings.Activate();
    }
    private void ApplyScrollbarStyle()
    {
        Styles.Clear();
        var bar = new Style(x => x.OfType<ScrollBar>());
        bar.Setters.Add(new Setter(ScrollBar.WidthProperty, 6d)); bar.Setters.Add(new Setter(ScrollBar.OpacityProperty, .55));
        bar.Setters.Add(new Setter(ScrollBar.BackgroundProperty, Brushes.Transparent)); Styles.Add(bar);
        var thumb = new Style(x => x.OfType<ScrollBar>().Descendant().OfType<Thumb>());
        thumb.Setters.Add(new Setter(Thumb.BackgroundProperty, _layout.Muted)); thumb.Setters.Add(new Setter(Thumb.MinHeightProperty, 28d)); Styles.Add(thumb);
    }
    private void RefreshHint()
    {
        if (_layout is not null) _layout.Hint.IsVisible = _vm.IsCurrentEmpty && !_carousel.IsBusy;
    }
    private void UpdateShellStatus()
    {
        if (_layout is null) return;
        _layout.Count.Text = _vm.PositionText;
        _layout.Count.FontSize = T.StatusSize; _layout.Count.Foreground = _layout.Muted;
        _layout.Save.Text = _vm.SaveLineText; _layout.Save.FontSize = T.StatusSize;
        _layout.Save.Foreground = _vm.SaveLineFailed ? new SolidColorBrush(CurrentLook.Accent) : _layout.Muted;
        RefreshHint();
        if (_layout.PreviousPeek is { } previous) previous.IsVisible = _layout.HasPrevious;
        if (_layout.NextPeek is { } next) next.IsVisible = _layout.HasNext;
        _layout.Dots.Children.Clear();
        foreach (var projection in _vm.PagerDots)
        {
            var i = projection.Index; var selected = projection.IsCurrent;
            var ink = selected ? T.DotSelected : T.Dot;
            var dot = new Button { Width = ink + T.DotSlot.Width - T.Dot, Height = T.DotSlot.Height, Padding = new Thickness(0), BorderThickness = new Thickness(0),
                Background = Brushes.Transparent, Content = new Border { Width = ink, Height = T.Dot, CornerRadius = new CornerRadius(T.DotRadius),
                    Background = selected ? new SolidColorBrush(CurrentLook.Accent) : _layout.Muted, Opacity = selected ? 1 : T.DotIdleOpacity },
                Command = Command("note.select"), CommandParameter = i };
            ShellLayoutContext.FocusRing(dot, CurrentLook, T);
            Avalonia.Automation.AutomationProperties.SetName(dot, $"Note {i + 1} of {_vm.Notes.Count}");
            _layout.Dots.Children.Add(dot);
        }
        if (_layout.Buttons.TryGetValue("Pin", out var pin))
        {
            // F57-pin-state-visual: the glyph itself carries the pinned state, so hover/press never hide it.
            pin.Background = _vm.Pinned ? new SolidColorBrush(CurrentLook.CodePanel) : Brushes.Transparent;
            if (pin.Content is TextBlock glyph) { glyph.Text = _vm.Pinned ? "" : ""; glyph.Foreground = _vm.Pinned ? new SolidColorBrush(CurrentLook.Accent) : _layout.Ink; }
            // The accessible name stays "Pin" (conformance route); the tooltip says the state.
            ToolTip.SetTip(pin, _vm.Pinned ? "Unpin (stays on top now)" : "Pin (keep on top)");
        }
    }
}



