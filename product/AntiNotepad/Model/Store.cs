using System.Text.Json;
using System.Text.Json.Serialization;

namespace AntiNotepad.Model;

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
public sealed partial class Store
{
    private static readonly TimeSpan SaveCoalesceWindow = TimeSpan.FromMilliseconds(50);
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
    private readonly object _saveGate = new();
    private readonly Dictionary<string, PendingBody> _pendingBodies = new(StringComparer.Ordinal);
    private readonly Queue<SaveCompletion> _completedSaves = new();
    private PendingSaveBatch? _pendingSave;
    private Task? _writerTask;
    private bool _lastSaveSucceeded = true;
    public StoreData Data { get; private set; } = new();
    internal SettingsService SettingsService { get; }
    public string? LastError { get; private set; }
    public bool IsAvailable { get; private set; } = true;
    public string Folder => _folder;

    public Store(string directory, bool createDirectories = true)
    {
        SettingsService = new SettingsService(Data.Settings, settings => Data.Settings = settings);
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
        DrainSaveQueue();
        if (_forcedUnavailable)
        {
            EnsureUsableSelection();
            return;
        }
        ClearSearchBodyCache();
        if (!Directory.Exists(_folder))
        {
            IsAvailable = false;
            LastError = $"Notes folder is unavailable: {_folder}";
            EnsureUsableSelection();
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
                SettingsService.Replace(data.Settings);
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
            EnsureUsableSelection();
            return;
        }
        if (!loaded)
        {
            Data = new StoreData();
            SettingsService.Replace(Data.Settings);
            _recoveryMode = true;
        }
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
            SettingsService.Replace(data.Settings);
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
        // This owner-internal load path normalizes the live object before UI subscribers are attached.
        SettingsService.Update(settings =>
        {
            if (settings.Font == "Segoe UI Variable Text") settings.Font = "Segoe UI";
            settings.SplitSlot = Math.Clamp(settings.SplitSlot, 1, 9);
        });
        if (d.Slots is not { Length: 9 }) d.Slots = Enumerable.Range(1, 9).Select(i => d.Slots?.ElementAtOrDefault(i - 1) ?? new Note { Slot = i }).ToArray();
        for (var i = 0; i < 9; i++) { d.Slots[i] ??= new Note(); d.Slots[i].Slot = i + 1; }
        d.Stack.RemoveAll(n => n is null);
        if (d.Stack.Count == 0) d.Stack.Add(new Note());
        d.Current = Math.Clamp(d.Current < 0 ? d.Stack.Count - 1 : d.Current, 0, d.Stack.Count - 1);
        d.ViewSlot = Math.Clamp(d.ViewSlot, 0, 9);
        // Trash keeps 30 days.
        var expired = d.Trash.Where(n => n.Deleted is { } when && (DateTime.Now - when).TotalDays > 30).Select(n => n.Id).ToList();
        d.Trash.RemoveAll(n => n.Deleted is { } when && (DateTime.Now - when).TotalDays > 30);
        return expired;
    }

    private void EnsureUsableSelection()
    {
        Data.Stack ??= [];
        Data.Stack.RemoveAll(note => note is null);
        if (Data.Stack.Count == 0) Data.Stack.Add(new Note());
        Data.Current = Math.Clamp(Data.Current < 0 ? Data.Stack.Count - 1 : Data.Current, 0, Data.Stack.Count - 1);
    }

