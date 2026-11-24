# AntiNotepad

Press a hotkey, jot it down, get back to work. A fast, translucent scratchpad for Windows 11.

AntiNotepad keeps quick notes close at hand and gets out of your way when you are done. It is also a personal learning project about building software with AI: the app and the evolving development process are both part of the story.

<!-- screenshot: the pad in use -->

## Features

**Everything here works today.**

**Summon it fast**
- Show or hide the pad from anywhere with a global hotkey (default Ctrl+Alt+Shift+S, configurable); it lives in the tray.
- Keep it on top, and let a pinned pad dim while you work elsewhere.
- Optionally start with Windows.

**Write**
- Bullet lists, numbered lists and checklists that continue as you press Enter.
- Countdown timers inside a note.
- An inline calculator with variables, units and dates.
- Optional clean markdown: hides `**` and `#` except on the line you are editing.
- Fold sections, change text size, and copy a whole note in one key.
- Read text from a screen snip.

**Organize**
- Notes in a stack, plus nine permanent slots, with split view to show a slot beside a note.
- Search in list or grid view; deleted notes wait in Trash.
- Move between notes with the keyboard or swipe-style paging.

**Make it yours**
- Rebind in-window shortcuts in Settings; press F1 for the live shortcut list.
- Choose paper, palette, text size, scroll feel and save delay.
- Notes are Markdown files stored locally; you pick the folder.

Notes are stored locally, by default in `Documents\AntiNotepad`; Settings can change the location.

## Install

Starting with the first release, download the latest zip from [Releases](https://github.com/NeelAPatel/AntiNotepad/releases), unzip it, and run `AntiNotepad.exe` on Windows 11. The release will be unsigned, so Windows may show a security warning. Check the release notes for runtime requirements.

## Get started

The default global show/hide hotkey is **Ctrl+Alt+Shift+S**; change it in Settings. In the pad, press **F1** for the current shortcut list.

| Default | Action |
|---|---|
| Ctrl+N | New note |
| Alt+Left / Alt+Right | Previous / next note |
| Ctrl+F | Search |
| Ctrl+\ | Split view |
| Ctrl+, | Settings |
| Ctrl+T | Keep on top |
| Esc | Hide the pad or close a panel |

<!-- screenshot: search or split view -->

## Build from source

On Windows, install the .NET 10 SDK and PowerShell 7. From the repository root, run `pwsh -NoProfile -File tools/setup.ps1` to restore, build and self-test. Or run the steps manually:

```powershell
$env:AVALONIA_TELEMETRY_OPTOUT = '1'
$env:ANTINOTEPAD_DATA = Join-Path $env:TEMP ('antinotepad-selftest-' + [guid]::NewGuid().ToString('N'))
dotnet build product/AntiNotepad -c Debug -o build/public-check
.\build\public-check\AntiNotepad.exe --selftest
```

For an isolated dev copy, run `pwsh -NoProfile -File tools/dev.ps1`. It builds and launches a separate dev instance with its own data; see [the dev loop](docs/dev-loop.md) before using it.

## Building with AI

Development is AI-led. This is a personal learning project for AI models, agentic development, and orchestration. I scrutinize the work: I review it, approve each plan, and test by hand. The [building-with-AI index](docs/building-with-ai.md) describes the loop, its history, and lessons; the [wiki](https://github.com/NeelAPatel/AntiNotepad/wiki) is the companion guide.

## Contributing

Ideas and focused fixes are welcome, especially thoughtful UI proposals and AI-leveraged testing ideas. See [CONTRIBUTING.md](CONTRIBUTING.md).

## Credits

AntiNotepad is inspired by [Antinote](https://antinote.io) by Jfung. Jfung has granted permission to use Antinote as inspiration, and for branding and styling, subject to change on request.

The project is AI-led with scrutiny; see [Building with AI](#building-with-ai) and the [building-with-AI index](docs/building-with-ai.md).

## License

[MIT](LICENSE). Third-party components have their own [notices](THIRD-PARTY-NOTICES.md).
