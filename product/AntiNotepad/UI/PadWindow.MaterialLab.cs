using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using AntiNotepad.Editor;
using AntiNotepad.Platform;

namespace AntiNotepad.UI;

internal sealed partial class PadWindow
{
    // The shipped pill is opaque. Dev builds can compare it with the sampled blur treatment.
    private string _labPillMaterial = "solid";
    private Grid? _labPillVisualHost;
    private LabBackdrop? _labPillBlur, _labTopLeftBlur, _labTopRightBlur;
    private LabBackdropGroup? _labPillGroup, _labTopGroup;
    private Border? _labPillTint, _labTopTint;
    private bool _labSourcesAttached, _labRefreshScheduled;
    private TranslateTransform? _labContentTransform, _labBottomTransform, _labTopTransform;
    private TranslateTransform? _labLeftTransform, _labRightTransform;
    private EventHandler<AvaloniaPropertyChangedEventArgs>? _labWindowChanged, _labContentChanged, _labBottomLayerChanged, _labTopLayerChanged;
    private EventHandler<AvaloniaPropertyChangedEventArgs>? _labContentTransformChanged, _labBottomTransformChanged, _labTopTransformChanged;
    private EventHandler<AvaloniaPropertyChangedEventArgs>? _labLeftPaneChanged, _labRightPaneChanged;
    private EventHandler<AvaloniaPropertyChangedEventArgs>? _labLeftTransformChanged, _labRightTransformChanged;
    private EventHandler? _labLeftScroll, _labRightScroll, _labLeftLines, _labRightLines;
    private Action? _labLeftEdited, _labRightEdited;
    private long _labRefreshSamples, _labDrawSamples, _labLastMetricsLog;
    private long _labScrollEvents, _labEditEvents, _labLineEvents, _labTransformEvents, _labLayoutEvents, _labBarEvents;
    private double _labRefreshTotalMs, _labRefreshMaxMs, _labDrawTotalMs, _labDrawMaxMs;

    internal string LabPillMaterial => _labPillMaterial;

