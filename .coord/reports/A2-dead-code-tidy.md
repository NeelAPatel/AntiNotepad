# A2-dead-code-tidy

## A1-21-zero-caller-api

Repo-wide C# `rg` searches, including `SelfTest.cs`, found only the listed declarations for `Anim.After` (`UI/Anim.cs:35`), `Kit.Segmented` and `Kit.Switch` (`UI/Kit.cs:102,149`), `Palette.B` and `Palette.Brush` (`Theme/Palette.cs:106-107`), `Modes.CodeLanguages` (`Model/Modes.cs:20`), and `Note.IsLoaded` (`Model/Store.cs:32`). All were removed. `Note.IsLoaded` was marked `[JsonIgnore]`.

`Store.Folder` (`Model/Store.cs:174`) was removed. The repo-wide `.Folder` search hits were `Program.cs:48` and `SelfTest.cs:72,92,111,127`, all uses of the migration result's `Folder`, not `Store.Folder`. `SingleInstance.WasAbandoned` (`Platform/SingleInstance.cs:24`) had only its declaration and assignment (`:43`); both were removed. No serialized `StoreData`, `Note`, or `Settings` property was removed.

## A1-23-leftover-icon-and-fence-state

`GlyphIcon.Snap` had only its declaration and read in `UI/Kit.cs`; removed the field and made the existing pixel snapping unconditional. Updated the glyph summary and replaced the stale icon-lab comment. `Render` draws once for `weight <= 0`; positive weight draws four offset copies plus the centered copy.

`Editor/Structure.cs`'s `Fence? open` was only initialized and checked, never assigned. Removed it and its check; `openLine` remains the fence discriminator.

## Files changed

- `product/AntiNotepad/UI/Anim.cs`
- `product/AntiNotepad/UI/Kit.cs`
- `product/AntiNotepad/Theme/Palette.cs`
- `product/AntiNotepad/Model/Modes.cs`
- `product/AntiNotepad/Model/Store.cs`
- `product/AntiNotepad/Platform/SingleInstance.cs`
- `product/AntiNotepad/Editor/Structure.cs`
- `.coord/reports/A2-dead-code-tidy.md`

## Validation

- Release build with telemetry opt-out and command-scoped `NuGetAudit=false`: **0 warnings, 0 errors**. The first prescribed build attempt compiled with two `NU1900` warnings because the vulnerability metadata endpoint at `127.0.0.1:9` was unreachable.
- `build/a2-check/AntiNotepad.exe --selftest`: **SELFTEST PASS**.
- Cleanup of `build/a2-check` remains incomplete: automatic approval review rejected both recursive-delete commands as blocked by policy.
