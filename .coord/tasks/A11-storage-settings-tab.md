# A11-storage-settings-tab

You are a worker (gpt-6-luna xhigh). Repo: this folder, a git worktree on branch `feat/A11-storage-settings-tab`. Never switch branch,
merge or commit (.git is read-only for you). App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). Read `AGENTS.md`.
Context: A7-notes-folder-and-recovery added a movable notes folder (`Platform/NotesLocation.cs`, `Program.MoveNotes`) and put its
controls in Settings › About. The user tested it and asked for the changes below. Match the existing Settings look (Card, Row, Group,
TextButton helpers in `UI/SettingsParts.cs`), light and dark. First-party only.

## 1. New "Storage" section in Settings
- Add a **Storage** entry to the Settings rail (`Sections` in `UI/SettingsWindow.cs`), just before About. Give it a fitting icon from
  the bundled Fluent font, in the same style as the other entries. Move all the file-path rows there, and remove the Notes folder
  row from About.
- **Notes folder** card, laid out the standard way:
  - The current path in a **read-only, selectable text field** (a TextBox using the existing input styles, IsReadOnly; the user
    can select and copy it).
  - Beside or under it, an **Open** button and a **Change location…** button. Change location uses the folder picker.
  - When the picked folder is **different from the current one**, show it as the pending new location with a
    **Migrate notes** button (the existing verified move, `Program.MoveNotes`) and **Cancel**. If the same folder is picked,
    nothing happens.
  - If the move is refused (the folder already holds notes, or is nested), show the reason **inline** under the field in the error
    color, not only as a toast.
  - After a successful move, the field shows the new path and a short success line: "Moved N notes. The old folder was kept as
    <backup name>." Use what `NotesMoveResult` provides.
  - Description: "Your notes are plain files here, saved as you type."
- **Recovered notes** row: the recovered folder path, the number of files in it ("2 files" or "Empty"), and **Open** (disabled
  or hidden when empty). Description: "Note files the app couldn't place. Open them in any text editor."
- **App data** row: the app-home path (`Program.AppHome`: log and folder pointer) with **Open**. Description: "Log file and the
  pointer to your notes folder."
- When the notes folder is unavailable (`Program.NotesLocationError`), the Notes folder card shows that error inline in the error
  color, and the button reads **Choose folder**.

## 2. Shorter, red error hints in the pad
`Editor/PadView.cs` (the `_loadHint` text and the `_hint` styling only):
- **Missing or unreadable note:** two lines:
  ```
  Error: Note not found

  Check recovered folder in Settings › Storage
  ```
- **Notes folder unavailable:** `Error: Notes folder not found` + blank line + the path + blank line + `Choose a folder in Settings › Storage`.
- Both are drawn in a **dim red**: the palette's `Error` color at reduced opacity, readable on light and dark palettes. The normal
  empty-note hint keeps its current faint color.
- Update the "Settings › About" strings in `Platform/NotesLocation.cs` to "Settings › Storage".

## Files in scope
`UI/SettingsWindow.cs`, `UI/SettingsParts.cs`, `Editor/PadView.cs` (hint text and color only), `Platform/NotesLocation.cs` (strings
only), `docs/dev-loop.md` (one line if it names About). Other workers are editing PadView's list and completion code, OCR, timers
and the calculator in parallel: stay out of those.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a11-check`: 0 errors, 0 warnings. `--selftest`
  PASS (update the selftest only if a string it checks changed). Never launch or kill AntiNotepad.
- Don't edit `.coord/STATUS.md` or the audit doc.
- Report at `.coord/reports/A11-storage-settings-tab.md`: the files changed, the Storage page layout top to bottom, each state
  (normal, pending, refused, moved, unavailable), and the hint texts.
