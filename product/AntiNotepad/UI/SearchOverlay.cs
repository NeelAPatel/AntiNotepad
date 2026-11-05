using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AntiNotepad.Model;
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

/// <summary>Search across stack and slots: filter as you type, list or card grid, keyboard-first, trash view, batch delete.</summary>
internal sealed class SearchOverlay : Grid
{
    private readonly PadWindow _window;
    private readonly Palette _p;
    private readonly Store _store;
    private readonly TextBox _query;
    private readonly ScrollViewer _scroll;
    private readonly Panel _results;
    private readonly TextBlock _status;
    private readonly HashSet<Note> _selected = [];
    private readonly List<Border> _cards = [];
    private readonly SearchGeneration _searchGeneration = new();
    private readonly DispatcherTimer _debounce;
    private List<Note> _items = [];
    private List<SearchMatchResult> _matches = [];
    private Dictionary<string, Note> _snapshotNotes = new(StringComparer.Ordinal);
    private string? _preferredCursorId;
    private string? _displayedQuery;
    private bool _resultsPending = true;
    private bool _disposed;
    private int _cursor;

    public string Query => _query.Text ?? "";
    public bool ShowingTrash { get; private set; }
    private bool GridView => _window.Settings.SearchGrid;

    public SearchOverlay(PadWindow window, Palette palette, Store store, string query, bool trash)
    {
        _window = window; _p = palette; _store = store; ShowingTrash = trash;
        _debounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(125) };
        _debounce.Tick += (_, _) => { _debounce.Stop(); StartSearch(); };
        DetachedFromVisualTree += (_, _) => { _disposed = true; _debounce.Stop(); _searchGeneration.Dispose(); };
        RowDefinitions = new RowDefinitions("Auto,*,Auto");
        Margin = new Thickness(14, 14, 14, 46);

        _query = new TextBox
        {
            Text = query,
            PlaceholderText = trash ? "Search the trash" : "Search notes",
            FontSize = Typography.Input,
            Background = new SolidColorBrush(_p.Surface),
            Foreground = new SolidColorBrush(_p.Ink),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(_p.Faint, Paint.FieldBorder),
            CornerRadius = new CornerRadius(Shape.Card),
            Padding = new Thickness(12, 8),
            CaretBrush = new SolidColorBrush(_p.Caret), SelectionBrush = new SolidColorBrush(_p.Selection),
        };
        Avalonia.Automation.AutomationProperties.SetName(_query, "Search notes");
        _status = Kit.Text("", _p.Faint, Typography.Caption);
        _status.Margin = new Thickness(4, 8, 0, 0);
        _query.TextChanged += (_, _) =>
        {
            if (_disposed) return;
            _cursor = 0;
            _preferredCursorId = null;
            _resultsPending = true;
            _searchGeneration.Cancel();
            _debounce.Stop();
            _debounce.Start();
            _status.Text = "Searching…";
        };
        _query.AddHandler(KeyDownEvent, OnKey, RoutingStrategies.Tunnel);

