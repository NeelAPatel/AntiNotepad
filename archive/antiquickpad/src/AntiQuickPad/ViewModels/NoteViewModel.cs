namespace AntiQuickPad.ViewModels;
public sealed class NoteViewModel(string id, string text, int caretOffset) : ObservableObject
{
    public string Id { get; } = id;
    public string Text { get; private set; } = text;
    public int CaretOffset { get; private set; } = caretOffset;
    internal void Update(string text, int caretOffset)
    {
        if (Text != text) { Text = text; Changed(nameof(Text)); }
        if (CaretOffset != caretOffset) { CaretOffset = caretOffset; Changed(nameof(CaretOffset)); }
    }
}
