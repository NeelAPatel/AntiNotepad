using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AntiQuickPad.Design;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct NativeRect(int Left, int Top, int Right, int Bottom)
{
    public override string ToString() => $"[{Left},{Top}..{Right},{Bottom}]";
}

internal sealed record ScreenCaptureResult(
    string File,
    NativeRect WindowRect,
    PixelSize Pixels,
    double DpiScale,
    byte[] Rgba);

internal static class ScreenCapture
{
    internal const int Margin = 24;

    internal static NativeRect WindowRect(Window window)
    {
        var handle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero || !GetWindowRect(handle, out var rect))
            throw new InvalidOperationException("Cannot read the native variant window rectangle.");
        return rect;
    }

    internal static ScreenCaptureResult Capture(Window window, string path, NativeRect backdropRect)
    {
        var windowRect = WindowRect(window);
        var captureRect = new NativeRect(windowRect.Left - Margin, windowRect.Top - Margin,
            windowRect.Right + Margin, windowRect.Bottom + Margin);
        if (captureRect.Left < backdropRect.Left || captureRect.Top < backdropRect.Top ||
            captureRect.Right > backdropRect.Right || captureRect.Bottom > backdropRect.Bottom)
            throw new InvalidOperationException($"Capture rectangle {captureRect} is outside the synthetic backdrop {backdropRect}.");

        var width = captureRect.Right - captureRect.Left;
        var height = captureRect.Bottom - captureRect.Top;
        if (width <= 0 || height <= 0) throw new InvalidOperationException("Invalid screen capture size.");

        var screen = GetDC(IntPtr.Zero);
        var memory = IntPtr.Zero;
        var bitmap = IntPtr.Zero;
        var old = IntPtr.Zero;
        try
        {
            if (screen == IntPtr.Zero) ThrowCaptureDenied("GetDC failed");
            memory = CreateCompatibleDC(screen);
            bitmap = CreateCompatibleBitmap(screen, width, height);
            if (memory == IntPtr.Zero || bitmap == IntPtr.Zero) ThrowCaptureDenied("GDI bitmap setup failed");
            old = SelectObject(memory, bitmap);
            if (!BitBlt(memory, 0, 0, width, height, screen, captureRect.Left, captureRect.Top, CopySource | CaptureBlt))
            {
                var error = Marshal.GetLastWin32Error();
                if (error == 5) ThrowCaptureDenied("BitBlt failed with access denied (error 5)");
                throw new InvalidOperationException($"Screen capture BitBlt failed with Win32 error {error}.");
            }
            var bytes = new byte[width * height * 4];
            var info = new BitmapInfo
            {
                Header = new BitmapInfoHeader { Size = (uint)Marshal.SizeOf<BitmapInfoHeader>(), Width = width,
                    Height = -height, Planes = 1, BitsPerPixel = 32, Compression = 0 }
            };
            if (GetDIBits(memory, bitmap, 0, (uint)height, bytes, ref info, 0) == 0)
                ThrowCaptureDenied("GetDIBits failed");
            WritePng(path, width, height, bytes);
            var dpi = GetDpiForWindow(window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero);
            return new(path, windowRect, new PixelSize(width, height), dpi == 0 ? 1 : dpi / 96d, bytes);
        }
        finally
        {
            if (old != IntPtr.Zero) SelectObject(memory, old);
            if (bitmap != IntPtr.Zero) DeleteObject(bitmap);
            if (memory != IntPtr.Zero) DeleteDC(memory);
            if (screen != IntPtr.Zero) ReleaseDC(IntPtr.Zero, screen);
        }
    }

    internal static double MeanDifference(byte[] left, byte[] right, out double changedFraction)
    {
        if (left.Length != right.Length) throw new ArgumentException("Glass-check images have different sizes.");
        long total = 0; var changed = 0;
        for (var i = 0; i < left.Length; i += 4)
        {
            var d = Math.Abs(left[i] - right[i]) + Math.Abs(left[i + 1] - right[i + 1]) + Math.Abs(left[i + 2] - right[i + 2]);
            total += d;
            if (d >= 9) changed++;
        }
        var pixels = left.Length / 4d;
        changedFraction = changed / pixels;
        return total / (pixels * 3d);
    }

    internal static IntPtr Foreground() => GetForegroundWindow();
    internal static void RestoreForeground(IntPtr handle)
    {
        if (handle != IntPtr.Zero) SetForegroundWindow(handle);
    }

    internal static void Activate(Window window)
    {
        var handle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (handle == IntPtr.Zero || !SetForegroundWindow(handle))
            throw new InvalidOperationException("Could not activate the variant window for Acrylic capture.");
    }

    private static void WritePng(string path, int width, int height, byte[] bytes)
    {
        var full = System.IO.Path.GetFullPath(path);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(full)!);
        using var bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        using (var frame = bitmap.Lock())
        {
            for (var row = 0; row < height; row++)
                Marshal.Copy(bytes, row * width * 4, frame.Address + row * frame.RowBytes, width * 4);
        }
        bitmap.Save(full, PngBitmapEncoderOptions.Default);
    }

    private static void ThrowCaptureDenied(string detail) =>
        throw new InvalidOperationException($"CAPTURE DENIED: run outside the sandbox ({detail}).");

    [StructLayout(LayoutKind.Sequential)] private struct BitmapInfo { public BitmapInfoHeader Header; public uint Colors; }
    [StructLayout(LayoutKind.Sequential)] private struct BitmapInfoHeader
    {
        public uint Size; public int Width; public int Height; public ushort Planes; public ushort BitsPerPixel;
        public uint Compression; public uint ImageSize; public int XPelsPerMeter; public int YPelsPerMeter;
        public uint ColorsUsed; public uint ColorsImportant;
    }
    private const uint CopySource = 0x00CC0020;
    private const uint CaptureBlt = 0x40000000;
    [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll", SetLastError = true)] private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool GetWindowRect(IntPtr hwnd, out NativeRect rect);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr CreateCompatibleBitmap(IntPtr dc, int width, int height);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern IntPtr SelectObject(IntPtr dc, IntPtr objectHandle);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern bool DeleteObject(IntPtr objectHandle);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern bool BitBlt(IntPtr destination, int x, int y, int width, int height, IntPtr source, int sourceX, int sourceY, uint rop);
    [DllImport("gdi32.dll", SetLastError = true)] private static extern int GetDIBits(IntPtr dc, IntPtr bitmap, uint start, uint lines, [Out] byte[] bits, ref BitmapInfo info, uint usage);
}
