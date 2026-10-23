using Avalonia.Media;
namespace AntiQuickPad.Design;
internal sealed record DesignSpec(string Id, string Title, string Subtitle, string Surface, string Canvas, string Rule, Look Look)
{
    public static DesignSpec Get(string id)
    {
        static Color C(string hex) => Color.Parse(hex);
        return id switch
        {
            "quiet" => new(id, "Quiet", "A little room to think", "#FAFAF7", "#FAFAF7", "#E2E4DF",
                new Look("Quiet", C("#FAFAF7"), C("#29332F"), C("#758079"), C("#537864"), "Segoe UI Variable Text", "Segoe UI Variable Text", 17, false)
                { EditorPadding = 0, HeadingColors = [C("#29332F"), C("#3F5146"), C("#3F5146")], HeadingSizes = [30, 22, 19], MarkdownMarker = C("#9DA69F"),
                    CodeFont = "Cascadia Mono", CodePanel = C("#EEF0EB"), Keyword = C("#70578C"), String = C("#52765B"), Comment = C("#7E8880"),
                    CheckboxSize = 14, CheckboxRadius = 4, CheckboxStroke = 1.25, CheckboxCheck = C("#FFFFFF"), IndentGuide = C("#DDE2DC"), IndentGuideActive = C("#A1B5A5"), StrikeThicknessEm = .10 }),
            "card" => new(id, "Card", "Your thoughts, one at a time", "#F8FAFF", "#DEE5F0", "#D5DDEB",
                new Look("Card", C("#F8FAFF"), C("#283852"), C("#76849C"), C("#556CD2"), "Segoe UI Variable Text", "Segoe UI Variable Text", 16, false)
                { EditorPadding = 0, HeadingColors = [C("#283852"), C("#475B8B"), C("#475B8B")], HeadingSizes = [30, 22, 19], MarkdownMarker = C("#A0ABC0"),
                    CodeFont = "Cascadia Mono", CodePanel = C("#ECF0F9"), Keyword = C("#7955A2"), String = C("#47745A"), Comment = C("#79889D"),
                    CheckboxSize = 15, CheckboxRadius = 5, CheckboxStroke = 1.4, CheckboxCheck = C("#FFFFFF"), IndentGuide = C("#D6DEEF"), IndentGuideActive = C("#9BAADE"), StrikeThicknessEm = .10 }),
            _ => new("pro", "Pro", "Everything within reach", "#20252C", "#1A1E24", "#343B45",
                Look.Default with { Name = "Pro", EditorFont = "Consolas", CodeFont = "Consolas", Background = C("#20252C"), Foreground = C("#DCE2EA"), Muted = C("#8896A8"), Accent = C("#9BB8DB"),
                    FontSize = 15, EditorPadding = 0, ShowGutter = true, HeadingColors = [C("#E4EBF5"), C("#A9BCD6"), C("#A9BCD6")], HeadingSizes = [25, 20, 18],
                    CodePanel = C("#272F39"), CheckboxSize = 13, CheckboxRadius = 3, CheckboxStroke = 1.2, IndentGuide = C("#37414F"), IndentGuideActive = C("#758BA9"), StrikeThicknessEm = .10 })
        };
    }
}