    /// <summary>Moves unknown Markdown bodies aside. Note lifecycle deletions are handled from the saved model, not by this sweep.</summary>
    public void RemoveOrphans()
    {
        DrainSaveQueue();
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

    public Task<bool> QueueSave()
    {
        if (IsStoreUnavailable()) return Task.FromResult(true);
        try { return EnqueueSave(CaptureSaveSnapshot(), forceNow: false).PublicTask; }
        catch (Exception ex)
        {
            LastError = ex.Message;
            lock (_saveGate) _lastSaveSucceeded = false;
            return Task.FromResult(false);
        }
    }

    /// <summary>Captures the current model and synchronously drains its write for quit, retry, and session end.</summary>
    public bool Save()
    {
        if (IsStoreUnavailable())
        {
            Flush();
            return true;
        }
        try
        {
            var batch = EnqueueSave(CaptureSaveSnapshot(), forceNow: true);
            var completion = batch.Completion.Task.GetAwaiter().GetResult();
            ApplyCompletedSaves();
            return completion.Result.Success;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            lock (_saveGate) _lastSaveSucceeded = false;
            return false;
        }
    }

    /// <summary>Drains queued saves immediately without taking a new model snapshot.</summary>
    public bool Flush()
    {
        var unavailable = IsStoreUnavailable();
        DrainSaveQueue();
        if (unavailable)
        {
            // A completed writer can clear LastError; refresh it after draining if the folder is still unavailable.
            IsStoreUnavailable();
            return true;
        }
        if (IsStoreUnavailable()) return true;
        lock (_saveGate) return _lastSaveSucceeded;
    }

    /// <summary>Waits for outstanding writer work without consulting the last completed save result.</summary>
    private void DrainSaveQueue()
    {
        while (true)
        {
            Task? writer;
            lock (_saveGate)
            {
                _pendingSave?.FlushRequested.TrySetResult(true);
                writer = _writerTask;
            }
            if (writer is null) break;
            try { writer.GetAwaiter().GetResult(); }
            catch (Exception ex)
            {
                LastError = ex.Message;
                lock (_saveGate) _lastSaveSucceeded = false;
                break;
            }
        }
        ApplyCompletedSaves();
    }

    private bool IsStoreUnavailable()
    {
        if (!Directory.Exists(_folder))
        {
            // Checked on every save and not remembered: a folder that comes back (OneDrive, a drive) saves again.
            LastError = $"Notes folder is unavailable: {_folder}";
            return true;
        }
        if (IsAvailable) return false;
        LastError ??= $"Notes folder is unavailable: {_folder}";
        return true;
    }

    private PendingSaveBatch EnqueueSave(SaveSnapshot snapshot, bool forceNow)
    {
        lock (_saveGate)
        {
            var batch = _pendingSave;
            if (batch is null)
            {
                batch = new PendingSaveBatch(snapshot);
                _pendingSave = batch;
                batch.PublicTask = CompleteQueuedSaveAsync(batch.Completion.Task);
            }
            else batch.Replace(snapshot);

            if (forceNow) batch.FlushRequested.TrySetResult(true);
            if (_writerTask is null) _writerTask = Task.Run(ProcessSaveQueueAsync);
            return batch;
        }
    }

    private async Task<bool> CompleteQueuedSaveAsync(Task<SaveCompletion> completionTask)
    {
        var completion = await completionTask;
        ApplyCompletedSaves();
        return completion.Result.Success;
    }

    private async Task ProcessSaveQueueAsync()
    {
        while (true)
        {
            PendingSaveBatch batch;
            lock (_saveGate)
            {
                if (_pendingSave is null) { _writerTask = null; return; }
                batch = _pendingSave;
            }

            await Task.WhenAny(Task.Delay(SaveCoalesceWindow), batch.FlushRequested.Task);

            SaveSnapshot snapshot;
            IReadOnlyDictionary<string, SaveAcknowledgement> acknowledgements;
            lock (_saveGate)
            {
                if (!ReferenceEquals(_pendingSave, batch)) continue;
                snapshot = batch.Snapshot;
                acknowledgements = batch.Acknowledgements;
                _pendingSave = null;
            }

            var result = WriteSnapshot(snapshot);
            var completion = new SaveCompletion(result, acknowledgements, snapshot.Data);
            lock (_saveGate)
            {
                foreach (var body in result.DurableBodies)
                    if (_pendingBodies.TryGetValue(body.Id, out var pending) && pending.Revision == body.Revision)
                        _pendingBodies.Remove(body.Id);
                _completedSaves.Enqueue(completion);
                _lastSaveSucceeded = result.Success;
            }
            batch.Completion.TrySetResult(completion);
        }
    }

    private SaveSnapshot CaptureSaveSnapshot()
    {
        var notes = Data.All.ToArray();
        lock (_saveGate)
        {
            var currentIds = notes.Select(note => note.Id).ToHashSet(StringComparer.Ordinal);
            foreach (var removed in _pendingBodies.Keys.Where(id => !currentIds.Contains(id)).ToArray())
                _pendingBodies.Remove(removed);

            foreach (var note in notes)
            {
                if (!note.Dirty || note.BodyUnavailable || !note.TryGetDirtyText(out var text)) continue;
                _pendingBodies[note.Id] = new PendingBody(text, note.SearchRevision);
            }

            var bodies = new List<SaveBodySnapshot>();
            var acknowledgements = new Dictionary<string, SaveAcknowledgement>(StringComparer.Ordinal);
            foreach (var note in notes)
            {
                if (note.BodyUnavailable) continue;
                if (!_pendingBodies.TryGetValue(note.Id, out var body)) continue;
                bodies.Add(new SaveBodySnapshot(note.Id, body.Text, body.Revision));
                acknowledgements[note.Id] = new SaveAcknowledgement(note, body.Revision);
            }

            return new SaveSnapshot(_folder, _index, _bodies, IsAvailable, CloneData(Data), bodies.ToArray(), acknowledgements);
        }
    }

    private static StoreData CloneData(StoreData data) => new()
    {
        Version = data.Version,
        Stack = data.Stack.Select(CloneMetadata).ToList(),
        Slots = data.Slots.Select(CloneMetadata).ToArray(),
        Trash = data.Trash.Select(CloneMetadata).ToList(),
        Current = data.Current,
        ViewSlot = data.ViewSlot,
        Settings = data.Settings.CopyForSave()
    };

    private static Note CloneMetadata(Note note) => new()
    {
        Id = note.Id,
        Created = note.Created,
        Modified = note.Modified,
        Caret = note.Caret,
        Scroll = note.Scroll,
        Deleted = note.Deleted,
        Slot = note.Slot,
        TitleCache = note.TitleCache,
        Blank = note.Blank,
        ModeCache = note.ModeCache
    };

    private SaveResult WriteSnapshot(SaveSnapshot snapshot)
    {
        if (!snapshot.IsAvailable || !IsAvailable || !Directory.Exists(snapshot.Folder))
        {
            LastError = $"Notes folder is unavailable: {snapshot.Folder}";
            return new SaveResult(true, []);
        }

        try
        {
            if (snapshot.Bodies.Length > 0) Directory.CreateDirectory(snapshot.BodiesFolder);
            foreach (var body in snapshot.Bodies)
            {
                var note = snapshot.Data.All.FirstOrDefault(candidate => candidate.Id == body.Id);
                if (note is not null)
                {
                    note.TitleCache = Note.MakeTitle(body.Text);
                    note.ModeCache = Modes.Detect(body.Text).Mode;
                }
                Atomic(SearchBodyPath(snapshot.BodiesFolder, body.Id), body.Text, backup: false);
            }

            Atomic(snapshot.Index, JsonSerializer.Serialize(snapshot.Data, Json), backup: !_preserveBackupOnce);
            _preserveBackupOnce = false;

            var current = snapshot.Data.All.Select(note => note.Id).ToHashSet(StringComparer.Ordinal);
            var failedDeletes = new List<string>();
            if (!_recoveryMode)
                foreach (var removed in _knownIds.Except(current, StringComparer.Ordinal))
                    if (!DeleteOwnBody(snapshot.BodiesFolder, removed)) failedDeletes.Add(removed);
            _knownIds = current;
            _knownIds.UnionWith(failedDeletes);

            if (failedDeletes.Count > 0)
                return new SaveResult(false, snapshot.Bodies);
            LastError = null;
            return new SaveResult(true, snapshot.Bodies);
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return new SaveResult(false, []);
        }
    }

    private void ApplyCompletedSaves()
    {
        SaveCompletion[] completions;
        lock (_saveGate)
        {
            completions = _completedSaves.ToArray();
            _completedSaves.Clear();
        }
        foreach (var completion in completions)
            foreach (var body in completion.Result.DurableBodies)
                if (completion.Acknowledgements.TryGetValue(body.Id, out var acknowledgement) &&
                    acknowledgement.Note.Dirty && acknowledgement.Note.SearchRevision == acknowledgement.Revision)
                {
                    var metadata = completion.Data.All.First(note => note.Id == body.Id);
                    acknowledgement.Note.TitleCache = metadata.TitleCache;
                    acknowledgement.Note.ModeCache = metadata.ModeCache;
                    acknowledgement.Note.Dirty = false;
                }
    }

    private bool DeleteOwnBody(string id)
    {
        try { File.Delete(BodyPath(id)); return true; }
        catch (Exception ex) { LastError = $"delete {id}: {ex.Message}"; return false; }
    }

    private bool DeleteOwnBody(string bodiesFolder, string id)
    {
        try { File.Delete(SearchBodyPath(bodiesFolder, id)); return true; }
        catch (Exception ex) { LastError = $"delete {id}: {ex.Message}"; return false; }
    }

    private sealed record PendingBody(string Text, long Revision);
    private sealed record SaveBodySnapshot(string Id, string Text, long Revision);
    private sealed record SaveAcknowledgement(Note Note, long Revision);
    private sealed record SaveSnapshot(string Folder, string Index, string BodiesFolder, bool IsAvailable, StoreData Data,
        SaveBodySnapshot[] Bodies, IReadOnlyDictionary<string, SaveAcknowledgement> Acknowledgements);
    private sealed record SaveResult(bool Success, SaveBodySnapshot[] DurableBodies);
    private sealed record SaveCompletion(SaveResult Result, IReadOnlyDictionary<string, SaveAcknowledgement> Acknowledgements, StoreData Data);

    private sealed class PendingSaveBatch(SaveSnapshot snapshot)
    {
        internal SaveSnapshot Snapshot { get; private set; } = snapshot;
        internal IReadOnlyDictionary<string, SaveAcknowledgement> Acknowledgements => Snapshot.Acknowledgements;
        internal TaskCompletionSource<bool> FlushRequested { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource<SaveCompletion> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal Task<bool> PublicTask { get; set; } = Task.FromResult(false);
        internal void Replace(SaveSnapshot snapshot) => Snapshot = snapshot;
    }

    private static bool IsIoFailure(Exception ex) => ex is IOException or UnauthorizedAccessException or System.Security.SecurityException;

    public void SwitchFolder(string folder)
    {
        DrainSaveQueue();
        _folder = Path.GetFullPath(folder);
        _index = Path.Combine(_folder, "index.json");
        _legacy = Path.Combine(_folder, "antipad.json");
        _bodies = Path.Combine(_folder, "notes");
        _forcedUnavailable = false;
        IsAvailable = Directory.Exists(_folder);
        lock (_saveGate) _lastSaveSucceeded = true;
        LastError = IsAvailable ? null : $"Notes folder is unavailable: {_folder}";
        Note.LoadBody = ReadBody;
        Note.InvalidateSearchBody = InvalidateSearchBody;
        ClearSearchBodyCache();
    }

    public void MarkUnavailable(string reason)
    {
        _forcedUnavailable = true;
        IsAvailable = false;
        LastError = reason;
        EnsureUsableSelection();
    }

    private static void Atomic(string path, string content, bool backup)
    {
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, content);
        if (File.Exists(path)) File.Replace(tmp, path, backup ? path + ".bak" : null, ignoreMetadataErrors: true);
        else File.Move(tmp, path);
    }
}
