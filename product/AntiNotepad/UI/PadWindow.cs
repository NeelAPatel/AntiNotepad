// Owns the resident pad window's lifecycle, persistence, and appearance.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using AntiNotepad;
using AntiNotepad.Editor;
using AntiNotepad.Model;
using AntiNotepad.Motion;
using AntiNotepad.Platform;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

/// <summary>The resident pad window. Hidden, never closed, until Quit.</summary>
internal sealed partial class PadWindow : Window, ISettingsHost
{
    private readonly Store _store;
    private readonly SettingsService _settingsService;
    private readonly NotesSession _notes;
    private readonly Timers _timers;
    private readonly RuntimeLog _log;
    private readonly bool _isDev;
    private readonly bool _softwareRendering;
    private readonly TaskbarTrace _trace;
    private readonly DispatcherTimer _saveTimer = new() { Interval = TimeSpan.FromMilliseconds(400) };
    private Palette _palette;
    private bool _quitting;
    private DateTime _hiddenAt = DateTime.Now;
    private IDisposable? _clipboardHook;
    private readonly HotkeyRegistrar _hotkeys;
    private TaskbarProxy? _taskbarProxy;
    private PadView _left = null!, _right = null!;
    private PadView _active = null!;

    public Action? QuitRequested { get; set; }
    public string HotkeyStatus => _hotkeys.HotkeyStatus;
    public event Action<string>? HotkeyStatusChanged
    {
        add => _hotkeys.HotkeyStatusChanged += value;
        remove => _hotkeys.HotkeyStatusChanged -= value;
    }
    internal StoreData Data => _store.Data;
    internal Settings Settings => _settingsService.Current;
    internal SettingsService SettingsService => _settingsService;

