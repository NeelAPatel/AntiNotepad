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
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Rendering;
using Antipad.Calc;
using Antipad.Model;
using Antipad.Theme;
using Antipad.UI;

namespace Antipad.Editor;

/// <summary>One editing pane bound to one note. Documents are cached per note so each note keeps its own undo history.</summary>
internal sealed class PadView : Grid
{
    private static readonly Dictionary<string, TextDocument> Documents = [];
    private static readonly Regex ListPrefix = new(@"^(\s*)((?:[-*+]\s+)?\[[ xX]\]\s?|[-*+]\s|(\d+)([.)])\s)", RegexOptions.Compiled);

    private readonly Timers _timers;
    private readonly TextBlock _hint;
    private readonly ResultRenderer _results;
    private IReadOnlyList<LineEval?> _math = [];
    private readonly Dictionary<int, CalcResult> _inline = [];
    private bool[] _fence = [];
    private bool _binding;
    private Anim.Handle? _arrive;
    private int _bindToken;
    private int _lastCaretLine;

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
    /// <summary>Copies go through the window so clipboard-capture notes don't record Antipad's own copies.</summary>
    public event Action<string>? Copy;

    public PadView(Palette palette, Settings settings, Timers timers)
    {
        Palette = palette; Settings = settings; _timers = timers;
        Editor = new TextEditor
        {
            WordWrap = true,
            ShowLineNumbers = false,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            Margin = new Thickness(26, 4, 18, 40),
        };
        Editor.Options.EnableHyperlinks = false;
        Editor.Options.EnableEmailHyperlinks = false;
        Editor.Options.HighlightCurrentLine = false;
        Editor.Options.AllowScrollBelowDocument = false;
        Editor.Options.ShowBoxForControlCharacters = false;
        Editor.Options.IndentationSize = 2;
        Editor.Options.ConvertTabsToSpaces = true;
        Editor.Options.CutCopyWholeLine = true;
        Editor.Options.LineHeightFactor = 1.32;
        Avalonia.Automation.AutomationProperties.SetName(Editor, "Note text");
        var view = Editor.TextArea.TextView;
        view.BackgroundRenderers.Add(new PaperRenderer(this));
        _results = new ResultRenderer(this);
        view.BackgroundRenderers.Add(_results);
        view.BackgroundRenderers.Add(new CheckboxRenderer(this));
        view.LineTransformers.Add(new NoteColorizer(this));
        view.LineTransformers.Add(new MathColorizer(this));
        view.ElementGenerators.Add(new CheckboxGenerator(this));
        view.ElementGenerators.Add(new BulletGenerator(this));
        view.ElementGenerators.Add(new LinkGenerator(this));
        view.ElementGenerators.Add(new MarkerGenerator(this));
        _hint = new TextBlock
        {
            IsHitTestVisible = false, Margin = new Thickness(27, 7, 18, 0), TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left,
        };
        Children.Add(Editor);
        Children.Add(_hint);
        Editor.TextArea.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        Editor.TextArea.TextEntered += OnTextEntered;
        Editor.TextArea.Caret.PositionChanged += (_, _) => OnCaretMoved();
        Editor.TextArea.GotFocus += (_, _) => FocusedPane?.Invoke();
        view.AddHandler(PointerPressedEvent, OnViewPressed, RoutingStrategies.Tunnel);
        view.PointerMoved += OnViewMoved;
        Editor.TemplateApplied += (_, _) => Dispatcher.UIThread.Post(ClearBackgrounds, DispatcherPriority.Loaded);
        Editor.Loaded += (_, _) => ClearBackgrounds();
        _timers.Tick += () => { if (Mode == NoteMode.Timer) view.InvalidateLayer(KnownLayer.Selection); };
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
        Editor.FontFamily = new FontFamily($"{settings.Font}, Cascadia Mono, Consolas, monospace");
        Editor.FontSize = settings.FontSize;
        Editor.Foreground = new SolidColorBrush(palette.Ink);
        Editor.TextArea.SelectionBrush = new SolidColorBrush(palette.Selection);
        Editor.TextArea.SelectionForeground = null;
        Editor.TextArea.SelectionBorder = null;
        Editor.TextArea.SelectionCornerRadius = 3;
        Editor.TextArea.Caret.CaretBrush = new SolidColorBrush(palette.Caret);
        _hint.FontFamily = Editor.FontFamily; _hint.FontSize = settings.FontSize;
        _hint.Foreground = new SolidColorBrush(palette.Faint);
        ApplyCodeHighlighting();
        Redraw();
    }

