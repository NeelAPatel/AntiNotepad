using System.Security.Cryptography;
using System.Text.Json;

namespace AntiNotepad.Platform;

internal sealed record NotesLocationResolution(string AppHome, string NotesFolder, string? Error, string? Notice = null, bool Custom = false);

internal enum NotesOldCopyFailure { NameCollision, AccessDenied, InUse, Other }

internal sealed record NotesOldCopyResult(string OriginalFolder, string? BackupFolder, NotesOldCopyFailure? Failure = null)
{
    internal string Message(int fileCount)
    {
        if (Failure is { } failure)
        {
            var reason = failure switch
            {
                NotesOldCopyFailure.NameCollision => "its backup name already exists",
                NotesOldCopyFailure.AccessDenied => "access was denied",
                NotesOldCopyFailure.InUse => "it is in use",
                _ => "its backup could not be created"
            };
            return $"Notes moved; the old folder remains at {OriginalFolder} because {reason}.";
        }
        var moved = $"Moved {fileCount} {(fileCount == 1 ? "file" : "files")}";
        return BackupFolder is { } backup ? $"{moved}; the old folder was kept as {backup}." : moved + ".";
    }
}

internal sealed record NotesMoveResult(bool Success, string Folder, string? Error = null, int FileCount = 0, long ByteCount = 0,
    NotesOldCopyResult? OldCopy = null)
{
    internal string? Warning => OldCopy?.Failure is not null ? OldCopy.Message(FileCount) : null;
}

/// <summary>Resolves and moves the user-visible notes folder. Paths are supplied by the caller for isolated testing.</summary>
internal static class NotesLocation
{
    internal const string PointerFileName = "location.json";
    private const string RecoveredFolderName = "recovered";
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    /// <summary>Ensures recovery storage exists without recreating a missing notes folder.</summary>
    internal static bool EnsureRecovered(string dataFolder)
    {
        try
        {
            if (!Directory.Exists(dataFolder)) return false;
            var recovered = Path.Combine(dataFolder, RecoveredFolderName);
            Directory.CreateDirectory(recovered);
            return Directory.Exists(recovered);
        }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }

    internal static NotesLocationResolution Resolve(string appHome, string documents, string? customFolder = null)
    {
        if (customFolder is { Length: > 0 })
        {
            // Dev and test runs: the custom folder is the app home. It follows its own pointer after an in-app move, and never
            // looks at Documents or does a first-run move.
            var custom = Path.GetFullPath(customFolder);
            var customPointer = Path.Combine(custom, PointerFileName);
            return File.Exists(customPointer) ? FromPointer(custom, customPointer) with { Custom = true } : new(custom, custom, null, Custom: true);
        }

        appHome = Path.GetFullPath(appHome);
        documents = Path.GetFullPath(documents);
        var pointer = Path.Combine(appHome, PointerFileName);
        if (File.Exists(pointer)) return FromPointer(appHome, pointer);

        var defaultFolder = Path.Combine(documents, AppInfo.DataFolderName);
        if (HasNotes(appHome))
        {
            if (File.Exists(Path.Combine(defaultFolder, "index.json")))
                return new(appHome, appHome, null, $"Notes were not moved because {defaultFolder} already contains an index. The existing folder is still in use.");

            var move = Move(appHome, appHome, defaultFolder);
            if (!move.Success) return new(appHome, appHome, $"Could not move notes to {defaultFolder}: {move.Error}");
            return new(appHome, defaultFolder, null, move.Warning);
        }

        var created = !Directory.Exists(defaultFolder);
        var homeExisted = Directory.Exists(appHome);
        try
        {
            Directory.CreateDirectory(defaultFolder);
            WritePointer(appHome, defaultFolder);
            return new(appHome, defaultFolder, null);
        }
        catch (Exception ex)
        {
            if (created) TryRemoveEmptyDirectory(defaultFolder);
            if (!homeExisted) TryRemoveEmptyDirectory(appHome);
            return new(appHome, appHome, $"Could not set the notes folder to {defaultFolder}: {ex.Message}");
        }
    }

    private static NotesLocationResolution FromPointer(string appHome, string pointer)
    {
        try
        {
            var stored = JsonSerializer.Deserialize<LocationPointer>(File.ReadAllText(pointer), Json)?.NotesFolder;
            if (string.IsNullOrWhiteSpace(stored) || !Path.IsPathFullyQualified(stored))
                return new(appHome, appHome, $"The notes folder pointer at {pointer} is invalid. Choose a notes folder in Settings › Storage.");
            var target = Path.GetFullPath(stored);
            if (!Directory.Exists(target))
                return new(appHome, target, $"The notes folder is unavailable: {target}. Choose a folder in Settings › Storage when it is available.");
            try { _ = Directory.EnumerateFileSystemEntries(target).Take(1).ToList(); }
            catch (Exception ex) { return new(appHome, target, $"The notes folder is unavailable: {target}. {ex.Message} Choose a folder in Settings › Storage."); }
            return new(appHome, target, null);
        }
        catch (Exception ex)
        {
            return new(appHome, appHome, $"The notes folder pointer at {pointer} could not be read: {ex.Message}");
        }
    }

