using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace InputProbe;

// Stateless per line: virtualized/partial redraw order cannot corrupt fence state.
internal sealed class MarkdownColorizer : DocumentColorizingTransformer
{
    private static readonly IBrush[] Headings = [
        new SolidColorBrush(Color.Parse("#FFD6A5")), new SolidColorBrush(Color.Parse("#9FD8FF")),
        new SolidColorBrush(Color.Parse("#C4B5FD")), new SolidColorBrush(Color.Parse("#A7F3D0")),
        new SolidColorBrush(Color.Parse("#F9A8D4")), new SolidColorBrush(Color.Parse("#FCA5A5"))];
    private static readonly IBrush Marker = new SolidColorBrush(Color.Parse("#F2C879"));
    private static readonly IBrush Keyword = new SolidColorBrush(Color.Parse("#C4A7FF"));
    private static readonly IBrush String = new SolidColorBrush(Color.Parse("#A9E6B5"));
    private static readonly Regex PythonKeywords = new(@"\b(def|class|return|if|elif|else|for|while|in|import|from|as|try|except|with|yield|async|await|True|False|None|pass|break|continue|lambda)\b", RegexOptions.Compiled);
    private static readonly Regex PythonStrings = new("(?<q>['\\\"])(?:\\\\.|(?!\\k<q>).)*?\\k<q>", RegexOptions.Compiled);
    private static readonly Regex Bold = new(@"\*\*.+?\*\*", RegexOptions.Compiled);
    private static readonly Regex Italic = new(@"(?<!\*)\*(?!\*).+?(?<!\*)\*(?!\*)", RegexOptions.Compiled);
    private static readonly Regex Strike = new(@"~~.+?~~", RegexOptions.Compiled);

    protected override void ColorizeLine(DocumentLine line)
    {
        var document = CurrentContext.Document;
        var text = document.GetText(line);
        var inPython = false;
        var openFence = false;
        foreach (var prior in document.Lines)
        {
            if (prior.Offset >= line.Offset) break;
            var priorText = document.GetText(prior).TrimStart();
            if (priorText.StartsWith("```")) { openFence = !openFence; inPython = openFence && priorText.StartsWith("```python", StringComparison.OrdinalIgnoreCase); }
        }
        if (text.TrimStart().StartsWith("```")) { Paint(line.Offset, line.EndOffset, Marker); return; }
        if (inPython)
        {
            foreach (Match match in PythonStrings.Matches(text)) Paint(line.Offset + match.Index, line.Offset + match.Index + match.Length, String);
            foreach (Match match in PythonKeywords.Matches(text)) Paint(line.Offset + match.Index, line.Offset + match.Index + match.Length, Keyword);
            return;
        }
        var prefix = text.TakeWhile(c => c == ' ').Count();
        var hashes = text.Skip(prefix).TakeWhile(c => c == '#').Count();
        if (hashes > 0 && hashes <= 6 && text.Skip(prefix + hashes).FirstOrDefault() is ' ' or '\0') Paint(line.Offset, line.EndOffset, Headings[hashes - 1]);
        foreach (Match match in Bold.Matches(text))
        {
            Paint(line.Offset + match.Index, line.Offset + match.Index + 2, Marker);
            Paint(line.Offset + match.Index + match.Length - 2, line.Offset + match.Index + match.Length, Marker);
            Format(line.Offset + match.Index + 2, line.Offset + match.Index + match.Length - 2, FontWeight.Bold, FontStyle.Normal, null);
        }
        foreach (Match match in Italic.Matches(text))
        {
            Paint(line.Offset + match.Index, line.Offset + match.Index + 1, Marker);
            Paint(line.Offset + match.Index + match.Length - 1, line.Offset + match.Index + match.Length, Marker);
            Format(line.Offset + match.Index + 1, line.Offset + match.Index + match.Length - 1, FontWeight.Normal, FontStyle.Italic, null);
        }
        foreach (Match match in Strike.Matches(text))
        {
            Paint(line.Offset + match.Index, line.Offset + match.Index + 2, Marker);
            Paint(line.Offset + match.Index + match.Length - 2, line.Offset + match.Index + match.Length, Marker);
            Format(line.Offset + match.Index + 2, line.Offset + match.Index + match.Length - 2, FontWeight.Normal, FontStyle.Normal, TextDecorations.Strikethrough);
        }
        foreach (Match match in Regex.Matches(text, @"\[[ xX]\]")) Paint(line.Offset + match.Index, line.Offset + match.Index + match.Length, Marker);
    }

    private void Paint(int start, int end, IBrush brush)
    {
        if (end <= start) return;
        ChangeLinePart(start, end, element => element.TextRunProperties.SetForegroundBrush(brush));
    }

    private void Format(int start, int end, FontWeight weight, FontStyle style, TextDecorationCollection? decorations)
    {
        if (end <= start) return;
        ChangeLinePart(start, end, element =>
        {
            var old = element.TextRunProperties.Typeface;
            element.TextRunProperties.SetTypeface(new Typeface(old.FontFamily, style, weight, old.Stretch));
            if (decorations is not null) element.TextRunProperties.SetTextDecorations(decorations);
        });
    }
}
