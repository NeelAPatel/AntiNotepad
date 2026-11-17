# A23-paste-keep-structure: paste bulleted and indented text with its structure

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: Medium · editor-feature (item chat confirms in Findings)
State: request
Branch: `feat/A23-paste-keep-structure` in `.claude/worktrees/feat/A23-paste-keep-structure` (created at Approve, from dev's tip)

## Request
User, 2026-11-16: "I want to be able to copy paste bullet pointed/formatted text from elsewhere and paste it with closest possible format. For example I tried to copy some bulleted and indented text while typing an AI prompt, but pasting in AntiNotepad removed all of it and only kept the text." Numbered A23 on the user's go, 2026-11-16.

- **Goal:** pasting bulleted or indented text from another app produces the closest equivalent in the note: bullets become the note's bullet prefix and indent levels carry over, instead of plain words only.
- **Done check:** (1) pasting a 3-level bulleted list copied from at least a browser page and a chat or AI prompt box gives a 3-level bullet list in the note; (2) plain text without structure pastes exactly as today; (3) one undo reverts the whole paste; (4) paste into a checklist or numbered note does not break their prefixes (scope question for the user in the Plan).
- **Proof:** the clipboard formats actually present for each source (listed in Findings, from a real copy, not assumed); a before and after sample of the pasted note text; selftest checks on the conversion from clipboard text or HTML to note text.
- **What we know:** today only the words survive; the app's note format has bullet prefixes and indent (see `Editor/PadView.cs` paste path and the bullet handling); sources may offer HTML, RTF, or plain text with glyphs like `•` and tabs. Do not assume which; look at the real clipboard. Related ideas: `idea: list-default-type`, `idea: selection-convert-menu` (not part of this item).
- **Constraints:** Medium tier; lab-first only if the result needs a design choice (say so in the Plan); dev copy only; Codex cannot read the real clipboard in its sandbox, so clipboard inspection is done by the item chat or the user; one Codex run unless the plan says more. Branch from dev's latest commit (`git rev-parse dev` at Approve). This item merges only after `A21-save-delay-setting` has merged to dev (director merges in that order); catch up to dev before merge.

## Findings
Item chat, 2026-11-16 (in progress).
- **Paste path today:** no custom paste. `PadView` menu calls `Editor.Paste()` (`PadView.cs:912`); grep finds no `Pasting` hook or HTML/RTF read anywhere. AvaloniaEdit's default paste inserts the clipboard's plain-text format only, as one undo step.
- **Note format:** bullet = `- ` / `* ` / `+ ` prefix, indent = leading spaces (two per level in list code and tests; tab-indented bullets also recognized). Helpers: `Editor/ListEditing.cs`, `Editor/TextCommands.cs` (`AsList` bullet), `Editor/Elements.cs` (`BulletGenerator`).
- **Clipboard formats:** not yet measured from a real copy (agent-driven browser copy did not reach the Windows clipboard). Dump script: scratchpad `clip-dump.ps1`. Needs user copies from a browser page and the AI prompt box.
- **Tier:** Medium confirmed so far (one paste hook, one converter, selftests). No Codex audit: paste path is under 5 files.

## Plan
(item chat)

## Files
(item chat, at Plan)

## Contracts
(item chat)

## Approve
(the user's answer, verbatim)

## For Codex
(item chat writes at Approve)
