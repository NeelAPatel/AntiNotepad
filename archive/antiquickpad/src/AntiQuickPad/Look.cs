using Avalonia.Media;
namespace AntiQuickPad;
public sealed record Look(string Name, Color Background, Color Foreground, Color Muted,
    Color Accent, string EditorFont, string InterfaceFont, double FontSize, bool ShowGutter)
{
    public string Material { get; init; } = "solid";
    public double TintAlpha { get; init; } = 1;
    public Color[] HeadingColors { get; init; } = [Color.Parse("#FFD6A5"), Color.Parse("#9FD8FF"), Color.Parse("#C4B5FD"), Color.Parse("#A7F3D0"), Color.Parse("#F9A8D4"), Color.Parse("#FCA5A5")];
    public double[] HeadingSizes { get; init; } = [28, 25, 22, 20, 18, 17];
    public Color MarkdownMarker { get; init; } = Color.Parse("#737F8E");
    public double StrikeThicknessEm { get; init; } = .18; // ~3 DIP at 17 DIP (user: much thicker). Renderer uses max(1.5 DIP, em * size) at half the actual x glyph height.
    public string CodeFont { get; init; } = "Cascadia Code, Consolas";
    public Color CodePanel { get; init; } = Color.Parse("#222E3B");
    public Color Keyword { get; init; } = Color.Parse("#C4A7FF");
    public Color String { get; init; } = Color.Parse("#A9E6B5");
    public Color Comment { get; init; } = Color.Parse("#8998A9");
    public bool ShowFolding { get; init; } = true;
    public bool ModernEditor { get; init; }
    public Color ActiveLine { get; init; } = Colors.Transparent;
    public double EditorPadding { get; init; } = 20;
    public bool ShowIndentGuides { get; init; } = true;
    public Color IndentGuide { get; init; } = Color.Parse("#445160");
    public Color IndentGuideActive { get; init; } = Color.Parse("#71859B");
    public double IndentGuideThickness { get; init; } = 1;
    public double CheckboxSize { get; init; } = 17;
    public double CheckboxRadius { get; init; } = 4;
    public double CheckboxStroke { get; init; } = 1.6;
    public Color CheckboxCheck { get; init; } = Color.Parse("#171D25");
    public double CodeRadius { get; init; } = 6;
    public double CodePanelInset { get; init; } = 2;
    public double CheckboxCheckStroke { get; init; } = 2;
    public double StatusSize { get; init; } = 11;
    public double GrabHeight { get; init; } = 15;
    public double IndicatorIdleOpacity { get; init; } = .15;
    public double BorderSize { get; init; } = 1;
    public double WindowWidth { get; init; } = 800;
    public double WindowHeight { get; init; } = 600;
    public double MinWindowWidth { get; init; } = 360;
    public double MinWindowHeight { get; init; } = 240;
    public double StatusVerticalMargin { get; init; } = 5;
    public static Look Default { get; } = new("Ink", Color.Parse("#171D25"),
        Color.Parse("#EBEFF4"), Color.Parse("#9BA9B8"), Color.Parse("#7FA9CA"),
        "Cascadia Mono, Consolas", "Segoe UI", 17, false);
}
