using AntiNotepad.Model;
using AntiNotepad.Platform;
using AntiNotepad.UI;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace AntiNotepad;

internal static partial class SelfTest
{
    private sealed record LayerRule(
        string From,
        string? To,
        bool AnyAntiNotepadNamespace = false,
        Type? ExactFrom = null,
        Type? ExactTo = null,
        bool AnyApplicationTypeExceptTarget = false,
        LayerTargetKind TargetKind = LayerTargetKind.Namespace,
        string? RawTypeName = null)
    {
        public string Label => TargetKind switch
        {
            LayerTargetKind.PInvokeHolder => $"{From}->pinvoke-holder",
            LayerTargetKind.RawType => $"{From}->{RawTypeName}",
            LayerTargetKind.WindowsNamespace => $"{From}->Windows.*",
            _ => AnyAntiNotepadNamespace ? $"{From}->*any other AntiNotepad namespace*" : $"{From}->{To}"
        };
    }

    private enum LayerTargetKind { Namespace, PInvokeHolder, RawType, WindowsNamespace }

    private sealed record SettingsSetterAllowance(Type CallerType, string[] Methods);

    private sealed record SettingsSetterViolation(MethodBase Caller, MethodBase Setter)
    {
        public string Display => $"{Caller.DeclaringType?.FullName}.{Caller.Name} -> {Setter.DeclaringType?.FullName}.{Setter.Name}";
    }

    private static readonly LayerRule[] LayerRules = CreateLayerRules();

