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
        var saveDelayUi = Memoize(() => RunExistingCheck(SaveDelayChecks, "save delay UI: "));
        var savedLabelClock = Memoize(() => RunExistingCheck(SavedLabelChecks, "saved label clock: "));
        var barInteraction = Memoize(() => RunExistingCheck(PinRingChecks, "bar interaction: "));
        ContractRow[] rows =
        [
            new(
                "recovered-folder-ensure",
                "Recovery storage is created idempotently only under an existing notes folder.",
                "A27-recovered-folder-always-open",
                () => RunExistingCheck(NotesLocationChecks, "notes location: "),
                "Dev copy: Settings Storage shows Open beside Empty and opens the recovered folder."),
            new(
                "installed-instance-id-stable",
                "The installed instance remains AntiNotepad even when ANTINOTEPAD_SLOT is set without custom dev data.",
                "T17-multi-dev-instances",
                () => RunExistingCheck(InstalledInstanceIdentityChecks, "installed instance identity: "),
                null),
            new(
                "dev-no-slot-keeps-legacy-id",
                "Custom data without a valid slot keeps the existing AntiNotepad-dev instance identity.",
                "T17-multi-dev-instances",
                () => RunExistingCheck(LegacyDevInstanceIdentityChecks, "legacy dev instance identity: "),
                null),
            new(
                "paste-list-structure",
                "Structured HTML lists upgrade plain clipboard text without changing already structured text.",
                "A23-paste-keep-structure",
                () => RunExistingCheck(PasteChecks, "paste: "),
                "Dev copy: paste three-level browser and AI lists, markdown, plain text and editor code; check one Undo and paste in checklist and numbered notes."),
            new(
                "hotkey-recorder-escape-routing",
                "Settings defers Escape only for event sources inside an active hotkey recorder; idle and outside sources keep the existing Settings command.",
                "A10-hotkey-recorder",
                () => RunExistingCheck(HotkeyRecorderEscapeChecks, "hotkey recorder Escape: "),
                "Dev copy: Esc cancels recording without clearing search or closing Settings; Ctrl+Esc records; outside recording Esc clears search, then closes Settings."),
            new(
                "hotkey-recorder-round-trip",
                "Every supported main key recorded with Ctrl, Alt or Shift round-trips through the hotkey parser; Windows-key combinations, modifier-only and unsupported keys record nothing, and unmodified Esc and Enter are commands.",
                "A10-hotkey-recorder",
                () => RunExistingCheck(HotkeyRecorderChecks, "hotkey recorder: "),
                "Dev copy: both rows capture, retain on release, Save, Cancel, focus-loss cancel, unsupported-key hint, refusal and optional Clear in light and dark."),
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
                "save-delay-settings-persist",
                "Fresh and invalid settings use 400 ms; all four delays survive a store restart; saved labels come from durable body times and stale or failed writes keep the prior time.",
                "A21-save-delay-setting",
                () => RunExistingCheck(SaveDelayStorageChecks, "save delay storage: "),
                null),
            new(
                "save-delay-state-machine",
                "The 400 ms default keeps its trailing-edge debounce; longer delays wait for a one-second idle gate, enforce per-note safety deadlines, and retry on a steady clock.",
                "A21-save-delay-setting-r3",
                () => RunExistingCheck(SaveTimingChecks, "save timing state: "),
                null),
            new(
                "saved-label-age-and-clock",
                "The saved label distinguishes never-saved notes, uses the requested age table and never goes negative; its clock aligns to saved-time seconds and runs whenever the pad is visible and not minimized, independent of bar visibility.",
                "A21-save-delay-setting-r3",
                savedLabelClock,
                null),
            new(
                "saved-label-future-clock-cadence",
                "A future durable timestamp refreshes the just-now label every second so a backward clock change cannot suspend the label clock.",
                "A21-save-delay-setting-r5",
                savedLabelClock,
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
                "save-delay-empty-first-write",
                "Clearing a new note during its first queued or stale body write keeps the empty revision dirty until it reaches disk.",
                "A21-save-delay-setting-r5",
                saveDelayUi,
                null),
            new(
                "save-delay-split-active-switch",
                "Switching active notes by Ctrl+slot or pane focus saves the departing split note immediately.",
                "A21-save-delay-setting-r5",
                saveDelayUi,
                null),
            new(
                "save-delay-timer-and-boundaries",
                "Per-note deadlines save all dirty notes without exceeding their delay; pending settings changes save immediately; save completion changes only label text; note transitions, hide, minimize and quit save immediately.",
                "A21-save-delay-setting-r3",
                saveDelayUi,
                "Dev copy: new blank note, type at 5 s, nonstop typing, split notes, switch/hide/minimize, then check that a save leaves both bars still. Keep sign-out and Windows shutdown as the open A30 live check."),
            new(
                "wrap-indent-text-column",
                "Wrapped continuation lines begin at the measured first text column for list markers and indented text; unindented text, document contents, caret, selection, and hit-testing stay aligned.",
                "A22-wrap-hanging-indent",
                () => RunExistingCheck(WrapIndentChecks, "wrap indent: "),
                "Dev copy: check nested levels 1–3, top-level bullets, numbered items, checkboxes, plain indented and unindented lines at several widths and fonts; confirm the supplied example, caret movement, selection, and click-to-caret behavior."),
            new(
                "wrapped-result-chip-text-end",
                "Result chips use the indented visual end of a wrapped line.",
                "A22-wrap-hanging-indent-r2",
                () => RunExistingCheck(WrapIndentChecks, "wrap indent: "),
                "Dev copy: check result chips on wrapped indented lines do not overlap text."),
            new(
                "avaloniaedit-fork-documented",
                "PATCHES.md lists every AntiNotepad patch A22 source marker and the pinned AvaloniaEdit version; the app references the vendored project.",
                "A22-wrap-hanging-indent",
                () => RunExistingCheck(AvaloniaEditForkChecks, "AvaloniaEdit fork: "),
                null),
            new(
                "list-default-kind",
                "The list keyword follows ListDefault; todo always creates checkboxes.",
                "A31-list-default-type",
                () => RunExistingCheck(ListEditingChecks, "list defaults: "),
                "Dev copy: change New list lines, try list and each direct keyword, and check the popup default text."),
            new(
                "list-tab-text-start",
                "Tab changes list level only at text start; Ctrl+[ / Ctrl+] changes list levels from anywhere, including selected list lines.",
                "A26-tab-at-text-start",
                () => RunExistingCheck(ListEditingChecks, "list Tab: "),
                "Dev copy: check Tab at the marker, text start and mid-text; Shift+Tab, Ctrl+Tab and Ctrl+[ / Ctrl+] on bullets, checkboxes, numbers and selected lines."),
            new(
                "checkbox-indent-preserved",
                "Checkbox toggles and Tab/Shift+Tab indentation apply the same text edit and one Undo restores it; checkbox regex groups remain stable.",
                "A42c-checkbox-indent",
                () => RunExistingCheck(CheckboxIndentChecks, "checkbox indent: "),
                "Dev copy: click [ ]task, [X] task, - [ ] task and - task; confirm text, checkbox, strikethrough and one-step Undo. Click a box away from the caret and confirm caret/selection stay put. Ctrl+Enter has no default action; assign it to Checkbox or timer in Settings to check that binding, then clear it. Tab/Shift+Tab a bullet, checkbox and nested numbered list; confirm two-space steps, fresh nested numbering at 1, 9 -> 10, and one Undo for indent plus renumber. Tab on plain or selected text still uses the editor behavior."),
            new(
                "note-switch-settles",
                "A note switch never leaves its pane or content transformed or faded once settled; interrupting a departure does not lose or repeat navigation.",
                "A6-note-switch-instant-swap",
                () => RunExistingCheck(NoteSwitchChecks, "note switch: "),
                "Touchpad and Shift+wheel both ways, fast repeats, split slots, stack return and Settings preview; verify departure, arrival, clipping and final opacity. Offline checks cover no-frame completion and seeded departure interruption; displayed frames and captured pixels require hand testing."),
            new(
                "horizontal-edge-pull-no-jump",
                "A horizontal edge pull never jumps in one frame; projected touchpad releases commit only when the projection reaches half a page.",
                "A20-overscroll-speed",
                () => RunExistingCheck(HorizontalScrollFeelChecks, "horizontal scroll feel: "),
                "Dev copy: at the first and last notes, pull with touchpad and wheel; the edge pull glides in, holds, and springs home without a snap."),
            new(
                "list-commands-preserved",
                "Enter continuation and Tab/Shift+Tab renumber preserve the same text and one Undo step; caret position and scrolling remain a dev-copy check.",
                "A42d-list-commands",
                () => RunExistingCheck(ListCommandsChecks, "list commands: "),
                "Dev copy: Enter at the end and in the middle of - ab, 9. ab, 9) ab and [x] ab; confirm the same markers, trailing text movement and caret after the new marker. Enter on an empty nested item outdents; Enter on an empty root item exits the list. Tab/Shift+Tab a numbered list across 9 -> 10 and confirm one Ctrl+Z per Enter or Tab. Check Shift+Enter, Enter with a selection and completion open, caret visibility at the bottom of a long note, and typing in a long list. In a timer note, Enter after the 10s test still starts the timer; Ctrl+Enter is unassigned by default."),
            new(
                "convert-list-undo-blanks",
                "Convert to list skips blank lines and is one undo step.",
                "A32-selection-convert-menu",
                () => RunExistingCheck(ListCommandsChecks, "convert to list: "),
                "Dev copy: select mixed lines with a blank, convert to each list kind and undo once; check submenu pointer and keyboard navigation."),
            // Keep these final: both checks initialize shared Avalonia state and construct a PadWindow.
            new("icon-disabled-fade", "Icon buttons settle at the disabled opacity and restore full opacity.",
                "A34-pill-bar-per-mode-r10", barInteraction, "Dev copy: enabled state fades in both directions."),
            new("chrome-pointer-hold", "Docked fading chrome stays visible under the bar or top row, including after swap.",
                "A34-pill-bar-per-mode-r10", barInteraction, "Dev copy: click swap while hovering and move away."),
            new("slot-navigation-cycle", "Navigation cycles slots, wraps and skips the other pane; stack and swap remain available.",
                "A34-pill-bar-per-mode-r10", barInteraction, "Dev copy: wheel, swipe and shortcuts cycle slots."),
            new("stack-bar-lazy-slot-titles", "Stack bar snapshots and rebuilds do not resolve slot titles; slot tooltips show titles for filled slots.",
                "A34-pill-bar-per-mode-r12", () => RunExistingCheck(BarChecks, "bar: "), null),
            new(
                "pinned-accent-ring",
                "Pinned window shows the accent ring; pin changes leave chrome visibility unchanged.",
                "A9-pinned-accent-ring",
                () => RunExistingCheck(PinRingChecks, "pinned accent ring: "),
                "Dev copy: with Fade chrome on, pin button and Ctrl+T leave toolbar and pager visible while the ring fades; typing still fades the bars. Check both themes, custom accents, split view, rapid toggles, Reduce motion, dragging, resizing and top-bar clicks."),
            new(
                "shortcut-map",
                "One key resolves to one action per scope; defaults avoid text editing, Windows and development-slot keys, Help toggles through its active shortcut, and the checkbox/timer action stays bindable without a default.",
                "A29-shortcut-map",
                () => RunExistingCheck(ShortcutMapChecks, "shortcut map: "),
                "Dev copy: record, clear, reset one row, reset all, restart, take a conflicting key, and confirm F1 opens, closes and reopens Help with current bindings. Confirm Ctrl+Enter is unassigned by default and Checkbox or timer can be bound in Settings.")
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

    /// <summary>Checks the vendored source markers and documented upstream pin.</summary>
    private static bool AvaloniaEditForkChecks(out string report)
    {
        const string expectedVersion = "12.0.0";
        const string markerPrefix = "// AntiNotepad patch A22:";

        string? FindRoot(string start)
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                var sourceRoot = Path.Combine(directory.FullName, "third_party", "AvaloniaEdit");
                if (File.Exists(Path.Combine(sourceRoot, "UPSTREAM.txt")) && File.Exists(Path.Combine(sourceRoot, "PATCHES.md")))
                    return directory.FullName;
                directory = directory.Parent;
            }
            return null;
        }

        var root = FindRoot(AppContext.BaseDirectory) ?? FindRoot(Environment.CurrentDirectory);
        if (root is null)
        {
            report = "repository source files were not found";
            return false;
        }

        var sourceRoot = Path.Combine(root, "third_party", "AvaloniaEdit");
        var sourceMarkers = Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !Path.GetRelativePath(sourceRoot, path)
                .Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries)
                .Any(part => part is "bin" or "obj"))
            .SelectMany(File.ReadLines)
            .Select(line => line.Trim())
            .Where(line => line.StartsWith(markerPrefix, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var patchDoc = File.ReadAllText(Path.Combine(sourceRoot, "PATCHES.md"));
        var documentedMarkers = patchDoc.Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.StartsWith(markerPrefix, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var upstream = File.ReadAllText(Path.Combine(sourceRoot, "UPSTREAM.txt"));
        var upstreamVersion = System.Text.RegularExpressions.Regex.Match(upstream, @"AvaloniaEdit ([0-9]+\.[0-9]+\.[0-9]+),").Groups[1].Value;
        var appProject = File.ReadAllText(Path.Combine(root, "product", "AntiNotepad", "AntiNotepad.csproj"));
        var markerPass = sourceMarkers.Length > 0 && sourceMarkers.SequenceEqual(documentedMarkers, StringComparer.Ordinal);
        var versionPass = upstreamVersion == expectedVersion && patchDoc.Contains($"Upstream version: {expectedVersion}", StringComparison.Ordinal);
        var referencePass = appProject.Contains("../../third_party/AvaloniaEdit/AvaloniaEdit.csproj", StringComparison.Ordinal)
            && !appProject.Contains("PackageReference Include=\"Avalonia.AvaloniaEdit\"", StringComparison.Ordinal);
        var licensePass = appProject.Contains("AvaloniaEdit-LICENSE.txt", StringComparison.Ordinal)
            && File.Exists(Path.Combine(sourceRoot, "LICENSE"));
        report = $"markers={sourceMarkers.Length}/{documentedMarkers.Length}; pin={upstreamVersion}; reference={referencePass}; license={licensePass}";
        return markerPass && versionPass && referencePass && licensePass;
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
