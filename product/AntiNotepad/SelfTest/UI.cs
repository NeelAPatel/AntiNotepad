// Capability checks for settings notifications, note gestures, and transition cleanup.
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using AntiNotepad.Editor;
using AntiNotepad.Model;
using AntiNotepad.Motion;
using AntiNotepad.Theme;

namespace AntiNotepad;

internal static partial class SelfTest
{
    /// <summary>Checks the Windows motion curves at endpoints and throughout their range.</summary>
    private static bool MotionCurveChecks(out string report)
    {
        var enter = MotionTokens.Win11Enter;
        var exit = MotionTokens.Win11Exit;
        bool Monotonic(Func<double, double> curve) => Enumerable.Range(1, 1000)
            .All(i => curve(i / 1000.0) >= curve((i - 1) / 1000.0));
        var passed = enter(0) == 0 && enter(1) == 1 && exit(0) == 0 && exit(1) == 1
            && Monotonic(enter) && Monotonic(exit) && Math.Abs(enter(0.1) - 0.4463304) < 0.00001 && exit(0.1) < 0.1;
        report = passed ? "endpoints, monotonicity and start speeds" : "motion curve check failed";
        return passed;
    }

    /// <summary>Checks the real ring's state, palette, geometry, and input ownership without showing a window.</summary>
    private static bool PinRingChecks(out string report)
    {
        if (Application.Current is null) AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();
        if (!Application.Current!.Styles.OfType<Avalonia.Themes.Fluent.FluentTheme>().Any())
        {
            Application.Current.Styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
            Application.Current.Styles.Add(new StyleInclude(new Uri(AppInfo.ResourceUri)) { Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml") });
        }
        var failures = new List<string>();
        void Expect(bool ok, string name) { if (!ok) failures.Add(name); }
        Expect(MotionCurveChecks(out _), "Windows motion curves");
        var folder = Path.Combine(Path.GetTempPath(), "AntiNotepad-ring-" + Guid.NewGuid().ToString("N"));
        using var log = new RuntimeLog();
        var reduced = Anim.Reduced;
        try
        {
            var palette = Palette.All[0];
            var store = new Store(Path.Combine(folder, palette.Id));
            store.Load();
            var settings = store.SettingsService;
            settings.Current.Theme = palette.Dark ? "dark" : "light";
            if (palette.Dark) settings.Current.DarkPalette = palette.Id; else settings.Current.LightPalette = palette.Id;
            Welcome.SeedIfNew(store.Data, settings);
            var notes = new NotesSession(store, folder, folder, null, log.Write);
            var pad = new UI.PadWindow(store, settings, log, false, true, notes);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            var ring = (Border)typeof(UI.PadWindow).GetField("_pinRing", fields)!.GetValue(pad)!;
            Expect(UI.PadWindow.PinRingOpacity(false) == 0
                && UI.PadWindow.PinRingOpacity(true) == Paint.FocusBorder, "target opacity");
            Expect(!ring.IsHitTestVisible && ring.Opacity == 0, "initial state and input pass-through");
            Expect(ring.BorderThickness == new Thickness(ChromeTokens.PinRingThickness)
                && ring.CornerRadius == new CornerRadius(Shape.Card), "ring geometry");
            foreach (var pinned in new[] { true, false, true, false })
            {
                settings.Current.Pinned = pinned;
                settings.Current.ReduceMotion = true;
                pad.ApplyAppearance();
                Expect(ring.Opacity == UI.PadWindow.PinRingOpacity(pinned), "reduced-motion toggle settles");
                Expect(ring.BorderBrush is SolidColorBrush brush && brush.Color == pad.CurrentPalette.Accent,
                    "palette accent brush");
            }
            settings.Current.Chrome = "fade";
            settings.Changed += pad.HandleSettingsChanged;
            var icons = (Control)typeof(UI.PadWindow).GetField("_icons", fields)!.GetValue(pad)!;
            var pill = (Control)typeof(UI.PadWindow).GetField("_pill", fields)!.GetValue(pad)!;
            var awake = typeof(UI.PadWindow).GetField("_chromeAwake", fields)!;
            var iconsOpacity = icons.Opacity;
            var pillOpacity = pill.Opacity;
            settings.Update(s => s.Pinned = true);
            settings.Update(s => s.DimWhenPinned = !s.DimWhenPinned);
            Expect((bool)awake.GetValue(pad)! && icons.Opacity == iconsOpacity && pill.Opacity == pillOpacity,
                "pin settings preserve awake chrome");
            var keyDown = typeof(UI.PadWindow).GetMethod("OnWindowKeyDown", fields)!;
            keyDown.Invoke(pad, [null, new KeyEventArgs { Key = Key.T, KeyModifiers = KeyModifiers.Control }]);
            Expect((bool)awake.GetValue(pad)! && icons.Opacity == iconsOpacity && pill.Opacity == pillOpacity,
                "pin shortcut preserves awake chrome");
            typeof(UI.PadWindow).GetMethod("Edited", fields)!.Invoke(pad, [true]);
            Expect(!(bool)awake.GetValue(pad)!, "normal edit sleeps chrome");
            iconsOpacity = icons.Opacity;
            pillOpacity = pill.Opacity;
            settings.Update(s => { s.Pinned = !s.Pinned; s.DimWhenPinned = !s.DimWhenPinned; });
            Expect(!(bool)awake.GetValue(pad)! && icons.Opacity == iconsOpacity && pill.Opacity == pillOpacity,
                "pin settings preserve sleeping chrome");
            report = failures.Count == 0 ? "ring and pin chrome visibility" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        finally
        {
            Anim.Reduced = reduced;
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
        }
    }

    /// <summary>Checks every supported recorder key against the unchanged platform parser.</summary>
    private static bool HotkeyRecorderChecks(out string report)
    {
        var failures = new List<string>();
        void Expect(bool ok, string name) { if (!ok) failures.Add(name); }
        const Avalonia.Input.KeyModifiers mods = Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Alt;
        var keys = Enumerable.Range((int)Avalonia.Input.Key.A, 26)
            .Concat(Enumerable.Range((int)Avalonia.Input.Key.D0, 10))
            .Concat(Enumerable.Range((int)Avalonia.Input.Key.F1, 24))
            .Select(value => (Avalonia.Input.Key)value)
            .Concat([Avalonia.Input.Key.Space, Avalonia.Input.Key.Escape, Avalonia.Input.Key.Tab,
                Avalonia.Input.Key.Enter, Avalonia.Input.Key.Insert, Avalonia.Input.Key.Delete]).ToArray();
        foreach (var key in keys)
        {
            var text = UI.HotkeyRecorder.GestureText(key, mods);
            Expect(text is not null && Platform.GlobalHotkey.HotkeyGesture.Parse(text).Label == text, $"round trip {key}");
            Expect(UI.HotkeyRecorder.GestureText(key, Avalonia.Input.KeyModifiers.None) is null, $"unmodified {key}");
        }
        Expect(UI.HotkeyRecorder.GestureText(Avalonia.Input.Key.K, mods | Avalonia.Input.KeyModifiers.Shift)
            == "Ctrl+Alt+Shift+K", "modifier order");
        foreach (var key in Enum.GetValues<Avalonia.Input.Key>().Distinct())
        {
            foreach (var modifiers in new[] { Avalonia.Input.KeyModifiers.None, Avalonia.Input.KeyModifiers.Control,
                Avalonia.Input.KeyModifiers.Alt, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Alt,
                Avalonia.Input.KeyModifiers.Shift, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift,
                Avalonia.Input.KeyModifiers.Alt | Avalonia.Input.KeyModifiers.Shift, mods | Avalonia.Input.KeyModifiers.Shift })
            {
                var text = UI.HotkeyRecorder.GestureText(key, modifiers);
                Expect(text is null || !text.Contains("Win", StringComparison.Ordinal), $"no Windows modifier {modifiers}+{key}");
                Expect(UI.HotkeyRecorder.GestureText(key, modifiers | Avalonia.Input.KeyModifiers.Meta) is null,
                    $"Windows modifier rejected {modifiers}+{key}");
            }
        }
        Expect(UI.HotkeyRecorder.GestureText(Avalonia.Input.Key.D0, mods) == "Ctrl+Alt+0", "digit spelling");
        foreach (var key in Enum.GetValues<Avalonia.Input.Key>().Distinct().Except(keys))
            Expect(UI.HotkeyRecorder.GestureText(key, mods) is null, $"unsupported or modifier-only {key}");
        foreach (var key in new[] { Avalonia.Input.Key.Escape, Avalonia.Input.Key.Enter })
        {
            Expect(UI.HotkeyRecorder.IsCommand(key, Avalonia.Input.KeyModifiers.None), $"command {key}");
            foreach (var modifier in new[] { Avalonia.Input.KeyModifiers.Control, Avalonia.Input.KeyModifiers.Alt,
                Avalonia.Input.KeyModifiers.Shift })
            {
                Expect(!UI.HotkeyRecorder.IsCommand(key, modifier)
                    && UI.HotkeyRecorder.GestureText(key, modifier) is not null, $"modified command key {modifier}+{key}");
            }
        }
        Expect(!UI.HotkeyRecorder.IsCommand(Avalonia.Input.Key.K, Avalonia.Input.KeyModifiers.None), "ordinary key is not a command");
        report = failures.Count == 0 ? $"{keys.Length} supported main keys round-trip; modifier order, rejected keys and commands" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    /// <summary>Checks the Settings Escape deferral boundary without opening a window.</summary>
    private static bool HotkeyRecorderEscapeChecks(out string report)
    {
        var failures = new List<string>();
        void Expect(bool ok, string name) { if (!ok) failures.Add(name); }
        var actions = new Dictionary<string, Action>();
        var recorder = new UI.HotkeyRecorder(Palette.All[0], () => "Ctrl+Alt+K", _ => true, false,
            (text, action, _) => { actions[text] = action; return new Button { Content = text }; });
        var host = (ContentControl)recorder.View;
        Expect(!UI.HotkeyRecorder.IsRecordingSource(host), "idle recorder keeps Settings Escape");
        actions["Change"]();
        var row = (StackPanel)((StackPanel)host.Content!).Children[0];
        var cancel = row.Children.OfType<Button>().Single(button => Equals(button.Content, "Cancel"));
        Expect(UI.HotkeyRecorder.IsRecordingSource(host), "active recorder receives Escape");
        Expect(UI.HotkeyRecorder.IsRecordingSource(cancel), "recorder descendants receive Escape");
        Expect(!UI.HotkeyRecorder.IsRecordingSource(new Button()), "outside source keeps Settings Escape");
        Expect(!UI.HotkeyRecorder.IsRecordingSource(null), "missing source keeps Settings Escape");
        actions["Cancel"]();
        Expect(!UI.HotkeyRecorder.IsRecordingSource(host), "cancel restores Settings Escape");
        report = failures.Count == 0 ? "Escape defers only inside an active recorder" : string.Join("; ", failures);
        return failures.Count == 0;
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

            var directions = new List<int>();
            var fired = new UI.NoteSwipe(pane, () => settings, () => pane, directions.Add);
            fired.Input(-1.5);
            fired.Input(-1.5);
            CheckCase(directions.SequenceEqual([1]), "one navigation per committed gesture");
            typeof(UI.NoteSwipe).GetField("_at", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(fired, DateTime.Now.AddSeconds(-1));
            fired.Input(1.5);
            CheckCase(directions.SequenceEqual([1, -1]), "rearmed gesture navigates once in the opposite direction");
            fired.Cancel();

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

    /// <summary>Exercises the real transition cleanup without creating or displaying a window.</summary>
    private static bool NoteSwitchChecks(out string report)
    {
        var failures = new List<string>();
        void Expect(bool ok, string name) { if (!ok) failures.Add(name); }
        var settings = new Settings { Swipe = "progress" };
        var folder = Path.Combine(Path.GetTempPath(), "AntiNotepad-swipe-" + Guid.NewGuid().ToString("N"));
        var store = new Store(folder, createDirectories: false);
        var notes = new NotesSession(store, folder, folder, null, _ => { });
        var timers = new Timers();
        var pad = new PadView(Palette.All[0], settings, timers, notes) { Private = true };
        pad.PrepareSwipe(0, 1);
        pad.SwitchTo(new Note(), 1);
        var content = (Grid)pad.Children.Single();
        var first = new Note { Text = "first" };
        var second = new Note { Text = "second" };
        var reduced = Anim.Reduced;
        bool Restored() => pad.RenderTransform is null && pad.Opacity == 1
            && content.RenderTransform is null && content.Opacity == 1 && pad.Children.Count == 1;
        try
        {
            foreach (var reduce in new[] { false, true })
            {
                Anim.Reduced = reduce;
                pad.Bind(first);
                pad.PrepareSwipe(-80, 0.6);
                pad.SwitchTo(second, 1);
                Expect(ReferenceEquals(pad.Note, second) && pad.Editor.Text == "second" && Restored(),
                    $"reduced {reduce}: no-frame switch settles");
            }

            // Seed an in-flight departure, then send the next real gesture. No test seam in product code is needed.
            var departure = new Anim.Handle();
            var image = new Image { RenderTransform = new TranslateTransform(-80, 0), Opacity = 0.6 };
            pad.Children.Add(image);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(PadView).GetField("_departure", fields)!.SetValue(pad, departure);
            typeof(PadView).GetField("_departureImage", fields)!.SetValue(pad, image);
            content.Opacity = 0;
            var calls = 0;
            var swipe = new UI.NoteSwipe(pad, () => settings, () => pad, direction =>
            {
                calls++;
                pad.SwitchTo(first, direction);
            });
            swipe.Input(-1.5);
            Expect(calls == 1 && departure.Cancelled && ReferenceEquals(pad.Note, first) && Restored(),
                "second fire during departure preserves the bound selection and navigates once");
            swipe.Input(-1.5);
            Expect(calls == 1, "inertia does not double navigation");
            pad.RenderTransform = new TranslateTransform(80, 0);
            pad.Opacity = 0.6;
            content.RenderTransform = new TranslateTransform(20, 0);
            content.Opacity = 0.5;
            swipe.Cancel();
            Expect(Restored() && !swipe.HasRunningTimer, "cancel restores pane and content even when detached");
            report = failures.Count == 0 ? "no-frame and reduced-motion switches, repeat-fire and detached cleanup" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        finally
        {
            Anim.Reduced = reduced;
            pad.Release();
            timers.Stop();
        }
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
}
