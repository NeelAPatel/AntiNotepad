using System.Text.Json;
namespace AntiQuickPad.Core;
public enum SaveState { Pending, Saving, Saved, Failed }
public sealed record WindowSettings(double Width, double Height, int X, int Y, bool Pinned,
    string ShellId = "classic", string LookId = "Ink", int LayoutVersion = 0, Preferences? Preferences = null);
/// <summary>M11a-settings-function: user preferences. Null means "use the design-token default".</summary>
public sealed record Preferences(string? Material = null, double? Tint = null, string? Bars = null, string? Hotkey = null,
    bool StartWithWindows = false, double? FontSize = null, double? MarkerOpacity = null, double? StrikeEm = null)
{
    public static readonly string[] Materials = ["acrylic", "mica", "solid"];
    public static readonly string[] BarModes = ["hover", "always"];
    /// <summary>Clamp every value to its supported range; unknown strings fall back to the default.</summary>
    public Preferences Normalized() => this with
    {
        Material = Material is { } m && Materials.Contains(m) ? m : null,
        Tint = Tint is { } t && double.IsFinite(t) ? Math.Clamp(t, .5, 1) : null,
        Bars = Bars is { } b && BarModes.Contains(b) ? b : null,
        Hotkey = string.IsNullOrWhiteSpace(Hotkey) ? null : Hotkey.Trim(),
        FontSize = FontSize is { } f && double.IsFinite(f) ? Math.Clamp(Math.Round(f), 11, 22) : null,
        MarkerOpacity = MarkerOpacity is { } o && double.IsFinite(o) ? Math.Clamp(o, .2, 1) : null,
        StrikeEm = StrikeEm is { } s && double.IsFinite(s) ? Math.Clamp(s, .05, .25) : null
    };
}
public sealed class SettingsStore(string path)
{
    public WindowSettings? Load()
    {
        if (!File.Exists(path)) return null;
        WindowSettings? value;
        try { value = JsonSerializer.Deserialize<WindowSettings>(File.ReadAllText(path)); }
        catch (JsonException) { return null; }
        if (value is null || !double.IsFinite(value.Width) || !double.IsFinite(value.Height)) return null;
        // Retired palette names become Quiet's shell-independent theme preferences.
        var look = value.LookId ?? "system";
        var legacyLook = look.Replace(" ", "").Replace("-", "").Replace("_", "").ToLowerInvariant();
        look = legacyLook switch
        {
            "card" or "cardsystem" => "system",
            "cardlight" => "light",
            "carddark" => "dark",
            _ => look
        };
        return value with { ShellId = string.Equals(value.ShellId, "card", StringComparison.OrdinalIgnoreCase) ? "quiet" : value.ShellId ?? "quiet", LookId = look,
            Preferences = (value.Preferences ?? new Preferences()).Normalized() };
    }
    public void Save(WindowSettings value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(value));
        File.Move(temp, path, true);
    }
}
