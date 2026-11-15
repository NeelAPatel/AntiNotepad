using AntiNotepad.Editor;
using AntiNotepad.Motion;

namespace AntiNotepad;

internal static partial class SelfTest
{
    /// <summary>Checks the locked horizontal projection, edge band, wheel glide, and click-rate scaling.</summary>
    private static bool HorizontalScrollFeelChecks(out string report)
    {
        var failures = new List<string>();
        void Expect(bool ok, string name) { if (!ok) failures.Add(name); }

        Expect(UI.NoteSwipe.ShouldCommit(100, 0.25, 400), "projected forward swipe commits");
        Expect(UI.NoteSwipe.ShouldCommit(-100, -0.25, 400), "projected reverse swipe commits");
        Expect(!UI.NoteSwipe.ShouldCommit(100, -0.25, 400), "opposing projection does not commit");
        Expect(!UI.NoteSwipe.ShouldCommit(100, 0, 400), "short pull does not commit");

        var small = EdgeBand.Resist(1, 400);
        var next = EdgeBand.Resist(2, 400);
        Expect(small > 0 && small < 1 && next > small && Math.Abs(EdgeBand.Unresist(small, 400) - 1) < 0.01,
            "edge band is continuous, monotonic, and reversible");

        var wheel = new EdgeBand();
        wheel.Push(192, 400, wheel: true, 1000);
        var target = EdgeBand.Resist(192, 400);
        var noInitialJump = wheel.Shown == 0;
        wheel.Step(1016, 0.016, 400);
        Expect(noInitialJump && wheel.Shown > 0 && wheel.Shown < target,
            "wheel edge pull glides in without a one-frame jump");

        var notches = new NotchGainClock();
        var slow = notches.Next(1000);
        var quick = notches.Next(1100);
        Expect(Math.Abs(EdgeBand.NotchGain(400) - 0.2) < 0.001
            && Math.Abs(EdgeBand.NotchGain(100) - 0.8666667) < 0.001
            && quick > slow && quick <= 1,
            "notch gain scales with a smoothed click cadence");

        report = failures.Count == 0 ? "projection, band continuity, wheel glide, and notch scaling" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    /// <summary>Checks vertical edge holding, spring return, and the Reduce motion bypass.</summary>
    private static bool VerticalOverscrollChecks(out string report)
    {
        var failures = new List<string>();
        void Expect(bool ok, string name) { if (!ok) failures.Add(name); }
        var previous = Anim.Reduced;
        try
        {
            Anim.Reduced = false;
            var edge = new EdgeBand();
            edge.Push(-96, 500, wheel: false, 1000);
            edge.Step(1089, 0.016, 500);
            Expect(edge.Shown < 0 && !edge.Returning, "vertical pull holds before release");
            edge.Step(1090, 0.016, 500);
            Expect(edge.Returning, "vertical pull starts return after the touch quiet period");
            for (var now = 1106L; edge.Active && now < 5000; now += 16) edge.Step(now, 0.016, 500);
            Expect(!edge.Active && edge.Shown == 0, "vertical pull springs fully home");

            Anim.Reduced = true;
            var reduced = new EdgeBand();
            reduced.Push(96, 500, wheel: true, 6000);
            Expect(!reduced.Active && reduced.Shown == 0 && !reduced.Step(7000, 0.016, 500),
                "Reduce motion skips edge pull and spring");
        }
        finally { Anim.Reduced = previous; }

        report = failures.Count == 0 ? "touch quiet hold, spring return, and Reduce motion bypass" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    /// <summary>Checks that every vertical style carries outward speed into a continuous edge return.</summary>
    private static bool VerticalCarryOverChecks(out string report)
    {
        var failures = new List<string>();
        void Expect(bool ok, string name) { if (!ok) failures.Add(name); }

        (string Name, double StopSpeed, double Position, double Velocity, double Boundary)[] arrivals =
        [
            ("Lines", MotionTokens.VerticalSoftStopSpeed, 0, -240, 0),
            ("Free", MotionTokens.VerticalCoastStopSpeed, -1, -240, 0),
            ("Rate-matched", MotionTokens.VerticalSoftStopSpeed, 501, 240, 500),
            ("Wheel-driven", MotionTokens.VerticalFlyStopSpeed, 501, 240, 500)
        ];

        foreach (var arrival in arrivals)
        {
            var starts = SmoothScroll.TryGetOutwardEdgeArrival(arrival.Position, arrival.Velocity, arrival.StopSpeed, 500, out var boundary);
            Expect(starts && boundary == arrival.Boundary, $"{arrival.Name} carries an outward edge arrival");
            var thresholdVelocity = arrival.StopSpeed * Math.Sign(arrival.Velocity);
            Expect(SmoothScroll.TryGetOutwardEdgeArrival(arrival.Position, thresholdVelocity, arrival.StopSpeed, 500, out _),
                $"{arrival.Name} carries at its stop speed");
            var slow = Math.Max(0, arrival.StopSpeed - 0.1) * Math.Sign(arrival.Velocity);
            Expect(!SmoothScroll.TryGetOutwardEdgeArrival(arrival.Position, slow, arrival.StopSpeed, 500, out _),
                $"{arrival.Name} does not carry below its stop speed");
        }

        Expect(!SmoothScroll.TryGetOutwardEdgeArrival(-1, 240, MotionTokens.VerticalCoastStopSpeed, 500, out _)
            && !SmoothScroll.TryGetOutwardEdgeArrival(501, -240, MotionTokens.VerticalFlyStopSpeed, 500, out _),
            "inward speed does not carry through an edge");

        var previousReduced = Anim.Reduced;
        try
        {
            Anim.Reduced = false;
            var edge = new EdgeBand();
            edge.StartMoving(-240);
            var startsAtRestPosition = edge.Active && edge.Returning && edge.Shown == 0;
            edge.Step(1001, 0.001, 500);
            var firstFrameSpeed = edge.Shown / 0.001;
            var smallFirstFrame = Math.Abs(edge.Shown) < 1;
            var smallSpeedStep = Math.Abs(firstFrameSpeed - -240) < 10;
            edge.Step(1017, 0.016, 500);
            Expect(startsAtRestPosition && smallFirstFrame && smallSpeedStep && edge.Shown < 0 && Math.Abs(edge.Shown) < 5,
                "edge return begins at zero displacement with continuous speed and no frame jump");

            var slowCarry = new EdgeBand();
            slowCarry.StartMoving(MotionTokens.VerticalFlyStopSpeed);
            slowCarry.Step(2001, 0.016, 500);
            Expect(slowCarry.Active && slowCarry.Shown > 0,
                "a speed at the fly stop threshold reaches its spring peak before settling");

            var currentPull = edge.CurrentPull(500);
            edge.Push(-24, 500, wheel: true, 1018);
            Expect(!edge.Returning && edge.Active && Math.Abs(edge.Pull - (currentPull - 24)) < 0.001,
                "wheel input interrupts carried return from its current pull");

            Anim.Reduced = true;
            var reduced = new EdgeBand();
            reduced.StartMoving(-240);
            Expect(!reduced.Active && reduced.Shown == 0 && !reduced.Returning,
                "Reduce motion skips momentum carry-over");
        }
        finally { Anim.Reduced = previousReduced; }

        report = failures.Count == 0 ? "Lines, Free, Rate-matched and Wheel-driven carry outward speed continuously; slow and reduced arrivals bypass" : string.Join("; ", failures);
        return failures.Count == 0;
    }
}
