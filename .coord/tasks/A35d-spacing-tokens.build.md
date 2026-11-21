# A35d-spacing-tokens build: four steps, one Codex run each

Each run does **one step only** (named in its wrapper). Claude commits between steps; the next run starts on that commit.
Read `AGENTS.md`, `.claude/rules/coding.md`, the item card `.coord/tasks/A35d-spacing-tokens.md`, the snap list
`.coord/reports/A35d-spacing-tokens-snaps.md`, `docs/design-scale.md` (R column, sections 2.9, 3.2 and 6), and the
token audit `.coord/reports/A35-ux-pass-s1.md` with `-s1-classified.tsv`. The lab `tools/playground/a35d-lab/` shows the
approved After. Line numbers in the snap list predate a rebase, so find each site by its content.

Rules for every step: the branch is checked out, so never switch, merge or commit. Edit only the card's Files. Never
launch or kill AntiNotepad, never run `--quit`, and never edit `.coord/STATUS.md` or the card. Use offline checks only.
Build into `build/A35d-spacing-tokens-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest`, then delete the folder.
The gate is `pwsh -NoProfile -File tools/gate.ps1 -Task A35d-spacing-tokens -Offline`. Done means 0 build warnings,
`SELFTEST PASS`, and the gate passing.

## Step 1: tokens and on-scale migration (no pixel moves)
- Add `Theme/Space.cs`: `Nudge 2, Xs 4, S 8, M 12, L 16, Xl 24, Xxl 32`, with a file header and summaries. Add stroke
  tokens `Thin 1, Thick 2` to `Theme/Shape.cs`. Add named size tokens for the on-grid window, panel, popup and row sizes
  the audit lists, in `Theme/` where they fit (a `Size` group or the owning token class).
- In `ChromeTokens`, alias each spacing value that equals a step to that step (`= Space.Xs`), per design-scale 3.2.
- Replace every on-scale spacing, size and stroke literal in the Files with its token. Zero stays a literal `0`.
- Add the named exceptions and derives from the snap list's Derive and Exceptions tables as tokens, with today's
  values. Examples: editor text inset 36/30, hint = inset + 1, toggle knob inset 3, text-fit column widths, caps letter
  spacing 0.8, the menu icon box from `IconGlyphSize + GlyphIconBoxExtra`. Give each exception a one-line why comment.
- **Proof:** `DesignHash` in `SelfTest/Design.cs` must stay byte-identical and the selftest must pass unchanged. If the
  hash moves, a value moved; find it and fix it.

## Step 2: scanner and baseline
- `tools/check-design-tokens.ps1`: the spacing category ignores a literal `0` (and `new Thickness(0)`). Add a `stroke`
  category for literal `new Pen(` thicknesses, and count literal radius arguments of `DrawRectangle(` and `RoundedRect(`
  as `radius`. Add one self-check fixture per rule, both hit and no-hit.
- Run `-Update` to rewrite `tools/design-tokens-baseline.tsv`. Report the old and new totals per category.

## Step 3: snaps (pixels move)
- Apply snap-list rows 1-18 with tokens (After values). Row 19: one shared method sets Fluent's `ControlCornerRadius`
  resource to `Shape.Control`. `Program.cs` and the selftest setups in `SelfTest/Design.cs` and `SelfTest/UI.cs` call it,
  so the app and the selftests see the same look.
- Re-pin `DesignHash` to the new value. In the report, list the `--design-dump` diff (before vs after, by control) and
  map each changed line to a snap row. A changed line with no row is a bug.
- Run `-Update` on the baseline again if counts dropped.

## Step 4: doc drift
- `docs/design-scale.md`: add the 10 tokens the audit lists as missing, plus `Space` and the stroke tokens. Set
  `PagerDotButtonHeight` to 32. Add the new exceptions to section 6, each with its reason. No other doc changes.

Report per step: `.coord/reports/A35d-spacing-tokens-build-<step>.md`, short. Sections: 1 Summary; 2 Change list
(`file:lines | what | step line`); 3 Behavior changes; 4 Out-of-scope files; 5 Deviations; 6 Checks run (with the
`DESIGN dump` line and the TOKENS lines); 7 Hand checks for the user (step 3 only).
