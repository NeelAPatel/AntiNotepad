using Avalonia.Controls;
using Avalonia.Input;

namespace Antipad.UI;

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
        if (e.Key == Key.F12 && Environment.GetEnvironmentVariable("ANTIPAD_SHOTS") is { Length: > 0 } shots)
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
        if (e.Key == Key.F11 && Program.IsDev)
        {
            // Dev-only memory probe: managed heap vs what the GC has committed.
            var info = GC.GetGCMemoryInfo();
            Program.Log($"mem managed={GC.GetTotalMemory(false) / 1048576.0:F1}MB heap={info.HeapSizeBytes / 1048576.0:F1}MB committed={info.TotalCommittedBytes / 1048576.0:F1}MB docs={Editor.PadView.CachedDocuments} gen2={GC.CollectionCount(2)}");
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
        Settings.FontSize = Math.Clamp(Settings.FontSize + step, 10, 28);
        _left.ApplyLook(_palette, Settings); _right.ApplyLook(_palette, Settings);
        Toast($"Text size {Settings.FontSize:0}");
        Edited();
    }

    // ---------- overlays ----------

    private void ShowOverlay(Control overlay)
    {
        _left.Flush(); _right.Flush();
        _overlay = overlay;
        _overlayHost.Children.Clear();
        _overlayHost.Children.Add(overlay);
        _overlayHost.IsVisible = true;
        _panes.Opacity = 0.0;
        _pill.IsVisible = false;
        WakeChrome(hold: true);
        Anim.Run(_overlayHost, 130, t => _overlayHost.Opacity = t);
    }

    internal bool IsOverlay(Control control) => ReferenceEquals(_overlay, control);

    internal void CloseOverlay()
    {
        if (_overlay is null) return;
        _overlay = null;
        _overlayHost.Children.Clear();
        _overlayHost.IsVisible = false;
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
            case SettingsOverlay o: OpenSettings(o.ScrollOffset); break;
            case HelpOverlay: OpenHelp(); break;
        }
    }

    internal void OpenSearch(string query = "", bool trash = false)
    {
        var search = new SearchOverlay(this, _palette, query, trash);
        ShowOverlay(search);
        search.FocusQuery();
    }

    internal void OpenSettings() => OpenSettings(0);

    private void OpenSettings(double scroll)
    {
        if (!IsVisible) ShowResident();
        var settings = new SettingsOverlay(this, _palette, scroll);
        ShowOverlay(settings);
    }

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
            Data.Settings = s;
            _left.ApplyLook(_palette, s); _right.ApplyLook(_palette, s);
            RegisterHotkey(s.Hotkey);
            RegisterNewNoteHotkey(s.NewNoteHotkey);
            SettingsChanged();
        }
    }

    /// <summary>Settings call this after any change: apply, persist, keep the panel open.</summary>
    internal void SettingsChanged()
    {
        Topmost = Settings.Pinned;
        ApplyAppearance();
        Edited();
    }
}
