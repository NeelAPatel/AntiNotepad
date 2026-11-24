# AntiNotepad

Your scratchnotes. Always ready. There when you need it, gone when you don't.

AntiNotepad keeps your quick notes close at hand and gets out of your way when you are done. It is also a personal learning project about building software with AI: the app and the evolving development process are both part of the story.

<!-- screenshot: the pad in use -->
<img width="552" height="907" alt="image" src="https://github.com/user-attachments/assets/2e641535-739d-4501-a858-45187afb4ce7" />

## Features

**Everything here works today, and is barely a fraction of the customizability you have** _with room for improvement!_

### Write
- [x] Notes are stored in Markdown! 
- [x] Lists: bullets, numbers, checklists
- [x] Indenting: Tab / Shift+Tab; wrapped lines align under text
- [x] Fold / unfold sections via gutter chevron
- [x] Mode word on line 1: checklist, timer, math, bullet, numberlist
- [x] `/` commands anywhere, with completion popup
- [x] Fenced code blocks, language picker
- [x] Clean markdown: hides `**` and `#` off the active line
- [x] Paste rich text and HTML tables as tidy note text
- [x] Ctrl+click opens links
- [x] Undo / redo, text size, copy whole note

### Calculate
- [x] Inline calculator: variables, units, dates
- [x] Countdown timers in a note
- [x] Text from a screen snip

### Organize
- [x] Note stack + 9 permanent slots
- [x] Split view: slot beside note
- [x] Search: list or grid; Trash for deleted
- [x] Keyboard or swipe paging between notes

### Summon
- [x] Global hotkey (Ctrl+Alt+Shift+S default), tray icon
- [x] Keep on top; pinned pad dims when idle
- [x] Optional start with Windows

### Customize
- [x] Rebind in-window shortcuts; F1 lists them
- [x] Paper, palette, text size, scroll feel, save delay
- [x] Notes are Markdown files, folder of your choice

## Data storage concerns

Many of the features, while working, have their own quirks that will take time to fix. As a result, I suggest you take backups. I'll do my best to ensure when you upgrade, your notes stay intact and migrate completely. 

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
