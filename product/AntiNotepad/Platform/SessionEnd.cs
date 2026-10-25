using Avalonia.Controls;

namespace AntiNotepad.Platform;

/// <summary>Process-local session-end state, shared by the pad and its taskbar stand-in.</summary>
internal static class SessionEnd
{
    internal static bool IsEnding { get; private set; }

    internal static bool Begin()
    {
        if (IsEnding) return false;
        IsEnding = true;
        return true;
    }

    internal static bool ShouldCancelClose(WindowCloseReason reason, bool exiting, bool sessionEnding) =>
        !exiting && !sessionEnding && reason != WindowCloseReason.OSShutdown;
}
