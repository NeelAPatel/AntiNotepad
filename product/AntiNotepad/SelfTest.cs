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
        Check(InputLimitChecks(out var inputReport), "input limits: " + inputReport);
        Check(NotesLocationChecks(out var notesReport), "notes location and recovery: " + notesReport);
        Check(StartupRequestLatchChecks(out var startupRequestReport), "startup request latch: " + startupRequestReport);
        Check(NoteSwipeChecks(out var swipeReport), "note swipe: " + swipeReport);
        Check(WelcomeSlotsSelfTest(out var welcomeReport), "welcome slots: " + welcomeReport);

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

    private static bool InputLimitChecks(out string report)
    {
        var failures = new List<string>();
        void Check(bool ok, string what) { if (!ok) failures.Add(what); }

        try
        {
            Check(!Editor.PadView.TryListNumber("123456789012. item", out _), "12-digit list marker is not continued or renumbered");

            Check(!Timers.TryParse("999999999999999999999h", out _, out _), "oversized timer is rejected");
            Check(!Timers.TryParse("\u0665m", out _, out _), "non-ASCII timer digits are rejected");
            Check(!Timers.TryParse("1000h", out _, out _), "1000-hour timer is rejected");
            Check(!Timers.TryParse("1000:00:00", out _, out _), "1000-hour clock timer is rejected");
            Check(!Timers.TryParse("999h60m", out _, out _), "timer beyond the maximum is rejected");
            Check(Timers.TryParse("999h59m59s", out var maxParts, out _) && maxParts == TimeSpan.FromSeconds(3_599_999), "999h59m59s timer is accepted");
            Check(Timers.TryParse("999:59:59", out var maxClock, out _) && maxClock == TimeSpan.FromSeconds(3_599_999), "999:59:59 clock timer is accepted");
            Check(Timers.Format(TimeSpan.FromSeconds(3_599_999)) == "999:59:59", "long timer formatting preserves three-digit hours");

            var deep = new string('(', 65) + "1" + new string(')', 65);
            Check(Calc.Calculator.EvaluateNote(["math", deep])[1]?.Result is null, "65-deep parentheses are invalid");
            Check(Calc.Calculator.EvaluateNote(["math", new string('(', 64) + "1" + new string(')', 64)])[1]?.Result?.Value == 1, "64-deep parentheses remain valid");
            Check(Calc.Calculator.EvaluateNote(["math", new string('-', 65) + "1"])[1]?.Result is null, "65 chained unary signs are invalid");

            Calc.CalcResult? One(string expression) => Calc.Calculator.EvaluateNote(["math", expression])[1]?.Result;
            Calc.LineEval? OneLine(string expression) => Calc.Calculator.EvaluateNote(["math", expression])[1];
            Check(One("$5 + €3") is null, "mixed-currency addition is invalid");
            Check(OneLine("$5 + €3")?.ErrorReason == Calc.CalcErrorReason.MixedCurrencies, "mixed-currency addition has an explicit error reason");
            Check(Calc.Calculator.EvaluateInlineLine("$5 + €3 =")?.ErrorReason == Calc.CalcErrorReason.MixedCurrencies, "inline mixed-currency addition has an explicit error reason");
            Check(One("$5 + $3")?.Display == "$8", "same-currency addition is preserved");
            Check(One("$5 + 3")?.Display == "$8", "plain number plus money keeps its currency");
            Check(One("$5 + €3%") is null, "mixed-currency percent addition is invalid");
            Check(One("min($5, €3)") is null && One("sum($5, €3)") is null, "mixed-currency comparisons and aggregates are invalid");
            Check(Calc.Calculator.EvaluateNote(["math", "$5", "€3", "sum"])[3]?.Result is null, "mixed-currency note aggregate is invalid");

            bool parsedFirst = Timers.TryParse("5m tea", out var firstDuration, out var firstLabel, out var firstText);
            bool parsedSecond = Timers.TryParse("5m coffee", out var secondDuration, out var secondLabel, out var secondText);
            var firstKey = Editor.PadView.TimerKey("selftest", firstText, 1);
            var secondKey = Editor.PadView.TimerKey("selftest", secondText, 1);
            Check(parsedFirst && parsedSecond && firstDuration == secondDuration && firstLabel != secondLabel && firstKey == secondKey,
                "timer key survives a label edit");

            var timers = new Timers(id => id == "live");
            var removed = timers.Ensure(firstKey, firstDuration, firstLabel, "live");
            removed.EndsAt = DateTime.Now.AddMinutes(5);
            Check(timers.Reconcile("live", Array.Empty<string>()) == 1 && timers.Get(firstKey) is null && timers.EntryCount == 0 && !removed.Running,
                "removed timer line stops and drops its entry");
            timers.Ensure("deleted:5m:1", firstDuration, firstLabel, "deleted");
            timers.Reconcile("live", Array.Empty<string>());
            Check(timers.Get("deleted:5m:1") is null, "deleted note timer is pruned");
            Check(!Timers.TryEndAt(DateTime.MaxValue, TimeSpan.FromSeconds(1), out _), "timer end overflow is rejected");

            report = failures.Count == 0 ? "list limits, timer bounds and identity, calculator depth and currencies, timer pruning and end overflow" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = "unexpected exception: " + ex.Message;
            return false;
        }
    }

    private static bool NotesLocationChecks(out string report)
    {
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-notes-location-selftest-{Guid.NewGuid():N}");
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        try
        {
            var freshHome = Path.Combine(root, "fresh-home");
            var freshDocuments = Path.Combine(root, "fresh-documents");
            var fresh = NotesLocation.Resolve(freshHome, freshDocuments);
            var freshExpected = Path.Combine(freshDocuments, AppInfo.DataFolderName);
            CheckCase(fresh.Error is null && fresh.NotesFolder == freshExpected && Directory.Exists(freshExpected)
                && NotesLocation.ReadPointer(freshHome) == freshExpected, "fresh install writes the Documents pointer");

            var moveHome = Path.Combine(root, "move-home");
            var moveDocuments = Path.Combine(root, "move-documents");
            var moveTarget = Path.Combine(moveDocuments, AppInfo.DataFolderName);
            Directory.CreateDirectory(Path.Combine(moveHome, "notes"));
            Directory.CreateDirectory(Path.Combine(moveHome, "recovered"));
            File.WriteAllText(Path.Combine(moveHome, "index.json"), "index bytes");
            File.WriteAllText(Path.Combine(moveHome, "index.json.bak"), "backup bytes");
            File.WriteAllText(Path.Combine(moveHome, "notes", "note1.md"), "a note body");
            File.WriteAllText(Path.Combine(moveHome, "recovered", "orphan.md"), "recovered body");
            File.WriteAllText(Path.Combine(moveHome, AppInfo.LogFileName), "the app log stays home");
            var moved = NotesLocation.Resolve(moveHome, moveDocuments);
            var homeBackup = Directory.EnumerateDirectories(moveHome, "backup-before-move-*").SingleOrDefault();
            CheckCase(moved.Error is null && moved.NotesFolder == moveTarget && moved.Notice is null
                && NotesLocation.ReadPointer(moveHome) == moveTarget
                && File.ReadAllText(Path.Combine(moveTarget, "index.json")) == "index bytes"
                && File.ReadAllText(Path.Combine(moveTarget, "index.json.bak")) == "backup bytes"
                && File.ReadAllText(Path.Combine(moveTarget, "notes", "note1.md")) == "a note body"
                && File.ReadAllText(Path.Combine(moveTarget, "recovered", "orphan.md")) == "recovered body"
                && File.Exists(Path.Combine(moveHome, AppInfo.LogFileName))
                && homeBackup is not null && File.Exists(Path.Combine(homeBackup, "index.json")), "first-run move verifies copies and leaves an app-home backup");

            var occupiedHome = Path.Combine(root, "occupied-home");
            var occupiedDocuments = Path.Combine(root, "occupied-documents");
            var occupiedTarget = Path.Combine(occupiedDocuments, AppInfo.DataFolderName);
            Directory.CreateDirectory(Path.Combine(occupiedHome, "notes"));
            Directory.CreateDirectory(occupiedTarget);
            File.WriteAllText(Path.Combine(occupiedHome, "index.json"), "keep old");
            File.WriteAllText(Path.Combine(occupiedTarget, "index.json"), "keep target");
            var occupied = NotesLocation.Resolve(occupiedHome, occupiedDocuments);
            CheckCase(occupied.NotesFolder == occupiedHome && occupied.Notice is not null && NotesLocation.ReadPointer(occupiedHome) is null
                && File.ReadAllText(Path.Combine(occupiedHome, "index.json")) == "keep old"
                && File.ReadAllText(Path.Combine(occupiedTarget, "index.json")) == "keep target", "existing target index is refused");

            var missingHome = Path.Combine(root, "missing-home");
            var missingTarget = Path.Combine(root, "missing-target");
            Directory.CreateDirectory(missingHome);
            File.WriteAllText(Path.Combine(missingHome, NotesLocation.PointerFileName), System.Text.Json.JsonSerializer.Serialize(new { NotesFolder = missingTarget }));
            var missing = NotesLocation.Resolve(missingHome, Path.Combine(root, "unused-documents"));
            CheckCase(missing.Error is not null && missing.NotesFolder == missingTarget && !Directory.Exists(missingTarget), "missing pointer target is not created");

            var recoveryFolder = Path.Combine(root, "unknown-body");
            var unknownStore = new Store(recoveryFolder);
            unknownStore.Load();
            unknownStore.Data.Stack[0].Text = "kept note";
            CheckCase(unknownStore.Save(), "fixture note saves for orphan recovery");
            var unknownBody = Path.Combine(recoveryFolder, "notes", "unknown.md");
            File.WriteAllText(unknownBody, "unknown body survives");
            var recoveryStore = new Store(recoveryFolder);
            recoveryStore.Load();
            var recoveredBody = Path.Combine(recoveryFolder, "recovered", "unknown.md");
            CheckCase(!File.Exists(unknownBody) && File.ReadAllText(recoveredBody) == "unknown body survives", "unknown body moves to recovered");

            var deleteFolder = Path.Combine(root, "delete-forever");
            var deleteStore = new Store(deleteFolder);
            deleteStore.Load();
            var deleted = deleteStore.Data.Stack[0];
            deleted.Text = "delete forever body";
            CheckCase(deleteStore.Save(), "fixture note saves for delete-forever");
            var deletedBody = Path.Combine(deleteFolder, "notes", deleted.Id + ".md");
            deleteStore.Data.Stack.Remove(deleted);
            deleted.Deleted = DateTime.Now;
            deleteStore.Data.Trash.Add(deleted);
            CheckCase(deleteStore.Save() && File.Exists(deletedBody), "trash entry keeps its body");
            deleteStore.Data.Trash.Remove(deleted);
            CheckCase(deleteStore.Save() && !File.Exists(deletedBody), "delete-forever removes its own body");

            var fallbackFolder = Path.Combine(root, "invalid-index");
            var fallbackStore = new Store(fallbackFolder);
            fallbackStore.Load();
            fallbackStore.Data.Stack[0].Text = "valid backup";
            fallbackStore.Save();
            fallbackStore.Save();
            File.WriteAllText(Path.Combine(fallbackFolder, "index.json"), "{\"Settings\":null}");
            var fallback = new Store(fallbackFolder);
            fallback.Load();
            CheckCase(fallback.Data.Settings is not null && fallback.Data.Stack[0].Text == "valid backup", "null Settings falls back to index backup");

            var missingBodyFolder = Path.Combine(root, "missing-body");
            var missingBodyStore = new Store(missingBodyFolder);
            missingBodyStore.Load();
            var missingNote = missingBodyStore.Data.Stack[0];
            missingNote.Text = "nonblank note";
            missingBodyStore.Save();
            var bodyPath = Path.Combine(missingBodyFolder, "notes", missingNote.Id + ".md");
            File.Delete(bodyPath);
            var missingBodyReload = new Store(missingBodyFolder);
            missingBodyReload.Load();
            var lostText = missingBodyReload.Data.Stack[0];
            var preview = lostText.PeekText();
            lostText.Text = "must not overwrite";
            lostText.Touched(blank: false, NoteMode.Plain);
            CheckCase(preview.Length > 0 && lostText.BodyUnavailable && !lostText.Dirty
                && missingBodyReload.Save() && !File.Exists(bodyPath), "missing nonblank body stays unavailable and is not saved");

            var bypassHome = Path.Combine(root, "bypass-home");
            var bypassDocuments = Path.Combine(root, "bypass-documents");
            var custom = Path.Combine(root, "custom-data");
            Directory.CreateDirectory(bypassHome);
            File.WriteAllText(Path.Combine(bypassHome, "index.json"), "must not inspect");
            var bypass = NotesLocation.Resolve(bypassHome, bypassDocuments, custom);
            CheckCase(bypass.Custom && bypass.AppHome == custom && bypass.NotesFolder == custom
                && !Directory.Exists(custom) && !Directory.Exists(bypassDocuments)
                && File.ReadAllText(Path.Combine(bypassHome, "index.json")) == "must not inspect", "ANTINOTEPAD_DATA bypasses Documents and first-run movement");

            var movedDev = Path.Combine(root, "custom-moved");
            Directory.CreateDirectory(custom); Directory.CreateDirectory(movedDev);
            File.WriteAllText(Path.Combine(custom, NotesLocation.PointerFileName), System.Text.Json.JsonSerializer.Serialize(new { NotesFolder = movedDev }));
            var followed = NotesLocation.Resolve(bypassHome, bypassDocuments, custom);
            CheckCase(followed.Custom && followed.AppHome == custom && followed.NotesFolder == movedDev && followed.Error is null
                && !Directory.Exists(bypassDocuments), "ANTINOTEPAD_DATA follows its own pointer after an in-app move");

            report = failures.Count == 0 ? "fresh location, verified move and backup, collision refusal, missing pointer, recovered unknown, direct deletion, invalid-index fallback, unavailable body, and custom-path bypass" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.Message));
            return false;
        }
        finally { try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { } }
    }

    private static bool StartupRequestLatchChecks(out string report)
    {
        var requests = new StartupRequestLatch();
        var shows = 0;
        var quits = 0;
        Action dispatchShow = () => shows++;
        Action dispatchQuit = () => quits++;

        requests.RequestShow(dispatchShow);
        requests.RequestQuit(dispatchQuit);
        var heldUntilReady = shows == 0 && quits == 0;
        requests.Attach(dispatchShow, dispatchQuit);
        var pendingConsumed = shows == 1 && quits == 1;
        requests.Attach(dispatchShow, dispatchQuit);
        var consumedOnce = shows == 1 && quits == 1;

        requests.RequestShow(dispatchShow);
        requests.RequestQuit(dispatchQuit);
        var readyRequestsDispatched = shows == 2 && quits == 2;

        report = heldUntilReady && pendingConsumed && consumedOnce && readyRequestsDispatched
            ? "early show and quit wait for attach, pending requests dispatch once, and ready requests dispatch directly"
            : $"early={heldUntilReady}, consumed={pendingConsumed}, once={consumedOnce}, ready={readyRequestsDispatched}";
        return heldUntilReady && pendingConsumed && consumedOnce && readyRequestsDispatched;
    }

    private static bool NoteSwipeChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        var settings = new Settings { Swipe = "progress" };
        var pane = new Avalonia.Controls.Control();
        var cancelled = new UI.NoteSwipe(pane, () => settings, () => pane, _ => { });
        var stalled = new UI.NoteSwipe(pane, () => settings, () => pane, _ => { });
        try
        {
            cancelled.Input(1);
            CheckCase(cancelled.OwedMotion > 0.5 && cancelled.HasRunningTimer, "no-frame swipe leaves owed motion and starts settle timer");
            cancelled.Cancel();
            CheckCase(!cancelled.HasRunningTimer && !cancelled.HasPendingProgress && cancelled.OwedMotion == 0,
                "cancel stops settle timer and clears pending progress");

            stalled.Input(1);
            var now = Environment.TickCount64;
            var waits = !stalled.ProgressSettleTick(now) && stalled.HasRunningTimer && stalled.OwedMotion > 0.5;
            CheckCase(waits, "settle waits while owed motion is recent");
            var gaveUp = stalled.ProgressSettleTick(now + UI.NoteSwipe.ProgressStallMs)
                && !stalled.HasRunningTimer && !stalled.HasPendingProgress && stalled.OwedMotion == 0;
            CheckCase(gaveUp, "settle drops stalled owed motion after the bound");

            report = failures.Count == 0 ? "no-frame cancel cleanup and deterministic stalled-settle timeout" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.Message));
            return false;
        }
        finally
        {
            cancelled.Cancel();
            stalled.Cancel();
        }
    }

    private static bool WelcomeSlotsSelfTest(out string report)
    {
        var fresh = new StoreData();
        Welcome.SeedIfNew(fresh);
        var expected = new[] { "# h1", "math", "list", "timer", $"# Welcome to {AppInfo.Name}" };
        var seeded = expected.Select((line, i) => fresh.Slots[i].Text.Split('\n')[0] == line).All(ok => ok)
            && fresh.Slots.Take(5).All(note => !note.IsEmpty)
            && fresh.Slots.Skip(5).All(note => note.IsEmpty)
            && fresh.Stack.Count == 0
            && fresh.ViewSlot == 5;
        var timerLines = fresh.Slots[3].Text.Replace("\r", "").Split('\n');
        var timersParse = timerLines.Length >= 6
            && Enumerable.Range(1, 5).All(i => Timers.TryParse(timerLines[i], out _, out _));

        var occupied = new StoreData();
        occupied.Slots[0].Text = "keep this note\nwith its body";
        Welcome.SeedIfNew(occupied);
        var preserved = occupied.Slots[0].Text == "keep this note\nwith its body";
        var beforeSecondCall = occupied.Slots.Select(note => note.Text).ToArray();
        var viewBeforeSecondCall = occupied.ViewSlot;
        Welcome.SeedIfNew(occupied);
        var secondCallNoOp = occupied.Slots.Select(note => note.Text).SequenceEqual(beforeSecondCall)
            && occupied.ViewSlot == viewBeforeSecondCall;
        var defaults = new Settings() is { Scrolling: "rate", Swipe: "progress" };

        report = "slots 1-5 seeded, 6-9 empty, stack empty, opens slot 5; occupied slot preserved; second call unchanged; five timers parse; rate/progress defaults";
        return seeded && timersParse && preserved && secondCallNoOp && defaults;
    }
}
