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
using AntiNotepad.Platform;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

/// <summary>The resident pad window. Hidden, never closed, until Quit.</summary>
internal sealed partial class PadWindow : Window
{
    private const uint WmSysCommand = 0x0112;
    private const long ScMinimize = 0xF020;
    private readonly Store _store;
    private readonly Timers _timers = new(id => Program.Store.Data.Stack.Any(n => n.Id == id) || Program.Store.Data.Slots.Any(n => n.Id == id));
    private readonly DispatcherTimer _saveTimer = new() { Interval = TimeSpan.FromMilliseconds(400) };
    private Palette _palette;
    private bool _quitting;
    private bool _lifecycleReady;
    private bool _suppressWindowStateReconcile;
    private DateTime _hiddenAt = DateTime.Now;
    private IDisposable? _clipboardHook;
    private IDisposable? _nativeTransitionStyles;
    private IDisposable? _sessionTransitionSuppression;
    private IDisposable? _prewarmSuppression;
    private GlobalHotkey? _hotkey, _newNoteHotkey;
    private TaskbarProxy? _taskbarProxy;
    private Win32Properties.CustomWndProcHookCallback? _lifecycleHook;
    private Anim.Handle? _openSlide;
    private PixelPoint? _openSlideTarget;
    private int _prewarmGeneration;
    private bool _prewarmPending;
    private PadView _left = null!, _right = null!;
    private PadView _active = null!;

    public Action? QuitRequested { get; set; }
    public string HotkeyStatus { get; private set; } = "";
    public event Action<string>? HotkeyStatusChanged;
    internal StoreData Data => _store.Data;
    internal Settings Settings => _store.Data.Settings;
    internal PadVisibilityState VisibilityState => _prewarmPending ? PadVisibilityState.Closed : !IsVisible ? PadVisibilityState.Closed
        : WindowState == WindowState.Minimized ? PadVisibilityState.Minimized : PadVisibilityState.Open;
    internal bool IsPadOpen => VisibilityState == PadVisibilityState.Open;

    public PadWindow(Store store)
    {
        _store = store;
        Title = AppInfo.DisplayName;
        Icon = new WindowIcon(Avalonia.Platform.AssetLoader.Open(new Uri(AppInfo.IconUri)));
        Anim.Reduced = Settings.ReduceMotion;
        _palette = ResolvePalette();
        Width = Math.Max(320, Settings.Width) * (Settings.Split ? 2 : 1); Height = Math.Max(260, Settings.Height);
        MinWidth = 320; MinHeight = 260;
        if (Settings.X is { } x && Settings.Y is { } y && OnAnyScreen(x, y)) { Position = new PixelPoint(x, y); WindowStartupLocation = WindowStartupLocation.Manual; }
        else WindowStartupLocation = WindowStartupLocation.CenterScreen;
        // Register before Avalonia rewrites the Win32 styles for its frameless, extended-client window.
        _nativeTransitionStyles = Native.ConfigureNativeTransitions(this);
        _lifecycleHook = (IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, ref bool handled) =>
        {
            if (message == WmSysCommand && (wParam.ToInt64() & 0xFFF0) == ScMinimize)
                CancelOpenSlide(settle: true);
            return IntPtr.Zero;
        };
        Win32Properties.AddWndProcHookCallback(this, _lifecycleHook);
        WindowDecorations = WindowDecorations.BorderOnly;
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = 36;
        ShowInTaskbar = true;
        Topmost = Settings.Pinned;
        Background = Brushes.Transparent;
        FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI");
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
            if (Program.IsDev) TaskbarPresence.Trace($"pad Avalonia.Deactivated visible={IsVisible} active={IsActive} {TaskbarPresence.PadActionContext()}");
            QueueSave();
            if (_saveFailureOverlay is { } prompt && IsOverlay(prompt)) CloseOverlay();
            // Moving into the Settings window is not "clicking away": the pad stays up, undimmed, to show each change.
            if (_settingsWindow is not null) return;
            if (Settings.HideOnBlur && !Topmost && !_overlayOpen && IsPadOpen) QueueHideOnBlur();
            else if (Topmost && Settings.DimWhenPinned) FadeWindow(0.72);
        };
        Activated += (_, _) =>
        {
            if (SessionEnd.IsEnding) return;
            if (Program.IsDev) TaskbarPresence.Trace($"pad Avalonia.Activated visible={IsVisible} active={IsActive} {TaskbarPresence.PadActionContext()}");
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
        Closed += (_, _) =>
        {
            _sessionTransitionSuppression?.Dispose(); _sessionTransitionSuppression = null;
            if (_lifecycleHook is { } hook)
            {
                Win32Properties.RemoveWndProcHookCallback(this, hook);
                _lifecycleHook = null;
            }
            _nativeTransitionStyles?.Dispose(); _nativeTransitionStyles = null;
            TaskbarPresence.Dispose(this);
        };
        PositionChanged += (_, _) => RememberBounds();
        SizeChanged += (_, _) => RememberBounds();
        if (Application.Current?.PlatformSettings is { } ps) ps.ColorValuesChanged += (_, _) => Dispatcher.UIThread.Post(ApplyAppearance);
        _lifecycleReady = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (_lifecycleReady && !_suppressWindowStateReconcile && change.Property == Window.WindowStateProperty)
            OnExternalWindowStateChanged();
    }

