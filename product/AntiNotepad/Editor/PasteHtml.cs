// Parses clipboard HTML and converts its blocks and inline formatting to note text.
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AntiNotepad.Editor;

internal static class PasteHtml
{
    internal sealed class Node(string name, string text = "")
    {
        internal string Name = name;
        internal string Text = text;
        internal Dictionary<string, string> Attributes = [];
        internal List<Node> Children = [];
    }

    internal static Node? Parse(string html)
    {
        var root = new Node("root");
        var stack = new List<Node> { root };
        var pos = 0;
        while (pos < html.Length)
        {
            if (html[pos] != '<')
            {
                var end = html.IndexOf('<', pos);
                if (end < 0) end = html.Length;
                stack[^1].Children.Add(new Node("#text", WebUtility.HtmlDecode(html[pos..end])));
                pos = end;
                continue;
            }
            if (html.AsSpan(pos).StartsWith("<!--"))
            {
                var end = html.IndexOf("-->", pos + 4, StringComparison.Ordinal);
                if (end < 0) return null;
                pos = end + 3;
                continue;
            }
            var match = Regex.Match(html[pos..], "^<\\s*(/?)\\s*([a-zA-Z][a-zA-Z0-9-]*)(?:\\s+((?:[^>\"']|\"[^\"]*\"|'[^']*')*))?\\s*(/?)>");
            if (!match.Success) return null;
            pos += match.Length;
            var name = match.Groups[2].Value.ToLowerInvariant();
            var closing = match.Groups[1].Length > 0;
            if (closing)
            {
                while (stack.Count > 1 && stack[^1].Name != name && Optional(stack[^1].Name)) stack.RemoveAt(stack.Count - 1);
                if (stack.Count == 1 || stack[^1].Name != name) return null;
                stack.RemoveAt(stack.Count - 1);
                continue;
            }
            while (stack.Count > 1 && Closes(stack[^1].Name, name)) stack.RemoveAt(stack.Count - 1);
            var node = new Node(name);
            foreach (Match attr in Regex.Matches(match.Groups[3].Value, "([a-zA-Z_:][a-zA-Z0-9_:.-]*)\\s*=\\s*(?:\"([^\"]*)\"|'([^']*)'|([^\\s/>]+))"))
                node.Attributes[attr.Groups[1].Value.ToLowerInvariant()] = WebUtility.HtmlDecode(attr.Groups[2].Success ? attr.Groups[2].Value : attr.Groups[3].Success ? attr.Groups[3].Value : attr.Groups[4].Value);
            stack[^1].Children.Add(node);
            if (name is not ("br" or "hr" or "img" or "meta" or "link" or "input" or "wbr") && match.Groups[4].Length == 0) stack.Add(node);
        }
        return stack.Count == 1 ? root : null;
    }
    private static bool Optional(string name) => name is "p" or "li" or "td" or "th" or "tr";
    private static bool Closes(string open, string next) => (open == "p" && next is "p" or "div" or "ul" or "ol" or "table" or "li" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6") || (open == "li" && next == "li") || (open is "td" or "th" && next is "td" or "th" or "tr") || (open == "tr" && next == "tr");

    internal static string Inline(Node node, bool formatted)
    {
        if (node.Name == "#text") return node.Text;
        if (node.Name == "br") return "\n";
        var value = string.Concat(node.Children.Select(x => Inline(x, formatted)));
        if (!formatted) return value;
        if (node.Name == "a" && node.Attributes.TryGetValue("href", out var href) && (href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || href.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)) && value.Trim() != href) value += " (" + href + ")";
        var mark = node.Name switch { "strong" or "b" => "**", "em" or "i" => "*", "u" => "++", "s" or "del" or "strike" => "~~", "code" when !value.Contains('`') => "`", _ => "" };
        if (mark.Length == 0 || string.IsNullOrWhiteSpace(value)) return value;
        var trimmed = value.Trim();
        var start = value.IndexOf(trimmed, StringComparison.Ordinal);
        return value[..start] + mark + trimmed + mark + value[(start + trimmed.Length)..];
    }
    internal static string Clean(string value, bool multiline = false) => multiline
        ? string.Join("\n", value.Split('\n').Select(x => Regex.Replace(x, @"[\t\r ]+", " ").Trim())).Trim()
        : Regex.Replace(value, @"\s+", " ").Trim();

    internal static (string Text, string Words, bool Structured)? Convert(Node root)
    {
        var blocks = new List<(string Text, bool Separate)>();
        var words = new List<string>();
        var structured = Descendants(root).Any(x => x.Name is "strong" or "b" or "em" or "i" or "u" or "s" or "del" or "strike" or "code" or "a" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6");
        void Add(string text, string raw, bool separate, bool empty = false)
        {
            if (text.Length == 0 && !empty) return;
            blocks.Add((text, separate));
            if (raw.Length > 0) words.Add(raw);
        }
        void List(Node list, int depth)
        {
            var count = 0;
            foreach (var item in list.Children.Where(x => x.Name == "li"))
            {
                count++;
                var content = new Node("content");
                content.Children.AddRange(item.Children.Where(x => x.Name is not ("ul" or "ol")));
                if (Descendants(content).Any(x => x.Name == "table")) throw new FormatException();
                var prefix = new string(' ', depth * 2) + (list.Name == "ul" ? "- " : $"{count}. ");
                Add(prefix + Clean(Inline(content, true)), Clean(Inline(content, false)), false, true);
                foreach (var nested in item.Children.Where(x => x.Name is "ul" or "ol")) List(nested, depth + 1);
            }
        }
        void Walk(Node node)
        {
            if (node.Name is "ul" or "ol") { structured = true; List(node, 0); return; }
            if (node.Name == "table")
            {
                var table = PasteTable.Convert(node);
                if (table == null) throw new FormatException();
                structured = true;
                Add(table.Value.Text, table.Value.Words, true);
                return;
            }
            if (node.Name is "root" or "div" or "blockquote")
            {
                var pending = new Node("content");
                void Flush() { Add(Clean(Inline(pending, true), true), Clean(Inline(pending, false), true), true); pending.Children.Clear(); }
                foreach (var child in node.Children)
                {
                    if (child.Name is "p" or "div" or "blockquote" or "ul" or "ol" or "table" or "li" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6") { Flush(); Walk(child); }
                    else pending.Children.Add(child);
                }
                Flush(); return;
            }
            var text = Clean(Inline(node, true), true);
            if (node.Name.Length == 2 && node.Name[0] == 'h' && node.Name[1] is >= '1' and <= '6') text = new string('#', node.Name[1] - '0') + " " + text;
            Add(text, Clean(Inline(node, false), true), true);
        }
        Walk(root);
        var output = new StringBuilder();
        for (var i = 0; i < blocks.Count; i++)
        {
            if (i > 0) output.Append(blocks[i].Separate || blocks[i - 1].Separate ? "\n\n" : "\n");
            output.Append(blocks[i].Text);
        }
        return (output.ToString(), string.Join(" ", words), structured);
    }
    internal static IEnumerable<Node> Descendants(Node node) => node.Children.SelectMany(x => new[] { x }.Concat(Descendants(x)));
}