        var tools = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, Margin = new Thickness(6, 0, 0, 0) };
        tools.Children.Add(Kit.IconButton(GridView ? Glyph.List : Glyph.Grid, GridView ? "List view (Ctrl+G)" : "Card grid (Ctrl+G)", _p, ToggleGrid, 32));
        tools.Children.Add(Kit.IconButton(trash ? Glyph.Back : Glyph.Delete, trash ? "Back to notes (Ctrl+T)" : $"Trash · {window.Data.Trash.Count} (Ctrl+T)", _p, ToggleTrash, 32));
        tools.Children.Add(Kit.IconButton(Glyph.Close, "Close (Esc)", _p, window.CloseOverlay, 32));
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), Margin = new Thickness(0, 0, 0, 8) };
        header.Children.Add(_query);
        Grid.SetColumn(tools, 1); header.Children.Add(tools);
        Children.Add(header);

        _results = GridView ? new WrapPanel { Orientation = Orientation.Horizontal } : new StackPanel { Spacing = 2 };
        _scroll = new ScrollViewer { Content = _results, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled };
        Grid.SetRow(_scroll, 1); Children.Add(_scroll);

        Grid.SetRow(_status, 2); Children.Add(_status);
        _status.Text = "Searching…";
        Dispatcher.UIThread.Post(() =>
        {
            if (!_disposed && _window.IsOverlay(this)) StartSearch();
        });
    }

    public void FocusQuery() => Avalonia.Threading.Dispatcher.UIThread.Post(() => { _query.Focus(); _query.CaretIndex = _query.Text?.Length ?? 0; });

    private IEnumerable<Note> Source()
    {
        var d = _window.Data;
        if (ShowingTrash) return d.Trash;
        // Newest stack notes first, then filled slots.
        return Enumerable.Reverse(d.Stack).Where(n => !n.IsEmpty).Concat(d.Slots.Where(s => !s.IsEmpty));
    }

    private void StartSearch()
    {
        if (_disposed || !_window.IsOverlay(this)) return;
        _debounce.Stop();
        var source = Source().ToList();
        _snapshotNotes = source.ToDictionary(note => note.Id, StringComparer.Ordinal);
        var snapshot = _store.CreateSearchSnapshot(source);
        var context = _store.CreateSearchContext(snapshot);
        var query = Query;
        _resultsPending = true;
        var request = _searchGeneration.Begin();
        _status.Text = "Searching…";
        _ = RunSearch(request, context, query);
    }

    private async Task RunSearch(SearchRequest request, SearchContext context, string query)
    {
        try
        {
            var matches = await Task.Run(() => Store.MatchSearch(context, query, request.Token), request.Token).ConfigureAwait(false);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!_searchGeneration.IsCurrent(request) || !_window.IsOverlay(this)) return;
                ApplyResults(matches, query);
            });
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_searchGeneration.IsCurrent(request) && _window.IsOverlay(this)) _status.Text = "Search couldn't be completed.";
            });
        }
        finally { request.Dispose(); }
    }

    private void ApplyResults(SearchMatchResult[] matches, string query)
    {
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        _matches = matches.Where(match => _snapshotNotes.ContainsKey(match.Id)).ToList();
        _items = _matches.Select(match => _snapshotNotes[match.Id]).ToList();
        if (_preferredCursorId is { } wanted)
        {
            var preferred = _items.FindIndex(note => note.Id == wanted);
            if (preferred >= 0) _cursor = preferred;
            _preferredCursorId = null;
        }
        _cursor = Math.Clamp(_cursor, 0, Math.Max(0, _items.Count - 1));
        _selected.RemoveWhere(n => !_items.Contains(n));
        _displayedQuery = query;
        _resultsPending = false;
        _results.Children.Clear();
        _cards.Clear();
        for (var i = 0; i < _items.Count; i++)
        {
            var card = Card(_items[i], _matches[i], i, terms);
            _cards.Add(card);
            _results.Children.Add(card);
        }
        if (_items.Count == 0)
            _results.Children.Add(new TextBlock { Text = ShowingTrash ? "The trash is empty." : query.Length > 0 ? "No notes match." : "No notes yet.", Foreground = new SolidColorBrush(_p.Faint), Margin = new Thickness(6, 16) });
        UpdateCardStates();
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var hint = ShowingTrash ? "Enter restores · Delete removes forever" : "Enter opens · Ctrl+Delete moves to trash · Ctrl+click selects several";
        _status.Text = $"{_items.Count} {(ShowingTrash ? "in trash" : _items.Count == 1 ? "note" : "notes")}{(_selected.Count > 0 ? $" · {_selected.Count} selected" : "")} · {hint}";
    }

    private Border Card(Note note, SearchMatchResult match, int index, string[] terms)
    {
        var title = Kit.Text(match.Title, _p.Ink, Typography.BodyEmphasis, Typography.Strong);
        title.TextTrimming = TextTrimming.CharacterEllipsis;
        var snippet = new TextBlock
        {
            Foreground = new SolidColorBrush(_p.Muted),
            FontSize = Typography.Small,
            TextWrapping = TextWrapping.Wrap,
            MaxLines = GridView ? 5 : 2,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontFamily = new FontFamily(Typography.MonoFamily),
        };
        Highlight(snippet, match.Snippet, terms);
        var meta = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        if (note.Slot > 0) meta.Children.Add(Kit.Text($"slot {note.Slot}", _p.Accent, Typography.Micro, Typography.Strong));
        if (match.Mode != NoteMode.Plain) meta.Children.Add(Kit.Text(Modes.Label(match.Mode, match.ModeArgument), _p.Keyword, Typography.Micro, Typography.Strong));
        meta.Children.Add(Kit.Text(ShowingTrash && note.Deleted is { } del ? "deleted " + Kit.Ago(del) : Kit.Ago(note.Modified), _p.Faint, Typography.Micro));
        var body = new StackPanel { Spacing = 3, Children = { title, snippet, meta } };
        var border = new Border
        {
            Child = body,
            Padding = new Thickness(12, 8),
            CornerRadius = new CornerRadius(Shape.Card),
            Background = new SolidColorBrush(_p.Surface, Paint.ResultSurface),
            BorderBrush = new SolidColorBrush(_p.Faint, Paint.ResultBorder),
            BorderThickness = new Thickness(1),
            Width = GridView ? CardWidth() : double.NaN,
            Height = GridView ? 152 : double.NaN,
            Margin = GridView ? new Thickness(0, 0, 8, 8) : new Thickness(0),
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        border.PointerPressed += (_, e) =>
        {
            e.Handled = true;
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) { if (!_selected.Add(note)) _selected.Remove(note); _cursor = index; UpdateCardStates(); UpdateStatus(); return; }
            _cursor = index;
            _press = (note, border, e.GetPosition(this));
            e.Pointer.Capture(border);
        };
        border.PointerMoved += (_, e) =>
        {
            if (_press is not { } press || press.Card != border) return;
            var at = e.GetPosition(this);
            if (!_dragging && Point.Distance(at, press.At) < 6) return;
            if (!CanReorder) return;
            _dragging = true;
            border.Opacity = Paint.Dimmed;
            border.RenderTransform = new TranslateTransform(GridView ? at.X - press.At.X : 0, at.Y - press.At.Y);
            _dropIndex = DropIndex(e);
        };
        border.PointerReleased += (_, e) =>
        {
            if (_press is not { } press || press.Card != border) return;
            e.Pointer.Capture(null);
            var dragged = _dragging;
            _press = null; _dragging = false;
            border.Opacity = 1; border.RenderTransform = null;
            if (!dragged) { Open(); return; }
            Reorder(press.Note, _dropIndex);
        };
        Avalonia.Automation.AutomationProperties.SetName(border, match.Title);
        return border;
    }

    private void UpdateCardStates()
    {
        for (var i = 0; i < _cards.Count; i++)
        {
            var border = _cards[i];
            var current = i == _cursor;
            var picked = _selected.Contains(_items[i]);
            border.Background = new SolidColorBrush(picked ? _p.Selection : _p.Surface, picked || current ? 1 : Paint.ResultSurface);
            border.BorderBrush = new SolidColorBrush(current ? _p.Accent : _p.Faint, current ? Paint.ResultCurrent : Paint.ResultBorder);
            border.BorderThickness = new Thickness(current ? 2 : 1);
        }
    }

    // ---------- drag to reorder (unfiltered stack only, where the order on screen is the stack's order) ----------

    private (Note Note, Border Card, Point At)? _press;
    private bool _dragging;
    private int _dropIndex;

    private bool CanReorder => HasCurrentResults && !ShowingTrash && Query.Trim().Length == 0;

    private int DropIndex(PointerEventArgs e)
    {
        // Layout bounds, not hit-testing: the dragged card itself is translated under the pointer.
        var p = e.GetPosition(_results);
        for (var i = 0; i < _results.Children.Count; i++)
        {
            var b = _results.Children[i].Bounds;
            if (GridView ? b.Contains(p) : p.Y < b.Bottom) return i;
        }
        return _results.Children.Count - 1;
    }

    private void Reorder(Note note, int viewIndex)
    {
        var stack = _window.Data.Stack;
        var target = _items.ElementAtOrDefault(viewIndex);
        if (note.Slot > 0 || target is null || target.Slot > 0 || ReferenceEquals(target, note)) return;
        var current = _window.Data.Stack.ElementAtOrDefault(_window.Data.Current);
        stack.Remove(note);
        // The view lists newest first; dropping onto a card puts the note just "newer" than it.
        stack.Insert(stack.IndexOf(target) + 1, note);
        if (current is not null) _window.Data.Current = stack.IndexOf(current);
        _window.SearchChangedData();
        _preferredCursorId = note.Id;
        StartSearch();
    }

    // Two or three columns that fill the pad's width exactly.
    private double CardWidth()
    {
        var width = Math.Max(300, _window.Bounds.Width - 28 - 14);
        var columns = Math.Max(2, (int)(width / 200));
        return Math.Floor((width - columns * 8) / columns);
    }

    /// <summary>Matched words stand out in the snippet so you can see why a note matched.</summary>
    private void Highlight(TextBlock block, string text, string[] terms)
    {
        var inlines = block.Inlines!;
        if (terms.Length == 0) { block.Text = text; return; }
        var at = 0;
        while (at < text.Length)
        {
            var next = terms.Select(t => (Term: t, Index: text.IndexOf(t, at, StringComparison.CurrentCultureIgnoreCase)))
                .Where(x => x.Index >= 0).OrderBy(x => x.Index).FirstOrDefault();
            if (next.Term is null) { inlines.Add(new Avalonia.Controls.Documents.Run(text[at..])); break; }
            if (next.Index > at) inlines.Add(new Avalonia.Controls.Documents.Run(text[at..next.Index]));
            inlines.Add(new Avalonia.Controls.Documents.Run(text.Substring(next.Index, next.Term.Length))
            {
                Foreground = new SolidColorBrush(_p.Ink),
                FontWeight = Typography.Strong,
                Background = new SolidColorBrush(_p.Selection),
            });
            at = next.Index + next.Term.Length;
        }
    }

    private void OnKey(object? sender, KeyEventArgs e)
    {
        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var step = GridView ? Math.Max(1, (int)((_scroll.Bounds.Width + 8) / (CardWidth() + 8))) : 1;
        switch (e.Key)
        {
            case Key.Down: Move(step); break;
            case Key.Up: Move(-step); break;
            case Key.Right when GridView && (_query.Text?.Length ?? 0) == _query.CaretIndex: Move(1); break;
            case Key.Left when GridView && _query.CaretIndex == 0: Move(-1); break;
            case Key.Enter: Open(); break;
            case Key.G when ctrl: ToggleGrid(); break;
            case Key.T when ctrl: ToggleTrash(); break;
            case Key.Space when ctrl: if (_items.Count > 0 && !_selected.Add(_items[_cursor])) _selected.Remove(_items[_cursor]); UpdateCardStates(); UpdateStatus(); break;
            case Key.Delete when ctrl || ShowingTrash: DeleteSelected(); break;
            case Key.Back when ctrl && (_query.Text?.Length ?? 0) == 0: DeleteSelected(); break;
            default: return;
        }
        e.Handled = true;
    }

    private void Move(int delta)
    {
        if (_items.Count == 0) return;
        _cursor = Math.Clamp(_cursor + delta, 0, _items.Count - 1);
        UpdateCardStates();
        if (_results.Children.ElementAtOrDefault(_cursor) is Control c) c.BringIntoView();
    }

    private void Open()
    {
        if (!HasCurrentResults || _items.Count == 0) return;
        var note = _items[_cursor];
        if (ShowingTrash) { _window.RestoreFromTrash(note); _window.CloseOverlay(); _window.Toast("Restored from trash"); return; }
        _window.JumpTo(note);
    }

    private void DeleteSelected()
    {
        if (!HasCurrentResults) return;
        var targets = _selected.Count > 0 ? _selected.ToList() : _items.Count > 0 ? [_items[_cursor]] : [];
        if (targets.Count == 0) return;
        var d = _window.Data;
        if (ShowingTrash)
        {
            foreach (var n in targets) d.Trash.Remove(n);
            _window.Toast(targets.Count == 1 ? "Deleted forever" : $"{targets.Count} notes deleted forever");
        }
        else
        {
            var removed = targets.Where(n => n.Slot == 0).Select(n => (Note: n, Index: d.Stack.IndexOf(n))).OrderBy(x => x.Index).ToList();
            foreach (var n in targets)
            {
                if (n.Slot > 0) continue; // slots are cleared from the editor, not from search
                d.Stack.Remove(n);
                Editor.PadView.Forget(n);
                n.Deleted = DateTime.Now;
                d.Trash.Insert(0, n);
            }
            if (d.Stack.Count == 0) d.Stack.Add(new Note());
            d.Current = Math.Clamp(d.Current, 0, d.Stack.Count - 1);
            _window.Toast(removed.Count == 1 ? "Moved to trash" : $"{removed.Count} notes moved to trash", "Undo", () =>
            {
                // Back where they were, oldest index first so later indexes stay valid.
                foreach (var (n, index) in removed)
                {
                    d.Trash.Remove(n); n.Deleted = null;
                    d.Stack.Insert(Math.Clamp(index, 0, d.Stack.Count), n);
                }
                d.Stack.RemoveAll(x => x.IsEmpty && d.Stack.Count > 1);
                d.Current = Math.Clamp(d.Current, 0, d.Stack.Count - 1);
                _window.SearchChangedData();
                if (_window.IsOverlay(this)) StartSearch();
            });
        }
        _selected.Clear();
        _window.SearchChangedData();
        StartSearch();
    }

    private bool HasCurrentResults => !_disposed && !_resultsPending && string.Equals(_displayedQuery, Query, StringComparison.Ordinal);

    private void ToggleGrid()
    {
        _window.SettingsService.Update(settings => settings.SearchGrid = !settings.SearchGrid);
        _window.OpenSearch(Query, ShowingTrash);
    }

    private void ToggleTrash() => _window.OpenSearch("", !ShowingTrash);
}
