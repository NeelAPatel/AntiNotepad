# L6-lv4-docs-diet-r2: restore rules the first pass reworded or dropped

Rework round 2 of `L6-lv4-docs-diet` (`docs/loop.md`). Read `.coord/tasks/L6-lv4-docs-diet.md` first (Plan, Files, Constraints), then
the old file with `git show dev:docs/loop.md`. Role: quick (Luna medium), because it is a verbatim restore. Never commit; never edit the card or STATUS.

Why: round 1 paraphrased parts of the core. The rule was "move text verbatim, drop no rule". Master reads the core as law, so
it must keep the old wording. A budget of about 5 KB is secondary to that: the core may grow to about 11 KB.

Do, in `docs/loop.md` only (plus the one pointer file noted):
1. **Phases table:** restore the old rows' full cell text (Who, Leaves on the card, STATUS state columns) verbatim, e.g. `auditor` sub-session
   wording, "the director spawns `lens_reviewer`", `## Go` note and the full `paused` paragraph.
2. **Tiers:** restore the old table verbatim ("Touches" lists with `LAYERS rows`, `Store`/`NotesSession`/..., "hand test always" for High).
3. **Status card:** restore the whole old section verbatim: the one-line form, the full-card code block, every bullet (decision block pointer,
   symbol meaning, sub-session role, the tier reason slug list, Markdown line rules, several items, quiet relays, the hook line).
4. **Rules:** restore every old bullet verbatim except the ones now living in `.claude/skills/review-task/phases.md` (rework rounds,
   gate reports, CONTRACTS row stays in core). Specifically bring back: Codex runs in their own linked visible sub-session; sub-sessions own
   Build/Check/Hand test and ask decisions with the card and record the answer verbatim; fast-lane quote with date; `Loop`/`D` short cards;
   "Who builds"; progress checks; rule overrides; proposals into STATUS at once; Merging (incl. sub-session merges only into branches it
   created); Loop freeze with date; the roadmap server line.
5. **IDs and numbering:** restore the old section verbatim (all four prefix rows with examples) and keep the `L` row, the L1 restart note
   and "lowercase slugs" as additions. Restore the eras bullet and "Always write `<ID>-<slug>`".
6. **Sessions (core part):** restore the picture, "A master", routing, step back, need-to-know and live directors bullets verbatim (they stay in core;
   the sub-session table, notes, names and handover prompt stay in `docs/loop-sessions.md`). Version line: keep v5.3.0 with the old bump-rule paragraph.
7. **Triggers in practice:** restore the old section verbatim.
8. `docs/coding.md`: reduce to two lines (remove the duplicate third line).
Do not touch other files unless a reference breaks. Run `pwsh -NoProfile -File tools/loop-lint.ps1` and
`pwsh -NoProfile -File tools/gate.ps1 -Task L6-lv4-docs-diet-r2 -Offline`.

Report `.coord/reports/L6-lv4-docs-diet-r2.md`: Summary; Change list; Behavior changes (None); the new byte sizes of `docs/loop.md`; Checks run.
