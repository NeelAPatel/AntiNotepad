// One note editor, its cached document, decorations, and interruptible note transitions.
using Avalonia.Input.Platform;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
using AntiNotepad.UI;

namespace AntiNotepad.Editor;

/// <summary>One editing pane bound to one note. Documents are cached per note so each note keeps its own undo history.</summary>
internal sealed class PadView : Grid
{
    private static readonly Dictionary<string, TextDocument> Documents = [];

    private readonly Timers _timers;
    private readonly INotesLocation _notesLocation;
    private Action? _timerTick;
    private readonly TextBlock _hint;
    private readonly ResultRenderer _results;
    private readonly SmoothCaret _caret;
    private readonly SmoothScroll _smoothScroll;
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
    private Grid? _motionContent;
    private Grid MotionContent => _motionContent ?? this;
    private Anim.Handle? _departure;
    private Image? _departureImage;
    private RenderTargetBitmap? _departureBitmap;
    private (double X, double Opacity, double Velocity)? _swipeStart;
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

    public PadView(Palette palette, Settings settings, Timers timers, INotesLocation notesLocation)
    {
        Palette = palette; Settings = settings; _timers = timers; _notesLocation = notesLocation;
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
        WrapIndent.Configure(view);
        view.BackgroundRenderers.Add(new IndentGuideRenderer(this));
        _results = new ResultRenderer(this);
        view.BackgroundRenderers.Add(_results);
        view.BackgroundRenderers.Add(new CheckboxRenderer(this));
        view.BackgroundRenderers.Add(new DividerRenderer(this));
        _caret = new SmoothCaret(this);
        view.BackgroundRenderers.Add(_caret);
        _smoothScroll = new SmoothScroll(Editor, () => Settings);
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
        DetachedFromVisualTree += (_, _) => StopNoteMotion();
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
        Editor.FontFamily = new FontFamily(settings.Font + Typography.EditorFallback);
        Editor.FontSize = settings.FontSize;
        Editor.Foreground = new SolidColorBrush(palette.Ink);
        Editor.TextArea.SelectionBrush = new SolidColorBrush(palette.Selection);
        Editor.TextArea.SelectionForeground = null;
        Editor.TextArea.SelectionBorder = null;
        Editor.TextArea.SelectionCornerRadius = 3;
        // SmoothCaret draws the caret; the built-in one stays (for IME placement) but invisible.
        Editor.TextArea.Caret.CaretBrush = Brushes.Transparent;
        _hint.FontFamily = Editor.FontFamily; _hint.FontSize = settings.FontSize;
        _hint.Foreground = _loadHintError ? new SolidColorBrush(palette.Error, Paint.ErrorHint) : new SolidColorBrush(palette.Faint);
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

    /// <summary>Stops obsolete motion and binds a note while preserving its cached undo history.</summary>
    public void Bind(Note note)
    {
        StopNoteMotion();
        if (ReferenceEquals(Note, note)) return;
        _smoothScroll.Stop();
        Flush();
        _binding = true;
        if (Editor.Document is { } old) old.Changed -= OnDocumentChanged;
        _analyse.Stop();
        _analysis.Reset();
        ClearPendingAnalysis();
        Note = note;
        var locationError = _notesLocation.NotesLocationError;
        if (locationError is null) _ = note.PeekText();
        var unavailable = locationError is not null || note.BodyUnavailable;
        _loadHintError = unavailable;
        _loadHint = locationError is not null
            ? $"Error: Notes folder not found\n\n{_notesLocation.DataFolder}\n\nChoose a folder in Settings › Storage"
            : note.BodyUnavailable ? "Error: Note not found\n\nCheck recovered folder in Settings › Storage" : null;
        _hint.Foreground = _loadHintError ? new SolidColorBrush(Palette.Error, Paint.ErrorHint) : new SolidColorBrush(Palette.Faint);
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
        StopNoteMotion();
        _smoothScroll.Stop();
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

    /// <summary>Remembers the committed pull for the next bind, without delaying model navigation.</summary>
    internal void PrepareSwipe(double x, double opacity, double velocity)
    {
        StopNoteMotion();
        _swipeStart = (x, opacity, velocity);
    }

    /// <summary>A boundary bump may not bind a note, so never retain its pull for a later command.</summary>
    internal void DiscardSwipe() => _swipeStart = null;

    /// <summary>Binds immediately while the frozen old page departs and the new content arrives.</summary>
    public void SwitchTo(Note note, int direction)
    {
        // capture -> bind -> transition
        var start = _swipeStart;
        _swipeStart = null;
        StopNoteMotion();
        if (start is not null) EnsureMotionContent();
        RenderTargetBitmap? bitmap = null;
        if (direction != 0 && start is not null && !Anim.Reduced
            && Bounds.Width > 0 && Bounds.Height > 0 && TopLevel.GetTopLevel(this) is { } top)
        {
            var scale = top.RenderScaling;
            try
            {
                bitmap = new RenderTargetBitmap(PixelSize.FromSize(Bounds.Size, scale), new Vector(96 * scale, 96 * scale));
                bitmap.Render(MotionContent);
            }
            catch (Exception ex) when (ex is NotSupportedException or InvalidOperationException)
            {
                bitmap?.Dispose();
                bitmap = null;
            }
        }

        // bind
        try { Bind(note); }
        catch { bitmap?.Dispose(); throw; }
        if (direction == 0) { bitmap?.Dispose(); return; }

        // transition
        if (bitmap is null)
        {
            if (start is { } pending && !Anim.Reduced) SpringSwipe(direction, pending, null, null);
            else Arrive(direction);
            return;
        }
        var pull = start!.Value;
        _departureBitmap = bitmap;
        var shift = new TranslateTransform(pull.X, 0);
        var image = new Image
        {
            Source = bitmap,
            Stretch = Stretch.Fill,
            IsHitTestVisible = false,
            RenderTransform = shift,
            Opacity = pull.Opacity
        };
        _departureImage = image;
        Children.Add(image);
        SpringSwipe(direction, pull, image, shift);
    }

    /// <summary>Moves the departing and arriving pages with the same release-speed spring.</summary>
    private void SpringSwipe(int direction, (double X, double Opacity, double Velocity) pull, Image? image, TranslateTransform? departure)
    {
        var width = Math.Max(1, Bounds.Width);
        var leaving = pull.X;
        var leavingVelocity = pull.Velocity * 1000;
        var leavingTarget = -direction * (width + MotionTokens.HorizontalPageGap);
        var arriving = direction * width;
        var arrivingVelocity = pull.Velocity * 1000;
        var shift = new TranslateTransform(arriving, 0);
        MotionContent.RenderTransform = shift;
        MotionContent.Opacity = 1;
        if (image is not null) image.Opacity = pull.Opacity;

        if (TopLevel.GetTopLevel(this) is not { } top)
        {
            if (departure is not null) departure.X = leavingTarget;
            shift.X = 0;
            MotionContent.RenderTransform = null;
            ClearDeparture();
            return;
        }

        var handle = new Anim.Handle();
        _departure = _arrive = handle;
        TimeSpan? last = null;
        void Frame(TimeSpan time)
        {
            if (handle.Cancelled) return;
            var dt = last is { } previous ? Math.Clamp((time - previous).TotalSeconds, 0, 0.05) : 1 / 60.0;
            last = time;
            leaving = EdgeBand.SpringStep(leaving, ref leavingVelocity, leavingTarget, dt);
            arriving = EdgeBand.SpringStep(arriving, ref arrivingVelocity, 0, dt);
            if (departure is not null) departure.X = leaving;
            shift.X = arriving;
            if (Math.Abs(leaving - leavingTarget) < 0.3 && Math.Abs(leavingVelocity) < 10
                && Math.Abs(arriving) < 0.3 && Math.Abs(arrivingVelocity) < 10)
            {
                if (departure is not null) departure.X = leavingTarget;
                shift.X = 0;
                MotionContent.RenderTransform = null;
                MotionContent.Opacity = 1;
                _departure = _arrive = null;
                ClearDeparture();
                return;
            }
            top.RequestAnimationFrame(Frame);
        }
        top.RequestAnimationFrame(Frame);
    }

    /// <summary>Adds a stationary overlay surface for the departing page.</summary>
    private void EnsureMotionContent()
    {
        if (_motionContent is not null) return;
        _motionContent = new Grid();
        foreach (var child in Children.ToArray())
        {
            Children.Remove(child);
            _motionContent.Children.Add(child);
        }
        Children.Add(_motionContent);
        _motionContent.Measure(Bounds.Size);
        _motionContent.Arrange(new Rect(Bounds.Size));
    }

    /// <summary>Stops visual work before another gesture or bind; the selected note is already bound.</summary>
    internal void StopNoteMotion()
    {
        _swipeStart = null;
        _arrive?.Cancel();
        _arrive = null;
        _departure?.Cancel();
        _departure = null;
        ClearDeparture();
        RenderTransform = null;
        Opacity = 1;
        MotionContent.RenderTransform = null;
        MotionContent.Opacity = 1;
    }

    /// <summary>Releases the frozen page as soon as it leaves or is interrupted.</summary>
    private void ClearDeparture()
    {
        if (_departureImage is { } image) { Children.Remove(image); image.Source = null; }
        _departureImage = null;
        _departureBitmap?.Dispose();
        _departureBitmap = null;
    }

    /// <summary>Slides the new content into place while keeping a departure image stationary in the pane.</summary>
    public void Arrive(int direction)
    {
        _arrive?.Cancel();
        var shift = new TranslateTransform();
        MotionContent.RenderTransform = shift;
        var x = direction * MotionTokens.NoteArrivalShift;
        var alpha = MotionTokens.NoteArrivalOpacity;
        _arrive = Anim.Run(this, MotionTokens.NoteArrive, t =>
        {
            shift.X = x * (1 - t);
            MotionContent.Opacity = alpha + (1 - alpha) * t;
        }, () => { MotionContent.RenderTransform = null; MotionContent.Opacity = 1; });
    }

    public void FocusText() => Editor.TextArea.Focus();

    private void OnDocumentChanged(object? sender, DocumentChangeEventArgs e)
    {
        if (_binding) return;
        var doc = Editor.Document;
        var (first, last) = NoteAnalysis.ChangedLineRange(doc, e);
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

    private void Redraw() => Editor.TextArea.TextView.Redraw();

    public bool InFence(int line) => Structure is { } s && line < s.InFence.Length && s.InFence[line];
    public LineEval? MathLine(int line) => line - 1 < _analysis.MathLines.Count ? _analysis.MathLines[line - 1] : null;
    public LineEval? InlineLine(int line) => _analysis.InlineLines.GetValueOrDefault(line);
    public CalcResult? InlineResult(int line) => InlineLine(line)?.Result;

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
        if (TryPressLink(GlyphAt(view, point), e)) return;
        if (TryPressUrl(PlainUrlAt(view, point), e, Platform.DesktopPlatform.OpenUrl)) return;
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
        view.Cursor = over || GlyphAt(view, point) is { Hand: true }
            || (e.KeyModifiers.HasFlag(KeyModifiers.Control) && PlainUrlAt(view, point) is not null)
            ? new Cursor(StandardCursorType.Hand) : new Cursor(StandardCursorType.Ibeam);
        ToolTip.SetTip(view, over ? (Mode == NoteMode.Timer ? "Start / pause" : "Click to copy") : null);
    }

    internal static GlyphElement? GlyphAt(TextView view, Point point)
    {
        if (!view.VisualLinesValid || !new Rect(view.Bounds.Size).Contains(point)) return null;
        var position = point + view.ScrollOffset;
        var line = view.GetVisualLineFromVisualTop(position.Y);
        if (line is null) return null;
        foreach (var glyph in line.Elements.OfType<GlyphElement>())
        {
            // A replacement glyph is indivisible, including when the surrounding text wraps.
            var textLine = line.GetTextLine(glyph.VisualColumn);
            var left = line.GetTextLineVisualXPosition(textLine, glyph.VisualColumn);
            var right = line.GetTextLineVisualXPosition(textLine, glyph.VisualColumn + glyph.VisualLength);
            var top = line.GetTextLineVisualYPosition(textLine, VisualYPosition.LineTop);
            var bottom = line.GetTextLineVisualYPosition(textLine, VisualYPosition.LineBottom);
            if (position.X >= left && position.X < right && position.Y >= top && position.Y < bottom) return glyph;
        }
        return null;
    }

    internal static string? PlainUrlAt(TextView view, Point point)
    {
        if (!view.VisualLinesValid || !new Rect(view.Bounds.Size).Contains(point)
            || GlyphAt(view, point) is not null) return null;
        var position = point + view.ScrollOffset;
        var line = view.GetVisualLineFromVisualTop(position.Y);
        if (line is null || position.Y < line.VisualTop || position.Y >= line.VisualTop + line.Height) return null;
        var hit = view.GetPositionFloor(position);
        if (hit is null) return null;
        var textLine = line.GetTextLine(hit.Value.VisualColumn);
        var left = line.GetTextLineVisualXPosition(textLine, textLine.FirstTextSourceIndex);
        var right = line.GetTextLineVisualXPosition(textLine, textLine.FirstTextSourceIndex + textLine.Length);
        if (position.X < left || position.X >= right) return null;
        var doc = view.Document;
        var offset = doc.GetOffset(hit.Value.Location);
        var documentLine = doc.GetLineByOffset(offset);
        foreach (Match match in LinkGenerator.Url.Matches(doc.GetText(documentLine)))
            if (offset >= documentLine.Offset + match.Index && offset < documentLine.Offset + match.Index + match.Length)
                return match.Value;
        return null;
    }

    internal static bool TryPressUrl(string? url, PointerPressedEventArgs e, Action<string> open)
    {
        if (e.Handled || url is null || !e.KeyModifiers.HasFlag(KeyModifiers.Control)
            || !e.GetCurrentPoint(null).Properties.IsLeftButtonPressed) return false;
        e.Handled = true;
        open(url);
        return true;
    }

    internal static bool TryPressLink(GlyphElement? glyph, PointerPressedEventArgs e)
    {
        if (e.Handled || glyph is not { IsLink: true, Pressed: not null }
            || !e.KeyModifiers.HasFlag(KeyModifiers.Control)
            || !e.GetCurrentPoint(null).Properties.IsLeftButtonPressed) return false;
        e.Handled = true;
        glyph.Pressed(e);
        return true;
    }

    private void OnCaretMoved()
    {
        // Links expand on the caret line; re-render the lines the caret left and entered.
        // Links expand and markdown markers reappear on the caret line; re-render when the caret changes line.
        if (CaretLine != _lastCaretLine) { _lastCaretLine = CaretLine; if (Settings.ShortenLinks || Settings.CleanMarkdown) Redraw(); }
    }

    public void ToggleCheckbox(int lineNumber)
        => DocumentCommands.ToggleCheckbox(Editor.Document, lineNumber);

    internal void ToggleCheckboxAtCaret() { if (Mode == NoteMode.Timer) ToggleTimerAtCaret(); else ToggleCheckbox(CaretLine); }
    internal bool FoldAtCaretShortcut(bool collapse) => FoldAtCaret(collapse);

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.V && e.KeyModifiers == KeyModifiers.Control)
        {
            e.Handled = true;
            _ = PasteAsync();
            return;
        }
        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        if (e.Key == Key.Enter && !ctrl && !shift && e.KeyModifiers == KeyModifiers.None && Editor.TextArea.Selection.IsEmpty && !Completion.IsOpen)
        {
            bool continued;
            using (Editor.Document.RunUpdate())
            {
                continued = DocumentCommands.ContinueList(Editor.Document, CaretLine, Mode, Insert, Settings.ListDefault);
                if (continued) DocumentCommands.Renumber(Editor.Document, CaretLine);
            }
            if (continued) { e.Handled = true; return; }
            if (Mode == NoteMode.Timer) StartTimerOnLine(CaretLine);
        }
        if (e.Key is Key.OemOpenBrackets or Key.OemCloseBrackets && e.KeyModifiers == KeyModifiers.Control)
        {
            var selection = Editor.TextArea.Selection;
            var first = selection.IsEmpty ? CaretLine : Editor.Document.GetLineByOffset(selection.SurroundingSegment.Offset).LineNumber;
            var last = selection.IsEmpty ? CaretLine : Editor.Document.GetLineByOffset(selection.SurroundingSegment.EndOffset - 1).LineNumber;
            e.Handled = DocumentCommands.IndentListLines(Editor.Document, first, last, e.Key == Key.OemCloseBrackets);
            return;
        }
        if (e.Key == Key.Tab && Editor.TextArea.Selection.IsEmpty)
        {
            var line = Editor.Document.GetLineByNumber(CaretLine);
            var action = ListEditing.TabDecision(Editor.Document.GetText(line), Editor.CaretOffset - line.Offset,
                shift, ctrl, e.KeyModifiers.HasFlag(KeyModifiers.Alt));
            if (action == ListEditing.TabAction.Default) return;
            e.Handled = true;
            if (action == ListEditing.TabAction.InsertSpaces) Insert("  ");
            else if (action is ListEditing.TabAction.Indent or ListEditing.TabAction.Outdent)
                DocumentCommands.IndentListLines(Editor.Document, CaretLine, CaretLine, action == ListEditing.TabAction.Indent);
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
            new("Undo", "Ctrl+Z", Icons.Undo, () => Editor.Undo(), Editor.CanUndo),
            new("Redo", "Ctrl+Y", Icons.Redo, () => Editor.Redo(), Editor.CanRedo),
            new("Select all", "Ctrl+A", Icons.SelectAll, () => Editor.SelectAll()),
            MenuEntry.Separator,
            new("Cut", "Ctrl+X", Icons.Cut, () => Editor.Cut(), hasSelection),
            new("Copy", "Ctrl+C", Icons.Copy, () => Editor.Copy(), hasSelection),
            new("Paste", "Ctrl+V", Icons.Paste, () => _ = PasteAsync()),
            new("Delete", "Delete", Icons.Delete, () => area.Selection.ReplaceSelectionWithText(string.Empty), hasSelection),
        };
        if (hasSelection)
        {
            var start = Editor.SelectionStart;
            var end = start + Editor.SelectionLength;
            var first = Editor.Document.GetLineByOffset(start).LineNumber;
            var last = Editor.Document.GetLineByOffset(end - 1).LineNumber;
            void Convert(ListEditing.ListKind kind) => DocumentCommands.ConvertToList(Editor.Document, first, last, kind);
            items.Add(ConvertMenu(true, Convert)!);
        }
        if (AppMenu?.Invoke() is { } extra)
        {
            items.Add(MenuEntry.Separator);
            items.AddRange(extra);
        }
        ShowMenu?.Invoke(Editor, items);
    }

