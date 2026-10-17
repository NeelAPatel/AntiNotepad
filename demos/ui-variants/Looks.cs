using Avalonia;
using Avalonia.Media;

namespace UiVariants;

// A "Look" is the whole theme: palette + type + gutter + feature switches. Swapping a theme = picking another Look.
internal sealed record Look
{
    public required string Name { get; init; }
    public required IBrush Backdrop { get; init; }        // stand-in for the desktop behind the window (NOT a real backdrop)
    public required string Panel { get; init; }
    public required string Border { get; init; }
    public required string Fg { get; init; }
    public required string Dim { get; init; }            // markdown markers
    public required string[] Headings { get; init; }     // H1..H6
    public required string Accent { get; init; }
    public required string Keyword { get; init; }
    public required string Str { get; init; }
    public required string Comment { get; init; }
    public required string CodePanel { get; init; }
    public required string EditorFont { get; init; }
    public required string CodeFont { get; init; }       // fenced code lines
    public double FontSize { get; init; } = 16;
    public bool LineNumbers { get; init; } = true;
    public string LnBrush { get; init; } = "#55FFFFFF";
    public string LnCurrent { get; init; } = "#CCFFFFFF";
    public double LnSize { get; init; } = 11;
    public bool LnSeparator { get; init; } = true;
    public bool HighlightCurrent { get; init; }
    public string CurrentBg { get; init; } = "#14FFFFFF";
    public bool CheckboxGraphics { get; init; }
    public bool Folding { get; init; }
    public double Pad { get; init; } = 16;
    public string FontNote { get; init; } = "";
    public bool Light { get; init; }

    static Brush G(string a, string b) => new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative), EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops = { new GradientStop(Color.Parse(a), 0), new GradientStop(Color.Parse(b), 1) }
    };

    public static readonly Dictionary<string, Look> All = new()
    {
        ["ink"] = new()
        {
            Name = "Ink", Backdrop = G("#1B2433", "#3A2B4A"),
            Panel = "#F2161A22", Border = "#33FFFFFF", Fg = "#E6E9EF", Dim = "#6E7686",
            Headings = ["#FFB86B", "#F2A8A0", "#C9A7E8", "#8FB8F5", "#7DD3E0", "#7DD3E0"],
            Accent = "#FFB86B", Keyword = "#C9A7E8", Str = "#9FDCB0", Comment = "#6E7686", CodePanel = "#16FFFFFF",
            EditorFont = "Segoe UI Variable Text, Segoe UI", CodeFont = "Cascadia Code, Consolas", FontSize = 17,
            LnBrush = "#4DFFFFFF", LnSize = 11, Pad = 16,
            FontNote = "Segoe UI Variable Text (proportional) + Cascadia Code (code fence)"
        },
        ["paper"] = new()
        {
            Name = "Paper", Light = true, Backdrop = G("#D8CFC0", "#BFB3A0"),
            Panel = "#FFFAF3E6", Border = "#33000000", Fg = "#3B3228", Dim = "#B3A58F",
            Headings = ["#8A3B12", "#A5541F", "#B97A3E", "#7A6A4F", "#7A6A4F", "#7A6A4F"],
            Accent = "#C2571A", Keyword = "#8A3B12", Str = "#4F7A3A", Comment = "#A39683", CodePanel = "#14000000",
            EditorFont = "Georgia, Cambria", CodeFont = "Cascadia Mono, Consolas", FontSize = 19,
            LineNumbers = false, CheckboxGraphics = true, Pad = 28,
            FontNote = "Georgia (serif body) + Cascadia Mono (code fence)"
        },
        ["terminal"] = new()
        {
            Name = "Terminal", Backdrop = G("#000000", "#10151C"),
            Panel = "#FF000000", Border = "#FF2D3B2D", Fg = "#E8F5E8", Dim = "#4F7A4F",
            Headings = ["#FFFF55", "#55FFFF", "#FF79FF", "#55FF55", "#FFAA55", "#FFAA55"],
            Accent = "#55FF55", Keyword = "#FF79FF", Str = "#55FF55", Comment = "#7A9A7A", CodePanel = "#FF0F1F14",
            EditorFont = "Cascadia Mono, Consolas", CodeFont = "Cascadia Mono, Consolas", FontSize = 16,
            LnBrush = "#99A0C8A0", LnCurrent = "#FFFFFF55", LnSize = 15, HighlightCurrent = true, CurrentBg = "#2255FF55", Folding = true, Pad = 12,
            FontNote = "Cascadia Mono everywhere"
        },
        ["slate"] = new()
        {
            Name = "Slate", Backdrop = G("#3B4A5E", "#6F7F96"),
            Panel = "#F02B3445", Border = "#26FFFFFF", Fg = "#D5DCE8", Dim = "#7C8AA0",
            Headings = ["#A9D1FF", "#9EC3F0", "#93B6E0", "#8AA9D0", "#8AA9D0", "#8AA9D0"],
            Accent = "#7FB2F0", Keyword = "#9DB8F5", Str = "#9ED4C0", Comment = "#7C8AA0", CodePanel = "#22000000",
            EditorFont = "Segoe UI Variable Display, Segoe UI", CodeFont = "Cascadia Code, Consolas", FontSize = 18,
            LnBrush = "#40FFFFFF", LnSize = 11, LnSeparator = false, Pad = 24,
            FontNote = "Segoe UI Variable Display, wide padding, no gutter rule + Cascadia Code (code fence)"
        },
        ["midnight"] = new()
        {
            Name = "Midnight", Backdrop = G("#05060A", "#10131C"),
            Panel = "#FF07080C", Border = "#1AFFFFFF", Fg = "#B9BFCC", Dim = "#3E4452",
            Headings = ["#D9B26B", "#C8A262", "#B59358", "#A38550", "#A38550", "#A38550"],
            Accent = "#D9B26B", Keyword = "#D9B26B", Str = "#8C9A82", Comment = "#4A505E", CodePanel = "#0DFFFFFF",
            EditorFont = "Cascadia Code, Consolas", CodeFont = "Cascadia Code, Consolas", FontSize = 15,
            LnBrush = "#2EFFFFFF", LnSize = 10, Pad = 20,
            FontNote = "Cascadia Code, grey on near-black, single muted gold accent"
        },
    };
}
