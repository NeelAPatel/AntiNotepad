# A23-paste-keep-structure-r3: tables, inline formatting, headings, links

Rework round of `A23-paste-keep-structure` (`docs/loop.md`). Read the item card `.coord/tasks/A23-paste-keep-structure.md` first, then `-r2.md`: Plan, Files,
Contracts and "For Codex" still apply unless this file says otherwise.

Tier: Medium, same as the card.
Role: terra, because it replaces the tokenizer with a small tree parser and adds three converters in a few files.

## Why this round
User, 2026-11-17, after copying a table and a formatted Claude answer from the chat: "lets build support for formatting as much as possible based on both of these tests. Tables, Text Formatting, Emoji mapping, Markdown pipes, Auto Aligned columns". Today's engine keeps only the words of bold, code, links and tables. The note already renders `**bold**`, `*italic*`, `++underline++` (or `__`), `~~strike~~`, backticks and heading hashes (`Editor/Colorizers.cs`, `Editor/Elements.cs`), so the target syntax exists.

## Change to the plan
The HTML reader now converts these to note syntax (all other rules and guards from the card and r2 stay):
1. **Inline marks** (inside paragraphs, headings, list items, table cells): `strong`/`b` to `**x**`, `em`/`i` to `*x*`, `u` to `++x++`, `s`/`del`/`strike` to `~~x~~`, `code` to backticks (plain text if the text holds a backtick). Nested marks combine (`***x***`). Marks must hug non-space text (the colorizer regexes need `**x**` not `** x **`): move leading and trailing spaces outside the marks; drop marks around empty text.
2. **Headings** `h1` to `h6` become `#` to `######` plus a space. Other block text stays a plain line.
3. **Links** `a href`: text only when the text equals the href or the href is not http(s)/mailto; otherwise `text (href)`.
4. **Tables:** a `table` becomes markdown pipes with auto-aligned columns: header row (`thead` or the first row, `th` or not), a divider row, body rows. Cells are trimmed single-line text with inline marks (newlines become a space, `|` becomes `\|`). Each column is padded with spaces to its widest cell by **display width** (emoji, CJK and fullwidth count 2 columns; combining marks, variation selectors and zero-width joiners count 0; a ZWJ emoji sequence counts 2). The divider uses dashes of the column width (minimum 3). Row form: `| a | b |`. Table blocks have a blank line to adjacent paragraphs, like lists. Bail out (return null, normal paste) on `colspan` or `rowspan` above 1, a nested table, or a table inside a list item.
5. **Emoji:** kept exactly as they are (the plain text already has them); only the alignment code is emoji-aware.
6. **Word guard:** the plain-versus-HTML word comparison uses the text without marks, link hrefs, pipes, padding and divider rows, so it still passes for these copies. If a table's plain text is tab separated, the comparison treats tabs as whitespace.
7. Replace the regex tokenizer with a small tolerant tree parser (void tags, optional close tags for `p`/`li`/`td`/`tr`, entities); malformed input still returns null, never throws. Split the code by job if `PasteEngine.cs` passes about 250 lines (for example `PasteHtml.cs` parser, `PasteTable.cs`); keep the reader table in `PasteEngine.cs`. Delete code the tree parser makes dead.

Capture D, real table copy from the Claude chat. Plain (tabs):
`If you say<TAB>What happens<TAB>Undo?
Tell Master 6<TAB>I message the Master 6 chat with the facts and the two fixes: merge adds only the item’s own files, and approve.ps1 accepts an already-tracked card<TAB>🟢 Just a message
Leave it<TAB>Nothing sent; I handle A23 alone and A22 hits the same wall<TAB>🟢 Nothing changed`
HTML fragment: `<table><thead><tr><th scope="col">If you say</th><th scope="col">What happens</th><th scope="col">Undo?</th></tr></thead><tbody><tr><td>Tell Master 6</td><td>I message the Master 6 chat with the facts and the two fixes: merge adds only the item’s own files, and <code dir="ltr">approve.ps1</code> accepts an already-tracked card</td><td>🟢 Just a message</td></tr><tr><td>Leave it</td><td>Nothing sent; I handle A23 alone and A22 hits the same wall</td><td>🟢 Nothing changed</td></tr></tbody></table>`
Expected: `| If you say    | What happens | Undo? |` style padded pipes, divider `| ---- ...`, two body rows, `approve.ps1` in backticks, emoji cells padded as width 2 per emoji. Derive the exact string from the rules and assert it in a selftest.

