using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using AntiNotepad;

namespace AntiNotepad.Platform;

/// <summary>Maps the taskbar preference to whether the taskbar proxy should be present.</summary>
internal static class TaskbarPresence
{
    private static readonly TimeSpan ForegroundLossGracePeriod = TimeSpan.FromMilliseconds(500);

    internal static bool ShouldShow(string? setting, bool padVisible) => setting switch
    {
        "visible" => padVisible,
        "always" => true,
        _ => false,
    };

    /// <summary>A taskbar click hides only when the visible pad owned the foreground before the click.</summary>
    internal static bool ShouldHideOnClick(bool padVisible, bool padForeground) => padVisible && padForeground;

    /// <summary>
    /// Windows can activate the taskbar before delivering the stand-in's restore request. Treat a recent loss
    /// to the taskbar or stand-in as foreground at click time, while ignoring losses to other windows.
    /// </summary>
    internal static bool WasPadRecentlyForeground(long lastForegroundLoss, long now, TaskbarActivationCounterpart counterpart) =>
        lastForegroundLoss > 0 && now >= lastForegroundLoss
        && counterpart is TaskbarActivationCounterpart.Taskbar or TaskbarActivationCounterpart.Proxy
        && Stopwatch.GetElapsedTime(lastForegroundLoss, now) <= ForegroundLossGracePeriod;
}

internal enum TaskbarActivationCounterpart
{
    Other,
    Taskbar,
    Proxy,
}

/// <summary>A stable minimized taskbar button for the resident pad.</summary>
internal sealed class TaskbarProxy : IDisposable
{
    private const uint WmActivate = 0x0006;
    private const uint WmSysCommand = 0x0112;
    private const long ScRestore = 0xF120;
    private const int WaInactive = 0;
    private const int WaActive = 1;
    private const int WaClickActive = 2;
    private const int SwHide = 0;
    private const int SwShowMinNoActive = 7;
    private readonly Window _pad;
    private readonly Window _window;
    private readonly Action<bool> _taskbarClick;
    private readonly uint _taskbarCreated = RegisterWindowMessage("TaskbarCreated");
    private Win32Properties.CustomWndProcHookCallback? _hook;
    private Win32Properties.CustomWndProcHookCallback? _padHook;
    private bool _disposed;
    private bool _shown;
    private bool _activationPending;
    private bool _padWasForegroundBeforeProxyActivation;
    private long _lastForegroundLoss;
    private TaskbarActivationCounterpart _lastForegroundLossCounterpart;
    private bool _suppressNextPadDeactivation;
    private int _focusSnapshotGeneration;
    private int _presenceGeneration;

    internal TaskbarProxy(Window pad, string title, WindowIcon? icon, Action<bool> taskbarClick)
    {
        _pad = pad;
        _taskbarClick = taskbarClick;
        _window = new Window
        {
            Title = title,
            Icon = icon,
            Width = 1,
            Height = 1,
            MinWidth = 1,
            MinHeight = 1,
            Opacity = 0,
            ShowInTaskbar = true,
            ShowActivated = false,
            WindowDecorations = WindowDecorations.None,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Position = new PixelPoint(-32000, -32000),
        };
        _window.Closing += (_, e) =>
        {
            if (_disposed) return;
            e.Cancel = true;
            ShowMinimized();
        };
        _window.Activated += (_, _) =>
        {
            if (!_shown || _disposed) return;
            QueueTaskbarClick(IntPtr.Zero);
            ShowNativeMinimized();
        };
        _hook = OnProxyWindowMessage;
        _padHook = OnPadWindowMessage;
        Win32Properties.AddWndProcHookCallback(_window, _hook);
        Win32Properties.AddWndProcHookCallback(_pad, _padHook);
    }

    internal bool ConsumePadDeactivationToProxy()
    {
        var suppress = _suppressNextPadDeactivation;
        _suppressNextPadDeactivation = false;
        return suppress;
    }

    internal void ShowMinimized()
    {
        if (_disposed || _shown) return;
        _presenceGeneration++;
        _shown = true;
        ShowNativeMinimized();
    }

    internal void Hide()
    {
        if (_disposed || !_shown) return;
        _shown = false;
        _presenceGeneration++;
        var hwnd = Handle;
        if (hwnd != IntPtr.Zero) ShowWindow(hwnd, SwHide);
    }

    private IntPtr Handle => _window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
    private IntPtr PadHandle => _pad.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;

    private void ShowNativeMinimized()
    {
        var hwnd = Handle;
        if (hwnd != IntPtr.Zero) ShowWindow(hwnd, SwShowMinNoActive);
        else Program.Log("taskbar: proxy native handle is unavailable");
    }

