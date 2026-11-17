// User preferences persisted in the store's index.
using System.Text.Json;
using System.Text.Json.Serialization;
using AntiNotepad.Theme;

namespace AntiNotepad.Model;

public sealed class Settings
{
    public string Theme { get; set; } = "system";          // system | light | dark
    public string LightPalette { get; set; } = "paper";
    public string DarkPalette { get; set; } = "ink";
    public string Paper { get; set; } = "blank";           // blank | lined | dotted | grid | biggrid
    public int PaperStrength { get; set; } = 1;            // 0 subtle, 1 clear, 2 bold
    public string Material { get; set; } = "acrylic";      // acrylic | mica | solid
    public double Tint { get; set; } = 0.7;
    public double FontSize { get; set; } = 15;
    public int TextWidth { get; set; }                    // characters per line before the column centres; 0 = use the whole pad
    public double LineSpacing { get; set; } = 1.32;
    public string Bubbles { get; set; } = "column";       // answer bubbles: column (just past the longest line) | edge (right edge of the text)
    public double UiScale { get; set; } = 1;              // scales chrome and text together
    public string Font { get; set; } = "Cascadia Mono";
    public string? Hotkey { get; set; }
    public string? NewNoteHotkey { get; set; }            // opens straight to a new note; null = off
    public int FreshNoteAfterMinutes { get; set; } = 30;   // 0 = never
    internal static readonly IReadOnlyList<int> SaveDelayChoicesMs = Array.AsReadOnly<int>([
        ChromeTokens.SaveDelayDefaultMs, ChromeTokens.SaveDelayFiveSecondsMs,
        ChromeTokens.SaveDelayTenSecondsMs, ChromeTokens.SaveDelayThirtySecondsMs]);

    private int _saveDelayMs = ChromeTokens.SaveDelayDefaultMs;
    public int SaveDelayMs
    {
        get => _saveDelayMs;
        set => _saveDelayMs = SaveDelayChoicesMs.Contains(value) ? value : ChromeTokens.SaveDelayDefaultMs;
    }
    public bool HideOnBlur { get; set; }
    public bool DimWhenPinned { get; set; } = true;     // a pinned pad fades back while you work elsewhere
    public string? Accent { get; set; }                   // theme maker: null = palette default
    public string Tone { get; set; } = "default";          // theme maker paper tone: default | warm | cool | green | rose | grey
    public string SplitPalette { get; set; } = "same";     // the split pane's own palette id, or same
    public string OpenAt { get; set; } = "remember";      // remember | center | mouse | tray | dropdown
    public bool ReduceMotion { get; set; }
    public string Scrolling { get; set; } = "rate";       // lines | free | rate (rate-matched glide) | wheel (wheel-driven sheet)
    // Tuning for the chosen feel (the Scroll Lab sliders, as presets).
    public int LinesPerNotch { get; set; } = 3;           // lines
    public double ScrollDistance { get; set; } = 1;       // free, rate, wheel: distance per click, x the picked default
    public double ScrollBoost { get; set; } = 1;          // rate, wheel: extra distance when spinning fast, x the picked default
    public double FreeFriction { get; set; } = 6;         // free: how quickly a coast stops
    public double SwipeLength { get; set; } = 220;
    public double SwipeFiresAt { get; set; } = 0.35;
    public string Swipe { get; set; } = "follow";
    [JsonIgnore]
    public string HorizontalScrollStyle => "follow";
    public string Chrome { get; set; } = "fade";           // fade | always
    public int FadeAfterMs { get; set; } = 1600;          // how long the controls stay after the mouse stops
    public string Bars { get; set; } = "docked";          // docked (top bar row, pager below the text) | overlay (both slide in over the note at the edges)
    public bool EdgeFade { get; set; } = true;            // text fades into the top/bottom edge where there is more to scroll
    public bool MenuActionRow { get; set; } = true;
    public bool MenuColorIcons { get; set; }
    public string CaretStyle { get; set; } = "bar";        // bar | block
    public bool CaretBlink { get; set; } = true;
    public bool CaretGlide { get; set; } = true;
    public bool InlineMath { get; set; } = true;
    public bool ShortenLinks { get; set; } = true;
    public bool CleanMarkdown { get; set; } = true;
    public bool IndentGuides { get; set; } = true;
    public bool Folding { get; set; } = true;
    public List<string> DisabledLanguages { get; set; } = [];
    public bool Pinned { get; set; }
    public string Taskbar { get; set; } = "never";          // visible | always | never
    public double Width { get; set; } = 460;
    public double Height { get; set; } = 560;
    public int? X { get; set; }
    public int? Y { get; set; }
    public int SplitSlot { get; set; } = 1;
    public bool Split { get; set; }
    public bool SearchGrid { get; set; }
    public bool Welcomed { get; set; }

    internal Settings CopyForSave()
    {
        var copy = (Settings)MemberwiseClone();
        copy.DisabledLanguages = DisabledLanguages is null ? [] : [.. DisabledLanguages];
        return copy;
    }
}

