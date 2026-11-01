using System.Globalization;

namespace AntiNotepad.Calc;

public enum TokenKind { Number, Identifier, Unit, Operator, Keyword, Comment, Function }

public readonly record struct CalcToken(int Start, int Length, TokenKind Kind);

/// <summary>Value is the number in the displayed unit. For percentages Unit is "%" and Value is the percentage (25 for "25%").</summary>
public sealed record CalcResult(string Display, double Value, string? Unit);

public enum CalcErrorReason { None, MixedCurrencies }

public sealed class LineEval
{
    public CalcResult? Result { get; init; }          // null = no numeric result; ErrorReason may request an error chip
    public CalcErrorReason ErrorReason { get; init; }
    public IReadOnlyList<CalcToken> Tokens { get; init; } = [];  // for syntax colouring, offsets within the line
    public string? AssignedName { get; init; }        // set for `name = expr`
}

/// <summary>Notepad-calculator engine (Soulver/Numi style). Pure, allocation-light, no exceptions on the hot path.</summary>
public static partial class Calculator
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    // ===================================================================== line evaluation

    private enum LineClass { Blank, Comment, Normal }

    private static readonly Dictionary<string, Q> NoVars = new(StringComparer.Ordinal);

    private static TokenKind? GuessKind(List<Tok> raw, int i, Dictionary<string, Q> vars)
    {
        var t = raw[i];
        switch (t.T)
        {
            case RT.Num: case RT.Date: return TokenKind.Number;
            case RT.Op: case RT.LParen: case RT.RParen: case RT.Percent: case RT.Equals: return TokenKind.Operator;
            case RT.Degree: return TokenKind.Unit;
            case RT.Ident:
            {
                string name = t.Text;
                if (vars.ContainsKey(name)) return TokenKind.Identifier;
                if (Functions.Contains(name) && i + 1 < raw.Count && raw[i + 1].T == RT.LParen) return TokenKind.Function;
                if (Keywords.Contains(name) || DateWords.Contains(name)) return TokenKind.Keyword;
                if (i > 0 && LookupUnit(name) != null)
                {
                    var p = raw[i - 1];
                    if (p.T is RT.Num or RT.RParen || (p.T == RT.Ident && p.Text is "in" or "to" or "as")) return TokenKind.Unit;
                }
                return TokenKind.Identifier;
            }
            default: return null;
        }
    }

    private static LineEval EvalLine(string line, Dictionary<string, Q> vars, Func<string, ContextResult>? ctx,
                                     out Q? value, out LineClass cls)
    {
        value = null;
        int n = line.Length, s = 0;
        while (s < n && char.IsWhiteSpace(line[s])) s++;
        if (s == n) { cls = LineClass.Blank; return new LineEval(); }
        if (line[s] == '#' || (line[s] == '/' && s + 1 < n && line[s + 1] == '/'))
        {
            cls = LineClass.Comment;
            return new LineEval { Tokens = [new CalcToken(s, n - s, TokenKind.Comment)] };
        }
        cls = LineClass.Normal;

        var tokens = new List<CalcToken>();
        int end = line.IndexOf("//", s, StringComparison.Ordinal);
        if (end < 0) end = n;
        int exprStart = s;
        int colon = line.IndexOf(':', s, end - s);
        if (colon >= 0)
        {
            tokens.Add(new CalcToken(s, colon + 1 - s, TokenKind.Comment));   // "Coffee:" label
            exprStart = colon + 1;
        }

        var raw = new List<Tok>(16);
        Lex(line, exprStart, end, raw);
        var kinds = new TokenKind?[raw.Count];

        int count = raw.Count;
        while (count > 0 && raw[count - 1].T == RT.Equals) { count--; kinds[count] = TokenKind.Operator; } // "12*4 ="

        string? assign = null;
        int first = 0;
        if (count >= 2 && raw[0].T == RT.Ident && raw[1].T == RT.Equals && IsAssignableName(raw[0].Text))
        {
            assign = raw[0].Text;
            kinds[0] = TokenKind.Identifier;
            kinds[1] = TokenKind.Operator;
            first = 2;
        }

        CalcResult? result = null;
        var errorReason = CalcErrorReason.None;
        if (first < count)
        {
            var parser = new Parser(raw, first, count, kinds, vars, ctx);
            var q = parser.ParseExpr();
            errorReason = parser.ErrorReason;
            if (!parser.Fail && parser.P == count && double.IsFinite(q.V))
            {
                result = ToResult(q);
                if (result != null)
                {
                    value = q;
                    if (assign != null) vars[assign] = q;
                }
            }
        }

        for (int i = 0; i < raw.Count; i++)
        {
            var k = kinds[i] ?? GuessKind(raw, i, vars);
            if (k is { } kind) tokens.Add(new CalcToken(raw[i].Start, raw[i].Len, kind));
        }
        if (end < n) tokens.Add(new CalcToken(end, n - end, TokenKind.Comment));

        return new LineEval { Result = result, ErrorReason = errorReason, Tokens = tokens, AssignedName = result != null ? assign : null };
    }

    private static bool IsAssignableName(string name)
    {
        if (name.Length == 0 || !(char.IsLetter(name[0]) || name[0] == '_') || Reserved.Contains(name)) return false;
        foreach (char c in name)
            if (!(char.IsLetterOrDigit(c) || c == '_')) return false;
        return true;
    }

    // prev/ans: nearest result above (within the block). sum/total/avg: results of the contiguous block above,
    // stopping at a blank or heading/comment line; prose lines without a result are skipped.
    private static ContextResult Context(string kw, int index, Q?[] vals, LineClass[] cls)
    {
        if (kw is "prev" or "ans")
        {
            for (int j = index - 1; j >= 1 && cls[j] == LineClass.Normal; j--)
                if (vals[j] is { } v) return new ContextResult(v);
            return default;
        }
        Q? acc = null;
        int count = 0;
        for (int j = index - 1; j >= 1 && cls[j] == LineClass.Normal; j--)
        {
            if (vals[j] is not { } v) continue;
            v = v with { Pct = false };
            if (acc is not { } a) acc = v;
            else if (a.Cur != null && v.Cur != null && a.Cur != v.Cur)
                return new ContextResult(null, CalcErrorReason.MixedCurrencies);
            else if (TryAdd(v, a, 1, out var sumQ)) acc = sumQ;   // topmost line's unit wins
            else return default;
            count++;
        }
        if (kw is "sum" or "total") return new ContextResult(acc ?? new Q(0));
        if (acc is not { } total || count == 0) return default;
        return new ContextResult(total with { V = total.V / count });
    }

    public static IReadOnlyList<LineEval?> EvaluateNote(IReadOnlyList<string> lines)
    {
        int n = lines.Count;
        var res = new LineEval?[n];
        if (n == 0) return res;
        var vars = new Dictionary<string, Q>(StringComparer.Ordinal);
        var vals = new Q?[n];
        var cls = new LineClass[n];
        cls[0] = LineClass.Comment;
        for (int i = 1; i < n; i++)
        {
            int idx = i;
            try
            {
                res[i] = EvalLine(lines[i] ?? "", vars, kw => Context(kw, idx, vals, cls), out vals[i], out cls[i]);
            }
            catch (Exception)
            {
                res[i] = new LineEval();
                vals[i] = null;
                cls[i] = LineClass.Normal;
            }
        }
        return res;
    }

    public static CalcResult? EvaluateInline(string line) => EvaluateInlineLine(line)?.Result;

    public static LineEval? EvaluateInlineLine(string line)
    {
        if (string.IsNullOrEmpty(line)) return null;
        int end = line.Length;
        while (end > 0 && char.IsWhiteSpace(line[end - 1])) end--;
        if (end == 0 || line[end - 1] != '=') return null;
        end--;
        try
        {
            var raw = new List<Tok>(16);
            Lex(line, 0, end, raw);
            if (raw.Count == 0) return null;
            var parser = new Parser(raw, 0, raw.Count, new TokenKind?[raw.Count], NoVars, null);
            var q = parser.ParseExpr();
            if (parser.Fail || parser.P != raw.Count || !parser.SawOp)
                return parser.ErrorReason == CalcErrorReason.MixedCurrencies
                    ? new LineEval { ErrorReason = parser.ErrorReason }
                    : null;
            var result = ToResult(q);
            return result is null ? null : new LineEval { Result = result };
        }
        catch (Exception)
        {
            return null;
        }
    }

}
