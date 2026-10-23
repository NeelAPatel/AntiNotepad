using AntiQuickPad.Controller;
using AntiQuickPad.Editor;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace AntiQuickPad.Views.Common;

public sealed partial class ResidentShellWindow
{
    internal IReadOnlyList<string> LiveRoutes(string id) => _controller.LiveRoutes(id);
    // M11b-settings-window: the command a settings entry runs (selftest).
    internal System.Windows.Input.ICommand SettingsEntry(string label) => Command(EnsureSettings().Routes[label]);
    bool IResidentSurface.HasPresentationRoute(string kind, string label, string commandId) => kind switch
    {
        "toolbar" => _layout.Buttons.TryGetValue(label, out var button) && button.Command is not null &&
            _layout.CommandIds.TryGetValue(label, out var toolbarId) && toolbarId == commandId,
        "settings" => EnsureSettings().Routes.TryGetValue(label, out var settingsId) && settingsId == commandId &&
            Command(settingsId) is not null,
        "gesture" => label switch
        {
            // CarouselController's constructor installs wheel/grab handlers and drag snap.
            "wheel notch" or "grab-strip drag" or "drag snap" => _carousel is not null && ReferenceEquals(_layout.Page, _carousel.View),
            "pager dot (Quiet)" => _shellId == "quiet" && _layout.Dots.GetVisualParent() is not null &&
                _layout.Dots.Children.OfType<Button>().Any(b => b.Command is not null),
            // EditorFactory installs the checkbox handler alongside this renderer.
            "checkbox click" => _editor is { IsReadOnly: false } &&
                _editor.TextArea.TextView.BackgroundRenderers.OfType<CheckboxRenderer>().Any(),
            // ChevronMargin replaces the stock margin and retains its FoldingManager.
            "fold chevron click" => _editor is not null && _editor.TextArea.LeftMargins.OfType<ChevronMargin>().Any(),
            "title-bar drag region" => _root.GetVisualDescendants().OfType<Control>().Any(c =>
                WindowDecorationProperties.GetElementRole(c) == WindowDecorationsElementRole.TitleBar),
            _ => false
        },
        _ => false
    };
}
