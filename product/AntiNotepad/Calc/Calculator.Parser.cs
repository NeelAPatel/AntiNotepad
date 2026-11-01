using System.Globalization;

namespace AntiNotepad.Calc;

public static partial class Calculator
{
    // ===================================================================== parser

    private sealed class Parser(List<Tok> toks, int start, int end, TokenKind?[] kinds,
                                Dictionary<string, Q> vars, Func<string, ContextResult>? ctx)
    {
        private static readonly Tok EndTok = new(RT.End, 0, 0);
        private const int MaxDepth = 64;
        public int P = start;
        public bool Fail;
        public CalcErrorReason ErrorReason { get; private set; }
        public bool SawOp;   // any binary operator, function, percent or conversion (for EvaluateInline)
        private int _depth;

        private Tok Cur => P < end ? toks[P] : EndTok;
        private Tok Peek(int k) => P + k < end ? toks[P + k] : EndTok;
        private void Mark(TokenKind k) { if (P < end) kinds[P] = k; }
        private void Take(TokenKind k) { Mark(k); P++; }
        private Q Bad(CalcErrorReason reason = CalcErrorReason.None)
        {
            Fail = true;
            if (ErrorReason == CalcErrorReason.None) ErrorReason = reason;
            return default;
        }
        private bool EnterDepth() { if (_depth >= MaxDepth) { Fail = true; return false; } _depth++; return true; }

        private bool IsWord(Tok t, string w) => t.T == RT.Ident && t.Text == w;
        private bool IsOp(Tok t, char c) => t.T == RT.Op && t.Op == c;
        private bool IsVar(string name) => vars.ContainsKey(name);

        private bool StartsOperand(Tok t, int offset)
        {
            if (t.T is RT.Num or RT.LParen) return true;
            if (t.T != RT.Ident || Reserved.Contains(t.Text)) return false;
            if (IsVar(t.Text) || TryConstant(t.Text, out _) || ContextWords.Contains(t.Text)) return true;
            return Functions.Contains(t.Text) && Peek(offset + 1).T == RT.LParen;
        }

        public Q ParseExpr()
        {
            var q = ParseAdd();
            while (!Fail && (IsWord(Cur, "in") || IsWord(Cur, "to") || IsWord(Cur, "as")))
            {
                Take(TokenKind.Keyword);
                SawOp = true;
                if (q.Date) return Bad();
                if (Cur.T == RT.Percent)
                {
                    Take(TokenKind.Operator);
                    if (IsWord(Cur, "of"))
                    {
                        Take(TokenKind.Keyword);
                        var whole = ParseAdd();
                        if (Fail) return default;
                        if (whole.Date) return Bad();
                        if (q.U != null || whole.U != null)
                        {
                            if (q.U == null || whole.U == null || !TryConvert(whole, q.U, out whole)) return Bad();
                        }
                        q = new Q(q.V / whole.V, Pct: true);
                    }
                    else q = new Q(q.V, Pct: true);
                    continue;
                }
                var t = Cur;
                if (t.T != RT.Ident) return Bad();
                var u = LookupUnit(t.Text);
                if (u == null) { Mark(TokenKind.Identifier); return Bad(); }
                Take(TokenKind.Unit);
                // a day/week count stays a count: (dec 25 - today) in weeks
                if (IsCount(q.U)) u = ReferenceEquals(u, DayUnit) ? DayCount : ReferenceEquals(u, WeekUnit) ? WeekCount : u;
                if (!TryConvert(q, u, out q)) return Bad();
            }
            return q;
        }

        private Q ParseAdd()
        {
            var l = ParseTerm();
            while (!Fail)
            {
                var t = Cur;
                double sign;
                if (IsOp(t, '+')) { sign = 1; Mark(TokenKind.Operator); }
                else if (IsOp(t, '-')) { sign = -1; Mark(TokenKind.Operator); }
                else if (IsWord(t, "plus")) { sign = 1; Mark(TokenKind.Keyword); }
                else if (IsWord(t, "minus")) { sign = -1; Mark(TokenKind.Keyword); }
                else break;
                P++;
                SawOp = true;
                var r = ParseTerm();
                if (Fail) break;
                if (l.Cur != null && r.Cur != null && l.Cur != r.Cur) return Bad(CalcErrorReason.MixedCurrencies);
                if (r.Pct && !l.Pct && !l.Date) l = l with { V = l.V * (1 + sign * r.V) };   // 50 + 10% = 55
                else if (!TryAdd(l, r, sign, out l)) return Bad();
            }
            return l;
        }

        private Q ParseTerm()
        {
            var l = ParseUnary();
            while (!Fail)
            {
                var t = Cur;
                char op;
                bool implicitMul = false;
                if (IsOp(t, '*') || IsOp(t, '×')) { op = '*'; Mark(TokenKind.Operator); }
                else if (IsOp(t, '/') || IsOp(t, '÷')) { op = '/'; Mark(TokenKind.Operator); }
                else if (t.T == RT.Percent) { op = '%'; Mark(TokenKind.Operator); }   // postfix declined it, so it's modulo
                else if (IsWord(t, "times")) { op = '*'; Mark(TokenKind.Keyword); }
                else if (IsWord(t, "mod")) { op = '%'; Mark(TokenKind.Keyword); }
                else if (IsWord(t, "x") && !IsVar("x") && StartsOperand(Peek(1), 1)) { op = '*'; Mark(TokenKind.Operator); }
                else if (StartsOperand(t, 0)) { op = '*'; implicitMul = true; }
                else break;
                if (!implicitMul) P++;
                SawOp = true;
                var r = implicitMul ? ParsePower() : ParseUnary();
                if (Fail) break;
                l = MulDiv(l, r, op);
            }
            return l;
        }

        private Q MulDiv(Q l, Q r, char op)
        {
            if (l.Date || r.Date) return Bad();
            switch (op)
            {
                case '*':
                    if (l.U != null && r.U != null) return Bad();
                    return new Q(l.V * r.V, l.U ?? r.U, l.Cur ?? r.Cur, l.Pct && !r.Pct);
                case '/':
                    if (r.U != null)
                    {
                        if (l.U == null || !TryConvert(r, l.U, out r)) return Bad();
                        return new Q(l.V / r.V, null, l.Cur != null && r.Cur != null ? null : l.Cur ?? r.Cur);
                    }
                    return new Q(l.V / r.V, l.U, l.Cur != null && r.Cur != null ? null : l.Cur ?? r.Cur, l.Pct && !r.Pct);
                default: // modulo
                    if (r.U != null && (l.U == null || !TryConvert(r, l.U, out r))) return Bad();
                    if (r.V == 0) return Bad();
                    return new Q(l.V % r.V, l.U, l.Cur ?? r.Cur);
            }
        }

        private Q ParseUnary()
        {
            if (IsOp(Cur, '-') || IsOp(Cur, '+'))
            {
                if (!EnterDepth()) return default;
                try
                {
                    bool negative = IsOp(Cur, '-');
                    Take(TokenKind.Operator);
                    var q = ParseUnary();
                    if (Fail) return default;
                    if (!negative) return q;
                    return q.Date ? Bad() : q with { V = -q.V };
                }
                finally { _depth--; }
            }
            return ParsePower();
        }

        private Q ParsePower()
        {
            var b = ParsePostfix();
            if (Fail || !IsOp(Cur, '^')) return b;
            Take(TokenKind.Operator);
            SawOp = true;
            if (!EnterDepth()) return default;
            Q e;
            try { e = ParseUnary(); }   // right-associative; -3^2 = -9 since unary sits above
            finally { _depth--; }
            if (Fail) return default;
            if (b.U != null || e.U != null || b.Date || e.Date) return Bad();
            return new Q(Math.Pow(b.V, e.V), null, b.Cur);
        }

        private Q ParsePostfix()
        {
            var q = ParsePrimary();
            if (Fail || Cur.T != RT.Percent || q.Date) return q;
            if (StartsOperand(Peek(1), 1)) return q;   // `10 % 3` is modulo; ParseTerm handles it
            Take(TokenKind.Operator);
            SawOp = true;
            q = q with { V = q.V / 100, Pct = true };
            bool of = IsWord(Cur, "of"), off = IsWord(Cur, "off");
            if (!of && !off) return q;
            Take(TokenKind.Keyword);
            var r = ParseUnary();
            if (Fail || r.Date) return Bad();
            return r with { V = of ? r.V * q.V : r.V * (1 - q.V), Pct = false };
        }

        private Q ParsePrimary()
        {
            var t = Cur;
            switch (t.T)
            {
                case RT.Num:
                    if (IsDayNum(t) && Peek(1).T == RT.Ident && !IsVar(Peek(1).Text) && Months.TryGetValue(Peek(1).Text, out int mon))
                    {
                        Take(TokenKind.Number);   // 25 dec
                        Take(TokenKind.Keyword);
                        return MonthDate(mon, (int)t.Num);
                    }
                    Take(TokenKind.Number);
                    return UnitSuffix(new Q(t.Num, Cur: t.Cur));
                case RT.Date:
                    Take(TokenKind.Number);
                    return new Q(t.Num, Date: true);
                case RT.LParen:
                    {
                        if (!EnterDepth()) return default;
                        try
                        {
                            Take(TokenKind.Operator);
                            var q = ParseExpr();
                            if (Fail) return default;
                            if (Cur.T != RT.RParen) return Bad();
                            Take(TokenKind.Operator);
                            return UnitSuffix(q);
                        }
                        finally { _depth--; }
                    }
                case RT.Ident:
                    {
                        string name = t.Text;
                        if (Functions.Contains(name) && Peek(1).T == RT.LParen) return ParseFunction(name);
                        if (vars.TryGetValue(name, out var val)) { Take(TokenKind.Identifier); return val; }
                        if (Reserved.Contains(name)) return Bad();
                        if (TryConstant(name, out double c)) { Take(TokenKind.Identifier); return new Q(c); }
                        if (ContextWords.Contains(name))
                        {
                            Take(TokenKind.Keyword);
                            var cv = ctx?.Invoke(name) ?? default;
                            return cv.Value ?? Bad(cv.ErrorReason);
                        }
                        if (TryDateWord(name, out var dw)) { Take(TokenKind.Keyword); return dw; }
                        if (Months.TryGetValue(name, out int month) && IsDayNum(Peek(1)))
                        {
                            Take(TokenKind.Keyword);   // dec 25
                            int day = (int)Cur.Num;
                            Take(TokenKind.Number);
                            return MonthDate(month, day);
                        }
                        if ((Eq(name, "days") || Eq(name, "day") || Eq(name, "weeks") || Eq(name, "week")) && Peek(1).T == RT.Ident &&
                            (Eq(Peek(1).Text, "until") || Eq(Peek(1).Text, "till") || Eq(Peek(1).Text, "since")))
                            return ParseUntil(name[0] is 'w' or 'W', Eq(Peek(1).Text, "since"));
                        Mark(TokenKind.Identifier);
                        return Bad();
                    }
                default:
                    return Bad();
            }
        }

        private static bool IsDayNum(Tok t) => t.T == RT.Num && t.Cur == null && t.Num >= 1 && t.Num <= 31 && t.Num == Math.Floor(t.Num);
        private static bool IsYearNum(Tok t) => t.T == RT.Num && t.Cur == null && t.Num >= 1000 && t.Num <= 9999 && t.Num == Math.Floor(t.Num);

        // month and day already taken; optional year: `dec 25 2027`, `dec 25, 2027`
        private Q MonthDate(int month, int day)
        {
            int year = 0;
            if (Cur.T == RT.Comma && IsYearNum(Peek(1))) { P++; year = (int)Cur.Num; Take(TokenKind.Number); }
            else if (IsYearNum(Cur)) { year = (int)Cur.Num; Take(TokenKind.Number); }
            return TryMakeDate(year, month, day, out var q) ? q : Bad();
        }

        // `days until dec 25`, `weeks since 2026-01-01`; counts from today, or from now when the target has a time
        private Q ParseUntil(bool weeks, bool since)
        {
            Take(TokenKind.Unit);
            Take(TokenKind.Keyword);
            SawOp = true;
            var target = ParseAdd();
            if (Fail) return default;
            if (!target.Date) return Bad();
            var t = ToDt(target.V);
            var now = Clock();
            var from = t.TimeOfDay == TimeSpan.Zero ? now.Date : now;
            double days = (since ? from - t : t - from).TotalDays;
            return weeks ? new Q(days / 7, WeekCount) : new Q(days, DayCount);
        }

        // Unit (or angle degrees) directly after a number or closing paren.
        private Q UnitSuffix(Q q)
        {
            var t = Cur;
            if (t.T == RT.Degree) { Take(TokenKind.Unit); return q with { V = q.V * Math.PI / 180 }; }
            if (t.T != RT.Ident || q.U != null || IsVar(t.Text) || Peek(1).T == RT.LParen) return q;
            if (IsDegreeWord(t.Text)) { Take(TokenKind.Unit); return q with { V = q.V * Math.PI / 180 }; }
            var u = LookupUnit(t.Text);
            if (u == null) return q;
            if (t.Text == "in")
            {
                // `5 in cm` -> conversion keyword; `2 in`, `2 in to cm` -> inches
                var nx = Peek(1);
                if (nx.T == RT.Percent) return q;
                if (nx.T == RT.Ident && !Reserved.Contains(nx.Text) && !IsVar(nx.Text) && LookupUnit(nx.Text) != null) return q;
            }
            Take(TokenKind.Unit);
            return q with { U = u };
        }

        private Q ParseFunction(string name)
        {
            if (!EnterDepth()) return default;
            try { return ParseFunctionCore(name); }
            finally { _depth--; }
        }

        private Q ParseFunctionCore(string name)
        {
            Take(TokenKind.Function);
            Take(TokenKind.Operator); // (
            SawOp = true;
            var args = new List<Q>(2);
            if (Cur.T != RT.RParen)
            {
                while (true)
                {
                    args.Add(ParseExpr());
                    if (Fail) return default;
                    if (Cur.T == RT.Comma) { P++; continue; }
                    break;
                }
            }
            if (Cur.T != RT.RParen) return Bad();
            Take(TokenKind.Operator);
            if (args.Count == 0) return Bad();
            foreach (var arg in args) if (arg.Date) return Bad();
            var a = args[0];
            switch (name)
            {
                case "abs": return args.Count == 1 ? a with { V = Math.Abs(a.V) } : Bad();
                case "floor": return args.Count == 1 ? a with { V = Math.Floor(a.V) } : Bad();
                case "ceil": return args.Count == 1 ? a with { V = Math.Ceiling(a.V) } : Bad();
                case "round":
                    {
                        if (args.Count > 2) return Bad();
                        int digits = args.Count == 2 ? (int)Math.Clamp(args[1].V, 0, 15) : 0;
                        if (a.Pct) return a with { V = Math.Round(a.V * 100, digits, MidpointRounding.AwayFromZero) / 100 };
                        return a with { V = Math.Round(a.V, digits, MidpointRounding.AwayFromZero) };
                    }
                case "pow":
                    if (args.Count != 2 || a.U != null || args[1].U != null) return Bad();
                    return new Q(Math.Pow(a.V, args[1].V), null, a.Cur);
                case "min":
                case "max":
                case "avg":
                case "average":
                case "sum":
                    {
                        double acc = a.V;
                        string? currency = a.Cur;
                        for (int i = 1; i < args.Count; i++)
                        {
                            var b = args[i];
                            if (currency != null && b.Cur != null && currency != b.Cur) return Bad(CalcErrorReason.MixedCurrencies);
                            currency ??= b.Cur;
                            if (a.U != null || b.U != null)
                            {
                                if (a.U == null || b.U == null || !TryConvert(b, a.U, out b)) return Bad();
                            }
                            acc = name switch
                            {
                                "min" => Math.Min(acc, b.V),
                                "max" => Math.Max(acc, b.V),
                                _ => acc + b.V,
                            };
                        }
                        if (name is "avg" or "average") acc /= args.Count;
                        return a with { V = acc };
                    }
            }
            if (args.Count != 1 || a.U != null) return Bad();
            double x = a.V;
            double v = name switch
            {
                "sqrt" => Math.Sqrt(x),
                "cbrt" => Math.Cbrt(x),
                "sin" => Math.Sin(x),
                "cos" => Math.Cos(x),
                "tan" => Math.Tan(x),
                "asin" => Math.Asin(x),
                "acos" => Math.Acos(x),
                "atan" => Math.Atan(x),
                "log" => Math.Log10(x),
                "ln" => Math.Log(x),
                "log2" => Math.Log2(x),
                "exp" => Math.Exp(x),
                _ => double.NaN,
            };
            return double.IsFinite(v) ? new Q(v) : Bad();
        }
    }

}
