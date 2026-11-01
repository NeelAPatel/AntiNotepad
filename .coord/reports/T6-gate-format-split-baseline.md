# T6-gate-format-split-baseline report

Built by the Claude builder, no Codex (fast lane, small change).

## Summary
`tools/gate.ps1` Format step: a changed product `.cs` file missing at the base revision no longer counts every line as touched. A line counts as touched only if its text (trailing whitespace and CR ignored) is not among the `-` lines of `git diff -U0 --no-renames <base> -- product/*.cs`. Lines moved verbatim into a new file become "older" problems; new or edited lines still fail. Tracked files keep the hunk logic.

## Changes
- `tools/gate.ps1`: lazy `$removed` set built once per run for the first new file; per-line touched set for new files; comment above the step updated.

## Tests
Throwaway local branches in a temp worktree (deleted, never pushed), full gate with `-Base <throwaway> -LooseScope -Offline`:
1. Moved a badly formatted class verbatim from MenuEntry.cs into a new T6Probe.cs: Format PASS, "6 older one(s) ignored".
2. Added a new badly formatted class to T6Probe.cs: Format FAIL, 6 problems on the new lines 10-12 only; the moved lines stayed older.
AntiNotepad itself was not launched by me; the gate's own headless selftest ran as part of the gate.
