using AntiNotepad.Model;
using AntiNotepad.Platform;

namespace AntiNotepad;

internal sealed class NotesSession : INotesLocation
{
    private readonly Action<string> _log;

    internal Store Store { get; }
    internal string AppHome { get; }
    public string DataFolder { get; private set; }
    public string? NotesLocationError { get; private set; }

    internal NotesSession(Store store, string appHome, string dataFolder, string? notesLocationError, Action<string> log)
    {
        Store = store;
        AppHome = appHome;
        DataFolder = dataFolder;
        NotesLocationError = notesLocationError;
        _log = log;
    }

    internal bool MoveNotes(string target, out string message, out NotesMoveResult? moveResult)
    {
        moveResult = null;
        if (Store is null || !Store.IsAvailable || NotesLocationError is not null)
        {
            message = NotesLocationError ?? $"The current notes folder is unavailable: {DataFolder}";
            return false;
        }
        if (!Store.Save())
        {
            message = Store.LastError is { } saveError
                ? $"The current notes could not be saved before moving them. {saveError}"
                : "The current notes could not be saved before moving them.";
            return false;
        }
        var result = NotesLocation.Move(AppHome, DataFolder, target);
        moveResult = result;
        if (!result.Success) { message = result.Error ?? "The notes folder could not be changed."; return false; }
        Store.SwitchFolder(result.Folder);
        DataFolder = result.Folder;
        var unavailable = $"The notes folder is unavailable: {result.Folder}";
        NotesLocationError = Store.IsAvailable ? null : unavailable;
        message = result.Warning ?? (Store.IsAvailable ? result.OldCopy!.Message(result.FileCount) : unavailable);
        if (result.Warning is { } warning) _log(warning);
        return true;
    }

    internal bool UseFolderOnNextStart(string folder, out string message)
    {
        if (NotesLocationError is null)
        {
            message = "The current notes folder is available. Use Change location to migrate notes.";
            return false;
        }

        string target;
        try
        {
            if (string.IsNullOrWhiteSpace(folder) || !Path.IsPathFullyQualified(folder))
            {
                message = "Choose a full path to an existing notes folder.";
                return false;
            }

            target = Path.GetFullPath(folder);
            if (!Directory.Exists(target))
            {
                message = $"The selected notes folder is unavailable: {target}";
                return false;
            }

            _ = Directory.EnumerateFileSystemEntries(target).Take(1).ToList();
            if (SameOrNested(DataFolder, target))
            {
                message = "Choose a folder separate from the unavailable notes folder.";
                return false;
            }
        }
        catch (Exception ex)
        {
            message = $"The selected notes folder is unavailable: {folder}. {ex.Message}";
            return false;
        }

        try { NotesLocation.WritePointer(AppHome, target); }
        catch (Exception ex)
        {
            message = $"Couldn't set the notes folder: {ex.Message}";
            return false;
        }

        message = $"Notes folder set to {target}. Quit and start AntiNotepad again to load it.";
        return true;
    }

    private static bool SameOrNested(string left, string right)
    {
        left = Path.TrimEndingDirectorySeparator(Path.GetFullPath(left));
        right = Path.TrimEndingDirectorySeparator(Path.GetFullPath(right));
        return IsAtOrBelow(left, right) || IsAtOrBelow(right, left);

        static bool IsAtOrBelow(string parent, string candidate)
        {
            var relative = Path.GetRelativePath(parent, candidate);
            return !Path.IsPathRooted(relative)
                && relative != ".."
                && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal);
        }
    }
}
