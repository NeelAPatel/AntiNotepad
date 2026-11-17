// Converts supported clipboard structure into note text.
using System.Text;
using System.Text.RegularExpressions;

namespace AntiNotepad.Editor;

internal sealed record ClipSnapshot(string? Plain, string? Html, string[] Formats, bool HasImage, string[] Files);

internal readonly record struct PasteOutcome(string? Text, bool StructureSeen);

internal static class PasteEngine
{
    private static readonly Func<ClipSnapshot, string?>[] Readers = [ImageReader, FilesReader, HtmlReader];
    internal static string? Upgrade(ClipSnapshot snapshot) => Analyze(snapshot).Text;

    internal static PasteOutcome Analyze(ClipSnapshot snapshot)
    {
        var structureSeen = snapshot.Html is { } html &&
            Regex.IsMatch(html, @"<\s*(?:li|table)(?=[\s/>])", RegexOptions.IgnoreCase);
        try
        {
            foreach (var reader in Readers)
                if (reader(snapshot) is { } text) return new(text, structureSeen);
        }
        catch (Exception) { }
        return new(null, structureSeen);
    }

    internal static bool IsPreformatted(ClipSnapshot snapshot) =>
        (snapshot.Plain is { } plain && Regex.IsMatch(plain, @"(?m)^[ \t]+\S|^\s*(?:[-*+•]|\d+[.)])\s+")) ||
        (snapshot.Html is { } html && Regex.IsMatch(html, @"<\s*pre\b|white-space\s*:\s*pre", RegexOptions.IgnoreCase));

    private static string? ImageReader(ClipSnapshot snapshot) => null;
    private static string? FilesReader(ClipSnapshot snapshot) => null;
    private static string? HtmlReader(ClipSnapshot snapshot)
    {
        if (string.IsNullOrEmpty(snapshot.Plain) || string.IsNullOrEmpty(snapshot.Html) || snapshot.Html.Length > 1_000_000) return null;
        if (IsPreformatted(snapshot)) return null;
        var html = snapshot.Html;
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
