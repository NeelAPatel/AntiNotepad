// Builds the pad's content and chrome layers and coordinates their visibility and input surfaces.
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
    private readonly Border _pinRing = new()
    {
        IsHitTestVisible = false,
        Opacity = 0,
        BorderThickness = new Thickness(ChromeTokens.PinRingThickness),
        CornerRadius = new CornerRadius(Shape.Card),
    };
    private Anim.Handle? _pinRingFade;
    private bool _pinRingInitialized;
    private readonly LayoutTransformControl _scaler = new();
    private readonly Grid _content = new() { RowDefinitions = new RowDefinitions($"{ChromeTokens.DockedRowHeight},*") };
    private readonly Grid _top = new() { ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto") };
    /// <summary>Which copy this is; fades with the icons.</summary>
    private readonly StackPanel _badges = new() { Orientation = Orientation.Horizontal, Spacing = ChromeTokens.BadgeSpacing, Margin = new Thickness(0, 0, ChromeTokens.BadgeRightInset, 0), VerticalAlignment = VerticalAlignment.Center };
    private TextBlock? _saved;
    private Note? _savedNote;
    private string? _savedTarget;
    private Anim.Handle? _savedMotion;
    private bool _bindingPanes;
    private readonly StackPanel _meta = new() { Orientation = Orientation.Horizontal, Spacing = ChromeTokens.MetaSpacing, Margin = new Thickness(ChromeTokens.FirstItemInset, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false };
    private readonly StackPanel _icons = new() { Orientation = Orientation.Horizontal, Spacing = ChromeTokens.IconSpacing, Margin = new Thickness(0, 0, ChromeTokens.BarEdgeInset, 0), VerticalAlignment = VerticalAlignment.Center };
    private readonly Grid _panes = new() { ColumnDefinitions = new ColumnDefinitions("*,0,0") };
    private readonly Border _divider = CreateSplitDivider();
    private Border _leftHost = null!, _rightHost = null!;
    private readonly List<PadBar> _bars = [];
    private Control _barsHost = null!;
    private readonly Border _toast = new() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(Space.L, 0, Space.L, ChromeTokens.ToastBottomMargin), CornerRadius = new CornerRadius(Shape.Card), Padding = new Thickness(Space.M, Space.S), IsVisible = false, MaxWidth = ChromeTokens.Size.ToastMaxWidth };
    private readonly Panel _overlayHost = new() { IsVisible = false };
    private Border _overlayDragBand = null!;
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
    private Anim.Handle? _toastAnim;
    private Action? _toastAction;
    private long _toastGeneration;
    private Note? _withdrawToastUndoOnEdit;
    /// <summary>Ctrl+Z undoes the toast's action (a delete) until the user types something.</summary>
    private bool _undoArmed;
    private Control? _saveFailureOverlay;
    private Border? _saveFailureCard;
    private TextBlock? _saveFailureReason;
    private Button? _saveFailureRetryButton, _saveFailureQuitButton;
    private Action? _saveFailureRetryAction;
    private Action? _saveFailureQuitAction;

    internal static Border CreateSplitDivider() => new()
    {
        Width = ChromeTokens.DividerWidth,
        Margin = new Thickness(0, ChromeTokens.DividerTopMargin, 0, ChromeTokens.DividerBottomMargin),
    };

    internal static (Border Left, Border Right) AddPaneHosts(Grid panes, Control leftPane, Border divider, Control rightPane)
    {
        var leftHost = new Border { Child = leftPane, ClipToBounds = true };
        var rightHost = new Border { Child = rightPane, ClipToBounds = true };
        panes.Children.Add(leftHost); Grid.SetColumn(leftHost, 0);
        panes.Children.Add(divider); Grid.SetColumn(divider, 1);
        panes.Children.Add(rightHost); Grid.SetColumn(rightHost, 2);
        return (leftHost, rightHost);
    }

    internal static void ApplyPaneSplit(Grid panes, Border leftHost, Border divider, Border rightHost, Control rightPane, bool split)
    {
        panes.ColumnDefinitions = new ColumnDefinitions(split ? "*,1,*" : "*,0,0");
        leftHost.ClipToBounds = split;
        rightHost.IsVisible = rightPane.IsVisible = divider.IsVisible = split;
    }

    /// <summary>Places the bar host among the window's chrome layers.</summary>
    internal static void AddChromeLayers(Panel root, Panel topLayer, Panel bottomLayer, Control toast, Control bars, Control floatPager)
    {
        root.Children.Add(topLayer);
        root.Children.Add(bottomLayer);
        root.Children.Add(toast);
        root.Children.Add(bars);
        root.Children.Add(floatPager);
    }

    /// <summary>Adds an overlay drag surface below the chrome so visible buttons keep input ownership.</summary>
    internal static Border AddOverlayDragBand(Panel root)
    {
        var band = new Border
        {
            Background = Brushes.Transparent,
            Height = ChromeTokens.DockedRowHeight,
            VerticalAlignment = VerticalAlignment.Top,
            IsVisible = false,
        };
        WindowDecorationProperties.SetElementRole(band, WindowDecorationsElementRole.TitleBar);
        root.Children.Add(band);
        return band;
    }

    /// <summary>Reserves sliding-bar clearance without adding a second header row in Docked mode.</summary>
    internal static void ApplyOverlayLayout(Panel host, Border band, bool sliding, bool draggable)
    {
        host.Margin = new Thickness(default, sliding ? ChromeTokens.DockedRowHeight : default, default, default);
        // A save prompt keeps ordinary client input here so outside clicks can dismiss it.
        band.IsVisible = sliding && host.IsVisible && draggable;
    }

    /// <summary>Keeps overlay clearance and drag access current after mode or overlay changes.</summary>
    private void UpdateOverlayLayout() => ApplyOverlayLayout(_overlayHost, _overlayDragBand, Sliding,
        _overlay is SearchOverlay or HelpOverlay);

    /// <summary>Builds the pad, attaches its chrome layers, and connects pointer behavior.</summary>
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
        (_leftHost, _rightHost) = AddPaneHosts(_panes, _left, _divider, _right);
        ApplyPaneSplit(_panes, _leftHost, _divider, _rightHost, _right, Settings.Split);
        Grid.SetRow(_panes, 1);
        _content.Children.Add(_panes);

        Grid.SetRow(_overlayHost, 1);
        _content.Children.Add(_overlayHost);

        var root = new Grid();
        root.Children.Add(_tint);
        root.Children.Add(_content);
        _overlayDragBand = AddOverlayDragBand(root);
        InitializeTopBlurVisuals();
        _bars.Add(new PadBar(BarPane.Left, BarContextFor(BarPane.Left), BarActionsFor(BarPane.Left)));
        _barsHost = CreateBarsHost();
        AddChromeLayers(root, _topLayer, _bottomLayer, _toast, _barsHost, _floatPager);
        root.Children.Add(_pinRing);
        // The pager can be reparented later; the ring must remain above every chrome layer.
        _pinRing.ZIndex = int.MaxValue;
        _topLayer.RenderTransform = new TranslateTransform();
        _floatPager.PlacementTarget = root;
        _bottomLayer.RenderTransform = new TranslateTransform();
        _barsSleep.Tick += (_, _) => BarsSleepTick();
        foreach (var bar in _bars)
            bar.Root.PointerExited += (_, _) => { if (Sliding) { _barsSleep.Stop(); _barsSleep.Start(); } else SleepChrome(soon: true); };
        _top.PointerExited += (_, _) => { if (!Sliding) SleepChrome(soon: true); };
        // UI scale: one layout transform over everything, so chrome, text and hit targets scale together.
        _scaler.Child = root;
        Content = _scaler;

        foreach (var bar in _bars) bar.Root.PointerEntered += (_, _) => WakeChrome(hold: true);
        _top.PointerEntered += (_, _) => WakeChrome(hold: true);
        _chromeSleep.Tick += (_, _) => { _chromeSleep.Stop(); SleepChrome(soon: false); };
        _toastTimer.Tick += (_, _) => { _toastTimer.Stop(); HideToast(); };
        InitializeTopBlur();
    }

    /// <summary>Groups multiple bars horizontally while keeping a single bar's visual tree direct.</summary>
    private Control CreateBarsHost()
    {
        if (_bars.Count == 1) return _bars[0].Root;
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
        };
        foreach (var bar in _bars) row.Children.Add(bar.Root);
        return row;
    }

    internal static double PinRingOpacity(bool pinned) => pinned ? Paint.FocusBorder : 0;

    /// <summary>Recolors the ring and replaces any pending fade from its current opacity.</summary>
    private void ApplyPinRing(bool animate)
    {
        _pinRingFade?.Cancel();
        _pinRingFade = null;
        _pinRing.BorderBrush = new SolidColorBrush(_palette.Accent);
        var from = _pinRing.Opacity;
        var to = PinRingOpacity(Settings.Pinned);
        if (!animate || !_pinRingInitialized || from == to)
            _pinRing.Opacity = to;
        else
            _pinRingFade = Anim.Run(_pinRing, MotionTokens.PinRingFade,
                t => _pinRing.Opacity = from + (to - from) * t);
        _pinRingInitialized = true;
    }

    private PadView NewPane()
    {
        var pane = new PadView(_palette, Settings, _timers, _notes);
        pane.Edited += () => OnPaneEdited(pane);
        pane.ModeChanged += UpdateChrome;
        pane.Toast += t => Toast(t);
        pane.Copy += text => _ = CopyText(text);
        pane.ShowMenu = (anchor, entries) => PadMenu.Show(anchor, entries, _palette, Settings);
        pane.AppMenu = () =>
        [
            new("New note", _shortcuts.GetLabel("new-note"), Icons.NewNote, NewNote),
            new("Search notes", _shortcuts.GetLabel("search"), Icons.Search, () => OpenSearch()),
            new(Settings.Split ? "Close split view" : "Split view", _shortcuts.GetLabel("split"), Icons.Split, ToggleSplit),
            new(Settings.Pinned ? "Unpin" : "Keep on top", _shortcuts.GetLabel("keep-on-top"), Icons.KeepOnTop, TogglePin),
            MenuEntry.Separator,
            new("Settings", _shortcuts.GetLabel("settings"), Icons.Settings, OpenSettings),
            new($"Hide {AppInfo.Name}", "Escape", null, HideResident),
        ];
        pane.FocusedPane += () => HandlePaneFocused(pane);
        return pane;
    }

    internal void HandlePaneFocused(PadView pane)
    {
        if (_active == pane) return;
        ActivatePane(pane);
        UpdateChrome();
    }

    /// <summary>Changes the active editor and saves the note that loses active status.</summary>
    private void ActivatePane(PadView pane, bool saveDeparting = true)
    {
        if (_active == pane) return;
        if (saveDeparting && _active?.Note is { } departing && !ReferenceEquals(departing, pane.Note)) SaveImmediately();
        _active = pane;
    }

    /// <summary>Schedules the changed note using the document state before the model's text event catches up.</summary>
    private void OnPaneEdited(PadView pane)
    {
        _undoArmed = false;
        if (_withdrawToastUndoOnEdit is { } slotUndoNote && ReferenceEquals(pane.Note, slotUndoNote)) HideToast();
        if (pane.Note is { } note) ScheduleNoteSave(note, IsBlankDocument(pane.Editor.Document));
        UpdateChrome();
        SleepChrome(soon: false);
    }

    /// <summary>Matches the editor's blank-note rule without copying long document text.</summary>
    private static bool IsBlankDocument(AvaloniaEdit.Document.TextDocument document)
    {
        var length = document.TextLength;
        return length == 0 || length < 256 && string.IsNullOrWhiteSpace(document.GetText(0, length));
    }

    /// <summary>Recreates chrome controls (palette changes), then refreshes their content.</summary>
    private void RebuildChrome()
    {
        var p = _palette;
        _icons.Children.Clear();
        // The repo launch route is the only copy with a badge.
        _badges.Children.Clear();
        if (_isDev) _badges.Children.Add(DevBadge(p.Number));
        _badges.IsVisible = _isDev;
        foreach (var bar in _bars) bar.Rebuild(BarContextFor(bar.Pane), BarActionsFor(bar.Pane));
        BuildSwapButton();
        _icons.Children.Add(_swapButton!);
        _icons.Children.Add(Kit.IconButton(Icons.Search, ShortcutTooltip("Search notes", _shortcuts.GetLabel("search")), p, ToggleSearch));
        _icons.Children.Add(Kit.IconButton(Icons.Split, ShortcutTooltip("Split view with a slot", _shortcuts.GetLabel("split")), p, ToggleSplit));
        // Pinned: the pin is filled with the accent, so the state reads at a glance.
        _icons.Children.Add(Kit.IconButton(Icons.KeepOnTop, ShortcutTooltip(Settings.Pinned ? "Unpin" : "Keep on top", _shortcuts.GetLabel("keep-on-top")), p, TogglePin,
            filled: Settings.Pinned, color: Settings.Pinned ? p.Accent : null));
        _icons.Children.Add(Kit.IconButton(Icons.Settings, ShortcutTooltip("Settings", _shortcuts.GetLabel("settings")), p, OpenSettings));
        _icons.Children.Add(Kit.IconButton(Icons.Hide, "Hide (Esc)", p, HideResident));
        _divider.Background = new SolidColorBrush(p.Faint, ChromeTokens.DividerAlpha);
        _toast.Background = new SolidColorBrush(p.Ink, Paint.Toast);
        _toast.BoxShadow = PadBar.PillShadow(p);
        _overlayHost.Background = new SolidColorBrush(p.Canvas, Settings.Material == "solid" ? 1 : Paint.GlassScrim);
        UpdateChrome();
    }

    private static string ShortcutTooltip(string action, string keys) => keys.Length == 0 ? action : $"{action} ({keys})";

    /// <summary>Refreshes metadata text, the pager and slot dots.</summary>
    private void UpdateChrome() => UpdateChrome(0);

    private void UpdateChrome(int direction)
    {
        if (_bindingPanes) return;
        if (_left is null || _active?.Note is not { } note) return;
        var p = _palette;
        _meta.Children.Clear();
        if (note.Slot > 0) _meta.Children.Add(Chip($"slot {note.Slot}", p.Accent));
        var label = Modes.Label(_active.Mode, _active.CodeLanguage);
        if (label.Length > 0) _meta.Children.Add(Chip(label, p.Keyword));
        _saved ??= Kit.Text("", p.Muted, ChromeTokens.SavedLabelFontSize);
        _saved.Foreground = new SolidColorBrush(p.Muted);
        _meta.Children.Add(_saved);
        SetSavedText(SavedText(note, DateTime.Now), ReferenceEquals(_savedNote, note) ? 0 : direction);
        _savedNote = note;
        if (_timers.AnyRunning && _active.Mode != NoteMode.Timer) _meta.Children.Add(Kit.Text("● timer running", p.Number, ChromeTokens.RunningTimerFontSize));
        foreach (var bar in _bars) bar.Update(BarContextFor(bar.Pane), BarActionsFor(bar.Pane));
        UpdateSwapButton();
        UpdateSavedLabelTimer();
    }

    /// <summary>Formats the last durable save time, keeping unsaved notes visibly unsaved.</summary>
    internal static string SavedText(Note note, DateTime now)
    {
        if (note.SavedAt is not { } savedAt) return "-";
        var age = now - savedAt;
        if (age.TotalMilliseconds < ChromeTokens.SaveIdleGateMs) return "Saved just now";
        if (age.TotalMilliseconds < ChromeTokens.SavedLabelMinuteMs) return $"Saved {(int)age.TotalSeconds}s ago";
        if (age.TotalMilliseconds < ChromeTokens.SavedLabelTimestampMs) return $"Saved {(int)age.TotalMinutes}m ago";
        if (savedAt.Date == now.Date) return $"Saved {savedAt.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture)}";
        var format = savedAt.Year == now.Year ? "MMM d, HH:mm" : "MMM d, yyyy HH:mm";
        return $"Saved {savedAt.ToString(format, System.Globalization.CultureInfo.InvariantCulture)}";
    }

    /// <summary>Aligns the next label refresh with a whole second after the saved time.</summary>
    internal static TimeSpan SavedLabelRefreshInterval(DateTime savedAt, DateTime now)
    {
        if (savedAt > now) return TimeSpan.FromMilliseconds(ChromeTokens.SavedLabelRefreshMs);
        var elapsedTicks = now.Ticks - savedAt.Ticks;
        var elapsedWholeSeconds = elapsedTicks < 0 ? 0 : elapsedTicks / TimeSpan.TicksPerSecond;
        var nextBoundary = savedAt.AddSeconds(elapsedWholeSeconds + 1);
        var interval = nextBoundary - now;
        return interval > TimeSpan.Zero ? interval : TimeSpan.FromMilliseconds(ChromeTokens.SavedLabelRefreshMs);
    }

    /// <summary>Allows the saved-time clock while the pad is visible and not minimized.</summary>
    internal static bool ShouldRunSavedLabelClock(bool isVisible, bool isMinimized) =>
        isVisible && !isMinimized;

    /// <summary>Maps the page direction to the departure and arrival offsets.</summary>
    internal static (double Exit, double Enter) SavedOffsets(int direction)
    {
        var shift = Math.Sign(direction) * MotionTokens.OverlayRise;
        return (-shift, shift);
    }

    /// <summary>Transitions between note labels, or updates immediately for ordinary refreshes.</summary>
    private void SetSavedText(string text, int direction)
    {
        if (_saved is not { } saved || _savedTarget == text) return;
        _savedMotion?.Cancel();
        _savedTarget = text;
        var shift = new TranslateTransform();
        saved.RenderTransform = shift;
        if (direction == 0 || Anim.Reduced || saved.Text is null or "")
        {
            saved.Text = text;
            saved.Opacity = 1;
            return;
        }
        var offsets = SavedOffsets(direction);
        saved.Opacity = 1;
        _savedMotion = Anim.Run(saved, MotionTokens.OverlayOut, t =>
        {
            saved.Opacity = 1 - t;
            shift.X = offsets.Exit * t;
        }, () =>
        {
            saved.Text = text;
            _savedMotion = Anim.Run(saved, MotionTokens.OverlayIn, t =>
            {
                saved.Opacity = t;
                shift.X = offsets.Enter * (1 - t);
            }, easing: MotionTokens.Win11Enter);
        }, easing: MotionTokens.Win11Exit);
    }

    /// <summary>Keeps "Saved 3m ago" true when the bar comes back after a while without edits.</summary>
    private void RefreshSaved()
    {
        if (_active?.Note is { } note) SetSavedText(SavedText(note, DateTime.Now), 0);
        UpdateSavedLabelTimer();
    }

    /// <summary>Runs the age clock while the visible pad can show its label.</summary>
    private void UpdateSavedLabelTimer()
    {
        _savedLabelTimer.Stop();
        if (!ShouldRunSavedLabelClock(IsVisible, WindowState == WindowState.Minimized)
            || _active?.Note?.SavedAt is not { } savedAt) return;
        _savedLabelTimer.Interval = SavedLabelRefreshInterval(savedAt, DateTime.Now);
        _savedLabelTimer.Start();
    }

    private Border Chip(string text, Color color, Color? textColor = null) => new()
    {
        Background = new SolidColorBrush(color, _palette.Dark ? ChromeTokens.BadgeDarkFillAlpha : ChromeTokens.BadgeLightFillAlpha),
        CornerRadius = new CornerRadius(ChromeTokens.BadgeCornerRadius),
        Padding = new Thickness(ChromeTokens.BadgeHorizontalPadding, ChromeTokens.BadgeTopPadding, ChromeTokens.BadgeHorizontalPadding, ChromeTokens.BadgeBottomPadding),
        Height = ChromeTokens.BadgeHeight,
        Child = Kit.Text(text, textColor ?? color, ChromeTokens.BadgeFontSize, Typography.Strong),
        VerticalAlignment = VerticalAlignment.Center
    };

    /// <summary>Builds the branch badge and animates between its measured labels.</summary>
    private Border DevBadge(Color color)
    {
        var (shortText, fullText) = AppInfo.FormatDevBadge(AppInfo.BuildBranch, AppInfo.BuildTime?.ToLocalTime());
        var slotColor = AppInfo.SlotColor;
        var badgeColor = slotColor ?? color;
        var textColor = slotColor is { } selected && !_palette.Dark
            ? MixColor(selected, _palette.Ink, ChromeTokens.DevBadgeLightTextInkMix)
            : badgeColor;
        var chip = Chip(shortText, badgeColor, textColor);
        ToolTip.SetTip(chip, fullText);
        Avalonia.Automation.AutomationProperties.SetName(chip, fullText);
        if (shortText == "dev") return chip;

        var glyphFont = FindDevBadgeGlyphFont();
        var resting = DevBadgeText(shortText, textColor, glyphFont);
        var expanded = DevBadgeText(fullText, textColor, glyphFont);
        resting.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        expanded.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var restingWidth = resting.DesiredSize.Width + 2 * ChromeTokens.BadgeHorizontalPadding;
        var expandedWidth = expanded.DesiredSize.Width + 2 * ChromeTokens.BadgeHorizontalPadding;
        var content = new Grid { ClipToBounds = true };
        content.Children.Add(resting);
        content.Children.Add(expanded);
        expanded.Opacity = 0;
        chip.Child = content;
        chip.ClipToBounds = true;
        chip.Width = restingWidth;

        Anim.Handle? widthMotion = null;
        Anim.Handle? fadeMotion = null;
        void SetExpanded(bool showFull)
        {
            widthMotion?.Cancel();
            fadeMotion?.Cancel();
            var fromWidth = chip.Width;
            var toWidth = showFull ? expandedWidth : restingWidth;
            var fromResting = resting.Opacity;
            var toResting = showFull ? 0 : 1;
            var fromExpanded = expanded.Opacity;
            var toExpanded = showFull ? 1 : 0;
            widthMotion = Anim.Run(chip, MotionTokens.DevBadgeWidth,
                progress => chip.Width = fromWidth + (toWidth - fromWidth) * progress,
                easing: MotionTokens.DevBadgeWidthEasing);
            fadeMotion = Anim.Run(chip, MotionTokens.DevBadgeFade, progress =>
            {
                resting.Opacity = fromResting + (toResting - fromResting) * progress;
                expanded.Opacity = fromExpanded + (toExpanded - fromExpanded) * progress;
            }, easing: MotionTokens.DevBadgeFadeEasing);
        }
        chip.PointerEntered += (_, _) => SetExpanded(true);
        chip.PointerExited += (_, _) => SetExpanded(false);
        return chip;
    }

    /// <summary>Builds one badge label while keeping the branch glyph in its icon font.</summary>
    private static TextBlock DevBadgeText(string value, Color color, FontFamily? glyphFont)
    {
        var text = Kit.Text("", color, ChromeTokens.BadgeFontSize, Typography.Strong);
        text.Text = null;
        text.TextWrapping = TextWrapping.NoWrap;
        text.HorizontalAlignment = HorizontalAlignment.Left;
        text.VerticalAlignment = VerticalAlignment.Center;
        text.IsHitTestVisible = false;
        text.Inlines!.Clear();
        const string badgeGlyph = AppInfo.DevBadgeGlyph;
        if (value.StartsWith(badgeGlyph + " ", StringComparison.Ordinal))
        {
            text.Inlines.Add(new Avalonia.Controls.Documents.Run(glyphFont is null ? "\u2387" : badgeGlyph)
            {
                FontFamily = glyphFont ?? text.FontFamily,
                FontWeight = Typography.Regular
            });
            text.Inlines.Add(new Avalonia.Controls.Documents.Run(value[1..]));
        }
        else text.Inlines.Add(new Avalonia.Controls.Documents.Run(value));
        return text;
    }

    /// <summary>Finds a system font that contains the branch glyph, if available.</summary>
    private static FontFamily? FindDevBadgeGlyphFont()
    {
        foreach (var family in FontManager.Current.SystemFonts)
        {
            if (family.Name is "Cascadia Mono" or "Cascadia Code"
                && FontManager.Current.TryGetGlyphTypeface(new Typeface(family), out var typeface)
                && typeface.CharacterToGlyphMap.ContainsGlyph(0xE0A0)) return family;
        }
        return null;
    }

    private static Color MixColor(Color from, Color to, double amount) => Color.FromRgb(
        MixChannel(from.R, to.R, amount), MixChannel(from.G, to.G, amount), MixChannel(from.B, to.B, amount));

    private static byte MixChannel(byte from, byte to, double amount) =>
        (byte)Math.Round(from + (to - from) * amount, MidpointRounding.AwayFromZero);

    // ---------- chrome visibility: present when the mouse moves, gone while typing ----------

    // ---------- bar style ----------
    // docked: a top bar row and a pager below the text (they fade while typing).
    // overlay: both bars sit over the note and slide in when the pointer reaches their edge.
    // float: the top bar slides in the same way; the pager floats just below the window, out of the note entirely.

    private bool Sliding => Settings.Bars is "overlay" or "float";

    /// <summary>Moves the top bar and bottom-bar host to the current placement style.</summary>
    private void ApplyBars()
    {
        if (_barsMode == Settings.Bars) return;
        _barsMode = Settings.Bars;
        var root = (Panel)_scaler.Child!;
        (_top.Parent as Panel)?.Children.Remove(_top);
        if (ReferenceEquals(_floatPager.Child, _barsHost)) _floatPager.Child = null;
        (_barsHost.Parent as Panel)?.Children.Remove(_barsHost);
        _floatPager.IsOpen = false;
        _topLayer.IsVisible = _bottomLayer.IsVisible = false;
        ((TranslateTransform)_topLayer.RenderTransform!).Y = 0;
        ((TranslateTransform)_bottomLayer.RenderTransform!).Y = 0;
        _topLayer.Opacity = _bottomLayer.Opacity = 1;
        _icons.Opacity = _badges.Opacity = 1;
        _icons.IsHitTestVisible = true;
        foreach (var bar in _bars)
        {
            bar.Root.Opacity = 1;
            bar.Root.IsHitTestVisible = true;
            bar.ConfigureBarStyle(BarContextFor(bar.Pane));
        }
        if (Settings.Bars == "float")
        {
            _floatPager.VerticalOffset = ChromeTokens.FloatGap;
            _floatPager.Child = _barsHost;
        }
        else if (Settings.Bars == "overlay") _bottomLayer.Children.Add(_barsHost);
        else root.Children.Add(_barsHost);
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
        UpdateOverlayLayout();
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

    /// <summary>Hides bars the pointer has left, using its latest position in the window.</summary>
    private void BarsSleepTick()
    {
        _barsSleep.Stop();
        var root = (Control)_scaler.Child!;
        var h = root.Bounds.Height;
        var window = new PixelRect(Position, PixelSize.FromSize(Bounds.Size, RenderScaling));
        var inside = window.Contains(DesktopPlatform.CursorPosition());
        // Over the drag area Windows stops sending moves, so "inside but no fresh position" means the top bar.
        var y = inside ? (double.IsNaN(_pointerY) ? 0 : _pointerY) : double.NaN;
        if (!(y < ChromeTokens.TopRevealZone) && !_top.IsPointerOver) SlideBar(top: true, show: false);
        if (!(y > h - BottomZone) && !_bars.Any(bar => bar.Root.IsPointerOver)) SlideBar(top: false, show: false);
    }

    private void SlideBar(bool top, bool show, bool instant = false)
    {
        if ((top ? _topShown : _bottomShown) == show && !instant) return;
        if (top) _topShown = show; else _bottomShown = show;
        if (top && show && _active?.Note is { } note) SetSavedText(SavedText(note, DateTime.Now), 0);
        if (!top && Settings.Bars == "float") { FloatPager(show, instant); return; }
        var layer = top ? _topLayer : _bottomLayer;
        var shift = (TranslateTransform)layer.RenderTransform!;
        var hidden = top ? -layer.Height * 0.6 : layer.Height * 0.6;
        var fromY = shift.Y; var toY = show ? 0 : hidden;
        var fromO = layer.Opacity; var toO = show ? 1.0 : 0;
        layer.IsHitTestVisible = show;
        if (instant || Anim.Reduced) { shift.Y = toY; layer.Opacity = toO; return; }
        Anim.Run(layer, MotionTokens.BarsReveal, t => { var k = Anim.EaseOut(t); shift.Y = fromY + (toY - fromY) * k; layer.Opacity = fromO + (toO - fromO) * k; }, decorative: false);
    }

    /// <summary>Fades the floating bars in their popup below the window.</summary>
    private void FloatPager(bool show, bool instant)
    {
        if (show)
        {
            foreach (var bar in _bars) bar.Root.Opacity = 0;
            _floatPager.IsOpen = true;
        }
        if (instant || Anim.Reduced)
        {
            foreach (var bar in _bars) bar.Root.Opacity = show ? 1 : 0;
            if (!show) _floatPager.IsOpen = false;
            return;
        }
        var to = show ? 1.0 : 0;
        for (var i = 0; i < _bars.Count; i++)
        {
            var root = _bars[i].Root;
            var from = root.Opacity;
            Action? after = i == _bars.Count - 1 && !show ? () => { if (!_bottomShown) _floatPager.IsOpen = false; } : null;
            Anim.Run(root, MotionTokens.PillFade, t => root.Opacity = from + (to - from) * Anim.EaseOut(t), after, decorative: false);
        }
    }

    private void WakeChrome(bool hold = false)
    {
        if (Sliding) return;
        _chromeSleep.Stop();
        if (!_chromeAwake) { _chromeAwake = true; RefreshSaved(); FadeChrome(1); }
        if (!hold && Settings.Chrome == "fade") { _chromeSleep.Interval = TimeSpan.FromMilliseconds(Math.Max(300, Settings.FadeAfterMs)); _chromeSleep.Start(); }
    }

    /// <summary>Fades docked chrome after typing or pointer exit unless a bar still owns the pointer.</summary>
    private void SleepChrome(bool soon, bool typing = false)
    {
        // Sliding styles: the pointer may have left for the drag area or the floating pager; let the tick decide.
        if (Sliding) { if (soon) { _barsSleep.Stop(); _barsSleep.Start(); } return; }
        if (Settings.Chrome != "fade" || _overlayOpen) return;
        if (!typing && (_bars.Any(bar => bar.Root.IsPointerOver) || _top.IsPointerOver))
        {
            WakeChrome(hold: false);
            return;
        }
        if (soon) { _chromeSleep.Stop(); _chromeSleep.Start(); return; }
        if (!_chromeAwake) return;
        _chromeAwake = false;
        FadeChrome(0);
    }

    /// <summary>Fades every bar and the top chrome as one visual group.</summary>
    private void FadeChrome(double to)
    {
        _chromeAnim?.Cancel();
        var from = _icons.Opacity;
        foreach (var bar in _bars) bar.Root.IsHitTestVisible = to > 0;
        _icons.IsHitTestVisible = to > 0;
        // Same duration both ways, so appearing and disappearing feel like one motion.
        _chromeAnim = Anim.Run(this, MotionTokens.ChromeShift, t =>
        {
            var v = from + (to - from) * t;
            _icons.Opacity = v;
            foreach (var bar in _bars) bar.Root.Opacity = v;
            _badges.Opacity = v;
        }, decorative: false);
    }

    // ---------- toast ----------

    internal void Toast(string text, string? action = null, Action? onAction = null, bool brief = false, Note? withdrawUndoOnEdit = null)
    {
        var p = _palette;
        var generation = ++_toastGeneration;
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = Space.M };
        row.Children.Add(new TextBlock { Text = text, Foreground = new SolidColorBrush(p.Canvas), FontSize = Typography.Caption, TextWrapping = TextWrapping.Wrap, MaxWidth = ChromeTokens.Size.ToastTextMaxWidth, VerticalAlignment = VerticalAlignment.Center });
        _toastAction = onAction;
        _undoArmed = onAction is not null;
        _withdrawToastUndoOnEdit = onAction is null ? null : withdrawUndoOnEdit;
        if (action is not null && onAction is not null)
        {
            var label = Kit.Text(action, p.Accent, Typography.Body, Typography.Strong);
            label.Margin = new Thickness(Space.S, Space.Nudge);
            var button = Kit.Plain(label, p with { Ink = p.Canvas }, height: ChromeTokens.ButtonSize);
            button.Click += (_, _) =>
            {
                if (_toastGeneration != generation || !ReferenceEquals(_toastAction, onAction)) return;
                onAction();
                if (_toastGeneration == generation) HideToast();
            };
            row.Children.Add(button);
        }
        _toast.Child = row;
        _toast.IsVisible = true;
        _toastAnim?.Cancel();
        var shift = new TranslateTransform();
        _toast.RenderTransform = shift;
        _toastAnim = Anim.Run(_toast, MotionTokens.OverlayIn, t =>
        {
            _toast.Opacity = t;
            shift.Y = MotionTokens.OverlayRise * (1 - t);
        }, easing: MotionTokens.Win11Enter);
        _toastTimer.Stop();
        _toastTimer.Interval = TimeSpan.FromMilliseconds(action is not null ? 7000 : brief ? 1300 : 2400);
        _toastTimer.Start();
    }

    private void HideToast()
    {
        _toastGeneration++;
        _toastAction = null; _undoArmed = false; _withdrawToastUndoOnEdit = null;
        _toastAnim?.Cancel();
        var startOpacity = _toast.Opacity;
        _toastAnim = Anim.Run(_toast, MotionTokens.OverlayOut, t => _toast.Opacity = startOpacity * (1 - t),
            () => _toast.IsVisible = false, easing: MotionTokens.Win11Exit);
    }

    private void ShowSaveFailurePrompt(string? reason, Action retry, Action quitAnyway)
    {
        var message = string.IsNullOrWhiteSpace(reason) ? "" : "Details: " + reason;
        if (_saveFailureOverlay is { } current && IsOverlay(current))
        {
            _saveFailureReason!.Text = message;
            _saveFailureReason.IsVisible = message.Length > 0;
            _saveFailureRetryAction = retry;
            _saveFailureQuitAction = quitAnyway;
            return;
        }

        var p = _palette;
        var title = Kit.Text("Couldn't save your notes", p.Ink, Typography.DialogTitle, Typography.Strong);
        Avalonia.Automation.AutomationProperties.SetName(title, "Couldn't save your notes");
        var detail = new TextBlock
        {
            Text = message,
            IsVisible = message.Length > 0,
            Foreground = new SolidColorBrush(p.Muted),
            FontSize = Typography.Body,
            TextWrapping = TextWrapping.Wrap, MaxWidth = ChromeTokens.Size.SaveFailureReasonMaxWidth,
        };
        Avalonia.Automation.AutomationProperties.SetName(detail, "Save error reason");

        var retryButton = Kit.Plain(Kit.Text("Retry", p.Accent, Typography.Body, Typography.Strong), p, 88, ChromeTokens.ButtonSize);
        if (retryButton.Content is Border retryFace) retryFace.Padding = new Thickness(Space.S, Space.Nudge);
        Avalonia.Automation.AutomationProperties.SetName(retryButton, "Retry");
        retryButton.Click += (_, _) => retry();
        var quitButton = Kit.Plain(Kit.Text("Quit anyway", p.Muted, Typography.Body), p, 104, ChromeTokens.ButtonSize);
        if (quitButton.Content is Border quitFace) quitFace.Padding = new Thickness(Space.S, Space.Nudge);
        Avalonia.Automation.AutomationProperties.SetName(quitButton, "Quit anyway");
        quitButton.Click += (_, _) => quitAnyway();
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal, Spacing = Space.S, HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, Space.S, 0, 0), Children = { retryButton, quitButton },
        };
        var body = new TextBlock { Text = "Your latest edits aren't on disk yet. Retry tries again. Quit anyway closes AntiNotepad, and those edits may be lost.", Foreground = new SolidColorBrush(p.Ink), FontSize = Typography.Body, TextWrapping = TextWrapping.Wrap, MaxWidth = ChromeTokens.Size.SaveFailureReasonMaxWidth };
        var content = new StackPanel { Spacing = Space.S, Children = { title, body, detail, buttons } };
        var card = new Border
        {
            Child = content,
            MaxWidth = ChromeTokens.Size.SaveFailureCardMaxWidth,
            MinWidth = ChromeTokens.Size.SaveFailureCardMinWidth,
            Margin = new Thickness(Space.M),
            Padding = new Thickness(Space.L),
            CornerRadius = new CornerRadius(Shape.Dialog),
            Background = new SolidColorBrush(p.Surface),
            BorderBrush = new SolidColorBrush(p.Faint, Paint.PopupBorder),
            BorderThickness = new Thickness(Shape.Thin),
            BoxShadow = PadBar.PillShadow(p),
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
        _saveFailureQuitAction = quitAnyway;
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
            InvokeSaveFailureEnter(_saveFailureQuitButton?.IsFocused == true || ReferenceEquals(e.Source, _saveFailureQuitButton),
                _saveFailureRetryAction, _saveFailureQuitAction);
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

    private static void InvokeSaveFailureEnter(bool quitFocused, Action? retry, Action? quitAnyway) =>
        (quitFocused ? quitAnyway : retry)?.Invoke();

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
        Anim.Run(_active, MotionTokens.Nudge, t => shift.X = direction * 7 * Math.Sin(t * Math.PI), () => _active.RenderTransform = null);
    }
}
