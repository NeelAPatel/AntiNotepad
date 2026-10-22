using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Threading;

namespace Antipad.Model;

/// <summary>Countdowns for timer notes. Keyed by note id + line text, so edits elsewhere do not reset a running timer.</summary>
public sealed class Timers
{
    public sealed class Entry
    {
        public required string Key { get; init; }
        public required TimeSpan Duration { get; init; }
        public required string Label { get; init; }
        public DateTime? EndsAt { get; set; }
        public TimeSpan? Paused { get; set; }
        public bool Done { get; set; }
        public TimeSpan Remaining => Done ? TimeSpan.Zero : Paused ?? (EndsAt is { } end ? end - DateTime.Now : Duration);
        public bool Running => EndsAt is not null && Paused is null && !Done;
    }

    private static readonly Regex Clock = new(@"^\s*(?:(\d{1,2}):)?(\d{1,3}):(\d{2})\b\s*(.*)$", RegexOptions.Compiled);
    private static readonly Regex Parts = new(@"(\d+(?:\.\d+)?)\s*(h|hr|hrs|hours?|m|min|mins|minutes?|s|sec|secs|seconds?)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly Dictionary<string, Entry> _entries = [];
    private readonly DispatcherTimer _tick = new() { Interval = TimeSpan.FromMilliseconds(250) };
    public event Action? Tick;
    public event Action<Entry>? Finished;

    public Timers() => _tick.Tick += (_, _) => OnTick();

    public static bool TryParse(string line, out TimeSpan duration, out string label)
    {
        duration = default; label = "";
        var text = line.Trim().TrimStart('-', '*', ' ');
        if (text.Length == 0) return false;
        var clock = Clock.Match(text);
        if (clock.Success)
        {
            var h = clock.Groups[1].Success ? int.Parse(clock.Groups[1].Value) : 0;
            duration = new TimeSpan(h, int.Parse(clock.Groups[2].Value), int.Parse(clock.Groups[3].Value));
            label = clock.Groups[4].Value.Trim();
            return duration > TimeSpan.Zero;
        }
        var total = 0.0; var consumed = 0; var first = -1;
        foreach (Match m in Parts.Matches(text))
        {
            // Duration parts must lead the line (e.g. "1h 30m tea"), not appear mid-sentence.
            if (first < 0) { if (m.Index != 0) break; first = m.Index; }
            else if (text[consumed..m.Index].Trim().Length > 0) break;
            var v = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            var u = m.Groups[2].Value.ToLowerInvariant();
            total += u.StartsWith('h') ? v * 3600 : u.StartsWith('m') ? v * 60 : v;
            consumed = m.Index + m.Length;
        }
        if (first < 0 || total <= 0) return false;
        duration = TimeSpan.FromSeconds(total);
        label = text[consumed..].Trim();
        return true;
    }

    public Entry? Get(string key) => _entries.GetValueOrDefault(key);

    public Entry Ensure(string key, TimeSpan duration, string label)
    {
        if (_entries.TryGetValue(key, out var e) && e.Duration == duration) return e;
        e = new Entry { Key = key, Duration = duration, Label = label };
        _entries[key] = e;
        return e;
    }

    public void Toggle(Entry e)
    {
        if (e.Done) { e.Done = false; e.EndsAt = null; e.Paused = null; }
        else if (e.EndsAt is null) e.EndsAt = DateTime.Now + e.Duration;
        else if (e.Paused is { } left) { e.EndsAt = DateTime.Now + left; e.Paused = null; }
        else e.Paused = e.EndsAt - DateTime.Now;
        Sync(); Tick?.Invoke();
    }

    public void Start(Entry e) { if (!e.Running) { e.Done = false; e.Paused = null; e.EndsAt = DateTime.Now + e.Duration; Sync(); } }

    public bool AnyRunning => _entries.Values.Any(e => e.Running);

    private void Sync() { if (AnyRunning) _tick.Start(); else _tick.Stop(); }

    private void OnTick()
    {
        foreach (var e in _entries.Values)
        {
            if (e.Running && e.EndsAt <= DateTime.Now) { e.Done = true; Finished?.Invoke(e); }
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
