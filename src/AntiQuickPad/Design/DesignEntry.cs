using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Threading;

namespace AntiQuickPad.Design;

// Isolated from resident notes, hotkey, tray and single-instance ownership.
internal static class DesignEntry
{
    public static string[]? Arguments { get; private set; }
    public static int Run(string[] args)
    {
        Arguments = args;
        if (args[0] == "--render-design" && (args.Length != 3 || !new[] { "quiet", "card", "pro" }.Contains(args[1].ToLowerInvariant())))
        { Console.Error.WriteLine("Usage: --render-design <quiet|card|pro> <outdir>"); return 2; }
        if (args[0] == "--probe-backdrop")
        {
            try { BackdropProbeOptions.Parse(args); }
            catch (ArgumentException ex) { Console.Error.WriteLine(ex.Message); return 2; }
        }
        if (args[0] == "--render-variants")
        {
            try { VariantRenderOptions.Parse(args); }
            catch (ArgumentException ex) { Console.Error.WriteLine(ex.Message); return 2; }
        }
        if (args[0] == "--glass-check")
        {
            try { GlassCheckOptions.Parse(args); }
            catch (ArgumentException ex) { Console.Error.WriteLine(ex.Message); return 2; }
        }
        return AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().StartWithClassicDesktopLifetime(args);
    }

    public static void Start(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var args = Arguments!;
        if (args[0] == "--render-design")
        {
            var window = new DesignWindow(args[1].ToLowerInvariant());
            Shell.HiddenWindows.Register(window);
            desktop.MainWindow = window;
            window.Opened += async (_, _) =>
            {
                try { await window.RenderStates(Path.GetFullPath(args[2])); window.Close(); desktop.Shutdown(0); }
                catch (Exception ex) { Console.Error.WriteLine(ex); desktop.Shutdown(1); }
            };
        }
        else if (args[0] == "--render-variants")
        {
            new VariantRenderer(VariantRenderOptions.Parse(args)).Start(desktop);
        }
        else if (args[0] == "--glass-check")
        {
            new VariantRenderer(GlassCheckOptions.Parse(args)).Start(desktop);
        }
        else
        {
            var window = new BackdropProbeWindow(BackdropProbeOptions.Parse(args));
            desktop.MainWindow = window;
            window.Closed += (_, _) => desktop.Shutdown();
        }
    }
}
