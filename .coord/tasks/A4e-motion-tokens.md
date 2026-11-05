# A4e-motion-tokens: animation durations come from one token class, timing identical

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Low · timing-identical (pure literal swaps; behavior timers stay out)
Branch: `refactor/A4e-motion-tokens` in `.claude/worktrees/refactor/A4e-motion-tokens`, cut from the umbrella
`feat/A4-design-tokens` (not dev) and squashed back into it (created at Approve)

## Request
2026-11-02, user: "okay lets collect these ... yes start motion tokens" (quoting the motion-tokens row: animation durations
and ease, 13 animations, timing identical). Scope as on the STATUS row `step: motion-tokens`, tweens only (behavior timers
are `step: timer-names`, deferred).

## Findings
- **Kind and why:** refactor; each animation spells its own duration in milliseconds at its `Anim.Run` call.
- **Now:** token scan on the umbrella (fae0dcc): tween 13 (PadView 1, NoteSwipe 1, PadWindow 3, PadWindow.Keys 1,
  PadWindow.Layout 6, SettingsWindow 1). Plus the caret glide's 80 ms in `Editor/Motion.cs` (`Current()`, a tween the scan
  does not see because it is not an `Anim.Run`).
  - Sites: note arrive 190 (`Editor/PadView.cs:445`), swipe spring-back 200 (`UI/NoteSwipe.cs:196`), window slide 150
    (`UI/PadWindow.cs:177`), content reveal 140 (`UI/PadWindow.cs:315`), window fade 160 (`UI/PadWindow.cs:327`), overlay
    fade-in 130 (`UI/PadWindow.Keys.cs:128`), bars reveal 170 (`UI/PadWindow.Layout.cs:399`), pill fade 150 (`:408`), chrome
    shift 180 (`:437`), toast in 120 (`:463`), toast out 160 (`:472`), nudge 180 (`:569`), settings page 150
    (`UI/SettingsWindow.cs:263`), caret glide 80 (`Editor/Motion.cs`, `Current()`).
  - Easing: one cubic ease-out, `Anim.EaseOut` in `Motion/Anim.cs`, already a single source; it stays.
- **Where:** `Motion/` is where `docs/coding.md` puts motion tokens. Out of scope: DispatcherTimer intervals (save, debounce,
  toast hold, caret blink, bar sleep: `step: timer-names`), scroll physics in `Editor/Motion.cs`, motion distances (6, 7,
  10 px: spacing, ratchet only), `NoteSwipe` gesture timings.
- **Risk:** a swap that changes a duration. The design dump does not cover timing; the proof is the review's site-by-site
  map plus the Tokens count reaching 0, and a hand look.
- **Source:** master scout.

## Plan
- **After:**
  - New `Motion/MotionTokens.cs`, a static class of `double` consts in milliseconds, one per animation role at today's
    value: `ToastIn` 120, `OverlayIn` 130, `ContentReveal` 140, `WindowSlide` 150, `PillFade` 150, `SettingsPage` 150,
    `WindowFade` 160, `ToastOut` 160, `BarsReveal` 170, `ChromeShift` 180, `Nudge` 180, `NoteArrive` 190,
    `SwipeSpringBack` 200, `CaretGlide` 80. A one-line `///` per group (pad window, chrome, toast, editor, settings).
    Roles that share a value stay separate names on purpose (grouping is `step: control-groups`).
  - Each of the 14 sites uses its token. `Anim.EaseOut` is untouched.
  - `tools/design-tokens-baseline.tsv` refreshed with `-Update`: tween reaches 0 (or the report names each one left and why).
- **Expected time:** about 40 minutes, request to umbrella merge (A4d: two rounds and a hand look; this is smaller).
- **Must not change:** any duration, easing, distance, timer interval, Reduce Motion behavior; files outside the list.
- **Scope questions:** None.
- **Test plan, code:** `--selftest` passes with the DESIGN hash unchanged
  (`8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b`, 16160 lines); Tokens tween 13 → 0. Review: every
  removed number maps to a token of the same value.
