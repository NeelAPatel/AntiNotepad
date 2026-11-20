using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using AntiNotepad.Editor;
using AntiNotepad.Platform;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

internal sealed partial class PadWindow
{
    private TopBlurVisual? _topBlurLeft, _topBlurRight;
    private Border? _topBlurTint;
    private Grid? _topBlurGroup;
    private bool _topBlurSourcesAttached, _topBlurRefreshScheduled;
    private TranslateTransform? _topBlurContentTransform, _topBlurTopTransform;
    private TranslateTransform? _topBlurLeftTransform, _topBlurRightTransform;
    private EventHandler<AvaloniaPropertyChangedEventArgs>? _topBlurWindowChanged, _topBlurContentChanged, _topBlurTopLayerChanged;
    private EventHandler<AvaloniaPropertyChangedEventArgs>? _topBlurContentTransformChanged, _topBlurTopTransformChanged;
    private EventHandler<AvaloniaPropertyChangedEventArgs>? _topBlurLeftPaneChanged, _topBlurRightPaneChanged;
    private EventHandler<AvaloniaPropertyChangedEventArgs>? _topBlurLeftTransformChanged, _topBlurRightTransformChanged;
    private EventHandler? _topBlurLeftScroll, _topBlurRightScroll, _topBlurLeftLines, _topBlurRightLines;
    private Action? _topBlurLeftEdited, _topBlurRightEdited;

    private void InitializeTopBlur()
    {
        _topBlurContentTransformChanged = (_, _) => RefreshTopBlurSources();
        _topBlurTopTransformChanged = (_, _) =>
        {
            UpdateTopBlurMasks();
            RefreshTopBlurSources();
        };
        _topBlurLeftTransformChanged = (_, _) =>
        {
            UpdateTopBlurMasks();
            RefreshTopBlurSources();
        };
        _topBlurRightTransformChanged = (_, _) =>
        {
            UpdateTopBlurMasks();
            RefreshTopBlurSources();
        };

        _topBlurWindowChanged = (_, e) =>
        {
            if (e.Property == Visual.IsVisibleProperty) UpdateTopBlurVisibility();
        };
        _topBlurContentChanged = (_, e) =>
        {
            if (e.Property == Visual.RenderTransformProperty)
                ObserveTopBlurTransform(_content, ref _topBlurContentTransform, _topBlurContentTransformChanged);
        };
        _topBlurTopLayerChanged = (_, e) =>
        {
            if (e.Property == Visual.RenderTransformProperty)
            {
                ObserveTopBlurTransform(_topLayer, ref _topBlurTopTransform, _topBlurTopTransformChanged);
                UpdateTopBlurMasks();
            }
            else if (e.Property is var property && (property == Visual.OpacityProperty || property == Visual.IsVisibleProperty))
                UpdateTopBlurBackdropVisibility();
        };
        _topBlurLeftPaneChanged = (_, e) => OnTopBlurPaneChanged(_left, ref _topBlurLeftTransform, _topBlurLeftTransformChanged, e);
        _topBlurRightPaneChanged = (_, e) => OnTopBlurPaneChanged(_right, ref _topBlurRightTransform, _topBlurRightTransformChanged, e);

        if (_topBlurWindowChanged is { } windowChanged) PropertyChanged += windowChanged;
        if (_topBlurContentChanged is { } contentChanged) _content.PropertyChanged += contentChanged;
        if (_topBlurTopLayerChanged is { } topChanged) _topLayer.PropertyChanged += topChanged;
        ObserveTopBlurTransform(_content, ref _topBlurContentTransform, _topBlurContentTransformChanged);
        ObserveTopBlurTransform(_topLayer, ref _topBlurTopTransform, _topBlurTopTransformChanged);
        _panes.SizeChanged += (_, _) =>
        {
            UpdateTopBlurMasks();
            RefreshTopBlurSources();
        };
        _topLayer.SizeChanged += (_, _) => RefreshTopBlurSources();
        _topBlurGroup!.SizeChanged += (_, _) => RefreshTopBlurSources();

        _floatPager.Opened += OnFloatPagerOpened;
        Closed += (_, _) => OnTopBlurWindowClosed();
        _topBlurLeftEdited = RefreshTopBlurSources;
        _topBlurRightEdited = RefreshTopBlurSources;
        _topBlurLeftScroll = (_, _) => RefreshTopBlurSources();
        _topBlurRightScroll = (_, _) => RefreshTopBlurSources();
        _topBlurLeftLines = (_, _) => RefreshTopBlurSources();
        _topBlurRightLines = (_, _) => RefreshTopBlurSources();

        ApplyTopBlur();
        UpdateTopBlurVisibility();
    }

