# AntiNotepad

AntiNotepad is a Windows 11 hotkey-summoned, chrome-free translucent scratchpad: a recreation of AntiNote (its functionality and feel, not a 1:1 copy).

## Roadmap (project overview)

A live project overview served from STATUS and Git.

```powershell
pwsh -NoProfile -File tools/roadmap-serve.ps1 -Open
```

```powershell
pwsh -NoProfile -File tools/roadmap-serve.ps1 -Stop
```

URL: http://localhost:47811

The server starts on its own with the first Claude prompt and stops after 12 hours unused.
How to launch, shut down and manage it: [docs/wiki/Roadmap-Portal.md](docs/wiki/Roadmap-Portal.md).