    /// <summary>Hotkey and clipboard hooks need only the native handle, so `--hidden` startup works without ever showing.</summary>
    internal void InstallHooks()
    {
        TaskbarPresence.Initialize(this, ApplyTaskbarSetting);
        // Keep the OS-created app-window tab out of the way until the selected taskbar state is reconciled.
        TaskbarPresence.SetPadPresence(this, present: false, force: true);
        RegisterHotkey(Settings.Hotkey);
        RegisterNewNoteHotkey(Settings.NewNoteHotkey);        _clipboardHook = Native.ListenClipboard(this, () => Dispatcher.UIThread.Post(() => _ = OnClipboardChanged()));
    }

    // ---------- lifecycle ----------

    public void ToggleResident() => ToggleResident(hotkeyPressed: 0);

    private void ToggleResident(long hotkeyPressed)
    {
        if (IsPadOpen && IsActive) HideResident("hotkey-toggle");
        else ShowResident(hotkeyPressed, "hotkey-toggle");
    }

    public void ShowResident() => ShowResident(hotkeyPressed: 0, cause: "api-show");

    internal void ToggleFromTray()
    {
        if (IsPadOpen) HideResident("tray-toggle");
        else ShowResident(hotkeyPressed: 0, cause: "tray-toggle");
    }

