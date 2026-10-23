using Avalonia.Media;

namespace AntiNotepad.Theme;

/// <summary>Semantic colour roles. Syntax roles are deliberately distinct: never one accent for everything.</summary>
public sealed record Palette(
    string Id, string Name, bool Dark,
    Color Canvas,      // window tint under the text
    Color Ink,         // body text
    Color Muted,       // secondary text, markers
    Color Faint,       // metadata, chrome glyphs at rest
    Color Keyword,     // first-line mode keyword, code keywords
    Color Identifier,  // math variables, code identifiers
    Color Number,      // numeric literals
    Color Result,      // computed answers
    Color Link,
    Color Selection,
    Color Done,        // completed checklist text
    Color Error,
    Color Confirm,
    Color Paper,       // paper texture strokes
    Color Caret,
    Color Heading,
    Color String,
    Color Comment,
    Color Surface,     // raised panels (search, settings, popups)
    Color Accent)      // chrome focus/selection accents
{
    public static readonly Palette[] All =
    [
        new("paper", "Paper", false,
            C("#FBF8F1"), C("#2B2A27"), C("#8A857A"), C("#B8B2A6"), C("#C2571A"), C("#2F6F9F"), C("#7A4FB0"), C("#B5761A"),
            C("#2F6F9F"), C("#33F2B84B"), C("#A39E92"), C("#C23B32"), C("#3C8D4F"), C("#1F000000"), C("#C2571A"), C("#1F1E1C"),
            C("#3C8D4F"), C("#9A958A"), C("#FFFDF8"), C("#D9822B")),
        new("snow", "Snow", false,
            C("#F7F8FA"), C("#22262E"), C("#7D8594"), C("#AEB4C0"), C("#C8326B"), C("#2463B8"), C("#7E3FC4"), C("#0E8A6E"),
            C("#2463B8"), C("#332463B8"), C("#9AA1AE"), C("#C8324A"), C("#178A4C"), C("#1A1F3A70"), C("#2463B8"), C("#151820"),
            C("#178A4C"), C("#8F96A3"), C("#FFFFFF"), C("#2F7CF6")),
        new("sage", "Sage", false,
            C("#F3F5EF"), C("#273027"), C("#7F8A7C"), C("#B0B8AC"), C("#8A5A12"), C("#2E6B55"), C("#80548F"), C("#B04E2E"),
            C("#2E6B55"), C("#33739A5C"), C("#9BA597"), C("#B8392E"), C("#3C8D4F"), C("#1C2E4A2E"), C("#2E6B55"), C("#1C241C"),
            C("#5B7F2A"), C("#94A08F"), C("#FBFCF8"), C("#4E8A5B")),
        new("ink", "Ink", true,
            C("#1C1B19"), C("#E8E4DA"), C("#8F897D"), C("#5E5A52"), C("#F0A05A"), C("#7FB7E6"), C("#C9A2F0"), C("#F2C46B"),
            C("#7FB7E6"), C("#40F2B84B"), C("#6E695F"), C("#F07167"), C("#7CCB8A"), C("#1FFFFFFF"), C("#F2B84B"), C("#F5F1E8"),
            C("#9BD48E"), C("#7A7468"), C("#262522"), C("#F2B84B")),
        new("midnight", "Midnight", true,
            C("#14171F"), C("#DCE3F0"), C("#8189A0"), C("#4E5468"), C("#FF8FB1"), C("#82AAFF"), C("#C792EA"), C("#7FE0C2"),
            C("#82AAFF"), C("#4082AAFF"), C("#5F667C"), C("#FF6E7A"), C("#7FE0A0"), C("#1CA0B4FF"), C("#82AAFF"), C("#F0F4FF"),
            C("#C3E88D"), C("#697089"), C("#1C2030"), C("#82AAFF")),
        new("graphite", "Graphite", true,
            C("#1E1F21"), C("#E3E3E3"), C("#8C8D90"), C("#55575B"), C("#E5C07B"), C("#61AFEF"), C("#D19A66"), C("#98C379"),
            C("#61AFEF"), C("#4061AFEF"), C("#66686C"), C("#E06C75"), C("#98C379"), C("#18FFFFFF"), C("#E3E3E3"), C("#FFFFFF"),
            C("#98C379"), C("#75777B"), C("#27282B"), C("#9DA5B4")),
        new("contrast-light", "High contrast light", false,
            C("#FFFFFF"), C("#000000"), C("#3A3A3A"), C("#5A5A5A"), C("#A30000"), C("#00419E"), C("#5C00A3"), C("#7A4A00"),
            C("#00419E"), C("#5500419E"), C("#5A5A5A"), C("#B00000"), C("#006B1F"), C("#40000000"), C("#000000"), C("#000000"),
            C("#006B1F"), C("#4A4A4A"), C("#FFFFFF"), C("#00419E")),
        new("contrast-dark", "High contrast dark", true,
            C("#000000"), C("#FFFFFF"), C("#C8C8C8"), C("#A0A0A0"), C("#FFD24A"), C("#7CC4FF"), C("#E3A6FF"), C("#7CFFA0"),
            C("#7CC4FF"), C("#667CC4FF"), C("#A0A0A0"), C("#FF7A7A"), C("#7CFFA0"), C("#50FFFFFF"), C("#FFFFFF"), C("#FFFFFF"),
            C("#7CFFA0"), C("#B0B0B0"), C("#0A0A0A"), C("#FFD24A")),
    ];

    public static Palette Get(string id, bool dark) =>
        All.FirstOrDefault(p => p.Id == id && p.Dark == dark) ?? All.First(p => p.Dark == dark);

    public static readonly (string Hex, string Name)[] Accents =
    [
        ("#D9822B", "Amber"), ("#E5484D", "Red"), ("#D6409F", "Pink"), ("#8E4EC6", "Violet"),
        ("#3E63DD", "Indigo"), ("#0090FF", "Blue"), ("#12A594", "Teal"), ("#46A758", "Green"),
    ];

    public static readonly (string Id, string Name)[] Tones = [("default", "Palette"), ("warm", "Warm"), ("cool", "Cool"), ("green", "Green"), ("rose", "Rose"), ("grey", "Grey")];

    /// <summary>Theme maker: an accent and a paper tone layered over a base palette. Syntax roles stay distinct.</summary>
    public Palette Customize(string? accent, string tone)
    {
        var p = this;
        if (accent is not null && Color.TryParse(accent, out var a))
            p = p with { Accent = a, Caret = a, Selection = Color.FromArgb(Dark ? (byte)0x55 : (byte)0x40, a.R, a.G, a.B) };
        if (tone == "default") return p;
        (Color light, Color dark) = tone switch
        {
            "warm" => (C("#FBF6EC"), C("#1F1B16")),
            "cool" => (C("#F3F6FB"), C("#161A21")),
            "green" => (C("#F1F6EF"), C("#161D18")),
            "rose" => (C("#FBF2F3"), C("#201719")),
            _ => (C("#F5F5F5"), C("#1C1C1C")),
        };
        var canvas = Dark ? dark : light;
        var surface = Dark ? Mix(canvas, Colors.White, .06) : Mix(canvas, Colors.White, .6);
        return p with { Canvas = canvas, Surface = surface };
    }

    /// <summary>Structure marks (fences, fold chevrons, indent guides): Faint reads on dark glass, light glass needs more ink.</summary>
    public Color Marker => Dark ? Faint : Mix(Muted, Ink, .35);

    private static Color Mix(Color a, Color b, double t) =>
        Color.FromRgb((byte)(a.R + (b.R - a.R) * t), (byte)(a.G + (b.G - a.G) * t), (byte)(a.B + (b.B - a.B) * t));

    public static IEnumerable<Palette> For(bool dark) => All.Where(p => p.Dark == dark);

    private static Color C(string hex) => Color.Parse(hex);
}
