namespace AntiQuickPad.Carousel;

// Presentation bridge: only numbers and callbacks cross into the motion state machine.
internal interface ICarouselSurface
{
    double PageWidth { get; }
    void SetReadOnly(bool value);
    void PreparePreview(int target);
    void HidePreview();
    void RenderOffset(double current, double preview);
    void Reset();
    void ReleasePointer();
    void RequestFrame(int version);
}

// A burst owns one destination for its entire animation and until 140ms idle.
// No accumulated wheel delta and no queued page requests: a notch cannot skip pages.
internal sealed class WheelGate
{
    private long _last = long.MinValue / 2;
    public int Request(double delta, long now, bool busy)
    {
        if (Math.Abs(delta) < .001) return 0;
        var fresh = now - _last > 140;
        _last = now;
        return !busy && fresh ? Math.Sign(delta) : 0;
    }
}

/// <summary>Input policy, target selection and animation samples; no Avalonia controls or clock.</summary>
internal sealed class CarouselMotion(ICarouselSurface surface, Func<int> index, Func<int> count, Action<int> select, Action activity)
{
    public const double SlideMs = 280;
    private readonly ICarouselSurface _surface = surface;
    private readonly Func<int> _index = index, _count = count;
    private readonly Action<int> _select = select;
    private readonly Action _activity = activity;
    private readonly WheelGate _wheel = new();
    private int _animationVersion, _target, _previewIndex = -1;
    private TimeSpan? _firstFrame;
    private bool _settling, _animating, _dragging, _previewVisible;
    private double _from, _to, _offset, _origin, _lastX, _lastMs, _velocity;
    internal event Action? MotionChanged;
    internal List<CarouselController.FrameSample> Frames { get; } = [];
    internal double Offset => _offset;
    internal bool IsAnimating => _animating;
    internal bool IsDragging => _dragging;
    public bool IsBusy => _animating || _dragging;
    private double PageWidth => _surface.PageWidth;

    public void RequestWheel(double delta, long now)
    {
        var step = _wheel.Request(delta, now, IsBusy);
        _activity();
        if (step == 0) return;
        var target = Math.Clamp(_index() + step, 0, _count() - 1);
        if (target == _index()) return;
        Prepare(target); StartAnimation(target);
    }

    public void BeginDrag(double x, double now)
    {
        if (_animating) return;
        _dragging = true; _origin = _lastX = x; _lastMs = now; _velocity = 0;
        _surface.SetReadOnly(true); MotionChanged?.Invoke(); _activity();
    }

    public void MoveDrag(double x, double now)
    {
        if (!_dragging) return;
        var elapsed = now - _lastMs;
        // Preserve the last movement sample on pointer release at the same position.
        if (Math.Abs(x - _lastX) > .01)
        {
            _velocity = (x - _lastX) / Math.Max(1, elapsed); _lastX = x; _lastMs = now;
        }
        var distance = Math.Clamp(x - _origin, -PageWidth, PageWidth);
        var direction = distance < 0 ? 1 : -1;
        var target = Math.Clamp(_index() + direction, 0, _count() - 1);
        if (target != _index()) Prepare(target);
        else { _surface.HidePreview(); _previewVisible = false; _previewIndex = -1; }
        SetOffset(target == _index() ? distance * .25 : distance);
    }

    public void EndDrag(double now)
    {
        if (!_dragging) return;
        _dragging = false;
        var velocity = now - _lastMs > 80 ? 0 : _velocity;
        var target = SnapIndex(_index(), _offset, velocity, PageWidth, _count());
        if (target != _index()) Prepare(target);
        StartAnimation(target, settling: true);
    }

    public static int SnapIndex(int index, double offset, double velocity, double width, int count)
    {
        var direction = Math.Abs(velocity) >= .55 ? -Math.Sign(velocity) :
            Math.Abs(offset) >= width / 2 ? -Math.Sign(offset) : 0;
        return Math.Clamp(index + direction, 0, count - 1);
    }

    private void Prepare(int target)
    {
        if (_previewIndex == target && _previewVisible) return;
        _previewIndex = target; _previewVisible = true;
        _surface.PreparePreview(target);
        SetOffset(_offset);
    }
    private void SetOffset(double offset)
    {
        _offset = offset;
        var direction = _previewIndex > _index() ? 1 : -1;
        _surface.RenderOffset(offset, offset + direction * PageWidth);
    }

    internal static double EasedOffset(double from, double to, double progress, bool settling)
    {
        var t = Math.Clamp(progress, 0, 1);
        var eased = settling ? 1 - Math.Pow(1 - t, 3) : t * t * (3 - 2 * t);
        return from + (to - from) * eased;
    }

    private void StartAnimation(int target, bool settling = false)
    {
        _target = target; _from = _offset; _settling = settling;
        _to = target == _index() ? 0 : -Math.Sign(target - _index()) * PageWidth;
        _surface.SetReadOnly(true); _animating = true; _firstFrame = null;
        Frames.Clear(); Frames.Add(new(0, _from));
        var version = ++_animationVersion;
        MotionChanged?.Invoke();
        _surface.RequestFrame(version);
    }