    public static TextDocument DocumentFor(Note note)
    {
        if (!Documents.TryGetValue(note.Id, out var doc))
        {
            doc = new TextDocument(note.Text);
            doc.UndoStack.SizeLimit = 400;
            doc.TextChanged += (_, _) => { note.Text = doc.Text; note.Modified = DateTime.Now; };
            Documents[note.Id] = doc;
        }
        return doc;
    }

    public static void Forget(Note note) => Documents.Remove(note.Id);

    public void Bind(Note note)
    {
        if (ReferenceEquals(Note, note)) return;
        Flush();
        _binding = true;
        if (Editor.Document is { } old) old.Changed -= OnDocumentChanged;
        Note = note;
        var doc = DocumentFor(note);
        Editor.Document = doc;
        doc.Changed += OnDocumentChanged;
        Editor.CaretOffset = Math.Clamp(note.Caret, 0, doc.TextLength);
        _binding = false;
        Analyse();
        var scroll = note.Scroll;
        var token = ++_bindToken;
        Editor.ScrollToVerticalOffset(scroll);
        // Layout may not be ready yet; re-apply once, unless another note was bound meanwhile.
        Dispatcher.UIThread.Post(() => { if (token == _bindToken) Editor.ScrollToVerticalOffset(scroll); }, DispatcherPriority.Loaded);
    }

    /// <summary>Copies caret and scroll back into the note (text is already live).</summary>
    public void Flush()
    {
        if (Note is null) return;
        Note.Caret = Editor.CaretOffset;
        Note.Scroll = Editor.TextArea.TextView.VerticalOffset;
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
        Analyse();
        Edited?.Invoke();
    }

    private void Analyse()
    {
        var doc = Editor.Document;
        var text = doc.Text;
        var (mode, keyword, argument) = Modes.Detect(text);
        var changed = mode != Mode || argument != CodeLanguage;
        Mode = mode; CodeLanguage = argument; ModeKeywordLength = keyword;
        var lines = new string[doc.LineCount];
        for (var i = 0; i < lines.Length; i++) lines[i] = doc.GetText(doc.GetLineByNumber(i + 1));
        _fence = new bool[lines.Length + 2];
        var inside = false;
        for (var i = 0; i < lines.Length; i++)
        {
            var fence = lines[i].TrimStart().StartsWith("```");
            _fence[i + 1] = inside || fence;
            if (fence) inside = !inside;
        }
        _math = mode == NoteMode.Math ? EvaluateMath(lines) : [];
        _inline.Clear();
        if (mode == NoteMode.Plain && Settings.InlineMath)
            for (var i = 0; i < lines.Length; i++)
                if (lines[i].TrimEnd().EndsWith('=') && (Calculator.EvaluateInline(StripListMarker(lines[i])) ?? Calculator.EvaluateInline(BlankLabel(StripListMarker(lines[i])))) is { } r) _inline[i + 1] = r;
        if (mode == NoteMode.Timer && Note is not null)
            for (var i = 1; i < lines.Length; i++)
                if (Timers.TryParse(lines[i], out var d, out var label)) _timers.Ensure(TimerKey(lines[i]), d, label);
        _hint.IsVisible = text.Length == 0;
        _hint.Text = Note is { Slot: > 0 } slot
            ? $"Slot {slot.Slot} is empty.\n\nType here, or press Ctrl+Shift+{slot.Slot}\non any note to keep it in this slot."
            : "Start typing.\n\nLine 1 can switch modes:\nmath · list · timer · code py · clipboard";
        if (changed) { ApplyCodeHighlighting(); ModeChanged?.Invoke(); }
        Redraw();
    }

