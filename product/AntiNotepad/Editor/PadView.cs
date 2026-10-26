using Avalonia.Input.Platform;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Rendering;
using AntiNotepad;
using AntiNotepad.Calc;
using AntiNotepad.Model;
using AntiNotepad.Motion;
using AntiNotepad.Theme;

namespace AntiNotepad.Editor;

/// <summary>One editing pane bound to one note. Documents are cached per note so each note keeps its own undo history.</summary>
internal sealed class PadView : Grid
{
    private static readonly Dictionary<string, TextDocument> Documents = [];
    private static readonly Regex ListPrefix = new(@"^(\s*)((?:[-*+]\s+)?\[[ xX]\]\s?|[-*+]\s|([0-9]+)([.)])\s)", RegexOptions.Compiled);

    private readonly Timers _timers;
    private Action? _timerTick;
    private readonly TextBlock _hint;
    private readonly ResultRenderer _results;
    private readonly SmoothCaret _caret;
    private readonly NoteAnalysis _analysis = new();
    private FoldingManager? _folding;
    private PaperLayer _paper = null!;
    private ChevronGutter? _gutter;
    private readonly FenceColorizer _fences;
    private IBrush? _edgeFadeMask;
    private (double Height, double EdgeBand, byte TopAlpha, byte BottomAlpha, double BackdropOffset, byte BackdropOpacity)? _edgeFadeKey;
    private double _topBackdropOffset;
    private double _topBackdropOpacity;
    public NoteStructure? Structure { get; private set; }
    private bool _binding;
    private string? _loadHint;
    private bool _loadHintError;
    private Anim.Handle? _arrive;
    private int _bindToken;
    private int _lastCaretLine;
    private int _observedLineCount;
    private int? _pendingFirstChanged;
    private int _pendingLastChanged;
    private int _pendingChangeCount;
    private bool _pendingFullAnalysis;

    public TextEditor Editor { get; }
    public Note? Note { get; private set; }
    public Palette Palette { get; private set; }
    public Settings Settings { get; private set; }
    public NoteMode Mode { get; private set; }
    public string? CodeLanguage { get; private set; }
    public int ModeKeywordLength { get; private set; }
    public bool Decorate => Mode != NoteMode.Code;
    public int CaretLine => Editor.TextArea.Caret.Line;

    public event Action? Edited;
    public event Action? ModeChanged;
    public event Action<string>? Toast;
    public event Action? FocusedPane;
    /// <summary>Copies go through the window so clipboard-capture notes don't record AntiNotepad's own copies.</summary>
    public event Action<string>? Copy;
    /// <summary>The pad's own items for the right-click menu (new note, search, pin...), added below the editing ones.</summary>
    public Func<IEnumerable<MenuEntry>>? AppMenu { get; set; }
    public Action<Control, IReadOnlyList<MenuEntry>>? ShowMenu { get; set; }

    public PadView(Palette palette, Settings settings, Timers timers)
    {
        Palette = palette; Settings = settings; _timers = timers;
        Editor = new ScrollableTopInsetTextEditor
        {
            WordWrap = true,
            ShowLineNumbers = false,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            Margin = new Thickness(36, 4, 30, ChromeTokens.PagerStripHeight),
        };
        Editor.Options.EnableHyperlinks = false;
        Editor.Options.EnableEmailHyperlinks = false;
        Editor.Options.HighlightCurrentLine = false;
        Editor.Options.AllowScrollBelowDocument = false;
        Editor.Options.ShowBoxForControlCharacters = false;
        Editor.Options.IndentationSize = 2;
        Editor.Options.ConvertTabsToSpaces = true;
        Editor.Options.CutCopyWholeLine = true;
        Avalonia.Automation.AutomationProperties.SetName(Editor, "Note text");
        var view = Editor.TextArea.TextView;
        view.BackgroundRenderers.Add(new IndentGuideRenderer(this));
        _results = new ResultRenderer(this);
        view.BackgroundRenderers.Add(_results);
        view.BackgroundRenderers.Add(new CheckboxRenderer(this));
        view.BackgroundRenderers.Add(new DividerRenderer(this));
        _caret = new SmoothCaret(this);
        view.BackgroundRenderers.Add(_caret);
        _ = new SmoothScroll(Editor, () => Settings);
        view.LineTransformers.Add(new NoteColorizer(this));
        view.LineTransformers.Add(new MathColorizer(this));
        _fences = new FenceColorizer(this);
        view.LineTransformers.Add(_fences);
        view.ElementGenerators.Add(new CheckboxGenerator(this));
        view.ElementGenerators.Add(new BulletGenerator(this));
        view.ElementGenerators.Add(new LinkGenerator(this));
        view.ElementGenerators.Add(new MarkerGenerator(this));
        _hint = new TextBlock
        {
            IsHitTestVisible = false, Margin = new Thickness(37, 7, 30, 0), TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left,
        };
        _paper = new PaperLayer(this).Init();
        Children.Add(_paper);
        Children.Add(Editor);
        ((ScrollableTopInsetTextEditor)Editor).InsetTextArea.ScrollOffsetChanged += (_, _) => _paper.InvalidateVisual();
        view.VisualLinesChanged += (_, _) => _paper.InvalidateVisual();
        Children.Add(_hint);
        ((ScrollableTopInsetTextEditor)Editor).InsetTextArea.ScrollOffsetChanged += (_, _) => UpdateEdgeFade();
        view.DocumentChanged += (_, _) => UpdateEdgeFade();
        // After a note loads or text reflows, the amount left to scroll changes without a scroll.
        view.VisualLinesChanged += (_, _) => UpdateEdgeFade();
        Editor.SizeChanged += (_, _) => UpdateEdgeFade();
        SizeChanged += (_, e) => { if (Math.Abs(e.NewSize.Width - e.PreviousSize.Width) > 0.5) ApplyMargins(); };
        Editor.TextArea.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        Editor.TextArea.TextEntered += OnTextEntered;
        Editor.TextArea.Caret.PositionChanged += (_, _) => OnCaretMoved();
        Editor.TextArea.GotFocus += (_, _) => FocusedPane?.Invoke();
        view.AddHandler(PointerPressedEvent, OnViewPressed, RoutingStrategies.Tunnel);
        Editor.TextArea.ContextRequested += (_, e) => { RequestMenu(); e.Handled = true; };
        view.PointerMoved += OnViewMoved;
        // Fold chevrons show while the pointer is over the note.
        PointerEntered += (_, _) => { if (_gutter is not null) _gutter.ShowAll = true; };
        PointerExited += (_, _) => { if (_gutter is not null) _gutter.ShowAll = false; };
        Editor.TemplateApplied += (_, _) => Dispatcher.UIThread.Post(ClearBackgrounds, DispatcherPriority.Loaded);
        Editor.Loaded += (_, _) => ClearBackgrounds();
        _analyse.Tick += (_, _) =>
        {
            _analyse.Stop();
            var first = _pendingFirstChanged ?? 0;
            var last = _pendingFirstChanged is null ? Editor.Document.LineCount - 1 : _pendingLastChanged;
            var forceFull = _pendingFullAnalysis;
            ClearPendingAnalysis();
            Analyse(first, last, forceFull);
        };
        _timerTick = () => { if (Mode == NoteMode.Timer) view.InvalidateLayer(KnownLayer.Selection); };
        _timers.Tick += _timerTick;
        ApplyLook(palette, settings);
    }

