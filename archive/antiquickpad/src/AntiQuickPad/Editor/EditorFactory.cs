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

/// <summary>Composes the token-driven presentation with document/input behaviour.</summary>
internal static class EditorFactory
{
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<TextEditor, Action> Cleanup = new();
    public static TextEditor Create(Look look, ViewModels.MainViewModel? viewModel = null, TextDocument? document = null)
    {
        var editor = EditorPresentation.CreateEditor(look, document);
        var scan = new MarkdownScanner(editor.Document);
        var presentation = new EditorPresentation(editor, look, scan);
        presentation.Install();
        var releaseBehaviour = EditorBehaviour.Attach(editor, scan, presentation, viewModel);
        Cleanup.Add(editor, () => { releaseBehaviour(); presentation.Release(); scan.Dispose(); });
        return editor;
    }
    public static void Release(TextEditor editor)
    {
        if (Cleanup.TryGetValue(editor, out var cleanup)) { cleanup(); Cleanup.Remove(editor); }
        editor.Document = new TextDocument();
    }
    public static void RunSmoke() => EditorSmoke.RunSmoke();
    internal static bool TryCheckboxAt(TextEditor editor, Point point, out int markerOffset) => EditorPresentation.TryCheckboxAt(editor, point, out markerOffset);
    internal static void UpdateCursor(TextEditor editor, Point point) => EditorPresentation.SetHandCursor(editor, TryCheckboxAt(editor, point, out _));
    internal static Cursor CheckboxHand => EditorPresentation.CheckboxHand;
}
