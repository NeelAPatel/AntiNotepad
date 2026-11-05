## Summary
GATE: FAIL
Head: 23ae02690b82c3aa7d91c1c4b271a32c251240fd-dirty
Build: FAIL - dotnet exit 0, 2 warning(s), 0 error(s)
Selftest: PASS - exe exit 0; contains SELFTEST PASS
Seam: PASS - check-platform-seam.ps1 exit 0
Tokens: PASS - check-design-tokens.ps1 exit 0
Format: PASS - 13 changed C# file(s), no format problems on changed lines; 45 older one(s) ignored
Scope: PASS - 14 changed file(s); all are allowed by the brief
LoopLint: SKIP - no loop files changed
Size: PASS - no changed product C# file crossed 600 lines or grew by more than 25 while already over it
Cleanup: SKIP - kept build and data folders because the gate failed

## Build
FAIL - dotnet exit 0, 2 warning(s), 0 error(s)
- F:\LocalVSProj\Visual Studio Apps\AntiQuickPad\.claude\worktrees\refactor\A4f-paint-tokens\product\AntiNotepad\AntiNotepad.csproj : warning NU1900: Error occurred while getting package vulnerability data: Unable to load the service index for source https://api.nuget.org/v3/index.json.
- F:\LocalVSProj\Visual Studio Apps\AntiQuickPad\.claude\worktrees\refactor\A4f-paint-tokens\product\AntiNotepad\AntiNotepad.csproj : warning NU1900: Error occurred while getting package vulnerability data: Unable to load the service index for source https://api.nuget.org/v3/index.json.
- Build succeeded.
- F:\LocalVSProj\Visual Studio Apps\AntiQuickPad\.claude\worktrees\refactor\A4f-paint-tokens\product\AntiNotepad\AntiNotepad.csproj : warning NU1900: Error occurred while getting package vulnerability data: Unable to load the service index for source https://api.nuget.org/v3/index.json.
- F:\LocalVSProj\Visual Studio Apps\AntiQuickPad\.claude\worktrees\refactor\A4f-paint-tokens\product\AntiNotepad\AntiNotepad.csproj : warning NU1900: Error occurred while getting package vulnerability data: Unable to load the service index for source https://api.nuget.org/v3/index.json.
-     2 Warning(s)
-     0 Error(s)

