// Computes save deadlines for a single note.
using AntiNotepad.Theme;

namespace AntiNotepad.UI;

/// <summary>Tracks the quiet period, idle deadline and repeating safety-write deadline for one note.</summary>
internal static class SaveTiming
{
    internal readonly record struct State(long FirstEditMs, long LastEditMs, long NextSafetyMs, int DelayMs, long? RetryAtMs,
        bool ResetBurstOnEdit = false);
    internal readonly record struct Step(State State, long DeadlineMs, bool SaveNow);

    /// <summary>Starts a new typing run or moves its trailing edge without losing its safety deadline.</summary>
    internal static Step Edit(State? current, long now, int delayMs, int idleGateMs)
    {
        var state = current is not { } prior || now - prior.LastEditMs >= idleGateMs || prior.DelayMs != delayMs || prior.ResetBurstOnEdit
            ? new State(now, now, now + delayMs, delayMs, null)
            : prior with { LastEditMs = now, RetryAtMs = null, ResetBurstOnEdit = false };
        return new(state, NextDeadline(state, now, idleGateMs), false);
    }

    /// <summary>Returns the next idle or save deadline and advances failed or repeating writes.</summary>
    internal static Step Tick(State state, long now, int idleGateMs)
    {
        var deadline = NextDeadline(state, now, idleGateMs);
        var idleDeadline = state.LastEditMs + idleGateMs;
        var idleOnly = state.DelayMs > ChromeTokens.SaveDelayDefaultMs
            && state.RetryAtMs is null && deadline == idleDeadline && state.NextSafetyMs > idleDeadline;
        if (now < deadline || idleOnly) return new(state, idleOnly ? NextAfterIdle(state, idleGateMs) : deadline, false);
        var retry = RetryAfterAttempt(state, now);
        return new(retry, retry.RetryAtMs!.Value, true);
    }

    /// <summary>Schedules the next attempt after a failed write or an immediate save boundary.</summary>
    internal static State RetryAfterAttempt(State state, long now, bool? resetBurstOnEdit = null) => state with
    {
        NextSafetyMs = now + state.DelayMs,
        RetryAtMs = now + state.DelayMs,
        ResetBurstOnEdit = resetBurstOnEdit ?? state.ResetBurstOnEdit
    };

    /// <summary>Finds the earliest deadline without consulting wall-clock time.</summary>
    internal static long NextDeadline(State state, long now, int idleGateMs)
    {
        if (state.RetryAtMs is { } retry) return retry;
        if (state.DelayMs <= ChromeTokens.SaveDelayDefaultMs) return state.LastEditMs + state.DelayMs;
        var idleDeadline = state.LastEditMs + idleGateMs;
        if (now < idleDeadline) return Math.Min(idleDeadline, state.NextSafetyMs);
        return state.NextSafetyMs <= idleDeadline ? state.NextSafetyMs : state.LastEditMs + state.DelayMs;
    }

    private static long NextAfterIdle(State state, int idleGateMs) =>
        state.NextSafetyMs <= state.LastEditMs + idleGateMs ? state.NextSafetyMs : state.LastEditMs + state.DelayMs;
}

