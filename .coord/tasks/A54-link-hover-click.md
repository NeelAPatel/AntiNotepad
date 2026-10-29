# A54-link-hover-click: shortened links show a hand cursor and open on Ctrl+click

Role: director (gpt-6.1-sol medium). Product `product/AntiNotepad`.
Branch: `feat/A54-link-hover-click` (already checked out; never switch, merge or commit: you cannot write to `.git`).
Tier: Low (rules: `docs/loop.md`)
Read first: `AGENTS.md`, then the files below. Line numbers are from the current tree; re-read before editing.

## Goal
User report: Ctrl+click on a link does nothing, and links show no hand cursor. Scope is only the long (shortened) links:
URLs over 34 chars, with Shorten links on and the caret off their line, render as a `GlyphElement` with `Hand = true` and a
`Pressed` handler that opens the URL on Ctrl+click (`Editor/Elements.cs:108-146`). Make that work: the hand cursor shows
while the pointer is over a shortened link, and Ctrl+left-click opens it in the default browser. Plain click on the link
keeps today's behavior (caret placement).

Must not change: short URLs stay plain colored text (making them clickable is a separate lab idea, not this task); the full
URL still shows on the caret line; result-chip hover/click (`PadView.OnViewMoved`, `OnViewPressed`) behaves as today; no look
changes.

Likely causes to confirm first (do not assume):
- `PadView.OnViewMoved` (`Editor/PadView.cs:576-583`) sets `view.Cursor` to Ibeam on every pointer move when not over a chip,
  overriding the `GlyphElement.OnQueryCursor` hand cursor.
- `PadView.OnViewPressed` is a Tunnel handler on the TextView (`PadView.cs:140`); check whether the press reaches
  `GlyphElement.OnPointerPressed` at all, and whether AvaloniaEdit's selection mouse handler takes it first.
- `GlyphElement.OnPointerPressed` uses `e.GetCurrentPoint(null)`; confirm the left-button check holds.
- Whether `e.KeyModifiers` carries Control in that handler.
State the actual cause in the report.

## Files
Only these files may change. Touching any other file needs a line under "Out-of-scope files" in the report.
- `product/AntiNotepad/Editor/Elements.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/SelfTest.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`

## Contracts
No existing row covers links. Add one row, `shortened-link-opens`: "A shortened link shows a hand cursor and opens on
Ctrl+click; a plain click places the caret." Automate what a selftest can prove headlessly (for example: the generator
returns a `GlyphElement` with `Hand` for a long URL off the caret line, and the cursor/press path is not overridden), and
leave the live part as the row's manual note.

## Constraints
- Smallest change that meets the goal. No drive-by refactors, renames, icon/token/style edits outside the goal.
- Build into `build/A54-link-hover-click-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A54-link-hover-click -Offline`; the manager re-runs it online.
- Never launch or kill AntiNotepad. Never run `--quit`. Never actually open a browser from the selftest (stub or skip `OpenUrl`).

## Done means
- Hovering a shortened link shows the hand cursor; leaving it restores the I-beam; chips still show hand.
- Ctrl+left-click on a shortened link opens it once in the default browser; plain click places the caret as before.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A54-link-hover-click` passes.

## Before you report (director only)
Spawn the `lens_reviewer` agent on your diff (it reads `.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why
not. Workers without spawning: run the same checklist yourself and say so.

## Report: `.coord/reports/A54-link-hover-click.md`
Keep it short. Use exactly these sections:
1. **Summary**: two or three sentences, including the actual cause.
2. **Change list**: one row per changed block: `file:line-range | what changed | which goal line it serves`.
3. **Behavior changes**: every changed public method result or meaning, new or changed shared state, timing, default, or
   setting effect. Write "None" only if true; the review checks this against the diff.
4. **Out-of-scope files**: file and why, or "None".
5. **Contracts**: rows added or changed, or "None".
6. **Lens review**: findings and what you did with each.
7. **Checks run**: build, selftest, gate result lines.
8. **Hand checks for the user**: what to try in the dev copy, or "None (not visible)".
