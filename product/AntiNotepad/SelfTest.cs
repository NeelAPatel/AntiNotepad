using AntiNotepad.Model;
using AntiNotepad.Platform;
using Avalonia.Platform;

namespace AntiNotepad;

/// <summary>Disposable capability smoke: calculator, mode detection, timer parsing, store round trip. `AntiNotepad --selftest`.</summary>
internal static class SelfTest
{
    public static int Run()
    {
        var failures = new List<string>();
        void Check(bool ok, string what) { if (!ok) failures.Add(what); }

        Check(Calc.Calculator.SelfTest(out var calcReport), "calculator: " + calcReport);
        Check(Editor.TextCommands.SelfTest(out var textReport), "text commands: " + textReport);
        Check(Modes.Detect("math\n1+1").Mode == NoteMode.Math, "detect math");
        Check(Modes.Detect("code: py\nx").Argument == "py", "detect code py");
        Check(Modes.Detect("Timer\n5m").Mode == NoteMode.Timer, "detect timer case-insensitive");
        Check(Modes.Detect("mathematics").Mode == NoteMode.Plain, "plain when not a keyword");
        Check(Timers.TryParse("25m focus", out var d1, out var l1) && d1 == TimeSpan.FromMinutes(25) && l1 == "focus", "timer 25m");
        Check(Timers.TryParse("1h 30m", out var d2, out _) && d2 == TimeSpan.FromMinutes(90), "timer 1h 30m");
        Check(Timers.TryParse("05:00 eggs", out var d3, out var l3) && d3 == TimeSpan.FromMinutes(5) && l3 == "eggs", "timer clock");
        Check(!Timers.TryParse("buy 5m of rope", out _, out _), "timer ignores mid-sentence");
        Check(Editor.LinkGenerator.Shorten("https://learn.microsoft.com/en-us/windows/apps/design/style/acrylic") == "learn.microsoft.com/…/acrylic", "link shorten");

        Check(AppInfoSelfTest(out var identityReport), "app identity: " + identityReport);

        Check(Calc.Calculator.EvaluateInline(Editor.PadView.BlankLabel("spent 12.50 + 8.25 + 3.99 ="))?.Value is > 24.73 and < 24.75, "inline with a label");
        Check(Calc.Calculator.EvaluateInline(Editor.PadView.StripListMarker("- 19.99 * 3 ="))?.Value is > 59.96 and < 59.98, "inline after a bullet");

        var dir = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-selftest-{Guid.NewGuid():N}");
        try
        {
            var store = new Store(dir);
            store.Load();
            store.Data.Stack[0].Text = "hello";
            store.Data.Slots[2].Text = "slot three";
            Check(store.Save() && store.Save(), "save twice (replace path)");
            var again = new Store(dir); again.Load();
            Check(again.Data.Stack[0].Text == "hello" && again.Data.Slots[2].Text == "slot three" && again.Data.Slots[2].Slot == 3, "store round trip");
            File.WriteAllText(Path.Combine(dir, "index.json"), "{ broken");
            var recovered = new Store(dir); recovered.Load();
            Check(recovered.Data.Stack[0].Text == "hello", "recovers from .bak");
        }
        finally { try { Directory.Delete(dir, true); } catch { } }

        Check(DefaultDataMigrationSelfTest(out var migrationReport), "default data folder migration: " + migrationReport);

        Console.WriteLine(failures.Count == 0 ? "SELFTEST PASS" : "SELFTEST FAIL\n" + string.Join("\n", failures));
        return failures.Count == 0 ? 0 : 1;
    }