    private void ShowResident(long hotkeyPressed, string cause = "show")
    {
        if (_quitting || SessionEnd.IsEnding) return;
        if (_prewarmPending) FinishPrewarm(_prewarmGeneration, applyTaskbar: false);
        var state = VisibilityState;
        TaskbarPresence.TracePadAction(cause, "show-begin", state);
        _prewarmGeneration++;
        CancelQueuedHideOnBlur();
        var away = DateTime.Now - _hiddenAt;
        if (state != PadVisibilityState.Open && Settings.FreshNoteAfterMinutes > 0 && away.TotalMinutes >= Settings.FreshNoteAfterMinutes) OpenFreshNote();
        var wasClosed = state == PadVisibilityState.Closed;
        var drop = wasClosed ? PlaceForOpen() : null;
        _trim.Stop();
        if (state == PadVisibilityState.Minimized) SetWindowState(WindowState.Normal);
        else if (wasClosed)
        {
            SetWindowState(WindowState.Normal);
            Show();
        }
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
                Program.Log($"hotkey-to-focus ms={System.Diagnostics.Stopwatch.GetElapsedTime(hotkeyPressed).TotalMilliseconds:F1} sync={sync:F1} focused={_active.Editor.TextArea.IsFocused} open-at={Settings.OpenAt}");
            }
        }, DispatcherPriority.Input);
        if (drop is { } target)
            StartDropSlide(target);
        WakeChrome();
        TaskbarPresence.TracePadAction(cause, "show-end", VisibilityState);
    }

    private void StartDropSlide(PixelPoint target)
    {
        CancelOpenSlide(settle: false);
        // This remains an explicit app motion option and runs after the native open/restore begins.
        if (Settings.ReduceMotion || !Native.AnimationsEnabled)
        {
            Position = target;
            return;
        }
        var from = Position.Y;
        _openSlideTarget = target;
        _openSlide = Anim.Run(this, 150,
            t => Position = new PixelPoint(target.X, (int)(from + (target.Y - from) * t)),
            () =>
            {
                Position = target;
                _openSlide = null;
                _openSlideTarget = null;
            });
    }

    private void CancelOpenSlide(bool settle)
    {
        _openSlide?.Cancel();
        _openSlide = null;
        var target = _openSlideTarget;
        _openSlideTarget = null;
        if (settle && target is { } final && IsPadOpen) Position = final;
    }

    /// <summary>Restores the resident window from the closed-state Always taskbar stand-in.</summary>
    private void RestoreFromClosedTaskbar()
    {
        if (_quitting || SessionEnd.IsEnding) return;
        if (VisibilityState != PadVisibilityState.Closed)
        {
            ShowResident(hotkeyPressed: 0, cause: "taskbar-restore");
            return;
        }

        const string cause = "taskbar-restore";
        TaskbarPresence.TracePadAction(cause, "restore-begin", VisibilityState);
        var away = DateTime.Now - _hiddenAt;
        if (Settings.FreshNoteAfterMinutes > 0 && away.TotalMinutes >= Settings.FreshNoteAfterMinutes) OpenFreshNote();
        var drop = PlaceForOpen();
        CancelQueuedHideOnBlur();
        _trim.Stop();
        Opacity = 0;
        ShowActivated = false;
        var suppress = Native.SuppressDwmTransitions(this);
        try
        {
            SetWindowState(WindowState.Normal);
            var previousReconcile = _suppressWindowStateReconcile;
            _suppressWindowStateReconcile = true;
            try
            {
                // Stage a normal no-activate Show first, then native-minimize it without Avalonia's activating path.
                Show();
                Native.ShowMinimizedNoActivate(this);
            }
            finally { _suppressWindowStateReconcile = previousReconcile; }
            // Keep the proxy as the only button until the real pad is fully staged as minimized.
            TaskbarPresence.SetPadPresence(this, present: false, force: true);
            _taskbarProxy?.Hide();
            TaskbarPresence.SetPadPresence(this, present: true, force: true);
            Opacity = 1;
            ShowActivated = true;
        }
        catch
        {
            suppress.Dispose();
            Opacity = 1;
            ShowActivated = true;
            throw;
        }
        suppress.Dispose();
        SetWindowState(WindowState.Normal);
        ApplyTaskbarSetting();
        Activate();
        FocusActive();
        Dispatcher.UIThread.Post(FocusActive, DispatcherPriority.Input);
        if (drop is { } target) StartDropSlide(target);
        WakeChrome();
        TaskbarPresence.TracePadAction(cause, "restore-end", VisibilityState);
    }

    /// <summary>Where the pad appears: where it was left, centred, under the mouse, or above the tray like a flyout.</summary>
    private PixelPoint? PlaceForOpen()
    {
        if (Settings.OpenAt == "remember") return null;
        var cursor = Native.CursorPosition();
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
        if (Settings.OpenAt == "dropdown" && !Settings.ReduceMotion && Native.AnimationsEnabled) { Position = new PixelPoint(final.X, a.Y - h); return final; }
        Position = final;
        return null;
    }

    /// <summary>Hidden start: lay out and render once, invisibly, so the first hotkey press is as fast as the rest.</summary>
    internal void Prewarm()
    {
        var generation = ++_prewarmGeneration;
        _prewarmPending = true;
        TaskbarPresence.TracePadAction("prewarm-show", "show-begin", VisibilityState);
        Opacity = 0; ShowActivated = false;
        _prewarmSuppression = Native.SuppressDwmTransitions(this);
        Show();
        TaskbarPresence.SetPadPresence(this, present: false, force: true);
        Dispatcher.UIThread.Post(() =>
        {
            FinishPrewarm(generation, applyTaskbar: true);
        }, DispatcherPriority.Background);
    }

    private void FinishPrewarm(int generation, bool applyTaskbar)
    {
        if (!_prewarmPending || generation != _prewarmGeneration) return;
        _prewarmPending = false;
        _prewarmGeneration++;
        TaskbarPresence.TracePadAction("prewarm-hide", "hide-begin", PadVisibilityState.Open);
        var suppress = _prewarmSuppression;
        _prewarmSuppression = null;
        try
        {
            if (IsVisible)
            {
                using (suppress) Hide();
            }
            else suppress?.Dispose();
        }
        finally
        {
            SetWindowState(WindowState.Normal);
            Opacity = 1;
            ShowActivated = true;
            _hiddenAt = DateTime.Now;
            _trim.Start();
            if (applyTaskbar) ApplyTaskbarSetting();
            TaskbarPresence.TracePadAction("prewarm-hide", "hide-end", VisibilityState);
        }
    }

    /// <summary>Appear without taking focus (finished timers): the user's current app keeps the keyboard.</summary>
    internal void ShowQuietly()
    {
        if (_quitting || SessionEnd.IsEnding) return;
        if (_prewarmPending) FinishPrewarm(_prewarmGeneration, applyTaskbar: false);
        if (IsPadOpen) return;
        TaskbarPresence.TracePadAction("timer-quiet-show", "show-begin", VisibilityState);
        CancelQueuedHideOnBlur();
        var state = VisibilityState;
        ShowActivated = false;
        using (Native.SuppressDwmTransitions(this))
        {
            if (state == PadVisibilityState.Minimized) Native.RestoreNoActivate(this);
            else if (state == PadVisibilityState.Closed)
            {
                SetWindowState(WindowState.Normal);
                Show();
            }
        }
        ShowActivated = true;
        ApplyTaskbarSetting();
        Dispatcher.UIThread.Post(ApplyTaskbarSetting, DispatcherPriority.Background);
        TaskbarPresence.TracePadAction("timer-quiet-show", "show-end", VisibilityState);
    }

    public void HideResident() => HideResident("hide");

    public void HideResident(string cause)
    {
        if (_quitting || SessionEnd.IsEnding) return;
        TaskbarPresence.TracePadAction(cause, "hide-begin", VisibilityState);
        CancelQueuedHideOnBlur();
        CloseOverlay();
        CancelOpenSlide(settle: true);
        HideBarsForWindowHidden();
        QueueSave();
        _hiddenAt = DateTime.Now;
        Hide();
        _trim.Stop(); _trim.Start();
        ApplyTaskbarSetting();
        TaskbarPresence.TracePadAction(cause, "hide-end", VisibilityState);
    }

    private void OnExternalWindowStateChanged()
    {
        if (_quitting || SessionEnd.IsEnding) return;
        var state = VisibilityState;
        if (Program.IsDev) TaskbarPresence.Trace($"pad WindowState changed state={state} visible={IsVisible} active={IsActive} {TaskbarPresence.PadActionContext()}");
        if (state == PadVisibilityState.Minimized)
        {
            _hiddenAt = DateTime.Now;
            CancelQueuedHideOnBlur();
            QueueSave();
            _trim.Stop(); _trim.Start();
            CancelOpenSlide(settle: false);
            HideBarsForWindowHidden();
            if (_overlayOpen) CloseOverlay();
        }
        else if (state == PadVisibilityState.Open)
        {
            CancelQueuedHideOnBlur();
            _trim.Stop();
            if (IsActive)
            {
                WakeChrome();
                FocusActive();
                Dispatcher.UIThread.Post(FocusActive, DispatcherPriority.Input);
            }
        }
        ApplyTaskbarSetting();
    }

    private void SetWindowState(WindowState state)
    {
        _suppressWindowStateReconcile = true;
        try { WindowState = state; }
        finally { _suppressWindowStateReconcile = false; }
    }

    public void Quit()
    {
        if (SessionEnd.IsEnding) return;
        if (!Save(showFailureToast: false))
        {
            if (!IsPadOpen) ShowResident();
            ShowSaveFailurePrompt(_store.LastError, RetryQuit, QuitAnyway);
            return;
        }

        CompleteQuit();
    }

    /// <summary>One synchronous save before Avalonia closes any windows; no prompt, retry or eviction.</summary>
    internal void PrepareForSessionEnd()
    {
        if (!SessionEnd.Begin()) return;
        if (_prewarmPending) FinishPrewarm(_prewarmGeneration, applyTaskbar: false);
        _sessionTransitionSuppression ??= Native.SuppressDwmTransitions(this);
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
            Program.Log($"session-end save={(saved ? "ok" : "failed")} ms={System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds:F1}" +
                (saved ? "" : $" error={_store.LastError}"));
        }
        catch (Exception ex)
        {
            Program.Log($"session-end save=failed ms={System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds:F1} error={ex.Message}");
        }
        finally { Program.FlushLogs(); }
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
        Program.FlushLogs();
        _sessionTransitionSuppression ??= Native.SuppressDwmTransitions(this);
        _quitting = true;
        CloseSettings();
        _hotkey?.Dispose();
        _newNoteHotkey?.Dispose();
        _clipboardHook?.Dispose();
        _taskbarProxy?.Dispose(); _taskbarProxy = null;
        Close();
        QuitRequested?.Invoke();
    }

    private Anim.Handle? _fade;

    /// <summary>Fades the whole pad (pinned and inactive: it steps back; active again: full strength).</summary>
    private void FadeWindow(double to)
    {
        _fade?.Cancel();
        var from = Opacity;
        if (Math.Abs(from - to) < 0.01) return;
        _fade = Anim.Run(this, 160, t => Opacity = from + (to - from) * t, decorative: false);
    }

    private void FocusActive() { if (ShowActivated && IsPadOpen && !_overlayOpen) _active.FocusText(); }

    /// <summary>Applies the user's taskbar choice after a settings change or a pad show/hide.</summary>
    internal void ApplyTaskbarSetting()
    {
        if (_quitting || SessionEnd.IsEnding)
        {
            _taskbarProxy?.Hide();
            return;
        }
        var state = VisibilityState;
        if (TaskbarPresence.ShouldShowProxy(Settings.Taskbar, state))
        {
            TaskbarPresence.SetPadPresence(this, present: false);
            EnsureTaskbarProxy().ShowMinimized();
            return;
        }

        _taskbarProxy?.Hide();
        TaskbarPresence.SetPadPresence(this, TaskbarPresence.ShouldShowPad(Settings.Taskbar, state));
    }

    private TaskbarProxy EnsureTaskbarProxy() =>
        _taskbarProxy ??= new TaskbarProxy(AppInfo.DisplayName, Icon, RestoreFromClosedTaskbar);

    private int _blurHideGeneration;

    private void QueueHideOnBlur()
    {
        var generation = ++_blurHideGeneration;
        if (Program.IsDev) TaskbarPresence.Trace($"pad hide-on-blur queued generation={generation} visible={IsPadOpen} active={IsActive}");
        Dispatcher.UIThread.Post(() =>
        {
            if (generation != _blurHideGeneration || IsActive || !IsPadOpen || _settingsWindow is not null || !Settings.HideOnBlur || Topmost || _overlayOpen)
            {
                if (Program.IsDev) TaskbarPresence.Trace($"pad hide-on-blur skipped generation={generation} current={_blurHideGeneration} visible={IsPadOpen} active={IsActive} settings={_settingsWindow is not null}");
                return;
            }
            if (Program.IsDev) TaskbarPresence.Trace($"pad hide-on-blur fired generation={generation}");
            HideResident("hide-on-blur");
        }, DispatcherPriority.Background);
    }

    private void CancelQueuedHideOnBlur() => _blurHideGeneration++;

    // ---------- persistence ----------

    private void Edited()
    {
        _saveTimer.Stop(); _saveTimer.Start();
        UpdateChrome();
        SleepChrome(soon: false);
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
        if (IsPadOpen) return;
        PadView.Evict(0, [_left.Note, _right.Note]);
        _store.RemoveOrphans();
        System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        Native.TrimWorkingSet();
        if (Program.IsDev) Program.Log($"trimmed docs={PadView.CachedDocuments} managed={GC.GetTotalMemory(false) / 1048576.0:F1}MB");
    }

    private void RememberBounds()
    {
        if (WindowState != WindowState.Normal || !IsVisible || _sizingForSplit) return;
        Settings.X = Position.X; Settings.Y = Position.Y;
        Settings.Width = Settings.Split ? Width / 2 : Width; Settings.Height = Height;
        _saveTimer.Stop(); _saveTimer.Start();
    }

    private bool OnAnyScreen(int x, int y) =>
        Screens.All.Any(s => s.WorkingArea.Contains(new PixelPoint(x + 40, y + 20)));

    // ---------- hotkey ----------

    internal bool RegisterHotkey(string? gesture)
    {
        try
        {
            // No gesture: the dev override, else the default.
            var wanted = gesture ?? (Environment.GetEnvironmentVariable(AppInfo.Env("HOTKEY")) is { Length: > 0 } env ? env : GlobalHotkey.DefaultGesture.Label);
            var label = GlobalHotkey.HotkeyGesture.Parse(wanted).Label;
            var ok = SwapHotkey(ref _hotkey, wanted, label,
                g => new GlobalHotkey(this, pressed => Dispatcher.UIThread.Post(() => ToggleResident(pressed)), g), out var error);
            HotkeyStatus = _hotkey?.GestureLabel ?? $"{label} unavailable: {error}";
            HotkeyStatusChanged?.Invoke(HotkeyStatus);
            return ok;
        }
        catch (FormatException ex) { Toast(ex.Message); return false; }
    }

    /// <summary>
    /// Replaces a registered hotkey. Windows keeps two registrations with the same id side by side and may release either,
    /// so the old key is released first, and put back if Windows refuses the new one. The same key again changes nothing.
    /// </summary>
    private static bool SwapHotkey(ref GlobalHotkey? slot, string wanted, string label, Func<string, GlobalHotkey> make, out string? error)
    {
        error = null;
        if (slot is { IsRegistered: true } && string.Equals(slot.GestureLabel, label, StringComparison.OrdinalIgnoreCase)) return true;
        var previous = slot?.GestureLabel;
        slot?.Dispose(); slot = null;
        var next = make(wanted);
        if (next.IsRegistered) { slot = next; return true; }
        error = next.RegistrationErrorMessage;
        next.Dispose();
        if (previous is not null)
        {
            var back = make(previous);
            if (back.IsRegistered) slot = back; else back.Dispose();
        }
        return false;
    }

    /// <summary>A second global hotkey that shows the pad on a fresh note. Empty turns it off. Returns false if Windows refused it.</summary>
    internal bool RegisterNewNoteHotkey(string? gesture)
    {
        if (string.IsNullOrWhiteSpace(gesture)) { _newNoteHotkey?.Dispose(); _newNoteHotkey = null; return true; }
        try
        {
            var label = GlobalHotkey.HotkeyGesture.Parse(gesture).Label;
            return SwapHotkey(ref _newNoteHotkey, gesture, label,
                g => new GlobalHotkey(this, _ => Dispatcher.UIThread.Post(() => { ShowResident(); NewNote(); }), g, id: 0x4151, fromEnvironment: false), out _);
        }
        catch (FormatException ex) { Toast(ex.Message); return false; }
    }

    internal string NewNoteHotkeyStatus => _newNoteHotkey?.GestureLabel ?? "off";

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
        if (_overlay is { } current) ReopenOverlay(current);
        _settingsWindow?.SyncLook();
    }

    private string? _appliedMaterial;
    private Palette? _stylesPalette;

    /// <summary>The palette the pad wears now (the Settings window dresses to match).</summary>
    internal Palette CurrentPalette => _palette;
}
