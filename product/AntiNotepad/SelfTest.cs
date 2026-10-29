using AntiNotepad.Model;
using AntiNotepad.Platform;
using AntiNotepad.Theme;
using AntiNotepad.UI;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Platform;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

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

        Console.WriteLine(failures.Count == 0 ? "SELFTEST PASS" : "SELFTEST FAIL\n" + string.Join("\n", failures));
        return failures.Count == 0 ? 0 : 1;
    }

    private static bool ShortenedLinkChecks(out string report)
    {
        // Exercise the glyph's real press hook without a formatter, window, or browser.
        // Layout needs a platform formatter; dispatch stays headless and uses a stub opener.
        var glyph = (Editor.GlyphElement)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(Editor.GlyphElement));
        var hook = typeof(Editor.GlyphElement).GetMethod("OnPointerPressed", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var calls = 0;
        var sawControl = false;
        typeof(Editor.GlyphElement).GetProperty("Pressed")!.SetValue(glyph,
            (Action<Avalonia.Input.PointerPressedEventArgs>)(e =>
            {
                calls++;
                sawControl = e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control);
                if (sawControl) e.Handled = true;
            }));
        using var pointer = new Avalonia.Input.Pointer(1, Avalonia.Input.PointerType.Mouse, true);
        Avalonia.Input.PointerPressedEventArgs Press(bool left, Avalonia.Input.KeyModifiers modifiers) =>
            new(null, pointer, null!, default, 0,
                new Avalonia.Input.PointerPointProperties(
                    left ? Avalonia.Input.RawInputModifiers.LeftMouseButton : Avalonia.Input.RawInputModifiers.RightMouseButton,
                    left ? Avalonia.Input.PointerUpdateKind.LeftButtonPressed : Avalonia.Input.PointerUpdateKind.RightButtonPressed),
                modifiers);
        var control = Press(true, Avalonia.Input.KeyModifiers.Control);
        hook.Invoke(glyph, [control]);
        var controlPass = calls == 1 && sawControl && control.Handled;
        var plain = Press(true, Avalonia.Input.KeyModifiers.None);
        hook.Invoke(glyph, [plain]);
        var plainPass = calls == 2 && !sawControl && !plain.Handled;
        hook.Invoke(glyph, [Press(false, Avalonia.Input.KeyModifiers.Control)]);
        var rightPass = calls == 2;
        typeof(Editor.GlyphElement).GetProperty("IsLink")!.SetValue(glyph, true);
        calls = 0;
        var tunnel = Press(true, Avalonia.Input.KeyModifiers.Control);
        var tunnelPass = Editor.PadView.TryPressLink(glyph, tunnel) && tunnel.Handled && calls == 1;
        hook.Invoke(glyph, [tunnel]);
        var oncePass = calls == 1 && !Editor.PadView.TryPressLink(glyph, tunnel);
        var plainTunnel = Press(true, Avalonia.Input.KeyModifiers.None);
        var tunnelPlainPass = !Editor.PadView.TryPressLink(glyph, plainTunnel) && !plainTunnel.Handled && calls == 1;
        var tunnelRightPass = !Editor.PadView.TryPressLink(glyph, Press(false, Avalonia.Input.KeyModifiers.Control)) && calls == 1;
        var missingPass = !Editor.PadView.TryPressLink(null, Press(true, Avalonia.Input.KeyModifiers.Control));
        typeof(Editor.GlyphElement).GetProperty("IsLink")!.SetValue(glyph, false);
        var nonLinkPass = !Editor.PadView.TryPressLink(glyph, Press(true, Avalonia.Input.KeyModifiers.Control)) && calls == 1;

        // Initialize only windowless layout services; no app lifetime, window, or shell opener.
        AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();
        var editor = new AvaloniaEdit.TextEditor();
        var pad = (Editor.PadView)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(Editor.PadView));
        typeof(Editor.PadView).GetField("<Editor>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(pad, editor);
        var settings = new Settings { ShortenLinks = true };
        typeof(Editor.PadView).GetProperty("Settings")!.SetValue(pad, settings);
        typeof(Editor.PadView).GetProperty("Palette")!.SetValue(pad, Palette.All[0]);
        var view = editor.TextArea.TextView;
        view.ElementGenerators.Add(new Editor.LinkGenerator(pad));
        const string shortUrl = "https://google.com";
        const string longUrl = "https://example.com/long/path/over/thirtyfour/characters";
        editor.Text = $"prefix {shortUrl} suffix\n{longUrl}\nend";
        void Layout()
        {
            view.Redraw();
            view.Measure(new Size(800, 200));
            view.Arrange(new Rect(0, 0, 800, 200));
            view.EnsureVisualLines();
        }
        Point Inside(int lineNumber, int column) => view.GetVisualPosition(
            new AvaloniaEdit.TextViewPosition(lineNumber, column), AvaloniaEdit.Rendering.VisualYPosition.LineMiddle)
            - view.ScrollOffset + new Vector(1, 0);
        Layout();
        var shortPoint = Inside(1, 10);
        var shortHit = Editor.PadView.PlainUrlAt(view, shortPoint) == shortUrl;
        var outside = Editor.PadView.PlainUrlAt(view, Inside(1, 1)) is null
            && Editor.PadView.PlainUrlAt(view, new Point(799, shortPoint.Y)) is null
            && Editor.PadView.PlainUrlAt(view, new Point(1, 199)) is null
            && Editor.PadView.PlainUrlAt(view, new Point(-1, 1)) is null;
        editor.TextArea.Caret.Line = 2;
        Layout();
        var expandedHit = Editor.PadView.PlainUrlAt(view, Inside(2, 10)) == longUrl;
        editor.TextArea.Caret.Line = 3;
        settings.ShortenLinks = false;
        Layout();
        var disabledHit = Editor.PadView.PlainUrlAt(view, Inside(2, 10)) == longUrl;
        calls = 0;
        string? opened = null;
        void Open(string url) { calls++; opened = url; }
        var urlPress = Press(true, Avalonia.Input.KeyModifiers.Control);
        var urlTunnel = Editor.PadView.TryPressUrl(Editor.PadView.PlainUrlAt(view, shortPoint), urlPress, Open)
            && urlPress.Handled && calls == 1 && opened == shortUrl
            && !Editor.PadView.TryPressUrl(shortUrl, urlPress, Open) && calls == 1;
        var urlPlain = Press(true, Avalonia.Input.KeyModifiers.None);
        var urlRejected = !Editor.PadView.TryPressUrl(shortUrl, urlPlain, Open) && !urlPlain.Handled
            && !Editor.PadView.TryPressUrl(shortUrl, Press(false, Avalonia.Input.KeyModifiers.Control), Open)
            && !Editor.PadView.TryPressUrl(null, Press(true, Avalonia.Input.KeyModifiers.Control), Open) && calls == 1;
        var plainUrlsPass = shortHit && expandedHit && disabledHit && outside && urlTunnel && urlRejected;

        var errors = new List<string>();
        bool Calls(MethodBase method, Type owner, string name)
        {
            var found = false;
            ScanMethod(method, [], errors, (_, opcode, target) =>
            {
                if ((opcode == OpCodes.Call || opcode == OpCodes.Callvirt || opcode == OpCodes.Newobj)
                    && target.DeclaringType == owner && target.Name == name) found = true;
            });
            return found;
        }
        var generator = typeof(Editor.LinkGenerator).GetMethod("ConstructElement")!;
        var generatorPass = Calls(generator, typeof(Editor.GlyphElement), ".ctor")
            && Calls(generator, typeof(Editor.GlyphElement), "set_Hand")
            && Calls(generator, typeof(Editor.GlyphElement), "set_IsLink")
            && Calls(generator, typeof(Editor.GlyphElement), "set_Pressed");
        var query = typeof(Editor.GlyphElement).GetMethod("OnQueryCursor", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var cursorPass = Calls(query, typeof(Avalonia.Input.InputElement), "set_Cursor")
            && Calls(query, typeof(Avalonia.Interactivity.RoutedEventArgs), "set_Handled");
        var moved = typeof(Editor.PadView).GetMethod("OnViewMoved", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var pressed = typeof(Editor.PadView).GetMethod("OnViewPressed", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var routePass = Calls(moved, typeof(Editor.PadView), "GlyphAt")
            && Calls(pressed, typeof(Editor.PadView), "GlyphAt") && Calls(pressed, typeof(Editor.PadView), "TryPressLink")
            && Calls(moved, typeof(Editor.PadView), "PlainUrlAt") && Calls(pressed, typeof(Editor.PadView), "PlainUrlAt")
            && Calls(pressed, typeof(Editor.PadView), "TryPressUrl");
        report = $"glyph Ctrl+left={controlPass}; plain unhandled={plainPass}; right ignored={rightPass}; generator wiring={generatorPass}; cursor handled={cursorPass}; tunnel={tunnelPass}; once={oncePass}; tunnel plain/right/missing/nonlink={tunnelPlainPass && tunnelRightPass && missingPass && nonLinkPass}; hit-test wiring={routePass}";
        Console.WriteLine("SHORTENED LINK " + report);
        report += $"; plain URL short/expanded/off={shortHit}/{expandedHit}/{disabledHit}; outside={outside}; plain URL once/rejected={urlTunnel}/{urlRejected}";
        Console.WriteLine("ANY URL " + report);
        return controlPass && plainPass && rightPass && generatorPass && cursorPass && tunnelPass && oncePass
            && tunnelPlainPass && tunnelRightPass && missingPass && nonLinkPass && routePass && plainUrlsPass && errors.Count == 0;
    }

    private static bool SplitDividerFullHeightChecks(out string report)
    {
        try
        {
            var divider = PadWindow.CreateSplitDivider();
            var panes = new Grid { ColumnDefinitions = new ColumnDefinitions("*,0,0") };
            Grid.SetColumn(divider, 1);
            panes.Children.Add(divider);
            var content = new Grid();
            content.Children.Add(panes);

            var root = new Grid();
            root.Children.Add(new Border());
            root.Children.Add(content);
            var topLayer = new Panel();
            var bottomLayer = new Panel();
            var pill = new Border();
            PadWindow.AddChromeLayers(root, topLayer, bottomLayer, new Border(), pill, new Border());

            var contentIndex = root.Children.IndexOf(content);
            var splitDivider = ReferenceEquals(divider.Parent, panes) && ReferenceEquals(panes.Parent, content);
            var dockedPagerAbove = splitDivider && root.Children.IndexOf(pill) > contentIndex;
            root.Children.Remove(pill);
            bottomLayer.Children.Add(pill);
            var overlayPagerAbove = splitDivider && ReferenceEquals(pill.Parent, bottomLayer)
                && root.Children.IndexOf(bottomLayer) > contentIndex;

            report = $"divider bottom={divider.Margin.Bottom}; docked pager above divider={dockedPagerAbove}; overlay pager above divider={overlayPagerAbove}";
            return divider.Margin.Bottom == ChromeTokens.PagerStripHeight + ChromeTokens.DividerPagerClearance && dockedPagerAbove && overlayPagerAbove;
        }
        catch (Exception ex)
        {
            report = ex.GetType().Name + ": " + ex.Message;
            return false;
        }
    }

    private static bool SplitPaneClippingChecks(out string report)
    {
        var left = new Border { RenderTransform = new Avalonia.Media.TranslateTransform() };
        var right = new Border { RenderTransform = new Avalonia.Media.TranslateTransform() };
        var divider = PadWindow.CreateSplitDivider(); var grid = new Grid();
        var (leftHost, rightHost) = PadWindow.AddPaneHosts(grid, left, divider, right);
        PadWindow.ApplyPaneSplit(grid, leftHost, divider, rightHost, right, true);
        grid.Measure(new Size(640, 400)); grid.Arrange(new Rect(0, 0, 640, 400));
        var structure = grid.Children.Count == 3 && ReferenceEquals(grid.Children[0], leftHost) && ReferenceEquals(grid.Children[1], divider) && ReferenceEquals(grid.Children[2], rightHost)
            && ReferenceEquals(leftHost.Parent, grid) && ReferenceEquals(rightHost.Parent, grid) && ReferenceEquals(leftHost.Child, left) && ReferenceEquals(rightHost.Child, right)
            && ReferenceEquals(left.Parent, leftHost) && ReferenceEquals(right.Parent, rightHost)
            && Grid.GetColumn(leftHost) == 0 && Grid.GetColumn(divider) == 1 && Grid.GetColumn(rightHost) == 2 && leftHost.ClipToBounds && rightHost.ClipToBounds
            && leftHost.IsVisible && rightHost.IsVisible && right.IsVisible && divider.IsVisible;
        var leftBounds = leftHost.Bounds; var rightBounds = rightHost.Bounds;
        var separated = leftBounds.Width > 0 && rightBounds.Width > 0 && leftBounds.Right <= divider.Bounds.Left
            && rightBounds.Left >= divider.Bounds.Right && divider.Bounds.Width == ChromeTokens.DividerWidth;
        var leftMotion = (Avalonia.Media.TranslateTransform)left.RenderTransform!; var rightMotion = (Avalonia.Media.TranslateTransform)right.RenderTransform!;
        leftMotion.X = 64; rightMotion.X = -64;
        var towardDividerStable = leftHost.Bounds == leftBounds && rightHost.Bounds == rightBounds;
        leftMotion.X = -64; rightMotion.X = 64;
        var awayFromDividerStable = leftHost.Bounds == leftBounds && rightHost.Bounds == rightBounds;
        var stationary = towardDividerStable && awayFromDividerStable && leftHost.RenderTransform is null && rightHost.RenderTransform is null;
        PadWindow.ApplyPaneSplit(grid, leftHost, divider, rightHost, right, false);
        var single = !leftHost.ClipToBounds && !rightHost.IsVisible && !right.IsVisible && !divider.IsVisible;
        PadWindow.ApplyPaneSplit(grid, leftHost, divider, rightHost, right, true);
        var restored = leftHost.ClipToBounds && rightHost.IsVisible && right.IsVisible && divider.IsVisible;
        report = $"split host structure={structure}; bounds separated by divider={separated}; toward/away translations keep host bounds={stationary}; single view unclipped/hidden={single}; split restored={restored}";
        return structure && separated && stationary && single && restored;
    }

    private sealed record LayerRule(
        string From,
        string? To,
        bool AnyAntiNotepadNamespace = false,
        Type? ExactFrom = null,
        Type? ExactTo = null,
        bool AnyApplicationTypeExceptTarget = false,
        LayerTargetKind TargetKind = LayerTargetKind.Namespace,
        string? RawTypeName = null)
    {
        public string Label => TargetKind switch
        {
            LayerTargetKind.PInvokeHolder => $"{From}->pinvoke-holder",
            LayerTargetKind.RawType => $"{From}->{RawTypeName}",
            LayerTargetKind.WindowsNamespace => $"{From}->Windows.*",
            _ => AnyAntiNotepadNamespace ? $"{From}->*any other AntiNotepad namespace*" : $"{From}->{To}"
        };
    }

    private enum LayerTargetKind { Namespace, PInvokeHolder, RawType, WindowsNamespace }

    private sealed record SettingsSetterAllowance(Type CallerType, string[] Methods);

    private sealed record SettingsSetterViolation(MethodBase Caller, MethodBase Setter)
    {
        public string Display => $"{Caller.DeclaringType?.FullName}.{Caller.Name} -> {Setter.DeclaringType?.FullName}.{Setter.Name}";
    }

    private sealed record FakeHotkeyOutcome(bool IsRegistered, string? ErrorMessage);

    private sealed class FakeHotkeyRegistration(string label, FakeHotkeyOutcome outcome, Action onDispose) : IHotkeyRegistration
    {
        public string GestureLabel { get; } = label;
        public bool IsRegistered { get; } = outcome.IsRegistered;
        public string? RegistrationErrorMessage { get; } = outcome.ErrorMessage;
        public int DisposeCount { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
            onDispose();
        }
    }

    private sealed class FakeHotkeyFactory(string name, List<string> order)
    {
        private readonly Dictionary<string, Queue<FakeHotkeyOutcome>> _outcomes = new(StringComparer.OrdinalIgnoreCase);
        public List<FakeHotkeyRegistration> Created { get; } = [];

        public void Queue(string label, params FakeHotkeyOutcome[] outcomes)
        {
            if (!_outcomes.TryGetValue(label, out var queue)) _outcomes[label] = queue = new Queue<FakeHotkeyOutcome>();
            foreach (var outcome in outcomes) queue.Enqueue(outcome);
        }

        public IHotkeyRegistration Create(string label, Action<long> onPressed)
        {
            order.Add($"create:{name}:{label}");
            var outcome = _outcomes.TryGetValue(label, out var queue) && queue.Count > 0
                ? queue.Dequeue()
                : new FakeHotkeyOutcome(true, null);
            var registration = new FakeHotkeyRegistration(label, outcome, () => order.Add($"dispose:{name}:{label}"));
            Created.Add(registration);
            return registration;
        }
    }

    private static bool HotkeySwapChecks(out string report)
    {
        const string firstLabel = "Ctrl+Alt+Shift+A";
        const string secondLabel = "Ctrl+Alt+Shift+B";
        const string newNoteLabel = "Ctrl+Alt+Shift+N";
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        HotkeyRegistrar Make(FakeHotkeyFactory show, FakeHotkeyFactory newNote, Action<string> toast) =>
            new(show.Create, newNote.Create, _ => { }, () => { }, toast);

        try
        {
            var sameOrder = new List<string>();
            var sameShow = new FakeHotkeyFactory("show", sameOrder);
            var sameNewNote = new FakeHotkeyFactory("new-note", sameOrder);
            var sameRegistrar = Make(sameShow, sameNewNote, _ => { });
            var sameStatusChanges = 0;
            sameRegistrar.HotkeyStatusChanged += _ => sameStatusChanges++;
            var firstRegistered = sameRegistrar.RegisterHotkey(firstLabel);
            var createdBeforeSame = sameShow.Created.Count;
            var sameLabel = sameRegistrar.RegisterHotkey(firstLabel);
            CheckCase(firstRegistered && sameLabel && sameShow.Created.Count == createdBeforeSame
                && sameShow.Created[0].DisposeCount == 0,
                "same registered label does not create or dispose a registration");
            CheckCase(sameStatusChanges == 2, "show status event fires for each registration request");
            sameRegistrar.Dispose();

            var order = new List<string>();
            var orderShow = new FakeHotkeyFactory("show", order);
            var orderNewNote = new FakeHotkeyFactory("new-note", order);
            var orderRegistrar = Make(orderShow, orderNewNote, _ => { });
            var orderStarted = orderRegistrar.RegisterHotkey(firstLabel);
            order.Clear();
            var orderSwapped = orderRegistrar.RegisterHotkey(secondLabel);
            CheckCase(orderStarted && orderSwapped && order.SequenceEqual(
                [$"dispose:show:{firstLabel}", $"create:show:{secondLabel}"]),
                "swap releases old registration before creating the new one");
            orderRegistrar.Dispose();

            var restoreOrder = new List<string>();
            var restoreShow = new FakeHotkeyFactory("show", restoreOrder);
            var restoreNewNote = new FakeHotkeyFactory("new-note", restoreOrder);
            restoreShow.Queue(secondLabel, new FakeHotkeyOutcome(false, "registration conflict"));
            var restoreRegistrar = Make(restoreShow, restoreNewNote, _ => { });
            var restoreEvents = 0;
            restoreRegistrar.HotkeyStatusChanged += _ => restoreEvents++;
            var restoreStarted = restoreRegistrar.RegisterHotkey(firstLabel);
            var refused = restoreRegistrar.RegisterHotkey(secondLabel);
            CheckCase(restoreStarted && !refused && restoreShow.Created.Select(h => h.GestureLabel)
                    .SequenceEqual([firstLabel, secondLabel, firstLabel])
                && restoreShow.Created[0].DisposeCount == 1 && restoreShow.Created[1].DisposeCount == 1
                && restoreShow.Created[2].IsRegistered
                && restoreShow.Created[1].RegistrationErrorMessage == "registration conflict"
                && restoreRegistrar.HotkeyStatus == firstLabel && restoreEvents == 2,
                "refused replacement restores the old label and reports the restored status");
            restoreRegistrar.Dispose();

            var failedOrder = new List<string>();
            var failedShow = new FakeHotkeyFactory("show", failedOrder);
            var failedNewNote = new FakeHotkeyFactory("new-note", failedOrder);
            failedShow.Queue(firstLabel, new FakeHotkeyOutcome(true, null), new FakeHotkeyOutcome(false, "restore refused"));
            failedShow.Queue(secondLabel, new FakeHotkeyOutcome(false, "new key unavailable"));
            var failedRegistrar = Make(failedShow, failedNewNote, _ => { });
            var failedStarted = failedRegistrar.RegisterHotkey(firstLabel);
            var failedReplacement = failedRegistrar.RegisterHotkey(secondLabel);
            CheckCase(failedStarted && !failedReplacement && failedShow.Created.Count == 3
                && failedShow.Created.All(h => h.DisposeCount == 1)
                && failedRegistrar.HotkeyStatus == $"{secondLabel} unavailable: new key unavailable",
                "failed replacement and restore leave the slot empty with the refusal status");
            failedRegistrar.Dispose();

            var blankOrder = new List<string>();
            var blankShow = new FakeHotkeyFactory("show", blankOrder);
            var blankNewNote = new FakeHotkeyFactory("new-note", blankOrder);
            var blankRegistrar = Make(blankShow, blankNewNote, _ => { });
            var newNoteEnabled = blankRegistrar.RegisterNewNoteHotkey(newNoteLabel);
            var newNoteDisabled = blankRegistrar.RegisterNewNoteHotkey(" ");
            CheckCase(newNoteEnabled && newNoteDisabled && blankNewNote.Created.Count == 1
                && blankNewNote.Created[0].DisposeCount == 1 && blankRegistrar.NewNoteHotkeyStatus == "off",
                "blank new-note gesture disposes and disables its slot");
            blankRegistrar.Dispose();

            var disposeOrder = new List<string>();
            var disposeShow = new FakeHotkeyFactory("show", disposeOrder);
            var disposeNewNote = new FakeHotkeyFactory("new-note", disposeOrder);
            var disposeRegistrar = Make(disposeShow, disposeNewNote, _ => { });
            disposeRegistrar.RegisterHotkey(firstLabel);
            disposeRegistrar.RegisterNewNoteHotkey(newNoteLabel);
            disposeRegistrar.Dispose();
            disposeRegistrar.Dispose();
            CheckCase(disposeShow.Created.Single().DisposeCount == 1 && disposeNewNote.Created.Single().DisposeCount == 1,
                "dispose releases each active slot exactly once, including repeated disposal");

            var parseOrder = new List<string>();
            var parseShow = new FakeHotkeyFactory("show", parseOrder);
            var parseNewNote = new FakeHotkeyFactory("new-note", parseOrder);
            var toasts = new List<string>();
            var parseRegistrar = Make(parseShow, parseNewNote, toasts.Add);
            var parseEvents = 0;
            parseRegistrar.HotkeyStatusChanged += _ => parseEvents++;
            parseRegistrar.RegisterHotkey(firstLabel);
            var parseDisposedBefore = parseShow.Created[0].DisposeCount;
            var invalid = parseRegistrar.RegisterHotkey("not-a-hotkey");
            CheckCase(!invalid && toasts.Count == 1 && toasts[0].Contains("Hotkey must", StringComparison.Ordinal)
                && parseShow.Created.Count == 1 && parseShow.Created[0].DisposeCount == parseDisposedBefore
                && parseRegistrar.HotkeyStatus == firstLabel && parseEvents == 1,
                "unparsable gesture toasts and leaves the current slot untouched");
            parseRegistrar.Dispose();

            report = failures.Count == 0
                ? "same-label no-op; release-before-create; refusal rollback and failed-restore status; blank new-note disable; idempotent disposal; parse toast; status events"
                : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.ToString()));
            return false;
        }
    }

    private static bool ClipboardPolicyChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        var blank = new Editor.ClipboardCapturePolicy();
        CheckCase(blank.CheckText(null) == Editor.ClipboardCapturePolicy.TextCheck.Blank
            && blank.CheckText("") == Editor.ClipboardCapturePolicy.TextCheck.Blank
            && blank.CheckText(" \t\r\n") == Editor.ClipboardCapturePolicy.TextCheck.Blank, "blank inputs");

        var ownCopy = new Editor.ClipboardCapturePolicy();
        ownCopy.RecordOwnCopy("x");
        CheckCase(ownCopy.CheckText("x") == Editor.ClipboardCapturePolicy.TextCheck.OwnCopy
            && ownCopy.CheckText(" x ") == Editor.ClipboardCapturePolicy.TextCheck.Eligible
            && ownCopy.CheckText("X") == Editor.ClipboardCapturePolicy.TextCheck.Eligible, "raw own-copy match");

        var lastCaptured = new Editor.ClipboardCapturePolicy();
        var acceptedRepeat = lastCaptured.Decide("x", "clipboard");
        CheckCase(acceptedRepeat
            && lastCaptured.CheckText("x") == Editor.ClipboardCapturePolicy.TextCheck.LastCaptured
            && lastCaptured.CheckText(" x ") == Editor.ClipboardCapturePolicy.TextCheck.Eligible
            && lastCaptured.Decide(" y ", "clipboard")
            && lastCaptured.CheckText(" y ") == Editor.ClipboardCapturePolicy.TextCheck.LastCaptured
            && lastCaptured.CheckText("y") == Editor.ClipboardCapturePolicy.TextCheck.Eligible, "raw last-captured match");

        var contained = new Editor.ClipboardCapturePolicy();
        CheckCase(!contained.Decide(" cat ", "clipboard\nconcatenate")
            && contained.Decide("Cat", "clipboard\nconcatenate"), "substring and case-sensitive containment");

        var rejectedKeepsLast = new Editor.ClipboardCapturePolicy();
        var acceptedOld = rejectedKeepsLast.Decide("old", "clipboard");
        var rejectedCat = rejectedKeepsLast.Decide("cat", "cat");
        CheckCase(acceptedOld && !rejectedCat
            && rejectedKeepsLast.CheckText("old") == Editor.ClipboardCapturePolicy.TextCheck.LastCaptured,
            "rejection preserves last capture");

        var dedupeBeforeFormatting = new Editor.ClipboardCapturePolicy();
        CheckCase(dedupeBeforeFormatting.Decide("a\r\nb", "a\nb")
            && !new Editor.ClipboardCapturePolicy().Decide(" a ", "a"), "trimmed raw containment");

        CheckCase(Editor.ClipboardCapturePolicy.FormatAppend(" \r\nalpha\r\n  beta\r\n ", false) == "\n- alpha\n    beta",
            "CRLF and indentation formatting");
        CheckCase(Editor.ClipboardCapturePolicy.FormatAppend("a\nb", true) == "- a\n  b"
            && Editor.ClipboardCapturePolicy.FormatAppend("a\rb", true) == "- a\rb", "LF and bare-CR formatting");
        CheckCase(Editor.ClipboardCapturePolicy.FormatAppend(" x ", true) == "- x"
            && Editor.ClipboardCapturePolicy.FormatAppend(" x ", false) == "\n- x", "newline join formatting");

        CheckCase(new Editor.ClipboardCapturePolicy().Decide("x", null), "null existing text");

        var precedence = new Editor.ClipboardCapturePolicy();
        precedence.RecordOwnCopy(" ");
        var blankPrecedesOwn = precedence.CheckText(" ") == Editor.ClipboardCapturePolicy.TextCheck.Blank;
        var ownPrecedesLast = new Editor.ClipboardCapturePolicy();
        ownPrecedesLast.Decide("x", "clipboard");
        ownPrecedesLast.RecordOwnCopy("x");
        CheckCase(blankPrecedesOwn
            && ownPrecedesLast.CheckText("x") == Editor.ClipboardCapturePolicy.TextCheck.OwnCopy, "filter precedence");

        var beforeInsert = new Editor.ClipboardCapturePolicy();
        CheckCase(beforeInsert.Decide("x", "clipboard")
            && beforeInsert.CheckText("x") == Editor.ClipboardCapturePolicy.TextCheck.LastCaptured, "record before insert effect");

        report = failures.Count == 0 ? "12 cases" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static bool RuntimeLogChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-runtime-log-{Guid.NewGuid():N}");
        try
        {
            var emptyHome = new RuntimeLog();
            emptyHome.Write("ignored before home");
            emptyHome.Flush();
            var loggerField = typeof(RuntimeLog).GetField("_logger", BindingFlags.Instance | BindingFlags.NonPublic);
            CheckCase(loggerField?.GetValue(emptyHome) is null && !Directory.Exists(root), "empty home Write creates no file or logger");
            emptyHome.Dispose();

            var firstHome = Path.Combine(root, "first");
            var otherHome = Path.Combine(root, "other");
            var firstPath = Path.Combine(firstHome, AppInfo.LogFileName);
            var otherPath = Path.Combine(otherHome, AppInfo.LogFileName);
            using (var logger = new RuntimeLog())
            {
                logger.SetHome(firstHome);
                logger.Write("first home");
                logger.Flush();
                var firstWriteLanded = File.Exists(firstPath) && File.ReadAllLines(firstPath).Last().EndsWith("first home", StringComparison.Ordinal);
                logger.SetHome(otherHome);
                logger.Write("still first home");
                logger.Flush();
                var staysAtFirstHome = File.ReadAllLines(firstPath).Last().EndsWith("still first home", StringComparison.Ordinal)
                    && !File.Exists(otherPath);
                CheckCase(firstWriteLanded && staysAtFirstHome, "lazy write uses first home and never retargets");
            }

            var crashPath = Path.Combine(root, "crash", AppInfo.LogFileName);
            var crashLog = new RuntimeLog();
            crashLog.SetHome(Path.GetDirectoryName(crashPath)!);
            crashLog.LogCrash(new InvalidOperationException("boom"));
            CheckCase(File.Exists(crashPath) && File.ReadAllLines(crashPath).Any(line => line.Contains("crash ", StringComparison.Ordinal)
                && line.EndsWith("boom", StringComparison.Ordinal)), "LogCrash flushes an early crash line");
            crashLog.Dispose();

            var disposePath = Path.Combine(root, "dispose", AppInfo.LogFileName);
            var disposeLog = new RuntimeLog();
            disposeLog.SetHome(Path.GetDirectoryName(disposePath)!);
            disposeLog.Write("drained on dispose");
            disposeLog.Dispose();
            var writeAfterDispose = true;
            try { disposeLog.Write("after dispose"); }
            catch { writeAfterDispose = false; }
            CheckCase(File.Exists(disposePath) && File.ReadAllLines(disposePath).Any(line => line.EndsWith("drained on dispose", StringComparison.Ordinal))
                && writeAfterDispose, "Dispose drains queued writes and later Write does not throw");

            var flushWithoutLogger = true;
            try
            {
                var neverCreated = new RuntimeLog();
                neverCreated.Flush();
                neverCreated.Dispose();
            }
            catch { flushWithoutLogger = false; }
            CheckCase(flushWithoutLogger, "Flush without a logger is safe");

            report = failures.Count == 0 ? "5 checks" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.ToString()));
            return false;
        }
        finally { try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { } }
    }

    private static bool TaskbarTraceChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        var suppressedLines = new List<string>();
        var suppressed = new TaskbarTrace(false, suppressedLines.Add);
        suppressed.Write("x");
        suppressed.TracePadAction("ignored", "show-begin", true);
        CheckCase(suppressedLines.Count == 0, "non-dev trace stays silent");

        var lines = new List<string>();
        var trace = new TaskbarTrace(true, lines.Add);
        CheckCase(trace.PadActionContext() == "cause=unknown age=na", "unknown cause before first pad action");
        trace.Write("x");
        CheckCase(lines.Count == 1 && IsTaskbarTraceLine(lines[0], "x"), "Write prefixes elapsed taskbar time");
        trace.TracePadAction("test-click", "show-begin", true);
        var context = trace.PadActionContext();
        CheckCase(context.StartsWith("cause=test-click age=", StringComparison.Ordinal) && context.EndsWith("ms", StringComparison.Ordinal),
            "pad action cause and age persist");
        CheckCase(lines.Count == 2 && IsTaskbarTraceLine(lines[1], "pad action=show-begin cause=test-click visible=True"),
            "TracePadAction writes the action line");

        report = failures.Count == 0 ? "5 checks" : string.Join("; ", failures);
        return failures.Count == 0;

        static bool IsTaskbarTraceLine(string line, string message)
        {
            const string prefix = "taskbar: t=";
            if (!line.StartsWith(prefix, StringComparison.Ordinal)) return false;
            var marker = line.IndexOf("ms ", prefix.Length, StringComparison.Ordinal);
            return marker > prefix.Length
                && double.TryParse(line[prefix.Length..marker], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.CurrentCulture, out _)
                && line[(marker + 3)..] == message;
        }
    }

    private static bool MathNotesChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        CheckCase(Editor.MathNotes.BlankLabel("spent 12.50 + 8.25 =") == "      12.50 + 8.25 =",
            "blank prose label through the first number");
        CheckCase(Editor.MathNotes.StripListMarker("- 19.99 * 3 =") == "  19.99 * 3 =",
            "blank bullet marker without shifting math");
        CheckCase(Editor.MathNotes.StripListMarker("2. 19 + 3 =") == "2. 19 + 3 =",
            "keep numbered marker for math evaluation");
        CheckCase(Editor.MathNotes.StripListMarker("  - [x] 3 + 4 =") == "        3 + 4 =",
            "blank checkbox bullet marker without shifting math");

        var evaluated = Editor.MathNotes.EvaluateMath(["math", "lunch 12 + 3 =", "x = 4", "x * 2 ="]);
        CheckCase(evaluated[1]?.Result?.Value == 15, "label line evaluates after retry");
        CheckCase(evaluated[2]?.AssignedName == "x" && evaluated[2]?.Result?.Value == 4
            && evaluated[3]?.Result?.Value == 8, "variables survive label retry");
        CheckCase(evaluated[1]?.Tokens is { Count: > 0 } tokens
            && tokens[0] == new Calc.CalcToken(0, "lunch ".Length, Calc.TokenKind.Comment),
            "label token keeps its comment span");
        CheckCase(Editor.MathNotes.EvaluateMath(["math", "- 2 + 3 ="])[1]?.Result?.Value == 5,
            "bullet math evaluates as positive");

        report = failures.Count == 0 ? "8 checks" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static bool ListEditingChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        CheckCase(Editor.ListEditing.TryListNumber("123456789. item", out var nineDigits) && nineDigits == 123_456_789,
            "9-digit list marker accepted");
        CheckCase(!Editor.ListEditing.TryListNumber("1234567890. item", out _),
            "10-digit list marker rejected");

        report = failures.Count == 0 ? "2 checks" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static readonly LayerRule[] LayerRules = CreateLayerRules();

    private static LayerRule[] CreateLayerRules() =>
    [
        new("Editor", "UI"),
        new("Model", "UI"),
        new("Model", "Editor"),
        new("Calc", null, AnyAntiNotepadNamespace: true),
        new("Theme", "UI"),
        new("Theme", "Editor"),
        new("Platform", "UI"),
        new("Platform", "Editor"),
        new("HotkeyRegistrar", "PadWindow", ExactFrom: typeof(HotkeyRegistrar), ExactTo: typeof(PadWindow)),
        new("HotkeyRegistrar", "PadView", ExactFrom: typeof(HotkeyRegistrar), ExactTo: typeof(Editor.PadView)),
        new("HotkeyRegistrar", "Program", ExactFrom: typeof(HotkeyRegistrar), ExactTo: typeof(Program)),
        new("ClipboardCapturePolicy", "UI", ExactFrom: typeof(Editor.ClipboardCapturePolicy)),
        new("ClipboardCapturePolicy", "Platform", ExactFrom: typeof(Editor.ClipboardCapturePolicy)),
        new("ClipboardCapturePolicy", "Motion", ExactFrom: typeof(Editor.ClipboardCapturePolicy)),
        new("MathNotes", "UI", ExactFrom: typeof(Editor.MathNotes)),
        new("MathNotes", "Platform", ExactFrom: typeof(Editor.MathNotes)),
        new("MathNotes", "Motion", ExactFrom: typeof(Editor.MathNotes)),
        new("MathNotes", "Theme", ExactFrom: typeof(Editor.MathNotes)),
        new("MathNotes", "PadWindow", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(PadWindow)),
        new("MathNotes", "PadView", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Editor.PadView)),
        new("MathNotes", "Program", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Program)),
        new("MathNotes", "DesktopPlatform", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(DesktopPlatform)),
        new("MathNotes", "Window", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Avalonia.Controls.Window)),
        new("MathNotes", "Control", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Avalonia.Controls.Control)),
        new("MathNotes", "Dispatcher", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
        new("MathNotes", "TextDocument", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(AvaloniaEdit.Document.TextDocument)),
        new("MathNotes", "Timers", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Timers)),
        new("MathNotes", "Settings", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Settings)),
        new("MathNotes", "SettingsService", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(SettingsService)),
        new("ListEditing", "UI", ExactFrom: typeof(Editor.ListEditing)),
        new("ListEditing", "Platform", ExactFrom: typeof(Editor.ListEditing)),
        new("ListEditing", "Motion", ExactFrom: typeof(Editor.ListEditing)),
        new("ListEditing", "Theme", ExactFrom: typeof(Editor.ListEditing)),
        new("ListEditing", "PadWindow", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(PadWindow)),
        new("ListEditing", "PadView", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Editor.PadView)),
        new("ListEditing", "Program", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Program)),
        new("ListEditing", "DesktopPlatform", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(DesktopPlatform)),
        new("ListEditing", "Window", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Avalonia.Controls.Window)),
        new("ListEditing", "Control", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Avalonia.Controls.Control)),
        new("ListEditing", "Dispatcher", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
        new("ListEditing", "TextDocument", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(AvaloniaEdit.Document.TextDocument)),
        new("ListEditing", "Timers", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Timers)),
        new("ListEditing", "Settings", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Settings)),
        new("ListEditing", "SettingsService", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(SettingsService)),
        new("ListEditing", "CheckboxGenerator", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Editor.CheckboxGenerator)),
        new("NoteAnalysis", "PadView", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Editor.PadView)),
        new("NoteAnalysis", "UI", ExactFrom: typeof(Editor.NoteAnalysis)),
        new("NoteAnalysis", "Platform", ExactFrom: typeof(Editor.NoteAnalysis)),
        new("NoteAnalysis", "Motion", ExactFrom: typeof(Editor.NoteAnalysis)),
        new("NoteAnalysis", "Theme", ExactFrom: typeof(Editor.NoteAnalysis)),
        new("NoteAnalysis", "PadWindow", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(PadWindow)),
        new("NoteAnalysis", "Program", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Program)),
        new("NoteAnalysis", "DesktopPlatform", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(DesktopPlatform)),
        new("NoteAnalysis", "Window", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Controls.Window)),
        new("NoteAnalysis", "Control", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Controls.Control)),
        new("NoteAnalysis", "Dispatcher", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
        new("NoteAnalysis", "Settings", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Settings)),
        new("NoteAnalysis", "SettingsService", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(SettingsService)),
        new("NoteAnalysis", "TextEditor", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.TextEditor)),
        new("NoteAnalysis", "TextArea", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.Editing.TextArea)),
        new("NoteAnalysis", "TextView", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.Rendering.TextView)),
        new("ClipboardCapturePolicy", "PadWindow", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(PadWindow)),
        new("ClipboardCapturePolicy", "PadView", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(Editor.PadView)),
        new("ClipboardCapturePolicy", "Program", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(Program)),
        new("ClipboardCapturePolicy", "DesktopPlatform", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(DesktopPlatform)),
        new("ClipboardCapturePolicy", "Window", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(Avalonia.Controls.Window)),
        new("ClipboardCapturePolicy", "Control", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(Avalonia.Controls.Control)),
        new("ClipboardCapturePolicy", "Dispatcher", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
        new("ClipboardCapturePolicy", "TextDocument", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(AvaloniaEdit.Document.TextDocument)),
        new("Platform", "Program", ExactTo: typeof(Program)),
        new("Model", "Program", ExactTo: typeof(Program)),
        new("RuntimeLog", "UI", ExactFrom: typeof(RuntimeLog)),
        new("RuntimeLog", "Editor", ExactFrom: typeof(RuntimeLog)),
        new("RuntimeLog", "Platform", ExactFrom: typeof(RuntimeLog)),
        new("RuntimeLog", "Program", ExactFrom: typeof(RuntimeLog), ExactTo: typeof(Program)),
        new("RuntimeLog", "Avalonia", ExactFrom: typeof(RuntimeLog), ExactTo: typeof(Avalonia.Application)),
        new("SettingsWindow", "PadWindow", ExactFrom: typeof(SettingsWindow), ExactTo: typeof(PadWindow)),
        new("SettingsService", "UI", ExactFrom: typeof(SettingsService)),
        new("SettingsService", "Editor", ExactFrom: typeof(SettingsService)),
        new("Editor", "Program", ExactTo: typeof(Program)),
        new("UI", "Program", ExactTo: typeof(Program)),
        new("App", "Program", ExactFrom: typeof(App), ExactTo: typeof(Program)),
        new("Model", "NotesSession", ExactTo: typeof(NotesSession)),
        new("Editor", "NotesSession", ExactTo: typeof(NotesSession)),
        new("INotesLocation", "UI", ExactFrom: typeof(INotesLocation)),
        new("INotesLocation", "Editor", ExactFrom: typeof(INotesLocation)),
        new("INotesLocation", "Platform", ExactFrom: typeof(INotesLocation)),
        new("*except Program", "Program", ExactTo: typeof(Program), AnyApplicationTypeExceptTarget: true),
        new("UI", null, TargetKind: LayerTargetKind.PInvokeHolder),
        new("Editor", null, TargetKind: LayerTargetKind.PInvokeHolder),
        new("UI", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "Marshal"),
        new("Editor", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "Marshal"),
        new("UI", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "IntPtr"),
        new("Editor", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "IntPtr"),
        new("UI", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "UIntPtr"),
        new("Editor", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "UIntPtr"),
        new("UI", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "Win32Properties"),
        new("Editor", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "Win32Properties"),
        new("UI", null, TargetKind: LayerTargetKind.WindowsNamespace),
        new("Editor", null, TargetKind: LayerTargetKind.WindowsNamespace),
    ];

    // Setter calls from these exact methods are passed to SettingsService.Update, except ResetSettings' defaults initializer.
    // Settings itself owns its detached-copy initializer; StoreData's constructor is the JSON/default construction path.
    private static readonly SettingsSetterAllowance[] SettingsSetterAllowances =
    [
        new(typeof(Settings), ["CopyForSave"]),
        new(typeof(StoreData), [".ctor"]),
        new(typeof(Store), ["Normalize"]),
        new(typeof(Welcome), ["SeedIfNew"]),
        new(typeof(PadWindow), ["Zoom", "RememberBounds", "BindPanes", "ToggleSplit", "CycleSplitSlot", "TogglePin", "ResetSettings"]),
        new(typeof(SettingsWindow), ["ThemePage", "AccentDots", "UiPage", "EditorPage", "FeelPage", "TuningRows", "SystemPage"]),
        new(typeof(SearchOverlay), ["ToggleGrid"]),
    ];

    private static bool DesktopPlatformSurfaceChecks(out string report)
    {
        var failures = new List<string>();
        try
        {
            var platform = typeof(DesktopPlatform);
            if (!platform.IsNotPublic || !platform.IsAbstract || !platform.IsSealed)
                failures.Add("DesktopPlatform is not an internal static class");

            (string Name, Type ReturnType, Type[] ParameterTypes)[] expected =
            [
                ("CursorPosition", typeof(Avalonia.PixelPoint), Type.EmptyTypes),
                ("ListenClipboard", typeof(IDisposable), [typeof(Avalonia.Controls.Window), typeof(Action)]),
                ("ClipboardIsPrivate", typeof(bool), Type.EmptyTypes),
                ("Chime", typeof(void), Type.EmptyTypes),
                ("TrimWorkingSet", typeof(void), Type.EmptyTypes),
                ("ConfigurePopupWindow", typeof(void), [typeof(Avalonia.Controls.TopLevel)]),
                ("Flash", typeof(void), [typeof(Avalonia.Controls.Window)]),
                ("OpenUrl", typeof(void), [typeof(string)]),
                ("OpenFolder", typeof(void), [typeof(string)]),
                ("StartScreenSnip", typeof(void), Type.EmptyTypes),
            ];
            const BindingFlags declaredStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var member in expected)
            {
                var method = platform.GetMethod(member.Name, declaredStatic, binder: null, types: member.ParameterTypes, modifiers: null);
                if (method is null || !method.IsStatic || method.ReturnType != member.ReturnType)
                    failures.Add("missing or mismatched " + member.Name);
            }

            var native = platform.GetNestedType("Native", BindingFlags.NonPublic);
            if (native is null) failures.Add("nested Native is missing");
            else if (!native.IsNestedPrivate) failures.Add("nested Native is not private");
        }
        catch (Exception ex) { failures.Add(ex.GetType().Name); }

        report = failures.Count == 0 ? "passed" : string.Join(", ", failures);
        return failures.Count == 0;
    }

    private static bool PInvokeHolderChecks(out string report)
    {
        var failures = new List<string>();
        try
        {
            CheckNestedPrivate(typeof(GlobalHotkey), "GlobalHotkey.Win32");
            CheckNestedPrivate(typeof(TaskbarProxy), "TaskbarProxy.Win32");
            CheckNestedPrivate(typeof(DesktopPlatform), "DesktopPlatform.Native");
        }
        catch (Exception ex) { failures.Add(ex.GetType().Name); }

        report = failures.Count == 0 ? "passed" : string.Join(", ", failures);
        return failures.Count == 0;

        void CheckNestedPrivate(Type parent, string label)
        {
            var name = label[(label.LastIndexOf('.') + 1)..];
            var nested = parent.GetNestedType(name, BindingFlags.NonPublic);
            if (nested is null) failures.Add(label + " missing");
            else if (!nested.IsNestedPrivate) failures.Add(label + " is not private");
            else if (name != "Native" && !IsPInvokeHolder(nested)) failures.Add(label + " has no P/Invoke methods");
        }
    }

    private static (List<string> Lines, List<string> Failures) CheckLayers()
    {
        var lines = new List<string>();
        var failures = new List<string>();
        var scanErrors = new List<string>();
        Type[] types;
        try { types = typeof(SelfTest).Assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.OfType<Type>().ToArray();
            scanErrors.Add($"assembly type load was partial ({ex.LoaderExceptions?.Length ?? 0} loader errors)");
        }

        var references = new Dictionary<Type, HashSet<Type>>();
        var rawReferences = new Dictionary<Type, HashSet<Type>>();
        foreach (var type in types.Where(t => LayerRules.Any(rule => MatchesSource(rule, t))))
        {
            var found = new HashSet<Type>();
            var rawFound = new HashSet<Type>();
            references[type] = found;
            rawReferences[type] = rawFound;
            try
            {
                AddType(type, found);
                AddType(type.BaseType, found);
                AddType(type.BaseType, found, rawFound);
                foreach (var iface in type.GetInterfaces())
                {
                    AddType(iface, found);
                    AddType(iface, found, rawFound);
                }

                const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    | BindingFlags.Static | BindingFlags.DeclaredOnly;
                foreach (var field in type.GetFields(declared))
                {
                    AddType(field.FieldType, found);
                    AddType(field.FieldType, rawFound);
                }
                foreach (var property in type.GetProperties(declared))
                {
                    AddType(property.PropertyType, found);
                    AddType(property.PropertyType, rawFound);
                    foreach (var parameter in property.GetIndexParameters())
                    {
                        AddType(parameter.ParameterType, found);
                        AddType(parameter.ParameterType, rawFound);
                    }
                }
                foreach (var ev in type.GetEvents(declared))
                {
                    AddType(ev.EventHandlerType, found);
                    AddType(ev.EventHandlerType, rawFound);
                }
                foreach (var constructor in type.GetConstructors(declared)) ScanMethod(constructor, found, scanErrors, directReferences: rawFound);
                if (type.TypeInitializer is { } initializer) ScanMethod(initializer, found, scanErrors, directReferences: rawFound);
                foreach (var method in type.GetMethods(declared))
                {
                    var allowedProgramTypeToken = type == typeof(SelfTest) && method.Name == nameof(CreateLayerRules)
                        ? LayerRules.Single(rule => rule.AnyApplicationTypeExceptTarget).ExactTo
                        : null;
                    ScanMethod(method, found, scanErrors, allowedProgramTypeToken: allowedProgramTypeToken, directReferences: rawFound);
                }
            }
            catch (Exception ex) { scanErrors.Add($"{type.FullName}: reflection {ex.GetType().Name}"); }
        }

        foreach (var rule in LayerRules)
        {
            var sourceReferences = rule.TargetKind == LayerTargetKind.RawType ? rawReferences : references;
            var offenders = sourceReferences.SelectMany(pair => pair.Value
                    .Where(target => MatchesRule(rule, pair.Key, target))
                    .Select(target => (Source: pair.Key, Target: target)))
                .Distinct()
                .OrderBy(pair => pair.Source.FullName, StringComparer.Ordinal)
                .ThenBy(pair => pair.Target.FullName, StringComparer.Ordinal)
                .ToArray();
            lines.Add($"LAYERS {rule.Label} {offenders.Length} refs");
            foreach (var offender in offenders)
                failures.Add($"layer violation {rule.Label}: {offender.Source.FullName} -> {offender.Target.FullName}");
        }

        var outsidePlatform = new List<MethodInfo>();
        foreach (var type in types)
        {
            try
            {
                const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    | BindingFlags.Static | BindingFlags.DeclaredOnly;
                foreach (var method in type.GetMethods(declared).Where(IsPInvokeMethod))
                    if (!InNamespace(TopLevelDeclaringType(method.DeclaringType!).Namespace, "Platform"))
                        outsidePlatform.Add(method);
            }
            catch (Exception ex) { scanErrors.Add($"{type.FullName}: P/Invoke reflection {ex.GetType().Name}"); }
        }
        lines.Add($"LAYERS pinvoke-outside-Platform {outsidePlatform.Count}");
        foreach (var method in outsidePlatform.OrderBy(method => method.DeclaringType?.FullName, StringComparer.Ordinal)
            .ThenBy(method => method.Name, StringComparer.Ordinal))
            failures.Add($"P/Invoke outside Platform: {method.DeclaringType?.FullName}.{method.Name}");

        var setterCheck = CheckSettingsSetters(types);
        lines.Add($"LAYERS Settings.set_* outside owner {setterCheck.Violations.Count}");
        foreach (var violation in setterCheck.Violations)
            failures.Add("settings setter violation: " + violation.Display);

        foreach (var error in scanErrors.Distinct(StringComparer.Ordinal)) failures.Add("layer scan: " + error);
        foreach (var error in setterCheck.ScanErrors) failures.Add("settings setter scan: " + error);
        foreach (var layer in new[] { "UI", "Editor" })
        {
            var processStartCheck = CheckProcessStartCalls(types, layer);
            lines.Add($"LAYERS {layer}->Process.Start {processStartCheck.Violations.Count}");
            foreach (var violation in processStartCheck.Violations)
                failures.Add($"Process.Start layer violation: {violation.Caller.DeclaringType?.FullName}.{violation.Caller.Name} -> {violation.Target.DeclaringType?.FullName}.{violation.Target.Name}");
            foreach (var error in processStartCheck.ScanErrors)
                failures.Add("Process.Start scan: " + error);
        }
        return (lines, failures);
    }

    private static (List<(MethodBase Caller, MethodBase Target)> Violations, List<string> ScanErrors)
        CheckProcessStartCalls(Type[] types, string layer)
    {
        var violations = new List<(MethodBase Caller, MethodBase Target)>();
        var scanErrors = new List<string>();
        const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var type in types.Where(type => InNamespace(type.Namespace, layer)))
        {
            try
            {
                foreach (var constructor in type.GetConstructors(declared))
                    ScanMethod(constructor, [], scanErrors, InspectCall);
                if (type.TypeInitializer is { } initializer)
                    ScanMethod(initializer, [], scanErrors, InspectCall);
                foreach (var method in type.GetMethods(declared))
                    ScanMethod(method, [], scanErrors, InspectCall);
            }
            catch (Exception ex) { scanErrors.Add($"{type.FullName}: reflection {ex.GetType().Name}"); }
        }

        return (violations.Distinct().ToList(), scanErrors.Distinct(StringComparer.Ordinal).ToList());

        void InspectCall(MethodBase caller, OpCode opcode, MethodBase target)
        {
            if ((opcode == OpCodes.Call || opcode == OpCodes.Callvirt)
                && target.DeclaringType == typeof(Process) && target.Name == "Start")
                violations.Add((caller, target));
        }
    }

    private static (List<SettingsSetterViolation> Violations, List<string> ScanErrors) CheckSettingsSetters(Type[] types)
    {
        var violations = new List<SettingsSetterViolation>();
        var scanErrors = new List<string>();
        var scannedTypes = new HashSet<Type>();
        foreach (var type in types)
        {
            const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var constructor in type.GetConstructors(declared))
                ScanMethod(constructor, scannedTypes, scanErrors, InspectCall);
            if (type.TypeInitializer is { } initializer) ScanMethod(initializer, scannedTypes, scanErrors, InspectCall);
            foreach (var method in type.GetMethods(declared)) ScanMethod(method, scannedTypes, scanErrors, InspectCall);
        }

        return (violations.Distinct().OrderBy(item => item.Display, StringComparer.Ordinal).ToList(), scanErrors.Distinct(StringComparer.Ordinal).ToList());

        void InspectCall(MethodBase caller, OpCode opcode, MethodBase target)
        {
            if (opcode != OpCodes.Call && opcode != OpCodes.Callvirt) return;
            if (target.DeclaringType != typeof(Settings) || !target.Name.StartsWith("set_", StringComparison.Ordinal)) return;
            if (IsAllowedSettingsSetterCaller(caller)) return;
            violations.Add(new SettingsSetterViolation(caller, target));
        }
    }

    private static bool IsAllowedSettingsSetterCaller(MethodBase caller)
    {
        var type = caller.DeclaringType;
        while (type?.DeclaringType is { } parent) type = parent;
        if (type == typeof(SettingsService) || type == typeof(SelfTest)) return true;
        var method = caller.Name;
        if (method.StartsWith('<') && method.IndexOf('>') is var end && end > 1) method = method[1..end];
        return SettingsSetterAllowances.Any(allowance => allowance.CallerType == type && allowance.Methods.Contains(method, StringComparer.Ordinal));
    }

    private static bool MatchesSource(LayerRule rule, Type source)
    {
        if (rule.AnyApplicationTypeExceptTarget)
            return rule.ExactTo is { } target && source.Assembly == target.Assembly && !IsExactOrNested(source, target);
        return rule.ExactFrom is { } exact
            ? IsExactOrNested(source, exact)
            : InNamespace(source.Namespace, rule.From);
    }

    private static Type ProgramRuleTarget => LayerRules.Single(rule => rule.AnyApplicationTypeExceptTarget).ExactTo!;

    private static bool StaticServiceExposureChecks(out string report)
    {
        var programType = ProgramRuleTarget;
        Type[] types;
        try { types = programType.Assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { types = ex.Types.OfType<Type>().ToArray(); }

        var serviceTypes = new HashSet<Type> { typeof(Store), typeof(SettingsService), typeof(RuntimeLog), typeof(NotesSession) };
        var exposures = new HashSet<string>(StringComparer.Ordinal);
        const BindingFlags declaredStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

        // This typed reflection guard covers static fields and properties, including compiler backing fields.
        // Delegates, dictionaries and object-typed locators are outside its bound.
        foreach (var type in types)
            CheckType(type);

        report = exposures.Count == 0 ? "0 refs" : string.Join("; ", exposures.OrderBy(value => value, StringComparer.Ordinal));
        return exposures.Count == 0;

        void CheckType(Type type)
        {
            foreach (var field in type.GetFields(declaredStatic))
                if (serviceTypes.Contains(field.FieldType)) exposures.Add($"{type.FullName}.{field.Name}: {field.FieldType.Name}");
            foreach (var property in type.GetProperties(declaredStatic))
            {
                var isStatic = property.GetAccessors(nonPublic: true).Any(accessor => accessor.IsStatic);
                if (isStatic && serviceTypes.Contains(property.PropertyType))
                    exposures.Add($"{type.FullName}.{property.Name}: {property.PropertyType.Name}");
            }
        }
    }

    private static bool EnvironmentReadConfinementChecks(out string report)
    {
        var programType = ProgramRuleTarget;
        Type[] types;
        try { types = programType.Assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { types = ex.Types.OfType<Type>().ToArray(); }

        var violations = new HashSet<string>(StringComparer.Ordinal);
        var observed = new HashSet<string>(StringComparer.Ordinal);
        var scanErrors = new List<string>();
        const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;
        foreach (var type in types)
        {
            var methods = type.GetConstructors(declared).Cast<MethodBase>().ToList();
            if (type.TypeInitializer is { } initializer) methods.Add(initializer);
            methods.AddRange(type.GetMethods(declared));
            foreach (var method in methods)
            {
                var readsEnvironment = false;
                var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                ScanMethod(method, [], scanErrors,
                    inspectCall: (caller, opcode, target) =>
                    {
                        if ((opcode == OpCodes.Call || opcode == OpCodes.Callvirt)
                            && target.DeclaringType == typeof(Environment) && target.Name == nameof(Environment.GetEnvironmentVariable))
                            readsEnvironment = true;
                    },
                    inspectString: (_, value) =>
                    {
                        if (value is "DATA" or "DEV" || value.EndsWith("_DATA", StringComparison.OrdinalIgnoreCase)
                            || value.EndsWith("_DEV", StringComparison.OrdinalIgnoreCase))
                            keys.Add(value);
                    });
                if (!readsEnvironment || keys.Count == 0) continue;
                if (!IsExactOrNested(method.DeclaringType!, programType))
                    foreach (var key in keys) violations.Add($"{method.DeclaringType?.FullName}.{method.Name} reads {key}");
                else
                    foreach (var key in keys)
                        observed.Add(key is "DATA" || key.EndsWith("_DATA", StringComparison.OrdinalIgnoreCase) ? "DATA" : "DEV");
            }
        }

        foreach (var error in scanErrors.Distinct(StringComparer.Ordinal)) violations.Add("scan: " + error);
        foreach (var key in new[] { "DATA", "DEV" })
            if (!observed.Contains(key)) violations.Add($"ANTINOTEPAD_{key} read not found in Program");
        report = violations.Count == 0 ? "0 refs" : string.Join("; ", violations.OrderBy(value => value, StringComparer.Ordinal));
        return violations.Count == 0;
    }

    private static bool MatchesRule(LayerRule rule, Type from, Type to)
    {
        if (!MatchesSource(rule, from)) return false;
        if (rule.TargetKind == LayerTargetKind.PInvokeHolder) return IsPInvokeHolder(to);
        if (rule.TargetKind == LayerTargetKind.RawType) return IsRawInteropType(to, rule.RawTypeName!);
        if (rule.TargetKind == LayerTargetKind.WindowsNamespace)
            return to.Namespace == "Windows" || to.Namespace?.StartsWith("Windows.", StringComparison.Ordinal) == true;
        if (rule.AnyAntiNotepadNamespace)
            return IsAntiNotepadNamespace(to.Namespace) && !InNamespace(to.Namespace, rule.From);
        if (rule.ExactTo is { } exact) return IsExactOrNested(to, exact);
        return InNamespace(to.Namespace, rule.To!);
    }

    private static bool IsExactOrNested(Type type, Type exact)
    {
        for (var current = type; current is not null; current = current.DeclaringType)
            if (current == exact) return true;
        return false;
    }

    private static bool SettingsChangedChecks(out string report)
    {
        var failures = new List<string>();
        void Expect(bool ok, string name) { if (!ok) failures.Add(name); }

        var service = new SettingsService(new Settings());
        var first = new List<SettingsChange>();
        var second = new List<SettingsChange>();
        var appearanceCalls = 0;
        var callbackDepth = 0;
        var maxCallbackDepth = 0;
        var nestedRequested = false;
        var nestedWasQueued = false;

        void RecordAndNest(SettingsChange change)
        {
            callbackDepth++;
            maxCallbackDepth = Math.Max(maxCallbackDepth, callbackDepth);
            first.Add(change);
            if (!nestedRequested && change.Properties.Contains(nameof(Settings.Theme)))
            {
                nestedRequested = true;
                service.Update(settings => settings.Tone = "cool");
                nestedWasQueued = service.Current.Tone == "default";
            }
            callbackDepth--;
        }

        void ObserveAppearance(SettingsChange change)
        {
            second.Add(change);
            if ((change.Flags & SettingsChangeFlags.Appearance) != 0) appearanceCalls++;
        }

        service.Changed += RecordAndNest;
        service.Changed += ObserveAppearance;
        service.Update(settings => { settings.FontSize = 18; settings.Theme = "dark"; });

        Expect(first.Count == 2 && second.Count == 2, "one batch and one queued follow-up");
        Expect(first.Count > 0 && first[0].Properties.Contains(nameof(Settings.FontSize))
            && first[0].Properties.Contains(nameof(Settings.Theme)), "both changed property names");
        Expect(nestedWasQueued && first.Any(change => change.Properties.Contains(nameof(Settings.Tone))), "handler update is deferred");
        Expect(maxCallbackDepth == 1, "follow-up is not recursive");
        Expect(service.Current.FontSize == 18 && service.Current.Theme == "dark" && service.Current.Tone == "cool", "final values");

        var countBeforeNoOp = first.Count;
        service.Update(settings => settings.Theme = settings.Theme);
        Expect(first.Count == countBeforeNoOp, $"no-op update emits nothing ({countBeforeNoOp}->{first.Count})");

        var countBeforeList = first.Count;
        service.Update(settings => settings.DisabledLanguages.Add("py"));
        Expect(first.Count == countBeforeList + 1 && first[^1].Properties.Contains(nameof(Settings.DisabledLanguages)),
            $"language list edit detected ({countBeforeList}->{first.Count}: {string.Join(',', first[^1].Properties)})");

        var countBeforeLanguageMethod = first.Count;
        service.SetLanguageEnabled("js", false);
        Expect(first.Count == countBeforeLanguageMethod + 1
            && first[^1].Properties.SequenceEqual([nameof(Settings.DisabledLanguages)])
            && first[^1].Flags == (SettingsChangeFlags.Appearance | SettingsChangeFlags.Analysis | SettingsChangeFlags.Persistence),
            $"SetLanguageEnabled uses the owner ({countBeforeLanguageMethod}->{first.Count}: {string.Join(',', first[^1].Properties)})");

        var replacement = new Settings { FontSize = 20 };
        var countBeforeReplace = first.Count;
        service.Replace(replacement);
        Expect(ReferenceEquals(service.Current, replacement), "Replace changes Current identity");
        Expect(first.Count == countBeforeReplace + 1 && first[^1].Flags == SettingsChangeFlags.All
            && first[^1].Properties.Count == 56, "Replace emits all flags once");

        var appearanceBeforeSplit = appearanceCalls;
        var countBeforeSplit = first.Count;
        service.Update(settings => settings.Split = true);
        Expect(first.Count == countBeforeSplit + 1 && first[^1].Properties.SequenceEqual([nameof(Settings.Split)])
            && first[^1].Flags == (SettingsChangeFlags.Split | SettingsChangeFlags.Persistence),
            $"split flags stay isolated ({countBeforeSplit}->{first.Count}: {first[^1].Flags})");
        Expect(appearanceCalls == appearanceBeforeSplit, "split skips appearance subscriber");

        var detachedCalls = 0;
        void Detached(SettingsChange _) => detachedCalls++;
        service.Changed += Detached;
        service.Changed -= Detached;
        var appearanceBeforeGeometry = appearanceCalls;
        var countBeforeGeometry = first.Count;
        service.Update(settings => settings.X = 40);
        Expect(first.Count == countBeforeGeometry + 1 && first[^1].Properties.SequenceEqual([nameof(Settings.X)])
            && first[^1].Flags == (SettingsChangeFlags.Geometry | SettingsChangeFlags.Persistence),
            $"geometry flags are isolated ({countBeforeGeometry}->{first.Count}: {first[^1].Flags} {string.Join(',', first[^1].Properties)})");
        Expect(appearanceCalls == appearanceBeforeGeometry, $"geometry skips appearance subscriber ({appearanceBeforeGeometry}->{appearanceCalls})");
        Expect(detachedCalls == 0, "detached subscriber is not called");

        report = failures.Count == 0 ? "passed" : string.Join(", ", failures);
        return failures.Count == 0;
    }

    private static bool InNamespace(string? actual, string segment) =>
        actual == $"AntiNotepad.{segment}" || actual?.StartsWith($"AntiNotepad.{segment}.", StringComparison.Ordinal) == true;

    private static bool IsAntiNotepadNamespace(string? value) =>
        value == "AntiNotepad" || value?.StartsWith("AntiNotepad.", StringComparison.Ordinal) == true;

    private static Type TopLevelDeclaringType(Type type)
    {
        while (type.DeclaringType is { } parent) type = parent;
        return type;
    }

    private static bool IsPInvokeHolder(Type type)
    {
        if (type.Assembly != typeof(SelfTest).Assembly) return false;
        const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;
        return type.GetMethods(declared).Any(IsPInvokeMethod);
    }

    private static bool IsPInvokeMethod(MethodInfo method) => (method.Attributes & MethodAttributes.PinvokeImpl) != 0;

    private static bool IsRawInteropType(Type type, string rawTypeName) => rawTypeName switch
    {
        "Marshal" => type == typeof(System.Runtime.InteropServices.Marshal),
        "IntPtr" => type.FullName == "System.IntPtr",
        "UIntPtr" => type.FullName == "System.UIntPtr",
        "Win32Properties" => type.Name == "Win32Properties",
        _ => false
    };

    private static void AddType(Type? type, HashSet<Type> found, HashSet<Type>? directReferences = null)
    {
        if (type is null) return;
        var added = found.Add(type);
        var directAdded = directReferences?.Add(type) ?? false;
        if (!added && !directAdded) return;
        if (type.HasElementType) AddType(type.GetElementType(), found, directReferences);
        if (type.IsGenericParameter)
        {
            foreach (var constraint in type.GetGenericParameterConstraints()) AddType(constraint, found, directReferences);
        }
        if (type.IsGenericType)
        {
            AddType(type.GetGenericTypeDefinition(), found, directReferences);
            foreach (var argument in type.GetGenericArguments()) AddType(argument, found, directReferences);
        }
        if (type.IsFunctionPointer)
        {
            AddType(type.GetFunctionPointerReturnType(), found, directReferences);
            foreach (var parameter in type.GetFunctionPointerParameterTypes()) AddType(parameter, found, directReferences);
        }
    }

    private static void ScanMethod(MethodBase method, HashSet<Type> found, List<string> errors,
        Action<MethodBase, OpCode, MethodBase>? inspectCall = null,
        Type? allowedProgramTypeToken = null,
        Action<MethodBase, string>? inspectString = null,
        HashSet<Type>? directReferences = null)
    {
        AddType(method.DeclaringType, found, directReferences);
        foreach (var parameter in method.GetParameters()) AddType(parameter.ParameterType, found, directReferences);
        if (method is MethodInfo info) AddType(info.ReturnType, found, directReferences);
        if (method is MethodInfo methodInfo)
        {
            foreach (var argument in methodInfo.GetGenericArguments())
            {
                AddType(argument, found, directReferences);
                if (argument.IsGenericParameter)
                    foreach (var constraint in argument.GetGenericParameterConstraints()) AddType(constraint, found, directReferences);
            }
        }

        MethodBody? body;
        try { body = method.GetMethodBody(); }
        catch (Exception ex) { errors.Add($"{method.DeclaringType?.FullName}.{method.Name}: {ex.GetType().Name}"); return; }
        if (body?.GetILAsByteArray() is not { } il) return;

        var module = method.Module;
        var typeArguments = method.DeclaringType?.GetGenericArguments();
        var methodArguments = method is MethodInfo genericMethod && genericMethod.IsGenericMethod
            ? genericMethod.GetGenericArguments() : null;
        var opcodes = OpCodeMap.Value;
        var position = 0;
        while (position < il.Length)
        {
            var instructionOffset = position;
            var code = il[position++];
            short value = code == 0xfe && position < il.Length
                ? unchecked((short)(0xfe00 | il[position++]))
                : code;
            if (!opcodes.TryGetValue(value, out var opcode))
            {
                errors.Add($"{method.DeclaringType?.FullName}.{method.Name}: unknown IL opcode at {instructionOffset}");
                break;
            }

            var operandStart = position;
            var tokenKind = opcode.OperandType;
            int operandSize;
            switch (tokenKind)
            {
                case OperandType.InlineNone: operandSize = 0; break;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar: operandSize = 1; break;
                case OperandType.InlineVar: operandSize = 2; break;
                case OperandType.InlineI:
                case OperandType.InlineBrTarget:
                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.ShortInlineR: operandSize = 4; break;
                case OperandType.InlineI8:
                case OperandType.InlineR: operandSize = 8; break;
                case OperandType.InlineSwitch:
                    if (!HasBytes(il, position, 4)) { errors.Add($"{method.Name}: truncated switch at {instructionOffset}"); return; }
                    var branches = BitConverter.ToInt32(il, position);
                    if (branches < 0 || branches > (il.Length - position - 4) / 4)
                    { errors.Add($"{method.Name}: invalid switch at {instructionOffset}"); return; }
                    operandSize = 4 + branches * 4;
                    break;
                default:
                    errors.Add($"{method.DeclaringType?.FullName}.{method.Name}: unsupported IL operand {tokenKind}");
                    return;
            }
            if (!HasBytes(il, position, operandSize))
            { errors.Add($"{method.DeclaringType?.FullName}.{method.Name}: truncated IL at {instructionOffset}"); return; }

            if (opcode == OpCodes.Ldstr && tokenKind == OperandType.InlineString && inspectString is not null)
            {
                var token = BitConverter.ToInt32(il, operandStart);
                try { inspectString(method, module.ResolveString(token)); }
                catch (Exception ex) { errors.Add($"{method.DeclaringType?.FullName}.{method.Name}: string token 0x{token:x8} {ex.GetType().Name}"); }
            }

            if (operandSize == 4 && tokenKind is OperandType.InlineType or OperandType.InlineMethod or OperandType.InlineField or OperandType.InlineTok)
            {
                var token = BitConverter.ToInt32(il, operandStart);
                try
                {
                    if (tokenKind == OperandType.InlineType)
                        AddType(module.ResolveType(token, typeArguments, methodArguments), found, directReferences);
                    else if (tokenKind == OperandType.InlineMethod)
                    {
                        if (module.ResolveMethod(token, typeArguments, methodArguments) is { } target)
                        {
                            AddMethodReference(target, found);
                            AddType(target.DeclaringType, found, directReferences);
                            inspectCall?.Invoke(method, opcode, target);
                        }
                    }
                    else if (tokenKind == OperandType.InlineField)
                    {
                        if (module.ResolveField(token, typeArguments, methodArguments) is { } field)
                        {
                            AddType(field.DeclaringType, found);
                            AddType(field.FieldType, found);
                            AddType(field.DeclaringType, found, directReferences);
                            AddType(field.FieldType, found, directReferences);
                        }
                    }
                    else if (module.ResolveMember(token, typeArguments, methodArguments) is { } member)
                    {
                        if (member is Type resolvedType)
                        {
                            if (opcode != OpCodes.Ldtoken || resolvedType != allowedProgramTypeToken)
                            {
                                AddType(resolvedType, found);
                                AddType(resolvedType, found, directReferences);
                            }
                        }
                        else if (member is MethodBase resolvedMethod)
                        {
                            AddMethodReference(resolvedMethod, found);
                            AddType(resolvedMethod.DeclaringType, found, directReferences);
                        }
                        else if (member is FieldInfo resolvedField)
                        {
                            AddType(resolvedField.DeclaringType, found);
                            AddType(resolvedField.FieldType, found);
                            AddType(resolvedField.DeclaringType, found, directReferences);
                            AddType(resolvedField.FieldType, found, directReferences);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{method.DeclaringType?.FullName}.{method.Name}: token 0x{token:x8} {ex.GetType().Name}");
                }
            }
            position += operandSize;
        }
    }

    private static void AddMethodReference(MethodBase method, HashSet<Type> found)
    {
        AddType(method.DeclaringType, found);
        foreach (var parameter in method.GetParameters()) AddType(parameter.ParameterType, found);
        if (method is MethodInfo info)
        {
            AddType(info.ReturnType, found);
            foreach (var argument in info.GetGenericArguments()) AddType(argument, found);
        }
    }

    private static bool HasBytes(byte[] il, int position, int count) =>
        count >= 0 && position >= 0 && position <= il.Length - count;

    private static readonly Lazy<Dictionary<short, OpCode>> OpCodeMap = new(() =>
        typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(OpCode))
            .Select(field => (OpCode)field.GetValue(null)!)
            .ToDictionary(opcode => opcode.Value));

    private static bool DefaultDataMigrationSelfTest(out string report)
    {
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-migration-selftest-{Guid.NewGuid():N}");
        try
        {
            // A successful move must preserve all nested files, including the legacy log and backups.
            var successRoot = Path.Combine(root, "success");
            var oldFolder = Path.Combine(successRoot, DefaultDataFolderMigration.LegacyFolderName);
            Directory.CreateDirectory(Path.Combine(oldFolder, "notes", "nested"));
            Directory.CreateDirectory(Path.Combine(oldFolder, "backups"));
            File.WriteAllText(Path.Combine(oldFolder, "index.json"), "index");
            // This legacy filename is deliberately kept in the fixture to prove migration preserves it byte-for-byte.
            File.WriteAllText(Path.Combine(oldFolder, "antipad.log"), "legacy log remains intact");
            File.WriteAllText(Path.Combine(oldFolder, "notes", "nested", "note.md"), "nested note");
            File.WriteAllText(Path.Combine(oldFolder, "backups", "index.json.bak"), "backup");
            File.WriteAllText(Path.Combine(oldFolder, "settings.json"), "settings");
            var moved = DefaultDataFolderMigration.Resolve(successRoot, null);
            var newFolder = Path.Combine(successRoot, AppInfo.DataFolderName);
            if (!moved.Migrated || moved.Folder != newFolder || Directory.Exists(oldFolder)
                || File.ReadAllText(Path.Combine(newFolder, "index.json")) != "index"
                || File.ReadAllText(Path.Combine(newFolder, "antipad.log")) != "legacy log remains intact"
                || File.ReadAllText(Path.Combine(newFolder, "notes", "nested", "note.md")) != "nested note"
                || File.ReadAllText(Path.Combine(newFolder, "backups", "index.json.bak")) != "backup"
                || File.ReadAllText(Path.Combine(newFolder, "settings.json")) != "settings")
            {
                report = "successful move did not preserve the complete old tree";
                return false;
            }

            // An existing destination wins without merging or changing either directory.
            var existingRoot = Path.Combine(root, "destination-exists");
            var existingOld = Path.Combine(existingRoot, DefaultDataFolderMigration.LegacyFolderName);
            var existingNew = Path.Combine(existingRoot, AppInfo.DataFolderName);
            Directory.CreateDirectory(existingOld);
            Directory.CreateDirectory(existingNew);
            File.WriteAllText(Path.Combine(existingOld, "old-only.txt"), "old");
            File.WriteAllText(Path.Combine(existingNew, "new-only.txt"), "new");
            var alreadyMoved = DefaultDataFolderMigration.Resolve(existingRoot, null);
            if (alreadyMoved.Folder != existingNew || alreadyMoved.Migrated
                || File.Exists(Path.Combine(existingNew, "old-only.txt"))
                || File.ReadAllText(Path.Combine(existingNew, "new-only.txt")) != "new"
                || File.ReadAllText(Path.Combine(existingOld, "old-only.txt")) != "old")
            {
                report = "existing destination was merged or altered";
                return false;
            }

            // A custom override is returned verbatim and does not inspect or alter either default folder.
            var customRoot = Path.Combine(root, "custom-override");
            var customOld = Path.Combine(customRoot, DefaultDataFolderMigration.LegacyFolderName);
            var customNew = Path.Combine(customRoot, AppInfo.DataFolderName);
            var customFolder = Path.Combine(root, "custom-path");
            Directory.CreateDirectory(customOld);
            Directory.CreateDirectory(customFolder);
            File.WriteAllText(Path.Combine(customOld, "old-only.txt"), "old");
            File.WriteAllText(Path.Combine(customFolder, "custom.txt"), "custom");
            var custom = DefaultDataFolderMigration.Resolve(customRoot, customFolder);
            if (custom.Folder != customFolder || custom.Migrated || custom.Error is not null
                || Directory.Exists(customNew) || File.ReadAllText(Path.Combine(customOld, "old-only.txt")) != "old"
                || File.ReadAllText(Path.Combine(customFolder, "custom.txt")) != "custom")
            {
                report = "custom override touched the default paths";
                return false;
            }

            // A failed atomic rename leaves the old tree intact and selects it for continued use.
            var failureRoot = Path.Combine(root, "move-fails");
            var failureOld = Path.Combine(failureRoot, DefaultDataFolderMigration.LegacyFolderName);
            var failureNew = Path.Combine(failureRoot, AppInfo.DataFolderName);
            Directory.CreateDirectory(Path.Combine(failureOld, "notes"));
            File.WriteAllText(Path.Combine(failureOld, "notes", "keep.md"), "keep");
            var failed = DefaultDataFolderMigration.Resolve(failureRoot, null,
                static (_, _) => throw new IOException("simulated move failure"));
            if (failed.Folder != failureOld || failed.Migrated || failed.Error is null
                || Directory.Exists(failureNew) || File.ReadAllText(Path.Combine(failureOld, "notes", "keep.md")) != "keep")
            {
                report = "failed move did not fall back to the intact old tree";
                return false;
            }

            report = "move, existing destination, custom override, and failure fallback";
            return true;
        }
        catch (Exception ex)
        {
            report = ex.Message;
            return false;
        }
        finally { try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { } }
    }

    private static bool AppInfoSelfTest(out string report)
    {
        try
        {
            var assembly = typeof(AppInfo).Assembly;
            var product = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), false)
                .Cast<System.Reflection.AssemblyProductAttribute>().Single().Product;
            var title = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false)
                .Cast<System.Reflection.AssemblyTitleAttribute>().Single().Title;
            var informationalVersion = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
                .Cast<System.Reflection.AssemblyInformationalVersionAttribute>().Single().InformationalVersion;
            var assemblyVersion = assembly.GetName().Version?.ToString();
            var fileVersion = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), false)
                .Cast<System.Reflection.AssemblyFileVersionAttribute>().Single().Version;
            var description = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false)
                .Cast<System.Reflection.AssemblyDescriptionAttribute>().Single().Description;
            var copyright = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false)
                .Cast<System.Reflection.AssemblyCopyrightAttribute>().Single().Copyright;

            if (AppInfo.DisplayName != product || AppInfo.DisplayName != title || assembly.GetName().Name != AppInfo.Name
                || AppInfo.InformationalVersion != informationalVersion
                || !informationalVersion.StartsWith(AppInfo.Version, StringComparison.Ordinal)
                || assemblyVersion != $"{AppInfo.Version}.0" || fileVersion != $"{AppInfo.Version}.0"
                || AppInfo.Description != description || AppInfo.Copyright != copyright
                || AppInfo.InstanceId != AppInfo.Name || AppInfo.DevInstanceId != $"{AppInfo.Name}-dev"
                || AppInfo.DataFolderName != AppInfo.Name || AppInfo.RunValueName != AppInfo.Name
                || AppInfo.LogFileName != $"{AppInfo.Name.ToLowerInvariant()}.log"
                || AppInfo.EnvPrefix != AppInfo.Name.ToUpperInvariant()
                || AppInfo.Env("DATA") != $"{AppInfo.EnvPrefix}_DATA"
                || AppInfo.Version + (AppInfo.Label.Length == 0 ? "" : $"-{AppInfo.Label}") != informationalVersion)
            {
                report = "AppInfo does not match the generated assembly identity or its derived identifiers";
                return false;
            }

            // Use the file resource loader directly: selftest never initializes the desktop platform.
            var assets = new StandardAssetLoader();
            using var icon = assets.Open(new Uri(AppInfo.IconUri));
            using var font = assets.Open(new Uri(AppInfo.FontUri));
            if (icon.ReadByte() < 0 || font.ReadByte() < 0)
            {
                report = "bundled icon or font resource is empty";
                return false;
            }

            report = "assembly attributes, derived identifiers, and bundled icon/font URIs";
            return true;
        }
        catch (Exception ex)
        {
            report = ex.Message;
            return false;
        }
    }

    private static bool InputLimitChecks(out string report)
    {
        var failures = new List<string>();
        void Check(bool ok, string what) { if (!ok) failures.Add(what); }

        try
        {
            Check(!Editor.ListEditing.TryListNumber("123456789012. item", out _), "12-digit list marker is not continued or renumbered");

            Check(!Timers.TryParse("999999999999999999999h", out _, out _), "oversized timer is rejected");
            Check(!Timers.TryParse("\u0665m", out _, out _), "non-ASCII timer digits are rejected");
            Check(!Timers.TryParse("1000h", out _, out _), "1000-hour timer is rejected");
            Check(!Timers.TryParse("1000:00:00", out _, out _), "1000-hour clock timer is rejected");
            Check(!Timers.TryParse("999h60m", out _, out _), "timer beyond the maximum is rejected");
            Check(Timers.TryParse("999h59m59s", out var maxParts, out _) && maxParts == TimeSpan.FromSeconds(3_599_999), "999h59m59s timer is accepted");
            Check(Timers.TryParse("999:59:59", out var maxClock, out _) && maxClock == TimeSpan.FromSeconds(3_599_999), "999:59:59 clock timer is accepted");
            Check(Timers.Format(TimeSpan.FromSeconds(3_599_999)) == "999:59:59", "long timer formatting preserves three-digit hours");

            var deep = new string('(', 65) + "1" + new string(')', 65);
            Check(Calc.Calculator.EvaluateNote(["math", deep])[1]?.Result is null, "65-deep parentheses are invalid");
            Check(Calc.Calculator.EvaluateNote(["math", new string('(', 64) + "1" + new string(')', 64)])[1]?.Result?.Value == 1, "64-deep parentheses remain valid");
            Check(Calc.Calculator.EvaluateNote(["math", new string('-', 65) + "1"])[1]?.Result is null, "65 chained unary signs are invalid");

            Calc.CalcResult? One(string expression) => Calc.Calculator.EvaluateNote(["math", expression])[1]?.Result;
            Calc.LineEval? OneLine(string expression) => Calc.Calculator.EvaluateNote(["math", expression])[1];
            Check(One("$5 + €3") is null, "mixed-currency addition is invalid");
            Check(OneLine("$5 + €3")?.ErrorReason == Calc.CalcErrorReason.MixedCurrencies, "mixed-currency addition has an explicit error reason");
            Check(Calc.Calculator.EvaluateInlineLine("$5 + €3 =")?.ErrorReason == Calc.CalcErrorReason.MixedCurrencies, "inline mixed-currency addition has an explicit error reason");
            Check(One("$5 + $3")?.Display == "$8", "same-currency addition is preserved");
            Check(One("$5 + 3")?.Display == "$8", "plain number plus money keeps its currency");
            Check(One("$5 + €3%") is null, "mixed-currency percent addition is invalid");
            Check(One("min($5, €3)") is null && One("sum($5, €3)") is null, "mixed-currency comparisons and aggregates are invalid");
            Check(Calc.Calculator.EvaluateNote(["math", "$5", "€3", "sum"])[3]?.Result is null, "mixed-currency note aggregate is invalid");

            bool parsedFirst = Timers.TryParse("5m tea", out var firstDuration, out var firstLabel, out var firstText);
            bool parsedSecond = Timers.TryParse("5m coffee", out var secondDuration, out var secondLabel, out var secondText);
            var firstKey = Editor.NoteAnalysis.TimerKey("selftest", firstText, 1);
            var secondKey = Editor.NoteAnalysis.TimerKey("selftest", secondText, 1);
            Check(parsedFirst && parsedSecond && firstDuration == secondDuration && firstLabel != secondLabel && firstKey == secondKey,
                "timer key survives a label edit");

            var timers = new Timers(id => id == "live");
            var removed = timers.Ensure(firstKey, firstDuration, firstLabel, "live");
            removed.EndsAt = DateTime.Now.AddMinutes(5);
            Check(timers.Reconcile("live", Array.Empty<string>()) == 1 && timers.Get(firstKey) is null && timers.EntryCount == 0 && !removed.Running,
                "removed timer line stops and drops its entry");
            timers.Ensure("deleted:5m:1", firstDuration, firstLabel, "deleted");
            timers.Reconcile("live", Array.Empty<string>());
            Check(timers.Get("deleted:5m:1") is null, "deleted note timer is pruned");
            Check(!Timers.TryEndAt(DateTime.MaxValue, TimeSpan.FromSeconds(1), out _), "timer end overflow is rejected");

            report = failures.Count == 0 ? "list limits, timer bounds and identity, calculator depth and currencies, timer pruning and end overflow" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = "unexpected exception: " + ex.Message;
            return false;
        }
    }

    private static bool NotesLocationChecks(out string report)
    {
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-notes-location-selftest-{Guid.NewGuid():N}");
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        try
        {
            var freshHome = Path.Combine(root, "fresh-home");
            var freshDocuments = Path.Combine(root, "fresh-documents");
            var fresh = NotesLocation.Resolve(freshHome, freshDocuments);
            var freshExpected = Path.Combine(freshDocuments, AppInfo.DataFolderName);
            CheckCase(fresh.Error is null && fresh.NotesFolder == freshExpected && Directory.Exists(freshExpected)
                && NotesLocation.ReadPointer(freshHome) == freshExpected, "fresh install writes the Documents pointer");

            var moveHome = Path.Combine(root, "move-home");
            var moveDocuments = Path.Combine(root, "move-documents");
            var moveTarget = Path.Combine(moveDocuments, AppInfo.DataFolderName);
            Directory.CreateDirectory(Path.Combine(moveHome, "notes"));
            Directory.CreateDirectory(Path.Combine(moveHome, "recovered"));
            File.WriteAllText(Path.Combine(moveHome, "index.json"), "index bytes");
            File.WriteAllText(Path.Combine(moveHome, "index.json.bak"), "backup bytes");
            File.WriteAllText(Path.Combine(moveHome, "notes", "note1.md"), "a note body");
            File.WriteAllText(Path.Combine(moveHome, "recovered", "orphan.md"), "recovered body");
            File.WriteAllText(Path.Combine(moveHome, AppInfo.LogFileName), "the app log stays home");
            var moved = NotesLocation.Resolve(moveHome, moveDocuments);
            var homeBackup = Directory.EnumerateDirectories(moveHome, "backup-before-move-*").SingleOrDefault();
            CheckCase(moved.Error is null && moved.NotesFolder == moveTarget && moved.Notice is null
                && NotesLocation.ReadPointer(moveHome) == moveTarget
                && File.ReadAllText(Path.Combine(moveTarget, "index.json")) == "index bytes"
                && File.ReadAllText(Path.Combine(moveTarget, "index.json.bak")) == "backup bytes"
                && File.ReadAllText(Path.Combine(moveTarget, "notes", "note1.md")) == "a note body"
                && File.ReadAllText(Path.Combine(moveTarget, "recovered", "orphan.md")) == "recovered body"
                && File.Exists(Path.Combine(moveHome, AppInfo.LogFileName))
                && homeBackup is not null && File.Exists(Path.Combine(homeBackup, "index.json")), "first-run move verifies copies and leaves an app-home backup");

            var occupiedHome = Path.Combine(root, "occupied-home");
            var occupiedDocuments = Path.Combine(root, "occupied-documents");
            var occupiedTarget = Path.Combine(occupiedDocuments, AppInfo.DataFolderName);
            Directory.CreateDirectory(Path.Combine(occupiedHome, "notes"));
            Directory.CreateDirectory(occupiedTarget);
            File.WriteAllText(Path.Combine(occupiedHome, "index.json"), "keep old");
            File.WriteAllText(Path.Combine(occupiedTarget, "index.json"), "keep target");
            var occupied = NotesLocation.Resolve(occupiedHome, occupiedDocuments);
            CheckCase(occupied.NotesFolder == occupiedHome && occupied.Notice is not null && NotesLocation.ReadPointer(occupiedHome) is null
                && File.ReadAllText(Path.Combine(occupiedHome, "index.json")) == "keep old"
                && File.ReadAllText(Path.Combine(occupiedTarget, "index.json")) == "keep target", "existing target index is refused");

            var missingHome = Path.Combine(root, "missing-home");
            var missingTarget = Path.Combine(root, "missing-target");
            Directory.CreateDirectory(missingHome);
            File.WriteAllText(Path.Combine(missingHome, NotesLocation.PointerFileName), System.Text.Json.JsonSerializer.Serialize(new { NotesFolder = missingTarget }));
            var missing = NotesLocation.Resolve(missingHome, Path.Combine(root, "unused-documents"));
            CheckCase(missing.Error is not null && missing.NotesFolder == missingTarget && !Directory.Exists(missingTarget), "missing pointer target is not created");

            var recoveryFolder = Path.Combine(root, "unknown-body");
            var unknownStore = new Store(recoveryFolder);
            unknownStore.Load();
            unknownStore.Data.Stack[0].Text = "kept note";
            CheckCase(unknownStore.Save(), "fixture note saves for orphan recovery");
            var unknownBody = Path.Combine(recoveryFolder, "notes", "unknown.md");
            File.WriteAllText(unknownBody, "unknown body survives");
            var recoveryStore = new Store(recoveryFolder);
            recoveryStore.Load();
            var recoveredBody = Path.Combine(recoveryFolder, "recovered", "unknown.md");
            CheckCase(!File.Exists(unknownBody) && File.ReadAllText(recoveredBody) == "unknown body survives", "unknown body moves to recovered");

            var deleteFolder = Path.Combine(root, "delete-forever");
            var deleteStore = new Store(deleteFolder);
            deleteStore.Load();
            var deleted = deleteStore.Data.Stack[0];
            deleted.Text = "delete forever body";
            CheckCase(deleteStore.Save(), "fixture note saves for delete-forever");
            var deletedBody = Path.Combine(deleteFolder, "notes", deleted.Id + ".md");
            deleteStore.Data.Stack.Remove(deleted);
            deleted.Deleted = DateTime.Now;
            deleteStore.Data.Trash.Add(deleted);
            CheckCase(deleteStore.Save() && File.Exists(deletedBody), "trash entry keeps its body");
            deleteStore.Data.Trash.Remove(deleted);
            CheckCase(deleteStore.Save() && !File.Exists(deletedBody), "delete-forever removes its own body");

            var fallbackFolder = Path.Combine(root, "invalid-index");
            var fallbackStore = new Store(fallbackFolder);
            fallbackStore.Load();
            fallbackStore.Data.Stack[0].Text = "valid backup";
            fallbackStore.Save();
            fallbackStore.Save();
            File.WriteAllText(Path.Combine(fallbackFolder, "index.json"), "{\"Settings\":null}");
            var fallback = new Store(fallbackFolder);
            fallback.Load();
            CheckCase(fallback.Data.Settings is not null && fallback.Data.Stack[0].Text == "valid backup", "null Settings falls back to index backup");

            var missingBodyFolder = Path.Combine(root, "missing-body");
            var missingBodyStore = new Store(missingBodyFolder);
            missingBodyStore.Load();
            var missingNote = missingBodyStore.Data.Stack[0];
            missingNote.Text = "nonblank note";
            missingBodyStore.Save();
            var bodyPath = Path.Combine(missingBodyFolder, "notes", missingNote.Id + ".md");
            File.Delete(bodyPath);
            var missingBodyReload = new Store(missingBodyFolder);
            missingBodyReload.Load();
            var lostText = missingBodyReload.Data.Stack[0];
            var preview = lostText.PeekText();
            lostText.Text = "must not overwrite";
            lostText.Touched(blank: false, NoteMode.Plain);
            CheckCase(preview.Length > 0 && lostText.BodyUnavailable && !lostText.Dirty
                && missingBodyReload.Save() && !File.Exists(bodyPath), "missing nonblank body stays unavailable and is not saved");

            var bypassHome = Path.Combine(root, "bypass-home");
            var bypassDocuments = Path.Combine(root, "bypass-documents");
            var custom = Path.Combine(root, "custom-data");
            Directory.CreateDirectory(bypassHome);
            File.WriteAllText(Path.Combine(bypassHome, "index.json"), "must not inspect");
            var bypass = NotesLocation.Resolve(bypassHome, bypassDocuments, custom);
            CheckCase(bypass.Custom && bypass.AppHome == custom && bypass.NotesFolder == custom
                && !Directory.Exists(custom) && !Directory.Exists(bypassDocuments)
                && File.ReadAllText(Path.Combine(bypassHome, "index.json")) == "must not inspect", "ANTINOTEPAD_DATA bypasses Documents and first-run movement");

            var movedDev = Path.Combine(root, "custom-moved");
            Directory.CreateDirectory(custom); Directory.CreateDirectory(movedDev);
            File.WriteAllText(Path.Combine(custom, NotesLocation.PointerFileName), System.Text.Json.JsonSerializer.Serialize(new { NotesFolder = movedDev }));
            var followed = NotesLocation.Resolve(bypassHome, bypassDocuments, custom);
            CheckCase(followed.Custom && followed.AppHome == custom && followed.NotesFolder == movedDev && followed.Error is null
                && !Directory.Exists(bypassDocuments), "ANTINOTEPAD_DATA follows its own pointer after an in-app move");

            report = failures.Count == 0 ? "fresh location, verified move and backup, collision refusal, missing pointer, recovered unknown, direct deletion, invalid-index fallback, unavailable body, and custom-path bypass" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.Message));
            return false;
        }
        finally { try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { } }
    }

    private static bool NotesMoveChecks(out string report)
    {
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-notes-session-selftest-{Guid.NewGuid():N}");
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        Store? store = null;
        Store? unavailableStore = null;
        try
        {
            var source = Path.Combine(root, "source");
            var target = Path.Combine(root, "target");
            store = new Store(source);
            store.Load();
            var data = store.Data;
            var settingsService = store.SettingsService;
            var note = store.Data.Stack[0];
            note.Text = "notes session before move";
            CheckCase(store.Save(), "source note saves before move");

            var session = new NotesSession(store, source, source, null, _ => { });
            var moved = session.MoveNotes(target, out _, out _);
            CheckCase(moved && ReferenceEquals(session.Store, store) && ReferenceEquals(store.Data, data)
                && ReferenceEquals(store.SettingsService, settingsService) && session.DataFolder == store.Folder
                && session.DataFolder == target && session.NotesLocationError is null && store.IsAvailable
                && Path.GetFullPath(NotesLocation.ReadPointer(source) ?? "") == Path.GetFullPath(target),
                "empty sibling move keeps Store, Data, and SettingsService and follows pointer");

            note.Text = "later save after move";
            var savedAfterMove = store.Save();
            var movedBody = Path.Combine(target, "notes", note.Id + ".md");
            CheckCase(savedAfterMove && ReferenceEquals(session.Store, store) && ReferenceEquals(store.Data, data)
                && ReferenceEquals(store.SettingsService, settingsService) && File.ReadAllText(movedBody) == "later save after move",
                "later save uses the original Store in the target folder");

            var occupied = Path.Combine(root, "occupied");
            Directory.CreateDirectory(occupied);
            File.WriteAllText(Path.Combine(occupied, "index.json"), "occupied");
            var pointerBeforeFailure = NotesLocation.ReadPointer(source);
            var folderBeforeFailure = session.DataFolder;
            var errorBeforeFailure = session.NotesLocationError;
            var occupiedRefused = !session.MoveNotes(occupied, out _, out _);
            CheckCase(occupiedRefused && session.DataFolder == folderBeforeFailure && session.NotesLocationError == errorBeforeFailure
                && ReferenceEquals(session.Store, store) && ReferenceEquals(store.Data, data)
                && ReferenceEquals(store.SettingsService, settingsService) && NotesLocation.ReadPointer(source) == pointerBeforeFailure,
                "occupied target refusal leaves folder, error, identities, and pointer unchanged");

            var nested = Path.Combine(target, "nested");
            folderBeforeFailure = session.DataFolder;
            errorBeforeFailure = session.NotesLocationError;
            var nestedRefused = !session.MoveNotes(nested, out _, out _);
            CheckCase(nestedRefused && !Directory.Exists(nested) && session.DataFolder == folderBeforeFailure
                && session.NotesLocationError == errorBeforeFailure && ReferenceEquals(session.Store, store)
                && ReferenceEquals(store.Data, data) && ReferenceEquals(store.SettingsService, settingsService)
                && NotesLocation.ReadPointer(source) == pointerBeforeFailure,
                "nested target refusal creates nothing and leaves session state unchanged");

            var unavailableFolder = Path.Combine(root, "unavailable-source");
            var unavailableTarget = Path.Combine(root, "unavailable-target");
            unavailableStore = new Store(unavailableFolder, createDirectories: false);
            unavailableStore.Load();
            var unavailableData = unavailableStore.Data;
            var unavailableSettings = unavailableStore.SettingsService;
            var unavailableError = unavailableStore.LastError ?? "unavailable";
            var unavailableSession = new NotesSession(unavailableStore, root, unavailableFolder, unavailableError, _ => { });
            var unavailableRefused = !unavailableSession.MoveNotes(unavailableTarget, out _, out _);
            CheckCase(unavailableRefused && !unavailableStore.IsAvailable && !Directory.Exists(unavailableFolder) && !Directory.Exists(unavailableTarget)
                && unavailableSession.DataFolder == unavailableFolder && unavailableSession.NotesLocationError == unavailableError
                && ReferenceEquals(unavailableSession.Store, unavailableStore) && ReferenceEquals(unavailableStore.Data, unavailableData)
                && ReferenceEquals(unavailableStore.SettingsService, unavailableSettings),
                "unavailable store refuses without creating either folder");

            report = failures.Count == 0 ? "passed" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.Message));
            return false;
        }
        finally
        {
            try { store?.Flush(); } catch { }
            try { unavailableStore?.Flush(); } catch { }
            try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
        }
    }

    private static bool NotesMoveRenameChecks(out string report)
    {
        var root = Path.Combine(Environment.CurrentDirectory, "build", $"A52-move-old-folder-rename-warning-fixtures-{Guid.NewGuid():N}");
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        Store? collisionStore = null;

        try
        {
            Directory.CreateDirectory(root);
            foreach (var (name, trailingSeparator) in new[]
            {
                ("trimmed", ""),
                ("backslash", "\\"),
                ("slash", "/")
            })
            {
                var caseRoot = Path.Combine(root, name);
                var caseAppHome = Path.Combine(caseRoot, "app-home");
                var source = Path.Combine(caseRoot, "test targetA");
                var target = Path.Combine(caseRoot, "targetB");
                var sourceFile = Path.Combine(source, "notes", "move-contract.md");
                Directory.CreateDirectory(caseAppHome);
                Directory.CreateDirectory(Path.GetDirectoryName(sourceFile)!);
                File.WriteAllText(sourceFile, "old note content");

                var sourceWithTrailingSeparator = source + trailingSeparator;
                var result = NotesLocation.Move(caseAppHome, sourceWithTrailingSeparator, target);
                var oldCopy = result.OldCopy;
                var backup = oldCopy?.BackupFolder;
                var displayMessage = oldCopy?.Message(result.FileCount);
                CheckCase(result.Success && oldCopy is not null && oldCopy.Failure is null
                    && PathEquals(oldCopy.OriginalFolder, source) && backup is not null
                    && Path.GetFileName(backup).StartsWith("test targetA (moved ", StringComparison.Ordinal)
                    && !Directory.Exists(source)
                    && File.ReadAllText(Path.Combine(backup!, "notes", "move-contract.md")) == "old note content"
                    && File.ReadAllText(Path.Combine(target, "notes", "move-contract.md")) == "old note content"
                    && displayMessage == $"Moved 1 file; the old folder was kept as {Path.GetFullPath(backup!)}."
                    && displayMessage is not null
                    && displayMessage.IndexOf(Path.GetFullPath(backup!), StringComparison.OrdinalIgnoreCase)
                        == displayMessage.LastIndexOf(Path.GetFullPath(backup!), StringComparison.OrdinalIgnoreCase),
                    $"{name} source path copied successfully and retained its old folder; outcome: {oldCopy?.Message(result.FileCount) ?? result.Error ?? "missing old-copy outcome"}");
            }

            var appHomeRoot = Path.Combine(root, "app-home-equivalence");
            var appHome = Path.Combine(appHomeRoot, "app-home");
            var appHomeTarget = Path.Combine(appHomeRoot, "target");
            Directory.CreateDirectory(Path.Combine(appHome, "notes"));
            File.WriteAllText(Path.Combine(appHome, "index.json"), "app-home index");
            File.WriteAllText(Path.Combine(appHome, "notes", "app-home.md"), "app-home note");
            var appHomeMove = NotesLocation.Move(appHome, appHome + "\\", appHomeTarget);
            var appHomeCopy = appHomeMove.OldCopy;
            var appHomeMessage = appHomeCopy?.Message(appHomeMove.FileCount);
            CheckCase(appHomeMove.Success && appHomeCopy is not null && appHomeCopy.Failure is null
                && PathEquals(appHomeCopy.OriginalFolder, appHome) && appHomeCopy.BackupFolder is not null
                && Path.GetFileName(appHomeCopy.BackupFolder).StartsWith("backup-before-move-", StringComparison.Ordinal)
                && File.ReadAllText(Path.Combine(appHomeTarget, "notes", "app-home.md")) == "app-home note"
                && File.ReadAllText(Path.Combine(appHomeCopy.BackupFolder!, "notes", "app-home.md")) == "app-home note"
                && appHomeMessage == $"Moved {appHomeMove.FileCount} files; the old folder was kept as {Path.GetFullPath(appHomeCopy.BackupFolder!)}."
                && appHomeMessage is not null
                && appHomeMessage.IndexOf(Path.GetFullPath(appHomeCopy.BackupFolder!), StringComparison.OrdinalIgnoreCase)
                    == appHomeMessage.LastIndexOf(Path.GetFullPath(appHomeCopy.BackupFolder!), StringComparison.OrdinalIgnoreCase)
                && NotesLocation.ReadPointer(appHome) is { } appHomePointer && PathEquals(appHomePointer, appHomeTarget),
                "app-home path with a trailing backslash is recognized as the app home and uses its backup path");

            var collisionRoot = Path.Combine(root, "collision");
            var collisionAppHome = Path.Combine(collisionRoot, "app-home");
            var collisionSource = Path.Combine(collisionRoot, "source");
            var collisionTarget = Path.Combine(collisionRoot, "target");
            collisionStore = new Store(collisionSource);
            collisionStore.Load();
            collisionStore.Data.Stack[0].Text = "move despite backup collision";
            CheckCase(collisionStore.Save(), "collision source note saves before move");
            var collisionBackupPaths = new[] { DateTime.Now, DateTime.Now.AddMinutes(1) }
                .Select(time => Path.Combine(collisionRoot, $"source (moved {time:yyyy-MM-dd HHmm})"))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            foreach (var collisionBackup in collisionBackupPaths) Directory.CreateDirectory(collisionBackup);
            var collisionSession = new NotesSession(collisionStore, collisionAppHome, collisionSource, null, _ => { });
            var collisionMoved = collisionSession.MoveNotes(collisionTarget, out var collisionMessage, out var collisionResult);
            var collisionOutcome = collisionResult?.OldCopy;
            var collisionNotePath = Path.Combine("notes", collisionStore.Data.Stack[0].Id + ".md");
            var collisionSourceBody = Path.Combine(collisionSource, collisionNotePath);
            var collisionTargetBody = Path.Combine(collisionTarget, collisionNotePath);
            var postMoveText = "post-collision save uses target";
            collisionStore.Data.Stack[0].Text = postMoveText;
            var collisionSaved = collisionStore.Save();
            CheckCase(collisionMoved && collisionResult is { Success: true }
                && collisionOutcome is { Failure: NotesOldCopyFailure.NameCollision, BackupFolder: null }
                && PathEquals(collisionOutcome.OriginalFolder, collisionSource)
                && collisionSession.DataFolder == collisionTarget
                && collisionStore.Folder == collisionTarget && collisionStore.IsAvailable && collisionSession.NotesLocationError is null
                && PathEquals(NotesLocation.ReadPointer(collisionAppHome) ?? "", collisionTarget)
                && Directory.Exists(collisionSource)
                && File.ReadAllText(collisionSourceBody) == "move despite backup collision"
                && collisionSaved && File.ReadAllText(collisionTargetBody) == postMoveText
                && collisionMessage == collisionOutcome.Message(collisionResult.FileCount)
                && collisionMessage == $"Notes moved; the old folder remains at {Path.GetFullPath(collisionSource)} because its backup name already exists.",
                $"backup-name collision keeps the old folder but completes the move and reports its outcome: {collisionMessage}");

            report = failures.Count == 0 ? "passed" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.Message));
            return false;
        }
        finally
        {
            try { collisionStore?.Flush(); } catch { }
            try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
        }
    }

    private static bool PathEquals(string left, string right)
    {
        try
        {
            var normalizedLeft = Path.TrimEndingDirectorySeparator(Path.GetFullPath(left));
            var normalizedRight = Path.TrimEndingDirectorySeparator(Path.GetFullPath(right));
            return string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    private static bool NotesUseFolderChecks(out string report)
    {
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-notes-use-folder-selftest-{Guid.NewGuid():N}");
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        Store? unavailableStore = null;
        Store? availableStore = null;
        try
        {
            var appHome = Path.Combine(root, "app-home");
            var missingFolder = Path.Combine(root, "missing-notes");
            var targetFolder = Path.Combine(root, "existing-notes");
            var untouchedFolder = Path.Combine(root, "untouched-notes");
            Directory.CreateDirectory(appHome);
            Directory.CreateDirectory(targetFolder);
            Directory.CreateDirectory(untouchedFolder);
            var targetStore = new Store(targetFolder);
            targetStore.Load();
            targetStore.Data.Stack[0].Text = "adopt this folder";
            CheckCase(targetStore.Save(), "existing target index is written");
            targetStore.Flush();
            NotesLocation.WritePointer(appHome, untouchedFolder);

            unavailableStore = new Store(missingFolder, createDirectories: false);
            unavailableStore.Load();
            var unavailableError = unavailableStore.LastError ?? $"The notes folder is unavailable: {missingFolder}";
            var unavailableData = unavailableStore.Data;
            var session = new NotesSession(unavailableStore, appHome, missingFolder, unavailableError, _ => { });
            var changed = session.UseFolderOnNextStart(targetFolder, out var message);
            CheckCase(changed && message == $"Notes folder set to {targetFolder}. Quit and start AntiNotepad again to load it."
                && NotesLocation.ReadPointer(appHome) == targetFolder && !Directory.Exists(missingFolder) && Directory.Exists(targetFolder)
                && session.DataFolder == missingFolder && session.NotesLocationError == unavailableError
                && ReferenceEquals(session.Store, unavailableStore) && ReferenceEquals(unavailableStore.Data, unavailableData),
                "unavailable session sets pointer without changing session or creating a folder");

            var resolved = NotesLocation.Resolve(appHome, Path.Combine(root, "unused-documents"));
            CheckCase(resolved.Error is null && resolved.NotesFolder == targetFolder,
                "fresh resolution follows the selected folder pointer");

            var availableFolder = Path.Combine(root, "available-notes");
            availableStore = new Store(availableFolder);
            availableStore.Load();
            var availableSession = new NotesSession(availableStore, appHome, availableFolder, null, _ => { });
            var pointerBeforeAvailable = NotesLocation.ReadPointer(appHome);
            var availableRefused = !availableSession.UseFolderOnNextStart(targetFolder, out _);
            CheckCase(availableRefused && NotesLocation.ReadPointer(appHome) == pointerBeforeAvailable
                && availableSession.DataFolder == availableFolder && availableSession.NotesLocationError is null
                && ReferenceEquals(availableSession.Store, availableStore),
                "available session refuses and leaves the pointer untouched");

            var missingTarget = Path.Combine(root, "missing-target");
            var pointerBeforeMissing = NotesLocation.ReadPointer(appHome);
            var missingRefused = !session.UseFolderOnNextStart(missingTarget, out _);
            CheckCase(missingRefused && !Directory.Exists(missingTarget) && NotesLocation.ReadPointer(appHome) == pointerBeforeMissing
                && session.DataFolder == missingFolder && session.NotesLocationError == unavailableError
                && ReferenceEquals(session.Store, unavailableStore) && ReferenceEquals(unavailableStore.Data, unavailableData),
                "missing target is refused without creation or pointer changes");

            var fileTarget = Path.Combine(root, "not-a-folder");
            File.WriteAllText(fileTarget, "file target");
            var pointerBeforeFile = NotesLocation.ReadPointer(appHome);
            var fileRefused = !session.UseFolderOnNextStart(fileTarget, out _);
            CheckCase(fileRefused && File.Exists(fileTarget) && !Directory.Exists(fileTarget)
                && NotesLocation.ReadPointer(appHome) == pointerBeforeFile && session.DataFolder == missingFolder
                && session.NotesLocationError == unavailableError && ReferenceEquals(session.Store, unavailableStore)
                && ReferenceEquals(unavailableStore.Data, unavailableData),
                "file target is refused without pointer or session changes");

            report = failures.Count == 0 ? "passed" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.Message));
            return false;
        }
        finally
        {
            try { unavailableStore?.Flush(); } catch { }
            try { availableStore?.Flush(); } catch { }
            try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
        }
    }

    private static bool UnavailableStoreStartupChecks(out string report)
    {
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-unavailable-store-selftest-{Guid.NewGuid():N}");
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        Store? missingStore = null;
        Store? fallbackStore = null;
        try
        {
            var missingFolder = Path.Combine(root, "missing-load");
            missingStore = new Store(missingFolder, createDirectories: false);
            missingStore.Load();
            var missingSelection = missingStore.Data.Stack.Count == 1 && missingStore.Data.Stack[0].IsEmpty
                && missingStore.Data.Current >= 0 && missingStore.Data.Current < missingStore.Data.Stack.Count;
            var missingData = missingStore.Data;
            var missingSave = missingStore.Save();
            CheckCase(!missingStore.IsAvailable && missingSelection && missingSave && missingStore.LastError is not null
                && !Directory.Exists(missingFolder), "missing-folder Load leaves a blank current note and Save creates nothing");

            var fallbackFolder = Path.Combine(root, "missing-fallback");
            fallbackStore = new Store(fallbackFolder, createDirectories: false);
            fallbackStore.MarkUnavailable("fixture constructor fallback");
            fallbackStore.Load();
            var fallbackSelection = fallbackStore.Data.Stack.Count == 1 && fallbackStore.Data.Stack[0].IsEmpty
                && fallbackStore.Data.Current >= 0 && fallbackStore.Data.Current < fallbackStore.Data.Stack.Count;
            var fallbackSave = fallbackStore.Save();
            CheckCase(!fallbackStore.IsAvailable && fallbackSelection && fallbackSave && fallbackStore.LastError is not null
                && !Directory.Exists(fallbackFolder), "MarkUnavailable constructor fallback leaves a blank current note and Save creates nothing");

            var validFolder = Path.Combine(root, "valid-folder");
            var source = new Store(validFolder);
            source.Load();
            source.Data.Stack[0].Text = "loaded from valid folder";
            var sourceSaved = source.Save();
            var placeholderData = missingStore.Data;
            missingStore.SwitchFolder(validFolder);
            missingStore.Load();
            CheckCase(sourceSaved && missingStore.IsAvailable && !ReferenceEquals(placeholderData, missingStore.Data)
                && missingStore.Data.Stack[missingStore.Data.Current].Text == "loaded from valid folder"
                && !ReferenceEquals(missingData, missingStore.Data), "SwitchFolder then Load replaces the placeholder with valid-folder data");

            report = failures.Count == 0 ? "missing-folder load, constructor fallback, no-write Save, and valid-folder reload" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.Message));
            return false;
        }
        finally
        {
            try { missingStore?.Flush(); } catch { }
            try { fallbackStore?.Flush(); } catch { }
            try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
        }
    }

    private static bool StartupRequestLatchChecks(out string report)
    {
        var requests = new StartupRequestLatch();
        var shows = 0;
        var quits = 0;
        Action dispatchShow = () => shows++;
        Action dispatchQuit = () => quits++;

        requests.RequestShow(dispatchShow);
        requests.RequestQuit(dispatchQuit);
        var heldUntilReady = shows == 0 && quits == 0;
        requests.Attach(dispatchShow, dispatchQuit);
        var pendingConsumed = shows == 1 && quits == 1;
        requests.Attach(dispatchShow, dispatchQuit);
        var consumedOnce = shows == 1 && quits == 1;

        requests.RequestShow(dispatchShow);
        requests.RequestQuit(dispatchQuit);
        var readyRequestsDispatched = shows == 2 && quits == 2;

        report = heldUntilReady && pendingConsumed && consumedOnce && readyRequestsDispatched
            ? "early show and quit wait for attach, pending requests dispatch once, and ready requests dispatch directly"
            : $"early={heldUntilReady}, consumed={pendingConsumed}, once={consumedOnce}, ready={readyRequestsDispatched}";
        return heldUntilReady && pendingConsumed && consumedOnce && readyRequestsDispatched;
    }

    private static bool NoteSwipeChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        var settings = new Settings { Swipe = "progress" };
        var pane = new Avalonia.Controls.Control();
        var cancelled = new UI.NoteSwipe(pane, () => settings, () => pane, _ => { });
        var stalled = new UI.NoteSwipe(pane, () => settings, () => pane, _ => { });
        try
        {
            cancelled.Input(1);
            CheckCase(cancelled.OwedMotion > 0.5 && cancelled.HasRunningTimer, "no-frame swipe leaves owed motion and starts settle timer");
            cancelled.Cancel();
            CheckCase(!cancelled.HasRunningTimer && !cancelled.HasPendingProgress && cancelled.OwedMotion == 0,
                "cancel stops settle timer and clears pending progress");

            stalled.Input(1);
            var now = Environment.TickCount64;
            var waits = !stalled.ProgressSettleTick(now) && stalled.HasRunningTimer && stalled.OwedMotion > 0.5;
            CheckCase(waits, "settle waits while owed motion is recent");
            var gaveUp = stalled.ProgressSettleTick(now + UI.NoteSwipe.ProgressStallMs)
                && !stalled.HasRunningTimer && !stalled.HasPendingProgress && stalled.OwedMotion == 0;
            CheckCase(gaveUp, "settle drops stalled owed motion after the bound");

            report = failures.Count == 0 ? "no-frame cancel cleanup and deterministic stalled-settle timeout" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.Message));
            return false;
        }
        finally
        {
            cancelled.Cancel();
            stalled.Cancel();
        }
    }

    private static bool WelcomeSlotsSelfTest(out string report)
    {
        var fresh = new StoreData();
        Welcome.SeedIfNew(fresh, new SettingsService(fresh.Settings));
        var expected = new[] { "# h1", "math", "list", "timer", $"# Welcome to {AppInfo.Name}" };
        var seeded = expected.Select((line, i) => fresh.Slots[i].Text.Split('\n')[0] == line).All(ok => ok)
            && fresh.Slots.Take(5).All(note => !note.IsEmpty)
            && fresh.Slots.Skip(5).All(note => note.IsEmpty)
            && fresh.Stack.Count == 0
            && fresh.ViewSlot == 5;
        var timerLines = fresh.Slots[3].Text.Replace("\r", "").Split('\n');
        var timersParse = timerLines.Length >= 6
            && Enumerable.Range(1, 5).All(i => Timers.TryParse(timerLines[i], out _, out _));

        var occupied = new StoreData();
        occupied.Slots[0].Text = "keep this note\nwith its body";
        Welcome.SeedIfNew(occupied, new SettingsService(occupied.Settings));
        var preserved = occupied.Slots[0].Text == "keep this note\nwith its body";
        var beforeSecondCall = occupied.Slots.Select(note => note.Text).ToArray();
        var viewBeforeSecondCall = occupied.ViewSlot;
        Welcome.SeedIfNew(occupied, new SettingsService(occupied.Settings));
        var secondCallNoOp = occupied.Slots.Select(note => note.Text).SequenceEqual(beforeSecondCall)
            && occupied.ViewSlot == viewBeforeSecondCall;
        var defaults = new Settings() is { Scrolling: "rate", Swipe: "progress" };

        report = "slots 1-5 seeded, 6-9 empty, stack empty, opens slot 5; occupied slot preserved; second call unchanged; five timers parse; rate/progress defaults";
        return seeded && timersParse && preserved && secondCallNoOp && defaults;
    }

    private static bool SearchSpeedChecks(out string report)
    {
        var root = Path.Combine(Path.GetTempPath(), $"antinotepad-search-selftest-{Guid.NewGuid():N}");
        var failures = new List<string>();
        var originalLoader = Note.LoadBody;
        var originalInvalidator = Note.InvalidateSearchBody;
        void CheckCase(bool ok, string name) { if (!ok) failures.Add(name); }
        try
        {
            var store = new Store(root);
            store.Load();
            store.Data.Stack.Clear();
            for (var i = 0; i < 200; i++)
                store.Data.Stack.Add(new Note { Text = $"Fixture {i}\nneedle shared " + new string('x', i % 3 == 0 ? 32768 : i % 3 == 1 ? 4096 : 128) });
            CheckCase(store.Save(), "fixture saves");
            store = new Store(root);
            store.Load();
            var notes = store.Data.Stack.ToArray();
            for (var i = 0; i < notes.Length; i += 2) _ = notes[i].Text;
            var terms = new[] { "needle", "shared" };
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var beforeMatches = notes.Count(n => terms.All(t => n.PeekText().Contains(t, StringComparison.CurrentCultureIgnoreCase)));
            timer.Stop();
            var beforeMs = timer.Elapsed.TotalMilliseconds;
            store.ResetSearchBodyReadCount();
            timer.Restart();
            var snapshot = store.CreateSearchSnapshot(notes);
            var context = store.CreateSearchContext(snapshot);
            timer.Stop();
            var afterMs = timer.Elapsed.TotalMilliseconds;
            CheckCase(store.SearchBodyReadCount == 0, "snapshot performs no disk reads");
            timer.Restart();
            var matches = Task.Run(() => Store.MatchSearch(context, "needle shared", CancellationToken.None)).GetAwaiter().GetResult();
            timer.Stop();
            var coldMs = timer.Elapsed.TotalMilliseconds;
            CheckCase(beforeMatches == 200 && matches.Length == 200 && matches.Any(m => m.Id == notes[0].Id) && matches.Any(m => m.Id == notes[1].Id), "loaded and unloaded matches");
            CheckCase(store.SearchBodyReadCount == 100, "one read per unloaded body for multi-term query and snippets");
            _ = Task.Run(() => Store.MatchSearch(context, "shared", CancellationToken.None)).GetAwaiter().GetResult();
            CheckCase(store.SearchBodyReadCount == 100, "unchanged query reuses body cache");

            notes[1].Text = "edited body\nreplacement";
            var edited = store.CreateSearchContext(store.CreateSearchSnapshot(notes));
            CheckCase(Store.MatchSearch(edited, "replacement", CancellationToken.None).Any(m => m.Id == notes[1].Id)
                && Store.MatchSearch(edited, "needle", CancellationToken.None).All(m => m.Id != notes[1].Id), "in-memory edit invalidates cached text");
            CheckCase(store.Save(), "edited fixture saves");
            notes[1].Detach();
            var saved = store.CreateSearchContext(store.CreateSearchSnapshot(notes));
            CheckCase(Store.MatchSearch(saved, "replacement", CancellationToken.None).Any(m => m.Id == notes[1].Id), "saved edit replaces cached text");

            var lost = notes[3];
            var bodyPath = Path.Combine(root, "notes", lost.Id + ".md");
            File.Delete(bodyPath);
            // A fresh store has no search cache and leaves the unavailable body unloaded.
            store = new Store(root); store.Load();
            lost = store.Data.Stack.Single(n => n.Id == lost.Id);
            var unavailable = store.CreateSearchContext(store.CreateSearchSnapshot([lost]));
            var missing = Store.MatchSearch(unavailable, "Fixture 3", CancellationToken.None);
            var reads = store.SearchBodyReadCount;
            _ = Store.MatchSearch(unavailable, "Fixture 3", CancellationToken.None);
            CheckCase(missing.Length == 1 && missing[0].BodyStatus == BodyReadStatus.Missing && store.SearchBodyReadCount == reads + 1,
                "missing body matches title, retains status, and is not cached as empty");
            CheckCase(!lost.Dirty && store.Save() && !File.Exists(bodyPath), "search does not dirty or save missing body");

            var locked = store.Data.Stack[5];
            var lockedPath = Path.Combine(root, "notes", locked.Id + ".md");
            var originalBody = File.ReadAllText(lockedPath);
            using (var held = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var blocked = store.CreateSearchContext(store.CreateSearchSnapshot([locked]));
                var unreadable = Task.Run(() => Store.MatchSearch(blocked, "Fixture 5", CancellationToken.None)).GetAwaiter().GetResult();
                CheckCase(unreadable.Length == 1 && unreadable[0].BodyStatus == BodyReadStatus.Unreadable && !locked.Dirty && store.Save(), "unreadable body remains distinct and unsaved");
            }
            CheckCase(File.ReadAllText(lockedPath) == originalBody, "unreadable body preserved byte for byte");

            using (var generation = new SearchGeneration())
            using (var first = generation.Begin())
            {
                var oldResults = Store.MatchSearch(saved, "needle", first.Token);
                using var second = generation.Begin();
                var published = Store.MatchSearch(saved, "replacement", second.Token);
                var cancellationObserved = false;
                try { _ = Store.MatchSearch(saved, "needle", first.Token); }
                catch (OperationCanceledException) { cancellationObserved = true; }
                if (generation.IsCurrent(first)) published = oldResults;
                CheckCase(cancellationObserved && !generation.IsCurrent(first) && generation.IsCurrent(second)
                    && published.Length == 1 && published[0].Id == notes[1].Id, "cancelled stale query cannot replace newer results");
                generation.Cancel();
                CheckCase(!generation.IsCurrent(second), "closing invalidates pending publication");
            }
            Console.WriteLine($"A18-search-speed timing: 200 notes (100 unloaded), old UI filter {beforeMs:F3} ms; snapshot/context {afterMs:F3} ms, 0 disk reads; cold background query {coldMs:F3} ms, 100 disk reads.");
            report = failures.Count == 0 ? "loaded/unloaded, read count, cache edit/save, unavailable bodies" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex) { report = string.Join("; ", failures.Append(ex.ToString())); return false; }
        finally
        {
            Note.LoadBody = originalLoader;
            Note.InvalidateSearchBody = originalInvalidator;
            try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
        }
    }

    private static bool DevBadgeBuildStampChecks(out string report)
    {
        var time = new DateTimeOffset(2026, 10, 29, 11, 30, 0, TimeSpan.FromHours(-4));
        var failures = new List<string>();
        void Check(bool ok, string detail) { if (!ok) failures.Add(detail); }
        Check(AppInfo.FormatDevBadge("feat/A54-link-hover-click", time) ==
            ("\uE0A0 A54 · 11:30 AM", "\uE0A0 dev/A54-link-hover-click@2026-10-29 11:30 AM"), "feature format");
        Check(AppInfo.FormatDevBadge("feat/A43b-notes-session", time).Short == "\uE0A0 A43b · 11:30 AM", "letter suffix");
        Check(AppInfo.FormatDevBadge("dev", time) == ("\uE0A0 dev", "\uE0A0 dev"), "dev branch");
        Check(AppInfo.FormatDevBadge("topic-long-branch-name", time) ==
            ("\uE0A0 topic-long-b · 11:30 AM", "\uE0A0 dev/topic-long-branch-name@2026-10-29 11:30 AM"), "other branch");
        Check(AppInfo.FormatDevBadge(null, time) == ("dev", "dev")
            && AppInfo.FormatDevBadge("", time) == ("dev", "dev")
            && AppInfo.FormatDevBadge("feat/A54-link-hover-click", null) == ("dev", "dev"), "missing stamp");
        if (!AppInfo.IsDevBuild)
            Check(!typeof(AppInfo).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .Any(a => a.Key is "AntiNotepadBranch" or "AntiNotepadBuildTime"), "non-dev assembly carries stamp");
        Check(AppInfo.BuildBranch is null || AppInfo.BuildTime is not null, "branch requires timestamp");
        report = failures.Count == 0 ? "formatting and assembly metadata OK" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static bool DevIdentitySelfTest(out string report)
    {
        try
        {
            var expectedName = AppInfo.IsDevBuild ? "AntiNotepad (dev)" : "AntiNotepad";
            var expectedIcon = AppInfo.IsDevBuild ? "antinotepad-dev.ico" : "antinotepad.ico";
            if (AppInfo.DisplayName != expectedName || AppInfo.IconUri != $"avares://AntiNotepad/{expectedIcon}"
                || AppInfo.Name != "AntiNotepad" || AppInfo.InstanceId != "AntiNotepad"
                || AppInfo.DevInstanceId != "AntiNotepad-dev" || AppInfo.DataFolderName != "AntiNotepad"
                || AppInfo.RunValueName != "AntiNotepad" || AppInfo.LogFileName != "antinotepad.log"
                || AppInfo.EnvPrefix != "ANTINOTEPAD" || AppInfo.ResourceUri != "avares://AntiNotepad/")
            {
                report = "display identity or stable storage/resource identity differs from the build contract";
                return false;
            }

            var assets = new StandardAssetLoader();
            foreach (var file in new[] { "antinotepad.ico", "antinotepad-dev.ico" })
            {
                using var stream = assets.Open(new Uri($"avares://AntiNotepad/{file}"));
                using var reader = new BinaryReader(stream);
                if (reader.ReadUInt16() != 0 || reader.ReadUInt16() != 1 || reader.ReadUInt16() == 0)
                    throw new InvalidOperationException($"Invalid icon: {file}");
            }
            report = $"{expectedName}, {AppInfo.IconUri}; both icons load; stable instance/data/resource identities";
            Console.WriteLine("IDENTITY " + report);
            return true;
        }
        catch (Exception ex)
        {
            report = ex.Message;
            return false;
        }
    }

    private static bool TaskbarChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        CheckCase(new Settings().Taskbar == "never", "new settings default to never");
        CheckCase(System.Text.Json.JsonSerializer.Deserialize<Settings>("{}")?.Taskbar == "never", "old JSON defaults to never");
        foreach (var setting in new[] { "never", "visible", "always" })
        {
            var json = System.Text.Json.JsonSerializer.Serialize(new StoreData { Settings = new Settings { Taskbar = setting } });
            CheckCase(System.Text.Json.JsonSerializer.Deserialize<StoreData>(json)?.Settings.Taskbar == setting, $"{setting} JSON round trip");
        }
        (string Setting, bool Visible, bool Expected)[] cases =
        [
            ("never", false, false), ("never", true, false),
            ("visible", false, false), ("visible", true, true),
            ("always", false, true), ("always", true, true),
        ];
        foreach (var test in cases)
            CheckCase(TaskbarPresence.ShouldShow(test.Setting, test.Visible) == test.Expected, $"{test.Setting}, visible={test.Visible}");
        foreach (var setting in new string?[] { null, "unknown" })
            foreach (var visible in new[] { false, true })
                CheckCase(!TaskbarPresence.ShouldShow(setting, visible), "missing/unknown mode has no button");
        CheckCase(!TaskbarPresence.ShouldHideOnClick(false, false), "hidden inactive pad click shows");
        CheckCase(!TaskbarPresence.ShouldHideOnClick(false, true), "hidden pad click shows even with stale foreground state");
        CheckCase(!TaskbarPresence.ShouldHideOnClick(true, false), "visible inactive pad click focuses");
        CheckCase(TaskbarPresence.ShouldHideOnClick(true, true), "visible foreground pad click hides");
        var lastLoss = 100L;
        var withinGrace = lastLoss + System.Diagnostics.Stopwatch.Frequency / 2;
        var outsideGrace = withinGrace + 1;
        CheckCase(TaskbarPresence.WasPadRecentlyForeground(lastLoss, withinGrace, TaskbarActivationCounterpart.Taskbar), "recent loss to taskbar counts as foreground");
        CheckCase(TaskbarPresence.WasPadRecentlyForeground(lastLoss, withinGrace, TaskbarActivationCounterpart.Proxy), "recent loss to proxy counts as foreground");
        CheckCase(TaskbarPresence.WasPadRecentlyForeground(lastLoss, withinGrace, TaskbarActivationCounterpart.None), "null counterpart counts through 500 ms");
        CheckCase(!TaskbarPresence.WasPadRecentlyForeground(lastLoss, withinGrace, TaskbarActivationCounterpart.Other), "recent loss to another window does not count");
        CheckCase(!TaskbarPresence.WasPadRecentlyForeground(lastLoss, outsideGrace, TaskbarActivationCounterpart.Taskbar), "taskbar loss expires after 500 ms");
        CheckCase(!TaskbarPresence.WasPadRecentlyForeground(withinGrace, lastLoss, TaskbarActivationCounterpart.Taskbar), "future foreground-loss time is rejected");
        var firstProxyMessage = lastLoss + System.Diagnostics.Stopwatch.Frequency * 17 / 1000;
        var snapshotPadWasForeground = TaskbarPresence.WasPadRecentlyForeground(lastLoss, firstProxyMessage, TaskbarActivationCounterpart.None);
        CheckCase(TaskbarPresence.ShouldHideOnClick(true, snapshotPadWasForeground), "null counterpart loss 17 ms before first proxy message hides visible pad");
        CheckCase(!TaskbarPresence.ShouldHideOnClick(false, snapshotPadWasForeground), "hidden pad still shows after recent null counterpart loss");
        var oldNullLoss = TaskbarPresence.WasPadRecentlyForeground(lastLoss, outsideGrace, TaskbarActivationCounterpart.None);
        CheckCase(!TaskbarPresence.ShouldHideOnClick(true, oldNullLoss), "old null counterpart loss leaves visible inactive pad to focus");
        CheckCase(!TaskbarPresence.ShouldHideOnClick(false, oldNullLoss), "old null counterpart loss leaves hidden pad to show");
        var hideDecision = 200L;
        var withinHideFallback = hideDecision + System.Diagnostics.Stopwatch.Frequency;
        var afterHideFallback = withinHideFallback + 1;
        CheckCase(TaskbarPresence.ShouldSuppressPostHideActivation(true, true, false, false, hideDecision, withinHideFallback), "post-hide activation fallback is suppressed");
        CheckCase(!TaskbarPresence.ShouldSuppressPostHideActivation(true, true, true, false, hideDecision, withinHideFallback), "fresh click start permits a new hidden-pad click");
        CheckCase(!TaskbarPresence.ShouldSuppressPostHideActivation(true, true, false, true, hideDecision, withinHideFallback), "explicit restore permits a new hidden-pad click");
        CheckCase(!TaskbarPresence.ShouldSuppressPostHideActivation(true, true, false, false, hideDecision, afterHideFallback), "post-hide suppression expires after one second");
        CheckCase(!TaskbarPresence.ShouldSuppressPostHideActivation(true, false, false, false, hideDecision, withinHideFallback), "focus decision does not suppress a later click");
        report = failures.Count == 0 ? "Never default, old JSON default, three JSON round trips, all six visibility combinations, missing/unknown modes, all four click states, recent foreground-loss attribution including null counterpart at 17 ms and expiry, and post-hide activation-fallback suppression" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static bool SessionEndCloseChecks(out string report)
    {
        var failures = new List<string>();
        foreach (var reason in Enum.GetValues<Avalonia.Controls.WindowCloseReason>())
        {
            // Covers pad X/Alt+F4 and stand-in closes, explicit quit/disposal, shutdown/restart,
            // and sign-out (Avalonia 12.1.4 uses ApplicationShutdown for nonzero end-session flags).
            if (SessionEnd.ShouldCancelClose(reason, exiting: false, sessionEnding: false)
                != (reason != Avalonia.Controls.WindowCloseReason.OSShutdown)) failures.Add($"ordinary {reason}");
            if (SessionEnd.ShouldCancelClose(reason, exiting: false, sessionEnding: true)) failures.Add($"session end {reason}");
            if (SessionEnd.ShouldCancelClose(reason, exiting: true, sessionEnding: false)) failures.Add($"quit/dispose {reason}");
            if (SessionEnd.ShouldCancelClose(reason, exiting: true, sessionEnding: true)) failures.Add($"quit during session end {reason}");
        }
        report = failures.Count == 0 ? "normal resident closes cancel; quit, OS shutdown and session-end sign-out closes proceed" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static bool SettingsSearchMatchingChecks(out string report)
    {
        var matchedCaseInsensitively = UI.SettingsWindow.SearchMatches("sub Light mode palette", "PALETTE");
        var ignoredPrefix = !UI.SettingsWindow.SearchMatches("sub Light mode palette", "sub");
        var nullQueryMatches = UI.SettingsWindow.SearchMatches(null, null);
        var passed = matchedCaseInsensitively && ignoredPrefix && nullQueryMatches;
        report = passed ? "case-insensitive match, ignored sub prefix, and null query" : "search match semantics changed";
        return passed;
    }

    private static bool FenceCacheChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        try
        {
            var palette = Theme.Palette.All[0];
            var doc = new AvaloniaEdit.Document.TextDocument("```cs\nint first = 1;\n```\n\n```js\nlet second = 2;\n```");
            var original = Editor.NoteStructure.Build(["```cs", "int first = 1;", "```", "", "```js", "let second = 2;", "```"]);
            var cache = new Editor.FenceCache<object>();
            var copies = 0;
            var builds = 0;
            object Create(string _) { builds++; return new object(); }
            string Copy(string text) { copies++; return text; }

            foreach (var fence in original.Fences)
            {
                var body = fence.Language == "cs" ? "int first = 1;\n" : "let second = 2;\n";
                cache.Get(fence.Open, fence.Close - 1, fence.Closed, doc.Version, fence.Language, palette, () => Copy(body), Create);
            }
            var first = cache.Get(1, 2, true, doc.Version, "cs", palette, () => Copy("int first = 1;\n"), Create);
            for (var i = 0; i < 100; i++)
                CheckCase(ReferenceEquals(first, cache.Get(1, 2, true, doc.Version, "cs", palette, () => Copy("int first = 1;\n"), Create)), "unchanged fence reuses its cached highlight");
            CheckCase(copies == 2 && builds == 2, "one body copy for each unchanged fence across repeated lines");

            var sameBodyNewVersion = new AvaloniaEdit.Document.TextDocument(doc.Text);
            for (var i = 0; i < 100; i++)
                cache.Get(1, 2, true, sameBodyNewVersion.Version, "cs", palette, () => Copy("int first = 1;\n"), Create);
            CheckCase(copies == 3 && builds == 2, "one body copy after a document version change with unchanged fence text");

            var boundsMissed = !cache.TryGet(1, 3, true, sameBodyNewVersion.Version, "cs", palette, out _);
            var changedBounds = cache.Get(1, 3, true, sameBodyNewVersion.Version, "cs", palette,
                () => Copy("int first = 1;\nextra line\n"), Create);
            CheckCase(boundsMissed && !ReferenceEquals(first, changedBounds) && copies == 4 && builds == 3,
                "same-version fence with changed body bounds misses and rebuilds");

            var moved = Editor.NoteStructure.Build(["intro", "```cs", "int first = 1;", "```"]);
            cache.Prune(moved.Fences.Select(f => f.Open));
            CheckCase(cache.Count == 0, "moved and deleted fence keys are pruned");
            var movedDoc = new AvaloniaEdit.Document.TextDocument("intro\n```cs\nint first = 1;\n```");
            cache.Get(moved.Fences[0].Open, 3, moved.Fences[0].Closed, movedDoc.Version, moved.Fences[0].Language, palette,
                () => Copy("int first = 1;\n"), Create);
            cache.Prune(moved.Fences.Select(f => f.Open));
            CheckCase(cache.Count == moved.Fences.Count && cache.TryGet(2, 3, true, movedDoc.Version, "cs", palette, out _)
                && !cache.TryGet(1, 2, true, movedDoc.Version, "cs", palette, out _) && !cache.TryGet(5, 3, true, movedDoc.Version, "js", palette, out _),
                "cache contains only the current moved fence");

            var pens = new Editor.ChevronPens();
            var firstPens = pens.Get(1, palette);
            var reusedPens = pens.Get(1, palette);
            var changedPalettePens = pens.Get(1, Theme.Palette.All[1]);
            var changedScalePens = pens.Get(1.25, Theme.Palette.All[1]);
            CheckCase(ReferenceEquals(firstPens, reusedPens) && !ReferenceEquals(firstPens, changedPalettePens)
                && !ReferenceEquals(changedPalettePens, changedScalePens), "chevron pens reuse by palette and scale");

            var brushes = new Editor.ColorizerBrushCache();
            var firstBrush = brushes.Get(palette.Keyword);
            var reusedBrush = brushes.Get(palette.Keyword);
            var changedBrush = brushes.Get(Theme.Palette.All[1].Keyword);
            CheckCase(ReferenceEquals(firstBrush, reusedBrush) && !ReferenceEquals(firstBrush, changedBrush),
                "colorizer brush reuses the same color and refreshes on palette color change");

            report = failures.Count == 0
                ? "fence cache pruning and body-copy reuse, plus chevron and colorizer brush resource reuse"
                : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex) { report = string.Join("; ", failures.Append(ex.ToString())); return false; }
    }

    private static bool IncrementalAnalysisChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        static string EvalSignature(Calc.LineEval? line)
        {
            if (line is null) return "null";
            var result = line.Result is { } value
                ? $"{value.Display}|{value.Value.ToString("R", System.Globalization.CultureInfo.InvariantCulture)}|{value.Unit}"
                : "-";
            var tokens = string.Join(",", line.Tokens.Select(t => $"{t.Start}:{t.Length}:{t.Kind}"));
            return $"{result}|{line.ErrorReason}|{line.AssignedName}|{tokens}";
        }

        static bool Equivalent(Editor.NoteAnalysis incremental, Editor.NoteAnalysis full, AvaloniaEdit.Document.TextDocument document)
        {
            var structureA = incremental.Structure;
            var structureB = full.Structure;
            if (incremental.Mode != full.Mode || incremental.CodeLanguage != full.CodeLanguage
                || incremental.ModeKeywordLength != full.ModeKeywordLength
                || !incremental.Lines.SequenceEqual(full.Lines)
                || structureA is null || structureB is null
                || !structureA.Fences.SequenceEqual(structureB.Fences)
                || !structureA.Indent.SequenceEqual(structureB.Indent)
                || !structureA.InFence.SequenceEqual(structureB.InFence)
                || incremental.MathLines.Count != full.MathLines.Count
                || !incremental.MathLines.Select(EvalSignature).SequenceEqual(full.MathLines.Select(EvalSignature))) return false;

            var inlineA = incremental.InlineLines.OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}:{EvalSignature(pair.Value)}");
            var inlineB = full.InlineLines.OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}:{EvalSignature(pair.Value)}");
            if (!inlineA.SequenceEqual(inlineB)) return false;

            var timersA = incremental.TimerKeys.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value}");
            var timersB = full.TimerKeys.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value}");
            if (!timersA.SequenceEqual(timersB)) return false;

            var foldsA = structureA.Folds(document, incremental.Lines, incremental.Mode == NoteMode.Code)
                .Select(fold => (fold.StartOffset, fold.EndOffset, fold.Name)).ToArray();
            var foldsB = structureB.Folds(document, full.Lines, full.Mode == NoteMode.Code)
                .Select(fold => (fold.StartOffset, fold.EndOffset, fold.Name)).ToArray();
            return foldsA.SequenceEqual(foldsB);
        }

        static Editor.NoteAnalysis Full(AvaloniaEdit.Document.TextDocument document, bool inlineMath, string? noteId)
        {
            var full = new Editor.NoteAnalysis();
            full.Update(document, 0, document.LineCount - 1, inlineMath, noteId, timers: null, forceFull: true);
            return full;
        }

        static (int First, int Last) Around(AvaloniaEdit.Document.TextDocument document, int lineNumber) =>
            (Math.Max(0, lineNumber - 2), Math.Min(document.LineCount - 1, lineNumber));

        static int FindLine(AvaloniaEdit.Document.TextDocument document, string text)
        {
            for (var number = 1; number <= document.LineCount; number++)
                if (document.GetText(document.GetLineByNumber(number)) == text) return number;
            return 0;
        }

        try
        {
            var document = new AvaloniaEdit.Document.TextDocument(string.Join('\n', new[]
            {
                "# Start", "intro", "```cs", "int value = 1;", "```", "## After",
                "- item one", "  details", "- item two", "total 2 + 3 ="
            }));
            var analysis = new Editor.NoteAnalysis();
            analysis.Update(document, 0, document.LineCount - 1, inlineMath: true, noteId: "a29-note", timers: null, forceFull: true);
            CheckCase(analysis.LastCopiedLineCount == document.LineCount, "initial analysis copies each document line once");
            CheckCase(analysis.InlineLines.Count == 1, "plain note retains its inline result");

            bool Refresh(AvaloniaEdit.Document.TextDocument target, Editor.NoteAnalysis current,
                int first, int last, bool inlineMath, string? noteId, string name)
            {
                current.Invalidate(target, first, last);
                current.Update(target, first, last, inlineMath, noteId, timers: null);
                var equivalent = Equivalent(current, Full(target, inlineMath, noteId), target);
                CheckCase(equivalent, name);
                return equivalent;
            }

            var body = document.GetLineByNumber(4);
            document.Replace(body.Offset, body.Length, "int value = 2;");
            var range = Around(document, 4);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "fence body edit matches full analysis");

            var opening = document.GetLineByNumber(3);
            document.Replace(opening.Offset, opening.Length, "```py");
            range = Around(document, 3);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "fence opening edit matches full analysis");

            var closing = document.GetLineByNumber(5);
            document.Replace(closing.Offset, closing.Length, "tail");
            range = Around(document, 5);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "open fence edit matches full analysis");

            closing = document.GetLineByNumber(5);
            document.Replace(closing.Offset, closing.Length, "```");
            range = Around(document, 5);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "fence close edit matches full analysis");

            document.Insert(0, "## Inserted\n");
            Refresh(document, analysis, 0, 2, true, "a29-note", "heading insertion matches full analysis");
            var insertedHeading = document.GetLineByNumber(1);
            document.Remove(insertedHeading.Offset, insertedHeading.TotalLength);
            Refresh(document, analysis, 0, Math.Min(2, document.LineCount - 1), true, "a29-note", "heading deletion matches full analysis");

            var detailsLine = FindLine(document, "  details");
            if (detailsLine == 0) throw new InvalidOperationException("list child fixture was lost");
            var details = document.GetLineByNumber(detailsLine);
            document.Replace(details.Offset, details.Length, "details");
            range = Around(document, detailsLine);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "list run edit matches full analysis");

            var mathDocument = new AvaloniaEdit.Document.TextDocument("math\nrate = 10\ncost = rate * 3\nlunch 2 + 4");
            var mathAnalysis = new Editor.NoteAnalysis();
            mathAnalysis.Update(mathDocument, 0, mathDocument.LineCount - 1, inlineMath: true, noteId: "a29-math", timers: null, forceFull: true);
            var rate = mathDocument.GetLineByNumber(2);
            mathDocument.Replace(rate.Offset, rate.Length, "rate = 12");
            Refresh(mathDocument, mathAnalysis, 1, 1, true, "a29-math", "later variable use matches full two-pass math analysis");
            CheckCase(mathAnalysis.MathLines[2]?.Result?.Value == 36, "later math line reflects the edited variable");

            var timerDocument = new AvaloniaEdit.Document.TextDocument("timer\n5m tea\n1m water\n5m coffee");
            var timerAnalysis = new Editor.NoteAnalysis();
            timerAnalysis.Update(timerDocument, 0, timerDocument.LineCount - 1, inlineMath: true, noteId: "a29-timers", timers: null, forceFull: true);
            var water = timerDocument.GetLineByNumber(3);
            timerDocument.Replace(water.Offset, water.Length, "5m water");
            range = Around(timerDocument, 3);
            Refresh(timerDocument, timerAnalysis, range.First, range.Last, true, "a29-timers", "timer duration edit matches full parsing");
            CheckCase(timerAnalysis.TimerKeys.TryGetValue(2, out var firstTimer) && firstTimer == "a29-timers:5m:1"
                && timerAnalysis.TimerKeys.TryGetValue(3, out var secondTimer) && secondTimer == "a29-timers:5m:2"
                && timerAnalysis.TimerKeys.TryGetValue(4, out var thirdTimer) && thirdTimer == "a29-timers:5m:3",
                "duplicate timer ordinals remain in document order");

            var burstDocument = new AvaloniaEdit.Document.TextDocument(string.Join("\n",
                Enumerable.Range(0, 5000).Select(index => $"ordinary line {index}")));
            var burst = new Editor.NoteAnalysis();
            burst.Update(burstDocument, 0, burstDocument.LineCount - 1, inlineMath: true, noteId: "a29-burst", timers: null, forceFull: true);
            var firstEdit = burstDocument.GetLineByNumber(2500);
            burstDocument.Replace(firstEdit.Offset, firstEdit.Length, "ordinary line 2499 edited");
            var firstRange = Around(burstDocument, 2500);
            burst.Invalidate(burstDocument, firstRange.First, firstRange.Last);
            var secondEdit = burstDocument.GetLineByNumber(2501);
            burstDocument.Replace(secondEdit.Offset, secondEdit.Length, "ordinary line 2500 edited");
            var secondRange = Around(burstDocument, 2501);
            burst.Invalidate(burstDocument, secondRange.First, secondRange.Last);
            var unionFirst = Math.Min(firstRange.First, secondRange.First);
            var unionLast = Math.Max(firstRange.Last, secondRange.Last);
            burst.Update(burstDocument, unionFirst, unionLast, inlineMath: true, noteId: "a29-burst", timers: null);
            CheckCase(burst.LastCopiedLineCount <= 8 && burst.LastAnalysedLineCount < 32,
                $"5,000-line burst copies {burst.LastCopiedLineCount} and analyses {burst.LastAnalysedLineCount} lines");
            CheckCase(Equivalent(burst, Full(burstDocument, true, "a29-burst"), burstDocument),
                "multiple same-count edits match full analysis");

            report = failures.Count == 0
                ? "fence, heading, list, math, timer, inline and fold equivalence; two-edit 5,000-line burst stayed bounded"
                : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.ToString()));
            return false;
        }
    }

    private static bool BackgroundSaveAndLogChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-background-save-selftest-{Guid.NewGuid():N}");
        try
        {
            var burstPath = Path.Combine(root, "burst");
            var burstStore = new Store(burstPath);
            burstStore.Load();
            CheckCase(burstStore.Save(), "baseline save for coalescing");
            var baselineIndex = File.ReadAllBytes(Path.Combine(burstPath, "index.json"));
            var note = burstStore.Data.Stack[0];
            Task<bool>? firstBatch = null;
            Task<bool>? lastBatch = null;
            string lastText = "";
            for (var i = 0; i < 40; i++)
            {
                lastText = $"queued snapshot {i}\nbody {i}";
                note.Text = lastText;
                var queued = burstStore.QueueSave();
                firstBatch ??= queued;
                lastBatch = queued;
            }
            var burstFlush = burstStore.Flush();
            var burstResult = lastBatch?.GetAwaiter().GetResult() ?? false;
            var burstReloaded = new Store(burstPath);
            burstReloaded.Load();
            var backupPath = Path.Combine(burstPath, "index.json.bak");
            CheckCase(firstBatch is not null && ReferenceEquals(firstBatch, lastBatch), "rapid queue burst coalesces into one task");
            CheckCase(burstFlush && burstResult && burstReloaded.Data.Stack[0].Text == lastText, "flush persists the burst's last snapshot");
            CheckCase(File.Exists(backupPath) && File.ReadAllBytes(backupPath).SequenceEqual(baselineIndex), "coalesced burst writes one index revision");

            var flushPath = Path.Combine(root, "flush");
            var flushStore = new Store(flushPath);
            flushStore.Load();
            flushStore.Data.Stack[0].Text = "queued then synchronously flushed";
            var queuedFlush = flushStore.QueueSave();
            var synchronousFlush = flushStore.Flush();
            var flushResult = queuedFlush.GetAwaiter().GetResult();
            var flushReloaded = new Store(flushPath);
            flushReloaded.Load();
            CheckCase(synchronousFlush && flushResult && flushReloaded.Data.Stack[0].Text == "queued then synchronously flushed",
                "Flush drains a queued save before returning");

            var livePath = Path.Combine(root, "live-note");
            var liveStore = new Store(livePath);
            liveStore.Load();
            var liveNote = liveStore.Data.Stack[0];
            var liveText = "first live value";
            liveNote.Attach(() => liveText);
            liveNote.Touched(blank: false, NoteMode.Plain);
            var liveQueued = liveStore.QueueSave();
            liveText = "latest live value before quit";
            liveNote.Touched(blank: false, NoteMode.Plain);
            var liveSaved = liveStore.Save();
            var liveQueueResult = liveQueued.GetAwaiter().GetResult();
            var liveReloaded = new Store(livePath);
            liveReloaded.Load();
            CheckCase(liveSaved && liveQueueResult && liveReloaded.Data.Stack[0].Text == liveText && liveNote.TitleCache == liveText,
                "synchronous Save replaces a pending snapshot with the latest live editor text");

            var settingsPath = Path.Combine(root, "snapshot");
            var settingsStore = new Store(settingsPath);
            settingsStore.Load();
            settingsStore.Data.Settings.DisabledLanguages = ["before-queue"];
            var settingsQueued = settingsStore.QueueSave();
            settingsStore.Data.Settings.DisabledLanguages.Add("after-queue");
            var settingsFlushed = settingsStore.Flush();
            var settingsResult = settingsQueued.GetAwaiter().GetResult();
            var settingsReloaded = new Store(settingsPath);
            settingsReloaded.Load();
            CheckCase(settingsFlushed && settingsResult
                && settingsReloaded.Data.Settings.DisabledLanguages.SequenceEqual(["before-queue"]),
                "queued save persists its immutable settings snapshot");

            var blockedPath = Path.Combine(root, "blocked");
            var blockedStore = new Store(blockedPath);
            blockedStore.Load();
            blockedStore.Data.Stack[0].Text = "write must fail";
            Directory.CreateDirectory(Path.Combine(blockedPath, "index.json"));
            var blockedQueued = blockedStore.QueueSave();
            var blockedFlush = blockedStore.Flush();
            var blockedQueueResult = blockedQueued.GetAwaiter().GetResult();
            var quitSaveGate = blockedStore.Save();
            CheckCase(!blockedFlush && !blockedQueueResult && !quitSaveGate,
                "blocked index write returns failure through queue, flush, and synchronous Save gate");
            Directory.Delete(Path.Combine(blockedPath, "index.json"));
            var retrySaved = blockedStore.Save();
            var retryReloaded = new Store(blockedPath);
            retryReloaded.Load();
            CheckCase(retrySaved && retryReloaded.Data.Stack[0].Text == "write must fail",
                "retry after removing the index obstruction saves the dirty note");

            var unavailablePath = Path.Combine(root, "unavailable");
            var unavailableStore = new Store(unavailablePath);
            unavailableStore.Load();
            var unavailableNote = unavailableStore.Data.Stack[0];
            unavailableNote.Text = "kept dirty while unavailable";
            var unavailableFilesBefore = Directory.GetFiles(unavailablePath, "*", SearchOption.AllDirectories).Order().ToArray();
            unavailableStore.MarkUnavailable("fixture store unavailable");
            var unavailableQueued = unavailableStore.QueueSave().GetAwaiter().GetResult();
            var unavailableSaved = unavailableStore.Save();
            var unavailableFlushed = unavailableStore.Flush();
            var unavailableFilesAfter = Directory.GetFiles(unavailablePath, "*", SearchOption.AllDirectories).Order().ToArray();
            CheckCase(unavailableQueued && unavailableSaved && unavailableFlushed
                && unavailableStore.LastError is not null && unavailableNote.Dirty
                && unavailableFilesBefore.SequenceEqual(unavailableFilesAfter),
                "unavailable existing folder makes QueueSave, Save, and Flush successful no-ops without clearing dirty text");

            var missingPath = Path.Combine(root, "missing-folder");
            var missingStore = new Store(missingPath);
            missingStore.Load();
            var missingNote = missingStore.Data.Stack[0];
            missingNote.Text = "kept dirty while folder is missing";
            var missingInitiallyAvailable = missingStore.IsAvailable;
            Directory.Delete(missingPath, recursive: true);
            var missingQueued = missingStore.QueueSave().GetAwaiter().GetResult();
            var missingSaved = missingStore.Save();
            var missingFlushed = missingStore.Flush();
            CheckCase(missingStore.IsAvailable == missingInitiallyAvailable && missingQueued && missingSaved && missingFlushed
                && missingStore.LastError is not null && missingNote.Dirty && !Directory.Exists(missingPath),
                "missing folder makes QueueSave, Save, and Flush successful no-ops without changing availability or clearing dirty text");
            Directory.CreateDirectory(missingPath);
            var missingRecoveredSaved = missingStore.Save();
            var missingReloaded = new Store(missingPath);
            missingReloaded.Load();
            CheckCase(missingRecoveredSaved && missingStore.IsAvailable == missingInitiallyAvailable
                && !missingNote.Dirty && missingReloaded.Data.Stack[0].Text == "kept dirty while folder is missing",
                "Save persists retained dirty text after the missing folder returns");

            var switchSourcePath = Path.Combine(root, "failed-switch-source");
            var switchStore = new Store(switchSourcePath);
            switchStore.Load();
            var switchNote = switchStore.Data.Stack[0];
            switchNote.Text = "retained after failed folder write";
            Directory.CreateDirectory(Path.Combine(switchSourcePath, "index.json"));
            var switchFailed = !switchStore.Save();
            var switchPath = Path.Combine(root, "fresh-folder");
            Directory.CreateDirectory(switchPath);
            switchStore.SwitchFolder(switchPath);
            var switchDirty = switchNote.Dirty;
            var switchedSaved = switchStore.Save();
            var switchedReloaded = new Store(switchPath);
            switchedReloaded.Load();
            CheckCase(switchFailed && switchDirty && switchedSaved && switchedReloaded.Data.Stack[0].Text == "retained after failed folder write"
                && File.Exists(Path.Combine(switchPath, "index.json")),
                "SwitchFolder drains a prior failed write and saves subsequent edits in the fresh folder");

            var logPath = Path.Combine(root, "logger", AppInfo.LogFileName);
            using (var logger = new BackgroundLog(logPath))
            {
                const int lines = 64;
                for (var i = 0; i < lines; i++) logger.Write($"ordered-{i:D3}");
                logger.Flush();
                var written = File.ReadAllLines(logPath);
                var orderedAndTimestamped = written.Length == lines && Enumerable.Range(0, lines).All(i =>
                    written[i].Length >= 20 && written[i][19] == ' '
                    && DateTime.TryParseExact(written[i][..19], "s", System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out _)
                    && written[i].EndsWith($"ordered-{i:D3}", StringComparison.Ordinal));
                CheckCase(orderedAndTimestamped, "logger Flush writes captured timestamps and entries in queue order");
            }

            report = failures.Count == 0
                ? "40 queued snapshots coalesce to the last index revision; queued Flush drains; unavailable saves are successful no-ops; blocked writes fail the synchronous Save gate; folder switching recovers after a failed write; logger Flush preserves timestamped order"
                : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.ToString()));
            return false;
        }
        finally { try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { } }
    }
}
