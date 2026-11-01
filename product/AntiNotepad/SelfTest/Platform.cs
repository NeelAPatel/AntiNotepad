using AntiNotepad.Model;
using AntiNotepad.Platform;

namespace AntiNotepad;

internal static partial class SelfTest
{
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
}
