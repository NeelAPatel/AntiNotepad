using System.Globalization;

namespace AntiNotepad.Calc;

public static partial class Calculator
{
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
        ["monday"] = DayOfWeek.Monday,
        ["tuesday"] = DayOfWeek.Tuesday,
        ["wednesday"] = DayOfWeek.Wednesday,
        ["thursday"] = DayOfWeek.Thursday,
        ["friday"] = DayOfWeek.Friday,
        ["saturday"] = DayOfWeek.Saturday,
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

}
