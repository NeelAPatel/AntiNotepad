using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace InputProbe;

public sealed class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = Program.Args.Contains("--probe", StringComparer.OrdinalIgnoreCase)
                ? new ProbeWindow()
                : new SpikeWindow();
        base.OnFrameworkInitializationCompleted();
    }
}
