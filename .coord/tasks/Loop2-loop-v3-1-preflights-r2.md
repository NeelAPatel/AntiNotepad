# Loop2-loop-v3-1-preflights-r2: placeholder rule misses labeled template lines

Rework round of `Loop2-loop-v3-1-preflights` (`docs/loop.md`). Read the item card `.coord/tasks/Loop2-loop-v3-1.md` and the
step brief `.coord/tasks/Loop2-loop-v3-1-preflights.md` first; they still apply unless this file says otherwise.

Tier: Low · docs-tools
Role: director

## Why this round
Proven review finding (`.coord/reports/Loop2-loop-v3-1-preflights-review.review.md`, finding 1): in
`tools/merge-preflight.ps1` (~line 35-37, same rule in `tools/approve.ps1`) a section counts as unfilled only if a line
starts with optional bullet then `<...>`. The real template (`.coord/templates/item.md`) puts Plan placeholders after bold
labels, e.g. `- **After:** <what it will do, next to "Now">`, so an unfilled Plan passes: false PASS.

## Change to the plan
None. Fix: a section is unfilled when it is empty or any line, after stripping an optional bullet (`-`/`*`) and an optional
bold label (`**Label:**`), starts with `<...>` (a whole `<text>` token). Put the rule in one place per script (a small
function), the same in both scripts. Keep lines that merely contain `<` later in the text as filled. Do not treat an
inline-code line like `` `<prefix>/x` `` as a placeholder.

## Files
Edit only `tools/merge-preflight.ps1` and `tools/approve.ps1`.

## Done means
- Probe in a throwaway clone under `build/Loop2-loop-v3-1-probe` (delete after, including any bundle/script leftovers;
  a cleanup that is blocked: say so): a card with the real item template's Plan left verbatim and the other sections
  filled => merge-preflight FAIL (Plan unfilled); the same card with Plan filled => PASS; Approve template line
  `<The user's answer...>` still FAIL in approve; labeled placeholder in approve's Approve section FAIL.
- Earlier results unchanged (spot-check approve PASS, merge PASS/behind-dev FAIL).
- `pwsh -NoProfile -File tools/gate.ps1 -Task Loop2-loop-v3-1 -Offline` passes.

Constraints: no commit, push, branch switch or merge in this checkout; never edit STATUS, the card, product.
Report: `.coord/reports/Loop2-loop-v3-1-preflights-r2.md` (short, step report format).
