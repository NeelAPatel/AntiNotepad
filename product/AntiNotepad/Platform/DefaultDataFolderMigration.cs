using AntiNotepad;

namespace AntiNotepad.Platform;

internal readonly record struct DataFolderResolution(string Folder, string? Error, bool Migrated);

/// <summary>Moves the legacy default data directory once, while leaving custom data paths alone.</summary>
internal static class DefaultDataFolderMigration
{
    // Deliberately names the pre-rename product folder for this one-time migration.
    internal const string LegacyFolderName = "Antipad";

    internal static DataFolderResolution Resolve(
        string localAppData,
        string? customFolder,
        Action<string, string>? moveDirectory = null)
    {
        if (customFolder is { Length: > 0 }) return new(customFolder, null, false);

        var oldFolder = Path.Combine(localAppData, LegacyFolderName);
        var newFolder = Path.Combine(localAppData, AppInfo.DataFolderName);
        try
        {
            var newKind = GetPathKind(newFolder);
            if (newKind == PathKind.Directory) return new(newFolder, null, false);
            if (newKind == PathKind.File)
                return new(oldFolder, $"Cannot migrate data: {newFolder} exists as a file; continuing with {oldFolder}.", false);

            var oldKind = GetPathKind(oldFolder);
            if (oldKind == PathKind.Missing) return new(newFolder, null, false);
            if (oldKind == PathKind.File)
                return new(oldFolder, $"Cannot migrate data: {oldFolder} exists as a file; continuing with the legacy path.", false);

            (moveDirectory ?? Directory.Move)(oldFolder, newFolder);
            return new(newFolder, null, true);
        }
        catch (Exception ex)
        {
            // Directory.Move is an atomic rename because both paths share LocalAppData's parent.
            // If the move fails, the original tree remains the source of truth and Store uses it.
            return new(oldFolder, $"Could not migrate data from {oldFolder} to {newFolder}: {ex.Message}; continuing with the legacy folder.", false);
        }
    }

    private static PathKind GetPathKind(string path)
    {
        try
        {
            var attributes = File.GetAttributes(path);
            return (attributes & FileAttributes.Directory) != 0 ? PathKind.Directory : PathKind.File;
        }
        catch (FileNotFoundException) { return PathKind.Missing; }
        catch (DirectoryNotFoundException) { return PathKind.Missing; }
    }

    private enum PathKind { Missing, File, Directory }
}
