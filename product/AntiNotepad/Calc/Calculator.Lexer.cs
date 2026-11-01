using System.Globalization;

namespace AntiNotepad.Calc;

public static partial class Calculator
{
    // ===================================================================== lexer

    private enum RT { Num, Date, Ident, Op, LParen, RParen, Comma, Percent, Degree, Equals, Unknown, End }

    private readonly struct Tok(RT t, int start, int len, double num = 0, string? cur = null, string? text = null, char op = '\0')
    {
        public readonly RT T = t;
        public readonly int Start = start, Len = len;
        public readonly double Num = num;
        public readonly string? Cur = cur;
        public readonly string Text = text ?? "";
        public readonly char Op = op;
    }

    private static bool IsDigit(char c) => c >= '0' && c <= '9';
    private static bool IsHex(char c) => IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
    private static bool IsIdentStart(char c) => char.IsLetter(c) || c == '_';
    private static bool IsIdentChar(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '²' || c == '³';

    private static void Lex(string s, int from, int to, List<Tok> list)
    {
        int i = from, depth = 0;
        while (i < to)
        {
            char c = s[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }
            int st = i;
            string? cur = null;
            if ((c == '$' || c == '€' || c == '£') && i + 1 < to &&
                (IsDigit(s[i + 1]) || (s[i + 1] == '.' && i + 2 < to && IsDigit(s[i + 2]))))
            {
                cur = c switch { '$' => "$", '€' => "€", _ => "£" };
                c = s[++i];
            }
            if (cur == null && TryLexIsoDate(s, i, to, out int dlen, out double oa))
            {
                list.Add(new Tok(RT.Date, st, dlen, oa));
                i += dlen;
                continue;
            }
            if (IsDigit(c) || (c == '.' && i + 1 < to && IsDigit(s[i + 1])))
            {
                if (LexNumber(s, ref i, to, depth == 0, out double v))
                    list.Add(new Tok(RT.Num, st, i - st, v, cur));
                else
                {
                    if (i <= st) i = st + 1;
                    while (i < to && IsIdentChar(s[i])) i++;
                    list.Add(new Tok(RT.Unknown, st, i - st));
                }
                continue;
            }
            if (c == '°')
            {
                if (i + 1 < to && (s[i + 1] == 'C' || s[i + 1] == 'F') && !(i + 2 < to && IsIdentChar(s[i + 2])))
                {
                    list.Add(new Tok(RT.Ident, st, 2, text: s[i + 1] == 'C' ? "°C" : "°F"));
                    i += 2;
                }
                else { list.Add(new Tok(RT.Degree, st, 1)); i++; }
                continue;
            }
            if (IsIdentStart(c))
            {
                i++;
                while (i < to && IsIdentChar(s[i])) i++;
                string text = s.Substring(st, i - st);
                // compound units such as km/h, m/s (no spaces around the slash)
                if (i + 1 < to && s[i] == '/' && IsIdentStart(s[i + 1]))
                {
                    int j = i + 1;
                    while (j < to && IsIdentChar(s[j])) j++;
                    string comp = s.Substring(st, j - st);
                    if (Units.ContainsKey(comp)) { text = comp; i = j; }
                }
                list.Add(new Tok(RT.Ident, st, i - st, text: text));
                continue;
            }
            i++;
            switch (c)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '^':
                case '×':
                case '÷':
                    list.Add(new Tok(RT.Op, st, 1, op: c)); break;
                case '−': list.Add(new Tok(RT.Op, st, 1, op: '-')); break;
                case '(': depth++; list.Add(new Tok(RT.LParen, st, 1)); break;
                case ')': if (depth > 0) depth--; list.Add(new Tok(RT.RParen, st, 1)); break;
                case ',': list.Add(new Tok(RT.Comma, st, 1)); break;
                case '%': list.Add(new Tok(RT.Percent, st, 1)); break;
                case '=': list.Add(new Tok(RT.Equals, st, 1)); break;
                default: list.Add(new Tok(RT.Unknown, st, 1)); break;
            }
        }
    }