    // AvaloniaEdit templates paint opaque backgrounds; the window material must show through.
    private void ClearBackgrounds()
    {
        Editor.Background = Brushes.Transparent; Editor.TextArea.Background = Brushes.Transparent;
        foreach (var visual in Editor.GetVisualDescendants())
        {
            if (visual is ScrollBar) continue;
            if (visual is Panel panel) panel.Background = Brushes.Transparent;
            else if (visual is Border border) border.Background = Brushes.Transparent;
            else if (visual is ContentPresenter presenter) presenter.Background = Brushes.Transparent;
        }
    }

    public void ApplyLook(Palette palette, Settings settings)
    {
        Palette = palette; Settings = settings;
        _caret.SettingsChanged();
        _paper?.InvalidateVisual();
        Editor.FontFamily = new FontFamily($"{settings.Font}, Cascadia Mono, Consolas, monospace");
        Editor.FontSize = settings.FontSize;
        Editor.Foreground = new SolidColorBrush(palette.Ink);
        Editor.TextArea.SelectionBrush = new SolidColorBrush(palette.Selection);
        Editor.TextArea.SelectionForeground = null;
        Editor.TextArea.SelectionBorder = null;
        Editor.TextArea.SelectionCornerRadius = 3;
        // SmoothCaret draws the caret; the built-in one stays (for IME placement) but invisible.
        Editor.TextArea.Caret.CaretBrush = Brushes.Transparent;
        _hint.FontFamily = Editor.FontFamily; _hint.FontSize = settings.FontSize;
        _hint.Foreground = _loadHintError ? new SolidColorBrush(palette.Error, .78) : new SolidColorBrush(palette.Faint);
        if (Math.Abs(Editor.Options.LineHeightFactor - settings.LineSpacing) > 0.001) Editor.Options.LineHeightFactor = settings.LineSpacing;
        ApplyMargins();
        ApplyCodeHighlighting();
        Redraw();
    }

    /// <summary>Most recently used first; only these notes keep an editor document (and undo history) in memory.</summary>
    private static readonly LinkedList<string> Recent = new();
    private static readonly Dictionary<string, Note> Owners = [];

    public static TextDocument DocumentFor(Note note)
    {
        if (!Documents.TryGetValue(note.Id, out var doc))
        {
            doc = new TextDocument(note.Text);
            doc.UndoStack.SizeLimit = 400;
            // The note reads the live document instead of copying it on every keystroke.
            note.Attach(() => doc.Text);
            doc.TextChanged += (_, _) => note.Touched(IsBlank(doc), Modes.Detect(FirstLine(doc)).Mode);
            Documents[note.Id] = doc;
            Owners[note.Id] = note;
        }
        Recent.Remove(note.Id);
        Recent.AddFirst(note.Id);
        return doc;
    }

    private static bool IsBlank(TextDocument doc) => doc.TextLength == 0 || doc.TextLength < 256 && string.IsNullOrWhiteSpace(doc.Text);

    private static string FirstLine(TextDocument doc) => doc.GetText(doc.GetLineByNumber(1));

    public static void Forget(Note note)
    {
        Documents.Remove(note.Id); Owners.Remove(note.Id); Recent.Remove(note.Id);
        note.Detach();
    }

    /// <summary>
    /// Drops editor documents beyond the <paramref name="keep"/> most recent, except notes on screen.
    /// Call after a save, so released notes leave nothing in memory (their text reloads from disk on demand).
    /// </summary>
    public static int Evict(int keep, IReadOnlyCollection<Note?> onScreen)
    {
        var released = 0;
        var index = 0;
        foreach (var id in Recent.ToList())
        {
            index++;
            if (index <= keep || onScreen.Any(n => n?.Id == id) || !Owners.TryGetValue(id, out var note) || note.Dirty) continue;
            Documents.Remove(id); Owners.Remove(id); Recent.Remove(id);
            note.Detach();
            released++;
        }
        return released;
    }

    public static int CachedDocuments => Documents.Count;

    /// <summary>
    /// A preview editor (Settings) keeps its sample documents to itself: out of the shared cache, so samples never push
    /// real notes' documents (and their undo history) out, and nothing static holds on to the preview once it's gone.
    /// </summary>
    private bool _isPrivate;
    public bool Private
    {
        get => _isPrivate;
        init
        {
            _isPrivate = value;
            if (Editor is ScrollableTopInsetTextEditor insetEditor) UpdateTopInset(insetEditor);
        }
    }
    private readonly Dictionary<Note, TextDocument> _private = [];

