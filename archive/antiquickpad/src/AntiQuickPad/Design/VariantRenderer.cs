using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace AntiQuickPad.Design;

internal sealed class VariantRenderer
{
    private readonly VariantRenderOptions? _renderOptions;
    private readonly GlassCheckOptions? _glassOptions;

    internal VariantRenderer(VariantRenderOptions options) => _renderOptions = options;
    internal VariantRenderer(GlassCheckOptions options) => _glassOptions = options;

    internal void Start(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var backdrop = new WallpaperWindow();
        desktop.MainWindow = backdrop;
        backdrop.Opened += async (_, _) =>
        {
            var exitCode = 0;
            try
            {
                backdrop.FitToPrimary();
                backdrop.UpdateLayout();
                await Task.Delay(120);
                if (_glassOptions is { } glass)
                {
                    await RunGlassCheck(backdrop, glass.Output);
                }
                else if (_renderOptions is { } render)
                {
                    await RunRender(backdrop, render);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                exitCode = 1;
            }
            finally
            {
                if (backdrop.IsVisible) backdrop.Close();
                desktop.Shutdown(exitCode);
            }
        };
    }

    private async Task RunRender(WallpaperWindow backdrop, VariantRenderOptions options)
    {
        Directory.CreateDirectory(options.Output);
        var activate = false;
        if (!options.NoCapture)
        {
            var scratch = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"AntiQuickPad-M8a-glass-{Environment.ProcessId}");
            var glass = await RunGlassCheck(backdrop, scratch);
            activate = glass.ActivateForCapture;
        }
        else
        {
            Console.WriteLine("GLASS CHECK: skipped for --no-capture; capture activation path is not selected.");
        }

        var entries = new List<ManifestEntry>();
        var specs = options.Only is null
            ? VariantRenderSpec.All
            : [VariantRenderSpec.Get(options.Only)];
        foreach (var spec in specs)
        {
            foreach (var dark in new[] { false, true })
            foreach (var hover in new[] { false, true })
            {
                var window = new VariantRenderWindow(spec, dark, hover);
                try
                {
                    await ShowSettled(window);
                    window.AssertAcrylic();
                    window.PrepareForCapture();
                    var file = options.NoCapture ? null : System.IO.Path.Combine(options.Output, spec.Id, window.StateId + ".png");
                    ScreenCaptureResult? capture = null;
                    var previousForeground = IntPtr.Zero;
                    try
                    {
                        if (activate)
                        {
                            previousForeground = ScreenCapture.Foreground();
                            ScreenCapture.Activate(window);
                            await Task.Delay(35);
                        }
                        if (!options.NoCapture)
                            capture = ScreenCapture.Capture(window, file!, backdrop.NativeBounds);
                    }
                    finally
                    {
                        if (activate) ScreenCapture.RestoreForeground(previousForeground);
                    }
                    var pixels = capture is { } shot
                        ? new PixelSizeEntry(shot.Pixels.Width, shot.Pixels.Height)
                        : CapturePixels(window);
                    var dpi = capture?.DpiScale ?? WindowDpi(window);
                    var relative = file is null ? null : System.IO.Path.GetRelativePath(options.Output, file).Replace('\\', '/');
                    entries.Add(new ManifestEntry(spec.Id, window.StateId,
                        new DipSize(spec.Width, spec.Height), new PixelSizeEntry(pixels.Width, pixels.Height),
                        window.ActualTransparency, dpi, relative));
                    Console.WriteLine($"RENDER variant={spec.Id} state={window.StateId} dip={spec.Width:0}x{spec.Height:0} pixels={pixels.Width}x{pixels.Height} dpi={dpi:0.##} actual={window.ActualTransparency} file={(relative ?? "<none>")}");
                }
                finally
                {
                    if (window.IsVisible) window.Close();
                    await Task.Delay(35);
                }
            }
        }
        WriteManifest(options.Output, entries);
        Console.WriteLine($"RENDER VARIANTS PASS: variants={specs.Count}; states={entries.Count}; output={options.Output}; activation={(activate ? "foreground-restored" : "inactive")}; capture={(options.NoCapture ? "skipped" : "screen")}");
    }

    private async Task<GlassDecision> RunGlassCheck(WallpaperWindow backdrop, string output)
    {
        Directory.CreateDirectory(output);
        var spec = VariantRenderSpec.Get("V1-compact-pill");
        ScreenCaptureResult? tint0 = null;
        ScreenCaptureResult? tint100 = null;
        foreach (var tint in new[] { 0, 100 })
        {
            var window = new VariantRenderWindow(spec, dark: false, hover: false, tint);
            try
            {
                await ShowSettled(window);
                window.AssertAcrylic();
                window.PrepareForCapture();
                var file = System.IO.Path.Combine(output, $"V1-compact-pill-light-idle-tint{tint}.png");
                var capture = ScreenCapture.Capture(window, file, backdrop.NativeBounds);
                if (tint == 0) tint0 = capture; else tint100 = capture;
                Console.WriteLine($"GLASS tint={tint} pixels={capture.Pixels.Width}x{capture.Pixels.Height} dpi={capture.DpiScale:0.##} actual={window.ActualTransparency} file={file}");
            }
            finally
            {
                if (window.IsVisible) window.Close();
                await Task.Delay(35);
            }
        }
        var mean = ScreenCapture.MeanDifference(tint0!.Rgba, tint100!.Rgba, out var changed);
        var clear = mean >= 4 && changed >= .08;
        var decision = new GlassDecision(mean, changed, clear, !clear);
        var metricsPath = System.IO.Path.Combine(output, "glass-check.json");
        File.WriteAllText(metricsPath, JsonSerializer.Serialize(decision, JsonOptions));
        Console.WriteLine($"GLASS CHECK {(clear ? "PASS" : "FALLBACK")}: mean-channel-difference={mean:0.###}; changed-fraction={changed:P1}; path={(clear ? "inactive" : "activate-before-capture")}; metrics={metricsPath}");
        return decision;
    }

    private static async Task ShowSettled(VariantRenderWindow window)
    {
        var opened = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        window.Opened += (_, _) => opened.TrySetResult();
        window.Show();
        await opened.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Task.Delay(650);
    }

    private static PixelSizeEntry CapturePixels(VariantRenderWindow window)
    {
        var rect = ScreenCapture.WindowRect(window);
        return new PixelSizeEntry(rect.Right - rect.Left + ScreenCapture.Margin * 2, rect.Bottom - rect.Top + ScreenCapture.Margin * 2);
    }

    private static double WindowDpi(Window window)
    {
        var handle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        return handle == IntPtr.Zero ? 1 : GetDpiForWindow(handle) / 96d;
    }

    private static void WriteManifest(string output, IReadOnlyList<ManifestEntry> entries)
    {
        var manifest = new { generatedAtUtc = DateTimeOffset.UtcNow, entries };
        File.WriteAllText(System.IO.Path.Combine(output, "render-manifest.json"), JsonSerializer.Serialize(manifest, JsonOptions));
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private sealed record ManifestEntry(string Variant, string State, DipSize SizeDip, PixelSizeEntry Pixels,
        string ActualTransparencyLevel, double DpiScale, string? File);
    private sealed record DipSize(double Width, double Height);
    private sealed record PixelSizeEntry(int Width, int Height);
    private sealed record GlassDecision(double MeanChannelDifference, double ChangedFraction, bool ClearDifference, bool ActivateForCapture);

    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern uint GetDpiForWindow(IntPtr hwnd);
}
