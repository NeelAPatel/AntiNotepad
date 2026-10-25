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
    private static readonly TimeSpan PostHideFallbackPeriod = TimeSpan.FromSeconds(1);
    private static readonly long TraceStartedAt = Stopwatch.GetTimestamp();
    private static long _padActionAt;
    private static string _padActionCause = "unknown";

    internal static bool ShouldShow(string? setting, bool padVisible) => setting switch
    {
        "visible" => padVisible,
        "always" => true,
        _ => false,
    };

    /// <summary>A taskbar click hides only when the visible pad owned the foreground before the click.</summary>
    internal static bool ShouldHideOnClick(bool padVisible, bool padForeground) => padVisible && padForeground;

    internal static bool ShouldSuppressPostHideActivation(bool decisionConsumed, bool lastDecisionWasHide, bool freshClickStart,
        bool explicitRestore, long hideDecisionAt, long now) =>
        decisionConsumed && lastDecisionWasHide && !freshClickStart && !explicitRestore
        && hideDecisionAt > 0 && now >= hideDecisionAt
        && Stopwatch.GetElapsedTime(hideDecisionAt, now) <= PostHideFallbackPeriod;

    /// <summary>
    /// Windows can activate the taskbar before delivering the stand-in's restore request. Treat a recent loss
    /// to the taskbar, stand-in, or a null (other-thread) counterpart as foreground at click time,
    /// while ignoring losses to known unrelated windows.
    /// </summary>
    internal static bool WasPadRecentlyForeground(long lastForegroundLoss, long now, TaskbarActivationCounterpart counterpart) =>
        lastForegroundLoss > 0 && now >= lastForegroundLoss
        && counterpart is TaskbarActivationCounterpart.None or TaskbarActivationCounterpart.Taskbar or TaskbarActivationCounterpart.Proxy
        && Stopwatch.GetElapsedTime(lastForegroundLoss, now) <= ForegroundLossGracePeriod;

    internal static void Trace(string message)
    {
        if (Program.IsDev) Program.Log($"taskbar: t={Stopwatch.GetElapsedTime(TraceStartedAt).TotalMilliseconds:F1}ms {message}");
    }

    internal static double SinceTraceStart(long timestamp) => Stopwatch.GetElapsedTime(TraceStartedAt, timestamp).TotalMilliseconds;

    internal static void TracePadAction(string cause, string phase, bool visible)
    {
        if (!Program.IsDev) return;
        _padActionCause = cause;
        _padActionAt = Stopwatch.GetTimestamp();
        Trace($"pad action={phase} cause={cause} visible={visible}");
    }

    internal static string PadActionContext()
    {
        if (!Program.IsDev || _padActionAt == 0) return "cause=unknown age=na";
        return $"cause={_padActionCause} age={Stopwatch.GetElapsedTime(_padActionAt).TotalMilliseconds:F1}ms";
    }
}

internal enum TaskbarActivationCounterpart
{
    Other,
    None,
    Taskbar,
    Proxy,
}

/// <summary>A stable minimized taskbar button for the resident pad.</summary>
internal sealed class TaskbarProxy : IDisposable
{
    private const uint WmActivate = 0x0006;
    private const uint WmActivateApp = 0x001C;
    private const uint WmNcActivate = 0x0086;
    private const uint WmShowWindow = 0x0018;
    private const uint WmWindowPosChanging = 0x0046;
    private const uint WmSysCommand = 0x0112;
    private const long ScRestore = 0xF120;
    private const int WaInactive = 0;
    private const int WaActive = 1;
    private const int WaClickActive = 2;
    private const int SwHide = 0;
    private const int SwShowMinNoActive = 7;
    private const uint SwpHideWindow = 0x0080;
    private readonly Window _pad;
    private readonly Window _window;
    private readonly Action<bool, string> _taskbarClick;
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
    private int _nativeVisibilityChange;
    private bool _clickSnapshotAvailable;
    private bool _clickSnapshotConsumed;
    private bool _lastDecisionWasHide;
    private long _lastDecisionAt;
    private ClickSnapshot _clickSnapshot;

