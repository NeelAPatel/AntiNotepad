# UI variants demo (Avalonia 12.1.4 + AvaloniaEdit 12.0.0)

Real Avalonia renders of five chrome-free styles plus a carousel frame. Disposable demo; not wired into spike A.

```powershell
cd demos/ui-variants
dotnet restore
dotnet build
dotnet bin/Debug/net10.0/UiVariants.dll --render ink out/ink.png     # variants: ink paper terminal slate midnight carousel
dotnet bin/Debug/net10.0/UiVariants.dll --show paper                  # live window (no render)
./render-all.ps1                                                      # build + all six PNGs
```

`--render` opens the real window, waits ~0.9 s for layout, then calls `RenderTargetBitmap.Render` on the root visual (same technique as spike A's `render-preview.png`). Output is 1100x760 at 96 dpi.

Files: `Looks.cs` (all palettes/type/switches), `EditorParts.cs` (colorizer, code panel, checkbox generator, gutter), `VariantWindow.cs` (layout and render), `Program.cs`, `App.*`.
