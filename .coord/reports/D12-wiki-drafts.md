# D12-wiki-drafts report

## Files written

- `docs/wiki/Home.md`, `docs/wiki/_Sidebar.md`, `docs/wiki/Getting-Started.md`, `docs/wiki/Writing-and-Organizing.md`, `docs/wiki/Settings-and-Shortcuts.md`.
- `docs/wiki/Dev-Loop-Overview.md`, `docs/wiki/Roles-and-a-Worked-Example.md`, `docs/wiki/Building-with-AI-and-Lessons.md`, `docs/wiki/Architecture-and-Data.md`, `docs/wiki/Decisions-and-Trade-Offs.md`.
- This report. Existing wiki pages outside those names were left in place. No commit made.

## Claim-to-source table

| Claim | Source checked |
|---|---|
| Global default and configurable hotkeys | `product/AntiNotepad/Platform/GlobalHotkey.cs`, `Platform/HotkeyRegistrar.cs`, `Model/Settings.cs` |
| Tray access | `product/AntiNotepad/Platform/Tray.cs`, `Program.cs` |
| New note, navigation, search, split, Settings, F1, OCR and configurable shortcuts | `product/AntiNotepad/UI/Shortcuts.cs`, `UI/PadWindow.Keys.cs` |
| Stack, nine slots and split | `product/AntiNotepad/Model/Note.cs`, `UI/Shortcuts.cs`, `Model/Settings.cs` |
| List, timer and math examples; units and dates | `product/AntiNotepad/Model/Modes.cs`, `Calc/Calculator.cs`, `Calc/Calculator.Units.cs`, `Calc/Calculator.Dates.cs` |
| Search grid and Trash | `product/AntiNotepad/UI/SearchOverlay.cs`, `UI/Shortcuts.cs`, `Model/Settings.cs` |
| Appearance, storage, save-delay choices | `product/AntiNotepad/Model/Settings.cs`, `UI/SettingsWindow.cs`, `Platform/NotesLocation.cs`, `SelfTest/Store.cs` |
| Default local folder, Markdown bodies, index, recovery and saving | `product/AntiNotepad/Program.cs`, `Platform/NotesLocation.cs`, `Model/Store.cs`, `UI/PadWindow.cs`, `UI/PadWindow.Navigation.cs`, `SelfTest/Store.cs` |
| Source areas and self-test | `product/AntiNotepad/AntiNotepad.csproj`, `Program.cs`, `SelfTest/Run.cs`; directory contents |
| Portable unsigned release | Owner decision and `README.md`; release artifact not checked |
| Process, roles, eras and lessons | `docs/loop.md`, `docs/agents.md`, `docs/building-with-ai.md`, `docs/decisions.md`, `.coord/lessons.md` |
| Worked example and hand-test limits | `.coord/tasks/A21-save-delay-setting.md`, `.coord/reports/A21-save-delay-setting.worker.codex.md` |

## Links checked

- All wiki links in the ten drafts resolve to local named wiki files. All `blob/main` targets exist in this checkout; this does not establish that they are present on public `main` yet. Releases and Antinote are external destinations and were not fetched. No links point into `.coord/research/`.
- Markdown and target checks were local and offline; no rendered GitHub wiki or release was tested.

## Items to confirm

- Publish the ten pages to the GitHub wiki and confirm the named pages and links render there. Decide what to do with the older wiki pages left outside this assignment.
- Before publishing, ensure the linked item card, worker report and lessons log are included and sanitized in public `main`. The current worker report contains a local absolute path; the link must not expose it publicly before that is addressed.
- Confirm the first release includes the stated portable unsigned zip and accurate runtime instructions. Add the marked screenshots when available.
