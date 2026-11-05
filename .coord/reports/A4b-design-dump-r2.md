# A4b-design-dump-r2 report

## Summary

Implemented the offline design dump and final selftest check for eight palettes. Build and selftest pass, but the requested offline gate fails its scope check, so the item is not complete.

## Change list

| File and lines | Change | Plan |
|---|---|---|
| `product/AntiNotepad/Program.cs:20` | Route `--design-dump <file>` before normal startup. | Standalone dump. |
| `product/AntiNotepad/SelfTest/Run.cs:144` | Run the design check last. | Determinism and pinned hash. |
| `product/AntiNotepad/SelfTest/Design.cs:1-120` | Build hidden controls with temporary data, capture sorted values, write and compare snapshots. | Dump and selftest. |

## Behavior changes

The new command writes the requested file and returns a failure code on error. Selftest now initializes the app styles, constructs hidden windows, and checks the design hash last; static event subscriptions and windows remain until the short-lived process exits, as authorized in this round. Normal application startup is unchanged.

## Out-of-scope files

None edited by this round beyond this requested report. The gate's scope comparison lists `docs/coding.md`, `docs/loop.md`, `tools/check-design-tokens.ps1`, and `tools/gate.ps1`; those were already present in the branch comparison, not edited here.

## Plan deviations

The gate fails: `.coord/tasks/A4b-design-dump-r2.md` has no backticked paths in its Files section, and the gate lists four inherited paths outside the allowed list. The supplied brief says no additional files and the item card prohibits editing the card; neither was changed here.

The dump traverses the constructed logical trees and records effective values, including defaults. Native system high-contrast mode overrides the selected palette in `PadWindow.ResolvePalette`, so palette coverage on a high-contrast host is not established. It does not exercise unopened expanders or rendered control-template visuals.

## Contracts

None changed.

## Lens review

Worker performed the lens checklist locally. A proven omission in the first version serialized gradient brushes only as their type; fixed by recording their opacity and color stops. The host high-contrast override and logical-tree coverage limits are noted above. Hidden windows and event handlers are intentionally retained per this round's instruction.

## Checks run

- Offline build: 0 warnings, 0 errors. `SELFTEST PASS`.
- `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160`.
- Two standalone dumps have the same SHA-256 as the selftest.
- Temporarily increased the pad's font size by one: selftest failed with actual `87bcf059ef251197709b774bfc5452fe88b120579762326a5fc24a9b29489820` and expected `8d3df36e1b5b818be3bcb7f7761c3b7ea80abd91f35a13eb062dbd34425ae7e6`; reverted before the final gradient serialization and hash update.
- Offline gate: build, selftest, seam, tokens, format, loop lint and size pass; scope fails as described above; `GATE: FAIL`.
- Cleanup of `build/A4b-design-dump-check` and the two temporary dump files was attempted, but the command was rejected by the execution policy; they remain in `build/`.

## Hand checks for the user

Run `--selftest` in the dev build and confirm no window flashes up.
