using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AntiQuickPad;

// The event stream is deliberately separate from the ordinary diagnostic log.
internal static class Measurement
{
    public static string? ChildRoot { get; private set; }
    public static string? InstanceId { get; private set; }
    public static bool IsChild => ChildRoot != null;
    private static StreamWriter? _events;
    private static DispatcherTimer? _commands;
    private static int _frameGeneration;
    public static long Now { get { QueryPerformanceCounter(out var value); return value; } }
    private static long Frequency { get { QueryPerformanceFrequency(out var value); return value; } }
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private sealed record Stamp(string Kind, long Qpc, string? Detail);

    public static void InitializeChild(string[] args)
    {
        if (!args.Contains("--measure-child")) return;
        ChildRoot = Path.GetFullPath(Value(args, "--data") ?? throw new ArgumentException("--data required"));
        InstanceId = Value(args, "--instance") ?? throw new ArgumentException("--instance required");
        Directory.CreateDirectory(ChildRoot);
        _events = new StreamWriter(new FileStream(Path.Combine(ChildRoot, "events.jsonl"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) { AutoFlush = true };
        Event("child-main");
    }

    public static void Event(string kind, long qpc = 0, string? detail = null)
    {
        if (_events == null) return;
        var stamp = new Stamp(kind, qpc == 0 ? Now : qpc, detail);
        _events.WriteLine(JsonSerializer.Serialize(stamp));
    }

    public static bool IsForeground(Window window) => GetForegroundWindow() == window.TryGetPlatformHandle()?.Handle;

    public static void Frame(Window window)
    {
        if (!IsChild) return;
        var generation = ++_frameGeneration;
        window.RequestAnimationFrame(_ =>
        {
            if (generation == _frameGeneration && window.IsVisible) Event("frame");
        });
    }

    public static void Attach(Views.IShellView shell, bool registered, double scale)
    {
        var window = shell.Host;
        Event("ready", detail: $"hotkey={registered};scale={scale}");
        Frame(window);
        _commands = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _commands.Tick += (_, _) =>
        {
            var path = Path.Combine(ChildRoot!, "command");
            if (!File.Exists(path)) return;
            var command = File.ReadAllText(path); File.Delete(path);
            try
            {
                switch (command)
                {
                    case "hide": shell.HideResident(); break;
                    case "show": shell.ShowResident("measure-control"); break;
                    case "heavy": shell.MeasurementHeavy(); break;
                    case "quit": shell.Quit(source: "command"); return;
                }
            }
            catch (Exception ex) { Event("error", detail: $"{command}: {ex.GetType().Name}: {ex.Message}"); return; }
            Event("ack", detail: command);
        };
        _commands.Start();
        // Graceful safety exit if the harness disappears; never terminate a process.
        var watchdog = new DispatcherTimer { Interval = TimeSpan.FromMinutes(10) };
        watchdog.Tick += (_, _) => { watchdog.Stop(); shell.Quit(source: "command"); };
        watchdog.Start();
    }

    private static string? Value(string[] args, string flag)
    {
        var i = Array.IndexOf(args, flag);
        if (i < 0) return null;
        if (i + 1 == args.Length) throw new ArgumentException($"{flag} needs a value");
        return args[i + 1];
    }
    private static ProcessStartInfo StartInfo(params string[] args)
    {
        // Always use the Release apphost, even when the harness was invoked via dotnet.
        var start = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "AntiQuickPad.exe"))
        { UseShellExecute = false, CreateNoWindow = true };
        if (Program.RequestedShell is { } shell) start.Environment["ANTIQUICKPAD_SHELL"] = shell;
        foreach (var arg in args) start.ArgumentList.Add(arg);
        start.Environment["ANTIQUICKPAD_HOTKEY"] = "Ctrl+Alt+F9";
        return start;
    }
    private static List<Stamp> Read(string root)
    {
        var result = new List<Stamp>();
        var file = Path.Combine(root, "events.jsonl");
        if (!File.Exists(file)) return result;
        using var reader = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        while (reader.ReadLine() is { } line)
        {
            try { if (JsonSerializer.Deserialize<Stamp>(line) is { } stamp) result.Add(stamp); }
            catch (JsonException) { /* Writer may be completing the final line. */ }
        }
        return result;
    }
    private static async Task<Stamp?> Wait(string root, Process child, string kind, long after, int timeout = 3000, string? detail = null)
    {
        var timer = Stopwatch.StartNew();
        while (!child.HasExited && timer.ElapsedMilliseconds < timeout)
        {
            var found = Read(root).FirstOrDefault(e => e.Kind == kind && e.Qpc > after && (detail == null || e.Detail == detail));
            if (found != null) return found;
            await Task.Delay(10);
        }
        return null;
    }
    private static async Task Command(string root, Process child, string command)
    {
        var before = Now;
        var path = Path.Combine(root, "command");
        File.WriteAllText(path + ".tmp", command); File.Move(path + ".tmp", path, true);
        if (command != "quit" && await Wait(root, child, "ack", before, timeout: command == "heavy" ? 30000 : 3000, detail: command) == null)
        {
            var error = Read(root).LastOrDefault(e => e.Kind == "error" && e.Qpc > before)?.Detail;
            throw new IOException(error is null ? $"Child control '{command}' timed out (exited={child.HasExited})." : $"Child control failed: {error}");
        }
    }

