# A35-ux-pass: whole-app UX audit (behavior, visual, icons)

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: High · many screens, judgment calls
State: done; stages 0-3 reported, all seven stage 4 steps merged here, ready for dev
Branch: `docs/A35-ux-pass` in `.claude/worktrees/docs/A35-ux-pass` (created at Approve); stage 4 fixes get their own branches or rounds

## Request
User via Master 7, 2026-11-20 (condensed from the chat):
- Run one UI/UX pass over the whole app, in one chat, not separate chats per lane. Use real design principles and evidence (Nielsen, Norman, Shneiderman, Krug, Fitts, Hick, Doherty, Dieter Rams, Apple HIG, Fluent 2/WinUI 3, Material, Müller-Brockmann, Refactoring UI, Gestalt), not taste.
- Lanes: **behavior** (heuristics, targets, destructive actions), **visual** (spacing grid, alignment, type ramp, corners, strokes, contrast, light and dark), **icons**. Also a token audit first: any hard-coded size, color, radius, stroke or duration that skips `Theme/` or `Motion/`.
- **Motion is out of this pass** (user, 2026-11-20: "lets remove motion from this pass then. we'll revisit after"). Do not audit or change animation timing.
- **Icons:** during A29 a Shortcuts icon rendered as a person with a do-not-disturb sign. Likely cause (unconfirmed): two icon fonts (system Segoe Fluent Icons, bundled Fluent System Icons) with different code tables, raw code strings, no registry. Confirm from the A29 reports and `UI/Kit.cs` `Glyph`, then propose: one icon registry (name, font, code, meaning), a selftest that every code exists in its font, a gate step against raw icon strings, a lessons rule (look codes up by name), and a contact sheet of every icon, approved by the user lab-first.
- Report "no issue" on a checked item too; do not stop at the first flaw.

## Stages
0 research (`.coord/research/ux-principles.md`) > 1 token audit > 2 icon inventory and contact sheet > 3 per-screen findings > 4 fixes: picked findings only, small lab-first batches, hand test per batch.

## Needs from you
Nothing. All steps are merged here; Master 7 merges this branch to dev.

## Steps (stage 4)
Merged here `--no-ff`, each hand-tested and gated (2026-11-21): A35b, A35a, A35g, A35d, A35e, A35f, A35c (gated, no visible change; B-9 dimming deferred to its own item). A-3 (F6 pane focus) dropped by the user for the split-view rework.
- `A35a-data-safety`: `fix/A35a-data-safety`, worktree `.claude/worktrees/fix/A35a-data-safety`.
- `A35b-icon-registry`: `feat/A35b-icon-registry`, worktree `.claude/worktrees/feat/A35b-icon-registry`. Adds: drop the unused faux-bold path in `GlyphIcon`; evaluate Microsoft Codicons (user, 2026-11-20).
- `A35d-spacing-tokens` (batch 5): `refactor/A35d-spacing-tokens`; plan now, build after A35b merges here (shares `UI/Kit.cs`).
- `A35g-labels-tooltips` (batch 6): `fix/A35g-labels-tooltips`; plan now, rebase before hand-back. Later: `A35e-targets-keyboard` and `A35f-contrast` (one lab) after A35d lands. User, 2026-11-20: "kick off anything else you want".
- For `pending.md` (Master 7): user idea during the A35a hand test, Undo toasts (7 s) show a countdown, the toast background draining right to left until it hides. Motion, so outside this pass; lab-first.
- For `pending.md` (Master 7): `note-stats-popup`. User, 2026-11-21 (A35g chat): "Let click show a pop up with note stats: # chars # words # lines # paragraphs etc things that are useful for people to keep track of somtimes in word, notepad, etc." Clicking the pager count opens it instead of search; lab-first. A35g drops its count tooltip for it.
- `A35c-design-contract`: one design contract doc plus enforcing checks (source ratchet to zero, rendered-tree, contrast, doc sync); gate for the dev merge. Inputs: Clear-tint minimum (hint 1.24:1 measured; user kept the old look; product call), B-9 dimming, the two product files over 600 lines, the 2.9 MB font copy in `tools/playground/a35d-lab`, and the 23 known survival lines as its baseline. User, 2026-11-20: "After batches 1-2", then "move esign contract at the end." So it runs last, after batches 3-6, and gates the dev merge. Merge order: A35a, then A35b rebases.

