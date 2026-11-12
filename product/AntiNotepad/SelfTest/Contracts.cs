// Regression promises and their focused selftest or manual checks.
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
                "sliding-overlay-top-band",
                "Sliding bar modes reserve the top band above overlays and keep it a window-drag surface.",
                "A5-search-bar-overlap",
                () => RunExistingCheck(SlidingOverlayTopBandChecks, "sliding overlay top band: "),
                "Dev copy: Search and Help clear the bar in all modes and scales; drag the band with the bar hidden; save-prompt outside clicks still dismiss."),
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
                "split-pane-clipping",
                "In split view each pane is clipped at its own column edge by a stationary host; the right host hides in single view.",
                "A55-split-view-clip",
                () => RunExistingCheck(SplitPaneClippingChecks, "split pane clipping: "),
                "Dev copy: repeat sideways swipes toward the divider; no text, caret, selection, or decoration pixels cross it."),
            new(
                "shortened-link-opens",
                "Any URL opens on Ctrl+left-click once; plain text shows Hand only with Control, shortened glyphs always show Hand, and a plain click places the caret.",
                "A54-link-hover-click",
                () => RunExistingCheck(ShortenedLinkChecks, "shortened link: "),
                "In the dev copy, test short, shortened, caret-line and Shorten links off URLs: Ctrl+left opens once, plain click places the caret; plain text shows Hand only with Control, shortened glyphs always show Hand. Check wrapped/scrolled links and unchanged checkboxes/result chips."),
            new(
                "checkbox-indent-preserved",
                "Checkbox toggle and Tab/Shift+Tab indentation apply the same text edit and one Undo restores it; checkbox regex groups remain stable.",
                "A42c-checkbox-indent",
                () => RunExistingCheck(CheckboxIndentChecks, "checkbox indent: "),
                "Dev copy: click and Ctrl+Enter on [ ]task, [X] task, - [ ] task and - task; confirm text, checkbox, strikethrough and one-step Undo. Click a box away from the caret and confirm caret/selection stay put. In a timer note, Ctrl+Enter still toggles the timer. Tab/Shift+Tab a bullet, checkbox and nested numbered list; confirm two-space steps, fresh nested numbering at 1, 9 -> 10, and one Undo for indent plus renumber. Tab on plain or selected text still uses the editor behavior."),
            new(
                "note-switch-settles",
                "A note switch never leaves its pane or content transformed or faded once settled; interrupting a departure does not lose or repeat navigation.",
                "A6-note-switch-instant-swap",
                () => RunExistingCheck(NoteSwitchChecks, "note switch: "),
                "Touchpad and Shift+wheel both ways, fast repeats, split slots, stack return and Settings preview; verify departure, arrival, clipping and final opacity. Offline checks cover no-frame completion and seeded departure interruption; displayed frames and captured pixels require hand testing."),
            new(
                "list-commands-preserved",
                "Enter continuation and Tab/Shift+Tab renumber preserve the same text and one Undo step; caret position and scrolling remain a dev-copy check.",
                "A42d-list-commands",
                () => RunExistingCheck(ListCommandsChecks, "list commands: "),
                "Dev copy: Enter at the end and in the middle of - ab, 9. ab, 9) ab and [x] ab; confirm the same markers, trailing text movement and caret after the new marker. Enter on an empty nested item outdents; Enter on an empty root item exits the list. Tab/Shift+Tab a numbered list across 9 -> 10 and confirm one Ctrl+Z per Enter or Tab. Check Shift+Enter, Enter with a selection and completion open, caret visibility at the bottom of a long note, and typing in a long list. In a timer note, Enter after 10s test still starts the timer and Ctrl+Enter still toggles it."),
            // Keep last: this check initializes shared Avalonia state and constructs a PadWindow.
            new(
                "pinned-accent-ring",
                "Pinned window shows the accent ring; pin changes leave chrome visibility unchanged.",
                "A9-pinned-accent-ring",
                () => RunExistingCheck(PinRingChecks, "pinned accent ring: "),
                "Dev copy: with Fade chrome on, pin button and Ctrl+T leave toolbar and pager visible while the ring fades; typing still fades the bars. Check both themes, custom accents, split view, rapid toggles, Reduce motion, dragging, resizing and top-bar clicks.")
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
