using System.Collections.Concurrent;

namespace AntiNotepad;

/// <summary>Owns the app-home log destination and its best-effort asynchronous writer.</summary>
internal sealed class RuntimeLog : IDisposable
{
    private readonly object _gate = new();
    private string _home = "";
    private BackgroundLog? _logger;

    internal void SetHome(string home)
    {
        lock (_gate) _home = home;
    }

    internal void Write(string line)
    {
        try
        {
            BackgroundLog logger;
            lock (_gate)
            {
                if (string.IsNullOrWhiteSpace(_home)) return;
                _logger ??= new BackgroundLog(Path.Combine(_home, AppInfo.LogFileName));
                logger = _logger;
            }
            logger.Write(line);
        }
        catch { }
    }

    internal void Flush()
    {
        BackgroundLog? logger;
        lock (_gate) logger = _logger;
        try { logger?.Flush(); } catch { }
    }

    internal void LogCrash(object? error)
    {
        Write("crash " + error);
        Flush();
    }

    public void Dispose()
    {
        BackgroundLog? logger;
        lock (_gate) logger = _logger;
        try { logger?.Dispose(); } catch { }
    }
}

/// <summary>Serializes best-effort product log writes on one background worker.</summary>
internal sealed class BackgroundLog : IDisposable
{
    private sealed record Entry(string? Line, TaskCompletionSource? Barrier);

    private readonly string _path;
    private readonly object _gate = new();
    private readonly BlockingCollection<Entry> _queue = new(new ConcurrentQueue<Entry>());
    private readonly Task _worker;
    private bool _disposed;

    internal BackgroundLog(string path)
    {
        _path = Path.GetFullPath(path);
        _worker = Task.Run(Consume);
    }

    internal void Write(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        lock (_gate)
        {
            if (_disposed) return;
            // Capture the timestamp here so it describes when the event happened, not when disk I/O runs.
            _queue.Add(new Entry($"{DateTime.Now:s} {line}", null));
        }
    }

    /// <summary>Waits until every write queued before this call has been attempted.</summary>
    internal void Flush()
    {
        var barrier = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task worker;
        var queued = false;
        lock (_gate)
        {
            worker = _worker;
            if (!_disposed)
            {
                _queue.Add(new Entry(null, barrier));
                queued = true;
            }
        }
        if (queued) barrier.Task.GetAwaiter().GetResult();
        else worker.GetAwaiter().GetResult();
    }

    private void Consume()
    {
        foreach (var entry in _queue.GetConsumingEnumerable())
        {
            if (entry.Barrier is { } barrier)
            {
                barrier.TrySetResult();
                continue;
            }

            try
            {
                var directory = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                // Match the existing product log rotation: reset the file once it has passed 64 KB.
                if (File.Exists(_path) && new FileInfo(_path).Length > 64 * 1024) File.Delete(_path);
                File.AppendAllText(_path, entry.Line + Environment.NewLine);
            }
            catch { }
        }
    }

    public void Dispose()
    {
        Task worker;
        lock (_gate)
        {
            if (!_disposed)
            {
                _disposed = true;
                _queue.CompleteAdding();
            }
            worker = _worker;
        }
        worker.GetAwaiter().GetResult();
    }
}
