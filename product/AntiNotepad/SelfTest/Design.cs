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
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;

namespace AntiNotepad;

internal static partial class SelfTest
{
    private const string DesignHash = "1032fadd9067c61f4ce6299ad46b414464a0fb383a95827eaa79960cdb5c716c";
    private static readonly string[] DesignProperties = ["FontSize", "FontWeight", "FontFamily", "FontStyle", "CornerRadius", "Opacity", "Background", "Foreground", "BorderBrush", "BorderThickness", "BoxShadow", "Margin", "Padding"];

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
            if (first.Length > 0 && first == second && hash == DesignHash) return true;
            report += $" FAIL expected {DesignHash}, second {secondHash}; run --design-dump <file> for a diff";
            Console.WriteLine("DESIGN " + report);
            return false;
        }
        catch (Exception ex) { report = ex.ToString(); Console.WriteLine("DESIGN dump FAIL " + report); return false; }
    }

    /// <summary>Builds the hidden controls with isolated data and snapshots each palette.</summary>
    private static string DesignDump()
    {
        if (Application.Current is null) AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting();
        if (!Application.Current!.Styles.OfType<Avalonia.Themes.Fluent.FluentTheme>().Any())
        {
            Application.Current.Styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
            Application.Current.Styles.Add(new StyleInclude(new Uri(AppInfo.ResourceUri)) { Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml") });
        }
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
                Capture(pad, palette.Id + "/pad", lines);
                foreach (var section in new[] { "theme", "ui", "editor", "feel", "system", "storage", "about" })
                {
                    window.ShowSection(section);
                    Capture(window, palette.Id + "/settings/" + section, lines);
                }
                Capture(new HelpOverlay(pad, palette), palette.Id + "/help", lines);
                Capture(new SearchOverlay(pad, palette, store, "", false), palette.Id + "/search", lines);
                IReadOnlyList<MenuEntry> entries = [new("New note", "Ctrl+N", MenuGlyphs.NewNote), new("Search notes", "Ctrl+F", MenuGlyphs.Search), new("Split view", "Ctrl+OemBackslash", MenuGlyphs.Split), new("Keep on top", "Ctrl+T", MenuGlyphs.Pin), MenuEntry.Separator, new("Settings", "Ctrl+OemComma", MenuGlyphs.Settings), new($"Hide {AppInfo.Name}", "Escape")];
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
    private static void Capture(ILogical node, string parent, List<string> lines)
    {
        var path = parent + "/" + node.GetType().Name;
        if (node is Control control)
        {
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
        foreach (var child in node.GetLogicalChildren()) Capture(child, path + "[" + index++ + "]", lines);
    }
}
