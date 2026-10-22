using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;
using Antipad.Model;
using Antipad.Platform;
using Antipad.UI;

namespace Antipad;

internal static class Program
{
    /// <summary>Separate from AntiQuickPad's folder so both apps can run side by side.</summary>
    public static string DataFolder { get; private set; } = "";
    public static bool StartHidden { get; private set; }
    public static bool IsDev { get; private set; }
    /// <summary>Started from a Claude Code shell (CLAUDECODE=1 is inherited by everything it launches).</summary>
    public static bool LaunchedByClaude { get; } = Environment.GetEnvironmentVariable("CLAUDECODE") == "1";
    public static bool SoftwareRendering { get; private set; }
    public static Store Store { get; private set; } = null!;
    private static long GpuCacheBytes => long.TryParse(Environment.GetEnvironmentVariable("ANTIPAD_GPU_CACHE_MB"), out var mb) ? mb * 1048576 : 8 * 1048576;
    private static PadWindow? _window;
    private static int _pendingShow;

    [STAThread]
    public static int Main(string[] args)
    {
        Environment.SetEnvironmentVariable("AVALONIA_TELEMETRY_OPTOUT", "1");
        // Any crash leaves its stack in the data folder's antipad.log.
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log("crash " + e.ExceptionObject);
        if (args is ["--selftest"]) return SelfTest.Run();
        if (args is ["--quit"]) return SingleInstanceService.RequestQuit();
        DataFolder = Environment.GetEnvironmentVariable("ANTIPAD_DATA")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Antipad");
        StartHidden = args.Contains("--hidden");
        // A custom data folder (dev/test runs) gets its own instance, so it can run beside the real one.
        IsDev = Environment.GetEnvironmentVariable("ANTIPAD_DATA") is { Length: > 0 };
        using var instance = new SingleInstanceService(RequestShow, IsDev ? "Antipad-dev" : "Antipad", RequestQuit);
        if (!instance.IsPrimary) return 0;
        // The store is plain files, so it loads before Avalonia: the renderer depends on a setting.
        Store = new Store(DataFolder);
        Store.Load();
        Welcome.SeedIfNew(Store.Data);
        var builder = AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace()
            // A scratchpad redraws little: a small GPU resource cache instead of Skia's default.
            .With(new Avalonia.SkiaOptions { MaxGpuResourceSizeBytes = GpuCacheBytes });
        // Solid windows need no GPU compositing: software rendering uses about 80 MB less memory.
        SoftwareRendering = Environment.GetEnvironmentVariable("ANTIPAD_RENDER") is { } r ? r == "software" : Store.Data.Settings.Material == "solid";
        if (SoftwareRendering) builder = builder.With(new Win32PlatformOptions { RenderingMode = [Win32RenderingMode.Software] });
        return builder.StartWithClassicDesktopLifetime(args);
    }

    private static void RequestShow()
    {
        if (_window is { } w) Dispatcher.UIThread.Post(w.ShowResident);
        else Interlocked.Exchange(ref _pendingShow, 1);
    }

    private static void RequestQuit()
    {
        if (_window is { } w) Dispatcher.UIThread.Post(w.Quit);
    }

    public static void Log(string line)
    {
        try
        {
            var path = Path.Combine(DataFolder, "antipad.log");
            // Kept tiny: a timing line per hotkey press, reset once it passes 64 KB.
            if (File.Exists(path) && new FileInfo(path).Length > 64 * 1024) File.Delete(path);
            File.AppendAllText(path, $"{DateTime.Now:s} {line}{Environment.NewLine}");
        }
        catch { }
    }

    public static void Attach(PadWindow window)
    {
        _window = window;
        if (Interlocked.Exchange(ref _pendingShow, 0) != 0) Dispatcher.UIThread.Post(window.ShowResident);
    }
}

public sealed class App : Application
{
    private Tray? _tray;

    public override void Initialize()
    {
        Styles.Add(new Avalonia.Themes.Fluent.FluentTheme());
        Styles.Add(new StyleInclude(new Uri("avares://Antipad/")) { Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml") });
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnExplicitShutdown;
            var store = Program.Store;
            var window = new PadWindow(store);
            window.QuitRequested = () => { _tray?.Dispose(); desktop.Shutdown(); };
            _tray = new Tray(window.ShowResident, () => { window.ShowResident(); window.NewNote(); }, window.OpenSettings, window.Quit,
                () => store.Data.Slots.Select(n => n.IsEmpty ? "empty" : n.Title).ToList(),
                slot => { window.ShowResident(); if (store.Data.ViewSlot != slot) window.OpenSlot(slot); });
            window.HotkeyStatusChanged += status => _tray.Tooltip = $"Antipad · {status}";
            window.InstallHooks();
            Program.Attach(window);
            if (!Program.StartHidden) window.ShowResident();
            else window.Prewarm();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
