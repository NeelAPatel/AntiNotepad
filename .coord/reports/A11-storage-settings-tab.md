# A11-storage-settings-tab

## Changed

- `product/AntiNotepad/UI/SettingsWindow.cs`: added Storage to the Settings rail, moved notes-folder controls out of About, and added the recovered-notes and app-data rows.
- `product/AntiNotepad/UI/SettingsParts.cs`: added a read-only selectable path field using the Settings input styles.
- `product/AntiNotepad/Editor/PadView.cs`: replaced missing-note and unavailable-folder hints and styles those errors with the palette error color at reduced opacity.
- `product/AntiNotepad/Platform/NotesLocation.cs`: changed the recovery instructions from Settings › About to Settings › Storage.
- `docs/dev-loop.md`: no change; it contains no Settings › About reference.

## Storage page layout

1. **Notes folder** card: description, current path in a read-only selectable text field, inline status, then the actions for the current state.
2. **Other locations** card: Recovered notes row (path, file count or Empty, and Open when nonempty), then App data row (app-home path and Open).

## Notes-folder states

- **Normal:** Open and Change location…; description is “Your notes are plain files here, saved as you type.”
- **Pending:** shows “New location: <path>” with Migrate notes and Cancel.
- **Refused:** keeps the pending destination and displays the refusal reason inline in the error color; the user can retry or cancel.
- **Moved:** updates the current path and shows “Moved N notes. The old folder was kept as <backup name>.” Move count follows the same file set counted by `NotesMoveResult`; the backup name is identified from the directory created by the existing `Program.MoveNotes` operation. Any move warning is also shown inline.
- **Unavailable:** shows `Program.NotesLocationError` inline in the error color and offers Choose folder.

## Pad hints

Missing or unreadable note:

```text
Error: Note not found

Check recovered folder in Settings › Storage
```

Unavailable notes folder:

```text
Error: Notes folder not found

<Program.DataFolder>

Choose a folder in Settings › Storage
```

Both errors use the palette Error color at reduced opacity. The ordinary empty-note hint retains its faint color.

## Verification

- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a11-check`: passed, 0 warnings and 0 errors. `NuGetAudit=false` was set for the successful run after the default vulnerability-feed lookup returned NU1900 because `api.nuget.org` was unreachable.
- `build/a11-check/AntiNotepad.exe --selftest`: `SELFTEST PASS`.
- `git diff --check`: passed.