    private static LayerRule[] CreateLayerRules() =>
    [
        new("Editor", "UI"),
        new("Model", "UI"),
        new("Model", "Editor"),
        new("Calc", null, AnyAntiNotepadNamespace: true),
        new("Theme", "UI"),
        new("Theme", "Editor"),
        new("Platform", "UI"),
        new("Platform", "Editor"),
        new("HotkeyRegistrar", "PadWindow", ExactFrom: typeof(HotkeyRegistrar), ExactTo: typeof(PadWindow)),
        new("HotkeyRegistrar", "PadView", ExactFrom: typeof(HotkeyRegistrar), ExactTo: typeof(Editor.PadView)),
        new("HotkeyRegistrar", "Program", ExactFrom: typeof(HotkeyRegistrar), ExactTo: typeof(Program)),
        new("ClipboardCapturePolicy", "UI", ExactFrom: typeof(Editor.ClipboardCapturePolicy)),
        new("ClipboardCapturePolicy", "Platform", ExactFrom: typeof(Editor.ClipboardCapturePolicy)),
        new("ClipboardCapturePolicy", "Motion", ExactFrom: typeof(Editor.ClipboardCapturePolicy)),
        new("MathNotes", "UI", ExactFrom: typeof(Editor.MathNotes)),
        new("MathNotes", "Platform", ExactFrom: typeof(Editor.MathNotes)),
        new("MathNotes", "Motion", ExactFrom: typeof(Editor.MathNotes)),
        new("MathNotes", "Theme", ExactFrom: typeof(Editor.MathNotes)),
        new("MathNotes", "PadWindow", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(PadWindow)),
        new("MathNotes", "PadView", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Editor.PadView)),
        new("MathNotes", "Program", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Program)),
        new("MathNotes", "DesktopPlatform", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(DesktopPlatform)),
        new("MathNotes", "Window", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Avalonia.Controls.Window)),
        new("MathNotes", "Control", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Avalonia.Controls.Control)),
        new("MathNotes", "Dispatcher", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
        new("MathNotes", "TextDocument", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(AvaloniaEdit.Document.TextDocument)),
        new("MathNotes", "Timers", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Timers)),
        new("MathNotes", "Settings", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(Settings)),
        new("MathNotes", "SettingsService", ExactFrom: typeof(Editor.MathNotes), ExactTo: typeof(SettingsService)),
        new("ListEditing", "UI", ExactFrom: typeof(Editor.ListEditing)),
        new("ListEditing", "Platform", ExactFrom: typeof(Editor.ListEditing)),
        new("ListEditing", "Motion", ExactFrom: typeof(Editor.ListEditing)),
        new("ListEditing", "Theme", ExactFrom: typeof(Editor.ListEditing)),
        new("ListEditing", "PadWindow", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(PadWindow)),
        new("ListEditing", "PadView", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Editor.PadView)),
        new("ListEditing", "Program", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Program)),
        new("ListEditing", "DesktopPlatform", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(DesktopPlatform)),
        new("ListEditing", "Window", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Avalonia.Controls.Window)),
        new("ListEditing", "Control", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Avalonia.Controls.Control)),
        new("ListEditing", "Dispatcher", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
        new("ListEditing", "TextDocument", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(AvaloniaEdit.Document.TextDocument)),
        new("ListEditing", "TextEditor", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(AvaloniaEdit.TextEditor)),
        new("ListEditing", "TextArea", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(AvaloniaEdit.Editing.TextArea)),
        new("ListEditing", "TextView", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(AvaloniaEdit.Rendering.TextView)),
        new("ListEditing", "Timers", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Timers)),
        new("ListEditing", "Settings", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Settings)),
        new("ListEditing", "SettingsService", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(SettingsService)),
        new("ListEditing", "CheckboxGenerator", ExactFrom: typeof(Editor.ListEditing), ExactTo: typeof(Editor.CheckboxGenerator)),
        new("DocumentCommands", "UI", ExactFrom: typeof(Editor.DocumentCommands)),
        new("DocumentCommands", "Platform", ExactFrom: typeof(Editor.DocumentCommands)),
        new("DocumentCommands", "Motion", ExactFrom: typeof(Editor.DocumentCommands)),
        new("DocumentCommands", "Theme", ExactFrom: typeof(Editor.DocumentCommands)),
        new("DocumentCommands", "PadWindow", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(PadWindow)),
        new("DocumentCommands", "PadView", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Editor.PadView)),
        new("DocumentCommands", "Program", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Program)),
        new("DocumentCommands", "DesktopPlatform", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(DesktopPlatform)),
        new("DocumentCommands", "Window", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Avalonia.Controls.Window)),
        new("DocumentCommands", "Control", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Avalonia.Controls.Control)),
        new("DocumentCommands", "Dispatcher", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
        new("DocumentCommands", "TextEditor", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(AvaloniaEdit.TextEditor)),
        new("DocumentCommands", "TextArea", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(AvaloniaEdit.Editing.TextArea)),
        new("DocumentCommands", "TextView", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(AvaloniaEdit.Rendering.TextView)),
        new("DocumentCommands", "Timers", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Timers)),
        new("DocumentCommands", "Settings", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Settings)),
        new("DocumentCommands", "SettingsService", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(SettingsService)),
        new("DocumentCommands", "CheckboxGenerator", ExactFrom: typeof(Editor.DocumentCommands), ExactTo: typeof(Editor.CheckboxGenerator)),
        new("NoteAnalysis", "PadView", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Editor.PadView)),
        new("NoteAnalysis", "UI", ExactFrom: typeof(Editor.NoteAnalysis)),
        new("NoteAnalysis", "Platform", ExactFrom: typeof(Editor.NoteAnalysis)),
        new("NoteAnalysis", "Motion", ExactFrom: typeof(Editor.NoteAnalysis)),
        new("NoteAnalysis", "Theme", ExactFrom: typeof(Editor.NoteAnalysis)),
        new("NoteAnalysis", "PadWindow", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(PadWindow)),
        new("NoteAnalysis", "Program", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Program)),
        new("NoteAnalysis", "DesktopPlatform", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(DesktopPlatform)),
        new("NoteAnalysis", "Window", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Controls.Window)),
        new("NoteAnalysis", "Control", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Controls.Control)),
        new("NoteAnalysis", "Dispatcher", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
        new("NoteAnalysis", "Settings", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(Settings)),
        new("NoteAnalysis", "SettingsService", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(SettingsService)),
        new("NoteAnalysis", "TextEditor", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.TextEditor)),
        new("NoteAnalysis", "TextArea", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.Editing.TextArea)),
        new("NoteAnalysis", "TextView", ExactFrom: typeof(Editor.NoteAnalysis), ExactTo: typeof(AvaloniaEdit.Rendering.TextView)),
        new("ClipboardCapturePolicy", "PadWindow", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(PadWindow)),
        new("ClipboardCapturePolicy", "PadView", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(Editor.PadView)),
        new("ClipboardCapturePolicy", "Program", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(Program)),
        new("ClipboardCapturePolicy", "DesktopPlatform", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(DesktopPlatform)),
        new("ClipboardCapturePolicy", "Window", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(Avalonia.Controls.Window)),
        new("ClipboardCapturePolicy", "Control", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(Avalonia.Controls.Control)),
        new("ClipboardCapturePolicy", "Dispatcher", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(Avalonia.Threading.Dispatcher)),
        new("ClipboardCapturePolicy", "TextDocument", ExactFrom: typeof(Editor.ClipboardCapturePolicy), ExactTo: typeof(AvaloniaEdit.Document.TextDocument)),
        new("Platform", "Program", ExactTo: typeof(Program)),
        new("Model", "Program", ExactTo: typeof(Program)),
        new("RuntimeLog", "UI", ExactFrom: typeof(RuntimeLog)),
        new("RuntimeLog", "Editor", ExactFrom: typeof(RuntimeLog)),
        new("RuntimeLog", "Platform", ExactFrom: typeof(RuntimeLog)),
        new("RuntimeLog", "Program", ExactFrom: typeof(RuntimeLog), ExactTo: typeof(Program)),
        new("RuntimeLog", "Avalonia", ExactFrom: typeof(RuntimeLog), ExactTo: typeof(Avalonia.Application)),
        new("SettingsWindow", "PadWindow", ExactFrom: typeof(SettingsWindow), ExactTo: typeof(PadWindow)),
        new("SettingsService", "UI", ExactFrom: typeof(SettingsService)),
        new("SettingsService", "Editor", ExactFrom: typeof(SettingsService)),
        new("Editor", "Program", ExactTo: typeof(Program)),
        new("UI", "Program", ExactTo: typeof(Program)),
        new("App", "Program", ExactFrom: typeof(App), ExactTo: typeof(Program)),
        new("Model", "NotesSession", ExactTo: typeof(NotesSession)),
        new("Editor", "NotesSession", ExactTo: typeof(NotesSession)),
        new("INotesLocation", "UI", ExactFrom: typeof(INotesLocation)),
        new("INotesLocation", "Editor", ExactFrom: typeof(INotesLocation)),
        new("INotesLocation", "Platform", ExactFrom: typeof(INotesLocation)),
        new("*except Program", "Program", ExactTo: typeof(Program), AnyApplicationTypeExceptTarget: true),
        new("UI", null, TargetKind: LayerTargetKind.PInvokeHolder),
        new("Editor", null, TargetKind: LayerTargetKind.PInvokeHolder),
        new("UI", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "Marshal"),
        new("Editor", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "Marshal"),
        new("UI", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "IntPtr"),
        new("Editor", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "IntPtr"),
        new("UI", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "UIntPtr"),
        new("Editor", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "UIntPtr"),
        new("UI", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "Win32Properties"),
        new("Editor", null, TargetKind: LayerTargetKind.RawType, RawTypeName: "Win32Properties"),
        new("UI", null, TargetKind: LayerTargetKind.WindowsNamespace),
        new("Editor", null, TargetKind: LayerTargetKind.WindowsNamespace),
    ];

    // Setter calls from these exact methods are passed to SettingsService.Update, except ResetSettings' defaults initializer.
    // Settings itself owns its detached-copy initializer; StoreData's constructor is the JSON/default construction path.
    private static readonly SettingsSetterAllowance[] SettingsSetterAllowances =
    [
        new(typeof(Settings), ["CopyForSave"]),
        new(typeof(StoreData), [".ctor"]),
        new(typeof(Store), ["Normalize"]),
        new(typeof(Welcome), ["SeedIfNew"]),
        new(typeof(PadWindow), ["Zoom", "RememberBounds", "BindPanes", "ToggleSplit", "CycleSplitSlot", "TogglePin", "ResetSettings"]),
        new(typeof(SettingsWindow), ["ThemePage", "AccentDots", "UiPage", "EditorPage", "FeelPage", "TuningRows", "SystemPage"]),
        new(typeof(SearchOverlay), ["ToggleGrid"]),
    ];

    private static bool DesktopPlatformSurfaceChecks(out string report)
    {
        var failures = new List<string>();
        try
        {
            var platform = typeof(DesktopPlatform);
            if (!platform.IsNotPublic || !platform.IsAbstract || !platform.IsSealed)
                failures.Add("DesktopPlatform is not an internal static class");

            (string Name, Type ReturnType, Type[] ParameterTypes)[] expected =
            [
                ("CursorPosition", typeof(Avalonia.PixelPoint), Type.EmptyTypes),
                ("ListenClipboard", typeof(IDisposable), [typeof(Avalonia.Controls.Window), typeof(Action)]),
                ("ClipboardIsPrivate", typeof(bool), Type.EmptyTypes),
                ("Chime", typeof(void), Type.EmptyTypes),
                ("TrimWorkingSet", typeof(void), Type.EmptyTypes),
                ("ConfigurePopupWindow", typeof(void), [typeof(Avalonia.Controls.TopLevel)]),
                ("Flash", typeof(void), [typeof(Avalonia.Controls.Window)]),
                ("OpenUrl", typeof(void), [typeof(string)]),
                ("OpenFolder", typeof(void), [typeof(string)]),
                ("StartScreenSnip", typeof(void), Type.EmptyTypes),
            ];
            const BindingFlags declaredStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var member in expected)
            {
                var method = platform.GetMethod(member.Name, declaredStatic, binder: null, types: member.ParameterTypes, modifiers: null);
                if (method is null || !method.IsStatic || method.ReturnType != member.ReturnType)
                    failures.Add("missing or mismatched " + member.Name);
            }

            var native = platform.GetNestedType("Native", BindingFlags.NonPublic);
            if (native is null) failures.Add("nested Native is missing");
            else if (!native.IsNestedPrivate) failures.Add("nested Native is not private");
        }
        catch (Exception ex) { failures.Add(ex.GetType().Name); }

        report = failures.Count == 0 ? "passed" : string.Join(", ", failures);
        return failures.Count == 0;
    }

    private static bool PInvokeHolderChecks(out string report)
    {
        var failures = new List<string>();
        try
        {
            CheckNestedPrivate(typeof(GlobalHotkey), "GlobalHotkey.Win32");
            CheckNestedPrivate(typeof(TaskbarProxy), "TaskbarProxy.Win32");
            CheckNestedPrivate(typeof(DesktopPlatform), "DesktopPlatform.Native");
        }
        catch (Exception ex) { failures.Add(ex.GetType().Name); }

        report = failures.Count == 0 ? "passed" : string.Join(", ", failures);
        return failures.Count == 0;

        void CheckNestedPrivate(Type parent, string label)
        {
            var name = label[(label.LastIndexOf('.') + 1)..];
            var nested = parent.GetNestedType(name, BindingFlags.NonPublic);
            if (nested is null) failures.Add(label + " missing");
            else if (!nested.IsNestedPrivate) failures.Add(label + " is not private");
            else if (name != "Native" && !IsPInvokeHolder(nested)) failures.Add(label + " has no P/Invoke methods");
        }
    }

    private static (List<string> Lines, List<string> Failures) CheckLayers()
    {
        var lines = new List<string>();
        var failures = new List<string>();
        var scanErrors = new List<string>();
        Type[] types;
        try { types = typeof(SelfTest).Assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.OfType<Type>().ToArray();
            scanErrors.Add($"assembly type load was partial ({ex.LoaderExceptions?.Length ?? 0} loader errors)");
        }

        var references = new Dictionary<Type, HashSet<Type>>();
        var rawReferences = new Dictionary<Type, HashSet<Type>>();
        foreach (var type in types.Where(t => LayerRules.Any(rule => MatchesSource(rule, t))))
        {
            var found = new HashSet<Type>();
            var rawFound = new HashSet<Type>();
            references[type] = found;
            rawReferences[type] = rawFound;
            try
            {
                AddType(type, found);
                AddType(type.BaseType, found);
                AddType(type.BaseType, found, rawFound);
                foreach (var iface in type.GetInterfaces())
                {
                    AddType(iface, found);
                    AddType(iface, found, rawFound);
                }

                const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    | BindingFlags.Static | BindingFlags.DeclaredOnly;
                foreach (var field in type.GetFields(declared))
                {
                    AddType(field.FieldType, found);
                    AddType(field.FieldType, rawFound);
                }
                foreach (var property in type.GetProperties(declared))
                {
                    AddType(property.PropertyType, found);
                    AddType(property.PropertyType, rawFound);
                    foreach (var parameter in property.GetIndexParameters())
                    {
                        AddType(parameter.ParameterType, found);
                        AddType(parameter.ParameterType, rawFound);
                    }
                }
                foreach (var ev in type.GetEvents(declared))
                {
                    AddType(ev.EventHandlerType, found);
                    AddType(ev.EventHandlerType, rawFound);
                }
                foreach (var constructor in type.GetConstructors(declared)) ScanMethod(constructor, found, scanErrors, directReferences: rawFound);
                if (type.TypeInitializer is { } initializer) ScanMethod(initializer, found, scanErrors, directReferences: rawFound);
                foreach (var method in type.GetMethods(declared))
                {
                    var allowedProgramTypeToken = type == typeof(SelfTest) && method.Name == nameof(CreateLayerRules)
                        ? LayerRules.Single(rule => rule.AnyApplicationTypeExceptTarget).ExactTo
                        : null;
                    ScanMethod(method, found, scanErrors, allowedProgramTypeToken: allowedProgramTypeToken, directReferences: rawFound);
                }
            }
            catch (Exception ex) { scanErrors.Add($"{type.FullName}: reflection {ex.GetType().Name}"); }
        }

        foreach (var rule in LayerRules)
        {
            var sourceReferences = rule.TargetKind == LayerTargetKind.RawType ? rawReferences : references;
            var offenders = sourceReferences.SelectMany(pair => pair.Value
                    .Where(target => MatchesRule(rule, pair.Key, target))
                    .Select(target => (Source: pair.Key, Target: target)))
                .Distinct()
                .OrderBy(pair => pair.Source.FullName, StringComparer.Ordinal)
                .ThenBy(pair => pair.Target.FullName, StringComparer.Ordinal)
                .ToArray();
            lines.Add($"LAYERS {rule.Label} {offenders.Length} refs");
            foreach (var offender in offenders)
                failures.Add($"layer violation {rule.Label}: {offender.Source.FullName} -> {offender.Target.FullName}");
        }

        var outsidePlatform = new List<MethodInfo>();
        foreach (var type in types)
        {
            try
            {
                const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    | BindingFlags.Static | BindingFlags.DeclaredOnly;
                foreach (var method in type.GetMethods(declared).Where(IsPInvokeMethod))
                    if (!InNamespace(TopLevelDeclaringType(method.DeclaringType!).Namespace, "Platform"))
                        outsidePlatform.Add(method);
            }
            catch (Exception ex) { scanErrors.Add($"{type.FullName}: P/Invoke reflection {ex.GetType().Name}"); }
        }
        lines.Add($"LAYERS pinvoke-outside-Platform {outsidePlatform.Count}");
        foreach (var method in outsidePlatform.OrderBy(method => method.DeclaringType?.FullName, StringComparer.Ordinal)
            .ThenBy(method => method.Name, StringComparer.Ordinal))
            failures.Add($"P/Invoke outside Platform: {method.DeclaringType?.FullName}.{method.Name}");

        var setterCheck = CheckSettingsSetters(types);
        lines.Add($"LAYERS Settings.set_* outside owner {setterCheck.Violations.Count}");
        foreach (var violation in setterCheck.Violations)
            failures.Add("settings setter violation: " + violation.Display);

        foreach (var error in scanErrors.Distinct(StringComparer.Ordinal)) failures.Add("layer scan: " + error);
        foreach (var error in setterCheck.ScanErrors) failures.Add("settings setter scan: " + error);
        foreach (var layer in new[] { "UI", "Editor" })
        {
            var processStartCheck = CheckProcessStartCalls(types, layer);
            lines.Add($"LAYERS {layer}->Process.Start {processStartCheck.Violations.Count}");
            foreach (var violation in processStartCheck.Violations)
                failures.Add($"Process.Start layer violation: {violation.Caller.DeclaringType?.FullName}.{violation.Caller.Name} -> {violation.Target.DeclaringType?.FullName}.{violation.Target.Name}");
            foreach (var error in processStartCheck.ScanErrors)
                failures.Add("Process.Start scan: " + error);
        }
        return (lines, failures);
    }

    private static (List<(MethodBase Caller, MethodBase Target)> Violations, List<string> ScanErrors)
        CheckProcessStartCalls(Type[] types, string layer)
    {
        var violations = new List<(MethodBase Caller, MethodBase Target)>();
        var scanErrors = new List<string>();
        const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var type in types.Where(type => InNamespace(type.Namespace, layer)))
        {
            try
            {
                foreach (var constructor in type.GetConstructors(declared))
                    ScanMethod(constructor, [], scanErrors, InspectCall);
                if (type.TypeInitializer is { } initializer)
                    ScanMethod(initializer, [], scanErrors, InspectCall);
                foreach (var method in type.GetMethods(declared))
                    ScanMethod(method, [], scanErrors, InspectCall);
            }
            catch (Exception ex) { scanErrors.Add($"{type.FullName}: reflection {ex.GetType().Name}"); }
        }

        return (violations.Distinct().ToList(), scanErrors.Distinct(StringComparer.Ordinal).ToList());

        void InspectCall(MethodBase caller, OpCode opcode, MethodBase target)
        {
            if ((opcode == OpCodes.Call || opcode == OpCodes.Callvirt)
                && target.DeclaringType == typeof(Process) && target.Name == "Start")
                violations.Add((caller, target));
        }
    }

    private static (List<SettingsSetterViolation> Violations, List<string> ScanErrors) CheckSettingsSetters(Type[] types)
    {
        var violations = new List<SettingsSetterViolation>();
        var scanErrors = new List<string>();
        var scannedTypes = new HashSet<Type>();
        foreach (var type in types)
        {
            const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var constructor in type.GetConstructors(declared))
                ScanMethod(constructor, scannedTypes, scanErrors, InspectCall);
            if (type.TypeInitializer is { } initializer) ScanMethod(initializer, scannedTypes, scanErrors, InspectCall);
            foreach (var method in type.GetMethods(declared)) ScanMethod(method, scannedTypes, scanErrors, InspectCall);
        }

        return (violations.Distinct().OrderBy(item => item.Display, StringComparer.Ordinal).ToList(), scanErrors.Distinct(StringComparer.Ordinal).ToList());

        void InspectCall(MethodBase caller, OpCode opcode, MethodBase target)
        {
            if (opcode != OpCodes.Call && opcode != OpCodes.Callvirt) return;
            if (target.DeclaringType != typeof(Settings) || !target.Name.StartsWith("set_", StringComparison.Ordinal)) return;
            if (IsAllowedSettingsSetterCaller(caller)) return;
            violations.Add(new SettingsSetterViolation(caller, target));
        }
    }

    private static bool IsAllowedSettingsSetterCaller(MethodBase caller)
    {
        var type = caller.DeclaringType;
        while (type?.DeclaringType is { } parent) type = parent;
        if (type == typeof(SettingsService) || type == typeof(SelfTest)) return true;
        var method = caller.Name;
        if (method.StartsWith('<') && method.IndexOf('>') is var end && end > 1) method = method[1..end];
        return SettingsSetterAllowances.Any(allowance => allowance.CallerType == type && allowance.Methods.Contains(method, StringComparer.Ordinal));
    }

    private static bool MatchesSource(LayerRule rule, Type source)
    {
        if (rule.AnyApplicationTypeExceptTarget)
            return rule.ExactTo is { } target && source.Assembly == target.Assembly && !IsExactOrNested(source, target);
        return rule.ExactFrom is { } exact
            ? IsExactOrNested(source, exact)
            : InNamespace(source.Namespace, rule.From);
    }

    private static Type ProgramRuleTarget => LayerRules.Single(rule => rule.AnyApplicationTypeExceptTarget).ExactTo!;

    private static bool StaticServiceExposureChecks(out string report)
    {
        var programType = ProgramRuleTarget;
        Type[] types;
        try { types = programType.Assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { types = ex.Types.OfType<Type>().ToArray(); }

        var serviceTypes = new HashSet<Type> { typeof(Store), typeof(SettingsService), typeof(RuntimeLog), typeof(NotesSession) };
        var exposures = new HashSet<string>(StringComparer.Ordinal);
        const BindingFlags declaredStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

        // This typed reflection guard covers static fields and properties, including compiler backing fields.
        // Delegates, dictionaries and object-typed locators are outside its bound.
        foreach (var type in types)
            CheckType(type);

        report = exposures.Count == 0 ? "0 refs" : string.Join("; ", exposures.OrderBy(value => value, StringComparer.Ordinal));
        return exposures.Count == 0;

        void CheckType(Type type)
        {
            foreach (var field in type.GetFields(declaredStatic))
                if (serviceTypes.Contains(field.FieldType)) exposures.Add($"{type.FullName}.{field.Name}: {field.FieldType.Name}");
            foreach (var property in type.GetProperties(declaredStatic))
            {
                var isStatic = property.GetAccessors(nonPublic: true).Any(accessor => accessor.IsStatic);
                if (isStatic && serviceTypes.Contains(property.PropertyType))
                    exposures.Add($"{type.FullName}.{property.Name}: {property.PropertyType.Name}");
            }
        }
    }

    private static bool EnvironmentReadConfinementChecks(out string report)
    {
        var programType = ProgramRuleTarget;
        Type[] types;
        try { types = programType.Assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { types = ex.Types.OfType<Type>().ToArray(); }

        var violations = new HashSet<string>(StringComparer.Ordinal);
        var observed = new HashSet<string>(StringComparer.Ordinal);
        var scanErrors = new List<string>();
        const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;
        foreach (var type in types)
        {
            var methods = type.GetConstructors(declared).Cast<MethodBase>().ToList();
            if (type.TypeInitializer is { } initializer) methods.Add(initializer);
            methods.AddRange(type.GetMethods(declared));
            foreach (var method in methods)
            {
                var readsEnvironment = false;
                var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                ScanMethod(method, [], scanErrors,
                    inspectCall: (caller, opcode, target) =>
                    {
                        if ((opcode == OpCodes.Call || opcode == OpCodes.Callvirt)
                            && target.DeclaringType == typeof(Environment) && target.Name == nameof(Environment.GetEnvironmentVariable))
                            readsEnvironment = true;
                    },
                    inspectString: (_, value) =>
                    {
                        if (value is "DATA" or "DEV" || value.EndsWith("_DATA", StringComparison.OrdinalIgnoreCase)
                            || value.EndsWith("_DEV", StringComparison.OrdinalIgnoreCase))
                            keys.Add(value);
                    });
                if (!readsEnvironment || keys.Count == 0) continue;
                if (!IsExactOrNested(method.DeclaringType!, programType))
                    foreach (var key in keys) violations.Add($"{method.DeclaringType?.FullName}.{method.Name} reads {key}");
                else
                    foreach (var key in keys)
                        observed.Add(key is "DATA" || key.EndsWith("_DATA", StringComparison.OrdinalIgnoreCase) ? "DATA" : "DEV");
            }
        }

        foreach (var error in scanErrors.Distinct(StringComparer.Ordinal)) violations.Add("scan: " + error);
        foreach (var key in new[] { "DATA", "DEV" })
            if (!observed.Contains(key)) violations.Add($"ANTINOTEPAD_{key} read not found in Program");
        report = violations.Count == 0 ? "0 refs" : string.Join("; ", violations.OrderBy(value => value, StringComparer.Ordinal));
        return violations.Count == 0;
    }

    private static bool MatchesRule(LayerRule rule, Type from, Type to)
    {
        if (!MatchesSource(rule, from)) return false;
        if (rule.TargetKind == LayerTargetKind.PInvokeHolder) return IsPInvokeHolder(to);
        if (rule.TargetKind == LayerTargetKind.RawType) return IsRawInteropType(to, rule.RawTypeName!);
        if (rule.TargetKind == LayerTargetKind.WindowsNamespace)
            return to.Namespace == "Windows" || to.Namespace?.StartsWith("Windows.", StringComparison.Ordinal) == true;
        if (rule.AnyAntiNotepadNamespace)
            return IsAntiNotepadNamespace(to.Namespace) && !InNamespace(to.Namespace, rule.From);
        if (rule.ExactTo is { } exact) return IsExactOrNested(to, exact);
        return InNamespace(to.Namespace, rule.To!);
    }

    private static bool IsExactOrNested(Type type, Type exact)
    {
        for (var current = type; current is not null; current = current.DeclaringType)
            if (current == exact) return true;
        return false;
    }
}