- **Test plan, UI:** dev build, about one minute: summon and hide the pad (slide and fade), switch notes (arrive), swipe
  and let go (spring-back), move the pointer to the top and bottom edge (bars and pill), trigger a toast, open help (overlay),
  switch Settings pages, move the caret with glide on. Nothing feels different.
- **Gates:** tier defaults: Build, Selftest (DESIGN hash), Format, Seam, Tokens (tween falls, none rise), Scope, Layers
  (`Motion/` must not use UI or Editor).
- **Needs from you:** approve; the one-minute hand look.

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Any other file needs a line under
"Out-of-scope files" in the report.
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/Editor/PadView.cs`
- `product/AntiNotepad/Editor/Motion.cs`
- `product/AntiNotepad/UI/NoteSwipe.cs`
- `product/AntiNotepad/UI/PadWindow.cs`
- `product/AntiNotepad/UI/PadWindow.Keys.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `tools/design-tokens-baseline.tsv`

## Contracts
- None changed. The DESIGN selftest hash must stay as above.

## Approve
2026-11-02, user: "Approve"

## For Codex
Role: terra (`docs/agents.md`), because it is a careful value-preserving swap across 7 files.
Product `product/AntiNotepad`. The branch is already checked out: never switch, merge or commit (you cannot write to
`.git`). Read `AGENTS.md`, `docs/coding.md`, then the files above. Line numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above. No drive-by refactors; no timer, distance or easing edits.
- Every swap keeps the exact value. Never change the DESIGN hash constant in `SelfTest/Design.cs`.
- Keep CRLF line endings. Lines you touch must pass the gate's Format step (one initializer entry per line where the
  formatter asks for it).
- Get the inventory with `pwsh -NoProfile -File tools/check-design-tokens.ps1 -Inventory <path>`; refresh the baseline
  with `-Update` only after the swaps.
- Build into `build/A4e-motion-tokens-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A4e-motion-tokens -Offline -Base feat/A4-design-tokens`; the
  manager re-runs it online.
- Never launch or kill AntiNotepad except `--selftest` from your check build. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means (offline checks only: build, selftest, gate, script checks, files; never ports, servers, browser or screen):
- `Motion/MotionTokens.cs` exists as in "After"; no tween duration literal remains (or the report lists each one left and why).
- `--selftest` prints `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160` and `SELFTEST PASS`.
- Build 0 warnings, the gate passes.

Before you report (director only): spawn the `lens_reviewer` agent on your diff (it reads
`.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why not. Workers without spawning: run the same
checklist yourself and say so.

Report: `.coord/reports/A4e-motion-tokens.md`, short, with exactly these sections:
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
- Gate (`-Offline -Base feat/A4-design-tokens`): PASS (Build 0 warnings; online run shows only the two expected NU1900). Selftest PASS, Seam, Tokens, Format, Scope, Size, Cleanup PASS.
- Site map: all 14 literals (190, 200, 130, 170, 150, 180, 120, 160, 180, 150, 140, 160, 150, 80) swapped to a token of the same value; `MotionTokens.cs` values match the Plan.
- Unchanged: `Anim.EaseOut`, timer intervals, distances (6, 7, 10), Reduce Motion paths (diff touches only the duration argument). `SelfTest/Design.cs` untouched. Tokens tween 13 -> 0. CRLF kept in all changed files.
- Report: Behavior changes None, Out-of-scope None, Deviations None: within plan.
- Misses: none. Noticed: none.

## Hand test
2026-11-02, user: "okay lets put this issue as a potential bug on the roadmap everything else seems the same as before". Issue reported: when switching notes by scroll/swipe, the next page shows up instantly during part of the change. Not caused by this item (the diff swaps literals only; `Fire()` swaps the text at once, then the tokenised 190 ms arrive plays). Proposed as an `idea:` row for the master (not an ID from this session). Result: pass.
