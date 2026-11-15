// Horizontal rubber-band resistance, wheel-rate scaling, and critically damped return physics.
namespace AntiNotepad.Motion;

/// <summary>Maps raw edge pulls to a bounded visual band and returns them without a snap.</summary>
internal sealed class EdgeBand
{
    private double _pull, _shown, _velocity;
    private long _lastInput;
    private bool _wheel, _returning;

    public bool Active { get; private set; }
    public double Pull => _pull;
    public double Shown => _shown;

    /// <summary>Returns the raw pull represented by the current display during a spring return.</summary>
    public double CurrentPull(double dimension) => _returning ? Unresist(_shown, dimension) : _pull;

    /// <summary>Adds input at an edge, retaining the current displayed pull if input interrupts its return.</summary>
    public void Push(double delta, double dimension, bool wheel, long now)
    {
        if (Anim.Reduced) { Reset(); return; }
        if (_returning)
        {
            _pull = CurrentPull(dimension);
            _velocity = 0;
            _returning = false;
        }

        _pull += delta;
        _lastInput = now;
        _wheel = wheel;
        Active = true;
        if (!wheel) _shown = Resist(_pull, dimension);
    }

    /// <summary>Advances wheel glide, the quiet hold, and the spring return.</summary>
    public bool Step(long now, double dt, double dimension)
    {
        if (Anim.Reduced) { Reset(); return false; }
        if (!Active) return false;

        if (!_returning)
        {
            var target = Resist(_pull, dimension);
            if (_wheel)
                _shown += (target - _shown) * (1 - Math.Exp(-dt * 1000 / MotionTokens.EdgeWheelGlide));
            else
                _shown = target;

            var hold = _wheel ? MotionTokens.EdgeWheelHold : MotionTokens.EdgeTouchQuiet;
            if (now - _lastInput >= hold)
            {
                _returning = true;
                _velocity = 0;
            }
            return true;
        }

        _shown = SpringStep(_shown, ref _velocity, 0, dt);
        if (Math.Abs(_shown) < 0.3 && Math.Abs(_velocity) < 10)
        {
            Reset();
            return false;
        }
        return true;
    }

    /// <summary>Returns the signed rubber-band displacement for a raw pull and view dimension.</summary>
    public static double Resist(double pull, double dimension)
    {
        if (dimension <= 0) return 0;
        var magnitude = Math.Abs(pull);
        return Math.Sign(pull) * (1 - 1 / (magnitude * MotionTokens.EdgeBandRatio / dimension + 1)) * dimension;
    }

    /// <summary>Recovers a raw pull when a new gesture interrupts the spring return.</summary>
    public static double Unresist(double shown, double dimension)
    {
        if (dimension <= 0) return 0;
        var magnitude = Math.Clamp(Math.Abs(shown), 0, dimension - 0.01);
        return Math.Sign(shown) * magnitude * dimension / (MotionTokens.EdgeBandRatio * (dimension - magnitude));
    }

    /// <summary>Converts a smoothed wheel-click interval into the lab's slow-to-fast nudge gain.</summary>
    public static double NotchGain(double gapMilliseconds) =>
        0.2 + 0.8 * Math.Clamp((1000 / Math.Max(1, gapMilliseconds) - 2.5) / 9, 0, 1);

    /// <summary>Advances a critically damped spring in stable substeps.</summary>
    public static double SpringStep(double position, ref double velocity, double target, double dt)
    {
        var omega = 2 * Math.PI / (MotionTokens.EdgeSpringPeriod / 1000);
        var count = Math.Max(1, (int)Math.Ceiling(dt / 0.002));
        var step = dt / count;
        for (var i = 0; i < count; i++)
        {
            velocity += (-omega * omega * (position - target) - 2 * MotionTokens.EdgeSpringDamping * omega * velocity) * step;
            position += velocity * step;
        }
        return position;
    }

    /// <summary>Clears a completed or canceled pull and its pending spring state.</summary>
    public void Reset()
    {
        _pull = _shown = _velocity = 0;
        _lastInput = 0;
        _wheel = _returning = Active = false;
    }
}

/// <summary>Tracks click cadence independently for each scroll axis.</summary>
internal sealed class NotchGainClock
{
    private long _last;
    private double _gap;

    /// <summary>Records a click and returns its gain from the smoothed interval.</summary>
    public double Next(long now)
    {
        var elapsed = _last == 0 ? 1000 : now - _last;
        _last = now;
        _gap = elapsed >= 400 || _gap <= 0 ? Math.Min(elapsed, 400) : _gap * 0.4 + elapsed * 0.6;
        return EdgeBand.NotchGain(_gap);
    }

    /// <summary>Forgets the prior device cadence after a surface is reset.</summary>
    public void Reset() { _last = 0; _gap = 0; }
}
