# A2-store-split

## Summary
Split `Model/Store.cs` into the planned model and search files while retaining the `AntiNotepad.Model` namespace. Moved blocks are byte-identical to the original, and behavior is unchanged.

## Change list
- `product/AntiNotepad/Model/Store.cs:7-556` | Retains `StoreData` and the non-search `Store` implementation; updates `Store` to `partial` | Plan: `Model/Store.cs` holds `StoreData` and the rest of `Store`.
- `product/AntiNotepad/Model/Search/SearchTypes.cs:6-123` | Moves search status, snapshot, context, cache, and request types (source lines 6-123) | Plan: `Model/Search/SearchTypes.cs`.
- `product/AntiNotepad/Model/Search/Store.Search.cs:6-128` | Moves the search cache field (source line 367) and Store search members (source lines 395-513) into `partial Store` | Plan: `Model/Search/Store.Search.cs` holds the Store search members.
- `product/AntiNotepad/Model/Note.cs:6-146` | Moves `Note` (source lines 125-265) | Plan: `Model/Note.cs`.
- `product/AntiNotepad/Model/Settings.cs:6-73` | Moves `Settings` (source lines 267-334) | Plan: `Model/Settings.cs`.

## Behavior changes
None.

## Out-of-scope files
None.

## Plan deviations
None.

## Contracts
None.

## Lens review
Ran the nine-item lens checklist against the diff and callers. No proven findings; signatures, shared state behavior, timing, failure paths, layering, contracts, and scope remain unchanged.

## Checks run
- Build: PASS, 0 warnings, 0 errors.
- Selftest: PASS; `CONTRACTS: 13 rows, 12 pass, 1 manual`; `SELFTEST PASS`.
- Verbatim blocks: PASS for all moved blocks, including CRLF line endings.
- Sorted-lines check: PASS, 674 lines before and after after ignoring the approved declarations, using/namespace lines, braces, and blank lines.
- Offline gate: PASS (`tools/gate.ps1 -Task A2-store-split -Offline`); build, selftest, seam, format, scope, and size passed. NuGet audit was disabled for the offline run.

## Hand checks for the user
None (not visible).
