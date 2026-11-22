# A35c-design-contract: one design contract, enforced by checks

Item card (`docs/loop.md`). Last step of `A35-ux-pass`; gates the merge of `docs/A35-ux-pass` to dev. Branch `feat/A35c-design-contract` from `docs/A35-ux-pass` 101fad6, in `.claude/worktrees/feat/A35c-design-contract`.

Tier: Medium · docs, checks, small refactors; no visible change planned
State: built and gated; B-9 deferred by the user (2026-11-21); waiting on hand-back

## Request
User, 2026-11-20 (via A35-ux-pass): "Before we merge back to dev, i want to ensure we have a proper design contract based on whatever we explored, learned, and fixed, to the point of enforcing the design paradigm so we dont have inconsistencies, hardcoded tokens, etc."

## Findings
- **Docs today:** `docs/design-scale.md` (361 lines) is still headed "Scale proposal ... nothing applied"; `design-systems.md` (study) and `design-fluent.md` (study) sit beside it; `decisions.md` points at scale section 3.2. No doc covers the icon registry, focus ring, contrast rules, or labels and tooltips.
- **Ratchet:** `check-design-tokens.ps1` passes with 27 baseline rows. By category: 15 timer or tween (motion, out of scope), 17 radius, 6 color, 5 spacing, 4 font-size, 3 stroke, 1 opacity. Hot spots: `SettingsParts.cs` radius 9, `Renderers.cs` radius 6. Exceptions are prose (scale section 6, 25 rows), not machine-read.
- **Existing checks:** `SelfTest/Design.cs` pins a SHA of a rendered dump (catches any change, explains none). `SelfTest/Contrast.cs` sweeps 8 palettes x 8 accents plus none for 11 pairs; it does not cover hint text, markdown markers or placeholder text. `SelfTest/Icons.cs` checks every code exists in its font.
- **Gaps:** nothing fails when a token is added without a doc row; nothing walks rendered controls for off-scale values; exceptions are unnamed in code.
- **Inputs:** Clear-tint hint 1.24:1 stays (user kept the old look; named exception). B-9 dimming. Product files over 600 lines: five (`PadView` 1075, `SettingsWindow` 1016, `PadWindow.Layout` 903, `SettingsParts` 698, `PadWindow` 683), not two. The `Size` gate step only warns on growth. Lab font copy 2.9 MB in `tools/playground/a35d-lab`. 23 survival lines are not restored.

## Plan
1. **Doc.** New `docs/design-contract.md`: rules first (spacing, sizes, type, corners, strokes, icons, focus ring, contrast, labels and tooltips, motion pointer), then a token table, then named exceptions (machine-readable rows: id, file, reason). Fold in what is still valid from `design-scale.md`; delete it; keep `design-systems.md` and `design-fluent.md` as background studies linked from the contract. Fix `decisions.md` links.
2. **Ratchet to named exceptions.** Replace the count baseline with `tools/design-exceptions.tsv` (file, category, count, reason). Fix what is a real token miss (radius, spacing, font-size, color); the rest get reasons. Timers and tweens stay exempt as motion. Goal: no row without a reason.
3. **Rendered-tree check.** A selftest walks the live pad, settings, search and help trees and fails on any FontSize, CornerRadius, Thickness, stroke or icon size not in the token value set or the exception list.
4. **Contrast.** Extend `SelfTest/Contrast.cs` with the hint, markers and placeholder pairs. The Clear-tint hint is a named exception, not a failure.
5. **Doc sync.** `tools/check-design-contract.ps1`: every public const in `Theme/*.cs`, `ChromeTokens` and `Icons` names appear in the contract table; every table row exists in code. Wired into `gate.ps1` as a `Contract` step beside `Tokens`.
6. **Custom controls and generator.** Contract section "Custom controls": each drawn or special control is declared once (tokens used, exceptions needed); one exception per drawn control, not per number. `tools/check-design-contract.ps1 -Write` regenerates the token table rows from code, so doc sync is a one-command fix.
7. **Housekeeping.** Lab points at the bundled font, 2.9 MB copy removed. B-9: disabled-with-reason for dependent controls (visible; lab first). File size: split the files that split cleanly; name the rest in the exceptions file; `Size` step reads that list.
- **Needs from you:** Approve; the B-9 lab look; hand test of a slot dev copy.
- **Gates:** `tools/gate.ps1 -Task A35c-design-contract -Offline -Base docs/A35-ux-pass`; `python -I .coord/tools/a35-survival.py feat/A35c-design-contract`.

## Files
- `docs/design-contract.md`
- `docs/design-scale.md`
- `docs/decisions.md`
- `tools/check-design-tokens.ps1`
- `tools/design-tokens-baseline.tsv`
- `tools/design-exceptions.tsv`
- `tools/check-design-contract.ps1`
- `tools/gate.ps1`
- `tools/playground/a35d-lab/core.css`
- `tools/playground/a35d-lab/FluentSystemIcons-Regular.ttf`
- `product/AntiNotepad/SelfTest/Design.cs`
- `product/AntiNotepad/SelfTest/Contrast.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Run.cs`
- `tools/size-exceptions.tsv`
- `docs/playground/measure.md`
- `docs/playground/tokens.json`
- `product/AntiNotepad/Editor/Renderers.cs`
- `product/AntiNotepad/Editor/Structure.cs`
- `product/AntiNotepad/UI/MotionPreview.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
Later rounds add the B-9 settings files and any split files.

## Approve
User, 2026-11-21: "Add both and approve" (custom-controls section and doc-row generator added to the plan).

## For Codex
Role: terra, a multi-file change across docs, scripts and selftests (`docs/agents.md`). Offline; no ports, no app launches. Never edit this card or STATUS; never commit.
- **Run 1 scope: Plan items 1-6 and the lab font in 7.** Not B-9 (visible, lab first) and not file splits (a later round).
- Item 2: the exceptions file's reasons come from `docs/design-scale.md` section 6 where a row matches; fix a literal only when a token already fits exactly, else name it. Motion categories (timer, tween) get one blanket reason row.
- Item 3: the walk list of screens lives in one place in `SelfTest/`.
- Item 5: the checker reads `Theme/*.cs`, `ChromeTokens`, `Icons`; `-Write` regenerates the table rows between marker comments in the doc.
- Build: `AVALONIA_TELEMETRY_OPTOUT=1`, into `build/A35c-design-contract-check`, `--selftest`, then delete that folder.
- **Done means:** build passes; `--selftest` SELFTEST PASS; `pwsh tools/check-design-tokens.ps1` and `pwsh tools/check-design-contract.ps1` exit 0; `tools/gate.ps1 -Task A35c-design-contract -Offline -Base docs/A35-ux-pass` has no FAIL; the design dump hash changes only if you list why.
- Report `.coord/reports/A35c-design-contract.md`: Summary, files changed, exceptions list with reasons, checks run.

## Check
(pending)
