using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AntiQuickPad.Editor;

namespace AntiQuickPad.Views.Common;

internal static class ShellRender
{
    private static string[]? _args;
    internal static bool IsRendering => _args is not null;
    public static void Initialize(string[] args) => _args = args;
    public static async Task Capture(ResidentShellWindow window)
    {
        var code = 1;
        try { await window.RenderProductionStates(_args![2], Path.GetFullPath(_args[3])); code = 0; }
        catch (Exception ex) { Console.Error.WriteLine(ex); }
        window.Quit(code, "render");
    }
}

public sealed partial class ResidentShellWindow
{
    internal async Task RenderProductionStates(string theme, string output)
    {
        Directory.CreateDirectory(output);
        for (var i = _vm.Notes.Count; i < 8; i++) _vm.CreateCommand.Execute(null);
        for (var i = 0; i < 8; i++)
        {
            _vm.Select(i); _editor.Text = i == 3 ? "# Make it feel effortless\n\nThe best tools get out of the way.\n\n## Next time\n- [ ] Try the shortcut without looking\n- [ ] Read a long note at 150% scaling\n" : Design.DesignWindow.Fixture;
        }
        _vm.Select(2); SetTheme(theme);
        Width = 480; Height = 560;
        _root.Width = 480; _root.Height = 560;
        _frame.Child = null;
        var capture = new Viewbox { Stretch = Stretch.Fill, Child = _root }; Content = capture;
        foreach (var state in new[] { "main", "bars-hover", "settings-open", "carousel", "fold-hover", "empty" })
        {
            _carousel.Cancel();
            if (state == "empty") { _vm.CreateCommand.Execute(null); }
            else { _editor.CaretOffset = _editor.Document.GetLineByNumber(13).Offset + 20; }
            SaveNotes();
            _barReveal!.CaptureState(state != "main");
            if (_layout.Buttons.TryGetValue("Settings", out var more)) more.Background = state is "bars-hover" or "settings-open" ? new SolidColorBrush(CurrentLook.CodePanel) : Brushes.Transparent;
            await Task.Delay(100); UpdateLayout(); _editor.TextArea.Focus(); _editor.TextArea.TextView.EnsureVisualLines();
            if (state == "carousel") { _carousel.BeginDrag(0, 0); _carousel.MoveDrag(-350, 100); }
            var margin = _editor.TextArea.LeftMargins.OfType<ChevronMargin>().Single();
            margin.HoverAt(state == "fold-hover" && margin.HitTargets.Count > 0 ? margin.HitTargets[0].Center : null);
            foreach (var scale in new[] { 1d, 1.5d })
            {
                capture.Width = 480 * scale; capture.Height = 560 * scale;
                if (state == "empty") _editor.TextArea.Caret.Hide(); else _editor.TextArea.Caret.Show();
                await Task.Delay(30);
                capture.Measure(new Size(480 * scale, 560 * scale)); capture.Arrange(new Rect(0, 0, 480 * scale, 560 * scale));
                using var bitmap = new RenderTargetBitmap(new PixelSize((int)(480 * scale), (int)(560 * scale)), new Vector(96, 96));
                bitmap.Render(capture);
                var path = Path.Combine(output, state + (scale == 1 ? "@1x.png" : "@1.5x.png")); bitmap.Save(path, PngBitmapEncoderOptions.Default); Console.WriteLine(path);
            }
            if (state == "fold-hover")
            {
                using var closeup = new RenderTargetBitmap(new PixelSize(700, 220), new Vector(96,96));
                closeup.Render(_editor); closeup.Save(Path.Combine(output, state + "-closeup.png"), PngBitmapEncoderOptions.Default);
            }
            if (state == "settings-open")
            {
                // The settings window is a separate (hidden in renders) window; RTB shows its content without the backdrop.
                OpenSettingsWindow(); var settings = _settings!;
                settings.Width = 320; settings.Height = 560; await Task.Delay(150); settings.UpdateLayout();
                var size = new PixelSize(320, 560);
                using var shot = new RenderTargetBitmap(size, new Vector(96, 96));
                shot.Render(settings); shot.Save(Path.Combine(output, "settings-window.png"), PngBitmapEncoderOptions.Default);
                settings.Hide();
            }
        }
    }
}

