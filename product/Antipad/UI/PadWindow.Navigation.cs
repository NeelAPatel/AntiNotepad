using Avalonia.Input.Platform;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Antipad.Editor;
using Antipad.Model;
using Antipad.Platform;

namespace Antipad.UI;

internal sealed partial class PadWindow
{
    private double _swipe;
    private DateTime _swipeAt;
    private bool _swipeFired;
    private bool _sizingForSplit;

    private Note LeftNote => Data.ViewSlot > 0 ? Data.Slots[Data.ViewSlot - 1] : Data.Stack[Data.Current];
    private Note RightNote => Data.Slots[Settings.SplitSlot - 1];

    /// <summary>Binds both panes to the current model selection.</summary>
    private void BindPanes(int arrive, bool focus = true)
    {
        _left.Bind(LeftNote);
        if (Settings.Split)
        {
            // Never show the same slot twice side by side.
            if (Data.ViewSlot == Settings.SplitSlot) Settings.SplitSlot = Settings.SplitSlot % 9 + 1;
            _right.Bind(RightNote);
        }
        _panes.ColumnDefinitions = new ColumnDefinitions(Settings.Split ? "*,1,*" : "*,0,0");
        _right.IsVisible = Settings.Split; _divider.IsVisible = Settings.Split;
        if (!Settings.Split && _active == _right) _active = _left;
        if (arrive != 0 && _active == _left) _left.Arrive(arrive);
        UpdateChrome();
        if (focus) FocusActive();
    }

    /// <summary>Leaving an empty stack note removes it: abandoned blanks never pile up.</summary>
    private bool DropIfAbandoned(Note leaving)
    {
        if (leaving.Slot != 0 || !leaving.IsEmpty || Data.Stack.Count <= 1) return false;
        var index = Data.Stack.IndexOf(leaving);
        if (index < 0) return false;
        Data.Stack.RemoveAt(index);
        PadView.Forget(leaving);
        if (Data.Current > index) Data.Current--;
        Data.Current = Math.Clamp(Data.Current, 0, Data.Stack.Count - 1);
        return true;
    }

    internal void Navigate(int direction)
    {
        if (_active == _right) { CycleSplitSlot(direction); return; }
        if (Data.ViewSlot > 0) { ShowStack(); return; }
        _left.Flush();
        var leaving = Data.Stack[Data.Current];
        var target = Data.Current + direction;
        var created = false;
        if (target < 0) { Bump(-1); return; }
        if (target >= Data.Stack.Count)
        {
            if (leaving.IsEmpty) { Bump(1); return; }
            Data.Stack.Add(new Note());
            created = true;
        }
        Data.Current = target;
        var arriving = Data.Stack[target];
        var dropped = DropIfAbandoned(leaving);
        Data.Current = Data.Stack.IndexOf(arriving);
        BindPanes(direction);
        // Small confirmations for the two things that happen implicitly while swiping.
        if (created) Toast("New note", brief: true);
        else if (dropped) Toast("Empty note removed", brief: true);
        Edited();
    }

    internal void NewNote()
    {
        _left.Flush();
        var leaving = LeftNote;
        Data.ViewSlot = 0;
        if (Data.Stack[^1].IsEmpty) Data.Current = Data.Stack.Count - 1;
        else { Data.Stack.Add(new Note()); Data.Current = Data.Stack.Count - 1; }
        var arriving = Data.Stack[Data.Current];
        if (!ReferenceEquals(leaving, arriving)) DropIfAbandoned(leaving);
        Data.Current = Data.Stack.IndexOf(arriving);
        _active = _left;
        BindPanes(1);
        Edited();
    }

    /// <summary>After a long absence the hotkey opens on a blank page; the old note is one swipe back.</summary>
    private void OpenFreshNote()
    {
        if (Data.ViewSlot != 0 || Data.Stack[Data.Current].IsEmpty) return;
        if (Data.Stack[^1].IsEmpty) Data.Current = Data.Stack.Count - 1;
        else { Data.Stack.Add(new Note()); Data.Current = Data.Stack.Count - 1; }
        _active = _left;
        BindPanes(0, focus: false);
    }

    internal void ShowStack()
    {
        if (Data.ViewSlot == 0) return;
        _left.Flush();
        Data.ViewSlot = 0;
        _active = _left;
        BindPanes(-1);
        Edited();
    }

