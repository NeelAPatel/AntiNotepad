using AntiQuickPad.Controller;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;
using AntiQuickPad.ViewModels;
using AntiQuickPad.Shell;
using AntiQuickPad.Carousel;
using Avalonia.Controls.Primitives;
namespace AntiQuickPad.Views.Common;

public sealed partial class ResidentShellWindow : Window, IShellView, IResidentSurface
{
    private TextEditor _editor = null!;
    private readonly MainViewModel _vm;
    private readonly ShellServices _services;
    private readonly IWindowPlatform _platform;
    public Window Host => this;
    public TextEditor Editor => _editor;
    public MainViewModel ViewModel => _vm;
    private CarouselController _carousel = null!;
    private readonly TextBlock _count = new();
    private readonly ToggleButton _pin = new() { Content = "Pin" };
    
    private WindowSnapshot? _normalSettings;
    private readonly ResidentController _controller;
    private bool _binding { get => _controller.Binding; set => _controller.Binding = value; }
    private readonly Border _frame = new();
    private readonly TextBlock _status = new();
    private readonly StreamWriter _log;
    private bool _saveFailed;
    private bool _initialized => _controller.Initialized;
    private bool _quitting => _controller.Quitting;
    public double LastShowToFocusMs => _controller.LastShowToFocusMs;

