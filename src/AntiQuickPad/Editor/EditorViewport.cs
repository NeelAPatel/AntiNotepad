using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using AvaloniaEdit;

namespace AntiQuickPad.Editor;

internal static class EditorViewport
{
    // AvaloniaEdit 12's ScrollTo*Offset methods did not update the live template
    // in the integrated probe. Set the actual ScrollViewer offset instead.
    public static void Restore(TextEditor editor, Vector offset)
    {
        editor.UpdateLayout();
        var viewer = editor.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (viewer is not null) viewer.Offset = offset;
    }
}