    internal void OpenSlot(int slot)
    {
        if (slot < 1 || slot > 9) return;
        if (Settings.Split && Settings.SplitSlot == slot) { _active = _right; BindPanes(0); return; }
        if (Data.ViewSlot == slot) { ShowStack(); return; }
        _left.Flush();
        var leaving = LeftNote;
        Data.ViewSlot = slot;
        _active = _left;
        DropIfAbandoned(leaving);
        BindPanes(1);
        Edited();
    }

    /// <summary>Moves the current stack note into a slot. The slot's old content (if any) takes its place in the stack.</summary>
    internal void PromoteToSlot(int slot)
    {
        if (Data.ViewSlot != 0 || slot < 1 || slot > 9) return;
        _left.Flush();
        var note = Data.Stack[Data.Current];
        if (note.IsEmpty) { Toast("Nothing to keep yet"); return; }
        var old = Data.Slots[slot - 1];
        note.Slot = slot;
        Data.Slots[slot - 1] = note;
        if (old.IsEmpty) { Data.Stack.RemoveAt(Data.Current); PadView.Forget(old); if (Data.Stack.Count == 0) Data.Stack.Add(new Note()); }
        else { old.Slot = 0; Data.Stack[Data.Current] = old; }
        Data.Current = Math.Clamp(Data.Current, 0, Data.Stack.Count - 1);
        Data.ViewSlot = slot;
        BindPanes(0);
        Toast($"Kept in slot {slot} · Ctrl+{slot} opens it");
        Edited();
    }

    internal void ToggleSplit()
    {
        _left.Flush(); _right.Flush();
        var single = Math.Max(320, Settings.Width);
        Settings.Split = !Settings.Split;
        _sizingForSplit = true;
        if (Settings.Split)
        {
            // Start the split on the first filled slot that isn't already on the left.
            var first = Enumerable.Range(1, 9).FirstOrDefault(i => i != Data.ViewSlot && !Data.Slots[i - 1].IsEmpty);
            if (first > 0) Settings.SplitSlot = first;
            var screen = Screens.ScreenFromWindow(this);
            var scale = screen?.Scaling ?? 1;
            Width = Math.Min(single * 2, (screen?.WorkingArea.Width ?? 4000) / scale - 24);
            if (screen?.WorkingArea is { } a && Position.X + Width * scale > a.Right) Position = new PixelPoint(Math.Max(a.X, (int)(a.Right - Width * scale - 12)), Position.Y);
            _active = _right;
        }
        else { Width = single; _active = _left; }
        _sizingForSplit = false;
        BindPanes(0);
        Edited();
    }

    private void CycleSplitSlot(int direction)
    {
        _right.Flush();
        var next = Settings.SplitSlot;
        do { next = (next - 1 + direction + 9) % 9 + 1; } while (next == Data.ViewSlot);
        Settings.SplitSlot = next;
        BindPanes(0);
        _right.Arrive(direction);
    }

    internal void TogglePin()
    {
        Settings.Pinned = !Settings.Pinned;
        Topmost = Settings.Pinned;
        RebuildChrome();
        Toast(Settings.Pinned ? "Kept on top" : "No longer on top");
        Edited();
    }

    /// <summary>Delete: stack notes go to the trash (undoable); slots are cleared, their text going to the trash.</summary>
    internal void DeleteCurrent()
    {
        var pane = _active;
        pane.Flush();
        var note = pane.Note!;
        if (note.Slot > 0)
        {
            if (note.IsEmpty) return;
            var copy = new Note { Text = note.Text, Created = note.Created, Modified = note.Modified, Deleted = DateTime.Now };
            Data.Trash.Insert(0, copy);
            PadView.DocumentFor(note).Text = "";
            Toast($"Slot {note.Slot} cleared", "Undo", () => { Data.Trash.Remove(copy); PadView.DocumentFor(note).Text = copy.Text; Edited(); });
            Edited();
            return;
        }
        var index = Data.Stack.IndexOf(note);
        Data.Stack.RemoveAt(index);
        PadView.Forget(note);
        if (!note.IsEmpty) { note.Deleted = DateTime.Now; Data.Trash.Insert(0, note); }
        if (Data.Stack.Count == 0) Data.Stack.Add(new Note());
        Data.Current = Math.Clamp(index - 1 < 0 ? 0 : index - 1, 0, Data.Stack.Count - 1);
        if (index < Data.Stack.Count && index > 0) Data.Current = index - 1;
        BindPanes(-1);
        if (!note.IsEmpty)
            Toast("Moved to trash", "Undo", () => RestoreFromTrash(note, index));
        Edited();
    }