    public void Bind(Note note)
    {
        if (ReferenceEquals(Note, note)) return;
        Flush();
        _binding = true;
        if (Editor.Document is { } old) old.Changed -= OnDocumentChanged;
        _analyse.Stop();
        _analysis.Reset();
        ClearPendingAnalysis();
        Note = note;
        var locationError = Program.NotesLocationError;
        if (locationError is null) _ = note.PeekText();
        var unavailable = locationError is not null || note.BodyUnavailable;
        _loadHintError = unavailable;
        _loadHint = locationError is not null
            ? $"Error: Notes folder not found\n\n{Program.DataFolder}\n\nChoose a folder in Settings › Storage"
            : note.BodyUnavailable ? "Error: Note not found\n\nCheck recovered folder in Settings › Storage" : null;
        _hint.Foreground = _loadHintError ? new SolidColorBrush(Palette.Error, .78) : new SolidColorBrush(Palette.Faint);
        Editor.IsReadOnly = unavailable;
        var doc = unavailable ? new TextDocument() : Private
            ? _private.TryGetValue(note, out var own) ? own : _private[note] = new TextDocument(note.PeekText())
            : DocumentFor(note);
        // Folding state belongs to a document: the old manager must go before the swap (its generator rejects other documents).
        if (_folding is not null) { FoldingManager.Uninstall(_folding); _folding = null; }
        foreach (var stale in Editor.TextArea.LeftMargins.OfType<ChevronGutter>().ToList()) Editor.TextArea.LeftMargins.Remove(stale);
        _gutter = null;
        Editor.Document = doc;
        doc.Changed += OnDocumentChanged;
        _observedLineCount = doc.LineCount;
        _folding = Settings.Folding ? FoldingManager.Install(Editor.TextArea) : null;
        if (_folding is not null) _gutter = ChevronGutter.Replace(Editor.TextArea, _folding, this);
        StyleFoldingMargin();
        _fences.Reset();
        Editor.CaretOffset = Math.Clamp(note.Caret, 0, doc.TextLength);
        _binding = false;
        Analyse();
        var scroll = note.Scroll;
        var token = ++_bindToken;
        SmoothScroll.SetOffset(Editor, scroll);
        // Layout may not be ready yet; re-apply once, unless another note was bound meanwhile.
        Dispatcher.UIThread.Post(() => { if (token == _bindToken) SmoothScroll.SetOffset(Editor, scroll); }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Text fades into the top or bottom edge, but only where there is more to scroll: the first and last lines are never
    /// dimmed, and the fade grows in over the first few pixels of scroll.
    /// </summary>
    private void UpdateEdgeFade()
    {
        var h = Editor.Bounds.Height;
        if (h <= 0) { ClearEdgeFade(); return; }

        var edgeFade = Settings.EdgeFade && h >= 60;
        var band = edgeFade ? ChromeTokens.TextEdgeFadeBand : 0;
        var topAlpha = byte.MaxValue;
        var bottomAlpha = byte.MaxValue;
        if (edgeFade)
        {
            var view = Editor.TextArea.TextView;
            var offset = Editor.VerticalOffset;
            var more = Math.Max(0, Editor.ExtentHeight - (offset + Editor.ViewportHeight));
            topAlpha = (byte)(255 * (1 - Math.Clamp(view.VerticalOffset / band, 0, 1)));
            bottomAlpha = (byte)(255 * (1 - Math.Clamp(more / band, 0, 1)));
        }
        var hasEdgeFade = edgeFade && (topAlpha < 255 || bottomAlpha < 255);

        var backdropOpacity = (byte)Math.Round(255 * Math.Clamp(_topBackdropOpacity, 0, 1));
        var backdropOffset = backdropOpacity == 0 ? 0 : _topBackdropOffset;
        if (!hasEdgeFade && backdropOpacity == 0) { ClearEdgeFade(); return; }

        var key = (h, hasEdgeFade ? band : 0, topAlpha, bottomAlpha, backdropOffset, backdropOpacity);
        if (_edgeFadeMask is not null && _edgeFadeKey == key)
        {
            if (!ReferenceEquals(Editor.OpacityMask, _edgeFadeMask)) Editor.OpacityMask = _edgeFadeMask;
            return;
        }

        var points = new SortedSet<double> { 0, h };
        if (hasEdgeFade)
        {
            points.Add(band);
            points.Add(h - band);
        }
        if (backdropOpacity > 0)
        {
            points.Add(backdropOffset);
            points.Add(backdropOffset + ChromeTokens.TopBackdrop.DenseStop);
            points.Add(backdropOffset + ChromeTokens.TopBackdrop.MidpointStop);
            points.Add(backdropOffset + ChromeTokens.TopBackdrop.ClearStop);
        }
        var boundaries = points.Where(y => y >= 0 && y <= h).ToArray();
        var samples = new List<double>(boundaries.Length + 12);
        for (var i = 0; i < boundaries.Length; i++)
        {
            var start = boundaries[i];
            samples.Add(start);
            if (i + 1 == boundaries.Length) continue;
            var end = boundaries[i + 1];
            if (EdgeFadeVaries(start, end, band, topAlpha, bottomAlpha, h)
                && backdropOpacity > 0 && Math.Abs(ChromeTokens.TopBackdrop.MaskAlphaAt(start - backdropOffset)
                    - ChromeTokens.TopBackdrop.MaskAlphaAt(end - backdropOffset)) > 0.001)
            {
                var subdivisions = Math.Max(1, (int)Math.Ceiling(end - start));
                for (var step = 1; step < subdivisions; step++) samples.Add(start + (end - start) * step / subdivisions);
            }
        }

        var mask = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative), EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        };
        foreach (var y in samples)
        {
            var edgeAlpha = EdgeFadeAlphaAt(y, h, band, topAlpha, bottomAlpha);
            var bandAlpha = backdropOpacity / 255.0 * ChromeTokens.TopBackdrop.MaskAlphaAt(y - backdropOffset);
            var alpha = (byte)Math.Clamp(Math.Round(255 * edgeAlpha * (1 - bandAlpha)), 0, 255);
            mask.GradientStops.Add(new GradientStop(Color.FromArgb(alpha, 0, 0, 0), y / h));
        }
        var immutableMask = mask.ToImmutable();
        _edgeFadeMask = immutableMask;
        _edgeFadeKey = key;
        Editor.OpacityMask = immutableMask;
    }

    private static bool EdgeFadeVaries(double start, double end, double band, byte topAlpha, byte bottomAlpha, double height) =>
        band > 0 && ((topAlpha < 255 && start < band && end > 0)
            || (bottomAlpha < 255 && start < height && end > height - band));

    private static double EdgeFadeAlphaAt(double y, double height, double band, byte topAlpha, byte bottomAlpha)
    {
        var alpha = 1.0;
        if (band > 0 && topAlpha < 255 && y < band)
            alpha *= topAlpha / 255.0 + (1 - topAlpha / 255.0) * Math.Clamp(y / band, 0, 1);
        if (band > 0 && bottomAlpha < 255 && y > height - band)
            alpha *= 1 + (bottomAlpha / 255.0 - 1) * Math.Clamp((y - (height - band)) / band, 0, 1);
        return alpha;
    }

    internal void SetTopBackdropMask(double editorOffset, double opacity)
    {
        var nextOpacity = Math.Clamp(opacity, 0, 1);
        if (Math.Abs(_topBackdropOffset - editorOffset) < 0.01 && Math.Abs(_topBackdropOpacity - nextOpacity) < 0.005) return;
        _topBackdropOffset = editorOffset;
        _topBackdropOpacity = nextOpacity;
        UpdateEdgeFade();
    }

    private void ClearEdgeFade()
    {
        _edgeFadeMask = null;
        _edgeFadeKey = null;
        if (Editor.OpacityMask is not null) Editor.OpacityMask = null;
    }

    /// <summary>Lets go of the document (a preview being closed), so nothing it was listening to keeps it alive.</summary>
    public void Release()
    {
        if (Editor.Document is { } doc) doc.Changed -= OnDocumentChanged;
        if (_timerTick is { } tick) { _timers.Tick -= tick; _timerTick = null; }
        ClearEdgeFade();
        _analyse.Stop();
        _analysis.Reset();
        ClearPendingAnalysis();
        _caret.Stop();
        _private.Clear();
    }

    /// <summary>Copies caret and scroll back into the note (text is already live).</summary>
    public void Flush()
    {
        if (Note is null) return;
        Note.Caret = Editor.CaretOffset;
        Note.Scroll = Editor.VerticalOffset;
    }

