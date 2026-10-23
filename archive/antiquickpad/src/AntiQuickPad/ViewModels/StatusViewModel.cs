namespace AntiQuickPad.ViewModels;
public sealed class StatusViewModel : ObservableObject
{
    public string Text => Failed ? "Could not save. Check disk access." : Saving ? "Saving" : "Saved";
    public bool Failed { get; private set; }
    public bool Saving { get; private set; }
    public string? Error { get; private set; }
    internal void Set(bool saving, bool failed, string? error = null)
    { Saving = saving; Failed = failed; Error = error; Changed(nameof(Text)); Changed(nameof(Failed)); Changed(nameof(Saving)); Changed(nameof(Error)); }
}
