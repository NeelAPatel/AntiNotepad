using AntiQuickPad.Core;
namespace AntiQuickPad.ViewModels;

// Small control-free capability check of the new shell projections and notification inputs.
internal static class ProjectionSmoke
{
    internal static void Run(string directory)
    {
        var vm = new MainViewModel(new NotesSession(directory), new SettingsStore(Path.Combine(directory, "settings.json")));
        var changed = new HashSet<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException("PROJECTIONS FAIL: " + message); }
        Check(vm.IsCurrentEmpty && vm.SaveLineText == "Ready to write", "empty state");
        vm.UpdateCurrent("text", 0); vm.Save();
        Check(!vm.IsCurrentEmpty && vm.SaveLineText == "All changes saved" && changed.Contains(nameof(vm.IsCurrentEmpty)), "text/save notification");
        vm.Status.Set(true, false); Check(vm.SaveLineText == "Saving", "saving text");
        vm.Fail(new IOException("probe")); Check(vm.SaveLineFailed && vm.SaveLineText == "Could not save. Check disk access." && changed.Contains(nameof(vm.SaveLineFailed)), "failure projection");
        vm.Status.Set(false, false);
        for (var i = 1; i < 12; i++) vm.CreateCommand.Execute(null);
        for (var index = 0; index < 12; index++)
        {
            changed.Clear(); vm.SelectCommand.Execute(index);
            var first = Math.Clamp(index - 3, 0, 4);
            Check(vm.PositionText == $"{index + 1} of 12" && vm.CountText == $"{index + 1} / 12", "position text");
            Check(vm.PagerFirstIndex == first && vm.PagerDots.Select(d => d.Index).SequenceEqual(Enumerable.Range(first, 8)) &&
                vm.PagerDots.Count(d => d.IsCurrent) == 1 && vm.PagerDots.Single(d => d.IsCurrent).Index == index, "pager clamp/current");
            Check(vm.HasPrevious == (index > 0) && vm.HasNext == (index < 11) && changed.Contains(nameof(vm.PagerDots)), "navigation projections/notification");
        }
        Console.WriteLine("PROJECTIONS PASS (empty/text/status, 12-note pager/navigation, notifications)");
        RunMeasurementFixture(Path.Combine(directory, "fixture"));
    }

    // F54-measure-heavy-index-crash: bound projections are read on every notification while the fixture fills.
    private static void RunMeasurementFixture(string directory)
    {
        var vm = new MainViewModel(new NotesSession(directory), new SettingsStore(Path.Combine(directory, "settings.json")));
        vm.PropertyChanged += (_, _) => _ = (vm.SaveLineText, vm.Current.Text, vm.PositionText, vm.PagerDots.Count);
        vm.Status.PropertyChanged += (_, _) => _ = vm.SaveLineText;
        void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException("F54-measure-heavy-index-crash FAIL: " + message); }
        vm.PopulateMeasurementFixture();
        Check(vm.Notes.Count == 20 && vm.CurrentIndex == 19 && vm.PositionText == "20 of 20", "fixture position");
        Check(vm.Current.Text.StartsWith("- [ ] Note 20, line 1:") && vm.Notes.All(n => n.Text.Split('\n').Length == 200), "fixture content");
        vm.Previous(); vm.Select(0); vm.Next();
        Check(vm.CurrentIndex == 1 && vm.PositionText == "2 of 20" && vm.Current.Text.StartsWith("- [ ] Note 2,"), "navigation after fixture");
        Console.WriteLine("F54-measure-heavy-index-crash PASS: 20 x 200 fixture with live projection reads; navigation consistent");
    }
}
