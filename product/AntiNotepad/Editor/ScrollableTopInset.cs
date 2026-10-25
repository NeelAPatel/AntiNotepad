using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;

namespace AntiNotepad.Editor;

/// <summary>TextEditor backed by a TextArea that adds scrollable leading whitespace without changing line metrics.</summary>
internal sealed class ScrollableTopInsetTextEditor : TextEditor
{
    public ScrollableTopInsetTextArea InsetTextArea { get; }

    public ScrollableTopInsetTextEditor() : this(new ScrollableTopInsetTextArea()) { }

    private ScrollableTopInsetTextEditor(ScrollableTopInsetTextArea area) : base(area) => InsetTextArea = area;

    protected override Type StyleKeyOverride => typeof(TextEditor);
}

/// <summary>
/// Adds a virtual top segment to AvaloniaEdit's logical scroll range. While that segment is at the top of the viewport,
/// the TextArea template root is arranged lower and shorter; TextView keeps its normal document line heights and its own
/// offset starts only after the leading segment has scrolled away.
/// </summary>
internal sealed class ScrollableTopInsetTextArea : TextArea, ILogicalScrollable
{
    private readonly ILogicalScrollable _viewScroll;
    private Control? _templateRoot;
    private Vector _offset;
    private double _topInset;
    private bool _settingViewOffset;
    private EventHandler? _scrollInvalidated;

    public event EventHandler? ScrollOffsetChanged;

    public double TopInset
    {
        get => _topInset;
        set
        {
            var inset = Math.Max(0, value);
            if (Math.Abs(_topInset - inset) < 0.01) return;

            var documentOffset = _viewScroll.Offset;
            _topInset = inset;
            // A zero TextView offset means the viewport was still in the leading blank segment.
            var logicalY = documentOffset.Y <= 0 ? 0 : documentOffset.Y + _topInset;
            SetLogicalOffset(new Vector(documentOffset.X, logicalY), updateView: false);
            InvalidateMeasure();
            InvalidateArrange();
            NotifyScrollInvalidated(EventArgs.Empty);
        }
    }

    public double LogicalVerticalOffset => _offset.Y;
    public double LogicalExtentHeight => _viewScroll.Extent.Height + _topInset;
    public double LogicalViewportHeight => Bounds.Height;
    public double VisibleGap => VisibleGapFor(_topInset, _offset.Y);

    public ScrollableTopInsetTextArea() : base()
    {
        _viewScroll = (ILogicalScrollable)TextView;
        _viewScroll.ScrollInvalidated += OnViewScrollInvalidated;
        TextView.ScrollOffsetChanged += OnViewScrollOffsetChanged;
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    internal static double VisibleGapFor(double inset, double offset) => Math.Max(0, inset - Math.Max(0, offset));
    internal static double DocumentOffsetFor(double inset, double offset) => Math.Max(0, offset - inset);

    protected override Type StyleKeyOverride => typeof(TextArea);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _templateRoot = this.GetVisualChildren().OfType<Control>().FirstOrDefault();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        if (_templateRoot is { } root)
        {
            var gap = Math.Min(finalSize.Height, VisibleGap);
            root.Arrange(new Rect(0, gap, finalSize.Width, Math.Max(0, finalSize.Height - gap)));
        }
        return result;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var gap = Math.Min(availableSize.Height, VisibleGap);
        var innerHeight = double.IsInfinity(availableSize.Height)
            ? availableSize.Height : Math.Max(0, availableSize.Height - gap);
        var measured = base.MeasureOverride(new Size(availableSize.Width, innerHeight));
        return double.IsInfinity(availableSize.Height)
            ? measured.WithHeight(measured.Height + gap)
            : availableSize;
    }

    public override void Render(DrawingContext context) =>
        context.DrawRectangle(Brushes.Transparent, null, new Rect(Bounds.Size));

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var modifiers = e.KeyModifiers;
        if (e.Key == Key.Home && (modifiers == KeyModifiers.Control || modifiers == (KeyModifiers.Control | KeyModifiers.Shift)))
            SetLogicalOffset(new Vector(_offset.X, 0), updateView: true);
    }

    private void SetLogicalOffset(Vector value, bool updateView)
    {
        var maxY = Math.Max(0, LogicalExtentHeight - LogicalViewportHeight);
        var next = new Vector(value.X, Math.Clamp(value.Y, 0, maxY));
        var changed = next != _offset;
        var gapChanged = Math.Abs(VisibleGapFor(_topInset, next.Y) - VisibleGapFor(_topInset, _offset.Y)) > 0.01;
        if (changed) SetAndRaise(OffsetProperty, ref _offset, next);
        if (gapChanged)
        {
            InvalidateMeasure();
            InvalidateArrange();
        }

        if (updateView)
        {
            _settingViewOffset = true;
            try { _viewScroll.Offset = new Vector(next.X, DocumentOffsetFor(_topInset, next.Y)); }
            finally { _settingViewOffset = false; }
        }

        if (changed)
        {
            ScrollOffsetChanged?.Invoke(this, EventArgs.Empty);
            NotifyScrollInvalidated(EventArgs.Empty);
        }
    }

    private void OnViewScrollOffsetChanged(object? sender, EventArgs e)
    {
        if (_settingViewOffset) return;
        var viewOffset = _viewScroll.Offset;
        var logicalY = viewOffset.Y <= 0 ? 0 : viewOffset.Y + _topInset;
        SetLogicalOffset(new Vector(viewOffset.X, logicalY), updateView: false);
    }

    private void OnViewScrollInvalidated(object? sender, EventArgs e)
    {
        var oldOffset = _offset;
        if (!_settingViewOffset && _viewScroll.Offset.Y > 0)
            SetLogicalOffset(new Vector(_viewScroll.Offset.X, _viewScroll.Offset.Y + _topInset), updateView: false);
        if (_offset == oldOffset) NotifyScrollInvalidated(e);
    }

    private void NotifyScrollInvalidated(EventArgs e) => _scrollInvalidated?.Invoke(this, e);

    event EventHandler? ILogicalScrollable.ScrollInvalidated
    {
        add => _scrollInvalidated += value;
        remove => _scrollInvalidated -= value;
    }

    bool ILogicalScrollable.IsLogicalScrollEnabled => _viewScroll.IsLogicalScrollEnabled;
    Size ILogicalScrollable.ScrollSize => _viewScroll.ScrollSize;
    Size ILogicalScrollable.PageScrollSize => _viewScroll.PageScrollSize;
    bool ILogicalScrollable.CanHorizontallyScroll
    {
        get => _viewScroll.CanHorizontallyScroll;
        set => _viewScroll.CanHorizontallyScroll = value;
    }
    bool ILogicalScrollable.CanVerticallyScroll
    {
        get => _viewScroll.CanVerticallyScroll;
        set => _viewScroll.CanVerticallyScroll = value;
    }
    Size IScrollable.Extent => new(_viewScroll.Extent.Width, LogicalExtentHeight);
    Vector IScrollable.Offset
    {
        get => _offset;
        set => SetLogicalOffset(value, updateView: true);
    }
    Size IScrollable.Viewport => new(_viewScroll.Viewport.Width, LogicalViewportHeight);

    bool ILogicalScrollable.BringIntoView(Control target, Rect targetRect) => base.BringIntoView(target, targetRect);
    Control ILogicalScrollable.GetControlInDirection(NavigationDirection direction, Control? from) => _viewScroll.GetControlInDirection(direction, from)!;
    void ILogicalScrollable.RaiseScrollInvalidated(EventArgs e) => NotifyScrollInvalidated(e);
}
