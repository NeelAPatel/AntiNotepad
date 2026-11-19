// Capability checks for settings notifications, note gestures, and transition cleanup.
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Threading;
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
            typeof(UI.PadWindow).GetMethod("Edited", fields)!.Invoke(pad, [null, true]);
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

    /// <summary>Checks the shortcut table, exact routing, persistence fallback, and its user-facing mirrors.</summary>
    private static bool ShortcutMapChecks(out string report)
    {
        var failures = new List<string>();
        void Expect(bool ok, string name) { if (!ok) failures.Add(name); }
        var defaults = new Model.Settings();
        var map = new UI.ShortcutMap(defaults);
        var seen = new HashSet<(UI.ShortcutScope Scope, UI.ShortcutGesture Gesture)>();
        foreach (var definition in UI.ShortcutMap.Definitions)
        {
            if (definition.DefaultKey.Length == 0) continue;
            var parsed = UI.ShortcutMap.TryParse(definition.DefaultKey, out var gesture);
            Expect(parsed, $"parse default {definition.Id}");
            if (!parsed) continue;
            Expect(UI.ShortcutMap.BlockedReason(definition.DefaultKey, defaults, definition) is null, $"default is allowed {definition.Id}");
            Expect(seen.Add((definition.Scope, gesture)), $"default key is unique in {definition.Scope}: {definition.DefaultKey}");
            var label = map.GetLabel(definition.Id);
            Expect(UI.ShortcutMap.TryParse(label, out var labelGesture) && labelGesture == gesture,
                $"default label round-trips {definition.Id}: {label}");
            var routed = map.TryGet(gesture.Key, gesture.Modifiers, out var route)
                && (definition.Scope == UI.ShortcutScope.Pad ? route.Pad == definition.Action : route.Search == definition.Action);
            Expect(routed, $"default routes to {definition.Action}");
        }

        Expect(map.GetLabel("text-size-up") == "Ctrl++"
            && UI.ShortcutMap.LabelParts(map.GetLabel("text-size-up")).SequenceEqual(["Ctrl", "+"]),
            "plus key keeps its label and Settings key cap");
        Expect(map.GetLabel("text-size-down") == "Ctrl+-" && map.GetLabel("split") == "Ctrl+\\"
            && map.GetLabel("fold") == "Ctrl+Shift+[" && map.GetLabel("unfold") == "Ctrl+Shift+]",
            "other symbol-key default labels keep their keys and modifiers");

        var checkbox = UI.ShortcutMap.Definitions.Single(definition => definition.Id == "checkbox-timer");
        Expect(checkbox.DefaultKey.Length == 0
            && !map.TryGet(Avalonia.Input.Key.Enter, Avalonia.Input.KeyModifiers.Control, out _),
            "checkbox or timer has no default Ctrl+Enter route");
        Expect(UI.ShortcutMap.BlockedReason("Ctrl+Enter", defaults, checkbox) is null,
            "Ctrl+Enter remains assignable to checkbox or timer");
        var boundCheckbox = new UI.ShortcutMap(new Model.Settings
        {
            ShortcutOverrides = new Dictionary<string, string>(StringComparer.Ordinal) { [checkbox.Id] = "Ctrl+Enter" },
        });
        Expect(boundCheckbox.TryGet(Avalonia.Input.Key.Enter, Avalonia.Input.KeyModifiers.Control, out var checkboxRoute)
            && checkboxRoute.Pad == UI.ShortcutAction.ToggleCheckbox,
            "a saved checkbox or timer key routes through the shared map");

        foreach (var reserved in new[]
        {
            "A", "Ctrl+Alt+Shift+1", "Ctrl+Alt+Shift+F1", "Ctrl+Alt+Shift+F9", "Ctrl+Alt+Shift+S",
            "Alt+Shift+Left", "Alt+Shift+Up", "Ctrl+Z", "Ctrl+Home", "Ctrl+[", "Shift+Tab", "Ctrl+Alt+Delete",
            "Shift+Backspace", "Shift+Delete", "Ctrl+Insert", "Shift+Insert", "Ctrl+Shift+Z",
        }) Expect(UI.ShortcutMap.BlockedReason(reserved, defaults) is not null, $"reserved key refused: {reserved}");
        Expect(UI.ShortcutMap.RecorderGestureText(Avalonia.Input.Key.F1, Avalonia.Input.KeyModifiers.None) == "F1",
            "unmodified F1 is recordable");
        Expect(UI.ShortcutMap.RecorderGestureText(Avalonia.Input.Key.OemPlus, Avalonia.Input.KeyModifiers.Control) == "Ctrl++",
            "plus key keeps its canonical spelling");

        var custom = new Model.Settings
        {
            ShortcutOverrides = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["new-note"] = "Ctrl+Q",
                ["help"] = "Ctrl+Alt+Shift+F1",
                ["settings"] = "Ctrl+,",
            },
        };
        var customMap = new UI.ShortcutMap(custom);
        Expect(customMap.GetLabel("new-note") == "Ctrl+Q" && customMap.GetLabel("search") == "Ctrl+F",
            "valid custom key coexists with untouched defaults");
        Expect(customMap.GetLabel("help") == "F1", "reserved saved value falls back to default");
        Expect(!customMap.TryGet(Avalonia.Input.Key.OemComma, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift, out _),
            "lookup requires exact modifiers");
        Expect(customMap.TryGet(Avalonia.Input.Key.OemComma, Avalonia.Input.KeyModifiers.Control, out var settingsRoute)
            && settingsRoute.Pad == UI.ShortcutAction.Settings, "custom Settings chord routes exactly");

        var duplicateSavedKeys = new Model.Settings
        {
            ShortcutOverrides = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["new-note"] = "Ctrl+F",
                ["search"] = "Ctrl+F",
            },
        };
        var duplicateMap = new UI.ShortcutMap(duplicateSavedKeys);
        Expect(duplicateMap.GetLabel("new-note") == "Ctrl+N" && duplicateMap.GetLabel("search") == "Ctrl+F",
            "duplicate saved keys fall back to both defaults");
        Expect(duplicateMap.TryGet(Avalonia.Input.Key.N, Avalonia.Input.KeyModifiers.Control, out var newNoteRoute)
            && newNoteRoute.Pad == UI.ShortcutAction.NewNote, "duplicate override preserves the New note route");
        Expect(duplicateMap.TryGet(Avalonia.Input.Key.F, Avalonia.Input.KeyModifiers.Control, out var searchRoute)
            && searchRoute.Pad == UI.ShortcutAction.Search, "duplicate override preserves the Search route");

        var invalidOverride = new Model.Settings
        {
            ShortcutOverrides = new Dictionary<string, string>(StringComparer.Ordinal) { ["search"] = "garbage" },
        };
        var invalidMap = new UI.ShortcutMap(invalidOverride);
        Expect(invalidMap.GetLabel("search") == "Ctrl+F"
            && invalidMap.TryGet(Avalonia.Input.Key.F, Avalonia.Input.KeyModifiers.Control, out var fallbackSearchRoute)
            && fallbackSearchRoute.Pad == UI.ShortcutAction.Search, "malformed override restores its default route");

        var invalidPairedValue = new Model.Settings
        {
            ShortcutOverrides = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["new-note"] = "Ctrl+F",
                ["search"] = "garbage",
            },
        };
        var invalidPairedMap = new UI.ShortcutMap(invalidPairedValue);
        Expect(invalidPairedMap.GetLabel("new-note") == "Ctrl+F" && invalidPairedMap.GetLabel("search").Length == 0,
            "invalid override falls back unless a valid override owns its default");
        Expect(invalidPairedMap.TryGet(Avalonia.Input.Key.F, Avalonia.Input.KeyModifiers.Control, out var newNoteOnSearchRoute)
            && newNoteOnSearchRoute.Pad == UI.ShortcutAction.NewNote
            && newNoteOnSearchRoute.Search == UI.ShortcutAction.None
            && !invalidPairedMap.TryGet(Avalonia.Input.Key.N, Avalonia.Input.KeyModifiers.Control, out _),
            "valid override keeps its key and does not leave a stale default route");

        var swap = new Model.Settings
        {
            ShortcutOverrides = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["new-note"] = "Ctrl+F",
                ["search"] = "Ctrl+N",
            },
        };
        var swapMap = new UI.ShortcutMap(swap);
        Expect(swapMap.GetLabel("new-note") == "Ctrl+F" && swapMap.GetLabel("search") == "Ctrl+N",
            "valid shortcut swaps remain assigned");

        var roundTrip = System.Text.Json.JsonSerializer.Deserialize<Model.Settings>(System.Text.Json.JsonSerializer.Serialize(custom))!;
        var loadedMap = new UI.ShortcutMap(roundTrip);
        Expect(loadedMap.GetLabel("settings") == "Ctrl+," && loadedMap.GetLabel("new-note") == "Ctrl+Q",
            "shortcut overrides survive save and load");
        roundTrip.ShortcutOverrides["search"] = "";
        Expect(new UI.ShortcutMap(roundTrip).GetLabel("search").Length == 0, "empty override clears a default");
        roundTrip.ShortcutOverrides.Clear();
        Expect(new UI.ShortcutMap(roundTrip).GetLabel("search") == "Ctrl+F", "reset restores the default");

        var emptyOpenSlotMap = new UI.ShortcutMap(new Model.Settings());
        Expect(UI.PadWindow.SlotPromotionMessage(3, emptyOpenSlotMap.GetLabel("open-slot")) == "Kept in slot 3",
            "slot promotion omits an unassigned Open slot hint");
        var assignedOpenSlotMap = new UI.ShortcutMap(new Model.Settings
        {
            ShortcutOverrides = new Dictionary<string, string>(StringComparer.Ordinal) { ["open-slot"] = "Alt+1" },
        });
        Expect(UI.PadWindow.SlotPromotionMessage(3, assignedOpenSlotMap.GetLabel("open-slot"))
            == "Kept in slot 3 · Alt+1 … 9 opens this slot", "slot promotion uses the effective Open slot hint");

        var service = new Model.SettingsService(new Model.Settings());
        Model.SettingsChange? observed = null;
        service.Changed += change => observed = change;
        service.Update(settings => settings.ShortcutOverrides["settings"] = "Ctrl+,");
        Expect(observed is { Flags: var flags } && (flags & Model.SettingsChangeFlags.Shortcuts) != 0
            && observed.Properties.Contains(nameof(Model.Settings.ShortcutOverrides)), "settings change tracks dictionary edits");
        var detached = service.Current.CopyForSave();
        detached.ShortcutOverrides.Clear();
        Expect(service.Current.ShortcutOverrides.ContainsKey("settings"), "saved settings contain a detached override dictionary");

        var helpRows = UI.ShortcutMap.HelpRows(duplicateMap);
        var activeCount = UI.ShortcutMap.Definitions.Count(definition => duplicateMap.GetLabel(definition.Id).Length > 0);
        Expect(helpRows.Count == activeCount + 1 + 8, "help contains each assigned action and all fixed rows");
        foreach (var definition in UI.ShortcutMap.Definitions.Where(definition => duplicateMap.GetLabel(definition.Id).Length > 0))
            Expect(helpRows.Any(row => row.Keys == duplicateMap.GetLabel(definition.Id) && row.Help == definition.Help),
                $"help mirrors {definition.Id}");

        string? FindRoot(string start)
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "docs", "shortcuts.md"))
                    && File.Exists(Path.Combine(directory.FullName, "product", "AntiNotepad", "AntiNotepad.csproj")))
                    return directory.FullName;
                directory = directory.Parent;
            }
            return null;
        }
        var root = FindRoot(AppContext.BaseDirectory) ?? FindRoot(Environment.CurrentDirectory);
        var docsMatch = false;
        if (root is not null)
        {
            var doc = File.ReadAllText(Path.Combine(root, "docs", "shortcuts.md")).Replace("\r\n", "\n", StringComparison.Ordinal);
            const string start = "<!-- shortcuts:table:start -->";
            const string end = "<!-- shortcuts:table:end -->";
            var first = doc.IndexOf(start, StringComparison.Ordinal);
            var last = doc.IndexOf(end, StringComparison.Ordinal);
            docsMatch = first >= 0 && last > first
                && doc[(first + start.Length)..last].Trim() == UI.ShortcutMap.DocumentationTable();
        }
        Expect(docsMatch, "documentation table matches shortcut definitions");

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1000; i++) map.TryGet(Avalonia.Input.Key.N, Avalonia.Input.KeyModifiers.Control, out _);
        Expect(GC.GetAllocatedBytesForCurrentThread() == before, "key lookup allocates nothing");

        if (Application.Current is null) AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();
        if (!Application.Current!.Styles.OfType<Avalonia.Themes.Fluent.FluentTheme>().Any())
        {
            Application.Current.Styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
            Application.Current.Styles.Add(new StyleInclude(new Uri(AppInfo.ResourceUri)) { Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml") });
        }
        var folder = Path.Combine(Path.GetTempPath(), "AntiNotepad-shortcuts-" + Guid.NewGuid().ToString("N"));
        using var log = new RuntimeLog();
        var store = new Store(folder);
        Model.SettingsService? padSettings = null;
        UI.PadWindow? pad = null;
        var subscribed = false;
        try
        {
            store.Load();
            store.Data.Stack[0].Text = "test note";
            padSettings = store.SettingsService;
            var notes = new NotesSession(store, folder, folder, null, log.Write);
            pad = new UI.PadWindow(store, padSettings, log, false, true, notes);
            var keyDown = typeof(UI.PadWindow).GetMethod("OnWindowKeyDown", BindingFlags.Instance | BindingFlags.NonPublic)!;
            padSettings.Changed += pad.HandleSettingsChanged;
            subscribed = true;
            padSettings.Update(settings => settings.ShortcutOverrides["settings"] = "Ctrl+,");
            var comma = new Avalonia.Input.KeyEventArgs
            {
                Key = Avalonia.Input.Key.OemComma,
                KeyModifiers = Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift,
            };
            keyDown.Invoke(pad, [null, comma]);
            var settingsWindow = typeof(UI.PadWindow).GetField("_settingsWindow", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pad);
            Expect(!comma.Handled && settingsWindow is null, "Ctrl+Shift+comma does not open Settings");

            var newNote = new Avalonia.Input.KeyEventArgs { Key = Avalonia.Input.Key.N, KeyModifiers = Avalonia.Input.KeyModifiers.Control };
            keyDown.Invoke(pad, [null, newNote]);
            Expect(newNote.Handled && pad.Data.Stack.Count == 2 && pad.Data.Stack[pad.Data.Current].IsEmpty,
                "Ctrl+N runs the real New note action");

            var wasPinned = pad.Topmost;
            var pin = new Avalonia.Input.KeyEventArgs { Key = Avalonia.Input.Key.T, KeyModifiers = Avalonia.Input.KeyModifiers.Control };
            keyDown.Invoke(pad, [null, pin]);
            Expect(pin.Handled && pad.Topmost != wasPinned && pad.Settings.Pinned == pad.Topmost,
                "Ctrl+T runs the real Keep on top action");

            var help = new Avalonia.Input.KeyEventArgs { Key = Avalonia.Input.Key.F1, KeyModifiers = Avalonia.Input.KeyModifiers.None };
            keyDown.Invoke(pad, [null, help]);
            var overlay = typeof(UI.PadWindow).GetField("_overlay", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pad);
            Expect(help.Handled && overlay is UI.HelpOverlay, "F1 opens the real Help overlay");

            var closeHelp = new Avalonia.Input.KeyEventArgs { Key = Avalonia.Input.Key.F1, KeyModifiers = Avalonia.Input.KeyModifiers.None };
            keyDown.Invoke(pad, [null, closeHelp]);
            overlay = typeof(UI.PadWindow).GetField("_overlay", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pad);
            Expect(closeHelp.Handled && overlay is null, "F1 closes Help while the overlay has focus");
            var reopenHelp = new Avalonia.Input.KeyEventArgs { Key = Avalonia.Input.Key.F1, KeyModifiers = Avalonia.Input.KeyModifiers.None };
            keyDown.Invoke(pad, [null, reopenHelp]);
            overlay = typeof(UI.PadWindow).GetField("_overlay", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pad);
            Expect(reopenHelp.Handled && overlay is UI.HelpOverlay, "F1 reopens Help after closing it");

            padSettings.Update(settings => settings.ShortcutOverrides["help"] = "Ctrl+H");
            var closeBoundHelp = new Avalonia.Input.KeyEventArgs { Key = Avalonia.Input.Key.H, KeyModifiers = KeyModifiers.Control };
            keyDown.Invoke(pad, [null, closeBoundHelp]);
            overlay = typeof(UI.PadWindow).GetField("_overlay", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pad);
            Expect(closeBoundHelp.Handled && overlay is null, "a user-bound Help key closes Help");
            var reopenBoundHelp = new Avalonia.Input.KeyEventArgs { Key = Avalonia.Input.Key.H, KeyModifiers = KeyModifiers.Control };
            keyDown.Invoke(pad, [null, reopenBoundHelp]);
            overlay = typeof(UI.PadWindow).GetField("_overlay", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pad);
            Expect(reopenBoundHelp.Handled && overlay is UI.HelpOverlay, "a user-bound Help key reopens Help");

            var search = new Avalonia.Input.KeyEventArgs { Key = Avalonia.Input.Key.F, KeyModifiers = KeyModifiers.Control };
            keyDown.Invoke(pad, [null, search]);
            overlay = typeof(UI.PadWindow).GetField("_overlay", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pad);
            Expect(search.Handled && overlay is UI.SearchOverlay, "Ctrl+F opens the real Search overlay");
            search = new Avalonia.Input.KeyEventArgs { Key = Avalonia.Input.Key.F, KeyModifiers = KeyModifiers.Control };
            keyDown.Invoke(pad, [null, search]);
            overlay = typeof(UI.PadWindow).GetField("_overlay", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pad);
            Expect(search.Handled && overlay is null, "Ctrl+F closes Search while the overlay is focused");
            search = new Avalonia.Input.KeyEventArgs { Key = Avalonia.Input.Key.F, KeyModifiers = KeyModifiers.Control };
            keyDown.Invoke(pad, [null, search]);
            overlay = typeof(UI.PadWindow).GetField("_overlay", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pad);
            Expect(search.Handled && overlay is UI.SearchOverlay, "Ctrl+F opens Search again after closing it");
        }
        finally
        {
            if (subscribed && padSettings is not null && pad is not null) padSettings.Changed -= pad.HandleSettingsChanged;
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
        report = failures.Count == 0 ? $"{seen.Count} unique defaults and label round trips, checkbox binding, reserved keys, Help toggle, persistence, docs parity, no-allocation lookup, and real actions" : string.Join("; ", failures);
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

        service.Update(settings => settings.SaveDelayMs = 5000);
        Expect(first[^1].Properties.SequenceEqual([nameof(Settings.SaveDelayMs)])
            && first[^1].Flags == SettingsChangeFlags.Persistence, "save delay is persistence-only");

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
            && first[^1].Properties.Count == 58, "Replace emits all flags once");

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

    /// <summary>Drives the save timing policy with monotonic fake-clock values.</summary>
    private static bool SaveTimingChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        const int idle = ChromeTokens.SaveIdleGateMs;
        var delays = new[] { ChromeTokens.SaveDelayFiveSecondsMs, ChromeTokens.SaveDelayTenSecondsMs, ChromeTokens.SaveDelayThirtySecondsMs };

        var quick = UI.SaveTiming.Edit(null, 1000, ChromeTokens.SaveDelayDefaultMs, idle);
        CheckCase(quick.DeadlineMs == 1400 && UI.SaveTiming.Tick(quick.State, 1399, idle).SaveNow == false
            && UI.SaveTiming.Tick(quick.State, 1400, idle).SaveNow, "400 ms keeps the old trailing-edge debounce");

        foreach (var delay in delays)
        {
            var idleEdit = UI.SaveTiming.Edit(null, 1000, delay, idle);
            var gate = UI.SaveTiming.Tick(idleEdit.State, 2000, idle);
            CheckCase(idleEdit.DeadlineMs == 2000 && !gate.SaveNow && gate.DeadlineMs == 1000 + delay,
                $"{delay} ms starts its countdown after the one-second idle gate");
            CheckCase(!UI.SaveTiming.Tick(gate.State, 1000 + delay - 1, idle).SaveNow
                && UI.SaveTiming.Tick(gate.State, 1000 + delay, idle).SaveNow,
                $"{delay} ms lands on the last-edit deadline");

            var continuous = UI.SaveTiming.Edit(null, 0, delay, idle).State;
            for (var elapsed = 900; elapsed < delay; elapsed += 900)
                continuous = UI.SaveTiming.Edit(continuous, elapsed, delay, idle).State;
            var safetyDeadline = UI.SaveTiming.NextDeadline(continuous, continuous.LastEditMs, idle);
            var safety = UI.SaveTiming.Tick(continuous, delay, idle);
            CheckCase(safetyDeadline <= delay && safety.SaveNow && safety.State.RetryAtMs == 2L * delay,
                $"{delay} ms safety write and repeat while typing");
            var retry = UI.SaveTiming.Tick(safety.State, 2L * delay, idle);
            CheckCase(retry.SaveNow && retry.State.RetryAtMs == 3L * delay,
                $"{delay} ms failed-write retry uses the next tick");
        }

        var pause = UI.SaveTiming.Edit(null, 0, 5000, idle).State;
        pause = UI.SaveTiming.Edit(pause, 900, 5000, idle).State;
        var idleGate = UI.SaveTiming.Tick(pause, 1900, idle);
        var resumed = UI.SaveTiming.Edit(idleGate.State, 2000, 5000, idle);
        CheckCase(!idleGate.SaveNow && idleGate.DeadlineMs == 5900 && resumed.State.FirstEditMs == 2000
            && !UI.SaveTiming.Tick(resumed.State, 5000, idle).SaveNow,
            "a full idle pause starts a new countdown on the next edit");

        var first = UI.SaveTiming.Edit(null, 100, 5000, idle).State;
        var second = UI.SaveTiming.Edit(null, 3000, 10000, idle).State;
        CheckCase(UI.SaveTiming.NextDeadline(first, 100, idle) == 1100
            && UI.SaveTiming.NextDeadline(second, 3000, idle) == 4000,
            "two notes retain independent deadlines");
        var wake = UI.SaveTiming.Edit(null, 10, 30000, idle).State;
        var afterWake = UI.SaveTiming.Tick(wake, 60000, idle);
        CheckCase(afterWake.SaveNow && afterWake.DeadlineMs == 90000, "a delayed dispatcher saves overdue edits once and retries from steady time");
        var changed = UI.SaveTiming.Edit(first, 150, 10000, idle);
        CheckCase(changed.State.FirstEditMs == 150 && changed.State.DelayMs == 10000,
            "a changed delay applies from the next edit");
        var boundary = UI.SaveTiming.RetryAfterAttempt(first, 150, resetBurstOnEdit: true);
        var afterBoundaryEdit = UI.SaveTiming.Edit(boundary, 151, 5000, idle);
        CheckCase(afterBoundaryEdit.State.FirstEditMs == 151,
            "an immediate save boundary starts a fresh run on the next edit");

        report = failures.Count == 0
            ? "400 ms debounce; 5/10/30 s idle and safety deadlines; retries; per-note deadlines; delay changes; and delayed ticks pass"
            : string.Join("; ", failures);
        return failures.Count == 0;
    }

    /// <summary>Checks timer updates, durable completion labels, and saves across real pane transitions.</summary>
    private static bool SaveDelayChecks(out string report)
    {
        var app = Application.Current ?? throw new InvalidOperationException(
            "shortened-link-opens must initialize Avalonia before save-delay UI checks.");
        if (!app.Styles.OfType<Avalonia.Themes.Fluent.FluentTheme>().Any())
        {
            app.Styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
            app.Styles.Add(new StyleInclude(new Uri(AppInfo.ResourceUri)) { Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml") });
        }

        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        CheckCase(UI.PadWindow.SaveDelayInterval(new Settings()) == TimeSpan.FromMilliseconds(400),
            "fresh settings keep the 400 ms delay");
        var root = Path.Combine(Path.GetTempPath(), "AntiNotepad-save-delay-ui-" + Guid.NewGuid().ToString("N"));
        using var log = new RuntimeLog();
        log.SetHome(root);
        var reduced = Anim.Reduced;
        UI.PadWindow? pad = null;
        SettingsService? service = null;
        Store? store = null;
        Action<Note>? savedHandler = null;
        try
        {
            store = new Store(root);
            store.Load();
            store.Data.Settings.SaveDelayMs = 30000;
            var notes = new NotesSession(store, root, root, null, log.Write);
            pad = new UI.PadWindow(store, store.SettingsService, log, false, true, notes);
            service = store.SettingsService;
            service.Changed += pad.HandleSettingsChanged;
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            var timer = (DispatcherTimer)typeof(UI.PadWindow).GetField("_saveTimer", fields)!.GetValue(pad)!;
            var labelTimer = (DispatcherTimer)typeof(UI.PadWindow).GetField("_savedLabelTimer", fields)!.GetValue(pad)!;
            var left = (PadView)typeof(UI.PadWindow).GetField("_left", fields)!.GetValue(pad)!;
            var right = (PadView)typeof(UI.PadWindow).GetField("_right", fields)!.GetValue(pad)!;
            PadView ActivePane() => (PadView)typeof(UI.PadWindow).GetField("_active", fields)!.GetValue(pad)!;
            var schedules = (Dictionary<Note, UI.SaveTiming.State>)typeof(UI.PadWindow)
                .GetField("_saveSchedules", fields)!.GetValue(pad)!;
            var savedLabel = (TextBlock)typeof(UI.PadWindow).GetField("_saved", fields)!.GetValue(pad)!;
            var topLayer = (Panel)typeof(UI.PadWindow).GetField("_topLayer", fields)!.GetValue(pad)!;
            var bottomLayer = (Panel)typeof(UI.PadWindow).GetField("_bottomLayer", fields)!.GetValue(pad)!;
            var pill = (Border)typeof(UI.PadWindow).GetField("_pill", fields)!.GetValue(pad)!;
            var pager = (Avalonia.Controls.Primitives.Popup)typeof(UI.PadWindow).GetField("_floatPager", fields)!.GetValue(pad)!;
            bool TopShown() => (bool)typeof(UI.PadWindow).GetField("_topShown", fields)!.GetValue(pad)!;
            bool BottomShown() => (bool)typeof(UI.PadWindow).GetField("_bottomShown", fields)!.GetValue(pad)!;
            var logPath = Path.Combine(root, AppInfo.LogFileName);
            bool BodyIs(Note note, string expected)
            {
                var bodyPath = Path.Combine(store.Folder, "notes", note.Id + ".md");
                return File.Exists(bodyPath) && File.ReadAllText(bodyPath) == expected;
            }
            void Edit(PadView pane, string body) => pane.Editor.Document.Text = body;

            var fresh = left.Note!;
            CheckCase(savedLabel.Text == "-" && !fresh.Dirty && schedules.Count == 0 && !timer.IsEnabled,
                "a never-saved blank note has no label, dirty state or deadline");
            left.Editor.Document.Insert(0, "temporary");
            left.Editor.Document.Replace(0, left.Editor.Document.TextLength, "");
            CheckCase(!fresh.Dirty && !schedules.ContainsKey(fresh) && !timer.IsEnabled && savedLabel.Text == "-",
                $"deleting all text from a never-saved note cancels its write (dirty={fresh.Dirty}, scheduled={schedules.ContainsKey(fresh)}, timer={timer.IsEnabled}, label={savedLabel.Text})");
            CheckCase(store.Flush() && !File.Exists(Path.Combine(root, "notes", fresh.Id + ".md")),
                "a blank never-saved note has no body write");

            left.Editor.Document.Insert(0, "text deleted during first save");
            var firstWrite = store.QueueSave();
            var queuedBeforeClear = !firstWrite.IsCompleted;
            left.Editor.Document.Replace(0, left.Editor.Document.TextLength, "");
            var dirtyWhileQueued = fresh.Dirty;
            var scheduledWhileQueued = schedules.ContainsKey(fresh);
            var firstWriteSucceeded = store.Flush();
            var dirtyAfterStaleCompletion = fresh.Dirty;
            var scheduledAfterStaleCompletion = schedules.ContainsKey(fresh);
            var quitSavedEmpty = pad.Save();
            var reloadedQueuedStore = new Store(store.Folder);
            reloadedQueuedStore.Load();
            var reloadedQueuedText = reloadedQueuedStore.Data.All.Single(note => note.Id == fresh.Id).Text;
            Console.WriteLine($"SAVE DELAY REGRESSION blank queued write: first={firstWriteSucceeded}, queuedBeforeClear={queuedBeforeClear}, clearDirty={dirtyWhileQueued}, clearScheduled={scheduledWhileQueued}, staleDirty={dirtyAfterStaleCompletion}, staleScheduled={scheduledAfterStaleCompletion}, quit={quitSavedEmpty}, liveLength={fresh.Text.Length}, savedAt={fresh.SavedAt:O}, reloadedText=[{reloadedQueuedText}]");
            CheckCase(queuedBeforeClear && firstWriteSucceeded && dirtyWhileQueued && scheduledWhileQueued
                && dirtyAfterStaleCompletion && scheduledAfterStaleCompletion && quitSavedEmpty && reloadedQueuedText == "",
                "clearing during the first queued body write keeps the empty revision dirty through a stale completion and quit saves it");

            left.Editor.Document.Insert(0, "saved text");
            CheckCase(store.Save() && fresh.SavedAt is not null, "a nonempty edit becomes a saved note");
            left.Editor.Document.Text = "";
            CheckCase(fresh.Dirty && schedules.ContainsKey(fresh), "deleting saved text still schedules the empty body");
            CheckCase(store.Save() && BodyIs(fresh, "") && !fresh.Dirty, "a saved note persists an empty body");

            foreach (var delay in new[] { 400, 5000, 10000, 30000 })
            {
                service.Update(settings => settings.SaveDelayMs = delay);
                store.Flush();
                var note = left.Note!;
                var previousSavedAt = note.SavedAt;
                var previousLabel = UI.PadWindow.SavedText(note, DateTime.Now);
                Edit(left, $"saved with delay {delay}");
                var hasSchedule = schedules.TryGetValue(note, out var state);
                CheckCase(hasSchedule && state.DelayMs == delay,
                    $"{delay} ms is captured for this note edit (settings={store.Data.Settings.SaveDelayMs}, dirty={note.Dirty}, scheduled={hasSchedule}, observed={state.DelayMs})");
                CheckCase(note.SavedAt == previousSavedAt && savedLabel.Text == previousLabel,
                    $"{delay} ms pending edit keeps the previous saved label");
                var oldPillChild = pill.Child;
                var oldTop = TopShown(); var oldBottom = BottomShown();
                var oldTopLayer = topLayer.IsVisible; var oldBottomLayer = bottomLayer.IsVisible; var oldPager = pager.IsOpen;
                var completed = store.Save();
                log.Flush();
                CheckCase(completed && !note.Dirty && note.SavedAt is not null, $"{delay} ms durable save completes");
                CheckCase(savedLabel.Text == previousLabel && ReferenceEquals(pill.Child, oldPillChild)
                    && TopShown() == oldTop && BottomShown() == oldBottom
                    && topLayer.IsVisible == oldTopLayer && bottomLayer.IsVisible == oldBottomLayer && pager.IsOpen == oldPager,
                    $"{delay} ms save completion does not rebuild or wake chrome while hidden");
                CheckCase(File.ReadAllLines(logPath).Any(line => line.Contains($"save delay={delay}ms actual=", StringComparison.Ordinal)),
                    $"{delay} ms durable save logs its actual time");
                typeof(UI.PadWindow).GetMethod("RefreshSaved", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(pad, null);
                CheckCase(savedLabel.Text == UI.PadWindow.SavedText(note, DateTime.Now),
                    $"{delay} ms label refreshes when the pad is shown again");
                CheckCase(!labelTimer.IsEnabled, "saved-label clock stays stopped while hidden");
            }

            service.Update(settings => settings.SaveDelayMs = 5000);
            store.Flush();
            var changedNote = left.Note!;
            Edit(left, "pending before delay change");
            var hasOldDelay = schedules.TryGetValue(changedNote, out var oldDelay);
            CheckCase(hasOldDelay && oldDelay.DelayMs == 5000,
                $"pending edits use the current delay (settings={store.Data.Settings.SaveDelayMs}, dirty={changedNote.Dirty}, scheduled={hasOldDelay}, observed={oldDelay.DelayMs})");
            service.Update(settings => settings.SaveDelayMs = 10000);
            CheckCase(store.Flush() && !changedNote.Dirty && changedNote.SavedAt is not null,
                "changing a delay saves pending text at once");
            left.Editor.Document.Insert(left.Editor.Document.TextLength, " next");
            var hasNewDelay = schedules.TryGetValue(changedNote, out var newDelay);
            CheckCase(hasNewDelay && newDelay.DelayMs == 10000,
                $"the new delay applies from the next edit (settings={store.Data.Settings.SaveDelayMs}, dirty={changedNote.Dirty}, scheduled={hasNewDelay}, observed={newDelay.DelayMs})");

            left.Editor.Document.Insert(left.Editor.Document.TextLength, " pasted");
            CheckCase(schedules.ContainsKey(changedNote), "paste-style document insertion schedules a save");
            left.Editor.Undo();
            CheckCase(schedules.ContainsKey(changedNote), "Undo counts as an edit");
            left.Editor.Redo();
            CheckCase(schedules.ContainsKey(changedNote), "Redo counts as an edit");
            Edit(left, "math\nrent = 1200\nrent / 3");
            CheckCase(changedNote.ModeCache == NoteMode.Math && schedules.ContainsKey(changedNote),
                "calculator-note text changes schedule a save");
            Edit(left, "timer\n5m tea");
            CheckCase(changedNote.ModeCache == NoteMode.Timer && schedules.ContainsKey(changedNote),
                "timer-note text changes schedule a save");

            var beforeUndo = left.Editor.Document.Text;
            left.Editor.Document.Insert(left.Editor.Document.TextLength, " undo-check");
            CheckCase(store.Save(), "type and save before Undo");
            left.Editor.Undo();
            CheckCase(left.Editor.Document.Text == beforeUndo, "saving copies text without changing the editor undo stack");

            var clickAway = left.Note!;
            var clickAwayText = left.Editor.Document.Text;
            pad.SaveImmediately();
            CheckCase(store.Flush() && BodyIs(clickAway, clickAwayText), "click-away save boundary bypasses the selected delay");

            var trashNote = left.Note!;
            Edit(left, "unsaved text kept in trash");
            pad.DeleteCurrent();
            CheckCase(store.Flush() && store.Data.Trash.Contains(trashNote) && BodyIs(trashNote, "unsaved text kept in trash")
                && !schedules.ContainsKey(trashNote), "trashing cancels the old deadline and keeps the latest text");

            var searchTrashNote = left.Note!;
            Edit(left, "search trash keeps unsaved text");
            pad.Data.Stack.Remove(searchTrashNote);
            PadView.Forget(searchTrashNote);
            searchTrashNote.Deleted = DateTime.Now;
            pad.Data.Trash.Insert(0, searchTrashNote);
            if (pad.Data.Stack.Count == 0) pad.Data.Stack.Add(new Note());
            pad.Data.Current = Math.Clamp(pad.Data.Current, 0, pad.Data.Stack.Count - 1);
            pad.SearchChangedData();
            CheckCase(store.Flush() && store.Data.Trash.Contains(searchTrashNote)
                && BodyIs(searchTrashNote, "search trash keeps unsaved text") && !schedules.ContainsKey(searchTrashNote),
                "search trash cancels the old deadline and saves its current text");

            void CheckTransition(PadView pane, string body, Action transition, string name)
            {
                var note = pane.Note!;
                Edit(pane, body);
                transition();
                CheckCase(store.Flush() && BodyIs(note, body), name);
            }

            CheckTransition(left, "pending before new note", pad.NewNote, "new note transition saves the departing body");
            CheckTransition(left, "pending before stack switch", () => pad.Navigate(-1), "stack switch saves the departing body");
            CheckTransition(left, "pending before slot", () => pad.OpenSlot(1), "slot jump saves the departing body");
            CheckTransition(left, "pending before stack return", pad.ShowStack, "stack return saves the departing body");
            var jumpTarget = pad.Data.Stack[^1];
            CheckTransition(left, "pending before search jump", () => pad.JumpTo(jumpTarget), "search jump saves the departing body");
            CheckTransition(left, "pending before split", pad.ToggleSplit, "split entry saves the departing body");
            CheckTransition(right, "pending before split closes", pad.ToggleSplit, "split exit saves the departing body");

            pad.ToggleSplit();
            pad.HandlePaneFocused(left);
            var shortcutDeparting = left.Note!;
            Edit(left, "split shortcut departure");
            pad.OpenSlot(right.Note!.Slot);
            var shortcutActiveRight = ReferenceEquals(ActivePane(), right);
            var shortcutSaved = store.Flush() && BodyIs(shortcutDeparting, "split shortcut departure");
            var focusDeparting = right.Note!;
            Edit(right, "split focus departure");
            pad.HandlePaneFocused(left);
            var focusActiveLeft = ReferenceEquals(ActivePane(), left);
            var focusSaved = store.Flush() && BodyIs(focusDeparting, "split focus departure");
            Console.WriteLine($"SAVE DELAY REGRESSION split active note: shortcutActiveRight={shortcutActiveRight}, shortcutSaved={shortcutSaved}, focusActiveLeft={focusActiveLeft}, focusSaved={focusSaved}");
            CheckCase(shortcutActiveRight && shortcutSaved && focusActiveLeft && focusSaved,
                "Ctrl+slot and pane focus save the departing split note immediately");
            pad.OpenSlot(1);
            CheckTransition(right, "pending before split slot", () => pad.Navigate(1), "split-slot jump saves the departing body");
            CheckTransition(left, "pending before hide", pad.HideResident, "hide saves a pending edit at the selected delay");

            var minimized = left.Note!;
            Edit(left, "pending before minimize");
            pad.HandleWindowStateChanged(WindowState.Minimized);
            CheckCase(store.Flush() && BodyIs(minimized, "pending before minimize"), "minimize saves a pending edit immediately");

            pad.ToggleSplit();
            var splitLeft = left.Note!;
            var splitRight = right.Note!;
            Edit(left, "left split safety save");
            Edit(right, "right split safety save");
            var now = Environment.TickCount64;
            schedules[splitLeft] = UI.SaveTiming.Edit(null, now - 5000, 5000, ChromeTokens.SaveIdleGateMs).State;
            schedules[splitRight] = UI.SaveTiming.Edit(null, now, 30000, ChromeTokens.SaveIdleGateMs).State;
            var priorLabel = savedLabel.Text;
            var priorPill = pill.Child;
            var priorTop = TopShown(); var priorBottom = BottomShown();
            var leftTextBeforeSafety = left.Editor.Document.Text;
            var rightTextBeforeSafety = right.Editor.Document.Text;
            splitLeft.Caret = (left.Editor.CaretOffset + 1) % (left.Editor.Document.TextLength + 1);
            splitRight.Caret = (right.Editor.CaretOffset + 1) % (right.Editor.Document.TextLength + 1);
            var leftCaretBeforeSafety = splitLeft.Caret;
            var rightCaretBeforeSafety = splitRight.Caret;
            pad.SaveTimerTick();
            CheckCase(store.Flush() && BodyIs(splitLeft, "left split safety save") && BodyIs(splitRight, "right split safety save"),
                "one due note saves all unsaved split notes before either note misses its delay");
            CheckCase(savedLabel.Text == priorLabel && ReferenceEquals(pill.Child, priorPill)
                && TopShown() == priorTop && BottomShown() == priorBottom
                && left.Editor.Document.Text == leftTextBeforeSafety && right.Editor.Document.Text == rightTextBeforeSafety
                && splitLeft.Caret == leftCaretBeforeSafety && splitRight.Caret == rightCaretBeforeSafety,
                "a hidden safety write performs no label, bar, caret or document work");

            var returning = left.Note!;
            var noteSaveCount = 0;
            savedHandler = note => { if (ReferenceEquals(note, returning)) noteSaveCount++; };
            store.NoteSaved += savedHandler;
            Edit(left, "switch away and back");
            pad.NewNote();
            store.Flush();
            var savedTime = returning.SavedAt;
            pad.JumpTo(returning);
            store.Flush();
            store.NoteSaved -= savedHandler;
            savedHandler = null;
            CheckCase(noteSaveCount == 1 && returning.SavedAt == savedTime,
                "switching straight back does not write the first note again");

            var quitting = left.Note!;
            Edit(left, "pending before quit");
            var quitRequested = false;
            pad.QuitRequested += () => quitRequested = true;
            pad.Quit();
            CheckCase(quitRequested && BodyIs(quitting, "pending before quit"), "quit saves a pending edit");

            var sessionPad = new UI.PadWindow(store, store.SettingsService, log, false, true, notes);
            try
            {
                var sessionLeft = (PadView)typeof(UI.PadWindow).GetField("_left", fields)!.GetValue(sessionPad)!;
                var sessionNote = sessionLeft.Note!;
                Edit(sessionLeft, "pending before session end");
                sessionPad.PrepareForSessionEnd();
                CheckCase(BodyIs(sessionNote, "pending before session end"), "session end saves a pending edit");
            }
            finally
            {
                typeof(AntiNotepad.Platform.SessionEnd).GetField("<IsEnding>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, false);
                foreach (var sessionTimer in typeof(UI.PadWindow).GetFields(fields)
                    .Select(field => field.GetValue(sessionPad)).OfType<DispatcherTimer>()) sessionTimer.Stop();
            }

            report = failures.Count == 0
                ? "blank and saved-empty behavior, per-edit delays, failure-safe labels, undo, trash, per-note hidden safety saves, transitions, minimize, switch-back and quit pass"
                : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.ToString()));
            return false;
        }
        finally
        {
            if (service is not null && pad is not null) service.Changed -= pad.HandleSettingsChanged;
            if (savedHandler is not null && store is not null) store.NoteSaved -= savedHandler;
            if (pad is not null)
                foreach (var timer in typeof(UI.PadWindow).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Select(field => field.GetValue(pad)).OfType<DispatcherTimer>()) timer.Stop();
            Anim.Reduced = reduced;
            log.Flush();
            try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
        }
    }

    private static bool NoteSwipeChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        CheckCase(UI.PadWindow.EdgeNotice(-1, 2, false) == "No older Notes", "first note notice");
        CheckCase(UI.PadWindow.EdgeNotice(2, 2, false) == "New Note Created", "new note notice");
        CheckCase(UI.PadWindow.EdgeNotice(2, 2, true) == "Already at Latest", "latest note notice");
        CheckCase(UI.PadWindow.EdgeNotice(1, 2, false) is null, "normal move has no edge notice");
        var pane = new Avalonia.Controls.Control();
        var cancelled = new UI.NoteSwipe(pane, () => pane, _ => false, _ => { });
        var committed = new UI.NoteSwipe(pane, () => pane, _ => true, _ => { });
        UI.NoteSwipe? stuck = null;
        try
        {
            cancelled.Input(1);
            CheckCase(cancelled.OwedMotion > 0.5 && cancelled.HasPendingProgress && cancelled.HasRunningTimer,
                "edge click starts a held pull without navigating");
            cancelled.Cancel();
            CheckCase(!cancelled.HasRunningTimer && !cancelled.HasPendingProgress && cancelled.OwedMotion == 0
                && pane.RenderTransform is null && pane.Opacity == 1, "cancel clears edge and restores the pane");

            var swipe = stuck = new UI.NoteSwipe(pane, () => pane, _ => false, _ => { });
            typeof(UI.NoteSwipe).GetField("_frameRunning", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(swipe, true);
            swipe.Input(1);
            CheckCase(swipe.HasPendingProgress && pane.RenderTransform is not null,
                "input keeps a held pull when a previous frame is stuck running");
            swipe.Cancel();
            CheckCase(!swipe.HasRunningTimer && !swipe.HasPendingProgress && swipe.OwedMotion == 0
                && pane.RenderTransform is null && pane.Opacity == 1,
                "cancel after a stuck frame clears the pull and restores the pane");
            swipe.Input(1);
            CheckCase(swipe.HasPendingProgress && pane.RenderTransform is not null,
                "input can pull again after cancelling a stuck frame");
            swipe.Cancel();

            var directions = new List<int>();
            committed = new UI.NoteSwipe(pane, () => pane, _ => true, directions.Add);
            committed.Input(-1);
            committed.Input(-1);
            CheckCase(directions.SequenceEqual([1]), "one wheel click commits one page and locks repeats");
            committed.Cancel();

            report = failures.Count == 0 ? "edge pull cleanup including a stuck frame, one-page click, and repeat lock" : string.Join("; ", failures);
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
            committed.Cancel();
            stuck?.Cancel();
        }
    }

    /// <summary>Exercises the real transition cleanup without creating or displaying a window.</summary>
    private static bool NoteSwitchChecks(out string report)
    {
        var failures = new List<string>();
        void Expect(bool ok, string name) { if (!ok) failures.Add(name); }
        var settings = new Settings();
        var folder = Path.Combine(Path.GetTempPath(), "AntiNotepad-swipe-" + Guid.NewGuid().ToString("N"));
        var store = new Store(folder, createDirectories: false);
        var notes = new NotesSession(store, folder, folder, null, _ => { });
        var timers = new Timers();
        var pad = new PadView(Palette.All[0], settings, timers, notes) { Private = true };
        pad.PrepareSwipe(0, 1, 0);
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
                pad.PrepareSwipe(-80, 0.6, 0);
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
            var swipe = new UI.NoteSwipe(pad, () => pad, _ => true, direction =>
            {
                calls++;
                pad.SwitchTo(first, direction);
            });
            swipe.Input(-1);
            Expect(calls == 1 && departure.Cancelled && ReferenceEquals(pad.Note, first) && Restored(),
                "second fire during departure preserves the bound selection and navigates once");
            swipe.Input(-1);
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