    /// <summary>Note-to-note arrival: slide in from the travel direction while the new text is already editable.</summary>
    public void Arrive(int direction)
    {
        _arrive?.Cancel();
        var shift = new TranslateTransform();
        RenderTransform = shift;
        _arrive = Anim.Run(this, 190, t =>
        {
            shift.X = direction * 34 * (1 - t);
            Opacity = 0.25 + 0.75 * t;
        }, () => { RenderTransform = null; Opacity = 1; });
    }

    public void FocusText() => Editor.TextArea.Focus();

    private void OnDocumentChanged(object? sender, DocumentChangeEventArgs e)
    {
        if (_binding) return;
        var doc = Editor.Document;
        var (first, last) = ChangedLineRange(doc, e);
        _analysis.Invalidate(doc, first, last);
        var lineCountChanged = doc.LineCount != _observedLineCount;
        _observedLineCount = doc.LineCount;
        // Long notes: analysis (modes, fences, folds, answers) waits for a typing pause instead of running per keystroke.
        if (doc.LineCount > 1500)
        {
            if (_pendingFirstChanged is null)
            {
                _pendingFirstChanged = first;
                _pendingLastChanged = last;
            }
            else
            {
                _pendingFirstChanged = Math.Min(_pendingFirstChanged.Value, first);
                _pendingLastChanged = Math.Max(_pendingLastChanged, last);
            }
            _pendingChangeCount++;
            // A second line-count change can move an earlier pending range. Fall back to a full pass rather than risk stale line maps.
            if (lineCountChanged && _pendingChangeCount > 1) _pendingFullAnalysis = true;
            _analyse.Stop(); _analyse.Start();
        }
        else
        {
            var hadPending = _pendingFirstChanged is not null;
            _analyse.Stop();
            ClearPendingAnalysis();
            Analyse(hadPending ? 0 : first, hadPending ? doc.LineCount - 1 : last, hadPending || lineCountChanged);
        }
        Edited?.Invoke();
    }

    private readonly DispatcherTimer _analyse = new() { Interval = TimeSpan.FromMilliseconds(140) };

    private static (int First, int Last) ChangedLineRange(TextDocument doc, DocumentChangeEventArgs e)
    {
        var startOffset = Math.Clamp(e.Offset, 0, doc.TextLength);
        var endOffset = Math.Clamp(e.Offset + e.InsertionLength, 0, doc.TextLength);
        var first = Math.Max(0, doc.GetLineByOffset(startOffset).LineNumber - 2);
        var last = Math.Min(doc.LineCount - 1, doc.GetLineByOffset(endOffset).LineNumber);
        return (first, Math.Max(first, last));
    }

    private void ClearPendingAnalysis()
    {
        _pendingFirstChanged = null;
        _pendingLastChanged = 0;
        _pendingChangeCount = 0;
        _pendingFullAnalysis = false;
    }

    private void Analyse() => Analyse(0, Editor.Document.LineCount - 1, forceFull: true);

    private void Analyse(int firstChanged, int lastChanged, bool forceFull = false)
    {
        var doc = Editor.Document;
        var previousMode = Mode;
        var previousLanguage = CodeLanguage;
        _analysis.Update(doc, firstChanged, lastChanged, Settings.InlineMath, Note?.Id, _timers, forceFull);
        Mode = _analysis.Mode;
        CodeLanguage = _analysis.CodeLanguage;
        ModeKeywordLength = _analysis.ModeKeywordLength;
        Structure = _analysis.Structure!;
        var lines = _analysis.Lines;
        var changed = previousMode != Mode || previousLanguage != CodeLanguage;
        _fences.UpdateStructure(Structure);
        UpdateFolds(lines);
        _hint.IsVisible = _loadHint is not null || doc.TextLength == 0;
        _hint.Text = _loadHint ?? (Note is { Slot: > 0 } slot
            ? $"Slot {slot.Slot} is empty.\n\nType here, or press Ctrl+Shift+{slot.Slot}\non any note to keep it in this slot."
            : "Start typing.\n\nLine 1 can switch modes:\nmath · list · timer · code py · clipboard");
        if (changed) { ApplyCodeHighlighting(); ModeChanged?.Invoke(); }
        Redraw();
    }

    /// <summary>
    /// Math notes with prose labels ("lunch 12 + 3"): lines that fail are retried with the leading words blanked,
    /// keeping column offsets so token colours still line up. Bullets are ignored the same way.
    /// </summary>
    internal static IReadOnlyList<LineEval?> EvaluateMath(string[] lines)
    {
        var cleaned = lines.Select((l, i) => i == 0 ? l : StripListMarker(l)).ToArray();
        var first = Calculator.EvaluateNote(cleaned);
        var retry = (string[])cleaned.Clone();
        var changed = false;
        for (var i = 1; i < retry.Length; i++)
        {
            if (first[i]?.Result is not null) continue;
            var blanked = BlankLabel(retry[i]);
            if (blanked != retry[i]) { retry[i] = blanked; changed = true; }
        }
        if (!changed) return first;
        var second = Calculator.EvaluateNote(retry);
        var merged = new LineEval?[first.Count];
        for (var i = 0; i < merged.Length; i++)
        {
            var retryLine = second[i];
            if (first[i]?.Result is not null || retryLine is null
                || (retryLine.Result is null && retryLine.ErrorReason == CalcErrorReason.None))
            { merged[i] = first[i]; continue; }
            var label = retry[i].Length - retry[i].TrimStart().Length;
            var tokens = new List<CalcToken> { new(0, label, TokenKind.Comment) };
            tokens.AddRange(retryLine.Tokens);
            merged[i] = new LineEval { Result = retryLine.Result, ErrorReason = retryLine.ErrorReason, Tokens = tokens, AssignedName = retryLine.AssignedName };
        }
        return merged;
    }

    /// <summary>"spent 12.50 + 8.25 =" → spaces up to the first number, currency sign or bracket.</summary>
    internal static string BlankLabel(string line)
    {
        var m = Regex.Match(line, @"^\s*[A-Za-z][A-Za-z' ]*?\s+(?=[\d$€£(.])");
        return m.Success ? new string(' ', m.Length) + line[m.Length..] : line;
    }

    /// <summary>`- 19.99 * 3 =` is a bullet, not a negative number.</summary>
    internal static string StripListMarker(string line)
    {
        var m = ListPrefix.Match(line);
        return m.Success && !m.Groups[3].Success ? new string(' ', m.Length) + line[m.Length..] : line;
    }

    private void Redraw() => Editor.TextArea.TextView.Redraw();

    public bool InFence(int line) => Structure is { } s && line < s.InFence.Length && s.InFence[line];
    public LineEval? MathLine(int line) => line - 1 < _analysis.MathLines.Count ? _analysis.MathLines[line - 1] : null;
    public LineEval? InlineLine(int line) => _analysis.InlineLines.GetValueOrDefault(line);
    public CalcResult? InlineResult(int line) => InlineLine(line)?.Result;
    internal static string TimerKey(string noteId, string durationText, int ordinal) => NoteAnalysis.TimerKey(noteId, durationText, ordinal);

