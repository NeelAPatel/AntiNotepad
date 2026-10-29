namespace AntiNotepad;

internal static partial class SelfTest
{
    private sealed record ContractResult(bool Passed, string Detail);

    private sealed record ContractRow(
        string Id,
        string Promise,
        string Incident,
        Func<ContractResult>? Check,
        string? Manual);

    private delegate bool ExistingCheck(out string report);

    private static void ContractsChecks(List<string> failures)
    {
        var saveAndLog = Memoize(() => RunExistingCheck(BackgroundSaveAndLogChecks, "background save and log: "));
        ContractRow[] rows =
        [
            new(
                "dev-badge-build-stamp",
                "A dev build stamped with a branch and build time shows them in the badge (short form, full form on hover and in the tooltip); without the stamp it shows plain dev; release builds carry no stamp.",
                "A45-dev-build-badge",
                () => RunExistingCheck(DevBadgeBuildStampChecks, "dev badge build stamp: "),
                "Dev copy: short badge expands on hover; tooltip shows full branch and timestamp; release has no badge."),
            new(
                "store-unavailable-save-noop",
                "An unavailable store's QueueSave, Save, and Flush succeed as no-ops, set LastError, and write nothing.",
                "A30",
                saveAndLog,
                null),
            new(
                "store-folder-returns",
                "A missing folder that returns is saved to again without the store becoming unavailable.",
                "A30",
                saveAndLog,
                null),
            new(
                "startup-unavailable-store",
                "Startup with a missing notes folder does not throw and keeps a valid selection.",
                "A43b",
                () => RunExistingCheck(UnavailableStoreStartupChecks, "unavailable store startup: "),
                null),
            new(
                "fence-cache-bounds",
                "A fence cache hit requires matching body bounds as well as the document version.",
                "A28",
                () => RunExistingCheck(FenceCacheChecks, "A28-fence-cache: "),
                null),
            new(
                "settings-changed-notifies",
                "Changing a setting raises the Settings.Changed event for its consumers.",
                "A40",
                () => RunExistingCheck(SettingsChangedChecks, "settings Changed: "),
                null),
            new(
                "notes-move-renames-old-folder",
                "Moving notes from a non-app-home folder renames the old folder even with trailing separators; if retention fails, the move succeeds and reports that the old path remains.",
                "A52-move-old-folder-rename-warning",
                () => RunExistingCheck(NotesMoveRenameChecks, "notes move old-folder rename: "),
                null),
            new(
                "window-style-bits",
                "The pad window's style flags are unchanged.",
                "A32/A34",
                null,
                "pad keeps rounded corners and shadow in all three bar styles"),
            new(
                "split-divider-above-pager",
                "The split divider ends just above the pager strip whether or not the pill shows, and the pager layers above it.",
                "A53-split-divider-height",
                () => RunExistingCheck(SplitDividerFullHeightChecks, "split divider full height: "),
                null),
            new(
                "shortened-link-opens",
                "Any URL opens on Ctrl+left-click once; plain text shows Hand only with Control, shortened glyphs always show Hand, and a plain click places the caret.",
                "A54-link-hover-click",
                () => RunExistingCheck(ShortenedLinkChecks, "shortened link: "),
                "In the dev copy, test short, shortened, caret-line and Shorten links off URLs: Ctrl+left opens once, plain click places the caret; plain text shows Hand only with Control, shortened glyphs always show Hand. Check wrapped/scrolled links and unchanged checkboxes/result chips.")
        ];

        var passed = 0;
        var manual = 0;
        foreach (var row in rows)
        {
            if (row.Check is null)
            {
                manual++;
                continue;
            }

            ContractResult result;
            try
            {
                result = row.Check();
            }
            catch (Exception ex)
            {
                result = new ContractResult(false, ex.Message);
            }

            if (result.Passed)
            {
                passed++;
                continue;
            }

            var detail = string.IsNullOrWhiteSpace(result.Detail) ? "check failed" : result.Detail;
            failures.Add($"contract {row.Id}: {detail}");
        }

        Console.WriteLine($"CONTRACTS: {rows.Length} rows, {passed} pass, {manual} manual");
    }

    private static ContractResult RunExistingCheck(ExistingCheck check, string checkName)
    {
        try
        {
            var passed = check(out var report);
            return new ContractResult(passed, checkName + report);
        }
        catch (Exception ex)
        {
            return new ContractResult(false, checkName + ex.Message);
        }
    }

    private static Func<ContractResult> Memoize(Func<ContractResult> check)
    {
        ContractResult? cached = null;
        return () => cached ??= check();
    }
}