    /// <summary>Copies and verifies all note data before updating the pointer and retaining the old copy.</summary>
    internal static NotesMoveResult Move(string appHome, string currentFolder, string targetFolder)
    {
        appHome = Path.GetFullPath(appHome);
        currentFolder = Path.GetFullPath(currentFolder);
        targetFolder = Path.GetFullPath(targetFolder);
        if (!Directory.Exists(currentFolder)) return new(false, currentFolder, $"The current notes folder is unavailable: {currentFolder}");
        if (SameOrNested(currentFolder, targetFolder) || SameOrNested(targetFolder, currentFolder))
            return new(false, currentFolder, "Choose a different folder outside the current notes folder.");
        if (File.Exists(Path.Combine(targetFolder, "index.json")))
            return new(false, currentFolder, $"The target already contains index.json: {targetFolder}");

        List<(string Source, string Destination)> copies;
        try
        {
            var sources = DataFiles(currentFolder);
            copies = sources.Select(file => (Source: file, Destination: Path.Combine(targetFolder, Path.GetRelativePath(currentFolder, file)))).ToList();
            foreach (var (_, destination) in copies)
            {
                if (File.Exists(destination) || Directory.Exists(destination))
                    return new(false, currentFolder, $"The target already contains {Path.GetRelativePath(targetFolder, destination)}.");
            }
        }
        catch (Exception ex) { return new(false, currentFolder, $"Could not inspect the current notes folder: {ex.Message}"); }

        var targetExisted = Directory.Exists(targetFolder);
        var createdDirectories = new List<string>();
        var createdFiles = new List<string>();
        try
        {
            EnsureDirectory(targetFolder, createdDirectories);
            foreach (var (source, destination) in copies)
            {
                EnsureDirectory(Path.GetDirectoryName(destination)!, createdDirectories);
                using var input = File.OpenRead(source);
                using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                createdFiles.Add(destination);
                input.CopyTo(output);
            }

            long byteCount = 0;
            foreach (var (source, destination) in copies)
            {
                var sourceLength = new FileInfo(source).Length;
                var destinationLength = new FileInfo(destination).Length;
                if (sourceLength != destinationLength || !SameHash(source, destination))
                    throw new IOException($"Verification failed for {Path.GetRelativePath(currentFolder, source)}.");
                byteCount = checked(byteCount + sourceLength);
            }
            if (DataFiles(targetFolder).Count(file => copies.Any(copy => string.Equals(copy.Destination, file, StringComparison.OrdinalIgnoreCase))) != copies.Count)
                throw new IOException("The copied file count did not match the source.");

            WritePointer(appHome, targetFolder);
            var oldCopy = KeepOldCopy(appHome, currentFolder);
            return new(true, targetFolder, FileCount: copies.Count, ByteCount: byteCount, OldCopy: oldCopy);
        }
        catch (Exception ex)
        {
            foreach (var file in createdFiles.AsEnumerable().Reverse())
                try { if (File.Exists(file)) File.Delete(file); } catch { }
            foreach (var directory in createdDirectories.AsEnumerable().Reverse()) TryRemoveEmptyDirectory(directory);
            if (!targetExisted) TryRemoveEmptyDirectory(targetFolder);
            return new(false, currentFolder, ex.Message);
        }
    }

    internal static string? ReadPointer(string appHome)
    {
        try
        {
            return JsonSerializer.Deserialize<LocationPointer>(File.ReadAllText(Path.Combine(appHome, PointerFileName)), Json)?.NotesFolder;
        }
        catch { return null; }
    }

    private static bool HasNotes(string folder) => File.Exists(Path.Combine(folder, "index.json")) || Directory.Exists(Path.Combine(folder, "notes"));

