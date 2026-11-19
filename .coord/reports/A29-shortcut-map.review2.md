# A29-shortcut-map-r3-review lens review

Reviewed `973f281..8be7747 -- product docs`, the card, first review, r2/r3 briefs and reports, supplied PASS gate, and affected callers. Product code remained read-only.

## Findings (proven)

1. **[major] product/AntiNotepad/UI/Shortcuts.cs:122 — Malformed paired overrides still remove Search.** With `new-note=Ctrl+F`, `search=garbage`, the valid candidate takes Ctrl+F before fallback; Search's fallback fails `HasConflict` at line 131. Independent compiled probe: `new-note=[Ctrl+F]; search=[]; Ctrl+N=False; Ctrl+F=True ShortcutRoute { Pad = NewNote, Search = None }`. This reproduces the malformed half of first-review finding 2. `SelfTest/UI.cs:238` now explicitly expects Search to be unassigned, so SELFTEST PASS does not prove this review's requirement that paired fallback keeps Search and New note. The r2 brief/report explicitly permit this exception; it nevertheless fails the current review request's stronger acceptance condition. Smallest fix: resolve fallback claims together with candidates, rejecting an override that steals a required fallback, while retaining valid swaps and explicit clears; change the paired-malformed check to require both default routes.

## Behavior changes check

Reports match the diff, including the disclosed fallback exception above. No further default keys changed.

- First-review findings 1, 3, 4 and 5 are fixed: Shift+Backspace and other checked editor double bindings are refused; the slot toast reads the effective Open slot label and omits an empty hint; the dispatch interception hook and its assigning caller are gone; all eleven requested method summaries are present.
- Finding 2 is partly fixed: duplicate Ctrl+F overrides restore New note Ctrl+N and Search Ctrl+F. Malformed paired recovery remains as finding 1 above. Selftest also covers swaps and explicit clears.
- Search and both aliases call `ToggleSearch`. Routed key events from the Search query control prove close/reopen routing; Esc and actual toolbar Click events still close/toggle. `SearchOverlay.cs` has no rework diff, so its navigation, selection and deletion key rules are unchanged.

## Scope and plan check

Six changed product/docs files serve the rework. No unrelated hunks. New helper `SlotPromotionMessage` has only the promotion caller and selftests; its input is the live map label. No new persistent state, deferred work, subscriptions, dependencies or save/shutdown changes. New allocations are confined to map refresh; the typing lookup and its callers retain exact value-type dictionary routing. The fallback exception is disclosed but conflicts with this review request.

## Unproven risks

- Native focus and physical keyboard delivery remain hand checks: all probe windows stayed unshown. The existing selftest invokes the handler directly; this review additionally raised routed events from the query control after applying templates/layout.

## Checks run

- Offline build with empty package sources, audit/telemetry disabled, all outputs/intermediates under `build/A29-shortcut-map-review2`: **0 warnings, 0 errors; 8.96 s**.
- Isolated DLL `--selftest`, DATA/TEMP/TMP inside that folder: **CONTRACTS: 36 rows, 35 pass, 1 manual; SELFTEST PASS**. Includes no-allocation lookup, slot messages, persistence and Help/docs parity.
- Independent compiled probes: editing reservations **6/6**; dev-slot chords **18/18**; extra Shift/Alt/Meta on Ctrl+F **3/3 unbound**; duplicate recovery passed; malformed recovery reproduced the finding.
- Routed Ctrl+F, bound Ctrl+P and Ctrl+K: each **open/close/reopen=True/False/True**, all nine events handled; close events originated in Search's query control. **Esc closed=True; toolbar Click open=True, close=True**.
- Initial probe lacked applied window templates and could not route events; after applying templates/layout, the completed probe exited 0. No product changes were needed.
- `git diff --check` passed; supplied gate read, not rerun. Every build/selftest/probe process had a 180-second bound. No resident app launched or killed; no git metadata edited.
- Cleanup incomplete: command policy rejected both the guarded cleanup and explicit absolute-path PowerShell `Remove-Item -Recurse -Force`. `build/A29-shortcut-map-review2` remains; no approval/escalation is available in this session.

VERDICT: rework
