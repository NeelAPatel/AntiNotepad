using System.Text.RegularExpressions;

namespace Antipad.Editor;

/// <summary>Pure whole-note transforms behind the line `/commands`. A mode keyword on line 1 is never touched.</summary>
internal static class TextCommands
{
    private static readonly Regex Marker = new(@"^(\s*)(?:(?:[-*+]\s+)?\[[ xX]\]\s?|[-*+]\s+|\d+[.)]\s+)", RegexOptions.Compiled);
    private static readonly Regex Done = new(@"^(\s*(?:[-*+]\s+)?)\[[xX]\]", RegexOptions.Compiled);

    /// <summary>Applies <paramref name="f"/> to the body lines. Blank lines are dropped unless <paramref name="keepBlank"/>.</summary>
    public static string Apply(string text, bool hasModeLine, Func<IEnumerable<string>, IEnumerable<string>> f, bool keepBlank = false)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n').ToList();
        var head = hasModeLine ? lines.Take(1).ToList() : [];
        var body = lines.Skip(head.Count).Where(l => keepBlank || l.Trim().Length > 0);
        return string.Join("\n", head.Concat(f(body)));
    }

    public static IEnumerable<string> Sort(IEnumerable<string> lines) => lines.OrderBy(l => Plain(l), StringComparer.CurrentCultureIgnoreCase);

    public static IEnumerable<string> Dedupe(IEnumerable<string> lines)
    {
        var seen = new HashSet<string>(StringComparer.CurrentCulture);
        return lines.Where(l => l.Trim().Length == 0 || seen.Add(l.TrimEnd()));
    }

    public static IEnumerable<string> ClearDone(IEnumerable<string> lines) => lines.Where(l => !Done.IsMatch(l));

    public static IEnumerable<string> Uncheck(IEnumerable<string> lines) => lines.Select(l => Done.Replace(l, "$1[ ]"));

    /// <summary>Turns every non-blank line into a list item of one style, replacing whatever marker it had.</summary>
    public static IEnumerable<string> AsList(IEnumerable<string> lines, string style)
    {
        var n = 0;
        foreach (var line in lines)
        {
            if (line.Trim().Length == 0) { yield return line; continue; }
            var m = Marker.Match(line);
            var indent = m.Success ? m.Groups[1].Value : line[..(line.Length - line.TrimStart().Length)];
            var content = m.Success ? line[m.Length..] : line.TrimStart();
            yield return style switch
            {
                "check" => $"{indent}[ ] {content}",
                "bullet" => $"{indent}- {content}",
                "number" => $"{indent}{++n}. {content}",
                _ => indent + content,
            };
        }
    }

    /// <summary>Trailing spaces go; runs of blank lines shrink to one.</summary>
    public static IEnumerable<string> Tidy(IEnumerable<string> lines)
    {
        var blank = false;
        foreach (var raw in lines)
        {
            var line = raw.TrimEnd();
            if (line.Length == 0) { if (blank) continue; blank = true; }
            else blank = false;
            yield return line;
        }
    }

    private static string Plain(string line)
    {
        var m = Marker.Match(line);
        return m.Success ? line[m.Length..] : line.Trim();
    }

    public static bool SelfTest(out string report)
    {
        var failures = new List<string>();
        void Check(string what, string actual, string expected) { if (actual != expected) failures.Add($"{what}: got <{actual}> want <{expected}>"); }
        Check("sort keeps mode line", Apply("list\n[ ] pears\n[x] apples\n\n[ ] milk", true, Sort), "list\n[x] apples\n[ ] milk\n[ ] pears");
        Check("dedupe keeps blanks", Apply("a\nb\n\na\nb \n\nc", false, Dedupe, keepBlank: true), "a\nb\n\n\nc");
        Check("clear done", Apply("list\n[x] a\n- [X] b\n[ ] c", true, ClearDone, keepBlank: true), "list\n[ ] c");
        Check("uncheck", Apply("[x] a\n  - [X] b\n[ ] c", false, Uncheck, keepBlank: true), "[ ] a\n  - [ ] b\n[ ] c");
        Check("as checklist", Apply("milk\n- eggs\n  2. jam\n\nbread", false, ls => AsList(ls, "check"), keepBlank: true), "[ ] milk\n[ ] eggs\n  [ ] jam\n\n[ ] bread");
        Check("as numbered", Apply("[ ] a\n- b\nc", false, ls => AsList(ls, "number"), keepBlank: true), "1. a\n2. b\n3. c");
        Check("as plain", Apply("1. a\n[x] b\n* c", false, ls => AsList(ls, "plain"), keepBlank: true), "a\nb\nc");
        Check("tidy", Apply("a  \n\n\n\nb\t\n", false, Tidy, keepBlank: true), "a\n\nb\n");
        report = failures.Count == 0 ? "TextCommands: all checks passed." : string.Join("\n", failures);
        return failures.Count == 0;
    }
}
