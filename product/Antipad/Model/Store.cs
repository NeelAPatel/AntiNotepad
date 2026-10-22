using System.Text.Json;
using System.Text.Json.Serialization;

namespace Antipad.Model;

/// <summary>
/// A note. Its body lives in its own file and is loaded only when needed; while a note is open in an editor,
/// <see cref="Text"/> reads the live document instead of keeping a copy.
/// The index keeps small caches (title, blank, mode) so closed notes answer cheap questions without loading.
/// </summary>
public sealed class Note
{
    /// <summary>Set by the store: reads a note body from disk by id.</summary>
    internal static Func<string, string>? LoadBody;

    private string? _text;
    private Func<string>? _live;

    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Modified { get; set; } = DateTime.Now;
    public int Caret { get; set; }
    public double Scroll { get; set; }
    public DateTime? Deleted { get; set; }
    /// <summary>0 = ephemeral stack note; 1..9 = permanent slot.</summary>
    public int Slot { get; set; }
    public string? TitleCache { get; set; }
    public bool Blank { get; set; } = true;
    public NoteMode ModeCache { get; set; }

    [JsonIgnore] public bool Dirty { get; set; }
    [JsonIgnore] public bool IsLoaded => _text is not null || _live is not null;

    [JsonIgnore]
    public string Text
    {
        get => _live?.Invoke() ?? (_text ??= LoadBody?.Invoke(Id) ?? "");
        set { _text = value; _live = null; Dirty = true; Blank = string.IsNullOrWhiteSpace(value); ModeCache = Modes.Detect(value).Mode; TitleCache = null; }
    }

    /// <summary>The body without pulling it into memory for good (search, snippets).</summary>
    public string PeekText() => _live?.Invoke() ?? _text ?? LoadBody?.Invoke(Id) ?? "";

    /// <summary>An editor now owns the text; edits arrive through <see cref="Touched"/>.</summary>
    public void Attach(Func<string> live) { _live = live; _text = null; }

    /// <summary>Called by the editor on every edit, with cheap facts about the document.</summary>
    public void Touched(bool blank, NoteMode mode) { Dirty = true; Blank = blank; ModeCache = mode; Modified = DateTime.Now; TitleCache = null; }

    /// <summary>The editor released this note: keep nothing in memory unless it still needs saving.</summary>
    public void Detach()
    {
        if (_live is not null && Dirty) _text = _live();
        _live = null;
        if (!Dirty) _text = null;
    }

    [JsonIgnore] public bool IsEmpty => Blank;

    [JsonIgnore] public string Title => TitleCache ??= MakeTitle(PeekText());

    internal static string MakeTitle(string text)
    {
        var skip = Modes.Detect(text).KeywordLength > 0 ? 1 : 0;
        foreach (var raw in text.Split('\n').Skip(skip).Take(40))
        {
            var line = raw.Trim().TrimStart('#', '-', '*', '>', ' ').Replace("[ ]", "").Replace("[x]", "").Trim();
            if (line.Length > 0) return line.Length > 60 ? line[..60] + "…" : line;
        }
        return "Empty note";
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
    public double Tint { get; set; } = 0.7;
    public double FontSize { get; set; } = 15;
    public double UiScale { get; set; } = 1;              // scales chrome and text together
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
    public bool IndentGuides { get; set; } = true;
    public bool Folding { get; set; } = true;
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

/// <summary>The index: every note's metadata and settings. Bodies are separate files.</summary>
public sealed class StoreData
{
    public int Version { get; set; } = 2;
    public List<Note> Stack { get; set; } = [];
    public Note[] Slots { get; set; } = Enumerable.Range(1, 9).Select(i => new Note { Slot = i }).ToArray();
    public List<Note> Trash { get; set; } = [];
    public int Current { get; set; } = -1;
    /// <summary>0 = viewing the stack; 1..9 = viewing that slot.</summary>
    public int ViewSlot { get; set; }
    public Settings Settings { get; set; } = new();
    [JsonIgnore] public IEnumerable<Note> All => Stack.Concat(Slots).Concat(Trash);
}

/// <summary>
/// Storage: index.json (small, atomically replaced, with .bak) plus notes/&lt;id&gt;.md per body.
/// A save writes the index and only the bodies that changed. v1's single antipad.json is migrated on first load.
/// </summary>
public sealed class Store
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = { new JsonStringEnumConverter() }
    };
    private readonly string _index, _bodies, _legacy;
    public StoreData Data { get; private set; } = new();
    public string? LastError { get; private set; }