    /// <summary>The chip to draw at a line's end: text, kind, colour, and the value clicking acts on.</summary>
    public (string Text, string Kind, Color Color, string Value)? ChipFor(int line)
    {
        var p = Palette;
        switch (Mode)
        {
            case NoteMode.Math when MathLine(line)?.Result is { } r:
                return (r.Display, "result", p.Result, r.Display);
            case NoteMode.Math when MathLine(line)?.ErrorReason == CalcErrorReason.MixedCurrencies:
                return ("mixed currencies", "error", p.Error, "");
            case NoteMode.Plain when InlineResult(line) is { } r:
                return (r.Display, "result", p.Result, r.Display);
            case NoteMode.Plain when InlineLine(line)?.ErrorReason == CalcErrorReason.MixedCurrencies:
                return ("mixed currencies", "error", p.Error, "");
            case NoteMode.Timer when line > 1:
                {
                    if (!_analysis.TimerKeys.TryGetValue(line, out var key)) return null;
                    if (_timers.IsDone(key)) return ("done", "timer:done", p.Confirm, key);
                    if (_timers.Get(key) is not { } t) return null;
                    // The kind carries the icon ResultRenderer draws before the time (drawn, not font glyphs).
                    if (t.Done) return ("done", "timer:done", p.Confirm, t.Key);
                    if (t.Running) return (Timers.Format(t.Remaining), "timer:running", p.Number, t.Key);
                    if (t.Paused is not null) return (Timers.Format(t.Remaining), "timer:paused", p.Muted, t.Key);
                    return (Timers.Format(t.Duration), "timer:ready", p.Muted, t.Key);
                }
        }
        return null;
    }

    private void OnViewPressed(object? sender, PointerPressedEventArgs e)
    {
        var view = Editor.TextArea.TextView;
        var point = e.GetPosition(view);
        var chip = _results.Chips.FirstOrDefault(c => c.Kind != "error" && c.Bounds.Contains(point));
        if (chip.Bounds.Width <= 0) return;
        e.Handled = true;
        if (chip.Kind.StartsWith("timer"))
        {
            if (_timers.Get(chip.Value) is { } t) _timers.Toggle(t);
            else ResetDoneTimer(chip.Value);
        }
        else { Copy?.Invoke(chip.Value); Toast?.Invoke($"Copied {chip.Value}"); }
        view.InvalidateLayer(KnownLayer.Selection);
    }

    private void OnViewMoved(object? sender, PointerEventArgs e)
    {
        var view = Editor.TextArea.TextView;
        var point = e.GetPosition(view);
        var over = _results.Chips.Any(c => c.Kind != "error" && c.Bounds.Contains(point));
        view.Cursor = over ? new Cursor(StandardCursorType.Hand) : new Cursor(StandardCursorType.Ibeam);
        ToolTip.SetTip(view, over ? (Mode == NoteMode.Timer ? "Start / pause" : "Click to copy") : null);
    }

    private void OnCaretMoved()
    {
        // Links expand on the caret line; re-render the lines the caret left and entered.
        // Links expand and markdown markers reappear on the caret line; re-render when the caret changes line.
        if (CaretLine != _lastCaretLine) { _lastCaretLine = CaretLine; if (Settings.ShortenLinks || Settings.CleanMarkdown) Redraw(); }
    }

