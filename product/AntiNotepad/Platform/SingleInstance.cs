using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Text;
using AntiNotepad;

namespace AntiNotepad.Platform;

/// <summary>Per-Windows-user process singleton with a signal for an already-running instance.</summary>
public sealed class SingleInstanceService : IDisposable
{
    private readonly Mutex _mutex;
    private readonly EventWaitHandle _showSignal;
    private readonly EventWaitHandle _quitSignal;
    private readonly MemoryMappedFile? _owner;
    private readonly Action? _onQuit;
    private readonly Action _onSecondLaunch;
    private readonly CancellationTokenSource _stop = new();
    private readonly Thread? _listener;
    private readonly int _ownerThreadId;
    private bool _disposed;

    public bool IsPrimary { get; }
    public bool WasAbandoned { get; }

    public SingleInstanceService(Action onSecondLaunch, string? applicationId = null, Action? onQuit = null)
    {
        _onSecondLaunch = onSecondLaunch ?? throw new ArgumentNullException(nameof(onSecondLaunch));
        applicationId ??= AppInfo.InstanceId;
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Single-instance coordination is currently supported on Windows only.");

        _onQuit = onQuit;
        var stem = ObjectStem(applicationId);
        _mutex = new Mutex(false, $"{stem}-Mutex");
        _showSignal = new EventWaitHandle(false, EventResetMode.AutoReset, $"{stem}-Show");
        _quitSignal = new EventWaitHandle(false, EventResetMode.AutoReset, $"{stem}-Quit");
        _ownerThreadId = Environment.CurrentManagedThreadId;

        try
        {
            try { IsPrimary = _mutex.WaitOne(0); }
            catch (AbandonedMutexException) { IsPrimary = true; WasAbandoned = true; }

            if (IsPrimary)
            {
                _owner = MemoryMappedFile.CreateOrOpen($"{stem}-Owner", 4);
                using (var data = _owner.CreateViewAccessor()) data.Write(0, Environment.ProcessId);
                _listener = new Thread(Listen) { IsBackground = true, Name = $"{applicationId} lifetime listener" };
                _listener.Start();
            }
            else _showSignal.Set();
        }
        catch
        {
            _owner?.Dispose(); _quitSignal.Dispose(); _showSignal.Dispose();
            if (IsPrimary) _mutex.ReleaseMutex();
            _mutex.Dispose();
            throw;
        }
    }

    private static string ObjectStem(string applicationId)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        // Local\ keeps objects within the current logon session; the SID-derived suffix separates users.
        var sid = System.Security.Principal.WindowsIdentity.GetCurrent().User?.Value
                  ?? $"{Environment.UserDomainName}\\{Environment.UserName}";
        var suffix = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sid)))[..20];
        return $"Local\\{applicationId}-{suffix}";
    }

    // This client opens only existing objects: it never claims residency or sends Show.
    public static int RequestQuit(string? applicationId = null)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        applicationId ??= AppInfo.InstanceId;
        var timer = Stopwatch.StartNew();
        var stem = ObjectStem(applicationId);
        try
        {
            if (!Mutex.TryOpenExisting($"{stem}-Mutex", out var mutex)) return 0;
            using (mutex)
            {
                bool idle;
                try { idle = mutex.WaitOne(0); }
                catch (AbandonedMutexException) { idle = true; }
                if (idle) { mutex.ReleaseMutex(); return 0; }
                // A primary may still be publishing its PID during startup.
                while (timer.ElapsedMilliseconds < 5000)
                {
                    try
                    {
                        using var owner = MemoryMappedFile.OpenExisting($"{stem}-Owner", MemoryMappedFileRights.Read);
                        using var data = owner.CreateViewAccessor(0, 4, MemoryMappedFileAccess.Read);
                        var pid = data.ReadInt32(0);
                        if (pid <= 0) { Thread.Sleep(10); continue; }
                        Process process;
                        try { process = Process.GetProcessById(pid); }
                        catch (ArgumentException) { return 0; }
                        using (process)
                        using (var quit = EventWaitHandle.OpenExisting($"{stem}-Quit"))
                        {
                            quit.Set();
                            if (process.WaitForExit(Math.Max(0, 5000 - (int)timer.ElapsedMilliseconds))) return 0;
                            break;
                        }
                    }
                    catch (FileNotFoundException) { Thread.Sleep(10); }
                    catch (WaitHandleCannotBeOpenedException) { Thread.Sleep(10); }
                }
            }
            Console.Error.WriteLine($"{AppInfo.Name} --quit: resident did not exit within 5 seconds.");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{AppInfo.Name} --quit: {ex.Message}"); return 1;
        }
    }

    private void Listen()
    {
        var handles = new WaitHandle[] { _showSignal, _quitSignal, _stop.Token.WaitHandle };
        while (true)
        {
            var signal = WaitHandle.WaitAny(handles);
            if (signal == 2 || _stop.IsCancellationRequested) break;
            try { if (signal == 1) _onQuit?.Invoke(); else _onSecondLaunch(); }
            catch { /* Owner callbacks cannot kill the lifetime listener. */ }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        if (IsPrimary && Environment.CurrentManagedThreadId != _ownerThreadId)
            throw new InvalidOperationException("Dispose the primary SingleInstanceService on the thread that created it, because that thread owns the named mutex.");

        _disposed = true;
        _stop.Cancel();
        _listener?.Join();
        _showSignal.Dispose();
        _quitSignal.Dispose();
        _owner?.Dispose();
        _stop.Dispose();
        if (IsPrimary) _mutex.ReleaseMutex();
        _mutex.Dispose();
    }
}