    private sealed record Sample(int Index, bool Warmup, long InputQpc, uint Sent, int SendError,
        double? HotkeyMs, double? FocusMs, double? FrameCallbackMs, long? EscapeQpc, uint EscapeSent, int EscapeError, double? HideMs);
    private static double Ms(long start, long end) => (end - start) * 1000.0 / Frequency;
    private static double? Delta(long start, Stamp? end) => end == null ? null : Ms(start, end.Qpc);

    private static async Task<List<Sample>> Samples(string root, Process child, int count, List<string> gaps)
    {
        var samples = new List<Sample>();
        await Command(root, child, "hide");
        for (var i = 0; i < count; i++)
        {
            await Task.Delay(300);
            var chord = Inputs(0x11, 0x12, 0x78);
            var input = Now;
            var sent = SendInput((uint)chord.Length, chord, Marshal.SizeOf<Input>());
            var error = sent == chord.Length ? 0 : Marshal.GetLastWin32Error();
            var hotkey = await Wait(root, child, "hotkey", input);
            var focused = hotkey == null ? null : await Wait(root, child, "focused", input);
            var frame = hotkey == null ? null : await Wait(root, child, "frame", input);
            long? escape = null; uint escapeSent = 0; int escapeError = 0; Stamp? hidden = null;
            if (focused != null)
            {
                var keys = Inputs(0x1B); escape = Now;
                escapeSent = SendInput((uint)keys.Length, keys, Marshal.SizeOf<Input>());
                escapeError = escapeSent == keys.Length ? 0 : Marshal.GetLastWin32Error();
                hidden = await Wait(root, child, "hidden", escape.Value);
            }
            samples.Add(new(i, i < 3, input, sent, error, Delta(input, hotkey), Delta(input, focused), Delta(input, frame),
                escape, escapeSent, escapeError, escape == null ? null : Delta(escape.Value, hidden)));
            if (hotkey == null || focused == null || frame == null || hidden == null)
            {
                gaps.Add($"Sample {i}: SendInput={sent}/{chord.Length}, error={error}; WM_HOTKEY={hotkey != null}, foreground editor focus={focused != null}, frame callback={frame != null}, Esc hidden={hidden != null}. Missing endpoints are unavailable, not zero. UIPI can block input without a diagnostic last-error.");
                await Command(root, child, "hide");
                if (hotkey == null || focused == null) break; // Repeating blocked desktop input yields no useful baseline.
            }
        }
        return samples;
    }

    private static object? Summary(IEnumerable<double?> values)
    {
        var sorted = values.Where(v => v.HasValue).Select(v => v!.Value).Order().ToArray();
        if (sorted.Length == 0) return null;
        return new { count = sorted.Length, min = sorted[0], median = (sorted[(sorted.Length - 1) / 2] + sorted[sorted.Length / 2]) / 2,
            p95 = sorted[(int)Math.Ceiling(sorted.Length * .95) - 1], max = sorted[^1] };
    }
    private static object Summaries(List<Sample> samples)
    {
        var kept = samples.Where(s => !s.Warmup).ToArray();
        return new { hotkeyMs = Summary(kept.Select(s => s.HotkeyMs)), focusMs = Summary(kept.Select(s => s.FocusMs)),
            frameCallbackMs = Summary(kept.Select(s => s.FrameCallbackMs)), hideMs = Summary(kept.Select(s => s.HideMs)),
            visibleAndFocusedMs = Summary(kept.Select(s => s.FocusMs.HasValue && s.FrameCallbackMs.HasValue ? Math.Max(s.FocusMs.Value, s.FrameCallbackMs.Value) : (double?)null)) };
    }

    private static async Task<object> Idle(string root, Process child, bool shown)
    {
        await Command(root, child, shown ? "show" : "hide");
        await Task.Delay(1000); // settle autosave/cue; excluded from CPU window
        child.Refresh(); var cpu = child.TotalProcessorTime; var start = Now;
        await Task.Delay(30000);
        child.Refresh(); var elapsed = Ms(start, Now);
        return new { shown, elapsedMs = elapsed, cpuPercentOneCore = (child.TotalProcessorTime - cpu).TotalMilliseconds / elapsed * 100,
            workingSetBytes = child.WorkingSet64, privateBytes = child.PrivateMemorySize64 };
    }