    public void ToggleCheckbox(int lineNumber)
    {
        var doc = Editor.Document;
        if (lineNumber < 1 || lineNumber > doc.LineCount) return;
        var line = doc.GetLineByNumber(lineNumber);
        var text = doc.GetText(line);
        var m = CheckboxGenerator.Box.Match(text);
        if (m.Success)
        {
            var at = line.Offset + m.Groups[2].Index;
            doc.Replace(at, 1, m.Groups[2].Value == " " ? "x" : " ");
        }
        else
        {
            var indent = text.Length - text.TrimStart().Length;
            var bullet = Regex.Match(text, @"^\s*[-*+]\s+");
            if (bullet.Success) doc.Replace(line.Offset + indent, bullet.Length - indent, "[ ] ");
            else doc.Insert(line.Offset + indent, "[ ] ");
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        if (ctrl && shift && e.Key is Key.OemOpenBrackets or Key.OemCloseBrackets)
        {
            e.Handled = FoldAtCaret(e.Key == Key.OemOpenBrackets);
            return;
        }
        if (e.Key == Key.Enter && ctrl && !shift)
        {
            e.Handled = true;
            if (Mode == NoteMode.Timer) ToggleTimerAtCaret(); else ToggleCheckbox(CaretLine);
            return;
        }
        if (e.Key == Key.Enter && !ctrl && !shift && e.KeyModifiers == KeyModifiers.None && Editor.TextArea.Selection.IsEmpty && !Completion.IsOpen)
        {
            bool continued;
            using (Editor.Document.RunUpdate()) { continued = ContinueList(); if (continued) Renumber(CaretLine); }
            if (continued) { e.Handled = true; return; }
            if (Mode == NoteMode.Timer) StartTimerOnLine(CaretLine);
        }
        if (e.Key == Key.Tab && Editor.TextArea.Selection.IsEmpty && IsListLine(CaretLine))
        {
            e.Handled = true;
            using (Editor.Document.RunUpdate()) { Indent(CaretLine, !shift); Renumber(CaretLine, fresh: !shift); }
        }
    }

    private void ToggleTimerAtCaret()
    {
        var text = Editor.Document.GetText(Editor.Document.GetLineByNumber(CaretLine));
        if (Timers.TryParse(text, out var d, out var label) && _analysis.TimerKeys.TryGetValue(CaretLine, out var key))
        {
            if (!ResetDoneTimer(key)) _timers.Toggle(_timers.Ensure(key, d, label, Note?.Id ?? ""));
        }
        Editor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
    }

    private bool ResetDoneTimer(string key)
    {
        if (!_timers.ClearDone(key)) return false;
        var line = _analysis.TimerKeys.FirstOrDefault(pair => pair.Value == key).Key;
        if (line > 1)
        {
            var text = Editor.Document.GetText(Editor.Document.GetLineByNumber(line));
            if (Timers.TryParse(text, out var duration, out var label)) _timers.Ensure(key, duration, label, Note?.Id ?? "");
        }
        return true;
    }

    private void StartTimerOnLine(int lineNumber)
    {
        if (lineNumber <= 1) return;
        var text = Editor.Document.GetText(Editor.Document.GetLineByNumber(lineNumber));
        if (Timers.TryParse(text, out var d, out var label) && _analysis.TimerKeys.TryGetValue(lineNumber, out var key) && !_timers.IsDone(key))
        {
            var entry = _timers.Ensure(key, d, label, Note?.Id ?? "");
            if (entry.EndsAt is null) { _timers.Start(entry); Toast?.Invoke($"Timer started · {Timers.Format(d)}{(label.Length > 0 ? " · " + label : "")}"); }
        }
    }

    /// <summary>Right-click: the standard editing commands, then the pad's own.</summary>
    private void RequestMenu()
    {
        var area = Editor.TextArea;
        var hasSelection = !area.Selection.IsEmpty;
        var items = new List<MenuEntry>
        {
            new("Undo", "Ctrl+Z", MenuGlyphs.Undo, () => Editor.Undo(), Editor.CanUndo),
            new("Redo", "Ctrl+Y", MenuGlyphs.Redo, () => Editor.Redo(), Editor.CanRedo),
            new("Select all", "Ctrl+A", MenuGlyphs.SelectAll, () => Editor.SelectAll()),
            MenuEntry.Separator,
            new("Cut", "Ctrl+X", MenuGlyphs.Cut, () => Editor.Cut(), hasSelection),
            new("Copy", "Ctrl+C", MenuGlyphs.Copy, () => Editor.Copy(), hasSelection),
            new("Paste", "Ctrl+V", MenuGlyphs.Paste, () => Editor.Paste()),
            new("Delete", "Delete", MenuGlyphs.Delete, () => area.Selection.ReplaceSelectionWithText(string.Empty), hasSelection),
        };
        if (AppMenu?.Invoke() is { } extra)
        {
            items.Add(MenuEntry.Separator);
            items.AddRange(extra);
        }
        ShowMenu?.Invoke(Editor, items);
    }

    private bool IsListLine(int lineNumber)
    {
        var m = ListPrefix.Match(Editor.Document.GetText(Editor.Document.GetLineByNumber(lineNumber)));
        return m.Success && (!m.Groups[3].Success || TryListNumber(m, out _));
    }

    internal static bool TryListNumber(string line, out int number) => TryListNumber(ListPrefix.Match(line), out number);

    private static bool TryListNumber(Match match, out int number)
    {
        number = 0;
        var digits = match.Groups[3];
        return digits.Success && digits.Length <= 9
            && int.TryParse(digits.Value, NumberStyles.None, CultureInfo.InvariantCulture, out number);
    }

    private void Indent(int lineNumber, bool deeper)
    {
        var doc = Editor.Document;
        var line = doc.GetLineByNumber(lineNumber);
        if (deeper) doc.Insert(line.Offset, "  ");
        else
        {
            var text = doc.GetText(line);
            var n = text.Length - text.TrimStart(' ').Length;
            if (n > 0) doc.Remove(line.Offset, Math.Min(2, n));
        }
    }

    /// <summary>
    /// Numbers each level of the list around a line in order: 1. / two nested items / then 2. again, not 4.
    /// A level keeps the number its first item starts at (a list may start at 5), except a line just indented to
    /// open a new level (<paramref name="fresh"/>), which starts at 1. Only runs on Return and Tab, never on typing.
    /// </summary>
    private void Renumber(int lineNumber, bool fresh = false)
    {
        var doc = Editor.Document;
        string Text(int n) => doc.GetText(doc.GetLineByNumber(n));
        // A list block: list items and their indented text, up to a blank or unindented non-list line.
        bool InBlock(int n) { var t = Text(n); return t.Trim().Length > 0 && (ListPrefix.IsMatch(t) || t.StartsWith(' ')); }
        if (lineNumber < 1 || lineNumber > doc.LineCount || !InBlock(lineNumber)) return;
        int first = lineNumber, last = lineNumber;
        while (first > 1 && InBlock(first - 1)) first--;
        while (last < doc.LineCount && InBlock(last + 1)) last++;
        var counts = new Dictionary<int, int>();
        for (var n = first; n <= last; n++)
        {
            var m = ListPrefix.Match(Text(n));
            if (!m.Success) continue;
            var indent = m.Groups[1].Length;
            foreach (var deeper in counts.Keys.Where(k => k > indent).ToList()) counts.Remove(deeper);
            if (!m.Groups[3].Success) { counts.Remove(indent); continue; }
            if (!TryListNumber(m, out var own)) { counts.Remove(indent); continue; }
            if (counts.TryGetValue(indent, out var previous) && previous == 999_999_999)
            { counts.Remove(indent); continue; }
            var want = counts.TryGetValue(indent, out var before) ? before + 1 : fresh && n == lineNumber ? 1 : own;
            counts[indent] = want;
            if (want != own) doc.Replace(doc.GetLineByNumber(n).Offset + m.Groups[3].Index, m.Groups[3].Length, want.ToString(CultureInfo.InvariantCulture));
        }
    }

    /// <summary>Return inside a list continues it; Return on an empty item ends the list. List mode adds boxes everywhere.</summary>
    private bool ContinueList()
    {
        var doc = Editor.Document;
        var line = doc.GetLineByNumber(CaretLine);
        var text = doc.GetText(line);
        if (Mode == NoteMode.Code) return false;
        var m = ListPrefix.Match(text);
        if (!m.Success)
        {
            if (Mode == NoteMode.List && (line.LineNumber == 1 || text.Trim().Length > 0))
            {
                Insert("\n[ ] ");
                return true;
            }
            return false;
        }
        if (m.Groups[3].Success && (!TryListNumber(m, out var ownNumber) || ownNumber == 999_999_999)) return false;
        var indent = m.Groups[1].Value;
        var prefix = m.Groups[2].Value;
        if (text.Trim() == prefix.Trim())
        {
            // Empty item: end the list (or outdent one level first).
            if (indent.Length >= 2) { doc.Remove(line.Offset, 2); Renumber(line.LineNumber); return true; }
            doc.Replace(line.Offset, line.Length, "");
            if (Mode == NoteMode.List) return true;
            return true;
        }
        string next;
        if (m.Groups[3].Success)
        {
            TryListNumber(m, out var nextNumber);
            next = $"{nextNumber + 1}{m.Groups[4].Value} ";
        }
        else if (prefix.Contains('[')) next = Regex.Replace(prefix, @"\[[ xX]\]", "[ ]");
        else next = prefix;
        if (!next.EndsWith(' ')) next += " ";
        Insert("\n" + indent + next);
        return true;
    }

    private void Insert(string text)
    {
        Editor.Document.Insert(Editor.CaretOffset, text);
        Editor.TextArea.Caret.BringCaretToView();
    }

    private void UpdateFolds(string[] lines)
    {
        if (_folding is null || Structure is null) return;
        try { _folding.UpdateFoldings(Structure.Folds(Editor.Document, lines, Mode == NoteMode.Code), -1); }
        catch (ArgumentException) { /* a fold boundary moved under an edit; the next pass fixes it */ }
    }

    /// <summary>Fold chevrons in the left gutter, palette-coloured; the gutter replaces part of the text margin.</summary>
    private void StyleFoldingMargin()
    {
        var p = Palette;
        FoldingElementGenerator.TextBrush = new SolidColorBrush(p.Muted);
        ApplyMargins();
    }

    /// <summary>
    /// Text sits 36 px from the left (the fold gutter takes 16 of those) and 30 px from the right. With a text width set,
    /// a pad wider than that many characters adds the spare room to both sides, so the column stays centred.
    /// The paper pattern, code panels and bubbles follow the text edges, so they centre with it.
    /// </summary>
    private void ApplyMargins()
    {
        double left = 36, right = 30;
        if (Settings.TextWidth > 0 && Bounds.Width > 0)
        {
            var glyph = new FormattedText("0", System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(Editor.FontFamily), Settings.FontSize, null);
            // Room for the editor's own scrollbar and rounding, so a full-width line doesn't wrap one character early.
            var spare = Bounds.Width - left - right - (glyph.WidthIncludingTrailingWhitespace * Settings.TextWidth + 15);
            if (spare > 0) { left += spare / 2; right += spare / 2; }
        }
        // Docked keeps text above the reserved pager strip; sliding bars and private previews keep their current inset.
        var bottom = Private ? 6 : Settings.Bars is "overlay" or "float" ? 10 : ChromeTokens.PagerStripHeight;
        Editor.Margin = new Thickness(_gutter is not null ? left - 16 : left, 4, right, bottom);
        if (Editor is ScrollableTopInsetTextEditor insetEditor) UpdateTopInset(insetEditor);
        UpdateEdgeFade();
        _hint.Margin = new Thickness(left + 1, 7, right, 0);
    }

    private void UpdateTopInset(ScrollableTopInsetTextEditor editor) =>
        editor.InsetTextArea.TopInset = !Private && Settings.Bars is "overlay" or "float"
            ? ChromeTokens.TopScrollableWhitespace : 0;

    /// <summary>Ctrl+Shift+[ folds the innermost block around the caret; Ctrl+Shift+] unfolds it.</summary>
    public bool FoldAtCaret(bool fold)
    {
        if (_folding is null) return false;
        var line = Editor.Document.GetLineByOffset(Editor.CaretOffset);
        var target = _folding.AllFoldings.Where(f => f.StartOffset <= line.EndOffset && f.EndOffset >= line.Offset)
            .OrderByDescending(f => f.StartOffset).FirstOrDefault(f => f.IsFolded != fold);
        if (target is null) return false;
        target.IsFolded = fold;
        return true;
    }

    private void ApplyCodeHighlighting()
    {
        if (Mode != NoteMode.Code) { Editor.SyntaxHighlighting = null; return; }
        var definition = CodeColors.Definition(CodeLanguage, Palette) ?? CodeColors.Definition("py", Palette);
        if (definition is null) { Editor.SyntaxHighlighting = null; return; }
        Editor.SyntaxHighlighting = null;
        Editor.SyntaxHighlighting = definition;
    }

    private void OnTextEntered(object? sender, TextInputEventArgs e) => Completion.OnTextEntered(this, e.Text);

    public void RaiseToast(string text) => Toast?.Invoke(text);
}

/// <summary>Cached per-document analysis shared by the pad and headless selftests.</summary>
internal sealed class NoteAnalysis
{
    private sealed record CachedLine(string Text);
    private sealed record TimerLine(TimeSpan Duration, string Label, string DurationText);

