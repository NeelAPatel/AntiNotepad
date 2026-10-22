using Antipad.Model;

namespace Antipad;

/// <summary>Disposable capability smoke: calculator, mode detection, timer parsing, store round trip. `Antipad --selftest`.</summary>
internal static class SelfTest
{
    public static int Run()
    {
        var failures = new List<string>();
        void Check(bool ok, string what) { if (!ok) failures.Add(what); }

        Check(Calc.Calculator.SelfTest(out var calcReport), "calculator: " + calcReport);
        Check(Editor.TextCommands.SelfTest(out var textReport), "text commands: " + textReport);
        Check(Modes.Detect("math\n1+1").Mode == NoteMode.Math, "detect math");
        Check(Modes.Detect("code: py\nx").Argument == "py", "detect code py");
        Check(Modes.Detect("Timer\n5m").Mode == NoteMode.Timer, "detect timer case-insensitive");
        Check(Modes.Detect("mathematics").Mode == NoteMode.Plain, "plain when not a keyword");
        Check(Timers.TryParse("25m focus", out var d1, out var l1) && d1 == TimeSpan.FromMinutes(25) && l1 == "focus", "timer 25m");
        Check(Timers.TryParse("1h 30m", out var d2, out _) && d2 == TimeSpan.FromMinutes(90), "timer 1h 30m");
        Check(Timers.TryParse("05:00 eggs", out var d3, out var l3) && d3 == TimeSpan.FromMinutes(5) && l3 == "eggs", "timer clock");
        Check(!Timers.TryParse("buy 5m of rope", out _, out _), "timer ignores mid-sentence");
        Check(Editor.LinkGenerator.Shorten("https://learn.microsoft.com/en-us/windows/apps/design/style/acrylic") == "learn.microsoft.com/…/acrylic", "link shorten");

        Check(Calc.Calculator.EvaluateInline(Editor.PadView.BlankLabel("spent 12.50 + 8.25 + 3.99 ="))?.Value is > 24.73 and < 24.75, "inline with a label");
        Check(Calc.Calculator.EvaluateInline(Editor.PadView.StripListMarker("- 19.99 * 3 ="))?.Value is > 59.96 and < 59.98, "inline after a bullet");

        var dir = Path.Combine(Path.GetTempPath(), "antipad-selftest-" + Guid.NewGuid().ToString("N")[..6]);
        try
        {
            var store = new Store(dir);
            store.Load();
            store.Data.Stack[0].Text = "hello";
            store.Data.Slots[2].Text = "slot three";
            Check(store.Save() && store.Save(), "save twice (replace path)");
            var again = new Store(dir); again.Load();
            Check(again.Data.Stack[0].Text == "hello" && again.Data.Slots[2].Text == "slot three" && again.Data.Slots[2].Slot == 3, "store round trip");
            File.WriteAllText(Path.Combine(dir, "index.json"), "{ broken");
            var recovered = new Store(dir); recovered.Load();
            Check(recovered.Data.Stack[0].Text == "hello", "recovers from .bak");
        }
        finally { try { Directory.Delete(dir, true); } catch { } }

        Console.WriteLine(failures.Count == 0 ? "SELFTEST PASS" : "SELFTEST FAIL\n" + string.Join("\n", failures));
        return failures.Count == 0 ? 0 : 1;
    }
}
