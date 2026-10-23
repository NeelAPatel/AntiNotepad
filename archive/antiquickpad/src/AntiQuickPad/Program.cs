using Avalonia;
using Avalonia.Threading;
using AntiQuickPad.Shell;
namespace AntiQuickPad;
internal static class Program
{
    public static bool SelfTest { get; private set; }
    public static bool QuitResidentTest { get; private set; }
    public static RuntimePaths Paths { get; private set; } = null!;
    public static string? RequestedShell { get; private set; }
    private static Views.IShellView? _window;
    private static int _pendingShow;
    private static int _pendingQuit;
    public static void Attach(Views.IShellView window)
    {
        Volatile.Write(ref _window, window);
        if (Interlocked.Exchange(ref _pendingQuit, 0) != 0)
        { Dispatcher.UIThread.Post(() => window.Quit(source: "command")); return; }
        if (Interlocked.Exchange(ref _pendingShow, 0) != 0)
            Dispatcher.UIThread.Post(() => window.ShowResident("second-launch"));
    }
    private static void RequestShow()
    {
        Interlocked.Exchange(ref _pendingShow, 1);
        var window = Volatile.Read(ref _window);
        if (window is null) return;
        Interlocked.Exchange(ref _pendingShow, 0);
        Dispatcher.UIThread.Post(() => window.ShowResident("second-launch"));
    }
    private static void RequestQuit()
    {
        Interlocked.Exchange(ref _pendingQuit, 1);
        var window = Volatile.Read(ref _window);
        if (window is null) return;
        Interlocked.Exchange(ref _pendingQuit, 0);
        Dispatcher.UIThread.Post(() => window.Quit(source: "command"));
    }
    [STAThread]
    public static int Main(string[] args)
    {
        Environment.SetEnvironmentVariable("AVALONIA_TELEMETRY_OPTOUT", "1");
        HiddenWindows.Initialize(args);
        var shellArg = Array.IndexOf(args, "--shell");
        RequestedShell = shellArg >= 0 && shellArg + 1 < args.Length ? args[shellArg + 1].ToLowerInvariant() : Environment.GetEnvironmentVariable("ANTIQUICKPAD_SHELL");
        if (args.Length > 0 && args[0] == "--render-shell")
        {
            if (args.Length != 4 || args[1] != "quiet" || args[2] is not ("light" or "dark"))
            { Console.Error.WriteLine("Usage: --render-shell <quiet> <light|dark> <outdir>"); return 2; }
            Views.Common.ShellRender.Initialize(args); RequestedShell = args[1];
        }
        if (args.Length > 0 && args[0] is "--render-design" or "--probe-backdrop" or "--render-variants" or "--glass-check")
            return HiddenWindows.Finish(Design.DesignEntry.Run(args), args[0]);
        if (args.Contains("--measure")) return Measurement.Run(args).GetAwaiter().GetResult();
        Measurement.InitializeChild(args);
        if (args is ["--quit"]) return SingleInstanceService.RequestQuit();
        if (args is ["--selftest-quit", var quitId]) return SingleInstanceService.RequestQuit(quitId);
        if (args is ["--selftest-quit-stalled", var stalledId, var ready])
        {
            using var stalled = new SingleInstanceService(() => { }, stalledId);
            File.WriteAllText(ready, "ready");
            Thread.Sleep(7000); return 0;
        }
        if (args is ["--selftest-signal", var instanceId])
        {
            using var secondary = new SingleInstanceService(() => { }, instanceId);
            return secondary.IsPrimary ? 1 : 0;
        }
        if (args is ["--selftest-reload", var directory])
        {
            return ViewModels.MainViewModel.VerifyReload(directory, "# editor round trip\nsecond line", 7, 1) ? 0 : 1;
        }
        QuitResidentTest = args is ["--selftest-quit-resident", _, _];
        SelfTest = args.Contains("--selftest", StringComparer.OrdinalIgnoreCase);
        Paths = Measurement.ChildRoot is { } root ? new RuntimePaths(root) : QuitResidentTest ? new RuntimePaths(args[2]) : SelfTest || Views.Common.ShellRender.IsRendering ? RuntimePaths.ForSelfTest() : RuntimePaths.Default;
        try
        {
            using ISingleInstanceService? instance = SelfTest || Views.Common.ShellRender.IsRendering ? null : new SingleInstanceService(RequestShow, Measurement.InstanceId ?? (QuitResidentTest ? args[1] : "AntiQuickPad"), RequestQuit);
            if (instance is { IsPrimary: false }) return 0;
            var exitCode = AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().StartWithClassicDesktopLifetime(args);
            if (SelfTest && exitCode == 0)
            {
                var log = File.ReadAllText(Paths.Log);
                if (log.LastIndexOf("quit-start", StringComparison.Ordinal) <= log.LastIndexOf("quit-aborted", StringComparison.Ordinal) ||
                    log.LastIndexOf("quit-finish window-closed=true", StringComparison.Ordinal) <= log.LastIndexOf("quit-start", StringComparison.Ordinal))
                { Console.Error.WriteLine("SELFTEST FAIL: quit start/finish log missing."); return 1; }
                Console.WriteLine("T006 PASS: successful quit start/finish logs.");
            }
            return HiddenWindows.Finish(exitCode, SelfTest ? "selftest" : QuitResidentTest ? "quit-resident child" : "render-shell");
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}

