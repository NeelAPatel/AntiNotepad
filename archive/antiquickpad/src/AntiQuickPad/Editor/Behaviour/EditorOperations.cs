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

/// <summary>Pure document operations shared by input handlers and smoke checks.</summary>
internal static class EditorOperations
{
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<TextDocument, ViewModels.MainViewModel> Models = new();
    internal static void Bind(TextDocument document, ViewModels.MainViewModel model) { Models.Remove(document); Models.Add(document, model); }
    internal static void Unbind(TextDocument document) => Models.Remove(document);
    internal readonly record struct TabResult(int CaretOffset, int? SelectionStart, int? SelectionEnd, bool Changed);

    public static string? ToggleCheckbox(TextDocument document, int markerOffset, ref int caretOffset)
    {
        var result = Execute(document, new(CoreText.TextEditKind.ToggleCheckbox, document.Text, caretOffset, MarkerOffset: markerOffset));
        caretOffset = result.CaretOffset;
        return result.CheckboxToken;
    }

    public static TabResult ApplyTab(TextDocument document, int caretOffset, int? selectionStart, int? selectionEnd, bool outdent, bool allowList = true)
    {
        var result = Execute(document, new(CoreText.TextEditKind.ApplyTab, document.Text, caretOffset, selectionStart, selectionEnd, outdent, allowList));
        return new(result.CaretOffset, result.SelectionStart, result.SelectionEnd, result.Changed);
    }

    // One replace operation gives continuation/exit its own single undo step.
    public static int? ContinueList(TextDocument document, int caretOffset)
    {
        return Execute(document, new(CoreText.TextEditKind.ContinueList, document.Text, caretOffset)).ContinuationCaret;
    }

    private static CoreText.TextEditResult Execute(TextDocument document, CoreText.TextEditRequest request)
    {
        var result = Models.TryGetValue(document, out var model) ? model.PlanEdit(request) : ViewModels.MainViewModel.PlanFixtureEdit(request);
        if (result.Edits.Count > 0)
        {
            document.BeginUpdate();
            try { foreach (var edit in result.Edits) document.Replace(edit.Offset, edit.Length, edit.Replacement); }
            finally { document.EndUpdate(); }
        }
        return result;
    }
}

