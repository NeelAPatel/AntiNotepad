# A35-ux-pass-s1b: token audit, part 2 (missed values and tokens vs scale)

Role: terra, read-only scan with judgment across several files (`docs/agents.md`).
Sandbox: read-only, offline. Never launch or kill AntiNotepad, never build, never edit or create files, never commit.
Your **final message is the report**. Goal and context: `.coord/tasks/A35-ux-pass.md` (whole-app UX audit; stage 1 finds
design values that skip the token system). The first run covered too much; the item chat classifies the 224 scanner
rows itself. **You do only the two parts below, completely.** Motion durations and easing are out of scope.

Target scale: `docs/design-scale.md` R column (applied by A4n-scale-lab, `docs/decisions.md`). Exceptions: its section 6.

## Part A: literals the scanner misses
`tools/check-design-tokens.ps1` catches `Margin =`, `Padding =`, `Width =`, `new Thickness(`, `CornerRadius`, `FontSize =`,
hex colors, `Color.FromArgb` and a few helper calls. List every **other** literal design value in
`product/AntiNotepad/UI/*.cs`, `product/AntiNotepad/Editor/*.cs` and `product/AntiNotepad/Motion/*.cs` (skip `Theme/`,
`SelfTest/`, `Calc/`, `Model/`, `Platform/`). Kinds to look for:
- pens and strokes: `new Pen(`, `new ImmutablePen(`, `StrokeThickness`, thickness arguments;
- geometry in `Render` overrides: `DrawRectangle`, `DrawEllipse`, `new RoundedRect(`, `new Rect(` sizes, radii;
- sizes passed as plain helper arguments: `GlyphIcon(` size, `Kit.*(` numbers, `Typography.X + 2` style arithmetic;
- colors: `Color.Parse`, `Colors.<Name>`, `Brushes.<Name>`; alpha math (`* 0.5`, `/ 2` on opacity);
- `Opacity` set to a variable computed from a literal; font families as string literals.
Skip 0, 1 used as a multiplier, and pure layout arithmetic (`width / 2`). Per hit: file:line, kind, literal, class
(`on-scale` with step, `layout`, `exception` with number, `off-scale` with nearest step by the doc's rounding rule).

## Part B: every token against the scale
For **every** `const` and `static readonly` value in `product/AntiNotepad/Theme/*.cs`, give today's value and the R value
from `design-scale.md` sections 3.1 and 3.2. Output only the rows that differ or are not in the doc (say "not in doc").
Known already: `PagerDotButtonHeight` is 32 today versus 20 in the doc. Say whether the doc or the code looks stale (look
at `git log -S` on the token if useful).

## Report format (final message)
```
# A35-ux-pass-s1b
## Summary         (counts per kind and class; top 5 files by off-scale count)
## Missed by scanner (table: file:line | kind | literal | class | nearest step)
## Tokens vs scale (table: token | today | R | stale side | note)
## Checks run
READY: yes | no - <what is missing>
```
