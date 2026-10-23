using System.Diagnostics;
using AvaloniaEdit.Document;

namespace InputProbe;

internal static class SmokeChecks
{
    public static int Run()
    {
        var timer = Stopwatch.StartNew(); var passed = 0; var failed = 0;
        void Check(string name, Func<bool> action)
        {
            var one = Stopwatch.StartNew();
            try
            {
                if (action()) { passed++; Console.WriteLine($"PASS {name} {one.Elapsed.TotalMilliseconds:F2}ms"); }
                else { failed++; Console.WriteLine($"FAIL {name} {one.Elapsed.TotalMilliseconds:F2}ms"); }
            }
            catch (Exception ex) { failed++; Console.WriteLine($"FAIL {name}: {ex.GetType().Name} {ex.Message}"); }
        }

        Check("one notch starts one page request and same-direction deltas coalesce", () => { var wheel = new WheelBurstState(); var a = wheel.Add(0, 4, 1, 0); var b = wheel.Add(0, 4, 1, 80, true); var c = wheel.Add(0, 4, 1, 170, true); var d = wheel.Add(0, 4, 1, 230, true); return a.BurstNumber == 1 && b.BurstNumber == 1 && c.BurstNumber == 1 && d.BurstNumber == 1 && a.TargetIndex == 1 && d.TargetIndex == 1; });
        Check("wheel reversal retargets same burst; idle gap starts next burst", () => { var wheel = new WheelBurstState(); wheel.Add(1, 4, 1, 0); var reverse = wheel.Add(1, 4, -1, 80, true); var next = wheel.Add(0, 4, -1, 300); return reverse.BurstNumber == 1 && reverse.TargetIndex == 0 && next.BurstNumber == 2 && next.TargetIndex == 0; });
        Check("newest-edge wheel create is deferred once and reversal cancels it", () => { var wheel = new WheelBurstState(); var edge = wheel.Add(2, 3, 1, 0); var reversed = wheel.Add(2, 3, -1, 80, true); var cancelled = wheel.Complete(); wheel.Add(2, 3, 1, 300); var completed = wheel.Complete(); return edge.CreateAtNewestEdge && !reversed.CreateAtNewestEdge && !cancelled.CreateAtNewestEdge && completed.CreateAtNewestEdge; });
        Check("mouse drag release uses distance, velocity and spring-back", () => CarouselMath.SnapIndex(1, -200, 0, 4) == 2 && CarouselMath.SnapIndex(1, 0, -.8, 4) == 2 && CarouselMath.SnapIndex(1, -40, 0, 4) == 1 && CarouselMath.RubberBand(-900) > -180 && CarouselMath.RubberBand(-900) < 0);
        Check("overscroll creates only past newest note", () => CarouselMath.ShouldCreateNote(2, 3, -180) && !CarouselMath.ShouldCreateNote(1, 3, -180) && !CarouselMath.ShouldCreateNote(2, 3, 100));
        Check("interior blank notes prune and trailing blank remains", () =>
        {
            var notes = new List<Note> { new() { Text = "a" }, new(), new() { Text = "c" }, new() };
            var retainedId = notes[2].Id;
            var result = CarouselMath.PruneInteriorBlanks(notes);
            return result.Count == 3 && result[^1].Text == "" && result[1].Id == retainedId;
        });

        var doc = new TextDocument("- [ ] task\nsecond\n"); var caret = 8;
        Check("checkbox toggle preserves caret and undoes as one edit", () => EditorOperations.ToggleCheckbox(doc, 2, ref caret) == "[x]" && doc.Text.StartsWith("- [x]", StringComparison.Ordinal) && caret == 8 && doc.UndoStack.CanUndo && UndoCheckbox(doc));
        Check("caret Tab and Shift+Tab use shared key helper", () => { var d = new TextDocument("- [ ] task\n    first\nsecond\n"); var tab = EditorOperations.ApplyTab(d, 3, null, null, false); var outdent = EditorOperations.ApplyTab(d, 19, null, null, true); var unindented = EditorOperations.ApplyTab(d, 22, null, null, true); return tab.Changed && tab.CaretOffset == 7 && outdent.Changed && outdent.CaretOffset == 15 && unindented.Changed == false && d.Text.StartsWith("- [     ] task\nfirst\nsecond", StringComparison.Ordinal); });
        Check("caret inside removed prefix clamps to line start and tab prefix outdents", () => { var d = new TextDocument("    first\n\tsecond\n"); var a = EditorOperations.ApplyTab(d, 2, null, null, true); var b = EditorOperations.ApplyTab(d, 12, null, null, true); return a.CaretOffset == 0 && b.Changed && d.Text == "first\nsecond\n"; });
        Check("selection indent preserves anchors and is one undo group", () => { var d = new TextDocument("first\nsecond\n"); var result = EditorOperations.ApplyTab(d, 2, 0, 12, false); var indented = d.Text; if (!result.Changed || result.CaretOffset != 6 || result.SelectionStart != 4 || result.SelectionEnd != 20 || !d.UndoStack.CanUndo) return false; d.UndoStack.Undo(); return indented.StartsWith("    first\n    second", StringComparison.Ordinal) && d.Text == "first\nsecond\n"; });
        Check("selection outdent preserves caret and mapped anchors", () => { var d = new TextDocument("    first\n  second\n"); var result = EditorOperations.ApplyTab(d, 8, 0, 16, true); return result.Changed && d.Text.StartsWith("first\nsecond", StringComparison.Ordinal) && result.CaretOffset == 4 && result.SelectionStart == 0 && result.SelectionEnd == 10; });

        var temp = Path.Combine(Path.GetTempPath(), $"aqp-smoke-{Guid.NewGuid():N}.json");
        Check("JSON persistence restores notes", () =>
        {
            try { var store = new NoteStore(temp); var notes = new List<Note> { new() { Text = NoteStore.Fixture }, new() { Text = "survives relaunch" } }; store.Save(notes); return store.Load().Select(n => n.Text).SequenceEqual(notes.Select(n => n.Text)); }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        });

        Console.WriteLine($"SMOKE {(failed == 0 ? "PASS" : "FAIL")} checks={passed + failed} passed={passed} failed={failed} elapsed={timer.Elapsed.TotalMilliseconds:F1}ms (model-level; does not measure visible focus/hardware/materials)");
        return failed == 0 ? 0 : 1;
    }

    private static bool UndoCheckbox(TextDocument document)
    {
        document.UndoStack.Undo();
        return document.Text.StartsWith("- [ ]", StringComparison.Ordinal);
    }
}
