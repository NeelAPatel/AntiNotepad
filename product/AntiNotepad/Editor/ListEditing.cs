using System.Globalization;
using System.Text.RegularExpressions;

namespace AntiNotepad.Editor;

internal static class ListEditing
{
    internal static readonly Regex ListPrefix = new(@"^(\s*)((?:[-*+]\s+)?\[[ xX]\]\s?|[-*+]\s|([0-9]+)([.)])\s)", RegexOptions.Compiled);

    internal static bool TryListNumber(string line, out int number) => TryListNumber(ListPrefix.Match(line), out number);

    internal static bool TryListNumber(Match match, out int number)
    {
        number = 0;
        var digits = match.Groups[3];
        return digits.Success && digits.Length <= 9
            && int.TryParse(digits.Value, NumberStyles.None, CultureInfo.InvariantCulture, out number);
    }
}
