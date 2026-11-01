using System.Globalization;

namespace AntiNotepad.Calc;

public static partial class Calculator
{
    // ===================================================================== self test

    public static bool SelfTest(out string report)
    {
        var fails = new List<string>();
        int total = 0;

        void Check(bool ok, string what) { total++; if (!ok) fails.Add(what); }
        static bool Close(double a, double b) => Math.Abs(a - b) <= 1e-6 * Math.Max(1, Math.Abs(b));
        static string Show(CalcResult? r) => r == null ? "null" : $"'{r.Display}' ({r.Value.ToString(Inv)} {r.Unit})";
        CalcResult? NoteLine(int line, params string[] ls) => EvaluateNote(ls)[line]?.Result;
        CalcResult? One(string expr) => NoteLine(1, "math", expr);
        void Val(string expr, double exp)
        {
            var r = One(expr);
            Check(r != null && Close(r.Value, exp), $"{expr} => {Show(r)}, expected {exp.ToString(Inv)}");
        }
        void Disp(string expr, string exp)
        {
            var r = One(expr);
            Check(r?.Display == exp, $"{expr} => {Show(r)}, expected '{exp}'");
        }
        void None(string expr)
        {
            var r = One(expr);
            Check(r == null, $"{expr} => {Show(r)}, expected no result");
        }

        // arithmetic & precedence
        Val("2+3*4", 14);
        Val("2^3^2", 512);
        Val("(1+2)*3", 9);
        Val("-3^2", -9);
        Val("10 / 4", 2.5);
        Val("2^-1", 0.5);
        Val("2(3+1)", 8);
        Val("2pi", 2 * Math.PI);
        Disp("2pi", "6.283185");
        Val("3 x 4", 12);
        Val("6 × 7", 42);
        Val("8 ÷ 2", 4);
        Val("2 plus 3 times 4", 14);
        Val("10 minus 4", 6);
        Val("10 mod 3", 1);
        Val("10 % 3", 1);
        None("1/0");
        None("2 +");
        // number literals
        Val("1,000 + 1", 1001);
        Val("1,000,000 / 1000", 1000);
        Val("5k * 2", 10000);
        Disp("5k * 2", "10,000");
        Val("2M / 4", 500000);
        Val("0x10", 16);
        Val("0x1F + 1", 32);
        Val(".5 + 1", 1.5);
        Val("1_000 * 2", 2000);
        Val("1.5e3", 1500);
        Val("1e-3 * 1000", 1);
        // percent
        Val("50 + 10%", 55);
        Val("50 - 10%", 45);
        Val("20% of 150", 30);
        Val("15% off 80", 68);
        Disp("30 as % of 120", "25%");
        Disp("20%", "20%");
        Val("200 * 10%", 20);
        // functions & constants
        Val("sqrt(16)", 4);
        Val("cbrt(27)", 3);
        Val("round(3.14159, 2)", 3.14);
        Val("round(2.5)", 3);
        Val("max(1,5,3)", 5);
        Val("min(4, 2, 8)", 2);
        Val("avg(2, 4)", 3);
        Val("pow(2, 10)", 1024);
        Val("abs(-5)", 5);
        Val("floor(2.7) + ceil(2.1)", 5);
        Val("log(1000)", 3);
        Val("ln(e)", 1);
        Val("log2(8)", 3);
        Val("sin(30°)", 0.5);
        Val("sin(30 deg)", 0.5);
        Val("cos(0)", 1);
        Val("tau / 2", Math.PI);
        Val("π", Math.PI);
        None("sqrt(-1)");
        Val("max(100,200,300)", 300);
        // currency
        Disp("$1200 / 3", "$400");
        Disp("$10.5 * 3", "$31.50");
        Disp("$1250", "$1,250");
        Disp("€3 + 2", "€5");
        // units
        Disp("5 km in mi", "3.11 mi");
        Disp("1 km + 500 m", "1.5 km");
        Disp("3 ft + 2 in", "3.17 ft");
        Disp("100 F in C", "37.78 °C");
        Disp("0 °C to °F", "32 °F");
        Val("2 GB in MB", 2000);
        Val("1 GiB in MiB", 1024);
        Disp("90 min in h", "1.5 h");
        Disp("60 mph in km/h", "96.56 km/h");
        Val("1 acre in m2", 4046.8564224);
        Val("2 cups in ml", 473.176473);
        Disp("5 kg * 2", "10 kg");
        Disp("10 km / 2", "5 km");
        Val("1 km / 500 m", 2);
        Disp("12 in to cm", "30.48 cm");
        Disp("5km in m", "5,000 m");
        None("5 km + 3 kg");
        None("5 km in kg");
        // comments, labels, prose
        Val("Coffee: 4.50", 4.5);
        Val("2 ^ 10 // comment", 1024);
        None("# heading");
        None("// note");
        None("groceries for the week");
        None("I'll buy 3 apples, maybe.");
        None("foo + 1");
        None("");
        None("Groceries:");

        // variables
        var nv = EvaluateNote(["math", "rent = 1200", "food = 300", "rent + food"]);
        Check(nv[0] == null, "line 0 (math keyword) should be null");
        Check(nv.Count == 4, "EvaluateNote returns one entry per line");
        Check(nv[1]?.AssignedName == "rent" && nv[1]?.Result?.Value == 1200, "rent = 1200 assigns");
        Check(nv[3]?.Result?.Value == 1500, $"rent + food => {Show(nv[3]?.Result)}");
        Check(NoteLine(3, "math", "a = 2", "a = a * 5", "a + 1")?.Value == 11, "reassignment");
        Check(NoteLine(2, "math", "m = 3", "2 m")?.Value == 6, "variable m shadows metre unit");
        Check(NoteLine(2, "math", "x = 4", "3 x")?.Value == 12, "variable x is implicit multiply, not operator");
        Check(NoteLine(2, "math", "rent: 1200", "rent + 1") == null, "colon is not assignment");
        // prev / sum / avg
        Check(NoteLine(2, "math", "10", "prev * 2")?.Value == 20, "prev");
        Check(NoteLine(2, "math", "7", "ans")?.Value == 7, "ans");
        Check(NoteLine(6, "math", "5", "", "1", "2", "3", "sum")?.Value == 6, "sum stops at blank line");
        Check(NoteLine(4, "math", "1", "2", "3", "total * 2")?.Value == 12, "total in expression");
        Check(NoteLine(5, "math", "100", "# Food", "5", "6", "total")?.Value == 11, "total stops at heading");
        Check(NoteLine(3, "math", "2", "4", "avg")?.Value == 3, "avg block");
        Check(NoteLine(3, "math", "Coffee: 4", "Tea: 2", "average")?.Value == 3, "average over labelled lines");
        Check(NoteLine(3, "math", "1 km", "500 m", "sum")?.Display == "1.5 km", "sum with units");
        Check(NoteLine(1, "math", "sum")?.Value == 0, "sum of empty block is 0");

        // tokens
        var tk = EvaluateNote(["math", "rent = 1200 * 2 // monthly"])[1];
        CalcToken[] expTok =
        [
            new(0, 4, TokenKind.Identifier), new(5, 1, TokenKind.Operator), new(7, 4, TokenKind.Number),
            new(12, 1, TokenKind.Operator), new(14, 1, TokenKind.Number), new(16, 10, TokenKind.Comment),
        ];
        Check(tk != null && tk.Tokens.SequenceEqual(expTok),
            "tokens for rent line: " + (tk == null ? "null" : string.Join(" ", tk.Tokens)));
        Check(tk?.Result?.Value == 2400 && tk.AssignedName == "rent", "rent line result 2400");
        var tl = EvaluateNote(["math", "Coffee: $4.50"])[1];
        Check(tl != null && tl.Tokens.Count == 2 && tl.Tokens[0] == new CalcToken(0, 7, TokenKind.Comment)
              && tl.Tokens[1] == new CalcToken(8, 5, TokenKind.Number), "label tokens");
        var th = EvaluateNote(["math", "  # Heading"])[1];
        Check(th != null && th.Tokens.Count == 1 && th.Tokens[0] == new CalcToken(2, 9, TokenKind.Comment), "heading token");
        var tu = EvaluateNote(["math", "5 km in mi"])[1];
        Check(tu != null && tu.Tokens.Select(t => t.Kind).SequenceEqual(
            [TokenKind.Number, TokenKind.Unit, TokenKind.Keyword, TokenKind.Unit]), "unit/keyword tokens");
        var tf = EvaluateNote(["math", "sqrt(x) + y"])[1];
        Check(tf != null && tf.Result == null && tf.Tokens.Count == 6 && tf.Tokens[0].Kind == TokenKind.Function
              && tf.Tokens[2].Kind == TokenKind.Identifier, "unknown identifiers still tokenized");

        // inline
        Check(EvaluateInline("12*4 =")?.Value == 48, "inline 12*4 =");
        Check(EvaluateInline("12*4=  ")?.Display == "48", "inline trailing spaces");
        Check(EvaluateInline("hello =") == null, "inline hello =");
        Check(EvaluateInline("5 =") == null, "inline 5 = (no operator)");
        Check(EvaluateInline("12*4") == null, "inline without =");
        var inl = EvaluateInline("5 km in m =");
        Check(inl != null && inl.Value == 5000 && inl.Unit == "m", $"inline 5 km in m = => {Show(inl)}");
        Check(EvaluateInline("sqrt(81) =")?.Value == 9, "inline function");
        Check(EvaluateInline("a = 2 + 3 =") == null, "inline rejects assignment");
        Check(EvaluateInline("prev + 1 =") == null, "inline has no context keywords");

        // format
        Check(Format(1234.5, null) == "1,234.5", "Format 1234.5");
        Check(Format(0.1 + 0.2, null) == "0.3", "Format 0.1+0.2");
        Check(Format(1e16, null) == "1e+16", $"Format 1e16 => {Format(1e16, null)}");
        Check(Format(double.NaN, null) == "", "Format NaN");
        Check(Format(-12.5, "$") == "-$12.50", "Format -$12.50");
        Check(Format(3.10686, "mi") == "3.11 mi", "Format mi");

        // dates, with the clock pinned to Thu 2026-10-22 09:30
        var savedClock = Clock;
        try
        {
            Clock = () => new DateTime(2026, 10, 22, 9, 30, 0);
            Disp("today", "Thu, Oct 22 2026");
            Disp("today + 30 days", "Sat, Nov 21 2026");
            Disp("days until dec 25", "64 days");
            Disp("dec 25 - today", "64 days");
            Disp("tomorrow", "Fri, Oct 23 2026");
            Disp("yesterday", "Wed, Oct 21 2026");
            Disp("today + 1 month", "Sun, Nov 22 2026");
            Disp("tomorrow + 1 month", "Mon, Nov 23 2026");
            Disp("today + 1 year", "Fri, Oct 22 2027");
            Disp("days until friday", "1 day");
            Disp("days until thursday", "7 days");
            Disp("weeks until 2027-01-01", "10.1 weeks");
            Disp("days since 2026-01-01", "294 days");
            Disp("now + 90 min", "Thu, Oct 22 2026 11:00");
            Disp("dec 25 - 2 weeks", "Fri, Dec 11 2026");
            Disp("25 dec", "Fri, Dec 25 2026");
            Disp("December 25", "Fri, Dec 25 2026");
            Disp("Dec 25, 2027", "Sat, Dec 25 2027");
            Disp("jan 5", "Tue, Jan 5 2027");
            Disp("oct 22", "Thu, Oct 22 2026");
            Disp("2026-12-25", "Fri, Dec 25 2026");
            Disp("30 days + today", "Sat, Nov 21 2026");
            Disp("dec 25 - today in weeks", "9.1 weeks");
            Disp("3 days in h", "72 h");
            Disp("90 min in h", "1.5 h");
            var dr = One("today");
            Check(dr != null && dr.Unit == "date" && dr.Value == new DateTime(2026, 10, 22).ToOADate(), $"today value => {Show(dr)}");
            var cr = One("days until dec 25");
            Check(cr != null && cr.Unit == "days" && cr.Value == 64, $"days until value => {Show(cr)}");
            None("see you tomorrow at lunch");
            None("today * 2");
            None("today + 5");
            None("-today");
            None("today + tomorrow");
            None("feb 30 2027");
            Val("2026-13-01", 2012);   // not a date: plain arithmetic
            Check(NoteLine(2, "math", "launch = dec 25", "days until launch")?.Display == "64 days", "date variable");
            Check(NoteLine(2, "math", "launch = dec 25", "launch + 1 week")?.Display == "Fri, Jan 1 2027", "date variable arithmetic");
            Check(NoteLine(3, "math", "today", "tomorrow", "sum") == null, "sum of dates has no result");
            var td = EvaluateNote(["math", "days until dec 25"])[1];
            Check(td != null && td.Tokens.Select(t => t.Kind).SequenceEqual(
                [TokenKind.Unit, TokenKind.Keyword, TokenKind.Keyword, TokenKind.Number]), "date tokens");
            Check(EvaluateInline("today + 1 week =")?.Display == "Thu, Oct 29 2026", "inline today + 1 week =");
            Check(EvaluateInline("today + 30 days =")?.Display == "Sat, Nov 21 2026", "inline today + 30 days =");
            Check(EvaluateInline("today =") == null, "inline today = (no operator)");
            Check(Format(new DateTime(2026, 12, 25, 14, 5, 0).ToOADate(), "date") == "Fri, Dec 25 2026 14:05", "Format date with time");
            Check(Format(1, "days") == "1 day" && Format(2.5, "weeks") == "2.5 weeks", "Format day/week counts");
        }
        finally { Clock = savedClock; }

        report = fails.Count == 0
            ? $"Calculator.SelfTest: all {total} checks passed."
            : $"Calculator.SelfTest: {total - fails.Count}/{total} passed. Failures:\n  " + string.Join("\n  ", fails);
        return fails.Count == 0;
    }
}
