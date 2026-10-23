using AntiQuickPad.Contract;

namespace AntiQuickPad.Controller;

internal sealed partial class ResidentController
{
    // Capability checks preserve the conformance contract; they do not claim physical delivery.
    public IReadOnlyList<string> LiveRoutes(string id)
    {
        var command = FunctionContract.Commands.SingleOrDefault(c => c.Id == id);
        if (command is null) return Array.Empty<string>();
        var hostKeys = new HashSet<string>(StringComparer.Ordinal) { "Esc", "Esc (close)", "Ctrl+N", "Ctrl+Shift+Delete" };
        var editorKeys = new HashSet<string>(StringComparer.Ordinal) { "Enter", "Tab", "Shift+Tab" };
        var live = new List<string>();
        foreach (var route in command.Routes)
        {
            var colon = route.IndexOf(':');
            var kind = colon < 0 ? route : route[..colon];
            var label = colon < 0 ? "" : route[(colon + 1)..];
            var available = kind switch
            {
                "toolbar" or "settings" or "gesture" => _surface.HasPresentationRoute(kind, label, id),
                "hotkey" => hostKeys.Contains(label) ? _initialized :
                    editorKeys.Contains(label) ? _surface.Editor is { IsReadOnly: false } :
                    label == "Ctrl+Alt+Shift+A (override `ANTIQUICKPAD_HOTKEY`)" && _services.CreateHotkey is not null,
                "tray" => label is "Show / Hide" or "icon click" or "Quit" && _services.CreateTray is not null,
                "system" => label switch
                {
                    "window Close" => _initialized && !_quitting && _platform is not null,
                    "second launch" or "`--quit`" => _initialized && _platform is not null,
                    _ => false
                },
                "implicit (typing, caret move)" => _initialized && _surface.Editor is not null && HasAutosave,
                _ => false
            };
            if (available) live.Add(route);
        }
        return live;
    }
}
