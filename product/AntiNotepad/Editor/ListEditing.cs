using System.Globalization;
using System.Text.RegularExpressions;

namespace AntiNotepad.Editor;

internal static class ListEditing
{
    internal static readonly Regex ListPrefix = new(@"^(\s*)((?:[-*+]\s+)?\[[ xX]\]\s?|[-*+]\s|([0-9]+)([.)])\s)", RegexOptions.Compiled);
    internal static readonly Regex Checkbox = new(@"^(\s*)(?:[-*+]\s+)?\[( |x|X)\]", RegexOptions.Compiled);

    internal readonly record struct LineEdit(int Offset, int RemovalLength, string InsertText);

    internal static LineEdit? ToggleCheckbox(string text)
    {
        var match = Checkbox.Match(text);
        if (match.Success)
            return new LineEdit(match.Groups[2].Index, 1, match.Groups[2].Value == " " ? "x" : " ");

        var indent = text.Length - text.TrimStart().Length;
        var bullet = Regex.Match(text, @"^\s*[-*+]\s+");
        return bullet.Success
            ? new LineEdit(indent, bullet.Length - indent, "[ ] ")
            : new LineEdit(indent, 0, "[ ] ");
    }

    internal static LineEdit? Indent(string text, bool deeper)
    {
        if (deeper) return new LineEdit(0, 0, "  ");

        var leadingSpaces = text.Length - text.TrimStart(' ').Length;
        return leadingSpaces > 0 ? new LineEdit(0, Math.Min(2, leadingSpaces), "") : null;
    }

    internal static bool TryListNumber(string line, out int number) => TryListNumber(ListPrefix.Match(line), out number);

    internal static bool TryListNumber(Match match, out int number)
    {
        number = 0;
        var digits = match.Groups[3];
        return digits.Success && digits.Length <= 9
            && int.TryParse(digits.Value, NumberStyles.None, CultureInfo.InvariantCulture, out number);
    }
}
