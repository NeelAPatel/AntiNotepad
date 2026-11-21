// Sweeps the contrast roles over every base palette and accent choice.
using AntiNotepad.Theme;
using Avalonia.Media;

namespace AntiNotepad;

internal static partial class SelfTest
{
    /// <summary>Checks the minimum contrast of each approved color pair.</summary>
    private static bool ContrastChecks(out string report)
    {
        var failures = new List<string>();
        var minima = new Dictionary<string, double>();
        void Check(string name, Palette p, Color foreground, Color background, double minimum)
        {
            var ratio = Contrast.Ratio(foreground, background);
            if (!minima.TryGetValue(name, out var old) || ratio < old) minima[name] = ratio;
            if (ratio < minimum) failures.Add($"{name}, {p.Name}, {p.Accent}: {ratio:F2}:1 (needs {minimum:F1})");
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
        report = failures.Count == 0 ? "all palette and accent pairs pass" : string.Join("; ", failures);
        return failures.Count == 0;
    }
}
