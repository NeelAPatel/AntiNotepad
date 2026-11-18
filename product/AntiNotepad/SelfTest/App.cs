// Capability checks for runtime identity, startup and the dev badge.
using AntiNotepad.Model;
using AntiNotepad.Platform;
using Avalonia.Platform;
using System.Reflection;

namespace AntiNotepad;

internal static partial class SelfTest
{
    private static bool RuntimeLogChecks(out string report)
    {
        var failures = new List<string>();
        void CheckCase(bool passed, string name) { if (!passed) failures.Add(name); }

        var root = Path.Combine(Path.GetTempPath(), $"{AppInfo.Name.ToLowerInvariant()}-runtime-log-{Guid.NewGuid():N}");
        try
        {
            var emptyHome = new RuntimeLog();
            emptyHome.Write("ignored before home");
            emptyHome.Flush();
            var loggerField = typeof(RuntimeLog).GetField("_logger", BindingFlags.Instance | BindingFlags.NonPublic);
            CheckCase(loggerField?.GetValue(emptyHome) is null && !Directory.Exists(root), "empty home Write creates no file or logger");
            emptyHome.Dispose();

            var firstHome = Path.Combine(root, "first");
            var otherHome = Path.Combine(root, "other");
            var firstPath = Path.Combine(firstHome, AppInfo.LogFileName);
            var otherPath = Path.Combine(otherHome, AppInfo.LogFileName);
            using (var logger = new RuntimeLog())
            {
                logger.SetHome(firstHome);
                logger.Write("first home");
                logger.Flush();
                var firstWriteLanded = File.Exists(firstPath) && File.ReadAllLines(firstPath).Last().EndsWith("first home", StringComparison.Ordinal);
                logger.SetHome(otherHome);
                logger.Write("still first home");
                logger.Flush();
                var staysAtFirstHome = File.ReadAllLines(firstPath).Last().EndsWith("still first home", StringComparison.Ordinal)
                    && !File.Exists(otherPath);
                CheckCase(firstWriteLanded && staysAtFirstHome, "lazy write uses first home and never retargets");
            }

            var crashPath = Path.Combine(root, "crash", AppInfo.LogFileName);
            var crashLog = new RuntimeLog();
            crashLog.SetHome(Path.GetDirectoryName(crashPath)!);
            crashLog.LogCrash(new InvalidOperationException("boom"));
            CheckCase(File.Exists(crashPath) && File.ReadAllLines(crashPath).Any(line => line.Contains("crash ", StringComparison.Ordinal)
                && line.EndsWith("boom", StringComparison.Ordinal)), "LogCrash flushes an early crash line");
            crashLog.Dispose();

            var disposePath = Path.Combine(root, "dispose", AppInfo.LogFileName);
            var disposeLog = new RuntimeLog();
            disposeLog.SetHome(Path.GetDirectoryName(disposePath)!);
            disposeLog.Write("drained on dispose");
            disposeLog.Dispose();
            var writeAfterDispose = true;
            try { disposeLog.Write("after dispose"); }
            catch { writeAfterDispose = false; }
            CheckCase(File.Exists(disposePath) && File.ReadAllLines(disposePath).Any(line => line.EndsWith("drained on dispose", StringComparison.Ordinal))
                && writeAfterDispose, "Dispose drains queued writes and later Write does not throw");

            var flushWithoutLogger = true;
            try
            {
                var neverCreated = new RuntimeLog();
                neverCreated.Flush();
                neverCreated.Dispose();
            }
            catch { flushWithoutLogger = false; }
            CheckCase(flushWithoutLogger, "Flush without a logger is safe");

            report = failures.Count == 0 ? "5 checks" : string.Join("; ", failures);
            return failures.Count == 0;
        }
        catch (Exception ex)
        {
            report = string.Join("; ", failures.Append(ex.ToString()));
            return false;
        }
        finally { try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { } }
    }

