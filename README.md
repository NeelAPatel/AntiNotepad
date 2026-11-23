# AntiNotepad

A translucent scratchpad for Windows 11, ready with a hotkey.

AntiNotepad keeps quick notes close at hand and gets out of your way when you are done. It is also a personal learning project about building software with AI: the app and the evolving development process are both part of the story.

<!-- screenshot: the pad in use -->

## Features

- Show or hide the pad with a configurable global hotkey; keep it on top when useful.
- Organize notes in a stack and slots, or show a slot beside a note in split view.
- Search notes in list or grid view and find deleted notes in Trash.
- Write bullet lists, numbered lists and checklists; use timers and inline calculations, including units and dates.
- Read text from a screen snip, customize shortcuts, and choose appearance settings.

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