    private void InitializeTopBlurVisuals()
    {
        _topBlurLeft = new TopBlurVisual
        {
            IsHitTestVisible = false,
            ClipToBounds = true,
            Effect = new BlurEffect { Radius = ChromeTokens.TopBackdrop.BlurRadius },
            IsVisible = false,
        };
        _topBlurRight = new TopBlurVisual
        {
            IsHitTestVisible = false,
            ClipToBounds = true,
            Effect = new BlurEffect { Radius = ChromeTokens.TopBackdrop.BlurRadius },
            IsVisible = false,
        };
        _topBlurTint = new Border { IsHitTestVisible = false, IsVisible = false };
        _topBlurGroup = new Grid { IsHitTestVisible = false, ClipToBounds = true, IsVisible = false };
        _topBlurGroup.Children.Add(_topBlurLeft);
        _topBlurGroup.Children.Add(_topBlurRight);
        _topBlurGroup.Children.Add(_topBlurTint);
        _topLayer.Children.Insert(0, _topBlurGroup);
    }

    private void OnTopBlurPaneChanged(Visual pane, ref TranslateTransform? transform, EventHandler<AvaloniaPropertyChangedEventArgs>? changed, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Visual.RenderTransformProperty)
        {
            ObserveTopBlurTransform(pane, ref transform, changed);
            UpdateTopBlurMasks();
        }
        else if (e.Property == Visual.OpacityProperty || e.Property == Visual.IsVisibleProperty)
        {
            UpdateTopBlurBackdropVisibility();
            RefreshTopBlurSources();
        }
    }

    private void ObserveTopBlurTransform(Visual visual, ref TranslateTransform? current, EventHandler<AvaloniaPropertyChangedEventArgs>? changed)
    {
        if (current is not null && changed is not null) current.PropertyChanged -= changed;
        current = visual.RenderTransform as TranslateTransform;
        if (current is not null && changed is not null) current.PropertyChanged += changed;
        RefreshTopBlurSources();
    }

    private void UpdateTopBlurVisibility()
    {
        if (IsVisible)
        {
            AttachTopBlurSourceObservers();
            if (Settings.Bars == "float" && _bottomShown) _floatPager.IsOpen = true;
        }
        else
        {
            DetachTopBlurSourceObservers();
            if (Settings.Bars == "float") _floatPager.IsOpen = false;
        }
        UpdateTopBlurBackdropVisibility();
    }

    private void AttachTopBlurSourceObservers()
    {
        if (_topBlurSourcesAttached) return;
        if (_topBlurLeftEdited is { } leftEdited) _left.Edited += leftEdited;
        if (_topBlurRightEdited is { } rightEdited) _right.Edited += rightEdited;
        if (_topBlurLeftScroll is { } leftScroll) ((ScrollableTopInsetTextEditor)_left.Editor).InsetTextArea.ScrollOffsetChanged += leftScroll;
        if (_topBlurRightScroll is { } rightScroll) ((ScrollableTopInsetTextEditor)_right.Editor).InsetTextArea.ScrollOffsetChanged += rightScroll;
        if (_topBlurLeftLines is { } leftLines) _left.Editor.TextArea.TextView.VisualLinesChanged += leftLines;
        if (_topBlurRightLines is { } rightLines) _right.Editor.TextArea.TextView.VisualLinesChanged += rightLines;
        if (_topBlurLeftPaneChanged is { } leftPaneChanged) _left.PropertyChanged += leftPaneChanged;
        if (_topBlurRightPaneChanged is { } rightPaneChanged) _right.PropertyChanged += rightPaneChanged;
        ObserveTopBlurTransform(_left, ref _topBlurLeftTransform, _topBlurLeftTransformChanged);
        ObserveTopBlurTransform(_right, ref _topBlurRightTransform, _topBlurRightTransformChanged);
        _topBlurSourcesAttached = true;
    }

    private void DetachTopBlurSourceObservers()
    {
        if (!_topBlurSourcesAttached) return;
        if (_topBlurLeftEdited is { } leftEdited) _left.Edited -= leftEdited;
        if (_topBlurRightEdited is { } rightEdited) _right.Edited -= rightEdited;
        if (_topBlurLeftScroll is { } leftScroll) ((ScrollableTopInsetTextEditor)_left.Editor).InsetTextArea.ScrollOffsetChanged -= leftScroll;
        if (_topBlurRightScroll is { } rightScroll) ((ScrollableTopInsetTextEditor)_right.Editor).InsetTextArea.ScrollOffsetChanged -= rightScroll;
        if (_topBlurLeftLines is { } leftLines) _left.Editor.TextArea.TextView.VisualLinesChanged -= leftLines;
        if (_topBlurRightLines is { } rightLines) _right.Editor.TextArea.TextView.VisualLinesChanged -= rightLines;
        if (_topBlurLeftPaneChanged is { } leftPaneChanged) _left.PropertyChanged -= leftPaneChanged;
        if (_topBlurRightPaneChanged is { } rightPaneChanged) _right.PropertyChanged -= rightPaneChanged;
        if (_topBlurLeftTransform is not null && _topBlurLeftTransformChanged is not null) _topBlurLeftTransform.PropertyChanged -= _topBlurLeftTransformChanged;
        if (_topBlurRightTransform is not null && _topBlurRightTransformChanged is not null) _topBlurRightTransform.PropertyChanged -= _topBlurRightTransformChanged;
        _topBlurLeftTransform = _topBlurRightTransform = null;
        _topBlurSourcesAttached = false;
    }

    private void ApplyTopBlur()
    {
        if (_topBlurLeft is null || _topBlurRight is null || _topBlurTint is null || _topBlurGroup is null) return;

        // The offscreen Avalonia 12.1.4 Skia/Win32 probe confirms BlurEffect works in software mode.
        _topBlurGroup.Background = null;
        _topBlurGroup.OpacityMask = TopBlurMask();
        _topBlurTint.Background = new SolidColorBrush(_palette.Canvas, ChromeTokens.TopBackdrop.TintAlpha);
        UpdateTopBlurBackdropVisibility();
        RefreshTopBlurSources();
    }

    private void UpdateTopBlurBackdropVisibility()
    {
        if (_topBlurGroup is null || _topBlurLeft is null || _topBlurRight is null) return;
        var topActive = IsVisible && Settings.Bars is "overlay" or "float"
            && _topLayer.IsVisible && _topLayer.Opacity > 0;
        _topBlurGroup.IsVisible = topActive;
        _topBlurLeft.IsVisible = topActive && _left.IsVisible;
        _topBlurRight.IsVisible = topActive && _right.IsVisible;
        _topBlurLeft.Opacity = _left.Opacity;
        _topBlurRight.Opacity = _right.Opacity;
        _topBlurTint!.IsVisible = topActive;
        UpdateTopBlurMasks();
        if (topActive) RefreshTopBlurSources();
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

    private void RefreshTopBlurSources()
    {
        if (!IsVisible || _topBlurRefreshScheduled || _topBlurGroup?.IsVisible != true) return;
        _topBlurRefreshScheduled = true;
        Dispatcher.UIThread.Post(RefreshTopBlurSourcesAfterLayout, DispatcherPriority.Render);
    }

    private void RefreshTopBlurSourcesAfterLayout()
    {
        _topBlurRefreshScheduled = false;
        if (!IsVisible || _topBlurGroup?.IsVisible != true) return;
        var root = (Visual?)_scaler.Child;
        if (root is null || _panes.Bounds.Width <= 0 || _panes.Bounds.Height <= 0) return;

        if (_left.IsVisible && _topBlurLeft is { } leftBlur)
            SetFreshTopBlurSource(leftBlur, _left.Editor.TextArea.TextView, root);
        if (_right.IsVisible && _topBlurRight is { } rightBlur)
            SetFreshTopBlurSource(rightBlur, _right.Editor.TextArea.TextView, root);
    }

    private void SetFreshTopBlurSource(TopBlurVisual target, Visual sourceVisual, Visual coordinateRoot)
    {
        if (target.Bounds.Width <= 0 || target.Bounds.Height <= 0) return;
        var targetOrigin = target.TranslatePoint(default, coordinateRoot);
        var sourceOrigin = sourceVisual.TranslatePoint(default, coordinateRoot);
        if (targetOrigin is not { } targetPoint || sourceOrigin is not { } sourcePoint) return;
        var source = new Rect(targetPoint.X - sourcePoint.X, targetPoint.Y - sourcePoint.Y, target.Bounds.Width, target.Bounds.Height);
        // Avalonia 12.1.4 VisualBrush caches source content; a new brush makes each dirty refresh sample current visuals.
        target.Background = new VisualBrush(sourceVisual)
        {
            Stretch = Stretch.Fill,
            SourceRect = new RelativeRect(source, RelativeUnit.Absolute),
            DestinationRect = new RelativeRect(new Rect(0, 0, target.Bounds.Width, target.Bounds.Height), RelativeUnit.Absolute),
        };
        target.InvalidateVisual();
    }

    private void UpdateTopBlurMasks()
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

    /// <summary>Applies window theme and transparency to the popup hosting the floating bars.</summary>
    private void OnFloatPagerOpened(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var popup = _bars.Select(bar => TopLevel.GetTopLevel(bar.Root)).FirstOrDefault(top => top is not null);
            if (!_floatPager.IsOpen || popup is null || ReferenceEquals(popup, this) || popup is Window) return;
            popup.RequestedThemeVariant = RequestedThemeVariant;
            popup.TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
            popup.TransparencyBackgroundFallback = Brushes.Transparent;
            DesktopPlatform.ConfigurePopupWindow(popup);
        }, DispatcherPriority.Loaded);
    }

    private void OnTopBlurWindowClosed()
    {
        DetachTopBlurSourceObservers();
        if (_topBlurWindowChanged is not null) PropertyChanged -= _topBlurWindowChanged;
        if (_topBlurContentChanged is not null) _content.PropertyChanged -= _topBlurContentChanged;
        if (_topBlurTopLayerChanged is not null) _topLayer.PropertyChanged -= _topBlurTopLayerChanged;
        if (_topBlurContentTransform is not null && _topBlurContentTransformChanged is not null) _topBlurContentTransform.PropertyChanged -= _topBlurContentTransformChanged;
        if (_topBlurTopTransform is not null && _topBlurTopTransformChanged is not null) _topBlurTopTransform.PropertyChanged -= _topBlurTopTransformChanged;
    }

    private sealed class TopBlurVisual : Control
    {
        public IBrush? Background { get; set; }

        public override void Render(DrawingContext context) => context.DrawRectangle(Background, null, new Rect(Bounds.Size));
    }
}
