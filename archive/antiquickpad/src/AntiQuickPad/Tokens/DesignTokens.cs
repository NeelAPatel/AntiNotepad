using System.Text.Json;
using Avalonia;
using Avalonia.Media;

namespace AntiQuickPad.Tokens;

// M6c-design-tokens: the one token set. Loaded once from the embedded DTCG file; the Quiet shell reads only this.
internal sealed class DesignTokens
{
    internal sealed record Colors(Color Canvas, Color Ink, Color Muted, Color Accent, Color Rule, Color Surface,
        Color Marker, Color CodePanel, Color ActiveLine, Color Guide, Color GuideActive, Color CheckboxCheck,
        Color Keyword, Color String, Color Comment, Color[] Heading, Color Shadow);

    private static readonly Lazy<DesignTokens> Instance = new(() => Load(Read()));
    internal static DesignTokens Quiet => Instance.Value;

    internal Colors Light { get; private init; } = null!;
    internal Colors Dark { get; private init; } = null!;
    internal string Material { get; private init; } = "solid";
    internal double TintAlpha { get; private init; }
    internal string EditorFont { get; private init; } = "";
    internal string CodeFont { get; private init; } = "";
    internal string InterfaceFont { get; private init; } = "";
    internal string IconFont { get; private init; } = "";
    internal double BodySize { get; private init; }
    internal double[] HeadingSizes { get; private init; } = [];
    internal double IconSize { get; private init; }
    internal double StatusSize { get; private init; }
    internal double HintSize { get; private init; }
    internal double MenuHeadingSize { get; private init; }
    internal double MenuLabelSize { get; private init; }
    internal double Inset { get; private init; }
    internal double TopBar { get; private init; }
    internal double BottomBar { get; private init; }
    internal double BottomGap { get; private init; }
    internal double PageBottom { get; private init; }
    internal double ActionSpacing { get; private init; }
    internal double DotSpacing { get; private init; }
    internal double SaveLineSpacing { get; private init; }
    internal double MenuTop { get; private init; }
    internal double MenuPadding { get; private init; }
    internal double MenuItem { get; private init; }
    internal double MenuCheck { get; private init; }
    internal double MenuList { get; private init; }
    internal Size Window { get; private init; }
    internal Size WindowMin { get; private init; }
    internal Size Button { get; private init; }
    internal double TargetMin { get; private init; }
    internal double Dot { get; private init; }
    internal double DotSelected { get; private init; }
    internal Size DotSlot { get; private init; }
    internal double DotIdleOpacity { get; private init; }
    internal double MenuWidth { get; private init; }
    internal double MenuRow { get; private init; }
    internal double Grab { get; private init; }
    internal double FocusRing { get; private init; }
    internal double ButtonRadius { get; private init; }
    internal double PillRadius { get; private init; }
    internal double MenuRadius { get; private init; }
    internal double MenuItemRadius { get; private init; }
    internal double DotRadius { get; private init; }
    internal double MenuShadowY { get; private init; }
    internal double MenuShadowBlur { get; private init; }
    internal TimeSpan RevealIn { get; private init; }
    internal TimeSpan RevealOut { get; private init; }
    internal TimeSpan RevealLinger { get; private init; }
    internal double EditorPadding { get; private init; }
    internal double Checkbox { get; private init; }
    internal double CheckboxRadius { get; private init; }
    internal double CheckboxStroke { get; private init; }
    internal double StrikeEm { get; private init; }

    internal Colors For(bool dark) => dark ? Dark : Light;