    public ResidentShellWindow(MainViewModel viewModel, ShellServices services, string shellId = "quiet")
    {
        _vm = viewModel; _services = services; _platform = services.CreateWindowPlatform(this);
        _controller = new ResidentController(this, _vm, _platform, services);
        DataContext = _vm;
        var tokens = Tokens.DesignTokens.Quiet;
        Title = "AntiQuickPad"; Width = tokens.Window.Width; Height = tokens.Window.Height; MinWidth = tokens.WindowMin.Width; MinHeight = tokens.WindowMin.Height;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowDecorations = WindowDecorations.BorderOnly;
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = tokens.TopBar;
        RestoreWindowSnapshot();
        _shellId = shellId;
        _theme = new ThemeService(this, _vm.LookId);
        _theme.Changed += (_, _) => RebuildAppearance();
        Directory.CreateDirectory(Path.GetDirectoryName(Program.Paths.Log)!);
        _log = new StreamWriter(new FileStream(Program.Paths.Log, FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };
        RebuildAppearance();
        _controller.InitializeNavigation();
        _controller.InitializeViewModel();
        BindCurrent();

        _controller.InitializeAutosave();
        _controller.InitializeCue();
        AddHandler(KeyDownEvent, _controller.OnKeyDown, RoutingStrategies.Tunnel, true);
        Opened += (_, _) => _controller.OnOpened(ShellRender.IsRendering);
        SizeChanged += (_, _) => RememberNormalBounds();
        PositionChanged += (_, _) => RememberNormalBounds();
        Activated += (_, _) => _controller.OnActivated();
        Closing += (_, e) => _controller.OnClosing(e);
        Deactivated += (_, _) => _controller.OnDeactivated();
        Closed += (_, _) => _controller.OnClosed();
    }

    public void Toggle(long started = 0) => _controller.Toggle(started);
    public void ShowResident(string source, long started = 0) => _controller.ShowResident(source, started);
    public void HideResident() => _controller.HideResident();
    private void FocusEditor() => _controller.FocusEditor();

    private bool SaveNotes() => _controller.SaveNotes();

    private void BindCurrent() => _controller.BindCurrent();
    public void CreateNote() => _vm.CreateNote();
    public void DeleteCurrentNote() => _vm.DeleteCurrent();
    public void SelectNote(int index) => _vm.Select(index);
    private void ShowIndicator() => _count.Opacity = 1;
    private void UpdateStatus()
    {
        _count.Text = _vm.CountText;
        _saveFailed = _vm.Status.Failed;
        _status.Foreground = new SolidColorBrush(_saveFailed ? CurrentLook.Accent : CurrentLook.Muted);
        _status.Text = _vm.Status.Text; UpdateShellStatus();
    }

    private void RestoreWindowSnapshot()
    {
        try
        {
            var settings = _vm.LoadSettings();
            if (settings is null || !double.IsFinite(settings.Width) || !double.IsFinite(settings.Height)) return;
            _platform.Pinned = settings.Pinned; _vm.Pinned = settings.Pinned;
            if (settings.LayoutVersion < MainViewModel.CurrentLayoutVersion) return;
            _platform.Bounds = new WindowBounds(Math.Max(MinWidth, settings.Width), Math.Max(MinHeight, settings.Height), settings.X, settings.Y); WindowStartupLocation = WindowStartupLocation.Manual;
        }
        catch (Exception ex) { System.Diagnostics.Trace.WriteLine($"window-settings-load: {ex.Message}"); }
    }
    private void RememberNormalBounds()
    {
        if (_initialized && WindowState == WindowState.Normal && IsVisible)
            _normalSettings = new WindowSnapshot(Width, Height, Position.X, Position.Y, Topmost, _vm.ShellId, _vm.LookId);
    }
    private void SaveWindowSnapshot()
    {
        RememberNormalBounds();
        var settings = (_normalSettings ?? new WindowSnapshot(Width, Height, Position.X, Position.Y, Topmost, _vm.ShellId, _vm.LookId)) with { Pinned = Topmost, ShellId = _shellId, LookId = _theme.Preference };
        _vm.SaveSettings(settings);
    }
    public void Quit(int exitCode = 0, string source = "tray") => _controller.Quit(exitCode, source);

    IHotkeyService IResidentSurface.CreateHotkey(Action<long> pressed, string? gesture) => _services.CreateHotkey(this, pressed, gesture);
    void IResidentSurface.AttachMeasurement(bool registered) => Measurement.Attach(this, registered, RenderScaling);
    Task IResidentSurface.RunSelfTest() => RunSelfTest();
    void IResidentSurface.DisposeAppearance() => _theme.Dispose();
    void IResidentSurface.DisposeLog() => _log.Dispose();
    void IResidentSurface.RefreshPin(bool pinned) => _pin.IsChecked = pinned;
    void IResidentSurface.Minimize() => WindowState = WindowState.Minimized;
    bool IResidentSurface.IsSettingsOpen => SettingsOpen;
    void IResidentSurface.OpenSettings() { OpenSettingsWindow(); _barReveal?.Refresh(); }
    void IResidentSurface.CloseSettings() { _settings?.Hide(); _barReveal?.Refresh(); }
    void IResidentSurface.ApplyPreferences() => ApplyPreferencesNow();
    string IResidentSurface.ShellId => _shellId;
    void IResidentSurface.ApplyShell(string id) { _shellId = id; RebuildAppearance(); }
    void IResidentSurface.ApplyTheme(string preference) => _theme.Set(preference);
    bool IResidentSurface.IsKeyboardFocusWithin => _editor.TextArea.IsKeyboardFocusWithin;
    bool IResidentSurface.IsForeground => Measurement.IsForeground(this);
    void IResidentSurface.RecordFrame() => Measurement.Frame(this);
    void IResidentSurface.SetFocusCue(bool active) => _frame.BorderBrush = new SolidColorBrush(active ? CurrentLook.Accent : CurrentLook.Muted);
    void IResidentSurface.CancelMotion() => _carousel.Cancel();
    void IResidentSurface.RefreshHint() => RefreshHint();
    void IResidentSurface.RefreshStatus() { UpdateStatus(); _settings?.RefreshHotkey(); }
    void IResidentSurface.SaveWindowSettings() => SaveWindowSnapshot();
    void IResidentSurface.Log(string message) => Log(message);

    private void Log(string message) => _log.WriteLine($"{DateTimeOffset.UtcNow:O} {message}");

    public void MeasurementHeavy()
    {
        _vm.PopulateMeasurementFixture(); BindCurrent(); SaveNotes();
    }
}


