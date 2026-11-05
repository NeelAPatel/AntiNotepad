using AntiNotepad.Model;
using AntiNotepad.Theme;

namespace AntiNotepad;

/// <summary>Disposable capability smoke: calculator, mode detection, timer parsing, store round trip. `AntiNotepad --selftest`.</summary>
internal static partial class SelfTest
{
    public static int Run()
    {
        var failures = new List<string>();
        void Check(bool ok, string what) { if (!ok) failures.Add(what); }

        var layerCheck = CheckLayers();
        foreach (var line in layerCheck.Lines) Console.WriteLine(line);
        foreach (var failure in layerCheck.Failures) Check(false, failure);

        var desktopPlatformSurfacePass = DesktopPlatformSurfaceChecks(out var desktopPlatformSurfaceReport);
        Console.WriteLine(desktopPlatformSurfacePass ? "PLATFORM DesktopPlatform surface" : "PLATFORM DesktopPlatform surface FAIL " + desktopPlatformSurfaceReport);
        Check(desktopPlatformSurfacePass, "platform DesktopPlatform surface: " + desktopPlatformSurfaceReport);

        var pinvokeHolderPass = PInvokeHolderChecks(out var pinvokeHolderReport);
        Console.WriteLine(pinvokeHolderPass ? "PLATFORM P/Invoke holders" : "PLATFORM P/Invoke holders FAIL " + pinvokeHolderReport);
        Check(pinvokeHolderPass, "platform P/Invoke holders: " + pinvokeHolderReport);

        var staticExposurePass = StaticServiceExposureChecks(out var staticExposureReport);
        Console.WriteLine(staticExposurePass ? "STATIC service exposure " + staticExposureReport : "STATIC service exposure FAIL " + staticExposureReport);
        Check(staticExposurePass, "static service exposure: " + staticExposureReport);

        var environmentReadPass = EnvironmentReadConfinementChecks(out var environmentReadReport);
        Console.WriteLine(environmentReadPass ? "ENV DATA/DEV confinement " + environmentReadReport : "ENV DATA/DEV confinement FAIL " + environmentReadReport);
        Check(environmentReadPass, "environment DATA/DEV confinement: " + environmentReadReport);

        var hotkeySwapPass = HotkeySwapChecks(out var hotkeySwapReport);
        Console.WriteLine(hotkeySwapPass ? "HOTKEYS swap " + hotkeySwapReport : "HOTKEYS swap FAIL " + hotkeySwapReport);
        Check(hotkeySwapPass, "hotkeys swap: " + hotkeySwapReport);

        var clipboardPolicyPass = ClipboardPolicyChecks(out var clipboardPolicyReport);
        Console.WriteLine(clipboardPolicyPass ? "CLIPBOARD policy " + clipboardPolicyReport : "CLIPBOARD policy FAIL " + clipboardPolicyReport);
        Check(clipboardPolicyPass, "clipboard policy: " + clipboardPolicyReport);

        var runtimeLogPass = RuntimeLogChecks(out var runtimeLogReport);
        Console.WriteLine(runtimeLogPass ? "RUNTIME LOG " + runtimeLogReport : "RUNTIME LOG FAIL " + runtimeLogReport);
        Check(runtimeLogPass, "runtime log: " + runtimeLogReport);

        var taskbarTracePass = TaskbarTraceChecks(out var taskbarTraceReport);
        Console.WriteLine(taskbarTracePass ? "TASKBAR TRACE " + taskbarTraceReport : "TASKBAR TRACE FAIL " + taskbarTraceReport);
        Check(taskbarTracePass, "taskbar trace: " + taskbarTraceReport);

        var mathNotesPass = MathNotesChecks(out var mathNotesReport);
        Console.WriteLine(mathNotesPass ? "MATH NOTES " + mathNotesReport : "MATH NOTES FAIL " + mathNotesReport);
        Check(mathNotesPass, "math notes: " + mathNotesReport);

        var listEditingPass = ListEditingChecks(out var listEditingReport);
        Console.WriteLine(listEditingPass ? "LIST EDITING " + listEditingReport : "LIST EDITING FAIL " + listEditingReport);
        Check(listEditingPass, "list editing: " + listEditingReport);

        var listCommandsPass = ListCommandsChecks(out var listCommandsReport);
        Console.WriteLine(listCommandsPass ? "LIST COMMANDS " + listCommandsReport : "LIST COMMANDS FAIL " + listCommandsReport);
        Check(listCommandsPass, "list commands: " + listCommandsReport);

        Check(Calc.Calculator.SelfTest(out var calcReport), "calculator: " + calcReport);
        Check(ChromeTokens.PillHeight == 42 && ChromeTokens.PagerStripHeight == 50
            && ChromeTokens.TopBackdrop.MidpointStop == 46 && ChromeTokens.TopScrollableWhitespace == 40
            && Editor.ScrollableTopInsetTextArea.VisibleGapFor(ChromeTokens.TopScrollableWhitespace, 0) == 40
            && Editor.ScrollableTopInsetTextArea.VisibleGapFor(ChromeTokens.TopScrollableWhitespace, 20) == 20
            && Editor.ScrollableTopInsetTextArea.VisibleGapFor(ChromeTokens.TopScrollableWhitespace, 40) == 0
            && Editor.ScrollableTopInsetTextArea.DocumentOffsetFor(ChromeTokens.TopScrollableWhitespace, 60) == 20
            && 480 - ChromeTokens.BarEdgeInset - ChromeTokens.ButtonSize == 444
            && ChromeTokens.TopBackdrop.MaskAlphaAt(0) == 1
            && ChromeTokens.TopBackdrop.MaskAlphaAt(40) == 1
            && ChromeTokens.TopBackdrop.MaskAlphaAt(46) == 0.5
            && ChromeTokens.TopBackdrop.MaskAlphaAt(52) == 0
            && ChromeTokens.TopBackdrop.TintAlpha == 0.4,
            "A35-bar-insets chrome geometry and scrollable top inset mapping");
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

        Check(AppInfoSelfTest(out var identityReport), "app identity: " + identityReport);
        Check(DevIdentitySelfTest(out var devIdentityReport), "dev identity: " + devIdentityReport);

        Check(Calc.Calculator.EvaluateInline(Editor.MathNotes.BlankLabel("spent 12.50 + 8.25 + 3.99 ="))?.Value is > 24.73 and < 24.75, "inline with a label");
        Check(Calc.Calculator.EvaluateInline(Editor.MathNotes.StripListMarker("- 19.99 * 3 ="))?.Value is > 59.96 and < 59.98, "inline after a bullet");

        var dir = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-selftest-{Guid.NewGuid():N}");
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

        Check(DefaultDataMigrationSelfTest(out var migrationReport), "default data folder migration: " + migrationReport);
        Check(InputLimitChecks(out var inputReport), "input limits: " + inputReport);
        Check(NotesLocationChecks(out var notesReport), "notes location and recovery: " + notesReport);
        var notesSessionPass = NotesMoveChecks(out var notesSessionReport);
        Console.WriteLine(notesSessionPass ? "NOTES SESSION move " + notesSessionReport : "NOTES SESSION move FAIL " + notesSessionReport);
        Check(notesSessionPass, "notes session move: " + notesSessionReport);
        var notesUseFolderPass = NotesUseFolderChecks(out var notesUseFolderReport);
        Console.WriteLine(notesUseFolderPass ? "NOTES SESSION use-folder " + notesUseFolderReport : "NOTES SESSION use-folder FAIL " + notesUseFolderReport);
        Check(notesUseFolderPass, "notes session use-folder: " + notesUseFolderReport);
        Check(StartupRequestLatchChecks(out var startupRequestReport), "startup request latch: " + startupRequestReport);
        Check(NoteSwipeChecks(out var swipeReport), "note swipe: " + swipeReport);
        Check(WelcomeSlotsSelfTest(out var welcomeReport), "welcome slots: " + welcomeReport);
        Check(SearchSpeedChecks(out var searchReport), "search speed: " + searchReport);
        Check(SettingsSearchMatchingChecks(out var settingsSearchReport), "A27-settings-search-build-matches: " + settingsSearchReport);
        Check(IncrementalAnalysisChecks(out var analysisReport), "A29-incremental-analysis: " + analysisReport);
        Check(TaskbarChecks(out var taskbarReport), "taskbar: " + taskbarReport);
        Check(SessionEndCloseChecks(out var sessionEndReport), "session-end close: " + sessionEndReport);
        ContractsChecks(failures);

        Check(DesignChecks(out var designReport), "design dump: " + designReport);

        Console.WriteLine(failures.Count == 0 ? "SELFTEST PASS" : "SELFTEST FAIL\n" + string.Join("\n", failures));
        return failures.Count == 0 ? 0 : 1;
    }
}
