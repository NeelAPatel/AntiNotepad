using System.Text.Json;

namespace AntiQuickPad.Core;

/// <summary>Loads and persists a small ordered collection of plain Markdown notes.</summary>
public sealed class NotesSession
{
    private const string StateFileName = "notes.json";
    private readonly string _directory;
    private readonly string _statePath;
    private List<Note> _notes;

    public NotesSession(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _directory = Path.GetFullPath(directory);
        _statePath = Path.Combine(_directory, StateFileName);

        if (File.Exists(_statePath))
        {
            (_notes, CurrentIndex) = Load();
        }
        else
        {
            _notes = [NewEmptyNote()];
            CurrentIndex = 0;
            Save();
        }
    }

    public event EventHandler<SaveState>? SaveStateChanged;
    public void Next() { if (CurrentIndex + 1 < _notes.Count) Select(CurrentIndex + 1); }
    public void Previous() { if (CurrentIndex > 0) Select(CurrentIndex - 1); }
    public void UpdateText(string text) => UpdateCurrent(text, Current.CaretOffset);
    public void UpdateCaret(int offset) => UpdateCurrent(Current.Text, offset);
    public void Flush() => Save();
    public IReadOnlyList<Note> Notes => _notes.AsReadOnly();
    public int CurrentIndex { get; private set; }
    public Note Current => _notes[CurrentIndex];

    public Note CreateNote()
    {
        var note = NewEmptyNote();
        _notes.Add(note);
        CurrentIndex = _notes.Count - 1;
        return note;
    }

    public void DeleteCurrent()
    {
        var removed = Current;
        _notes.RemoveAt(CurrentIndex);
        if (_notes.Count == 0)
        {
            _notes.Add(NewEmptyNote());
            CurrentIndex = 0;
        }
        else if (CurrentIndex >= _notes.Count)
        {
            CurrentIndex = _notes.Count - 1;
        }

        // Persist the new index/order before removing content, so a failed manifest
        // write never leaves a manifest pointing at a deleted note.
        Save();
        var oldPath = NotePath(removed.Id);
        if (File.Exists(oldPath)) File.Delete(oldPath);
    }

    public void Select(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _notes.Count);
        CurrentIndex = index;
    }

    public void UpdateCurrent(string text, int caretOffset)
    {
        ArgumentNullException.ThrowIfNull(text);
        SaveStateChanged?.Invoke(this, SaveState.Pending);
        _notes[CurrentIndex] = Current with
        {
            Text = text,
            CaretOffset = Math.Clamp(caretOffset, 0, text.Length)
        };
    }

    /// <summary>Atomically replaces each Markdown file and then the session manifest.</summary>
    public void Save()
    {
        SaveStateChanged?.Invoke(this, SaveState.Saving);
        try
        {
        Directory.CreateDirectory(_directory);
        foreach (var note in _notes)
            AtomicWrite(NotePath(note.Id), note.Text);

        var manifest = new StateDto
        {
            CurrentIndex = CurrentIndex,
            Notes = _notes.Select(n => new NoteDto { Id = n.Id, CaretOffset = n.CaretOffset }).ToList()
        };
        AtomicWrite(_statePath, JsonSerializer.Serialize(manifest, JsonOptions));
        SaveStateChanged?.Invoke(this, SaveState.Saved);
        }
        catch { SaveStateChanged?.Invoke(this, SaveState.Failed); throw; }
    }

    /// <summary>Exercises create, edit, save, and reload without touching user data.</summary>
    public static bool RunPersistenceSmoke(string parentDirectory)
    {
        var scratch = Path.Combine(Path.GetFullPath(parentDirectory), "notes-smoke-" + Guid.NewGuid().ToString("N"));
        try
        {
            var first = new NotesSession(scratch);
            first.UpdateCurrent("# round trip\n", 4);
            first.CreateNote();
            first.UpdateCurrent("second", 3);
            first.Select(0);
            first.Save();

            var restored = new NotesSession(scratch);
            var roundTrip = restored.Notes.Count == 2
                && restored.CurrentIndex == 0
                && restored.Current.Text == "# round trip\n"
                && restored.Current.CaretOffset == 4
                && restored.Notes[1].Text == "second"
                && restored.Notes[1].CaretOffset == 3;
            var deletedId = restored.Current.Id;
            restored.DeleteCurrent();
            var afterDelete = new NotesSession(scratch);
            var deleted = afterDelete.Notes.Count == 1 && afterDelete.Current.Text == "second"
                && !File.Exists(Path.Combine(scratch, deletedId + ".md"));
            afterDelete.DeleteCurrent();
            var afterLastDelete = new NotesSession(scratch);
            return roundTrip && deleted && afterLastDelete.Notes.Count == 1
                && afterLastDelete.Current.Text == string.Empty && afterLastDelete.CurrentIndex == 0;
        }
        finally
        {
            if (Directory.Exists(scratch)) Directory.Delete(scratch, recursive: true);
        }
    }

    private (List<Note> Notes, int CurrentIndex) Load()
    {
        try
        {
            var json = File.ReadAllText(_statePath);
            var state = JsonSerializer.Deserialize<StateDto>(json, JsonOptions)
                ?? throw new InvalidDataException("Notes manifest is empty.");
            if (state.Notes is null || state.Notes.Count == 0)
                throw new InvalidDataException("Notes manifest contains no notes.");
            if (state.CurrentIndex < 0 || state.CurrentIndex >= state.Notes.Count)
                throw new InvalidDataException("Notes manifest current index is out of range.");

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var notes = new List<Note>(state.Notes.Count);
            foreach (var entry in state.Notes)
            {
                if (!Guid.TryParseExact(entry.Id, "N", out _) || !ids.Add(entry.Id))
                    throw new InvalidDataException("Notes manifest contains an invalid or duplicate note id.");
                var text = File.ReadAllText(NotePath(entry.Id));
                notes.Add(new Note(entry.Id, text, Math.Clamp(entry.CaretOffset, 0, text.Length)));
            }

            return (notes, state.CurrentIndex);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Notes manifest is corrupt; it was left untouched.", ex);
        }
    }

    private static Note NewEmptyNote() => new(Guid.NewGuid().ToString("N"), string.Empty, 0);
    private string NotePath(string id) => Path.Combine(_directory, id + ".md");

    private static void AtomicWrite(string destination, string contents)
    {
        var temp = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temp, contents);
            File.Move(temp, destination, overwrite: true);
        }
        finally
        {
            if (File.Exists(temp)) File.Delete(temp);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private sealed class StateDto
    {
        public int CurrentIndex { get; set; }
        public List<NoteDto>? Notes { get; set; }
    }

    private sealed class NoteDto
    {
        public string Id { get; set; } = string.Empty;
        public int CaretOffset { get; set; }
    }
}