    private ConditionalWeakTable<DocumentLine, CachedLine> _lineText = new();
    private TextDocument? _document;
    private int _previousLineCount;
    private bool _hasProcessed;
    private bool _previousInlineMath;
    private TimerLine?[] _timerLines = [];
    private readonly Dictionary<int, LineEval> _inlineLines = [];
    private readonly Dictionary<int, string> _timerKeys = [];
    private IReadOnlyList<LineEval?> _mathLines = [];

    public NoteMode Mode { get; private set; }
    public string? CodeLanguage { get; private set; }
    public int ModeKeywordLength { get; private set; }
    public NoteStructure? Structure { get; private set; }
    public string[] Lines { get; private set; } = [];
    public IReadOnlyList<LineEval?> MathLines => _mathLines;
    public IReadOnlyDictionary<int, LineEval> InlineLines => _inlineLines;
    public IReadOnlyDictionary<int, string> TimerKeys => _timerKeys;
    /// <summary>DocumentLine.GetText calls performed by the most recent update.</summary>
    public int LastCopiedLineCount { get; private set; }
    /// <summary>Changed structure signatures and inline, math, or timer lines processed by the most recent update.</summary>
    public int LastAnalysedLineCount { get; private set; }

    /// <summary>Invalidate text for lines changed by a document event. Line objects keep cache identity across shifts.</summary>
    public void Invalidate(TextDocument document, int firstChanged, int lastChanged)
    {
        if (!ReferenceEquals(document, _document)) return;
        if (document.LineCount == 0) return;
        firstChanged = Math.Clamp(firstChanged, 0, document.LineCount - 1);
        lastChanged = Math.Clamp(lastChanged, firstChanged, document.LineCount - 1);
        for (var i = firstChanged; i <= lastChanged; i++)
            _lineText.Remove(document.GetLineByNumber(i + 1));
    }

    /// <summary>Drop document and derived state. The next update is a full analysis and text snapshot.</summary>
    public void Reset()
    {
        _lineText = new ConditionalWeakTable<DocumentLine, CachedLine>();
        _document = null;
        _previousLineCount = 0;
        _hasProcessed = false;
        _previousInlineMath = false;
        _timerLines = [];
        _inlineLines.Clear();
        _timerKeys.Clear();
        _mathLines = [];
        Structure = null;
        Lines = [];
        Mode = NoteMode.Plain;
        CodeLanguage = null;
        ModeKeywordLength = 0;
        LastCopiedLineCount = 0;
        LastAnalysedLineCount = 0;
    }

