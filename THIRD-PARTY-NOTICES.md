# Third-party notices

These components keep their own licenses. The NuGet licenses below were read from each package's own metadata (the `.nuspec` in the local NuGet cache) on 2026-11-23, for the 22 packages the app build resolves.

| Component | Version | License | Where |
|---|---|---|---|
| Avalonia, Avalonia.Desktop, Avalonia.Themes.Fluent, Avalonia.Skia, Avalonia.Win32, Avalonia.Native, Avalonia.X11, Avalonia.FreeDesktop (and `.AtSpi`), Avalonia.HarfBuzz, Avalonia.Remote.Protocol | 12.1.4 | MIT | NuGet |
| Avalonia.BuildServices | 11.3.2 | MIT | NuGet |
| SkiaSharp (and `.NativeAssets.Linux`, `.WebAssembly`, `.Win32`) | 3.119.4 | MIT | NuGet |
| HarfBuzzSharp (and `.NativeAssets.Linux`, `.WebAssembly`, `.Win32`) | 8.3.1.3 | MIT | NuGet |
| MicroCom.Runtime | 0.11.6 | MIT | NuGet |
| Tmds.DBus.Protocol | 0.94.1 | MIT | NuGet |
| Avalonia.Angle.Windows.Natives | 2.1.27548.20260419 | BSD-3-Clause (The ANGLE Project Authors); license text ships in the package | NuGet |
| AvaloniaEdit (vendored) | in repo | MIT, copyright Eli Arbel; see [license](third_party/AvaloniaEdit/LICENSE) | `third_party/AvaloniaEdit` |
| Fluent UI System Icons (font) | in repo | MIT, copyright Microsoft Corporation; see [license](product/AntiNotepad/Assets/Fonts/FluentSystemIcons-LICENSE.txt) | `product/AntiNotepad/Assets/Fonts` |

- The Linux, WebAssembly and macOS-style native packages are pulled in as dependencies of Avalonia and Skia, but the Windows build ships only the Windows parts.
- The portable release zip includes this file, the project license, and the AvaloniaEdit and Fluent icon licenses.
- If you notice a missing or wrong notice, please open an issue.
