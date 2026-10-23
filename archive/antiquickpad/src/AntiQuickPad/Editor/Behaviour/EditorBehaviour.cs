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

internal static class EditorBehaviour
{
    internal static Action Attach(TextEditor editor, MarkdownScanner scan, IEditorPresentation presentation,
        ViewModels.MainViewModel? viewModel = null)
    {
        var document = editor.Document;
        if (viewModel is not null) EditorOperations.Bind(document, viewModel);
        editor.Options.IndentationSize = 4;
        EventHandler caretChanged = (_, _) =>
        {
            presentation.CaretLine = editor.TextArea.Caret.Line;
            presentation.InvalidateGuides();
        };
        EventHandler<PointerPressedEventArgs> pressed = (_, e) =>
        {
            if (editor.IsReadOnly || !e.GetCurrentPoint(editor).Properties.IsLeftButtonPressed ||
                !presentation.TryCheckboxAt(e.GetPosition(editor.TextArea.TextView), out var offset)) return;
            var caret = editor.CaretOffset;
            EditorOperations.ToggleCheckbox(document, offset, ref caret);
            editor.CaretOffset = caret; e.Handled = true;
        };
        EventHandler<DocumentChangeEventArgs> redraw = (_, _) => editor.TextArea.TextView.Redraw();
        EventHandler<PointerEventArgs> moved = (_, e) => presentation.SetHandCursor(presentation.TryCheckboxAt(e.GetPosition(editor.TextArea.TextView), out var ignored));
        EventHandler<PointerEventArgs> exited = (_, _) => presentation.SetHandCursor(false);
        EventHandler<KeyEventArgs> key = (_, e) => ApplyKey(editor, scan, e);
        editor.TextArea.Caret.PositionChanged += caretChanged;
        editor.AddHandler(InputElement.PointerPressedEvent, pressed, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        document.Changed += redraw;
        editor.AddHandler(InputElement.PointerMovedEvent, moved, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        editor.PointerExited += exited;
        editor.TextArea.TextView.PointerExited += exited;
        editor.AddHandler(InputElement.KeyDownEvent, key, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        Action releaseFolding = () => { };
        if (presentation.ShowFolding)
        {
            var folding = FoldingManager.Install(editor.TextArea);
            void UpdateFoldings() => folding.UpdateFoldings(scan.GetFoldings().ToList(), -1);
            UpdateFoldings();
            EventHandler<DocumentChangeEventArgs> update = (_, _) => UpdateFoldings();
            document.Changed += update;
            presentation.ConfigureFolding();
            releaseFolding = () =>
            {
                foreach (var margin in editor.TextArea.LeftMargins.OfType<ChevronMargin>()) margin.ReleaseInput();
                document.Changed -= update; FoldingManager.Uninstall(folding);
            };
        }
        return () =>
        {
            releaseFolding();
            editor.TextArea.Caret.PositionChanged -= caretChanged;
            editor.RemoveHandler(InputElement.PointerPressedEvent, pressed);
            document.Changed -= redraw;
            editor.RemoveHandler(InputElement.PointerMovedEvent, moved);
            editor.PointerExited -= exited;
            editor.TextArea.TextView.PointerExited -= exited;
            editor.RemoveHandler(InputElement.KeyDownEvent, key);
            if (viewModel is not null) EditorOperations.Unbind(document);
        };
    }
    internal static Action AttachChevron(ChevronMargin margin)
    {
        EventHandler<PointerEventArgs> moved = (_, e) => margin.HoverAt(e.GetPosition(margin));
        EventHandler<PointerEventArgs> exited = (_, _) => margin.HoverAt(null);
        EventHandler<PointerPressedEventArgs> pressed = (_, e) =>
        {
            if (!e.GetCurrentPoint(margin).Properties.IsLeftButtonPressed) return;
            var target = margin.FoldingAt(e.GetPosition(margin));
            if (target is null) return;
            target.IsFolded = !target.IsFolded; e.Handled = true;
        };
        margin.PointerMoved += moved; margin.PointerExited += exited; margin.PointerPressed += pressed;
        return () => { margin.PointerMoved -= moved; margin.PointerExited -= exited; margin.PointerPressed -= pressed; };
    }
    private static void ApplyKey(TextEditor editor, MarkdownScanner scanner, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && e.KeyModifiers == KeyModifiers.None && !editor.IsReadOnly && editor.SelectionLength == 0 &&
            !scanner.GetLine(editor.Document.GetLineByOffset(editor.CaretOffset).LineNumber).IsCode &&
            EditorOperations.ContinueList(editor.Document, editor.CaretOffset) is int caret)
        {
            editor.CaretOffset = caret;
            e.Handled = true;
            return;
        }
        if (e.Key != Key.Tab || editor.IsReadOnly) return;
        var selection = editor.TextArea.Selection.SurroundingSegment;
        var result = EditorOperations.ApplyTab(editor.Document, editor.CaretOffset, selection?.Offset, selection?.EndOffset,
            e.KeyModifiers.HasFlag(KeyModifiers.Shift), !scanner.GetLine(editor.Document.GetLineByOffset(editor.CaretOffset).LineNumber).IsCode);
        if (result.SelectionStart is int start && result.SelectionEnd is int end && end != start)
            editor.TextArea.Selection = Selection.Create(editor.TextArea, start, end);
        editor.CaretOffset = result.CaretOffset;
        e.Handled = true;
    }

}

