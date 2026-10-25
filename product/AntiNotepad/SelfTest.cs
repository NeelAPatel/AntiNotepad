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
        Check(DevIdentitySelfTest(out var devIdentityReport), "dev identity: " + devIdentityReport);

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
        Check(SearchSpeedChecks(out var searchReport), "search speed: " + searchReport);
        Check(SettingsSearchMatchingChecks(out var settingsSearchReport), "A27-settings-search-build-matches: " + settingsSearchReport);
        Check(FenceCacheChecks(out var fenceCacheReport), "A28-fence-cache: " + fenceCacheReport);
        Check(IncrementalAnalysisChecks(out var analysisReport), "A29-incremental-analysis: " + analysisReport);
        Check(TaskbarChecks(out var taskbarReport), "taskbar: " + taskbarReport);
        Check(SessionEndCloseChecks(out var sessionEndReport), "session-end close: " + sessionEndReport);
        Check(BackgroundSaveAndLogChecks(out var backgroundSaveAndLogReport), "background save and log: " + backgroundSaveAndLogReport);
        Check(NativeTransitionStyleChecks(out var nativeTransitionStyleReport), "native transition styles: " + nativeTransitionStyleReport);

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

            if (AppInfo.DisplayName != product || AppInfo.DisplayName != title || assembly.GetName().Name != AppInfo.Name
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

    private static bool SearchSpeedChecks(out string report)
    {
        var root = Path.Combine(Path.GetTempPath(), $"antinotepad-search-selftest-{Guid.NewGuid():N}");
        var failures = new List<string>();
        var originalLoader = Note.LoadBody;
        var originalInvalidator = Note.InvalidateSearchBody;
        void CheckCase(bool ok, string name) { if (!ok) failures.Add(name); }
        try
        {
            var store = new Store(root);
            store.Load();
            store.Data.Stack.Clear();
            for (var i = 0; i < 200; i++)
                store.Data.Stack.Add(new Note { Text = $"Fixture {i}\nneedle shared " + new string('x', i % 3 == 0 ? 32768 : i % 3 == 1 ? 4096 : 128) });
            CheckCase(store.Save(), "fixture saves");
            store = new Store(root);
            store.Load();
            var notes = store.Data.Stack.ToArray();
            for (var i = 0; i < notes.Length; i += 2) _ = notes[i].Text;
            var terms = new[] { "needle", "shared" };
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var beforeMatches = notes.Count(n => terms.All(t => n.PeekText().Contains(t, StringComparison.CurrentCultureIgnoreCase)));
            timer.Stop();
            var beforeMs = timer.Elapsed.TotalMilliseconds;
            store.ResetSearchBodyReadCount();
            timer.Restart();
            var snapshot = store.CreateSearchSnapshot(notes);
            var context = store.CreateSearchContext(snapshot);
            timer.Stop();
            var afterMs = timer.Elapsed.TotalMilliseconds;
            CheckCase(store.SearchBodyReadCount == 0, "snapshot performs no disk reads");
            timer.Restart();
            var matches = Task.Run(() => Store.MatchSearch(context, "needle shared", CancellationToken.None)).GetAwaiter().GetResult();
            timer.Stop();
            var coldMs = timer.Elapsed.TotalMilliseconds;
            CheckCase(beforeMatches == 200 && matches.Length == 200 && matches.Any(m => m.Id == notes[0].Id) && matches.Any(m => m.Id == notes[1].Id), "loaded and unloaded matches");
            CheckCase(store.SearchBodyReadCount == 100, "one read per unloaded body for multi-term query and snippets");
            _ = Task.Run(() => Store.MatchSearch(context, "shared", CancellationToken.None)).GetAwaiter().GetResult();
            CheckCase(store.SearchBodyReadCount == 100, "unchanged query reuses body cache");

            notes[1].Text = "edited body\nreplacement";
            var edited = store.CreateSearchContext(store.CreateSearchSnapshot(notes));
            CheckCase(Store.MatchSearch(edited, "replacement", CancellationToken.None).Any(m => m.Id == notes[1].Id)
                && Store.MatchSearch(edited, "needle", CancellationToken.None).All(m => m.Id != notes[1].Id), "in-memory edit invalidates cached text");
            CheckCase(store.Save(), "edited fixture saves");
            notes[1].Detach();
            var saved = store.CreateSearchContext(store.CreateSearchSnapshot(notes));
            CheckCase(Store.MatchSearch(saved, "replacement", CancellationToken.None).Any(m => m.Id == notes[1].Id), "saved edit replaces cached text");

            var lost = notes[3];
            var bodyPath = Path.Combine(root, "notes", lost.Id + ".md");
            File.Delete(bodyPath);
            // A fresh store has no search cache and leaves the unavailable body unloaded.
            store = new Store(root); store.Load();
            lost = store.Data.Stack.Single(n => n.Id == lost.Id);
            var unavailable = store.CreateSearchContext(store.CreateSearchSnapshot([lost]));
            var missing = Store.MatchSearch(unavailable, "Fixture 3", CancellationToken.None);
            var reads = store.SearchBodyReadCount;
            _ = Store.MatchSearch(unavailable, "Fixture 3", CancellationToken.None);
            CheckCase(missing.Length == 1 && missing[0].BodyStatus == BodyReadStatus.Missing && store.SearchBodyReadCount == reads + 1,
                "missing body matches title, retains status, and is not cached as empty");
            CheckCase(!lost.Dirty && store.Save() && !File.Exists(bodyPath), "search does not dirty or save missing body");

            var locked = store.Data.Stack[5];
            var lockedPath = Path.Combine(root, "notes", locked.Id + ".md");
            var originalBody = File.ReadAllText(lockedPath);
            using (var held = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var blocked = store.CreateSearchContext(store.CreateSearchSnapshot([locked]));
                var unreadable = Task.Run(() => Store.MatchSearch(blocked, "Fixture 5", CancellationToken.None)).GetAwaiter().GetResult();
                CheckCase(unreadable.Length == 1 && unreadable[0].BodyStatus == BodyReadStatus.Unreadable && !locked.Dirty && store.Save(), "unreadable body remains distinct and unsaved");
            }
            CheckCase(File.ReadAllText(lockedPath) == originalBody, "unreadable body preserved byte for byte");

            using (var generation = new SearchGeneration())
            using (var first = generation.Begin())
            {
                var oldResults = Store.MatchSearch(saved, "needle", first.Token);
                using var second = generation.Begin();
                var published = Store.MatchSearch(saved, "replacement", second.Token);
                var cancellationObserved = false;
                try { _ = Store.MatchSearch(saved, "needle", first.Token); }
                catch (OperationCanceledException) { cancellationObserved = true; }
                if (generation.IsCurrent(first)) published = oldResults;
                CheckCase(cancellationObserved && !generation.IsCurrent(first) && generation.IsCurrent(second)
                    && published.Length == 1 && published[0].Id == notes[1].Id, "cancelled stale query cannot replace newer results");
                generation.Cancel();
                CheckCase(!generation.IsCurrent(second), "closing invalidates pending publication");
            }
            Console.WriteLine($"A18-search-speed timing: 200 notes (100 unloaded), old UI filter {beforeMs:F3} ms; snapshot/context {afterMs:F3} ms, 0 disk reads; cold background query {coldMs:F3} ms, 100 disk reads.");
            report = failures.Count == 0 ? "loaded/unloaded, read count, cache edit/save, unavailable bodies" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex) { report = string.Join("; ", failures.Append(ex.ToString())); return false; }
        finally
        {
            Note.LoadBody = originalLoader;
            Note.InvalidateSearchBody = originalInvalidator;
            try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
        }
    }

    private static bool DevIdentitySelfTest(out string report)
    {
        try
        {
            var expectedName = AppInfo.IsDevBuild ? "AntiNotepad (dev)" : "AntiNotepad";
            var expectedIcon = AppInfo.IsDevBuild ? "antinotepad-dev.ico" : "antinotepad.ico";
            if (AppInfo.DisplayName != expectedName || AppInfo.IconUri != $"avares://AntiNotepad/{expectedIcon}"
                || AppInfo.Name != "AntiNotepad" || AppInfo.InstanceId != "AntiNotepad"
                || AppInfo.DevInstanceId != "AntiNotepad-dev" || AppInfo.DataFolderName != "AntiNotepad"
                || AppInfo.RunValueName != "AntiNotepad" || AppInfo.LogFileName != "antinotepad.log"
                || AppInfo.EnvPrefix != "ANTINOTEPAD" || AppInfo.ResourceUri != "avares://AntiNotepad/")
            {
                report = "display identity or stable storage/resource identity differs from the build contract";
                return false;
            }

            var assets = new StandardAssetLoader();
            foreach (var file in new[] { "antinotepad.ico", "antinotepad-dev.ico" })
            {
                using var stream = assets.Open(new Uri($"avares://AntiNotepad/{file}"));
                using var reader = new BinaryReader(stream);
                if (reader.ReadUInt16() != 0 || reader.ReadUInt16() != 1 || reader.ReadUInt16() == 0)
                    throw new InvalidOperationException($"Invalid icon: {file}");
            }
            report = $"{expectedName}, {AppInfo.IconUri}; both icons load; stable instance/data/resource identities";
            Console.WriteLine("IDENTITY " + report);
            return true;
        }
        catch (Exception ex)
        {
            report = ex.Message;
            return false;
        }
    }

    private static bool TaskbarChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        CheckCase(new Settings().Taskbar == "never", "new settings default to never");
        CheckCase(System.Text.Json.JsonSerializer.Deserialize<Settings>("{}")?.Taskbar == "never", "old JSON defaults to never");
        foreach (var setting in new[] { "never", "visible", "always" })
        {
            var json = System.Text.Json.JsonSerializer.Serialize(new StoreData { Settings = new Settings { Taskbar = setting } });
            CheckCase(System.Text.Json.JsonSerializer.Deserialize<StoreData>(json)?.Settings.Taskbar == setting, $"{setting} JSON round trip");
        }
        (string Setting, bool Visible, bool Expected)[] cases =
        [
            ("never", false, false), ("never", true, false),
            ("visible", false, false), ("visible", true, true),
            ("always", false, true), ("always", true, true),
        ];
        foreach (var test in cases)
            CheckCase(TaskbarPresence.ShouldShow(test.Setting, test.Visible) == test.Expected, $"{test.Setting}, visible={test.Visible}");
        foreach (var setting in new string?[] { null, "unknown" })
            foreach (var visible in new[] { false, true })
                CheckCase(!TaskbarPresence.ShouldShow(setting, visible), "missing/unknown mode has no button");
        CheckCase(!TaskbarPresence.ShouldHideOnClick(false, false), "hidden inactive pad click shows");
        CheckCase(!TaskbarPresence.ShouldHideOnClick(false, true), "hidden pad click shows even with stale foreground state");
        CheckCase(!TaskbarPresence.ShouldHideOnClick(true, false), "visible inactive pad click focuses");
        CheckCase(TaskbarPresence.ShouldHideOnClick(true, true), "visible foreground pad click hides");
        var lastLoss = 100L;
        var withinGrace = lastLoss + System.Diagnostics.Stopwatch.Frequency / 2;
        var outsideGrace = withinGrace + 1;
        CheckCase(TaskbarPresence.WasPadRecentlyForeground(lastLoss, withinGrace, TaskbarActivationCounterpart.Taskbar), "recent loss to taskbar counts as foreground");
        CheckCase(TaskbarPresence.WasPadRecentlyForeground(lastLoss, withinGrace, TaskbarActivationCounterpart.Proxy), "recent loss to proxy counts as foreground");
        CheckCase(TaskbarPresence.WasPadRecentlyForeground(lastLoss, withinGrace, TaskbarActivationCounterpart.None), "null counterpart counts through 500 ms");
        CheckCase(!TaskbarPresence.WasPadRecentlyForeground(lastLoss, withinGrace, TaskbarActivationCounterpart.Other), "recent loss to another window does not count");
        CheckCase(!TaskbarPresence.WasPadRecentlyForeground(lastLoss, outsideGrace, TaskbarActivationCounterpart.Taskbar), "taskbar loss expires after 500 ms");
        CheckCase(!TaskbarPresence.WasPadRecentlyForeground(withinGrace, lastLoss, TaskbarActivationCounterpart.Taskbar), "future foreground-loss time is rejected");
        var firstProxyMessage = lastLoss + System.Diagnostics.Stopwatch.Frequency * 17 / 1000;
        var snapshotPadWasForeground = TaskbarPresence.WasPadRecentlyForeground(lastLoss, firstProxyMessage, TaskbarActivationCounterpart.None);
        CheckCase(TaskbarPresence.ShouldHideOnClick(true, snapshotPadWasForeground), "null counterpart loss 17 ms before first proxy message hides visible pad");
        CheckCase(!TaskbarPresence.ShouldHideOnClick(false, snapshotPadWasForeground), "hidden pad still shows after recent null counterpart loss");
        var oldNullLoss = TaskbarPresence.WasPadRecentlyForeground(lastLoss, outsideGrace, TaskbarActivationCounterpart.None);
        CheckCase(!TaskbarPresence.ShouldHideOnClick(true, oldNullLoss), "old null counterpart loss leaves visible inactive pad to focus");
        CheckCase(!TaskbarPresence.ShouldHideOnClick(false, oldNullLoss), "old null counterpart loss leaves hidden pad to show");
        var hideDecision = 200L;
        var withinHideFallback = hideDecision + System.Diagnostics.Stopwatch.Frequency;
        var afterHideFallback = withinHideFallback + 1;
        CheckCase(TaskbarPresence.ShouldSuppressPostHideActivation(true, true, false, false, hideDecision, withinHideFallback), "post-hide activation fallback is suppressed");
        CheckCase(!TaskbarPresence.ShouldSuppressPostHideActivation(true, true, true, false, hideDecision, withinHideFallback), "fresh click start permits a new hidden-pad click");
        CheckCase(!TaskbarPresence.ShouldSuppressPostHideActivation(true, true, false, true, hideDecision, withinHideFallback), "explicit restore permits a new hidden-pad click");
        CheckCase(!TaskbarPresence.ShouldSuppressPostHideActivation(true, true, false, false, hideDecision, afterHideFallback), "post-hide suppression expires after one second");
        CheckCase(!TaskbarPresence.ShouldSuppressPostHideActivation(true, false, false, false, hideDecision, withinHideFallback), "focus decision does not suppress a later click");
        report = failures.Count == 0 ? "Never default, old JSON default, three JSON round trips, all six visibility combinations, missing/unknown modes, all four click states, recent foreground-loss attribution including null counterpart at 17 ms and expiry, and post-hide activation-fallback suppression" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static bool SessionEndCloseChecks(out string report)
    {
        var failures = new List<string>();
        foreach (var reason in Enum.GetValues<Avalonia.Controls.WindowCloseReason>())
        {
            // Covers pad X/Alt+F4 and stand-in closes, explicit quit/disposal, shutdown/restart,
            // and sign-out (Avalonia 12.1.4 uses ApplicationShutdown for nonzero end-session flags).
            if (SessionEnd.ShouldCancelClose(reason, exiting: false, sessionEnding: false)
                != (reason != Avalonia.Controls.WindowCloseReason.OSShutdown)) failures.Add($"ordinary {reason}");
            if (SessionEnd.ShouldCancelClose(reason, exiting: false, sessionEnding: true)) failures.Add($"session end {reason}");
            if (SessionEnd.ShouldCancelClose(reason, exiting: true, sessionEnding: false)) failures.Add($"quit/dispose {reason}");
            if (SessionEnd.ShouldCancelClose(reason, exiting: true, sessionEnding: true)) failures.Add($"quit during session end {reason}");
        }
        report = failures.Count == 0 ? "normal resident closes cancel; quit, OS shutdown and session-end sign-out closes proceed" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static bool SettingsSearchMatchingChecks(out string report)
    {
        var matchedCaseInsensitively = UI.SettingsWindow.SearchMatches("sub Light mode palette", "PALETTE");
        var ignoredPrefix = !UI.SettingsWindow.SearchMatches("sub Light mode palette", "sub");
        var nullQueryMatches = UI.SettingsWindow.SearchMatches(null, null);
        var passed = matchedCaseInsensitively && ignoredPrefix && nullQueryMatches;
        report = passed ? "case-insensitive match, ignored sub prefix, and null query" : "search match semantics changed";
        return passed;
    }

    private static bool FenceCacheChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        try
        {
            var palette = Theme.Palette.All[0];
            var doc = new AvaloniaEdit.Document.TextDocument("```cs\nint first = 1;\n```\n\n```js\nlet second = 2;\n```");
            var original = Editor.NoteStructure.Build(["```cs", "int first = 1;", "```", "", "```js", "let second = 2;", "```"]);
            var cache = new Editor.FenceCache<object>();
            var copies = 0;
            var builds = 0;
            object Create(string _) { builds++; return new object(); }
            string Copy(string text) { copies++; return text; }

            foreach (var fence in original.Fences)
            {
                var body = fence.Language == "cs" ? "int first = 1;\n" : "let second = 2;\n";
                cache.Get(fence.Open, fence.Close - 1, fence.Closed, doc.Version, fence.Language, palette, () => Copy(body), Create);
            }
            var first = cache.Get(1, 2, true, doc.Version, "cs", palette, () => Copy("int first = 1;\n"), Create);
            for (var i = 0; i < 100; i++)
                CheckCase(ReferenceEquals(first, cache.Get(1, 2, true, doc.Version, "cs", palette, () => Copy("int first = 1;\n"), Create)), "unchanged fence reuses its cached highlight");
            CheckCase(copies == 2 && builds == 2, "one body copy for each unchanged fence across repeated lines");

            var sameBodyNewVersion = new AvaloniaEdit.Document.TextDocument(doc.Text);
            for (var i = 0; i < 100; i++)
                cache.Get(1, 2, true, sameBodyNewVersion.Version, "cs", palette, () => Copy("int first = 1;\n"), Create);
            CheckCase(copies == 3 && builds == 2, "one body copy after a document version change with unchanged fence text");

            var boundsMissed = !cache.TryGet(1, 3, true, sameBodyNewVersion.Version, "cs", palette, out _);
            var changedBounds = cache.Get(1, 3, true, sameBodyNewVersion.Version, "cs", palette,
                () => Copy("int first = 1;\nextra line\n"), Create);
            CheckCase(boundsMissed && !ReferenceEquals(first, changedBounds) && copies == 4 && builds == 3,
                "same-version fence with changed body bounds misses and rebuilds");

            var moved = Editor.NoteStructure.Build(["intro", "```cs", "int first = 1;", "```"]);
            cache.Prune(moved.Fences.Select(f => f.Open));
            CheckCase(cache.Count == 0, "moved and deleted fence keys are pruned");
            var movedDoc = new AvaloniaEdit.Document.TextDocument("intro\n```cs\nint first = 1;\n```");
            cache.Get(moved.Fences[0].Open, 3, moved.Fences[0].Closed, movedDoc.Version, moved.Fences[0].Language, palette,
                () => Copy("int first = 1;\n"), Create);
            cache.Prune(moved.Fences.Select(f => f.Open));
            CheckCase(cache.Count == moved.Fences.Count && cache.TryGet(2, 3, true, movedDoc.Version, "cs", palette, out _)
                && !cache.TryGet(1, 2, true, movedDoc.Version, "cs", palette, out _) && !cache.TryGet(5, 3, true, movedDoc.Version, "js", palette, out _),
                "cache contains only the current moved fence");

            var pens = new Editor.ChevronPens();
            var firstPens = pens.Get(1, palette);
            var reusedPens = pens.Get(1, palette);
            var changedPalettePens = pens.Get(1, Theme.Palette.All[1]);
            var changedScalePens = pens.Get(1.25, Theme.Palette.All[1]);
            CheckCase(ReferenceEquals(firstPens, reusedPens) && !ReferenceEquals(firstPens, changedPalettePens)
                && !ReferenceEquals(changedPalettePens, changedScalePens), "chevron pens reuse by palette and scale");

            var brushes = new Editor.ColorizerBrushCache();
            var firstBrush = brushes.Get(palette.Keyword);
            var reusedBrush = brushes.Get(palette.Keyword);
            var changedBrush = brushes.Get(Theme.Palette.All[1].Keyword);
            CheckCase(ReferenceEquals(firstBrush, reusedBrush) && !ReferenceEquals(firstBrush, changedBrush),
                "colorizer brush reuses the same color and refreshes on palette color change");

            report = failures.Count == 0
                ? "fence cache pruning and body-copy reuse, plus chevron and colorizer brush resource reuse"
                : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex) { report = string.Join("; ", failures.Append(ex.ToString())); return false; }
    }

    private static bool IncrementalAnalysisChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        static string EvalSignature(Calc.LineEval? line)
        {
            if (line is null) return "null";
            var result = line.Result is { } value
                ? $"{value.Display}|{value.Value.ToString("R", System.Globalization.CultureInfo.InvariantCulture)}|{value.Unit}"
                : "-";
            var tokens = string.Join(",", line.Tokens.Select(t => $"{t.Start}:{t.Length}:{t.Kind}"));
            return $"{result}|{line.ErrorReason}|{line.AssignedName}|{tokens}";
        }

        static bool Equivalent(Editor.NoteAnalysis incremental, Editor.NoteAnalysis full, AvaloniaEdit.Document.TextDocument document)
        {
            var structureA = incremental.Structure;
            var structureB = full.Structure;
            if (incremental.Mode != full.Mode || incremental.CodeLanguage != full.CodeLanguage
                || incremental.ModeKeywordLength != full.ModeKeywordLength
                || !incremental.Lines.SequenceEqual(full.Lines)
                || structureA is null || structureB is null
                || !structureA.Fences.SequenceEqual(structureB.Fences)
                || !structureA.Indent.SequenceEqual(structureB.Indent)
                || !structureA.InFence.SequenceEqual(structureB.InFence)
                || incremental.MathLines.Count != full.MathLines.Count
                || !incremental.MathLines.Select(EvalSignature).SequenceEqual(full.MathLines.Select(EvalSignature))) return false;

            var inlineA = incremental.InlineLines.OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}:{EvalSignature(pair.Value)}");
            var inlineB = full.InlineLines.OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}:{EvalSignature(pair.Value)}");
            if (!inlineA.SequenceEqual(inlineB)) return false;

            var timersA = incremental.TimerKeys.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value}");
            var timersB = full.TimerKeys.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value}");
            if (!timersA.SequenceEqual(timersB)) return false;

            var foldsA = structureA.Folds(document, incremental.Lines, incremental.Mode == NoteMode.Code)
                .Select(fold => (fold.StartOffset, fold.EndOffset, fold.Name)).ToArray();
            var foldsB = structureB.Folds(document, full.Lines, full.Mode == NoteMode.Code)
                .Select(fold => (fold.StartOffset, fold.EndOffset, fold.Name)).ToArray();
            return foldsA.SequenceEqual(foldsB);
        }

        static Editor.NoteAnalysis Full(AvaloniaEdit.Document.TextDocument document, bool inlineMath, string? noteId)
        {
            var full = new Editor.NoteAnalysis();
            full.Update(document, 0, document.LineCount - 1, inlineMath, noteId, timers: null, forceFull: true);
            return full;
        }

        static (int First, int Last) Around(AvaloniaEdit.Document.TextDocument document, int lineNumber) =>
            (Math.Max(0, lineNumber - 2), Math.Min(document.LineCount - 1, lineNumber));

        static int FindLine(AvaloniaEdit.Document.TextDocument document, string text)
        {
            for (var number = 1; number <= document.LineCount; number++)
                if (document.GetText(document.GetLineByNumber(number)) == text) return number;
            return 0;
        }

        try
        {
            var document = new AvaloniaEdit.Document.TextDocument(string.Join('\n', new[]
            {
                "# Start", "intro", "```cs", "int value = 1;", "```", "## After",
                "- item one", "  details", "- item two", "total 2 + 3 ="
            }));
            var analysis = new Editor.NoteAnalysis();
            analysis.Update(document, 0, document.LineCount - 1, inlineMath: true, noteId: "a29-note", timers: null, forceFull: true);
            CheckCase(analysis.LastCopiedLineCount == document.LineCount, "initial analysis copies each document line once");
            CheckCase(analysis.InlineLines.Count == 1, "plain note retains its inline result");

            bool Refresh(AvaloniaEdit.Document.TextDocument target, Editor.NoteAnalysis current,
                int first, int last, bool inlineMath, string? noteId, string name)
            {
                current.Invalidate(target, first, last);
                current.Update(target, first, last, inlineMath, noteId, timers: null);
                var equivalent = Equivalent(current, Full(target, inlineMath, noteId), target);
                CheckCase(equivalent, name);
                return equivalent;
            }

            var body = document.GetLineByNumber(4);
            document.Replace(body.Offset, body.Length, "int value = 2;");
            var range = Around(document, 4);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "fence body edit matches full analysis");

            var opening = document.GetLineByNumber(3);
            document.Replace(opening.Offset, opening.Length, "```py");
            range = Around(document, 3);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "fence opening edit matches full analysis");

            var closing = document.GetLineByNumber(5);
            document.Replace(closing.Offset, closing.Length, "tail");
            range = Around(document, 5);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "open fence edit matches full analysis");

            closing = document.GetLineByNumber(5);
            document.Replace(closing.Offset, closing.Length, "```");
            range = Around(document, 5);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "fence close edit matches full analysis");

            document.Insert(0, "## Inserted\n");
            Refresh(document, analysis, 0, 2, true, "a29-note", "heading insertion matches full analysis");
            var insertedHeading = document.GetLineByNumber(1);
            document.Remove(insertedHeading.Offset, insertedHeading.TotalLength);
            Refresh(document, analysis, 0, Math.Min(2, document.LineCount - 1), true, "a29-note", "heading deletion matches full analysis");

            var detailsLine = FindLine(document, "  details");
            if (detailsLine == 0) throw new InvalidOperationException("list child fixture was lost");
            var details = document.GetLineByNumber(detailsLine);
            document.Replace(details.Offset, details.Length, "details");
            range = Around(document, detailsLine);
            Refresh(document, analysis, range.First, range.Last, true, "a29-note", "list run edit matches full analysis");

            var mathDocument = new AvaloniaEdit.Document.TextDocument("math\nrate = 10\ncost = rate * 3\nlunch 2 + 4");
            var mathAnalysis = new Editor.NoteAnalysis();
            mathAnalysis.Update(mathDocument, 0, mathDocument.LineCount - 1, inlineMath: true, noteId: "a29-math", timers: null, forceFull: true);
            var rate = mathDocument.GetLineByNumber(2);
            mathDocument.Replace(rate.Offset, rate.Length, "rate = 12");
            Refresh(mathDocument, mathAnalysis, 1, 1, true, "a29-math", "later variable use matches full two-pass math analysis");
            CheckCase(mathAnalysis.MathLines[2]?.Result?.Value == 36, "later math line reflects the edited variable");

            var timerDocument = new AvaloniaEdit.Document.TextDocument("timer\n5m tea\n1m water\n5m coffee");
            var timerAnalysis = new Editor.NoteAnalysis();
            timerAnalysis.Update(timerDocument, 0, timerDocument.LineCount - 1, inlineMath: true, noteId: "a29-timers", timers: null, forceFull: true);
            var water = timerDocument.GetLineByNumber(3);
            timerDocument.Replace(water.Offset, water.Length, "5m water");
            range = Around(timerDocument, 3);
            Refresh(timerDocument, timerAnalysis, range.First, range.Last, true, "a29-timers", "timer duration edit matches full parsing");
            CheckCase(timerAnalysis.TimerKeys.TryGetValue(2, out var firstTimer) && firstTimer == "a29-timers:5m:1"
                && timerAnalysis.TimerKeys.TryGetValue(3, out var secondTimer) && secondTimer == "a29-timers:5m:2"
                && timerAnalysis.TimerKeys.TryGetValue(4, out var thirdTimer) && thirdTimer == "a29-timers:5m:3",
                "duplicate timer ordinals remain in document order");

            var burstDocument = new AvaloniaEdit.Document.TextDocument(string.Join("\n",
                Enumerable.Range(0, 5000).Select(index => $"ordinary line {index}")));
            var burst = new Editor.NoteAnalysis();
            burst.Update(burstDocument, 0, burstDocument.LineCount - 1, inlineMath: true, noteId: "a29-burst", timers: null, forceFull: true);
            var firstEdit = burstDocument.GetLineByNumber(2500);
            burstDocument.Replace(firstEdit.Offset, firstEdit.Length, "ordinary line 2499 edited");
            var firstRange = Around(burstDocument, 2500);
            burst.Invalidate(burstDocument, firstRange.First, firstRange.Last);
            var secondEdit = burstDocument.GetLineByNumber(2501);
            burstDocument.Replace(secondEdit.Offset, secondEdit.Length, "ordinary line 2500 edited");
            var secondRange = Around(burstDocument, 2501);
            burst.Invalidate(burstDocument, secondRange.First, secondRange.Last);
            var unionFirst = Math.Min(firstRange.First, secondRange.First);
            var unionLast = Math.Max(firstRange.Last, secondRange.Last);
            burst.Update(burstDocument, unionFirst, unionLast, inlineMath: true, noteId: "a29-burst", timers: null);
            CheckCase(burst.LastCopiedLineCount <= 8 && burst.LastAnalysedLineCount < 32,
                $"5,000-line burst copies {burst.LastCopiedLineCount} and analyses {burst.LastAnalysedLineCount} lines");
            CheckCase(Equivalent(burst, Full(burstDocument, true, "a29-burst"), burstDocument),
                "multiple same-count edits match full analysis");

            report = failures.Count == 0
                ? "fence, heading, list, math, timer, inline and fold equivalence; two-edit 5,000-line burst stayed bounded"
                : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.ToString()));
            return false;
        }
    }

    private static bool BackgroundSaveAndLogChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-background-save-selftest-{Guid.NewGuid():N}");
        try
        {
            var burstPath = Path.Combine(root, "burst");
            var burstStore = new Store(burstPath);
            burstStore.Load();
            CheckCase(burstStore.Save(), "baseline save for coalescing");
            var baselineIndex = File.ReadAllBytes(Path.Combine(burstPath, "index.json"));
            var note = burstStore.Data.Stack[0];
            Task<bool>? firstBatch = null;
            Task<bool>? lastBatch = null;
            string lastText = "";
            for (var i = 0; i < 40; i++)
            {
                lastText = $"queued snapshot {i}\nbody {i}";
                note.Text = lastText;
                var queued = burstStore.QueueSave();
                firstBatch ??= queued;
                lastBatch = queued;
            }
            var burstFlush = burstStore.Flush();
            var burstResult = lastBatch?.GetAwaiter().GetResult() ?? false;
            var burstReloaded = new Store(burstPath);
            burstReloaded.Load();
            var backupPath = Path.Combine(burstPath, "index.json.bak");
            CheckCase(firstBatch is not null && ReferenceEquals(firstBatch, lastBatch), "rapid queue burst coalesces into one task");
            CheckCase(burstFlush && burstResult && burstReloaded.Data.Stack[0].Text == lastText, "flush persists the burst's last snapshot");
            CheckCase(File.Exists(backupPath) && File.ReadAllBytes(backupPath).SequenceEqual(baselineIndex), "coalesced burst writes one index revision");

            var flushPath = Path.Combine(root, "flush");
            var flushStore = new Store(flushPath);
            flushStore.Load();
            flushStore.Data.Stack[0].Text = "queued then synchronously flushed";
            var queuedFlush = flushStore.QueueSave();
            var synchronousFlush = flushStore.Flush();
            var flushResult = queuedFlush.GetAwaiter().GetResult();
            var flushReloaded = new Store(flushPath);
            flushReloaded.Load();
            CheckCase(synchronousFlush && flushResult && flushReloaded.Data.Stack[0].Text == "queued then synchronously flushed",
                "Flush drains a queued save before returning");

            var livePath = Path.Combine(root, "live-note");
            var liveStore = new Store(livePath);
            liveStore.Load();
            var liveNote = liveStore.Data.Stack[0];
            var liveText = "first live value";
            liveNote.Attach(() => liveText);
            liveNote.Touched(blank: false, NoteMode.Plain);
            var liveQueued = liveStore.QueueSave();
            liveText = "latest live value before quit";
            liveNote.Touched(blank: false, NoteMode.Plain);
            var liveSaved = liveStore.Save();
            var liveQueueResult = liveQueued.GetAwaiter().GetResult();
            var liveReloaded = new Store(livePath);
            liveReloaded.Load();
            CheckCase(liveSaved && liveQueueResult && liveReloaded.Data.Stack[0].Text == liveText && liveNote.TitleCache == liveText,
                "synchronous Save replaces a pending snapshot with the latest live editor text");

            var settingsPath = Path.Combine(root, "snapshot");
            var settingsStore = new Store(settingsPath);
            settingsStore.Load();
            settingsStore.Data.Settings.DisabledLanguages = ["before-queue"];
            var settingsQueued = settingsStore.QueueSave();
            settingsStore.Data.Settings.DisabledLanguages.Add("after-queue");
            var settingsFlushed = settingsStore.Flush();
            var settingsResult = settingsQueued.GetAwaiter().GetResult();
            var settingsReloaded = new Store(settingsPath);
            settingsReloaded.Load();
            CheckCase(settingsFlushed && settingsResult
                && settingsReloaded.Data.Settings.DisabledLanguages.SequenceEqual(["before-queue"]),
                "queued save persists its immutable settings snapshot");

            var blockedPath = Path.Combine(root, "blocked");
            var blockedStore = new Store(blockedPath);
            blockedStore.Load();
            blockedStore.Data.Stack[0].Text = "write must fail";
            Directory.CreateDirectory(Path.Combine(blockedPath, "index.json"));
            var blockedQueued = blockedStore.QueueSave();
            var blockedFlush = blockedStore.Flush();
            var blockedQueueResult = blockedQueued.GetAwaiter().GetResult();
            var quitSaveGate = blockedStore.Save();
            CheckCase(!blockedFlush && !blockedQueueResult && !quitSaveGate,
                "blocked index write returns failure through queue, flush, and synchronous Save gate");
            Directory.Delete(Path.Combine(blockedPath, "index.json"));
            var retrySaved = blockedStore.Save();
            var retryReloaded = new Store(blockedPath);
            retryReloaded.Load();
            CheckCase(retrySaved && retryReloaded.Data.Stack[0].Text == "write must fail",
                "retry after removing the index obstruction saves the dirty note");

            var unavailablePath = Path.Combine(root, "unavailable");
            var unavailableStore = new Store(unavailablePath);
            unavailableStore.Load();
            var unavailableNote = unavailableStore.Data.Stack[0];
            unavailableNote.Text = "kept dirty while unavailable";
            var unavailableFilesBefore = Directory.GetFiles(unavailablePath, "*", SearchOption.AllDirectories).Order().ToArray();
            unavailableStore.MarkUnavailable("fixture store unavailable");
            var unavailableQueued = unavailableStore.QueueSave().GetAwaiter().GetResult();
            var unavailableSaved = unavailableStore.Save();
            var unavailableFlushed = unavailableStore.Flush();
            var unavailableFilesAfter = Directory.GetFiles(unavailablePath, "*", SearchOption.AllDirectories).Order().ToArray();
            CheckCase(unavailableQueued && unavailableSaved && unavailableFlushed
                && unavailableStore.LastError is not null && unavailableNote.Dirty
                && unavailableFilesBefore.SequenceEqual(unavailableFilesAfter),
                "unavailable existing folder makes QueueSave, Save, and Flush successful no-ops without clearing dirty text");

            var missingPath = Path.Combine(root, "missing-folder");
            var missingStore = new Store(missingPath);
            missingStore.Load();
            var missingNote = missingStore.Data.Stack[0];
            missingNote.Text = "kept dirty while folder is missing";
            var missingInitiallyAvailable = missingStore.IsAvailable;
            Directory.Delete(missingPath, recursive: true);
            var missingQueued = missingStore.QueueSave().GetAwaiter().GetResult();
            var missingSaved = missingStore.Save();
            var missingFlushed = missingStore.Flush();
            CheckCase(missingStore.IsAvailable == missingInitiallyAvailable && missingQueued && missingSaved && missingFlushed
                && missingStore.LastError is not null && missingNote.Dirty && !Directory.Exists(missingPath),
                "missing folder makes QueueSave, Save, and Flush successful no-ops without changing availability or clearing dirty text");
            Directory.CreateDirectory(missingPath);
            var missingRecoveredSaved = missingStore.Save();
            var missingReloaded = new Store(missingPath);
            missingReloaded.Load();
            CheckCase(missingRecoveredSaved && missingStore.IsAvailable == missingInitiallyAvailable
                && !missingNote.Dirty && missingReloaded.Data.Stack[0].Text == "kept dirty while folder is missing",
                "Save persists retained dirty text after the missing folder returns");

            var switchSourcePath = Path.Combine(root, "failed-switch-source");
            var switchStore = new Store(switchSourcePath);
            switchStore.Load();
            var switchNote = switchStore.Data.Stack[0];
            switchNote.Text = "retained after failed folder write";
            Directory.CreateDirectory(Path.Combine(switchSourcePath, "index.json"));
            var switchFailed = !switchStore.Save();
            var switchPath = Path.Combine(root, "fresh-folder");
            Directory.CreateDirectory(switchPath);
            switchStore.SwitchFolder(switchPath);
            var switchDirty = switchNote.Dirty;
            var switchedSaved = switchStore.Save();
            var switchedReloaded = new Store(switchPath);
            switchedReloaded.Load();
            CheckCase(switchFailed && switchDirty && switchedSaved && switchedReloaded.Data.Stack[0].Text == "retained after failed folder write"
                && File.Exists(Path.Combine(switchPath, "index.json")),
                "SwitchFolder drains a prior failed write and saves subsequent edits in the fresh folder");

            var logPath = Path.Combine(root, "logger", AppInfo.LogFileName);
            using (var logger = new BackgroundLog(logPath))
            {
                const int lines = 64;
                for (var i = 0; i < lines; i++) logger.Write($"ordered-{i:D3}");
                logger.Flush();
                var written = File.ReadAllLines(logPath);
                var orderedAndTimestamped = written.Length == lines && Enumerable.Range(0, lines).All(i =>
                    written[i].Length >= 20 && written[i][19] == ' '
                    && DateTime.TryParseExact(written[i][..19], "s", System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out _)
                    && written[i].EndsWith($"ordered-{i:D3}", StringComparison.Ordinal));
                CheckCase(orderedAndTimestamped, "logger Flush writes captured timestamps and entries in queue order");
            }

            report = failures.Count == 0
                ? "40 queued snapshots coalesce to the last index revision; queued Flush drains; unavailable saves are successful no-ops; blocked writes fail the synchronous Save gate; folder switching recovers after a failed write; logger Flush preserves timestamped order"
                : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.ToString()));
            return false;
        }
        finally { try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { } }
    }

    private static bool NativeTransitionStyleChecks(out string report)
    {
        const uint borderOnlyVisibleResizable = 0x10000000 | 0x02000000 | 0x04000000 | 0x00800000 | 0x00040000;
        const uint expectedTransitionBits = 0x00CB0000;
        var expected = borderOnlyVisibleResizable | expectedTransitionBits;
        var actual = Native.AddNativeTransitionStyles(borderOnlyVisibleResizable);
        var failures = new List<string>();
        if (actual != expected)
            failures.Add("style policy preserves BorderOnly visibility, clipping, border, and resize flags while adding the expected native frame flags");

        (string? Setting, PadVisibilityState State, bool Pad, bool Proxy)[] presenceCases =
        [
            ("never", PadVisibilityState.Closed, false, false),
            ("never", PadVisibilityState.Minimized, false, false),
            ("never", PadVisibilityState.Open, false, false),
            ("visible", PadVisibilityState.Closed, false, false),
            ("visible", PadVisibilityState.Minimized, false, false),
            ("visible", PadVisibilityState.Open, true, false),
            ("always", PadVisibilityState.Closed, false, true),
            ("always", PadVisibilityState.Minimized, true, false),
            ("always", PadVisibilityState.Open, true, false),
            (null, PadVisibilityState.Closed, false, false),
            (null, PadVisibilityState.Minimized, false, false),
            (null, PadVisibilityState.Open, false, false),
            ("unknown", PadVisibilityState.Closed, false, false),
            ("unknown", PadVisibilityState.Minimized, false, false),
            ("unknown", PadVisibilityState.Open, false, false),
        ];
        foreach (var test in presenceCases)
        {
            if (TaskbarPresence.ShouldShowPad(test.Setting, test.State) != test.Pad
                || TaskbarPresence.ShouldShowProxy(test.Setting, test.State) != test.Proxy)
                failures.Add($"taskbar presence setting={test.Setting ?? "null"}, state={test.State}");
        }

        report = failures.Count == 0
            ? "style policy preserves BorderOnly/resize bits and adds the expected native frame flags; taskbar presence policy covers Never, While visible, Always, and unknown settings across closed, minimized, and open states"
            : string.Join("; ", failures);
        return failures.Count == 0;
    }
}