    // yyyy-m-d (one or two digit month/day); anything invalid lexes as plain arithmetic
    private static bool TryLexIsoDate(string s, int i, int to, out int len, out double oa)
    {
        len = 0; oa = 0;
        if (i + 8 > to) return false;
        int y = 0;
        for (int k = 0; k < 4; k++)
        {
            if (!IsDigit(s[i + k])) return false;
            y = y * 10 + (s[i + k] - '0');
        }
        if (s[i + 4] != '-') return false;
        int j = i + 5;
        int mo = ReadSmallInt(s, ref j, to);
        if (mo < 0 || j >= to || s[j] != '-') return false;
        j++;
        int d = ReadSmallInt(s, ref j, to);
        if (d < 0 || (j < to && (IsIdentChar(s[j]) || s[j] == '.'))) return false;
        if (y < 100 || mo < 1 || mo > 12 || d < 1 || d > DateTime.DaysInMonth(y, mo)) return false;
        oa = new DateTime(y, mo, d).ToOADate();
        len = j - i;
        return true;
    }

    // one or two digits, else -1
    private static int ReadSmallInt(string s, ref int j, int to)
    {
        int v = 0, n = 0;
        while (j < to && IsDigit(s[j])) { if (++n > 2) return -1; v = v * 10 + (s[j++] - '0'); }
        return n == 0 ? -1 : v;
    }

    private static bool LexNumber(string s, ref int i, int to, bool allowCommas, out double v)
    {
        v = 0;
        if (s[i] == '0' && i + 2 < to && (s[i + 1] == 'x' || s[i + 1] == 'X') && IsHex(s[i + 2]))
        {
            i += 2;
            long acc = 0;
            int digits = 0;
            while (i < to && IsHex(s[i]))
            {
                char h = s[i++];
                if (++digits > 15) return false;
                acc = acc * 16 + (IsDigit(h) ? h - '0' : (h | 0x20) - 'a' + 10);
            }
            v = acc;
            return !(i < to && IsIdentChar(s[i]));
        }

        Span<char> buf = stackalloc char[72];
        int n = 0;
        bool any = false;
        while (i < to)
        {
            char c = s[i];
            if (IsDigit(c)) { if (n >= 60) return false; buf[n++] = c; i++; any = true; continue; }
            if (c == '_' && any && i + 1 < to && IsDigit(s[i + 1])) { i++; continue; }
            // thousands comma: exactly three digits follow (and not a fourth); never inside parens
            if (c == ',' && allowCommas && any && i + 3 < to && IsDigit(s[i + 1]) && IsDigit(s[i + 2]) && IsDigit(s[i + 3]) &&
                !(i + 4 < to && IsDigit(s[i + 4])))
            { i++; continue; }
            break;
        }
        if (i + 1 < to && s[i] == '.' && IsDigit(s[i + 1]))
        {
            buf[n++] = '.';
            i++;
            while (i < to && (IsDigit(s[i]) || (s[i] == '_' && i + 1 < to && IsDigit(s[i + 1]))))
            {
                if (s[i] != '_') { if (n >= 60) return false; buf[n++] = s[i]; }
                i++;
            }
        }
        if (i < to && (s[i] == 'e' || s[i] == 'E'))
        {
            int j = i + 1;
            if (j < to && (s[j] == '+' || s[j] == '-')) j++;
            if (j < to && IsDigit(s[j]))
            {
                buf[n++] = 'e';
                if (s[i + 1] == '-') buf[n++] = '-';
                i = j;
                while (i < to && IsDigit(s[i])) { if (n >= 68) return false; buf[n++] = s[i++]; }
            }
        }
        if (!double.TryParse(buf[..n], NumberStyles.Float, Inv, out v)) return false;
        if (i < to && (s[i] == 'k' || s[i] == 'M') && !(i + 1 < to && IsIdentChar(s[i + 1])))
        {
            v *= s[i] == 'k' ? 1e3 : 1e6;
            i++;
        }
        return true;
    }

}
