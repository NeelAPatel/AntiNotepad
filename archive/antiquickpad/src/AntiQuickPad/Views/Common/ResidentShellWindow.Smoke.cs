using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using AntiQuickPad.Carousel;
using AntiQuickPad.Editor;

namespace AntiQuickPad.Views.Common;

public sealed partial class ResidentShellWindow
{
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    private void RunSettingsMigrationSmoke()
    {
        var original = File.Exists(Program.Paths.Settings) ? File.ReadAllBytes(Program.Paths.Settings) : null;
        var bounds = _platform.Bounds;
        var pinned = _vm.Pinned;
        var normal = _normalSettings;
        var startup = WindowStartupLocation;
        try
        {
            // Deliberately distinguish the current window from the retired 960 x 800 bounds.
            Width += 20; Height += 20;
            var expected = _platform.Bounds;
            File.WriteAllText(Program.Paths.Settings,
                """{"Width":960,"Height":800,"X":12,"Y":24,"Pinned":true,"ShellId":"card","LookId":"Card"}""");
            var legacy = _vm.LoadSettings();
            Check(legacy is { ShellId: "quiet", LookId: "system", Pinned: true, LayoutVersion: 0 }, "Legacy Card settings migration failed");
            RestoreWindowSnapshot();
            Check(_platform.Bounds == expected && WindowStartupLocation == startup && Topmost && _vm.Pinned,
                "Legacy bounds/startup changed or pin was lost");
            SaveWindowSnapshot();
            var saved = _vm.LoadSettings()!;
            Check(saved.LayoutVersion == ViewModels.MainViewModel.CurrentLayoutVersion && saved.Pinned &&
                saved.ShellId == "quiet" && saved.LookId == "system", "Migrated re-save lost version/pin/appearance");
            Width += 20; Height += 20;
            RestoreWindowSnapshot();
            Check(Width == saved.Width && Height == saved.Height && Position.X == saved.X && Position.Y == saved.Y,
                "Current layout bounds did not restore");
            foreach (var look in new[] { "system", "light", "dark", "Card Light", "Card Dark" })
            {
                File.WriteAllText(Program.Paths.Settings, System.Text.Json.JsonSerializer.Serialize(saved with { ShellId = "card", LookId = look }));
                var migrated = _vm.LoadSettings()!;
                Check(migrated.ShellId == "quiet" && migrated.LookId == (look == "Card Light" ? "light" : look == "Card Dark" ? "dark" : look),
                    "Theme preference changed during Card migration");
            }
            File.WriteAllText(Program.Paths.Settings, "{invalid");
            Check(_vm.LoadSettings() is null, "Malformed old settings were not tolerated");
            Console.WriteLine("M8b-retire-card-function SETTINGS PASS: legacy Card -> Quiet/System; old bounds ignored; pin kept; re-save version current; current bounds restored; theme preferences preserved; malformed JSON tolerated.");
        }
        finally
        {
            _platform.Bounds = bounds; _platform.Pinned = pinned; _vm.Pinned = pinned;
            _normalSettings = normal; WindowStartupLocation = startup;
            if (original is null) File.Delete(Program.Paths.Settings); else File.WriteAllBytes(Program.Paths.Settings, original);
            _vm.LoadSettings();
        }
    }
    private async Task RunSelfTest()
    {
        var code = 1;
        try
        {
            Contract.BoundaryCheck.Run();
            await Task.Delay(150);
            RunSettingsMigrationSmoke();
            ViewModels.ProjectionSmoke.Run(Path.Combine(Program.Paths.Root, "projections-" + Guid.NewGuid().ToString("N")));
            var unknownRejected = false;
            try { _layout.Button("", "Unbound probe", "unknown.probe"); }
            catch (ArgumentException ex) when (ex.ParamName == "id") { unknownRejected = true; }
            Check(unknownRejected && !_layout.Buttons.ContainsKey("Unbound probe"), "Unknown ID did not fail during layout construction");
            var renamed = _layout.Button("", "Renamed probe", "note.create");
            Check(ReferenceEquals(renamed.Command, _vm.CreateCommand), "Button command depended on its label");
            _layout.Buttons.Remove("Renamed probe"); _layout.CommandIds.Remove("Renamed probe");
            Console.WriteLine("COMMAND-BINDING PASS (unknown ID rejected during layout construction; label-independent command)");
            var cardRejected = false;
            try { _controller.Command("shell.card"); }
            catch (ArgumentException) { cardRejected = true; }
            SwitchShell("card");
            Check(cardRejected && _shellId == "quiet" && !EnsureSettings().Routes.ContainsKey("Card"), "Retired Card command/shell still reachable");
            Console.WriteLine("M8b-retire-card-function COMMAND PASS: Card rejected; Quiet retained.");
            Width = 960; Height = 800; // Stable isolated renderer fixtures; compact layout gets its own raster gate.
            EditorFactory.RunSmoke(); CarouselController.RunSmoke();
            foreach (var shell in new[] { "quiet" })
            {
                SwitchShell(shell);
                await RevealPresentationSmoke();
                // Selftest-only fault injection: restore the route even when Check throws.
                // Pin has only a toolbar route (Minimize moved to the settings window in M11b-settings-window).
                var routeProbe = Environment.GetEnvironmentVariable("ANTIQUICKPAD_CONFORMANCE_PROBE") == "missing-pin";
                var pin = _layout.Buttons["Pin"];
                try
                {
                    if (routeProbe) _layout.Buttons.Remove("Pin");
                    Contract.FunctionContract.Check(shell, LiveRoutes);
                }
                finally { if (routeProbe) _layout.Buttons["Pin"] = pin; }
                _editor.Text = "# Same note\nkeep this caret\n- [ ] task"; _editor.CaretOffset = 17;
                var id = _vm.Current.Id; var text = _editor.Text;
                var document = _editor.Document;
                SwitchShell("quiet");
                Check(_vm.Current.Id == id && _editor.Text == text && _editor.CaretOffset == 17, "Quiet selection lost note/caret");
                Check(ReferenceEquals(document, _editor.Document), "Quiet selection replaced the note document/undo history");
                SwitchShell(shell);
                var count = _vm.Notes.Count;
                Check(ReferenceEquals(SettingsEntry("New note"), _vm.CreateCommand) && ReferenceEquals(SettingsEntry("Delete note"), _vm.DeleteCommand), "Settings did not route to VM commands");
                SettingsEntry("New note").Execute(null); Check(_vm.Notes.Count == count + 1, "Settings New failed");
                SettingsEntry("Delete note").Execute(null); Check(_vm.Notes.Count == count, "Settings Delete failed");
                foreach (var theme in new[] { "light", "dark" })
                {
                    SettingsEntry(theme == "light" ? "Light" : "Dark").Execute(null);
                    Check(!_layout.Hint.IsVisible, "Nonempty hint after theme/shell rebuild");
                    Width = 480; Height = 560; await Task.Delay(50); UpdateLayout();
                    await AlignmentPixels(Path.Combine(Environment.GetEnvironmentVariable("ANTIQUICKPAD_ALIGNMENT_OUTPUT") ?? Path.Combine(AppContext.BaseDirectory, "../../../../../docs/design/m6/built/T015"), shell + "-" + theme));
                    await CompactPresentationSmoke();
                    Width = 960; Height = 800; await Task.Delay(50); UpdateLayout();
                    var nonempty = _vm.CurrentIndex;
                    _vm.CreateCommand.Execute(null);
                    Check(_layout.Hint.IsVisible, "Empty hint missing on navigation");
                    SwitchShell("quiet");
                    Check(_layout.Hint.IsVisible, "Empty hint missing after Quiet selection");
                    SwitchShell(shell);
                    _vm.Select(nonempty); Check(!_layout.Hint.IsVisible, "Stale empty hint on nonempty note");
                    _vm.Select(_vm.Notes.Count - 1); _editor.Text = "typed";
                    Check(!_layout.Hint.IsVisible, "Hint did not follow document insert");
                    _editor.Text = ""; Check(_layout.Hint.IsVisible, "Hint did not follow document deletion");
                    _carousel.BeginDrag(0, 0); _carousel.MoveDrag(100, 100);
                    Check(!_layout.Hint.IsVisible, "Empty source hint covered nonempty carousel preview");
                    _carousel.Cancel(); Check(_layout.Hint.IsVisible, "Empty hint did not return on cancel");
                    _vm.DeleteCurrent();
                    await Task.Delay(80); UpdateLayout(); _editor.TextArea.TextView.EnsureVisualLines();
                    var margin = _editor.TextArea.LeftMargins.OfType<ChevronMargin>().Single();
                    Check(margin.HitTargets.Count > 0 && margin.HitTargets.All(r => r.Width >= 24 && r.Height >= 24), "Fold hit area below 24x24");
                    _editor.Text = "# A\n## B\nb\n# C\nc"; await Task.Delay(80); UpdateLayout(); _editor.TextArea.TextView.EnsureVisualLines();
                    Check(margin.HitTargets.Count == 3, "Nested fold fixture expected three chevrons");
                    margin.Foldings.First().IsFolded = true; await Task.Delay(80); UpdateLayout(); _editor.TextArea.TextView.EnsureVisualLines();
                    Check(margin.HitTargets.Count == 2, "Hidden nested folds drew chevrons on the folded line");
                    margin.Foldings.First().IsFolded = false;
                    Check(_editor.FontFamily.Name == "Cascadia Mono" && CurrentLook.CodeFont == CurrentLook.EditorFont, "Editor font mismatch");
                    var proofDir = Path.Combine(Program.Paths.Root, "T014-" + shell + "-" + theme); Directory.CreateDirectory(proofDir);
                    _editor.Text = string.Join("\n", Enumerable.Range(1,6).Select(n => new string('#', n) + " Heading " + n));
                    await Task.Delay(80); UpdateLayout(); _editor.TextArea.TextView.EnsureVisualLines();
                    for (var level = 1; level <= 6; level++)
                    {
                        var visual = _editor.TextArea.TextView.VisualLines[level - 1];
                        var props = visual.Elements.Last().TextRunProperties;
                        Check(props.Typeface.FontFamily.Name == "Cascadia Mono" && props.FontRenderingEmSize == CurrentLook.HeadingSizes[level-1], "Heading font/size mismatch");
                        Check(((ISolidColorBrush)props.ForegroundBrush!).Color == CurrentLook.HeadingColors[level-1], "Heading colour mismatch");
                    }
                    using (var proof = new Avalonia.Media.Imaging.RenderTargetBitmap(new PixelSize((int)_editor.Bounds.Width, 240), new Vector(96,96)))
                    { proof.Render(_editor); proof.Save(Path.Combine(proofDir, "headings.png"), Avalonia.Media.Imaging.PngBitmapEncoderOptions.Default); }
                    Console.WriteLine($"T014 {shell}/{theme} six heading levels font/size/colour PASS");
                    // These legacy pixel readers assume an opaque editor-only RTB control surface.
                    // Production glass is verified separately; preserve their glyph/guide intent.
                    _editor.Background = new SolidColorBrush(CurrentLook.Background);
                    await AlignmentProof.Run(_editor, proofDir, CurrentLook);
                    await StrikeProof.Run(_editor, Path.Combine(proofDir, "strike.png"), CurrentLook);
                    // Isolate guide pixels from the independently checked caret-row background.
                    var row = _editor.TextArea.TextView.BackgroundRenderers.Single(r => r.GetType().Name == "ActiveLineRenderer");
                    _editor.TextArea.TextView.BackgroundRenderers.Remove(row);
                    await ActiveGuideProof.Run(_editor, Path.Combine(proofDir, "active-guide.png"), CurrentLook);
                    _editor.TextArea.TextView.BackgroundRenderers.Add(row);
                    Console.WriteLine($"T014 {shell}/{theme} PASS: hint bind/switch/edit, mono, 24x24 fold targets, checkbox/strike/guide pixels");
                    EditorPresentation.ClearGlassBackgrounds(_editor);
                    Check(((ISolidColorBrush)_editor.Background!).Color == Colors.Transparent &&
                        ((SolidColorBrush)TransparencyBackgroundFallback!).Color == ShellPalette.Get(shell, theme == "dark").Canvas,
                        "Glass editor/fallback theme mismatch");
                }
                SettingsEntry("System").Execute(null);
                _theme.SimulateSystem(false); var light = ((SolidColorBrush)TransparencyBackgroundFallback!).Color;
                _theme.SimulateSystem(true); Check(((SolidColorBrush)TransparencyBackgroundFallback!).Color != light, "System simulated update did not reach editor");
                _theme.SimulateSystem(null);
                await SettingsSmoke();
                _vm.Pinned = true; Check(Topmost, "Pin failed"); _vm.Pinned = false;
                Check(_vm.LoadSettings() is { } saved && saved.ShellId == shell && saved.LookId == "system", "Appearance persistence failed");
                var original = _editor;
                for (var i = 0; i < 3; i++)
                {
                    HideResident(); await Task.Delay(30);
                    _controller.FocusCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    ShowResident("selftest-" + shell);
                    var ms = await _controller.FocusCompletion.Task.WaitAsync(TimeSpan.FromSeconds(3));
                    Check(ms < 100 && ReferenceEquals(original, _editor), "Resident focus budget/editor identity failed");
                    Console.WriteLine($"T013 {shell} show-to-focus internal ms={ms:F3}");
                }
                Close(); Check(!IsVisible && !_quitting, "Close must hide"); ShowResident("selftest-close");
                _editor.Text = "debounced " + shell; _editor.CaretOffset = 7; await Task.Delay(500);
                Check(ViewModels.MainViewModel.VerifyCurrentText(Program.Paths.Notes, _editor.Text), "Autosave failed");
                _vm.CreateCommand.Execute(null); _vm.PreviousCommand.Execute(null);
                _carousel.RequestWheel(1, 1000); await Task.Delay(500);
                var frames = _carousel.Frames.ToArray();
                Check(frames.Length >= 5 && frames.Zip(frames.Skip(1)).All(pair => pair.Second.Ms >= pair.First.Ms && pair.Second.Offset <= pair.First.Offset + .001), "Frame offsets must be monotonic");
                foreach (var frame in frames) Console.WriteLine($"T014 FRAME {shell} ms={frame.Ms:F4} offset={frame.Offset:F6}");
                Check(_vm.CurrentIndex == _vm.Notes.Count - 1 && !_carousel.IsBusy, "Live carousel wheel failed");
                _carousel.BeginDrag(0, 2000); _carousel.MoveDrag(80, 2050);
                var releaseOffset = _carousel.Offset; _carousel.EndDrag(2200);
                Check(_carousel.Offset == releaseOffset, "Drag release jumped");
                Check(!_layout.Hint.IsVisible, "Hint covered preview");
                await Task.Delay(500);
                _vm.DeleteCommand.Execute(null);
                Console.WriteLine($"T013 {shell} PASS: switching/caret, palettes, simulated system update, menu VM routes, persistence, pin, resident focus, close/hide, autosave, carousel.");
            }
            // Retain Classic capabilities; interactive measurement is run separately on the real desktop.
            var start = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true };
            if (string.Equals(Path.GetFileNameWithoutExtension(Environment.ProcessPath),"dotnet", StringComparison.OrdinalIgnoreCase)) start.ArgumentList.Add(typeof(Program).Assembly.Location);
            foreach (var arg in new[] { "--selftest", "--shell", "classic" }) start.ArgumentList.Add(arg);
            using var child = Process.Start(start)!;
            var childOutput = child.StandardOutput.ReadToEndAsync();
            var childError = child.StandardError.ReadToEndAsync();
            await child.WaitForExitAsync().WaitAsync(TimeSpan.FromMinutes(5));
            Console.Write(await childOutput); Console.Error.Write(await childError);
            Check(child.ExitCode == 0, "Retained Classic selftest failed");
            Console.WriteLine("SELFTEST PASS: Quiet production shell and retained Classic capability suite.");
            Log("SELFTEST PASS"); code = 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("SELFTEST FAIL: " + ex); Log("SELFTEST FAIL: " + ex); }
        finally { Quit(code); }
    }
}






