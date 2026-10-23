# I6-win11-menu-toolbar-in-app

You are the director (gpt-6.1-sol medium); delegate to gpt-6-luna xhigh subagents where useful.
Repo: this folder. Branch `claude/chrome-overlays` is checked out: work here, never switch branch or merge.
App: `product/Antipad` (Avalonia 12 + AvaloniaEdit 12, .NET 10). US spelling in UI text.

## Goal
The user approved the Windows 11 toolbar and right-click menu designed in the Icon Lab. Put that exact design into the app.
The approved reference is the lab on the other branch; read it with
`git show claude/icon-lab:product/Antipad/UI/IconLab.cs` (do not check that branch out). Match its proposed menu
(inline preview and "Open as real popup") and toolbar preview pixel for pixel: panel, insets, row heights, columns, hover,
separators, action row tiles, content-sized width, colours, text sizes and fonts. Do not bring IconLab.cs itself here.

Locked recipe: Segoe Fluent Icons (`Glyph.Font`), 16 px, colour `p.Ink`, drawn once: `new GlyphIcon(glyph, p.Ink, 16, 0)`
(GlyphIcon in UI/Kit.cs already draws a weight-0 glyph once and snaps to whole pixels).

### 1. Right-click menu (replace the current Avalonia ContextMenu)
- Files: `UI/PadMenu.cs` (renderer, `MenuEntry`), `Editor/PadView.cs` (`ShowMenu` builds the entries), `UI/PadWindow.Layout.cs`
  (`pane.AppMenu` app items). Keep the `MenuEntry` model and the AppMenu hook; replace how `PadMenu.Show` draws: build the
  approved menu as a real popup at the pointer (Flyout or Popup), grayscale text (`TextOptions.SetTextRenderingMode(x, TextRenderingMode.Antialias)`),
  closes on Esc, click outside, or after an item runs. Colours from the pad's palette (`Palette p`), light and dark.
- Order (approved): Undo, Redo, Select all | Cut, Copy, Paste, Delete | New note, Search notes, Split view (or Close split view),
  Keep on top (or Unpin) | Settings, Hide Antipad (Esc, no icon). Shortcuts dimmed on the right. Hide is labelled "Hide Antipad".
- Action row: Cut, Copy, Paste, Delete as icon-over-label tiles across the top (they leave the list). Enabled state is real:
  Cut, Copy and Delete need a selection; Paste is always enabled; disabled items dim icon and label together and don't react.
- Icon colours: the lab's "Coloured icons" tint (New note and Keep on top in Accent, Delete in Error) vs all Ink.
- Two settings for these, in `Model/Store.cs` Settings (with defaults) and as rows in Settings > UI > Controls
  (`UI/SettingsWindow.cs`, the "Controls" card, using the existing `Row`/`Toggle` helpers like "Edge fade"):
  `MenuActionRow` (bool, default true, row "Menu action row", description "Cut, copy, paste and delete as buttons across the top")
  and `MenuColorIcons` (bool, default false, row "Colored menu icons", description "Tint a few menu icons with your accent").
- Undo/Redo still use `Editor.Undo/Redo` with `CanUndo/CanRedo`.

### 2. Toolbar and pager
- `UI/Kit.cs` `IconButton` and the pager in `UI/PadWindow.Layout.cs` (`BuildPill`): icons become the locked recipe (16 px, Ink,
  weight 0) on the lab's Windows 11 buttons (about 32 px square, 4 px rounded hover). The pinned pin keeps its accent
  colour and its filled-under-outline look. Keep tooltips, automation names and click behaviour. Keep the pager compact enough
  for the top bar (36 px row) and pager pill; adjust only what the lab's design requires.
- Settings window icons are NOT part of this task; leave SettingsParts/SettingsWindow drawing alone apart from the two rows.

## Done means
- `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/Antipad -c Debug -o build/i6-check`: 0 errors, 0 new warnings;
  `build/i6-check/Antipad.exe --selftest` prints SELFTEST PASS; then delete `build/i6-check`. Don't launch or kill any Antipad.
- Don't try to commit (.git is read-only for you). Don't edit `.coord/STATUS.md`.
- Short report in `.coord/reports/I6-win11-menu-toolbar-in-app.md`: files changed, how the menu opens and closes, the two
  settings, toolbar sizes, build and selftest result, anything not done.
