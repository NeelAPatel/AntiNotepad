namespace AntiQuickPad;
public sealed record RuntimePaths(string Root)
{
    public string Notes => Path.Combine(Root, "notes");
    public string Log => Path.Combine(Root, "logs", "shell.log");
    public string Settings => Path.Combine(Root, "settings.json");
    public static RuntimePaths Default => new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AntiQuickPad"));
    public static RuntimePaths ForSelfTest() => new(Path.Combine(AppContext.BaseDirectory,
        "selftest", Guid.NewGuid().ToString("N")));
}
