using Avalonia;

namespace UiVariants;

internal static class Program
{
    public static string Variant = "ink";
    public static string? OutPath;

    [STAThread]
    public static int Main(string[] args)
    {
        Environment.SetEnvironmentVariable("AVALONIA_TELEMETRY_OPTOUT", "1");
        // --render <variant> <out.png>  |  --show <variant>
        var i = Array.FindIndex(args, a => a is "--render" or "--show");
        if (i < 0 || i + 1 >= args.Length || (args[i] == "--render" && i + 2 >= args.Length))
        {
            Console.Error.WriteLine("usage: --render <variant> <out.png> | --show <variant>\nvariants: " + string.Join(", ", Look.All.Keys) + ", carousel");
            return 2;
        }
        Variant = args[i + 1].ToLowerInvariant();
        if (args[i] == "--render") OutPath = Path.GetFullPath(args[i + 2]);
        return AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().StartWithClassicDesktopLifetime([]);
    }
}
