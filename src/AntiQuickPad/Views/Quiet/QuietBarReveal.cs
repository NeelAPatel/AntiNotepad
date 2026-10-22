using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace AntiQuickPad.Views.Quiet;

// Only presentation state. No command, document, navigation, persistence or focus ownership.
internal sealed class QuietBarReveal : IDisposable
{
    private readonly Control _root;
    private readonly Func<bool> _settingsOpen;
    // M11b-settings-window: "always" keeps the bars visible (preference F56).
    private bool _always;
    internal bool Always { get => _always; set { _always = value; if (!_disposed) Refresh(); } }
    private readonly IReadOnlyList<Control> _bars;
    private readonly DispatcherTimer _linger = new() { Interval = Tokens.DesignTokens.Quiet.RevealLinger };
    private bool? _captureOverride;
    private bool? _pointerProbe;
    private bool _disposed;
    internal bool Revealed { get; private set; }

    internal QuietBarReveal(Control root, Func<bool> settingsOpen, IReadOnlyList<Control> bars)
    {
        _root = root; _settingsOpen = settingsOpen; _bars = bars;
        foreach (var bar in bars) { bar.Opacity = 0; bar.IsHitTestVisible = false; }
        _linger.Tick += OnLinger;
        root.PointerEntered += OnPointer; root.PointerExited += OnPointer;
        root.GotFocus += OnFocus; root.LostFocus += OnFocus;
        root.DetachedFromVisualTree += OnDetached;
        Refresh();
    }
    private bool WantsBars => _captureOverride ?? (_always || (_pointerProbe ?? _root.IsPointerOver) ||
        _settingsOpen() || _bars.Any(bar => bar.IsKeyboardFocusWithin));
    private void OnPointer(object? sender, PointerEventArgs e) => Refresh();
    private void OnFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        Dispatcher.UIThread.Post(() => { if (!_disposed) Refresh(); });
    private void OnDetached(object? sender, VisualTreeAttachmentEventArgs e) => _linger.Stop();
    private void OnLinger(object? sender, EventArgs e)
    {
        _linger.Stop();
        if (!WantsBars) Show(false); else Show(true);
    }
    internal void Refresh()
    {
        _linger.Stop();
        if (WantsBars) Show(true);
        else if (Revealed) _linger.Start();
        else Show(false);
    }
    private void Show(bool show)
    {
        Revealed = show;
        foreach (var bar in _bars)
        {
            bar.Transitions = _captureOverride.HasValue ? null : new Transitions {
                new DoubleTransition { Property = Visual.OpacityProperty,
                    Duration = show ? Tokens.DesignTokens.Quiet.RevealIn : Tokens.DesignTokens.Quiet.RevealOut } };
            bar.IsHitTestVisible = show;
            bar.Opacity = show ? 1 : 0;
        }
    }
    // Hidden renders/proofs force presentation states without moving the real pointer.
    internal void CaptureState(bool? shown)
    { _captureOverride = shown; _linger.Stop(); if (shown.HasValue) Show(shown.Value); else Refresh(); }
    internal void ProbePointer(bool? over) { _pointerProbe = over; Refresh(); }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true; _linger.Stop(); _linger.Tick -= OnLinger;
        _root.PointerEntered -= OnPointer; _root.PointerExited -= OnPointer;
        _root.GotFocus -= OnFocus; _root.LostFocus -= OnFocus;
        _root.DetachedFromVisualTree -= OnDetached;
    }
}
