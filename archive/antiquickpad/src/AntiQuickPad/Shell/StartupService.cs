using Microsoft.Win32;

namespace AntiQuickPad.Shell;

/// <summary>M11a-settings-function: HKCU Run entry pointing at the apphost next to this build.</summary>
public sealed class RegistryStartupService : IStartupService
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "AntiQuickPad";
    // The apphost, never dotnet.exe (Environment.ProcessPath when started as `dotnet AntiQuickPad.dll`).
    internal static string Command => "\"" + Path.Combine(AppContext.BaseDirectory, "AntiQuickPad.exe") + "\"";

    public bool IsEnabled
    {
        get
        {
            if (!OperatingSystem.IsWindows()) return false;
            using var key = Registry.CurrentUser.OpenSubKey(RunKey);
            return key?.GetValue(ValueName) is string value && string.Equals(value, Command, StringComparison.OrdinalIgnoreCase);
        }
    }

    public void SetEnabled(bool enabled)
    {
        if (!OperatingSystem.IsWindows()) return;
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled) key.SetValue(ValueName, Command);
        else if (key.GetValue(ValueName) is not null) key.DeleteValue(ValueName);
    }
}

/// <summary>Selftest stand-in: never touches the registry.</summary>
public sealed class MemoryStartupService : IStartupService
{
    public bool IsEnabled { get; private set; }
    public void SetEnabled(bool enabled) => IsEnabled = enabled;
}

/// <summary>Selftest stand-in hotkey: registers unless the gesture is the simulated conflict.</summary>
public sealed class MemoryHotkeyService(string? gesture) : IHotkeyService
{
    internal const string Conflict = "Ctrl+Alt+F12";
    private readonly string _label = string.IsNullOrWhiteSpace(gesture) ? GlobalHotkey.DefaultGesture.Label : GlobalHotkey.HotkeyGesture.Parse(gesture).Label;
    public bool IsRegistered => !string.Equals(_label, Conflict, StringComparison.OrdinalIgnoreCase);
    public int RegistrationError => IsRegistered ? 0 : 1409;
    public string? RegistrationErrorMessage => IsRegistered ? null : "Hot key is already registered.";
    public string GestureLabel => _label;
    public void Dispose() { }
}