## Files
Stages 0-3:
- `.coord/reports/A35-ux-pass-s1-classified.tsv`
- `.coord/reports/A35-ux-pass-s1-classify.py`
- `.coord/reports/A35-ux-pass-s1-inventory.tsv`
- `.coord/reports/A35-ux-pass-s1.codex-a.md`
- `.coord/reports/A35-ux-pass-s1.codex-b.md`
- `.coord/reports/A35-ux-pass-s1.md`
- `.coord/reports/A35-ux-pass-s2-fontcodes.py`
- `.coord/reports/A35-ux-pass-s2-icons.tsv`
- `.coord/reports/A35-ux-pass-s2-sheet.py`
- `.coord/reports/A35-ux-pass-s2.codex.md`
- `.coord/reports/A35-ux-pass-s2.md`
- `.coord/reports/A35-ux-pass-s3-dump.md`
- `.coord/reports/A35-ux-pass-s3.md`
- `.coord/reports/A35-ux-pass-s3a.codex.md`
- `.coord/reports/A35-ux-pass-s3b.codex.md`
- `.coord/reports/A35-ux-pass-s3c.codex.md`
- `.coord/reports/A35-ux-pass-s3d.codex.md`
- `.coord/research/ux-principles.md`
- `.coord/tasks/A35-ux-pass-s1.md`
- `.coord/tasks/A35-ux-pass-s1b.md`
- `.coord/tasks/A35-ux-pass-s2.md`
- `.coord/tasks/A35-ux-pass-s3-common.md`
- `.coord/tasks/A35-ux-pass-s3a.md`
- `.coord/tasks/A35-ux-pass-s3b.md`
- `.coord/tasks/A35-ux-pass-s3c.md`
- `.coord/tasks/A35-ux-pass-s3d.md`
- `.coord/tasks/A35-ux-pass.md`

Stage 4 steps (each step card also lists its own):
- `docs/decisions.md`
- `docs/design-contract.md`
- `docs/design-scale.md`
- `docs/playground/measure.md`
- `docs/playground/tokens.json`
- `docs/shortcuts.md`
- `product/AntiNotepad/AppInfo.cs`
- `product/AntiNotepad/Editor/Completion.cs`
- `product/AntiNotepad/Editor/MenuEntry.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/Editor/Renderers.cs`
- `product/AntiNotepad/Editor/Structure.cs`
- `product/AntiNotepad/Icons.cs`
- `product/AntiNotepad/Model/Welcome.cs`
- `product/AntiNotepad/Platform/TaskbarPresence.cs`
- `product/AntiNotepad/Program.cs`
- `product/AntiNotepad/SelfTest/Bar.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/SelfTest/Contrast.cs`
- `product/AntiNotepad/SelfTest/Design.cs`
- `product/AntiNotepad/SelfTest/Icons.cs`
- `product/AntiNotepad/SelfTest/Run.cs`
- `product/AntiNotepad/SelfTest/Store.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `product/AntiNotepad/Theme/Contrast.cs`
- `product/AntiNotepad/Theme/Paint.cs`
- `product/AntiNotepad/Theme/Shape.cs`
- `product/AntiNotepad/Theme/Space.cs`
- `product/AntiNotepad/Theme/Typography.cs`
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `product/AntiNotepad/UI/HotkeyRecorder.cs`
- `product/AntiNotepad/UI/Kit.cs`
- `product/AntiNotepad/UI/MotionPreview.cs`
- `product/AntiNotepad/UI/PadBar.cs`
- `product/AntiNotepad/UI/PadMenu.cs`
- `product/AntiNotepad/UI/PadWindow.Bar.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/PadWindow.Navigation.cs`
- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/UI/Shortcuts.cs`
- `tools/check-design-contract.ps1`
- `tools/check-design-tokens.ps1`
- `tools/design-exceptions.tsv`
- `tools/design-tokens-baseline.tsv`
- `tools/gate.ps1`
- `tools/playground/a35d-lab/core.css`
- `tools/playground/a35d-lab/core.js`
- `tools/playground/a35d-lab/help.js`
- `tools/playground/a35d-lab/index.html`
- `tools/playground/a35d-lab/kit-test.html`
- `tools/playground/a35d-lab/pad.html`
- `tools/playground/a35d-lab/search.js`
- `tools/playground/a35d-lab/settings-a.js`
- `tools/playground/a35d-lab/settings-b.js`
- `tools/playground/a35d-lab/settings-c.js`
- `tools/playground/a35d-lab/settings.html`
- `tools/size-exceptions.tsv`

