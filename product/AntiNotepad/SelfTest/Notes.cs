using AntiNotepad.Model;
using AntiNotepad.Platform;

namespace AntiNotepad;

internal static partial class SelfTest
{
    private static bool NotesLocationChecks(out string report)
    {
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-notes-location-selftest-{Guid.NewGuid():N}");
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        try
        {
            var ensureFolder = Path.Combine(root, "ensure-notes");
            Directory.CreateDirectory(ensureFolder);
            var ensured = NotesLocation.EnsureRecovered(ensureFolder);
            CheckCase(ensured && Directory.Exists(Path.Combine(ensureFolder, "recovered"))
                && NotesLocation.EnsureRecovered(ensureFolder), "recovered folder is created and ensure is idempotent");
            var missingEnsureFolder = Path.Combine(root, "missing-ensure-notes");
            CheckCase(!NotesLocation.EnsureRecovered(missingEnsureFolder) && !Directory.Exists(missingEnsureFolder),
                "recovered ensure does not create a missing notes folder");

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

    private static bool NotesMoveChecks(out string report)
    {
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-notes-session-selftest-{Guid.NewGuid():N}");
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        Store? store = null;
        Store? unavailableStore = null;
        try
        {
            var source = Path.Combine(root, "source");
            var target = Path.Combine(root, "target");
            store = new Store(source);
            store.Load();
            var data = store.Data;
            var settingsService = store.SettingsService;
            var note = store.Data.Stack[0];
            note.Text = "notes session before move";
            CheckCase(store.Save(), "source note saves before move");

            var session = new NotesSession(store, source, source, null, _ => { });
            var moved = session.MoveNotes(target, out _, out _);
            CheckCase(moved && ReferenceEquals(session.Store, store) && ReferenceEquals(store.Data, data)
                && ReferenceEquals(store.SettingsService, settingsService) && session.DataFolder == store.Folder
                && session.DataFolder == target && session.NotesLocationError is null && store.IsAvailable
                && Path.GetFullPath(NotesLocation.ReadPointer(source) ?? "") == Path.GetFullPath(target),
                "empty sibling move keeps Store, Data, and SettingsService and follows pointer");

            note.Text = "later save after move";
            var savedAfterMove = store.Save();
            var movedBody = Path.Combine(target, "notes", note.Id + ".md");
            CheckCase(savedAfterMove && ReferenceEquals(session.Store, store) && ReferenceEquals(store.Data, data)
                && ReferenceEquals(store.SettingsService, settingsService) && File.ReadAllText(movedBody) == "later save after move",
                "later save uses the original Store in the target folder");

            var occupied = Path.Combine(root, "occupied");
            Directory.CreateDirectory(occupied);
            File.WriteAllText(Path.Combine(occupied, "index.json"), "occupied");
            var pointerBeforeFailure = NotesLocation.ReadPointer(source);
            var folderBeforeFailure = session.DataFolder;
            var errorBeforeFailure = session.NotesLocationError;
            var occupiedRefused = !session.MoveNotes(occupied, out _, out _);
            CheckCase(occupiedRefused && session.DataFolder == folderBeforeFailure && session.NotesLocationError == errorBeforeFailure
                && ReferenceEquals(session.Store, store) && ReferenceEquals(store.Data, data)
                && ReferenceEquals(store.SettingsService, settingsService) && NotesLocation.ReadPointer(source) == pointerBeforeFailure,
                "occupied target refusal leaves folder, error, identities, and pointer unchanged");

            var nested = Path.Combine(target, "nested");
            folderBeforeFailure = session.DataFolder;
            errorBeforeFailure = session.NotesLocationError;
            var nestedRefused = !session.MoveNotes(nested, out _, out _);
            CheckCase(nestedRefused && !Directory.Exists(nested) && session.DataFolder == folderBeforeFailure
                && session.NotesLocationError == errorBeforeFailure && ReferenceEquals(session.Store, store)
                && ReferenceEquals(store.Data, data) && ReferenceEquals(store.SettingsService, settingsService)
                && NotesLocation.ReadPointer(source) == pointerBeforeFailure,
                "nested target refusal creates nothing and leaves session state unchanged");

            var unavailableFolder = Path.Combine(root, "unavailable-source");
            var unavailableTarget = Path.Combine(root, "unavailable-target");
            unavailableStore = new Store(unavailableFolder, createDirectories: false);
            unavailableStore.Load();
            var unavailableData = unavailableStore.Data;
            var unavailableSettings = unavailableStore.SettingsService;
            var unavailableError = unavailableStore.LastError ?? "unavailable";
            var unavailableSession = new NotesSession(unavailableStore, root, unavailableFolder, unavailableError, _ => { });
            var unavailableRefused = !unavailableSession.MoveNotes(unavailableTarget, out _, out _);
            CheckCase(unavailableRefused && !unavailableStore.IsAvailable && !Directory.Exists(unavailableFolder) && !Directory.Exists(unavailableTarget)
                && unavailableSession.DataFolder == unavailableFolder && unavailableSession.NotesLocationError == unavailableError
                && ReferenceEquals(unavailableSession.Store, unavailableStore) && ReferenceEquals(unavailableStore.Data, unavailableData)
                && ReferenceEquals(unavailableStore.SettingsService, unavailableSettings),
                "unavailable store refuses without creating either folder");

            report = failures.Count == 0 ? "passed" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.Message));
            return false;
        }
        finally
        {
            try { store?.Flush(); } catch { }
            try { unavailableStore?.Flush(); } catch { }
            try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
        }
    }

