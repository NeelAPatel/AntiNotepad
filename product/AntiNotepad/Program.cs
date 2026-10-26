using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;
using AntiNotepad.Model;
using AntiNotepad.Platform;
using AntiNotepad.UI;
using System.Collections.Concurrent;

namespace AntiNotepad;

internal static class Program
{
    /// <summary>The resolved user-visible notes directory.</summary>
    public static string DataFolder { get; private set; } = "";
    /// <summary>The per-user app directory for logs and the notes-folder pointer.</summary>
    public static string AppHome { get; private set; } = "";
    public static string? NotesLocationError { get; private set; }
    public static string? StartupNotice { get; private set; }
    public static bool StartHidden { get; private set; }
    public static bool IsDev { get; private set; }
    public static bool SoftwareRendering { get; private set; }
    public static Store Store { get; private set; } = null!;
    public static SettingsService SettingsService { get; private set; } = null!;
    private static long GpuCacheBytes => long.TryParse(Environment.GetEnvironmentVariable(AppInfo.Env("GPU_CACHE_MB")), out var mb) ? mb * 1048576 : 8 * 1048576;
    private static PadWindow? _window;
    private static readonly StartupRequestLatch _pendingRequests = new();

    [STAThread]
    public static int Main(string[] args)
    {
        if (args is ["--selftest"]) return SelfTest.Run();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => FlushLogs();
        try { return RunApplication(args); }
        finally { DisposeLogs(); }
    }

    private static int RunApplication(string[] args)
    {
        Environment.SetEnvironmentVariable("AVALONIA_TELEMETRY_OPTOUT", "1");
        // Any crash leaves its stack in the data folder's product log.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Log("crash " + e.ExceptionObject);
            FlushLogs();
        };
        // UI-thread exceptions end the process before the handler above runs; log them on the way out too.
        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            Log("crash " + e.Exception);
            FlushLogs();
        };

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
        AppHome = dataFolder.Folder;
        if (dataFolder.Error is { } migrationError) Log(migrationError);
        if (!usesCustomData) Startup.MigrateLegacy(Log);

        var documents = usesCustomData ? "" : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        NotesLocationResolution notes;
        try
        {
            if (usesCustomData) notes = NotesLocation.Resolve(AppHome, documents, customDataFolder);
            else if (string.IsNullOrWhiteSpace(documents)) notes = new(AppHome, AppHome, "Windows did not provide a Documents folder path.");
            else notes = NotesLocation.Resolve(AppHome, documents);
        }
        catch (Exception ex) { notes = new(AppHome, AppHome, $"Could not resolve the notes folder: {ex.Message}"); }
        AppHome = notes.AppHome;
        DataFolder = notes.NotesFolder;
        NotesLocationError = notes.Error;
        StartupNotice = notes.Notice;
        if (notes.Error is { } locationError) Log(locationError);
        if (notes.Notice is { } notice) Log(notice);

        // The store is plain files, so it loads before Avalonia: the renderer depends on a setting.
        try { Store = new Store(DataFolder, createDirectories: usesCustomData && notes.Error is null); }
        catch (Exception ex)
        {
            Store = new Store(DataFolder, createDirectories: false);
            notes = notes with { Error = $"The notes folder is unavailable: {DataFolder}. {ex.Message}" };
            NotesLocationError = notes.Error;
            StartupNotice = notes.Notice;
            Store.MarkUnavailable(notes.Error);
        }
        SettingsService = Store.SettingsService;
        if (notes.Error is { } unavailable) Store.MarkUnavailable(unavailable);
        Store.Load();
        if (!Store.IsAvailable && NotesLocationError is null) NotesLocationError = Store.LastError ?? $"The notes folder is unavailable: {DataFolder}";
        if (Store.IsAvailable)
        {
            Welcome.SeedIfNew(Store.Data, SettingsService);
            Showcase.EnsureInDev(Store.Data, IsDev);
        }
        if (Store.LastError is { } storeError) Log(storeError);
        var builder = AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace()
            // A scratchpad redraws little: a small GPU resource cache instead of Skia's default.
            .With(new Avalonia.SkiaOptions { MaxGpuResourceSizeBytes = GpuCacheBytes });
        // Solid windows need no GPU compositing: software rendering uses about 80 MB less memory.
        SoftwareRendering = Environment.GetEnvironmentVariable(AppInfo.Env("RENDER")) is { } r ? r == "software" : SettingsService.Current.Material == "solid";
        if (SoftwareRendering) builder = builder.With(new Win32PlatformOptions { RenderingMode = [Win32RenderingMode.Software] });
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

    public static void Log(string line)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(AppHome)) return;
            BackgroundLog logger;
            lock (_logGate)
            {
                _logger ??= new BackgroundLog(Path.Combine(AppHome, AppInfo.LogFileName));
                logger = _logger;
            }
            logger.Write(line);
        }
        catch { }
    }

    private static readonly object _logGate = new();
    private static BackgroundLog? _logger;

    public static void FlushLogs()
    {
        BackgroundLog? logger;
        lock (_logGate) logger = _logger;
        try { logger?.Flush(); } catch { }
    }

    private static void DisposeLogs()
    {
        BackgroundLog? logger;
        lock (_logGate) logger = _logger;
        try { logger?.Dispose(); } catch { }
    }

    public static bool MoveNotes(string target, out string message)
    {
        if (Store is null || !Store.IsAvailable || NotesLocationError is not null)
        {
            message = NotesLocationError ?? $"The current notes folder is unavailable: {DataFolder}";
            return false;
        }
        if (!Store.Save())
        {
            message = Store.LastError is { } saveError
                ? $"The current notes could not be saved before moving them. {saveError}"
                : "The current notes could not be saved before moving them.";
            return false;
        }
        var result = NotesLocation.Move(AppHome, DataFolder, target);
        if (!result.Success) { message = result.Error ?? "The notes folder could not be changed."; return false; }
        Store.SwitchFolder(result.Folder);
        DataFolder = result.Folder;
        var unavailable = $"The notes folder is unavailable: {result.Folder}";
        NotesLocationError = Store.IsAvailable ? null : unavailable;
        message = result.Warning ?? (Store.IsAvailable ? $"Notes moved to {result.Folder}." : unavailable);
        if (result.Warning is { } warning) Log(warning);
        return true;
    }

    public static void Attach(PadWindow window)
    {
        _window = window;
        _pendingRequests.Attach(
            () => Dispatcher.UIThread.Post(window.ShowResident),
            () => Dispatcher.UIThread.Post(window.Quit));
    }
}

