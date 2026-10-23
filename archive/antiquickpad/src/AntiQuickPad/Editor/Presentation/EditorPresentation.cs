using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Rendering;

namespace AntiQuickPad.Editor;

// Geometry and visual policy are exposed without giving behaviour renderer details.
internal interface IEditorPresentation
{
    bool ShowFolding { get; }
    int CaretLine { set; }
    bool TryCheckboxAt(Point point, out int offset);
    void SetHandCursor(bool hand);
    void InvalidateGuides();
    void ConfigureFolding();
}

internal sealed class EditorPresentation(TextEditor editor, Look look, MarkdownScanner scan) : IEditorPresentation
{
    private IndentGuideRenderer _guides = null!;
    public bool ShowFolding => look.ShowFolding;
    public int CaretLine { set => _guides.CaretLine = value; }
    internal static TextEditor CreateEditor(Look look, TextDocument? document)
    {
        var editor = new TextEditor
        {
            Document = document ?? new TextDocument(),
            FontFamily = new FontFamily(look.EditorFont), FontSize = look.FontSize,
            Background = look.Material == "solid" ? new SolidColorBrush(look.Background) : Brushes.Transparent, Foreground = new SolidColorBrush(look.Foreground),
            LineNumbersForeground = new SolidColorBrush(look.Muted), ShowLineNumbers = look.ShowGutter,
            WordWrap = true, Padding = new Thickness(look.EditorPadding),
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };
        if (look.Material != "solid")
        {
            void Clear() => ClearGlassBackgrounds(editor);
            editor.TextArea.Background = Brushes.Transparent;
            editor.Loaded += (_, _) => Clear();
            editor.TemplateApplied += (_, _) => Avalonia.Threading.Dispatcher.UIThread.Post(Clear, Avalonia.Threading.DispatcherPriority.Loaded);
        }
        return editor;
    }
    internal static void ClearGlassBackgrounds(TextEditor editor)
    {
        editor.Background = Brushes.Transparent; editor.TextArea.Background = Brushes.Transparent;
        foreach (var visual in editor.GetVisualDescendants())
        {
            if (visual is Panel panel) panel.Background = Brushes.Transparent;
            if (visual is Border border) border.Background = Brushes.Transparent;
            if (visual is TemplatedControl control) control.Background = Brushes.Transparent;
            if (visual is ContentPresenter presenter) presenter.Background = Brushes.Transparent;
        }
    }
    internal void Install()
    {
        if (look.ModernEditor)
        {
            editor.FontWeight = FontWeight.Normal;
            editor.TextArea.TextView.LineTransformers.Add(new BaseTypeface(look.EditorFont));
            editor.TextArea.TextView.BackgroundRenderers.Add(new ActiveLineRenderer(editor, new SolidColorBrush(look.ActiveLine)));
        }
        editor.TextArea.TextView.LineTransformers.Add(new MarkdownColorizer(look, scan));
        editor.TextArea.TextView.BackgroundRenderers.Add(new CodePanelRenderer(look, scan));
        editor.TextArea.TextView.BackgroundRenderers.Add(new StrikeRenderer(look, scan));
        editor.TextArea.TextView.BackgroundRenderers.Add(new CheckboxRenderer(look, scan));
        _guides = new IndentGuideRenderer(look, scan);
        editor.TextArea.TextView.BackgroundRenderers.Add(_guides);
    }
    public bool TryCheckboxAt(Point point, out int offset) => TryCheckboxAt(editor, point, out offset);
    internal static bool TryCheckboxAt(TextEditor editor, Point point, out int offset)
    {
        var hit = editor.TextArea.TextView.BackgroundRenderers.OfType<CheckboxRenderer>().Single().Boxes
            .FirstOrDefault(b => b.Bounds.Contains(point));
        offset = hit.Offset;
        return hit.Bounds.Width > 0;
    }
    internal static readonly Cursor CheckboxHand = new(StandardCursorType.Hand);
    private static readonly Cursor TextCursor = new(StandardCursorType.Ibeam);
    public void SetHandCursor(bool hand) => SetHandCursor(editor, hand);
    internal static void SetHandCursor(TextEditor editor, bool hand)
    {
        var cursor = hand ? CheckboxHand : TextCursor;
        editor.Cursor = cursor; editor.TextArea.Cursor = cursor; editor.TextArea.TextView.Cursor = cursor;
        ToolTip.SetTip(editor.TextArea.TextView, hand ? "Toggle task checkbox" : null);
    }
    public void InvalidateGuides() => editor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
    public void ConfigureFolding()
    {
        foreach (var margin in editor.TextArea.LeftMargins.OfType<FoldingMargin>())
            margin.SetValue(FoldingMargin.FoldingMarkerBrushProperty, new SolidColorBrush(look.Accent));
        if (look.ModernEditor) ChevronMargin.Install(editor, new SolidColorBrush(look.Muted));
    }
    internal void Release()
    {
        foreach (var margin in editor.TextArea.LeftMargins.OfType<ChevronMargin>()) margin.ReleaseInput();
        editor.TextArea.TextView.LineTransformers.Clear();
        editor.TextArea.TextView.BackgroundRenderers.Clear();
        editor.TextArea.LeftMargins.Clear();
    }
    private sealed class BaseTypeface(string font) : DocumentColorizingTransformer
    {
        protected override void ColorizeLine(DocumentLine line)
        {
            if (line.Length > 0) ChangeLinePart(line.Offset, line.EndOffset,
                e => e.TextRunProperties.SetTypeface(new Typeface(new FontFamily(font), FontStyle.Normal, FontWeight.Normal)));
        }
    }
    private sealed class ActiveLineRenderer(TextEditor editor, IBrush brush) : IBackgroundRenderer
    {
        public KnownLayer Layer => KnownLayer.Background;
        public void Draw(TextView view, DrawingContext dc)
        {
            if (!view.VisualLinesValid || view.Document is null || view.Document.TextLength == 0) return;
            var line = view.VisualLines.FirstOrDefault(v => v.FirstDocumentLine.LineNumber <= editor.TextArea.Caret.Line && v.LastDocumentLine.LineNumber >= editor.TextArea.Caret.Line);
            if (line is not null) dc.DrawRectangle(brush, null, new Rect(0, line.VisualTop - view.VerticalOffset, view.Bounds.Width, line.Height), 3, 3);
        }
    }

}