    private IntPtr OnPadWindowMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == WmActivate && (wParam.ToInt64() & 0xFFFF) == WaInactive && _shown)
        {
            // WM_ACTIVATE carries the next active window. Explorer can take foreground before the proxy receives
            // SC_RESTORE, so preserve the loss time and counterpart while this message still identifies both.
            _lastForegroundLoss = Stopwatch.GetTimestamp();
            _lastForegroundLossCounterpart = GetCounterpart(lParam);

            var proxyHwnd = Handle;
            if (proxyHwnd != IntPtr.Zero && lParam == proxyHwnd)
            {
                // Capture before Avalonia raises Deactivated and before Windows can make the proxy foreground.
                _padWasForegroundBeforeProxyActivation = _pad.IsActive || GetForegroundWindow() == hwnd;
                _suppressNextPadDeactivation = true;
                var generation = ++_focusSnapshotGeneration;
                Dispatcher.UIThread.Post(() =>
                {
                    if (generation != _focusSnapshotGeneration) return;
                    _padWasForegroundBeforeProxyActivation = false;
                    _suppressNextPadDeactivation = false;
                }, DispatcherPriority.Background);
            }
            else
            {
                _padWasForegroundBeforeProxyActivation = false;
                _suppressNextPadDeactivation = false;
                _focusSnapshotGeneration++;
            }
        }
        return IntPtr.Zero;
    }

    private TaskbarActivationCounterpart GetCounterpart(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return TaskbarActivationCounterpart.Other;
        if (hwnd == Handle) return TaskbarActivationCounterpart.Proxy;

        var className = new StringBuilder(64);
        var length = GetClassName(hwnd, className, className.Capacity);
        return length > 0 && (string.Equals(className.ToString(), "Shell_TrayWnd", StringComparison.OrdinalIgnoreCase)
            || string.Equals(className.ToString(), "Shell_SecondaryTrayWnd", StringComparison.OrdinalIgnoreCase))
            ? TaskbarActivationCounterpart.Taskbar
            : TaskbarActivationCounterpart.Other;
    }

    private IntPtr OnProxyWindowMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (_taskbarCreated != 0 && message == _taskbarCreated)
        {
            if (_shown)
            {
                ShowWindow(hwnd, SwHide);
                ShowNativeMinimized();
            }
            return IntPtr.Zero;
        }

        if (message == WmSysCommand && (wParam.ToInt64() & 0xFFF0) == ScRestore && _shown)
        {
            // Decide from the foreground snapshot before restore/activation can move it to this helper.
            handled = true;
            QueueTaskbarClick(IntPtr.Zero);
            return IntPtr.Zero;
        }

        var activation = (int)(wParam.ToInt64() & 0xFFFF);
        if (message == WmActivate && (activation == WaActive || activation == WaClickActive) && _shown)
        {
            QueueTaskbarClick(lParam);
            ShowNativeMinimized();
        }
        return IntPtr.Zero;
    }

    private void QueueTaskbarClick(IntPtr previousForeground)
    {
        if (_activationPending || _disposed || !_shown) return;
        var padHwnd = PadHandle;
        var now = Stopwatch.GetTimestamp();
        var padWasForeground = _padWasForegroundBeforeProxyActivation
            || (padHwnd != IntPtr.Zero && previousForeground == padHwnd)
            || (padHwnd != IntPtr.Zero && GetForegroundWindow() == padHwnd)
            || TaskbarPresence.WasPadRecentlyForeground(_lastForegroundLoss, now, _lastForegroundLossCounterpart)
            || _pad.IsActive;
        _padWasForegroundBeforeProxyActivation = false;
        _lastForegroundLoss = 0;
        _lastForegroundLossCounterpart = TaskbarActivationCounterpart.Other;
        var hide = TaskbarPresence.ShouldHideOnClick(_pad.IsVisible, padWasForeground);
        var presenceGeneration = _presenceGeneration;
        _activationPending = true;
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                if (!_disposed && _shown && presenceGeneration == _presenceGeneration) _taskbarClick(hide);
            }
            finally { _activationPending = false; }
        }, DispatcherPriority.Input);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_shown) ShowWindow(Handle, SwHide);
        _shown = false;
        if (_hook is { } hook)
        {
            Win32Properties.RemoveWndProcHookCallback(_window, hook);
            _hook = null;
        }
        if (_padHook is { } padHook)
        {
            Win32Properties.RemoveWndProcHookCallback(_pad, padHook);
            _padHook = null;
        }
        _window.Close();
    }

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, int command);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hwnd, StringBuilder className, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint RegisterWindowMessage(string message);
}
