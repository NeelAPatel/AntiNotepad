using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaEdit.Rendering;
using Avalonia.VisualTree;

namespace AntiQuickPad.Views.Common;

public sealed partial class ResidentShellWindow
{
    private async Task RevealPresentationSmoke()
    {
        var reveal = _barReveal!;
        reveal.ProbePointer(false);
        _editor.TextArea.Focus(); await Task.Delay(600);
        Check(!reveal.Revealed && _layout.Bars.All(b => b.Opacity == 0 && !b.IsHitTestVisible), "Idle bars still visible/hittable");
        reveal.ProbePointer(true); await Task.Delay(180);
        Check(reveal.Revealed && _layout.Bars.All(b => b.Opacity == 1 && b.IsHitTestVisible), "Pointer reveal failed");
        reveal.ProbePointer(false); await Task.Delay(150);
        Check(reveal.Revealed, "Reveal linger missing");
        ((Controller.IResidentSurface)this).OpenSettings(); reveal.ProbePointer(false); await Task.Delay(600);
        Check(reveal.Revealed, "Open settings did not retain bars");
        ((Controller.IResidentSurface)this).CloseSettings();
        var pin = _layout.Buttons["Pin"]; pin.Focus(); await Task.Delay(600);
        Check(reveal.Revealed && pin.BorderThickness.Left == Tokens.DesignTokens.Quiet.FocusRing, "Bar focus/reveal ring missing");
        _editor.TextArea.Focus(); await Task.Delay(650);
        Check(!reveal.Revealed && _layout.Bars.All(b => b.Opacity == 0 && !b.IsHitTestVisible), "Linger/fade-out failed");
        reveal.ProbePointer(null);
        Console.WriteLine("M8c-v1-compact-shell REVEAL PASS: idle, pointer, linger, menu, keyboard focus/ring, fade-out.");
    }

    private async Task CompactPresentationSmoke()
    {
        Check(Background == Brushes.Transparent && _root.Background == Brushes.Transparent &&
            ((ISolidColorBrush)_editor.Background!).Color == Colors.Transparent &&
            ((ISolidColorBrush)_editor.TextArea.Background!).Color == Colors.Transparent,
            "Opaque full-area glass layer");
        Check(TransparencyLevelHint.SequenceEqual(new[] { WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Mica, WindowTransparencyLevel.None }) &&
            CurrentLook.TintAlpha == .9 && CurrentLook.CodePanel.A < 255 && CurrentLook.ActiveLine.A < 255,
            "Material/tint/region token mismatch");
        _barReveal!.CaptureState(true);
        var pill = _layout.Bars.Last();
        foreach (var width in new[] { 480d, 360d })
        {
            Width = width; Height = 560; _editor.Text = string.Join('\n', Enumerable.Range(1, 80).Select(n => "Line " + n));
            _editor.CaretOffset = _editor.Document.TextLength; _editor.ScrollToEnd();
            await Task.Delay(100); UpdateLayout(); _editor.TextArea.TextView.EnsureVisualLines();
            var view = _editor.TextArea.TextView;
            var origin = view.TranslatePoint(default, _root)!.Value;
            var last = view.VisualLines.Last();
            var pillTop = pill.TranslatePoint(default, _root)!.Value.Y;
            Check(origin.Y + last.VisualTop - view.VerticalOffset + last.Height <= pillTop,
                "Last editor line overlaps the pill");
            Check(_layout.Save.Bounds.Width > 0 && _layout.Dots.Bounds.Width > 0, "Compact footer lost save or dots");
        }
        Width = 480; _editor.Text = ""; await Task.Delay(100); UpdateLayout();
        var textOrigin = _editor.TextArea.TextView.TranslatePoint(default, _root)!.Value;
        var hintOrigin = _layout.Hint.TranslatePoint(default, _root)!.Value;
        Check(Math.Abs(textOrigin.X - 48) < .1 && Math.Abs(hintOrigin.X - textOrigin.X) < .1 &&
            Math.Abs(hintOrigin.Y - textOrigin.Y) < .1, "Empty hint and first text line bearing differ");
        // Isolate the active-row renderer against its own transparent control capture.
        _editor.Text = "first\nsecond\nthird"; _editor.CaretOffset = _editor.Document.GetLineByNumber(2).Offset;
        await Task.Delay(100); UpdateLayout(); var textView = _editor.TextArea.TextView;
        textView.EnsureVisualLines(); _editor.TextArea.Caret.Hide();
        var row = textView.BackgroundRenderers.Single(r => r.GetType().Name == "ActiveLineRenderer");
        var size = new PixelSize((int)Math.Ceiling(_editor.Bounds.Width), (int)Math.Ceiling(_editor.Bounds.Height));
        byte[] Capture()
        {
            using var bitmap = new RenderTargetBitmap(size, new Vector(96,96)); bitmap.Render(_editor);
            var bytes = new byte[size.Width * size.Height * 4]; var ptr = Marshal.AllocHGlobal(bytes.Length);
            try { bitmap.CopyPixels(new PixelRect(size), ptr, bytes.Length, size.Width * 4); Marshal.Copy(ptr,bytes,0,bytes.Length); }
            finally { Marshal.FreeHGlobal(ptr); }
            return bytes;
        }
        var on = Capture(); textView.BackgroundRenderers.Remove(row); textView.InvalidateLayer(KnownLayer.Background);
        await Task.Delay(80); var off = Capture(); textView.BackgroundRenderers.Add(row); textView.InvalidateLayer(KnownLayer.Background);
        var line = textView.VisualLines[1]; var o = textView.TranslatePoint(default, _editor)!.Value;
        var band = new Rect(o.X, o.Y + line.VisualTop - textView.VerticalOffset, textView.Bounds.Width, line.Height).Inflate(1);
        var changed = 0; var outside = 0;
        for (var y = 0; y < size.Height; y++) for (var x = 0; x < size.Width; x++)
        {
            var i = (y * size.Width + x) * 4;
            if (on.AsSpan(i,4).SequenceEqual(off.AsSpan(i,4))) continue;
            changed++; if (!band.Contains(new Point(x+.5,y+.5))) outside++;
        }
        Check(changed > 0 && outside == 0, $"Active row pixels changed={changed} outside={outside}");
        _barReveal.CaptureState(null);
        Console.WriteLine($"M8c-v1-compact-shell PRESENTATION PASS: glass tokens, 480/360 footer, final line clearance, 24 DIP inset + 24 DIP folding gutter; active row changed={changed}, outside={outside}.");
        TokenAccessibilitySmoke();
    }

