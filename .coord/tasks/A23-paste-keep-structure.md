# A23-paste-keep-structure: paste bulleted and indented text with its structure

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: Medium · editor-feature (item chat confirms in Findings)
State: approved
Branch: `feat/A23-paste-keep-structure` in `.claude/worktrees/feat/A23-paste-keep-structure` (created at Approve, from dev's tip)

## Request
User, 2026-11-16: "I want to be able to copy paste bullet pointed/formatted text from elsewhere and paste it with closest possible format. For example I tried to copy some bulleted and indented text while typing an AI prompt, but pasting in AntiNotepad removed all of it and only kept the text." Numbered A23 on the user's go, 2026-11-16.

- **Goal:** pasting bulleted or indented text from another app produces the closest equivalent in the note: bullets become the note's bullet prefix and indent levels carry over, instead of plain words only.
- **Done check:** (1) pasting a 3-level bulleted list copied from at least a browser page and a chat or AI prompt box gives a 3-level bullet list in the note; (2) plain text without structure pastes exactly as today; (3) one undo reverts the whole paste; (4) paste into a checklist or numbered note does not break their prefixes (scope question for the user in the Plan).
- **Proof:** the clipboard formats actually present for each source (listed in Findings, from a real copy, not assumed); a before and after sample of the pasted note text; selftest checks on the conversion from clipboard text or HTML to note text.
- **What we know:** today only the words survive; the app's note format has bullet prefixes and indent (see `Editor/PadView.cs` paste path and the bullet handling); sources may offer HTML, RTF, or plain text with glyphs like `•` and tabs. Do not assume which; look at the real clipboard. Related ideas: `idea: list-default-type`, `idea: selection-convert-menu` (not part of this item).
- **Constraints:** Medium tier; lab-first only if the result needs a design choice (say so in the Plan); dev copy only; Codex cannot read the real clipboard in its sandbox, so clipboard inspection is done by the item chat or the user; one Codex run unless the plan says more. Branch from dev's latest commit (`git rev-parse dev` at Approve). This item merges only after `A21-save-delay-setting` has merged to dev (director merges in that order); catch up to dev before merge.

## Findings
Item chat, 2026-11-16, real clipboard reads (Chromium, ProseMirror editor, Windows).
- **Paste path today:** no custom paste. `PadView` menu calls `Editor.Paste()` (`PadView.cs:912`); no `Pasting` hook, no HTML/RTF read anywhere. AvaloniaEdit's default inserts plain text only, one undo step.
- **Note format:** bullet `- ` / `* ` / `+ `, numbered `N. `, indent = leading spaces (two per level). Helpers: `Editor/ListEditing.cs`, `Editor/TextCommands.cs`, `Editor/Elements.cs`.
- **Capture A, rendered AI-chat list (the bug):** formats `HTML Format, UnicodeText, Text, OEMText`. Plain text = plain lines `test from ai chat`, `test 2 from ai chat`, ...: bullets, numbers and indent all gone. HTML = nested `<ul><li><p>..</p><ul>..` and `<ol>..` with full structure (`<!--StartFragment-->` offsets).
- **Capture B, markdown source typed in the editor:** plain text already has `- ` and two-space indents; HTML is flat `<p>` lines. Pastes fine today; must stay untouched.
- **How Windows stores it:** one copy = several formats at once. Standard ids (`CF_UNICODETEXT`, `CF_TEXT`, `CF_OEMTEXT`) plus registered names (`HTML Format` = HTML with an offset header; Chromium adds source URL and token). Measured here with Win32 `EnumClipboardFormats`: A held `HTML Format` (477 B) and text only; B the same. From memory, not measured: Word adds `Rich Text Format` plus `MsoList` HTML; VS Code adds a `vscode-editor-data` language tag; Excel adds `Csv` and an HTML table.
- **Avalonia 12.1.4 can read it:** `IClipboard.TryGetDataAsync`, `GetDataFormatsAsync`, so a custom format by name needs no Win32. Codex confirms the exact call in the build.
- **Not yet captured:** a normal web page, Word, VS Code, Excel. Each is a later reader plus one capture.
- **Tier:** Medium confirmed. No Codex audit (paste path under 5 files).

## Plan
Goal: a small paste engine that upgrades what lands in the note when the clipboard holds richer structure than its plain text. Rule one: **plain text is the default and is trusted**; the engine only upgrades, never rewrites. Code, markdown source and plain text pass through untouched.
1. **Engine** (new `Editor/PasteEngine.cs`, pure): input = snapshot `{Plain, Html?, Formats[]}`; output = note text or null (null = normal paste). A table of readers tried in order, first non-null wins, so a new source is one new row.
2. **Reader 1 only, in this item: HTML lists** (`ul`/`ol`/`li`/`p`/`br`, entities). Depth gives indent (2 spaces per level), `- ` bullets, `N. ` per ordered list. Small tag scanner, no new dependency.
3. **Guards:** upgrade only if the HTML holds `li` AND the plain text has no list markers. Pass through if the HTML has `<pre>` or `white-space:pre`, or the plain text is already indented (code, markdown source).
4. **Hook** in `PadView` paste (Ctrl+V and menu): reads formats async via `IClipboard`, one edit, so one Undo. Checklist and numbered notes: pasted lines are plain `- `/`N. ` text; existing prefixes untouched.
5. **Selftests:** fixtures are real captures A and B, plus plain text, mixed 3-level `ul`/`ol`, entities, empty items, a code-like HTML (`<pre>`), markdown-source no-op, bad HTML never throws.
6. **Dev aid:** `tools/clip-dump.ps1` (prints formats, sizes, text and HTML) so each new source is captured the same way.
7. **Placeholders for images and files** (user, 2026-11-17): the snapshot also carries `HasImage` and `Files` (paths), filled from the clipboard formats; the reader table has two named stub rows, `ImageReader` and `FilesReader`, that return null today. A selftest proves an image-only or file-only snapshot still takes the normal paste. The note has no image or file form yet, so nothing visible changes.
8. **Later readers, separate items:** Word lists (`MsoList` HTML, RTF), tables (`Csv`/HTML table to tab or `|` text), headings and bold. Each starts from a real capture.
- **Lab:** none; no visible design choice.
- **Codex:** one run, offline checks only. Hand test in the dev copy: web page list, AI box list, markdown source, plain text, code from an editor, Undo, checklist and numbered notes.

## Files
- `product/AntiNotepad/Editor/PasteEngine.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/SelfTest/Paste.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `tools/clip-dump.ps1`
- `.coord/tasks/A23-paste-keep-structure.md`
- `.coord/reports/A23-paste-keep-structure.md`
- `.coord/reports/A23-paste-keep-structure.terra.codex.md`

## Contracts
Engine: `string? Upgrade(ClipSnapshot s)` returns note text or null (null = plain paste). Pure; never throws on bad HTML. Reader = `Func<ClipSnapshot,string?>` in an ordered list.

## Approve
User, 2026-11-17, verbatim: "Scope up: if we happen to build a paste engine, have placeholders ready for images and files as well. theres no representation of this in antinote yet but we can have the paste engine relatively ready for the future. But yes currently the plan is approved"

## For Codex
Role: `terra` (normal feature across a few files: engine, paste hook, selftests). Read the whole card first; do not edit it, do not commit (you cannot write to .git).
Build in the worktree `.claude/worktrees/feat/A23-paste-keep-structure`. Offline checks only: no ports, no browser, no clipboard, never launch AntiNotepad. Set `AVALONIA_TELEMETRY_OPTOUT=1`; build into `build/A23-check`, run `--selftest` there, delete the folder.
Do the Plan items 1 to 7. Follow `.claude/rules/coding.md` and `docs/coding.md`. Avoid `PadWindow*`, Settings, Store files. No code comments except a non-obvious why. US spelling.
Fixtures for selftests (real captures):
- A, rendered AI-chat list. Plain: `test from ai chat`, `test 2 from ai chat`, `test 3`, `test2 again`, `test from ai chat`, `test 2`, `test3`, `test 2` (one per line, no markers). HTML fragment (inside `<!--StartFragment-->`): `<ul data-pm-slice="3 1 []"><li><p>test from ai chat</p><ul><li><p>test 2 from ai chat</p><ul><li><p>test 3</p></li></ul></li><li><p>test2 again</p></li></ul></li></ul><ol><li><p>test from ai chat</p><ol><li><p>test 2</p><ol><li><p>test3</p></li></ol></li><li><p>test 2</p></li></ol></li></ol><p></p>`. Expected note text: `- test from ai chat` / `  - test 2 from ai chat` / `    - test 3` / `  - test2 again` / `1. test from ai chat` / `  1. test 2` / `    1. test3` / `  2. test 2`.
- B, markdown source: plain text `- Alpha
- Beta
  - Nested item
- Gamma

1. First
2. Second
   1. Nested step`; HTML is flat `<p>` per line. Expected: engine returns null.
`HTML Format` payload has a header (`StartFragment:`/`EndFragment:` byte offsets): parse it, fall back to the whole string if the header is missing. Confirm the Avalonia 12.1.4 call that reads a named format (`IClipboard.TryGetDataAsync` / `GetDataFormatsAsync`; fallback `Platform/` Win32) and say which in the report. Hook both Ctrl+V and the Paste menu entry (`PadView.cs:912`); paste is async, keep one Undo group.
Also add `tools/clip-dump.ps1` (formats, sizes, text, HTML of the current clipboard; STA; read only) and a hand-test line in `SelfTest/Contracts.cs`.
Done means: `dotnet build` clean; `--selftest` passes including the new paste checks; `tools/gate.ps1 -Offline` passes; `git status` shows only the files in `## Files`.
Report at `.coord/reports/A23-paste-keep-structure.md`: files changed, Avalonia call used, checks run with results, anything not verified.
