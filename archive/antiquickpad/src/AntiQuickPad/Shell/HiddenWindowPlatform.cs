using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AntiQuickPad.Shell;

// Only selected by the CLI composition root for isolated test/render runs.
internal sealed class HiddenWindowPlatform(Window window) : IWindowPlatform
{
    public bool IsVisible => window.IsVisible;
    public bool Pinned { get => window.Topmost; set => window.Topmost = value; }
    public WindowBounds Bounds
    {
        get => new(window.Width, window.Height, window.Position.X, window.Position.Y);
        set { window.Width = value.Width; window.Height = value.Height; HiddenWindows.Prepare(window); }
    }
    public void Show() { HiddenWindows.Prepare(window); window.Show(); HiddenWindows.Check(window); }
    public void Hide() => window.Hide();
    public void Activate() { }
}

internal static class HiddenWindows
{
    internal static bool Enabled { get; private set; }
    private static IntPtr _initialForeground;
    private static IntPtr _hook;
    private static readonly WinEvent Callback = OnForeground;
    private static readonly SubclassProc PlacementCallback = OnWindowMessage;
    private static readonly Dictionary<IntPtr, PixelPoint> Placements = [];
    private static string? _failure;
    private static readonly List<Window> Windows = [];
    private static DispatcherTimer? _timer;
    private static int _checks;

    internal static void Initialize(string[] args)
    {
        Enabled = args.Contains("--selftest", StringComparer.OrdinalIgnoreCase) ||
            args.Contains("--selftest-quit-resident") ||
            (args.Length > 0 && args[0] is "--render-shell" or "--render-design");
        if (!Enabled) return;
        _initialForeground = GetForegroundWindow();
        _hook = SetWinEventHook(3, 3, IntPtr.Zero, Callback, (uint)Environment.ProcessId, 0, 0);
        if (_hook == IntPtr.Zero) throw new InvalidOperationException("Foreground event hook failed.");
    }

