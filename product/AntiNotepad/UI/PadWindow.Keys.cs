// Routes pad commands, overlays, and settings changes from keyboard and chrome actions.
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using AntiNotepad;
using AntiNotepad.Model;
using AntiNotepad.Motion;
using AntiNotepad.Platform;

namespace AntiNotepad.UI;

internal sealed partial class PadWindow
{
    private Control? _overlay;
    private int _galleryShot;
    private bool _overlayOpen => _overlay is not null;

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var alt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        if (e.Key == Key.F12 && Environment.GetEnvironmentVariable(AppInfo.Env("SHOTS")) is { Length: > 0 } shots)
        {
            // Dev-only capture of the rendered window (the OS backdrop itself is not included).
            e.Handled = true;
            Directory.CreateDirectory(shots);
            var scale = 2.0;
            using var bitmap = new Avalonia.Media.Imaging.RenderTargetBitmap(new Avalonia.PixelSize((int)(Bounds.Width * scale), (int)(Bounds.Height * scale)), new Avalonia.Vector(96 * scale, 96 * scale));
            bitmap.Render(this);
            var name = e.KeyModifiers.HasFlag(KeyModifiers.Control) ? $"gallery-{++_galleryShot:00}.png"
                : e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? "shot-b.png" : "shot.png";
#pragma warning disable CS0618
            bitmap.Save(Path.Combine(shots, name));
#pragma warning restore CS0618
            return;
        }
        if (e.Key == Key.F11 && _isDev)
        {
            // Dev-only memory probe: managed heap vs what the GC has committed.
            var info = GC.GetGCMemoryInfo();
            _log.Write($"mem managed={GC.GetTotalMemory(false) / 1048576.0:F1}MB heap={info.HeapSizeBytes / 1048576.0:F1}MB committed={info.TotalCommittedBytes / 1048576.0:F1}MB docs={Editor.PadView.CachedDocuments} gen2={GC.CollectionCount(2)}");
            e.Handled = true;
            return;
        }
        if (e.Key == Key.Escape && Editor.Completion.IsOpen) return; // the popup closes itself
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            if (_overlayOpen) CloseOverlay(); else HideResident();
            return;
        }
        if (_overlayOpen)
        {
            // Overlays own their keys; a few globals still work.
            if (ctrl && e.Key == Key.OemComma) { e.Handled = true; OpenSettings(); }
            else if (ctrl && e.Key is Key.F or Key.P or Key.K) { e.Handled = true; OpenSearch(); }
            else if (ctrl && e.Key == Key.Z && _undoArmed && _toastAction is { } undoInOverlay) { e.Handled = true; undoInOverlay(); HideToast(); }
            return;
        }
        if (!ctrl && !alt && e.Key == Key.F1) { e.Handled = true; OpenHelp(); return; }
        if (e.Key is not (Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt)) SleepChrome(soon: false);
        if (alt && !ctrl)
        {
            switch (e.Key)
            {
                case Key.Left: e.Handled = true; Navigate(-1); return;
                case Key.Right: e.Handled = true; Navigate(1); return;
                case Key.Up when shift: e.Handled = true; MoveCurrent(-1); return;
                case Key.Down when shift: e.Handled = true; MoveCurrent(1); return;
            }
            return;
        }
        if (!ctrl) return;
        var digit = e.Key is >= Key.D1 and <= Key.D9 ? e.Key - Key.D0 : e.Key is >= Key.NumPad1 and <= Key.NumPad9 ? e.Key - Key.NumPad0 : 0;
        if (digit > 0)
        {
            e.Handled = true;
            if (shift) PromoteToSlot(digit); else OpenSlot(digit);
            return;
        }
        if (e.Key == Key.Z && !shift && _undoArmed && _toastAction is { } undo) { e.Handled = true; undo(); HideToast(); return; }
        switch (e.Key)
        {
            case Key.N when !shift: NewNote(); break;
            case Key.OemOpenBrackets or Key.PageUp when !shift: Navigate(-1); break;
            case Key.OemCloseBrackets or Key.PageDown when !shift: Navigate(1); break;
            case Key.D0 or Key.NumPad0 or Key.OemTilde: ShowStack(); break;
            case Key.Back when shift: DeleteCurrent(); break;
            case Key.F or Key.P or Key.K when !shift: OpenSearch(); break;
            case Key.OemComma: OpenSettings(); break;
            case Key.OemPipe or Key.OemBackslash: ToggleSplit(); break;
            case Key.T when !shift: TogglePin(); break;
            case Key.OemQuestion: OpenHelp(); break;
            case Key.O when shift: _ = RunOcr(_active); break;
            case Key.C when shift: _ = CopyText(_active.Editor.Text); Toast("Copied the whole note"); break;
            case Key.OemPlus or Key.Add: Zoom(1); break;
            case Key.OemMinus or Key.Subtract: Zoom(-1); break;
            case Key.Q when shift: Quit(); break;
            default: return;
        }
        e.Handled = true;
    }

    private void Zoom(int step)
    {
        var previous = Settings.FontSize;
        _settingsService.Update(settings => settings.FontSize = Math.Clamp(settings.FontSize + step, 10, 28));
        if (Settings.FontSize == previous)
        {
            _left.ApplyLook(_palette, Settings); _right.ApplyLook(_palette, Settings);
            Edited();
        }
        Toast($"Text size {Settings.FontSize:0}");
    }

    // ---------- overlays ----------

    private void ShowOverlay(Control overlay)
    {
        _left.Flush(); _right.Flush();
        _overlay = overlay;
        _overlayHost.Children.Clear();
        _overlayHost.Children.Add(overlay);
        _overlayHost.IsVisible = true;
        UpdateOverlayLayout();
        _panes.Opacity = 0.0;
        _pill.IsVisible = false;
        WakeChrome(hold: true);
        Anim.Run(_overlayHost, MotionTokens.OverlayIn, t => _overlayHost.Opacity = t);
    }

    internal bool IsOverlay(Control control) => ReferenceEquals(_overlay, control);

    internal void CloseOverlay()
    {
        if (_overlay is null) return;
        _overlay = null;
        _overlayHost.Children.Clear();
        _overlayHost.IsVisible = false;
        UpdateOverlayLayout();
        _panes.Opacity = 1;
        _pill.IsVisible = true;
        UpdateChrome();
        FocusActive();
    }

    private void ReopenOverlay(Control current)
    {
        switch (current)
        {
            case SearchOverlay s: OpenSearch(s.Query, s.ShowingTrash); break;
            case HelpOverlay: OpenHelp(); break;
        }
    }

    internal void OpenSearch(string query = "", bool trash = false)
    {
        var search = new SearchOverlay(this, _palette, _store, query, trash);
        ShowOverlay(search);
        search.FocusQuery();
    }

    private SettingsWindow? _settingsWindow;

    /// <summary>Settings open in their own window beside the pad, so every change shows on the real note.</summary>
    internal void OpenSettings()
    {
        // From the tray the pad may be hidden: show it too, so changes are visible on a real note.
        if (!IsVisible) ShowResident();
        if (_settingsWindow is { } open) { open.Activate(); return; }
        var window = new SettingsWindow(_settingsService, this, _softwareRendering, _notes);
        _settingsWindow = window;
        _settingsGone = new WeakReference(window);
        window.Closed += (_, _) =>
        {
            if (ReferenceEquals(_settingsWindow, window)) _settingsWindow = null;
            // Hand the closed window's memory back now; the hidden-pad trim would wait until the pad hides.
            _settingsTrim.Stop(); _settingsTrim.Start();
        };
        window.Show();
    }

    internal void CloseSettings() => _settingsWindow?.Close();

    private WeakReference? _settingsGone;
    private DispatcherTimer? _settingsTrimTimer;
    private DispatcherTimer _settingsTrim => _settingsTrimTimer ??= NewSettingsTrim();

    /// <summary>
    /// 1.5 s after Settings closes: collect the window, its preview editor and their caches, compact the heap and give
    /// unused pages back to Windows, so memory returns to where it was before Settings opened.
    /// </summary>
    private DispatcherTimer NewSettingsTrim()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            if (_settingsWindow is not null) return;
            System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            DesktopPlatform.TrimWorkingSet();
            if (_isDev)
            {
                using var self = System.Diagnostics.Process.GetCurrentProcess();
                _log.Write($"settings closed: freed={!(_settingsGone?.IsAlive ?? false)} managed={GC.GetTotalMemory(false) / 1048576.0:F1}MB private={self.PrivateMemorySize64 / 1048576.0:F0}MB ws={self.WorkingSet64 / 1048576.0:F0}MB threads={self.Threads.Count}");
            }
        };
        return timer;
    }

    /// <summary>Dev only: reads the AppInfo-derived debug-settings-cycles environment variable to open and close Settings while logging memory.</summary>
    internal async void CycleSettingsForDev()
    {
        if (!_isDev || !int.TryParse(Environment.GetEnvironmentVariable(AppInfo.Env("DEBUG_SETTINGS_CYCLES")), out var cycles)) return;
        await Task.Delay(1500);
        void Mem(string when)
        {
            using var self = System.Diagnostics.Process.GetCurrentProcess();
            _log.Write($"cycle {when}: private={self.PrivateMemorySize64 / 1048576.0:F0}MB ws={self.WorkingSet64 / 1048576.0:F0}MB threads={self.Threads.Count} managed={GC.GetTotalMemory(false) / 1048576.0:F1}MB");
        }
        Mem("start");
        for (var i = 0; i < cycles; i++)
        {
            OpenSettings();
            _settingsWindow?.ShowSection("feel");
            await Task.Delay(300);
            _settingsWindow?.FocusPreview();
            // Idle on the page with the live trace: memory should go flat once the trace stops (2 s after the last motion).
            for (var s = 1; s <= 6; s++) { await Task.Delay(1000); Mem($"{i + 1} open {s}s"); }
            CloseSettings();
            await Task.Delay(2500);
            Mem($"{i + 1} closed+trim");
        }
    }

    /// <summary>
    /// Settings lost focus or closed. Moving from the pad into Settings skipped the click-away rule; if focus has now gone
    /// somewhere else entirely, apply it (hide, or dim a pinned pad) as if the pad had just lost focus.
    /// </summary>
    internal void SettingsLostFocus() => Avalonia.Threading.Dispatcher.UIThread.Post(() =>
    {
        if (IsActive || _settingsWindow is { IsActive: true }) return;
        if (Settings.HideOnBlur && !Topmost && !_overlayOpen && IsVisible) HideResident();
        else if (Topmost && Settings.DimWhenPinned) FadeWindow(0.72);
    }, Avalonia.Threading.DispatcherPriority.Background);

    internal void OpenHelp() => ShowOverlay(new HelpOverlay(this, _palette));

    /// <summary>Back to defaults, keeping where the window is and that the tour was seen. Undoable from the toast.</summary>
    internal void ResetSettings()
    {
        var old = System.Text.Json.JsonSerializer.Serialize(Settings);
        var fresh = new Model.Settings { Width = Settings.Width, Height = Settings.Height, X = Settings.X, Y = Settings.Y, Welcomed = true, Split = Settings.Split, SplitSlot = Settings.SplitSlot };
        Apply(fresh);
        Toast("Settings restored to defaults", "Undo", () => { if (System.Text.Json.JsonSerializer.Deserialize<Model.Settings>(old) is { } back) Apply(back); });

        void Apply(Model.Settings s)
        {
            _left.ApplyLook(_palette, s); _right.ApplyLook(_palette, s);
            RegisterHotkey(s.Hotkey);
            RegisterNewNoteHotkey(s.NewNoteHotkey);
            _settingsService.Replace(s);
            _settingsWindow?.Refresh();
        }
    }

    /// <summary>Runs the single ordered settings reaction for a committed batch.</summary>
    internal void HandleSettingsChanged(SettingsChange change)
    {
        var flags = change.Flags;
        if ((flags & (SettingsChangeFlags.Appearance | SettingsChangeFlags.WindowPolicy)) != 0) Topmost = Settings.Pinned;
        if ((flags & (SettingsChangeFlags.Appearance | SettingsChangeFlags.Taskbar)) != 0)
        {
            if (Settings.Taskbar != "never") EnsureTaskbarProxy();
            ApplyTaskbarSetting();
        }
        if ((flags & SettingsChangeFlags.Appearance) != 0) ApplyAppearance();
        if ((flags & SettingsChangeFlags.Persistence) != 0)
        {
            if ((flags & SettingsChangeFlags.Appearance) != 0) Edited();
            else ScheduleSave();
        }
    }
}