    internal TaskbarProxy(Window pad, string title, WindowIcon? icon, Action<bool, string> taskbarClick)
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
        Native.DisableDwmTransitions(_window);
        _window.Closing += (_, e) =>
        {
            if (!SessionEnd.ShouldCancelClose(e.CloseReason, _disposed, SessionEnd.IsEnding)) return;
            e.Cancel = true;
            ShowMinimized();
        };
        _window.Activated += (_, _) =>
        {
            if (!_shown || _disposed || SessionEnd.IsEnding) return;
            if (Program.IsDev) TaskbarPresence.Trace("proxy Avalonia.Activated fallback");
            QueueTaskbarClick(IntPtr.Zero, "Avalonia.Activated");
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
        if (Program.IsDev) TaskbarPresence.Trace($"proxy presence=show generation={_presenceGeneration}");
        ShowNativeMinimized();
    }

    internal void Hide()
    {
        if (_disposed || !_shown) return;
        _shown = false;
        _presenceGeneration++;
        if (Program.IsDev) TaskbarPresence.Trace($"proxy presence=hide generation={_presenceGeneration}");
        var hwnd = Handle;
        if (hwnd != IntPtr.Zero) NativeShowWindow(hwnd, SwHide);
    }

    private IntPtr Handle => _window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
    private IntPtr PadHandle => _pad.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;

    private void ShowNativeMinimized()
    {
        var hwnd = Handle;
        if (hwnd != IntPtr.Zero) NativeShowWindow(hwnd, SwShowMinNoActive);
        else if (Program.IsDev) TaskbarPresence.Trace("proxy native handle is unavailable");
    }

    private void NativeShowWindow(IntPtr hwnd, int command)
    {
        _nativeVisibilityChange++;
        try { ShowWindow(hwnd, command); }
        finally { _nativeVisibilityChange--; }
    }

    private IntPtr OnPadWindowMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (SessionEnd.IsEnding) return IntPtr.Zero;
        if (message == WmActivate)
        {
            var state = (int)(wParam.ToInt64() & 0xFFFF);
            var other = Program.IsDev ? DescribeWindow(lParam) : "not-logged";
            if (Program.IsDev) TaskbarPresence.Trace($"pad WM_ACTIVATE state={ActivationName(state)} wParam=0x{wParam.ToInt64():X} counterpart={other} visible={_pad.IsVisible} active={_pad.IsActive} {TaskbarPresence.PadActionContext()}");
            if (state == WaInactive && _shown)
            {
                // Preserve the loss while the native message still identifies its counterpart.
                var lossAt = Stopwatch.GetTimestamp();
                var lossKind = GetCounterpart(lParam);
                if (!_clickSnapshotConsumed)
                {
                    _lastForegroundLoss = lossAt;
                    _lastForegroundLossCounterpart = lossKind;
                }

                var proxyHwnd = Handle;
                if (proxyHwnd != IntPtr.Zero && lParam == proxyHwnd)
                {
                    if (!_clickSnapshotConsumed)
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
                if (Program.IsDev) TaskbarPresence.Trace($"pad foreground-loss kind={lossKind} at={TaskbarPresence.SinceTraceStart(lossAt):F1}ms counterpart={other} included={!_clickSnapshotConsumed}");
            }
        }
        else if (message == WmShowWindow)
        {
            if (Program.IsDev) TaskbarPresence.Trace($"pad WM_SHOWWINDOW show={wParam != IntPtr.Zero} reason={lParam.ToInt64()} visible={_pad.IsVisible} {TaskbarPresence.PadActionContext()}");
        }
        else if (message == WmNcActivate)
        {
            if (Program.IsDev) TaskbarPresence.Trace($"pad WM_NCACTIVATE active={wParam != IntPtr.Zero} lParam=0x{lParam.ToInt64():X} visible={_pad.IsVisible} {TaskbarPresence.PadActionContext()}");
        }
        else if (message == WmActivateApp)
        {
            if (Program.IsDev) TaskbarPresence.Trace($"pad WM_ACTIVATEAPP active={wParam != IntPtr.Zero} thread={lParam.ToInt64()} visible={_pad.IsVisible} {TaskbarPresence.PadActionContext()}");
        }
        return IntPtr.Zero;
    }

