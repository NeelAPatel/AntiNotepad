using System.Diagnostics;
using System.Runtime.InteropServices;
using AntiNotepad;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;

namespace AntiNotepad.Platform;

/// <summary>Controls the real pad tab and the closed-state Always stand-in.</summary>
internal static class TaskbarPresence
{
    private static readonly long TraceStartedAt = Stopwatch.GetTimestamp();
    private const string TaskbarListClassId = "56FDF344-FD6D-11d0-958A-006097C9A090";
    private static long _padActionAt;
    private static string _padActionCause = "unknown";
    private static Action? _reconcile;
    private static Win32Properties.CustomWndProcHookCallback? _padHook;
    private static uint _taskbarCreated;
    private static uint _taskbarButtonCreated;
    private static ITaskbarList? _taskbarList;
    private static bool _taskbarListAttempted;
    private static IntPtr _registeredPadHandle;
    private static bool? _registeredPad;

    internal static bool ShouldShowPad(string? setting, PadVisibilityState state) => setting switch
    {
        "visible" => state == PadVisibilityState.Open,
        "always" => state != PadVisibilityState.Closed,
        _ => false,
    };

    internal static bool ShouldShowProxy(string? setting, PadVisibilityState state) =>
        setting == "always" && state == PadVisibilityState.Closed;

    internal static void Initialize(Window pad, Action reconcile)
    {
        _reconcile = reconcile;
        EnsureTaskbarList();
        _taskbarCreated = Native.RegisterWindowMessage("TaskbarCreated");
        _taskbarButtonCreated = Native.RegisterWindowMessage("TaskbarButtonCreated");
        _padHook = OnPadWindowMessage;
        Win32Properties.AddWndProcHookCallback(pad, _padHook);
    }

    internal static void PrepareForSettingsChange() => EnsureTaskbarList(retry: true);

    private static IntPtr OnPadWindowMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if ((_taskbarCreated != 0 && message == _taskbarCreated)
            || (_taskbarButtonCreated != 0 && message == _taskbarButtonCreated))
        {
            var shellRestarted = message == _taskbarCreated;
            if (shellRestarted)
            {
                ReleaseTaskbarList(resetAttempted: true);
                _registeredPad = null;
                _registeredPadHandle = IntPtr.Zero;
            }
            Dispatcher.UIThread.Post(() =>
            {
                if (_reconcile is null) return;
                EnsureTaskbarList(retry: shellRestarted);
                if (!shellRestarted)
                {
                    // Explorer created the default tab for this HWND. Reconcile it against the selected mode.
                    _registeredPadHandle = hwnd;
                    _registeredPad = true;
                }
                _reconcile?.Invoke();
            }, DispatcherPriority.Background);
        }
        return IntPtr.Zero;
    }

    internal static void SetPadPresence(Window pad, bool present, bool force = false)
    {
        var hwnd = pad.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (hwnd == IntPtr.Zero) return;
        if (!force && _registeredPadHandle == hwnd && _registeredPad == present) return;
        if (_taskbarList is null) return;

        // Cache before the COM call. Explorer can post TaskbarButtonCreated as part of AddTab.
        _registeredPadHandle = hwnd;
        _registeredPad = present;
        try
        {
            var hr = present ? _taskbarList!.AddTab(hwnd) : _taskbarList!.DeleteTab(hwnd);
            if (hr < 0)
            {
                _registeredPad = null;
                Trace($"pad {(present ? "AddTab" : "DeleteTab")} failed hr=0x{hr:X8}");
            }
        }
        catch (Exception ex)
        {
            _registeredPad = null;
            ReleaseTaskbarList(resetAttempted: false);
            Trace($"pad {(present ? "AddTab" : "DeleteTab")} failed: {ex.Message}");
        }
    }

    private static bool EnsureTaskbarList(bool retry = false)
    {
        if (_taskbarList is not null) return true;
        if (_taskbarListAttempted && !retry) return false;
        _taskbarListAttempted = true;
        try
        {
            var type = Type.GetTypeFromCLSID(Guid.Parse(TaskbarListClassId), throwOnError: true)!;
            var instance = Activator.CreateInstance(type) as ITaskbarList;
            if (instance is null) return false;
            var hr = instance.HrInit();
            if (hr < 0)
            {
                Marshal.FinalReleaseComObject(instance);
                Trace($"ITaskbarList.HrInit failed hr=0x{hr:X8}");
                return false;
            }
            _taskbarList = instance;
            return true;
        }
        catch (Exception ex)
        {
            Trace($"ITaskbarList initialization failed: {ex.Message}");
            return false;
        }
    }

    private static void ReleaseTaskbarList(bool resetAttempted)
    {
        var list = _taskbarList;
        _taskbarList = null;
        if (resetAttempted) _taskbarListAttempted = false;
        if (list is not null && Marshal.IsComObject(list))
        {
            try { Marshal.FinalReleaseComObject(list); } catch { }
        }
    }

    internal static void Dispose(Window pad)
    {
        if (_padHook is { } hook)
        {
            Win32Properties.RemoveWndProcHookCallback(pad, hook);
            _padHook = null;
        }
        _reconcile = null;
        _registeredPad = null;
        _registeredPadHandle = IntPtr.Zero;
        ReleaseTaskbarList(resetAttempted: false);
    }

    internal static void Trace(string message)
    {
        if (Program.IsDev) Program.Log($"taskbar: t={Stopwatch.GetElapsedTime(TraceStartedAt).TotalMilliseconds:F1}ms {message}");
    }

    internal static double SinceTraceStart(long timestamp) => Stopwatch.GetElapsedTime(TraceStartedAt, timestamp).TotalMilliseconds;

    internal static void TracePadAction(string cause, string phase, PadVisibilityState state)
    {
        if (!Program.IsDev) return;
        _padActionCause = cause;
        _padActionAt = Stopwatch.GetTimestamp();
        Trace($"pad action={phase} cause={cause} state={state}");
    }

    internal static string PadActionContext()
    {
        if (!Program.IsDev || _padActionAt == 0) return "cause=unknown age=na";
        return $"cause={_padActionCause} age={Stopwatch.GetElapsedTime(_padActionAt).TotalMilliseconds:F1}ms";
    }

    [ComImport]
    [Guid("56FDF342-FD6D-11d0-958A-006097C9A090")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITaskbarList
    {
        [PreserveSig] int HrInit();
        [PreserveSig] int AddTab(IntPtr hwnd);
        [PreserveSig] int DeleteTab(IntPtr hwnd);
        [PreserveSig] int ActivateTab(IntPtr hwnd);
        [PreserveSig] int SetActiveAlt(IntPtr hwnd);
    }
}

