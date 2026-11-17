// Capability checks for settings persistence and acknowledged note-body saves.
using AntiNotepad.Model;
using AntiNotepad.Platform;

namespace AntiNotepad;

internal static partial class SelfTest
{
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

    private static bool InputLimitChecks(out string report)
    {
        var failures = new List<string>();
        void Check(bool ok, string what) { if (!ok) failures.Add(what); }

        try
        {
            Check(!Editor.ListEditing.TryListNumber("123456789012. item", out _), "12-digit list marker is not continued or renumbered");

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
            var firstKey = Editor.NoteAnalysis.TimerKey("selftest", firstText, 1);
            var secondKey = Editor.NoteAnalysis.TimerKey("selftest", secondText, 1);
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

    private static bool UnavailableStoreStartupChecks(out string report)
    {
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-unavailable-store-selftest-{Guid.NewGuid():N}");
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        Store? missingStore = null;
        Store? fallbackStore = null;
        try
        {
            var missingFolder = Path.Combine(root, "missing-load");
            missingStore = new Store(missingFolder, createDirectories: false);
            missingStore.Load();
            var missingSelection = missingStore.Data.Stack.Count == 1 && missingStore.Data.Stack[0].IsEmpty
                && missingStore.Data.Current >= 0 && missingStore.Data.Current < missingStore.Data.Stack.Count;
            var missingData = missingStore.Data;
            var missingSave = missingStore.Save();
            CheckCase(!missingStore.IsAvailable && missingSelection && missingSave && missingStore.LastError is not null
                && !Directory.Exists(missingFolder), "missing-folder Load leaves a blank current note and Save creates nothing");

            var fallbackFolder = Path.Combine(root, "missing-fallback");
            fallbackStore = new Store(fallbackFolder, createDirectories: false);
            fallbackStore.MarkUnavailable("fixture constructor fallback");
            fallbackStore.Load();
            var fallbackSelection = fallbackStore.Data.Stack.Count == 1 && fallbackStore.Data.Stack[0].IsEmpty
                && fallbackStore.Data.Current >= 0 && fallbackStore.Data.Current < fallbackStore.Data.Stack.Count;
            var fallbackSave = fallbackStore.Save();
            CheckCase(!fallbackStore.IsAvailable && fallbackSelection && fallbackSave && fallbackStore.LastError is not null
                && !Directory.Exists(fallbackFolder), "MarkUnavailable constructor fallback leaves a blank current note and Save creates nothing");

            var validFolder = Path.Combine(root, "valid-folder");
            var source = new Store(validFolder);
            source.Load();
            source.Data.Stack[0].Text = "loaded from valid folder";
            var sourceSaved = source.Save();
            var placeholderData = missingStore.Data;
            missingStore.SwitchFolder(validFolder);
            missingStore.Load();
            CheckCase(sourceSaved && missingStore.IsAvailable && !ReferenceEquals(placeholderData, missingStore.Data)
                && missingStore.Data.Stack[missingStore.Data.Current].Text == "loaded from valid folder"
                && !ReferenceEquals(missingData, missingStore.Data), "SwitchFolder then Load replaces the placeholder with valid-folder data");

            report = failures.Count == 0 ? "missing-folder load, constructor fallback, no-write Save, and valid-folder reload" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.Message));
            return false;
        }
        finally
        {
            try { missingStore?.Flush(); } catch { }
            try { fallbackStore?.Flush(); } catch { }
            try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
        }
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

    /// <summary>Checks save-delay defaults, normalization, persistence, and revision-matched save timestamps.</summary>
    private static bool SaveDelayStorageChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-save-delay-selftest-{Guid.NewGuid():N}");
        try
        {
            var fresh = new Settings();
            CheckCase(fresh.SaveDelayMs == 400, "fresh settings default to 400 ms");
            fresh.SaveDelayMs = 250;
            CheckCase(fresh.SaveDelayMs == 400, "unsupported delay normalizes to 400 ms");
            var invalidJson = System.Text.Json.JsonSerializer.Deserialize<StoreData>("{\"Settings\":{\"SaveDelayMs\":250}}")!;
            CheckCase(invalidJson.Settings.SaveDelayMs == 400, "unsupported persisted delay normalizes to 400 ms");

            var settingsStore = new Store(Path.Combine(root, "settings"));
            settingsStore.Load();
            foreach (var delay in new[] { 400, 5000, 10000, 30000 })
            {
                settingsStore.Data.Settings.SaveDelayMs = delay;
                var saved = settingsStore.Save();
                var reloaded = new Store(settingsStore.Folder);
                reloaded.Load();
                CheckCase(saved && reloaded.Data.Settings.SaveDelayMs == delay, $"{delay} ms settings round trip");
            }

            var durableStore = new Store(Path.Combine(root, "durable"));
            durableStore.Load();
            var durableNote = durableStore.Data.Stack[0];
            durableNote.Text = "saved body";
            var modified = DateTime.Now.AddMinutes(-2);
            durableNote.Modified = modified;
            var queued = durableStore.QueueSave().GetAwaiter().GetResult();
            CheckCase(queued && !durableNote.Dirty && durableNote.SavedAt is { } savedAt && savedAt >= modified
                && durableNote.Modified == modified, "durable completion sets SavedAt without changing Modified");
            var index = File.ReadAllText(Path.Combine(durableStore.Folder, "index.json"));
            CheckCase(!index.Contains("SavedAt", StringComparison.Ordinal), "SavedAt stays out of the index format");
            var durableReload = new Store(durableStore.Folder);
            durableReload.Load();
            CheckCase(durableReload.Data.Stack[0].SavedAt == File.GetLastWriteTime(Path.Combine(durableStore.Folder, "notes", durableNote.Id + ".md")),
                "loaded labels use the durable body's file time");

            var emptySavedAt = durableNote.SavedAt;
            durableNote.Text = "";
            var emptySaved = durableStore.Save();
            var emptyBodyPath = Path.Combine(durableStore.Folder, "notes", durableNote.Id + ".md");
            var emptyBody = File.Exists(emptyBodyPath) ? File.ReadAllText(emptyBodyPath) : null;
            CheckCase(emptySaved && !durableNote.Dirty && emptySavedAt is { } previousEmptyTime
                && durableNote.SavedAt is { } emptyTime && emptyTime >= previousEmptyTime
                && durableNote.Modified == modified && emptyBody == "",
                $"a previously saved note durably stores its empty body without touching Modified (saved={emptySaved}, dirty={durableNote.Dirty}, oldSavedAt={emptySavedAt:O}, savedAt={durableNote.SavedAt:O}, modified={durableNote.Modified:O}, expected={modified:O}, bodyExists={emptyBody is not null}, bodyLength={emptyBody?.Length})");

            var failedStore = new Store(Path.Combine(root, "failed"));
            failedStore.Load();
            var failedNote = failedStore.Data.Stack[0];
            failedNote.Text = "old durable body";
            CheckCase(failedStore.Save(), "failure fixture has an initial durable body");
            var previousSaveTime = failedNote.SavedAt;
            failedNote.Text = "new body that cannot be written";
            var notesFolder = Path.Combine(failedStore.Folder, "notes");
            Directory.Delete(notesFolder, recursive: true);
            File.WriteAllText(notesFolder, "block directory creation");
            var failedWrite = failedStore.QueueSave().GetAwaiter().GetResult();
            CheckCase(!failedWrite && failedNote.Dirty && failedNote.SavedAt == previousSaveTime
                && failedStore.LastError is not null, "failed disk write keeps the prior saved time and dirty body");

            var largeStore = new Store(Path.Combine(root, "large"));
            largeStore.Load();
            var largeNote = largeStore.Data.Stack[0];
            const int largeChars = 1_000_000;
            largeNote.Text = new string('x', largeChars);
            var queueStarted = System.Diagnostics.Stopwatch.GetTimestamp();
            var largeSave = largeStore.QueueSave();
            var enqueueMs = System.Diagnostics.Stopwatch.GetElapsedTime(queueStarted).TotalMilliseconds;
            var pendingBeforeWrite = !largeSave.IsCompleted;
            var writeStarted = System.Diagnostics.Stopwatch.GetTimestamp();
            var largeSaved = largeSave.GetAwaiter().GetResult();
            var completeMs = System.Diagnostics.Stopwatch.GetElapsedTime(writeStarted).TotalMilliseconds;
            CheckCase(pendingBeforeWrite && largeSaved && !largeNote.Dirty
                && File.ReadAllText(Path.Combine(largeStore.Folder, "notes", largeNote.Id + ".md")).Length == largeChars,
                "large body save returns before background write completion and keeps the body");
            var largeTimingSample = $"large-note chars={largeChars} enqueue={enqueueMs:F1}ms completion-wait={completeMs:F1}ms";
            Console.WriteLine("SAVE DELAY TIMING " + largeTimingSample);

            var staleStore = new Store(Path.Combine(root, "stale"));
            staleStore.Load();
            var staleNote = staleStore.Data.Stack[0];
            staleNote.Text = "older snapshot";
            var staleQueue = staleStore.QueueSave();
            staleNote.Text = "newer revision";
            var newerModified = DateTime.Now;
            staleNote.Modified = newerModified;
            var staleCompleted = staleQueue.GetAwaiter().GetResult();
            CheckCase(staleCompleted && staleNote.Dirty && staleNote.SavedAt is null && staleNote.Modified == newerModified,
                "stale completion leaves the newer revision dirty and untimestamped");
            var latestSaved = staleStore.Save();
            CheckCase(latestSaved && !staleNote.Dirty && staleNote.SavedAt is not null && staleNote.Modified == newerModified,
                $"matching completion timestamps the latest revision without changing Modified (saved={latestSaved}, dirty={staleNote.Dirty}, savedAt={staleNote.SavedAt:O}, modified={staleNote.Modified:O}, expected={newerModified:O}, revision={staleNote.SearchRevision})");

            var deletedStore = new Store(Path.Combine(root, "deleted"));
            deletedStore.Load();
            var deletedNote = deletedStore.Data.Stack[0];
            deletedNote.Text = "body to delete";
            var deletedWrite = deletedStore.QueueSave();
            var wasQueued = deletedNote.BodyWriteInFlight;
            deletedStore.Data.Stack.Remove(deletedNote);
            var deletionSaved = deletedStore.Save();
            var deletedWriteFinished = deletedWrite.GetAwaiter().GetResult();
            CheckCase(wasQueued && deletionSaved && deletedWriteFinished
                && !File.Exists(Path.Combine(deletedStore.Folder, "notes", deletedNote.Id + ".md")),
                "deleting a queued note leaves no body file; the pending flag lives on the note, so no store state remains");

            report = failures.Count == 0
                ? $"deleted queued note cleanup, default, invalid values, delay round trips, durable load times, saved-empty body, failed-write label retention, stale acknowledgements, unchanged Modified, background large-body sample {largeTimingSample}, no SavedAt serialization"
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
}
