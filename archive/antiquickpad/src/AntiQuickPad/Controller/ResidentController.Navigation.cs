using Avalonia;
using AntiQuickPad.Editor;

namespace AntiQuickPad.Controller;

internal sealed partial class ResidentController
{
    private readonly Dictionary<string, Vector> _scroll = new();
    private string? _departingId;
    public Vector ScrollFor(int index) => _scroll.GetValueOrDefault(_vm.Notes[index].Id);
    public void InitializeNavigation()
    {
        _vm.Navigating += (_, e) => { _surface.CancelMotion(); e.Cancel = !SaveNotes(); if (!e.Cancel) { RememberScroll(); _departingId = _vm.Current.Id; } };
        _vm.Navigated += (_, e) => { if (e.Kind == "delete" && _departingId is not null) _scroll.Remove(_departingId); BindCurrent(); if (e.Kind != "delete") SaveNotes(); FocusEditor(); };
    }
    public void BindCurrent()
    {
        Binding = true;
        try { _surface.Editor.Text = _vm.Current.Text; _surface.Editor.CaretOffset = _vm.Current.CaretOffset; _surface.Editor.Document.UndoStack.ClearAll(); }
        finally { Binding = false; _surface.RefreshHint(); }
        var scroll = _scroll.GetValueOrDefault(_vm.Current.Id);
        EditorViewport.Restore(_surface.Editor, scroll);
        _surface.RefreshStatus();
    }

    private void RememberScroll() => _scroll[_vm.Current.Id] = _surface.Editor.TextArea.TextView.ScrollOffset;
}