    // The view delivers frame timestamps. Stale callbacks remain inert after cancellation.
    internal bool Advance(TimeSpan time, int version)
    {
        if (!_animating || version != _animationVersion) return false;
        _firstFrame ??= time;
        var elapsed = (time - _firstFrame.Value).TotalMilliseconds;
        var progress = Math.Clamp(elapsed / SlideMs, 0, 1);
        SetOffset(EasedOffset(_from, _to, progress, _settling));
        Frames.Add(new(elapsed, _offset));
        if (progress < 1) return true;
        var target = _target; Cancel();
        if (target != _index()) _select(target);
        _activity();
        return false;
    }
    public void Cancel()
    {
        ++_animationVersion; _animating = _dragging = false; _surface.ReleasePointer();
        _offset = 0; _surface.Reset();
        _previewVisible = false; _previewIndex = -1; _surface.SetReadOnly(false); MotionChanged?.Invoke();
    }

    public static void RunSmoke()
    {
        foreach (var hz in new[] { 60, 120, 144 })
        {
            var offsets = Enumerable.Range(0, (int)Math.Ceiling(SlideMs * hz / 1000) + 1)
                .Select(n => EasedOffset(0, -800, n * 1000d / hz / SlideMs, false)).ToArray();
            var deltas = offsets.Zip(offsets.Skip(1), (a,b) => a-b).ToArray();
            if (deltas.Any(d => d < 0) || offsets[^1] != -800 || deltas.Max() > 800 * 1.5 / SlideMs * 1000 / hz + .01)
                throw new Exception("Per-frame ease sampling failed");
            Console.WriteLine($"T014 curve {hz}Hz: samples={offsets.Length}, maxDelta={deltas.Max():F4}, monotonic PASS");
        }
        var gate = new WheelGate(); var index = 1;
        index += gate.Request(1, 0, false);
        for (var t = 10; t <= 260; t += 10) index += gate.Request(1, t, t < 220);
        if (index != 2) throw new Exception("Wheel notch/burst skipped more than one note.");
        if (gate.Request(1, 450, false) != 1) throw new Exception("Next separate wheel burst lost.");
        if (SnapIndex(1, -450, 0, 800, 4) != 2 || SnapIndex(1, -30, 0, 800, 4) != 1 ||
            SnapIndex(1, -30, -.8, 800, 4) != 2 || SnapIndex(0, 450, 0, 800, 4) != 0)
            throw new Exception("Drag distance/velocity snap failed.");
        // Drive the extracted state machine with numbers and a control-free surface.
        var surface = new SmokeSurface(); var selected = 1;
        var motion = new CarouselMotion(surface, () => selected, () => 4, i => selected = i, () => { });
        motion.BeginDrag(500, 0); motion.MoveDrag(100, 100);
        if (motion.Offset != -400 || surface.Preview != 2 || !surface.ReadOnly)
            throw new Exception("Numeric motion lost 1:1 drag or preview/read-only state.");
        motion.EndDrag(200); // Last velocity is stale; exact half-page distance commits.
        var version = surface.Version;
        if (!motion.Advance(TimeSpan.Zero, version) ||
            !motion.Advance(TimeSpan.FromMilliseconds(140), version) || motion.Offset != -750 ||
            motion.Advance(TimeSpan.FromMilliseconds(280), version) || selected != 2 || motion.IsBusy || surface.ReadOnly)
            throw new Exception("Numeric settling frame/selection lifecycle failed.");
        if (!motion.Frames.Select(f => f.Offset).SequenceEqual(new[] { -400d, -400d, -750d, -800d }))
            throw new Exception("Numeric settling frame offsets changed.");
        motion.RequestWheel(1, 0); version = surface.Version;
        motion.RequestWheel(1, 10); motion.Advance(TimeSpan.Zero, version);
        motion.Advance(TimeSpan.FromMilliseconds(140), version);
        if (motion.Offset != -400) throw new Exception("Numeric slide midpoint changed.");
        motion.Cancel();
        if (motion.Advance(TimeSpan.FromMilliseconds(280), version) || selected != 2 || motion.Offset != 0)
            throw new Exception("Cancelled numeric motion accepted a stale frame.");
        motion.RequestWheel(1, 150); // Exactly 140 ms idle is still the same burst.
        if (motion.IsBusy) throw new Exception("Wheel idle boundary changed.");
        motion.RequestWheel(1, 291); // Strictly more than 140 ms starts a new burst.
        if (!motion.IsBusy) throw new Exception("Fresh numeric wheel burst was lost.");
        motion.Cancel();
        Console.WriteLine("M9c-behaviour-presentation-split numeric motion PASS: drag, snap, exact frame offsets, selection, cancellation, wheel idle boundary.");
    }

    private sealed class SmokeSurface : ICarouselSurface
    {
        public double PageWidth => 800;
        internal bool ReadOnly;
        internal int Preview = -1, Version;
        public void SetReadOnly(bool value) => ReadOnly = value;
        public void PreparePreview(int target) => Preview = target;
        public void HidePreview() => Preview = -1;
        public void RenderOffset(double current, double preview) { }
        public void Reset() => Preview = -1;
        public void ReleasePointer() { }
        public void RequestFrame(int version) => Version = version;
    }
}