    public Store(string directory)
    {
        Directory.CreateDirectory(directory);
        _index = Path.Combine(directory, "index.json");
        _legacy = Path.Combine(directory, "antipad.json");
        _bodies = Path.Combine(directory, "notes");
        Directory.CreateDirectory(_bodies);
        Note.LoadBody = ReadBody;
    }

    public string Folder => Path.GetDirectoryName(_index)!;

    private string BodyPath(string id) => Path.Combine(_bodies, id + ".md");

    private string ReadBody(string id)
    {
        try { var path = BodyPath(id); return File.Exists(path) ? File.ReadAllText(path) : ""; }
        catch (Exception ex) { LastError = $"read {id}: {ex.Message}"; return ""; }
    }

    public void Load()
    {
        var loaded = false;
        foreach (var candidate in new[] { _index, _index + ".bak" })
        {
            try
            {
                if (!File.Exists(candidate)) continue;
                if (JsonSerializer.Deserialize<StoreData>(File.ReadAllText(candidate), Json) is not { } data) continue;
                Data = data; loaded = true; break;
            }
            catch (Exception ex) { LastError = $"load {Path.GetFileName(candidate)}: {ex.Message}"; }
        }
        if (!loaded && File.Exists(_legacy)) MigrateV1();
        Normalize();
        RemoveOrphans();
    }

    /// <summary>v1 kept every body inside antipad.json. Read them once, write v2, keep the old file as a backup.</summary>
    private void MigrateV1()
    {
        try
        {
            var json = File.ReadAllText(_legacy);
            if (JsonSerializer.Deserialize<StoreData>(json, Json) is not { } data) return;
            var root = System.Text.Json.Nodes.JsonNode.Parse(json)!;
            var texts = new Dictionary<string, string>();
            foreach (var list in new[] { "Stack", "Slots", "Trash" })
                foreach (var n in root[list]?.AsArray() ?? [])
                    if (n?["Id"]?.GetValue<string>() is { } id) texts[id] = n["Text"]?.GetValue<string>() ?? "";
            foreach (var note in data.All) note.Text = texts.GetValueOrDefault(note.Id, "");
            Data = data;
            Normalize();
            if (Save()) File.Move(_legacy, _legacy + ".v1-backup", overwrite: true);
        }
        catch (Exception ex) { LastError = "migrate: " + ex.Message; }
    }

    private void Normalize()
    {
        var d = Data;
        d.Version = 2;
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

    /// <summary>Body files whose note is gone (deleted forever, expired, dropped while empty).</summary>
    public void RemoveOrphans()
    {
        try
        {
            var live = Data.All.Select(n => n.Id).ToHashSet();
            foreach (var file in Directory.EnumerateFiles(_bodies, "*.md"))
                if (!live.Contains(Path.GetFileNameWithoutExtension(file))) File.Delete(file);
        }
        catch (Exception ex) { LastError = "tidy: " + ex.Message; }
    }

    public bool Save()
    {
        try
        {
            foreach (var note in Data.All.Where(n => n.Dirty).ToList())
            {
                var text = note.Text;
                note.TitleCache = Note.MakeTitle(text);
                note.ModeCache = Modes.Detect(text).Mode;
                Atomic(BodyPath(note.Id), text, backup: false);
                note.Dirty = false;
            }
            Atomic(_index, JsonSerializer.Serialize(Data, Json), backup: true);
            LastError = null; return true;
        }
        catch (Exception ex) { LastError = ex.Message; return false; }
    }

    private static void Atomic(string path, string content, bool backup)
    {
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, content);
        if (File.Exists(path)) File.Replace(tmp, path, backup ? path + ".bak" : null, ignoreMetadataErrors: true);
        else File.Move(tmp, path);
    }
}