    private TaskbarActivationCounterpart GetCounterpart(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return TaskbarActivationCounterpart.None;
        if (hwnd == Handle) return TaskbarActivationCounterpart.Proxy;

        var className = GetWindowClass(hwnd);
        return string.Equals(className, "Shell_TrayWnd", StringComparison.OrdinalIgnoreCase)
            || string.Equals(className, "Shell_SecondaryTrayWnd", StringComparison.OrdinalIgnoreCase)
            ? TaskbarActivationCounterpart.Taskbar
            : TaskbarActivationCounterpart.Other;
    }

    private IntPtr OnProxyWindowMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (SessionEnd.IsEnding) return IntPtr.Zero;
        TraceProxyMessage(hwnd, message, wParam, lParam);

        if (_taskbarCreated != 0 && message == _taskbarCreated)
        {
            if (_shown)
            {
                NativeShowWindow(hwnd, SwHide);
                ShowNativeMinimized();
            }
            if (Program.IsDev) TaskbarPresence.Trace($"proxy TaskbarCreated shown={_shown}");
            return IntPtr.Zero;
        }

        if (message == WmWindowPosChanging && _shown && _nativeVisibilityChange == 0
            && TryGetWindowPosFlags(lParam, out var flags) && (flags & SwpHideWindow) == 0)
        {
            StartClickSnapshot("WM_WINDOWPOSCHANGING", fresh: _clickSnapshotConsumed);
        }
        else if (message == WmShowWindow && wParam != IntPtr.Zero && _shown && _nativeVisibilityChange == 0)
        {
            StartClickSnapshot("WM_SHOWWINDOW(show)", fresh: _clickSnapshotConsumed);
        }

        if (message == WmSysCommand && (wParam.ToInt64() & 0xFFF0) == ScRestore && _shown)
        {
            // SC_RESTORE is the clearest click signal. Preserve any earlier pre-activation snapshot.
            handled = true;
            if (!_clickSnapshotAvailable || _clickSnapshotConsumed || IsSnapshotStale())
                StartClickSnapshot("WM_SYSCOMMAND/SC_RESTORE", fresh: true);
            else if (Program.IsDev) TaskbarPresence.Trace("proxy click snapshot reused for SC_RESTORE");
            QueueTaskbarClick(IntPtr.Zero, "SC_RESTORE", explicitRestore: true);
            return IntPtr.Zero;
        }

        if (message == WmNcActivate && wParam != IntPtr.Zero && _shown)
        {
            EnsureFallbackSnapshot("WM_NCACTIVATE");
        }