## Plan
Stages 0-3 report only. Stage 4 fixes picked findings in seven steps, each its own branch and side chat, merged here `--no-ff`:
- `A35a-data-safety`: no pad action silently destroys note text (Undo for trash delete, slot clear, delete forever).
- `A35b-icon-registry`: one typed icon registry with glyph-exists checks, so no raw icon codes (the A29 wrong-icon cause).
- `A35g-labels-tooltips`: controls name their action, key and target.
- `A35d-spacing-tokens`: spacing, stroke and size literals become tokens on one scale.
- `A35e-targets-keyboard`: 32 px controls, 24 px slot targets, one focus ring, a keyboard path for every control.
- `A35f-contrast`: text and controls meet WCAG contrast in all palettes and both themes.
- `A35c-design-contract`: one design contract doc and the checks that enforce it (tokens, rendered tree, contrast, doc sync, size); runs last.
Why: stage 3 found 13 high, 23 medium and 9 low issues (`.coord/reports/A35-ux-pass-s3.md`); these steps fix the picked ones.
Must not change: nothing outside the listed step files; motion timing (out of this pass); STATUS and pending.md.

## Approve
User, 2026-11-20: stage 0-3 scope approved ("kick off ux-pass on its own branch/worktree only after 0.7.0 is live and confirmed on main + installed"); 2026-11-20 after install: "it works 0.7.0 is on main and installed". Stage 4 needs its own Approve. Overrides granted by the user, 2026-11-20: this chat runs on Opus, medium effort; it may start its own side chats; Codex role `terra` (medium) for its tasks; internet research is done by this chat, not Codex (Codex's sandbox has no network, tested 2026-11-20). `AGENTS.md` "never starts other chats" is waived for this item only. User, 2026-11-20 after the stage 0 gate: "going forward no need to wait for my approval , you may proceed through all the stages and make subchats on your own accord for this chat and model. I will await your final report and how we want to apply your findings to the project" (stage gates 1-3 waived; stage 4 still waits for the final report). User, 2026-11-20 after the report: "Yes both" (batch 1 data safety and batch 2 icons start now) and "I want you to treat THIS branch as a temporary "dev" so anything you kick off from here merges back here, including becoming a subchat of this chat." So stage 4 steps branch from `docs/A35-ux-pass`, run as side chats of this chat, and this chat merges them back here `--no-ff`; `tools/approve.ps1` (dev-only) is not used for them, and Master 7 merges this branch to dev later.

## Findings
Stages 0-3 done 2026-11-20, report-only. Reports `.coord/reports/A35-ux-pass-s1.md` (tokens), `-s2.md` (icons; A29 cause: bundled U+F6ED is `person_subtract`), `-s3.md` (13 high, 23 medium, 9 low; six-batch stage 4 proposal). Live focus and contrast checks open.

## For Codex
Role: terra, normal reasoning across several files (`docs/agents.md`). Scans run `CODEX_SANDBOX=read-only`, offline. Never launch the app, never edit this card or STATUS, never commit. Report `.coord/reports/A35-ux-pass-<stage>.md`, short, with a Summary, the table, and checks run.

## Check
Gate at the tip, 2026-11-22: `tools/gate.ps1 -Task A35-ux-pass -Offline` PASS. Build (0 warnings), Selftest, Seam, Tokens, Contract, Format, Scope (61 step files allowed), LoopLint and Cleanup PASS. Size WARN: 4 product files over 600 lines, named in `tools/size-exceptions.tsv`. LoopLint WARN: this card is over its 4 KB budget.
Each step was gated offline against this branch before its merge; the survival check found only intended losses (26 lines).
Misses: None.

## Hand test
Per step, in the user's words (dev slot copies, 2026-11-21):
- `A35a-data-safety`: "everything else works fine".
- `A35b-icon-registry`: "All good".
- `A35g-labels-tooltips`: "undo works, wrap it up".
- `A35d-spacing-tokens`: "All looks right"; after the rebase, "Both look right".
- `A35e-targets-keyboard`: "everything else works"; palette wrap, "Looks right".
- `A35f-contrast`: "all good, hand test passes".
- `A35c-design-contract`: none needed, no visible change (design dump hash unchanged).
