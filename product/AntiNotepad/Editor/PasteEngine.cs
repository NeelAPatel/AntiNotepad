// Converts supported clipboard structure into note text.
using System.Text;
using System.Text.RegularExpressions;

namespace AntiNotepad.Editor;

internal sealed record ClipSnapshot(string? Plain, string? Html, string[] Formats, bool HasImage, string[] Files);

internal static class PasteEngine
{
    private static readonly Func<ClipSnapshot, string?>[] Readers = [ImageReader, FilesReader, HtmlReader];
    internal static string? Upgrade(ClipSnapshot snapshot)
    {
        try
        {
            foreach (var reader in Readers)
                if (reader(snapshot) is { } text) return text;
        }
        catch (Exception) { }
        return null;
    }
    private static string? ImageReader(ClipSnapshot snapshot) => null;
    private static string? FilesReader(ClipSnapshot snapshot) => null;
    private static string? HtmlReader(ClipSnapshot snapshot)
    {
        if (string.IsNullOrEmpty(snapshot.Plain) || string.IsNullOrEmpty(snapshot.Html)) return null;
        if (Regex.IsMatch(snapshot.Plain, @"(?m)^[ \t]+\S|^\s*(?:[-*+•]|\d+[.)])\s+")) return null;
        var html = snapshot.Html;
        if (Regex.IsMatch(html, @"<\s*pre\b|white-space\s*:\s*pre", RegexOptions.IgnoreCase)) return null;
        var start = Regex.Match(html, @"StartFragment:(\d+)");
        var end = Regex.Match(html, @"EndFragment:(\d+)");
        if (start.Success && end.Success && int.TryParse(start.Groups[1].Value, out var a) && int.TryParse(end.Groups[1].Value, out var b))
        {
            var bytes = Encoding.UTF8.GetBytes(html);
            if (a >= 0 && b >= a && b <= bytes.Length) html = Encoding.UTF8.GetString(bytes, a, b - a);
        }
        else if (html.Contains("<!--StartFragment-->"))
        {
            var a2 = html.IndexOf("<!--StartFragment-->", StringComparison.Ordinal) + "<!--StartFragment-->".Length;
            var b2 = html.IndexOf("<!--EndFragment-->", a2, StringComparison.Ordinal);
            if (b2 >= a2) html = html[a2..b2];
        }
        var root = PasteHtml.Parse(html);
        if (root == null) return null;
        var result = PasteHtml.Convert(root);
        if (result == null || !result.Value.Structured) return null;
        static string Words(string value) => Regex.Replace(value, @"\s+", " ").Trim();
        return Words(snapshot.Plain) == Words(result.Value.Words) ? result.Value.Text : null;
    }
}
