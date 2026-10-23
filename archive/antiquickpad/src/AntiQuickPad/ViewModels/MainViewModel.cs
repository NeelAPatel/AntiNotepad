using System.ComponentModel;
using System.Windows.Input;
using AntiQuickPad.Core;
namespace AntiQuickPad.ViewModels;
public sealed class NoteNavigationEventArgs(string kind) : CancelEventArgs
{ public string Kind { get; } = kind; }
public sealed class MainViewModel : ObservableObject
{
    public const int CurrentLayoutVersion = 1; // M8-antinote-ui-overhaul layout.
    private readonly NotesSession _session;
    private readonly SettingsStore _settings;
    private List<NoteViewModel> _notes;
    private bool _pinned;
    public string ShellId { get; private set; } = "classic";
    public string LookId { get; private set; } = "Ink";
    // M11a-settings-function: preferences live here and are merged into every settings save, so window saves never wipe them.
    public Preferences Preferences { get; private set; } = new();
    public void UpdatePreferences(Func<Preferences, Preferences> change)
    {
        var next = change(Preferences).Normalized();
        if (next == Preferences) return;
        Preferences = next; Changed(nameof(Preferences));
    }
    public MainViewModel(NotesSession session, SettingsStore settings)
    {
        _session = session; _settings = settings;
        _notes = ProjectNotes();
        _session.SaveStateChanged += (_, state) =>
        {
            if (state == SaveState.Saved) Status.Set(false, false);
            else Status.Set(true, state == SaveState.Failed || Status.Failed, Status.Error);
        };
        Status.PropertyChanged += (_, _) => { Changed(nameof(SaveLineText)); Changed(nameof(SaveLineFailed)); };
        SelectCommand = new IndexCommand(Select);
        CreateCommand = new RelayCommand(CreateNote); DeleteCommand = new RelayCommand(DeleteCurrent);
        NextCommand = new RelayCommand(Next); PreviousCommand = new RelayCommand(Previous);
        PinCommand = new RelayCommand(() => Pinned = !Pinned);
    }
    public IReadOnlyList<NoteViewModel> Notes => _notes;
    public NoteViewModel Current => _notes[CurrentIndex];
    public int CurrentIndex => _session.CurrentIndex;
    public string CountText => $"{CurrentIndex + 1} / {Notes.Count}";
    // Display projections: raised alongside the source changes they derive from.
    public static readonly IReadOnlySet<string?> ProjectionNames = new HashSet<string?> { nameof(PositionText), nameof(IsCurrentEmpty),
        nameof(SaveLineText), nameof(SaveLineFailed), nameof(HasPrevious), nameof(HasNext), nameof(PagerFirstIndex), nameof(PagerDots) };
    public string PositionText => $"{CurrentIndex + 1} of {Notes.Count}";
    public bool IsCurrentEmpty => Current.Text.Length == 0;
    public string SaveLineText => Status.Failed || Status.Saving ? Status.Text : IsCurrentEmpty ? "Ready to write" : "All changes saved";
    public bool SaveLineFailed => Status.Failed;
    public bool HasPrevious => CurrentIndex > 0;
    public bool HasNext => CurrentIndex + 1 < Notes.Count;
    public int PagerFirstIndex => Math.Clamp(CurrentIndex - 3, 0, Math.Max(0, Notes.Count - 8));
    public IReadOnlyList<PagerDot> PagerDots => Enumerable.Range(PagerFirstIndex, Math.Min(8, Notes.Count - PagerFirstIndex))
        .Select(i => new PagerDot(i, i == CurrentIndex)).ToArray();
    public ICommand SelectCommand { get; }
    public StatusViewModel Status { get; } = new();
    public bool Pinned { get => _pinned; set { if (_pinned == value) return; _pinned = value; Changed(); } }
    public ICommand CreateCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand PinCommand { get; }
    public event EventHandler<NoteNavigationEventArgs>? Navigating;
    public event EventHandler<NoteNavigationEventArgs>? Navigated;
    public event EventHandler<Exception>? OperationFailed;
    public void CreateNote() => Navigate("create", () => _session.CreateNote());
    public void DeleteCurrent() => Navigate("delete", _session.DeleteCurrent);
    public void Select(int index) => Navigate("select", () => _session.Select(index));
    public void Next() { if (CurrentIndex + 1 < Notes.Count) Select(CurrentIndex + 1); }
    public void Previous() { if (CurrentIndex > 0) Select(CurrentIndex - 1); }
    private void Navigate(string kind, Action action)
    {
        var args = new NoteNavigationEventArgs(kind); Navigating?.Invoke(this, args);
        if (args.Cancel) return;
        try { action(); Refresh(); Navigated?.Invoke(this, args); }
        catch (Exception ex) { Refresh(); Fail(ex); OperationFailed?.Invoke(this, ex); }
    }
    public void UpdateCurrent(string text, int caretOffset)
    {
        _session.UpdateCurrent(text, caretOffset); Current.Update(_session.Current.Text, _session.Current.CaretOffset);
        Changed(nameof(IsCurrentEmpty)); Changed(nameof(SaveLineText));
    }
    public void UpdateText(string text) => UpdateCurrent(text, Current.CaretOffset);
    public void UpdateCaret(int offset) => UpdateCurrent(Current.Text, offset);
    public void Save() { try { _session.Save(); } catch (Exception ex) { Fail(ex); throw; } }
    public void Flush() => Save();
    public Core.Text.TextEditResult PlanEdit(Core.Text.TextEditRequest request) => Core.Text.MarkdownTextOperations.Execute(request);
    internal static Core.Text.TextEditResult PlanFixtureEdit(Core.Text.TextEditRequest request) => Core.Text.MarkdownTextOperations.Execute(request);
    internal static Core.Text.MarkdownScanResult ScanMarkdown(string text) => Core.Text.MarkdownScanner.Scan(text);
    public void Fail(Exception ex) => Status.Set(Status.Saving, true, ex.Message);
    public WindowSnapshot? LoadSettings()
    {
        var s = _settings.Load(); if (s is null) return null;
        ShellId = s.ShellId; LookId = s.LookId; Preferences = s.Preferences ?? new();
        return new(s.Width, s.Height, s.X, s.Y, s.Pinned, s.ShellId, s.LookId, s.LayoutVersion);
    }
    public void SaveSettings(WindowSnapshot s)
    {
        _settings.Save(new(s.Width, s.Height, s.X, s.Y, s.Pinned, s.ShellId, s.LookId, CurrentLayoutVersion, Preferences));
        ShellId = s.ShellId; LookId = s.LookId; Changed(nameof(ShellId)); Changed(nameof(LookId));
    }
    private List<NoteViewModel> ProjectNotes() => _session.Notes.Select(n => new NoteViewModel(n.Id, n.Text, n.CaretOffset)).ToList();
    private void Refresh() { _notes = ProjectNotes(); Changed(nameof(Notes)); Changed(nameof(Current)); Changed(nameof(CurrentIndex)); Changed(nameof(CountText));
        foreach (var name in ProjectionNames) Changed(name);
    }
    internal void PopulateMeasurementFixture()
    {
        for (var i = _session.Notes.Count; i < 20; i++) _session.CreateNote();
        // F54-measure-heavy-index-crash: UpdateCurrent raises save state, and bound projections read Current.
        Refresh();
        for (var i = 0; i < 20; i++)
        {
            _session.Select(i);
            _session.UpdateCurrent(string.Join('\n', Enumerable.Range(1, 200).Select(n => $"- [ ] Note {i + 1}, line {n}: baseline editor content **bold** and `code`.")), 0);
        }
        _session.Select(19); Refresh();
    }
    internal static bool VerifyReload(string directory, string text, int caret, int index)
    { var s = new NotesSession(directory); return s.Current.Text == text && s.Current.CaretOffset == caret && s.CurrentIndex == index; }
    internal static bool VerifyCurrentText(string directory, string text) => new NotesSession(directory).Current.Text == text;
    internal static bool CoreHasNoAvaloniaReference() => !typeof(NotesSession).Assembly.GetReferencedAssemblies().Any(a => a.Name!.Contains("Avalonia", StringComparison.OrdinalIgnoreCase));
    internal static bool PersistenceSmoke(string directory) => NotesSession.RunPersistenceSmoke(directory);
}

public sealed record WindowSnapshot(double Width, double Height, int X, int Y, bool Pinned, string ShellId = "classic", string LookId = "Ink", int LayoutVersion = 0);

public sealed record PagerDot(int Index, bool IsCurrent);