    private static List<string> DataFiles(string folder)
    {
        var files = new List<string>();
        foreach (var name in new[] { "index.json", "index.json.bak" })
        {
            var path = Path.Combine(folder, name);
            if (File.Exists(path)) files.Add(path);
        }
        foreach (var name in new[] { "notes", RecoveredFolderName })
        {
            var directory = Path.Combine(folder, name);
            if (Directory.Exists(directory)) files.AddRange(Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories));
        }
        return files.OrderBy(path => Path.GetRelativePath(folder, path), StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static void EnsureDirectory(string path, List<string> created)
    {
        if (File.Exists(path)) throw new IOException($"A file blocks folder creation: {path}");
        if (Directory.Exists(path)) return;
        var parent = Path.GetDirectoryName(path);
        if (parent is not null && !Directory.Exists(parent)) EnsureDirectory(parent, created);
        Directory.CreateDirectory(path);
        created.Add(path);
    }

    private static bool SameHash(string left, string right)
    {
        using var a = File.OpenRead(left);
        using var b = File.OpenRead(right);
        return CryptographicOperations.FixedTimeEquals(SHA256.HashData(a), SHA256.HashData(b));
    }

    internal static void WritePointer(string appHome, string notesFolder)
    {
        Directory.CreateDirectory(appHome);
        var pointer = Path.Combine(appHome, PointerFileName);
        var temporary = pointer + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(new LocationPointer(notesFolder), Json));
            if (File.Exists(pointer)) File.Replace(temporary, pointer, null, ignoreMetadataErrors: true);
            else File.Move(temporary, pointer);
        }
        catch
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); } catch { }
            throw;
        }
    }

    private static NotesOldCopyResult KeepOldCopy(string appHome, string currentFolder)
    {
        currentFolder = Path.TrimEndingDirectorySeparator(currentFolder);
        if (string.Equals(Path.TrimEndingDirectorySeparator(appHome), currentFolder, StringComparison.OrdinalIgnoreCase))
            return BackupAppHomeData(appHome);
        try
        {
            var parent = Path.GetDirectoryName(currentFolder);
            if (parent is null) return new(currentFolder, null, NotesOldCopyFailure.Other);
            var leaf = Path.GetFileName(currentFolder);
            var backup = Path.Combine(parent, $"{leaf} (moved {DateTime.Now:yyyy-MM-dd HHmm})");
            if (Directory.Exists(backup) || File.Exists(backup)) return new(currentFolder, null, NotesOldCopyFailure.NameCollision);
            Directory.Move(currentFolder, backup);
            return new(currentFolder, backup);
        }
        catch (Exception ex) { return new(currentFolder, null, RetentionFailure(ex)); }
    }

    private static NotesOldCopyResult BackupAppHomeData(string appHome)
    {
        var items = new[] { "index.json", "index.json.bak", "notes", RecoveredFolderName }
            .Select(name => Path.Combine(appHome, name)).Where(path => File.Exists(path) || Directory.Exists(path)).ToList();
        if (items.Count == 0) return new(appHome, null);
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var backup = Path.Combine(appHome, $"backup-before-move-{stamp}");
        for (var suffix = 1; Directory.Exists(backup) || File.Exists(backup); suffix++)
            backup = Path.Combine(appHome, $"backup-before-move-{stamp}-{suffix:00}");
        var moved = new List<(string From, string To, bool Directory)>();
        try
        {
            Directory.CreateDirectory(backup);
            foreach (var item in items)
            {
                var destination = Path.Combine(backup, Path.GetFileName(item));
                var isDirectory = Directory.Exists(item);
                if (isDirectory) Directory.Move(item, destination); else File.Move(item, destination);
                moved.Add((item, destination, isDirectory));
            }
            return new(appHome, backup);
        }
        catch (Exception ex)
        {
            foreach (var (from, to, isDirectory) in moved.AsEnumerable().Reverse())
                try { if (isDirectory) Directory.Move(to, from); else File.Move(to, from); } catch { }
            TryRemoveEmptyDirectory(backup);
            return new(appHome, null, RetentionFailure(ex));
        }
    }

    private static NotesOldCopyFailure RetentionFailure(Exception ex) => ex switch
    {
        UnauthorizedAccessException => NotesOldCopyFailure.AccessDenied,
        IOException when (ex.HResult & 0xffff) is 80 or 183 => NotesOldCopyFailure.NameCollision,
        IOException when (ex.HResult & 0xffff) is 5 => NotesOldCopyFailure.AccessDenied,
        IOException when (ex.HResult & 0xffff) is 32 or 33 => NotesOldCopyFailure.InUse,
        _ => NotesOldCopyFailure.Other
    };

    private static bool SameOrNested(string parent, string candidate)
    {
        parent = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        candidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return string.Equals(parent, candidate, StringComparison.OrdinalIgnoreCase)
            || candidate.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || candidate.StartsWith(parent + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static void TryRemoveEmptyDirectory(string path)
    {
        try { if (Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any()) Directory.Delete(path); } catch { }
    }

    private sealed record LocationPointer(string NotesFolder);
}
