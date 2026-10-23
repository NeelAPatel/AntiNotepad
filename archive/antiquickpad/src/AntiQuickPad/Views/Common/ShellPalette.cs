using Avalonia.Media;
using AntiQuickPad.Tokens;

namespace AntiQuickPad.Views.Common;

// M6c-design-tokens: the editor Look and shell colours come only from Tokens/quiet.tokens.json.
internal sealed record ShellPalette(Look Look, Color Canvas, Color Rule, Color ActiveLine, DesignTokens Tokens, DesignTokens.Colors Colors)
{
    public static ShellPalette Get(string shell, bool dark, Core.Preferences? preferences = null)
    {
        var t = DesignTokens.Quiet;
        var c = t.For(dark);
        var p = preferences ?? new Core.Preferences();
        // M11b-settings-window: preferences override token defaults (material, tint, font size, marker opacity, strike).
        var size = p.FontSize ?? t.BodySize;
        var material = p.Material ?? t.Material;
        var look = new Look(dark ? "Quiet Dark" : "Quiet", c.Canvas, c.Ink, c.Muted, c.Accent, t.EditorFont, t.InterfaceFont, size, false)
        {
            Material = material, TintAlpha = material == "solid" ? 1 : p.Tint ?? t.TintAlpha, CodeFont = t.CodeFont, GrabHeight = t.Grab,
            EditorPadding = t.EditorPadding, HeadingColors = c.Heading, HeadingSizes = t.HeadingSizes.Select(h => Math.Round(h * size / t.BodySize)).ToArray(),
            MarkdownMarker = p.MarkerOpacity is { } o ? Color.FromArgb((byte)Math.Round(c.Marker.A * o), c.Marker.R, c.Marker.G, c.Marker.B) : c.Marker,
            CodePanel = c.CodePanel, Keyword = c.Keyword, String = c.String, Comment = c.Comment,
            CheckboxSize = t.Checkbox, CheckboxRadius = t.CheckboxRadius, CheckboxStroke = t.CheckboxStroke, CheckboxCheck = c.CheckboxCheck,
            IndentGuide = c.Guide, IndentGuideActive = c.GuideActive, StrikeThicknessEm = p.StrikeEm ?? t.StrikeEm, StatusSize = t.StatusSize
        };
        return new(look, c.Canvas, c.Rule, c.ActiveLine, t, c);
    }
}
