# spike — Avalonia + AvaloniaEdit

Default launch is the mouse-driven scratchpad prototype. Diagnostic mode is `--probe`. Dependencies are already cached; no new package is required.

From the repository root:

```powershell
$env:AVALONIA_TELEMETRY_OPTOUT = '1'
dotnet build .\spikes\A\InputProbe.csproj --no-restore -p:UseAppHost=false -p:OutDir=bin/verify/retest/
dotnet .\spikes\A\bin\verify\retest\InputProbe.dll
```

Use the DLL command, not the old Debug executable. An old user-owned diagnostic process may still lock that executable; this output avoids it. The application finds spikes/A by its project file regardless of output-directory depth.

Controls:
- Horizontal wheel: first notch starts one 220 ms cubic page slide. Quick bursts coalesce; no page queue or repeated same-direction restart. Burst ends after slide completion and a 140 ms idle gap. Reversal retargets continuously. Positive Avalonia delta requests newer notes; hi-res/inertial drivers remain untested.
- Blue `DRAG NOTES HERE` strip: left-button drag with pointer capture and real release; a short slow drag springs back. Editor mouse selection is kept separate.
- Pull left beyond the newest note by more than 170 logical pixels to create one note. Resistance is capped near 180 px; one wheel burst beyond newest commits one new note. `+ Note` / Ctrl+N also adds a note. Blank interior notes are removed when creating another note; newest blank remains.
- Checkbox source markers show a Hand cursor and toggle tooltip on hover, and are clickable. Ctrl+Z undoes changes. Tab/Shift+Tab handles caret-only and selected lines; outdent removes up to four spaces or one tab. Fixture keeps Markdown markers visible and colors heading levels, emphasis and Python fence keyword/string tokens.
- `Fold code` installs/removes folding margins for current fences; click the margin chevron to fold. Line numbers are enabled. Folding definitions are a one-shot probe, not maintained live after document edits.
- Material selector requests AcrylicBlur, Mica or Transparent; footer records actual value.
- Escape / Hide retains the initialized window. Tray click/menu always shows and focuses. Ctrl+Alt+Shift+A toggles globally when registered. Footer/tray tooltip shows the chord or unavailable; Exit older copies to release it. Exit quits.

Notes autosave synchronously to `spikes/A/data/notes.json` via temporary-file replacement. Content/order restore on launch; current index/undo history do not persist. This disposable store is not a recovery system: keep test data disposable. Data/logs/build outputs are ignored by git.

Small checks:

```powershell
dotnet .\spikes\A\bin\verify\retest\InputProbe.dll --smoke       # 12 model/editor/store checks
dotnet .\spikes\A\bin\verify\retest\InputProbe.dll --ui-smoke    # temporary notes, opens, checks, renders preview, exits
dotnet .\spikes\A\bin\verify\retest\InputProbe.dll --probe       # raw Avalonia / native input diagnostic
```

UI smoke logs focus, template/layout, programmatic insert/undo, material actual values, memory sample and method-level hide/show dispatch. Its preview excludes Windows compositor backdrops. Logs/previews go to spikes/A/logs; the UI smoke JSON is temporary and deleted on exit. These checks do not prove physical typing, mouse feel, hotkey delivery, tray accessibility or text clarity on desktop materials. Use docs/spikes/A/MANUAL-CHECKS.md and the CURRENT instructions at the end of .coord/status-A.md.