    public PadWindow(Store store, SettingsService settingsService, RuntimeLog log, bool isDev, bool softwareRendering, NotesSession notes)
    {
        _store = store;
        _settingsService = settingsService;
        _notes = notes;
        _timers = new(id => store.Data.Stack.Any(n => n.Id == id) || store.Data.Slots.Any(n => n.Id == id));
        _log = log;
        _isDev = isDev;
        _softwareRendering = softwareRendering;
        _trace = new TaskbarTrace(isDev, log.Write);
        _clipboardPolicy = new();
        _hotkeys = new HotkeyRegistrar(
            (gesture, onPressed) => new GlobalHotkeyRegistration(this, onPressed, gesture),
            (gesture, onPressed) => new GlobalHotkeyRegistration(this, onPressed, gesture, id: 0x4151, fromEnvironment: false),
            pressed => Dispatcher.UIThread.Post(() => ToggleResident(pressed)),
            () => Dispatcher.UIThread.Post(() => { ShowResident(); NewNote(); }),
            text => Toast(text));
        Title = AppInfo.DisplayName;
        Icon = new WindowIcon(Avalonia.Platform.AssetLoader.Open(new Uri(AppInfo.IconUri)));
        Anim.Reduced = Settings.ReduceMotion;
        _palette = ResolvePalette();
        Width = Math.Max(320, Settings.Width) * (Settings.Split ? 2 : 1); Height = Math.Max(260, Settings.Height);
        MinWidth = 320; MinHeight = 260;
        if (Settings.X is { } x && Settings.Y is { } y && OnAnyScreen(x, y)) { Position = new PixelPoint(x, y); WindowStartupLocation = WindowStartupLocation.Manual; }
        else WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowDecorations = WindowDecorations.BorderOnly;
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = 36;
        ShowInTaskbar = false;
        Topmost = Settings.Pinned;
        Background = Brushes.Transparent;
        FontFamily = new FontFamily(Typography.UiFamily);
        // Grayscale antialiasing: ClearType fringes and smears small text over a translucent, blurred backdrop.
        TextOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        BuildLayout();
        ApplyAppearance();
        BindPanes(arrive: 0);
        _saveTimer.Tick += (_, _) => { _saveTimer.Stop(); QueueSave(); };
        _trim.Tick += (_, _) => TrimWhileHidden();
        _timers.Finished += OnTimerFinished;
        Completion.OcrRequested += pad => _ = RunOcr(pad);
        AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);
        AddHandler(PointerWheelChangedEvent, OnWheel, RoutingStrategies.Tunnel, handledEventsToo: true);
        PointerMoved += (_, e) => { WakeChrome(); OverlayPointer(e); };
        PointerExited += (_, _) => SleepChrome(soon: true);
        Deactivated += (_, _) =>
        {
            if (SessionEnd.IsEnding) return;
            if (_isDev) _trace.Write($"pad Avalonia.Deactivated visible={IsVisible} active={IsActive} {_trace.PadActionContext()}");
            QueueSave();
            if (_saveFailureOverlay is { } prompt && IsOverlay(prompt)) CloseOverlay();
            var taskbarTransfer = _taskbarProxy?.ConsumePadDeactivationToProxy() ?? false;
            // Moving into the Settings window is not "clicking away": the pad stays up, undimmed, to show each change.
            if (_settingsWindow is not null) return;
            if (Settings.HideOnBlur && !Topmost && !_overlayOpen && IsVisible && !taskbarTransfer) QueueHideOnBlur();
            else if (Topmost && Settings.DimWhenPinned) FadeWindow(0.72);
        };
        Activated += (_, _) =>
        {
            if (SessionEnd.IsEnding) return;
            if (_isDev) _trace.Write($"pad Avalonia.Activated visible={IsVisible} active={IsActive} {_trace.PadActionContext()}");
            CancelQueuedHideOnBlur(); WakeChrome(); FadeWindow(1);
        };
        Closing += (_, e) =>
        {
            if (e.CloseReason == WindowCloseReason.OSShutdown) PrepareForSessionEnd();
            if (SessionEnd.ShouldCancelClose(e.CloseReason, _quitting, SessionEnd.IsEnding))
            {
                e.Cancel = true;
                HideResident();
            }
        };
        Opened += (_, _) => { FocusActive(); CycleSettingsForDev(); };
        PositionChanged += (_, _) => RememberBounds();
        SizeChanged += (_, _) => RememberBounds();
        if (Application.Current?.PlatformSettings is { } ps) ps.ColorValuesChanged += (_, _) => Dispatcher.UIThread.Post(ApplyAppearance);
    }

    /// <summary>Hotkey and clipboard hooks need only the native handle, so `--hidden` startup works without ever showing.</summary>
    internal void InstallHooks()
    {
        if (Settings.Taskbar != "never") EnsureTaskbarProxy();
        RegisterHotkey(Settings.Hotkey);
        RegisterNewNoteHotkey(Settings.NewNoteHotkey);        _clipboardHook = DesktopPlatform.ListenClipboard(this, () => Dispatcher.UIThread.Post(() => _ = OnClipboardChanged()));
        ApplyTaskbarSetting();
    }

    // ---------- lifecycle ----------

    public void ToggleResident() => ToggleResident(hotkeyPressed: 0);

    private void ToggleResident(long hotkeyPressed)
    {
        if (IsVisible && IsActive) HideResident("hotkey-toggle");
        else ShowResident(hotkeyPressed, "hotkey-toggle");
    }

    public void ShowResident() => ShowResident(hotkeyPressed: 0, cause: "api-show");

    private void ShowResident(long hotkeyPressed, string cause = "show")
    {
        if (SessionEnd.IsEnding) return;
        _trace.TracePadAction(cause, "show-begin", IsVisible);
        CancelQueuedHideOnBlur();
        var away = DateTime.Now - _hiddenAt;
        if (!IsVisible && Settings.FreshNoteAfterMinutes > 0 && away.TotalMinutes >= Settings.FreshNoteAfterMinutes) OpenFreshNote();
        var wasVisible = IsVisible;
        var drop = wasVisible ? null : PlaceForOpen();
        WindowState = WindowState.Normal;
        Show();
        ApplyTaskbarSetting();
        Activate();
        FocusActive();
        // sync = keyboard focus set inside this call; ms = when the dispatcher next confirms it after the first frames.
        var sync = hotkeyPressed == 0 ? 0 : System.Diagnostics.Stopwatch.GetElapsedTime(hotkeyPressed).TotalMilliseconds;
        Dispatcher.UIThread.Post(() =>
        {
            FocusActive();
            if (hotkeyPressed != 0)
            {
                _log.Write($"hotkey-to-focus ms={System.Diagnostics.Stopwatch.GetElapsedTime(hotkeyPressed).TotalMilliseconds:F1} sync={sync:F1} focused={_active.Editor.TextArea.IsFocused} open-at={Settings.OpenAt}");
            }
        }, DispatcherPriority.Input);
        if (drop is { } target)
        {
            // Drop-down presentation: the pad slides down from the top edge, Quake-console style.
            var from = Position.Y;
            Anim.Run(this, MotionTokens.WindowSlide, t => Position = new PixelPoint(target.X, (int)(from + (target.Y - from) * t)), () => Position = target);
        }
        else if (!wasVisible) PlayShow();
        WakeChrome();
        _trace.TracePadAction(cause, "show-end", IsVisible);
    }

    /// <summary>Where the pad appears: where it was left, centred, under the mouse, or above the tray like a flyout.</summary>
    private PixelPoint? PlaceForOpen()
    {
        if (Settings.OpenAt == "remember") return null;
        var cursor = DesktopPlatform.CursorPosition();
        var screen = Settings.OpenAt is "mouse" or "dropdown" ? Screens.ScreenFromPoint(cursor) ?? Screens.Primary : Screens.Primary;
        if (screen is null) return null;
        var a = screen.WorkingArea; var scale = screen.Scaling;
        var w = (int)(Width * scale); var h = (int)(Height * scale);
        var (x, y) = Settings.OpenAt switch
        {
            "center" => (a.X + (a.Width - w) / 2, a.Y + (a.Height - h) / 2),
            "tray" => (a.Right - w - (int)(12 * scale), a.Bottom - h - (int)(12 * scale)),
            "dropdown" => (a.X + (a.Width - w) / 2, a.Y + (int)(10 * scale)),
            _ => (cursor.X - w / 2, cursor.Y - (int)(24 * scale)),
        };
        var final = new PixelPoint(Math.Clamp(x, a.X, Math.Max(a.X, a.Right - w)), Math.Clamp(y, a.Y, Math.Max(a.Y, a.Bottom - h)));
        if (Settings.OpenAt == "dropdown" && !Settings.ReduceMotion) { Position = new PixelPoint(final.X, a.Y - h); return final; }
        Position = final;
        return null;
    }

    /// <summary>Hidden start: lay out and render once, invisibly, so the first hotkey press is as fast as the rest.</summary>
    internal void Prewarm()
    {
        _trace.TracePadAction("prewarm-show", "show-begin", IsVisible);
        Opacity = 0; ShowActivated = false;
        Show();
        Dispatcher.UIThread.Post(() =>
        {
            _trace.TracePadAction("prewarm-hide", "hide-begin", IsVisible);
            Hide();
            Opacity = 1; ShowActivated = true; _hiddenAt = DateTime.Now; _trim.Start();
            ApplyTaskbarSetting();
            _trace.TracePadAction("prewarm-hide", "hide-end", IsVisible);
        }, DispatcherPriority.Background);
    }

    /// <summary>Appear without taking focus (finished timers): the user's current app keeps the keyboard.</summary>
    internal void ShowQuietly()
    {
        if (IsVisible) return;
        _trace.TracePadAction("timer-quiet-show", "show-begin", IsVisible);
        CancelQueuedHideOnBlur();
        ShowActivated = false;
        Show();
        ShowActivated = true;
        ApplyTaskbarSetting();
        _trace.TracePadAction("timer-quiet-show", "show-end", IsVisible);
    }

    public void HideResident() => HideResident("hide");

    public void HideResident(string cause)
    {
        if (SessionEnd.IsEnding) return;
        _trace.TracePadAction(cause, "hide-begin", IsVisible);
        CancelQueuedHideOnBlur();
        CloseOverlay();
        QueueSave();
        _hiddenAt = DateTime.Now;
        Hide();
        _trim.Stop(); _trim.Start();
        ApplyTaskbarSetting();
        _trace.TracePadAction(cause, "hide-end", IsVisible);
    }

    public void Quit()
    {
        if (SessionEnd.IsEnding) return;
        if (!Save(showFailureToast: false))
        {
            if (!IsVisible) ShowResident();
            ShowSaveFailurePrompt(_store.LastError, RetryQuit, QuitAnyway);
            return;
        }

        CompleteQuit();
    }

    /// <summary>One synchronous save before Avalonia closes any windows; no prompt, retry or eviction.</summary>
    internal void PrepareForSessionEnd()
    {
        if (!SessionEnd.Begin()) return;
        var started = System.Diagnostics.Stopwatch.GetTimestamp();
        try
        {
            _saveTimer.Stop();
            _trim.Stop();
            CancelQueuedHideOnBlur();
            _left.Flush();
            _right.Flush();
            // Capture the latest edit and drain every queued write before accepting session end.
            var saved = _store.Save() && _store.LastError is null;
            _log.Write($"session-end save={(saved ? "ok" : "failed")} ms={System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds:F1}" +
                (saved ? "" : $" error={_store.LastError}"));
        }
        catch (Exception ex)
        {
            _log.Write($"session-end save=failed ms={System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds:F1} error={ex.Message}");
        }
        finally { _log.Flush(); }
    }

    private void RetryQuit()
    {
        if (Save(showFailureToast: false)) CompleteQuit();
        else ShowSaveFailurePrompt(_store.LastError, RetryQuit, QuitAnyway);
    }

    private void QuitAnyway() => CompleteQuit();

    private void CompleteQuit()
    {
        // Even Quit anyway must let an in-flight write finish before the process exits.
        _store.Flush();
        _log.Flush();
        _quitting = true;
        CloseSettings();
        _hotkeys.Dispose();
        _clipboardHook?.Dispose();
        _taskbarProxy?.Dispose(); _taskbarProxy = null;
        Close();
        QuitRequested?.Invoke();
    }

    private void PlayShow()
    {
        // The window is focusable the moment it is shown; only the content fades in.
        var shift = new TranslateTransform();
        _content.RenderTransform = shift;
        Anim.Run(this, MotionTokens.ContentReveal, t => { _content.Opacity = 0.35 + 0.65 * t; shift.Y = 6 * (1 - t); },
            () => { _content.Opacity = 1; _content.RenderTransform = null; });
    }

    private Anim.Handle? _fade;

    /// <summary>Fades the whole pad (pinned and inactive: it steps back; active again: full strength).</summary>
    private void FadeWindow(double to)
    {
        _fade?.Cancel();
        var from = Opacity;
        if (Math.Abs(from - to) < 0.01) return;
        _fade = Anim.Run(this, MotionTokens.WindowFade, t => Opacity = from + (to - from) * t, decorative: false);
    }

    private void FocusActive() { if (IsVisible && !_overlayOpen) _active.FocusText(); }

    /// <summary>Applies the user's taskbar choice after a settings change or a pad show/hide.</summary>
    internal void ApplyTaskbarSetting()
    {
        var visible = IsVisible;
        var shouldShow = TaskbarPresence.ShouldShow(Settings.Taskbar, visible);
        if (shouldShow) _taskbarProxy?.ShowMinimized();
        else _taskbarProxy?.Hide();
    }

    private void EnsureTaskbarProxy() =>
        _taskbarProxy ??= new TaskbarProxy(this, AppInfo.DisplayName, Icon, OnTaskbarProxyClick, _isDev, _trace);

    private void OnTaskbarProxyClick(bool hide, string source)
    {
        var cause = $"taskbar-click:{source}";
        if (_isDev) _trace.Write($"pad taskbar callback source={source} action={(hide ? "hide" : IsVisible ? "focus" : "show")} visible={IsVisible}");
        CancelQueuedHideOnBlur();
        if (hide)
        {
            if (IsVisible) HideResident(cause);
        }
        else ShowResident(0, cause);
    }

    private int _blurHideGeneration;

    private void QueueHideOnBlur()
    {
        var generation = ++_blurHideGeneration;
        if (_isDev) _trace.Write($"pad hide-on-blur queued generation={generation} visible={IsVisible} active={IsActive}");
        Dispatcher.UIThread.Post(() =>
        {
            if (generation != _blurHideGeneration || IsActive || !IsVisible || _settingsWindow is not null || !Settings.HideOnBlur || Topmost || _overlayOpen)
            {
                if (_isDev) _trace.Write($"pad hide-on-blur skipped generation={generation} current={_blurHideGeneration} visible={IsVisible} active={IsActive} settings={_settingsWindow is not null}");
                return;
            }
            if (_isDev) _trace.Write($"pad hide-on-blur fired generation={generation}");
            HideResident("hide-on-blur");
        }, DispatcherPriority.Background);
    }

    private void CancelQueuedHideOnBlur() => _blurHideGeneration++;

    // ---------- persistence ----------

    private void Edited(bool sleepChrome = true)
    {
        ScheduleSave();
        UpdateChrome();
        if (sleepChrome) SleepChrome(soon: false);
    }

    private void ScheduleSave()
    {
        _saveTimer.Stop(); _saveTimer.Start();
    }

    internal bool Save() => Save(showFailureToast: true);

    private async void QueueSave()
    {
        if (SessionEnd.IsEnding || _quitting) return;
        _left.Flush(); _right.Flush();
        try
        {
            var saved = await _store.QueueSave();
            if (SessionEnd.IsEnding || _quitting) return;
            if (!saved) Toast("Couldn't save · " + _store.LastError);
            else PadView.Evict(KeepDocuments, [_left.Note, _right.Note]);
        }
        catch (Exception ex)
        {
            if (!SessionEnd.IsEnding && !_quitting) Toast("Couldn't save · " + ex.Message);
        }
    }

    private bool Save(bool showFailureToast)
    {
        // Closing/deactivation can call Save again after the session-end flush. Never show UI
        // or repeat disk work on that path, even if the single shutdown save failed.
        if (SessionEnd.IsEnding) return true;
        _left.Flush(); _right.Flush();
        if (!_store.Save())
        {
            if (showFailureToast) Toast("Couldn't save · " + _store.LastError);
            return false;
        }
        // Keep editor documents (and undo) for the few notes you swipe between; older ones go back to disk.
        PadView.Evict(KeepDocuments, [_left.Note, _right.Note]);
        return true;
    }

    private const int KeepDocuments = 6;
    // Long enough that hide-and-show within a minute never pays for paging memory back in.
    private readonly DispatcherTimer _trim = new() { Interval = TimeSpan.FromSeconds(TrimDelaySeconds) };
    private static double TrimDelaySeconds => double.TryParse(Environment.GetEnvironmentVariable(AppInfo.Env("TRIM_SECONDS")), out var s) ? s : 45;

    /// <summary>
    /// 45 s after hiding: release every cold document, compact the heap and hand unused pages back to
    /// Windows. A quick hide-and-show inside the delay skips all of it, so toggling stays instant.
    /// </summary>
    private void TrimWhileHidden()
    {
        _trim.Stop();
        if (IsVisible) return;
        PadView.Evict(0, [_left.Note, _right.Note]);
        _store.RemoveOrphans();
        System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        DesktopPlatform.TrimWorkingSet();
        if (_isDev) _log.Write($"trimmed docs={PadView.CachedDocuments} managed={GC.GetTotalMemory(false) / 1048576.0:F1}MB");
    }

    private void RememberBounds()
    {
        if (WindowState != WindowState.Normal || !IsVisible || _sizingForSplit) return;
        _settingsService.Update(settings =>
        {
            settings.X = Position.X; settings.Y = Position.Y;
            settings.Width = settings.Split ? Width / 2 : Width; settings.Height = Height;
        });
    }

    private bool OnAnyScreen(int x, int y) =>
        Screens.All.Any(s => s.WorkingArea.Contains(new PixelPoint(x + 40, y + 20)));

    // ---------- hotkey ----------

    internal bool RegisterHotkey(string? gesture) => _hotkeys.RegisterHotkey(gesture);
    internal bool RegisterNewNoteHotkey(string? gesture) => _hotkeys.RegisterNewNoteHotkey(gesture);
    internal string NewNoteHotkeyStatus => _hotkeys.NewNoteHotkeyStatus;

    // ---------- appearance ----------

    private bool IsDark => Settings.Theme switch
    {
        "light" => false,
        "dark" => true,
        _ => Application.Current?.PlatformSettings?.GetColorValues().ThemeVariant == PlatformThemeVariant.Dark
    };

    /// <summary>Windows' own high-contrast mode overrides palette choices with the contrast palettes.</summary>
    private static bool SystemHighContrast =>
        Application.Current?.PlatformSettings?.GetColorValues().ContrastPreference == Avalonia.Platform.ColorContrastPreference.High;

    private Palette ResolvePalette() => SystemHighContrast
        ? Palette.Get(IsDark ? "contrast-dark" : "contrast-light", IsDark)
        : Palette.Get(IsDark ? Settings.DarkPalette : Settings.LightPalette, IsDark).Customize(Settings.Accent, Settings.Tone);

    /// <summary>The split pane may wear its own palette, so a reference slot reads differently from the scratch note.</summary>
    private Palette SplitPalette() => Settings.SplitPalette == "same" ? _palette : Palette.Get(Settings.SplitPalette, IsDark).Customize(Settings.Accent, Settings.Tone);

    internal void ApplyAppearance()
    {
        _palette = ResolvePalette();
        Anim.Reduced = Settings.ReduceMotion;
        RequestedThemeVariant = _palette.Dark ? ThemeVariant.Dark : ThemeVariant.Light;
        ApplyPinRing(animate: _pinRingInitialized);
        // Re-applying the material or the styles makes the window flash, so each happens only when its input changed.
        if (_appliedMaterial != Settings.Material) { TransparencyLevelHint = Kit.Material(Settings.Material); _appliedMaterial = Settings.Material; }
        TransparencyBackgroundFallback = new SolidColorBrush(_palette.Canvas);
        var tint = Settings.Material == "solid" ? 1 : Math.Clamp(Settings.Tint, 0.3, 1);
        _tint.Background = new SolidColorBrush(_palette.Canvas, tint);
        var scale = Math.Clamp(Settings.UiScale, 0.8, 2);
        _scaler.LayoutTransform = Math.Abs(scale - 1) < 0.01 ? null : new ScaleTransform(scale, scale);
        Editor.CodeColors.Disabled = Settings.DisabledLanguages.ToHashSet();
        _left?.ApplyLook(_palette, Settings);
        _right?.ApplyLook(SplitPalette(), Settings);
        if (_right is not null) _right.Background = Settings.SplitPalette == "same" ? null : new SolidColorBrush(SplitPalette().Canvas, Settings.Material == "solid" ? 1 : Math.Clamp(Settings.Tint + .08, 0, 1));
        if (!Equals(_stylesPalette, _palette)) { Kit.InputStyles(Styles, _palette); _stylesPalette = _palette; }
        RebuildChrome();
        ApplyBars();
        ApplyTopBlur();
        if (_overlay is { } current) ReopenOverlay(current);
        _settingsWindow?.SyncLook();
    }

    private string? _appliedMaterial;
    private Palette? _stylesPalette;

    /// <summary>The palette the pad wears now (the Settings window dresses to match).</summary>
    internal Palette CurrentPalette => _palette;

    Palette ISettingsHost.CurrentPalette => CurrentPalette;
    WindowIcon? ISettingsHost.Icon => Icon;
    string ISettingsHost.HotkeyStatus => HotkeyStatus;
    string ISettingsHost.NewNoteHotkeyStatus => NewNoteHotkeyStatus;
    bool ISettingsHost.RegisterHotkey(string? gesture) => RegisterHotkey(gesture);
    bool ISettingsHost.RegisterNewNoteHotkey(string? gesture) => RegisterNewNoteHotkey(gesture);
    void ISettingsHost.Toast(string text, string? action, Action? onAction, bool brief) => Toast(text, action, onAction, brief);
    void ISettingsHost.OpenHelp() => OpenHelp();
    void ISettingsHost.ResetSettings() => ResetSettings();
    bool ISettingsHost.Save() => Save();
    void ISettingsHost.SettingsLostFocus() => SettingsLostFocus();
}
