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

/// <summary>The one resident window. Hidden, never closed, until Quit.</summary>
internal sealed partial class PadWindow : Window
{
    private readonly Store _store;
    private readonly Timers _timers = new();
    private readonly DispatcherTimer _saveTimer = new() { Interval = TimeSpan.FromMilliseconds(400) };
    private Palette _palette;
    private bool _quitting;
    private DateTime _hiddenAt = DateTime.Now;
    private IDisposable? _clipboardHook;
    private GlobalHotkey? _hotkey, _newNoteHotkey;
    private PadView _left = null!, _right = null!;
    private PadView _active = null!;

    public Action? QuitRequested { get; set; }
    public string HotkeyStatus { get; private set; } = "";
    public event Action<string>? HotkeyStatusChanged;
    internal StoreData Data => _store.Data;
    internal Settings Settings => _store.Data.Settings;

    public PadWindow(Store store)
    {
        _store = store;
        Title = Program.IsDev ? $"{AppInfo.Name} (dev)" : AppInfo.Name;
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
        FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI");
        // Grayscale antialiasing: ClearType fringes and smears small text over a translucent, blurred backdrop.
        TextOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        BuildLayout();
        ApplyAppearance();
        BindPanes(arrive: 0);
        _saveTimer.Tick += (_, _) => { _saveTimer.Stop(); Save(); };
        _trim.Tick += (_, _) => TrimWhileHidden();
        _timers.Finished += OnTimerFinished;
        Completion.OcrRequested += pad => _ = RunOcr(pad);
        AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);
        AddHandler(PointerWheelChangedEvent, OnWheel, RoutingStrategies.Tunnel, handledEventsToo: true);
        PointerMoved += (_, e) => { WakeChrome(); OverlayPointer(e); };
        PointerExited += (_, _) => SleepChrome(soon: true);
        Deactivated += (_, _) =>
        {
            Save();
            // Moving into the Settings window is not "clicking away": the pad stays up, undimmed, to show each change.
            if (_settingsWindow is not null) return;
            if (Settings.HideOnBlur && !Topmost && !_overlayOpen && IsVisible) HideResident();
            else if (Topmost && Settings.DimWhenPinned) FadeWindow(0.72);
        };
        Activated += (_, _) => { WakeChrome(); FadeWindow(1); };
        Closing += (_, e) => { if (!_quitting) { e.Cancel = true; HideResident(); } };
        Opened += (_, _) => { FocusActive(); CycleSettingsForDev(); };
        PositionChanged += (_, _) => RememberBounds();
        SizeChanged += (_, _) => RememberBounds();
        if (Application.Current?.PlatformSettings is { } ps) ps.ColorValuesChanged += (_, _) => Dispatcher.UIThread.Post(ApplyAppearance);
    }

    /// <summary>Hotkey and clipboard hooks need only the native handle, so `--hidden` startup works without ever showing.</summary>
    internal void InstallHooks()
    {
        RegisterHotkey(Settings.Hotkey);
        RegisterNewNoteHotkey(Settings.NewNoteHotkey);        _clipboardHook = Native.ListenClipboard(this, () => Dispatcher.UIThread.Post(() => _ = OnClipboardChanged()));
    }

    // ---------- lifecycle ----------

    public void ToggleResident() { if (IsVisible && IsActive) HideResident(); else ShowResident(); }

    private long _hotkeyPressed;

    public void ShowResident()
    {
        var away = DateTime.Now - _hiddenAt;
        if (!IsVisible && Settings.FreshNoteAfterMinutes > 0 && away.TotalMinutes >= Settings.FreshNoteAfterMinutes) OpenFreshNote();
        var wasVisible = IsVisible;
        var drop = wasVisible ? null : PlaceForOpen();
        WindowState = WindowState.Normal;
        Show();
        Activate();
        FocusActive();
        // sync = keyboard focus set inside this call; ms = when the dispatcher next confirms it after the first frames.
        var pressed = _hotkeyPressed;
        var sync = pressed == 0 ? 0 : System.Diagnostics.Stopwatch.GetElapsedTime(pressed).TotalMilliseconds;
        Dispatcher.UIThread.Post(() =>
        {
            FocusActive();
            if (Interlocked.Exchange(ref _hotkeyPressed, 0) is var t && t != 0)
                Program.Log($"hotkey-to-focus ms={System.Diagnostics.Stopwatch.GetElapsedTime(t).TotalMilliseconds:F1} sync={sync:F1} focused={_active.Editor.TextArea.IsFocused} open-at={Settings.OpenAt}");
        }, DispatcherPriority.Input);
        if (drop is { } target)
        {
            // Drop-down presentation: the pad slides down from the top edge, Quake-console style.
            var from = Position.Y;
            Anim.Run(this, 150, t => Position = new PixelPoint(target.X, (int)(from + (target.Y - from) * t)), () => Position = target);
        }
        else if (!wasVisible) PlayShow();
        WakeChrome();
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
        if (Settings.OpenAt == "dropdown" && !Settings.ReduceMotion) { Position = new PixelPoint(final.X, a.Y - h); return final; }
        Position = final;
        return null;
    }

    /// <summary>Hidden start: lay out and render once, invisibly, so the first hotkey press is as fast as the rest.</summary>
    internal void Prewarm()
    {
        Opacity = 0; ShowActivated = false;
        Show();
        Dispatcher.UIThread.Post(() => { Hide(); Opacity = 1; ShowActivated = true; _hiddenAt = DateTime.Now; _trim.Start(); }, DispatcherPriority.Background);
    }

    /// <summary>Appear without taking focus (finished timers): the user's current app keeps the keyboard.</summary>
    internal void ShowQuietly()
    {
        if (IsVisible) return;
        ShowActivated = false;
        Show();
        ShowActivated = true;
    }

    public void HideResident()
    {
        CloseOverlay();
        Save();
        _hiddenAt = DateTime.Now;
        Hide();
        _trim.Stop(); _trim.Start();
    }

    public void Quit()
    {
        Save();
        _quitting = true;
        CloseSettings();
        _hotkey?.Dispose();
        _newNoteHotkey?.Dispose();
        _clipboardHook?.Dispose();
        Close();
        QuitRequested?.Invoke();
    }

    private void PlayShow()
    {
        // The window is focusable the moment it is shown; only the content fades in.
        var shift = new TranslateTransform();
        _content.RenderTransform = shift;
        Anim.Run(this, 140, t => { _content.Opacity = 0.35 + 0.65 * t; shift.Y = 6 * (1 - t); },
            () => { _content.Opacity = 1; _content.RenderTransform = null; });
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

    private void FocusActive() { if (IsVisible && !_overlayOpen) _active.FocusText(); }

    // ---------- persistence ----------

    private void Edited()
    {
        _saveTimer.Stop(); _saveTimer.Start();
        UpdateChrome();
        SleepChrome(soon: false);
    }

    internal void Save()
    {
        _left.Flush(); _right.Flush();
        if (!_store.Save()) { Toast("Couldn't save · " + _store.LastError); return; }
        // Keep editor documents (and undo) for the few notes you swipe between; older ones go back to disk.
        PadView.Evict(KeepDocuments, [_left.Note, _right.Note]);
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
                g => new GlobalHotkey(this, pressed => Dispatcher.UIThread.Post(() => { _hotkeyPressed = pressed; ToggleResident(); }), g), out var error);
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
