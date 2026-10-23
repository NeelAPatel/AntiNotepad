using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace AntiNotepad.Platform;

/// <summary>Windows' built-in OCR on the clipboard image (from a screen snip or a copied picture). Nothing leaves the PC.</summary>
internal static class Ocr
{
    public static async Task<bool> ClipboardHasImage()
    {
        try { return Clipboard.GetContent().Contains(StandardDataFormats.Bitmap); }
        catch { await Task.CompletedTask; return false; }
    }

    public static async Task<(string? Text, string? Error)> ReadClipboardImage()
    {
        try
        {
            var content = Clipboard.GetContent();
            if (!content.Contains(StandardDataFormats.Bitmap)) return (null, "No image on the clipboard");
            var reference = await content.GetBitmapAsync();
            using var stream = await reference.OpenReadAsync();
            var decoder = await BitmapDecoder.CreateAsync(stream);
            using var bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            var engine = OcrEngine.TryCreateFromUserProfileLanguages();
            if (engine is null) return (null, "No OCR language installed (Settings › Time & language › Language)");
            var result = await engine.RecognizeAsync(bitmap);
            var text = string.Join("\n", result.Lines.Select(l => l.Text));
            return text.Trim().Length == 0 ? (null, "No text found in the image") : (text, null);
        }
        catch (Exception ex) { return (null, "OCR failed · " + ex.Message); }
    }
}
