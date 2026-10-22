# F25a-resident-backdrop-probe

Completed 2026-10-21 for F25-translucency-live-look / M8-antinote-ui-overhaul. Gate PASS.

## Built

DesignEntry now validates and delegates --probe-backdrop to BackdropProbeWindow.
Existing material, --report and --close-after-ms arguments remain supported; added
--theme light|dark and --tint 0-100 (defaults dark / 60). Invalid arguments return 2.
No packages, Program.cs changes, git commands, network, process kills or --measure.

Independent centred 960 x 800 window, minimum 600 x 480, BorderOnly,
ExtendClientAreaToDecorationsHint=true, title-bar height hint 58; CanResize=true.
The transparent top 58 DIP Border uses WindowDecorationProperties.SetElementRole
with WindowDecorationsElementRole.TitleBar, exactly the resident mechanism.
An editable in-memory sample has H1/H2, paragraph, bullets, checked/unchecked tasks,
strike and a fenced Python block. No resident controller, note store, tray, hotkey
or single-instance channel is created by the probe.

ShellPalette.Get("quiet", dark) supplies the shipping palette. The Look receives
the same ModernEditor / ActiveLine overrides as ResidentShellWindow.Appearance.
EditorFactory.Create(look, null, document) supplies the actual editor. Theme changes
rebuild its presentation while preserving the document, caret and viewport and
releasing the old editor. Editor positioning follows Quiet's 90 DIP horizontal/top
insets, with a 90 DIP bottom inset to leave the HUD clear.

## Keys and reporting

Ctrl+Alt+1 none; Ctrl+Alt+2 mica; Ctrl+Alt+3 acrylic; Ctrl+Alt+4 blur.
Ctrl+Alt+T toggles Quiet light/dark. Ctrl+Alt+Down/Up steps tint through
0, 15, 30, 45, 60, 75, 90, 100 (also accepts any integer 0-100 at launch).
Ctrl+Alt+H hides/shows the bottom-left HUD. Esc closes from anywhere; native close
also writes the final pick. Plain text typing retains normal editor input.

HUD: requested material -> actual level, theme, tint, shortcuts and the exact
wallpaper/inactive/move/resize/text-sharpness check hint from the task.
Reports stream to stdout and optional --report file with auto-flush: OS/Avalonia,
timestamped start, every material/theme/tint/HUD change, actual-level changes,
800 ms settled state and final state. --close-after-ms counts after settling as
before. Actual-level changes may produce an additional record during a material
switch; the final line is authoritative.

## Transparent layers and hypothesis

Window.Background and root Grid.Background are transparent. A single first-child
Border fills the root with Quiet Canvas RGB and the selected alpha (rounded to an
8-bit brush alpha); it is behind the editor, drag strip and HUD. Editor.Background
and TextArea.Background receive local transparent brushes. After template loading,
all descendant Panel, Border, TemplatedControl (including ScrollViewer) and
ContentPresenter backgrounds are cleared on this instance; repeated after theme
rebuilds. No shared styles or production palette values change.

TextView exposes no Background property in this AvaloniaEdit version; its drawing
surface has no full-area background brush to clear. Its existing localized Quiet
background renderers remain: code panel, active row, checkbox fills and guides.
They intentionally paint only their own regions; they can mask the backdrop in
those regions. There is no known uncleared full-window container layer. No
forbidden-folder edit was required. TransparencyBackgroundFallback is opaque Quiet
Canvas, used by the platform for unavailable material / none; it is not another
foreground tint layer. The HUD has no painted background.

Source evidence establishes that an opaque full-area canvas/editor background
occludes the backdrop in its covered region. It does not establish the cause of
the historical visual verdict. The live 0% vs 100% tint comparison, same material,
is the user's test of that hypothesis. Mica/Acrylic granted levels are capability
evidence only; visual quality and wallpaper response remain for live judgment.

## Gate evidence

- Graceful --quit from src/AntiQuickPad: exit 0.
- First compile found read-only TextView.ScrollOffset and absent TextView.Background;
  one fix used the existing EditorViewport.Restore and documented the absent brush.
