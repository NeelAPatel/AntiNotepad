using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;

namespace InputProbe;

public sealed class SpikeWindow : Window
{
    private const int HotkeyId = 0x5141;
    private const uint WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001, ModControl = 0x0002, ModShift = 0x0004, ModNoRepeat = 0x4000;
    private const double CardWidth = 760, CardGap = 24, WheelPageMs = 220;
    private readonly NoteStore _store;
    private List<Note> _notes;
    private readonly List<TextEditor> _editors = [];
    private readonly Canvas _canvas = new();
    private readonly Canvas _stripCanvas = new();
    private readonly Border _grabStrip = new();
    private readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap };
    private readonly ComboBox _material = new();
    private readonly DispatcherTimer _animation;
    private readonly WheelBurstState _wheelBurst = new();
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly string _logPath;
    private StreamWriter? _log;
    private TrayIcon? _tray;
    private FoldingManager? _folding;
    private long _gestureAt, _animationStartedAt, _showRequestedAt;
    private Point _dragStart;
    private Point _lastPointer;
    private double _dragOffset, _targetOffset, _lastVelocity, _dragOriginOffset, _animationStartX, _wheelTargetX;
    private int _initializationCount;
    private int _currentIndex;
    private IPointer? _captured;
    private bool _isDragging, _closing, _hotkeyRegistered, _initialized, _wheelAnimation;
    private IntPtr _hwnd;
    private string _hotkeyLabel = "Ctrl+Alt+Shift+A";

    public SpikeWindow()
    {
        Title = "AntiQuickPad spike";
        Width = 900; Height = 700; MinWidth = 760; MinHeight = 560;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowDecorations = Avalonia.Controls.WindowDecorations.None;
        Topmost = true;
        Background = new SolidColorBrush(Color.Parse("#CC111820"));
        TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Mica, WindowTransparencyLevel.Transparent];

        var data = Path.Combine(RuntimePaths.Root, "data");
        var storePath = Program.IsPersistenceMode ? Program.PersistencePath!
            : Program.Args.Contains("--ui-smoke", StringComparer.OrdinalIgnoreCase)
                ? Path.Combine(Path.GetTempPath(), $"aqp-ui-smoke-{Guid.NewGuid():N}.json")
                : Path.GetFullPath(Path.Combine(data, "notes.json"));
        _store = new NoteStore(storePath);
        _notes = _store.Load();
        _logPath = Path.Combine(RuntimePaths.Root, "logs", $"spike-{DateTime.Now:yyyyMMdd-HHmmss-fff}.log");
        Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
        _log = new StreamWriter(new FileStream(_logPath, FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };

        BuildEditors();
        var toolbar = BuildToolbar();
        _grabStrip.Child = new TextBlock { Text = "≡   DRAG NOTES HERE   ≡", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.White, FontWeight = FontWeight.Bold };
        _grabStrip.Height = 34; _grabStrip.Background = new SolidColorBrush(Color.Parse("#663E83A8"));
        _grabStrip.Cursor = new Cursor(StandardCursorType.SizeAll);
        _grabStrip.PointerPressed += StartDrag;
        _grabStrip.PointerMoved += MoveDrag;
        _grabStrip.PointerReleased += EndDrag;
        _grabStrip.PointerCaptureLost += (_, _) => CancelDrag("capture-lost");

        var root = new DockPanel { LastChildFill = true, Margin = new Thickness(12) };
        DockPanel.SetDock(toolbar, Dock.Top); root.Children.Add(toolbar);
        DockPanel.SetDock(_grabStrip, Dock.Top); root.Children.Add(_grabStrip);
        _canvas.ClipToBounds = true; _canvas.Children.Add(_stripCanvas);
        var footer = new Border { Child = _status, Margin = new Thickness(2, 6) };
        DockPanel.SetDock(footer, Dock.Bottom); root.Children.Add(footer);
        root.Children.Add(_canvas);
        Content = root;

        _animation = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _animation.Tick += (_, _) => AnimateSnap();
        AddHandler(InputElement.PointerWheelChangedEvent, Wheel, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        AddHandler(InputElement.KeyDownEvent, KeyPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
        Opened += (_, _) =>
        {
            if (_initialized) { Dispatcher.UIThread.Post(FocusCurrent, DispatcherPriority.Input); return; }
            _initialized = true;
            _initializationCount++;
            Log($"opened os={RuntimeInformation.OSDescription} framework={RuntimeInformation.FrameworkDescription} dpi={RenderScaling * 96:F0} restoredNotes={_notes.Count} processToOpened={Program.ElapsedMs:F1}ms");
            InstallNativeHooks();
            Dispatcher.UIThread.Post(FocusCurrent, DispatcherPriority.Input);
            Dispatcher.UIThread.Post(FocusCurrent, DispatcherPriority.Background);
            if (Program.PersistenceWrite) Dispatcher.UIThread.Post(RunPersistenceWrite, DispatcherPriority.Background);
            else if (Program.PersistenceRead) Dispatcher.UIThread.Post(RunPersistenceRead, DispatcherPriority.Background);
            else if (Program.Args.Contains("--ui-smoke", StringComparer.OrdinalIgnoreCase)) Dispatcher.UIThread.Post(RunUiSmoke, DispatcherPriority.Background);
        };
        Activated += (_, _) => Dispatcher.UIThread.Post(FocusCurrent, DispatcherPriority.Input);
        Deactivated += (_, _) => { CancelDrag("deactivated"); SaveNotes(); };
        Closed += (_, _) => Cleanup();
        UpdateStrip(0);
    }

    private void BuildEditors()
    {
        _stripCanvas.Children.Clear(); _editors.Clear();
        for (var i = 0; i < _notes.Count; i++)
        {
            var editor = new TextEditor
            {
                Width = CardWidth, Height = 560, FontFamily = new FontFamily("Cascadia Code, Consolas"), FontSize = 17,
                ShowLineNumbers = true, WordWrap = true, Background = new SolidColorBrush(Color.Parse("#E8172029")),
                Foreground = new SolidColorBrush(Color.Parse("#FFF5F7FA")), Padding = new Thickness(18),
                Text = _notes[i].Text
            };
            editor.TextArea.TextView.LineTransformers.Add(new MarkdownColorizer());
            var id = i;
            editor.TextChanged += (_, _) => { _notes[id].Text = editor.Text; SaveNotes(); Log($"text-changed note={id} chars={editor.Text.Length} focused={editor.IsKeyboardFocusWithin}"); };
            editor.AddHandler(InputElement.PointerPressedEvent, (_, e) => TryToggleCheckbox(editor, e), Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
            editor.AddHandler(InputElement.PointerMovedEvent, (_, e) => UpdateCheckboxHover(editor, e.GetPosition(editor.TextArea.TextView)), Avalonia.Interactivity.RoutingStrategies.Tunnel, true);
            editor.PointerExited += (_, _) => SetEditorCursor(editor, false);
            editor.TextArea.TextView.PointerExited += (_, _) => SetEditorCursor(editor, false);
            editor.GotFocus += (_, _) => Log($"editor-focus note={id} actual={editor.IsKeyboardFocusWithin}");
            _editors.Add(editor);
            var card = new Border { Child = editor, CornerRadius = new CornerRadius(12), BorderBrush = new SolidColorBrush(Color.Parse("#665F8299")), BorderThickness = new Thickness(1), ClipToBounds = true };
            Canvas.SetLeft(card, i * (CardWidth + CardGap)); Canvas.SetTop(card, 0); _stripCanvas.Children.Add(card);
        }
        SetCanvasPosition(0);
    }

    private Control BuildToolbar()
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        row.Children.Add(new TextBlock { Text = "AntiQuickPad", FontSize = 18, FontWeight = FontWeight.Bold, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
        AddButton(row, "＋ Note", CreateNote);
        AddButton(row, "←", () => GoTo(_currentIndex - 1)); AddButton(row, "→", () => GoTo(_currentIndex + 1));
        AddButton(row, "Fold code", ToggleCodeFold);
        _material.ItemsSource = new[] { "AcrylicBlur", "Mica", "Transparent" }; _material.SelectedIndex = 0; _material.Width = 130;
        _material.SelectionChanged += (_, _) => ApplyMaterial(); row.Children.Add(_material);
        AddButton(row, "Hide", HideResident); AddButton(row, "Exit", ExitApp);
        return row;
    }

    private static void AddButton(Panel panel, string text, Action action)
    { var b = new Button { Content = text, Padding = new Thickness(9, 5) }; b.Click += (_, _) => action(); panel.Children.Add(b); }

    private void ApplyMaterial()
    {
        var name = _material.SelectedItem?.ToString() ?? "AcrylicBlur";
        var level = name switch { "Mica" => WindowTransparencyLevel.Mica, "Transparent" => WindowTransparencyLevel.Transparent, _ => WindowTransparencyLevel.AcrylicBlur };
        TransparencyLevelHint = [level, WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Mica, WindowTransparencyLevel.Transparent];
        Background = name == "Transparent" ? Brushes.Transparent : new SolidColorBrush(Color.Parse("#D9141D26"));
        Dispatcher.UIThread.Post(() => { UpdateFocusStatus(); Log($"material requested={name} actual={ActualTransparencyLevel}"); });
    }

    private void Wheel(object? sender, PointerWheelEventArgs e)
    {
        if (Math.Abs(e.Delta.X) < .001) return;
        if (_isDragging) { e.Handled = true; return; }
        var nowMs = (long)_clock.Elapsed.TotalMilliseconds;
        var animationInFlight = _wheelAnimation && Stopwatch.GetElapsedTime(_animationStartedAt).TotalMilliseconds < WheelPageMs;
        if (_wheelBurst.IsExpired(nowMs, animationInFlight)) FinishWheelBurst();
        var result = _wheelBurst.Add(_currentIndex, _notes.Count, e.Delta.X, nowMs, animationInFlight);
        if (!_wheelAnimation)
        {
            _animation.Stop();
            _animationStartX = (_stripCanvas.RenderTransform as TranslateTransform)?.X ?? -_currentIndex * (CardWidth + CardGap);
            _animationStartedAt = Stopwatch.GetTimestamp();
            _wheelAnimation = true;
        }
        var requestedX = result.CreateAtNewestEdge
            ? -_currentIndex * (CardWidth + CardGap) - 100
            : -result.TargetIndex * (CardWidth + CardGap);
        if (_wheelAnimation && requestedX != _wheelTargetX)
        {
            _animationStartX = (_stripCanvas.RenderTransform as TranslateTransform)?.X ?? _animationStartX;
            _animationStartedAt = Stopwatch.GetTimestamp();
        }
        _wheelTargetX = requestedX;
        _animation.Start();
        Log($"wheel-page burst={result.BurstNumber} dx={e.Delta.X:F3} target={result.TargetIndex} edge={result.CreateAtNewestEdge} animated=220ms");
        e.Handled = true;
    }

    private void StartDrag(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(_grabStrip).Properties.IsLeftButtonPressed) return;
        if (_wheelBurst.IsActive) FinishWheelBurst();
        _dragOriginOffset = ((_stripCanvas.RenderTransform as TranslateTransform)?.X ?? 0) + _currentIndex * (CardWidth + CardGap);
        _animation.Stop(); _wheelAnimation = false; _captured = e.Pointer; _captured.Capture(_grabStrip); _isDragging = true;
        _dragStart = _lastPointer = e.GetPosition(_grabStrip); _dragOffset = _dragOriginOffset; _lastVelocity = 0; _gestureAt = Stopwatch.GetTimestamp();
        e.Handled = true; Log("drag-start capture=true");
    }

    private void MoveDrag(object? sender, PointerEventArgs e)
    {
        if (!_isDragging) return;
        var p = e.GetPosition(_grabStrip); var now = Stopwatch.GetTimestamp();
        var delta = CarouselMath.DragDelta(_lastPointer.X, p.X); _dragOffset = _dragOriginOffset + CarouselMath.DragDelta(_dragStart.X, p.X);
        var ms = Math.Max(.1, Stopwatch.GetElapsedTime(_gestureAt, now).TotalMilliseconds);
        _lastVelocity = delta / ms; _lastPointer = p; _gestureAt = now;
        UpdateStrip(_dragOffset); e.Handled = true;
    }

    private void EndDrag(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging) return;
        if (Stopwatch.GetElapsedTime(_gestureAt).TotalMilliseconds > 80) _lastVelocity = 0;
        _isDragging = false; _captured?.Capture(null); _captured = null;
        Log($"drag-release distance={_dragOffset:F1} velocity={_lastVelocity:F3}px/ms"); FinishGesture(); e.Handled = true;
    }

    private void CancelDrag(string reason)
    {
        if (!_isDragging) return;
        _isDragging = false; var pointer = _captured; _captured = null; pointer?.Capture(null); Log($"drag-cancel {reason}");
        _dragOffset = 0; SnapTo(_currentIndex);
    }

    private void FinishGesture()
    {
        var displacement = _dragOffset;
        var edge = CarouselMath.ShouldCreateNote(_currentIndex, _notes.Count, displacement);
        if (edge) { CreateNote(); Log($"rubber-band create displacement={displacement:F1}"); }
        else
        {
            SnapTo(CarouselMath.SnapIndex(_currentIndex, displacement, _lastVelocity, _notes.Count));
        }
        _dragOffset = 0;
    }

    private void UpdateStrip(double offset)
    {
        var min = -_currentIndex * (CardWidth + CardGap);
        var max = -Math.Max(0, _notes.Count - 1) * (CardWidth + CardGap);
        var global = min + offset;
        if (global > 0) global = CarouselMath.RubberBand(global);
        if (global < max) global = max + CarouselMath.RubberBand(global - max);
        SetCanvasPosition(global);
    }
    private void SetCanvasPosition(double x) => _stripCanvas.RenderTransform = new TranslateTransform(x, 0);

    private void GoTo(int index)
    {
        _wheelBurst.Reset(); _wheelAnimation = false;
        var clamped = Math.Clamp(index, 0, _notes.Count - 1);
        _wheelAnimation = false; _currentIndex = clamped; _targetOffset = -_currentIndex * (CardWidth + CardGap); _animationStartedAt = Stopwatch.GetTimestamp(); _animationStartX = (_stripCanvas.RenderTransform as TranslateTransform)?.X ?? 0; _animation.Start(); UpdateFocusStatus();
    }
    private void SnapTo(int index) { _wheelBurst.Reset(); _dragOffset = 0; _wheelAnimation = false; _currentIndex = Math.Clamp(index, 0, _notes.Count - 1); _targetOffset = -_currentIndex * (CardWidth + CardGap); _animationStartX = (_stripCanvas.RenderTransform as TranslateTransform)?.X ?? 0; _animationStartedAt = Stopwatch.GetTimestamp(); _animation.Start(); }
    private void AnimateSnap()
    {
        var transform = _stripCanvas.RenderTransform as TranslateTransform;
        var x = transform?.X ?? 0;
        if (_wheelAnimation)
        {
            var elapsed = Stopwatch.GetElapsedTime(_animationStartedAt).TotalMilliseconds;
            var progress = Math.Clamp(elapsed / WheelPageMs, 0, 1);
            var eased = 1 - Math.Pow(1 - progress, 3);
            SetCanvasPosition(_animationStartX + (_wheelTargetX - _animationStartX) * eased);
            if (progress >= 1 && _wheelBurst.IsExpired((long)_clock.Elapsed.TotalMilliseconds)) FinishWheelBurst();
            return;
        }
        var progressToTarget = Math.Clamp(Stopwatch.GetElapsedTime(_animationStartedAt).TotalMilliseconds / WheelPageMs, 0, 1);
        var next = _animationStartX + (_targetOffset - _animationStartX) * (1 - Math.Pow(1 - progressToTarget, 3));
        if (progressToTarget >= 1) { next = _targetOffset; _animation.Stop(); Dispatcher.UIThread.Post(FocusCurrent, DispatcherPriority.Input); }
        SetCanvasPosition(next); UpdateFocusStatus();
    }

    private void FinishWheelBurst()
    {
        if (!_wheelBurst.IsActive) return;
        var result = _wheelBurst.Complete();
        _wheelAnimation = false;
        if (result.CreateAtNewestEdge) { Log($"wheel-edge create burst={result.BurstNumber}"); CreateNote(); return; }
        _currentIndex = result.TargetIndex;
        _targetOffset = -_currentIndex * (CardWidth + CardGap);
        SetCanvasPosition(_targetOffset);
        _animation.Stop();
        UpdateFocusStatus();
        Dispatcher.UIThread.Post(FocusCurrent, DispatcherPriority.Input);
        Log($"wheel-page-complete burst={result.BurstNumber} index={_currentIndex}");
    }
    private void CreateNote()
    {
        _animation.Stop(); _wheelAnimation = false; _wheelBurst.Reset();
        if (_folding is not null) { FoldingManager.Uninstall(_folding); _folding = null; }
        for (var i = 0; i < _editors.Count; i++) _notes[i].Text = _editors[i].Text;
        _notes = CarouselMath.PruneInteriorBlanks(_notes.Append(new Note()).ToList());
        _currentIndex = _notes.Count - 1; BuildEditors();
        _targetOffset = -_currentIndex * (CardWidth + CardGap); SetCanvasPosition(_targetOffset); SaveNotes(); FocusCurrent(); Log("note-created");
    }

    private void TryToggleCheckbox(TextEditor editor, PointerPressedEventArgs e)
    {
        if (e.Handled || !e.GetCurrentPoint(editor).Properties.IsLeftButtonPressed) return;
        if (!TryGetCheckboxAtPoint(editor, e.GetPosition(editor.TextArea.TextView), out var markerOffset, out var line, out var checkedMarker)) return;
        var caret = editor.TextArea.Caret.Offset;
        var replacement = EditorOperations.ToggleCheckbox(editor.Document, markerOffset, ref caret);
        if (replacement is null) return;
        editor.TextArea.Caret.Offset = caret;
        editor.TextArea.TextView.Redraw();
        Log($"checkbox-toggle line={line.LineNumber} checked={!checkedMarker} undo={editor.Document.UndoStack.CanUndo} caret={editor.TextArea.Caret.Offset}");
        e.Handled = true;
    }

    private static bool TryGetCheckboxAtPoint(TextEditor editor, Point visualPoint, out int markerOffset, out DocumentLine line, out bool checkedMarker)
    {
        markerOffset = 0; line = null!; checkedMarker = false;
        var position = editor.TextArea.TextView.GetPosition(visualPoint + editor.TextArea.TextView.ScrollOffset);
        if (position is null) return false;
        var offset = editor.Document.GetOffset(position.Value.Location);
        line = editor.Document.GetLineByOffset(offset);
        var text = editor.Document.GetText(line);
        var marker = text.IndexOf("[ ]", StringComparison.Ordinal);
        if (marker < 0) { marker = text.IndexOf("[x]", StringComparison.OrdinalIgnoreCase); checkedMarker = true; }
        markerOffset = line.Offset + Math.Max(0, marker);
        return marker >= 0 && offset >= markerOffset && offset <= markerOffset + 2;
    }

    private static void UpdateCheckboxHover(TextEditor editor, Point visualPoint)
    {
        var hit = TryGetCheckboxAtPoint(editor, visualPoint, out _, out _, out _);
        SetEditorCursor(editor, hit);
    }

    private static void SetEditorCursor(TextEditor editor, bool checkbox)
    {
        var cursor = new Cursor(checkbox ? StandardCursorType.Hand : StandardCursorType.Ibeam);
        editor.Cursor = cursor; editor.TextArea.Cursor = cursor; editor.TextArea.TextView.Cursor = cursor;
        ToolTip.SetTip(editor.TextArea.TextView, checkbox ? "Toggle task checkbox" : null);
    }

    private void HandleEditorKey(TextEditor editor, KeyEventArgs e)
    {
        if (e.Key != Key.Tab) return;
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var segment = editor.TextArea.Selection.SurroundingSegment;
        var result = EditorOperations.ApplyTab(editor.Document, editor.TextArea.Caret.Offset, segment?.Offset, segment?.EndOffset, shift);
        if (result.SelectionStart is int start && result.SelectionEnd is int end && end > start)
            editor.TextArea.Selection = AvaloniaEdit.Editing.Selection.Create(editor.TextArea, start, end);
        editor.TextArea.Caret.Offset = result.CaretOffset;
        Log($"selection-indent shift={shift} selection={editor.TextArea.Selection.Length} changed={result.Changed} undo={editor.Document.UndoStack.CanUndo}"); e.Handled = true;
    }

    private void ToggleCodeFold()
    {
        var editor = _editors[_currentIndex];
        try
        {
            if (_folding is not null) { FoldingManager.Uninstall(_folding); _folding = null; UpdateFocusStatus(); Log("folding disabled"); return; }
            _folding = FoldingManager.Install(editor.TextArea);
            var lines = editor.Document.Lines.ToArray(); var ranges = new List<NewFolding>(); var start = -1;
            foreach (var line in lines)
            {
                var t = editor.Document.GetText(line);
                if (t.TrimStart().StartsWith("```")) { if (start < 0) start = line.Offset; else { ranges.Add(new NewFolding(start, line.EndOffset)); start = -1; } }
            }
            _folding.UpdateFoldings(ranges, -1); UpdateFocusStatus(); Log($"folding available=true ranges={ranges.Count}");
        }
        catch (Exception ex) { UpdateFocusStatus(); Log($"folding error={ex.GetType().Name}:{ex.Message}"); }
    }

    private void UpdateFocusStatus()
    {
        if (_editors.Count == 0) return;
        _status.Text = $"Note {_currentIndex + 1}/{_notes.Count} · focus={_editors[_currentIndex].IsKeyboardFocusWithin} · {_material.SelectedItem} -> {ActualTransparencyLevel} · {(_hotkeyRegistered ? $"Reopen: {_hotkeyLabel} / tray" : "Reopen: tray (hotkey unavailable)")}";
    }
    private void FocusCurrent()
    {
        if (!IsVisible || _editors.Count == 0) return;
        Activate(); _editors[_currentIndex].TextArea.Focus();
        var showRequestedAt = _showRequestedAt;
        _showRequestedAt = 0;
        Dispatcher.UIThread.Post(() => { var focused = _editors[_currentIndex].TextArea.IsKeyboardFocusWithin; UpdateFocusStatus(); var showDelta = showRequestedAt == 0 ? "n/a" : $"{Stopwatch.GetElapsedTime(showRequestedAt).TotalMilliseconds:F1}ms"; Log($"focus-check actual={focused} editorWithin={_editors[_currentIndex].IsKeyboardFocusWithin} mainToFocus={Program.ElapsedMs:F1}ms showToFocus={showDelta}"); }, DispatcherPriority.Background);
    }
    private void HideResident() { SaveNotes(); var started = Stopwatch.GetTimestamp(); Hide(); Log($"window-hidden resident=true hideDispatch={Stopwatch.GetElapsedTime(started).TotalMilliseconds:F2}ms"); SampleIdleMemory(); }
    private void ShowToggle()
    {
        if (IsVisible && IsActive) HideResident();
        else ShowResident("hotkey");
    }
    private void ShowResident(string source)
    {
        var started = Stopwatch.GetTimestamp();
        _showRequestedAt = started;
        WindowState = WindowState.Normal; Show(); Activate();
        Dispatcher.UIThread.Post(FocusCurrent, DispatcherPriority.Input);
        Log($"resident-show source={source} dispatchMs={Stopwatch.GetElapsedTime(started).TotalMilliseconds:F3}");
    }
    private void SampleIdleMemory()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => { timer.Stop(); if (IsVisible) return; using var process = Process.GetCurrentProcess(); process.Refresh(); Log($"idle-memory privateBytes={process.PrivateMemorySize64} workingSetBytes={process.WorkingSet64} sampleAfterHide=1000ms"); };
        timer.Start();
    }
    private void ExitApp() { SaveNotes(); _closing = true; Close(); }
    private void SaveNotes()
    {
        if (_closing) return;
        try { _store.Save(_notes); Log($"autosave notes={_notes.Count} path={_store.Path}"); }
        catch (Exception ex) { Log($"autosave-error {ex.GetType().Name}:{ex.Message}"); }
    }
    private void InstallNativeHooks()
    {
        if (!OperatingSystem.IsWindows()) { _status.Text = "Windows hotkey unavailable"; return; }
        Win32Properties.AddWndProcHookCallback(this, WndProc);
        _hwnd = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (_hwnd != IntPtr.Zero)
        {
            _hotkeyRegistered = RegisterHotKey(_hwnd, HotkeyId, ModControl | ModAlt | ModShift | ModNoRepeat, 0x41);
            Log($"hotkey-register result={_hotkeyRegistered} error={Marshal.GetLastWin32Error()}");
        }
        SetupTray();
    }
    private void SetupTray()
    {
        var menu = new NativeMenu();
        var show = new NativeMenuItem(_hotkeyRegistered ? $"Show / focus · {_hotkeyLabel}" : "Show / focus · hotkey unavailable"); show.Click += (_, _) => ShowResident("tray-menu");
        var exit = new NativeMenuItem("Exit"); exit.Click += (_, _) => ExitApp();
        menu.Items.Add(show); menu.Items.Add(exit);
        _tray = new TrayIcon { ToolTipText = _hotkeyRegistered ? $"AntiQuickPad · {_hotkeyLabel} · click to show" : "AntiQuickPad · hotkey unavailable · click to show", IsVisible = true, Menu = menu, Icon = CreateTrayIcon() };
        _tray.Clicked += (_, _) => ShowResident("tray-click");
        TrayIcon.SetIcons(Application.Current!, new TrayIcons { _tray });
        _status.Text = _hotkeyRegistered ? $"Reopen: {_hotkeyLabel} / tray" : "Reopen: tray (hotkey unavailable)";
    }
    private IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId) { handled = true; ShowToggle(); return IntPtr.Zero; }
        return IntPtr.Zero;
    }
    private void Cleanup()
    {
        _animation.Stop();
        SaveNotes();
        if (OperatingSystem.IsWindows())
        {
            Win32Properties.RemoveWndProcHookCallback(this, WndProc);
            if (_hotkeyRegistered) UnregisterHotKey(_hwnd, HotkeyId);
        }
        if (_tray is not null) { _tray.IsVisible = false; _tray.Dispose(); }
        if (Program.Args.Contains("--ui-smoke", StringComparer.OrdinalIgnoreCase) && File.Exists(_store.Path)) File.Delete(_store.Path);
        _log?.Dispose(); _log = null;
        if (Program.Args.Contains("--ui-smoke", StringComparer.OrdinalIgnoreCase) && File.Exists(_store.Path)) File.Delete(_store.Path);
    }

    private WindowIcon CreateTrayIcon()
    {
        // Tiny generated 16x16 opaque blue-green icon; no extra asset/package required.
        using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream);
        writer.Write((ushort)0); writer.Write((ushort)1); writer.Write((ushort)1);
        writer.Write((byte)16); writer.Write((byte)16); writer.Write((byte)0); writer.Write((byte)0); writer.Write((ushort)1); writer.Write((ushort)32); writer.Write(40 + 16 * 16 * 4 + 64); writer.Write(22);
        writer.Write(40); writer.Write(16); writer.Write(32); writer.Write((ushort)1); writer.Write((ushort)32); writer.Write(0); writer.Write(16 * 16 * 4); writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(0);
        for (var i = 0; i < 256; i++) { writer.Write((byte)0xB7); writer.Write((byte)0x8B); writer.Write((byte)0x38); writer.Write((byte)0xFF); }
        for (var i = 0; i < 64; i++) writer.Write((byte)0);
        stream.Position = 0; return new WindowIcon(stream);
    }

    private void RunUiSmoke()
    {
        if (_editors.Count == 0) { Environment.ExitCode = 1; Log("ui-smoke FAIL no editor"); Close(); return; }
        FocusCurrent();
        var editor = _editors[_currentIndex];
        Dispatcher.UIThread.Post(() =>
        {
            var focused = editor.TextArea.IsKeyboardFocusWithin;
            var before = editor.Text;
            const string marker = "\nui-smoke-programmatic-edit";
            editor.Document.Insert(editor.Document.TextLength, marker);
            var inserted = editor.Text.EndsWith(marker, StringComparison.Ordinal);
            if (inserted) editor.Document.UndoStack.Undo();
            var restored = editor.Text == before;
            Log($"ui-smoke editableTemplate={editor.TextArea.IsVisible} textViewMeasureValid={editor.TextArea.TextView.IsMeasureValid} focused={focused} programmaticInsert={inserted} undoRestored={restored} equality={editor.Text == before}");
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1900) };
            t.Tick += (_, _) =>
            {
                t.Stop();
                using var process = Process.GetCurrentProcess();
                process.Refresh();
                Environment.ExitCode = focused && inserted && restored && editor.TextArea.TextView.IsMeasureValid ? 0 : 1;
                Log($"ui-smoke settled material={_material.SelectedItem} actual={ActualTransparencyLevel} bounds={Bounds.Width:F0}x{Bounds.Height:F0} textViewBounds={editor.TextArea.TextView.Bounds} measureValid={editor.TextArea.TextView.IsMeasureValid} focus={editor.TextArea.IsKeyboardFocusWithin} privateBytes={process.PrivateMemorySize64} workingSetBytes={process.WorkingSet64} processToSettled={Program.ElapsedMs:F1}ms; physical typing not exercised");
                try
                {
                    using var preview = new Avalonia.Media.Imaging.RenderTargetBitmap(new PixelSize((int)Bounds.Width, (int)Bounds.Height), new Vector(96, 96));
                    preview.Render(this);
                    var path = Path.ChangeExtension(_logPath, ".png");
                    preview.Save(path, Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default);
                    Log($"render-preview={path}; framework render only, excludes Windows compositor backdrop");
                }
                catch (Exception ex) { Log($"render-preview-error {ex.Message}"); }
                var stage = 0;
                var materials = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
                materials.Tick += (_, _) =>
                {
                    if (stage == 0) { _material.SelectedIndex = 1; stage++; }
                    else if (stage == 1) { Log($"ui-smoke material requested=Mica actual={ActualTransparencyLevel}"); _material.SelectedIndex = 2; stage++; }
                    else if (stage == 2) { Log($"ui-smoke material requested=Transparent actual={ActualTransparencyLevel}"); HideResident(); materials.Interval = TimeSpan.FromMilliseconds(1400); stage++; }
                    else if (stage == 3) { ShowResident("ui-smoke-tray-handler"); materials.Interval = TimeSpan.FromMilliseconds(180); stage++; }
                    else
                    {
                        var sameEditor = ReferenceEquals(editor, _editors[_currentIndex]);
                        var trayShowPass = IsVisible && editor.TextArea.IsKeyboardFocusWithin && sameEditor && _initializationCount == 1;
                        Log($"ui-smoke resident-show visible={IsVisible} focused={editor.TextArea.IsKeyboardFocusWithin} sameEditor={sameEditor} initializationCount={_initializationCount} trayShowHandlerPass={trayShowPass}; physical tray and hotkey delivery not exercised");
                        if (!trayShowPass) Environment.ExitCode = 1;
                        materials.Stop(); SaveNotes(); Close();
                    }
                };
                materials.Start();
            };
            t.Start();
        }, DispatcherPriority.Background);
    }

    private void RunPersistenceWrite()
    {
        var marker = $"AQP-PERSISTENCE-{Guid.NewGuid():N}";
        try
        {
            File.WriteAllText(Program.PersistenceMarkerPath, marker);
            var editor = _editors[_currentIndex];
            editor.Document.Insert(editor.Document.TextLength, $"\n{marker}");
            SaveNotes();
            var wrote = _store.Load().Any(note => note.Text.Contains(marker, StringComparison.Ordinal));
            Environment.ExitCode = wrote ? 0 : 1;
            Log($"persistence-write {(wrote ? "PASS" : "FAIL")} markerChars={marker.Length} notes={_notes.Count} path={_store.Path}; actual document edit and autosave");
        }
        catch (Exception ex) { Environment.ExitCode = 1; Log($"persistence-write FAIL {ex.GetType().Name}:{ex.Message}"); }
        Close();
    }

    private void RunPersistenceRead()
    {
        try
        {
            var marker = File.ReadAllText(Program.PersistenceMarkerPath);
            var restored = _notes.Any(note => note.Text.Contains(marker, StringComparison.Ordinal));
            Environment.ExitCode = restored && _notes.Count > 0 ? 0 : 1;
            Log($"persistence-read {(Environment.ExitCode == 0 ? "PASS" : "FAIL")} markerRestored={restored} notes={_notes.Count} path={_store.Path}; loaded by next process through SpikeWindow");
        }
        catch (Exception ex) { Environment.ExitCode = 1; Log($"persistence-read FAIL {ex.GetType().Name}:{ex.Message}"); }
        Close();
    }
    private void Log(string message) => _log?.WriteLine($"{DateTimeOffset.UtcNow:O} +{_clock.Elapsed.TotalMilliseconds:F1}ms visible={IsVisible} focused={(_editors.Count > 0 && _editors[_currentIndex].IsKeyboardFocusWithin)} {message}");
    private void KeyPressed(object? sender, KeyEventArgs e)
    {
        if (e.Handled) return;
        if (e.Key == Key.Tab)
        {
            var editor = _editors.FirstOrDefault(x => x.IsKeyboardFocusWithin);
            if (editor is null) return;
            HandleEditorKey(editor, e);
        }
        else if (e.Key == Key.Escape) { HideResident(); e.Handled = true; }
        else if (e.Key == Key.N && e.KeyModifiers.HasFlag(KeyModifiers.Control)) { CreateNote(); e.Handled = true; }
    }
    [DllImport("user32.dll", SetLastError = true)] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, int key);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
