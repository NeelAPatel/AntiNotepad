// Owns live preference changes and classifies their effects for subscribers.
using System.Reflection;

namespace AntiNotepad.Model;

[Flags]
internal enum SettingsChangeFlags
{
    None = 0,
    Appearance = 1 << 0,
    Palette = 1 << 1,
    EditorLook = 1 << 2,
    Analysis = 1 << 3,
    Motion = 1 << 4,
    Caret = 1 << 5,
    Chrome = 1 << 6,
    Hotkeys = 1 << 7,
    WindowPolicy = 1 << 8,
    Taskbar = 1 << 9,
    Geometry = 1 << 10,
    Split = 1 << 11,
    Search = 1 << 12,
    Persistence = 1 << 13,
    Shortcuts = 1 << 14,
    All = (1 << 15) - 1,
}

internal sealed record SettingsChange(SettingsChangeFlags Flags, IReadOnlyList<string> Properties);

/// <summary>
/// Owns the live settings object. Updates are UI-thread-only; saves receive detached copies from Store.
/// </summary>
internal sealed class SettingsService
{
    private sealed record Field(
        string Name,
        SettingsChangeFlags Flags,
        Func<Settings, object?> Capture,
        Func<object?, object?, bool>? Equal = null)
    {
        public bool Changed(object? before, object? after) => Equal is not null ? !Equal(before, after) : !Equals(before, after);
    }

    private const SettingsChangeFlags Appearance = SettingsChangeFlags.Appearance;
    private const SettingsChangeFlags Persistence = SettingsChangeFlags.Persistence;