- Final Release --no-restore -p:UseSharedCompilation=false, telemetry opt-out: exit 0,
  0 warnings / 0 errors. [build log](../scratch/F25a/build.log).
- --selftest: exit 0. BOUNDARY PASS (220 types); CONFORMANCE quiet PASS and
  CONFORMANCE card PASS (24 commands each); all four light/dark raster proofs and
  checkbox/strike/guide/heading pixel checks PASS. Expected values untouched.
  Existing conformance KNOWN-GAP window.move diagnostic remains followed by PASS.
  [full selftest log](../scratch/F25a/selftest.log).
- Alignment output used absolute .coord/scratch/F25a/align rather than the brief's
  .coord/scratch/align to honor the explicit scratch write allowlist. Built-in
  selftest capability proofs also use their existing bin/Release/net10.0/selftest
  output path. No test source/expected value changes.
- Exactly two visible probe launches; each exit 0, 800 ms settling + 600 ms close
  timer. Their reports are reproduced below. No extra probe launches.
- Design snapshot: .coord/scratch/F25a/Design/; completion marker:
  .coord/scratch/F25a/done. Initial source/docs snapshot: scratch/F25a/start/.
- README change confined to the probe section (prefix/suffix comparison PASS).
  STATUS and TRACKER left to manager: neither is in this task's write allowlist;
  no feedback priority/status changed here.

### Acrylic/dark, tint 45

```text
OS=Microsoft Windows 10.0.26340; Avalonia=12.1.4.0
2026-10-21T15:53:19.1820707+00:00 start: material=acrylic; requested=AcrylicBlur; actual=AcrylicBlur; theme=dark; tint=45; hud=True
2026-10-21T15:53:20.2141094+00:00 settled: material=acrylic; requested=AcrylicBlur; actual=AcrylicBlur; theme=dark; tint=45; hud=True
2026-10-21T15:53:20.8313071+00:00 final: material=acrylic; requested=AcrylicBlur; actual=AcrylicBlur; theme=dark; tint=45; hud=True
```

### Mica/light, tint 45

```text
OS=Microsoft Windows 10.0.26340; Avalonia=12.1.4.0
2026-10-21T15:53:26.3259534+00:00 start: material=mica; requested=Mica; actual=Mica; theme=light; tint=45; hud=True
2026-10-21T15:53:27.3518145+00:00 settled: material=mica; requested=Mica; actual=Mica; theme=light; tint=45; hud=True
2026-10-21T15:53:27.9676631+00:00 final: material=mica; requested=Mica; actual=Mica; theme=light; tint=45; hud=True
```

## Exact manager commands for the five-minute live check

From repository root, enter src/AntiQuickPad once; run one command at a time:

```powershell
Set-Location 'F:\LocalVSProj\Visual Studio Apps\AntiQuickPad\src\AntiQuickPad'
dotnet bin/Release/net10.0/AntiQuickPad.dll --probe-backdrop none --theme dark --tint 45 --report ../../.coord/scratch/F25a/live-none.txt
dotnet bin/Release/net10.0/AntiQuickPad.dll --probe-backdrop mica --theme light --tint 45 --report ../../.coord/scratch/F25a/live-mica.txt
dotnet bin/Release/net10.0/AntiQuickPad.dll --probe-backdrop acrylic --theme dark --tint 45 --report ../../.coord/scratch/F25a/live-acrylic.txt
dotnet bin/Release/net10.0/AntiQuickPad.dll --probe-backdrop blur --theme dark --tint 45 --report ../../.coord/scratch/F25a/live-blur.txt
```

Each command also allows switching all materials live. Over bright and dark content,
compare tint 0/45/100, both themes, inactive state, move/resize and text sharpness.
Hide the HUD for a clean look, then close with Esc when the chosen state is set.
Read the last `final:` line in stdout or the matching live report: `material`,
`actual`, `theme`, `tint` are the user's pick. If actual differs from requested,
the manager records both. These automated smoke reports are not the user's pick.
Physical drag/resize, interactive keys, live theme transitions, and visual-quality
judgment were not exercised by the two timed smoke launches.
