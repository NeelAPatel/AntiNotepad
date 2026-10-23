namespace InputProbe;

internal static class RuntimePaths
{
    public static string Root { get; } = FindRoot();
    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "InputProbe.csproj"))) return directory.FullName;
        throw new InvalidOperationException("Run this disposable spike from its spikes/A checkout build output.");
    }
}
