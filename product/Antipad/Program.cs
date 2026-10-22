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
    private static PadWindow? _window;
    private static int _pendingShow;

    [STAThread]
    public static int Main(string[] args)
    {
        Environment.SetEnvironmentVariable("AVALONIA_TELEMETRY_OPTOUT", "1");
        if (args is ["--selftest"]) return SelfTest.Run();
        if (args is ["--quit"]) return SingleInstanceService.RequestQuit();
        DataFolder = Environment.GetEnvironmentVariable("ANTIPAD_DATA")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Antipad");
        StartHidden = args.Contains("--hidden");
        using var instance = new SingleInstanceService(RequestShow, "Antipad", RequestQuit);
        if (!instance.IsPrimary) return 0;
        return AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().StartWithClassicDesktopLifetime(args);
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
            var store = new Store(Program.DataFolder);
            store.Load();
            Welcome.SeedIfNew(store.Data);
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
