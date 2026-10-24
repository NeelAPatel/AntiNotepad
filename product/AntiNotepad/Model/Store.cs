using System.Text.Json;
using System.Text.Json.Serialization;

namespace AntiNotepad.Model;

internal enum BodyReadStatus { Available, Missing, Unreadable }

internal readonly record struct BodyReadResult(string? Text, BodyReadStatus Status);

internal sealed record SearchNoteSnapshot(string Id, string Title, bool TitleKnown, DateTime Modified, long Revision, string? InMemoryText, bool BodyUnavailable);

internal sealed record SearchMatchResult(string Id, string Title, string Snippet, NoteMode Mode, string? ModeArgument, BodyReadStatus BodyStatus);

internal sealed class SearchContext
{
    internal SearchNoteSnapshot[] Notes { get; }
    internal string BodyFolder { get; }
    internal SearchBodyCache Cache { get; }
    internal long CacheEpoch { get; }

    internal SearchContext(SearchNoteSnapshot[] notes, string bodyFolder, SearchBodyCache cache, long cacheEpoch)
    {
        Notes = notes;
        BodyFolder = bodyFolder;
        Cache = cache;
        CacheEpoch = cacheEpoch;
    }
}

internal sealed class SearchBodyCache
{
    internal readonly object Gate = new();
    internal readonly Dictionary<string, SearchBodyCacheEntry> Entries = new(StringComparer.Ordinal);
    internal int ReadCount;
    internal long Epoch;
}

internal readonly record struct SearchBodyCacheEntry(DateTime Modified, long Revision, string Text);

/// <summary>Current-query guard shared by the overlay and headless stale-result checks.</summary>
internal sealed class SearchGeneration : IDisposable
{
    private readonly object _gate = new();
    private long _generation;
    private CancellationTokenSource? _current;
    private bool _disposed;

    internal SearchRequest Begin()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _current?.Cancel();
            var source = new CancellationTokenSource();
            _current = source;
            return new SearchRequest(this, ++_generation, source);
        }
    }

    internal void Cancel()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _generation++;
            _current?.Cancel();
            _current = null;
        }
    }

    internal bool IsCurrent(SearchRequest request)
    {
        lock (_gate) return !_disposed && _generation == request.Generation && ReferenceEquals(_current, request.Source) && !request.Token.IsCancellationRequested;
    }

    internal void Release(SearchRequest request)
    {
        lock (_gate)
        {
            if (ReferenceEquals(_current, request.Source))
            {
                _current = null;
                _generation++;
            }
            request.Source.Dispose();
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            _generation++;
            _current?.Cancel();
            _current = null;
        }
    }
}

internal sealed class SearchRequest : IDisposable
{
    private bool _disposed;
    internal SearchGeneration Owner { get; }
    internal long Generation { get; }
    internal CancellationTokenSource Source { get; }
    internal CancellationToken Token => Source.Token;