    internal static string Read()
    {
        using var stream = typeof(DesignTokens).Assembly.GetManifestResourceStream("AntiQuickPad.Tokens.quiet.tokens.json")
            ?? throw new InvalidOperationException("Embedded token file missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    internal static DesignTokens Load(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        JsonElement V(string path)
        {
            var e = root;
            foreach (var part in path.Split('.'))
                if (!e.TryGetProperty(part, out e)) throw new InvalidOperationException($"Token missing: {path}");
            return e.TryGetProperty("$value", out var value) ? value : throw new InvalidOperationException($"Token has no $value: {path}");
        }
        double N(string path) => V(path).GetDouble();
        string S(string path) => V(path).GetString()!;
        double[] A(string path) => V(path).EnumerateArray().Select(x => x.GetDouble()).ToArray();
        Size Z(string path) { var a = A(path); return new Size(a[0], a[1]); }
        TimeSpan T(string path) => TimeSpan.FromMilliseconds(double.Parse(S(path).Replace("ms", ""), System.Globalization.CultureInfo.InvariantCulture));
        Colors C(string mode)
        {
            Color c(string name) => Color.Parse(S($"color.{mode}.{name}"));
            return new(c("canvas"), c("ink"), c("muted"), c("accent"), c("rule"), c("surface"), c("marker"), c("codePanel"),
                c("activeLine"), c("guide"), c("guideActive"), c("checkboxCheck"), c("keyword"), c("string"), c("comment"),
                V($"color.{mode}.heading").EnumerateArray().Select(x => Color.Parse(x.GetString()!)).ToArray(), c("shadow"));
        }
        var shadow = V("elevation.menu");
        return new DesignTokens
        {
            Light = C("light"), Dark = C("dark"),
            Material = S("material.kind"), TintAlpha = N("material.tintAlpha"),
            EditorFont = S("font.editor"), CodeFont = S("font.code"), InterfaceFont = S("font.interface"), IconFont = S("font.icon"),
            BodySize = N("font.size.body"), HeadingSizes = A("font.size.heading"), IconSize = N("font.size.icon"),
            StatusSize = N("font.size.status"), HintSize = N("font.size.hint"), MenuHeadingSize = N("font.size.menuHeading"), MenuLabelSize = N("font.size.menuLabel"),
            Inset = N("space.inset"), TopBar = N("space.topBar"), BottomBar = N("space.bottomBar"), BottomGap = N("space.bottomGap"),
            PageBottom = N("space.pageBottom"), ActionSpacing = N("space.actions"), DotSpacing = N("space.dots"), SaveLineSpacing = N("space.saveLine"),
            MenuTop = N("space.menuTop"), MenuPadding = N("space.menuPadding"), MenuItem = N("space.menuItem"), MenuCheck = N("space.menuCheck"), MenuList = N("space.menuList"),
            Window = Z("size.window"), WindowMin = Z("size.windowMin"), Button = Z("size.button"), TargetMin = N("size.targetMin"),
            Dot = N("size.dot"), DotSelected = N("size.dotSelected"), DotSlot = Z("size.dotSlot"), DotIdleOpacity = N("size.dotIdleOpacity"),
            MenuWidth = N("size.menuWidth"), MenuRow = N("size.menuRow"), Grab = N("size.grab"), FocusRing = N("size.focusRing"),
            ButtonRadius = N("radius.button"), PillRadius = N("radius.pill"), MenuRadius = N("radius.menu"), MenuItemRadius = N("radius.menuItem"), DotRadius = N("radius.dot"),
            MenuShadowY = shadow.GetProperty("offsetY").GetDouble(), MenuShadowBlur = shadow.GetProperty("blur").GetDouble(),
            RevealIn = T("motion.revealIn"), RevealOut = T("motion.revealOut"), RevealLinger = T("motion.revealLinger"),
            EditorPadding = N("editor.padding"), Checkbox = N("editor.checkbox"), CheckboxRadius = N("editor.checkboxRadius"),
            CheckboxStroke = N("editor.checkboxStroke"), StrikeEm = N("editor.strikeEm")
        };
    }

    // WCAG 2.x relative-luminance contrast; alpha colours are composited over the given background.
    internal static double Contrast(Color foreground, Color background)
    {
        static double Channel(double c) => c <= .03928 ? c / 12.92 : Math.Pow((c + .055) / 1.055, 2.4);
        static double L(Color c) => .2126 * Channel(c.R / 255d) + .7152 * Channel(c.G / 255d) + .0722 * Channel(c.B / 255d);
        var f = Over(foreground, background);
        var (a, b) = (L(f), L(background));
        return (Math.Max(a, b) + .05) / (Math.Min(a, b) + .05);
    }
    internal static Color Over(Color top, Color bottom)
    {
        var t = top.A / 255d;
        byte M(byte x, byte y) => (byte)Math.Round(x * t + y * (1 - t));
        return Color.FromRgb(M(top.R, bottom.R), M(top.G, bottom.G), M(top.B, bottom.B));
    }
}
