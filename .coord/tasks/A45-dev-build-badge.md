# A45-dev-build-badge: the dev badge names the exact build being tested

Role: director (gpt-6.1-sol medium). Product `product/AntiNotepad`.
Branch: `feat/A45-dev-build-badge` (already checked out; never switch, merge or commit: you cannot write to `.git`).
Tier: Low (rules: `docs/loop.md`; dev-only UI text, visible, so the user hand-tests it)
Read first: `AGENTS.md`, `docs/dev-loop.md` (dev identity vs runtime dev badge), then the files below.

## Goal
User (2026-10-29): "let the badge represent the exact version that's being tested right now. Branch glyph? Powerline symbol
(U+E0A0). `<glyph>dev/<current branch, i.e A54-link-hover-click>@<build time>`. The badge may get contracted, or might be
expandable on hover, but hovering should definitely show me an alt text or something showing the full timestamp if it is not
able to expand."

Today the top bar shows a plain "dev" chip (`UI/PadWindow.Layout.cs:151-154`, `Chip` at ~194) only in the repo dev copy.

Wanted:
1. Build stamp: `tools/dev.ps1` passes the current git branch to the Debug build (for example
   `-p:AntiNotepadBranch=<git branch --show-current>`); the project stamps it and the build time as `AssemblyMetadata`, only
   when `AntiNotepadDev` is true (release builds stay deterministic and unchanged). `AppInfo` exposes both (null when absent).
2. Badge text: full form ` dev/<branch>@<yyyy-MM-dd h:mm tt>` with the `feat/` prefix dropped from the branch (on the
   `dev` branch itself: ` dev`). Short form shown by default: ` <ID>` plus time, for example ` A54 · 11:30 AM`
   (the leading `<letter><digits><letter?>` of the branch slug; other branches: the first 12 chars). Hover expands the chip to
   the full form, and a tooltip always shows the full form with the full date and time.
3. Glyph: U+E0A0 renders from Cascadia Mono/Code (ships with Windows 11). If that font is not available, use `⎇` (U+2387).
4. No branch metadata (built some other way): keep today's plain "dev" chip.
5. Same chip style and position as today; only the text, hover and tooltip change. No other chrome changes.

## Files
Only these files may change. Touching any other file needs a line under "Out-of-scope files" in the report.
- `product/AntiNotepad/AntiNotepad.csproj`
- `product/AntiNotepad/AppInfo.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `tools/dev.ps1`
- `product/AntiNotepad/SelfTest.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Contracts
New row `dev-badge-build-stamp`: "A dev build stamped with a branch and build time shows them in the badge (short form, full
form on hover and in the tooltip); without the stamp it shows plain dev; release builds carry no stamp." Automate the text
formatting (pure function: branch + time -> short/full strings, `feat/` dropped, `dev` branch, missing stamp) headlessly.

## Constraints
- Smallest change that meets the goal. No drive-by refactors, token or style edits.
- Build into `build/A45-dev-build-badge-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete it.
- Gate inside the sandbox: `pwsh -NoProfile -File tools/gate.ps1 -Task A45-dev-build-badge -Offline`.
- Never launch or kill AntiNotepad. Never run `--quit`. Keep `tools/dev.ps1`'s C25 launcher logic intact; only add the
  build property.

## Done means
- Dev copy launched from a worktree shows the short badge; hovering shows the full branch and timestamp.
- Release build: no stamp, no badge (as today).
- Build 0 warnings, `SELFTEST PASS`, gate passes.

## Before you report (director only)
Spawn the `lens_reviewer` agent on your diff (it reads `.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why
not.

## Report: `.coord/reports/A45-dev-build-badge.md`
Keep it short. Use exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per changed block: `file:line-range | what changed | which goal line it serves`.
3. **Behavior changes**: every changed result, build property, metadata or UI text. "None" only if true.
4. **Out-of-scope files**: file and why, or "None".
5. **Contracts**: rows added or changed, or "None".
6. **Lens review**: findings and what you did with each.
7. **Checks run**: build, selftest, gate result lines.
8. **Hand checks for the user**: what to look at in the dev copy.