internal enum PadVisibilityState
{
    Closed,
    Minimized,
    Open,
}

/// <summary>A transparent minimized button retained only while the closed pad is set to Always.</summary>
internal sealed class TaskbarProxy : IDisposable
{
    private const uint WmSysCommand = 0x0112;
    private const long ScRestore = 0xF120;
    private readonly Window _window;
    private readonly Action _restore;
    private Win32Properties.CustomWndProcHookCallback? _hook;
    private bool _shown;
    private bool _disposed;
    private bool _restorePending;

    internal TaskbarProxy(string title, WindowIcon? icon, Action restore)
    {
        _restore = restore;
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
            Position = new Avalonia.PixelPoint(-32000, -32000),
        };
        _window.Closing += (_, e) =>
        {
            if (!SessionEnd.ShouldCancelClose(e.CloseReason, _disposed, SessionEnd.IsEnding)) return;
            e.Cancel = true;
            ShowMinimized();
        };
        _window.Activated += (_, _) => QueueRestore();
        _hook = OnWindowMessage;
        Win32Properties.AddWndProcHookCallback(_window, _hook);
    }

    internal void ShowMinimized()
    {
        if (_disposed || _shown) return;
        _shown = true;
        using var suppress = Native.SuppressDwmTransitions(_window);
        Native.ShowMinimizedNoActivate(_window);
    }

    internal void Hide()
    {
        if (_disposed || !_shown) return;
        _shown = false;
        _restorePending = false;
        using var suppress = Native.SuppressDwmTransitions(_window);
        Native.HideNativeWindow(_window);
    }

    private IntPtr OnWindowMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (_shown && message == WmSysCommand && (wParam.ToInt64() & 0xFFF0) == ScRestore)
        {
            handled = true;
            QueueRestore();
        }
        return IntPtr.Zero;
    }

    private void QueueRestore()
    {
        if (!_shown || _disposed || _restorePending || SessionEnd.IsEnding) return;
        _restorePending = true;
        Dispatcher.UIThread.Post(() =>
        {
            _restorePending = false;
            if (_shown && !_disposed && !SessionEnd.IsEnding) _restore();
        }, DispatcherPriority.Input);
    }

    public void Dispose()
    {
        if (_disposed) return;
        Hide();
        _disposed = true;
        if (_hook is { } hook)
        {
            Win32Properties.RemoveWndProcHookCallback(_window, hook);
            _hook = null;
        }
        _window.Close();
    }
}