    private static bool AppInfoSelfTest(out string report)
    {
        try
        {
            var assembly = typeof(AppInfo).Assembly;
            var product = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), false)
                .Cast<System.Reflection.AssemblyProductAttribute>().Single().Product;
            var title = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false)
                .Cast<System.Reflection.AssemblyTitleAttribute>().Single().Title;
            var informationalVersion = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
                .Cast<System.Reflection.AssemblyInformationalVersionAttribute>().Single().InformationalVersion;
            var assemblyVersion = assembly.GetName().Version?.ToString();
            var fileVersion = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), false)
                .Cast<System.Reflection.AssemblyFileVersionAttribute>().Single().Version;
            var description = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false)
                .Cast<System.Reflection.AssemblyDescriptionAttribute>().Single().Description;
            var copyright = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false)
                .Cast<System.Reflection.AssemblyCopyrightAttribute>().Single().Copyright;

            if (AppInfo.DisplayName != product || AppInfo.DisplayName != title || assembly.GetName().Name != AppInfo.Name
                || AppInfo.InformationalVersion != informationalVersion
                || !informationalVersion.StartsWith(AppInfo.Version, StringComparison.Ordinal)
                || assemblyVersion != $"{AppInfo.Version}.0" || fileVersion != $"{AppInfo.Version}.0"
                || AppInfo.Description != description || AppInfo.Copyright != copyright
                || AppInfo.InstanceId != AppInfo.Name || AppInfo.DevInstanceId != $"{AppInfo.Name}-dev"
                || AppInfo.DataFolderName != AppInfo.Name || AppInfo.RunValueName != AppInfo.Name
                || AppInfo.LogFileName != $"{AppInfo.Name.ToLowerInvariant()}.log"
                || AppInfo.EnvPrefix != AppInfo.Name.ToUpperInvariant()
                || AppInfo.Env("DATA") != $"{AppInfo.EnvPrefix}_DATA"
                || AppInfo.Version + (AppInfo.Label.Length == 0 ? "" : $"-{AppInfo.Label}") != informationalVersion)
            {
                report = "AppInfo does not match the generated assembly identity or its derived identifiers";
                return false;
            }

            if (!DevSlotIdentityChecks(out var slotReport))
            {
                report = "dev slots: " + slotReport;
                return false;
            }

            // Use the file resource loader directly: selftest never initializes the desktop platform.
            var assets = new StandardAssetLoader();
            using var icon = assets.Open(new Uri(AppInfo.IconUri));
            using var font = assets.Open(new Uri(AppInfo.FontUri));
            if (icon.ReadByte() < 0 || font.ReadByte() < 0)
            {
                report = "bundled icon or font resource is empty";
                return false;
            }

            report = "assembly attributes, derived identifiers, bundled icon/font URIs; " + slotReport;
            return true;
        }
        catch (Exception ex)
        {
            report = ex.Message;
            return false;
        }
    }

    private static bool StartupRequestLatchChecks(out string report)
    {
        var requests = new StartupRequestLatch();
        var shows = 0;
        var quits = 0;
        Action dispatchShow = () => shows++;
        Action dispatchQuit = () => quits++;

        requests.RequestShow(dispatchShow);
        requests.RequestQuit(dispatchQuit);
        var heldUntilReady = shows == 0 && quits == 0;
        requests.Attach(dispatchShow, dispatchQuit);
        var pendingConsumed = shows == 1 && quits == 1;
        requests.Attach(dispatchShow, dispatchQuit);
        var consumedOnce = shows == 1 && quits == 1;

        requests.RequestShow(dispatchShow);
        requests.RequestQuit(dispatchQuit);
        var readyRequestsDispatched = shows == 2 && quits == 2;

        report = heldUntilReady && pendingConsumed && consumedOnce && readyRequestsDispatched
            ? "early show and quit wait for attach, pending requests dispatch once, and ready requests dispatch directly"
            : $"early={heldUntilReady}, consumed={pendingConsumed}, once={consumedOnce}, ready={readyRequestsDispatched}";
        return heldUntilReady && pendingConsumed && consumedOnce && readyRequestsDispatched;
    }

    private static bool WelcomeSlotsSelfTest(out string report)
    {
        var fresh = new StoreData();
        Welcome.SeedIfNew(fresh, new SettingsService(fresh.Settings));
        var expected = new[] { "# h1", "math", "list", "timer", $"# Welcome to {AppInfo.Name}" };
        var seeded = expected.Select((line, i) => fresh.Slots[i].Text.Split('\n')[0] == line).All(ok => ok)
            && fresh.Slots.Take(5).All(note => !note.IsEmpty)
            && fresh.Slots.Skip(5).All(note => note.IsEmpty)
            && fresh.Stack.Count == 0
            && fresh.ViewSlot == 5;
        var timerLines = fresh.Slots[3].Text.Replace("\r", "").Split('\n');
        var timersParse = timerLines.Length >= 6
            && Enumerable.Range(1, 5).All(i => Timers.TryParse(timerLines[i], out _, out _));

        var occupied = new StoreData();
        occupied.Slots[0].Text = "keep this note\nwith its body";
        Welcome.SeedIfNew(occupied, new SettingsService(occupied.Settings));
        var preserved = occupied.Slots[0].Text == "keep this note\nwith its body";
        var beforeSecondCall = occupied.Slots.Select(note => note.Text).ToArray();
        var viewBeforeSecondCall = occupied.ViewSlot;
        Welcome.SeedIfNew(occupied, new SettingsService(occupied.Settings));
        var secondCallNoOp = occupied.Slots.Select(note => note.Text).SequenceEqual(beforeSecondCall)
            && occupied.ViewSlot == viewBeforeSecondCall;
        var defaults = new Settings() is { Scrolling: "rate", HorizontalScrollStyle: "follow" };

        report = "slots 1-5 seeded, 6-9 empty, stack empty, opens slot 5; occupied slot preserved; second call unchanged; five timers parse; rate/follow-through defaults";
        return seeded && timersParse && preserved && secondCallNoOp && defaults;
    }

    private static bool DevBadgeBuildStampChecks(out string report)
    {
        var time = new DateTimeOffset(2026, 10, 29, 15, 4, 0, TimeSpan.FromHours(-4));
        var failures = new List<string>();
        void Check(bool ok, string detail) { if (!ok) failures.Add(detail); }
        Check(AppInfo.FormatDevBadge("feat/A54-link-hover-click", time) ==
            ("\uE0A0 A54-3:04", "\uE0A0 A54 - link-hover-click - 3:04 PM"), "feature format");
        Check(AppInfo.FormatDevBadge("fix/A11-settings-thread-count", time) ==
            ("\uE0A0 A11-3:04", "\uE0A0 A11 - settings-thread-count - 3:04 PM"), "fix prefix");
        Check(AppInfo.FormatDevBadge("docs/D1-ui-region-names", time) ==
            ("\uE0A0 D1-3:04", "\uE0A0 D1 - ui-region-names - 3:04 PM"), "docs prefix");
        Check(AppInfo.FormatDevBadge("chore/T17-multi-dev-instances", time) ==
            ("\uE0A0 T17-3:04", "\uE0A0 T17 - multi-dev-instances - 3:04 PM"), "chore prefix");
        Check(AppInfo.FormatDevBadge("feat/A43b-notes-session", time) ==
            ("\uE0A0 A43b-3:04", "\uE0A0 A43b - notes-session - 3:04 PM"), "letter suffix");
        Check(AppInfo.FormatDevBadge("Loop12-sample-slug", time) ==
            ("\uE0A0 Loop12-3:04", "\uE0A0 Loop12 - sample-slug - 3:04 PM"), "multi-letter ID");
        Check(AppInfo.FormatDevBadge("dev", time) == ("\uE0A0 dev", "\uE0A0 dev - 3:04 PM"), "dev branch");
        Check(AppInfo.FormatDevBadge("topic-long-branch-name", time) ==
            ("\uE0A0 topic-long-b-3:04", "\uE0A0 topic-long-branch-name - 3:04 PM"), "other branch");
        Check(AppInfo.FormatDevBadge("(detached HEAD)", time) ==
            ("\uE0A0 3:04", "\uE0A0 3:04 PM (detached HEAD)")
            && AppInfo.FormatDevBadge(null, time) == ("\uE0A0 3:04", "\uE0A0 3:04 PM (detached HEAD)")
            && AppInfo.FormatDevBadge("", time) == ("\uE0A0 3:04", "\uE0A0 3:04 PM (detached HEAD)"), "detached or empty branch");
        Check(AppInfo.FormatDevBadge(null, null) == ("dev", "dev")
            && AppInfo.FormatDevBadge("feat/A54-link-hover-click", null) == ("dev", "dev"), "missing stamp");
        if (!AppInfo.IsDevBuild)
            Check(!typeof(AppInfo).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .Any(a => a.Key is "AntiNotepadBranch" or "AntiNotepadBuildTime"), "non-dev assembly carries stamp");
        Check(AppInfo.BuildBranch is null || AppInfo.BuildTime is not null, "branch requires timestamp");
        report = failures.Count == 0 ? "formatting and assembly metadata OK" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static bool DevSlotIdentityChecks(out string report)
    {
        var failures = new List<string>();
        void Check(bool passed, string detail) { if (!passed) failures.Add(detail); }
        var slots = AppInfo.DevSlots;
        var slotIds = slots.Select(slot => AppInfo.DevInstanceIdForSlot(slot.Name)).ToArray();
        Check(slots.Select(slot => slot.Name).SequenceEqual(["Blue", "Red", "Green", "Purple", "Orange", "Silver"]), "slot order");
        Check(slotIds.All(id => id is not null && id != AppInfo.InstanceId && id != AppInfo.DevInstanceId)
            && slotIds.Distinct(StringComparer.Ordinal).Count() == slots.Count, "slot ids are distinct from installed and legacy dev ids");
        Check(AppInfo.ResolveSlotName(null) is null && AppInfo.ResolveSlotName("") is null
            && AppInfo.ResolveSlotName("chartreuse") is null
            && AppInfo.ResolveSlotName("bLuE") == "Blue", "slot validation and canonical name");
        Check(AppInfo.Env("NEWNOTE_HOTKEY") == "ANTINOTEPAD_NEWNOTE_HOTKEY", "new-note hotkey environment name");

        var oldSlot = Environment.GetEnvironmentVariable(AppInfo.Env("SLOT"));
        var oldActiveSlot = AppInfo.ActiveSlotName;
        try
        {
            Environment.SetEnvironmentVariable(AppInfo.Env("SLOT"), "Red");
            var validEnvironmentSlot = AppInfo.SlotName == "Red";
            AppInfo.SetActiveSlotName(null);
            var slotWithoutDataIgnored = AppInfo.SlotName == "Red" && AppInfo.ActiveSlotName is null
                && AppInfo.IconUri == AppInfo.ResolveIconUri(AppInfo.IsDevBuild, null);
            AppInfo.SetActiveSlotName(AppInfo.SlotName);
            var slotWithDataActive = AppInfo.ActiveSlotName == "Red"
                && AppInfo.SlotColor == AppInfo.DevSlots.Single(slot => slot.Name == "Red").Color
                && AppInfo.IconUri == AppInfo.ResolveIconUri(AppInfo.IsDevBuild, "Red");
            Environment.SetEnvironmentVariable(AppInfo.Env("SLOT"), "chartreuse");
            AppInfo.SetActiveSlotName(AppInfo.SlotName);
            var invalidEnvironmentSlot = AppInfo.SlotName is null && AppInfo.ActiveSlotName is null;
            Environment.SetEnvironmentVariable(AppInfo.Env("SLOT"), "");
            AppInfo.SetActiveSlotName(AppInfo.SlotName);
            var emptyEnvironmentSlot = AppInfo.SlotName is null && AppInfo.ActiveSlotName is null;
            Check(validEnvironmentSlot && slotWithoutDataIgnored && slotWithDataActive
                && invalidEnvironmentSlot && emptyEnvironmentSlot, "slot environment parsing and data confinement");
        }
        finally
        {
            Environment.SetEnvironmentVariable(AppInfo.Env("SLOT"), oldSlot);
            AppInfo.SetActiveSlotName(oldActiveSlot);
        }

        var isolatedIds = slots.Select(slot => AppInfo.ResolveInstanceId("worktree-data", slot.Name)).ToArray();
        Check(isolatedIds.Distinct(StringComparer.Ordinal).Count() == slots.Count, "custom data selects distinct slot ids");
        Check(AppInfo.ResolveInstanceId(null, "Blue") == AppInfo.InstanceId
            && AppInfo.ResolveInstanceId("", "Red") == AppInfo.InstanceId, "slot ignored without custom data");
        Check(AppInfo.ResolveInstanceId("worktree-data", null) == AppInfo.DevInstanceId
            && AppInfo.ResolveInstanceId("worktree-data", "chartreuse") == AppInfo.DevInstanceId, "no valid slot keeps legacy dev id");

        var hotkeysParse = slots.Select((slot, index) =>
        {
            var show = GlobalHotkey.HotkeyGesture.Parse(slot.ShowHideHotkey);
            var newNote = GlobalHotkey.HotkeyGesture.Parse(slot.NewNoteHotkey);
            return show.VirtualKey == '1' + index && newNote.VirtualKey == 0x70 + index;
        }).All(passed => passed);
        Check(hotkeysParse, "show/hide and new-note hotkey strings parse in slot order");

        var reads = 0;
        string? ReadEnvironment() { reads++; return "Ctrl+Alt+Shift+F1"; }
        var noSlotDefault = HotkeyRegistrar.ResolveInitialNewNoteHotkey(null, false, ReadEnvironment) is null && reads == 0;
        var configuredWins = HotkeyRegistrar.ResolveInitialNewNoteHotkey("Ctrl+Shift+N", true, ReadEnvironment) == "Ctrl+Shift+N" && reads == 0;
        var slotDefault = HotkeyRegistrar.ResolveInitialNewNoteHotkey(null, true, ReadEnvironment) == "Ctrl+Alt+Shift+F1" && reads == 1;
        Check(noSlotDefault && configuredWins && slotDefault, "new-note environment default is lazy and slot-only");

        report = failures.Count == 0 ? "six slot ids, isolation, environment parsing, and hotkey defaults" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static bool InstalledInstanceIdentityChecks(out string report)
    {
        var stable = AppInfo.InstanceId == "AntiNotepad"
            && AppInfo.DevInstanceIdForSlot("Blue") != AppInfo.InstanceId
            && AppInfo.DevSlots.All(slot => AppInfo.ResolveInstanceId(null, slot.Name) == AppInfo.InstanceId);
        report = stable ? "installed identity remains AntiNotepad with or without a slot environment" : "installed identity changed";
        return stable;
    }

    private static bool LegacyDevInstanceIdentityChecks(out string report)
    {
        var stable = AppInfo.DevInstanceId == "AntiNotepad-dev"
            && AppInfo.ResolveInstanceId("worktree-data", null) == AppInfo.DevInstanceId
            && AppInfo.ResolveInstanceId("worktree-data", "invalid") == AppInfo.DevInstanceId;
        report = stable ? "custom data without a valid slot keeps AntiNotepad-dev" : "legacy dev identity changed";
        return stable;
    }

    private static bool DevIdentitySelfTest(out string report)
    {
        try
        {
            var expectedName = AppInfo.IsDevBuild ? "AntiNotepad (dev)" : "AntiNotepad";
            if (AppInfo.DisplayName != expectedName || AppInfo.IconUri != AppInfo.ResolveIconUri(AppInfo.IsDevBuild, AppInfo.ActiveSlotName)
                || AppInfo.Name != "AntiNotepad" || AppInfo.InstanceId != "AntiNotepad"
                || AppInfo.DevInstanceId != "AntiNotepad-dev" || AppInfo.DataFolderName != "AntiNotepad"
                || AppInfo.RunValueName != "AntiNotepad" || AppInfo.LogFileName != "antinotepad.log"
                || AppInfo.EnvPrefix != "ANTINOTEPAD" || AppInfo.ResourceUri != "avares://AntiNotepad/")
            {
                report = "display identity or stable storage/resource identity differs from the build contract";
                return false;
            }

            var assets = new StandardAssetLoader();
            var icons = new[] { "antinotepad.ico", "antinotepad-dev.ico" }
                .Concat(AppInfo.DevSlots.Select(slot => $"antinotepad-dev-{slot.Name.ToLowerInvariant()}.ico"));
            foreach (var file in icons)
            {
                using var stream = assets.Open(new Uri($"avares://AntiNotepad/{file}"));
                using var reader = new BinaryReader(stream);
                if (reader.ReadUInt16() != 0 || reader.ReadUInt16() != 1 || reader.ReadUInt16() == 0)
                    throw new InvalidOperationException($"Invalid icon: {file}");
            }
            var slotIconsResolve = AppInfo.DevSlots.All(slot =>
                AppInfo.ResolveIconUri(true, slot.Name) == $"avares://AntiNotepad/antinotepad-dev-{slot.Name.ToLowerInvariant()}.ico");
            if (!slotIconsResolve) throw new InvalidOperationException("slot icon URI does not match its resource");
            report = $"{expectedName}, {AppInfo.IconUri}; all icons load; stable instance/data/resource identities";
            Console.WriteLine("IDENTITY " + report);
            return true;
        }
        catch (Exception ex)
        {
            report = ex.Message;
            return false;
        }
    }
}