        if (message == WmActivate)
        {
            var activation = (int)(wParam.ToInt64() & 0xFFFF);
            if ((activation == WaActive || activation == WaClickActive) && _shown)
            {
                EnsureFallbackSnapshot("WM_ACTIVATE");
                QueueTaskbarClick(lParam, "WM_ACTIVATE");
                ShowNativeMinimized();
            }
        }
        return IntPtr.Zero;
    }

    private void TraceProxyMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam)
    {
        if (!Program.IsDev) return;
        var prefix = $"proxy hwnd={FormatHwnd(hwnd)}";
        if (message == WmSysCommand)
        {
            var command = wParam.ToInt64() & 0xFFF0;
            TaskbarPresence.Trace($"{prefix} WM_SYSCOMMAND wParam=0x{wParam.ToInt64():X} command=0x{command:X} name={(command == ScRestore ? "SC_RESTORE" : "other")} lParam=0x{lParam.ToInt64():X}");
        }
        else if (message == WmActivate)
        {
            var state = (int)(wParam.ToInt64() & 0xFFFF);
            var minimized = ((wParam.ToInt64() >> 16) & 0xFFFF) != 0;
            TaskbarPresence.Trace($"{prefix} WM_ACTIVATE state={ActivationName(state)} minimized={minimized} counterpart={DescribeWindow(lParam)} wParam=0x{wParam.ToInt64():X}");
        }
        else if (message == WmActivateApp)
        {
            TaskbarPresence.Trace($"{prefix} WM_ACTIVATEAPP active={wParam != IntPtr.Zero} thread={lParam.ToInt64()}");
        }
        else if (message == WmNcActivate)
        {
            TaskbarPresence.Trace($"{prefix} WM_NCACTIVATE active={wParam != IntPtr.Zero} lParam=0x{lParam.ToInt64():X}");
        }
        else if (message == WmShowWindow)
        {
            TaskbarPresence.Trace($"{prefix} WM_SHOWWINDOW show={wParam != IntPtr.Zero} reason={lParam.ToInt64()}");
        }
        else if (message == WmWindowPosChanging)
        {
            if (TryGetWindowPosFlags(lParam, out var flags))
                TaskbarPresence.Trace($"{prefix} WM_WINDOWPOSCHANGING flags=0x{flags:X8} names={WindowPosFlagNames(flags)} internal={_nativeVisibilityChange > 0}");
            else TaskbarPresence.Trace($"{prefix} WM_WINDOWPOSCHANGING flags=unavailable internal={_nativeVisibilityChange > 0}");
        }
    }

    private void EnsureFallbackSnapshot(string source)
    {
        if (_clickSnapshotConsumed && ShouldSuppressPostHideFallback(source)) return;
        if (!_clickSnapshotAvailable || _clickSnapshotConsumed || IsSnapshotStale())
            StartClickSnapshot(source, fresh: true);
    }

    private bool ShouldSuppressPostHideFallback(string source)
    {
        var now = Stopwatch.GetTimestamp();
        var suppress = TaskbarPresence.ShouldSuppressPostHideActivation(_clickSnapshotConsumed, _lastDecisionWasHide,
            freshClickStart: false, explicitRestore: false, hideDecisionAt: _lastDecisionAt, now: now);
        if (Program.IsDev && suppress)
            TaskbarPresence.Trace($"fallback suppressed source={source} reason=post-hide duplicate elapsed={Stopwatch.GetElapsedTime(_lastDecisionAt, now).TotalMilliseconds:F1}ms");
        return suppress;
    }

    private void StartClickSnapshot(string source, bool fresh)
    {
        if (!fresh && _clickSnapshotAvailable && !_clickSnapshotConsumed && !IsSnapshotStale()) return;

        var now = Stopwatch.GetTimestamp();
        var foreground = GetForegroundWindow();
        var padHandle = PadHandle;
        var padVisible = _pad.IsVisible;
        var padActive = _pad.IsActive;
        var foregroundLossAge = Program.IsDev ? LossAgeMilliseconds(now) : null;
        var recentlyLostToTaskbar = TaskbarPresence.WasPadRecentlyForeground(_lastForegroundLoss, now, _lastForegroundLossCounterpart);
        var padWasForeground = padVisible && (padActive || (padHandle != IntPtr.Zero && foreground == padHandle)
            || _padWasForegroundBeforeProxyActivation || recentlyLostToTaskbar);
        _clickSnapshot = new ClickSnapshot(now, padVisible, padActive, foreground, Program.IsDev ? GetWindowClass(foreground) : string.Empty,
            _lastForegroundLoss, _lastForegroundLossCounterpart, foregroundLossAge, padWasForeground);
        _clickSnapshotAvailable = true;
        _clickSnapshotConsumed = false;
        if (Program.IsDev) TaskbarPresence.Trace($"click snapshot source={source} padVisible={padVisible} padActive={padActive} fg={DescribeWindow(foreground)} lossKind={_lastForegroundLossCounterpart} lossAt={LossTimestampText(_lastForegroundLoss)} lossAge={AgeText(foregroundLossAge)} padWasForeground={padWasForeground}");
        // Keep the loss available through the decision; only QueueTaskbarClick consumes it.
        _padWasForegroundBeforeProxyActivation = false;
    }

    private bool IsSnapshotStale() => !_clickSnapshotAvailable || Stopwatch.GetElapsedTime(_clickSnapshot.Timestamp).TotalMilliseconds > 1000;

    private void QueueTaskbarClick(IntPtr previousForeground, string source, bool explicitRestore = false)
    {
        if (_disposed || !_shown)
        {
            if (Program.IsDev) TaskbarPresence.Trace($"decision skipped source={source} disposed={_disposed} proxyShown={_shown}");
            return;
        }
        if (_activationPending)
        {
            if (Program.IsDev) TaskbarPresence.Trace($"decision duplicate-suppressed source={source} reason=activation-pending");
            return;
        }
        if (_clickSnapshotConsumed && !explicitRestore && ShouldSuppressPostHideFallback(source)) return;
        if (!_clickSnapshotAvailable || _clickSnapshotConsumed || IsSnapshotStale())
            StartClickSnapshot(source, fresh: true);

        var now = Stopwatch.GetTimestamp();
        var foreground = GetForegroundWindow();
        var padHandle = PadHandle;
        var padVisible = _pad.IsVisible;
        var lossAge = Program.IsDev ? LossAgeMilliseconds(now) : null;
        var recentlyLost = TaskbarPresence.WasPadRecentlyForeground(_lastForegroundLoss, now, _lastForegroundLossCounterpart);
        var padWasForeground = (_clickSnapshot.PadVisible && _clickSnapshot.PadWasForeground)
            || (padVisible && padHandle != IntPtr.Zero && previousForeground == padHandle)
            || (padVisible && padHandle != IntPtr.Zero && foreground == padHandle)
            || (padVisible && _pad.IsActive)
            || (padVisible && recentlyLost);
        var hide = TaskbarPresence.ShouldHideOnClick(padVisible, padWasForeground);
        var decision = hide ? "hide" : padVisible ? "focus" : "show";
        var snapshotAge = Program.IsDev ? Stopwatch.GetElapsedTime(_clickSnapshot.Timestamp, now).TotalMilliseconds : 0;
        if (Program.IsDev) TaskbarPresence.Trace($"decision source={source} result={decision} padVisible={padVisible} padActive={_pad.IsActive} fg={DescribeWindow(foreground)} previousForeground={DescribeWindow(previousForeground)} lossKind={_lastForegroundLossCounterpart} lossAt={LossTimestampText(_lastForegroundLoss)} lossAge={AgeText(lossAge)} recentTaskbarLoss={recentlyLost} earlySnapshotPadVisible={_clickSnapshot.PadVisible} earlySnapshotPadActive={_clickSnapshot.PadActive} earlySnapshotForeground={FormatHwnd(_clickSnapshot.Foreground)} earlySnapshotClass={_clickSnapshot.ForegroundClass} earlySnapshotLossKind={_clickSnapshot.ForegroundLossKind} earlySnapshotLossAt={LossTimestampText(_clickSnapshot.ForegroundLossTimestamp)} earlySnapshotLossAge={AgeText(_clickSnapshot.ForegroundLossAgeMilliseconds)} earlySnapshotPadWasForeground={_clickSnapshot.PadWasForeground} earlySnapshotAge={snapshotAge:F1}ms");
        _lastForegroundLoss = 0;
        _lastForegroundLossCounterpart = TaskbarActivationCounterpart.Other;

        _clickSnapshotConsumed = true;
        _lastDecisionWasHide = hide;
        _lastDecisionAt = now;
        _activationPending = true;
        var presenceGeneration = _presenceGeneration;
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                if (_disposed || !_shown || presenceGeneration != _presenceGeneration)
                {
                    if (Program.IsDev) TaskbarPresence.Trace($"decision dispatch discarded source={source} disposed={_disposed} proxyShown={_shown} generation={presenceGeneration}/{_presenceGeneration}");
                    return;
                }
                if (Program.IsDev) TaskbarPresence.Trace($"decision dispatch source={source} result={decision} padVisible={_pad.IsVisible}");
                if (hide) _lastDecisionAt = Stopwatch.GetTimestamp();
                _taskbarClick(hide, source);
            }
            finally
            {
                // Avalonia's Activated fallback can follow the native message in the same dispatcher turn.
                Dispatcher.UIThread.Post(() => _activationPending = false, DispatcherPriority.Background);
            }
        }, DispatcherPriority.Input);
    }

    private double? LossAgeMilliseconds(long now) =>
        _lastForegroundLoss > 0 && now >= _lastForegroundLoss
            ? Stopwatch.GetElapsedTime(_lastForegroundLoss, now).TotalMilliseconds
            : null;

    private static string LossTimestampText(long timestamp) => timestamp == 0 ? "none" : $"{TaskbarPresence.SinceTraceStart(timestamp):F1}ms";
    private static string AgeText(double? age) => age is { } value ? $"{value:F1}ms" : "na";
    private static string ActivationName(int value) => value switch { WaInactive => "WA_INACTIVE", WaActive => "WA_ACTIVE", WaClickActive => "WA_CLICKACTIVE", _ => $"0x{value:X}" };
    private static string FormatHwnd(IntPtr hwnd) => $"0x{hwnd.ToInt64():X}";

    private static string DescribeWindow(IntPtr hwnd) => hwnd == IntPtr.Zero
        ? "0x0/(none)"
        : $"{FormatHwnd(hwnd)}/{GetWindowClass(hwnd)}";

    private static string GetWindowClass(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero) return "none";
        var className = new StringBuilder(128);
        var length = GetClassName(hwnd, className, className.Capacity);
        return length > 0 ? className.ToString() : "unknown";
    }

    private static bool TryGetWindowPosFlags(IntPtr lParam, out uint flags)
    {
        flags = 0;
        if (lParam == IntPtr.Zero) return false;
        try
        {
            flags = Marshal.PtrToStructure<WindowPos>(lParam).Flags;
            return true;
        }
        catch { return false; }
    }

    private static string WindowPosFlagNames(uint flags)
    {
        var names = new List<string>();
        if ((flags & 0x0001) != 0) names.Add("SWP_NOSIZE");
        if ((flags & 0x0002) != 0) names.Add("SWP_NOMOVE");
        if ((flags & 0x0004) != 0) names.Add("SWP_NOZORDER");
        if ((flags & 0x0008) != 0) names.Add("SWP_NOREDRAW");
        if ((flags & 0x0010) != 0) names.Add("SWP_NOACTIVATE");
        if ((flags & 0x0020) != 0) names.Add("SWP_FRAMECHANGED");
        if ((flags & 0x0040) != 0) names.Add("SWP_SHOWWINDOW");
        if ((flags & 0x0080) != 0) names.Add("SWP_HIDEWINDOW");
        if ((flags & 0x0100) != 0) names.Add("SWP_NOCOPYBITS");
        if ((flags & 0x0200) != 0) names.Add("SWP_NOOWNERZORDER");
        if ((flags & 0x0400) != 0) names.Add("SWP_NOSENDCHANGING");
        if ((flags & 0x2000) != 0) names.Add("SWP_DEFERERASE");
        if ((flags & 0x4000) != 0) names.Add("SWP_ASYNCWINDOWPOS");
        return names.Count == 0 ? "none" : string.Join("|", names);
    }

    private readonly record struct ClickSnapshot(long Timestamp, bool PadVisible, bool PadActive, IntPtr Foreground,
        string ForegroundClass, long ForegroundLossTimestamp, TaskbarActivationCounterpart ForegroundLossKind,
        double? ForegroundLossAgeMilliseconds, bool PadWasForeground);

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowPos
    {
        public IntPtr Hwnd;
        public IntPtr HwndInsertAfter;
        public int X;
        public int Y;
        public int Cx;
        public int Cy;
        public uint Flags;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (Program.IsDev) TaskbarPresence.Trace($"proxy dispose shown={_shown}");
        if (_shown) NativeShowWindow(Handle, SwHide);
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
