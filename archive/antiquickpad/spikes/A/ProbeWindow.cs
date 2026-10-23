using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;

namespace InputProbe;

public sealed class ProbeWindow : Window
{
    private const uint WmPointerUpdate = 0x0245;
    private const uint WmPointerDown = 0x0246;
    private const uint WmPointerUp = 0x0247;
    private const uint WmPointerEnter = 0x0249;
    private const uint WmPointerLeave = 0x024A;
    private const uint WmPointerCaptureChanged = 0x024C;
    private const uint WmMouseWheel = 0x020A;
    private const uint WmMouseHWheel = 0x020E;
    private const uint WmGesture = 0x0119;
    private readonly TextEditor _editor;
    private readonly TextBlock _latest;
    private readonly ObservableCollection<string> _recent = new();
    private readonly StreamWriter _writer;
    private readonly string _logPath;
    private readonly long _startedTimestamp = Stopwatch.GetTimestamp();

    public ProbeWindow()
    {
        Title = "AntiQuickPad — Avalonia input probe";
        Width = 1000;
        Height = 760;
        MinWidth = 720;
        MinHeight = 560;

        var logDirectory = Path.Combine(RuntimePaths.Root, "logs");
        Directory.CreateDirectory(logDirectory);
        _logPath = Path.Combine(logDirectory, $"input-probe-{DateTime.Now:yyyyMMdd-HHmmss}.log");
        _writer = new StreamWriter(new FileStream(_logPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
        {
            AutoFlush = true
        };

        _latest = new TextBlock { TextWrapping = TextWrapping.Wrap, FontWeight = FontWeight.SemiBold };
        var logList = new ListBox { ItemsSource = _recent, Height = 145, FontFamily = new FontFamily("Consolas") };
        _editor = new TextEditor
        {
            ShowLineNumbers = true,
            FontFamily = new FontFamily("Consolas"),
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = 300,
            Text = "Avalonia input probe\n\nKeep this editor focused while testing the touchpad.\nYou can type here to confirm focus.\n"
        };

        var instructions = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = "TEST (keep this editor focused):\n" +
                   "1. Move two fingers horizontally on the precision touchpad; watch movement events.\n" +
                   "2. While fingers stay down, pause for about two seconds; check whether events stop or a phase changes.\n" +
                   "3. Resume, then lift both fingers; watch for a distinct release phase and any subsequent inertia.\n" +
                   "4. Start another pan, then reverse direction or lift one finger early; check for cancellation/phase changes.\n" +
                   "5. Compare with vertical and horizontal mouse-wheel input. No idle timeout is treated as finger release."
        };
        var clear = new Button { Content = "Clear visible summary" };
        clear.Click += (_, _) => _recent.Clear();
        var logPath = new TextBlock { Text = $"Log: {_logPath}", TextWrapping = TextWrapping.Wrap, FontSize = 11 };

        var panel = new StackPanel { Margin = new Thickness(14), Spacing = 8 };
        panel.Children.Add(new TextBlock { Text = "Precision-touchpad phase probe", FontSize = 21, FontWeight = FontWeight.Bold });
        panel.Children.Add(instructions);
        panel.Children.Add(new TextBlock
        {
            Text = "Human phase markers: while the editor is focused, press F1 MOVE, F2 PAUSE (still touching), " +
                   "F3 LIFT, F4 INERTIA, F5 CANCEL, F6 END. Each marker records editor focus and render scaling."
        });
        panel.Children.Add(_latest);
        panel.Children.Add(_editor);
        panel.Children.Add(new TextBlock { Text = "Recent events (full stream is in the file):", FontWeight = FontWeight.SemiBold });
        panel.Children.Add(logList);
        panel.Children.Add(clear);
        panel.Children.Add(logPath);
        Content = new ScrollViewer { Content = panel };

        AddHandler(InputElement.PointerPressedEvent, (_, e) => LogPointer("pointer-pressed", e), RoutingStrategies.Tunnel, true);
        AddHandler(InputElement.PointerMovedEvent, (_, e) => LogPointer("pointer-moved", e), RoutingStrategies.Tunnel, true);
        AddHandler(InputElement.PointerReleasedEvent, (_, e) => LogPointer("pointer-released", e), RoutingStrategies.Tunnel, true);
        _editor.PointerCaptureLost += (_, e) => Log("pointer-capture-lost", $"type={e.Pointer.Type}");
        AddHandler(InputElement.PointerWheelChangedEvent, (_, e) => Log("wheel", $"delta=({F(e.Delta.X)},{F(e.Delta.Y)}) pointer={e.Pointer.Type}"), RoutingStrategies.Tunnel, true);
        AddHandler(InputElement.ScrollGestureEvent, (_, e) => Log("scroll-gesture", $"delta=({F(e.Delta.X)},{F(e.Delta.Y)})"), RoutingStrategies.Tunnel, true);
        AddHandler(InputElement.ScrollGestureInertiaStartingEvent, (_, e) => Log("scroll-inertia-starting", $"id={e.Id} inertia=({F(e.Inertia.X)},{F(e.Inertia.Y)})"), RoutingStrategies.Tunnel, true);
        AddHandler(InputElement.ScrollGestureEndedEvent, (_, _) => Log("scroll-gesture-ended", "Avalonia gesture ended (may follow inertia/capture loss)"), RoutingStrategies.Tunnel, true);
        AddHandler(InputElement.KeyDownEvent, (_, e) =>
        {
            if (!_editor.IsKeyboardFocusWithin)
                return;
            var marker = e.Key switch
            {
                Key.F1 => "MOVE",
                Key.F2 => "PAUSE_STILL_TOUCHING",
                Key.F3 => "LIFT",
                Key.F4 => "INERTIA",
                Key.F5 => "CANCEL",
                Key.F6 => "END",
                _ => null
            };
            if (marker is not null)
            {
                Log("human-phase", marker);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel, true);

        Opened += (_, _) =>
        {
            Log("probe-start", $"os={RuntimeInformation.OSDescription}; framework={RuntimeInformation.FrameworkDescription}; renderScaling={RenderScaling:F3}; dpi={RenderScaling * 96:F1}; editorFocused={_editor.IsKeyboardFocusWithin}; log={_logPath}");
            if (OperatingSystem.IsWindows())
                Win32Properties.AddWndProcHookCallback(this, WndProcHook);
            Dispatcher.UIThread.Post(() => { _editor.TextArea.Focus(); Log("focus-check", $"actual={_editor.TextArea.IsKeyboardFocusWithin}"); }, DispatcherPriority.Background);
        };
        Activated += (_, _) => Dispatcher.UIThread.Post(() => _editor.TextArea.Focus(), DispatcherPriority.Input);
        _editor.TextChanged += (_, _) => Log("text-changed", $"chars={_editor.Text.Length}");
        Closed += (_, _) =>
        {
            if (OperatingSystem.IsWindows())
                Win32Properties.RemoveWndProcHookCallback(this, WndProcHook);
            Log("probe-close", "window closed");
            _writer.Dispose();
        };
    }

    private void LogPointer(string name, PointerEventArgs e)
    {
        var p = e.GetPosition(_editor);
        Log(name, $"type={e.Pointer.Type} position=({F(p.X)},{F(p.Y)})");
    }

    private IntPtr WndProcHook(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        var name = msg switch
        {
            WmPointerUpdate => "WM_POINTERUPDATE",
            WmPointerDown => "WM_POINTERDOWN",
            WmPointerUp => "WM_POINTERUP",
            WmPointerEnter => "WM_POINTERENTER",
            WmPointerLeave => "WM_POINTERLEAVE",
            WmPointerCaptureChanged => "WM_POINTERCAPTURECHANGED",
            WmMouseWheel => "WM_MOUSEWHEEL",
            WmMouseHWheel => "WM_MOUSEHWHEEL",
            WmGesture => "WM_GESTURE",
            _ => null
        };
        if (name is not null)
        {
            var detail = msg is WmMouseWheel or WmMouseHWheel
                ? $"delta={unchecked((short)((long)wParam >> 16))} keyState={unchecked((ushort)(long)wParam)} lParam=0x{lParam.ToInt64():X}"
                : $"wParam=0x{wParam.ToInt64():X} lParam=0x{lParam.ToInt64():X}";
            Log($"native-{name}", detail);
        }
        return IntPtr.Zero;
    }

    private void Log(string name, string detail)
    {
        var elapsed = Stopwatch.GetElapsedTime(_startedTimestamp);
        var line = $"{DateTimeOffset.UtcNow:O} +{elapsed.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)}ms focused={_editor.IsKeyboardFocusWithin} scale={RenderScaling:F3} {name} {detail}";
        _writer.WriteLine(line);
        Dispatcher.UIThread.Post(() =>
        {
            _latest.Text = line;
            _recent.Add(line);
            while (_recent.Count > 30)
                _recent.RemoveAt(0);
        });
    }

    private static string F(double value) => value.ToString("F3", CultureInfo.InvariantCulture);
}
