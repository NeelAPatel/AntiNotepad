// Captures the design values applied to real controls for later token migrations.
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AntiNotepad.Editor;
using AntiNotepad.Model;
using AntiNotepad.Theme;
using AntiNotepad.UI;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AntiNotepad;

internal static partial class SelfTest
{
    private const string DesignHash = "ac08aea9779b8398a054f60b94cc833a9d28df467850dd5e9d662d7b3f012c22";
    private static readonly string[] DesignProperties = ["FontSize", "FontWeight", "FontFamily", "FontStyle", "CornerRadius", "Opacity", "Background", "Foreground", "BorderBrush", "BorderThickness", "BoxShadow", "Margin", "Padding"];
    private static readonly DesignTokenAllowance[] DesignTokenAllowList = [];

    private sealed record DesignTokenValue(string Path, string Property, double Value);
    private sealed record DesignTokenAllowance(string Property, double Value, string Reason);

    /// <summary>Writes the same design snapshot used by the selftest.</summary>
    internal static int WriteDesignDump(string path)
    {
        try { File.WriteAllText(path, DesignDump(), new UTF8Encoding(false)); return 0; }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    /// <summary>Checks determinism and the pinned design snapshot.</summary>
    private static bool DesignChecks(out string report)
    {
        try
        {
            var first = DesignDump();
            var second = DesignDump();
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(first))).ToLowerInvariant();
            var secondHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(second))).ToLowerInvariant();
            report = $"{hash} {first.Count(c => c == '\n')}";
            Console.WriteLine($"DESIGN dump {report}");
            if (first.Length > 0 && first == second && hash == DesignHash && SegmentBoundsChecks() && SegmentStartChecks()
                && SavedLabelChecks(out _)) return true;
            report += $" FAIL expected {DesignHash}, second {secondHash}; run --design-dump <file> for a diff";
            Console.WriteLine("DESIGN " + report);
            return false;
        }
        catch (Exception ex) { report = ex.ToString(); Console.WriteLine("DESIGN dump FAIL " + report); return false; }
    }

    /// <summary>Checks live pad, settings, search and help geometry against reflected theme tokens.</summary>
    private static bool DesignTokenChecks(out string report)
    {
        try
        {
            var values = new List<DesignTokenValue>();
            _ = DesignDump(values);
            var tokens = DesignTokenValueSet();
            var failures = new List<DesignTokenValue>();
            var allowed = new List<(DesignTokenValue Value, DesignTokenAllowance Allowance)>();
            foreach (var value in values)
            {
                if (tokens.Contains(TokenKey(value.Value))) continue;
                var exception = DesignTokenAllowList.FirstOrDefault(row => row.Property == value.Property && Near(row.Value, value.Value));
                if (exception is null) failures.Add(value);
                else allowed.Add((value, exception));
            }

            if (failures.Count == 0)
            {
                report = $"{values.Count} values, {allowed.Count} allowed";
                Console.WriteLine("DESIGN-TOKENS " + report);
                return true;
            }

            var details = failures
                .GroupBy(value => (value.Property, Key: TokenKey(value.Value)))
                .OrderBy(group => group.Key.Property, StringComparer.Ordinal)
                .ThenBy(group => group.Key.Key)
                .Take(12)
                .Select(group => $"{group.Key.Property}={FormatTokenValue(group.First().Value)} at {group.First().Path} ({group.Count()} occurrence(s))");
            report = $"FAIL {failures.Count} of {values.Count} values: " + string.Join("; ", details);
            Console.WriteLine("DESIGN-TOKENS " + report);
            return false;
        }
        catch (Exception ex)
        {
            report = ex.ToString();
            Console.WriteLine("DESIGN-TOKENS FAIL " + report);
            return false;
        }
    }

    /// <summary>Builds the numeric token set, including reflected composite constants and common half-size geometry.</summary>
    private static HashSet<long> DesignTokenValueSet()
    {
        var direct = new HashSet<double> { 0 };
        var visited = new HashSet<Type>();
        void AddType(Type type)
        {
            if (!visited.Add(type)) return;
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (!field.IsLiteral || !IsNumericType(field.FieldType)) continue;
                if (field.GetRawConstantValue() is { } raw) direct.Add(Convert.ToDouble(raw, CultureInfo.InvariantCulture));
            }
            foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)) AddType(nested);
        }

        AddType(typeof(Space));
        AddType(typeof(Shape));
        AddType(typeof(Typography));
        AddType(typeof(ChromeTokens));
        AddType(typeof(Paint));

        var tokens = direct.Select(TokenKey).ToHashSet();
        var numbers = direct.Where(double.IsFinite).ToArray();
        foreach (var value in numbers) tokens.Add(TokenKey(value / 2));
        return tokens;
    }

    private static bool IsNumericType(Type type) => Type.GetTypeCode(type) is
        TypeCode.Byte or TypeCode.SByte or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or
        TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal;

    private static long TokenKey(double value) => (long)Math.Round(value * 1000, MidpointRounding.AwayFromZero);
    private static bool Near(double first, double second) => TokenKey(first) == TokenKey(second);
    private static string FormatTokenValue(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    /// <summary>Checks whether a rebuilt selector resumes only from a recent different choice.</summary>
    private static bool SegmentStartChecks()
    {
        var old = new Rect(10, 2, 40, 20);
        var target = new Rect(52, 2, 70, 20);
        var previous = new SettingsWindow.SegmentPosition("light", old, 1000, new object());
        return SettingsWindow.SegmentStart(previous, "dark", target, 2000, false) == old
            && SettingsWindow.SegmentStart(previous, "dark", target, 2001, false) == target
            && SettingsWindow.SegmentStart(null, "dark", target, 1000, false) == target
            && SettingsWindow.SegmentStart(previous, "light", target, 1000, false) == target
            && SettingsWindow.SegmentStart(previous, "dark", target, 1000, true) == target
            && SettingsWindow.SegmentStart(previous, "dark", target, 999, false) == target;
    }

    /// <summary>Checks measured targets for unequal choices and a wrapped row.</summary>
    private static bool SegmentBoundsChecks()
    {
        var row = new WrapPanel();
        var first = new Border { Width = 40, Height = 20, Margin = new Thickness(0, 0, 2, 0) };
        var second = new Border { Width = 70, Height = 20, Margin = new Thickness(0, 0, 2, 0) };
        row.Children.Add(first); row.Children.Add(second);
        row.Measure(new Size(100, 100));
        row.Arrange(new Rect(0, 0, 100, 100));
        var origin = second.TranslatePoint(default, row);
        return origin is { } point && SettingsWindow.SegmentTarget(point, second.Bounds.Size) == new Rect(0, 20, 70, 20)
            && SettingsWindow.SegmentTarget(new Point(42, 0), new Size(70, 20)) == new Rect(42, 0, 70, 20);
    }

    /// <summary>Checks saved-label thresholds, future times, clock alignment and window visibility gating.</summary>
    private static bool SavedLabelChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        var now = new DateTime(2026, 11, 16, 14, 2, 0, DateTimeKind.Local);
        var note = new Note();
        CheckCase(PadWindow.SavedText(note, now) == "-", "never-saved note");
        note.SavedAt = now.AddMilliseconds(-999);
        CheckCase(PadWindow.SavedText(note, now) == "Saved just now", "under one second");
        note.SavedAt = now.AddSeconds(-1);
        CheckCase(PadWindow.SavedText(note, now) == "Saved 1s ago", "one second");
        note.SavedAt = now.AddSeconds(-59);
        CheckCase(PadWindow.SavedText(note, now) == "Saved 59s ago", "fifty-nine seconds");
        note.SavedAt = now.AddMinutes(-1);
        CheckCase(PadWindow.SavedText(note, now) == "Saved 1m ago", "one minute");
        note.SavedAt = now.AddMinutes(-4).AddSeconds(-59);
        CheckCase(PadWindow.SavedText(note, now) == "Saved 4m ago", "four minutes");
        note.SavedAt = now.AddMinutes(-5);
        CheckCase(PadWindow.SavedText(note, now) == "Saved 13:57", "five-minute timestamp");
        note.SavedAt = now;
        CheckCase(PadWindow.SavedText(note, now.AddMinutes(5)) == "Saved 14:02", "timestamp uses the save time");
        note.SavedAt = now.AddDays(-1);
        CheckCase(PadWindow.SavedText(note, now) == "Saved Nov 15, 14:02", "date on a prior day");
        note.SavedAt = now.AddYears(-1).AddDays(-1);
        CheckCase(PadWindow.SavedText(note, now) == "Saved Nov 15, 2025 14:02", "date on a prior year");
        note.SavedAt = now.AddSeconds(5);
        CheckCase(PadWindow.SavedText(note, now) == "Saved just now", "future timestamp");
        note.SavedAt = now.AddMinutes(-2);
        var previousLabel = PadWindow.SavedText(note, now);
        note.Modified = now;
        CheckCase(PadWindow.SavedText(note, now) == previousLabel, "pending or failed edit keeps the previous save time");

        var savedAt = now;
        CheckCase(PadWindow.SavedLabelRefreshInterval(savedAt, now.AddMilliseconds(250))
            == TimeSpan.FromMilliseconds(ChromeTokens.SavedLabelRefreshMs - 250),
            "clock aligns to the next saved-time second");
        CheckCase(PadWindow.SavedLabelRefreshInterval(savedAt, now.AddMilliseconds(999)) == TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond),
            "clock handles a nearly reached second");
        var futureRefresh = PadWindow.SavedLabelRefreshInterval(savedAt.AddHours(1), now);
        Console.WriteLine($"SAVE DELAY REGRESSION future label refresh: savedAtInFuture=True intervalMs={futureRefresh.TotalMilliseconds:F0}");
        CheckCase(futureRefresh == TimeSpan.FromMilliseconds(ChromeTokens.SavedLabelRefreshMs),
            "future timestamps refresh just now labels every second");
        note.SavedAt = now.AddSeconds(-3);
        CheckCase(PadWindow.ShouldRunSavedLabelClock(true, false)
            && PadWindow.SavedText(note, now) == "Saved 3s ago", "visible fade or hidden-bar label counts without a bar event");
        CheckCase(!PadWindow.ShouldRunSavedLabelClock(false, false)
            && !PadWindow.ShouldRunSavedLabelClock(true, true), "clock stops while hidden or minimized");

        report = failures.Count == 0
            ? "saved label table, prior dates, future time, aligned clock and window visibility pass"
            : string.Join("; ", failures);
        return failures.Count == 0;
    }

    /// <summary>Builds the hidden controls with isolated data and snapshots each palette.</summary>
    private static string DesignDump(List<DesignTokenValue>? tokenValues = null)
    {
        if (Application.Current is null) AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();
        App.ConfigureFluentStyles(Application.Current!);
        var dir = Path.Combine(Path.GetTempPath(), $"antinotepad-design-{Guid.NewGuid():N}");
        var lines = new List<string>();
        try
        {
            foreach (var palette in Palette.All)
            {
                var store = new Store(Path.Combine(dir, palette.Id));
                store.Load();
                var settings = store.SettingsService;
                settings.Current.Theme = palette.Dark ? "dark" : "light";
                if (palette.Dark) settings.Current.DarkPalette = palette.Id; else settings.Current.LightPalette = palette.Id;
                Welcome.SeedIfNew(store.Data, settings);
                using var log = new RuntimeLog();
                var notes = new NotesSession(store, dir, dir, null, log.Write);
                var pad = new PadWindow(store, settings, log, false, true, notes);
                var window = new SettingsWindow(settings, pad, true, notes);
                Capture(pad, palette.Id + "/pad", lines, tokenValues);
                foreach (var section in new[] { "theme", "ui", "editor", "feel", "system", "storage", "about" })
                {
                    window.ShowSection(section);
                    Capture(window, palette.Id + "/settings/" + section, lines, tokenValues);
                }
                Capture(new HelpOverlay(pad, palette), palette.Id + "/help", lines, tokenValues);
                Capture(new SearchOverlay(pad, palette, store, "", false), palette.Id + "/search", lines, tokenValues);
                IReadOnlyList<MenuEntry> entries = [new("New note", "Ctrl+N", Icons.NewNote), new("Search notes", "Ctrl+F", Icons.Search), new("Split view", "Ctrl+OemBackslash", Icons.Split), new("Keep on top", "Ctrl+T", Icons.KeepOnTop), MenuEntry.Separator, new("Settings", "Ctrl+OemComma", Icons.Settings), new($"Hide {AppInfo.Name}", "Escape")];
                var builder = typeof(PadMenu).GetMethod("BuildMenu", BindingFlags.NonPublic | BindingFlags.Static)!;
                var menu = (Border)builder.Invoke(null, [entries, palette, settings.Current, (Action)(() => { })])!;
                Capture(menu, palette.Id + "/menu", lines);
            }
            lines.Sort(StringComparer.Ordinal);
            return string.Join('\n', lines) + "\n";
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    /// <summary>Records control values and their stable logical-tree paths.</summary>
    private static void Capture(ILogical node, string parent, List<string> lines, List<DesignTokenValue>? tokenValues = null)
    {
        var path = parent + "/" + node.GetType().Name;
        if (node is Control control)
        {
            if (tokenValues is not null) CollectDesignTokenValues(control, path, tokenValues);
            var values = new List<string>();
            foreach (var name in DesignProperties)
            {
                var property = control.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (property is null || property.GetIndexParameters().Length != 0) continue;
                var value = property.GetValue(control);
                var formatted = value switch
                {
                    ISolidColorBrush brush => $"{brush.Color}@{brush.Opacity.ToString(CultureInfo.InvariantCulture)}",
                    IGradientBrush brush => $"{brush.GetType().Name}@{brush.Opacity.ToString(CultureInfo.InvariantCulture)}({string.Join(",", brush.GradientStops.Select(stop => $"{stop.Color}@{stop.Offset.ToString(CultureInfo.InvariantCulture)}"))})",
                    null => "null",
                    _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null"
                };
                values.Add(name + "=" + formatted);
            }
            lines.Add(path + " | " + string.Join("; ", values));
        }
        var index = 0;
        foreach (var child in node.GetLogicalChildren()) Capture(child, path + "[" + index++ + "]", lines, tokenValues);
    }

    /// <summary>Collects scalar typography, shape, spacing and shadow values while the snapshot walk visits each control.</summary>
    private static void CollectDesignTokenValues(Control control, string path, List<DesignTokenValue> values)
    {
        void Add(string property, double value) => values.Add(new DesignTokenValue(path, property, value));
        object? Property(string name) => control.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)?.GetValue(control);
        if (Property("FontSize") is double fontSize) Add("FontSize", fontSize);
        if (Property("CornerRadius") is CornerRadius radius)
        {
            Add("CornerRadius.TopLeft", radius.TopLeft);
            Add("CornerRadius.TopRight", radius.TopRight);
            Add("CornerRadius.BottomRight", radius.BottomRight);
            Add("CornerRadius.BottomLeft", radius.BottomLeft);
        }
        if (Property("Margin") is Thickness margin) AddThickness("Margin", margin, Add);
        if (Property("Padding") is Thickness padding) AddThickness("Padding", padding, Add);
        if (Property("BorderThickness") is Thickness borderThickness) AddThickness("BorderThickness", borderThickness, Add);
        if (Property("BoxShadow") is BoxShadows shadows)
        {
            for (var i = 0; i < shadows.Count; i++)
            {
                var shadow = shadows[i];
                Add($"BoxShadow[{i}].OffsetX", shadow.OffsetX);
                Add($"BoxShadow[{i}].OffsetY", shadow.OffsetY);
                Add($"BoxShadow[{i}].Blur", shadow.Blur);
                Add($"BoxShadow[{i}].Spread", shadow.Spread);
                Add($"BoxShadow[{i}].ColorAlpha", shadow.Color.A);
            }
        }
    }

    private static void AddThickness(string property, Thickness thickness, Action<string, double> add)
    {
        add(property + ".Left", thickness.Left);
        add(property + ".Top", thickness.Top);
        add(property + ".Right", thickness.Right);
        add(property + ".Bottom", thickness.Bottom);
    }

    /// <summary>Exercises the production layout helpers across bar modes and overlay lifetimes.</summary>
    private static bool SlidingOverlayTopBandChecks(out string report)
    {
        var failures = new List<string>();
        var root = new Grid();
        var content = new Grid { RowDefinitions = new RowDefinitions($"{ChromeTokens.DockedRowHeight},*") };
        var host = new Panel { IsVisible = true };
        Grid.SetRow(host, 1);
        content.Children.Add(host);
        root.Children.Add(content);
        var band = PadWindow.AddOverlayDragBand(root);
        var top = new Panel { IsHitTestVisible = false };
        var bottom = new Panel();
        PadWindow.AddChromeLayers(root, top, bottom, new Border(), new Border(), new Border());

        foreach (var mode in new[] { "docked", "overlay", "float", "docked" })
        {
            var sliding = mode is "overlay" or "float";
            content.RowDefinitions[0].Height = new GridLength(sliding ? 0 : ChromeTokens.DockedRowHeight);
            PadWindow.ApplyOverlayLayout(host, band, sliding, draggable: true);
            root.Measure(new Size(640, 400));
            root.Arrange(new Rect(0, 0, 640, 400));
            var hostTop = host.TranslatePoint(default, root)?.Y;
            if (hostTop != ChromeTokens.DockedRowHeight || host.Margin.Top != (sliding ? ChromeTokens.DockedRowHeight : 0))
                failures.Add(mode + " clearance");
            if (band.IsVisible != sliding || (sliding && band.Bounds.Height != ChromeTokens.DockedRowHeight))
                failures.Add(mode + " drag visibility/height");
            if (sliding && (!band.IsHitTestVisible || band.Background is null || band.Bounds.Y != 0
                || WindowDecorationProperties.GetElementRole(band) != WindowDecorationsElementRole.TitleBar))
                failures.Add(mode + " drag role/geometry");
        }

        var layering = root.Children.IndexOf(band) > root.Children.IndexOf(content)
            && root.Children.IndexOf(band) < root.Children.IndexOf(top)
            && root.Children.IndexOf(band) < root.Children.IndexOf(bottom);
        if (!layering) failures.Add("drag band layer order");
        PadWindow.ApplyOverlayLayout(host, band, sliding: true, draggable: false);
        if (band.IsVisible) failures.Add("save prompt owns outside clicks");
        PadWindow.ApplyOverlayLayout(host, band, sliding: true, draggable: true);
        if (!band.IsVisible) failures.Add("Help/Search drag restored");
        host.IsVisible = false;
        PadWindow.ApplyOverlayLayout(host, band, sliding: true, draggable: true);
        if (band.IsVisible) failures.Add("closed overlay releases band");
        host.IsVisible = true;
        PadWindow.ApplyOverlayLayout(host, band, sliding: true, draggable: true);
        if (!band.IsVisible) failures.Add("reopened overlay restores band");
        report = failures.Count == 0 ? "mode round trip, clearance, title-bar band, layering and input ownership pass" : string.Join("; ", failures);
        return failures.Count == 0;
    }
}
