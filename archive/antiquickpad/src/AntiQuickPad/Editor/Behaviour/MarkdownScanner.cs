using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Rendering;
using CoreText = AntiQuickPad.Core.Text;

namespace AntiQuickPad.Editor;

/// <summary>UI adapter retaining renderer-facing signatures while Core owns Markdown rules.</summary>
internal sealed class MarkdownScanner : IDisposable
{
    private readonly TextDocument _document;
    private bool _dirty = true;
    private LineInfo[] _lines = [];
    private NewFolding[] _foldings = [];
    public IReadOnlyList<LineInfo> Lines { get { Ensure(); return _lines; } }
    public MarkdownScanner(TextDocument document) { _document = document; document.Changed += OnChanged; }
    private void OnChanged(object? sender, DocumentChangeEventArgs e) => _dirty = true;
    public void Dispose() => _document.Changed -= OnChanged;
    public LineInfo GetLine(int number) { Ensure(); return _lines[Math.Clamp(number - 1, 0, _lines.Length - 1)]; }
    public IReadOnlyList<NewFolding> GetFoldings() { Ensure(); return _foldings; }

    private void Ensure()
    {
        if (!_dirty) return;
        var scan = ViewModels.MainViewModel.ScanMarkdown(_document.Text);
        _lines = scan.Lines.Select(source =>
        {
            var line = new LineInfo(source.Text)
            {
                HeadingLevel = source.HeadingLevel,
                HeadingMarkerLength = source.HeadingMarkerLength,
                CheckboxOffset = source.CheckboxOffset,
                CheckboxChecked = source.CheckboxChecked,
                BulletLength = source.BulletLength,
                FenceMarker = source.FenceMarker,
                FenceLanguage = source.FenceLanguage,
                IsFence = source.IsFence,
                IsCode = source.IsCode,
                IsPython = source.IsPython,
                PythonMarker = source.PythonMarker,
                CommentOffset = source.CommentOffset
            };
            line.Bold.AddRange(source.Bold.Select(s => new Span(s.Start, s.Length, s.MarkerLength)));
            line.Italic.AddRange(source.Italic.Select(s => new Span(s.Start, s.Length, s.MarkerLength)));
            line.Strike.AddRange(source.Strike.Select(s => new Span(s.Start, s.Length, s.MarkerLength)));
            return line;
        }).ToArray();
        _foldings = scan.Foldings.Select(f => new NewFolding(f.StartOffset, f.EndOffset)).ToArray();
        _dirty = false;
    }

    internal sealed class LineInfo(string text)
    {
        public string Text { get; } = text;
        public int HeadingLevel { get; init; }
        public int HeadingMarkerLength { get; init; }
        public int CheckboxOffset { get; init; } = -1;
        public bool CheckboxChecked { get; init; }
        public int BulletLength { get; init; }
        public string? FenceMarker { get; init; }
        public string FenceLanguage { get; init; } = string.Empty;
        public bool IsFence { get; set; }
        public bool IsCode { get; set; }
        public bool IsPython { get; set; }
        public bool PythonMarker { get; set; }
        public int CommentOffset { get; set; } = -1;
        public List<Span> Bold { get; } = [];
        public List<Span> Italic { get; } = [];
        public List<Span> Strike { get; } = [];
    }
    internal readonly record struct Span(int Start, int Length, int MarkerLength);
}
