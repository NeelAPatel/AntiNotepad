# A35b-icon-registry: Check and hand test

Item chat, 2026-11-21. Card: `.coord/tasks/A35b-icon-registry.md`.

## Rounds
- Run 1 (terra): stopped without edits; report said "Not implemented".
- r2 (worker): registry, selftest, scanner, faux bold removed. Item chat moved `UI/Icons.cs` to `Icons.cs` (namespace
  matches folder) and replaced a `(char)0xE0A0` scanner dodge with `AppInfo.DevBadgeGlyph`.
- r2 Codex review: VERDICT rework, one finding (lowercase `` / `(char)0xe8b3` passed the scanner). Fixed by the
  item chat with lowercase fixtures; `TOKENS SELFCHECK PASS`.
- r3 (quick): five hand-test icon picks (lab `build/a35b-icon-lab4.html`).

## Gates
- r2: `GATE: PASS` (build 0 warnings, `SELFTEST PASS`, tokens, format, scope 20 files, size).
- r3: `GATE: PASS` (Codex report).
- A29 proof: Keyboard set to `0xF6ED` failed the selftest with "U+F6ED is ic_fluent_person_subtract_16_regular,
  expected ic_fluent_keyboard_20_regular"; reverted.

## Bounds
Within plan plus approved additions: r3's five picks came from the hand test and were approved on the lab. Out-of-plan
files: root `Icons.cs` (moved from `UI/`) and `AppInfo.cs` (badge constant), both listed in the r2 brief. Icon-only
edits in `UI/PadWindow.Layout.cs` and `UI/SearchOverlay.cs`, which sibling A35a also edits (parent told; A35a merges
first). Lessons line dropped by the parent chat: the selftest and scanner encode it.

## Hand test (Red dev copy, verbatim)
- r2, 2026-11-21: "Doesnt quite hit the mark: * Folding * Inconsistent Split view icon * Storage Icon * Slots/Stack.
  Everything else is good enough"; "Trash icon (not delete) can it be recycle bin?"
- r3 picks, 2026-11-21: "recycle arrows makes sense. everything else is good"
- r3 build, 2026-11-21: "All good"

## Noticed
- `Icons.List` meaning still says "stack or list view"; it now serves only the search results toggle. Cosmetic.
- `.coord/lessons.md` (11.7 KB vs 3 KB) and `docs/loop.md` are over budget; not this item's.
