# A44-platform-seam-impact-r2: READ-ONLY delta audit on latest dev

Role: director (gpt-6.1-sol medium; may delegate reading to luna subagents). Branch `reorg/A44-platform-seam` (this worktree), which
now contains dev `1e89288` (A39 to A43b merged). READ-ONLY: do not edit, create or delete any file except your report
`.coord/reports/A44-platform-seam-impact-r2.md`. Do not build, launch or kill AntiNotepad. Do not change product code.
Read `AGENTS.md`, then `.coord/tasks/A44-platform-seam-plan.md` and `.coord/reports/A44-platform-seam-impact.md` (the r1 audit
and plan, written against dev `b54b476`).

## What changed since r1 (verify each, find more)
A40 to A43b landed: `SettingsService`; `HotkeyRegistrar` in Platform; `ClipboardCapturePolicy`, `MathNotes`, `ListEditing`,
`NoteAnalysis`; `RuntimeLog`, `TaskbarTrace`, `AppLaunch`; `NotesSession` and `INotesLocation`; `Program.` tokens outside
`Program.cs` are now 0; the `LAYERS` selftest is now a large table-driven IL scanner over all namespaces (not Editor-only).
A39 moved `Anim` to `Motion`, `ChromeTokens` to `Theme`, `MenuEntry` to `Editor`.

## Work
1. Re-inventory (same columns as r1: location file:line, member, purpose, thread, handle, timing sensitivity, can-move?):
   every P/Invoke / `DllImport` / `LibraryImport` / `Marshal` / `IntPtr` / `nint` / `TryGetPlatformHandle` / WndProc / DWM
   touchpoint, every `Platform` type (public surface), and every reference from OUTSIDE `Platform` into Platform types,
   `Native.*`, `Process.Start`, `Process.GetCurrentProcess`, `Registry`, `Environment` OS calls. Give counts before/after vs r1:
   `Native.` refs outside Platform (r1: 11), P/Invoke-declaring types referenced by UI/Editor (r1: GlobalHotkey, TaskbarProxy,
   Native), `DllImport` total and per file (r1: 20 in 3 files).
2. Delta table: for each r1 claim in the plan (What tiers 1-3, Why examples, How steps, call-site list, risks, "kept as is" list,
   conflicts section), mark STILL TRUE / STALE / NEW with file:line evidence. Especially re-check: (a) r1 assumed A39's scanner
   covered only Editor types: read the landed `LAYERS` code in `SelfTest.cs` (and any split files) and say exactly which rule
   sources, target kinds, source-guard checks and negative-proof hooks exist now, and what A44 would add; (b) r1 assumed
   `GlobalHotkey`/`Tray`/`TaskbarPresence` use `Program.IsDev/Log`: check what they use now (`RuntimeLog`, `TaskbarTrace`,
   `AppLaunch`); (c) `HotkeyRegistrar` (what it owns, whether UI still constructs `GlobalHotkey`/`HotkeyGesture`), where clipboard
   listening, trim, taskbar glue, popup configure, OCR and snip now live after A41/A42, and whether UI/Editor still reference
   P/Invoke-declaring types; (d) whether any existing rule already forbids UI/Editor -> `Platform.*` types or Native; (e) the
   `Editor/Elements.cs` `Platform.Native.OpenUrl` call and anything new in Editor/Model/Theme touching the OS.
3. For every remaining outside call decide: can move behind the facade unchanged, or must stay (hot path, handle lifetime,
   ordering) with the reason. Flag new ordering hazards created by the A41 splits.
4. Propose the updated facade shape (type name, exact members, nesting of `Native` and extern holders), the updated guard list
   (rules added to the existing `LAYERS` table, source guard, negative proofs) compatible with the landed scanner, and an
   updated file/line estimate.
5. Recommendation on handling: scope (which tiers still make sense), whether to split into sub-tasks (name them `A44a-...`,
   `A44b-...` with exact scopes and the order), the riskiest parts, and the hand checks the user should do per sub-task.

## Report: `.coord/reports/A44-platform-seam-impact-r2.md`
Compact tables, file:line evidence for every claim, no padding. Sections: (a) summary of what A44 still needs and no longer
needs; (b) fresh inventory and counts; (c) delta table vs r1; (d) landed `LAYERS` capabilities; (e) facade and guard proposal;
(f) risks ranked; (g) handling recommendation; (h) estimate. State anything you could not verify.
