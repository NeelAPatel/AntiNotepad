using System.Globalization;

namespace AntiNotepad.Calc;

public enum TokenKind { Number, Identifier, Unit, Operator, Keyword, Comment, Function }

public readonly record struct CalcToken(int Start, int Length, TokenKind Kind);

/// <summary>Value is the number in the displayed unit. For percentages Unit is "%" and Value is the percentage (25 for "25%").</summary>
public sealed record CalcResult(string Display, double Value, string? Unit);

public sealed class LineEval
{
    public CalcResult? Result { get; init; }          // null = no result to show (blank, comment, heading, error, incomplete)
    public IReadOnlyList<CalcToken> Tokens { get; init; } = [];  // for syntax colouring, offsets within the line
    public string? AssignedName { get; init; }        // set for `name = expr`
}

/// <summary>Notepad-calculator engine (Soulver/Numi style). Pure, allocation-light, no exceptions on the hot path.</summary>
public static class Calculator
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    // ===================================================================== units

    private enum Dim { Length, Mass, Time, Data, Volume, Temperature, Speed, Area }

    // base value = v * Factor + Offset (Offset only for temperature)
    private sealed record UnitDef(string Symbol, Dim Dim, double Factor, double Offset);

    private static readonly Dictionary<string, UnitDef> Units = BuildUnits();

    private static Dictionary<string, UnitDef> BuildUnits()
    {
        var d = new Dictionary<string, UnitDef>(StringComparer.Ordinal);
        void Add(string sym, Dim dim, double factor, double offset, params string[] names)
        {
            var u = new UnitDef(sym, dim, factor, offset);
            d[sym] = u;
            foreach (var n in names) d[n] = u;
        }
        // length (base m)
        Add("mm", Dim.Length, 0.001, 0, "millimeter", "millimeters");
        Add("cm", Dim.Length, 0.01, 0, "centimeter", "centimeters");
        Add("m", Dim.Length, 1, 0, "meter", "meters", "metre", "metres");
        Add("km", Dim.Length, 1000, 0, "kilometer", "kilometers", "kilometre", "kilometres");
        Add("in", Dim.Length, 0.0254, 0, "inch", "inches");
        Add("ft", Dim.Length, 0.3048, 0, "foot", "feet");
        Add("yd", Dim.Length, 0.9144, 0, "yard", "yards");
        Add("mi", Dim.Length, 1609.344, 0, "mile", "miles");
        // mass (base g)
        Add("mg", Dim.Mass, 0.001, 0);
        Add("g", Dim.Mass, 1, 0, "gram", "grams");
        Add("kg", Dim.Mass, 1000, 0, "kilogram", "kilograms", "kilo", "kilos");
        Add("lb", Dim.Mass, 453.59237, 0, "lbs", "pound", "pounds");
        Add("oz", Dim.Mass, 28.349523125, 0, "ounce", "ounces");
        Add("t", Dim.Mass, 1e6, 0, "tonne", "tonnes");
        // time (base s)
        Add("ms", Dim.Time, 0.001, 0);
        Add("s", Dim.Time, 1, 0, "sec", "secs", "second", "seconds");
        Add("min", Dim.Time, 60, 0, "mins", "minute", "minutes");
        Add("h", Dim.Time, 3600, 0, "hr", "hrs", "hour", "hours");
        Add("d", Dim.Time, 86400, 0, "day", "days");
        Add("wk", Dim.Time, 604800, 0, "week", "weeks");
        Add("mo", Dim.Time, 2629800, 0, "month", "months");   // average month; calendar months when added to a date
        Add("yr", Dim.Time, 31557600, 0, "year", "years");
        // data (base bit); KB = 1000, KiB = 1024
        Add("bit", Dim.Data, 1, 0, "b", "bits");
        Add("B", Dim.Data, 8, 0, "byte", "bytes");
        Add("KB", Dim.Data, 8e3, 0, "kB");
        Add("MB", Dim.Data, 8e6, 0);
        Add("GB", Dim.Data, 8e9, 0);
        Add("TB", Dim.Data, 8e12, 0);
        Add("KiB", Dim.Data, 8.0 * 1024, 0);
        Add("MiB", Dim.Data, 8.0 * 1024 * 1024, 0);
        Add("GiB", Dim.Data, 8.0 * 1024 * 1024 * 1024, 0);
        Add("TiB", Dim.Data, 8.0 * 1024 * 1024 * 1024 * 1024, 0);
        // volume (base ml)
        Add("ml", Dim.Volume, 1, 0, "mL");
        Add("L", Dim.Volume, 1000, 0, "l", "liter", "liters", "litre", "litres");
        Add("cup", Dim.Volume, 236.5882365, 0, "cups");
        Add("tbsp", Dim.Volume, 14.78676478125, 0);
        Add("tsp", Dim.Volume, 4.92892159375, 0);
        Add("floz", Dim.Volume, 29.5735295625, 0);
        Add("gal", Dim.Volume, 3785.411784, 0, "gallon", "gallons");
        // temperature (base K, affine)
        Add("K", Dim.Temperature, 1, 0, "kelvin");
        Add("°C", Dim.Temperature, 1, 273.15, "C", "degC", "celsius");
        Add("°F", Dim.Temperature, 5.0 / 9.0, 273.15 - 32.0 * 5.0 / 9.0, "F", "degF", "fahrenheit");
        // speed (base m/s)
        Add("m/s", Dim.Speed, 1, 0, "mps");
        Add("km/h", Dim.Speed, 1 / 3.6, 0, "kmh", "kph");
        Add("mph", Dim.Speed, 0.44704, 0);
        // area (base m²)
        Add("m²", Dim.Area, 1, 0, "m2", "sqm");
        Add("km²", Dim.Area, 1e6, 0, "km2");
        Add("sqft", Dim.Area, 0.09290304, 0, "ft2", "ft²");
        Add("acre", Dim.Area, 4046.8564224, 0, "acres");
        Add("ha", Dim.Area, 1e4, 0, "hectare", "hectares");
        return d;
    }

    private static UnitDef? LookupUnit(string name)
    {
        if (Units.TryGetValue(name, out var u)) return u;
        // case-insensitive fallback only for longer names ("Miles", "KM" no: 2 chars stays exact to keep MB/Mb, kB distinct)
        if (name.Length >= 3 && Units.TryGetValue(name.ToLowerInvariant(), out u)) return u;
        return null;
    }

    // duration units with calendar meaning next to a date; day/week counts (results of date differences) display as "64 days"
    private static readonly UnitDef DayUnit = Units["d"], WeekUnit = Units["wk"], MonthUnit = Units["mo"], YearUnit = Units["yr"];
    private static readonly UnitDef DayCount = new("days", Dim.Time, 86400, 0), WeekCount = new("weeks", Dim.Time, 604800, 0);

    private static bool IsCount(UnitDef? u) => ReferenceEquals(u, DayCount) || ReferenceEquals(u, WeekCount);

    private static double ToBase(double v, UnitDef u) => v * u.Factor + u.Offset;
    private static double FromBase(double b, UnitDef u) => (b - u.Offset) / u.Factor;

    // ===================================================================== words

    private static readonly HashSet<string> Functions = new(StringComparer.Ordinal)
    {
        "sqrt", "cbrt", "abs", "round", "floor", "ceil", "sin", "cos", "tan", "asin", "acos", "atan",
        "log", "ln", "log2", "exp", "min", "max", "avg", "average", "sum", "pow",
    };

    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "in", "to", "as", "of", "off", "prev", "ans", "sum", "total", "avg", "average", "plus", "minus", "times", "mod",
    };

    // words that can never be variables or operands
    private static readonly HashSet<string> Reserved = new(StringComparer.Ordinal)
    {
        "in", "to", "as", "of", "off", "plus", "minus", "times", "mod",
    };

    private static readonly HashSet<string> ContextWords = new(StringComparer.Ordinal)
    {
        "prev", "ans", "sum", "total", "avg", "average",
    };

    private static bool TryConstant(string name, out double v)
    {
        switch (name)
        {
            case "pi": case "π": v = Math.PI; return true;
            case "e": v = Math.E; return true;
            case "tau": case "τ": v = 2 * Math.PI; return true;
            default: v = 0; return false;
        }
    }

    private static bool IsDegreeWord(string s) => s is "deg" or "degree" or "degrees";

    // ===================================================================== dates

    /// <summary>Current time source; SelfTest pins it to a fixed instant.</summary>
    internal static Func<DateTime> Clock = () => DateTime.Now;

    private static readonly Dictionary<string, int> Months = BuildMonths();

    private static Dictionary<string, int> BuildMonths()
    {
        var d = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        string[] full = ["january", "february", "march", "april", "may", "june", "july", "august", "september", "october", "november", "december"];
        for (int i = 0; i < 12; i++) { d[full[i]] = i + 1; d[full[i][..3]] = i + 1; }
        d["sept"] = 9;
        return d;
    }

    private static readonly Dictionary<string, DayOfWeek> Weekdays = new(StringComparer.OrdinalIgnoreCase)
    {
        ["monday"] = DayOfWeek.Monday, ["tuesday"] = DayOfWeek.Tuesday, ["wednesday"] = DayOfWeek.Wednesday,
        ["thursday"] = DayOfWeek.Thursday, ["friday"] = DayOfWeek.Friday, ["saturday"] = DayOfWeek.Saturday,
        ["sunday"] = DayOfWeek.Sunday,
    };

    // coloured as keywords even when the line does not parse
    private static readonly HashSet<string> DateWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "today", "tomorrow", "yesterday", "now", "until", "till", "since",
    };

    private static bool Eq(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static DateTime ToDt(double oa) => DateTime.FromOADate(oa);
    private static Q DateQ(DateTime dt) => new(dt.ToOADate(), Date: true);

    private static bool TryDateWord(string name, out Q q)
    {
        q = default;
        DateTime now = Clock(), today = now.Date;
        if (Eq(name, "today")) q = DateQ(today);
        else if (Eq(name, "tomorrow")) q = DateQ(today.AddDays(1));
        else if (Eq(name, "yesterday")) q = DateQ(today.AddDays(-1));
        else if (Eq(name, "now")) q = DateQ(now);
        else if (Weekdays.TryGetValue(name, out var wd))
        {
            int ahead = ((int)wd - (int)today.DayOfWeek + 7) % 7;
            q = DateQ(today.AddDays(ahead == 0 ? 7 : ahead));   // the next one; a week out when it is today
        }
        else return false;
        return true;
    }

    // a date without a year (year 0) is the next occurrence on or after today
    private static bool TryMakeDate(int year, int month, int day, out Q q)
    {
        q = default;
        if (day < 1 || day > 31) return false;
        if (year != 0)
        {
            if (year < 100 || year > 9999 || day > DateTime.DaysInMonth(year, month)) return false;
            q = DateQ(new DateTime(year, month, day));
            return true;
        }
        var today = Clock().Date;
        for (int y = today.Year; y <= today.Year + 8 && y <= 9999; y++)
        {
            if (day > DateTime.DaysInMonth(y, month)) continue;
            var dt = new DateTime(y, month, day);
            if (dt >= today) { q = DateQ(dt); return true; }
        }
        return false;
    }

    // date +/- duration; whole months and years move by calendar
    private static bool TryShiftDate(Q d, Q dur, double sign, out Q res)
    {
        res = default;
        if (dur.Date || dur.Pct || dur.Cur != null || dur.U == null || dur.U.Dim != Dim.Time || !double.IsFinite(dur.V)) return false;
        double n = sign * dur.V;
        var dt = ToDt(d.V);
        try
        {
            bool cal = ReferenceEquals(dur.U, MonthUnit) || ReferenceEquals(dur.U, YearUnit);
            if (cal && n == Math.Round(n) && Math.Abs(n) <= 120000)
                dt = ReferenceEquals(dur.U, MonthUnit) ? dt.AddMonths((int)n) : dt.AddYears((int)n);
            else
            {
                double secs = n * dur.U.Factor;
                if (Math.Abs(secs) > 3.2e11) return false;
                dt = dt.AddSeconds(secs);
            }
        }
        catch (ArgumentOutOfRangeException) { return false; }
        if (dt.Year < 100) return false;
        res = DateQ(dt);
        return true;
    }

    private static bool TryDateAdd(Q l, Q r, double sign, out Q res)
    {
        res = default;
        if (l.Date && r.Date)
        {
            if (sign > 0) return false;
            res = new Q((ToDt(l.V) - ToDt(r.V)).TotalDays, DayCount);
            return true;
        }
        if (l.Date) return TryShiftDate(l, r, sign, out res);
        return sign > 0 && TryShiftDate(r, l, 1, out res);   // 30 days + today
    }

    private static string FormatDate(double oa)
    {
        if (oa < -657434 || oa > 2958465) return "";
        var dt = ToDt(oa);
        const long M = TimeSpan.TicksPerMinute;
        dt = new DateTime((dt.Ticks + M / 2) / M * M);
        return dt.ToString(dt.TimeOfDay == TimeSpan.Zero ? "ddd, MMM d yyyy" : "ddd, MMM d yyyy HH:mm", Inv);
    }

    // ===================================================================== values

    // Date: V is an OLE automation date (U, Cur, Pct unused)
    private readonly record struct Q(double V, UnitDef? U = null, string? Cur = null, bool Pct = false, bool Date = false);

    private static bool TryConvert(Q q, UnitDef to, out Q r)
    {
        r = default;
        if (q.Date) return false;
        if (q.U == null) { r = q with { U = to, Pct = false }; return true; }   // "5 in cm" just labels a bare number
        if (q.U.Dim != to.Dim) return false;
        r = q with { V = ReferenceEquals(q.U, to) ? q.V : FromBase(ToBase(q.V, q.U), to), U = to };
        return true;
    }

    private static bool TryAdd(Q l, Q r, double sign, out Q res)
    {
        res = default;
        if (l.Date || r.Date) return TryDateAdd(l, r, sign, out res);
        if (l.U != null && r.U != null && !TryConvert(r, l.U, out r)) return false;
        res = new Q(l.V + sign * r.V, l.U ?? r.U, l.Cur ?? r.Cur, l.Pct && r.Pct);
        return true;
    }

    private static CalcResult? ToResult(Q q)
    {
        double v = q.V;
        if (!double.IsFinite(v)) return null;
        if (q.Date) return new CalcResult(Format(v, "date"), v, "date");
        string? unit;
        if (q.U != null) unit = q.U.Symbol;
        else if (q.Pct) { unit = "%"; v *= 100; }
        else unit = q.Cur;
        if (v == 0) v = 0; // drop negative zero
        return new CalcResult(Format(v, unit), v, unit);
    }

    // ===================================================================== formatting

    private static readonly string[] PlainFmt = ["0", "0.#", "0.##", "0.###", "0.####", "0.#####", "0.######"];
    private static readonly string[] SepFmt = ["#,##0", "#,##0.#", "#,##0.##", "#,##0.###", "#,##0.####", "#,##0.#####", "#,##0.######"];

    public static string Format(double value, string? unit)
    {
        if (!double.IsFinite(value)) return "";
        if (value == 0) value = 0;
        if (unit == "date") return FormatDate(value);
        if (unit is "days" or "weeks")
        {
            double rc = Math.Round(value, 1, MidpointRounding.AwayFromZero);
            if (rc == 0) rc = 0;
            string nc = rc.ToString(Math.Abs(rc) >= 1000 ? SepFmt[1] : PlainFmt[1], Inv);
            return nc + " " + (Math.Abs(rc) == 1 ? unit[..^1] : unit);
        }
        bool currency = unit is "$" or "€" or "£";
        double abs = Math.Abs(value);
        string num;
        if (abs >= 1e15 || (abs > 0 && abs < 1e-6))
        {
            num = value.ToString("0.##e+0", Inv);
            if (currency) return unit + num;
        }
        else if (currency)
        {
            double r = Math.Round(value, 2, MidpointRounding.AwayFromZero);
            bool integer = Math.Abs(r - Math.Round(r)) < 1e-9;
            num = Math.Abs(r).ToString(integer ? "#,##0" : "#,##0.00", Inv);
            return (r < 0 ? "-" : "") + unit + num;
        }
        else
        {
            int dec = unit != null && unit != "%" && abs >= 1 ? 2 : 6;
            double r = Math.Round(value, dec, MidpointRounding.AwayFromZero);
            if (r == 0) r = 0;
            num = r.ToString(Math.Abs(r) >= 1000 ? SepFmt[dec] : PlainFmt[dec], Inv);
        }
        if (unit == null) return num;
        return unit == "%" ? num + "%" : num + " " + unit;
    }

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
                case '+': case '-': case '*': case '/': case '^': case '×': case '÷':
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

    // ===================================================================== parser

    private sealed class Parser(List<Tok> toks, int start, int end, TokenKind?[] kinds,
                                Dictionary<string, Q> vars, Func<string, Q?>? ctx)
    {
        private static readonly Tok EndTok = new(RT.End, 0, 0);
        public int P = start;
        public bool Fail;
        public bool SawOp;   // any binary operator, function, percent or conversion (for EvaluateInline)

        private Tok Cur => P < end ? toks[P] : EndTok;
        private Tok Peek(int k) => P + k < end ? toks[P + k] : EndTok;
        private void Mark(TokenKind k) { if (P < end) kinds[P] = k; }
        private void Take(TokenKind k) { Mark(k); P++; }
        private Q Bad() { Fail = true; return default; }

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
            if (IsOp(Cur, '-')) { Take(TokenKind.Operator); var q = ParseUnary(); return q.Date ? Bad() : q with { V = -q.V }; }
            if (IsOp(Cur, '+')) { Take(TokenKind.Operator); return ParseUnary(); }
            return ParsePower();
        }

        private Q ParsePower()
        {
            var b = ParsePostfix();
            if (Fail || !IsOp(Cur, '^')) return b;
            Take(TokenKind.Operator);
            SawOp = true;
            var e = ParseUnary();   // right-associative; -3^2 = -9 since unary sits above
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
                    Take(TokenKind.Operator);
                    var q = ParseExpr();
                    if (Fail) return default;
                    if (Cur.T != RT.RParen) return Bad();
                    Take(TokenKind.Operator);
                    return UnitSuffix(q);
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
                        var cv = ctx?.Invoke(name);
                        return cv ?? Bad();
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
                case "min": case "max": case "avg": case "average": case "sum":
                {
                    double acc = a.V;
                    for (int i = 1; i < args.Count; i++)
                    {
                        var b = args[i];
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

    private static LineEval EvalLine(string line, Dictionary<string, Q> vars, Func<string, Q?>? ctx,
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
        if (first < count)
        {
            var parser = new Parser(raw, first, count, kinds, vars, ctx);
            var q = parser.ParseExpr();
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

        return new LineEval { Result = result, Tokens = tokens, AssignedName = result != null ? assign : null };
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
    private static Q? Context(string kw, int index, Q?[] vals, LineClass[] cls)
    {
        if (kw is "prev" or "ans")
        {
            for (int j = index - 1; j >= 1 && cls[j] == LineClass.Normal; j--)
                if (vals[j] is { } v) return v;
            return null;
        }
        Q? acc = null;
        int count = 0;
        for (int j = index - 1; j >= 1 && cls[j] == LineClass.Normal; j--)
        {
            if (vals[j] is not { } v) continue;
            v = v with { Pct = false };
            if (acc is not { } a) acc = v;
            else if (TryAdd(v, a, 1, out var sumQ)) acc = sumQ;   // topmost line's unit wins
            else return null;
            count++;
        }
        if (kw is "sum" or "total") return acc ?? new Q(0);
        if (acc is not { } total || count == 0) return null;
        return total with { V = total.V / count };
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

    public static CalcResult? EvaluateInline(string line)
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
            if (parser.Fail || parser.P != raw.Count || !parser.SawOp) return null;
            return ToResult(q);
        }
        catch (Exception)
        {
            return null;
        }
    }

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
        Check(NoteLine(1, "math", "sum") ?.Value == 0, "sum of empty block is 0");

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
