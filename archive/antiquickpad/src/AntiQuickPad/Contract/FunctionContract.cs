namespace AntiQuickPad.Contract;

// One code mirror of the Commands table. No UI dependencies.
internal sealed record FunctionCommand(string Id, string Required, string ReachableBy)
{
    public IReadOnlyList<string> Routes => ReachableBy.Split("; ", StringSplitOptions.None);
}

internal static class FunctionContract
{
    public static IReadOnlyList<FunctionCommand> Commands { get; } = Array.AsReadOnly(new FunctionCommand[]
    {
        new("app.toggle", "shared", "hotkey:Ctrl+Alt+Shift+A (override `ANTIQUICKPAD_HOTKEY`); tray:Show / Hide"),
        new("app.show", "shared", "tray:icon click; system:second launch"),
        new("app.hide", "yes", "toolbar:Hide; hotkey:Esc; system:window Close"),
        new("app.quit", "yes", "settings:Quit AntiQuickPad; tray:Quit; system:`--quit`"),
        new("window.minimize", "yes", "settings:Minimize"),
        new("window.pin", "yes", "toolbar:Pin"),
        new("window.move", "yes", "gesture:title-bar drag region"),
        new("settings.toggle", "yes", "toolbar:Settings; hotkey:Esc (close)"),
        new("note.create", "yes", "toolbar:New note; settings:New note; hotkey:Ctrl+N"),
        new("note.delete", "yes", "settings:Delete note; hotkey:Ctrl+Shift+Delete"),
        new("note.next", "yes", "toolbar:Next; gesture:wheel notch; gesture:grab-strip drag"),
        new("note.previous", "yes", "toolbar:Previous; gesture:wheel notch; gesture:grab-strip drag"),
        new("note.select", "shared", "gesture:pager dot (Quiet); gesture:drag snap"),
        new("note.autosave", "shared", "implicit (typing, caret move)"),
        new("theme.system", "yes", "settings:System"),
        new("theme.light", "yes", "settings:Light"),
        new("theme.dark", "yes", "settings:Dark"),
        new("settings.material", "yes", "settings:Material"),
        new("settings.tint", "yes", "settings:Tint"),
        new("settings.bars", "yes", "settings:Bars"),
        new("settings.hotkey", "yes", "settings:Hotkey"),
        new("settings.hotkey-capture", "shared", "settings:Hotkey capture"),
        new("settings.startup", "yes", "settings:Start with Windows"),
        new("settings.font-size", "yes", "settings:Font size"),
        new("settings.marker-dim", "yes", "settings:Marker dimness"),
        new("settings.strike", "yes", "settings:Strike thickness"),
        new("editor.list-continue", "shared", "hotkey:Enter"),
        new("editor.indent", "shared", "hotkey:Tab"),
        new("editor.outdent", "shared", "hotkey:Shift+Tab"),
        new("editor.checkbox-toggle", "shared", "gesture:checkbox click"),
        new("editor.fold-toggle", "shared", "gesture:fold chevron click"),
    });

    public static void RequireCommand(string id)
    {
        if (!Commands.Any(c => c.Id == id)) throw new ArgumentException($"Unknown function command ID: {id}", nameof(id));
    }

    // Only the additional yes-route rule is waived; a live gesture is still mandatory.
    public static IReadOnlyDictionary<string, string> KnownRequiredRouteGaps { get; } =
        new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            ["window.move"] = "Declared as yes, but only declares a native title-bar gesture; no toolbar/menu/hotkey route."
        });

    public static void Check(string shell, Func<string, IReadOnlyList<string>> liveRoutes)
    {
        var failures = new List<string>();
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "docs", "function-contract.md")))
            directory = directory.Parent;
        if (directory is null)
            throw new InvalidOperationException($"CONFORMANCE {shell} FAIL: docs/function-contract.md missing (all IDs unverified: {string.Join(", ", Commands.Select(c => c.Id))})");

        var documented = new Dictionary<string, FunctionCommand>(StringComparer.Ordinal);
        var inCommands = false;
        foreach (var line in File.ReadLines(Path.Combine(directory.FullName, "docs", "function-contract.md")))
        {
            if (line.Trim() == "## Commands") { inCommands = true; continue; }
            if (inCommands && line.StartsWith("## ", StringComparison.Ordinal)) break;
            if (!inCommands) continue;
            var cells = line.Split('|').Select(c => c.Trim()).ToArray();
            if (cells.Length < 2 || !cells[1].StartsWith('`')) continue;
            var id = cells[1].Trim('`');
            if (cells.Length != 7 || !cells[1].EndsWith('`'))
            { failures.Add($"{id}: malformed Commands row"); continue; }
            if (!documented.TryAdd(id, new(id, cells[5], cells[4]))) failures.Add($"{id}: duplicate doc ID");
        }
        var mirrored = Commands.ToDictionary(c => c.Id, StringComparer.Ordinal);
        foreach (var id in documented.Keys.Union(mirrored.Keys, StringComparer.Ordinal))
        {
            if (!documented.TryGetValue(id, out var doc)) failures.Add($"{id}: missing doc ID");
            else if (!mirrored.TryGetValue(id, out var code)) failures.Add($"{id}: missing code ID");
            else
            {
                if (doc.Required != code.Required) failures.Add($"{id}: Required mismatch ({doc.Required} vs {code.Required})");
                if (doc.ReachableBy != code.ReachableBy) failures.Add($"{id}: Reachable by mismatch");
            }
        }
        foreach (var command in Commands)
        {
            var live = liveRoutes(command.Id);
            if (live.Count == 0) failures.Add($"{command.Id}: no live declared route");
            if (live.Except(command.Routes, StringComparer.Ordinal).Any()) failures.Add($"{command.Id}: undeclared live route");
            if (command.Required == "yes" && !live.Any(r => r.StartsWith("toolbar:", StringComparison.Ordinal) ||
                    r.StartsWith("settings:", StringComparison.Ordinal) || r.StartsWith("hotkey:", StringComparison.Ordinal)))
            {
                if (KnownRequiredRouteGaps.TryGetValue(command.Id, out var reason))
                    Console.WriteLine($"CONFORMANCE {shell} KNOWN-GAP {command.Id}: {reason}");
                else failures.Add($"{command.Id}: yes requires a live toolbar/settings/hotkey route");
            }
        }
        if (failures.Count > 0) throw new InvalidOperationException($"CONFORMANCE {shell} FAIL: " + string.Join("; ", failures));
        Console.WriteLine($"CONFORMANCE {shell} PASS ({Commands.Count} commands)");
    }
}
