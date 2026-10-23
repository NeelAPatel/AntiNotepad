using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;
using AntiQuickPad.Editor;

namespace AntiQuickPad.Carousel;

/// <summary>The visual tree, preview editor, input adapters and frame-clock hookup.</summary>
internal sealed class CarouselView : ICarouselSurface
{
    private readonly TextEditor _editor, _preview;
    private readonly Func<int, string> _text;
    private readonly Func<int, Vector> _scroll;
    private readonly Func<double, double>? _pageStride;
    private readonly Grid _viewport = new() { ClipToBounds = true };
    private readonly TranslateTransform _currentTransform = new(), _previewTransform = new();
    private readonly Border _grab;
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private IPointer? _pointer;
    private CarouselMotion _motion = null!;
    public Control View { get; }
    public double PageWidth => Math.Max(1, _pageStride?.Invoke(_viewport.Bounds.Width) ?? _viewport.Bounds.Width);

    internal CarouselView(TextEditor editor, Look look, Func<int, string> text, Func<int, Vector> scroll,
        Func<Control, Control>? decoratePage, Func<double, double>? pageStride)
    {
        _editor = editor; _text = text; _scroll = scroll; _pageStride = pageStride;
        _preview = EditorFactory.Create(look); _preview.IsReadOnly = true;
        _preview.IsHitTestVisible = false; _preview.Focusable = false; _preview.IsVisible = false;
        var currentPage = decoratePage?.Invoke(editor) ?? _editor;
        var previewPage = decoratePage?.Invoke(_preview) ?? _preview;
        currentPage.RenderTransform = _currentTransform; previewPage.RenderTransform = _previewTransform;
        // The wrapper follows preview visibility, while the editor remains the authoritative page state.
        if (!ReferenceEquals(previewPage, _preview))
        {
            previewPage.IsVisible = false;
            _preview.PropertyChanged += (_, e) => { if (e.Property == Visual.IsVisibleProperty) previewPage.IsVisible = _preview.IsVisible; };
        }
        _viewport.Children.Add(currentPage); _viewport.Children.Add(previewPage);
        _grab = new Border
        {
            Height = look.GrabHeight, Background = look.Material == "solid" ? new SolidColorBrush(look.Background) : Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.SizeAll),
            Child = new TextBlock { Text = "···", Foreground = new SolidColorBrush(look.Muted),
                FontSize = look.StatusSize, FontFamily = new FontFamily(look.InterfaceFont),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center }
        };
        if (look.Material != "solid") _grab.Child = null; // Keep the grab target without idle chrome.
        ToolTip.SetTip(_grab, "Drag sideways to change notes");
        Panel host;
        if (decoratePage is not null)
        {
            _grab.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
            _grab.Background = Brushes.Transparent; _grab.Child = null;
            host = new Grid(); host.Children.Add(_viewport); host.Children.Add(_grab);
        }
        else
        {
            host = new DockPanel(); DockPanel.SetDock(_grab, Dock.Top);
            host.Children.Add(_grab); host.Children.Add(_viewport);
        }
        View = host;
    }

    internal void Attach(CarouselMotion motion)
    {
        _motion = motion;
        var host = (Panel)View;
        host.AddHandler(InputElement.PointerWheelChangedEvent, Wheel, RoutingStrategies.Tunnel, true);
        _grab.PointerPressed += (_, e) =>
        {
            if (!e.GetCurrentPoint(_grab).Properties.IsLeftButtonPressed || _motion.IsAnimating) return;
            _motion.BeginDrag(e.GetPosition(_grab).X, _clock.Elapsed.TotalMilliseconds);
            _pointer = e.Pointer; e.Pointer.Capture(_grab); e.Handled = true;
        };
        _grab.PointerMoved += (_, e) =>
        {
            if (!_motion.IsDragging) return;
            _motion.MoveDrag(e.GetPosition(_grab).X, _clock.Elapsed.TotalMilliseconds); e.Handled = true;
        };
        _grab.PointerReleased += (_, e) =>
        {
            if (!_motion.IsDragging) return;
            _motion.MoveDrag(e.GetPosition(_grab).X, _clock.Elapsed.TotalMilliseconds);
            _motion.EndDrag(_clock.Elapsed.TotalMilliseconds); ReleasePointer(); e.Handled = true;
        };
        _grab.PointerCaptureLost += (_, _) => { if (_motion.IsDragging) _motion.Cancel(); };
        _viewport.SizeChanged += (_, _) => { if (_motion.IsBusy) _motion.Cancel(); };
    }

    private void Wheel(object? sender, PointerWheelEventArgs e)
    {
        var delta = Math.Abs(e.Delta.X) > .001 ? e.Delta.X :
            e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? -e.Delta.Y : 0;
        if (Math.Abs(delta) < .001) return; // ordinary vertical editor scrolling
        _motion.RequestWheel(delta, (long)_clock.Elapsed.TotalMilliseconds); e.Handled = true;
    }

    public void PreparePreview(int target)
    {
        _preview.Text = _text(target); _preview.CaretOffset = 0;
        _preview.Document.UndoStack.ClearAll();
        _preview.IsVisible = true;
        _preview.UpdateLayout();
        var scroll = _scroll(target);
        EditorViewport.Restore(_preview, scroll);
    }

    public void SetReadOnly(bool value) => _editor.IsReadOnly = value;
    public void HidePreview() => _preview.IsVisible = false;
    public void RenderOffset(double current, double preview)
    {
        _currentTransform.X = current; _previewTransform.X = preview;
    }
    public void Reset()
    {
        _currentTransform.X = _previewTransform.X = 0;
        _preview.IsVisible = false;
    }
    public void ReleasePointer() { var pointer = _pointer; _pointer = null; pointer?.Capture(null); }
    public void RequestFrame(int version)
    {
        var top = TopLevel.GetTopLevel(_viewport) ?? throw new InvalidOperationException("Carousel requires an attached TopLevel.");
        top.RequestAnimationFrame(time => Animate(top, time, version));
    }
    private void Animate(TopLevel top, TimeSpan time, int version)
    {
        if (_motion.Advance(time, version)) top.RequestAnimationFrame(next => Animate(top, next, version));
    }
}
