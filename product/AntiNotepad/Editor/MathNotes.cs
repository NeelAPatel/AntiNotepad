using System.Text.RegularExpressions;
using AntiNotepad.Calc;

namespace AntiNotepad.Editor;

internal static class MathNotes
{
    /// <summary>
    /// Math notes with prose labels ("lunch 12 + 3"): lines that fail are retried with the leading words blanked,
    /// keeping column offsets so token colours still line up. Bullets are ignored the same way.
    /// </summary>
    internal static IReadOnlyList<LineEval?> EvaluateMath(string[] lines)
    {
        var cleaned = lines.Select((l, i) => i == 0 ? l : StripListMarker(l)).ToArray();
        var first = Calculator.EvaluateNote(cleaned);
        var retry = (string[])cleaned.Clone();
        var changed = false;
        for (var i = 1; i < retry.Length; i++)
        {
            if (first[i]?.Result is not null) continue;
            var blanked = BlankLabel(retry[i]);
            if (blanked != retry[i]) { retry[i] = blanked; changed = true; }
        }
        if (!changed) return first;
        var second = Calculator.EvaluateNote(retry);
        var merged = new LineEval?[first.Count];
        for (var i = 0; i < merged.Length; i++)
        {
            var retryLine = second[i];
            if (first[i]?.Result is not null || retryLine is null
                || (retryLine.Result is null && retryLine.ErrorReason == CalcErrorReason.None))
            { merged[i] = first[i]; continue; }
            var label = retry[i].Length - retry[i].TrimStart().Length;
            var tokens = new List<CalcToken> { new(0, label, TokenKind.Comment) };
            tokens.AddRange(retryLine.Tokens);
            merged[i] = new LineEval { Result = retryLine.Result, ErrorReason = retryLine.ErrorReason, Tokens = tokens, AssignedName = retryLine.AssignedName };
        }
        return merged;
    }

    /// <summary>"spent 12.50 + 8.25 =" → spaces up to the first number, currency sign or bracket.</summary>
    internal static string BlankLabel(string line)
    {
        var m = Regex.Match(line, @"^\s*[A-Za-z][A-Za-z' ]*?\s+(?=[\d$€£(.])");
        return m.Success ? new string(' ', m.Length) + line[m.Length..] : line;
    }

    /// <summary>`- 19.99 * 3 =` is a bullet, not a negative number.</summary>
    internal static string StripListMarker(string line)
    {
        var m = ListEditing.ListPrefix.Match(line);
        return m.Success && !m.Groups[3].Success ? new string(' ', m.Length) + line[m.Length..] : line;
    }
}