    private void InitializeMaterialLab()
    {
        _labLastMetricsLog = Stopwatch.GetTimestamp();
        _labContentTransformChanged = (_, _) => RefreshLabBackdropSources("transform");
        _labBottomTransformChanged = (_, _) => RefreshLabBackdropSources("transform");
        _labTopTransformChanged = (_, _) =>
        {
            UpdateTopBackdropMasks();
            RefreshLabBackdropSources("transform");
        };
        _labLeftTransformChanged = (_, _) =>
        {
            UpdateTopBackdropMasks();
            RefreshLabBackdropSources("transform");
        };
        _labRightTransformChanged = (_, _) =>
        {
            UpdateTopBackdropMasks();
            RefreshLabBackdropSources("transform");
        };

        _labWindowChanged = (_, e) =>
        {
            if (e.Property == Visual.IsVisibleProperty) UpdateLabVisibility();
        };
        _labContentChanged = (_, e) =>
        {
            if (e.Property == Visual.RenderTransformProperty)
                ObserveLabTransform(_content, ref _labContentTransform, _labContentTransformChanged);
        };
        _labBottomLayerChanged = (_, e) =>
        {
            if (e.Property == Visual.RenderTransformProperty)
                ObserveLabTransform(_bottomLayer, ref _labBottomTransform, _labBottomTransformChanged);
            else if (e.Property is var property && (property == Visual.OpacityProperty || property == Visual.IsVisibleProperty))
                UpdateLabBackdropVisibility();
        };
        _labTopLayerChanged = (_, e) =>
        {
            if (e.Property == Visual.RenderTransformProperty)
            {
                ObserveLabTransform(_topLayer, ref _labTopTransform, _labTopTransformChanged);
                UpdateTopBackdropMasks();
            }
            else if (e.Property is var property && (property == Visual.OpacityProperty || property == Visual.IsVisibleProperty))
                UpdateLabBackdropVisibility();
        };
        _labLeftPaneChanged = (_, e) => OnLabPaneChanged(_left, ref _labLeftTransform, _labLeftTransformChanged, e);
        _labRightPaneChanged = (_, e) => OnLabPaneChanged(_right, ref _labRightTransform, _labRightTransformChanged, e);

        if (_labWindowChanged is { } windowChanged) PropertyChanged += windowChanged;
        if (_labContentChanged is { } contentChanged) _content.PropertyChanged += contentChanged;
        if (_labBottomLayerChanged is { } bottomChanged) _bottomLayer.PropertyChanged += bottomChanged;
        if (_labTopLayerChanged is { } topChanged) _topLayer.PropertyChanged += topChanged;
        ObserveLabTransform(_content, ref _labContentTransform, _labContentTransformChanged);
        ObserveLabTransform(_bottomLayer, ref _labBottomTransform, _labBottomTransformChanged);
        ObserveLabTransform(_topLayer, ref _labTopTransform, _labTopTransformChanged);
        _panes.SizeChanged += (_, _) =>
        {
            UpdateTopBackdropMasks();
            RefreshLabBackdropSources();
        };
        _pill.SizeChanged += (_, _) => RefreshLabBackdropSources();
        _topLayer.SizeChanged += (_, _) => RefreshLabBackdropSources();
        _bottomLayer.SizeChanged += (_, _) => RefreshLabBackdropSources();
        _labTopGroup!.SizeChanged += (_, _) => RefreshLabBackdropSources();
        _labPillGroup!.SizeChanged += (_, _) => RefreshLabBackdropSources();

        _labTopLeftBlur!.CpuRenderMeasured += (_, milliseconds) => RecordLabDrawSubmit(milliseconds);
        _labTopRightBlur!.CpuRenderMeasured += (_, milliseconds) => RecordLabDrawSubmit(milliseconds);
        _labPillBlur!.CpuRenderMeasured += (_, milliseconds) => RecordLabDrawSubmit(milliseconds);
        _floatPager.Opened += OnFloatPagerOpened;
        Closed += (_, _) => OnLabWindowClosed();
        _labLeftEdited = () => RefreshLabBackdropSources("edit");
        _labRightEdited = () => RefreshLabBackdropSources("edit");
        _labLeftScroll = (_, _) => RefreshLabBackdropSources("scroll");
        _labRightScroll = (_, _) => RefreshLabBackdropSources("scroll");
        _labLeftLines = (_, _) => RefreshLabBackdropSources("visual-lines");
        _labRightLines = (_, _) => RefreshLabBackdropSources("visual-lines");

        ApplyLabMaterials();
        UpdateLabVisibility();
    }

    private void InitializeMaterialLabVisuals()
    {
        _labPillBlur = new LabBackdrop
        {
            IsHitTestVisible = false,
            ClipToBounds = true,
            Effect = new BlurEffect { Radius = ChromeTokens.TopBackdrop.BlurRadius },
            IsVisible = false,
        };
        _labPillTint = new Border { IsHitTestVisible = false, IsVisible = false };
        _labPillGroup = new LabBackdropGroup
        {
            IsHitTestVisible = false,
            ClipRadius = ChromeTokens.PillCornerRadius - ChromeTokens.PillBorderThickness,
            IsVisible = false,
        };
        _labPillGroup.Children.Add(_labPillBlur);
        _labPillGroup.Children.Add(_labPillTint);
        _labPillVisualHost = new Grid();
        _labPillVisualHost.Children.Add(_labPillGroup);

        _labTopLeftBlur = new LabBackdrop
        {
            IsHitTestVisible = false,
            ClipToBounds = true,
            Effect = new BlurEffect { Radius = ChromeTokens.TopBackdrop.BlurRadius },
            IsVisible = false,
        };
        _labTopRightBlur = new LabBackdrop
        {
            IsHitTestVisible = false,
            ClipToBounds = true,
            Effect = new BlurEffect { Radius = ChromeTokens.TopBackdrop.BlurRadius },
            IsVisible = false,
        };
        _labTopTint = new Border { IsHitTestVisible = false, IsVisible = false };
        _labTopGroup = new LabBackdropGroup { IsHitTestVisible = false, ClipToBounds = true, IsVisible = false };
        _labTopGroup.Children.Add(_labTopLeftBlur);
        _labTopGroup.Children.Add(_labTopRightBlur);
        _labTopGroup.Children.Add(_labTopTint);
        _topLayer.Children.Insert(0, _labTopGroup);
    }

