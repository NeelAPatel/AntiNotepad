using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AntiNotepad;
using AntiNotepad.Editor;
using AntiNotepad.Model;
using AntiNotepad.Motion;
using AntiNotepad.Platform;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

internal sealed partial class PadWindow
{
    private readonly Border _tint = new() { IsHitTestVisible = false };
    private readonly LayoutTransformControl _scaler = new();
    private readonly Grid _content = new() { RowDefinitions = new RowDefinitions($"{ChromeTokens.DockedRowHeight},*") };
    private readonly Grid _top = new() { ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto") };
    /// <summary>Which copy this is; fades with the icons.</summary>
    private readonly StackPanel _badges = new() { Orientation = Orientation.Horizontal, Spacing = ChromeTokens.BadgeSpacing, Margin = new Thickness(0, 0, ChromeTokens.BadgeRightInset, 0), VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false };
    private TextBlock? _saved;
    private readonly StackPanel _meta = new() { Orientation = Orientation.Horizontal, Spacing = ChromeTokens.MetaSpacing, Margin = new Thickness(ChromeTokens.FirstItemInset, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false };
    private readonly StackPanel _icons = new() { Orientation = Orientation.Horizontal, Spacing = ChromeTokens.IconSpacing, Margin = new Thickness(0, 0, ChromeTokens.BarEdgeInset, 0), VerticalAlignment = VerticalAlignment.Center };
    private readonly Grid _panes = new() { ColumnDefinitions = new ColumnDefinitions("*,0,0") };
    private readonly Border _divider = new() { Width = ChromeTokens.DividerWidth, Margin = new Thickness(0, ChromeTokens.DividerTopMargin, 0, ChromeTokens.DividerBottomMargin) };
    private readonly Border _pill = new() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 0, ChromeTokens.PillBottomInset), CornerRadius = new CornerRadius(ChromeTokens.PillCornerRadius), Padding = new Thickness(ChromeTokens.PillInset) };
    private readonly Border _toast = new() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(16, 0, 16, ChromeTokens.ToastBottomMargin), CornerRadius = new CornerRadius(10), Padding = new Thickness(12, 7), IsVisible = false, MaxWidth = 380 };
    private readonly Panel _overlayHost = new() { IsVisible = false };
    // The top bar sits on the sampled Canvas blur when the pointer reaches the edge.
    private readonly Panel _topLayer = new() { VerticalAlignment = VerticalAlignment.Top, Height = ChromeTokens.TopBackdrop.ClearStop, IsVisible = false };
    private readonly Panel _bottomLayer = new() { VerticalAlignment = VerticalAlignment.Bottom, Height = ChromeTokens.PagerStripHeight, IsVisible = false };
    private string? _barsMode;
    private bool _topShown, _bottomShown;
    private double _pointerY = double.NaN;
    // The float style's pager lives in its own popup just below the window, centred under it.
    private readonly Avalonia.Controls.Primitives.Popup _floatPager = new()
    {
        Placement = PlacementMode.Bottom, VerticalOffset = ChromeTokens.FloatGap, IsLightDismissEnabled = false,
        ShouldUseOverlayLayer = false, Topmost = false, PlacementConstraintAdjustment = PopupPositionerConstraintAdjustment.None,
    };
    private readonly DispatcherTimer _barsSleep = new() { Interval = TimeSpan.FromMilliseconds(450) };
    private readonly DispatcherTimer _chromeSleep = new() { Interval = TimeSpan.FromMilliseconds(1600) };
    private readonly DispatcherTimer _toastTimer = new() { Interval = TimeSpan.FromMilliseconds(2600) };
    private bool _chromeAwake = true;
    private Anim.Handle? _chromeAnim;
    private Action? _toastAction;
    /// <summary>Ctrl+Z undoes the toast's action (a delete) until the user types something.</summary>
    private bool _undoArmed;
    private Control? _saveFailureOverlay;
    private Border? _saveFailureCard;
    private TextBlock? _saveFailureReason;
    private Button? _saveFailureRetryButton, _saveFailureQuitButton;
    private Action? _saveFailureRetryAction;

    private void BuildLayout()
    {
        AddHandler(KeyDownEvent, OnSaveFailureKeyDown, RoutingStrategies.Tunnel);
        AddHandler(PointerPressedEvent, OnSaveFailurePointerPressed, RoutingStrategies.Tunnel);
        var drag = new Border { Background = Brushes.Transparent };
        WindowDecorationProperties.SetElementRole(drag, WindowDecorationsElementRole.TitleBar);
        Grid.SetColumnSpan(drag, 3);
        _top.Children.Add(drag);
        _top.Children.Add(_meta);
        Grid.SetColumn(_badges, 1);
        _top.Children.Add(_badges);
        Grid.SetColumn(_icons, 2);
        _top.Children.Add(_icons);
        _content.Children.Add(_top);

        _left = NewPane();
        _right = NewPane();
        _active = _left;
        _panes.Children.Add(_left);
        Grid.SetColumn(_divider, 1); _panes.Children.Add(_divider);
        Grid.SetColumn(_right, 2); _panes.Children.Add(_right);
        Grid.SetRow(_panes, 1);
        _content.Children.Add(_panes);

        Grid.SetRow(_overlayHost, 1);
        _content.Children.Add(_overlayHost);

        var root = new Grid();
        root.Children.Add(_tint);
        root.Children.Add(_content);
        InitializeTopBlurVisuals();
        root.Children.Add(_topLayer);
        root.Children.Add(_bottomLayer);
        root.Children.Add(_toast);
        root.Children.Add(_pill);
        _topLayer.RenderTransform = new TranslateTransform();
        root.Children.Add(_floatPager);
        _floatPager.PlacementTarget = root;
        _bottomLayer.RenderTransform = new TranslateTransform();
        _barsSleep.Tick += (_, _) => BarsSleepTick();
        _pill.PointerExited += (_, _) => { if (Sliding) { _barsSleep.Stop(); _barsSleep.Start(); } };
        // UI scale: one layout transform over everything, so chrome, text and hit targets scale together.
        _scaler.Child = root;
        Content = _scaler;

        _pill.PointerEntered += (_, _) => WakeChrome(hold: true);
        _top.PointerEntered += (_, _) => WakeChrome(hold: true);
        _chromeSleep.Tick += (_, _) => { _chromeSleep.Stop(); SleepChrome(soon: false); };
        _toastTimer.Tick += (_, _) => { _toastTimer.Stop(); HideToast(); };
        InitializeTopBlur();
    }

    private PadView NewPane()
    {
        var pane = new PadView(_palette, Settings, _timers);
        pane.Edited += () => { _undoArmed = false; Edited(); };
        pane.ModeChanged += UpdateChrome;
        pane.Toast += t => Toast(t);
        pane.Copy += text => _ = CopyText(text);
        pane.ShowMenu = (anchor, entries) => PadMenu.Show(anchor, entries, _palette, Settings);
        pane.AppMenu = () =>
        [
            new("New note", "Ctrl+N", MenuGlyphs.NewNote, NewNote),
            new("Search notes", "Ctrl+F", MenuGlyphs.Search, () => OpenSearch()),
            new(Settings.Split ? "Close split view" : "Split view", "Ctrl+OemBackslash", MenuGlyphs.Split, ToggleSplit),
            new(Settings.Pinned ? "Unpin" : "Keep on top", "Ctrl+T", MenuGlyphs.Pin, TogglePin),
            MenuEntry.Separator,
            new("Settings", "Ctrl+OemComma", MenuGlyphs.Settings, OpenSettings),
            new($"Hide {AppInfo.Name}", "Escape", null, HideResident),
        ];
        pane.FocusedPane += () => { if (_active != pane) { _active = pane; UpdateChrome(); } };
        return pane;
    }

    /// <summary>Recreates chrome controls (palette changes), then refreshes their content.</summary>
    private void RebuildChrome()
    {
        var p = _palette;
        _icons.Children.Clear();
        // The repo launch route is the only copy with a badge.
        _badges.Children.Clear();
        if (Program.IsDev) _badges.Children.Add(Chip("dev", p.Number));
        _badges.IsVisible = Program.IsDev;
        _icons.Children.Add(Kit.IconButton(Glyph.Search, "Search notes (Ctrl+F)", p, () => OpenSearch()));
        _icons.Children.Add(Kit.IconButton(Glyph.Split, "Split view with a slot (Ctrl+\\)", p, ToggleSplit));
        // Pinned: the pin is filled with the accent, so the state reads at a glance.
        _icons.Children.Add(Kit.IconButton(Glyph.Pin, Settings.Pinned ? "Unpin (Ctrl+T)" : "Keep on top (Ctrl+T)", p, TogglePin,
            filled: Settings.Pinned, color: Settings.Pinned ? p.Accent : null));
        _icons.Children.Add(Kit.IconButton(Glyph.Settings, "Settings (Ctrl+,)", p, OpenSettings));
        _icons.Children.Add(Kit.IconButton(Glyph.Hide, "Hide (Esc)", p, HideResident));
        _divider.Background = new SolidColorBrush(p.Faint, ChromeTokens.DividerAlpha);
        _pill.Background = new SolidColorBrush(p.Surface);
        _pill.BorderBrush = new SolidColorBrush(p.Faint, ChromeTokens.PillBorderAlpha); _pill.BorderThickness = new Thickness(ChromeTokens.PillBorderThickness);
        _pill.BoxShadow = new BoxShadows(new BoxShadow { Blur = ChromeTokens.PillShadowBlur, OffsetY = ChromeTokens.PillShadowOffsetY, Color = Color.FromArgb(p.Dark ? ChromeTokens.PillShadowDarkAlpha : ChromeTokens.PillShadowLightAlpha, 0, 0, 0) });
        _toast.Background = new SolidColorBrush(p.Ink, .92);
        _toast.BoxShadow = _pill.BoxShadow;
        _overlayHost.Background = new SolidColorBrush(p.Canvas, Settings.Material == "solid" ? 1 : .55);
        UpdateChrome();
    }

    /// <summary>Refreshes metadata text, the pager and slot dots.</summary>
    private void UpdateChrome()
    {
        if (_left is null || _active?.Note is not { } note) return;
        var p = _palette;
        _meta.Children.Clear();
        if (note.Slot > 0) _meta.Children.Add(Chip($"slot {note.Slot}", p.Accent));
        var label = Modes.Label(_active.Mode, _active.CodeLanguage);
        if (label.Length > 0) _meta.Children.Add(Chip(label, p.Keyword));
        _meta.Children.Add(_saved = Kit.Text(SavedText(note), p.Muted, ChromeTokens.SavedLabelFontSize));
        if (_timers.AnyRunning && _active.Mode != NoteMode.Timer) _meta.Children.Add(Kit.Text("● timer running", p.Number, ChromeTokens.RunningTimerFontSize));
        BuildPill();
    }

    private static string SavedText(Note note) => "Saved " + Kit.Ago(note.Modified);

    /// <summary>Keeps "Saved 3m ago" true when the bar comes back after a while without edits.</summary>
    private void RefreshSaved()
    {
        if (_saved is not null && _active?.Note is { } note) _saved.Text = SavedText(note);
    }

    private Border Chip(string text, Color color) => new()
    {
        Background = new SolidColorBrush(color, _palette.Dark ? ChromeTokens.BadgeDarkFillAlpha : ChromeTokens.BadgeLightFillAlpha),
        CornerRadius = new CornerRadius(ChromeTokens.BadgeCornerRadius),
        Padding = new Thickness(ChromeTokens.BadgeHorizontalPadding, ChromeTokens.BadgeTopPadding, ChromeTokens.BadgeHorizontalPadding, ChromeTokens.BadgeBottomPadding),
        Height = ChromeTokens.BadgeHeight,
        Child = Kit.Text(text, color, ChromeTokens.BadgeFontSize, FontWeight.SemiBold), VerticalAlignment = VerticalAlignment.Center
    };

    private void BuildPill()
    {
        var p = _palette;
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = ChromeTokens.PagerRowSpacing, VerticalAlignment = VerticalAlignment.Center };
        var onStack = Data.ViewSlot == 0;
        row.Children.Add(Kit.IconButton(Glyph.Back, "Previous note (Ctrl+[)", p, () => Navigate(-1), radius: ChromeTokens.PagerArrowHoverRadius));
        var count = Kit.Text(onStack ? $"{Data.Current + 1} / {Data.Stack.Count}" : "stack", onStack ? p.Muted : p.Faint, ChromeTokens.PagerCountFontSize, FontWeight.SemiBold);
        // Fixed box, centred both ways, tabular digits: "5 / 5" sits in the middle and doesn't shift as numbers change.
        count.TextAlignment = TextAlignment.Center; count.HorizontalAlignment = HorizontalAlignment.Center; count.VerticalAlignment = VerticalAlignment.Center;
        count.FontFeatures = new FontFeatureCollection { new FontFeature { Tag = "tnum", Value = 1 } };
        var digits = Math.Max(Data.Stack.Count.ToString().Length, 1);
        var countButton = Kit.Plain(count, p, width: ChromeTokens.PagerCountBaseWidth + digits * ChromeTokens.PagerCountDigitWidth, height: ChromeTokens.BadgeHeight, radius: ChromeTokens.PagerCountHoverRadius);
        countButton.Click += (_, _) => { if (!onStack) ShowStack(); else OpenSearch(); };
        Avalonia.Automation.AutomationProperties.SetName(countButton, onStack ? $"Note {Data.Current + 1} of {Data.Stack.Count}" : "Back to the stack");
        row.Children.Add(countButton);
        row.Children.Add(Kit.IconButton(Glyph.Forward, "Next note · past the last one starts a new note (Ctrl+])", p, () => Navigate(1), radius: ChromeTokens.PagerArrowHoverRadius));
        row.Children.Add(new Border { Width = ChromeTokens.PagerSeparatorWidth, Height = ChromeTokens.PagerSeparatorHeight, Background = new SolidColorBrush(p.Faint, ChromeTokens.PagerSeparatorAlpha), Margin = new Thickness(ChromeTokens.PagerSeparatorMargin, 0) });
        for (var i = 1; i <= 9; i++)
        {
            var slot = i;
            var filled = !Data.Slots[i - 1].IsEmpty;
            var current = Data.ViewSlot == i || (Settings.Split && Settings.SplitSlot == i);
            var dot = new Border
            {
                Width = current ? ChromeTokens.PagerDotSelectedWidth : ChromeTokens.PagerDotWidth, Height = ChromeTokens.PagerDotHeight, CornerRadius = new CornerRadius(ChromeTokens.PagerDotRadius),
                Background = current ? new SolidColorBrush(p.Accent) : filled ? new SolidColorBrush(p.Muted) : Brushes.Transparent,
                BorderBrush = new SolidColorBrush(current ? p.Accent : p.Faint), BorderThickness = new Thickness(filled || current ? 0 : ChromeTokens.PagerDotBorderThickness),
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
            };
            var b = Kit.Plain(dot, p, current ? ChromeTokens.PagerDotSelectedButtonWidth : ChromeTokens.PagerDotButtonWidth, ChromeTokens.PagerDotButtonHeight, ChromeTokens.PagerDotHoverRadius);
            b.Click += (_, _) => OpenSlot(slot);
            var title = Data.Slots[i - 1].IsEmpty ? "empty" : Data.Slots[i - 1].Title;
            ToolTip.SetTip(b, $"Slot {i} · {title} (Ctrl+{i})");
            Avalonia.Automation.AutomationProperties.SetName(b, $"Slot {i}");
            row.Children.Add(b);
        }
        _pill.Child = row;
    }

    // ---------- chrome visibility: present when the mouse moves, gone while typing ----------

    // ---------- bar style ----------
    // docked: a top bar row and a pager below the text (they fade while typing).
    // overlay: both bars sit over the note and slide in when the pointer reaches their edge.
    // float: the top bar slides in the same way; the pager floats just below the window, out of the note entirely.

    private bool Sliding => Settings.Bars is "overlay" or "float";

    /// <summary>Moves the top bar and the pager to where the current style wants them.</summary>
    private void ApplyBars()
    {
        if (_barsMode == Settings.Bars) return;
        _barsMode = Settings.Bars;
        var root = (Panel)_scaler.Child!;
        (_top.Parent as Panel)?.Children.Remove(_top);
        (_pill.Parent as Panel)?.Children.Remove(_pill);
        if (ReferenceEquals(_floatPager.Child, _pill)) _floatPager.Child = null;
        _floatPager.IsOpen = false;
        _topLayer.IsVisible = _bottomLayer.IsVisible = false;
        ((TranslateTransform)_topLayer.RenderTransform!).Y = 0;
        ((TranslateTransform)_bottomLayer.RenderTransform!).Y = 0;
        _topLayer.Opacity = _bottomLayer.Opacity = 1;
        _icons.Opacity = _pill.Opacity = _badges.Opacity = 1;
        _icons.IsHitTestVisible = _pill.IsHitTestVisible = true;
        _pill.Margin = new Thickness(0, 0, 0, Settings.Bars == "float" ? 0 : ChromeTokens.PillBottomInset);
        if (Settings.Bars == "float")
        {
            _floatPager.VerticalOffset = ChromeTokens.FloatGap;
            _floatPager.Child = _pill;
        }
        else if (Settings.Bars == "overlay") _bottomLayer.Children.Add(_pill);
        else root.Children.Add(_pill);
        if (Sliding)
        {
            _content.RowDefinitions[0].Height = new GridLength(0);
            _top.Height = ChromeTokens.DockedRowHeight; _top.VerticalAlignment = VerticalAlignment.Top;
            _topLayer.Children.Add(_top);
            _topLayer.IsVisible = true;
            if (Settings.Bars == "overlay") _bottomLayer.IsVisible = true;
            _topShown = _bottomShown = true;
            SlideBar(top: true, show: false, instant: true);
            SlideBar(top: false, show: false, instant: true);
        }
        else
        {
            _content.RowDefinitions[0].Height = new GridLength(ChromeTokens.DockedRowHeight);
            _top.Height = double.NaN;
            Grid.SetRow(_top, 0);
            _content.Children.Insert(0, _top);
        }
        ApplyTopBlur();
    }

    /// <summary>Sliding styles: the top bar shows within 20 px of the top edge; the pager within 20 px (overlay) or 8 px (float) of the bottom.</summary>
    private void OverlayPointer(PointerEventArgs e)
    {
        if (!Sliding || _overlayOpen) return;
        var root = (Control)_scaler.Child!;
        var y = _pointerY = e.GetPosition(root).Y;
        var h = root.Bounds.Height;
        var nearTop = y < ChromeTokens.TopRevealZone;
        var nearBottom = y > h - BottomZone;
        if (nearTop) SlideBar(top: true, show: true);
        if (nearBottom) SlideBar(top: false, show: true);
        if ((!nearTop && _topShown) || (!nearBottom && _bottomShown)) { _barsSleep.Stop(); _barsSleep.Start(); }
    }

    private double BottomZone => Settings.Bars == "float" ? ChromeTokens.FloatPagerRevealZone : ChromeTokens.OverlayPagerRevealZone;

    /// <summary>Hides whichever bar the pointer has moved away from, judged by where the cursor really is now.</summary>
    private void BarsSleepTick()
    {
        _barsSleep.Stop();
        var root = (Control)_scaler.Child!;
        var h = root.Bounds.Height;
        var window = new PixelRect(Position, PixelSize.FromSize(Bounds.Size, RenderScaling));
        var inside = window.Contains(Native.CursorPosition());
        // Over the drag area Windows stops sending moves, so "inside but no fresh position" means the top bar.
        var y = inside ? (double.IsNaN(_pointerY) ? 0 : _pointerY) : double.NaN;
        if (!(y < ChromeTokens.TopRevealZone) && !_top.IsPointerOver) SlideBar(top: true, show: false);
        if (!(y > h - BottomZone) && !_pill.IsPointerOver) SlideBar(top: false, show: false);
    }

    private void SlideBar(bool top, bool show, bool instant = false)
    {
        if ((top ? _topShown : _bottomShown) == show && !instant) return;
        if (top) _topShown = show; else _bottomShown = show;
        if (top && show) RefreshSaved();
        if (!top && Settings.Bars == "float") { FloatPager(show, instant); return; }
        var layer = top ? _topLayer : _bottomLayer;
        var shift = (TranslateTransform)layer.RenderTransform!;
        var hidden = top ? -layer.Height * 0.6 : layer.Height * 0.6;
        var fromY = shift.Y; var toY = show ? 0 : hidden;
        var fromO = layer.Opacity; var toO = show ? 1.0 : 0;
        layer.IsHitTestVisible = show;
        if (instant || Anim.Reduced) { shift.Y = toY; layer.Opacity = toO; return; }
        Anim.Run(layer, 170, t => { var k = Anim.EaseOut(t); shift.Y = fromY + (toY - fromY) * k; layer.Opacity = fromO + (toO - fromO) * k; }, decorative: false);
    }

    /// <summary>The floating pager: its own small popup just below the window, faded in and out.</summary>
    private void FloatPager(bool show, bool instant)
    {
        if (show) { _pill.Opacity = 0; _floatPager.IsOpen = true; }
        if (instant || Anim.Reduced) { _pill.Opacity = show ? 1 : 0; if (!show) _floatPager.IsOpen = false; return; }
        var from = _pill.Opacity; var to = show ? 1.0 : 0;
        Anim.Run(_pill, 150, t => _pill.Opacity = from + (to - from) * Anim.EaseOut(t), () => { if (!show && !_bottomShown) _floatPager.IsOpen = false; }, decorative: false);
    }

    private void WakeChrome(bool hold = false)
    {
        if (Sliding) return;
        _chromeSleep.Stop();
        if (!_chromeAwake) { _chromeAwake = true; RefreshSaved(); FadeChrome(1); }
        if (!hold && Settings.Chrome == "fade") { _chromeSleep.Interval = TimeSpan.FromMilliseconds(Math.Max(300, Settings.FadeAfterMs)); _chromeSleep.Start(); }
    }

    private void SleepChrome(bool soon)
    {
        // Sliding styles: the pointer may have left for the drag area or the floating pager; let the tick decide.
        if (Sliding) { if (soon) { _barsSleep.Stop(); _barsSleep.Start(); } return; }
        if (Settings.Chrome != "fade" || _overlayOpen) return;
        if (_pill.IsPointerOver || _top.IsPointerOver) { if (soon) return; }
        if (soon) { _chromeSleep.Stop(); _chromeSleep.Start(); return; }
        if (!_chromeAwake) return;
        _chromeAwake = false;
        FadeChrome(0);
    }

    private void FadeChrome(double to)
    {
        _chromeAnim?.Cancel();
        var from = _icons.Opacity;
        _pill.IsHitTestVisible = to > 0; _icons.IsHitTestVisible = to > 0;
        // Same duration both ways, so appearing and disappearing feel like one motion.
        _chromeAnim = Anim.Run(this, 180, t =>
        {
            var v = from + (to - from) * t;
            _icons.Opacity = v; _pill.Opacity = v; _badges.Opacity = v;
        }, decorative: false);
    }

    // ---------- toast ----------

    internal void Toast(string text, string? action = null, Action? onAction = null, bool brief = false)
    {
        var p = _palette;
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        row.Children.Add(new TextBlock { Text = text, Foreground = new SolidColorBrush(p.Canvas), FontSize = 12.5, TextWrapping = TextWrapping.Wrap, MaxWidth = 300, VerticalAlignment = VerticalAlignment.Center });
        _toastAction = onAction;
        _undoArmed = onAction is not null;
        if (action is not null && onAction is not null)
        {
            var label = Kit.Text(action, p.Accent, 12.5, FontWeight.SemiBold);
            label.Margin = new Thickness(6, 1);
            var button = Kit.Plain(label, p with { Ink = p.Canvas });
            button.Click += (_, _) => { onAction(); HideToast(); };
            row.Children.Add(button);
        }
        _toast.Child = row;
        _toast.IsVisible = true;
        Anim.Run(_toast, 120, t => _toast.Opacity = t);
        _toastTimer.Stop();
        _toastTimer.Interval = TimeSpan.FromMilliseconds(action is not null ? 7000 : brief ? 1300 : 2400);
        _toastTimer.Start();
    }

    private void HideToast()
    {
        _toastAction = null; _undoArmed = false;
        Anim.Run(_toast, 160, t => _toast.Opacity = 1 - t, () => _toast.IsVisible = false);
    }

    private void ShowSaveFailurePrompt(string? reason, Action retry, Action quitAnyway)
    {
        var message = string.IsNullOrWhiteSpace(reason) ? "The notes could not be written to disk." : reason;
        if (_saveFailureOverlay is { } current && IsOverlay(current))
        {
            _saveFailureReason!.Text = message;
            _saveFailureRetryAction = retry;
            return;
        }

        var p = _palette;
        var title = Kit.Text("Couldn't save your notes", p.Ink, 17, FontWeight.SemiBold);
        Avalonia.Automation.AutomationProperties.SetName(title, "Couldn't save your notes");
        var detail = new TextBlock
        {
            Text = message, Foreground = new SolidColorBrush(p.Muted), FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap, MaxWidth = 380,
        };
        Avalonia.Automation.AutomationProperties.SetName(detail, "Save error reason");

        var retryButton = Kit.Plain(Kit.Text("Retry", p.Accent, 12.5, FontWeight.SemiBold), p, 88, 34);
        Avalonia.Automation.AutomationProperties.SetName(retryButton, "Retry");
        retryButton.Click += (_, _) => retry();
        var quitButton = Kit.Plain(Kit.Text("Quit anyway", p.Muted, 12.5), p, 104, 34);
        Avalonia.Automation.AutomationProperties.SetName(quitButton, "Quit anyway");
        quitButton.Click += (_, _) => quitAnyway();
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal, Spacing = 6, HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 6, 0, 0), Children = { retryButton, quitButton },
        };
        var content = new StackPanel { Spacing = 9, Children = { title, detail, buttons } };
        var card = new Border
        {
            Child = content, MaxWidth = 420, MinWidth = 240, Margin = new Thickness(12), Padding = new Thickness(18),
            CornerRadius = new CornerRadius(12), Background = new SolidColorBrush(p.Surface),
            BorderBrush = new SolidColorBrush(p.Faint, .45), BorderThickness = new Thickness(1), BoxShadow = _pill.BoxShadow,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
        };
        Avalonia.Automation.AutomationProperties.SetName(card, "Save failure prompt");
        var overlay = new Grid { Background = Brushes.Transparent, Children = { card } };
        Avalonia.Automation.AutomationProperties.SetName(overlay, "Save failure prompt");
        _saveFailureOverlay = overlay;
        _saveFailureCard = card;
        _saveFailureReason = detail;
        _saveFailureRetryButton = retryButton;
        _saveFailureQuitButton = quitButton;
        _saveFailureRetryAction = retry;
        ShowOverlay(overlay);
        Dispatcher.UIThread.Post(() => { if (IsOverlay(overlay)) retryButton.Focus(); });
    }

    private void OnSaveFailureKeyDown(object? sender, KeyEventArgs e)
    {
        if (_saveFailureOverlay is not { } overlay || !IsOverlay(overlay)) return;
        if (e.Key == Key.Escape) return;
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            _saveFailureRetryAction?.Invoke();
            return;
        }
        if (e.Key == Key.Tab)
        {
            e.Handled = true;
            if (_saveFailureRetryButton?.IsFocused == true) _saveFailureQuitButton?.Focus();
            else _saveFailureRetryButton?.Focus();
            return;
        }
        if (e.Key is Key.Space or Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt) return;
        e.Handled = true;
    }

    private void OnSaveFailurePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_saveFailureOverlay is not { } overlay || !IsOverlay(overlay) || _saveFailureCard is not { } card) return;
        var at = e.GetPosition(card);
        if (at.X >= 0 && at.Y >= 0 && at.X < card.Bounds.Width && at.Y < card.Bounds.Height) return;
        CloseOverlay();
        e.Handled = true;
    }

    /// <summary>A tiny shake: there is nothing further in that direction.</summary>
    private void Bump(int direction)
    {
        var shift = new TranslateTransform();
        _active.RenderTransform = shift;
        Anim.Run(_active, 180, t => shift.X = direction * 7 * Math.Sin(t * Math.PI), () => _active.RenderTransform = null);
    }
}
