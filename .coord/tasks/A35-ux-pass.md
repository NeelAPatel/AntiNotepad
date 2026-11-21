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

## Stages (stop at each gate for the user)
| Stage | Output | Who | Gate |
|---|---|---|---|
| 0 Research | `.coord/research/ux-principles.md`: each principle, source link, how to test it here | this chat, internet | user skims |
| 1 Token audit | table: literal, file, nearest token or named exception (`docs/design-scale.md` is the target) | Codex terra, offline | user reads summary |
| 2 Icon inventory | table of every icon, font, code, size, weight, what changes it; rendered contact sheet | Codex terra scan, this chat renders | user approves sheet |
| 3 Findings | per screen (pad Standard/Permanent/split, settings, search, menus, toasts, save-failure card, shortcut map, dev badge): finding, lane, evidence, severity, fix | this chat, screenshots | user picks what to fix |
| 4 Fixes | only the picked findings, small lab-first batches, icon registry and checks | Codex terra, this chat gates | hand test per batch |

## Needs from you
Stage gates, the icon contact sheet approval, which findings to fix. Visible changes are lab-first (`AGENTS.md`).

## Files
Stages 0-3: `.coord/research/`, `.coord/reports/`, `tools/playground/` (lab and contact sheet) only. Stage 4 files are listed at its Approve.

## Approve
User, 2026-11-20: stage 0-3 scope approved ("kick off ux-pass on its own branch/worktree only after 0.7.0 is live and confirmed on main + installed"); 2026-11-20 after install: "it works 0.7.0 is on main and installed". Stage 4 needs its own Approve. Overrides granted by the user, 2026-11-20: this chat runs on Opus, medium effort; it may start its own side chats; Codex role `terra` (medium) for its tasks; internet research is done by this chat, not Codex (Codex's sandbox has no network, tested 2026-11-20). `AGENTS.md` "never starts other chats" is waived for this item only.

## For Codex
Role: terra, normal reasoning across several files (`docs/agents.md`). Scans run `CODEX_SANDBOX=read-only`, offline. Never launch the app, never edit this card or STATUS, never commit. Report `.coord/reports/A35-ux-pass-<stage>.md`, short, with a Summary, the table, and checks run.

## Check
(pending)

## Hand test
(pending)