/// <summary>Serializes best-effort product log writes on one background worker.</summary>
internal sealed class BackgroundLog : IDisposable
{
    private sealed record Entry(string? Line, TaskCompletionSource? Barrier);

    private readonly string _path;
    private readonly object _gate = new();
    private readonly BlockingCollection<Entry> _queue = new(new ConcurrentQueue<Entry>());
    private readonly Task _worker;
    private bool _disposed;

    internal BackgroundLog(string path)
    {
        _path = Path.GetFullPath(path);
        _worker = Task.Run(Consume);
    }

    internal void Write(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        lock (_gate)
        {
            if (_disposed) return;
            // Capture the timestamp here so it describes when the event happened, not when disk I/O runs.
            _queue.Add(new Entry($"{DateTime.Now:s} {line}", null));
        }
    }

    /// <summary>Waits until every write queued before this call has been attempted.</summary>
    internal void Flush()
    {
        var barrier = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task worker;
        var queued = false;
        lock (_gate)
        {
            worker = _worker;
            if (!_disposed)
            {
                _queue.Add(new Entry(null, barrier));
                queued = true;
            }
        }
        if (queued) barrier.Task.GetAwaiter().GetResult();
        else worker.GetAwaiter().GetResult();
    }

    private void Consume()
    {
        foreach (var entry in _queue.GetConsumingEnumerable())
        {
            if (entry.Barrier is { } barrier)
            {
                barrier.TrySetResult();
                continue;
            }

            try
            {
                var directory = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                // Match the existing product log rotation: reset the file once it has passed 64 KB.
                if (File.Exists(_path) && new FileInfo(_path).Length > 64 * 1024) File.Delete(_path);
                File.AppendAllText(_path, entry.Line + Environment.NewLine);
            }
            catch { }
        }
    }

    public void Dispose()
    {
        Task worker;
        lock (_gate)
        {
            if (!_disposed)
            {
                _disposed = true;
                _queue.CompleteAdding();
            }
            worker = _worker;
        }
        worker.GetAwaiter().GetResult();
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
            var window = new PadWindow(store, Program.SettingsService);
            // One composition-root subscription preserves the ordered pad effects and persistence debounce.
            Program.SettingsService.Changed += window.HandleSettingsChanged;
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
                () => store.Data.Slots.Select(n => (n.Title, !n.IsEmpty)).ToList(),
                slot => { window.ShowResident(); if (store.Data.ViewSlot != slot) window.OpenSlot(slot); });
            window.HotkeyStatusChanged += status => _tray.Tooltip = $"{AppInfo.DisplayName} · {status}";
            window.InstallHooks();
            Program.Attach(window);
            if (!Program.StartHidden) window.ShowResident();
            else window.Prewarm();
            if (Program.StartupNotice is { } notice) window.Toast(notice);
        }
        base.OnFrameworkInitializationCompleted();
    }
}
