# A18-search-speed

Implemented both findings. Final Release build: **0 warnings, 0 errors**. Final executable selftest: **SELFTEST PASS** (exit 0).

## Split

- Director: integration, review, the single new `SelfTest.cs` method and call, timing fixture, Release build/selftest, and this report.
- `a18_note_search` (`gpt-6-luna`, xhigh): `UI/SearchOverlay.cs` and the search/peek path in `Model/Store.cs`.
- `a18_settings_search` (`gpt-6-luna`, xhigh): `UI/SettingsWindow.cs` and `UI/SettingsParts.cs`.

All workers share this worktree and have disjoint file ownership. No branch changes, commits, installed app access, or normal app launches are authorized. `.coord/STATUS.md` and the audit document are untouched per the task brief.

## Files changed

- `product/AntiNotepad/UI/SearchOverlay.cs`
- `product/AntiNotepad/Model/Store.cs` (search snapshot/cache helpers, nonserialized edit revision and search-cache invalidation hooks; no Settings defaults, storage format, or save serialization changes)
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/UI/SettingsParts.cs`
- `product/AntiNotepad/SelfTest.cs` (one new method at the end and one call in Run)
- This report.

## Design as built

Note search debounces typed queries for 125 ms. The UI thread captures the ordered ids, cached titles, modified stamps, edit revisions, and text already in memory, including live editor text. Snapshotting never calls the disk-loading Text/Title/PeekText getters. Background matching receives the captured body-folder path and snapshot, reads each unloaded body at most once per query, and uses that same string for all terms, mode detection and snippets. It does not access mutable Note/Store state or update LastError. The search-only cache checks modified stamp plus edit revision; edits and saves invalidate entries, while store reload/folder changes advance a cache epoch. Only successful reads are cached. Missing/unreadable bodies retain their status and unavailable snippet, can match their cached title, and never become empty-body cache entries or dirty notes through search.

Each new query cancels the previous request. A generation guard is checked again on the UI dispatcher before publishing, including after close. Initial search is deferred until the overlay is active. Result order and text/highlight rules are preserved. Selection and cursor movement update existing card styling without rerunning search or reconstructing rows. Applying a new query's result set still rebuilds its cards. All stores use the 125 ms typing debounce; an instant small-store first-character fast path was optional and is not implemented.

Settings search uses a metadata catalog with deferred row factories. It evaluates current title/description text and row visibility first, applies the previous trimmed ordinal-insensitive substring match and `sub ` normalization, then invokes only matching factories. It never calls BuildSection for search. Section ordering, conditional rows, dynamic descriptions, control values and actions were compared against the existing page builders. Metadata and ordinary page builders are separate definitions and must be updated together when settings rows change. Dynamic recovered-folder text still requires a file-count lookup to preserve existing search semantics; the count is reused within the query.

## Checks and timing

The single new SearchSpeedChecks method uses a disposable temp directory with 200 notes (body padding of 128, 4,096 or 32,768 characters), reloads them, and loads exactly half. A two-term query matches all notes. The before measurement executes the old `terms.All(PeekText.Contains)` filter; the after measurement executes snapshot plus context preparation synchronously, then measures matching separately on Task.Run. Both phases use the same fixture. These are wall-clock fixture measurements with warm filesystem caches, not a GUI frame/typing benchmark; debounce delay and result-control rendering are excluded.

Final run: old UI filter **31.325 ms**, new snapshot/context **0.648 ms** with **0 disk reads**; cold search-cache background query **24.086 ms**, **100 disk reads**. The earlier integration run measured 29.609 / 0.635 / 21.303 ms respectively, illustrating normal fixture timing variation.

Selftest capabilities:

- Loaded and unloaded bodies both match; all 200 results are returned and the 100 unloaded bodies are each read once for a multi-term query including snippets.
- A subsequent query reuses unchanged body cache entries.
- An in-memory edit and subsequent save/detach replace old cached content.
- Missing and exclusively locked body files remain Missing/Unreadable, can match on title, stay clean and are not overwritten/recreated by Save; unavailable reads are retried rather than cached as empty.
- A superseded request observes cancellation; the shared publication guard rejects its completed results after a newer query, and closing invalidates pending publication.
- Settings `font` (including trim/case variation) returns Font and Font size in order; the production matching helper invokes matching probe factories once each and never invokes a nonmatching Theme factory. This is a headless metadata/factory capability check, not rendered UI validation.

Final gate commands: `AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a18-check` and `build/a18-check/AntiNotepad.exe --selftest`. The build completed with 0 warnings and 0 errors; selftest printed SELFTEST PASS and exited 0. No normal app launch or installed/real-data access occurred. No git writes occurred. `git diff --check` passed.

Cleanup not done: automatic command review rejected recursive deletion of `build/a18-check` with the reason `blocked by policy`, including a literal-path retry after independently verifying its resolved absolute path was this worktree's task output. The build output remains for the manager to remove.

## Live verification

Needs a live check: Ctrl+F typing responsiveness, result highlighting, keyboard navigation, selection, trash actions, grid/list toggle, and empty states. Settings result appearance and interactions also need a live check. The manager cannot perform live UI checks this round; headless checks will be reported separately.

Needs a live check: fast Enter/Delete while a replacement query is pending. Those actions are ignored until the current results arrive, to avoid acting on a previous query's rows. Also check closing/reopening during a search, initial empty-query rendering, and drag reorder. No live UI checks were performed or claimed. Full query-result card reuse is not implemented; cursor/selection row reuse is implemented.
