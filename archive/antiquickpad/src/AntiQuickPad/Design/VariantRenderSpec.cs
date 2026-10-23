namespace AntiQuickPad.Design;

internal sealed record VariantRenderSpec(
    string Id,
    string Title,
    double Width,
    double Height,
    double TextSize,
    double Insets,
    double TopBarHeight,
    double BottomBarHeight,
    string Tradeoff,
    bool PaperGrid = false,
    bool EdgeStrips = false,
    bool Bare = false)
{
    internal static IReadOnlyList<VariantRenderSpec> All { get; } =
    [
        new("V1-compact-pill", "Compact pill", 480, 560, 14, 24, 38, 36,
            "The closest AntiNote-like footprint; it trades writing room for a calm, legible control capsule."),
        new("V2-roomy-calm", "Roomy calm", 760, 680, 15, 48, 46, 42,
            "The most forgiving reading surface; it trades compactness for breathing room and full-width rules."),
        new("V3-paper-grid", "Paper grid", 480, 560, 14, 24, 38, 36,
            "A tactile dotted writing surface; it trades a little visual quiet for paper orientation." , PaperGrid: true),
        new("V4-edge-strips", "Edge strips", 560, 640, 14, 32, 50, 44,
            "A full-width information edge; it trades overlay subtlety for explicit title, date and save context.", EdgeStrips: true),
        new("V5-bare", "Bare", 520, 600, 14, 28, 38, 38,
            "The editor is almost all that remains; it trades discoverability for the quietest idle state.", Bare: true)
    ];

    internal static VariantRenderSpec Get(string id) =>
        All.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"Unknown variant: {id}");
}
