// Formats HTML tables as display-width-aligned pipe rows.
using System.Globalization;
using System.Text;

namespace AntiNotepad.Editor;

internal static class PasteTable
{
    internal static (string Text, string Words)? Convert(PasteHtml.Node table)
    {
        var all = PasteHtml.Descendants(table).ToList();
        if (all.Any(x => x.Name == "table" || (x.Attributes.TryGetValue("colspan", out var c) && (!int.TryParse(c, out var n) || n > 1)) || (x.Attributes.TryGetValue("rowspan", out var r) && (!int.TryParse(r, out var span) || span > 1)))) return null;
        var rows = all.Where(x => x.Name == "tr").ToList();
        var header = table.Children.FirstOrDefault(x => x.Name == "thead");
        var headerRow = header == null ? null : PasteHtml.Descendants(header).FirstOrDefault(x => x.Name == "tr");
        if (headerRow != null) { rows.Remove(headerRow); rows.Insert(0, headerRow); }
        if (rows.Count == 0) return null;
        var cells = rows.Select(row => row.Children.Where(x => x.Name is "td" or "th").ToList()).ToList();
        var columns = cells.Max(x => x.Count);
        if (columns == 0) return null;
        var formatted = cells.Select(row => Enumerable.Range(0, columns).Select(i => i < row.Count ? PasteHtml.Clean(PasteHtml.Inline(row[i], true)).Replace("|", "\\|") : "").ToArray()).ToList();
        var widths = Enumerable.Range(0, columns).Select(i => Math.Max(3, formatted.Max(row => Width(row[i])))).ToArray();
        string Row(string[] row) => "| " + string.Join(" | ", row.Select((x, i) => x + new string(' ', widths[i] - Width(x)))) + " |";
        var output = new List<string> { Row(formatted[0]), "| " + string.Join(" | ", widths.Select(x => new string('-', x))) + " |" };
        output.AddRange(formatted.Skip(1).Select(Row));
        var words = string.Join(" ", cells.SelectMany(row => row.Select(cell => PasteHtml.Clean(PasteHtml.Inline(cell, false)))));
        return (string.Join("\n", output), words);
    }
    internal static int Width(string value)
    {
        var width = 0;
        var elements = StringInfo.GetTextElementEnumerator(value);
        while (elements.MoveNext())
        {
            var runes = elements.GetTextElement().EnumerateRunes().ToArray();
            var emoji = runes.Any(r => r.Value is 0xfe0f or 0x200d or 0x20e3 or >= 0x1f000 and <= 0x1faff);
            if (emoji) { width += 2; continue; }
            foreach (var rune in runes)
            {
                var v = rune.Value;
                var category = Rune.GetUnicodeCategory(rune);
                if (category is UnicodeCategory.NonSpacingMark or UnicodeCategory.EnclosingMark or UnicodeCategory.Format || v is >= 0xfe00 and <= 0xfe0f || v is >= 0xe0100 and <= 0xe01ef) continue;
                var wide = v is >= 0x1100 and <= 0x115f || v is >= 0x2e80 and <= 0xa4cf || v is >= 0xac00 and <= 0xd7a3 || v is >= 0xf900 and <= 0xfaff || v is >= 0xfe10 and <= 0xfe6f || v is >= 0xff01 and <= 0xff60 || v is >= 0xffe0 and <= 0xffe6 || v is >= 0x20000 and <= 0x3fffd;
                width += wide ? 2 : 1;
            }
        }
        return width;
    }
}