    private static bool NotesMoveRenameChecks(out string report)
    {
        var root = Path.Combine(Environment.CurrentDirectory, "build", $"A52-move-old-folder-rename-warning-fixtures-{Guid.NewGuid():N}");
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }
        Store? collisionStore = null;

        try
        {
            Directory.CreateDirectory(root);
            foreach (var (name, trailingSeparator) in new[]
            {
                ("trimmed", ""),
                ("backslash", "\\"),
                ("slash", "/")
            })
            {
                var caseRoot = Path.Combine(root, name);
                var caseAppHome = Path.Combine(caseRoot, "app-home");
                var source = Path.Combine(caseRoot, "test targetA");
                var target = Path.Combine(caseRoot, "targetB");
                var sourceFile = Path.Combine(source, "notes", "move-contract.md");
                Directory.CreateDirectory(caseAppHome);
                Directory.CreateDirectory(Path.GetDirectoryName(sourceFile)!);
                File.WriteAllText(sourceFile, "old note content");

                var sourceWithTrailingSeparator = source + trailingSeparator;
                var result = NotesLocation.Move(caseAppHome, sourceWithTrailingSeparator, target);
                var oldCopy = result.OldCopy;
                var backup = oldCopy?.BackupFolder;
                var displayMessage = oldCopy?.Message(result.FileCount);
                CheckCase(result.Success && oldCopy is not null && oldCopy.Failure is null
                    && PathEquals(oldCopy.OriginalFolder, source) && backup is not null
                    && Path.GetFileName(backup).StartsWith("test targetA (moved ", StringComparison.Ordinal)
                    && !Directory.Exists(source)
                    && File.ReadAllText(Path.Combine(backup!, "notes", "move-contract.md")) == "old note content"
                    && File.ReadAllText(Path.Combine(target, "notes", "move-contract.md")) == "old note content"
                    && displayMessage == $"Moved 1 file; the old folder was kept as {Path.GetFullPath(backup!)}."
                    && displayMessage is not null
                    && displayMessage.IndexOf(Path.GetFullPath(backup!), StringComparison.OrdinalIgnoreCase)
                        == displayMessage.LastIndexOf(Path.GetFullPath(backup!), StringComparison.OrdinalIgnoreCase),
                    $"{name} source path copied successfully and retained its old folder; outcome: {oldCopy?.Message(result.FileCount) ?? result.Error ?? "missing old-copy outcome"}");
            }

            var appHomeRoot = Path.Combine(root, "app-home-equivalence");
            var appHome = Path.Combine(appHomeRoot, "app-home");
            var appHomeTarget = Path.Combine(appHomeRoot, "target");
            Directory.CreateDirectory(Path.Combine(appHome, "notes"));
            File.WriteAllText(Path.Combine(appHome, "index.json"), "app-home index");
            File.WriteAllText(Path.Combine(appHome, "notes", "app-home.md"), "app-home note");
            var appHomeMove = NotesLocation.Move(appHome, appHome + "\\", appHomeTarget);
            var appHomeCopy = appHomeMove.OldCopy;
            var appHomeMessage = appHomeCopy?.Message(appHomeMove.FileCount);
            CheckCase(appHomeMove.Success && appHomeCopy is not null && appHomeCopy.Failure is null
                && PathEquals(appHomeCopy.OriginalFolder, appHome) && appHomeCopy.BackupFolder is not null
                && Path.GetFileName(appHomeCopy.BackupFolder).StartsWith("backup-before-move-", StringComparison.Ordinal)
                && File.ReadAllText(Path.Combine(appHomeTarget, "notes", "app-home.md")) == "app-home note"
                && File.ReadAllText(Path.Combine(appHomeCopy.BackupFolder!, "notes", "app-home.md")) == "app-home note"
                && appHomeMessage == $"Moved {appHomeMove.FileCount} files; the old folder was kept as {Path.GetFullPath(appHomeCopy.BackupFolder!)}."
                && appHomeMessage is not null
                && appHomeMessage.IndexOf(Path.GetFullPath(appHomeCopy.BackupFolder!), StringComparison.OrdinalIgnoreCase)
                    == appHomeMessage.LastIndexOf(Path.GetFullPath(appHomeCopy.BackupFolder!), StringComparison.OrdinalIgnoreCase)
                && NotesLocation.ReadPointer(appHome) is { } appHomePointer && PathEquals(appHomePointer, appHomeTarget),
                "app-home path with a trailing backslash is recognized as the app home and uses its backup path");

            var collisionRoot = Path.Combine(root, "collision");
            var collisionAppHome = Path.Combine(collisionRoot, "app-home");
            var collisionSource = Path.Combine(collisionRoot, "source");
            var collisionTarget = Path.Combine(collisionRoot, "target");
            collisionStore = new Store(collisionSource);
            collisionStore.Load();
            collisionStore.Data.Stack[0].Text = "move despite backup collision";
            CheckCase(collisionStore.Save(), "collision source note saves before move");
            var collisionBackupPaths = new[] { DateTime.Now, DateTime.Now.AddMinutes(1) }
                .Select(time => Path.Combine(collisionRoot, $"source (moved {time:yyyy-MM-dd HHmm})"))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            foreach (var collisionBackup in collisionBackupPaths) Directory.CreateDirectory(collisionBackup);
            var collisionSession = new NotesSession(collisionStore, collisionAppHome, collisionSource, null, _ => { });
            var collisionMoved = collisionSession.MoveNotes(collisionTarget, out var collisionMessage, out var collisionResult);
            var collisionOutcome = collisionResult?.OldCopy;
            var collisionNotePath = Path.Combine("notes", collisionStore.Data.Stack[0].Id + ".md");
            var collisionSourceBody = Path.Combine(collisionSource, collisionNotePath);
            var collisionTargetBody = Path.Combine(collisionTarget, collisionNotePath);
            var postMoveText = "post-collision save uses target";
            collisionStore.Data.Stack[0].Text = postMoveText;
            var collisionSaved = collisionStore.Save();
            CheckCase(collisionMoved && collisionResult is { Success: true }
                && collisionOutcome is { Failure: NotesOldCopyFailure.NameCollision, BackupFolder: null }
                && PathEquals(collisionOutcome.OriginalFolder, collisionSource)
                && collisionSession.DataFolder == collisionTarget
                && collisionStore.Folder == collisionTarget && collisionStore.IsAvailable && collisionSession.NotesLocationError is null
                && PathEquals(NotesLocation.ReadPointer(collisionAppHome) ?? "", collisionTarget)
                && Directory.Exists(collisionSource)
                && File.ReadAllText(collisionSourceBody) == "move despite backup collision"
                && collisionSaved && File.ReadAllText(collisionTargetBody) == postMoveText
                && collisionMessage == collisionOutcome.Message(collisionResult.FileCount)
                && collisionMessage == $"Notes moved; the old folder remains at {Path.GetFullPath(collisionSource)} because its backup name already exists.",
                $"backup-name collision keeps the old folder but completes the move and reports its outcome: {collisionMessage}");

            report = failures.Count == 0 ? "passed" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.Message));
            return false;
        }
        finally
        {
            try { collisionStore?.Flush(); } catch { }
            try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
        }
    }

    private static bool PathEquals(string left, string right)
    {
        try
        {
            var normalizedLeft = Path.TrimEndingDirectorySeparator(Path.GetFullPath(left));
            var normalizedRight = Path.TrimEndingDirectorySeparator(Path.GetFullPath(right));
            return string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    private static bool NotesUseFolderChecks(out string report)
    {
        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-notes-use-folder-selftest-{Guid.NewGuid():N}");
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        Store? unavailableStore = null;
        Store? availableStore = null;
        try
        {
            var appHome = Path.Combine(root, "app-home");
            var missingFolder = Path.Combine(root, "missing-notes");
            var targetFolder = Path.Combine(root, "existing-notes");
            var untouchedFolder = Path.Combine(root, "untouched-notes");
            Directory.CreateDirectory(appHome);
            Directory.CreateDirectory(targetFolder);
            Directory.CreateDirectory(untouchedFolder);
            var targetStore = new Store(targetFolder);
            targetStore.Load();
            targetStore.Data.Stack[0].Text = "adopt this folder";
            CheckCase(targetStore.Save(), "existing target index is written");
            targetStore.Flush();
            NotesLocation.WritePointer(appHome, untouchedFolder);

            unavailableStore = new Store(missingFolder, createDirectories: false);
            unavailableStore.Load();
            var unavailableError = unavailableStore.LastError ?? $"The notes folder is unavailable: {missingFolder}";
            var unavailableData = unavailableStore.Data;
            var session = new NotesSession(unavailableStore, appHome, missingFolder, unavailableError, _ => { });
            var changed = session.UseFolderOnNextStart(targetFolder, out var message);
            CheckCase(changed && message == $"Notes folder set to {targetFolder}. Quit and start AntiNotepad again to load it."
                && NotesLocation.ReadPointer(appHome) == targetFolder && !Directory.Exists(missingFolder) && Directory.Exists(targetFolder)
                && session.DataFolder == missingFolder && session.NotesLocationError == unavailableError
                && ReferenceEquals(session.Store, unavailableStore) && ReferenceEquals(unavailableStore.Data, unavailableData),
                "unavailable session sets pointer without changing session or creating a folder");

            var resolved = NotesLocation.Resolve(appHome, Path.Combine(root, "unused-documents"));
            CheckCase(resolved.Error is null && resolved.NotesFolder == targetFolder,
                "fresh resolution follows the selected folder pointer");

            var availableFolder = Path.Combine(root, "available-notes");
            availableStore = new Store(availableFolder);
            availableStore.Load();
            var availableSession = new NotesSession(availableStore, appHome, availableFolder, null, _ => { });
            var pointerBeforeAvailable = NotesLocation.ReadPointer(appHome);
            var availableRefused = !availableSession.UseFolderOnNextStart(targetFolder, out _);
            CheckCase(availableRefused && NotesLocation.ReadPointer(appHome) == pointerBeforeAvailable
                && availableSession.DataFolder == availableFolder && availableSession.NotesLocationError is null
                && ReferenceEquals(availableSession.Store, availableStore),
                "available session refuses and leaves the pointer untouched");

            var missingTarget = Path.Combine(root, "missing-target");
            var pointerBeforeMissing = NotesLocation.ReadPointer(appHome);
            var missingRefused = !session.UseFolderOnNextStart(missingTarget, out _);
            CheckCase(missingRefused && !Directory.Exists(missingTarget) && NotesLocation.ReadPointer(appHome) == pointerBeforeMissing
                && session.DataFolder == missingFolder && session.NotesLocationError == unavailableError
                && ReferenceEquals(session.Store, unavailableStore) && ReferenceEquals(unavailableStore.Data, unavailableData),
                "missing target is refused without creation or pointer changes");

            var fileTarget = Path.Combine(root, "not-a-folder");
            File.WriteAllText(fileTarget, "file target");
            var pointerBeforeFile = NotesLocation.ReadPointer(appHome);
            var fileRefused = !session.UseFolderOnNextStart(fileTarget, out _);
            CheckCase(fileRefused && File.Exists(fileTarget) && !Directory.Exists(fileTarget)
                && NotesLocation.ReadPointer(appHome) == pointerBeforeFile && session.DataFolder == missingFolder
                && session.NotesLocationError == unavailableError && ReferenceEquals(session.Store, unavailableStore)
                && ReferenceEquals(unavailableStore.Data, unavailableData),
                "file target is refused without pointer or session changes");

            report = failures.Count == 0 ? "passed" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.Message));
            return false;
        }
        finally
        {
            try { unavailableStore?.Flush(); } catch { }
            try { availableStore?.Flush(); } catch { }
            try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
        }
    }
}
