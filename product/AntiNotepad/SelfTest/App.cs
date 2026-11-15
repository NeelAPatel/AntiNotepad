using AntiNotepad.Model;
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

            // Use the file resource loader directly: selftest never initializes the desktop platform.
            var assets = new StandardAssetLoader();
            using var icon = assets.Open(new Uri(AppInfo.IconUri));
            using var font = assets.Open(new Uri(AppInfo.FontUri));
            if (icon.ReadByte() < 0 || font.ReadByte() < 0)
            {
                report = "bundled icon or font resource is empty";
                return false;
            }

            report = "assembly attributes, derived identifiers, and bundled icon/font URIs";
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
        var time = new DateTimeOffset(2026, 10, 29, 11, 30, 0, TimeSpan.FromHours(-4));
        var failures = new List<string>();
        void Check(bool ok, string detail) { if (!ok) failures.Add(detail); }
        Check(AppInfo.FormatDevBadge("feat/A54-link-hover-click", time) ==
            ("\uE0A0 A54 · 11:30 AM", "\uE0A0 dev/A54-link-hover-click@2026-10-29 11:30 AM"), "feature format");
        Check(AppInfo.FormatDevBadge("feat/A43b-notes-session", time).Short == "\uE0A0 A43b · 11:30 AM", "letter suffix");
        Check(AppInfo.FormatDevBadge("dev", time) == ("\uE0A0 dev", "\uE0A0 dev"), "dev branch");
        Check(AppInfo.FormatDevBadge("topic-long-branch-name", time) ==
            ("\uE0A0 topic-long-b · 11:30 AM", "\uE0A0 dev/topic-long-branch-name@2026-10-29 11:30 AM"), "other branch");
        Check(AppInfo.FormatDevBadge(null, time) == ("dev", "dev")
            && AppInfo.FormatDevBadge("", time) == ("dev", "dev")
            && AppInfo.FormatDevBadge("feat/A54-link-hover-click", null) == ("dev", "dev"), "missing stamp");
        if (!AppInfo.IsDevBuild)
            Check(!typeof(AppInfo).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .Any(a => a.Key is "AntiNotepadBranch" or "AntiNotepadBuildTime"), "non-dev assembly carries stamp");
        Check(AppInfo.BuildBranch is null || AppInfo.BuildTime is not null, "branch requires timestamp");
        report = failures.Count == 0 ? "formatting and assembly metadata OK" : string.Join("; ", failures);
        return failures.Count == 0;
    }

    private static bool DevIdentitySelfTest(out string report)
    {
        try
        {
            var expectedName = AppInfo.IsDevBuild ? "AntiNotepad (dev)" : "AntiNotepad";
            var expectedIcon = AppInfo.IsDevBuild ? "antinotepad-dev.ico" : "antinotepad.ico";
            if (AppInfo.DisplayName != expectedName || AppInfo.IconUri != $"avares://AntiNotepad/{expectedIcon}"
                || AppInfo.Name != "AntiNotepad" || AppInfo.InstanceId != "AntiNotepad"
                || AppInfo.DevInstanceId != "AntiNotepad-dev" || AppInfo.DataFolderName != "AntiNotepad"
                || AppInfo.RunValueName != "AntiNotepad" || AppInfo.LogFileName != "antinotepad.log"
                || AppInfo.EnvPrefix != "ANTINOTEPAD" || AppInfo.ResourceUri != "avares://AntiNotepad/")
            {
                report = "display identity or stable storage/resource identity differs from the build contract";
                return false;
            }

            var assets = new StandardAssetLoader();
            foreach (var file in new[] { "antinotepad.ico", "antinotepad-dev.ico" })
            {
                using var stream = assets.Open(new Uri($"avares://AntiNotepad/{file}"));
                using var reader = new BinaryReader(stream);
                if (reader.ReadUInt16() != 0 || reader.ReadUInt16() != 1 || reader.ReadUInt16() == 0)
                    throw new InvalidOperationException($"Invalid icon: {file}");
            }
            report = $"{expectedName}, {AppInfo.IconUri}; both icons load; stable instance/data/resource identities";
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
