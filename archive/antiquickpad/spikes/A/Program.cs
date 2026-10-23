using Avalonia;

namespace InputProbe;

internal static class Program
{
    private static readonly long StartedAt = System.Diagnostics.Stopwatch.GetTimestamp();
    public static string[] Args { get; private set; } = [];
    public static double ElapsedMs => System.Diagnostics.Stopwatch.GetElapsedTime(StartedAt).TotalMilliseconds;
    public static string? PersistencePath { get; private set; }
    public static string PersistenceMarkerPath => PersistencePath + ".expected-marker.txt";
    public static bool PersistenceWrite => Args.Contains("--persistence-write", StringComparer.OrdinalIgnoreCase);
    public static bool PersistenceRead => Args.Contains("--persistence-read", StringComparer.OrdinalIgnoreCase);
    public static bool IsPersistenceMode => PersistenceWrite || PersistenceRead;

    [STAThread]
    public static void Main(string[] args)
    {
        Environment.SetEnvironmentVariable("AVALONIA_TELEMETRY_OPTOUT", "1");
        Args = args;
        if (!ConfigurePersistence(args)) return;
        if (args.Contains("--smoke", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = SmokeChecks.Run();
            return;
        }
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static bool ConfigurePersistence(string[] args)
    {
        var write = args.Contains("--persistence-write", StringComparer.OrdinalIgnoreCase);
        var read = args.Contains("--persistence-read", StringComparer.OrdinalIgnoreCase);
        var pathIndex = Array.FindIndex(args, x => string.Equals(x, "--data-path", StringComparison.OrdinalIgnoreCase));
        if (!write && !read && pathIndex < 0) return true;
        if (write == read || pathIndex < 0 || pathIndex + 1 >= args.Length)
        {
            Console.Error.WriteLine("Use exactly one persistence mode with --data-path <isolated-temp-file.json>.");
            Environment.ExitCode = 2;
            return false;
        }

        var path = Path.GetFullPath(args[pathIndex + 1]);
        var tempRoot = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!path.StartsWith(tempRoot, StringComparison.OrdinalIgnoreCase) || !path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Persistence smoke accepts a .json path inside the system temp directory only.");
            Environment.ExitCode = 2;
            return false;
        }
        if (write && (File.Exists(path) || File.Exists(path + ".expected-marker.txt")))
        {
            Console.Error.WriteLine("Refusing to overwrite existing persistence smoke files.");
            Environment.ExitCode = 2;
            return false;
        }
        if (read && (!File.Exists(path) || !File.Exists(path + ".expected-marker.txt")))
        {
            Console.Error.WriteLine("Persistence read needs existing data and expected-marker files from a write run.");
            Environment.ExitCode = 2;
            return false;
        }
        PersistencePath = path;
        return true;
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
}