    internal void RestoreFromTrash(Note note, int index = -1)
    {
        Data.Trash.Remove(note);
        note.Deleted = null; note.Slot = 0;
        _left.Flush();
        var leaving = LeftNote;
        if (index < 0 || index > Data.Stack.Count) index = Data.Stack.Count;
        Data.Stack.Insert(index, note);
        Data.ViewSlot = 0;
        Data.Current = index;
        DropIfAbandoned(leaving);
        Data.Current = Data.Stack.IndexOf(note);
        _active = _left;
        BindPanes(0);
        Edited();
    }

    /// <summary>Alt+Up/Down style reordering inside the stack.</summary>
    internal void MoveCurrent(int direction)
    {
        if (Data.ViewSlot != 0) return;
        var i = Data.Current; var j = i + direction;
        if (j < 0 || j >= Data.Stack.Count) { Bump(direction); return; }
        (Data.Stack[i], Data.Stack[j]) = (Data.Stack[j], Data.Stack[i]);
        Data.Current = j;
        UpdateChrome();
        Edited();
    }

    /// <summary>Search removed or restored notes behind the panes' backs: rebind without animation.</summary>
    internal void SearchChangedData()
    {
        if (Data.ViewSlot == 0 && !Data.Stack.Contains(_left.Note!)) BindPanes(0, focus: false);
        Edited();
    }

    internal void JumpTo(Note note)
    {
        _left.Flush();
        var leaving = LeftNote;
        if (note.Slot > 0) { CloseOverlay(); if (Data.ViewSlot != note.Slot) OpenSlot(note.Slot); return; }
        Data.ViewSlot = 0;
        var direction = Math.Sign(Data.Stack.IndexOf(note) - Data.Current);
        Data.Current = Data.Stack.IndexOf(note);
        if (!ReferenceEquals(leaving, note)) DropIfAbandoned(leaving);
        Data.Current = Data.Stack.IndexOf(note);
        _active = _left;
        CloseOverlay();
        BindPanes(direction == 0 ? 1 : direction);
        Edited();
    }

    /// <summary>Two-finger horizontal swipe (touchpad) or Shift+wheel pages through the stack.</summary>
    // ---------- swipe: a detent, not a trigger ----------
    // Every horizontal input (touchpad, tilt wheel, Shift+wheel) drags the page with rubber-band resistance.
    // When input stops, the page commits to the next note only past a distance or speed threshold;
    // otherwise it springs back. One gentle notch is never enough, a deliberate flick always is.
    private const double SwipePxPerUnit = 48, SwipeCommitPx = 90, SwipeCommitSpeed = 0.9 /* px per ms */, SwipeFarPx = 200;
    private readonly List<(DateTime At, double Dx)> _swipeSamples = [];

    private void OnWheel(object? sender, PointerWheelEventArgs e)
    {
        if (_overlayOpen) return;
        var horizontal = Math.Abs(e.Delta.X) > 0.001;
        if (!horizontal && !e.KeyModifiers.HasFlag(KeyModifiers.Shift)) return;
        var units = horizontal ? e.Delta.X : e.Delta.Y;
        if (Math.Abs(units) < 0.001) return;
        e.Handled = true;
        var now = DateTime.Now;
        if (Settings.Swipe == "progress") { ProgressSwipe(units, now); return; }
        if ((now - _swipeAt).TotalMilliseconds > 350) { _swipe = 0; _swipeFired = false; _swipeSamples.Clear(); }
        _swipeAt = now;
        if (_swipeFired) return; // the rest of a committed gesture's inertia
        var dx = units * SwipePxPerUnit;
        _swipe += dx;
        _swipeSamples.Add((now, dx));
        _swipeSamples.RemoveAll(x => (now - x.At).TotalMilliseconds > 90);
        if (Math.Abs(_swipe) >= SwipeFarPx) { FireSwipe(); return; }
        // Rubber band: the page follows less and less the further it is pulled.
        var shown = _swipe / (1 + Math.Abs(_swipe) / 260);
        if (_active.RenderTransform is not TranslateTransform follow) _active.RenderTransform = follow = new TranslateTransform();
        follow.X = shown;
        _active.Opacity = 1 - Math.Abs(shown) / 420;
        _swipeSettle ??= NewSettleTimer();
        _swipeSettle.Stop(); _swipeSettle.Start();
    }

