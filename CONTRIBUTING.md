# Contributing

Ideas, atomic fixes and small patches are welcome.

- **Ideas:** open an issue.
- **Fixes and patches:** open a PR against `dev`.
- **UI and interaction ideas:** especially welcome, because that's my hardest area.
- **AI-leveraged testing ideas:** strong ones are appreciated (see below).

## Report a bug

- Say what you expected and what happened.
- Include your Windows version and AntiNotepad version.
- List the steps to reproduce.
- Blur or crop private note text in screenshots.

## Propose a UI idea

- Describe the problem, not just the solution.
- Say who it helps.
- Describe one concrete interaction.
  - *A sketch or before-and-after picture beats a paragraph.*
- Open an issue before building anything big.
  - *Visible design changes start as a lab page and get approved before code. A discussion first saves your time.*

## Send a pull request

- Branch from `dev` and target `dev`.
- Keep it atomic: one problem per PR.
  - *Small PRs get reviewed and merged faster.*
- Leave the project buildable and the self-test passing.
- Add a focused check where it helps.
- Write what the problem was, what you changed, what you ran, and any limits.
- Add a screenshot or manual check steps for anything visible.
- Call out new dependencies and their licenses.
- Review AI-generated changes yourself before submitting.
  - *You own what you submit, however it was written.*
- You don't need to reproduce my dev loop.
  - *It's how I work and learn. See [building with AI](docs/building-with-ai.md) if you're curious.*

## Build and check

- Claude Code users: see [my optional loop hooks](docs/claude-hooks.md) and project settings.

- Install the .NET 10 SDK and PowerShell 7 (Windows).
- From the repository root, run `pwsh -NoProfile -File tools/setup.ps1` to restore, build and self-test. Or run the steps manually:

```powershell
$env:AVALONIA_TELEMETRY_OPTOUT = '1'
$env:ANTINOTEPAD_DATA = Join-Path $env:TEMP ('antinotepad-selftest-' + [guid]::NewGuid().ToString('N'))
dotnet build product/AntiNotepad -c Debug -o build/contributor-check
.\build\contributor-check\AntiNotepad.exe --selftest
```

- A passing run prints `SELFTEST PASS`.
- For a separate dev instance and hand-testing tips, see [docs/dev-loop.md](docs/dev-loop.md).

## AI-leveraged testing ideas

Examples of what I'd love to see:

- Generated nested lists and paste inputs.
  - *Check that parsing finishes and keeps structure.*
- Generated shortcut combinations.
  - *Compare them against window-level handlers and configured shortcuts to find conflicts.*
- Real hook transcripts, sanitized, turned into fixtures.
  - *Vary line endings and metadata, and check the expected allow or stop result.*

When you propose one:

- Say what the oracle is (how a pass or fail is decided).
- Give reproducible inputs.
- Say what still needs a human.
- Don't submit private notes or transcripts.
