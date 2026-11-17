# Loop12 title spike log (2026-11-17, item chat Loop12-chat-title-scheme)

Throwaway chat `local_00281c24...` (Haiku, blank). App build: Claude Code 2.1.325, desktop app. All checks via `get_session` / `list_sessions` (the app's own view) and the files.

| # | Test | Result |
|---|---|---|
| 1 | `set_session_title` on this chat, then read its session file | File title unchanged (stale); app already held a newer title than the file. File mtime follows turn edges, not renames. |
| 2 | Edit `title` in `claude-code-sessions/.../local_<id>.json` of the idle throwaway; wait 10 s; `get_session` | App still shows old title: file is not read back. |
| 3 | Send the chat a message (a turn) | App rewrote the file with its own title: the edit is overwritten. |
| 4 | `UserPromptSubmit` hook (temp, in gitignored `.claude/settings.local.json`) printing `hookSpecificOutput.sessionTitle` | Hook ran live in the running chat (stdin carries `session_id` = cliSessionId and `transcript_path`); Claude Code wrote a `custom-title` record to the transcript; desktop sidebar/`get_session` title did NOT change. Matches known issues (claudeissues #55951, #67755). Emoji came out as `??` (console code page): ASCII or UTF-8 output setup needed. |
| 5 | Docs: `sessionTitle` is only documented for `SessionStart` and `UserPromptSubmit`; no hook for Stop/Notification/PreToolUse. | Even if the desktop honored it, Stop-time states (a, parked, needs you) could not be set. |

Verdict: no script or hook can retitle a chat in the desktop app. Only the app's `set_session_title` tool (the model, or the director for another chat) works. Fallback chosen (card Plan). Temp hook removed, PStack test2 restored byte for byte, throwaway chat left idle (user may archive it).
Not tested: SessionStart `sessionTitle`, a local socket or deep link into the app. Neither is documented.
