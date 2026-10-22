using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
namespace AntiQuickPad;
public sealed class App : Application
{
    public override void Initialize()
    {
        RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Default;
        Styles.Add(new FluentTheme());
        Styles.Add(new StyleInclude(new Uri("avares://AntiQuickPad/"))
        { Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml") });
    }
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnExplicitShutdown;
            if (Design.DesignEntry.Arguments is not null)
            {
                Design.DesignEntry.Start(desktop);
                base.OnFrameworkInitializationCompleted();
                return;
            }
            var vm = new ViewModels.MainViewModel(new Core.NotesSession(Program.Paths.Notes), new Core.SettingsStore(Program.Paths.Settings));
            var services = Shell.HiddenWindows.Enabled
                ? new Shell.ShellServices(createWindowPlatform: window => new Shell.HiddenWindowPlatform(window),
                    createHotkey: (_, _, gesture) => new Shell.MemoryHotkeyService(gesture), startup: new Shell.MemoryStartupService())
                : new Shell.ShellServices();
            try { vm.LoadSettings(); }
            catch (Exception ex) { System.Diagnostics.Trace.WriteLine($"window-settings-load: {ex.Message}"); }
            // Classic/Ink was the pre-shell-choice default. Adopt Quiet/System for those legacy settings.
            var selected = Program.RequestedShell ?? (File.Exists(Program.Paths.Settings) && !(vm.ShellId == "classic" && vm.LookId == "Ink") ? vm.ShellId : "quiet");
            Views.IShellView shell = selected switch
            {
                "classic" => new Views.Classic.ClassicShellWindow(vm, services),
                "quiet" => new Views.Common.ResidentShellWindow(vm, services, selected),
                _ => new Views.Classic.ClassicShellWindow(vm, services)
            };
            if (Shell.HiddenWindows.Enabled) Shell.HiddenWindows.Register(shell.Host);
            desktop.MainWindow = shell.Host;
            Program.Attach(shell);
            if (Views.Common.ShellRender.IsRendering && shell is Views.Common.ResidentShellWindow resident)
                resident.Opened += async (_, _) => await Views.Common.ShellRender.Capture(resident);
        }
        base.OnFrameworkInitializationCompleted();
    }
}

