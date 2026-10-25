# A36-drop-pill-lab: remove the A35 material lab; solid pill ships, top-bar blur stays

Role: worker (gpt-6-luna xhigh). Branch `feat/A36-drop-pill-lab` (this worktree, off dev). You cannot commit.
Product: `product/AntiNotepad`. Read `AGENTS.md` and the newest entry in `docs/decisions.md` ("Bar insets and chrome tokens").

## Goal
A35-bar-insets shipped with a dev-only Lab (Settings > UI > Lab: "Pill material: Solid / Blur behind"). The user picked
**Solid**. Remove the lab and everything only it needs; keep the shipped behavior identical.

## Work
1. Delete the Lab group/row in `UI/SettingsWindow.cs`.
2. In `UI/PadWindow.MaterialLab.cs`, `UI/PadWindow.Layout.cs`, `UI/PadWindow.cs`: remove the pill blur option and its state
   (`LabPillMaterial`/`SetLabPillMaterial`, `_labPillMaterial`, pill blur group/visual host/tint, pill sampling, the
   `BuildPill` visual-host branch: the pill's child is just the row again). The pill stays solid palette Surface in every
   style and material, exactly as now.
3. Keep the top-bar blur behind (overlay/float) exactly as it behaves now, including the editor opacity-mask cooperation
   with `PadView`, freshness observers, software rendering and split view.
4. The word "lab" goes: rename the file to `UI/PadWindow.TopBlur.cs` and the remaining members from `Lab*`/`_lab*` to plain
   names describing the top-bar blur (e.g. `InitializeTopBlur`, `ApplyTopBlur`, `_topBlurLeft`). Remove the dev-only
   `bar-lab-render` telemetry (timer, counters, CPU sampling, `MeasureCpuRender`) unless something else needs it.
   `ChromeTokens.TopBackdrop.BlurTintAlpha` (0.78, pill-only) goes if unused.
5. Match the surrounding code style and comment density; no behavior changes beyond removing the lab.

## Constraints
- Do not touch window frame styles, DWM attributes, show/hide paths, or `ChromeTokens` geometry values.
- Never launch or kill AntiNotepad. Build into `build/A36-drop-pill-lab-check`
  (`AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Debug -o build/A36-drop-pill-lab-check -p:AntiNotepadDev=true -p:NuGetAudit=false`),
  run `--selftest` with `ANTINOTEPAD_DATA` under `build/`.

## Done means
- 0 warnings, SELFTEST PASS; `grep -ri "lab" product/AntiNotepad/UI/PadWindow*.cs product/AntiNotepad/UI/SettingsWindow.cs`
  finds nothing from A35 (unrelated words like "label" are fine).
- Short report at `.coord/reports/A36-drop-pill-lab.md`: files changed, lines removed, anything you were unsure about.