    // This is the single property-to-flags table. Unlisted writable properties use Appearance|Persistence.
    private static readonly Field[] PropertyMap =
    [
        new(nameof(Settings.Theme), Appearance | SettingsChangeFlags.Palette | Persistence, s => s.Theme),
        new(nameof(Settings.LightPalette), Appearance | SettingsChangeFlags.Palette | Persistence, s => s.LightPalette),
        new(nameof(Settings.DarkPalette), Appearance | SettingsChangeFlags.Palette | Persistence, s => s.DarkPalette),
        new(nameof(Settings.Accent), Appearance | SettingsChangeFlags.Palette | Persistence, s => s.Accent),
        new(nameof(Settings.Tone), Appearance | SettingsChangeFlags.Palette | Persistence, s => s.Tone),
        new(nameof(Settings.SplitPalette), Appearance | SettingsChangeFlags.Palette | Persistence, s => s.SplitPalette),
        new(nameof(Settings.Material), Appearance | Persistence, s => s.Material),
        new(nameof(Settings.Tint), Appearance | Persistence, s => s.Tint),
        new(nameof(Settings.UiScale), Appearance | Persistence, s => s.UiScale),
        new(nameof(Settings.Font), Appearance | SettingsChangeFlags.EditorLook | Persistence, s => s.Font),
        new(nameof(Settings.FontSize), Appearance | SettingsChangeFlags.EditorLook | Persistence, s => s.FontSize),
        new(nameof(Settings.TextWidth), Appearance | SettingsChangeFlags.EditorLook | Persistence, s => s.TextWidth),
        new(nameof(Settings.LineSpacing), Appearance | SettingsChangeFlags.EditorLook | Persistence, s => s.LineSpacing),
        new(nameof(Settings.Paper), Appearance | SettingsChangeFlags.EditorLook | Persistence, s => s.Paper),
        new(nameof(Settings.PaperStrength), Appearance | SettingsChangeFlags.EditorLook | Persistence, s => s.PaperStrength),
        new(nameof(Settings.Bubbles), Appearance | SettingsChangeFlags.EditorLook | Persistence, s => s.Bubbles),
        new(nameof(Settings.InlineMath), Appearance | SettingsChangeFlags.Analysis | Persistence, s => s.InlineMath),
        new(nameof(Settings.ShortenLinks), Appearance | SettingsChangeFlags.Analysis | Persistence, s => s.ShortenLinks),
        new(nameof(Settings.CleanMarkdown), Appearance | SettingsChangeFlags.Analysis | Persistence, s => s.CleanMarkdown),
        new(nameof(Settings.IndentGuides), Appearance | SettingsChangeFlags.Analysis | Persistence, s => s.IndentGuides),
        new(nameof(Settings.Folding), Appearance | SettingsChangeFlags.Analysis | Persistence, s => s.Folding),
        new(nameof(Settings.DisabledLanguages), Appearance | SettingsChangeFlags.Analysis | Persistence,
            s => s.DisabledLanguages is null ? Array.Empty<string>() : s.DisabledLanguages.ToArray(),
            (a, b) => ((string[])a!).SequenceEqual((string[])b!, StringComparer.Ordinal)),
        new(nameof(Settings.CaretStyle), Appearance | SettingsChangeFlags.Caret | Persistence, s => s.CaretStyle),
        new(nameof(Settings.CaretBlink), Appearance | SettingsChangeFlags.Caret | Persistence, s => s.CaretBlink),
        new(nameof(Settings.CaretGlide), Appearance | SettingsChangeFlags.Caret | Persistence, s => s.CaretGlide),
        new(nameof(Settings.ReduceMotion), Appearance | SettingsChangeFlags.Motion | Persistence, s => s.ReduceMotion),
        new(nameof(Settings.Scrolling), Appearance | SettingsChangeFlags.Motion | Persistence, s => s.Scrolling),
        new(nameof(Settings.LinesPerNotch), Appearance | SettingsChangeFlags.Motion | Persistence, s => s.LinesPerNotch),
        new(nameof(Settings.ScrollDistance), Appearance | SettingsChangeFlags.Motion | Persistence, s => s.ScrollDistance),
        new(nameof(Settings.ScrollBoost), Appearance | SettingsChangeFlags.Motion | Persistence, s => s.ScrollBoost),
        new(nameof(Settings.FreeFriction), Appearance | SettingsChangeFlags.Motion | Persistence, s => s.FreeFriction),
        new(nameof(Settings.SwipeLength), Appearance | SettingsChangeFlags.Motion | Persistence, s => s.SwipeLength),
        new(nameof(Settings.SwipeFiresAt), Appearance | SettingsChangeFlags.Motion | Persistence, s => s.SwipeFiresAt),
        new(nameof(Settings.Swipe), Appearance | SettingsChangeFlags.Motion | Persistence, s => s.Swipe),
        new(nameof(Settings.Chrome), Appearance | SettingsChangeFlags.Chrome | Persistence, s => s.Chrome),
        new(nameof(Settings.FadeAfterMs), Appearance | SettingsChangeFlags.Chrome | Persistence, s => s.FadeAfterMs),
        new(nameof(Settings.Bars), Appearance | SettingsChangeFlags.Chrome | Persistence, s => s.Bars),
        new(nameof(Settings.EdgeFade), Appearance | SettingsChangeFlags.Chrome | Persistence, s => s.EdgeFade),
        new(nameof(Settings.MenuActionRow), Appearance | SettingsChangeFlags.Chrome | Persistence, s => s.MenuActionRow),
        new(nameof(Settings.MenuColorIcons), Appearance | SettingsChangeFlags.Chrome | Persistence, s => s.MenuColorIcons),
        new(nameof(Settings.Hotkey), Appearance | SettingsChangeFlags.Hotkeys | Persistence, s => s.Hotkey),
        new(nameof(Settings.NewNoteHotkey), Appearance | SettingsChangeFlags.Hotkeys | Persistence, s => s.NewNoteHotkey),
        new(nameof(Settings.ShortcutOverrides), SettingsChangeFlags.Shortcuts | Persistence,
            s => s.ShortcutOverrides is null ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(s.ShortcutOverrides, StringComparer.Ordinal),
            (a, b) => ((Dictionary<string, string>)a!).Count == ((Dictionary<string, string>)b!).Count
                && ((Dictionary<string, string>)a!).All(pair => ((Dictionary<string, string>)b!).TryGetValue(pair.Key, out var value)
                    && string.Equals(pair.Value, value, StringComparison.Ordinal))),
        new(nameof(Settings.FreshNoteAfterMinutes), Appearance | SettingsChangeFlags.WindowPolicy | Persistence, s => s.FreshNoteAfterMinutes),
        new(nameof(Settings.SaveDelayMs), Persistence, s => s.SaveDelayMs),
        new(nameof(Settings.ListDefault), Persistence, s => s.ListDefault),
        new(nameof(Settings.HideOnBlur), Appearance | SettingsChangeFlags.WindowPolicy | Persistence, s => s.HideOnBlur),
        new(nameof(Settings.DimWhenPinned), Appearance | SettingsChangeFlags.WindowPolicy | Persistence, s => s.DimWhenPinned),
        new(nameof(Settings.OpenAt), Appearance | SettingsChangeFlags.WindowPolicy | Persistence, s => s.OpenAt),
        new(nameof(Settings.Pinned), Appearance | SettingsChangeFlags.WindowPolicy | Persistence, s => s.Pinned),
        new(nameof(Settings.Taskbar), Appearance | SettingsChangeFlags.Taskbar | Persistence, s => s.Taskbar),
        new(nameof(Settings.Width), SettingsChangeFlags.Geometry | Persistence, s => s.Width),
        new(nameof(Settings.Height), SettingsChangeFlags.Geometry | Persistence, s => s.Height),
        new(nameof(Settings.X), SettingsChangeFlags.Geometry | Persistence, s => s.X),
        new(nameof(Settings.Y), SettingsChangeFlags.Geometry | Persistence, s => s.Y),
        new(nameof(Settings.Split), SettingsChangeFlags.Split | Persistence, s => s.Split),
        new(nameof(Settings.SplitSlot), SettingsChangeFlags.Split | Persistence, s => s.SplitSlot),
        new(nameof(Settings.SearchGrid), SettingsChangeFlags.Search | Persistence, s => s.SearchGrid),
        new(nameof(Settings.Welcomed), Persistence, s => s.Welcomed),
    ];
    private static readonly Field[] Fields = BuildFields();

