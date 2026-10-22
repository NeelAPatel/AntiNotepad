using System.Text.Json;
using System.Text.Json.Serialization;

namespace Antipad.Model;

public sealed class Note
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];
    public string Text { get; set; } = "";
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Modified { get; set; } = DateTime.Now;
    public int Caret { get; set; }
    public double Scroll { get; set; }
    public DateTime? Deleted { get; set; }
    /// <summary>0 = ephemeral stack note; 1..9 = permanent slot.</summary>
    public int Slot { get; set; }
    [JsonIgnore] public bool IsEmpty => string.IsNullOrWhiteSpace(Text);
    public string Title
    {
        get
        {
            var skip = Modes.Detect(Text).KeywordLength > 0 ? 1 : 0;
            foreach (var raw in Text.Split('\n').Skip(skip))
            {
                var line = raw.Trim().TrimStart('#', '-', '*', '>', ' ').Replace("[ ]", "").Replace("[x]", "").Trim();
                if (line.Length > 0) return line.Length > 60 ? line[..60] + "…" : line;
            }
            return "Empty note";
        }
    }
}

public sealed class Settings
{
    public string Theme { get; set; } = "system";          // system | light | dark
    public string LightPalette { get; set; } = "paper";
    public string DarkPalette { get; set; } = "ink";
    public string Paper { get; set; } = "blank";           // blank | lined | dotted | grid | biggrid
    public int PaperStrength { get; set; } = 1;            // 0 subtle, 1 clear, 2 bold
    public string Material { get; set; } = "acrylic";      // acrylic | mica | solid
    public double Tint { get; set; } = 0.82;
    public double FontSize { get; set; } = 15;
    public string Font { get; set; } = "Cascadia Mono";
    public string? Hotkey { get; set; }
    public int FreshNoteAfterMinutes { get; set; } = 30;   // 0 = never
    public bool HideOnBlur { get; set; }
    public bool DimWhenPinned { get; set; } = true;     // a pinned pad fades back while you work elsewhere
    public string? Accent { get; set; }                   // theme maker: null = palette default
    public string Tone { get; set; } = "default";          // theme maker paper tone: default | warm | cool | green | rose | grey
    public string SplitPalette { get; set; } = "same";     // the split pane's own palette id, or same
    public string OpenAt { get; set; } = "remember";      // remember | center | mouse | tray | dropdown
    public bool ReduceMotion { get; set; }
    public string Chrome { get; set; } = "fade";           // fade | always
    public bool InlineMath { get; set; } = true;
    public bool ShortenLinks { get; set; } = true;
    public bool CleanMarkdown { get; set; } = true;
    public bool Pinned { get; set; }
    public double Width { get; set; } = 460;
    public double Height { get; set; } = 560;
    public int? X { get; set; }
    public int? Y { get; set; }
    public int SplitSlot { get; set; } = 1;
    public bool Split { get; set; }
    public bool SearchGrid { get; set; }
    public bool Welcomed { get; set; }
}

/// <summary>Everything Antipad keeps, in one atomically replaced JSON file plus a .bak.</summary>
public sealed class StoreData
{
    public List<Note> Stack { get; set; } = [];
    public Note[] Slots { get; set; } = Enumerable.Range(1, 9).Select(i => new Note { Slot = i }).ToArray();
    public List<Note> Trash { get; set; } = [];
    public int Current { get; set; } = -1;
    /// <summary>0 = viewing the stack; 1..9 = viewing that slot.</summary>
    public int ViewSlot { get; set; }
    public Settings Settings { get; set; } = new();
}

public sealed class Store
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
    private readonly string _path;
    public StoreData Data { get; private set; } = new();
    public string? LastError { get; private set; }
    public event Action? Saved;

    public Store(string directory)
    {
        Directory.CreateDirectory(directory);
        _path = Path.Combine(directory, "antipad.json");
    }

    public string Folder => Path.GetDirectoryName(_path)!;

    public void Load()
    {
        foreach (var candidate in new[] { _path, _path + ".bak" })
        {
            try
            {
                if (!File.Exists(candidate)) continue;
                var data = JsonSerializer.Deserialize<StoreData>(File.ReadAllText(candidate), Json);
                if (data is null) continue;
                Data = data; break;
            }
            catch (Exception ex) { LastError = $"load {Path.GetFileName(candidate)}: {ex.Message}"; }
        }
        Normalize();
    }

    private void Normalize()
    {
        var d = Data;
        if (d.Slots is not { Length: 9 }) d.Slots = Enumerable.Range(1, 9).Select(i => d.Slots?.ElementAtOrDefault(i - 1) ?? new Note { Slot = i }).ToArray();
        for (var i = 0; i < 9; i++) { d.Slots[i] ??= new Note(); d.Slots[i].Slot = i + 1; }
        d.Stack.RemoveAll(n => n is null);
        if (d.Stack.Count == 0) d.Stack.Add(new Note());
        d.Current = Math.Clamp(d.Current < 0 ? d.Stack.Count - 1 : d.Current, 0, d.Stack.Count - 1);
        d.ViewSlot = Math.Clamp(d.ViewSlot, 0, 9);
        d.Settings.SplitSlot = Math.Clamp(d.Settings.SplitSlot, 1, 9);
        // Trash keeps 30 days.
        d.Trash.RemoveAll(n => n.Deleted is { } when && (DateTime.Now - when).TotalDays > 30);
    }

    public bool Save()
    {
        try
        {
            var tmp = _path + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(Data, Json));
            if (File.Exists(_path)) File.Replace(tmp, _path, _path + ".bak", ignoreMetadataErrors: true);
            else File.Move(tmp, _path);
            LastError = null; Saved?.Invoke(); return true;
        }
        catch (Exception ex) { LastError = ex.Message; return false; }
    }
}
