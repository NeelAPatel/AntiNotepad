using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using AntiNotepad;

namespace AntiNotepad.Platform;

/// <summary>Maps the taskbar preference to whether the pad should have a taskbar item.</summary>
internal static class TaskbarPresence
{
    internal static bool ShouldShow(string? setting, bool padVisible) => setting switch
    {
        "visible" => padVisible,
        "always" => true,
        _ => false,
    };
}

/// <summary>Manages a manually registered taskbar item for the resident pad window.</summary>
internal sealed class TaskbarItem : IDisposable
{
    private const uint WmSysCommand = 0x0112;
    private const long ScMinimize = 0xF020;
    private readonly Window _window;
    private readonly Action _minimize;
    private readonly uint _taskbarCreated = RegisterWindowMessage("TaskbarCreated");
    private Win32Properties.CustomWndProcHookCallback? _hook;
    private ITaskbarList? _taskbar;
    private bool _added;
    private bool _wanted;
    private bool _disposed;
    private string? _lastFailure;

    internal TaskbarItem(Window window, Action minimize)
    {
        _window = window;
        _minimize = minimize;
        _hook = OnWindowMessage;
        Win32Properties.AddWndProcHookCallback(window, _hook);
    }

    internal void Apply(bool shouldShow)
    {
        _wanted = shouldShow;
        if (_disposed || _window.TryGetPlatformHandle()?.Handle is not { } hwnd || hwnd == IntPtr.Zero) return;

        if (shouldShow)
        {
            if (_added) return;
            try
            {
                EnsureTaskbar().AddTab(hwnd);
                _added = true;
                _lastFailure = null;
            }
            catch (Exception ex) { _added = false; ResetTaskbar(); ReportFailure(ex); }
        }
        else if (_added)
        {
            try
            {
                EnsureTaskbar().DeleteTab(hwnd);
                _added = false;
                _lastFailure = null;
            }
            catch (Exception ex) { ResetTaskbar(); ReportFailure(ex); }
        }
    }

    internal void Prepare()
    {
        if (_disposed) return;
        try { _ = EnsureTaskbar(); _lastFailure = null; }
        catch (Exception ex) { ResetTaskbar(); ReportFailure(ex); }
    }

    private ITaskbarList EnsureTaskbar()
    {
        if (_taskbar is not null) return _taskbar;
        var type = Type.GetTypeFromCLSID(new Guid("56FDF344-FD6D-11D0-958A-006097C9A090"), throwOnError: true)!;
        var taskbar = (ITaskbarList)Activator.CreateInstance(type)!;
        try { taskbar.HrInit(); }
        catch
        {
            try { Marshal.ReleaseComObject(taskbar); } catch { }
            throw;
        }
        _taskbar = taskbar;
        return taskbar;
    }

    private IntPtr OnWindowMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (_taskbarCreated != 0 && message == _taskbarCreated)
        {
            _added = false;
            ResetTaskbar();
            if (_wanted) Apply(true);
            return IntPtr.Zero;
        }

        if (message == WmSysCommand && (wParam.ToInt64() & 0xFFF0) == ScMinimize && _window.IsVisible && _window.IsActive)
        {
            // A focused taskbar button normally asks Windows to minimize its window. Route that through
            // the pad's normal hide path so it also saves and starts the hidden-document trim timer.
            handled = true;
            Dispatcher.UIThread.Post(_minimize, DispatcherPriority.Input);
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        var hwnd = _window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (_added && hwnd != IntPtr.Zero)
        {
            try { EnsureTaskbar().DeleteTab(hwnd); }
            catch (Exception ex) { ReportFailure(ex); }
            _added = false;
        }
        if (_hook is { } hook)
        {
            Win32Properties.RemoveWndProcHookCallback(_window, hook);
            _hook = null;
        }
        ResetTaskbar();
    }

    private void ResetTaskbar()
    {
        if (_taskbar is null) return;
        try { Marshal.ReleaseComObject(_taskbar); } catch { }
        _taskbar = null;
    }

    private void ReportFailure(Exception ex)
    {
        if (_lastFailure == ex.Message) return;
        _lastFailure = ex.Message;
        Program.Log("taskbar: " + ex.Message);
    }

    [ComImport]
    [Guid("56FDF342-FD6D-11D0-958A-006097C9A090")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITaskbarList
    {
        void HrInit();
        void AddTab(IntPtr hwnd);
        void DeleteTab(IntPtr hwnd);
        void ActivateTab(IntPtr hwnd);
        void SetActiveAlt(IntPtr hwnd);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint RegisterWindowMessage(string message);
}

/// <summary>A stable minimized button that stands in for the hidden pad when Always is selected.</summary>
internal sealed class TaskbarProxy : IDisposable
{
    private const uint WmSysCommand = 0x0112;
    private const long ScRestore = 0xF120;
    private const int SwHide = 0;
    private const int SwShowMinNoActive = 7;
    private readonly Window _window;
    private readonly Action _activatePad;
    private Win32Properties.CustomWndProcHookCallback? _hook;
    private bool _disposed;
    private bool _shown;
    private bool _activationPending;

    internal TaskbarProxy(string title, WindowIcon? icon, Action activatePad)
    {
        _activatePad = activatePad;
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
            ShowNativeMinimized();
            QueueActivation();
        };
        _hook = OnWindowMessage;
        Win32Properties.AddWndProcHookCallback(_window, _hook);
    }

    internal void ShowMinimized()
    {
        if (_disposed) return;
        if (_shown) return;
        _shown = true;
        ShowNativeMinimized();
    }

    internal void Hide()
    {
        if (_disposed) return;
        if (!_shown) return;
        _shown = false;
        ShowWindow(Handle, SwHide);
    }

    private IntPtr Handle => _window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;

    private void ShowNativeMinimized()
    {
        var hwnd = Handle;
        if (hwnd != IntPtr.Zero) ShowWindow(hwnd, SwShowMinNoActive);
        else Program.Log("taskbar: proxy native handle is unavailable");
    }

    private IntPtr OnWindowMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == WmSysCommand && (wParam.ToInt64() & 0xFFF0) == ScRestore && _shown)
        {
            // Keep the proxy minimized and send the click to the resident pad without showing or focusing
            // this 1 px helper window.
            handled = true;
            QueueActivation();
        }
        return IntPtr.Zero;
    }

    private void QueueActivation()
    {
        if (_activationPending) return;
        _activationPending = true;
        Dispatcher.UIThread.Post(() =>
        {
            _activationPending = false;
            if (!_disposed && _shown) _activatePad();
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
        _window.Close();
    }

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, int command);
}