    private Avalonia.Threading.DispatcherTimer? _swipeSettle;

    private Avalonia.Threading.DispatcherTimer NewSettleTimer()
    {
        var timer = new Avalonia.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(130) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            if (_swipeFired) return;
            var span = _swipeSamples.Count > 1 ? Math.Max(16, (_swipeSamples[^1].At - _swipeSamples[0].At).TotalMilliseconds) : 90;
            var speed = Math.Abs(_swipeSamples.Sum(x => x.Dx)) / span;
            if (Math.Abs(_swipe) >= SwipeCommitPx || (speed >= SwipeCommitSpeed && Math.Abs(_swipe) >= SwipeCommitPx / 2)) { FireSwipe(); return; }
            SpringBack();
        };
        return timer;
    }

    private void SpringBack()
    {
        var pane = _active;
        var from = (pane.RenderTransform as TranslateTransform)?.X ?? 0;
        var opacity = pane.Opacity;
        _swipe = 0;
        Anim.Run(pane, 200, t =>
        {
            if (pane.RenderTransform is TranslateTransform tt) tt.X = from * (1 - t);
            pane.Opacity = opacity + (1 - opacity) * t;
        }, () => { pane.RenderTransform = null; pane.Opacity = 1; }, decorative: false);
    }

    private void FireSwipe()
    {
        _swipeFired = true;
        _swipeSettle?.Stop();
        _active.RenderTransform = null; _active.Opacity = 1;
        Navigate(_swipe > 0 ? -1 : 1);
    }

    // ---------- swipe: progress (picked in the Scroll Lab) ----------
    // The swipe is measured in its own distance, not the window's: 220 px of input is a whole note, so the same
    // movement moves the note the same share of the way in a narrow or a very wide window. Passing a third of the
    // way changes the note at once; the rest of that gesture (touchpad inertia, more wheel clicks) is ignored.
    // Wheel clicks (the MX thumb wheel, Shift+wheel) are paid out smoothly over a few frames, further when spun faster.
    private const double ProgressFullPx = 220, ProgressFiresAt = 0.35, ProgressClickPx = 26, ProgressPxPerUnit = 100;
    private const int ProgressQuietMs = 260, ProgressRearmMs = 110;
    private double _progress, _progressOwed, _progressGap;
    private long _progressClick, _progressFrameAt;
    private bool _progressDraining;
    private TimeSpan? _progressLast;
    private Avalonia.Threading.DispatcherTimer? _progressSettle;

    private void ProgressSwipe(double units, DateTime now)
    {
        // After a change, a short pause re-arms the next swipe; before one, slow wheel clicks keep adding up for longer.
        if ((now - _swipeAt).TotalMilliseconds > (_swipeFired ? ProgressRearmMs : ProgressQuietMs)) { _progress = 0; _progressOwed = 0; _swipeFired = false; }
        _swipeAt = now;        if (_swipeFired) return;
        var click = Math.Abs(units - Math.Round(units)) < 0.01;
        if (!click) ProgressAdvance(units * ProgressPxPerUnit);
        else
        {
            var tick = Environment.TickCount64;
            var gap = _progressClick == 0 ? 1000 : tick - _progressClick;
            _progressClick = tick;
            _progressGap = gap >= 400 || _progressGap <= 0 ? Math.Min(gap, 400) : _progressGap * 0.4 + gap * 0.6;
            var gain = 1 + 2 * Math.Clamp((1000 / _progressGap - 6) / 24, 0, 1);
            _progressOwed += units * ProgressClickPx * gain;
            // A hidden window gets no frames, so a pay-out cut off by hiding is restarted rather than trusted.
            if (!_progressDraining || tick - _progressFrameAt > 150) { _progressDraining = true; _progressLast = null; _progressFrameAt = tick; ProgressFrame(); }
        }
        _progressSettle ??= NewProgressSettle();
        _progressSettle.Stop(); _progressSettle.Start();
    }

    private void ProgressFrame() => RequestAnimationFrame(time =>
    {
        var dt = _progressLast is { } last ? Math.Clamp((time - last).TotalSeconds, 0, 0.05) : 1 / 60.0;
        _progressLast = time;
        _progressFrameAt = Environment.TickCount64;
        if (_swipeFired || Math.Abs(_progressOwed) < 0.01) { _progressOwed = 0; _progressDraining = false; return; }
        var part = _progressOwed * (1 - Math.Exp(-22 * dt));
        if (Math.Abs(_progressOwed - part) < 0.5) part = _progressOwed;
        _progressOwed -= part;
        ProgressAdvance(part);
        ProgressFrame();
    });

    private void ProgressAdvance(double px)
    {
        if (_swipeFired) return;
        _progress += px / ProgressFullPx;
        if (Math.Abs(_progress) >= ProgressFiresAt) { _swipe = _progress; _progressOwed = 0; FireSwipe(); return; }
        // Nearly 1:1 for the first half note, then a rubber band (never reached before it fires at the defaults).
        var a = Math.Abs(_progress);
        var share = Math.Sign(_progress) * (a <= 0.5 ? a : 0.5 + (1 - 1 / ((a - 0.5) * 0.55 / 0.5 + 1)) * 0.5);
        if (_active.RenderTransform is not TranslateTransform follow) _active.RenderTransform = follow = new TranslateTransform();
        follow.X = share * _active.Bounds.Width;        _active.Opacity = 1 - Math.Min(0.7, Math.Abs(share) * 1.2);
    }

    private Avalonia.Threading.DispatcherTimer NewProgressSettle()
    {
        var timer = new Avalonia.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ProgressQuietMs) };
        timer.Tick += (_, _) =>
        {
            if (Math.Abs(_progressOwed) > 0.5) return;     // still paying out the last clicks
            timer.Stop();
            if (_swipeFired) return;
            _progress = 0;
            SpringBack();
        };
        return timer;
    }

    // ---------- clipboard capture ----------

    private string? _ownClipboard;
    private string? _lastCaptured;

    internal async Task CopyText(string text)
    {
        _ownClipboard = text;
        if (Clipboard is { } clipboard) await clipboard.SetTextAsync(text);
    }

    private async Task OnClipboardChanged()
    {
        if (_awaitingSnip is { } since && (DateTime.Now - since).TotalSeconds < 120 && await Ocr.ClipboardHasImage())
        {
            _awaitingSnip = null;
            await OcrIntoPane(_snipPane ?? _active);
            return;
        }
        var target = Data.Stack.Concat(Data.Slots).Where(n => n.ModeCache == NoteMode.Clipboard).MaxBy(n => n.Modified);
        if (target is null || Clipboard is not { } clipboard) return;
        // Password managers mark secrets as not-for-history; never capture those.
        if (Native.ClipboardIsPrivate()) return;
        string? text;
        try { text = await clipboard.TryGetTextAsync(); } catch { return; }
        if (string.IsNullOrWhiteSpace(text) || text == _ownClipboard || text == _lastCaptured) return;
        // Copies out of the clipboard note itself would echo back in; skip anything already in it.
        if (target.PeekText().Contains(text.Trim())) return;
        _lastCaptured = text;
        var doc = PadView.DocumentFor(target);
        var entry = text.Trim().Replace("\r\n", "\n").Replace("\n", "\n  ");
        doc.Insert(doc.TextLength, (doc.Text.EndsWith('\n') ? "" : "\n") + "- " + entry);
        if (!IsVisible) _hiddenAt = DateTime.Now;
        Edited();
    }

    // ---------- timers ----------

    private void OnTimerFinished(Timers.Entry entry)
    {
        Native.Chime();
        var name = entry.Label.Length > 0 ? entry.Label : Timers.Format(entry.Duration) + " timer";
        ShowQuietly();
        Toast($"⏰ {name} is done");
        UpdateChrome();
    }

    // ---------- OCR ----------

    private DateTime? _awaitingSnip;
    private PadView? _snipPane;

    private async Task RunOcr(PadView pad)
    {
        if (await Ocr.ClipboardHasImage()) { await OcrIntoPane(pad); return; }
        _awaitingSnip = DateTime.Now; _snipPane = pad;
        Toast("Snip an area · the text lands here");
        Native.StartScreenSnip();
    }

    private async Task OcrIntoPane(PadView pad)
    {
        var (text, error) = await Ocr.ReadClipboardImage();
        if (!IsVisible) ShowResident(); else Activate();
        if (text is null) { Toast(error ?? "No text found"); return; }
        var doc = pad.Editor.Document;
        doc.Insert(pad.Editor.CaretOffset, text.Trim());
        Toast($"Read {text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length} words from the image");
        pad.FocusText();
    }
}
