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

}