    internal SearchRequest(SearchGeneration owner, long generation, CancellationTokenSource source)
    {
        Owner = owner;
        Generation = generation;
        Source = source;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Owner.Release(this);
    }
}

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
    public double SwipeLength { get; set; } = 220;        // progress: px of input for a whole note
    public double SwipeFiresAt { get; set; } = 0.35;      // progress: share of a note where it changes
    public string Swipe { get; set; } = "progress";       // between notes: detent (decide when input stops) | progress (width-independent, fires mid-swipe)
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
/// A save writes the index and only the bodies that changed. The legacy v1 antipad.json file is intentionally recognized
/// and migrated on first load so existing notes remain readable.
/// </summary>
public sealed class Store
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, Converters = { new JsonStringEnumConverter() }
    };
    private string _folder, _index, _bodies, _legacy;
    private HashSet<string> _knownIds = [];
    private bool _recoveryMode = true;
    private bool _preserveBackupOnce = true;
    private bool _forcedUnavailable;
    private bool _indexReadUnavailable;
    private readonly SearchBodyCache _searchBodyCache = new();
    public StoreData Data { get; private set; } = new();
    public string? LastError { get; private set; }
    public bool IsAvailable { get; private set; } = true;
    public string Folder => _folder;

    public Store(string directory, bool createDirectories = true)
    {
        _folder = Path.GetFullPath(directory);
        if (createDirectories) Directory.CreateDirectory(_folder);
        _index = Path.Combine(_folder, "index.json");
        // Intentional legacy compatibility: v1 used this on-disk name; keep it so existing notes can migrate.
        _legacy = Path.Combine(_folder, "antipad.json");
        _bodies = Path.Combine(_folder, "notes");
        if (createDirectories) Directory.CreateDirectory(_bodies);
        IsAvailable = createDirectories || Directory.Exists(_folder);
        Note.LoadBody = ReadBody;
        Note.InvalidateSearchBody = InvalidateSearchBody;
    }

    /// <summary>Captures only note metadata and text already in memory; this method never reads a body file.</summary>
    internal SearchNoteSnapshot[] CreateSearchSnapshot(IEnumerable<Note> notes)
    {
        var snapshot = new List<SearchNoteSnapshot>();
        foreach (var note in notes)
        {
            string? text = null;
            var hasText = !note.BodyUnavailable && note.TryGetSearchText(out text);
            var knownTitle = note.TitleCache is not null || hasText;
            var title = note.TitleCache ?? (hasText ? Note.MakeTitle(text!) : "");
            snapshot.Add(new SearchNoteSnapshot(note.Id, title, knownTitle, note.Modified, note.SearchRevision,
                hasText ? text : null, note.BodyUnavailable));
        }
        return snapshot.ToArray();
    }

    /// <summary>Captures the immutable body-folder path and search cache on the caller thread before background work begins.</summary>
    internal SearchContext CreateSearchContext(IReadOnlyList<SearchNoteSnapshot> notes)
    {
        lock (_searchBodyCache.Gate) return new(notes.ToArray(), _bodies, _searchBodyCache, _searchBodyCache.Epoch);
    }

    /// <summary>Searches from one body read per note at most; snippets and matching share that same text.</summary>
    internal static SearchMatchResult[] MatchSearch(SearchContext context, string query, CancellationToken cancellationToken)
    {
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matches = new List<SearchMatchResult>();
        foreach (var note in context.Notes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? text = note.InMemoryText;
            var status = text is null ? BodyReadStatus.Unreadable : BodyReadStatus.Available;
            if (text is not null) CacheSearchBody(context.Cache, context.CacheEpoch, note, text);
            else
            {
                var body = ReadSearchBody(context.BodyFolder, context.Cache, context.CacheEpoch, note, cancellationToken);
                status = body.Status;
                text = body.Text;
            }

            var (mode, keywordLength, modeArgument) = status == BodyReadStatus.Available ? Modes.Detect(text ?? "") : (NoteMode.Plain, 0, (string?)null);
            var title = note.TitleKnown ? note.Title : status == BodyReadStatus.Available ? Note.MakeTitle(text ?? "", keywordLength) : "Note text unavailable";
            var matchesQuery = terms.Length == 0 || terms.All(term =>
                title.Contains(term, StringComparison.CurrentCultureIgnoreCase) ||
                status == BodyReadStatus.Available && (text ?? "").Contains(term, StringComparison.CurrentCultureIgnoreCase));
            if (!matchesQuery) continue;

            var snippet = status == BodyReadStatus.Available ? MakeSearchSnippet(text ?? "", terms, keywordLength) : "Note text unavailable";
            matches.Add(new SearchMatchResult(note.Id, title, snippet, mode, modeArgument, status));
        }
        return matches.ToArray();
    }

    internal int SearchBodyReadCount => Volatile.Read(ref _searchBodyCache.ReadCount);

    internal void ResetSearchBodyReadCount() => Interlocked.Exchange(ref _searchBodyCache.ReadCount, 0);

    private void InvalidateSearchBody(string id)
    {
        lock (_searchBodyCache.Gate) _searchBodyCache.Entries.Remove(id);
    }

    private static void CacheSearchBody(SearchBodyCache cache, long epoch, SearchNoteSnapshot note, string text)
    {
        lock (cache.Gate)
        {
            if (cache.Epoch != epoch) return;
            if (cache.Entries.TryGetValue(note.Id, out var existing) &&
                (existing.Revision > note.Revision || existing.Revision == note.Revision && existing.Modified > note.Modified)) return;
            cache.Entries[note.Id] = new SearchBodyCacheEntry(note.Modified, note.Revision, text);
        }
    }

    private static BodyReadResult ReadSearchBody(string folder, SearchBodyCache cache, long epoch, SearchNoteSnapshot note, CancellationToken cancellationToken)
    {
        lock (cache.Gate)
            if (cache.Epoch == epoch && cache.Entries.TryGetValue(note.Id, out var cached) && cached.Modified == note.Modified && cached.Revision == note.Revision)
                return new(cached.Text, BodyReadStatus.Available);

        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref cache.ReadCount);
        BodyReadResult result;
        try { result = new(File.ReadAllText(SearchBodyPath(folder, note.Id)), BodyReadStatus.Available); }
        catch (FileNotFoundException) { result = new(null, BodyReadStatus.Missing); }
        catch (DirectoryNotFoundException) { result = new(null, BodyReadStatus.Missing); }
        catch { result = new(null, BodyReadStatus.Unreadable); }

        cancellationToken.ThrowIfCancellationRequested();
        if (result.Status == BodyReadStatus.Available) CacheSearchBody(cache, epoch, note, result.Text ?? "");
        return result;
    }

    private static string SearchBodyPath(string folder, string id)
    {
        if (string.IsNullOrWhiteSpace(id) || !string.Equals(Path.GetFileName(id), id, StringComparison.Ordinal) || id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new InvalidDataException("The note has an invalid id.");
        return Path.Combine(folder, id + ".md");
    }

    private static string MakeSearchSnippet(string text, string[] terms, int keywordLength)
    {
        var lines = text.Replace("\r", "").Split('\n').Take(400)
            .Skip(keywordLength > 0 ? 2 : 1).Where(line => line.Trim().Length > 0).ToList();
        if (terms.Length > 0)
        {
            var hit = lines.FindIndex(line => line.Contains(terms[0], StringComparison.CurrentCultureIgnoreCase));
            if (hit > 0) lines = lines.Skip(hit).ToList();
        }
        return string.Join("  ·  ", lines.Take(6).Select(line => line.Trim().Replace("[ ]", "☐").Replace("[x]", "☑")));
    }

    private void ClearSearchBodyCache()
    {
        lock (_searchBodyCache.Gate)
        {
            _searchBodyCache.Epoch++;
            _searchBodyCache.Entries.Clear();
        }
    }

    private string BodyPath(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || !string.Equals(Path.GetFileName(id), id, StringComparison.Ordinal) || id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new InvalidDataException("The note has an invalid id.");
        return Path.Combine(_bodies, id + ".md");
    }

    private BodyReadResult ReadBody(string id)
    {
        try { return new(File.ReadAllText(BodyPath(id)), BodyReadStatus.Available); }
        catch (FileNotFoundException) { return new(null, BodyReadStatus.Missing); }
        catch (DirectoryNotFoundException) { return new(null, BodyReadStatus.Missing); }
        catch (Exception ex) { LastError = $"read {id}: {ex.Message}"; return new(null, BodyReadStatus.Unreadable); }
    }

    public void Load()
    {
        if (_forcedUnavailable) return;
        ClearSearchBodyCache();
        if (!Directory.Exists(_folder))
        {
            IsAvailable = false;
            LastError = $"Notes folder is unavailable: {_folder}";
            return;
        }
        IsAvailable = true;
        _recoveryMode = true;
        var loaded = false;
        _indexReadUnavailable = false;
        foreach (var candidate in new[] { _index, _index + ".bak" })
        {
            try
            {
                if (!File.Exists(candidate)) continue;
                if (JsonSerializer.Deserialize<StoreData>(File.ReadAllText(candidate), Json) is not { } data || !IsComplete(data)) continue;
                Data = data;
                _recoveryMode = !string.Equals(candidate, _index, StringComparison.OrdinalIgnoreCase);
                loaded = true;
                break;
            }
            catch (Exception ex)
            {
                LastError = $"load {Path.GetFileName(candidate)}: {ex.Message}";
                if (IsIoFailure(ex)) _indexReadUnavailable = true;
            }
        }
        var migratedLegacy = !loaded && File.Exists(_legacy) && MigrateV1();
        loaded |= migratedLegacy;
        if (!loaded && _indexReadUnavailable)
        {
            IsAvailable = false;
            return;
        }
        if (!loaded) { Data = new StoreData(); _recoveryMode = true; }
        _preserveBackupOnce = _recoveryMode;
        var expired = Normalize();
        _knownIds = Data.All.Select(note => note.Id).ToHashSet(StringComparer.Ordinal);
        if (!_recoveryMode) foreach (var id in expired) DeleteOwnBody(id);
        RemoveOrphans();
        _recoveryMode = false;
        if (migratedLegacy) _ = Save();
    }

    private static bool IsComplete(StoreData data) => data.Settings is not null && data.Stack is not null && data.Slots is not null && data.Trash is not null
        && data.Stack.All(note => note is not null) && data.Slots.All(note => note is not null) && data.Trash.All(note => note is not null);

    /// <summary>Intentional legacy migration: v1 kept bodies inside antipad.json. Read it once, write v2, keep the old file as a backup.</summary>
    private bool MigrateV1()
    {
        try
        {
            var json = File.ReadAllText(_legacy);
            if (JsonSerializer.Deserialize<StoreData>(json, Json) is not { } data || !IsComplete(data)) return false;
            var root = System.Text.Json.Nodes.JsonNode.Parse(json)!;
            var texts = new Dictionary<string, string>();
            foreach (var list in new[] { "Stack", "Slots", "Trash" })
                foreach (var n in root[list]?.AsArray() ?? [])
                    if (n?["Id"]?.GetValue<string>() is { } id) texts[id] = n["Text"]?.GetValue<string>() ?? "";
            foreach (var note in data.All) note.SetMigratedText(texts.GetValueOrDefault(note.Id, ""));
            Data = data;
            _recoveryMode = true;
            Normalize();
            return true;
        }
        catch (Exception ex)
        {
            LastError = "migrate: " + ex.Message;
            if (IsIoFailure(ex)) _indexReadUnavailable = true;
            return false;
        }
    }

    private List<string> Normalize()
    {
        var d = Data;
        d.Version = 2;
        // Older builds stored a family name the editor can't load (it fell back to Cascadia Mono without a word).
        if (d.Settings.Font == "Segoe UI Variable Text") d.Settings.Font = "Segoe UI";
        if (d.Slots is not { Length: 9 }) d.Slots = Enumerable.Range(1, 9).Select(i => d.Slots?.ElementAtOrDefault(i - 1) ?? new Note { Slot = i }).ToArray();
        for (var i = 0; i < 9; i++) { d.Slots[i] ??= new Note(); d.Slots[i].Slot = i + 1; }
        d.Stack.RemoveAll(n => n is null);
        if (d.Stack.Count == 0) d.Stack.Add(new Note());
        d.Current = Math.Clamp(d.Current < 0 ? d.Stack.Count - 1 : d.Current, 0, d.Stack.Count - 1);
        d.ViewSlot = Math.Clamp(d.ViewSlot, 0, 9);
        d.Settings.SplitSlot = Math.Clamp(d.Settings.SplitSlot, 1, 9);
        // Trash keeps 30 days.
        var expired = d.Trash.Where(n => n.Deleted is { } when && (DateTime.Now - when).TotalDays > 30).Select(n => n.Id).ToList();
        d.Trash.RemoveAll(n => n.Deleted is { } when && (DateTime.Now - when).TotalDays > 30);
        return expired;
    }

    /// <summary>Moves unknown Markdown bodies aside. Note lifecycle deletions are handled from the saved model, not by this sweep.</summary>
    public void RemoveOrphans()
    {
        try
        {
            if (!Directory.Exists(_bodies)) return;
            var live = Data.All.Select(n => n.Id).ToHashSet();
            var recovered = Path.Combine(_folder, "recovered");
            foreach (var file in Directory.EnumerateFiles(_bodies, "*.md", SearchOption.AllDirectories).ToList())
            {
                if (live.Contains(Path.GetFileNameWithoutExtension(file))) continue;
                Directory.CreateDirectory(recovered);
                var name = Path.GetFileName(file);
                var destination = Path.Combine(recovered, name);
                for (var suffix = 1; File.Exists(destination) || Directory.Exists(destination); suffix++)
                    destination = Path.Combine(recovered, $"{Path.GetFileNameWithoutExtension(name)} (recovered {suffix}).md");
                File.Move(file, destination);
            }
        }
        catch (Exception ex) { LastError = "tidy: " + ex.Message; }
    }

    public bool Save()
    {
        if (!IsAvailable || !Directory.Exists(_folder)) { LastError = $"Notes folder is unavailable: {_folder}"; return true; }
        try
        {
            if (Data.All.Any(note => note.Dirty && !note.BodyUnavailable)) Directory.CreateDirectory(_bodies);
            foreach (var note in Data.All.Where(n => n.Dirty).ToList())
            {
                if (note.BodyUnavailable) continue;
                note.InvalidateSearchCache();
                var text = note.Text;
                note.TitleCache = Note.MakeTitle(text);
                note.ModeCache = Modes.Detect(text).Mode;
                Atomic(BodyPath(note.Id), text, backup: false);
                note.Dirty = false;
            }
            Atomic(_index, JsonSerializer.Serialize(Data, Json), backup: !_preserveBackupOnce);
            _preserveBackupOnce = false;
            var current = Data.All.Select(note => note.Id).ToHashSet(StringComparer.Ordinal);
            var failedDeletes = new List<string>();
            if (!_recoveryMode)
                foreach (var removed in _knownIds.Except(current, StringComparer.Ordinal))
                    if (!DeleteOwnBody(removed)) failedDeletes.Add(removed);
            _knownIds = current;
            _knownIds.UnionWith(failedDeletes);
            if (failedDeletes.Count > 0) return false;
            LastError = null; return true;
        }
        catch (Exception ex) { LastError = ex.Message; return false; }
    }

    private bool DeleteOwnBody(string id)
    {
        try { File.Delete(BodyPath(id)); return true; }
        catch (Exception ex) { LastError = $"delete {id}: {ex.Message}"; return false; }
    }

    private static bool IsIoFailure(Exception ex) => ex is IOException or UnauthorizedAccessException or System.Security.SecurityException;

    public void SwitchFolder(string folder)
    {
        _folder = Path.GetFullPath(folder);
        _index = Path.Combine(_folder, "index.json");
        _legacy = Path.Combine(_folder, "antipad.json");
        _bodies = Path.Combine(_folder, "notes");
        _forcedUnavailable = false;
        IsAvailable = Directory.Exists(_folder);
        Note.LoadBody = ReadBody;
        Note.InvalidateSearchBody = InvalidateSearchBody;
        ClearSearchBodyCache();
    }

    public void MarkUnavailable(string reason)
    {
        _forcedUnavailable = true;
        IsAvailable = false;
        LastError = reason;
    }

    private static void Atomic(string path, string content, bool backup)
    {
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, content);
        if (File.Exists(path)) File.Replace(tmp, path, backup ? path + ".bak" : null, ignoreMetadataErrors: true);
        else File.Move(tmp, path);
    }
}
