using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Threading;

namespace AntiNotepad.Model;

/// <summary>Countdowns for timer notes. Keys follow a note's timer lines, so editing a label does not reset a running timer.</summary>
public sealed class Timers
{
    public sealed class Entry
    {
        public required string Key { get; init; }
        public required string NoteId { get; init; }
        public required TimeSpan Duration { get; init; }
        public required string Label { get; set; }
        public DateTime? EndsAt { get; set; }
        public TimeSpan? Paused { get; set; }
        public bool Done { get; set; }
        public TimeSpan Remaining => Done ? TimeSpan.Zero : Paused ?? (EndsAt is { } end ? end - DateTime.Now : Duration);
        public bool Running => EndsAt is not null && Paused is null && !Done;
    }

    private static readonly Regex Clock = new(@"^\s*(?:([0-9]{1,3}):)?([0-9]{1,3}):([0-9]{2})\b\s*(.*)$", RegexOptions.Compiled);
    private static readonly Regex Parts = new(@"([0-9]+(?:\.[0-9]+)?)\s*(h|hr|hrs|hours?|m|min|mins|minutes?|s|sec|secs|seconds?)(?:\b|(?=[0-9]))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly TimeSpan MaxDuration = TimeSpan.FromSeconds(3_599_999);

    private readonly Dictionary<string, Entry> _entries = [];
    private readonly Dictionary<string, string> _completed = [];
    private readonly DispatcherTimer _tick = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private readonly Func<string, bool>? _isLive;
    public event Action? Tick;
    public event Action<Entry>? Finished;

    public Timers(Func<string, bool>? isLive = null)
    {
        _isLive = isLive;
        _tick.Tick += (_, _) => OnTick();
    }

    public static bool TryParse(string line, out TimeSpan duration, out string label)
        => TryParse(line, out duration, out label, out _);

    public static bool TryParse(string line, out TimeSpan duration, out string label, out string durationText)
    {
        duration = default; label = ""; durationText = "";
        var text = line.Trim().TrimStart('-', '*', ' ');
        if (text.Length == 0) return false;
        var clock = Clock.Match(text);
        if (clock.Success)
        {
            var h = 0;
            if ((clock.Groups[1].Success && !int.TryParse(clock.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out h))
                || !int.TryParse(clock.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var m)
                || !int.TryParse(clock.Groups[3].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var s)) return false;
            var seconds = h * 3600d + m * 60d + s;
            if (seconds <= 0 || seconds > MaxDuration.TotalSeconds) return false;
            duration = TimeSpan.FromSeconds(seconds);
            label = clock.Groups[4].Value.Trim();
            durationText = text[..clock.Groups[4].Index].Trim();
            return duration > TimeSpan.Zero;
        }
        var total = 0.0; var consumed = 0; var first = -1;
        foreach (Match m in Parts.Matches(text))
        {
            // Duration parts must lead the line (e.g. "1h 30m tea"), not appear mid-sentence.
            if (first < 0) { if (m.Index != 0) break; first = m.Index; }
            else if (text[consumed..m.Index].Trim().Length > 0) break;
            if (!double.TryParse(m.Groups[1].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var v)
                || !double.IsFinite(v)) return false;
            var u = m.Groups[2].Value.ToLowerInvariant();
            var seconds = u.StartsWith('h') ? v * 3600 : u.StartsWith('m') ? v * 60 : v;
            if (!double.IsFinite(seconds) || seconds > MaxDuration.TotalSeconds) return false;
            total += seconds;
            if (!double.IsFinite(total) || total > MaxDuration.TotalSeconds) return false;
            consumed = m.Index + m.Length;
        }
        if (first < 0 || total <= 0 || total > MaxDuration.TotalSeconds) return false;
        duration = TimeSpan.FromSeconds(total);
        label = text[consumed..].Trim();
        durationText = text[..consumed].Trim();
        return true;
    }

    public Entry? Get(string key) => _entries.GetValueOrDefault(key);

    public Entry Ensure(string key, TimeSpan duration, string label, string noteId = "")
    {
        if (_entries.TryGetValue(key, out var e) && e.Duration == duration)
        {
            e.Label = label;
            return e;
        }
        e = new Entry { Key = key, NoteId = noteId, Duration = duration, Label = label };
        _entries[key] = e;
        Sync();
        return e;
    }

    internal int Reconcile(string noteId, IReadOnlyCollection<string> currentKeys)
    {
        var keep = currentKeys as HashSet<string> ?? new HashSet<string>(currentKeys, StringComparer.Ordinal);
        int removed = 0;
        foreach (var (key, entry) in _entries.ToArray())
            if (entry.NoteId == noteId && !keep.Contains(key) && Remove(key)) removed++;
        foreach (var (key, owner) in _completed.ToArray())
            if (owner == noteId && !keep.Contains(key) && _completed.Remove(key)) removed++;
        removed += PruneDeadNotes();
        Sync();
        return removed;
    }

    internal int EntryCount => _entries.Count;
    internal bool IsDone(string key) => _completed.ContainsKey(key);
    internal bool ClearDone(string key) => _completed.Remove(key);

    private bool Remove(string key)
    {
        if (!_entries.Remove(key, out var entry)) return false;
        entry.EndsAt = null;
        entry.Paused = null;
        entry.Done = false;
        return true;
    }

    private int PruneDeadNotes()
    {
        if (_isLive is null) return 0;
        int removed = 0;
        foreach (var (key, entry) in _entries.ToArray())
            if (entry.NoteId.Length > 0 && !_isLive(entry.NoteId) && Remove(key)) removed++;
        foreach (var (key, noteId) in _completed.ToArray())
            if (noteId.Length > 0 && !_isLive(noteId) && _completed.Remove(key)) removed++;
        return removed;
    }

    public void Toggle(Entry e)
    {
        if (e.Done) { e.Done = false; e.EndsAt = null; e.Paused = null; }
        else if (e.EndsAt is null)
        {
            if (!TryEndAt(DateTime.Now, e.Duration, out var endsAt)) return;
            e.EndsAt = endsAt;
        }
        else if (e.Paused is { } left)
        {
            if (!TryEndAt(DateTime.Now, left, out var endsAt)) return;
            e.EndsAt = endsAt; e.Paused = null;
        }
        else e.Paused = e.EndsAt - DateTime.Now;
        Sync(); Tick?.Invoke();
    }

    public void Start(Entry e)
    {
        if (e.Running || !TryEndAt(DateTime.Now, e.Duration, out var endsAt)) return;
        e.Done = false; e.Paused = null; e.EndsAt = endsAt;
        Sync();
    }

    internal static bool TryEndAt(DateTime now, TimeSpan duration, out DateTime endsAt)
    {
        endsAt = default;
        if (duration.Ticks < 0 || duration.Ticks > DateTime.MaxValue.Ticks - now.Ticks) return false;
        endsAt = now.AddTicks(duration.Ticks);
        return true;
    }

    /// <summary>Stops this timer collection when its owner (the Settings preview) is released.</summary>
    public void Stop()
    {
        _tick.Stop();
        foreach (var entry in _entries.Values)
        {
            entry.EndsAt = null;
            entry.Paused = null;
            entry.Done = false;
        }
        _entries.Clear();
        _completed.Clear();
    }

    public bool AnyRunning => _entries.Values.Any(e => e.Running);

    private void Sync() { if (AnyRunning) _tick.Start(); else _tick.Stop(); }

    private void OnTick()
    {
        PruneDeadNotes();
        foreach (var e in _entries.Values.ToArray())
        {
            if (e.Running && e.EndsAt <= DateTime.Now)
            {
                e.Done = true;
                _completed[e.Key] = e.NoteId;
                _entries.Remove(e.Key);
                Finished?.Invoke(e);
            }
        }
        Sync(); Tick?.Invoke();
    }

    public static string Format(TimeSpan t)
    {
        if (t < TimeSpan.Zero) t = TimeSpan.Zero;
        var s = (int)Math.Ceiling(t.TotalSeconds);
        return s >= 3600 ? $"{s / 3600}:{s / 60 % 60:00}:{s % 60:00}" : $"{s / 60}:{s % 60:00}";
    }
}
