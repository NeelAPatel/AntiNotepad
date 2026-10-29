# A42d-list-commands-audit

Source audit of `feat/A42d-list-commands`, HEAD `b74024d`. Paths below are relative to `product/AntiNotepad/` unless prefixed with `.coord/` or `docs/`. No app, build, selftest or probe was run; visible results below are source-derived, not observations of a running dev copy. Only this report was written; STATUS and cards remain untouched under the audit's explicit boundary.

## 1. Kind and why

**Polish/reorg, behavior-neutral.** The Request is extraction, not a change to list behavior (`.coord/tasks/A42d-list-commands.md`, Request). It is not already done: `Editor/PadView.cs:746-831` still owns list recognition, continuation, renumbering and insertion. A42c-checkbox-indent established the pure-decision/document-adapter pattern (`Editor/ListEditing.cs:12-32`, `Editor/DocumentCommands.cs:8-49`); the remaining operations cannot currently be exercised directly without the control. Today's visible behavior is the baseline to preserve.

## 2. Now

| Input / area | What the user would see today; evidence |
|---|---|
| Enter routing | Custom continuation only for unmodified Enter, empty selection and closed completion. Ctrl+Enter instead toggles a checkbox or a timer; Shift+Enter bypasses continuation (`Editor/PadView.cs:669-680`). |
| Bullets | At end of `- ab`, Enter creates `- ab\n- `; midway at `- a|b`, it creates `- a\n- b`. The full line determines the marker, but insertion is at the caret, with the suffix left in place. `*` and `+` are preserved (`:794`, `:815-831`; grammar `Editor/ListEditing.cs:8`). |
| Numbers | `9. ab|` inserts `\n10. `; `9) a|b` inserts `\n10) ` before `b`. Renumber then normalizes the surrounding block (`Editor/PadView.cs:678`, `:804`, `:816-824`). |
| Checkboxes | `[x] a|b` becomes `[x] a\n[ ] b`; `[X]` also continues unchecked. Existing indentation and optional bullet wrapper survive; the original item's check stays unchanged (`:805-806`, `:821-824`; `Editor/ListEditing.cs:8`). |
| Empty items | Emptiness uses the **whole line**, not just text before the caret. With at least two indent characters it removes the first two characters and renumbers; e.g. `  - ` becomes `- ` with no newline. Otherwise it clears the whole line, exiting the list; a single indent character is cleared too (`Editor/PadView.cs:807-813`). Do not silently change the character-based rule to ASCII spaces. |
| Limits and modes | `999999999. item` declines continuation, even if empty; oversized numeric markers also decline. Normal editor Enter then receives the event (no next marker from this path). Code mode always declines. List mode supplies `\n[ ] ` for a non-list nonblank line, or even a blank first line, but not a blank later line (`:793-804`; parser `Editor/ListEditing.cs:35-42`). |
| Tab / Shift+Tab | Empty selection plus a recognized list line invokes indent/outdent, then renumber on the **post-edit** caret line. Tab adds two spaces and supplies `fresh: true`; Shift+Tab removes up to two ASCII spaces and supplies false. No mode or additional modifier exclusion exists here. Plain/selected text falls through (`Editor/PadView.cs:682-685`, `:746-753`; `Editor/ListEditing.cs:27-32`). |
| Numbering details | Scans the contiguous nonblank block of list lines or space-leading text. Counters are keyed by indent length; first numbers are retained, fresh target level starts at 1, shallower items clear deeper counters, bullets/invalid numbers reset their level. Overflow resets rather than creating ten digits. Replacements obtain each line's live offset, so earlier `9 -> 10` growth shifts later lines safely (`Editor/PadView.cs:764-783`). |
| Undo and caret | Enter continuation plus outer renumber share one `RunUpdate`; Tab indent plus renumber share another. Empty nested continuation also renumbers inside the command before the outer renumber. One Ctrl+Z is intended to restore each whole operation. No explicit caret assignment occurs here: document edits let AvaloniaEdit track it; insertion calls `BringCaretToView` **before** outer renumber. Thus Enter is intended to leave the caret after the new marker and visible at the bottom of a long note (`:678`, `:685`, `:810`, `:828-831`). Live anchoring/scrolling is unverified in this audit. |

