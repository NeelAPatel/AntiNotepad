// Chooses a safe runtime instance identity before startup or a quit request.
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
    private static long GpuCacheBytes => long.TryParse(Environment.GetEnvironmentVariable(AppInfo.Env("GPU_CACHE_MB")), out var mb) ? mb * 1048576 : 8 * 1048576;
    private static PadWindow? _window;
    private static readonly StartupRequestLatch _pendingRequests = new();

    [STAThread]
    public static int Main(string[] args)
    {
        if (args is ["--selftest"]) return SelfTest.Run();
        if (args is ["--design-dump", var path]) return SelfTest.WriteDesignDump(path);
        var log = new RuntimeLog();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => log.Flush();
        try { return RunApplication(args, log); }
        finally { log.Dispose(); }
    }

    private static int RunApplication(string[] args, RuntimeLog log)
    {
        Environment.SetEnvironmentVariable("AVALONIA_TELEMETRY_OPTOUT", "1");
        // Any crash leaves its stack in the data folder's product log.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            log.LogCrash(e.ExceptionObject);
        };
        // UI-thread exceptions end the process before the handler above runs; log them on the way out too.
        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            log.LogCrash(e.Exception);
        };

        // A custom data folder (dev/test runs) gets its own instance. Slots are ignored without one.
        // Decided before --quit, so quitting a dev run never quits the real copy.
        var customDataFolder = Environment.GetEnvironmentVariable(AppInfo.Env("DATA"));
        var usesCustomData = customDataFolder is { Length: > 0 };
        var slotName = usesCustomData ? AppInfo.SlotName : null;
        AppInfo.SetActiveSlotName(slotName);
        // Only the repo launch scripts opt into dev UI; a data override alone remains isolated.
        var isDev = usesCustomData && Environment.GetEnvironmentVariable(AppInfo.Env("DEV")) == "1";
        var instanceId = AppInfo.ResolveInstanceId(customDataFolder, slotName);
        if (args is ["--quit"]) return SingleInstanceService.RequestQuit(instanceId);
        var startHidden = args.Contains("--hidden");
        using var instance = new SingleInstanceService(RequestShow, instanceId, RequestQuit);
        if (!instance.IsPrimary) return 0;

        // The new identity lock is held before moving the default folder, so two new launches cannot race.
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var migratedHome = DefaultDataFolderMigration.Resolve(localAppData, customDataFolder);
        var appHome = migratedHome.Folder;
        log.SetHome(appHome);
        if (migratedHome.Error is { } migrationError) log.Write(migrationError);
        if (!usesCustomData) Startup.MigrateLegacy(log.Write);

        var documents = usesCustomData ? "" : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        NotesLocationResolution notes;
        try
        {
            if (usesCustomData) notes = NotesLocation.Resolve(appHome, documents, customDataFolder);
            else if (string.IsNullOrWhiteSpace(documents)) notes = new(appHome, appHome, "Windows did not provide a Documents folder path.");
            else notes = NotesLocation.Resolve(appHome, documents);
        }
        catch (Exception ex) { notes = new(appHome, appHome, $"Could not resolve the notes folder: {ex.Message}"); }
        appHome = notes.AppHome;
        log.SetHome(appHome);
        var dataFolder = notes.NotesFolder;
        var notesLocationError = notes.Error;
        var startupNotice = notes.Notice;
        if (notes.Error is { } locationError) log.Write(locationError);
        if (startupNotice is { } notice) log.Write(notice);

        // The store is plain files, so it loads before Avalonia: the renderer depends on a setting.
        Store store;
        try
        {
            store = new Store(dataFolder, createDirectories: usesCustomData && notes.Error is null);
        }
        catch (Exception ex)
        {
            store = new Store(dataFolder, createDirectories: false);
            notes = notes with { Error = $"The notes folder is unavailable: {dataFolder}. {ex.Message}" };
            notesLocationError = notes.Error;
            store.MarkUnavailable(notes.Error);
        }
        var settingsService = store.SettingsService;
        if (notes.Error is { } unavailable) store.MarkUnavailable(unavailable);
        store.Load();
        if (!store.IsAvailable && notesLocationError is null) notesLocationError = store.LastError ?? $"The notes folder is unavailable: {dataFolder}";
        if (store.IsAvailable)
        {
            Welcome.SeedIfNew(store.Data, settingsService);
            Showcase.EnsureInDev(store.Data, isDev);
        }
        if (store.LastError is { } storeError) log.Write(storeError);
        // Solid windows need no GPU compositing: software rendering uses about 80 MB less memory.
        var softwareRendering = Environment.GetEnvironmentVariable(AppInfo.Env("RENDER")) is { } r ? r == "software" : settingsService.Current.Material == "solid";
        var launch = new AppLaunch(log, isDev, startHidden, startupNotice, softwareRendering, Attach);
        var notesSession = new NotesSession(store, appHome, dataFolder, notesLocationError, log.Write);
        var builder = AppBuilder.Configure<App>(() => new App(launch, store, settingsService, notesSession)).UsePlatformDetect().LogToTrace()
            // A scratchpad redraws little: a small GPU resource cache instead of Skia's default.
            .With(new Avalonia.SkiaOptions { MaxGpuResourceSizeBytes = GpuCacheBytes });
        if (softwareRendering) builder = builder.With(new Win32PlatformOptions { RenderingMode = [Win32RenderingMode.Software] });
        return builder.StartWithClassicDesktopLifetime(args);
    }

    private static void RequestShow()
    {
        _pendingRequests.RequestShow(() =>
        {
            var window = _window ?? throw new InvalidOperationException("The window must be attached before dispatching a show request.");
            Dispatcher.UIThread.Post(window.ShowResident);
        });
    }

    private static void RequestQuit()
    {
        _pendingRequests.RequestQuit(() =>
        {
            var window = _window ?? throw new InvalidOperationException("The window must be attached before dispatching a quit request.");
            Dispatcher.UIThread.Post(window.Quit);
        });
    }

    public static void Attach(PadWindow window)
    {
        _window = window;
        _pendingRequests.Attach(
            () => Dispatcher.UIThread.Post(window.ShowResident),
            () => Dispatcher.UIThread.Post(window.Quit));
    }
}