    public void Update(TextDocument document, int firstChanged, int lastChanged, bool inlineMath,
        string? noteId, Timers? timers, bool forceFull = false)
    {
        var newDocument = !ReferenceEquals(document, _document);
        if (newDocument)
        {
            Reset();
            _document = document;
            forceFull = true;
        }

        var oldMode = Mode;
        var oldInlineMath = _previousInlineMath;
        var previousCount = _previousLineCount;
        LastCopiedLineCount = 0;
        Lines = ReadLines(document);
        if (Lines.Length == 0) Lines = [string.Empty];

        var detected = Modes.Detect(Lines[0]);
        Mode = detected.Mode;
        CodeLanguage = detected.Argument;
        ModeKeywordLength = detected.KeywordLength;
        var lineCountDelta = Lines.Length - previousCount;
        var structuralModeChanged = _hasProcessed && oldMode != Mode;
        var inlineSettingChanged = _hasProcessed && oldInlineMath != inlineMath;

        if (!_hasProcessed || forceFull || firstChanged < 0 || lastChanged < firstChanged
            || lastChanged >= Lines.Length || previousCount == 0)
        {
            forceFull = true;
            firstChanged = 0;
            lastChanged = Lines.Length - 1;
        }
        else
        {
            var oldLastChanged = lastChanged - lineCountDelta;
            if (oldLastChanged < firstChanged - 1 || oldLastChanged >= previousCount)
            {
                forceFull = true;
                firstChanged = 0;
                lastChanged = Lines.Length - 1;
            }
        }

        var structuralWork = 0;
        if (forceFull || Structure is null)
        {
            Structure = NoteStructure.Build(Lines);
            structuralWork = Structure.LastAnalysedLineCount;
        }
        else
        {
            Structure = Structure.Update(Lines, firstChanged, lastChanged);
            structuralWork = Structure.LastAnalysedLineCount;
        }

        var inlineWork = 0;
        if (Mode == NoteMode.Plain && inlineMath)
        {
            var fullInline = forceFull || structuralModeChanged || inlineSettingChanged || oldMode != NoteMode.Plain;
            if (fullInline) _inlineLines.Clear();
            else ShiftInlineLines(firstChanged, lastChanged, lineCountDelta);

            var from = fullInline ? 0 : firstChanged;
            var to = fullInline ? Lines.Length - 1 : lastChanged;
            for (var i = from; i <= to; i++)
            {
                _inlineLines.Remove(i + 1);
                inlineWork++;
                if (!Lines[i].TrimEnd().EndsWith('=')) continue;
                var marked = PadView.StripListMarker(Lines[i]);
                if ((Calculator.EvaluateInlineLine(marked) ?? Calculator.EvaluateInlineLine(PadView.BlankLabel(marked))) is { } result
                    && (result.Result is not null || result.ErrorReason == CalcErrorReason.MixedCurrencies))
                    _inlineLines[i + 1] = result;
            }
        }
        else _inlineLines.Clear();

        if (Mode == NoteMode.Math)
        {
            // Variables and prose-label retry semantics can affect later lines, so math deliberately keeps the full pass.
            _mathLines = PadView.EvaluateMath(Lines);
        }
        else _mathLines = [];

        var timerWork = UpdateTimers(firstChanged, lastChanged, lineCountDelta, forceFull || oldMode != NoteMode.Timer,
            noteId, timers);
        LastAnalysedLineCount = structuralWork + inlineWork + timerWork + (Mode == NoteMode.Math ? Lines.Length : 0);
        _previousLineCount = Lines.Length;
        _previousInlineMath = inlineMath;
        _hasProcessed = true;
    }

    private string[] ReadLines(TextDocument document)
    {
        var lines = new string[document.LineCount];
        for (var i = 0; i < lines.Length; i++)
        {
            var documentLine = document.GetLineByNumber(i + 1);
            if (!_lineText.TryGetValue(documentLine, out var cached))
            {
                cached = new CachedLine(document.GetText(documentLine));
                _lineText.Add(documentLine, cached);
                LastCopiedLineCount++;
            }
            lines[i] = cached.Text;
        }
        return lines;
    }

    private void ShiftInlineLines(int firstChanged, int lastChanged, int lineCountDelta)
    {
        var oldLastChanged = lastChanged - lineCountDelta;
        if (lineCountDelta == 0)
        {
            for (var line = firstChanged; line <= lastChanged; line++) _inlineLines.Remove(line + 1);
            return;
        }
        var shifted = new Dictionary<int, LineEval>();
        foreach (var pair in _inlineLines.ToArray())
        {
            var zeroBased = pair.Key - 1;
            if (zeroBased >= firstChanged && zeroBased <= oldLastChanged) continue;
            var shiftedLine = zeroBased > oldLastChanged ? zeroBased + lineCountDelta : zeroBased;
            if (shiftedLine >= 0 && shiftedLine < Lines.Length) shifted[shiftedLine + 1] = pair.Value;
        }
        _inlineLines.Clear();
        foreach (var pair in shifted) _inlineLines[pair.Key] = pair.Value;
    }

    private int UpdateTimers(int firstChanged, int lastChanged, int lineCountDelta, bool forceFull,
        string? noteId, Timers? timers)
    {
        _timerKeys.Clear();
        var hasTimerNote = Mode == NoteMode.Timer;
        if (!hasTimerNote)
        {
            _timerLines = [];
            if (noteId is not null) timers?.Reconcile(noteId, Array.Empty<string>());
            return 0;
        }

        var oldCount = _timerLines.Length;
        var next = new TimerLine?[Lines.Length];
        var parsed = 0;

        if (!forceFull && oldCount > 0)
        {
            var oldLastChanged = lastChanged - lineCountDelta;
            if (oldLastChanged >= firstChanged - 1 && oldLastChanged < oldCount)
            {
                var prefixEnd = Math.Min(firstChanged, Math.Min(oldCount, next.Length));
                if (prefixEnd > 0) Array.Copy(_timerLines, 0, next, 0, prefixEnd);
                for (var oldIndex = Math.Max(0, oldLastChanged + 1); oldIndex < oldCount; oldIndex++)
                {
                    var newIndex = oldIndex + lineCountDelta;
                    if (newIndex >= 0 && newIndex < next.Length) next[newIndex] = _timerLines[oldIndex];
                }
            }
            else forceFull = true;
        }

        var from = forceFull ? 1 : Math.Max(1, firstChanged);
        var to = forceFull ? Lines.Length - 1 : Math.Min(Lines.Length - 1, lastChanged);
        for (var i = from; i <= to; i++)
        {
            parsed++;
            if (Timers.TryParse(Lines[i], out var duration, out var label, out var durationText))
                next[i] = new TimerLine(duration, label, durationText);
        }

        _timerLines = next;
        var timerKeys = new HashSet<string>(StringComparer.Ordinal);
        if (noteId is not null && hasTimerNote)
        {
            var ordinals = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 1; i < _timerLines.Length; i++)
            {
                if (_timerLines[i] is not { } timerLine) continue;
                var ordinal = ordinals[timerLine.DurationText] = ordinals.GetValueOrDefault(timerLine.DurationText) + 1;
                var key = TimerKey(noteId, timerLine.DurationText, ordinal);
                _timerKeys[i + 1] = key;
                timerKeys.Add(key);
                if (timers is not null && !timers.IsDone(key))
                    timers.Ensure(key, timerLine.Duration, timerLine.Label, noteId);
            }
        }

        if (noteId is not null) timers?.Reconcile(noteId, timerKeys);
        var keyLinesExamined = noteId is not null ? Math.Max(0, Lines.Length - 1) : 0;
        return Math.Max(parsed, keyLinesExamined);
    }

    internal static string TimerKey(string noteId, string durationText, int ordinal) => $"{noteId}:{durationText}:{ordinal}";
}