Existing selftests cover parser limits and individual checkbox/indent edits with exact text and one Undo, **not** continuation or combined renumber (`SelfTest.cs:733-736`, `:768-814`). A42c-checkbox-indent's card records its earlier hand checks passing, including combined Tab, but that is historical evidence, not a fresh dev observation (`.coord/tasks/A42c-checkbox-indent.md`, Hand test).

**Timer boundary:** declined continuation in Timer mode calls `StartTimerOnLine(CaretLine)` before normal editor Enter; it checks line > 1, parse, analysis key and not-done, ensures the entry and starts it if not running (`Editor/PadView.cs:680`, `:711-719`). Preserve this exact position and behavior, including the existing guards; timer-enter-no-autostart is separate (item Request).

## 3. Reproduction (bugs)

Not applicable: no bug claimed. Exact baseline trace: tunneled key handler (`Editor/PadView.cs:136`) -> Enter gate (`:675`) -> `RunUpdate` -> `ContinueList` reads current line/mode (`:790-804`) -> empty-item Remove/Replace or caret Insert (`:807-831`) -> optional inner renumber (`:810`) -> outer `Renumber(CaretLine)` (`:678`) -> handled return, or timer fall-through (`:679-680`). Tab trace: list gate -> same update scope around `Indent` then post-edit-caret `Renumber` (`:682-685`). No temporary probe directory was created.

## 4. Where

- Proposed product files: `Editor/PadView.cs`, `Editor/ListEditing.cs`, `Editor/DocumentCommands.cs`, `SelfTest.cs`, `SelfTest/Contracts.cs`. No new Editor class is needed: A42c-checkbox-indent already supplied the owners. The current card's Files placeholders need these exact paths at Plan.
- Direct callers: `IsListLine` gates Tab; `ContinueList` is called by Enter; `Renumber` is called by Enter, Tab and empty nested continuation; `Insert` is used by continuation only (`Editor/PadView.cs:678-685`, `:799`, `:810`, `:824`). Checkbox callers and renderers need no change: the shared regex already lives in ListEditing (`:657-658`; `Editor/ListEditing.cs:8-9`).
- Reachable state: active `TextDocument`, its line offsets/undo stack, caret/selection/scroll; cached documents retain per-note history with limit 400 (`Editor/PadView.cs:201-209`, `:284-298`). Edits invoke analysis invalidation, immediate analysis or >1,500-line pending work with 140 ms debounce, then `Edited` (`:455-508`). Host subscription reaches the 400 ms save timer (`UI/PadWindow.Layout.cs:144`; `UI/PadWindow.cs:29`, `:378-387`). These owners and timings stay untouched. Renumber's counter dictionary is local, not a new cache (`Editor/PadView.cs:770`).

## 5. Risk

Highest risk is changing edit order/size or using stale absolute offsets after a digit-width replacement, then moving the caret read or splitting undo scopes (`Editor/PadView.cs:678`, `:685`, `:783`, `:810`, `:830-831`). A precomputed whole-block replacement could also change anchoring and document-change notifications (`:455-488`).

Use Gates menu defaults: `tools/gate.ps1` for build/selftest/LAYERS/CONTRACTS/seam/format/scope/size; fresh Codex `lens-review` for ordering, anchors, coupling and timer fall-through; **Hand test** for actual caret, scrolling and Ctrl+Z (`docs/loop.md`, Gates menu). Add a preservation CONTRACTS row to name the new automated promise. No bug reproduction gate or design lab is needed for this behavior-neutral extraction. Keep work bounded to the list block; no whole-note snapshots or new scheduler/cache.

## 6. Tier

**Confirm Medium.** This reshapes editor input, caret-sensitive edits and undo grouping, matching `docs/loop.md`, Tiers. The older plan's “highest” describes relative risk among extraction steps, not a High tier requirement. Timer implementation, scheduling, persistence and document-cache ownership remain outside scope; moving them would require reassessment (item Request; `Editor/PadView.cs:680`, `:711-719`).