    /// <summary>
    /// Math notes with prose labels ("lunch 12 + 3"): lines that fail are retried with the leading words blanked,
    /// keeping column offsets so token colours still line up. Bullets are ignored the same way.
    /// </summary>
    private static IReadOnlyList<LineEval?> EvaluateMath(string[] lines)
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
            if (first[i]?.Result is not null || second[i]?.Result is null) { merged[i] = first[i]; continue; }
            var label = retry[i].Length - retry[i].TrimStart().Length;
            var tokens = new List<CalcToken> { new(0, label, TokenKind.Comment) };
            tokens.AddRange(second[i]!.Tokens);
            merged[i] = new LineEval { Result = second[i]!.Result, Tokens = tokens, AssignedName = second[i]!.AssignedName };
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

    public bool InFence(int line) => line < _fence.Length && _fence[line];
    public LineEval? MathLine(int line) => line - 1 < _math.Count ? _math[line - 1] : null;
    public CalcResult? InlineResult(int line) => _inline.GetValueOrDefault(line);
    private string TimerKey(string line) => $"{Note?.Id}:{line.Trim()}";

    /// <summary>The chip to draw at a line's end: text, kind, colour, and the value clicking acts on.</summary>
    public (string Text, string Kind, Color Color, string Value)? ChipFor(int line)
    {
        var p = Palette;
        switch (Mode)
        {
            case NoteMode.Math when MathLine(line)?.Result is { } r:
                return (r.Display, "result", p.Result, r.Display);
            case NoteMode.Plain when InlineResult(line) is { } r:
                return (r.Display, "result", p.Result, r.Display);
            case NoteMode.Timer when line > 1:
                {
                    var text = Editor.Document.GetText(Editor.Document.GetLineByNumber(line));
                    if (!Timers.TryParse(text, out _, out _) || _timers.Get(TimerKey(text)) is not { } t) return null;
                    if (t.Done) return ("✓ done", "timer", p.Confirm, t.Key);
                    if (t.Running) return ("● " + Timers.Format(t.Remaining), "timer", p.Number, t.Key);
                    if (t.Paused is not null) return ("‖ " + Timers.Format(t.Remaining), "timer", p.Muted, t.Key);
                    return ("▶ " + Timers.Format(t.Duration), "timer", p.Faint, t.Key);
                }
        }
        return null;
    }

    private void OnViewPressed(object? sender, PointerPressedEventArgs e)
    {
        var view = Editor.TextArea.TextView;
        var point = e.GetPosition(view);
        var chip = _results.Chips.FirstOrDefault(c => c.Bounds.Contains(point));
        if (chip.Bounds.Width <= 0) return;
        e.Handled = true;
        if (chip.Kind == "timer") { if (_timers.Get(chip.Value) is { } t) _timers.Toggle(t); }
        else { Copy?.Invoke(chip.Value); Toast?.Invoke($"Copied {chip.Value}"); }
        view.InvalidateLayer(KnownLayer.Selection);
    }

    private void OnViewMoved(object? sender, PointerEventArgs e)
    {
        var view = Editor.TextArea.TextView;
        var point = e.GetPosition(view);
        var over = _results.Chips.Any(c => c.Bounds.Contains(point));
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
        if (e.Key == Key.Enter && ctrl && !shift)
        {
            e.Handled = true;
            if (Mode == NoteMode.Timer) ToggleTimerAtCaret(); else ToggleCheckbox(CaretLine);
            return;
        }
        if (e.Key == Key.Enter && !ctrl && !shift && e.KeyModifiers == KeyModifiers.None && Editor.TextArea.Selection.IsEmpty)
        {
            if (ContinueList()) { e.Handled = true; return; }
            if (Mode == NoteMode.Timer) StartTimerOnLine(CaretLine);
        }
        if (e.Key == Key.Tab && Editor.TextArea.Selection.IsEmpty && IsListLine(CaretLine))
        {
            e.Handled = true; Indent(CaretLine, !shift);
        }
    }