## Selftest
PASS - exe exit 0; contains SELFTEST PASS
- LAYERS Editor->UI 0 refs
- LAYERS Model->UI 0 refs
- LAYERS Model->Editor 0 refs
- LAYERS Calc->*any other AntiNotepad namespace* 0 refs
- LAYERS Theme->UI 0 refs
- LAYERS Theme->Editor 0 refs
- LAYERS Platform->UI 0 refs
- LAYERS Platform->Editor 0 refs
- LAYERS HotkeyRegistrar->PadWindow 0 refs
- LAYERS HotkeyRegistrar->PadView 0 refs
- LAYERS HotkeyRegistrar->Program 0 refs
- LAYERS ClipboardCapturePolicy->UI 0 refs
- LAYERS ClipboardCapturePolicy->Platform 0 refs
- LAYERS ClipboardCapturePolicy->Motion 0 refs
- LAYERS MathNotes->UI 0 refs
- LAYERS MathNotes->Platform 0 refs
- LAYERS MathNotes->Motion 0 refs
- LAYERS MathNotes->Theme 0 refs
- LAYERS MathNotes->PadWindow 0 refs
- LAYERS MathNotes->PadView 0 refs
- LAYERS MathNotes->Program 0 refs
- LAYERS MathNotes->DesktopPlatform 0 refs
- LAYERS MathNotes->Window 0 refs
- LAYERS MathNotes->Control 0 refs
- LAYERS MathNotes->Dispatcher 0 refs
- LAYERS MathNotes->TextDocument 0 refs
- LAYERS MathNotes->Timers 0 refs
- LAYERS MathNotes->Settings 0 refs
- LAYERS MathNotes->SettingsService 0 refs
- LAYERS ListEditing->UI 0 refs
- LAYERS ListEditing->Platform 0 refs
- LAYERS ListEditing->Motion 0 refs
- LAYERS ListEditing->Theme 0 refs
- LAYERS ListEditing->PadWindow 0 refs
- LAYERS ListEditing->PadView 0 refs
- LAYERS ListEditing->Program 0 refs
- LAYERS ListEditing->DesktopPlatform 0 refs
- LAYERS ListEditing->Window 0 refs
- LAYERS ListEditing->Control 0 refs
- LAYERS ListEditing->Dispatcher 0 refs
- LAYERS ListEditing->TextDocument 0 refs
- LAYERS ListEditing->TextEditor 0 refs
- LAYERS ListEditing->TextArea 0 refs
- LAYERS ListEditing->TextView 0 refs
- LAYERS ListEditing->Timers 0 refs
- LAYERS ListEditing->Settings 0 refs
- LAYERS ListEditing->SettingsService 0 refs
- LAYERS ListEditing->CheckboxGenerator 0 refs
- LAYERS DocumentCommands->UI 0 refs
- LAYERS DocumentCommands->Platform 0 refs
- LAYERS DocumentCommands->Motion 0 refs
- LAYERS DocumentCommands->Theme 0 refs
- LAYERS DocumentCommands->PadWindow 0 refs
- LAYERS DocumentCommands->PadView 0 refs
- LAYERS DocumentCommands->Program 0 refs
- LAYERS DocumentCommands->DesktopPlatform 0 refs
- LAYERS DocumentCommands->Window 0 refs
- LAYERS DocumentCommands->Control 0 refs
- LAYERS DocumentCommands->Dispatcher 0 refs
- LAYERS DocumentCommands->TextEditor 0 refs
- LAYERS DocumentCommands->TextArea 0 refs
- LAYERS DocumentCommands->TextView 0 refs
- LAYERS DocumentCommands->Timers 0 refs
- LAYERS DocumentCommands->Settings 0 refs
- LAYERS DocumentCommands->SettingsService 0 refs
- LAYERS DocumentCommands->CheckboxGenerator 0 refs
- LAYERS NoteAnalysis->PadView 0 refs
- LAYERS NoteAnalysis->UI 0 refs
- LAYERS NoteAnalysis->Platform 0 refs
- LAYERS NoteAnalysis->Motion 0 refs
- LAYERS NoteAnalysis->Theme 0 refs
- LAYERS NoteAnalysis->PadWindow 0 refs
- LAYERS NoteAnalysis->Program 0 refs
- LAYERS NoteAnalysis->DesktopPlatform 0 refs
- LAYERS NoteAnalysis->Window 0 refs
- LAYERS NoteAnalysis->Control 0 refs
- LAYERS NoteAnalysis->Dispatcher 0 refs
- LAYERS NoteAnalysis->Settings 0 refs
- LAYERS NoteAnalysis->SettingsService 0 refs
- LAYERS NoteAnalysis->TextEditor 0 refs
- LAYERS NoteAnalysis->TextArea 0 refs
- LAYERS NoteAnalysis->TextView 0 refs
- LAYERS ClipboardCapturePolicy->PadWindow 0 refs
- LAYERS ClipboardCapturePolicy->PadView 0 refs
- LAYERS ClipboardCapturePolicy->Program 0 refs
- LAYERS ClipboardCapturePolicy->DesktopPlatform 0 refs
- LAYERS ClipboardCapturePolicy->Window 0 refs
- LAYERS ClipboardCapturePolicy->Control 0 refs
- LAYERS ClipboardCapturePolicy->Dispatcher 0 refs
- LAYERS ClipboardCapturePolicy->TextDocument 0 refs
- LAYERS Platform->Program 0 refs
- LAYERS Model->Program 0 refs
- LAYERS RuntimeLog->UI 0 refs
- LAYERS RuntimeLog->Editor 0 refs
- LAYERS RuntimeLog->Platform 0 refs
- LAYERS RuntimeLog->Program 0 refs
- LAYERS RuntimeLog->Avalonia 0 refs
- LAYERS SettingsWindow->PadWindow 0 refs
- LAYERS SettingsService->UI 0 refs
- LAYERS SettingsService->Editor 0 refs
- LAYERS Editor->Program 0 refs
- LAYERS UI->Program 0 refs
- LAYERS App->Program 0 refs
- LAYERS Model->NotesSession 0 refs
- LAYERS Editor->NotesSession 0 refs
- LAYERS INotesLocation->UI 0 refs
- LAYERS INotesLocation->Editor 0 refs
- LAYERS INotesLocation->Platform 0 refs
- LAYERS *except Program->Program 0 refs
- LAYERS UI->pinvoke-holder 0 refs
- LAYERS Editor->pinvoke-holder 0 refs
- LAYERS UI->Marshal 0 refs
- LAYERS Editor->Marshal 0 refs
- LAYERS UI->IntPtr 0 refs
- LAYERS Editor->IntPtr 0 refs
- LAYERS UI->UIntPtr 0 refs
- LAYERS Editor->UIntPtr 0 refs
- LAYERS UI->Win32Properties 0 refs
- LAYERS Editor->Win32Properties 0 refs
- LAYERS UI->Windows.* 0 refs
- LAYERS Editor->Windows.* 0 refs
- LAYERS pinvoke-outside-Platform 0
- LAYERS Settings.set_* outside owner 0
- LAYERS UI->Process.Start 0
- LAYERS Editor->Process.Start 0
- CONTRACTS: 13 rows, 12 pass, 1 manual
- SELFTEST PASS

## Seam
PASS - check-platform-seam.ps1 exit 0
- PLATFORM-SEAM source guard: 0 violations in 74 files

## Tokens
PASS - check-design-tokens.ps1 exit 0
- TOKENS PASS

## Format
PASS - 13 changed C# file(s), no format problems on changed lines; 45 older one(s) ignored

## Scope
PASS - 14 changed file(s); all are allowed by the brief

## LoopLint
SKIP - no loop files changed

## Size
PASS - no changed product C# file crossed 600 lines or grew by more than 25 while already over it

## Cleanup
SKIP - kept build and data folders because the gate failed
