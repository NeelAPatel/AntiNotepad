using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Styling;

namespace AntiQuickPad.Views.Common;

// UI-only preference. Avalonia's Windows platform settings publish live app-theme changes.
internal sealed class ThemeService : IDisposable
{
    private readonly Window _window;
    private readonly IPlatformSettings? _platform;
    private bool? _simulatedDark;
    public string Preference { get; private set; }
    public bool IsDark => Preference == "dark" || Preference == "system" &&
        (_simulatedDark ?? _platform?.GetColorValues().ThemeVariant == PlatformThemeVariant.Dark);
    public event EventHandler? Changed;
    public ThemeService(Window window, string preference)
    {
        _window = window; _platform = Avalonia.Application.Current?.PlatformSettings;
        Preference = Normalize(preference);
        if (_platform is not null) _platform.ColorValuesChanged += OnSystemChanged;
        Apply();
    }
    private static string Normalize(string value) => value.ToLowerInvariant() is "light" or "dark" or "system" ? value.ToLowerInvariant() : "system";
    public void Set(string preference) { Preference = Normalize(preference); Apply(); Changed?.Invoke(this, EventArgs.Empty); }
    private void Apply() => _window.RequestedThemeVariant = IsDark ? ThemeVariant.Dark : ThemeVariant.Light;
    private void OnSystemChanged(object? sender, PlatformColorValues values)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => { if (Preference == "system") { Apply(); Changed?.Invoke(this, EventArgs.Empty); } });
    }
    internal void SimulateSystem(bool? dark) { _simulatedDark = dark; if (Preference == "system") { Apply(); Changed?.Invoke(this, EventArgs.Empty); } }
    public void Dispose() { if (_platform is not null) _platform.ColorValuesChanged -= OnSystemChanged; }
}