    /// <summary>Creates the selection conversion submenu.</summary>
    internal static MenuEntry? ConvertMenu(bool hasSelection, Action<ListEditing.ListKind> convert) => !hasSelection ? null : new("Convert to list", Children:
    [
        new("Checklist", Click: () => convert(ListEditing.ListKind.Checklist)),
        new("Bullet list", Click: () => convert(ListEditing.ListKind.Bullet)),
        new("Numbered list", Click: () => convert(ListEditing.ListKind.Numbered)),
    ]);

    private const string PasteFormattingFound = "Paste Formatting Found";
    private const string PasteFormattingFallback = "Paste Formatting Fallback";

    private readonly SemaphoreSlim _pasteLock = new(1, 1);

    private async Task PasteAsync()
    {
        await _pasteLock.WaitAsync();
        try { await PasteCoreAsync(); }
        finally { _pasteLock.Release(); }
    }

    private async Task PasteCoreAsync()
    {
        var document = Editor.Document;
        var selection = Editor.TextArea.Selection;
        var caret = Editor.CaretOffset;
        var outcome = new PasteOutcome(null, false);
        var preformatted = false;
        try
        {
            if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
            {
                var formats = (await clipboard.GetDataFormatsAsync()).ToArray();
                var plain = await clipboard.TryGetTextAsync();
                var htmlBytes = await clipboard.TryGetValueAsync(DataFormat.CreateBytesPlatformFormat("HTML Format"));
                var html = htmlBytes is null ? null : System.Text.Encoding.UTF8.GetString(htmlBytes).TrimEnd('\0');
                var files = await clipboard.TryGetFilesAsync();
                var snapshot = new ClipSnapshot(plain, html, formats.Select(f => f.Identifier).ToArray(),
                    formats.Contains(DataFormat.Bitmap), files?.Select(f => f.Path.LocalPath).ToArray() ?? []);
                outcome = PasteEngine.Analyze(snapshot);
                preformatted = PasteEngine.IsPreformatted(snapshot);
            }
        }
        catch (Exception) { }
        if (Editor.Document != document || Editor.TextArea.Selection != selection || Editor.CaretOffset != caret) return;
        if (outcome.Text is null)
        {
            Editor.Paste();
            if (outcome.StructureSeen && !preformatted) RaiseToast(PasteFormattingFallback);
        }
        else
        {
            Editor.TextArea.Selection.ReplaceSelectionWithText(outcome.Text);
            RaiseToast(PasteFormattingFound);
        }
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
