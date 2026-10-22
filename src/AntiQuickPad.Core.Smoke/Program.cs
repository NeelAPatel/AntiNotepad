using AntiQuickPad.Core;
var parent = args.Length > 0 ? args[0] : AppContext.BaseDirectory;
if (typeof(NotesSession).Assembly.GetReferencedAssemblies().Any(a => a.Name!.Contains("Avalonia", StringComparison.OrdinalIgnoreCase))) return 1;
if (!NotesSession.RunPersistenceSmoke(parent)) return 2;
var settingsPath = Path.Combine(parent, "settings-smoke-" + Guid.NewGuid().ToString("N") + ".json");
try
{
    var store = new SettingsStore(settingsPath);
    File.WriteAllText(settingsPath, """{"Width":800,"Height":600,"X":12,"Y":24,"Pinned":true}""");
    var legacy = store.Load();
    if (legacy is not { Pinned: true, ShellId: "classic", LookId: "Ink" }) return 4;
    store.Save(legacy with { ShellId = "card", LookId = "Card" });
    if (store.Load() is not { ShellId: "quiet", LookId: "system", X: 12, Y: 24 }) return 5; // M8b-retire-card-function: Card migrates on load
}
finally { if (File.Exists(settingsPath)) File.Delete(settingsPath); }
if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name!.Contains("Avalonia", StringComparison.OrdinalIgnoreCase))) return 3;
Console.WriteLine("CORE SMOKE PASS: create/edit/save/reload/delete; legacy and shell/look settings; no Avalonia references or loaded assemblies.");
return 0;
