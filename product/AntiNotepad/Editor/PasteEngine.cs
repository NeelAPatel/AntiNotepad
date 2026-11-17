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
        var tags = new Stack<string>();
        var output = new List<(string Prefix, StringBuilder Text, bool Block)>();
        StringBuilder? block = null;
        var position = 0;
        var hasItem = false;
        foreach (Match token in Regex.Matches(html, @"<!--[\s\S]*?-->|<[^>]*>|[^<]+"))
        {
            if (token.Index != position) return null;
            position += token.Length;
            var value = token.Value;
            if (!value.StartsWith('<'))
            {
                var decoded = WebUtility.HtmlDecode(value);
                if (items.Count > 0) items.Peek().Append(decoded);
                else if (block != null) block.Append(decoded);
                else if (!string.IsNullOrWhiteSpace(decoded)) return null;
                continue;
            }
            if (value.StartsWith("<!--")) continue;
            var tag = Regex.Match(value, @"^<\s*(/?)\s*([a-zA-Z0-9]+)\b[^>]*>$");
            if (!tag.Success) return null;
            var closing = tag.Groups[1].Value.Length != 0;
            var name = tag.Groups[2].Value.ToLowerInvariant();
            var isBlock = name is "p" or "div" or "blockquote" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6";
            var isVoid = name is "br" or "hr" or "img" or "meta" or "link" or "input";
            if (closing)
            {
                if (isVoid || tags.Count == 0 || tags.Pop() != name) return null;
            }
            else if (!isVoid && !value.EndsWith("/>")) tags.Push(name);
            else if (!isVoid) return null;

            if (name is "ul" or "ol")
            {
                if (!closing)
                {
                    if (lists.Count > 0 && items.Count != lists.Count) return null;
                    block = null;
                    lists.Push((name, 0));
                }
                else
                {
                    if (lists.Count == 0 || lists.Pop().Kind != name) return null;
                    if (lists.Count == 0 && tags.Any(x => x is "div" or "blockquote"))
                    {
                        block = new StringBuilder();
                        output.Add(("", block, true));
                    }
                }
            }
            else if (name == "li")
            {
                if (!closing)
                {
                    if (lists.Count == 0 || items.Count != lists.Count - 1) return null;
                    var list = lists.Pop(); list.Count++; lists.Push(list);
                    var text = new StringBuilder(); items.Push(text);
                    output.Add((new string(' ', (lists.Count - 1) * 2) + (list.Kind == "ul" ? "- " : $"{list.Count}. "), text, false));
                    hasItem = true;
                }
                else if (items.Count == 0) return null; else items.Pop();
            }
            else if (isBlock && items.Count == 0)
            {
                block = null;
                if (!closing)
                {
                    block = new StringBuilder();
                    output.Add(("", block, true));
                }
                else if (tags.Any(x => x is "div" or "blockquote"))
                {
                    block = new StringBuilder();
                    output.Add(("", block, true));
                }
            }
            else if (name == "br" && !closing)
            {
                if (items.Count > 0) items.Peek().Append('\n');
                else block?.Append('\n');
            }
        }
        if (position != html.Length || tags.Count != 0 || lists.Count != 0 || items.Count != 0 || !hasItem) return null;
        var lines = new List<string>();
        var words = new List<string>();
        var previousBlock = false;
        foreach (var item in output)
        {
            var content = item.Block
                ? string.Join("\n", item.Text.ToString().Split('\n').Select(x => Regex.Replace(x, @"[\t\r ]+", " ").Trim())).Trim()
                : Regex.Replace(item.Text.ToString(), @"[\t\r\n ]+", " ").Trim();
            if (item.Block && content.Length == 0) continue;
            if (lines.Count > 0 && (item.Block || previousBlock)) lines.Add("");
            lines.Add(item.Prefix + content);
            words.Add(content);
            previousBlock = item.Block;
        }
        var plainWords = Regex.Replace(snapshot.Plain, @"\s+", " ").Trim();
        var htmlWords = Regex.Replace(string.Join(" ", words), @"\s+", " ").Trim();
        return plainWords == htmlWords ? string.Join("\n", lines) : null;
    }
}
