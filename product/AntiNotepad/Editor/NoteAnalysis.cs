using System.Runtime.CompilerServices;
using AvaloniaEdit.Document;
using AntiNotepad.Calc;
using AntiNotepad.Model;

namespace AntiNotepad.Editor;

/// <summary>Cached per-document analysis shared by the pad and headless selftests.</summary>
internal sealed class NoteAnalysis
{
    internal static (int First, int Last) ChangedLineRange(TextDocument doc, DocumentChangeEventArgs e)
    {
        var startOffset = Math.Clamp(e.Offset, 0, doc.TextLength);
        var endOffset = Math.Clamp(e.Offset + e.InsertionLength, 0, doc.TextLength);
        var first = Math.Max(0, doc.GetLineByOffset(startOffset).LineNumber - 2);
        var last = Math.Min(doc.LineCount - 1, doc.GetLineByOffset(endOffset).LineNumber);
        return (first, Math.Max(first, last));
    }

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
                var marked = MathNotes.StripListMarker(Lines[i]);
                if ((Calculator.EvaluateInlineLine(marked) ?? Calculator.EvaluateInlineLine(MathNotes.BlankLabel(marked))) is { } result
                    && (result.Result is not null || result.ErrorReason == CalcErrorReason.MixedCurrencies))
                    _inlineLines[i + 1] = result;
            }
        }
        else _inlineLines.Clear();

        if (Mode == NoteMode.Math)
        {
            // Variables and prose-label retry semantics can affect later lines, so math deliberately keeps the full pass.
            _mathLines = MathNotes.EvaluateMath(Lines);
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