    public static async Task<int> Run(string[] args)
    {
        var gaps = new List<string>();
        var requestedSamples = 30;
        var output = Path.GetFullPath(Value(args, "--out") ?? Path.Combine(AppContext.BaseDirectory, "measure.json"));
        var root = Path.Combine(AppContext.BaseDirectory, "measure", Guid.NewGuid().ToString("N"));
        var id = "AntiQuickPad-measure-" + Guid.NewGuid().ToString("N");
        Process? child = null; object? baseline = null, heavy = null; double? cold = null; string? ready = null;
        var success = true;
        try
        {
            var count = int.Parse(Value(args, "--samples") ?? "30");
            requestedSamples = count;
            if (count is < 1 or > 10000) throw new ArgumentException("--samples must be 1..10000 (first 3 are warmups per state).");
            Directory.CreateDirectory(root);
            var start = Now;
            child = Process.Start(StartInfo("--measure-child", "--instance", id, "--data", root)) ?? throw new IOException("Child launch failed");
            ready = (await Wait(root, child, "ready", start, 15000))?.Detail;
            cold = Delta(start, await Wait(root, child, "frame", start));
            if (ready == null) throw new IOException($"Child never became ready; exited={child.HasExited}.");
            if (!ready.Contains("hotkey=True")) gaps.Add("Alternate Ctrl+Alt+F9 registration failed; inspect child shell.log for Win32 error.");
            var first = await Samples(root, child, count, gaps);
            baseline = new { samples = first, summary = Summaries(first), hiddenIdle = await Idle(root, child, false), shownIdle = await Idle(root, child, true) };
            await Command(root, child, "heavy");
            var second = await Samples(root, child, count, gaps);
            heavy = new { notes = 20, linesPerNote = 200, samples = second, summary = Summaries(second), hiddenIdle = await Idle(root, child, false), shownIdle = await Idle(root, child, true) };
        }
        catch (Exception ex) { gaps.Add(ex.ToString()); success = false; }
        finally
        {
            if (child != null)
            {
                if (!child.HasExited)
                {
                    try { await Command(root, child, "quit"); await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(6)); }
                    catch (Exception ex) { gaps.Add("Graceful cleanup failed; child has 10-minute graceful watchdog: " + ex.Message); success = false; }
                }
                if (child.HasExited && child.ExitCode != 0) { gaps.Add($"Child exit={child.ExitCode}"); success = false; }
                child.Dispose();
            }
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            File.WriteAllText(output, JsonSerializer.Serialize(new { schema = 1, utc = DateTimeOffset.UtcNow, qpcFrequency = Frequency,
                os = RuntimeInformation.OSDescription, processors = Environment.ProcessorCount, framework = RuntimeInformation.FrameworkDescription,
                release = !IsDebug, requestedSamples, warmupsPerState = 3, minimumSpacingMs = 300, idleWindowSeconds = 30,
                childRoot = root, instance = id, ready, coldStartFrameCallbackMs = cold, baseline, heavy, gaps }, Json));
            Console.WriteLine($"MEASURE {(success ? "COMPLETE" : "INCOMPLETE")}: {output}; gaps={gaps.Count}");
        }
        return success ? 0 : 1;
    }
    private static bool IsDebug
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
    public static async Task Smoke(string parent)
    {
        var output = Path.Combine(parent, "measure-smoke.json");
        using var child = Process.Start(StartInfo("--measure", "--samples", "3", "--out", output)) ?? throw new IOException("Measure smoke launch failed");
        await child.WaitForExitAsync().WaitAsync(TimeSpan.FromMinutes(4));
        using var json = JsonDocument.Parse(File.ReadAllText(output));
        if (child.ExitCode != 0 || json.RootElement.GetProperty("baseline").ValueKind != JsonValueKind.Object ||
            json.RootElement.GetProperty("heavy").ValueKind != JsonValueKind.Object) throw new Exception("Measure smoke failed; inspect " + output);
        Console.WriteLine("T010 PASS: --measure --samples 3 wrote baseline/heavy JSON (unavailable desktop endpoints remain explicit gaps).");
    }

    // INPUT's union must include MOUSEINPUT size/alignment (40 bytes total on x64).
    [StructLayout(LayoutKind.Sequential)] private struct Input { public uint Type; public InputUnion Data; }
    [StructLayout(LayoutKind.Explicit)] private struct InputUnion
    {
        [FieldOffset(0)] public Keyboard Keyboard;
        [FieldOffset(0)] public Mouse Mouse;
    }
    [StructLayout(LayoutKind.Sequential)] private struct Keyboard { public ushort Key, Scan; public uint Flags, Time; public UIntPtr Extra; }
    [StructLayout(LayoutKind.Sequential)] private struct Mouse { public int X, Y; public uint Data, Flags, Time; public UIntPtr Extra; }
    private static Input[] Inputs(params ushort[] keys) => keys.Select(k => new Input { Type = 1, Data = new InputUnion { Keyboard = new Keyboard { Key = k } } })
        .Concat(keys.Reverse().Select(k => new Input { Type = 1, Data = new InputUnion { Keyboard = new Keyboard { Key = k, Flags = 2 } } })).ToArray();
    [DllImport("user32.dll", SetLastError = true)] private static extern uint SendInput(uint count, Input[] inputs, int size);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("kernel32.dll")] private static extern bool QueryPerformanceCounter(out long value);
    [DllImport("kernel32.dll")] private static extern bool QueryPerformanceFrequency(out long value);
}