## 7. Recommendation

**Move Renumber too.** Leaving it in PadView would leave combined Enter/Tab behavior dependent on the control and defeat the headless checks requested by the old plan (`.coord/tasks/A42-padview-commands-plan.md`, How step 4). Extend ListEditing with pure recognition/continuation decisions and numbering decisions; extend DocumentCommands with live-document application/block traversal. Preserve per-line small edits, live offsets, counter resets and both current renumber calls for empty nested Enter. Avoid a generic command framework.

**After:** users see the same text, numbering, undo, caret and scroll. PadView retains key/modifier/selection/completion routing, caller-owned `RunUpdate`, the post-mutation `CaretLine` read, timer fall-through/actions and UI insertion glue. Keep the thin `Insert` helper's `Document.Insert(Editor.CaretOffset, text)` plus immediate `BringCaretToView` together, as the old plan recommends; move the decision about what to insert. Helpers must not compute a new caret position or take ownership of selection. All analysis, caches, mode transitions, scheduling, rendering and `TextCommands` grammar remain with their existing owners (`Editor/PadView.cs:675-685`, `:828-831`; earlier plan, Stays/How).

**Code proof to add, not run here:**

- Pure table cases for all bullets, `.`/`)`, checked/unchecked and bullet-wrapped boxes; mid/end continuation; empty nested/root/one-indent; Code decline; List fallback including blank-first/blank-later; 999,999,999 and ten-digit declines. Keep regex captures/spacing and literal LF insertion unchanged (`Editor/ListEditing.cs:8`; `Editor/PadView.cs:793-824`).
- Real TextDocument integration checks using the production decisions/application, not a copied algorithm: exact text for mid/end Enter and grouped continue+renumber, nested outdent+renumber and root exit. Mirror PadView's existing caller scope and use a document anchor to obtain the post-insertion line, not an assumed initial line. For each changing case, one `Undo()` restores initial text and `CanUndo` is false, following `SelfTest.cs:768-781`. Document anchors are bounded evidence; they cannot prove a live editor caret.
- Group real indent+renumber and outdent+renumber. For example, Tab on the first line of `8. a\n9. b\n9. c\n9. d` must produce `  1. a\n9. b\n10. c\n11. d`; Shift+Tab on `8. a\n  1. b\n9. c` must produce `8. a\n9. b\n10. c`. These exercise fresh numbering and offsets after digit growth; one Undo restores each fixture (`Editor/PadView.cs:685`, `:775-783`). Add block-boundary, nested-level, bullet-reset, overflow, nonzero-offset and CRLF fixtures.
- Add `list-commands-preserved` CONTRACTS entry; retain checkbox-indent-preserved. Existing ListEditing/DocumentCommands layer bans already cover the intended boundary (`SelfTest.cs:857-893`); retain them and existing incremental-analysis bounds (`:2896-2897`). Run the planned build/selftest/gate at implementation, not as evidence claimed by this audit.

**UI proof:** in the identified dev build, repeat mid/end Enter on bullets, both number separators and checked boxes; empty nested/root; limit fall-through; Tab/Shift+Tab with fresh nesting and 9 -> 10; one Ctrl+Z per whole Enter/Tab. Check caret after marker, suffix preservation, selection/modifier/completion bypass, Code/List fallback, and caret visibility at the bottom of a long note. Compare long-list typing/scroll feel. Confirm Timer-note Enter still auto-starts under today's guards and Ctrl+Enter still toggles it. These cover the UI dependencies that headless text/Undo checks cannot prove (`Editor/PadView.cs:669-685`, `:711-719`, `:828-831`).

**Open scope questions:** none for the user; preserve existing quirks. Plan must finalize helper signatures and exact Files without expanding behavior. **Split:** no further item split recommended; recognition, continuation and renumber are coupled by the same two input transactions. Keep timer-enter-no-autostart and C29-headless-render-probe separate (item Request; `.coord/STATUS.md`, Pending work).

READY: yes