    private static bool DefaultDataMigrationSelfTest(out string report)
    {
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-migration-selftest-{Guid.NewGuid():N}");
        try
        {
            // A successful move must preserve all nested files, including the legacy log and backups.
            var successRoot = Path.Combine(root, "success");
            var oldFolder = Path.Combine(successRoot, DefaultDataFolderMigration.LegacyFolderName);
            Directory.CreateDirectory(Path.Combine(oldFolder, "notes", "nested"));
            Directory.CreateDirectory(Path.Combine(oldFolder, "backups"));
            File.WriteAllText(Path.Combine(oldFolder, "index.json"), "index");
            // This legacy filename is deliberately kept in the fixture to prove migration preserves it byte-for-byte.
            File.WriteAllText(Path.Combine(oldFolder, "antipad.log"), "legacy log remains intact");
            File.WriteAllText(Path.Combine(oldFolder, "notes", "nested", "note.md"), "nested note");
            File.WriteAllText(Path.Combine(oldFolder, "backups", "index.json.bak"), "backup");
            File.WriteAllText(Path.Combine(oldFolder, "settings.json"), "settings");
            var moved = DefaultDataFolderMigration.Resolve(successRoot, null);
            var newFolder = Path.Combine(successRoot, AppInfo.DataFolderName);
            if (!moved.Migrated || moved.Folder != newFolder || Directory.Exists(oldFolder)
                || File.ReadAllText(Path.Combine(newFolder, "index.json")) != "index"
                || File.ReadAllText(Path.Combine(newFolder, "antipad.log")) != "legacy log remains intact"
                || File.ReadAllText(Path.Combine(newFolder, "notes", "nested", "note.md")) != "nested note"
                || File.ReadAllText(Path.Combine(newFolder, "backups", "index.json.bak")) != "backup"
                || File.ReadAllText(Path.Combine(newFolder, "settings.json")) != "settings")
            {
                report = "successful move did not preserve the complete old tree";
                return false;
            }

            // An existing destination wins without merging or changing either directory.
            var existingRoot = Path.Combine(root, "destination-exists");
            var existingOld = Path.Combine(existingRoot, DefaultDataFolderMigration.LegacyFolderName);
            var existingNew = Path.Combine(existingRoot, AppInfo.DataFolderName);
            Directory.CreateDirectory(existingOld);
            Directory.CreateDirectory(existingNew);
            File.WriteAllText(Path.Combine(existingOld, "old-only.txt"), "old");
            File.WriteAllText(Path.Combine(existingNew, "new-only.txt"), "new");
            var alreadyMoved = DefaultDataFolderMigration.Resolve(existingRoot, null);
            if (alreadyMoved.Folder != existingNew || alreadyMoved.Migrated
                || File.Exists(Path.Combine(existingNew, "old-only.txt"))
                || File.ReadAllText(Path.Combine(existingNew, "new-only.txt")) != "new"
                || File.ReadAllText(Path.Combine(existingOld, "old-only.txt")) != "old")
            {
                report = "existing destination was merged or altered";
                return false;
            }

            // A custom override is returned verbatim and does not inspect or alter either default folder.
            var customRoot = Path.Combine(root, "custom-override");
            var customOld = Path.Combine(customRoot, DefaultDataFolderMigration.LegacyFolderName);
            var customNew = Path.Combine(customRoot, AppInfo.DataFolderName);
            var customFolder = Path.Combine(root, "custom-path");
            Directory.CreateDirectory(customOld);
            Directory.CreateDirectory(customFolder);
            File.WriteAllText(Path.Combine(customOld, "old-only.txt"), "old");
            File.WriteAllText(Path.Combine(customFolder, "custom.txt"), "custom");
            var custom = DefaultDataFolderMigration.Resolve(customRoot, customFolder);
            if (custom.Folder != customFolder || custom.Migrated || custom.Error is not null
                || Directory.Exists(customNew) || File.ReadAllText(Path.Combine(customOld, "old-only.txt")) != "old"
                || File.ReadAllText(Path.Combine(customFolder, "custom.txt")) != "custom")
            {
                report = "custom override touched the default paths";
                return false;
            }

            // A failed atomic rename leaves the old tree intact and selects it for continued use.
            var failureRoot = Path.Combine(root, "move-fails");
            var failureOld = Path.Combine(failureRoot, DefaultDataFolderMigration.LegacyFolderName);
            var failureNew = Path.Combine(failureRoot, AppInfo.DataFolderName);
            Directory.CreateDirectory(Path.Combine(failureOld, "notes"));
            File.WriteAllText(Path.Combine(failureOld, "notes", "keep.md"), "keep");
            var failed = DefaultDataFolderMigration.Resolve(failureRoot, null,
                static (_, _) => throw new IOException("simulated move failure"));
            if (failed.Folder != failureOld || failed.Migrated || failed.Error is null
                || Directory.Exists(failureNew) || File.ReadAllText(Path.Combine(failureOld, "notes", "keep.md")) != "keep")
            {
                report = "failed move did not fall back to the intact old tree";
                return false;
            }

            report = "move, existing destination, custom override, and failure fallback";
            return true;
        }
        catch (Exception ex)
        {
            report = ex.Message;
            return false;
        }
        finally { try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { } }
    }

    private static bool AppInfoSelfTest(out string report)
    {
        try
        {
            var assembly = typeof(AppInfo).Assembly;
            var product = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), false)
                .Cast<System.Reflection.AssemblyProductAttribute>().Single().Product;
            var title = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false)
                .Cast<System.Reflection.AssemblyTitleAttribute>().Single().Title;
            var informationalVersion = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
                .Cast<System.Reflection.AssemblyInformationalVersionAttribute>().Single().InformationalVersion;
            var assemblyVersion = assembly.GetName().Version?.ToString();
            var fileVersion = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), false)
                .Cast<System.Reflection.AssemblyFileVersionAttribute>().Single().Version;
            var description = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false)
                .Cast<System.Reflection.AssemblyDescriptionAttribute>().Single().Description;
            var copyright = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false)
                .Cast<System.Reflection.AssemblyCopyrightAttribute>().Single().Copyright;

            if (AppInfo.Name != product || AppInfo.Name != title || assembly.GetName().Name != AppInfo.Name
                || AppInfo.InformationalVersion != informationalVersion
                || !informationalVersion.StartsWith(AppInfo.Version, StringComparison.Ordinal)
                || assemblyVersion != $"{AppInfo.Version}.0" || fileVersion != $"{AppInfo.Version}.0"
                || AppInfo.Description != description || AppInfo.Copyright != copyright
                || AppInfo.InstanceId != AppInfo.Name || AppInfo.DevInstanceId != $"{AppInfo.Name}-dev"
                || AppInfo.DataFolderName != AppInfo.Name || AppInfo.RunValueName != AppInfo.Name
                || AppInfo.LogFileName != $"{AppInfo.Name.ToLowerInvariant()}.log"
                || AppInfo.EnvPrefix != AppInfo.Name.ToUpperInvariant()
                || AppInfo.Env("DATA") != $"{AppInfo.EnvPrefix}_DATA"
                || AppInfo.Version + (AppInfo.Label.Length == 0 ? "" : $"-{AppInfo.Label}") != informationalVersion)
            {
                report = "AppInfo does not match the generated assembly identity or its derived identifiers";
                return false;
            }

            // Use the file resource loader directly: selftest never initializes the desktop platform.
            var assets = new StandardAssetLoader();
            using var icon = assets.Open(new Uri(AppInfo.IconUri));
            using var font = assets.Open(new Uri(AppInfo.FontUri));
            if (icon.ReadByte() < 0 || font.ReadByte() < 0)
            {
                report = "bundled icon or font resource is empty";
                return false;
            }

            report = "assembly attributes, derived identifiers, and bundled icon/font URIs";
            return true;
        }
        catch (Exception ex)
        {
            report = ex.Message;
            return false;
        }
    }
}