/// <summary>Queues show and quit requests until the resident window is ready.</summary>
internal sealed class StartupRequestLatch
{
    private readonly object _gate = new();
    private bool _ready;
    private bool _pendingShow;
    private bool _pendingQuit;

    public void RequestShow(Action dispatch) => Request(ref _pendingShow, dispatch);

    public void RequestQuit(Action dispatch) => Request(ref _pendingQuit, dispatch);

    private void Request(ref bool pending, Action dispatch)
    {
        ArgumentNullException.ThrowIfNull(dispatch);
        lock (_gate)
        {
            if (!_ready) pending = true;
            else dispatch();
        }
    }

    public void Attach(Action dispatchShow, Action dispatchQuit)
    {
        ArgumentNullException.ThrowIfNull(dispatchShow);
        ArgumentNullException.ThrowIfNull(dispatchQuit);
        lock (_gate)
        {
            _ready = true;
            if (_pendingShow)
            {
                _pendingShow = false;
                dispatchShow();
            }
            if (_pendingQuit)
            {
                _pendingQuit = false;
                dispatchQuit();
            }
        }
    }
}

internal sealed record AppLaunch(RuntimeLog Log, bool IsDev, bool StartHidden, string? StartupNotice,
    bool SoftwareRendering, Action<PadWindow> Attach);

public sealed class App : Application
{
    private readonly AppLaunch _launch;
    private readonly Store _store;
    private readonly SettingsService _settingsService;
    private readonly NotesSession _notes;
    private Tray? _tray;

    internal App(AppLaunch launch, Store store, SettingsService settingsService, NotesSession notes)
    {
        _launch = launch;
        _store = store;
        _settingsService = settingsService;
        _notes = notes;
    }

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
            var window = new PadWindow(_store, _settingsService, _launch.Log, _launch.IsDev, _launch.SoftwareRendering, _notes);
            // One composition-root subscription preserves the ordered pad effects and persistence debounce.
            _settingsService.Changed += window.HandleSettingsChanged;
            // Win32 raises this on WM_QUERYENDSESSION, before Closing. In 12.1.4 the OS reason
            // is internal, and sign-out is labeled ApplicationShutdown rather than OSShutdown.
            // Our explicit Quit uses forced Shutdown(), which does not raise this event.
            desktop.ShutdownRequested += (_, e) =>
            {
                window.PrepareForSessionEnd();
                e.Cancel = false;
            };
            window.QuitRequested = () => { _tray?.Dispose(); desktop.Shutdown(); };
            _tray = new Tray(window.ShowResident, () => { window.ShowResident(); window.NewNote(); }, window.OpenSettings, window.Quit,
                () => _store.Data.Slots.Select(n => (n.Title, !n.IsEmpty)).ToList(),
                slot => { window.ShowResident(); if (_store.Data.ViewSlot != slot) window.OpenSlot(slot); });
            window.HotkeyStatusChanged += status => _tray.Tooltip = $"{AppInfo.DisplayName} · {status}";
            window.InstallHooks();
            _launch.Attach(window);
            if (!_launch.StartHidden) window.ShowResident();
            else window.Prewarm();
            if (_launch.StartupNotice is { } notice) window.Toast(notice);
        }
        base.OnFrameworkInitializationCompleted();
    }
}
