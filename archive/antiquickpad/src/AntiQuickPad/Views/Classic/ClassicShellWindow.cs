using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using AntiQuickPad.Editor;
using AntiQuickPad.ViewModels;
using AntiQuickPad.Shell;
using AntiQuickPad.Carousel;
using System.Text.Json;
using Avalonia.Controls.Primitives;
namespace AntiQuickPad.Views.Classic;

public sealed partial class ClassicShellWindow : Window, IShellView
{
    private readonly TextEditor _editor;
    private readonly MainViewModel _vm;
    private readonly ShellServices _services;
    private readonly IWindowPlatform _platform;
    public Window Host => this;
    public TextEditor Editor => _editor;
    public MainViewModel ViewModel => _vm;
    private readonly CarouselController _carousel;
    private readonly Dictionary<string, Vector> _scroll = new();
    private readonly TextBlock _count = new();
    private readonly ToggleButton _pin = new() { Content = "Pin" };
    
    private WindowSnapshot? _normalSettings;
    private readonly DispatcherTimer _autosave = new() { Interval = TimeSpan.FromMilliseconds(350) };
    private readonly DispatcherTimer _cue = new() { Interval = TimeSpan.FromMilliseconds(130) };
    private readonly Border _frame = new();
    private readonly TextBlock _status = new() { FontSize = Look.Default.StatusSize, Margin = new Thickness(Look.Default.EditorPadding, Look.Default.StatusVerticalMargin) };
    private readonly StreamWriter _log;
    private ITrayService? _tray;
    private IHotkeyService? _hotkey;
    private bool _binding, _quitting, _initialized, _saveFailed;
    private string? _lastSaveError;
    private string? _departingId;
    private long _showStarted;
    private string _quitSource = "tray";
    private string _showSource = "startup";
    private TaskCompletionSource<double>? _focusCompletion;
    public double LastShowToFocusMs { get; private set; }

