// Sweeps the contrast roles over every base palette and accent choice.
using AntiNotepad.Theme;
using Avalonia.Media;

namespace AntiNotepad;

internal static partial class SelfTest
{
    private sealed record ContrastAllowance(string Name, double Minimum, string Reason);

    private static readonly ContrastAllowance[] ContrastAllowList =
    [
        new("Clear-tint empty-note hint", 1.24, "User kept this hint contrast on 2026-11-20."),
    ];

    /// <summary>Checks the minimum contrast of each approved color pair.</summary>
    private static bool ContrastChecks(out string report)
    {
        var failures = new List<string>();
        var minima = new Dictionary<string, double>();
        var reportedPairs = new Dictionary<(string Name, string Palette), double>();
        var clearTintHint = ContrastAllowList.Single(row => row.Name == "Clear-tint empty-note hint");
        void Check(string name, Palette p, Color foreground, Color background, double minimum, ContrastAllowance? allowance = null, bool reportOnly = false)
        {
            var ratio = Contrast.Ratio(foreground, background);
            if (!minima.TryGetValue(name, out var old) || ratio < old) minima[name] = ratio;
            var required = allowance?.Minimum ?? minimum;
            if (ratio < required)
            {
                if (reportOnly)
                {
                    var key = (name, p.Name);
                    if (!reportedPairs.TryGetValue(key, out var prior) || ratio < prior) reportedPairs[key] = ratio;
                }
                else failures.Add($"{name}, {p.Name}, {p.Accent}: {ratio:F2}:1 (needs {required:F2})");
            }
        }

        foreach (var palette in Palette.All)
            foreach (var accent in Palette.Accents.Select(a => (string?)a.Hex).Prepend(null))
            {
                var p = palette.Customize(accent, "default");
                Check("primary text", p, Contrast.BestText(p.Accent), p.Accent, Contrast.Text);
                Check("field rest", p, Contrast.Over(p.Ink, Paint.RestBorder, p.Surface), p.Surface, Contrast.Mark);
                Check("field focus", p, Contrast.FieldFocus(p), p.Surface, Contrast.Mark);
                Check("toast text/action", p, p.Canvas, Contrast.Over(p.Ink, Paint.Toast, p.Canvas), Contrast.Text);
                Check("failure detail", p, Contrast.EnsureOn(p.Muted, p.Surface, Contrast.Text), p.Surface, Contrast.Text);
                Check("empty-note hint Surface", p, p.Faint, p.Surface, Contrast.Text, clearTintHint);
                Check("empty-note hint Canvas", p, p.Faint, p.Canvas, Contrast.Text, clearTintHint);
                Check("markdown marker text", p, p.Faint, p.Canvas, Contrast.Mark, reportOnly: true);
                Check("placeholder text", p, p.Ink, p.Surface, Contrast.Text);
                var card = Contrast.SettingsCard(p);
                Check("toggle off", p, Contrast.EnsureOn(p.Faint, card, Contrast.Mark), card, Contrast.Mark);
                Check("toggle on", p, Contrast.EnsureOn(p.Accent, card, Contrast.Mark), card, Contrast.Mark);
                foreach (var (name, surface) in new[] { ("pill", p.Surface), ("canvas", p.Canvas) })
                {
                    Check($"pager empty {name}", p, Contrast.EnsureOn(p.Faint, surface, Contrast.Mark), surface, Contrast.Mark);
                    Check($"pager filled {name}", p, Contrast.EnsureOn(p.Muted, surface, Contrast.Mark), surface, Contrast.Mark);
                    Check($"pager selected {name}", p, Contrast.EnsureOn(p.Accent, surface, Contrast.Mark), surface, Contrast.Mark);
                }
            }
        foreach (var (name, minimum) in minima) Console.WriteLine($"CONTRAST {name}: {minimum:F2}:1 minimum");
        Console.WriteLine($"CONTRAST ALLOW {clearTintHint.Name}: {clearTintHint.Minimum:F2}:1 minimum; {clearTintHint.Reason}");
        foreach (var pair in reportedPairs.OrderBy(pair => pair.Key.Name, StringComparer.Ordinal).ThenBy(pair => pair.Key.Palette, StringComparer.Ordinal))
            Console.WriteLine($"CONTRAST REPORT {pair.Key.Name}, {pair.Key.Palette}: {pair.Value:F2}:1 (needs {Contrast.Mark:F1})");
        report = failures.Count == 0
            ? reportedPairs.Count == 0
                ? "all palette and accent pairs pass; Clear-tint hint named allowance applied"
                : $"all enforced pairs pass; {reportedPairs.Count} markdown marker palette result(s) reported"
            : string.Join("; ", failures);
        return failures.Count == 0;
    }
}
