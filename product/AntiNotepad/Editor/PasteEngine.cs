// Converts supported clipboard structure into note text.
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AntiNotepad.Editor;

internal sealed record ClipSnapshot(string? Plain, string? Html, string[] Formats, bool HasImage, string[] Files);

internal static class PasteEngine
{
    private static readonly Func<ClipSnapshot, string?>[] Readers = [ImageReader, FilesReader, HtmlListReader];

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

    private static string? HtmlListReader(ClipSnapshot snapshot)
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
        var lists = new Stack<(string Kind, int Count)>();
        var items = new Stack<StringBuilder>();
        var lines = new List<string>();
        var output = new List<(string Prefix, StringBuilder Text)>();
        foreach (Match token in Regex.Matches(html, @"<!--[\s\S]*?-->|<[^>]*>|[^<]+"))
        {
            var value = token.Value;
            if (!value.StartsWith('<'))
            {
                var decoded = WebUtility.HtmlDecode(value);
                if (items.Count > 0) items.Peek().Append(decoded);
                else if (!string.IsNullOrWhiteSpace(decoded)) return null;
                continue;
            }
            var tag = Regex.Match(value, @"^<\s*(/?)\s*([a-zA-Z0-9]+)\b[^>]*>$");
            if (!tag.Success) { if (!value.StartsWith("<!--")) return null; continue; }
            var closing = tag.Groups[1].Value.Length != 0;
            var name = tag.Groups[2].Value.ToLowerInvariant();
            if (name is "ul" or "ol")
            {
                if (!closing) lists.Push((name, 0));
                else if (lists.Count == 0 || lists.Pop().Kind != name) return null;
            }
            else if (name == "li")
            {
                if (!closing)
                {
                    if (lists.Count == 0) return null;
                    var list = lists.Pop(); list.Count++; lists.Push(list);
                    var text = new StringBuilder(); items.Push(text);
                    output.Add((new string(' ', (lists.Count - 1) * 2) + (list.Kind == "ul" ? "- " : $"{list.Count}. "), text));
                }
                else if (items.Count == 0) return null; else items.Pop();
            }
            else if (name == "br" && items.Count > 0) items.Peek().Append('\n');
        }
        if (lists.Count != 0 || items.Count != 0 || output.Count == 0) return null;
        foreach (var item in output) lines.Add(item.Prefix + Regex.Replace(item.Text.ToString(), @"[\t\r\n ]+", " ").Trim());
        var plainWords = Regex.Replace(snapshot.Plain, @"\s+", " ").Trim();
        var htmlWords = Regex.Replace(string.Join(" ", output.Select(x => x.Text.ToString())), @"\s+", " ").Trim();
        return plainWords == htmlWords ? string.Join("\n", lines) : null;
    }
}
