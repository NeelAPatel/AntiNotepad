# A35-ux-pass-s1: token audit brief (stage 1)

Role: terra, read-only scan across many files with judgment on each literal (`docs/agents.md`).
Sandbox: read-only, offline. Never launch or kill AntiNotepad, never build, never edit or create files, never commit.
Your **final message is the report** (the item chat saves it as `.coord/reports/A35-ux-pass-s1.md`).

## Goal
The user wants one whole-app UX audit (`.coord/tasks/A35-ux-pass.md`, read it). Stage 1 finds every design value that
skips the token system, and checks the tokens themselves against the approved scale. Report only. Motion durations and
easing are **out of scope** (user, 2026-11-20): list `tween` hits in one line count, do not classify them.

## Inputs
- Target scale: `docs/design-scale.md`, the **R** (Recommended) column. It was applied by A4n-scale-lab (user, 2026-11-05,
  `docs/decisions.md` "Bar insets and chrome tokens"). Section 5 says layout sizes snap to the 4 grid but are not steps;
  section 6 lists the named exceptions.
- Rules: `.claude/rules/coding.md` "Design values" (new radius, font, color, shadow, opacity, motion, timer values live in
  `Theme/` or `Motion/`).
- Tokens: `product/AntiNotepad/Theme/*.cs` (`Typography`, `Shape`, `Paint`, `Palette`, `ChromeTokens`, `DevSlotTokens`).
- Scanner output: `.coord/reports/A35-ux-pass-s1-inventory.tsv`, made by `tools/check-design-tokens.ps1 -Inventory`
  (224 hits; columns category, file, line, matched text, code). The scanner skips `Theme/`, `SelfTest/`, `Calc/`.

## Do
1. **Classify every inventory row** (spacing, radius, font-size, color, opacity, timer) as one of:
   - `on-scale`: the literal equals an R step (say which, e.g. "S 8"); still a literal, so name the token it should use if
     one exists.
   - `layout`: a window, panel, column or popup size (section 5), not a scale step. Say whether it is on the 4 grid.
   - `exception`: matches a section 6 exception; give its number.
   - `off-scale`: none of the above. Give the nearest step by the section 2 rounding rule (nearest; tie goes up) and the
     token that would hold it.
   - `timer`: debounce, delay or polling interval (not motion). Say what it times and whether a `Motion/` or `Theme/` token
     already exists for it.
   Group rows that share a file and value into one table row with a line list, to keep the table short.
2. **Find what the scanner misses.** Read `UI/`, `Editor/`, `Platform/` (UI parts only) for literal design values the
   regexes do not catch, for example: pen and stroke thickness (`new Pen(`, `StrokeThickness`, `Thickness` passed to
   helpers), icon or font sizes passed as plain helper arguments (`Kit.*`, `Glyph(`, `GlyphIcon(`), drawing geometry in
   `Render` overrides (`DrawRectangle`, `DrawEllipse`, radii in `RoundedRect`), `Color.Parse`, `Colors.*` named colors,
   literal alpha math (`* 0.5`), hard-coded font families. List each with file:line and the same classes as step 1.
3. **Check the tokens themselves.** For each token in `Theme/` (and `ChromeTokens`), compare today's value with the R
   column of `design-scale.md` sections 3.1 and 3.2. List only rows where they differ, plus any token not in the doc.
4. **Count and summarize.** Per category: total, on-scale literals, layout, exceptions, off-scale, missed-by-scanner.
   Name the five files with the most off-scale values.

## Report format (final message)
```
# A35-ux-pass-s1: token audit
## Summary            (5-8 bullets, counts first)
## Off-scale          (table: file:lines | category | literal | nearest step | token to use)
## On-scale literals  (table, same columns; "token to use" may say "none yet")
## Layout sizes       (table: file:lines | value | on 4 grid?)
## Exceptions         (table: file:lines | value | section 6 number)
## Timers             (table: file:lines | value | what it times | token?)
## Missed by scanner  (table: file:line | kind | literal | class | nearest step)
## Tokens vs scale    (table: token | today | R value | note)
## Tween              (one line: count only, out of scope)
## Checks run         (commands you ran)
READY: yes | no - <what is missing>
```
Every claim carries file:line. Write "none" for an empty section. Do not propose code changes beyond naming the token.
