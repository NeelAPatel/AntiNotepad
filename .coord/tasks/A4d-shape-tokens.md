# A4d-shape-tokens: corner radii and popup shadows come from one token class, look byte-identical

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Low · look-identical (values unchanged, pinned by the design dump)
Branch: `refactor/A4d-shape-tokens` in `.claude/worktrees/refactor/A4d-shape-tokens`, cut from the umbrella
`feat/A4-design-tokens` (not dev) and squashed back into it (created at Approve)

## Request
2026-11-02, user: "Shape, then trim" (start shape-tokens; paint-tokens and timer-names trimmed to deferred). Scope as on the
STATUS row `step: shape-tokens`: corner radii by role and the popup shadows as they are today; `ChromeTokens` extended, not
replaced.

## Findings
- **Kind and why:** refactor; corner radii are hard-coded at 25 call sites, and the menu and dropdown each spell out the
  same shadow.
- **Now:** token scan inventory on the umbrella (d7b95e8): radius 25, shadow 4 (two shadows, Blur and OffsetY each).
  - Files: `UI/SettingsParts.cs` 15, `UI/SettingsWindow.cs` 3, `UI/MotionPreview.cs` 2, `UI/SearchOverlay.cs` 2,
    `UI/PadWindow.Layout.cs` 2, `UI/PadMenu.cs` 1 + shadow, `UI/HelpOverlay.cs` 1.
  - Radius values: 1, 1.5, 3.5, 5, 7, 8, 9, 10, 12, and `8 - ring` (picture inside a ring frame,
    `UI/SettingsParts.cs:317`). Several are half of the element's size (knob 14 → 7, dot 7 → 3.5, swatch 18 → 9, legend
    bar 3 → 1.5, switch track 20 → 10).
  - Shadows: menu `Blur 18, OffsetY 4` (`UI/PadMenu.cs:86`), dropdown `Blur 16, OffsetY 4` (`UI/SettingsParts.cs:249`),
    both colored black at alpha 110 dark / 40 light.
- **Where:** `Theme/ChromeTokens.cs` already holds the pad chrome radii (badge 6, hover 8/9, pill 12) and the pill shadow;
  they stay. `Theme/` is exempt from the Tokens scan. LAYERS: Theme must not use UI or Editor.
- **Risk:** a swap that changes a value. The dump covers the pad (toast included), Settings, help, search and menu. Not in
  the dump: the save-failure dialog (`UI/PadWindow.Layout.cs:512`) and the settings dropdown popup's open list; the review
  checks those two by hand.
- **Source:** master scout.

## Plan
- **After:**
  - New `Theme/Shape.cs`, a static class of consts (the row's `Radius.cs` + `Elevation.cs` become one file, like
    `Typography.cs`):
    - Radii by role: `Hairline` 1 (caret sample), `Key` 5 (key badges in help and the shortcut editor), `Control` 7
      (dropdown box, buttons, text boxes), `Popup` 8 (menu, dropdown list, find box, picture frame), `Card` 10 (settings
      card, search box and results, toast, preview frame), `Dialog` 12 (save-failure dialog).
    - `Round(double size) => size / 2`: fully round shapes (knob, dots, swatch, legend bar, switch track) use
      `Shape.Round(<their size>)`; where the size is a literal in the same initializer, pass that same literal.
    - Popup shadow: `PopupShadowOffsetY` 4, `PopupShadowDarkAlpha` 110, `PopupShadowLightAlpha` 40, `MenuShadowBlur` 18,
      `DropdownShadowBlur` 16.
    - One `///` line per group saying where it is used. Never invent a new value; if a site's role is unclear, use the
      token with the same value and the closest role.
  - Every radius and shadow literal outside `Theme/`, `SelfTest/`, `Calc/` uses these tokens; `8 - ring` becomes
    `Shape.Popup - ring`. `ChromeTokens.cs` is not touched.
  - `tools/design-tokens-baseline.tsv` refreshed with `-Update`: radius and shadow reach 0, and alpha drops by the
    shadow bytes it no longer counts (or the report names each one left and why).
- **Expected time:** about 45 minutes, request to umbrella merge (A4c: two Codex rounds and a hand look, smaller here).
- **Must not change:** any applied value; fonts, spacing, colors, opacity, motion; `ChromeTokens.cs`; files outside the list.
- **Scope questions:** None.
- **Test plan, code:** `--selftest` passes with the DESIGN hash unchanged
  (`8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b`, 16160 lines); Tokens counts down. Review: every
  removed literal maps to a token of the same value; the save-failure dialog and dropdown popup first.
- **Test plan, UI:** dev build, about one minute: the pad menu, a Settings dropdown opened, a switch, the theme cards,
  search (Ctrl+F) and a toast. Nothing looks different.