    private void ToggleTimerAtCaret()
    {
        var text = Editor.Document.GetText(Editor.Document.GetLineByNumber(CaretLine));
        if (Timers.TryParse(text, out var d, out var label)) _timers.Toggle(_timers.Ensure(TimerKey(text), d, label));
        Editor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
    }

    private void StartTimerOnLine(int lineNumber)
    {
        if (lineNumber <= 1) return;
        var text = Editor.Document.GetText(Editor.Document.GetLineByNumber(lineNumber));
        if (Timers.TryParse(text, out var d, out var label))
        {
            var entry = _timers.Ensure(TimerKey(text), d, label);
            if (entry.EndsAt is null) { _timers.Start(entry); Toast?.Invoke($"Timer started · {Timers.Format(d)}{(label.Length > 0 ? " · " + label : "")}"); }
        }
    }

    private bool IsListLine(int lineNumber) =>
        ListPrefix.IsMatch(Editor.Document.GetText(Editor.Document.GetLineByNumber(lineNumber)));

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
        var indent = m.Groups[1].Value;
        var prefix = m.Groups[2].Value;
        if (text.Trim() == prefix.Trim())
        {
            // Empty item: end the list (or outdent one level first).
            if (indent.Length >= 2) { doc.Remove(line.Offset, 2); return true; }
            doc.Replace(line.Offset, line.Length, "");
            if (Mode == NoteMode.List) return true;
            return true;
        }
        string next;
        if (m.Groups[3].Success) next = $"{int.Parse(m.Groups[3].Value) + 1}{m.Groups[4].Value} ";
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

    private void ApplyCodeHighlighting()
    {
        if (Mode != NoteMode.Code) { Editor.SyntaxHighlighting = null; return; }
        var ext = CodeLanguage switch
        {
            "py" or "python" => ".py", "js" or "javascript" or "ts" or "typescript" => ".js", "cs" or "csharp" or "c#" => ".cs",
            "json" => ".json", "html" => ".html", "xml" => ".xml", "css" => ".css", "sql" => ".sql",
            "ps1" or "powershell" or "pwsh" => ".ps1", "java" => ".java", "cpp" or "c" or "c++" or "h" => ".cpp",
            "md" or "markdown" => ".md", "php" => ".php", _ => ".py"
        };
        var definition = HighlightingManager.Instance.GetDefinitionByExtension(ext);
        if (definition is null) { Editor.SyntaxHighlighting = null; return; }
        foreach (var color in definition.NamedHighlightingColors)
        {
            var n = color.Name.ToLowerInvariant();
            Color? c = n.Contains("comment") ? Palette.Comment
                : n.Contains("string") || n.Contains("char") ? Palette.String
                : n.Contains("number") || n.Contains("digit") ? Palette.Number
                : n.Contains("method") || n.Contains("function") ? Palette.Identifier
                : n.Contains("keyword") || n.Contains("modifier") || n.Contains("type") || n.Contains("visibility") || n.Contains("namespace")
                  || n.Contains("statement") || n.Contains("access") || n.Contains("reference") || n.Contains("operator") || n.Contains("tag") ? Palette.Keyword
                : n.Contains("attribute") || n.Contains("property") ? Palette.Identifier
                : null;
            if (c is { } value) color.Foreground = new SimpleHighlightingBrush(value);
            color.Background = null;
        }
        Editor.SyntaxHighlighting = null;
        Editor.SyntaxHighlighting = definition;
    }

    private void OnTextEntered(object? sender, TextInputEventArgs e) => Completion.OnTextEntered(this, e.Text);

    public void RaiseToast(string text) => Toast?.Invoke(text);
}