    private void OnLabPaneChanged(Visual pane, ref TranslateTransform? transform, EventHandler<AvaloniaPropertyChangedEventArgs>? changed, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Visual.RenderTransformProperty)
        {
            ObserveLabTransform(pane, ref transform, changed);
            UpdateTopBackdropMasks();
        }
        else if (e.Property == Visual.OpacityProperty || e.Property == Visual.IsVisibleProperty)
        {
            UpdateLabBackdropVisibility();
            RefreshLabBackdropSources();
        }
    }

    private void ObserveLabTransform(Visual visual, ref TranslateTransform? current, EventHandler<AvaloniaPropertyChangedEventArgs>? changed)
    {
        if (current is not null && changed is not null) current.PropertyChanged -= changed;
        current = visual.RenderTransform as TranslateTransform;
        if (current is not null && changed is not null) current.PropertyChanged += changed;
        RefreshLabBackdropSources();
    }

    private void UpdateLabVisibility()
    {
        if (IsVisible)
        {
            AttachLabSourceObservers();
            if (Settings.Bars == "float" && _bottomShown) _floatPager.IsOpen = true;
        }
        else
        {
            DetachLabSourceObservers();
            if (Settings.Bars == "float") _floatPager.IsOpen = false;
        }
        UpdateLabBackdropVisibility();
    }

    private void AttachLabSourceObservers()
    {
        if (_labSourcesAttached) return;
        if (_labLeftEdited is { } leftEdited) _left.Edited += leftEdited;
        if (_labRightEdited is { } rightEdited) _right.Edited += rightEdited;
        if (_labLeftScroll is { } leftScroll) ((ScrollableTopInsetTextEditor)_left.Editor).InsetTextArea.ScrollOffsetChanged += leftScroll;
        if (_labRightScroll is { } rightScroll) ((ScrollableTopInsetTextEditor)_right.Editor).InsetTextArea.ScrollOffsetChanged += rightScroll;
        if (_labLeftLines is { } leftLines) _left.Editor.TextArea.TextView.VisualLinesChanged += leftLines;
        if (_labRightLines is { } rightLines) _right.Editor.TextArea.TextView.VisualLinesChanged += rightLines;
        if (_labLeftPaneChanged is { } leftPaneChanged) _left.PropertyChanged += leftPaneChanged;
        if (_labRightPaneChanged is { } rightPaneChanged) _right.PropertyChanged += rightPaneChanged;
        ObserveLabTransform(_left, ref _labLeftTransform, _labLeftTransformChanged);
        ObserveLabTransform(_right, ref _labRightTransform, _labRightTransformChanged);
        _labSourcesAttached = true;
    }

    private void DetachLabSourceObservers()
    {
        if (!_labSourcesAttached) return;
        if (_labLeftEdited is { } leftEdited) _left.Edited -= leftEdited;
        if (_labRightEdited is { } rightEdited) _right.Edited -= rightEdited;
        if (_labLeftScroll is { } leftScroll) ((ScrollableTopInsetTextEditor)_left.Editor).InsetTextArea.ScrollOffsetChanged -= leftScroll;
        if (_labRightScroll is { } rightScroll) ((ScrollableTopInsetTextEditor)_right.Editor).InsetTextArea.ScrollOffsetChanged -= rightScroll;
        if (_labLeftLines is { } leftLines) _left.Editor.TextArea.TextView.VisualLinesChanged -= leftLines;
        if (_labRightLines is { } rightLines) _right.Editor.TextArea.TextView.VisualLinesChanged -= rightLines;
        if (_labLeftPaneChanged is { } leftPaneChanged) _left.PropertyChanged -= leftPaneChanged;
        if (_labRightPaneChanged is { } rightPaneChanged) _right.PropertyChanged -= rightPaneChanged;
        if (_labLeftTransform is not null && _labLeftTransformChanged is not null) _labLeftTransform.PropertyChanged -= _labLeftTransformChanged;
        if (_labRightTransform is not null && _labRightTransformChanged is not null) _labRightTransform.PropertyChanged -= _labRightTransformChanged;
        _labLeftTransform = _labRightTransform = null;
        _labSourcesAttached = false;
    }

    private void ApplyLabMaterials()
    {
        if (_labPillBlur is null || _labPillTint is null || _labPillGroup is null
            || _labTopLeftBlur is null || _labTopRightBlur is null || _labTopTint is null || _labTopGroup is null) return;

        var p = _palette;
        var canSamplePill = Program.IsDev && _labPillMaterial == "blur" && Settings.Bars == "overlay";
        _pill.Background = new SolidColorBrush(canSamplePill ? p.Canvas : p.Surface);
        _labPillGroup.Background = new SolidColorBrush(p.Canvas);
        _labPillTint.Background = new SolidColorBrush(p.Canvas, ChromeTokens.TopBackdrop.BlurTintAlpha);
        _labPillBlur.MeasureCpuRender = Program.IsDev;
        _labPillBlur.IsVisible = canSamplePill;
        _labPillTint.IsVisible = canSamplePill;
        _labPillGroup.IsVisible = canSamplePill;

        // The offscreen Avalonia 12.1.4 Skia/Win32 probe confirms BlurEffect works in software mode.
        _labTopGroup.Background = null;
        _labTopGroup.OpacityMask = TopBlurMask();
        _labTopTint.Background = new SolidColorBrush(p.Canvas, ChromeTokens.TopBackdrop.TintAlpha);
        _labTopLeftBlur.MeasureCpuRender = Program.IsDev;
        _labTopRightBlur.MeasureCpuRender = Program.IsDev;
        _labTopLeftBlur.IsVisible = true;
        _labTopRightBlur.IsVisible = true;
        _labTopTint.IsVisible = true;
        UpdateLabBackdropVisibility();
        RefreshLabBackdropSources();
    }

    private void UpdateLabBackdropVisibility()
    {
        if (_labTopGroup is null || _labPillGroup is null || _labTopLeftBlur is null || _labTopRightBlur is null) return;
        var topActive = IsVisible && Settings.Bars is "overlay" or "float"
            && _topLayer.IsVisible && _topLayer.Opacity > 0;
        var pillActive = IsVisible && Program.IsDev && _labPillMaterial == "blur" && Settings.Bars == "overlay"
            && _bottomLayer.IsVisible && _bottomLayer.Opacity > 0 && _pill.Opacity > 0;
        _labTopGroup.IsVisible = topActive;
        _labTopLeftBlur.IsVisible = topActive && _left.IsVisible;
        _labTopRightBlur.IsVisible = topActive && _right.IsVisible;
        _labTopLeftBlur.Opacity = _left.Opacity;
        _labTopRightBlur.Opacity = _right.Opacity;
        _labTopTint!.IsVisible = topActive;
        _labPillGroup.IsVisible = pillActive;
        _labPillBlur!.IsVisible = pillActive;
        _labPillTint!.IsVisible = pillActive;
        UpdateTopBackdropMasks();
        if (topActive || pillActive) RefreshLabBackdropSources("bar");
        else FlushLabRenderMetrics("backdrops-hidden");
    }

    private static LinearGradientBrush TopBlurMask() => new()
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        GradientStops =
        {
            new GradientStop(Color.FromArgb((byte)(255 * ChromeTokens.TopBackdrop.DenseMaskAlpha), 255, 255, 255), 0),
            new GradientStop(Color.FromArgb((byte)(255 * ChromeTokens.TopBackdrop.DenseMaskAlpha), 255, 255, 255), ChromeTokens.TopBackdrop.DensePosition),
            new GradientStop(Color.FromArgb((byte)(255 * ChromeTokens.TopBackdrop.MidpointMaskAlpha), 255, 255, 255), ChromeTokens.TopBackdrop.MidpointPosition),
            new GradientStop(Color.FromArgb((byte)(255 * ChromeTokens.TopBackdrop.ClearMaskAlpha), 255, 255, 255), 1),
        },
    };

    private void RefreshLabBackdropSources() => RefreshLabBackdropSources("layout");

    private void RefreshLabBackdropSources(string cause)
    {
        if (!IsVisible || _labRefreshScheduled || !AnyLabBackdropActive()) return;
        CountLabRefreshCause(cause);
        _labRefreshScheduled = true;
        Dispatcher.UIThread.Post(RefreshLabBackdropSourcesAfterLayout, DispatcherPriority.Render);
    }

    private bool AnyLabBackdropActive() => _labTopGroup?.IsVisible == true || _labPillGroup?.IsVisible == true;

    private void RefreshLabBackdropSourcesAfterLayout()
    {
        _labRefreshScheduled = false;
        if (!IsVisible || !AnyLabBackdropActive()) return;
        var root = (Visual?)_scaler.Child;
        if (root is null || _panes.Bounds.Width <= 0 || _panes.Bounds.Height <= 0) return;

        var started = Stopwatch.GetTimestamp();
        if (_labTopGroup?.IsVisible == true)
        {
            if (_left.IsVisible && _labTopLeftBlur is { } leftBlur)
                SetFreshLabSource(leftBlur, _left.Editor.TextArea.TextView, root);
            if (_right.IsVisible && _labTopRightBlur is { } rightBlur)
                SetFreshLabSource(rightBlur, _right.Editor.TextArea.TextView, root);
        }

        if (_labPillGroup?.IsVisible == true && _labPillBlur is { } pillBlur
            && pillBlur.Bounds.Width > 0 && pillBlur.Bounds.Height > 0)
        {
            var paneOrigin = _panes.TranslatePoint(default, root) ?? default;
            var pillOrigin = pillBlur.TranslatePoint(default, root);
            if (pillOrigin is { } origin)
                SetFreshLabSource(pillBlur, new Rect(origin.X - paneOrigin.X, origin.Y - paneOrigin.Y, pillBlur.Bounds.Width, pillBlur.Bounds.Height), pillBlur.Bounds.Size);
        }
        RecordLabRefreshCpu(Stopwatch.GetElapsedTime(started).TotalMilliseconds);
    }

    private void SetFreshLabSource(LabBackdrop target, Rect source, Size viewport)
    {
        // Avalonia 12.1.4 VisualBrush caches source content; a new brush makes each dirty refresh sample current visuals.
        target.Background = new VisualBrush(_panes)
        {
            Stretch = Stretch.Fill,
            SourceRect = new RelativeRect(source, RelativeUnit.Absolute),
            DestinationRect = new RelativeRect(new Rect(0, 0, viewport.Width, viewport.Height), RelativeUnit.Absolute),
        };
        target.InvalidateVisual();
    }

    private void SetFreshLabSource(LabBackdrop target, Visual sourceVisual, Visual coordinateRoot)
    {
        if (target.Bounds.Width <= 0 || target.Bounds.Height <= 0) return;
        var targetOrigin = target.TranslatePoint(default, coordinateRoot);
        var sourceOrigin = sourceVisual.TranslatePoint(default, coordinateRoot);
        if (targetOrigin is not { } targetPoint || sourceOrigin is not { } sourcePoint) return;
        var source = new Rect(targetPoint.X - sourcePoint.X, targetPoint.Y - sourcePoint.Y, target.Bounds.Width, target.Bounds.Height);
        target.Background = new VisualBrush(sourceVisual)
        {
            Stretch = Stretch.Fill,
            SourceRect = new RelativeRect(source, RelativeUnit.Absolute),
            DestinationRect = new RelativeRect(new Rect(0, 0, target.Bounds.Width, target.Bounds.Height), RelativeUnit.Absolute),
        };
        target.InvalidateVisual();
    }

    private void UpdateTopBackdropMasks()
    {
        if (_left is null || _right is null) return;
        var active = Settings.Bars is "overlay" or "float" && _topLayer.IsVisible;
        var opacity = active ? Math.Clamp(_topLayer.Opacity, 0, 1) : 0;
        var root = (Visual?)_scaler.Child;
        if (root is null || opacity <= 0)
        {
            _left.SetTopBackdropMask(0, 0);
            _right.SetTopBackdropMask(0, 0);
            return;
        }

        var bandOrigin = _topLayer.TranslatePoint(default, root);
        var leftOrigin = _left.Editor.TranslatePoint(default, root);
        var rightOrigin = _right.Editor.TranslatePoint(default, root);
        if (bandOrigin is not { } band || leftOrigin is not { } left || rightOrigin is not { } right)
        {
            _left.SetTopBackdropMask(0, 0);
            _right.SetTopBackdropMask(0, 0);
            return;
        }

        _left.SetTopBackdropMask(band.Y - left.Y, opacity);
        _right.SetTopBackdropMask(band.Y - right.Y, opacity);
    }

    private void RecordLabRefreshCpu(double milliseconds)
    {
        if (!Program.IsDev) return;
        _labRefreshSamples++;
        _labRefreshTotalMs += milliseconds;
        _labRefreshMaxMs = Math.Max(_labRefreshMaxMs, milliseconds);
        MaybeLogLabRenderMetrics();
    }

    private void CountLabRefreshCause(string cause)
    {
        if (!Program.IsDev) return;
        switch (cause)
        {
            case "scroll": _labScrollEvents++; break;
            case "edit": _labEditEvents++; break;
            case "visual-lines": _labLineEvents++; break;
            case "transform": _labTransformEvents++; break;
            case "bar": _labBarEvents++; break;
            default: _labLayoutEvents++; break;
        }
    }

    private void RecordLabDrawSubmit(double milliseconds)
    {
        if (!Program.IsDev) return;
        _labDrawSamples++;
        _labDrawTotalMs += milliseconds;
        _labDrawMaxMs = Math.Max(_labDrawMaxMs, milliseconds);
        MaybeLogLabRenderMetrics();
    }

    private void MaybeLogLabRenderMetrics()
    {
        // VisualBrush source capture and the compositor draw happen after the refresh request; wait for a measured render.
        if (_labDrawSamples == 0) return;
        var now = Stopwatch.GetTimestamp();
        if (Stopwatch.GetElapsedTime(_labLastMetricsLog, now).TotalSeconds < 5
            && _labRefreshSamples < 60 && _labDrawSamples < 60) return;
        FlushLabRenderMetrics("active");
    }

    private void FlushLabRenderMetrics(string reason)
    {
        if (!Program.IsDev || _labRefreshSamples == 0 && _labDrawSamples == 0
            && _labScrollEvents == 0 && _labEditEvents == 0 && _labLineEvents == 0
            && _labTransformEvents == 0 && _labLayoutEvents == 0 && _labBarEvents == 0) return;
        var refreshMean = _labRefreshSamples == 0 ? 0 : _labRefreshTotalMs / _labRefreshSamples;
        var drawMean = _labDrawSamples == 0 ? 0 : _labDrawTotalMs / _labDrawSamples;
        Program.Log($"bar-lab-render reason={reason} refresh-request-cpu-ms mean={refreshMean:F3} max={_labRefreshMaxMs:F3} n={_labRefreshSamples} " +
            $"draw-submit-per-control-cpu-ms mean={drawMean:F3} max={_labDrawMaxMs:F3} n={_labDrawSamples} " +
            $"activity scroll={_labScrollEvents} edit={_labEditEvents} visual-lines={_labLineEvents} transform={_labTransformEvents} layout={_labLayoutEvents} bar={_labBarEvents} " +
            "gpu=unavailable visualbrush-source-capture=deferred");
        _labRefreshSamples = _labDrawSamples = 0;
        _labRefreshTotalMs = _labRefreshMaxMs = _labDrawTotalMs = _labDrawMaxMs = 0;
        _labScrollEvents = _labEditEvents = _labLineEvents = _labTransformEvents = _labLayoutEvents = _labBarEvents = 0;
        _labLastMetricsLog = Stopwatch.GetTimestamp();
    }

    private void OnFloatPagerOpened(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!_floatPager.IsOpen || TopLevel.GetTopLevel(_pill) is not { } popup
                || ReferenceEquals(popup, this) || popup is Window) return;
            popup.RequestedThemeVariant = RequestedThemeVariant;
            popup.TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
            popup.TransparencyBackgroundFallback = Brushes.Transparent;
            Native.ConfigurePopupWindow(popup);
        }, DispatcherPriority.Loaded);
    }

    private void OnLabWindowClosed()
    {
        DetachLabSourceObservers();
        FlushLabRenderMetrics("window-closed");
        if (_labWindowChanged is not null) PropertyChanged -= _labWindowChanged;
        if (_labContentChanged is not null) _content.PropertyChanged -= _labContentChanged;
        if (_labBottomLayerChanged is not null) _bottomLayer.PropertyChanged -= _labBottomLayerChanged;
        if (_labTopLayerChanged is not null) _topLayer.PropertyChanged -= _labTopLayerChanged;
        if (_labContentTransform is not null && _labContentTransformChanged is not null) _labContentTransform.PropertyChanged -= _labContentTransformChanged;
        if (_labBottomTransform is not null && _labBottomTransformChanged is not null) _labBottomTransform.PropertyChanged -= _labBottomTransformChanged;
        if (_labTopTransform is not null && _labTopTransformChanged is not null) _labTopTransform.PropertyChanged -= _labTopTransformChanged;
    }

    internal void SetLabPillMaterial(string value)
    {
        if (!Program.IsDev || value is not ("solid" or "blur")) return;
        _labPillMaterial = value;
        ApplyLabMaterials();
    }

    private sealed class LabBackdrop : Control
    {
        public event Action<LabBackdrop, double>? CpuRenderMeasured;
        public bool MeasureCpuRender { get; set; }
        public IBrush? Background { get; set; }

        public override void Render(DrawingContext context)
        {
            var started = MeasureCpuRender ? Stopwatch.GetTimestamp() : 0;
            context.DrawRectangle(Background, null, new Rect(Bounds.Size));
            if (MeasureCpuRender) CpuRenderMeasured?.Invoke(this, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
    }

    private sealed class LabBackdropGroup : Grid
    {
        private Size _clipSize = new(0, 0);

        public double ClipRadius { get; set; }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var arranged = base.ArrangeOverride(finalSize);
            if (ClipRadius <= 0)
            {
                Clip = null;
                ClipToBounds = true;
            }
            else if (_clipSize != arranged)
            {
                _clipSize = arranged;
                ClipToBounds = false;
                Clip = RoundedClip(arranged, ClipRadius);
            }
            return arranged;
        }

        private static StreamGeometry RoundedClip(Size size, double radius)
        {
            var r = Math.Min(radius, Math.Min(size.Width, size.Height) / 2);
            var geometry = new StreamGeometry();
            using var context = geometry.Open();
            context.BeginFigure(new Point(r, 0), true);
            context.LineTo(new Point(size.Width - r, 0));
            context.ArcTo(new Point(size.Width, r), new Size(r, r), 0, false, SweepDirection.Clockwise, true);
            context.LineTo(new Point(size.Width, size.Height - r));
            context.ArcTo(new Point(size.Width - r, size.Height), new Size(r, r), 0, false, SweepDirection.Clockwise, true);
            context.LineTo(new Point(r, size.Height));
            context.ArcTo(new Point(0, size.Height - r), new Size(r, r), 0, false, SweepDirection.Clockwise, true);
            context.LineTo(new Point(0, r));
            context.ArcTo(new Point(r, 0), new Size(r, r), 0, false, SweepDirection.Clockwise, true);
            context.EndFigure(true);
            return geometry;
        }
    }
}
