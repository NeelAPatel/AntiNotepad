using System.Text.Json;
using System.Text.Json.Serialization;

namespace AntiNotepad.Model;

internal enum BodyReadStatus { Available, Missing, Unreadable }

internal readonly record struct BodyReadResult(string? Text, BodyReadStatus Status);

internal sealed record SearchNoteSnapshot(string Id, string Title, bool TitleKnown, DateTime Modified, long Revision, string? InMemoryText, bool BodyUnavailable);

internal sealed record SearchMatchResult(string Id, string Title, string Snippet, NoteMode Mode, string? ModeArgument, BodyReadStatus BodyStatus);

internal sealed class SearchContext
{
    internal SearchNoteSnapshot[] Notes { get; }
    internal string BodyFolder { get; }
    internal SearchBodyCache Cache { get; }
    internal long CacheEpoch { get; }

    internal SearchContext(SearchNoteSnapshot[] notes, string bodyFolder, SearchBodyCache cache, long cacheEpoch)
    {
        Notes = notes;
        BodyFolder = bodyFolder;
        Cache = cache;
        CacheEpoch = cacheEpoch;
    }
}

internal sealed class SearchBodyCache
{
    internal readonly object Gate = new();
    internal readonly Dictionary<string, SearchBodyCacheEntry> Entries = new(StringComparer.Ordinal);
    internal int ReadCount;
    internal long Epoch;
}

internal readonly record struct SearchBodyCacheEntry(DateTime Modified, long Revision, string Text);

/// <summary>Current-query guard shared by the overlay and headless stale-result checks.</summary>
internal sealed class SearchGeneration : IDisposable
{
    private readonly object _gate = new();
    private long _generation;
    private CancellationTokenSource? _current;
    private bool _disposed;

    internal SearchRequest Begin()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _current?.Cancel();
            var source = new CancellationTokenSource();
            _current = source;
            return new SearchRequest(this, ++_generation, source);
        }
    }

    internal void Cancel()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _generation++;
            _current?.Cancel();
            _current = null;
        }
    }

    internal bool IsCurrent(SearchRequest request)
    {
        lock (_gate) return !_disposed && _generation == request.Generation && ReferenceEquals(_current, request.Source) && !request.Token.IsCancellationRequested;
    }

    internal void Release(SearchRequest request)
    {
        lock (_gate)
        {
            if (ReferenceEquals(_current, request.Source))
            {
                _current = null;
                _generation++;
            }
            request.Source.Dispose();
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            _generation++;
            _current?.Cancel();
            _current = null;
        }
    }
}

internal sealed class SearchRequest : IDisposable
{
    private bool _disposed;
    internal SearchGeneration Owner { get; }
    internal long Generation { get; }
    internal CancellationTokenSource Source { get; }
    internal CancellationToken Token => Source.Token;

    internal SearchRequest(SearchGeneration owner, long generation, CancellationTokenSource source)
    {
        Owner = owner;
        Generation = generation;
        Source = source;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Owner.Release(this);
    }
}