    public ClassicShellWindow(MainViewModel viewModel, ShellServices services)
    {
        _vm = viewModel; _services = services; _platform = services.CreateWindowPlatform(this);
        _editor = EditorFactory.Create(Look.Default, _vm);
        DataContext = _vm;
        Title = "AntiQuickPad"; Width = Look.Default.WindowWidth; Height = Look.Default.WindowHeight;
        MinWidth = Look.Default.MinWindowWidth; MinHeight = Look.Default.MinWindowHeight;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowDecorations = WindowDecorations.Full; Topmost = false;
        RestoreWindowSnapshot();
        _normalSettings = new WindowSnapshot(Width, Height, Position.X, Position.Y, Topmost, _vm.ShellId, _vm.LookId);
        Background = new SolidColorBrush(Look.Default.Background);
        Foreground = new SolidColorBrush(Look.Default.Foreground);
        FontFamily = new FontFamily(Look.Default.InterfaceFont);
        Directory.CreateDirectory(Path.GetDirectoryName(Program.Paths.Log)!);
        _log = new StreamWriter(new FileStream(Program.Paths.Log, FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };
        _carousel = new CarouselController(_editor, Look.Default, _vm,
            i => _scroll.GetValueOrDefault(_vm.Notes[i].Id), ShowIndicator);
        var content = new DockPanel();
        var top = new WrapPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(8, 4) };
        void AddButton(string label, System.Windows.Input.ICommand command)
        {
            var button = new Button { Content = label, Margin = new Thickness(2), Background = new SolidColorBrush(Look.Default.CodePanel), Foreground = Foreground };
            button.Command = command; top.Children.Add(button);
        }
        AddButton("New note", _vm.CreateCommand); AddButton("Delete note", _vm.DeleteCommand);
        AddButton("Previous", _vm.PreviousCommand); AddButton("Next", _vm.NextCommand);
        _pin.IsChecked = Topmost;
        _pin.IsCheckedChanged += (_, _) => _vm.Pinned = _pin.IsChecked == true;
        top.Children.Add(_pin);
        var more = new Button { Content = "\u2026" };
        var quitItem = new MenuItem { Header = "Quit" }; quitItem.Click += (_, _) => Quit();
        more.Flyout = new MenuFlyout { Items = { quitItem } }; top.Children.Add(more);
        DockPanel.SetDock(top, Dock.Top); content.Children.Add(top);
        var bottom = new DockPanel { Background = new SolidColorBrush(Look.Default.CodePanel), LastChildFill = true };
        _count.Margin = new Thickness(12, 5); _count.Foreground = new SolidColorBrush(Look.Default.Muted);
        var hint = new TextBlock { Text = Environment.GetEnvironmentVariable("ANTIQUICKPAD_HOTKEY") ?? "Ctrl+Alt+Shift+A · Esc hide",
            FontSize = Look.Default.StatusSize, Foreground = new SolidColorBrush(Look.Default.Muted), Margin = new Thickness(8, 5) };
        DockPanel.SetDock(_count, Dock.Left); bottom.Children.Add(_count);
        DockPanel.SetDock(hint, Dock.Right); bottom.Children.Add(hint); bottom.Children.Add(_status);
        DockPanel.SetDock(bottom, Dock.Bottom); content.Children.Add(bottom); content.Children.Add(_carousel.View);
        _frame.Child = content; _frame.BorderThickness = new Thickness(Look.Default.BorderSize);
        _frame.BorderBrush = new SolidColorBrush(Look.Default.Muted); Content = _frame;
        _vm.Navigating += (_, e) => { _carousel.Cancel(); e.Cancel = !SaveNotes(); if (!e.Cancel) { RememberScroll(); _departingId = _vm.Current.Id; } };
        _vm.Navigated += (_, e) => { if (e.Kind == "delete" && _departingId is not null) _scroll.Remove(_departingId); BindCurrent(); if (e.Kind != "delete") SaveNotes(); FocusEditor(); };
        _vm.OperationFailed += (_, ex) => { UpdateStatus(); Log($"note-operation-error {ex}"); };
        _vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(MainViewModel.Pinned)) SetPinned(_vm.Pinned); UpdateStatus(); };
        _vm.Status.PropertyChanged += (_, _) => UpdateStatus();
        BindCurrent();
        _editor.TextChanged += (_, _) => ScheduleSave();
        _editor.TextArea.Caret.PositionChanged += (_, _) => ScheduleSave();
        _editor.TextArea.GotFocus += (_, _) => CompleteFocus();
        _autosave.Tick += (_, _) => SaveNotes();
        _cue.Tick += (_, _) => { _cue.Stop(); _frame.BorderBrush = new SolidColorBrush(Look.Default.Muted); };
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel, true);
        Opened += (_, _) => InitializeResident();
        SizeChanged += (_, _) => RememberNormalBounds();
        PositionChanged += (_, _) => RememberNormalBounds();
        Activated += (_, _) => Dispatcher.UIThread.Post(FocusEditor, DispatcherPriority.Input);
        Closing += (_, e) => { if (!_quitting) { e.Cancel = true; HideResident(); } };
        Deactivated += (_, _) => _carousel.Cancel();
        Closed += (_, _) => { _carousel.Cancel(); _autosave.Stop(); _cue.Stop(); _hotkey?.Dispose(); _tray?.Dispose(); if (_quitting) Log($"quit-finish window-closed=true source={_quitSource}"); _log.Dispose(); };
    }

    private void InitializeResident()
    {
        if (_initialized) return;
        _initialized = true;
        // The selftest does not claim the real hotkey or affect an existing resident app.
        if (!Program.SelfTest && !Program.QuitResidentTest)
        {
            InstallHotkey();
            SetupTray();
        }
        _showStarted = Stopwatch.GetTimestamp();
        if (Measurement.IsChild) Measurement.Attach(this, _hotkey?.IsRegistered == true, RenderScaling);
        Dispatcher.UIThread.Post(FocusEditor, DispatcherPriority.Input);
        if (Program.QuitResidentTest)
        {
            _editor.Text = "command quit saves this pending edit";
            Hide();
            File.WriteAllText(Path.Combine(Program.Paths.Root, "ready"), "ready");
        }
        if (Program.SelfTest) Dispatcher.UIThread.Post(async () => await RunSelfTest(), DispatcherPriority.Background);
    }

    private void InstallHotkey()
    {
        try
        {
            _hotkey = _services.CreateHotkey(this, timestamp => Toggle(timestamp), null);
            Log($"hotkey gesture={_hotkey.GestureLabel} registered={_hotkey.IsRegistered} error={_hotkey.RegistrationError} {_hotkey.RegistrationErrorMessage}");
        }
        catch (FormatException ex) { Log($"hotkey-configuration-error {ex.Message}"); }
    }

    public void Toggle(long started = 0)
    {
        Measurement.Event("hotkey", started);
        if (IsVisible) HideResident(); else ShowResident("toggle", started);
    }

    public void ShowResident(string source, long started = 0)
    {
        _showStarted = started == 0 ? Stopwatch.GetTimestamp() : started; _showSource = source;
        _platform.Show(); _platform.Activate();
        Measurement.Frame(this);
        _frame.BorderBrush = new SolidColorBrush(Look.Default.Accent); _cue.Stop(); _cue.Start();
        FocusEditor();
        Dispatcher.UIThread.Post(FocusEditor, DispatcherPriority.Input);
    }

    public void HideResident()
    {
        _carousel.Cancel();
        if (!SaveNotes()) return;
        _showStarted = 0; _platform.Hide(); Measurement.Event("hidden"); Log("hidden resident=true");
    }

    private void FocusEditor()
    {
        if (!IsVisible) return;
        _platform.Activate(); _editor.TextArea.Focus(); CompleteFocus();
    }

    private void CompleteFocus()
    {
        if (_showStarted == 0 || !IsVisible || !_editor.TextArea.IsKeyboardFocusWithin) return;
        if (Measurement.IsChild && (!IsActive || !Measurement.IsForeground(this))) return;
        Measurement.Event("focused");
        LastShowToFocusMs = Stopwatch.GetElapsedTime(_showStarted).TotalMilliseconds;
        _showStarted = 0;
        Log($"show-to-focus source={_showSource} ms={LastShowToFocusMs:F3} focused=true");
        _focusCompletion?.TrySetResult(LastShowToFocusMs);
    }

    private void ScheduleSave()
    {
        if (_binding) return;
        _vm.UpdateCurrent(_editor.Text, _editor.CaretOffset);
        UpdateStatus();
        _autosave.Stop(); _autosave.Start();
    }

    private bool SaveNotes()
    {
        _autosave.Stop();
        try
        {
            _vm.UpdateCurrent(_editor.Text, _editor.CaretOffset); _vm.Save(); SaveWindowSnapshot();
            _saveFailed = false; _lastSaveError = null; UpdateStatus(); return true;
        }
        catch (Exception ex)
        {
            _vm.Fail(ex); _saveFailed = true; _lastSaveError = ex.Message; UpdateStatus(); Log($"save-error {ex}"); return false;
        }
    }

    private void BindCurrent()
    {
        _binding = true;
        try { _editor.Text = _vm.Current.Text; _editor.CaretOffset = _vm.Current.CaretOffset; _editor.Document.UndoStack.ClearAll(); }
        finally { _binding = false; }
        var scroll = _scroll.GetValueOrDefault(_vm.Current.Id);
        EditorViewport.Restore(_editor, scroll);
        UpdateStatus();
    }

    private void RememberScroll() => _scroll[_vm.Current.Id] = _editor.TextArea.TextView.ScrollOffset;
    public void CreateNote() => _vm.CreateNote();
    public void DeleteCurrentNote() => _vm.DeleteCurrent();
    public void SelectNote(int index) => _vm.Select(index);
    private void ShowIndicator() => _count.Opacity = 1;
    private void UpdateStatus()
    {
        _count.Text = _vm.CountText;
        _saveFailed = _vm.Status.Failed;
        _status.Foreground = new SolidColorBrush(_saveFailed ? Look.Default.Accent : Look.Default.Muted);
        _status.Text = _vm.Status.Text;
    }

    private void RestoreWindowSnapshot()
    {
        try
        {
            var settings = _vm.LoadSettings();
            if (settings is null || !double.IsFinite(settings.Width) || !double.IsFinite(settings.Height)) return;
            _platform.Bounds = new WindowBounds(Math.Max(MinWidth, settings.Width), Math.Max(MinHeight, settings.Height), settings.X, settings.Y); WindowStartupLocation = WindowStartupLocation.Manual;
            _platform.Pinned = settings.Pinned; _vm.Pinned = settings.Pinned;
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
        var settings = (_normalSettings ?? new WindowSnapshot(Width, Height, Position.X, Position.Y, Topmost, _vm.ShellId, _vm.LookId)) with { Pinned = Topmost, ShellId = _vm.ShellId, LookId = _vm.LookId };
        _vm.SaveSettings(settings);
    }
    private void SetPinned(bool pinned)
    {
        _platform.Pinned = pinned; _pin.IsChecked = pinned;
        try { SaveWindowSnapshot(); }
        catch (Exception ex) { Log($"window-settings-save-error {ex}"); }
    }

    public void Quit(int exitCode = 0, string source = "tray")
    {
        _quitSource = source;
        Log($"quit-start source={source}");
        _carousel.Cancel();
        if (!SaveNotes())
        {
            Log($"quit-aborted save-failed reason={_lastSaveError}");
            if (source != "command") ShowResident("quit-save-failed");
            UpdateStatus();
            return;
        }
        _quitting = true; Close();
        (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.Shutdown(exitCode);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { HideResident(); e.Handled = true; }
        else if (e.Key == Key.N && e.KeyModifiers == KeyModifiers.Control) { CreateNote(); e.Handled = true; }
        else if (e.Key == Key.Delete && e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift)) { DeleteCurrentNote(); e.Handled = true; }
    }

    private void SetupTray()
    {
        _tray = _services.CreateTray();
        var hotkeyStatus = _hotkey is { IsRegistered: true } ? _hotkey.GestureLabel : "hotkey unavailable; see log";
        _tray.Initialize(() => Toggle(), () => Quit(), () => ShowResident("tray"), hotkeyStatus);
    }

    private void Log(string message) => _log.WriteLine($"{DateTimeOffset.UtcNow:O} {message}");

    public void MeasurementHeavy()
    {
        _vm.PopulateMeasurementFixture(); BindCurrent(); SaveNotes();
    }
}
