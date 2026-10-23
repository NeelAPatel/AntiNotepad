using Avalonia.Controls;

namespace AntiQuickPad.Shell;

/// <summary>A registered system-wide hotkey and its current registration status.</summary>
public interface IHotkeyService : IDisposable
{
    bool IsRegistered { get; }
    int RegistrationError { get; }
    string? RegistrationErrorMessage { get; }
    string GestureLabel { get; }
}

/// <summary>Ownership and launch signals for the resident application instance.</summary>
public interface ISingleInstanceService : IDisposable
{
    bool IsPrimary { get; }
    bool WasAbandoned { get; }
}

/// <summary>The resident process tray icon and its three user actions.</summary>
public interface ITrayService : IDisposable
{
    void Initialize(Action toggle, Action quit, Action show, string hotkeyStatus);
    void SetHotkeyStatus(string hotkeyStatus);
}

/// <summary>M11a-settings-function: the per-user "start with Windows" entry.</summary>
public interface IStartupService
{
    bool IsEnabled { get; }
    void SetEnabled(bool enabled);
}

/// <summary>Window geometry in desktop pixels and Avalonia logical dimensions.</summary>
public readonly record struct WindowBounds(double Width, double Height, int X, int Y);

/// <summary>Window operations needed by shells without exposing native window handles.</summary>
public interface IWindowPlatform
{
    bool IsVisible { get; }
    bool Pinned { get; set; }
    WindowBounds Bounds { get; set; }
    void Show();
    void Hide();
    void Activate();
}

/// <summary>Factories owned by the application composition root for shell platform services.</summary>
public sealed class ShellServices
{
    public Func<Window, Action<long>, string?, IHotkeyService> CreateHotkey { get; }
    public Func<ITrayService> CreateTray { get; }
    public Func<Window, IWindowPlatform> CreateWindowPlatform { get; }
    public IStartupService Startup { get; }

    public ShellServices(
        Func<Window, Action<long>, string?, IHotkeyService>? createHotkey = null,
        Func<ITrayService>? createTray = null,
        Func<Window, IWindowPlatform>? createWindowPlatform = null,
        IStartupService? startup = null)
    {
        CreateHotkey = createHotkey ?? ((window, onPressed, gesture) => new GlobalHotkey(window, onPressed, gesture));
        CreateTray = createTray ?? (() => new AvaloniaTrayService());
        CreateWindowPlatform = createWindowPlatform ?? (window => new AvaloniaWindowPlatform(window));
        Startup = startup ?? new RegistryStartupService();
    }
}