    private readonly int _ownerThreadId = Environment.CurrentManagedThreadId;
    private readonly Action<Settings>? _replaced;
    private readonly Queue<Action> _deferred = new();
    private readonly Queue<SettingsChange> _pending = new();
    private int _batchDepth;
    private bool _dispatching;
    private bool _draining;
    private bool _replacedInBatch;
    private object?[]? _batchBefore;

    internal Settings Current { get; private set; }
    internal event Action<SettingsChange>? Changed;

    internal SettingsService(Settings current, Action<Settings>? replaced = null)
    {
        Current = current ?? throw new ArgumentNullException(nameof(current));
        _replaced = replaced;
    }

    internal void Update(Action<Settings> change)
    {
        ArgumentNullException.ThrowIfNull(change);
        AssertOwnerThread();
        if (_dispatching)
        {
            _deferred.Enqueue(() => RunUpdate(change));
            return;
        }
        RunUpdate(change);
    }

    internal void SetLanguageEnabled(string language, bool enabled) => Update(settings =>
    {
        if (enabled) settings.DisabledLanguages.Remove(language);
        else if (!settings.DisabledLanguages.Contains(language)) settings.DisabledLanguages.Add(language);
    });

    internal void Replace(Settings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        AssertOwnerThread();
        if (_dispatching)
        {
            _deferred.Enqueue(() => ReplaceCore(settings));
            return;
        }
        ReplaceCore(settings);
    }

    private void RunUpdate(Action<Settings> change)
    {
        var outer = _batchDepth == 0;
        if (outer)
        {
            _batchBefore = Capture(Current);
            _replacedInBatch = false;
        }

        _batchDepth++;
        try { change(Current); }
        finally
        {
            _batchDepth--;
            if (outer) CompleteBatch();
        }
    }

    private void ReplaceCore(Settings settings)
    {
        Current = settings;
        _replaced?.Invoke(settings);
        if (_batchDepth > 0) _replacedInBatch = true;
        else _pending.Enqueue(AllChange());
        Drain();
    }

    private void CompleteBatch()
    {
        var change = _replacedInBatch ? AllChange() : Diff(_batchBefore!, Current);
        _batchBefore = null;
        _replacedInBatch = false;
        if (change is not null) _pending.Enqueue(change);
        Drain();
    }

    private void Drain()
    {
        if (_draining || _batchDepth > 0) return;
        _draining = true;
        try
        {
            while (_pending.Count > 0 || _deferred.Count > 0)
            {
                if (_pending.TryDequeue(out var change))
                {
                    _dispatching = true;
                    try { Changed?.Invoke(change); }
                    finally { _dispatching = false; }
                    continue;
                }

                _batchBefore = Capture(Current);
                _replacedInBatch = false;
                _batchDepth = 1;
                try
                {
                    while (_deferred.TryDequeue(out var operation)) operation();
                }
                finally
                {
                    _batchDepth = 0;
                    var followup = _replacedInBatch ? AllChange() : Diff(_batchBefore!, Current);
                    _batchBefore = null;
                    _replacedInBatch = false;
                    if (followup is not null) _pending.Enqueue(followup);
                }
            }
        }
        finally { _draining = false; }
    }

    private static object?[] Capture(Settings settings) => Fields.Select(field => field.Capture(settings)).ToArray();

    private static Field[] BuildFields()
    {
        var mapped = PropertyMap.Select(field => field.Name).ToHashSet(StringComparer.Ordinal);
        var fallback = typeof(Settings).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.CanRead && property.SetMethod is not null && !mapped.Contains(property.Name))
            .Select(property =>
            {
                if (property.PropertyType == typeof(List<string>))
                    return new Field(property.Name, Appearance | Persistence,
                        settings => ((List<string>?)property.GetValue(settings))?.ToArray() ?? Array.Empty<string>(),
                        (a, b) => ((string[])a!).SequenceEqual((string[])b!, StringComparer.Ordinal));
                return new Field(property.Name, Appearance | Persistence, property.GetValue);
            });
        return PropertyMap.Concat(fallback).ToArray();
    }

    private static SettingsChange? Diff(object?[] before, Settings after)
    {
        var names = new List<string>();
        var flags = SettingsChangeFlags.None;
        for (var i = 0; i < Fields.Length; i++)
        {
            var value = Fields[i].Capture(after);
            if (!Fields[i].Changed(before[i], value)) continue;
            names.Add(Fields[i].Name);
            flags |= Fields[i].Flags;
        }
        return names.Count == 0 ? null : new SettingsChange(flags, names.AsReadOnly());
    }

    private static SettingsChange AllChange() => new(SettingsChangeFlags.All, Array.AsReadOnly(Fields.Select(field => field.Name).ToArray()));

    private void AssertOwnerThread()
    {
        if (Environment.CurrentManagedThreadId != _ownerThreadId)
            throw new InvalidOperationException("SettingsService is UI-thread only.");
    }
}
