// Note selection, split-slot navigation, and input routing for the pad window.
using Avalonia.Input.Platform;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AntiNotepad.Editor;
using AntiNotepad.Model;
using AntiNotepad.Platform;

namespace AntiNotepad.UI;

internal sealed partial class PadWindow
{
    private NoteSwipe? _noteSwipe;
    private bool _sizingForSplit;

    private Note LeftNote => Data.ViewSlot > 0 ? Data.Slots[Data.ViewSlot - 1] : Data.Stack[Data.Current];
    private Note RightNote => Data.Slots[Settings.SplitSlot - 1];

    /// <summary>Binds both panes to the current model selection.</summary>
    private void BindPanes(int arrive, bool focus = true, int rightArrive = 0)
    {
        var previous = _savedNote;
        _bindingPanes = true;
        try
        {
            _left.SwitchTo(LeftNote, _active == _left ? arrive : 0);
            if (Settings.Split)
            {
                // Never show the same slot twice side by side.
                if (Data.ViewSlot == Settings.SplitSlot)
                    _settingsService.Update(settings => settings.SplitSlot = settings.SplitSlot % 9 + 1);
                _right.SwitchTo(RightNote, rightArrive);
            }
        }
        finally { _bindingPanes = false; }
        ApplyPaneSplit(_panes, _leftHost, _divider, _rightHost, _right, Settings.Split);
        if (!Settings.Split && _active == _right) _active = _left;
        var direction = _active == _left ? arrive : rightArrive;
        UpdateChrome(ReferenceEquals(previous, _active.Note) ? 0 : direction);
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

    /// <summary>Returns the notice for a stack edge or an implicit new note.</summary>
    internal static string? EdgeNotice(int target, int count, bool leavingEmpty) =>
        target < 0 ? "No older Notes" : target >= count ? leavingEmpty ? "Already at Latest" : "New Note Created" : null;

    internal void Navigate(int direction)
    {
        if (_active == _right) { CycleSplitSlot(direction); return; }
        if (Data.ViewSlot > 0) { ShowStack(); return; }
        _left.Flush();
        var leaving = Data.Stack[Data.Current];
        var target = Data.Current + direction;
        var notice = EdgeNotice(target, Data.Stack.Count, leaving.IsEmpty);
        if (target < 0) { Bump(-1); Toast(notice!, brief: true); return; }
        if (target >= Data.Stack.Count)
        {
            if (leaving.IsEmpty) { Bump(1); Toast(notice!, brief: true); return; }
            Data.Stack.Add(new Note());
        }
        Data.Current = target;
        var arriving = Data.Stack[target];
        var dropped = DropIfAbandoned(leaving);
        Data.Current = Data.Stack.IndexOf(arriving);
        BindPanes(direction);
        // Small confirmations for the two things that happen implicitly while swiping.
        if (notice is not null) Toast(notice, brief: true);
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
        var split = !Settings.Split;
        var first = split
            ? Enumerable.Range(1, 9).FirstOrDefault(i => i != Data.ViewSlot && !Data.Slots[i - 1].IsEmpty)
            : 0;
        _settingsService.Update(settings =>
        {
            settings.Split = split;
            if (first > 0) settings.SplitSlot = first;
        });
        _sizingForSplit = true;
        if (Settings.Split)
        {
            // Start the split on the first filled slot that isn't already on the left.
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
        _settingsService.Update(settings => settings.SplitSlot = next);
        BindPanes(0, rightArrive: direction);
    }

    internal void TogglePin()
    {
        _settingsService.Update(settings => settings.Pinned = !settings.Pinned);
        Toast(Settings.Pinned ? "Kept on top" : "No longer on top");
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

    /// <summary>Two-finger horizontal swipe (touchpad), tilt or thumb wheel, or Shift+wheel pages through the stack (see NoteSwipe).</summary>
    private void OnWheel(object? sender, PointerWheelEventArgs e)
    {
        if (_overlayOpen) return;
        var horizontal = Math.Abs(e.Delta.X) > 0.001;
        if (!horizontal && !e.KeyModifiers.HasFlag(KeyModifiers.Shift)) return;
        var units = horizontal ? e.Delta.X : e.Delta.Y;
        if (Math.Abs(units) < 0.001) return;
        e.Handled = true;
        _noteSwipe ??= new NoteSwipe(this, () => Settings, () => _active, Navigate);
        _noteSwipe.Input(units);
    }

    // ---------- clipboard capture ----------

    private readonly ClipboardCapturePolicy _clipboardPolicy;

    internal async Task CopyText(string text)
    {
        _clipboardPolicy.RecordOwnCopy(text);
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
        if (DesktopPlatform.ClipboardIsPrivate()) return;
        string? text;
        try { text = await clipboard.TryGetTextAsync(); } catch { return; }
        if (_clipboardPolicy.CheckText(text) != ClipboardCapturePolicy.TextCheck.Eligible) return;
        // Copies out of the clipboard note itself would echo back in; skip anything already in it.
        if (!_clipboardPolicy.Decide(text!, target.PeekText())) return;
        var doc = PadView.DocumentFor(target);
        doc.Insert(doc.TextLength, ClipboardCapturePolicy.FormatAppend(text!, doc.Text.EndsWith('\n')));
        if (!IsVisible) _hiddenAt = DateTime.Now;
        Edited();
    }

    // ---------- timers ----------

    private void OnTimerFinished(Timers.Entry entry)
    {
        DesktopPlatform.Chime();
        var name = entry.Label.Length > 0 ? entry.Label : Timers.Format(entry.Duration) + " timer";
        ShowQuietly();
        Toast($"⏰ {name} is done");
        UpdateChrome();
    }

    // ---------- OCR ----------

    private sealed record OcrTarget(Note Note, AvaloniaEdit.Document.TextDocument Document, int CaretOffset);

    private DateTime? _awaitingSnip;
    private PadView? _snipPane;
    private OcrTarget? _snipTarget;

    private Task RunOcr(PadView pad)
    {
        _snipTarget = CaptureOcrTarget(pad);
        _awaitingSnip = DateTime.Now; _snipPane = pad;
        Toast("Snip an area · the text lands here");
        DesktopPlatform.StartScreenSnip();
        return Task.CompletedTask;
    }

    private async Task OcrIntoPane(PadView pad)
    {
        var target = _snipTarget ?? CaptureOcrTarget(pad);
        _snipTarget = null;
        await OcrIntoTarget(target);
    }

    private static OcrTarget CaptureOcrTarget(PadView pad)
    {
        var note = pad.Note!;
        var document = pad.Editor.Document;
        return new OcrTarget(note, document, Math.Clamp(pad.Editor.CaretOffset, 0, document.TextLength));
    }

    private async Task OcrIntoTarget(OcrTarget target)
    {
        var (text, error) = await Ocr.ReadClipboardImage();
        if (!IsVisible) ShowResident(); else Activate();
        var note = target.Note;
        if (!Data.Stack.Contains(note) && !Data.Slots.Contains(note))
        {
            Toast("Target note was deleted · text not added");
            return;
        }
        if (text is null) { Toast(error ?? "No text found"); return; }

        var pane = ReferenceEquals(_left.Note, note) && ReferenceEquals(_left.Editor.Document, target.Document)
            ? _left
            : Settings.Split && ReferenceEquals(_right.Note, note) && ReferenceEquals(_right.Editor.Document, target.Document)
                ? _right
                : null;
        var recognized = text.Trim();
        if (pane is null)
        {
            var title = note.Title;
            var current = note.Text;
            var separator = current.Length == 0 || current.EndsWith('\n') ? "" : "\n";
            // Keep any cached document in sync, then snapshot the appended text through the note model.
            var document = PadView.DocumentFor(note);
            document.Insert(document.TextLength, separator + recognized);
            note.Text = document.Text;
            note.Attach(() => document.Text);
            Edited();
            Toast($"Text added to {title}");
            return;
        }

        var offset = Math.Clamp(target.CaretOffset, 0, target.Document.TextLength);
        target.Document.Insert(offset, recognized);
        pane.Editor.CaretOffset = Math.Clamp(offset + recognized.Length, 0, target.Document.TextLength);
        _active = pane;
        UpdateChrome();
        Toast($"Read {text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length} words from the image");
        pane.FocusText();
    }
}
