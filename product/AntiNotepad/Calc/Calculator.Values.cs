using System.Globalization;

namespace AntiNotepad.Calc;

public static partial class Calculator
{
    // ===================================================================== values

    // Date: V is an OLE automation date (U, Cur, Pct unused)
    private readonly record struct Q(double V, UnitDef? U = null, string? Cur = null, bool Pct = false, bool Date = false);
    private readonly record struct ContextResult(Q? Value, CalcErrorReason ErrorReason = CalcErrorReason.None);

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
        if (l.Cur != null && r.Cur != null && l.Cur != r.Cur) return false;
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

}
