# A59-design-ownership-audit: read-only audit of ownership, duplication and design values

Role: director (gpt-6.1-sol medium); spawn workers (gpt-6-luna xhigh) for the sweeps if useful. Product `product/AntiNotepad`.
Branch: none. Work in the current `dev` checkout, read-only. Never switch, merge or commit (you cannot write to `.git`).
Tier: Medium (audit item; rules: `docs/loop.md`)
Read first: `AGENTS.md`, `docs/loop.md`, `docs/decisions.md`, `.coord/tasks/P1-project-reevaluation-plan.md` (sections 2-4
and 7 are what you confirm or correct). Use `rg` and targeted reads, not whole-file dumps.

## Goal
The user wants three things from the coming reorg: one owner per responsibility, less duplicated code, and one central
design system (one source of truth per value: corner radius, fonts, colors, shadows, motion). The user's first rule:
**the app's look and feel stay identical**; tokens take today's values, and only lab-approved changes may ever alter pixels.
This audit gives the facts each later loop item needs. It changes no code.

## Files
Only these files may be created. Nothing else may change.
- `.coord/reports/A59-design-ownership-audit.md`
- `.coord/reports/A59-design-ownership-audit-inventory.tsv`

## Contracts
None touched (read-only). Name any existing `CONTRACTS` rows (`product/AntiNotepad/SelfTest/Contracts.cs`) each proposed
item would touch.

## Constraints
- Read-only: no edits to product code, tools, docs or STATUS. No builds needed; if you build, use `build/A59-check` with
  `AVALONIA_TELEMETRY_OPTOUT=1` and delete it after.
- Never launch or kill AntiNotepad. Never run `--quit`.
- Facts with `file:line`. Mark anything inferred as "inferred".
- Do not choose new design values. Where near-duplicate values exist (for example radius 7/8/9/10), list them as candidate
  groups for the later lab (A64-design-unify-lab); the user decides there.
- Item names are always `<ID>-<slug>` with their scope; never a bare ID or a range.

## Done means
The report answers every section below, and the inventory file lists every hard-coded design value.

## Before you report (director only)
Spawn the `lens_reviewer` agent on your report (it reads `.agents/skills/lens-review/SKILL.md`) and ask it to check the
facts: sample 10 `file:line` claims and recount one inventory category. Fix what it proves.

## Report: `.coord/reports/A59-design-ownership-audit.md`
Keep it short and scannable (tables). Use exactly these sections:
1. **Summary**: three to five sentences; what in the P1 plan's section 2 is confirmed, corrected or missing.
2. **Large files**: every file over 600 lines with size and the responsibilities it holds.
3. **Ownership map (draft)**: one row per responsibility (colors, fonts, radius, shadows, motion/tweens, scroll/caret
   physics, brushes, window material, time text, notes paths, settings, search, selftests, logging, app lifetime):
   current owner(s) with `file:line`, proposed single owner (folder/class), conflicts.
4. **Duplication**: a full sweep, beyond name matches (same logic under different names, copy-pasted blocks, parallel
   control kits). One row each: what, every `file:line`, proposed fix, which item fixes it.
5. **Design value catalog**: per category (radius, font family, font size, font weight, color construction, brush alpha,
   shadow, tween duration, easing, opacity, thickness, spacing/size, behavior timers): count, distinct values, proposed
   token names at today's values (e.g. `Radius.Card = 10`), and candidate unification groups for the lab. Also say what
   `Theme/ChromeTokens.cs`, `Theme/Palette.cs` and `Motion/Anim.cs` already cover.
6. **Per-item facts**: for each proposed item in the P1 plan section 4 (A60-design-token-scan, A61-theme-tokens-core,
   A71-tokens-pad-chrome, A72-tokens-overlays, A73-tokens-editor, A62-settings-tokens, A63-motion-tokens,
   A66-ui-kit-merge, A67-model-store-split, A70-selftest-split, A68-notes-path-dedupe, A69-ownership-map): exact file list,
   suggested tier, riskiest part, `CONTRACTS` rows touched, hand checks for the user, and whether the item should be split,
   merged or dropped (with why).
7. **Look-identical proof**: how each migration item can prove pixels did not change (selftest rows, what the user should
   look at), and any spot where a value is computed, animated or theme-dependent so a plain token swap could change it.
8. **Conflicts with planned bug fixes**: files shared with A52-move-old-folder-rename-warning, A58-split-divider-height,
   A54-link-hover-click, A57-split-view-clip, A42c-checkbox-indent, A42d-list-commands.
9. **Open questions for the user**.

## Inventory: `.coord/reports/A59-design-ownership-audit-inventory.tsv`
Tab-separated, one row per hard-coded design value outside `Theme/` and `SelfTest`:
`category	file	line	value	context (short code excerpt)	proposed token`.