- **Gates:** tier defaults: Build, Selftest (DESIGN hash = the look proof), Format, Seam, Tokens (radius and shadow fall,
  none rise), Scope, Layers (Shape must not use UI or Editor).
- **Needs from you:** approve; the one-minute hand look.

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Any other file needs a line under
"Out-of-scope files" in the report.
- `product/AntiNotepad/Theme/Shape.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/UI/MotionPreview.cs`
- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/PadMenu.cs`
- `product/AntiNotepad/UI/HelpOverlay.cs`
- `tools/design-tokens-baseline.tsv`

## Contracts
- None changed. The DESIGN selftest hash is the promise and must stay as above.

## Approve
2026-11-02, user: "Approve"

## For Codex
Role: terra (`docs/agents.md`), because it is a careful value-preserving swap across 7 files.
Product `product/AntiNotepad`. The branch is already checked out: never switch, merge or commit (you cannot write to
`.git`). Read `AGENTS.md`, `docs/coding.md`, then the files above. Line numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above. No drive-by refactors or font/spacing/color/opacity/motion edits.
- Every swap keeps the exact value. Never change the DESIGN hash constant in `SelfTest/Design.cs`; if the DESIGN line
  fails, a swap changed a value: find it and fix the swap.
- Keep CRLF line endings. Lines you touch must pass the gate's Format step (one initializer entry per line where the
  formatter asks for it).
- Get the inventory with `pwsh -NoProfile -File tools/check-design-tokens.ps1 -Inventory <path>`; refresh the baseline
  with `-Update` only after the swaps.
- Build into `build/A4d-shape-tokens-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A4d-shape-tokens -Offline -Base feat/A4-design-tokens`; the
  manager re-runs it online.
- Never launch or kill AntiNotepad except `--selftest` from your check build. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means (offline checks only: build, selftest, gate, script checks, files; never ports, servers, browser or screen):
- `Theme/Shape.cs` exists as in "After"; no radius or shadow literal remains outside `Theme/`, `SelfTest/`, `Calc/`
  (or the report lists each one left and why).
- `--selftest` prints `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160` and `SELFTEST PASS`.
- Build 0 warnings, the gate passes.

Before you report (director only): spawn the `lens_reviewer` agent on your diff (it reads
`.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why not. Workers without spawning: run the same
checklist yourself and say so.

Report: `.coord/reports/A4d-shape-tokens.md`, short, with exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per file: `file | literals swapped (count) | tokens used`.
3. **Behavior changes**: write "None" only if true; the review checks it against the diff.
4. **Out-of-scope files**: file and why, or "None".
5. **Plan deviations**: anything done differently from the Plan, and why, or "None".
6. **Contracts**: rows added or changed, or "None".
7. **Lens review**: findings and what you did with each.
8. **Checks run**: build, selftest (with the DESIGN line), gate result lines, Tokens counts before and after.
9. **Hand checks for the user**: the Plan's UI checks.

## Check
2026-11-02, reviewer. Round 1 + `-r2` (Format reflow only).
- Gate (online, `-Base feat/A4-design-tokens`): Build 0 errors, only the 2 expected NU1900 warnings; Selftest, Seam, Tokens, Format, Scope, Size PASS. Round 1 failed Format (35 whitespace lines on 14 touched lines); `-r2` fixed it, whitespace only.
- `--selftest`: `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160`, `SELFTEST PASS`.
- Values: every removed radius/shadow literal maps to a token of the same value (Round(14)=7, Round(20)=10, Round(7)=3.5, Round(3)=1.5, Round(18)=9, `Popup - ring` = `8 - ring`; shadow blur 18/16, offset 4, alpha 110/40). Save-failure dialog 12 = `Dialog`; dropdown popup 8, blur 16, offset 4 unchanged. **within plan**.
- CRLF on all changed files; `ChromeTokens.cs` and `SelfTest/Design.cs` untouched. **within plan**.
- Literals left (per report): `SettingsParts.cs` radius 0 and 6 (no role), `Kit.cs` defaults 7/8, `PadView.cs` 3 (outside Files). **justified**: unchanged. Scanner: radius 25 to 6 (size args to `Round`), shadow 4 to 0, alpha 7 to 5, color 8 to 10 (RGB zeros in shadow calls). **justified**.
- Palette card uses `Round(18)` for radius 9 (same value). **justified**.
- Noticed (proposals): the gate's Format step does not check the formatter's exit code; Codex's sandbox cannot run `dotnet format`, so Format only shows online.
- Misses: Format failed on touched initializer lines; found at the online gate; cause `code`; the offline gate cannot run Format; lesson: run the gate online before accepting and tell Codex to reflow touched initializers one entry per line.
Verdict: accepted.

## Hand test
2026-11-02, user: "All unchanged" (pad menu, Settings dropdown, switch, theme cards, search, toast). Dev build from this worktree, DATA `build/testdata`.
