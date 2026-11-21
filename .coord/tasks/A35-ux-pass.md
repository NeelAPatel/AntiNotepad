# A35-ux-pass: whole-app UX audit (behavior, visual, icons)

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: High · many screens, judgment calls
State: approved for stages 0-3 (report-only); stage 4 pending its own Approve
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
Which stage 4 batch goes first. Visible changes are lab-first (`AGENTS.md`).

## Files
Stages 0-3 (stage 4 files are listed at its Approve):
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

## Approve
User, 2026-11-20: stage 0-3 scope approved ("kick off ux-pass on its own branch/worktree only after 0.7.0 is live and confirmed on main + installed"); 2026-11-20 after install: "it works 0.7.0 is on main and installed". Stage 4 needs its own Approve. Overrides granted by the user, 2026-11-20: this chat runs on Opus, medium effort; it may start its own side chats; Codex role `terra` (medium) for its tasks; internet research is done by this chat, not Codex (Codex's sandbox has no network, tested 2026-11-20). `AGENTS.md` "never starts other chats" is waived for this item only. User, 2026-11-20 after the stage 0 gate: "going forward no need to wait for my approval , you may proceed through all the stages and make subchats on your own accord for this chat and model. I will await your final report and how we want to apply your findings to the project" (stage gates 1-3 waived; stage 4 still waits for the final report).

## Findings
Stages 0-3 done 2026-11-20, report-only. Reports `.coord/reports/A35-ux-pass-s1.md` (tokens), `-s2.md` (icons; A29 cause: bundled U+F6ED is `person_subtract`), `-s3.md` (13 high, 23 medium, 9 low; six-batch stage 4 proposal). Live focus and contrast checks open.

## For Codex
Role: terra, normal reasoning across several files (`docs/agents.md`). Scans run `CODEX_SANDBOX=read-only`, offline. Never launch the app, never edit this card or STATUS, never commit. Report `.coord/reports/A35-ux-pass-<stage>.md`, short, with a Summary, the table, and checks run.

## Check
(pending)

## Hand test
(pending)
