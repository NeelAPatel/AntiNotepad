namespace AntiNotepad.Model;

internal interface INotesLocation
{
    string DataFolder { get; }
    string? NotesLocationError { get; }
}
