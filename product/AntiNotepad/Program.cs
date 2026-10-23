using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;
using AntiNotepad.Model;
using AntiNotepad.Platform;
using AntiNotepad.UI;

namespace AntiNotepad;

internal static class Program
{
    /// <summary>The resolved product data directory, including a custom dev/test path when configured.</summary>
    public static string DataFolder { get; private set; } = "";
    public static bool StartHidden { get; private set; }
    public static bool IsDev { get; private set; }
    public static bool SoftwareRendering { get; private set; }
    public static Store Store { get; private set; } = null!;
    private static long GpuCacheBytes => long.TryParse(Environment.GetEnvironmentVariable(AppInfo.Env("GPU_CACHE_MB")), out var mb) ? mb * 1048576 : 8 * 1048576;
    private static PadWindow? _window;
    private static int _pendingShow;

    [STAThread]
    public static int Main(string[] args)
    {
        if (args is ["--selftest"]) return SelfTest.Run();
        Environment.SetEnvironmentVariable("AVALONIA_TELEMETRY_OPTOUT", "1");
        // Any crash leaves its stack in the data folder's product log.
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log("crash " + e.ExceptionObject);
        // UI-thread exceptions end the process before the handler above runs; log them on the way out too.
        Dispatcher.UIThread.UnhandledException += (_, e) => Log("crash " + e.Exception);

        // A custom data folder (dev/test runs) gets its own instance, so it can run beside the real one.
        // Decided before --quit, so quitting a dev run never quits the real copy.
        var customDataFolder = Environment.GetEnvironmentVariable(AppInfo.Env("DATA"));
        var usesCustomData = customDataFolder is { Length: > 0 };
        // Only the repo launch scripts opt into dev UI; a data override alone remains isolated.
        IsDev = usesCustomData && Environment.GetEnvironmentVariable(AppInfo.Env("DEV")) == "1";
        var instanceId = usesCustomData ? AppInfo.DevInstanceId : AppInfo.InstanceId;
        if (args is ["--quit"]) return SingleInstanceService.RequestQuit(instanceId);
        StartHidden = args.Contains("--hidden");
        using var instance = new SingleInstanceService(RequestShow, instanceId, RequestQuit);
        if (!instance.IsPrimary) return 0;

        // The new identity lock is held before moving the default folder, so two new launches cannot race.
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dataFolder = DefaultDataFolderMigration.Resolve(localAppData, customDataFolder);
        DataFolder = dataFolder.Folder;
        if (dataFolder.Error is { } migrationError) Log(migrationError);
        if (!usesCustomData) Startup.MigrateLegacy(Log);

        // The store is plain files, so it loads before Avalonia: the renderer depends on a setting.
        Store = new Store(DataFolder);
        Store.Load();
        Welcome.SeedIfNew(Store.Data);
        Showcase.EnsureInDev(Store.Data, IsDev);
        var builder = AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace()
            // A scratchpad redraws little: a small GPU resource cache instead of Skia's default.
            .With(new Avalonia.SkiaOptions { MaxGpuResourceSizeBytes = GpuCacheBytes });
        // Solid windows need no GPU compositing: software rendering uses about 80 MB less memory.
        SoftwareRendering = Environment.GetEnvironmentVariable(AppInfo.Env("RENDER")) is { } r ? r == "software" : Store.Data.Settings.Material == "solid";
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
            if (string.IsNullOrWhiteSpace(DataFolder)) return;
            var path = Path.Combine(DataFolder, AppInfo.LogFileName);
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
        Styles.Add(new StyleInclude(new Uri(AppInfo.ResourceUri)) { Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml") });
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
            window.HotkeyStatusChanged += status => _tray.Tooltip = $"{AppInfo.Name} · {status}";
            window.InstallHooks();
            Program.Attach(window);
            if (!Program.StartHidden) window.ShowResident();
            else window.Prewarm();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
