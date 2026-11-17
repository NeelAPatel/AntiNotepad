# A23-paste-keep-structure-r2: paragraphs mixed with lists

Rework round of `A23-paste-keep-structure` (`docs/loop.md`). Read the item card `.coord/tasks/A23-paste-keep-structure.md` first: its Plan, Files,
Contracts and "For Codex" section still apply unless this file says otherwise.

Tier: Medium, same as the card.
Role: terra, because it is a contained rewrite of one reader plus its selftests.

## Why this round
Hand test failed, user 2026-11-17: copying a rendered Claude chat answer (a paragraph, then nested bullets) pasted as flat plain text. `HtmlListReader` returns null whenever text sits outside a list (the leading `<p>`). The user: "I expect the app to support things like this."

## Change to the plan
Plan item 2 widens: the HTML reader handles block text (paragraphs, headings, divs) mixed with lists. The guards stay (list present, plain text without markers or indentation, no `<pre>`/`white-space:pre`, word equality of plain vs HTML text). Rules:
- `p`, `div`, `h1` to `h6`, `blockquote` text outside a list becomes one line (inline tags such as `strong`, `em`, `code`, `span`, `a` contribute their text only; `br` inside a paragraph is a line break).
- One blank line separates a paragraph from an adjacent block (paragraph or list). No blank line between two adjacent top-level lists (capture A stays as it is).
- Empty paragraphs are dropped; no leading or trailing blank lines in the result.
- Nested lists inside `li` keep indent: two spaces per level; text before a nested list stays on the item line.
- Still null on malformed HTML, unbalanced list tags, or word mismatch. Never throws.
- Keep the code in `PasteEngine.cs` small; if a block scanner replaces the regex tokenizer, delete the old one. No new dependency.

Capture C, real Claude chat copy (plain text has no markers):
Plain: `Both are told, and both messages were delivered, though neither is confirmed read.` / blank / `Master 6 got the facts and three asks:` / `Resolve the A23 card from the branch version at merge.` / `Add only an item’s own files on later merges.` / `Watch for the untracked A21 lab pages, which could be swept in the same way.` / `PStack Manager was asked to plan a permanent fix. I suggested these as starting points:` / `A merge preflight that fails when a dev commit touches the cards of an item that has no branch.` / `A staged-files allowlist at merge.` / `approve.ps1 accepting a card that is already tracked.` / `A home for the untracked lab pages.`
HTML fragment: `<p dir="ltr">Both are told, and both messages were delivered, though neither is confirmed read.</p><ul dir="ltr"><li><strong>Master 6</strong> got the facts and three asks:<ul dir="ltr"><li>Resolve the A23 card from the branch version at merge.</li><li>Add only an item’s own files on later merges.</li><li>Watch for the untracked A21 lab pages, which could be swept in the same way.</li></ul></li><li><strong>PStack Manager</strong> was asked to plan a permanent fix. I suggested these as starting points:<ul dir="ltr"><li>A merge preflight that fails when a dev commit touches the cards of an item that has no branch.</li><li>A staged-files allowlist at merge.</li><li><code dir="ltr">approve.ps1</code> accepting a card that is already tracked.</li><li>A home for the untracked lab pages.</li></ul></li></ul>`
Expected note text: `Both are told, ... confirmed read.` / blank / `- Master 6 got the facts and three asks:` / `  - Resolve the A23 card from the branch version at merge.` / `  - Add only an item’s own files on later merges.` / `  - Watch for ... swept in the same way.` / `- PStack Manager was asked ... starting points:` / `  - A merge preflight ... no branch.` / `  - A staged-files allowlist at merge.` / `  - approve.ps1 accepting a card that is already tracked.` / `  - A home for the untracked lab pages.`
Add selftests: capture C exact; paragraph after a list (blank line between); two paragraphs then a list; heading plus list; `br` in a paragraph; capture A and B unchanged.

## Files
- `product/AntiNotepad/Editor/PasteEngine.cs`
- `product/AntiNotepad/SelfTest/Paste.cs`

## Done means
Offline checks only (`docs/agents.md`, "What Codex can and cannot check").
- Capture C converts to the expected text; all earlier paste selftests still pass.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A23-paste-keep-structure-r2 -Offline` passes.

Report: `.coord/reports/A23-paste-keep-structure-r2.md`, in the card's report format.