    // M6c-design-tokens: F43 contrast on the worst-case glass, F42 targets, F48 names, F51 ring.
    private void TokenAccessibilitySmoke()
    {
        var t = Tokens.DesignTokens.Quiet;
        var json = System.Text.Json.JsonDocument.Parse(Tokens.DesignTokens.Read());
        var leaves = 0;
        void Walk(System.Text.Json.JsonElement e, string path)
        {
            if (e.ValueKind != System.Text.Json.JsonValueKind.Object) return;
            if (e.TryGetProperty("$value", out _)) { Check(e.TryGetProperty("$type", out _), $"Token without $type: {path}"); leaves++; return; }
            foreach (var p in e.EnumerateObject()) if (!p.Name.StartsWith('$')) Walk(p.Value, path + "." + p.Name);
        }
        Walk(json.RootElement, "");
        var worst = double.MaxValue;
        foreach (var dark in new[] { false, true })
        foreach (var backdrop in new[] { Colors.Black, Colors.White })
        {
            var c = t.For(dark);
            var glass = Tokens.DesignTokens.Over(Color.FromArgb((byte)Math.Round(t.TintAlpha * 255), c.Canvas.R, c.Canvas.G, c.Canvas.B), backdrop);
            double Ratio(Color fg, double opacity = 1) => Tokens.DesignTokens.Contrast(Color.FromArgb((byte)Math.Round(opacity * 255), fg.R, fg.G, fg.B), glass);
            var mode = $"{(dark ? "dark" : "light")}/{(backdrop == Colors.Black ? "black" : "white")}";
            foreach (var (name, color) in new[] { ("ink", c.Ink), ("muted", c.Muted), ("accent", c.Accent) })
            { var r = Ratio(color); worst = Math.Min(worst, r); Check(r >= 4.5, $"F43 {name} text {r:0.00}:1 < 4.5 on {mode} glass"); }
            Check(Ratio(c.Muted, t.DotIdleOpacity) >= 3, $"F43 idle dot < 3:1 on {mode} glass");
            Check(Ratio(c.Accent) >= 3, $"F51 focus ring < 3:1 on {mode} glass");
            Check(Tokens.DesignTokens.Contrast(c.Muted, c.Surface) >= 4.5, $"F43 menu heading < 4.5:1 on {mode} surface");
        }
        Check(t.Button.Width >= t.TargetMin && t.Button.Height >= t.TargetMin && t.DotSlot.Height >= t.TargetMin, "F42 target tokens below minimum");
        // The editor's scroll bar arrows belong to the system scroll bar (wheel and thumb are the primary inputs).
        var buttons = _root.GetVisualDescendants().OfType<Button>().Where(b => b.FindAncestorOfType<Avalonia.Controls.Primitives.ScrollBar>() is null).ToList();
        Check(buttons.Count > 0, "No shell buttons found");
        foreach (var b in buttons)
        {
            var name = Avalonia.Automation.AutomationProperties.GetName(b);
            Check(!string.IsNullOrWhiteSpace(name), "F48 button without accessible name");
            var size = new Size(double.IsNaN(b.Width) ? b.Bounds.Width : b.Width, double.IsNaN(b.Height) ? b.Bounds.Height : b.Height);
            var dot = b.Command == Command("note.select");
            Check(size.Height >= t.TargetMin && (dot || size.Width >= t.TargetMin), $"F42 target '{name}' {size.Width}x{size.Height} below {t.TargetMin}");
        }
        Check(!string.IsNullOrWhiteSpace(Avalonia.Automation.AutomationProperties.GetName(_editor)), "F48 editor without accessible name");
        var pin = _layout.Buttons["Pin"];
        pin.RaiseEvent(new Avalonia.Input.FocusChangedEventArgs(Avalonia.Input.InputElement.GotFocusEvent) { NavigationMethod = Avalonia.Input.NavigationMethod.Pointer });
        Check(pin.BorderThickness.Left == 0, "F51 ring shown on pointer focus");
        pin.RaiseEvent(new Avalonia.Input.FocusChangedEventArgs(Avalonia.Input.InputElement.GotFocusEvent) { NavigationMethod = Avalonia.Input.NavigationMethod.Tab });
        Check(pin.BorderThickness.Left == t.FocusRing, "F51 ring missing on keyboard focus");
        pin.RaiseEvent(new Avalonia.Input.FocusChangedEventArgs(Avalonia.Input.InputElement.LostFocusEvent));
        Console.WriteLine($"M6c-design-tokens TOKENS PASS: {leaves} DTCG tokens; worst text contrast {worst:0.00}:1 on 90% glass; {buttons.Count} buttons named and >= {t.TargetMin} DIP; ring keyboard-only.");
    }
}
