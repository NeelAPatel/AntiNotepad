using System.Text.Json;
using System.Text.Json.Serialization;

namespace AntiNotepad.Model;

/// <summary>
/// A note. Its body lives in its own file and is loaded only when needed; while a note is open in an editor,
/// <see cref="Text"/> reads the live document instead of keeping a copy.
/// The index keeps small caches (title, blank, mode) so closed notes answer cheap questions without loading.
/// </summary>
public sealed class Note
{
    /// <summary>Set by the store: reads a note body from disk by id.</summary>
    internal static Func<string, BodyReadResult>? LoadBody;

    private string? _text;
    private Func<string>? _live;
    private long _searchRevision;

    /// <summary>Set by Store so search-only body cache entries expire immediately on edits and saves.</summary>
    internal static Action<string>? InvalidateSearchBody;

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
    [JsonIgnore] public bool BodyUnavailable { get; private set; }

    [JsonIgnore]
    public string Text
    {
        get
        {
            if (_live is not null) return _live();
            if (_text is not null) return _text;
            var result = ReadBody();
            if (result.Status != BodyReadStatus.Available) return "";
            return _text = result.Text ?? "";
        }
        set
        {
            if (_live is null && _text is null && ReadBody().Status != BodyReadStatus.Available) return;
            InvalidateSearchCache();
            _text = value; _live = null; Dirty = true; Blank = string.IsNullOrWhiteSpace(value); ModeCache = Modes.Detect(value).Mode; TitleCache = null;
        }
    }

    /// <summary>The body without pulling it into memory for good (search, snippets).</summary>
    public string PeekText()
    {
        if (_live is not null) return _live();
        if (_text is not null) return _text;
        var result = ReadBody();
        return result.Status == BodyReadStatus.Available ? result.Text ?? "" : "[Note text unavailable]";
    }

    private BodyReadResult ReadBody()
    {
        if (LoadBody is null) { BodyUnavailable = false; return new("", BodyReadStatus.Available); }
        var result = LoadBody(Id);
        BodyUnavailable = result.Status != BodyReadStatus.Available;
        if (result.Status == BodyReadStatus.Missing && Blank)
        {
            BodyUnavailable = false;
            return new("", BodyReadStatus.Available);
        }
        return result;
    }

    internal bool TryGetSearchText(out string? text)
    {
        if (_live is not null) { text = _live(); return true; }
        text = _text;
        return text is not null;
    }

    internal long SearchRevision => _searchRevision;

    internal void InvalidateSearchCache()
    {
        unchecked { _searchRevision++; }
        InvalidateSearchBody?.Invoke(Id);
    }

    internal bool TryGetDirtyText(out string text)
    {
        if (_live is not null) { text = _live(); return true; }
        if (_text is not null) { text = _text; return true; }
        text = "";
        return false;
    }

    /// <summary>An editor now owns the text; edits arrive through <see cref="Touched"/>.</summary>
    public void Attach(Func<string> live) { _live = live; _text = null; }

    internal void SetMigratedText(string value)
    {
        InvalidateSearchCache();
        _text = value; _live = null; Dirty = true; Blank = string.IsNullOrWhiteSpace(value); ModeCache = Modes.Detect(value).Mode; TitleCache = null;
    }

    /// <summary>Called by the editor on every edit, with cheap facts about the document.</summary>
    public void Touched(bool blank, NoteMode mode)
    {
        if (BodyUnavailable) return;
        InvalidateSearchCache();
        Dirty = true; Blank = blank; ModeCache = mode; Modified = DateTime.Now; TitleCache = null;
    }

    /// <summary>The editor released this note: keep nothing in memory unless it still needs saving.</summary>
    public void Detach()
    {
        if (_live is not null && Dirty) _text = _live();
        _live = null;
        if (!Dirty) _text = null;
    }

    [JsonIgnore] public bool IsEmpty => Blank;

    [JsonIgnore] public string Title => BodyUnavailable ? "Note text unavailable" : TitleCache ??= MakeTitle(PeekText());

    internal static string MakeTitle(string text)
    {
        return MakeTitle(text, Modes.Detect(text).KeywordLength);
    }

    internal static string MakeTitle(string text, int keywordLength)
    {
        var skip = keywordLength > 0 ? 1 : 0;
        foreach (var raw in text.Split('\n').Skip(skip).Take(40))
        {
            var line = raw.Trim().TrimStart('#', '-', '*', '>', ' ').Replace("[ ]", "").Replace("[x]", "").Trim();
            if (line.Length > 0) return line.Length > 60 ? line[..60] + "…" : line;
        }
        return "Empty note";
    }
}
