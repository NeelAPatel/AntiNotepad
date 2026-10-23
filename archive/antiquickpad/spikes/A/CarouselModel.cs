using System.Text.Json;

namespace InputProbe;

internal sealed class Note
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Text { get; set; } = "";
}

internal sealed class NoteStore(string path)
{
    public string Path { get; } = path;
    public List<Note> Load()
    {
        try
        {
            if (!File.Exists(Path)) return [new Note { Text = Fixture }];
            var notes = JsonSerializer.Deserialize<List<Note>>(File.ReadAllText(Path));
            return notes is { Count: > 0 } ? notes : [new Note { Text = Fixture }];
        }
        catch { return [new Note { Text = Fixture }]; }
    }

    public void Save(IReadOnlyList<Note> notes)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
        var clean = notes.Select(n => new Note { Id = n.Id, Text = n.Text }).ToList();
        var temp = Path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(clean, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temp, Path, true);
    }

    public static string Fixture => "# AntiQuickPad spike\n\n## A focused scratchpad\n\nVisible **bold**, *italic*, and ~~strike~~ markers remain editable.\n\n- [ ] Click the checkbox marker to toggle it (undo with Ctrl+Z)\n- [x] Keep notes local\n\n```python\ndef quick_note():\n    return 'wrapped and highlighted'\n```\n\nDrag the grab strip to move notes directly. One horizontal wheel burst switches one note.";
}

internal static class CarouselMath
{
    public static double DragDelta(double origin, double current) => current - origin;
    public static bool ShouldCreateNote(int index, int count, double displacement) => count > 0 && index == count - 1 && displacement < -170;
    public static double RubberBand(double offset, double limit = 180) => Math.Sign(offset) * limit * (1 - Math.Exp(-Math.Abs(offset) / limit));
    public static int SnapIndex(int current, double displacement, double velocityPxPerMs, int count, bool wheel = false)
    {
        if (count < 1) return 0;
        var threshold = wheel ? 60d : 170d;
        var direction = displacement < -threshold || velocityPxPerMs < -.55 ? 1 : displacement > threshold || velocityPxPerMs > .55 ? -1 : 0;
        return Math.Clamp(current + direction, 0, count - 1);
    }
    public static List<Note> PruneInteriorBlanks(IReadOnlyList<Note> notes)
    {
        var result = notes.Where((n, i) => !string.IsNullOrWhiteSpace(n.Text) || i == notes.Count - 1).ToList();
        return result.Count == 0 ? [new Note()] : result;
    }
}

internal readonly record struct WheelBurstResult(int TargetIndex, bool CreateAtNewestEdge, int BurstNumber);

// Treat a burst as one page request. A quick reversal changes the requested
// destination for that same burst instead of queueing another page animation.
internal sealed class WheelBurstState
{
    public const int IdleGapMs = 140;
    private int _startIndex;
    private int _count;
    private int _direction;
    private long _lastEventMs;
    private int _burstNumber;
    public bool IsActive { get; private set; }
    public long LastEventMs => _lastEventMs;
    public int BurstNumber => _burstNumber;

    public WheelBurstResult Add(int settledIndex, int noteCount, double delta, long nowMs, bool transitionInFlight = false)
    {
        if (Math.Abs(delta) < .001 || noteCount < 1) return new(settledIndex, false, _burstNumber);
        if (!IsActive || (!transitionInFlight && nowMs - _lastEventMs > IdleGapMs))
        {
            _startIndex = Math.Clamp(settledIndex, 0, noteCount - 1);
            _count = noteCount;
            _direction = 0;
            _burstNumber++;
            IsActive = true;
        }

        _direction = Math.Sign(delta);
        _lastEventMs = nowMs;
        var requested = _startIndex + _direction;
        return new(Math.Clamp(requested, 0, _count - 1), requested >= _count, _burstNumber);
    }

    public bool IsExpired(long nowMs, bool transitionInFlight = false) => IsActive && !transitionInFlight && nowMs - _lastEventMs > IdleGapMs;

    public void Reset() => IsActive = false;

    public WheelBurstResult Complete()
    {
        var requested = _startIndex + _direction;
        var result = new WheelBurstResult(Math.Clamp(requested, 0, Math.Max(0, _count - 1)), requested >= _count, _burstNumber);
        IsActive = false;
        return result;
    }
}
