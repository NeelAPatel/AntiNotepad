# A26-allow-windows-shutdown

Implemented on `feat/A26-allow-windows-shutdown`; no branch changes, merge or commit. STATUS and the audit document are unchanged.

## Session-end path and source evidence

Avalonia 12.1.4's hidden `AvaloniaMessageWindow <guid>` receives `WM_QUERYENDSESSION` and raises the platform shutdown event. Its OS flag is internal and only true when lParam is zero; sign-out flags therefore produce `ApplicationShutdown`, not `OSShutdown`. See [Win32Platform.cs, lines 139-160 and 205-224](https://github.com/AvaloniaUI/Avalonia/blob/12.1.4/src/Windows/Avalonia.Win32/Win32Platform.cs#L139-L160) and [ShutdownRequestedEventArgs.cs](https://github.com/AvaloniaUI/Avalonia/blob/12.1.4/src/Avalonia.Controls/ApplicationLifetimes/ShutdownRequestedEventArgs.cs).

The desktop lifetime raises its public `ShutdownRequested` before closing windows; cancellation by a window propagates back as a rejected query. Forced `Shutdown()` skips that event. See [ClassicDesktopStyleApplicationLifetime.cs, lines 61-68 and 161-223](https://github.com/AvaloniaUI/Avalonia/blob/12.1.4/src/Avalonia.Controls/ApplicationLifetimes/ClassicDesktopStyleApplicationLifetime.cs#L161-L223).

`Program.cs:222` now handles this event before any window closes. `PadWindow.PrepareForSessionEnd` sets process-local `SessionEnd.IsEnding`, stops the save/trim timers, invalidates queued hide work, flushes both panes and synchronously calls the store once. It logs success/failure and elapsed milliseconds, catches exceptions, and never calls Quit, the A8 prompt, a toast, document eviction or a retry. Subsequent ordinary Save calls skip work during exit. Pad show/hide and activation/deactivation actions also stop once session end begins.

Both resident Closing handlers use `SessionEnd.ShouldCancelClose`: ordinary X/Alt+F4 still hide the pad; ordinary stand-in closes still cancel; explicit quit/disposal and session-end closes proceed. The pad also handles `OSShutdown` directly as a fallback. The shared state covers sign-out's `ApplicationShutdown` reason. No native hook is required: this uses Avalonia's existing query path, before `WM_ENDSESSION`.

Normal Quit retains A8's Retry / Quit anyway prompt. Its successful forced desktop shutdown does not enter the session-end handler. Each process has its own static state, store, windows and subscription; there is no IPC or shared signal between installed and dev copies. Both binaries need this change built into them; no running copy was upgraded here.

Avalonia closes the application at query time, so it also exits if another application subsequently cancels shutdown. That is the existing framework lifetime behavior. Microsoft documents that a nonzero query reply accepts session end and that the default native reply is TRUE: [WM_QUERYENDSESSION](https://learn.microsoft.com/en-us/windows/win32/shutdown/wm-queryendsession).

## What is saved and timing

`PadView.Flush` copies each bound pane's caret and scroll offsets; note text already comes from live editor documents. The existing `Store.Save` traverses all dirty notes in `Data.All` (stack, slots and trash), writes their bodies through temporary files and replacement, then writes the index with settings, selection and note metadata, preserving its existing backup/recovery and removed-body behavior. Clean bodies are not rewritten or loaded. Unavailable bodies remain protected. Unavailable/missing folders and failed writes are logged as failures, without UI.

There is one immediate synchronous pass, with no debounce wait, background task wait, retry loop, GC or cache eviction. Actual duration is recorded as `session-end save=ok ms=...` or `session-end save=failed ms=... error=...` in that copy's existing log. No live shutdown-save duration was measured: only selftest execution was authorized. A hard wall-clock ceiling cannot be guaranteed for synchronous filesystem operations on stalled/unavailable storage; the code adds no deliberate wait, but this storage limitation remains. The log's `ms` covers preparation and store saving, excluding the subsequent log append and framework window cleanup.

## Verification performed

```text
AVALONIA_TELEMETRY_OPTOUT=1 dotnet build product/AntiNotepad -c Release -o build/a26-check
Build succeeded. 0 Warning(s), 0 Error(s). Final build: 3.80 seconds.
build/a26-check/AntiNotepad.exe --selftest
SELFTEST PASS (exit 0)
git diff --check
exit 0
```

No NuGetAudit override was needed. Exactly one method was added at the end of SelfTest.cs: `SessionEndCloseChecks`. It exercises all five close reasons against ordinary residency, session end, explicit quit/disposal and combined exit state (20 logic checks). This does not validate native Windows message delivery, save-failure UI absence live, filesystem timing, or the event log. The report's PowerShell probe passed parsing and its C# interop declaration compiled; the probe was never invoked. Cleanup of the verified task-only `build/a26-check` output was attempted per docs/agents.md, but automatic approval review rejected the deletion as "blocked by policy" before execution; the output remains for the manager to remove.

No normal app instance was launched or killed; no installed-data, Programs or Documents paths were accessed. Only `--selftest` was launched.

## Manager verification without restarting Windows (not executed)

Use the updated dev copy through the normal manager dev workflow, with throwaway `build/testdata`. Check ordinary Alt+F4 and the stand-in taskbar Close first. Set Taskbar icon to Always to include the stand-in; repeat with Never and hidden startup if desired. Edit both split panes and a third note, including an edit immediately before the query so the 400 ms autosave has not necessarily run.

The following optional PowerShell probe targets only the dev PID whose executable path and start time match this repo's `build/dev.pid`. It enumerates that PID's hidden Avalonia message window and sends `WM_QUERYENDSESSION` to it, never broadcasting. Sending to the pad itself would miss Avalonia's lifetime event. This probe gracefully exits the dev copy through the real framework path; it neither requests an actual Windows shutdown nor touches the installed copy. Run from the repo that launched the dev copy. Do not point it at an installed process.

```powershell
$repoPath = (Get-Location).Path
$record = Get-Content -LiteralPath (Join-Path $repoPath 'build/dev.pid') -Raw | ConvertFrom-Json
$expectedExe = [IO.Path]::GetFullPath((Join-Path $repoPath 'build/dev/AntiNotepad.exe'))
$devProcess = Get-Process -Id ([int]$record.pid) -ErrorAction Stop
if ($record.exePath -ne $expectedExe -or $devProcess.Path -ne $expectedExe -or
    $devProcess.StartTime.ToUniversalTime().Ticks -ne [long]$record.startTimeUtcTicks) {
    throw 'Dev identity mismatch; no message sent.'
}
Add-Type @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
public static class A26DevSessionProbe {
    private delegate bool EnumProc(IntPtr hwnd, IntPtr arg);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc cb, IntPtr arg);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hwnd, StringBuilder name, int size);
    [DllImport("user32.dll", SetLastError=true)]
    private static extern IntPtr SendMessageTimeout(IntPtr hwnd, uint msg, UIntPtr wp,
        IntPtr lp, uint flags, uint timeout, out UIntPtr result);
    public static string Query(int targetPid, long sessionFlags) {
        var matches = new List<IntPtr>();
        EnumWindows((hwnd, arg) => {
            uint pid; GetWindowThreadProcessId(hwnd, out pid);
            if (pid == (uint)targetPid) {
                var name = new StringBuilder(256);
                GetClassName(hwnd, name, name.Capacity);
                if (name.ToString().StartsWith("AvaloniaMessageWindow ", StringComparison.Ordinal))
                    matches.Add(hwnd);
            }
            return true;
        }, IntPtr.Zero);
        if (matches.Count != 1) throw new InvalidOperationException("Expected exactly one dev Avalonia message window.");
        UIntPtr result;
        var sent = SendMessageTimeout(matches[0], 0x0011, UIntPtr.Zero,
            new IntPtr(sessionFlags), 0x0002, 2000, out result);
        return "Send completed=" + (sent != IntPtr.Zero) + "; reply=" + result.ToUInt64();
    }
}
'@
# 0 = shutdown/restart. After relaunching the dev copy and rereading its PID record,
# repeat with 2147483648 for sign-out; 1 and 1073741824 cover close-app/critical flags.
[A26DevSessionProbe]::Query($devProcess.Id, 0)
```

Expect a nonzero acceptance reply and dev exit, one `session-end save=ok ms=...` log entry, persisted edits/caret/scroll/settings after manager relaunch, and no prompt or session-end `cause=hide` sequence. Check the installed PID remains running. The sender's 2-second timeout only bounds the probe's wait; it does not bound filesystem work in the target. A window disappearing during dispatch can also make the sender report failure, so inspect the dev log and exit state rather than treating the reply alone as proof.

For failure behavior, use throwaway dev data only: after launch, temporarily deny writes or rename its notes directory, make a pending edit and send the same targeted query. Expect `save=failed`, exit and no prompt. Restore fixture permissions/path before relaunch. Repeat with Settings open and with the A8 failure prompt already visible. Do not infer real Windows event-log behavior from synthetic delivery; the next naturally requested shutdown/sign-out remains the final live gate for Winsrv 10001.