Capture E, real formatted copy (bold headings with emoji, code spans, bullets). Plain:
`What happened 🟢

The A21 merge commit e2bb364 (17 Nov, 00:23) lists, in its own message: “Also: A22/A23 cards, launch.json, STATUS and pending refresh.”
The A23 card was committed to dev there, in its early Request state, together with .claude/launch.json.
Master 6 had created that card earlier, so it sat untracked in the main checkout. The merge commit swept it in.
The merge step in .claude/skills/merge-task/SKILL.md says to commit only STATUS and lessons. The merge also took the cards and launch.json, so a broader add was used.
branches.md says a card stays untracked until Approve, then moves into the item’s branch.
That is why tools/approve.ps1 refused: it requires an untracked card.

What could go wrong if left unchecked 🟡

Approve breaks for any item whose card gets swept in. I had to do the branch and card steps by hand, which skips the script’s checks (cap, files overlap).
Two copies of the card diverge. Dev holds the old Request-only version, and the item branch holds the real one. The merge back is a conflict on a file nobody should have to resolve.
Dev carries unapproved cards. An item can look started in dev’s history before the user said Approve. The same applies to launch.json.
Noise in the A21 merge. The “one clean merge per item” history now mixes in other items’ files, which defeats the purpose of tracing a break to one item.
Same hit as A22: its card was committed separately (90ef390), so it is already tracked and will block approve.ps1 the same way.
A21’s lab pages (tools/playground/a21-*.html) are still untracked, so the same sweep could happen again.`
HTML fragment: `<p dir="ltr"><strong>What happened 🟢</strong></p><ul dir="ltr"><li>The A21 merge commit <code dir="ltr">e2bb364</code> (17 Nov, 00:23) lists, in its own message: “Also: A22/A23 cards, launch.json, STATUS and pending refresh.”</li><li>The A23 card was committed to dev there, in its early Request state, together with <code dir="ltr">.claude/launch.json</code>.</li><li>Master 6 had created that card earlier, so it sat untracked in the main checkout. The merge commit swept it in.</li><li>The merge step in <code dir="ltr">.claude/skills/merge-task/SKILL.md</code> says to commit only STATUS and lessons. The merge also took the cards and <code dir="ltr">launch.json</code>, so a broader add was used.</li><li><code dir="ltr">branches.md</code> says a card stays untracked until Approve, then moves into the item’s branch.</li><li>That is why <code dir="ltr">tools/approve.ps1</code> refused: it requires an untracked card.</li></ul><p dir="ltr"><strong>What could go wrong if left unchecked 🟡</strong></p><ul dir="ltr"><li><strong>Approve breaks</strong> for any item whose card gets swept in. I had to do the branch and card steps by hand, which skips the script’s checks (cap, files overlap).</li><li><strong>Two copies of the card diverge.</strong> Dev holds the old Request-only version, and the item branch holds the real one. The merge back is a conflict on a file nobody should have to resolve.</li><li><strong>Dev carries unapproved cards.</strong> An item can look started in dev’s history before the user said Approve. The same applies to <code dir="ltr">launch.json</code>.</li><li><strong>Noise in the A21 merge.</strong> The “one clean merge per item” history now mixes in other items’ files, which defeats the purpose of tracing a break to one item.</li><li><strong>Same hit as A22:</strong> its card was committed separately (<code dir="ltr">90ef390</code>), so it is already tracked and will block <code dir="ltr">approve.ps1</code> the same way.</li><li><strong>A21’s lab pages</strong> (<code dir="ltr">tools/playground/a21-*.html</code>) are still untracked, so the same sweep could happen again.</li></ul>`
Expected: headings stay bold lines as in the source (`**What happened 🟢**`, they are `strong` in a `p`, not `h*`); list items keep nested prefixes; `e2bb364` and file names in backticks; `**Approve breaks** for any item...`. Assert the exact result in a selftest.

Add selftests: captures D and E exact; `h2` heading; underline, italic, strike, nested bold-italic; mark with edge spaces; link with different text and with equal text; table without `thead`; table with `|` in a cell; colspan returns null; table inside `li` returns null; wide-emoji column alignment; earlier captures A, B, C unchanged.

## Files
- `product/AntiNotepad/Editor/PasteEngine.cs`
- `product/AntiNotepad/Editor/PasteHtml.cs`
- `product/AntiNotepad/Editor/PasteTable.cs`
- `product/AntiNotepad/SelfTest/Paste.cs`

## Done means
Offline checks only (`docs/agents.md`, "What Codex can and cannot check").
- Captures D and E convert to the exact expected text; all earlier paste selftests pass.
- Build 0 warnings, `SELFTEST PASS`, `pwsh -NoProfile -File tools/gate.ps1 -Task A23-paste-keep-structure-r3 -Offline` passes.

Report: `.coord/reports/A23-paste-keep-structure-r3.md`, in the card's report format.
