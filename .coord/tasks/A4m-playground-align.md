# A4m-playground-align

Tier: Medium · Branch: chore/A4m-playground-align (off feat/A4-design-tokens) · Step of A4-design-tokens

## Request
User, 2026-11-05: "let me know when all the design token data and the localhost playground are both aligned and ready
for me to review and figure out if the new token dimensions are right for me."

## Findings
Read-only audit: `.coord/reports/A4m-playground-align-audit.md` (37 rows). Every role's numeric today value matches the
product source. Blocking: #1 Alt preset cannot reproduce the proposal's Alt (roles bound only to their R scale entry);
#2 PagerDotButtonHeight hard-coded 18, so the locked 18->20 change is invisible. Rows 36-37 refuted (no change).

## Plan
1. Fix #1: per-role Alt step in tokens.json (e.g. `stepsAlt`), `setPreset` reads it; check (a)-(e) of row 1 under Alt.
2. Fix #2: PagerDotButtonHeight follows a token/per-preset size (18 Today, 20 R).
3. Fix cosmetic rows 3-24 and 28 (page literals, scale-entry today arrays, role strings).
4. measure.md rows 29-35 (pill y 597, client x values, origin offset, labels, sums).
5. Exceptions (26): add an `exceptions` list (13 rows) to tokens.json, shown read-only on the page.
6. Honest preview (section D of the audit): the "on canvas" tag counts only what the current view actually draws;
   a card whose token draws nothing in the current view says so ("not drawn in this view" / "not drawn yet").
   Rows 27 (TopBackdrop) and the split divider may stay undrawn but must be tagged that way.
Not in scope: new views, small window, lock switch, product code.

## Files
- `tools/playground/index.html`
- `docs/playground/tokens.json`
- `docs/playground/measure.md`

## Checks
JSON parses; page loads at localhost:47812 with no console errors; Today / Recommended / Alternative presets each spot-
checked against design-scale.md (row 1 a-e, row 2); the gate (Scope, Format) passes with -Base feat/A4-design-tokens.

## Approve
User, 2026-11-05 (the request above asks for alignment before review; master opened this step to deliver it).

## For Codex
Role: terra (`docs/agents.md`), because it is careful data and page edits across three files with design-scale.md as truth.
The branch is already checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`,
`docs/coding.md` (as far as it applies to the page), `docs/design-scale.md`, the audit `.coord/reports/A4m-playground-align-audit.md`
(all 37 rows and section D), then the three Files. Line numbers in the audit are from the umbrella tree; re-read before editing.

Constraints:
- Do Plan steps 1-6 and the audit rows named there (1, 2, 3-24, 28, 29-35, 26, section D). Rows 25, 27 (may stay undrawn
  but tagged), 36, 37 need no change. Product code is out of scope; do not touch `product/`.
- Step 1: add per-role Alt step (`stepsAlt` or similar) to tokens.json for every role whose Alt differs from its R step,
  taken from design-scale.md Alt column; `setPreset("alternative")` must use it so Alt reproduces row 1 a-e (Corner.Sm 6 for
  Control and Badge, 8s and 7s stay 8 under Alt, ToastIn/OverlayIn fold to Base 160, .78/.8 fold to .9, focus keeps 1.5).
- Step 2: PagerDotButtonHeight reads 18 for Today and 20 for Recommended, through a token or per-preset size, and the
  page's dotBtnH uses it.
- Step 6: the "on canvas" tag counts only what the current view (view, theme, mode, reveal) actually draws; a card whose
  tokens draw nothing says "not drawn in this view" or "not drawn yet". Fill `used` after the cuts, not before.
- Step 5: `exceptions` list (13 rows from design-scale.md exceptions) in tokens.json, shown read-only on the page.
- Keep the page a single HTML file, no libraries; keep existing behavior (compare overlay, picks, markers, eye buttons).
- Keep CRLF/LF as the files already have. tokens.json must parse (`pwsh -c "Get-Content ... | ConvertFrom-Json"` or node).
- Never edit `.coord/STATUS.md` or this card. Never launch AntiNotepad; no ports, servers or browsers (sandbox cannot).

Done means (offline checks only):
- tokens.json parses; every audit row 3-24, 28 fixed as the audit's "Correct value" column says; measure.md rows 29-35 fixed.
- A small node or pwsh script of your own (not committed) confirms: under Alt each of row 1 a-e values resolves as DS Alt;
  under R the dot button height is 20.
- `pwsh -NoProfile -File tools/gate.ps1 -Task A4m-playground-align -Offline -Base feat/A4-design-tokens` Scope and Format pass.

Report at `.coord/reports/A4m-playground-align.md`: sections What changed (by audit row), Choices, Gaps (cards that still preview
nothing, with why), Out-of-scope files.
