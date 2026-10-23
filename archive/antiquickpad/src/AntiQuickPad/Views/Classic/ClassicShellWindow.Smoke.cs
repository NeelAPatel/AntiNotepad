using System.Diagnostics;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using AntiQuickPad.ViewModels;
using AntiQuickPad.Shell;
using AntiQuickPad.Editor;
using AntiQuickPad.Carousel;
namespace AntiQuickPad.Views.Classic;
public sealed partial class ClassicShellWindow
{
    private async Task RunSelfTest()
    {
        var exitCode = 1;
        try
        {
            if (!MainViewModel.CoreHasNoAvaloniaReference())
                throw new Exception("Core must have no Avalonia assembly references.");
            EditorFactory.RunSmoke();
            if (WindowDecorations != WindowDecorations.Full || Topmost) throw new Exception("Standard chrome/default pin failed.");
            _pin.IsChecked = true;
            if (!Topmost || !JsonSerializer.Deserialize<WindowSnapshot>(File.ReadAllText(Program.Paths.Settings))!.Pinned) throw new Exception("Pin on/persistence failed.");
            // F37-hidden-selftest-windows: native maximize moves onto a monitor; physical check only.
            _pin.IsChecked = false;
            if (Topmost || JsonSerializer.Deserialize<WindowSnapshot>(File.ReadAllText(Program.Paths.Settings))!.Pinned) throw new Exception("Pin off/persistence failed.");
            var expectedPosition = Position; var expectedWidth = Width; var expectedHeight = Height;
            SaveWindowSnapshot(); Width += 20; Height += 20; RestoreWindowSnapshot();
            if (Width != expectedWidth || Height != expectedHeight || Position != expectedPosition) throw new Exception("Window bounds round trip failed.");
            Close();
            if (IsVisible || _quitting) throw new Exception("Close must hide the resident window.");
            ShowResident("selftest-close");
            var appearance = _vm.LoadSettings()!;
            _vm.SaveSettings(appearance with { ShellId = "quiet", LookId = "dark" });
            SaveWindowSnapshot();
            if (_vm.LoadSettings() is not { ShellId: "quiet", LookId: "dark" }) throw new Exception("Shell/look IDs lost during bounds save.");
            _vm.SaveSettings(appearance);
            Console.WriteLine("T012 PASS: shell/look IDs can change through VM settings and survive bounds saves.");
            Console.WriteLine("T007 chrome PASS: Close hides; Pin toggles Topmost and persists; bounds round trip.");
            await Task.Delay(200);
            if (!IsVisible || !_editor.TextArea.IsKeyboardFocusWithin) throw new Exception("Startup editor focus failed.");
            _editor.Text = "# Heading one\n## Heading two\n### Three\n#### Four\n##### Five\n###### Six\n**bold** *italic* ~~strike~~\n- [ ] todo\n- [x] done\n```python\ndef note():\n    return '# string' # comment\n```\n";
            await Task.Delay(80); // Exercise real visual-line generation, inline controls and code background.
            _editor.TextArea.TextView.EnsureVisualLines();
            if (_editor.TextArea.TextView.VisualLines.Count == 0) throw new Exception("Decorated fixture did not lay out.");
            Console.WriteLine("M3 PASS: Markdown classification; undoable checkbox text edit; Shift+Tab; decorated visual lines.");
            await StrikeProof.Run(_editor, Path.Combine(Program.Paths.Root, "T-009-strike.png"));
            await AlignmentProof.Run(_editor, Program.Paths.Root);
            await ActiveGuideProof.Run(_editor, Path.Combine(Program.Paths.Root, "T-009-active.png"));
            await RunCommandQuitSmoke();
            Console.WriteLine("F37-hidden-selftest-windows: real --measure omitted; manager runs it separately on the desktop.");
            _editor.Text = "";
            var originalEditor = _editor;
            var timings = new List<double>();
            for (var i = 0; i < 3; i++)
            {
                Toggle(); if (IsVisible) throw new Exception("Toggle did not hide.");
                await Task.Delay(40);
                _focusCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
                Toggle(Stopwatch.GetTimestamp());
                timings.Add(await _focusCompletion.Task.WaitAsync(TimeSpan.FromSeconds(3)));
                if (!ReferenceEquals(originalEditor, _editor) || !_editor.TextArea.IsKeyboardFocusWithin)
                    throw new Exception("Resident editor was recreated or not focused.");
            }
            CreateNote(); _editor.Text = "# editor round trip\nsecond line"; _editor.CaretOffset = 7;
            await Task.Delay(550); // Let the real debounce timer persist the edit and caret.
            if (!MainViewModel.VerifyReload(Program.Paths.Notes, _editor.Text, 7, 1))
                throw new Exception("Editor round trip failed.");
            if (!MainViewModel.PersistenceSmoke(Program.Paths.Root)) throw new Exception("Persistence smoke failed.");
            await RunChild("--selftest-reload", Program.Paths.Notes);
            await RunCarouselSmoke();
            Toggle();
            var signalId = "AntiQuickPad-selftest-" + Guid.NewGuid().ToString("N");
            var signal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _focusCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            using (var singleton = new SingleInstanceService(() => Dispatcher.UIThread.Post(() =>
            {
                ShowResident("selftest-second-launch"); signal.TrySetResult();
            }), signalId))
            {
                if (!singleton.IsPrimary) throw new Exception("Isolated singleton was not primary.");
                await RunChild("--selftest-signal", signalId);
                await signal.Task.WaitAsync(TimeSpan.FromSeconds(3));
                await _focusCompletion.Task.WaitAsync(TimeSpan.FromSeconds(3));
            }
            if (!IsVisible || !_editor.TextArea.IsKeyboardFocusWithin) throw new Exception("Second-launch show/focus failed.");
            foreach (var ms in timings) Console.WriteLine($"show-to-focus internal ms={ms:F3}");
            if (timings.Any(ms => ms >= 100)) throw new Exception("Internal show-to-focus exceeded 100 ms.");
            // Lock the manifest to exercise the actual SaveNotes/Quit failure path without touching user notes.
            using (var locked = new FileStream(Path.Combine(Program.Paths.Notes, "notes.json"), FileMode.Open, FileAccess.Read, FileShare.None))
            {
                Hide(); Quit(source: "command");
                if (IsVisible || _quitting || !_saveFailed) throw new Exception("Failed command quit must stay hidden and resident.");
                Quit();
                using var logReader = new StreamReader(new FileStream(Program.Paths.Log, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                var log = logReader.ReadToEnd();
                if (_quitting || !IsVisible || !_saveFailed || !_status.Text!.StartsWith("Could not save") ||
                    !log.Contains("quit-start") || !log.Contains("quit-aborted save-failed reason=") || !log.Contains("save-error"))
                    throw new Exception("Quit save-failure visibility/logging failed.");
            }
            Console.WriteLine("T006 PASS: heading boundaries; list Enter/undo; quit failure restores visible status and logs reason.");
            Console.WriteLine("SELFTEST PASS: same resident editor; toggle/focus; debounced save; next-process reload; second-process signal/show; notes round trip.");
            Log("SELFTEST PASS"); exitCode = 0;
        }
        catch (Exception ex) { Console.Error.WriteLine($"SELFTEST FAIL: {ex}"); Log($"SELFTEST FAIL: {ex}"); }
        finally
        {
            Quit(exitCode);
        }
    }
    private async Task RunCarouselSmoke()
    {
        CarouselController.RunSmoke();
        CreateNote(); SelectNote(0);
        _carousel.RequestWheel(1, 0);
        for (var t = 20; t <= 180; t += 20) _carousel.RequestWheel(1, t);
        await Task.Delay(450);
        _carousel.RequestWheel(1, 250); // trailing event of the same burst, after animation
        await Task.Delay(60);
        if (_vm.CurrentIndex != 1 || _carousel.IsBusy) throw new Exception("Integrated wheel notch/burst double skip.");
        var width = _carousel.View.Bounds.Width;
        _carousel.BeginDrag(0, 1000); _carousel.MoveDrag(-width * .6, 2000);
        if (Math.Abs(((TranslateTransform)_editor.RenderTransform!).X + width * .6) > 1)
            throw new Exception("Drag was not 1:1.");
        _carousel.EndDrag(2100); await Task.Delay(450);
        if (_vm.CurrentIndex != 2) throw new Exception("Integrated long drag did not snap to next note.");
        _carousel.BeginDrag(0, 3000); _carousel.MoveDrag(20, 3100); _carousel.EndDrag(3250);
        await Task.Delay(450);
        if (_vm.CurrentIndex != 2) throw new Exception("Integrated short drag did not return.");
        _editor.Text = string.Join('\n', Enumerable.Range(0, 120).Select(i => $"Line {i} saved scroll"));
        _editor.CaretOffset = 7; await Task.Delay(60);
        _editor.UpdateLayout(); _editor.TextArea.TextView.EnsureVisualLines();
        EditorViewport.Restore(_editor, new Vector(0, 300));
        await Task.Delay(120);
        var saved = _editor.VerticalOffset;
        SelectNote(1); SelectNote(2); await Task.Delay(60);
        if (_editor.CaretOffset != 7 || saved < 100 || Math.Abs(_editor.VerticalOffset - saved) > 1)
            throw new Exception($"Caret/scroll restore failed: caret={_editor.CaretOffset} scroll={_editor.VerticalOffset} expected={saved} editor={_editor.Bounds} documentHeight={_editor.TextArea.TextView.DocumentHeight}.");
        _carousel.BeginDrag(0, 4000); _carousel.MoveDrag(40, 4100); _carousel.Cancel();
        if (_carousel.IsBusy || _editor.IsReadOnly || ((TranslateTransform)_editor.RenderTransform!).X != 0)
            throw new Exception("Drag cancellation failed.");
        DeleteCurrentNote(); DeleteCurrentNote(); DeleteCurrentNote();
        if (_vm.Notes.Count != 1) throw new Exception("Delete must keep one note.");
        Console.WriteLine("M4 PASS: integrated notch/burst; 1:1 drag; long/short snap; caret/scroll; cancellation; delete keeps one note.");
    }
    private static Process StartChild(params string[] args) => StartChild(false, args);
    private static Process StartChild(bool captureError, params string[] args)
    {
        var executable = Environment.ProcessPath!;
        var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = captureError,
            RedirectStandardOutput = args.Contains("--selftest-quit-resident") };
        if (string.Equals(Path.GetFileNameWithoutExtension(executable), "dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(typeof(Program).Assembly.Location);
        foreach (var arg in args) start.ArgumentList.Add(arg);
        return Process.Start(start) ?? throw new Exception("Could not start child.");
    }
    private static async Task RunCommandQuitSmoke()
    {
        var id = "AntiQuickPad-quit-" + Guid.NewGuid().ToString("N");
        var root = Path.Combine(Program.Paths.Root, "command-resident");
        Directory.CreateDirectory(root);
        using (var resident = StartChild("--selftest-quit-resident", id, root))
        {
            var ready = Path.Combine(root, "ready");
            var timer = Stopwatch.StartNew();
            while (!File.Exists(ready) && !resident.HasExited && timer.Elapsed < TimeSpan.FromSeconds(5)) await Task.Delay(20);
            if (!File.Exists(ready)) throw new Exception("Quit resident not ready.");
            using var command = StartChild("--selftest-quit", id);
            await command.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(6));
            if (command.ExitCode != 0 || !resident.HasExited || resident.ExitCode != 0) throw new Exception("Command did not await clean resident exit.");
            Console.Write(await resident.StandardOutput.ReadToEndAsync());
            var paths = new RuntimePaths(root);
            var log = File.ReadAllText(paths.Log);
            if (!log.Contains("quit-start source=command") || !log.Contains("quit-finish window-closed=true source=command") || log.Contains("source=second-launch"))
                throw new Exception("Command quit source/show logging failed.");
            if (!MainViewModel.VerifyCurrentText(paths.Notes, "command quit saves this pending edit")) throw new Exception("Command quit did not save.");
            Console.WriteLine($"T009 command quit PASS: isolated resident exit=0, pending edit saved, source=command start/finish, no Show signal; elapsed={timer.ElapsedMilliseconds}ms.");
        }
        using (var absent = StartChild("--selftest-quit", id))
        {
            await absent.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(2));
            if (absent.ExitCode != 0) throw new Exception("No-resident quit failed.");
        }
        var stalledReady = Path.Combine(root, "stalled-ready");
        using var stalled = StartChild("--selftest-quit-stalled", id, stalledReady);
        while (!File.Exists(stalledReady) && !stalled.HasExited) await Task.Delay(20);
        var timeout = Stopwatch.StartNew();
        using var timed = StartChild(true, "--selftest-quit", id);
        await timed.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(6));
        if (timed.ExitCode != 1 || timeout.ElapsedMilliseconds < 4900 || timeout.ElapsedMilliseconds > 5900) throw new Exception("Quit timeout contract failed.");
        if (!(await timed.StandardError.ReadToEndAsync()).Contains("resident did not exit within 5 seconds")) throw new Exception("Quit timeout message missing.");
        Console.WriteLine($"T009 timeout PASS: exit=1 after {timeout.ElapsedMilliseconds}ms; no-resident exit=0.");
        await stalled.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(3));
    }
    private static async Task RunChild(string mode, string argument)
    {
        var executable = Environment.ProcessPath!;
        var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true };
        if (string.Equals(Path.GetFileNameWithoutExtension(executable), "dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(typeof(Program).Assembly.Location);
        start.ArgumentList.Add(mode); start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new Exception("Could not start selftest child.");
        try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
        catch { throw; }
        if (process.ExitCode != 0) throw new Exception($"{mode} child exit={process.ExitCode}");
    }
}