    internal static void Register(Window window)
    {
        var handle = window.TryGetPlatformHandle()?.Handle ?? throw new InvalidOperationException("Missing native test window.");
        // Cloak before the first Show: even native startup placement cannot flash on-screen.
        var cloak = 1;
        Marshal.ThrowExceptionForHR(DwmSetWindowAttribute(handle, 13, ref cloak, sizeof(int)));
        Placements[handle] = OffScreen(window);
        if (!SetWindowSubclass(handle, PlacementCallback, 1, UIntPtr.Zero))
            throw new InvalidOperationException("Cannot constrain hidden native placement.");
        Prepare(window); // Must precede desktop lifetime's automatic MainWindow.Show().
        Windows.Add(window);
        window.Activated += (_, _) => _failure ??= "Hidden window activated.";
        window.Opened += (_, _) => Check(window);
        window.PositionChanged += (_, _) => CheckIfShown(window);
        window.SizeChanged += (_, _) => CheckIfShown(window);
        _timer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Send,
            (_, _) => { foreach (var item in Windows) if (item.IsVisible) Check(item); });
        _timer.Start();
    }

    private static void CheckIfShown(Window window)
    {
        // Avalonia sets IsVisible before it applies startup geometry to the native HWND.
        var handle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (window.IsVisible && IsWindowVisible(handle)) Check(window);
    }

    internal static void Prepare(Window window)
    {
        window.ShowActivated = false;
        window.ShowInTaskbar = false;
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.WindowState = WindowState.Normal;
        // Place beyond the whole virtual desktop, allowing for native frame/shadow extents.
        var point = OffScreen(window);
        var handle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (Placements.ContainsKey(handle)) Placements[handle] = point;
        window.Position = point;
    }

    private static PixelPoint OffScreen(Window window) => new(
        window.Screens.All.Select(s => s.Bounds.Right).DefaultIfEmpty(0).Max() + 4096,
        window.Screens.All.Select(s => s.Bounds.Bottom).DefaultIfEmpty(0).Max() + 4096);

    private static IntPtr OnWindowMessage(IntPtr hwnd, uint message, IntPtr wparam, IntPtr lparam, nuint id, UIntPtr data)
    {
        // Win32/Avalonia constrain initial placement to a monitor. Override the proposed native
        // position before it is applied, including resize, pin and desktop-lifetime Show paths.
        if (message == 0x0046 && Placements.TryGetValue(hwnd, out var point)) // WM_WINDOWPOSCHANGING
        {
            var pos = Marshal.PtrToStructure<WindowPos>(lparam);
            pos.X = point.X; pos.Y = point.Y;
            pos.Flags = (pos.Flags & ~2u) | 0x10u; // clear SWP_NOMOVE; set SWP_NOACTIVATE
            Marshal.StructureToPtr(pos, lparam, false);
        }
        if (message == 0x0021) return new IntPtr(3); // WM_MOUSEACTIVATE: MA_NOACTIVATE
        var result = DefSubclassProc(hwnd, message, wparam, lparam);
        if (message == 0x0082) Placements.Remove(hwnd); // WM_NCDESTROY
        return result;
    }

    internal static void Check(Window window)
    {
        _checks++;
        if (window.ShowInTaskbar || window.ShowActivated) _failure ??= "Taskbar/activation flags enabled.";
        var handle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero || !GetWindowRect(handle, out var rect))
        { _failure ??= "Cannot read native window rectangle."; return; }
        if (DwmGetWindowAttribute(handle, 14, out var cloaked, sizeof(int)) != 0 || cloaked == 0)
            _failure ??= "Native test window is not cloaked.";
        foreach (var screen in window.Screens.All)
        {
            var b = screen.Bounds;
            if (rect.Left < b.Right && rect.Right > b.X && rect.Top < b.Bottom && rect.Bottom > b.Y)
                _failure ??= $"Native window rectangle {rect.Left},{rect.Top},{rect.Right},{rect.Bottom} intersects screen {b}.";
        }
        ObserveForeground(GetForegroundWindow());
    }

    private static void OnForeground(IntPtr hook, uint evt, IntPtr hwnd, int obj, int child, uint thread, uint time)
        => _failure ??= "A test-owned window became foreground (WinEvent).";
    private static void ObserveForeground(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return;
        GetWindowThreadProcessId(hwnd, out var pid);
        if (pid == (uint)Environment.ProcessId) _failure ??= "A test-owned window became foreground.";
    }

    internal static void Assert(string context)
    {
        foreach (var window in Windows) if (window.IsVisible) Check(window);
        var final = GetForegroundWindow(); ObserveForeground(final);
        if (_failure is not null) throw new InvalidOperationException("F37-hidden-selftest-windows: " + _failure);
        if (Windows.Count == 0 || _checks == 0) throw new InvalidOperationException("No hidden windows checked.");
        Console.WriteLine($"F37-hidden-selftest-windows PASS: {context}; windows={Windows.Count}; native rect/cloak/taskbar checks={_checks}; no activation/foreground events; foreground start=0x{_initialForeground:X} end=0x{final:X} ({(final == _initialForeground ? "unchanged" : "external change; end is not test-owned")}).");
    }

    internal static int Finish(int code, string context)
    {
        if (!Enabled) return code;
        try { Assert(context); }
        catch (Exception ex) { Console.Error.WriteLine(ex); code = 1; }
        _timer?.Stop();
        if (_hook != IntPtr.Zero) { UnhookWinEvent(_hook); _hook = IntPtr.Zero; }
        return code;
    }

    private delegate void WinEvent(IntPtr hook, uint evt, IntPtr hwnd, int obj, int child, uint thread, uint time);
    private delegate IntPtr SubclassProc(IntPtr hwnd, uint message, IntPtr wparam, IntPtr lparam, nuint id, UIntPtr data);
    [StructLayout(LayoutKind.Sequential)] private struct WindowPos { public IntPtr Hwnd, InsertAfter; public int X, Y, Width, Height; public uint Flags; }
    [StructLayout(LayoutKind.Sequential)] private struct NativeRect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out NativeRect rect);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
    [DllImport("dwmapi.dll")] private static extern int DwmGetWindowAttribute(IntPtr hwnd, int attribute, out int value, int size);
    [DllImport("comctl32.dll")] private static extern bool SetWindowSubclass(IntPtr hwnd, SubclassProc callback, nuint id, UIntPtr data);
    [DllImport("comctl32.dll")] private static extern IntPtr DefSubclassProc(IntPtr hwnd, uint message, IntPtr wparam, IntPtr lparam);
    [DllImport("user32.dll")] private static extern IntPtr SetWinEventHook(uint min, uint max, IntPtr module, WinEvent callback, uint process, uint thread, uint flags);
    [DllImport("user32.dll")] private static extern bool UnhookWinEvent(IntPtr hook);
}
