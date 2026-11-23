# Architecture and data

The app keeps notes locally and separates editing, storage, platform integration and UI.

- The [product source](https://github.com/NeelAPatel/AntiNotepad/blob/main/product/AntiNotepad) uses `Model/` for notes, settings, storage and search; `Editor/` for the editor; `UI/` for windows and controls; `Calc/` for calculations; `Theme/` for visual tokens; and `Platform/` for Windows integration.
- The default notes folder is `Documents/AntiNotepad`; Settings can move it. Note bodies are Markdown files, with an index for app data.
- Saving is delayed after edits; hiding, switching notes and quitting request a save. An abrupt crash or power loss can still lose pending edits.
  - *Recovery and backup paths address particular failures; I do not treat them as a substitute for my own backups.*
- For development, [the dev loop](https://github.com/NeelAPatel/AntiNotepad/blob/main/docs/dev-loop.md) describes isolated slot copies and data. The app also has offline `--selftest` capability checks; they do not replace a hand test.
- See [[Settings and Shortcuts]] for the save-delay setting and [[Decisions and Trade-Offs]] for the choices behind this setup.
