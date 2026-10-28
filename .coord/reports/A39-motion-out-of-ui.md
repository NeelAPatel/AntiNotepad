# A39-motion-out-of-ui

## Changes

- Moved `Anim` unchanged from `UI/Anim.cs` to `Motion/Anim.cs` in `AntiNotepad.Motion`; updated the editor and UI callers.
- Moved `ChromeTokens` unchanged from `UI/Chrome.cs` to `Theme/ChromeTokens.cs` in `AntiNotepad.Theme`; layout and backdrop values are unchanged.
- Moved `MenuEntry` and the menu glyph IDs to `Editor/MenuEntry.cs`. `PadView` now sends its anchor and entries through `ShowMenu`; `PadWindow.NewPane` assigns a delegate that calls `PadMenu.Show(anchor, entries, _palette, Settings)`. `PadMenu` still owns popup rendering, action-row behavior, icon coloring, shortcuts, and settings reads. `AppMenu` still appends the same application entries.
- Removed the unused `using AntiNotepad.UI` from `Editor/Elements.cs`.

## Layer selftest

`SelfTest` reflects every type in the `AntiNotepad.Editor` namespace, including nested/compiler-generated types. It collects base/interface, field, property, event, method/constructor, and generic signature types, then scans method IL and resolves type, method, and field tokens. Scan errors fail the selftest. The rules are table-driven:

| Forbidden dependency | Result |
| --- | ---: |
| Editor -> UI | 0 refs |
| Model -> UI | 0 refs |
| Model -> Editor | 0 refs |
| Calc -> any other AntiNotepad namespace | 0 refs |
| Theme -> UI | 0 refs |
| Theme -> Editor | 0 refs |
| Platform -> UI | 0 refs |
| Platform -> Editor | 0 refs |

Negative proof: I temporarily added an Editor method returning `typeof(AntiNotepad.UI.PadWindow)`. The selftest printed `LAYERS Editor->UI 1 refs`, then `SELFTEST FAIL` and `layer violation Editor->UI: AntiNotepad.Editor.PadView -> AntiNotepad.UI.PadWindow`. I removed the probe and reran the clean selftest successfully.

## Verification and limits

- Build to `build/A39-check`: 0 warnings, 0 errors. `AVALONIA_TELEMETRY_OPTOUT=1` was set. The build used `-p:NuGetAudit=false` because the sandbox could not reach nuget.org for vulnerability-feed data.
- `build/A39-check/AntiNotepad.exe --selftest`: `SELFTEST PASS`; all eight layer lines report 0 refs.
- `rg -n "AntiNotepad\.UI" product/AntiNotepad/Editor`: no matches.
- No forbidden rule was omitted. No packages were added. No commit was made.
- I did not run the live GUI in this worker task. The popup construction code is unchanged; the manager can perform the dev-copy visual check.
