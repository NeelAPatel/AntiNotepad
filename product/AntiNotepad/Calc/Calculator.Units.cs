using System.Globalization;

namespace AntiNotepad.Calc;

public static partial class Calculator
{
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

}
